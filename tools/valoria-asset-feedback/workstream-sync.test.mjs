import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import {validateWorkstream} from './workstream-sync.mjs';
const registry=JSON.parse(fs.readFileSync(new URL('./registry.json',import.meta.url)));
const ids=JSON.parse(fs.readFileSync(new URL('./asset-ids.json',import.meta.url)));
const selected={
 base_63:{expected_count:63},constraints:{art_approved:false},
 selected:[
  {id:'tower_square_short',sha256:'a'.repeat(64),usage:'candidate_in_scene'},
  {id:'wall_panel_regular_4m',sha256:'b'.repeat(64),usage:'candidate_in_scene'}]};
function plan(workstream){
 return {schema:'eldoria.valoria63.workstream-feedback.v1',workstream,phase:'EXPERIMENT_ONLY',art_approved:false,
  source_asset_count:63,
  corrections:registry.corrections.map(c=>({id:c.id,rationale:'Record exact context and keep local adjustments local to each scene.',
   evidence_url:c.proof,disposition:c.scope==='SCENE_LOCAL'?'scene_only':c.scope==='ASSEMBLY_CONTRACT'?'preserved_assembly':'pending_review'})),
  selected_versions:selected.selected.map(x=>({id:x.id,sha256:x.sha256,usage:x.usage}))};
}
test('artist acknowledges all observed defects',()=>assert.equal(validateWorkstream('artist',plan('artist'),registry,ids).asset_count,63));
test('Unity pins exact SHA and acknowledges corrections',()=>assert.equal(validateWorkstream('unity',plan('unity'),registry,ids,selected).selected,2));
test('a new issue in main requires new explicit decisions',()=>{const p=plan('artist');p.corrections.pop();assert.throws(()=>validateWorkstream('artist',p,registry,ids),/unacknowledged correction/)});
test('scene-specific resize never becomes global asset fix',()=>{const p=plan('artist');p.corrections.find(x=>x.id==='B1-TOWER-SCALE-001').disposition='applied';assert.throws(()=>validateWorkstream('artist',p,registry,ids),/illegal transfer/)});
test('Unity GLB swapped without feedback refresh rejected',()=>{const p=plan('unity');p.selected_versions[0].sha256='f'.repeat(64);assert.throws(()=>validateWorkstream('unity',p,registry,ids,selected),/SHA changed/)});
test('unfinished experimental art cannot claim promotion',()=>{const p=plan('artist');p.phase='PROMOTION_REVIEW';assert.throws(()=>validateWorkstream('artist',p,registry,ids),/unreviewed correction/)});
test('art approval is not inferred from technical success',()=>{const p=plan('unity');p.art_approved=true;assert.throws(()=>validateWorkstream('unity',p,registry,ids,selected),/ART_APPROVED/)});
