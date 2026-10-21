#!/usr/bin/env node
/**
 * Zero-credit, read-only bridge between certified Tripo sources and Eldoria's
 * existing art-production planner / Qwen --inventory contract.
 * No network, Tripo API submission, Unity scene edits or ART_APPROVED writes.
 */
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import zlib from 'node:zlib';
import { fileURLToPath } from 'node:url';

const sha256 = (bytes) => crypto.createHash('sha256').update(bytes).digest('hex');
const isSha = (value) => typeof value === 'string' && /^[a-f0-9]{64}$/i.test(value);
const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
export const CATALOG_PATH = 'pipeline/tripo-art-brain/catalog.json';

export function confined(root, name) {
  if (typeof name !== 'string' || !name || path.isAbsolute(name) ||
      name.split(/[\\/]/).includes('..') || name.includes('\\')) {
    throw new Error('UNSAFE_PATH: ' + name);
  }
  const target = path.resolve(root, name);
  if (!target.startsWith(path.resolve(root) + path.sep)) {
    throw new Error('UNSAFE_PATH: ' + name);
  }
  if (!fs.existsSync(target)) throw new Error('MISSING_FILE: ' + name);
  const realRoot = fs.realpathSync(root);
  const real = fs.realpathSync(target);
  if (!real.startsWith(realRoot + path.sep)) {
    throw new Error('SYMLINK_ESCAPE: ' + name);
  }
  return target;
}

export function pinnedBytes(root, spec) {
  if (!spec || !isSha(spec.sha256)) throw new Error('INVALID_SOURCE_PIN');
  const p = confined(root, spec.path);
  const bytes = fs.readFileSync(p);
  if (bytes.subarray(0, 43).toString('utf8').startsWith('version https://git-lfs.github.com/spec')) {
    throw new Error('UNMATERIALIZED_LFS: ' + spec.path);
  }
  const actual = sha256(bytes);
  if (actual !== spec.sha256) throw new Error('SHA256_MISMATCH: ' + spec.path);
  return bytes;
}

export function inspectGLB(bytes) {
  if (bytes.length < 20 || bytes.readUInt32LE(0) !== 0x46546c67 ||
      bytes.readUInt32LE(4) !== 2 || bytes.readUInt32LE(8) !== bytes.length) {
    throw new Error('INVALID_GLB_HEADER');
  }
  const jsonLength = bytes.readUInt32LE(12);
  if (bytes.readUInt32LE(16) !== 0x4e4f534a || jsonLength < 2 ||
      20 + jsonLength > bytes.length) throw new Error('INVALID_GLB_JSON_CHUNK');
  let gltf;
  try { gltf = JSON.parse(bytes.subarray(20, 20 + jsonLength).toString('utf8').trim()); }
  catch { throw new Error('INVALID_GLB_JSON'); }
  if (!Array.isArray(gltf.meshes) || !Array.isArray(gltf.accessors)) throw new Error('INVALID_GLB_MESH');
  let triangles = 0;
  let uv0 = true;
  let normals = true;
  let primitiveCount = 0;
  for (const mesh of gltf.meshes) {
    for (const primitive of mesh.primitives || []) {
      primitiveCount++;
      const mode = primitive.mode ?? 4;
      if (mode !== 4) throw new Error('UNSUPPORTED_GLB_PRIMITIVE_MODE: ' + mode);
      const idx = primitive.indices ?? primitive.attributes?.POSITION;
      const accessor = gltf.accessors[idx];
      if (!accessor || !Number.isInteger(accessor.count) || accessor.count % 3 !== 0) {
        throw new Error('INVALID_GLB_TRIANGLE_ACCESSOR');
      }
      triangles += accessor.count / 3;
      uv0 = uv0 && Number.isInteger(primitive.attributes?.TEXCOORD_0);
      normals = normals && Number.isInteger(primitive.attributes?.NORMAL);
    }
  }
  if (!primitiveCount || !triangles) throw new Error('EMPTY_GLB_GEOMETRY');
  return {
    triangles, meshes: gltf.meshes.length, primitives: primitiveCount,
    uv0, normals, materials: (gltf.materials || []).length,
    images: (gltf.images || []).length,
    skins: (gltf.skins || []).length, animations: (gltf.animations || []).length
  };
}

