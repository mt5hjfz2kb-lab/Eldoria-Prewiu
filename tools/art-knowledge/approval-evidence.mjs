import fs from 'node:fs';
import path from 'node:path';
const HEX=/^[0-9a-f]{64}$/;
export function checkFutureApprovedQwenMemory(request,workstreamId,repoRoot=process.cwd()){
 if(!['ART_APPROVED','MOBILE_VALIDATED','GAMEPLAY_VALIDATED','RELEASED'].includes(String(request?.visual_gate_state||'').toUpperCase()))
   return [];
 const violations=[];
 const p=request?.art_memory_consumption_proof;
 if(typeof p!=='string'||!p.startsWith('docs/evidence/')||p.includes('..')||!p.endsWith('.json')){
  return ['ART_MEMORY_APPROVAL_PROOF_REQUIRED: future approved art must pin an actual Qwen-memory consumption manifest in docs/evidence/'];
 }
 const absolute=path.resolve(repoRoot,p),base=path.resolve(repoRoot)+path.sep;
 if(!absolute.startsWith(base)||!fs.existsSync(absolute)||!fs.statSync(absolute).isFile())return ['ART_MEMORY_APPROVAL_PROOF_MISSING'];
 const real=fs.realpathSync(absolute),realRoot=fs.realpathSync(repoRoot)+path.sep;
 if(!real.startsWith(realRoot))return ['ART_MEMORY_APPROVAL_SYMLINK_OUTSIDE_REPO'];
 let proof;
 try{proof=JSON.parse(fs.readFileSync(absolute,'utf8'));}catch{return ['ART_MEMORY_APPROVAL_PROOF_INVALID_JSON'];}
 if(proof?.status!=='EXACT_QWEN_INVENTORY_CONSUMED')violations.push('ART_MEMORY_NOT_ACTUALLY_CONSUMED');
 if(proof.schema_version!==2)violations.push('ART_MEMORY_WRONG_PROOF_SCHEMA');
 if(!Array.isArray(proof.selected_ids)||proof.selected_ids.length===0)violations.push('ART_MEMORY_NO_SELECTED_CASES');
 for(const key of ['inventory_sha256','catalog_sha256','inference_sha256','prompt_sha256'])
  if(!HEX.test(proof[key]||''))violations.push('ART_MEMORY_BAD_PROVENANCE_'+key);
 if(typeof proof.model!=='string'||!proof.model.trim())violations.push('ART_MEMORY_MODEL_UNKNOWN');
 if(proof.art_approved!==false||proof.independent_visual_review!==false)violations.push('ART_MEMORY_PROOF_CLAIMS_APPROVAL');
 if(request?.workstream_id!==workstreamId)violations.push('ART_MEMORY_WORKSTREAM_MISMATCH');
 return violations;
}
