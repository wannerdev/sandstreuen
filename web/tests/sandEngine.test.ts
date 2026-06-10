import { describe, expect, it } from 'vitest';
import { SandEngine } from '../src/core/sandEngine';

describe('SandEngine', () => {
  it('starts with a cone of sand and produces a mesh', () => {
    const engine = new SandEngine(32, 'cone');
    expect(engine.grid.countSolid()).toBeGreaterThan(0);
    const mesh = engine.buildMeshIfDirty();
    expect(mesh).not.toBeNull();
    expect(mesh!.indices.length).toBeGreaterThan(0);
    // Nothing changed, so no rebuild on the next call.
    expect(engine.buildMeshIfDirty()).toBeNull();
  });

  it('adds sand in cone mode and rejects edits outside the area', () => {
    const engine = new SandEngine(32, 'empty');
    expect(engine.grid.countSolid()).toBe(0);

    expect(engine.applyEdit('cone', 16, 0, 16, 0)).toBe(true);
    expect(engine.grid.countSolid()).toBeGreaterThan(0);

    expect(engine.applyEdit('cone', 200, 0, 16, 0)).toBe(false);
    expect(engine.applyEdit('cone', -10, 0, 16, 0)).toBe(false);
  });

  it('placing a second cone never erases the first (union)', () => {
    const engine = new SandEngine(32, 'empty');
    engine.applyEdit('cone', 12, 0, 16, 0);
    const after1 = engine.grid.countSolid();
    engine.applyEdit('cone', 20, 0, 16, 0);
    expect(engine.grid.countSolid()).toBeGreaterThanOrEqual(after1);
  });

  it('single mode places and remove mode carves sand', () => {
    const engine = new SandEngine(32, 'empty');
    engine.gravityEnabled = false;

    engine.applyEdit('single', 16, 4, 16, 0);
    const placed = engine.grid.countSolid();
    expect(placed).toBeGreaterThan(0);

    engine.applyEdit('remove', 16, 5.5, 16, 0);
    expect(engine.grid.countSolid()).toBeLessThan(placed);
  });

  it('grow mode raises the pile when applied repeatedly at one spot', () => {
    const engine = new SandEngine(32, 'empty');
    engine.gravityEnabled = false;
    engine.applyEdit('grow', 16, 0, 16, 0);
    const first = engine.grid.countSolid();
    engine.applyEdit('grow', 16, 0, 16, 0);
    engine.applyEdit('grow', 16, 0, 16, 0);
    expect(engine.grid.countSolid()).toBeGreaterThan(first);
  });

  it('gravity ticks settle a floating blob and dirty meshes are rebuilt', () => {
    const engine = new SandEngine(24, 'empty');
    engine.applyEdit('single', 12, 10, 12, 0);
    engine.buildMeshIfDirty();

    let movedAny = false;
    for (let i = 0; i < 50; i++) {
      if (engine.tickGravity()) movedAny = true;
    }
    expect(movedAny).toBe(true);
    expect(engine.buildMeshIfDirty()).not.toBeNull();
  });

  it('reset restores the preset', () => {
    const engine = new SandEngine(24, 'empty');
    engine.applyEdit('cone', 12, 0, 12, 0);
    engine.reset('empty');
    expect(engine.grid.countSolid()).toBe(0);
    const mesh = engine.buildMeshIfDirty();
    expect(mesh).not.toBeNull();
    expect(mesh!.indices.length).toBe(0);
  });
});
