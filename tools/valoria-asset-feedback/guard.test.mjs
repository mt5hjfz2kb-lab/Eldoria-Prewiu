import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import {validateRegistry,checkCandidate} from './guard.mjs';
const reg=JSON.parse(fs.readFileSync(new URL('./registry.json',import.meta.url),'utf8'));
const ids=JSON.parse(fs.readFileSync(new URL('./asset-ids.json',import.meta.url),'utf8'));
function candidate(id){return {asset_id:id,sha256:'f'.repeat(64),commit:'a'.repeat(40),blender_ab_url:'https://example.org/blender-a-b',ready_for_unity:false,decisions:reg.assets.find(a=>a.id===id).correction_ids.map(c=>{const issue=reg.corrections.find(x=>x.id===c);return {id:c,disposition:issue.scope==='ASSEMBLY_CONTRACT'?'preserved_assembly':issue.scope==='SCENE_LOCAL'?'scene_only':'pending_review',evidence_url:'https://example.org/evidence'}})}}}
test('full original inventory, corrections and provenance',()=>assert.deepEqual(validateRegistry(reg,ids),{assets:63,corrections:6}));
test('all individual records exist including ones without issues',()=>assert.equal(checkCandidate('artist',candidate('pine_small'),reg,ids).corrections_checked,0));
test('tower candidate explicitly acknowledges all 3 issues without blindly scaling asset',()=>assert.equal(checkCandidate('artist',candidate('tower_square_short'),reg,ids).corrections_checked,3));
test('reject any unacknowledged per-piece feedback',()=>{const c=candidate('tower_square_short');c.decisions.pop();assert.throws(()=>checkCandidate('artist',c,reg,ids),/unacknowledged correction/)});
test('reject scene correction applied as global geometry change',()=>{const c=candidate('tower_square_short');c.decisions[0].disposition='applied';assert.throws(()=>checkCandidate('artist',c,reg,ids),/scene-level/)});
test('reject pending art promoted as ready for Unity',()=>{const c=candidate('tower_square_short');c.ready_for_unity=true;assert.throws(()=>checkCandidate('artist',c,reg,ids),/unresolved defect/)});
test('require exact Unity A\/B and approval provenance',()=>{const c=candidate('pine_small');assert.throws(()=>checkCandidate('unity',c,reg,ids),/same-camera/);c.unity={same_camera:true,run_id:55,artifact_id:56,before_url:'https://example.org/a',after_url:'https://example.org/b',art_approved:false};assert.equal(checkCandidate('unity',c,reg,ids).phase,'unity')});
test('reject duplicate catalog entries',()=>{const a=structuredClone(reg);a.assets[0].id=a.assets[1].id;assert.throws(()=>validateRegistry(a,ids),/duplicate asset/)});
