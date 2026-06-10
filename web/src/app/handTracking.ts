/**
 * Hand tracking via MediaPipe's gesture recognizer — the modern, open
 * replacement for the discontinued Manomotion SDK used by the original app.
 *
 * Interactions mirror the original prototype:
 * - pinch (thumb + index finger) ... spread sand at the fingertip
 * - closed fist                  ... next material
 * - open palm                    ... next edit mode
 */
import type { GestureRecognizer } from '@mediapipe/tasks-vision';

export interface HandCallbacks {
  /** Continuous while pinching; coordinates are NDC (-1..1, mirrored). */
  onPinch(ndcX: number, ndcY: number): void;
  onFist(): void;
  onOpenPalm(): void;
  onGesture(label: string): void;
}

const WASM_URL = 'https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.21/wasm';
const MODEL_URL =
  'https://storage.googleapis.com/mediapipe-models/gesture_recognizer/gesture_recognizer/float16/1/gesture_recognizer.task';

const PINCH_THRESHOLD = 0.07;
const GESTURE_HOLD_MS = 350;
const GESTURE_COOLDOWN_MS = 1200;

export class HandTracking {
  private recognizer: GestureRecognizer | null = null;
  private stream: MediaStream | null = null;
  private running = false;
  private lastVideoTime = -1;

  private heldGesture = '';
  private heldSince = 0;
  private lastFired = 0;

  constructor(
    private readonly video: HTMLVideoElement,
    private readonly cb: HandCallbacks,
  ) {}

  async start(): Promise<void> {
    const { GestureRecognizer, FilesetResolver } = await import('@mediapipe/tasks-vision');
    const vision = await FilesetResolver.forVisionTasks(WASM_URL);
    const options = (delegate: 'GPU' | 'CPU') => ({
      baseOptions: { modelAssetPath: MODEL_URL, delegate },
      runningMode: 'VIDEO' as const,
      numHands: 1,
    });
    try {
      this.recognizer = await GestureRecognizer.createFromOptions(vision, options('GPU'));
    } catch {
      this.recognizer = await GestureRecognizer.createFromOptions(vision, options('CPU'));
    }

    this.stream = await navigator.mediaDevices.getUserMedia({
      video: { facingMode: 'user', width: 640, height: 480 },
      audio: false,
    });
    this.video.srcObject = this.stream;
    await this.video.play();

    this.running = true;
    requestAnimationFrame(this.loop);
  }

  stop(): void {
    this.running = false;
    this.stream?.getTracks().forEach((t) => t.stop());
    this.stream = null;
    this.recognizer?.close();
    this.recognizer = null;
  }

  get active(): boolean {
    return this.running;
  }

  private loop = (): void => {
    if (!this.running || !this.recognizer) return;
    requestAnimationFrame(this.loop);

    if (this.video.currentTime === this.lastVideoTime) return;
    this.lastVideoTime = this.video.currentTime;

    const result = this.recognizer.recognizeForVideo(this.video, performance.now());
    const landmarks = result.landmarks?.[0];
    const gesture = result.gestures?.[0]?.[0]?.categoryName ?? 'None';
    const now = performance.now();

    if (!landmarks) {
      this.heldGesture = '';
      this.cb.onGesture('no hand');
      return;
    }

    // Pinch: thumb tip (4) close to index fingertip (8).
    const thumb = landmarks[4];
    const index = landmarks[8];
    const pinchDist = Math.hypot(thumb.x - index.x, thumb.y - index.y, thumb.z - index.z);
    if (pinchDist < PINCH_THRESHOLD) {
      // The preview is mirrored, so mirror x for a natural mapping.
      const ndcX = -(index.x * 2 - 1);
      const ndcY = -(index.y * 2 - 1);
      this.cb.onGesture('pinch');
      this.cb.onPinch(ndcX, ndcY);
      this.heldGesture = '';
      return;
    }

    this.cb.onGesture(gesture === 'None' ? 'hand' : gesture.replace('_', ' ').toLowerCase());

    if (gesture !== 'Closed_Fist' && gesture !== 'Open_Palm') {
      this.heldGesture = '';
      return;
    }
    if (gesture !== this.heldGesture) {
      this.heldGesture = gesture;
      this.heldSince = now;
      return;
    }
    if (now - this.heldSince >= GESTURE_HOLD_MS && now - this.lastFired >= GESTURE_COOLDOWN_MS) {
      this.lastFired = now;
      this.heldSince = now;
      if (gesture === 'Closed_Fist') this.cb.onFist();
      else this.cb.onOpenPalm();
    }
  };
}
