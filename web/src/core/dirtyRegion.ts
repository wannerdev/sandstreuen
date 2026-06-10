/** Axis-aligned bounds of grid cells that changed since the last remesh. */
export class DirtyRegion {
  minX = Infinity;
  minY = Infinity;
  minZ = Infinity;
  maxX = -Infinity;
  maxY = -Infinity;
  maxZ = -Infinity;

  get empty(): boolean {
    return this.minX > this.maxX;
  }

  reset(): void {
    this.minX = this.minY = this.minZ = Infinity;
    this.maxX = this.maxY = this.maxZ = -Infinity;
  }

  include(x: number, y: number, z: number): void {
    if (x < this.minX) this.minX = x;
    if (y < this.minY) this.minY = y;
    if (z < this.minZ) this.minZ = z;
    if (x > this.maxX) this.maxX = x;
    if (y > this.maxY) this.maxY = y;
    if (z > this.maxZ) this.maxZ = z;
  }

  includeBox(
    minX: number,
    minY: number,
    minZ: number,
    maxX: number,
    maxY: number,
    maxZ: number,
  ): void {
    this.include(minX, minY, minZ);
    this.include(maxX, maxY, maxZ);
  }

  /** Grow the region by `r` cells in every direction. */
  expand(r: number): void {
    if (this.empty) return;
    this.minX -= r;
    this.minY -= r;
    this.minZ -= r;
    this.maxX += r;
    this.maxY += r;
    this.maxZ += r;
  }
}
