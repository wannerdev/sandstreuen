/**
 * A scalar field sampled on a regular grid, backed by a flat Float32Array.
 * Negative values are inside (sand), positive values are outside (air).
 *
 * The grid covers `cells` cells per axis, which needs `cells + 2` samples per
 * axis (one extra layer like the original prototype, so cell lookups never go
 * out of bounds).
 */
export class VoxelGrid {
  readonly cells: number;
  readonly dim: number;
  readonly sdf: Float32Array;

  constructor(cells: number) {
    this.cells = cells;
    this.dim = cells + 2;
    this.sdf = new Float32Array(this.dim * this.dim * this.dim);
  }

  index(x: number, y: number, z: number): number {
    return (x * this.dim + y) * this.dim + z;
  }

  get(x: number, y: number, z: number): number {
    return this.sdf[(x * this.dim + y) * this.dim + z];
  }

  set(x: number, y: number, z: number, v: number): void {
    this.sdf[(x * this.dim + y) * this.dim + z] = v;
  }

  inBounds(x: number, y: number, z: number): boolean {
    return x >= 0 && y >= 0 && z >= 0 && x < this.dim && y < this.dim && z < this.dim;
  }

  /** Fill with "air" whose value grows with height, like the original floor preset. */
  fillAir(): void {
    const { dim } = this;
    for (let x = 0; x < dim; x++) {
      for (let y = 0; y < dim; y++) {
        const v = 1 + y;
        for (let z = 0; z < dim; z++) {
          this.sdf[(x * dim + y) * dim + z] = v;
        }
      }
    }
  }

  /** Trilinearly interpolated field value at an arbitrary (grid space) point. */
  sample(x: number, y: number, z: number): number {
    const max = this.dim - 1;
    const cx = Math.min(Math.max(x, 0), max - 1e-4);
    const cy = Math.min(Math.max(y, 0), max - 1e-4);
    const cz = Math.min(Math.max(z, 0), max - 1e-4);
    const x0 = Math.floor(cx);
    const y0 = Math.floor(cy);
    const z0 = Math.floor(cz);
    const fx = cx - x0;
    const fy = cy - y0;
    const fz = cz - z0;

    const v000 = this.get(x0, y0, z0);
    const v001 = this.get(x0, y0, z0 + 1);
    const v010 = this.get(x0, y0 + 1, z0);
    const v011 = this.get(x0, y0 + 1, z0 + 1);
    const v100 = this.get(x0 + 1, y0, z0);
    const v101 = this.get(x0 + 1, y0, z0 + 1);
    const v110 = this.get(x0 + 1, y0 + 1, z0);
    const v111 = this.get(x0 + 1, y0 + 1, z0 + 1);

    const v00 = v000 + (v001 - v000) * fz;
    const v01 = v010 + (v011 - v010) * fz;
    const v10 = v100 + (v101 - v100) * fz;
    const v11 = v110 + (v111 - v110) * fz;
    const v0 = v00 + (v01 - v00) * fy;
    const v1 = v10 + (v11 - v10) * fy;
    return v0 + (v1 - v0) * fx;
  }

  /**
   * Numerical gradient of the field at an arbitrary point, normalized.
   * Used as the surface normal estimate for dual contouring.
   */
  gradient(x: number, y: number, z: number, out: [number, number, number]): void {
    const h = 0.5;
    let gx = this.sample(x + h, y, z) - this.sample(x - h, y, z);
    let gy = this.sample(x, y + h, z) - this.sample(x, y - h, z);
    let gz = this.sample(x, y, z + h) - this.sample(x, y, z - h);
    const len = Math.hypot(gx, gy, gz);
    if (len > 1e-9) {
      gx /= len;
      gy /= len;
      gz /= len;
    } else {
      gx = 0;
      gy = 1;
      gz = 0;
    }
    out[0] = gx;
    out[1] = gy;
    out[2] = gz;
  }

  /** Number of solid (sand) samples; handy invariant for tests. */
  countSolid(contour = 0): number {
    let n = 0;
    for (let i = 0; i < this.sdf.length; i++) {
      if (this.sdf[i] < contour) n++;
    }
    return n;
  }
}
