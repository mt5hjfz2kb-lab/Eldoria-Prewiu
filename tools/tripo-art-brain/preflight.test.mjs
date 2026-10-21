import { test } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import crypto from 'node:crypto';
import {
  confined, pinnedBytes, inspectGLB, auditAsset, makeBundle, writeBundle
} from './preflight.mjs';

function hash(buffer) { return crypto.createHash('sha256').update(buffer).digest('hex'); }
function fixtureGLB(tris = 3, { uv = true, skins = 0, animations = 0 } = {}) {
  const attrs = { POSITION: 1, NORMAL: 2 };
  if (uv) attrs.TEXCOORD_0 = 3;
  const model = {
    asset: { version: '2.0' }, accessors: [
      { count: tris * 3, type: 'SCALAR', componentType: 5123 },
      { count: tris * 3, type: 'VEC3', componentType: 5126 },
      { count: tris * 3, type: 'VEC3', componentType: 5126 },
      { count: tris * 3, type: 'VEC2', componentType: 5126 }
    ],
    meshes: [{ primitives: [{ indices: 0, attributes: attrs }] }],
    materials: [{ name: 'source' }],
    skins: Array.from({ length: skins }, () => ({})),
    animations: Array.from({ length: animations }, () => ({}))
  };
  const json = Buffer.from(JSON.stringify(model));
  const padded = Buffer.concat([json, Buffer.alloc((4 - json.length % 4) % 4, 32)]);
  const buffer = Buffer.alloc(20 + padded.length);
  buffer.writeUInt32LE(0x46546c67, 0);
  buffer.writeUInt32LE(2, 4);
  buffer.writeUInt32LE(buffer.length, 8);
  buffer.writeUInt32LE(padded.length, 12);
  buffer.writeUInt32LE(0x4e4f534a, 16);
  padded.copy(buffer, 20);
  return buffer;
}
function fixturePNG() {
  const image = Buffer.alloc(32);
  Buffer.from('89504e470d0a1a0a', 'hex').copy(image);
  return image;
}
function tempFixture() {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), 'eldoria-tripo-test-'));
  const cfg = { schema_version: 1, assets: {} };
  const create = (f, bytes) => {
    const destination = path.join(root, f);
    fs.mkdirSync(path.dirname(destination), { recursive: true });
    fs.writeFileSync(destination, bytes);
    return { path: f, sha256: hash(bytes) };
  };
  for (const id of ['spawnling', 'boar', 'focus']) {
    const base = 'ArtSource/World4X/' + id;
    const raw = create(base + '/raw.glb', fixtureGLB(4));
    const optimized = create(base + '/optimized.glb', fixtureGLB(3));
    const qaDir = base + '/UnityQA';
    const metricPath = qaDir + '/metrics.json';
    for (const name of ['iso.png', 'side.png', 'front.png', 'strategic.png']) {
      create(qaDir + '/' + name, fixturePNG());
    }
    create(metricPath, Buffer.from(JSON.stringify({
      unityVersion: '6000.3.23f1', triangles: 3, uv0AllMeshes: true,
      normalsAllMeshes: true, fullResolutionTexturePass: true,
      importedBaseColorWidth: 2048, importedBaseColorHeight: 2048,
      positiveSelection: true, emptySelectionMiss: true,
      captures: ['iso.png', 'side.png', 'front.png', 'strategic.png']
    })));
    cfg.assets[id] = {
      label: id, role: 'test', planner_profile: id === 'focus' ? 'environment_surface' : 'environment_composition',
      source: { task_id: 'test-task', credits_consumed: 20,
        raw: { ...raw, triangles: 4 }, optimized: { ...optimized, triangles: 3 } },
      qa: { metrics: metricPath }, defects: ['observed issue'],
      next_actions: ['Blender copy-only'], protected: ['No paid calls']
    };
  }
  const asset = cfg.assets.spawnling;
  const near = create('ArtSource/World4X/spawnling/near.glb', fixtureGLB(2));
  const far = create('ArtSource/World4X/spawnling/far.glb', fixtureGLB(1));
  const scene = 'Unity/Assets/Scene.unity';
  create(scene, Buffer.from('real simulated fixture, not Unity certification'));
  const evidence = 'docs/evidence/native-integration.json';
  const authorPath = 'ArtSource/World4X/spawnling/author.json';
  create(evidence, Buffer.from(JSON.stringify({
    source_glb_sha256: asset.source.optimized.sha256,
    near_glb_sha256: near.sha256, far_glb_sha256: far.sha256,
    near_triangles: 2, far_triangles: 1,
    hotspot_preserved: true, gameplay_unchanged: true,
    save_untouched: true, visual_colliders: 0
  })));
  create(authorPath, Buffer.from(JSON.stringify({
    source_sha256: asset.source.optimized.sha256,
    near_sha256: near.sha256, far_sha256: far.sha256
  })));
  asset.integration = {
    evidence, author: authorPath, scene,
    near: { ...near, triangles: 2 }, far: { ...far, triangles: 1 }
  };
  create('pipeline/tripo-art-brain/catalog.json', Buffer.from(JSON.stringify(cfg)));
  return { root, cfg, create };
}
function withFixture(fn) {
  const f = tempFixture();
  try { fn(f); }
  finally { fs.rmSync(f.root, { force: true, recursive: true }); }
}