export function auditEditableBlend(root, spec) {
  if (!spec || !spec.path?.endsWith('.blend')) throw new Error('INVALID_EDITABLE_BLEND_PIN');
  const bytes = pinnedBytes(root, spec);
  let storage = 'uncompressed';
  const magic = bytes.subarray(0, 7).toString('ascii');
  if (bytes.length < 12) throw new Error('INVALID_EDITABLE_BLEND_HEADER: ' + spec.path);
  if (magic !== 'BLENDER') {
    if (bytes[0] === 0x1f && bytes[1] === 0x8b) {
      // Blender can save .blend directly as gzip. Validate bounded unpacked magic.
      const decoded = zlib.gunzipSync(bytes, { maxOutputLength: 250_000_000 });
      if (decoded.subarray(0, 7).toString('ascii') !== 'BLENDER') {
        throw new Error('INVALID_EDITABLE_BLEND_HEADER: ' + spec.path);
      }
      storage = 'gzip';
    } else if (bytes.subarray(0, 4).toString('hex') === '28b52ffd') {
      // Zstd-compressed Blender files: SHA is exact/pinned; do not pretend to
      // inspect the inner body when this runtime lacks a zstd decoder.
      storage = 'zstd_sha_pinned';
    } else {
      throw new Error('INVALID_EDITABLE_BLEND_HEADER: ' + spec.path +
        ' prefix=' + bytes.subarray(0, 8).toString('hex'));
    }
  }
  return { path: spec.path, sha256: spec.sha256, file_bytes: bytes.length, storage };
}

function readJson(root, relativePath) {
  return JSON.parse(fs.readFileSync(confined(root, relativePath), 'utf8'));
}

function verifyCapture(root, relativePath) {
  const bytes = fs.readFileSync(confined(root, relativePath));
  if (bytes.length < 32 ||
      !bytes.subarray(0, 8).equals(Buffer.from('89504e470d0a1a0a', 'hex'))) {
    throw new Error('INVALID_PNG: ' + relativePath);
  }
  return { path: relativePath, sha256: sha256(bytes) };
}

export function loadCatalog(root = repoRoot) {
  const catalog = readJson(root, CATALOG_PATH);
  if (catalog.schema_version !== 1 || !catalog.assets ||
      Object.keys(catalog.assets).length !== 3) throw new Error('INVALID_CATALOG');
  return catalog;
}

function auditModel(root, model) {
  const info = inspectGLB(pinnedBytes(root, model));
  if (model.triangles !== info.triangles) {
    throw new Error('TRIANGLE_DRIFT: ' + model.path + ' actual=' + info.triangles);
  }
  if (!info.uv0 || !info.normals) throw new Error('MISSING_UV_OR_NORMALS: ' + model.path);
  return { ...info, path: model.path, sha256: model.sha256 };
}

