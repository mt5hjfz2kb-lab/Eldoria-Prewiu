import fs from 'node:fs';
import assert from 'node:assert/strict';
import { createHash } from 'node:crypto';
const read=p=>fs.readFileSync(p,'utf8');
const baseline=JSON.parse(read('pipeline/development-baseline.json'));
const routing=JSON.parse(read('pipeline/visual-authority-routing-v1.json'));
const ownerReferenceHash='86fbffa9410514e935b121a844802573cbb7f94c73abb7358ce2f8ee15845c5d';
assert.equal(routing.canonical_visual_authority.reference_sha256,ownerReferenceHash,'Owner-approved reference fingerprint must not change without an explicit new approval');
assert.equal(createHash('sha256').update(fs.readFileSync(routing.canonical_visual_authority.reference)).digest('hex'),ownerReferenceHash,'Wrong/missing JPEG: do not certify Valoria art against a substituted reference');
assert.equal(routing.canonical_visual_authority.owner_visual_overrides.visible_four_plots,'KEEP_REMOVED','Four plots visible in JPEG must remain absent from playable Valoria');
assert.equal(routing.canonical_visual_authority.owner_visual_overrides.enemy_tents,'FORBIDDEN');
assert.equal(routing.canonical_visual_authority.owner_visual_overrides.large_cliffs,'FORBIDDEN');
assert.equal(baseline.branch,'main');
assert.equal(routing.canonical_visual_authority.certified_runtime_scene,baseline.scene);
assert.equal(routing.canonical_visual_authority.runtime_component,'ValoriaIntegratedWorld');
const scene=read('Unity/'+baseline.scene);
const hostMeta=read('Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs.meta');
assert.ok(scene.includes(hostMeta.match(/guid: (\w+)/)[1]),'Canonical scene missing integrated host');
const settings=read('Unity/ProjectSettings/EditorBuildSettings.asset');
const enabled=[...settings.matchAll(/- enabled: 1\s+path: (.+)/g)].map(m=>m[1].trim());
assert.deepEqual(enabled,[baseline.scene]);
const builder=read(routing.canonical_visual_authority.production_generator);
assert.ok(builder.includes(baseline.scene),'Builder must compile selected scene');
assert.ok(builder.includes('INTEGRATION_PASS_ART_REVIEW_REQUIRED'),'Native gameplay proof required');
read(routing.canonical_visual_authority.production_workflow);
const registry=JSON.parse(read('pipeline/active-workstreams.json'));
const canonicalOwners=registry.active.filter(w=>w.id==='valoria-master-plan-layered-construction-20261016');
assert.equal(canonicalOwners.length,1,'Exactly one registered integrated Valoria workstream is required');
// The production baseline remains main (checked above). A claimed executor may
// validate a candidate branch before integration; that does not change art authority.
const canonical=canonicalOwners[0];
if(canonical.branch!=='main'){
  assert.equal(canonical.ownership_state,'claimed','Candidate branch requires an explicit exclusive claim');
  assert.ok(typeof canonical.branch==='string'&&canonical.branch.trim(),'Candidate branch must be named');
  assert.match(canonical.base_main_sha||'',/^[0-9a-f]{40}$/,'Candidate requires its traceable canonical main base');
  assert.ok(Number.isInteger(canonical.ownership_revision)&&canonical.ownership_revision>0,'Candidate requires a versioned ownership claim');
}
if(canonicalOwners[0].ownership_state==='available'){
  assert.equal(canonicalOwners[0].owner,null,'Available canonical work has no exclusive chat owner');
  assert.deepEqual(canonicalOwners[0].resources,[],'Available canonical work cannot reserve resources');
}else{
  assert.ok(typeof canonicalOwners[0].owner==='string'&&canonicalOwners[0].owner.trim(),'Claimed canonical work needs its current owner');
}
assert.equal(JSON.parse(read('pipeline/unity-publish-request.json')).enabled,false,'Historical Pages publication must remain explicitly retired');
assert.ok(routing.current_screenshot_policy.forbidden_as_canonical.includes('eldoria-valoria-captures-*'));
read(routing.legacy_fallback.fail_closed_guard);
console.log('VALORIA_VISUAL_AUTHORITY_ROUTING=PASS');
console.log('CANONICAL_DEVELOPMENT=INTEGRATED_MAIN; ART_APPROVED=false; LEGACY_RETAINED_FOR_REFERENCE');