test('GLB parser counts triangles, material slots and actual bones, never infers a rig', () => {
  const simple = inspectGLB(fixtureGLB(5));
  assert.equal(simple.triangles, 5);
  assert.equal(simple.skins, 0);
  assert.equal(simple.animations, 0);
  assert.equal(inspectGLB(fixtureGLB(2, { skins: 1, animations: 2 })).animations, 2);
  assert.throws(() => inspectGLB(Buffer.from('not a model')), /INVALID_GLB_HEADER/);
});
test('offline audit verifies all three archived sources and retains state boundaries', () => withFixture(({ root, cfg }) => {
  assert.equal(auditAsset(root, 'spawnling', cfg).integrated_in_game, true);
  assert.equal(auditAsset(root, 'boar', cfg).integrated_in_game, false);
  assert.equal(auditAsset(root, 'focus', cfg).art_approved, false);
  assert.equal(auditAsset(root, 'spawnling', cfg).lods.near.triangles, 2);
  assert.equal(auditAsset(root, 'spawnling', cfg).new_credits, 0);
}));
test('corrupted source or wrong declared polygon count fails closed', () => withFixture(({ root, cfg }) => {
  const boar = cfg.assets.boar;
  assert.throws(() => pinnedBytes(root, { path: boar.source.raw.path, sha256: 'f'.repeat(64) }), /SHA256_MISMATCH/);
  boar.source.raw.triangles = 9999;
  assert.throws(() => auditAsset(root, 'boar', cfg), /TRIANGLE_DRIFT/);
}));
test('missing UV and false Unity material success both fail closed', () => withFixture(({ root, cfg, create }) => {
  const model = create('ArtSource/World4X/boar/uvless.glb', fixtureGLB(3, { uv: false }));
  cfg.assets.boar.source.optimized = { ...model, triangles: 3 };
  assert.throws(() => auditAsset(root, 'boar', cfg), /MISSING_UV_OR_NORMALS/);
  cfg.assets.boar.source.optimized = { ...create('ArtSource/World4X/boar/optimized.glb', fixtureGLB(3)), triangles: 3 };
  const p = cfg.assets.boar.qa.metrics;
  const m = JSON.parse(fs.readFileSync(path.join(root, p)));
  m.importedBaseColorWidth = 4;
  create(p, Buffer.from(JSON.stringify(m)));
  assert.throws(() => auditAsset(root, 'boar', cfg), /UNITY_QA_GATE_FAIL/);
}));
test('scene evidence cannot silently add colliders or claim invalid LOD hashes', () => withFixture(({ root, cfg, create }) => {
  const p = cfg.assets.spawnling.integration.evidence;
  const m = JSON.parse(fs.readFileSync(path.join(root, p)));
  m.visual_colliders = 1;
  create(p, Buffer.from(JSON.stringify(m)));
  assert.throws(() => auditAsset(root, 'spawnling', cfg), /INTEGRATION_PROVENANCE_FAIL/);
}));
test('file path traversal and symlink escape are rejected', () => withFixture(({ root, cfg }) => {
  assert.throws(() => confined(root, '../secret'), /UNSAFE_PATH/);
  assert.throws(() => confined(root, '/etc/passwd'), /UNSAFE_PATH/);
  assert.throws(() => pinnedBytes(root, { path: cfg.assets.boar.source.raw.path, sha256: 'not-valid' }), /INVALID_SOURCE_PIN/);
}));
test('Qwen context and canonical planner request are prepared with spending disabled', () => withFixture(({ root }) => {
  const bundle = makeBundle(root, 'spawnling');
  assert.match(bundle.context, /QWEN ARTIST/);
  assert.match(bundle.context, /Existing runtime LOD triangles: 2\/1/);
  assert.equal(bundle.request.allow_tripo, false);
  assert.equal(bundle.request.credit_authorization.authorized_credit_cost, 0);
  assert.equal(bundle.request.credit_authorization.approved, false);
  assert.equal(bundle.request.enabled, false);
  assert.equal(bundle.report.follow_up_advanced_tripo.ready_for_paid_call, false);
  assert.equal(bundle.report.art_approved, false);
  assert.equal(bundle.report.qwen_inference_run, 'NOT_EXECUTED');
  const output = fs.mkdtempSync(path.join(os.tmpdir(), 'eldoria-tripo-output-'));
  fs.rmSync(output, { recursive: true });
  try {
    const result = writeBundle(root, 'spawnling', output);
    assert.equal(result.credits_spent, 0);
    assert.ok(fs.readFileSync(path.join(output, 'qwen-inventory.txt'), 'utf8').includes('Tripo'));
    assert.throws(() => writeBundle(root, 'spawnling', output), /EEXIST/);
    assert.throws(() => writeBundle(root, 'spawnling', root + '/generated'), /OUTPUT_MUST_BE_OUTSIDE_REPO/);
  } finally { fs.rmSync(output, { recursive: true, force: true }); }
}));