export function auditAsset(root, id, catalog = loadCatalog(root)) {
  const asset = catalog.assets[id];
  if (!asset) throw new Error('UNKNOWN_ASSET: ' + id);
  if (!asset.source?.raw || !asset.source?.optimized || !asset.source?.editable ||
      !asset.qa?.metrics || !Array.isArray(asset.defects)) throw new Error('INVALID_ASSET_CONFIG: ' + id);
  if (asset.source.credits_consumed !== 20) throw new Error('UNEXPECTED_HISTORICAL_COST');
  const raw = auditModel(root, asset.source.raw);
  const optimized = auditModel(root, asset.source.optimized);
  const editable = auditEditableBlend(root, asset.source.editable);
  const metrics = readJson(root, asset.qa.metrics);
  const qaCaptures = metrics.captureFiles || metrics.captures;
  if (!Array.isArray(qaCaptures) || qaCaptures.length < 4) throw new Error('INCOMPLETE_UNITY_CAPTURES: ' + id);
  const qaFolder = path.posix.dirname(asset.qa.metrics);
  const captures = qaCaptures.map(name => {
    if (typeof name !== 'string' || name.includes('/') || name.includes('\\')) {
      throw new Error('UNSAFE_CAPTURE_NAME');
    }
    return verifyCapture(root, qaFolder + '/' + name);
  });
  if (metrics.triangles !== optimized.triangles || !metrics.uv0AllMeshes ||
      !metrics.normalsAllMeshes || !metrics.fullResolutionTexturePass ||
      metrics.importedBaseColorWidth < 1024 || metrics.importedBaseColorHeight < 1024 ||
      !metrics.positiveSelection || !metrics.emptySelectionMiss) {
    throw new Error('UNITY_QA_GATE_FAIL: ' + id);
  }
  let integrated = false;
  let lods = null;
  if (asset.integration) {
    const evidence = readJson(root, asset.integration.evidence);
    const author = readJson(root, asset.integration.author);
    if (evidence.source_glb_sha256 !== asset.source.optimized.sha256 ||
        author.source_sha256 !== asset.source.optimized.sha256 ||
        !evidence.hotspot_preserved || !evidence.gameplay_unchanged ||
        !evidence.save_untouched || evidence.visual_colliders !== 0) {
      throw new Error('INTEGRATION_PROVENANCE_FAIL: ' + id);
    }
    const near = auditModel(root, asset.integration.near);
    const finishedBlend = auditEditableBlend(root, asset.integration.editable);
    if (finishedBlend.sha256 !== author.blend_sha256) throw new Error('FINISHED_BLEND_PROVENANCE_FAIL: ' + id);
    const far = auditModel(root, asset.integration.far);
    if (near.sha256 !== evidence.near_glb_sha256 || far.sha256 !== evidence.far_glb_sha256 ||
        near.sha256 !== author.near_sha256 || far.sha256 !== author.far_sha256 ||
        near.triangles !== evidence.near_triangles || far.triangles !== evidence.far_triangles) {
      throw new Error('LOD_PROVENANCE_FAIL: ' + id);
    }
    const scene = confined(root, asset.integration.scene);
    if (!fs.statSync(scene).isFile()) throw new Error('MISSING_INTEGRATED_SCENE: ' + id);
    lods = { near, far, finished_blend: finishedBlend, provenance: asset.integration.evidence };
    integrated = true;
  }
  const verifiedFailures = (asset.verified_failures || []).map(failurePath => {
    const record = readJson(root, failurePath);
    if (record.asset !== id || record.art_approved !== false ||
        !['ART_REJECTED_GLOBAL_TRANSLATION_LIKE',
          'QWEN_PLAN_REJECTED_NON_EXECUTABLE'].includes(record.classification) ||
        record.source?.source_blend_sha256 !== asset.integration?.editable?.sha256 ||
        !Number.isInteger(record.source?.repo_run) ||
        !record.lesson || !record.next_action) {
      throw new Error('UNTRUSTED_REJECTED_ART_LEARNING: ' + failurePath);
    }
    return { path: failurePath, run_id: record.source.repo_run,
      classification: record.classification, lesson: record.lesson,
      next_action: record.next_action };
  });
  return {
    id, label: asset.label, role: asset.role, task_id: asset.source.task_id,
    historical_credits: asset.source.credits_consumed, new_credits: 0,
    raw, optimized, source_editable: editable, unity_technical_pass: true, unity_captures: captures,
    unity_version: metrics.unityVersion, imported_basecolor_px:
      [metrics.importedBaseColorWidth, metrics.importedBaseColorHeight],
    integrated_in_game: integrated, lods,
    has_skin: Boolean(lods?.near.skins || optimized.skins),
    has_animation: Boolean(lods?.near.animations || optimized.animations),
    art_approved: false, physical_mobile_profiled: false,
    defects: asset.defects, next_actions: asset.next_actions, verified_failures: verifiedFailures,
    source_report: asset.source_report, allowed_destination: asset.allowed_destination,
    protected: asset.protected
  };
}

