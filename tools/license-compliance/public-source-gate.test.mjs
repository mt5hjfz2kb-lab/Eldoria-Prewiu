import test from 'node:test';
import assert from 'node:assert/strict';
import { inspectPullRequestFiles, validateRightsEvidence, PROTECTED_IMPORTED_ROOTS } from './public-source-gate.mjs';

const change = (filename, status = 'added', previous_filename) => ({filename, status, previous_filename});

test('covers exactly the seven known Asset Store import families and Quaternius source comparison', () => {
  assert.equal(PROTECTED_IMPORTED_ROOTS.length, 8);
  for (const root of PROTECTED_IMPORTED_ROOTS) {
    const r = inspectPullRequestFiles([change(root + 'new-texture.png')]);
    assert.equal(r.blocking.length, 1, root);
    assert.equal(r.public_source_license_cleared, false);
  }
});

test('blocks a modified existing external texture as well as a newly added file', () => {
  const changed = inspectPullRequestFiles([
    change('Unity/Assets/Tree_Packs/URP_Tree_Pack/Textures/TreeBark.png', 'modified'),
    change('Unity/Assets/EmaceArt/Slavic World Free/Meshes/house.fbx'),
  ]);
  assert.equal(changed.blocking.length, 2);
});

test('does not block restoring Unity .meta or editing source notices', () => {
  const changes = inspectPullRequestFiles([
    change('Unity/Assets/Tree_Packs/URP_Tree_Pack/Textures/TreeBark.png.meta'),
    change('Unity/Assets/NatureStarterKit2/NOTICE.txt', 'modified'),
    change('Unity/Assets/EmaceArt/Slavic World Free/LICENSE.md', 'modified'),
  ]);
  assert.deepEqual(changes.blocking, []);
});

test('allows removing a risky asset without pretending historical exposure was erased', () => {
  const changes = inspectPullRequestFiles([change('Unity/Assets/Tree_Packs/URP_Tree_Pack/Textures/tree.png', 'removed')]);
  assert.equal(changes.blocking.length, 0);
  assert.equal(changes.public_source_license_cleared, false);
});

test('blocks renamed imported assets even when moved out of vendor folder', () => {
  const changes = inspectPullRequestFiles([
    change('Unity/Assets/Eldoria/ArtTests/ImportedOldCastle.fbx', 'renamed',
      'Unity/Assets/Bublik/Simple Modular Castle Assets/Meshes/castle.fbx'),
  ]);
  assert.equal(changes.blocking.length, 1);
});

test('blocks a previously unknown external Unity package in an unrecognized top-level folder', () => {
  const r = inspectPullRequestFiles([change('Unity/Assets/NewUnverifiedVendor/Textures/Bark.png')]);
  assert.equal(r.blocking.length, 1);
  assert.match(r.blocking[0].reason, /NEW_UNKNOWN/);
});

test('does not interrupt existing Eldoria game code/UI and gives advisory for new self-authored art', () => {
  const r = inspectPullRequestFiles([
    change('Unity/Assets/Eldoria/Scripts/Domain/Food.cs', 'modified'),
    change('Unity/Assets/Eldoria/Resources/UI/NewOwnerIcon.png'),
    change('ArtSource/World4X/Spawnling_v1/NewEditable.blend'),
  ]);
  assert.equal(r.blocking.length, 0);
  assert.equal(r.advisory.length, 2);
});

test('rejects malformed API responses and unsafe paths', () => {
  assert.throws(() => inspectPullRequestFiles(null), /array/);
  assert.throws(() => inspectPullRequestFiles([change('../outside.png')]), /unsafe/);
  assert.throws(() => inspectPullRequestFiles([change('Unity/Assets/Bublik/X.fbx', 'nonsense')]), /Unrecognized/);
});

const sampleLedger = () => ({schema_version:1,families:[
  {id:'tree',risk:'CRITICAL',proof_required:['my-assets receipt', 'source terms']},
  {id:'tripo',risk:'HIGH',proof_required:['plan at generation']},
]});
const sampleShelf = (item) => ({groups:[{id:'production',items:[item]}]});
const item = () => ({path:'Unity/Assets/Eldoria/Resources/Valoria/Sawmill.glb',license_certified:false});

test('missing license entitlements remain UNVERIFIED without failing CI or becoming cleared', () => {
  const r = validateRightsEvidence(sampleLedger(),sampleShelf(item()));
  assert.equal(r.pass,true);
  assert.equal(r.pending_rights,1);
  assert.equal(r.shelf_items,1);
});

test('license_certified:true needs a documented private-proof reference, contract and authorized scopes', () => {
  let marked = {...item(),license_certified:true};
  let r = validateRightsEvidence(sampleLedger(),sampleShelf(marked));
  assert.equal(r.pass,false);
  assert.match(r.errors[0], /Unsubstantiated/);
  marked.license_evidence = {
    licensor:'hypothetical provider',license_id:'recorded-contract-ID',
    private_proof_reference:'private://owner/receipts/ref-42',verified_by:'owner',
    verified_at:'2026-11-06',allowed_embedding:'game-only',
    allowed_public_sources:'not-permitted',
  };
  r = validateRightsEvidence(sampleLedger(),sampleShelf(marked));
  assert.equal(r.pass,true);
  assert.equal(r.pending_rights,0);
});

test('ledger evidence must be structurally sound and must never self-certify entitlement', () => {
  const bad = sampleLedger();
  bad.families[1].id='tree';
  assert.equal(validateRightsEvidence(bad,sampleShelf(item())).pass,false);
  assert.equal(validateRightsEvidence({schema_version:7},sampleShelf(item())).pass,false);
});
