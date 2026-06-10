/**
 * Signed distance functions, ported from the original Unity prototype (Bodies.cs).
 * Cone SDF credit: Inigo Quilez, https://iquilezles.org/articles/distfunctions/
 */

const clamp = (v: number, lo: number, hi: number) => Math.min(hi, Math.max(lo, v));

/**
 * Exact signed distance to a cone with its apex at the origin, opening
 * downwards to a base at y = -h. `sinA`/`cosA` describe the half-angle
 * (angle of repose) of the cone.
 */
export function sdCone(
  px: number,
  py: number,
  pz: number,
  sinA: number,
  cosA: number,
  h: number,
): number {
  // q is the 2D point at the base of the cone.
  const qx = h * (sinA / cosA);
  const qy = -h;

  const wx = Math.hypot(px, pz);
  const wy = py;

  const t = clamp((wx * qx + wy * qy) / (qx * qx + qy * qy), 0, 1);
  const ax = wx - qx * t;
  const ay = wy - qy * t;

  const s = clamp(wx / qx, 0, 1);
  const bx = wx - qx * s;
  const by = wy - qy;

  const k = Math.sign(qy);
  const d = Math.min(ax * ax + ay * ay, bx * bx + by * by);
  const sign = Math.max(k * (wx * qy - wy * qx), k * (wy - qy));
  return Math.sqrt(d) * Math.sign(sign);
}

/** Distance to a sphere, used by tests and the demo presets. */
export function sdSphere(
  px: number,
  py: number,
  pz: number,
  cx: number,
  cy: number,
  cz: number,
  r: number,
): number {
  return Math.hypot(px - cx, py - cy, pz - cz) - r;
}

// --- 2D Perlin noise (replacement for Mathf.PerlinNoise) ---------------------

const PERM = new Uint8Array(512);
{
  // Deterministic permutation table (mulberry32 with a fixed seed).
  let s = 0x5eed5;
  const rand = () => {
    s |= 0;
    s = (s + 0x6d2b79f5) | 0;
    let t = Math.imul(s ^ (s >>> 15), 1 | s);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
  const p = Array.from({ length: 256 }, (_, i) => i);
  for (let i = 255; i > 0; i--) {
    const j = Math.floor(rand() * (i + 1));
    [p[i], p[j]] = [p[j], p[i]];
  }
  for (let i = 0; i < 512; i++) PERM[i] = p[i & 255];
}

const fade = (t: number) => t * t * t * (t * (t * 6 - 15) + 10);
const lerp = (a: number, b: number, t: number) => a + (b - a) * t;

function grad2(hash: number, x: number, y: number): number {
  switch (hash & 3) {
    case 0: return x + y;
    case 1: return -x + y;
    case 2: return x - y;
    default: return -x - y;
  }
}

/** Classic 2D Perlin noise, remapped to roughly [0, 1] like Unity's Mathf.PerlinNoise. */
export function perlin2(x: number, y: number): number {
  const xi = Math.floor(x) & 255;
  const yi = Math.floor(y) & 255;
  const xf = x - Math.floor(x);
  const yf = y - Math.floor(y);
  const u = fade(xf);
  const v = fade(yf);

  const aa = PERM[PERM[xi] + yi];
  const ab = PERM[PERM[xi] + yi + 1];
  const ba = PERM[PERM[xi + 1] + yi];
  const bb = PERM[PERM[xi + 1] + yi + 1];

  const val = lerp(
    lerp(grad2(aa, xf, yf), grad2(ba, xf - 1, yf), u),
    lerp(grad2(ab, xf, yf - 1), grad2(bb, xf - 1, yf - 1), u),
    v,
  );
  return val * 0.5 + 0.5;
}

/** Pseudo 3D Perlin noise, same averaging trick as the original prototype. */
export function perlin3(x: number, y: number, z: number): number {
  return (
    (perlin2(x, y) + perlin2(x, z) + perlin2(y, z) +
      perlin2(y, x) + perlin2(z, x) + perlin2(z, y)) / 6
  );
}
