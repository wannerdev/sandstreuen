/**
 * Sand materials from the original prototype. The aperture is the half-angle
 * (radians) of the deposited cone — wetter sand piles up steeper.
 */
export interface SandMaterial {
  id: string;
  label: string;
  aperture: number;
  color: number;
}

export const SAND_MATERIALS: readonly SandMaterial[] = [
  { id: 'sandDry', label: 'Dry sand', aperture: 0.56, color: 0xd9c08f },
  { id: 'sandWetStart', label: 'Wet sand (start)', aperture: 0.26, color: 0xb89a6a },
  { id: 'sandWetEnd', label: 'Wet sand (end)', aperture: 0.5, color: 0xc4a877 },
  { id: 'sandWet', label: 'Wet sand', aperture: 0.7, color: 0xa68a5b },
];

export const EDIT_MODES = ['cone', 'single', 'remove', 'grow'] as const;
export type EditMode = (typeof EDIT_MODES)[number];
