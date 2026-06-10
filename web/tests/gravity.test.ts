import { describe, expect, it } from 'vitest';
import { VoxelGrid } from '../src/core/voxelGrid';
import { stepGravity } from '../src/core/gravity';

function settle(grid: VoxelGrid, maxSteps = 200): number {
  let steps = 0;
  while (steps < maxSteps) {
    if (stepGravity(grid, steps++) === 0) break;
  }
  return steps;
}

function maxSolidHeight(grid: VoxelGrid): number {
  let max = -1;
  for (let x = 0; x < grid.dim; x++) {
    for (let y = 0; y < grid.dim; y++) {
      for (let z = 0; z < grid.dim; z++) {
        if (grid.get(x, y, z) < 0 && y > max) max = y;
      }
    }
  }
  return max;
}

describe('stepGravity', () => {
  it('drops a floating grain one cell per step until it reaches the ground', () => {
    const grid = new VoxelGrid(10);
    grid.fillAir();
    grid.set(5, 5, 5, -1);

    expect(stepGravity(grid, 0)).toBe(1);
    expect(grid.get(5, 4, 5)).toBeLessThan(0);
    expect(grid.get(5, 5, 5)).toBeGreaterThan(0);

    settle(grid);
    expect(grid.get(5, 0, 5)).toBeLessThan(0);
    expect(grid.countSolid()).toBe(1);
  });

  it('conserves the amount of sand', () => {
    const grid = new VoxelGrid(12);
    grid.fillAir();
    for (let y = 0; y < 6; y++) grid.set(6, y, 6, -1);
    for (let y = 3; y < 8; y++) grid.set(3, y, 3, -1);

    const before = grid.countSolid();
    settle(grid);
    expect(grid.countSolid()).toBe(before);
  });

  it('topples a single column into a flatter pile', () => {
    const grid = new VoxelGrid(12);
    grid.fillAir();
    for (let y = 0; y < 6; y++) grid.set(6, y, 6, -1);

    const heightBefore = maxSolidHeight(grid);
    settle(grid);
    expect(maxSolidHeight(grid)).toBeLessThan(heightBefore);
  });

  it('does not move supported sand', () => {
    const grid = new VoxelGrid(8);
    grid.fillAir();
    // A solid 3x1x3 slab on the ground is stable.
    for (let x = 3; x <= 5; x++) {
      for (let z = 3; z <= 5; z++) {
        grid.set(x, 0, z, -1);
      }
    }
    expect(stepGravity(grid, 0)).toBe(0);
  });
});
