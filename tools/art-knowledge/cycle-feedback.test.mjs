import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import crypto from 'node:crypto';
import {prepareInvocation,verifyArtistConsumption} from './run-artist.mjs';
import {recordInference,stageReviewed} from './cycle-feedback.mjs';

const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
const req='pipeline/art-production-requests/eldoria-tripo-art-brain-phase2-20261021.json';
const args=(out,more=[])=>['--reference','original.jpg','--unity-before','unity.jpg','--target-object','Spawnling_Near_Authored','--subject-kind','creature','--cpu-compact','--out',out,...more];
test('source inventory and selected knowledge co-exist in exact Qwen input',()=>{
 const dir=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-learning-'));
 try{
  const src=path.join(dir,'source.txt');
  fs.writeFileSync(src,'SOURCE_VERIFIED: Spawnling_B2_Near.glb, raw SHA known, head +X, foot contacts protected');
  const p=prepareInvocation({requestFile:req,prefix:path.join(dir,'cycle'),sourceInventoryPath:src,
    artistArgs:args(path.join(dir,'qwen'))});
  const inventory=fs.readFileSync(p.inventory,'utf8');
  assert.match(inventory,/SOURCE_VERIFIED/);
  assert.match(inventory,/ELDORIA HISTORICAL ART KNOWLEDGE/);
  assert.equal(sha(Buffer.from(inventory)),p.expected_inventory_sha256);
  assert.ok(inventory.length<=3200);
  assert.equal(p.command.at(-2),'--inventory');
  assert.equal(p.command.at(-1),p.inventory);
  assert.throws(()=>verifyArtistConsumption(p),/inference evidence not found/);
  assert.throws(()=>prepareInvocation({requestFile:req,prefix:path.join(dir,'cycle'),artistArgs:args('other')}),/Refuse overwrite/);
 }finally{fs.rmSync(dir,{recursive:true,force:true});}
});
test('any Qwen result is automatically staged, not artistically approved',()=>{
 const dir=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-feedback-'));
 try{
  const out=path.join(dir,'qwen'),prefix=path.join(dir,'cycle');
  const p=prepareInvocation({requestFile:req,prefix,artistArgs:args(out)});
  const fail=recordInference({invocation:p,outFile:path.join(dir,'failed-feedback.json'),exitStatus:1});
  assert.equal(fail.status,'INFERENCE_FAILED_UNREVIEWED');
  assert.equal(fail.art_approved,false);
  fs.mkdirSync(out);
  fs.writeFileSync(path.join(out,'artist-inference.json'),JSON.stringify({
   model:'qwen3-vl:2b-instruct',asset_inventory_sha256:p.expected_inventory_sha256,
   artist_brief_sha256:'abc',system_prompt_sha256:'def',self_approval:false,reference_sha256:'ref'
  }));
  const ok=recordInference({invocation:p,outFile:path.join(dir,'attempt.json'),exitStatus:0});
  assert.equal(ok.qwen_consumed_catalog,true);
  assert.equal(ok.review_state,'PENDING_INDEPENDENT_ART_REVIEW');
  assert.equal(verifyArtistConsumption(p).status,'EXACT_QWEN_INVENTORY_CONSUMED');
  assert.throws(()=>recordInference({invocation:p,outFile:path.join(dir,'attempt.json'),exitStatus:0}),/REFUSE_OVERWRITE/);
 }finally{fs.rmSync(dir,{recursive:true,force:true});}
});
test('fake approval, absent true frames, invented independent review are rejected',()=>{
 const dir=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-evidence-'));
 try{
  const event=path.join(dir,'event.json');fs.writeFileSync(event,JSON.stringify({schema_version:1,kind:'eldoria_art_cycle_feedback',art_approved:false,evidence_verified_to_bytes:true,actual_inference_sha256:'a'.repeat(64),qwen_consumed_catalog:true}));
  const before=path.join(dir,'before.png'),after=path.join(dir,'after.png');
  fs.writeFileSync(before,'not-an-image');fs.writeFileSync(after,'not-an-image');
  const review=path.join(dir,'review.json');fs.writeFileSync(review,JSON.stringify({verdict:'INTEGRATED_VISUAL_REVIEW_PASS',independent_reviewer:'EXTERNAL_TO_AUTHOR',explanation:'A'.repeat(80),same_camera_verified:true,same_light_verified:true,same_resolution_verified:true,original_files_inspected:true,camera_matrix_sha256:'a'.repeat(64),unity_evidence_sha256:'b'.repeat(64)}));
  assert.throws(()=>stageReviewed({feedbackPath:event,beforePath:before,afterPath:after,reviewPath:review,outFile:path.join(dir,'staged.json')}),/PNG_OR_JPEG/);
 }finally{fs.rmSync(dir,{recursive:true,force:true});}
});
