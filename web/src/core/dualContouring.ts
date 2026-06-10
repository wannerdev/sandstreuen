import { VoxelGrid } from './voxelGrid';

export interface MeshData {
  positions: Float32Array;
  indices: Uint32Array;
}

/**
 * Dual contouring of a VoxelGrid, ported from the original prototype
 * (DualContouring3D.cs, itself based on Boris The Brave's tutorial).
 *
 * Differences from the original:
 * - one flat Float32Array per grid instead of managed 3D arrays
 * - vertex normals for the QEF relaxation come from the actual field
 *   gradient instead of a fixed brush cone
 * - adaptive vertices are only recomputed inside a dirty region
 * - output is an indexed triangle mesh with shared vertices
 */
export class DualContourer {
  private readonly grid: VoxelGrid;
  /** Cells per axis (one adaptive vertex per cell). */
  private readonly vdim: number;
  private readonly vertGrid: Float32Array;
  private readonly vertIndex: Int32Array;

  /** Maximum relaxation steps for the particle-based QEF solver (Schmitz). */
  private static readonly MAX_PARTICLE_ITERATIONS = 50;
  private static readonly FORCE_THRESHOLD_SQ = 1e-6;

  contour = 0;
  /** When false, vertices snap to cell centers (blocky "minecraft" look). */
  adaptive = true;

  constructor(grid: VoxelGrid) {
    this.grid = grid;
    this.vdim = grid.cells + 1;
    this.vertGrid = new Float32Array(this.vdim * this.vdim * this.vdim * 3);
    this.vertIndex = new Int32Array(this.vdim * this.vdim * this.vdim);
    this.updateVertices();
  }

  private vIdx(x: number, y: number, z: number): number {
    return (x * this.vdim + y) * this.vdim + z;
  }

  /** Recompute adaptive vertices, optionally only inside a region (inclusive bounds). */
  updateVertices(
    minX = 0,
    minY = 0,
    minZ = 0,
    maxX = this.vdim - 1,
    maxY = this.vdim - 1,
    maxZ = this.vdim - 1,
  ): void {
    const lo = (v: number) => Math.max(0, Math.floor(v));
    const hi = (v: number) => Math.min(this.vdim - 1, Math.ceil(v));
    const out: [number, number, number] = [0, 0, 0];
    for (let x = lo(minX); x <= hi(maxX); x++) {
      for (let y = lo(minY); y <= hi(maxY); y++) {
        for (let z = lo(minZ); z <= hi(maxZ); z++) {
          this.solveCell(x, y, z, out);
          const i = this.vIdx(x, y, z) * 3;
          this.vertGrid[i] = out[0];
          this.vertGrid[i + 1] = out[1];
          this.vertGrid[i + 2] = out[2];
        }
      }
    }
  }

