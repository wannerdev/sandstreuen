import { VoxelGrid } from './voxelGrid';
import { DirtyRegion } from './dirtyRegion';

/**
 * Value written into cells a grain has just vacated. Magnitude 1 keeps the
 * field well-behaved near the surface (crossings stay near edge midpoints).
 */
const VACATED = 1;

/**
 * One step of the cellular sand simulation, ported from the original
 * prototype's gravity(): a solid cell falls straight down if unsupported,
 * and a surface grain (air above) topples diagonally down when there is room.
 *
 * Unlike the original, the four topple directions are tried in a rotating
 * order so piles spread evenly instead of drifting towards +x.
 *
 * Returns the number of grains that moved.
 */
export function stepGravity(grid: VoxelGrid, step: number, dirty?: DirtyRegion): number {
  const n = grid.cells;
  let moved = 0;

  const move = (sx: number, sy: number, sz: number, tx: number, ty: number, tz: number) => {
    grid.set(tx, ty, tz, grid.get(sx, sy, sz));
    grid.set(sx, sy, sz, VACATED);
    moved++;
    dirty?.include(sx, sy, sz);
    dirty?.include(tx, ty, tz);
  };

  // x offset / z offset per topple direction
  const DIRS = [
    [1, 0],
    [-1, 0],
    [0, 1],
    [0, -1],
  ];

  for (let y = 1; y <= n; y++) {
    for (let x = 0; x <= n; x++) {
      for (let z = 0; z <= n; z++) {
        if (grid.get(x, y, z) >= 0) continue;

        if (grid.get(x, y - 1, z) >= 0) {
          move(x, y, z, x, y - 1, z);
          continue;
        }

        // Only surface grains topple.
        if (grid.get(x, y + 1, z) <= 0) continue;

        const rot = (step + x + z) & 3;
        for (let d = 0; d < 4; d++) {
          const [ox, oz] = DIRS[(d + rot) & 3];
          const tx = x + ox;
          const tz = z + oz;
          if (tx < 0 || tz < 0 || tx > n || tz > n) continue;
          if (grid.get(tx, y - 1, tz) >= 0 && grid.get(tx, y, tz) >= 0) {
            move(x, y, z, tx, y - 1, tz);
            break;
          }
        }
      }
    }
  }

  return moved;
}
