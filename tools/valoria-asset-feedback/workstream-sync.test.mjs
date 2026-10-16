import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import {validateWorkstream,verifySourceRevision} from './workstream-sync.mjs';
import os from 'node:os';
import path from 'node:path';
import crypto from 'node:crypto';
const registry=JSON.parse(fs.readFileSync(new URL('./registry.json',import.meta.url)));
const ids=JSON.parse(fs.readFileSync(new URL('./asset-ids.json',import.meta.url)));
const selected={
 base_63:{expected_count:63},constraints:{art_approved:false},
 selected:[
  {id:'tower_square_short',sha256:'a'.repeat(64),usage:'candidate_in_scene'},
  {id:'wall_panel_regular_4m',sha256:'b'.repeat(64),usage:'candidate_in_scene'}]};
function plan(workstream){
 return {schema:'eldoria.valoria63.workstream-feedback.v1',workstream,phase:'EXPERIMENT_ONLY',art_approved:false,source_file:'test/fixed-source',source_git_blob_sha1:'a'.repeat(40),
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

test('identical authored source Git blob hash passes',()=>{
 const dir=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-feedback-'));
 try{
  const file=path.join(dir,'source.py');const content=Buffer.from('irreversible change protection\\n','utf8');
  fs.writeFileSync(file,content);
  const sha=crypto.createHash('sha1').update(Buffer.from('blob '+content.length+String.fromCharCode(0))).update(content).digest('hex');
  assert.equal(verifySourceRevision({source_file:'source.py',source_git_blob_sha1:sha},file),sha);
 }finally{fs.rmSync(dir,{recursive:true,force:true})}
});
test('changed authored source without updated feedback is rejected',()=>{
 const dir=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-feedback-'));
 try{const file=path.join(dir,'source.py');fs.writeFileSync(file,'new geometry');
  assert.throws(()=>verifySourceRevision({source_file:'source.py',source_git_blob_sha1:'0'.repeat(40)},file),/source changed without per-asset feedback review/);
 }finally{fs.rmSync(dir,{recursive:true,force:true})}
});
