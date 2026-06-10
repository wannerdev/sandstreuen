import { describe, expect, it } from 'vitest';
import { sdCone, sdSphere, perlin2, perlin3 } from '../src/core/sdf';

const APERTURE = 0.56;
const SIN = Math.sin(APERTURE);
const COS = Math.cos(APERTURE);
const H = 5;

describe('sdCone', () => {
  it('is negative inside the cone body', () => {
    // Straight below the apex, halfway down.
    expect(sdCone(0, -H / 2, 0, SIN, COS, H)).toBeLessThan(0);
    expect(sdCone(0.5, -H + 0.5, 0.5, SIN, COS, H)).toBeLessThan(0);
  });

  it('is positive outside the cone', () => {
    expect(sdCone(0, 2, 0, SIN, COS, H)).toBeGreaterThan(0); // above the apex
    expect(sdCone(H * 2, -1, 0, SIN, COS, H)).toBeGreaterThan(0); // beside it
    expect(sdCone(0, -H * 2, 0, SIN, COS, H)).toBeGreaterThan(0); // below the base
  });

  it('is approximately zero on the surface near the apex', () => {
    expect(Math.abs(sdCone(0, 0, 0, SIN, COS, H))).toBeLessThan(1e-6);
  });

  it('widens with a larger aperture', () => {
    const narrow = sdCone(3, -4, 0, Math.sin(0.26), Math.cos(0.26), H);
    const wide = sdCone(3, -4, 0, Math.sin(0.7), Math.cos(0.7), H);
    expect(wide).toBeLessThan(narrow);
  });
});

describe('sdSphere', () => {
  it('matches the analytic distance', () => {
    expect(sdSphere(0, 0, 0, 0, 0, 0, 2)).toBeCloseTo(-2);
    expect(sdSphere(3, 0, 0, 0, 0, 0, 2)).toBeCloseTo(1);
  });
});

describe('perlin noise', () => {
  it('stays roughly within [0, 1] and is deterministic', () => {
    for (let i = 0; i < 200; i++) {
      const v = perlin2(i * 0.173, i * 0.0913);
      expect(v).toBeGreaterThanOrEqual(-0.2);
      expect(v).toBeLessThanOrEqual(1.2);
    }
    expect(perlin3(1.5, 2.5, 3.5)).toBe(perlin3(1.5, 2.5, 3.5));
  });

  it('is not constant', () => {
    const values = new Set<number>();
    for (let i = 0; i < 50; i++) values.add(perlin3(i * 0.31, i * 0.17, i * 0.53));
    expect(values.size).toBeGreaterThan(10);
  });
});
