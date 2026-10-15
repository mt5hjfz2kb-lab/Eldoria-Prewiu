#!/usr/bin/env node
// Static entry gate only. No Blender, Unity rendering, runner reservation or downloads.
// IMPORTANT: SOURCE_PASS is not UNITY_RENDER_PASS and never ART_APPROVED.
import { existsSync, readFileSync, statSync } from 'node:fs';
import { resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { dirname } from 'node:path';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '../..');
const sources = {
  approvedReference: ['references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg', 702827],
  heroFbx: ['Unity/Assets/Eldoria/ArtTests/OriginalHero/valoria_hero_fragment.fbx', 272492],
  tree: ['Unity/Assets/Eldoria/Resources/WorldInventory/Tree01A.prefab', 1],
  cliff: ['Unity/Assets/Eldoria/Resources/Valoria/SM_Cliffs_01.prefab', 1],
  rock: ['Unity/Assets/Eldoria/Resources/WorldInventory/Rock01.prefab', 1],
  stoneTexture: ['Unity/Assets/Eldoria/ArtTests/OriginalHero/Textures/limestone.png', 1],
  oakTexture: ['Unity/Assets/Eldoria/ArtTests/OriginalHero/Textures/oak.png', 1],
  darkStoneTexture: ['Unity/Assets/Eldoria/ArtTests/OriginalHero/Textures/oldstone.png', 1],
  slateTexture: ['Unity/Assets/Eldoria/ArtTests/OriginalHero/Textures/slate.png', 1],
  rockTexture: ['Unity/Assets/Eldoria/ArtTests/OriginalHero/Textures/rock.png', 1]
};
const checked = [];
for (const [kind, [relative, minBytes]] of Object.entries(sources)) {
  const file = resolve(root, relative);
  if (!existsSync(file)) throw Error('SOURCE_MISSING: ' + kind + ': ' + relative);
  const size = statSync(file).size;
  if (size < minBytes) throw Error('SOURCE_UNDERSIZED: ' + relative + ': ' + size);
  checked.push({kind, relative, bytes: size});
}
const version = readFileSync(resolve(root,'Unity/ProjectSettings/ProjectVersion.txt'), 'utf8');
if (!version.includes('6000.3.23f1')) throw Error('UNITY_VERSION_MISMATCH');
const manifest = JSON.parse(readFileSync(resolve(root,'Unity/Packages/manifest.json'), 'utf8'));
if (!manifest.dependencies['com.unity.render-pipelines.universal']) throw Error('URP_MISSING');

const path = 'Unity/Assets/Eldoria/ArtTests/ReferenceFirstPilot/Editor/ReferenceFirstPilotCapture.cs';
const src = readFileSync(resolve(root, path), 'utf8');
const rules = [
  ['batch-only', /if \(!Application\.isBatchMode\)/],
  ['isolated-scene', /ReferenceFirstValoriaLookdev\.unity/],
  ['reference-size-width', /CaptureWidth\s*=\s*1536/],
  ['reference-size-height', /CaptureHeight\s*=\s*1024/],
  ['render-from-camera', /camera\.Render\(\)/],
  ['real-png', /tex\.EncodeToPNG\(\)/],
  ['honest-manifest', /artApproved\s*=\s*false/],
  ['legal-unverified', /licenseVerified\s*=\s*false/],
  ['real-hero-import', /MustLoad\(HeroPath\)/],
  ['real-forest-import', /MustLoad\(TreePath\)/],
  ['real-cliff-import', /MustLoad\(CliffPath\)/],
  ['working-evidence-export', /render-manifest\.json/]
];
for (const [name, expression] of rules) {
  if (!expression.test(src)) throw Error('CAPTURE_CONTRACT_MISSING: ' + name);
}
if (/ProductionSlice\/Runtime|pipeline\/unity-publish-request|PrefabUtility\.SaveAsPrefabAsset/.test(src))
  throw Error('ISOLATION_FAILURE: runtime/publishing route referenced by pilot');

console.log(JSON.stringify({
  stage: 'STATIC_SOURCE_GATE_ONLY_NOT_VISUAL_APPROVAL',
  pass: true,
  unityEditor: '6000.3.23f1',
  realAssetCandidates: checked.length,
  checked,
  targetRender: '1536x1024',
  artApproved: false,
  licenseVerified: false,
  windowsRunnerUsed: false
}, null, 2));
