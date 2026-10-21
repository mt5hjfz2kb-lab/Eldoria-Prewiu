import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {checkFutureApprovedQwenMemory} from './approval-evidence.mjs';
test('approved future artist must pin exact real Qwen memory proof',()=>{
 const d=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-approval-'));
 const req={workstream_id:'fresh-demo',visual_gate_state:'ART_APPROVED'};
 try{
  assert.match(checkFutureApprovedQwenMemory(req,'fresh-demo',d).join(),/PROOF_REQUIRED/);
  req.art_memory_consumption_proof='docs/evidence/artist-consumption.json';
  const target=path.join(d,req.art_memory_consumption_proof);
  fs.mkdirSync(path.dirname(target),{recursive:true});
  const proof={schema_version:2,status:'EXACT_QWEN_INVENTORY_CONSUMED',model:'qwen3-vl:2b-instruct',
   selected_ids:['K19'],inventory_sha256:'a'.repeat(64),catalog_sha256:'b'.repeat(64),
   inference_sha256:'c'.repeat(64),prompt_sha256:'d'.repeat(64),
   independent_visual_review:false,art_approved:false};
  fs.writeFileSync(target,JSON.stringify(proof));
  assert.deepEqual(checkFutureApprovedQwenMemory(req,'fresh-demo',d),[]);
  proof.art_approved=true;fs.writeFileSync(target,JSON.stringify(proof));
  assert.match(checkFutureApprovedQwenMemory(req,'fresh-demo',d).join(),/CLAIMS_APPROVAL/);
  req.visual_gate_state='AUTHORING';
  assert.deepEqual(checkFutureApprovedQwenMemory(req,'fresh-demo',d),[]);
 }finally{fs.rmSync(d,{recursive:true,force:true});}
});
