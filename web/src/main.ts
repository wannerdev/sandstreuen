import { SandEngine } from './core/sandEngine';
import { SAND_MATERIALS, EDIT_MODES, EditMode } from './core/materials';
import { SandScene } from './app/scene';
import type { HandTracking } from './app/handTracking';

const GRAVITY_HZ = 15;
const PAINT_INTERVAL_MS = 90;

// --- world ------------------------------------------------------------------

// The sandbox is always WORLD units across; `?size` is the grid *resolution*,
// i.e. how finely that fixed volume is subdivided — higher = sharper sand,
// same physical sandbox and brush sizes. No upper limit (the grid and the
// per-frame gravity scan grow with the cube of the resolution, so very large
// values get heavy); the floor of 32 just keeps the grid non-degenerate.
const WORLD = 64;
const params = new URLSearchParams(location.search);
const cells = Math.max(32, Math.round(Number(params.get('size')) || 64));

const engine = new SandEngine(cells, 'cone', WORLD);
const scene = new SandScene(document.getElementById('app')!, cells, WORLD);

// --- state ------------------------------------------------------------------

let mode: EditMode = 'cone';
let materialIndex = 0;
let lastPaint = 0;
let warningTimer = 0;

// --- hud --------------------------------------------------------------------

const statusMode = document.getElementById('status-mode')!;
const statusMaterial = document.getElementById('status-material')!;
const statusWarning = document.getElementById('status-warning')!;
const modeButtons = document.getElementById('mode-buttons')!;
const materialButtons = document.getElementById('material-buttons')!;

for (const m of EDIT_MODES) {
  const b = document.createElement('button');
  b.textContent = m;
  b.dataset.mode = m;
  b.addEventListener('click', () => setMode(m));
  modeButtons.appendChild(b);
}
SAND_MATERIALS.forEach((m, i) => {
  const b = document.createElement('button');
  b.textContent = m.label;
  b.dataset.material = String(i);
  b.addEventListener('click', () => setMaterial(i));
  materialButtons.appendChild(b);
});

function setMode(m: EditMode): void {
  mode = m;
  statusMode.textContent = `Mode: ${m}`;
  modeButtons.querySelectorAll('button').forEach((b) => {
    b.classList.toggle('active', b.dataset.mode === m);
  });
}

function setMaterial(i: number): void {
  materialIndex = i;
  const m = SAND_MATERIALS[i];
  statusMaterial.textContent = `Material: ${m.label}`;
  scene.setSandColor(m.color);
  materialButtons.querySelectorAll('button').forEach((b) => {
    b.classList.toggle('active', b.dataset.material === String(i));
  });
}

function cycleMode(): void {
  setMode(EDIT_MODES[(EDIT_MODES.indexOf(mode) + 1) % EDIT_MODES.length]);
}

function cycleMaterial(): void {
  setMaterial((materialIndex + 1) % SAND_MATERIALS.length);
}

function warnOutside(): void {
  statusWarning.textContent = 'Outside of area';
  clearTimeout(warningTimer);
  warningTimer = window.setTimeout(() => (statusWarning.textContent = ''), 1200);
}

setMode('cone');
setMaterial(0);

const gravityBtn = document.getElementById('btn-gravity') as HTMLButtonElement;
gravityBtn.addEventListener('click', () => {
  engine.gravityEnabled = !engine.gravityEnabled;
  gravityBtn.textContent = `Gravity: ${engine.gravityEnabled ? 'on' : 'off'}`;
});

document.getElementById('btn-reset')!.addEventListener('click', () => engine.reset('cone'));

// --- painting ---------------------------------------------------------------

function paintAt(ndcX: number, ndcY: number, force = false): void {
  const now = performance.now();
  if (!force && now - lastPaint < PAINT_INTERVAL_MS) return;
  const hit = scene.pick(ndcX, ndcY);
  if (!hit) return;
  lastPaint = now;
  if (!engine.applyEdit(mode, hit.x, hit.y, hit.z, materialIndex)) {
    warnOutside();
  }
}

const canvas = scene.renderer.domElement;
let painting = false;

const toNdc = (e: PointerEvent): [number, number] => [
  (e.clientX / window.innerWidth) * 2 - 1,
  -(e.clientY / window.innerHeight) * 2 + 1,
];

canvas.addEventListener('pointerdown', (e) => {
  if (e.button !== 0 || !e.isPrimary) return;
  painting = true;
  paintAt(...toNdc(e), true);
});
canvas.addEventListener('pointermove', (e) => {
  if (!painting || !e.isPrimary) return;
  paintAt(...toNdc(e));
});
const stopPainting = () => (painting = false);
canvas.addEventListener('pointerup', stopPainting);
canvas.addEventListener('pointercancel', stopPainting);
canvas.addEventListener('pointerleave', stopPainting);

// --- hand tracking (lazy loaded) ---------------------------------------------

const handsBtn = document.getElementById('btn-hands') as HTMLButtonElement;
const handPanel = document.getElementById('hand-panel')!;
const handGesture = document.getElementById('hand-gesture')!;
let hands: HandTracking | null = null;

handsBtn.addEventListener('click', async () => {
  if (hands?.active) {
    hands.stop();
    hands = null;
    handPanel.hidden = true;
    handsBtn.textContent = '✋ Enable hand tracking';
    return;
  }
  handsBtn.disabled = true;
  handsBtn.textContent = 'Loading model…';
  try {
    const { HandTracking } = await import('./app/handTracking');
    hands = new HandTracking(document.getElementById('hand-video') as HTMLVideoElement, {
      onPinch: (x, y) => paintAt(x, y),
      onFist: cycleMaterial,
      onOpenPalm: cycleMode,
      onGesture: (label) => (handGesture.textContent = label),
    });
    await hands.start();
    handPanel.hidden = false;
    handsBtn.textContent = '✋ Disable hand tracking';
  } catch (err) {
    console.error(err);
    handsBtn.textContent = '✋ Camera unavailable';
    hands = null;
  } finally {
    handsBtn.disabled = false;
  }
});

// --- main loop ----------------------------------------------------------------

let gravityAccumulator = 0;
let lastTime = performance.now();

function frame(now: number): void {
  requestAnimationFrame(frame);

  gravityAccumulator += Math.min(now - lastTime, 250);
  lastTime = now;
  const tick = 1000 / GRAVITY_HZ;
  while (gravityAccumulator >= tick) {
    gravityAccumulator -= tick;
    engine.tickGravity();
  }

  const meshData = engine.buildMeshIfDirty();
  if (meshData) scene.updateMesh(meshData);

  scene.render();
}

requestAnimationFrame(frame);
