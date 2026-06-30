import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { MeshData } from '../core/dualContouring';

/**
 * Three.js scene: the sand mesh, a ground disc, lights and orbit camera.
 * The voxel grid lives in [0, cells]³ but the sandbox is always `worldSize`
 * units across: the mesh is scaled by `voxelScale = worldSize / cells` and
 * centered on the origin, so the grid resolution never changes the camera framing.
 */
export class SandScene {
  readonly renderer: THREE.WebGLRenderer;
  readonly scene: THREE.Scene;
  readonly camera: THREE.PerspectiveCamera;
  readonly controls: OrbitControls;

  private readonly worldSize: number;
  private readonly voxelScale: number;
  private readonly sandGeometry = new THREE.BufferGeometry();
  private readonly sandMaterial: THREE.MeshStandardMaterial;
  private readonly sandMesh: THREE.Mesh;
  private readonly ground: THREE.Mesh;
  private readonly raycaster = new THREE.Raycaster();

  constructor(container: HTMLElement, cells: number, worldSize = cells) {
    this.worldSize = worldSize;
    this.voxelScale = worldSize / cells;

    this.renderer = new THREE.WebGLRenderer({ antialias: true });
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    this.renderer.setSize(window.innerWidth, window.innerHeight);
    this.renderer.shadowMap.enabled = true;
    this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    container.appendChild(this.renderer.domElement);

    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color(0x181612);
    this.scene.fog = new THREE.Fog(0x181612, worldSize * 2, worldSize * 6);

    this.camera = new THREE.PerspectiveCamera(
      50,
      window.innerWidth / window.innerHeight,
      0.1,
      worldSize * 10,
    );
    this.camera.position.set(0, worldSize * 0.65, worldSize * 1.05);

    this.controls = new OrbitControls(this.camera, this.renderer.domElement);
    this.controls.target.set(0, 4, 0);
    this.controls.enableDamping = true;
    this.controls.dampingFactor = 0.08;
    this.controls.maxPolarAngle = Math.PI * 0.49;
    this.controls.minDistance = 8;
    this.controls.maxDistance = worldSize * 3;
    // Left button paints sand; orbit with the right button / two fingers.
    this.controls.mouseButtons = {
      LEFT: null,
      MIDDLE: THREE.MOUSE.PAN,
      RIGHT: THREE.MOUSE.ROTATE,
    };
    this.controls.touches = {
      ONE: null,
      TWO: THREE.TOUCH.DOLLY_ROTATE,
    };

    const hemi = new THREE.HemisphereLight(0xfff4dd, 0x7a6a4e, 1.0);
    this.scene.add(hemi);
    this.scene.add(new THREE.AmbientLight(0xfff4dd, 0.25));

    const sun = new THREE.DirectionalLight(0xffe8c0, 2.2);
    sun.position.set(worldSize * 0.8, worldSize * 1.2, worldSize * 0.5);
    sun.castShadow = true;
    sun.shadow.mapSize.set(2048, 2048);
    const s = worldSize * 0.9;
    sun.shadow.camera.left = -s;
    sun.shadow.camera.right = s;
    sun.shadow.camera.top = s;
    sun.shadow.camera.bottom = -s;
    sun.shadow.camera.far = worldSize * 4;
    this.scene.add(sun);

    this.ground = new THREE.Mesh(
      new THREE.CircleGeometry(worldSize * 0.95, 64),
      new THREE.MeshStandardMaterial({ color: 0x2c2820, roughness: 1 }),
    );
    this.ground.rotation.x = -Math.PI / 2;
    this.ground.position.y = 0;
    this.ground.receiveShadow = true;
    this.scene.add(this.ground);

    this.sandMaterial = new THREE.MeshStandardMaterial({
      color: 0xd9c08f,
      roughness: 0.95,
      metalness: 0,
    });
    this.sandMesh = new THREE.Mesh(this.sandGeometry, this.sandMaterial);
    // The mesh is built in grid space ([0, cells]); scale it to the fixed
    // world size and center it, so resolution changes detail, not framing.
    this.sandMesh.scale.setScalar(this.voxelScale);
    this.sandMesh.position.set(-worldSize / 2, 0, -worldSize / 2);
    this.sandMesh.castShadow = true;
    this.sandMesh.receiveShadow = true;
    this.scene.add(this.sandMesh);

    window.addEventListener('resize', this.onResize);
  }

  private onResize = (): void => {
    this.camera.aspect = window.innerWidth / window.innerHeight;
    this.camera.updateProjectionMatrix();
    this.renderer.setSize(window.innerWidth, window.innerHeight);
  };

  setSandColor(color: number): void {
    this.sandMaterial.color.setHex(color);
  }

  updateMesh(data: MeshData): void {
    this.sandGeometry.setAttribute('position', new THREE.BufferAttribute(data.positions, 3));
    this.sandGeometry.setIndex(new THREE.BufferAttribute(data.indices, 1));
    // The vertex count changes between rebuilds; computeVertexNormals would
    // silently reuse (and underfill) a stale normal attribute.
    this.sandGeometry.deleteAttribute('normal');
    this.sandGeometry.computeVertexNormals();
    this.sandGeometry.computeBoundingSphere();
  }

  /**
   * Cast a ray from normalized device coordinates into the scene and return
   * the hit point on the sand surface (or the ground) in grid space.
   */
  pick(ndcX: number, ndcY: number): { x: number; y: number; z: number } | null {
    this.raycaster.setFromCamera(new THREE.Vector2(ndcX, ndcY), this.camera);
    const hits = this.raycaster.intersectObjects([this.sandMesh, this.ground], false);
    if (hits.length === 0) return null;
    const p = hits[0].point;
    // World space → grid space (the engine works in [0, cells]).
    const s = this.voxelScale;
    return {
      x: (p.x + this.worldSize / 2) / s,
      y: Math.max(0, p.y / s),
      z: (p.z + this.worldSize / 2) / s,
    };
  }

  render(): void {
    this.controls.update();
    this.renderer.render(this.scene, this.camera);
  }
}