  /**
   * Find the representative vertex of one cell: the point minimising the
   * distance to the planes through each edge crossing (particle relaxation).
   */
  private solveCell(x: number, y: number, z: number, out: [number, number, number]): void {
    out[0] = x + 0.5;
    out[1] = y + 0.5;
    out[2] = z + 0.5;
    if (!this.adaptive) return;

    const g = this.grid;
    const c = this.contour;
    // Field values at the 8 cell corners, offset by the contour so that the
    // crossing always sits at value 0.
    const v: number[] = new Array(8);
    for (let i = 0; i < 8; i++) {
      v[i] = g.get(x + (i >> 2), y + ((i >> 1) & 1), z + (i & 1)) - c;
    }
    const corner = (dx: number, dy: number, dz: number) => v[(dx << 2) | (dy << 1) | dz];
    const adapt = (v0: number, v1: number) => (0 - v0) / (v1 - v0);

    const points: number[] = [];
    for (let dx = 0; dx < 2; dx++) {
      for (let dy = 0; dy < 2; dy++) {
        const v0 = corner(dx, dy, 0);
        const v1 = corner(dx, dy, 1);
        if ((v0 > 0) !== (v1 > 0)) points.push(x + dx, y + dy, z + adapt(v0, v1));
      }
    }
    for (let dx = 0; dx < 2; dx++) {
      for (let dz = 0; dz < 2; dz++) {
        const v0 = corner(dx, 0, dz);
        const v1 = corner(dx, 1, dz);
        if ((v0 > 0) !== (v1 > 0)) points.push(x + dx, y + adapt(v0, v1), z + dz);
      }
    }
    for (let dy = 0; dy < 2; dy++) {
      for (let dz = 0; dz < 2; dz++) {
        const v0 = corner(0, dy, dz);
        const v1 = corner(1, dy, dz);
        if ((v0 > 0) !== (v1 > 0)) points.push(x + adapt(v0, v1), y + dy, z + dz);
      }
    }

    const count = points.length / 3;
    if (count <= 1) return;

    const normals = new Float32Array(points.length);
    const n: [number, number, number] = [0, 0, 0];
    for (let i = 0; i < count; i++) {
      this.grid.gradient(points[i * 3], points[i * 3 + 1], points[i * 3 + 2], n);
      normals[i * 3] = n[0];
      normals[i * 3 + 1] = n[1];
      normals[i * 3 + 2] = n[2];
    }

    // Start at the mean of the crossings, then relax towards the planes.
    let cx = 0;
    let cy = 0;
    let cz = 0;
    for (let i = 0; i < count; i++) {
      cx += points[i * 3];
      cy += points[i * 3 + 1];
      cz += points[i * 3 + 2];
    }
    cx /= count;
    cy /= count;
    cz /= count;

    const maxIter = DualContourer.MAX_PARTICLE_ITERATIONS;
    for (let iter = 0; iter < maxIter; iter++) {
      let fx = 0;
      let fy = 0;
      let fz = 0;
      for (let i = 0; i < count; i++) {
        const nx = normals[i * 3];
        const ny = normals[i * 3 + 1];
        const nz = normals[i * 3 + 2];
        const d =
          nx * (cx - points[i * 3]) + ny * (cy - points[i * 3 + 1]) + nz * (cz - points[i * 3 + 2]);
        fx -= nx * d;
        fy -= ny * d;
        fz -= nz * d;
      }
      const damping = (1 - iter / maxIter) / count;
      cx += fx * damping;
      cy += fy * damping;
      cz += fz * damping;
      if (fx * fx + fy * fy + fz * fz < DualContourer.FORCE_THRESHOLD_SQ) break;
    }

    // Keep the vertex inside its cell; stray vertices produce spike artifacts.
    out[0] = Math.min(Math.max(cx, x), x + 1);
    out[1] = Math.min(Math.max(cy, y), y + 1);
    out[2] = Math.min(Math.max(cz, z), z + 1);
  }

  private isInside(x: number, y: number, z: number): boolean {
    return this.grid.get(x, y, z) < this.contour;
  }

