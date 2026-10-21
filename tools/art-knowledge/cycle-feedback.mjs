import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
const sha=x=>crypto.createHash('sha256').update(x).digest('hex');
const SHA=/^[0-9a-f]{64}$/;
function requireFile(file,limit=25_000_000){
 if(!file||!fs.existsSync(file))throw Error('ART_FEEDBACK_MISSING_EVIDENCE '+file);
 const st=fs.statSync(file);if(!st.isFile()||st.size>limit||st.size<5)throw Error('ART_FEEDBACK_INVALID_EVIDENCE '+file);
 return fs.readFileSync(file);
}
function writeOnce(file,value){
 if(!file)throw Error('ART_FEEDBACK_OUTPUT_REQUIRED');
 if(fs.existsSync(file))throw Error('ART_FEEDBACK_REFUSE_OVERWRITE '+file);
 fs.mkdirSync(path.dirname(file),{recursive:true});
 fs.writeFileSync(file,JSON.stringify(value,null,2)+'\n',{flag:'wx'});
 return value;
}
export function recordInference({invocation,outFile,exitStatus}){
 const reportPath=path.join(invocation.out,'artist-inference.json');
 const raw=fs.existsSync(reportPath)?requireFile(reportPath):null;
 const report=raw?JSON.parse(raw):null;
 const inventoryBytes=requireFile(invocation.inventory,10000);
 const consumed=!!report&&report.asset_inventory_sha256===sha(Buffer.from(inventoryBytes.toString('utf8').slice(0,4000)));
 if(raw&&!consumed)throw Error('ART_FEEDBACK_INVENTORY_HASH_MISMATCH');
 const event={
  schema_version:1,kind:'eldoria_art_cycle_feedback',
  status:!report?'INFERENCE_FAILED_UNREVIEWED':'QWEN_INFERRED_ART_UNREVIEWED',
  requested_model:report?.model||null,
  qwen_consumed_catalog:consumed,
  catalog_sha256:invocation.selected.catalog_sha256,
  knowledge_ids:invocation.selected.selected_ids,
  rejected_warning_ids:invocation.selected.rejected_ids,
  inventory_sha256:sha(inventoryBytes),
  actual_inference_sha256:raw?sha(raw):null,
  qwen_prompt_sha256:report?.artist_brief_sha256||null,
  request_id:invocation.selected.request_id,
  exit_status:exitStatus,
  evidence_verified_to_bytes:!!raw,
  review_state:'PENDING_INDEPENDENT_ART_REVIEW',
  reference_sha256:report?.reference_sha256||null,
  art_approved:false,mobile_verified:false,
  can_enter_canonical_learning:false
 };
 if(outFile)writeOnce(outFile,event);
 return event;
}
export function stageReviewed({feedbackPath,beforePath,afterPath,reviewPath,outFile}){
 const feedback=JSON.parse(requireFile(feedbackPath));
 if(feedback?.schema_version!==1||feedback.kind!=='eldoria_art_cycle_feedback'||feedback.art_approved!==false)throw Error('ART_FEEDBACK_UNTRUSTED_CYCLE');
 if(!feedback.evidence_verified_to_bytes||!feedback.actual_inference_sha256||feedback.qwen_consumed_catalog!==true)throw Error('ART_FEEDBACK_NO_REAL_QWEN_PROOF');
 const review=JSON.parse(requireFile(reviewPath));
 if(!['ART_REJECTED','ART_SOURCE_REVIEW_PASS','INTEGRATED_VISUAL_REVIEW_PASS'].includes(review.verdict)||
    review.independent_reviewer!=='EXTERNAL_TO_AUTHOR'||typeof review.explanation!=='string'||review.explanation.length<40)
    throw Error('ART_FEEDBACK_NO_INDEPENDENT_ARTISTIC_JUDGMENT');
 const original=requireFile(beforePath),candidate=requireFile(afterPath);
 function image(x){return x.subarray(0,8).equals(Buffer.from('89504e470d0a1a0a','hex'))||(x[0]===255&&x[1]===216);}
 if(!image(original)||!image(candidate))throw Error('ART_FEEDBACK_MATCHED_PNG_OR_JPEG_REQUIRED');
 if(!SHA.test(review.camera_matrix_sha256||'')||!review.same_camera_verified||
    !review.same_light_verified||!review.same_resolution_verified||
    !review.original_files_inspected)throw Error('ART_FEEDBACK_NO_MATCHED_CAMERA_VERIFICATION');
 if(review.verdict==='INTEGRATED_VISUAL_REVIEW_PASS'&&!review.unity_evidence_sha256)throw Error('ART_FEEDBACK_MISSING_UNITY_VISUAL_PROOF');
 if(review.before_sha256!==sha(original)||review.after_sha256!==sha(candidate))
    throw Error('ART_FEEDBACK_REVIEW_IMAGE_HASH_MISMATCH');
 const event={
  schema_version:1,kind:'eldoria_art_reviewed_lesson',
  source_event_sha256:sha(requireFile(feedbackPath)),
  verdict:review.verdict,independent_review_sha256:sha(requireFile(reviewPath)),
  before_sha256:sha(original),after_sha256:sha(candidate),
  camera_matrix_sha256:review.camera_matrix_sha256,
  knowledge_ids:feedback.knowledge_ids,catalog_sha256:feedback.catalog_sha256,
  lesson:String(review.lesson||'').slice(0,1100), next_action:String(review.next_action||'').slice(0,1100),
  verified_files:true,review_state:'INDEPENDENT_EVIDENCE_RECORD_READY',
  art_approved:false,can_enter_canonical_learning:false,
  reason:'Validated for staging only. Governed human/owner-approved admission required; never auto-promote gameplay art.'
 };
 if(event.lesson.length<35||event.next_action.length<25)throw Error('ART_FEEDBACK_INCOMPLETE_LEARNING');
 return writeOnce(outFile,event);
}
if(process.argv[1]?.endsWith('cycle-feedback.mjs')){
 try{
  const [operation,...args]=process.argv.slice(2);
  if(operation!=='review'||args.length!==5)throw Error('Usage: node cycle-feedback.mjs review ATTEMPT.json before.png after.png INDEPENDENT_REVIEW.json output.json');
  const r=stageReviewed({feedbackPath:args[0],beforePath:args[1],afterPath:args[2],reviewPath:args[3],outFile:args[4]});
  console.log('ART_FEEDBACK_REVIEW_STAGED',r.verdict);
 }catch(e){console.error(e.message);process.exitCode=1;}
}
