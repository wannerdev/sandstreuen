import { VoxelGrid } from './voxelGrid';
import { DualContourer, MeshData } from './dualContouring';
import { DirtyRegion } from './dirtyRegion';
import { stepGravity } from './gravity';
import { sdCone, perlin3 } from './sdf';
import { SAND_MATERIALS, EditMode } from './materials';

export type Preset = 'empty' | 'cone' | 'noise';

/**
 * The sandbox itself: owns the field, applies edits, runs gravity and decides
 * when the mesh needs rebuilding. Coordinates are in grid space, x/z in
 * [0, cells] with y up; the renderer centers the grid around the world origin.
 */
export class SandEngine {
  readonly cells: number;
  readonly grid: VoxelGrid;
  readonly mesher: DualContourer;

  gravityEnabled = true;

  private readonly dirty = new DirtyRegion();
  private meshDirty = true;
  private gravityStep = 0;

  /** State for grow mode: repeated strokes at the same spot raise the cone. */
  private growHeight = 5;
  private growX = NaN;
  private growZ = NaN;

  constructor(cells = 64, preset: Preset = 'cone') {
    this.cells = cells;
    this.grid = new VoxelGrid(cells);
    this.fill(preset);
    this.mesher = new DualContourer(this.grid);
  }

  reset(preset: Preset): void {
    this.fill(preset);
    this.mesher.updateVertices();
    this.dirty.reset();
    this.meshDirty = true;
  }

  private fill(preset: Preset): void {
    this.grid.fillAir();
    const n = this.cells;
    switch (preset) {
      case 'cone':
        this.stampCone(n / 2, 10, n / 2, 10, SAND_MATERIALS[0].aperture);
        break;
      case 'noise': {
        const dim = this.grid.dim;
        for (let x = 0; x < dim; x++) {
          for (let y = 0; y < dim; y++) {
            for (let z = 0; z < dim; z++) {
              this.grid.set(x, y, z, perlin3(x * 0.1, y * 0.1, z * 0.1) - 0.5 + y * 0.04);
            }
          }
        }
        break;
      }
      case 'empty':
        break;
    }
  }

  inBounds(x: number, y: number, z: number): boolean {
    const n = this.cells;
    return x >= 0 && x <= n && y >= 0 && y <= n && z >= 0 && z <= n;
  }

  /**
   * Apply the current edit mode at a point on the sand/ground surface
   * (grid space). Returns false when the point is outside the sandbox.
   */
  applyEdit(mode: EditMode, x: number, y: number, z: number, materialIndex: number): boolean {
    const aperture = SAND_MATERIALS[materialIndex]?.aperture ?? SAND_MATERIALS[0].aperture;
    switch (mode) {
      case 'cone':
        // Apex above the surface so the cone's base lands on it.
        return this.addCone(x, y + 5, z, 5, aperture);
      case 'single':
        return this.stampStar(x, y + 1, z, -1);
      case 'remove':
        return this.stampStar(x, y - 0.5, z, 1);
      case 'grow': {
        // Same spot (within a cell and a half) keeps raising the pile.
        if (Math.hypot(x - this.growX, z - this.growZ) < 1.5) {
          this.growHeight += 0.5;
        } else {
          this.growHeight = 5;
        }
        this.growX = x;
        this.growZ = z;
        return this.addCone(x, this.growHeight, z, this.growHeight, aperture);
      }
    }
  }

  /** Deposit a cone of sand with its apex at (x, y, z), opening downwards. */
  addCone(x: number, y: number, z: number, height: number, aperture: number): boolean {
    if (!this.inBounds(x, y, z)) return false;
    this.stampCone(x, y, z, height, aperture);
    return true;
  }

  private stampCone(cx: number, cy: number, cz: number, height: number, aperture: number): void {
    const sinA = Math.sin(aperture);
    const cosA = Math.cos(aperture);
    const n = this.cells;

    // The cone only influences samples within its bounding box plus a band
    // where the SDF is < 1 (same idea as the original's isConeOptimized).
    const r = height * (sinA / cosA) + 2;
    const minX = Math.max(0, Math.floor(cx - r));
    const maxX = Math.min(n + 1, Math.ceil(cx + r));
    const minY = Math.max(0, Math.floor(cy - height - 2));
    const maxY = Math.min(n + 1, Math.ceil(cy + 2));
    const minZ = Math.max(0, Math.floor(cz - r));
    const maxZ = Math.min(n + 1, Math.ceil(cz + r));

    for (let x = minX; x <= maxX; x++) {
      for (let y = minY; y <= maxY; y++) {
        for (let z = minZ; z <= maxZ; z++) {
          const d = sdCone(x - cx, y - cy, z - cz, sinA, cosA, height);
          if (d < 1) {
            // Union with existing sand (min), instead of overwriting like the
            // original — placing a new cone never erases an old one.
            const i = this.grid.index(x, y, z);
            if (d < this.grid.sdf[i]) this.grid.sdf[i] = d;
          }
        }
      }
    }
    this.dirty.includeBox(minX, minY, minZ, maxX, maxY, maxZ);
    this.markChanged();
  }

  /** The original add_single: a small plus-shaped stamp (place or carve). */
  private stampStar(x: number, y: number, z: number, value: number): boolean {
    if (!this.inBounds(x, y, z)) return false;
    const xi = Math.round(x);
    const yi = Math.round(y);
    const zi = Math.round(z);
    const offsets = [
      [0, 0, 0],
      [1, 0, 0],
      [-1, 0, 0],
      [0, 1, 0],
      [0, -1, 0],
      [0, 0, 1],
      [0, 0, -1],
    ];
    for (const [ox, oy, oz] of offsets) {
      const px = xi + ox;
      const py = yi + oy;
      const pz = zi + oz;
      if (px >= 0 && py >= 0 && pz >= 0 && px <= this.cells + 1 && py <= this.cells + 1 && pz <= this.cells + 1) {
        this.grid.set(px, py, pz, value);
        this.dirty.include(px, py, pz);
      }
    }
    this.markChanged();
    return true;
  }

  private markChanged(): void {
    this.meshDirty = true;
  }

  /** Advance the sand by one gravity tick. Returns true if anything moved. */
  tickGravity(): boolean {
    if (!this.gravityEnabled) return false;
    const moved = stepGravity(this.grid, this.gravityStep++, this.dirty);
    if (moved > 0) this.meshDirty = true;
    return moved > 0;
  }

  /** Rebuild and return the mesh if anything changed since the last call. */
  buildMeshIfDirty(): MeshData | null {
    if (!this.meshDirty) return null;
    if (!this.dirty.empty) {
      this.dirty.expand(2);
      this.mesher.updateVertices(
        this.dirty.minX,
        this.dirty.minY,
        this.dirty.minZ,
        this.dirty.maxX,
        this.dirty.maxY,
        this.dirty.maxZ,
      );
      this.dirty.reset();
    }
    this.meshDirty = false;
    return this.mesher.buildMesh();
  }
}
