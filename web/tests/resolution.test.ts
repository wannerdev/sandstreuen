import { describe, expect, it } from 'vitest';
import { SandEngine } from '../src/core/sandEngine';

/**
 * `?size` controls the grid *resolution*, not the sandbox size: the physical
 * volume and brush sizes stay constant while a higher `cells` count just
 * subdivides the same world more finely. These tests pin that behaviour.
 */
describe('resolution (fixed world size)', () => {
  const WORLD = 64;

  /** Highest sand vertex, in world units (grid space scaled by worldSize/cells). */
  const worldHeight = (engine: SandEngine): number => {
    const mesh = engine.buildMeshIfDirty()!;
    const scale = WORLD / engine.cells;
    let maxY = 0;
    for (let i = 1; i < mesh.positions.length; i += 3) {
      maxY = Math.max(maxY, mesh.positions[i]);
    }
    return maxY * scale;
  };

  it('keeps the starting cone the same physical height at any resolution', () => {
    const low = worldHeight(new SandEngine(32, 'cone', WORLD));
    const high = worldHeight(new SandEngine(96, 'cone', WORLD));
    expect(low).toBeGreaterThan(0);
    // Same physical sandbox => same world-space pile height, independent of grid.
    // (Without scaling the brushes, these would differ ~3x.)
    expect(Math.abs(high - low)).toBeLessThan(3);
  });

  it('produces a finer mesh at higher resolution', () => {
    const low = new SandEngine(32, 'cone', WORLD).buildMeshIfDirty()!;
    const high = new SandEngine(96, 'cone', WORLD).buildMeshIfDirty()!;
    expect(high.indices.length).toBeGreaterThan(low.indices.length * 4);
  });

  it('scales brush edits so a deposited cone covers the same physical volume', () => {
    const low = new SandEngine(32, 'empty', WORLD);
    low.gravityEnabled = false;
    low.applyEdit('cone', 16, 0, 16, 0); // grid center of a 32³ box

    const high = new SandEngine(96, 'empty', WORLD);
    high.gravityEnabled = false;
    high.applyEdit('cone', 48, 0, 48, 0); // grid center of a 96³ box

    // ~ (96/32)^3 = 27x more solid cells for the same physical cone; without
    // brush scaling the ratio would be ~1.
    const ratio = high.grid.countSolid() / low.grid.countSolid();
    expect(ratio).toBeGreaterThan(15);
  });

  it('defaults to one cell per unit (original grid-space behaviour)', () => {
    expect(new SandEngine(64, 'empty').voxelsPerUnit).toBe(1);
    expect(new SandEngine(96, 'empty', WORLD).voxelsPerUnit).toBe(1.5);
  });
});