export function makePlannerRequest(assetId, item, catalog) {
  const asset = catalog.assets[assetId];
  const profile = asset.planner_profile;
  if (!['environment_composition', 'environment_surface', 'animated_asset'].includes(profile)) {
    throw new Error('UNSAFE_PLANNER_PROFILE');
  }
  return {
    schema_version: 2, enabled: false, status: 'PREPARED_ONLY',
    request_id: 'tripo-art-brain-zero-credit-' + assetId,
    workstream_id: 'eldoria-tripo-art-brain-integration-20261021',
    asset_or_sector: item.label + ' — copy-only material/source improvement',
    profile, ai_authorship_policy: 'required',
    geometry_gap_proven: profile === 'environment_composition',
    allow_tripo: false,
    credit_authorization: { approved: false, authorized_credit_cost: 0 },
    diagnosis: item.defects.join(' '),
    capabilities: { existing_qualified_donor_assets: true,
      blender_material_consolidation: true, unity_material_blending: true },
    validation: {
      source_gate: 'Actual GLB SHA and UnityQA captures verified; independent Qwen direction BEFORE any Blender editing.',
      final_gate: 'Real same-camera Unity A/B, independent art judgement and mobile performance; no blind promotion.'
    },
    artist: {
      system_prompt: 'tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md',
      qwen_existing_command: 'tools/art-direction/ai-authoring/author_brief.py',
      inventory_context: 'generated/qwen-inventory.txt',
      note: 'Use --inventory with exact generated path; require real reference and real Unity before screenshot.'
    },
    notes: 'Unexecuted advisory request. Do NOT modify existing main gameplay or claim ART_APPROVED. No API tasks. '
      + (asset.protected || []).join('; ')
  };
}

export function makeBundle(root, id, catalog = loadCatalog(root)) {
  const item = auditAsset(root, id, catalog);
  const asset = catalog.assets[id];
  const request = makePlannerRequest(id, item, catalog);
  const diagnosis = item.defects.map((s, i) => String(i + 1) + '. ' + s).join('\n');
  const actions = item.next_actions.map((s, i) => String(i + 1) + '. ' + s).join('\n');
  const context = [
    'ELDORIA / TRIPO — VERIFIED SOURCE INVENTORY FOR EXISTING QWEN ARTIST (not a model inference).',
    'Target: ' + item.label + '. Gameplay role: ' + item.role + '.',
    'Original Tripo task: ' + item.task_id + '. Historical spend: 20 API credits; new credits: ZERO.',
    'Immutable raw: ' + item.raw.path + ' SHA256 ' + item.raw.sha256 + '; ' + item.raw.triangles + ' tris.',
    'Certified isolated-Unity source: ' + item.optimized.path + ' SHA256 ' + item.optimized.sha256
      + '; ' + item.optimized.triangles + ' tris; BaseColor ' + item.imported_basecolor_px.join('x') + '.',
    'Editable Blender: ' + item.source_editable.path + ' SHA256 ' + item.source_editable.sha256 + '.',
    'Integrated in game: ' + item.integrated_in_game + '. True Unity tech QA: ' + item.unity_technical_pass + '.',
    'Existing runtime LOD triangles: ' + (item.lods ? item.lods.near.triangles + '/' + item.lods.far.triangles : 'NONE, only isolated source'),
    'Finished editable Blender: ' + (item.lods ? item.lods.finished_blend.path + ' SHA256 ' + item.lods.finished_blend.sha256 : 'NOT INTEGRATED'),
    'Actual rig/skin/animation in these GLBs: ' + item.has_skin + '/' + item.has_animation + '.',
    'Known visual issues (documented; Qwen MUST verify from genuine images):\n' + diagnosis,
    'VERIFIED REAL REJECTED QWEN ART CYCLES, NOT APPROVED (never repeat):\n' +
      (item.verified_failures.length ? item.verified_failures.map(f => 'Run ' + f.run_id +
       ' => ' + f.classification + ': ' + f.lesson + ' Next: ' + f.next_action).join('\n') :
       'No source-specific reviewed failures.'),
    'Current evidence-aware next actions (not scripted edits):\n' + actions,
    'Preserve: ' + (item.protected || []).join('; '),
    'External Tripo API is NOT authorized. Do not generate, regenerate, animate, retarget or upload from this bundle.',
    'Require owner camera reference, genuine Unity before image, mesh inspection and independent visual comparison.',
    'Do not infer visual approval from SHA/technical QA. Art approval=false; mobile profiling=false.'
  ].join('\n\n') + '\n';
  const report = {
    schema_version: 1, status: 'ZERO_CREDIT_PREFLIGHT_PASS', item,
    reviewed_failed_qwen_cycles: item.verified_failures.length,
    follow_up_advanced_tripo: {
      ready_for_paid_call: false, credits_authorized: 0,
      rig_check_api_task: 'NOT_EXECUTED',
      rigging_api_task: 'NOT_EXECUTED',
      animation_api_task: 'NOT_EXECUTED',
      next_boundary: item.id === 'spawnling' ? 'Review anatomy and verify eligible Tripo task for Rig Check (API quoted free), then request specific owner authorization before rigging/retarget.' :
        'Use existing Blender/Unity zero-credit art route; API transformation requires separate owner consent.'
    },
    qwen_inference_run: 'NOT_EXECUTED',
    blender_edit_run: 'NOT_EXECUTED',
    unity_new_qa_run: 'NOT_EXECUTED',
    art_approved: false,
    mobile_profile_pass: false
  };
  return { report, context, request };
}

