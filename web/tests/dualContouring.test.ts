import { describe, expect, it } from 'vitest';
import { VoxelGrid } from '../src/core/voxelGrid';
import { DualContourer, MeshData } from '../src/core/dualContouring';
import { sdSphere } from '../src/core/sdf';

function triangleNormal(
  mesh: MeshData,
  t: number,
): [number, number, number] {
  const { positions: p, indices } = mesh;
  const a = indices[t * 3] * 3;
  const b = indices[t * 3 + 1] * 3;
  const c = indices[t * 3 + 2] * 3;
  const abx = p[b] - p[a];
  const aby = p[b + 1] - p[a + 1];
  const abz = p[b + 2] - p[a + 2];
  const acx = p[c] - p[a];
  const acy = p[c + 1] - p[a + 1];
  const acz = p[c + 2] - p[a + 2];
  return [aby * acz - abz * acy, abz * acx - abx * acz, abx * acy - aby * acx];
}

describe('DualContourer', () => {
  it('meshes a flat floor with all faces pointing up', () => {
    const grid = new VoxelGrid(12);
    const floorY = 4;
    for (let x = 0; x < grid.dim; x++) {
      for (let y = 0; y < grid.dim; y++) {
        for (let z = 0; z < grid.dim; z++) {
          grid.set(x, y, z, y < floorY ? -1 : 1);
        }
      }
    }
    const dc = new DualContourer(grid);
    const mesh = dc.buildMesh();

    expect(mesh.indices.length).toBeGreaterThan(0);
    expect(mesh.indices.length % 3).toBe(0);

    for (let t = 0; t < mesh.indices.length / 3; t++) {
      const [, ny] = triangleNormal(mesh, t);
      expect(ny).toBeGreaterThan(0);
    }
    // The surface should sit at the sign change, between y=3 and y=4.
    for (let v = 0; v < mesh.positions.length / 3; v++) {
      const y = mesh.positions[v * 3 + 1];
      expect(y).toBeGreaterThanOrEqual(3);
      expect(y).toBeLessThanOrEqual(4);
    }
  });

  it('meshes a sphere with outward normals and adaptive vertices on the surface', () => {
    const n = 16;
    const grid = new VoxelGrid(n);
    const c = n / 2 + 1;
    const r = 5;
    for (let x = 0; x < grid.dim; x++) {
      for (let y = 0; y < grid.dim; y++) {
        for (let z = 0; z < grid.dim; z++) {
          grid.set(x, y, z, sdSphere(x, y, z, c, c, c, r));
        }
      }
    }
    const dc = new DualContourer(grid);
    const mesh = dc.buildMesh();

    expect(mesh.indices.length).toBeGreaterThan(0);

    // Adaptive vertices should hug the sphere surface.
    for (let v = 0; v < mesh.positions.length / 3; v++) {
      const d = Math.hypot(
        mesh.positions[v * 3] - c,
        mesh.positions[v * 3 + 1] - c,
        mesh.positions[v * 3 + 2] - c,
      );
      expect(Math.abs(d - r)).toBeLessThan(1.0);
    }

    // Every face must point away from the center.
    for (let t = 0; t < mesh.indices.length / 3; t++) {
      const [nx, ny, nz] = triangleNormal(mesh, t);
      const a = mesh.indices[t * 3] * 3;
      const dot =
        nx * (mesh.positions[a] - c) +
        ny * (mesh.positions[a + 1] - c) +
        nz * (mesh.positions[a + 2] - c);
      expect(dot).toBeGreaterThan(0);
    }
  });

  it('produces no geometry for an empty grid', () => {
    const grid = new VoxelGrid(8);
    grid.fillAir();
    const dc = new DualContourer(grid);
    const mesh = dc.buildMesh();
    expect(mesh.indices.length).toBe(0);
  });

  it('snaps vertices to cell centers when adaptivity is off', () => {
    const grid = new VoxelGrid(8);
    grid.fillAir();
    const dc = new DualContourer(grid);
    dc.adaptive = false;
    grid.set(4, 4, 4, -1);
    dc.updateVertices();
    const mesh = dc.buildMesh();
    expect(mesh.indices.length).toBeGreaterThan(0);
    for (let v = 0; v < mesh.positions.length / 3; v++) {
      expect(mesh.positions[v * 3] % 1).toBeCloseTo(0.5);
    }
  });
});