  /**
   * Extract the surface as an indexed triangle mesh. For every grid edge with
   * a sign change, the four adjacent cell vertices form a quad facing the
   * outside (air) end of the edge.
   */
  buildMesh(): MeshData {
    const n = this.grid.cells;
    const positions: number[] = [];
    const indices: number[] = [];
    this.vertIndex.fill(-1);

    const emit = (x: number, y: number, z: number): number => {
      const vi = this.vIdx(x, y, z);
      let m = this.vertIndex[vi];
      if (m === -1) {
        m = positions.length / 3;
        this.vertIndex[vi] = m;
        positions.push(this.vertGrid[vi * 3], this.vertGrid[vi * 3 + 1], this.vertGrid[vi * 3 + 2]);
      }
      return m;
    };

    // Geometric normal (unnormalized) of a triangle of mesh vertices.
    const triDot = (i0: number, i1: number, i2: number, dx: number, dy: number, dz: number) => {
      const a = i0 * 3;
      const b = i1 * 3;
      const c = i2 * 3;
      const abx = positions[b] - positions[a];
      const aby = positions[b + 1] - positions[a + 1];
      const abz = positions[b + 2] - positions[a + 2];
      const acx = positions[c] - positions[a];
      const acy = positions[c + 1] - positions[a + 1];
      const acz = positions[c + 2] - positions[a + 2];
      return (
        (aby * acz - abz * acy) * dx + (abz * acx - abx * acz) * dy + (abx * acy - aby * acx) * dz
      );
    };

    // Quad vertices arrive in counter-clockwise order as seen from the air
    // side of the edge (`flip` reverses for the negative axis). Adaptive
    // vertices can twist a quad, so split along the diagonal that best
    // matches the known air direction and force the winding of stragglers —
    // otherwise twisted faces light up black.
    const quad = (
      ax: number, ay: number, az: number,
      bx: number, by: number, bz: number,
      cx: number, cy: number, cz: number,
      dx: number, dy: number, dz: number,
      flip: boolean,
      nx: number, ny: number, nz: number,
    ) => {
      const a = emit(ax, ay, az);
      const b = emit(bx, by, bz);
      const c = emit(cx, cy, cz);
      const d = emit(dx, dy, dz);
      if (flip) {
        nx = -nx;
        ny = -ny;
        nz = -nz;
      }
      const v = flip ? [a, d, c, b] : [a, b, c, d];

      const splitAC: [number, number, number][] = [
        [v[0], v[1], v[2]],
        [v[0], v[2], v[3]],
      ];
      const splitBD: [number, number, number][] = [
        [v[0], v[1], v[3]],
        [v[1], v[2], v[3]],
      ];
      const score = (tris: [number, number, number][]) =>
        tris.reduce((n, t) => n + (triDot(t[0], t[1], t[2], nx, ny, nz) >= 0 ? 1 : 0), 0);
      const sAC = score(splitAC);
      const tris = sAC >= score(splitBD) ? splitAC : splitBD;

      for (const [i0, i1, i2] of tris) {
        if (triDot(i0, i1, i2, nx, ny, nz) < 0) {
          indices.push(i0, i2, i1);
        } else {
          indices.push(i0, i1, i2);
        }
      }
    };

    for (let x = 0; x <= n; x++) {
      for (let y = 0; y <= n; y++) {
        for (let z = 0; z <= n; z++) {
          if (x > 0 && y > 0) {
            const solid1 = this.isInside(x, y, z);
            const solid2 = this.isInside(x, y, z + 1);
            if (solid1 !== solid2) {
              // Edge along +z; CCW from +z when the air side is +z (solid1).
              quad(
                x - 1, y - 1, z,
                x, y - 1, z,
                x, y, z,
                x - 1, y, z,
                !solid1,
                0, 0, 1,
              );
            }
          }
          if (x > 0 && z > 0) {
            const solid1 = this.isInside(x, y, z);
            const solid2 = this.isInside(x, y + 1, z);
            if (solid1 !== solid2) {
              // Edge along +y; CCW from +y when the air side is +y (solid1).
              quad(
                x - 1, y, z - 1,
                x - 1, y, z,
                x, y, z,
                x, y, z - 1,
                !solid1,
                0, 1, 0,
              );
            }
          }
          if (y > 0 && z > 0) {
            const solid1 = this.isInside(x, y, z);
            const solid2 = this.isInside(x + 1, y, z);
            if (solid1 !== solid2) {
              // Edge along +x; CCW from +x when the air side is +x (solid1).
              quad(
                x, y - 1, z - 1,
                x, y, z - 1,
                x, y, z,
                x, y - 1, z,
                !solid1,
                1, 0, 0,
              );
            }
          }
        }
      }
    }

    return {
      positions: new Float32Array(positions),
      indices: new Uint32Array(indices),
    };
  }
}