export function writeBundle(root, id, out) {
  const bundle = makeBundle(root, id);
  const absoluteOut = path.resolve(out);
  const rootReal = fs.realpathSync(root);
  if (absoluteOut === rootReal || absoluteOut.startsWith(rootReal + path.sep)) {
    throw new Error('OUTPUT_MUST_BE_OUTSIDE_REPO');
  }
  fs.mkdirSync(absoluteOut, { recursive: true });
  const outputs = {
    'preflight-report.json': JSON.stringify(bundle.report, null, 2) + '\n',
    'qwen-inventory.txt': bundle.context,
    'art-production-request.json': JSON.stringify(bundle.request, null, 2) + '\n'
  };
  for (const [name, value] of Object.entries(outputs)) {
    fs.writeFileSync(path.join(absoluteOut, name), value, { flag: 'wx' });
  }
  return { asset: id, out: absoluteOut, files: Object.keys(outputs), credits_spent: 0 };
}

const invokedDirectly = process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url);
if (invokedDirectly) {
  try {
    const [cmd = 'audit', id = 'all', option, dir] = process.argv.slice(2);
    if (!['audit', 'prepare'].includes(cmd)) throw new Error('USAGE: audit [all|spawnling|boar|focus] OR prepare ID --out ABSOLUTE_DIRECTORY');
    const catalog = loadCatalog();
    if (cmd === 'audit') {
      const ids = id === 'all' ? Object.keys(catalog.assets) : [id];
      const results = ids.map(assetId => auditAsset(repoRoot, assetId, catalog));
      console.log(JSON.stringify({ status: 'ZERO_CREDIT_SOURCE_AUDIT_PASS', results }, null, 2));
    } else {
      if (!catalog.assets[id] || option !== '--out' || !dir) throw new Error('PREPARE_REQUIRES_ASSET_AND_OUT');
      console.log(JSON.stringify(writeBundle(repoRoot, id, dir), null, 2));
    }
  } catch (error) {
    console.error('TRIPO_ART_BRAIN_PREFLIGHT_FAIL: ' + error.message);
    process.exitCode = 2;
  }
}
