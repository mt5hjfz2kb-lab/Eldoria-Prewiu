// Evidence-gated append-only learning journal: independently reviewed outcomes.
// No automatic claims of art approval, no direct writes to the canonical source.
import fs from 'node:fs';
import crypto from 'node:crypto';
import path from 'node:path';
const hash=bytes=>crypto.createHash('sha256').update(bytes).digest('hex');
const HEX=/^[0-9a-f]{64}$/;
function read(file){const b=fs.readFileSync(file);if(!b.length||b.length>25_000_000)throw Error('ART_REVIEW_INVALID_SOURCE_BYTES');return b;}
export function validateJournal(j){
 if(j?.schema_version!==1||!Array.isArray(j.records))throw Error('ART_REVIEW_INVALID_JOURNAL');
 const ids=new Set();
 for(const entry of j.records){
  if(!/^L[a-f0-9]{20}$/.test(entry.id)||ids.has(entry.id))throw Error('ART_REVIEW_DUPLICATE_OR_INVALID_ID');
  ids.add(entry.id);
  if(!['reviewed_positive','reviewed_negative'].includes(entry.classification))throw Error('ART_REVIEW_INVALID_CLASSIFICATION');
  if(!HEX.test(entry.evidence_sha256||'')||!HEX.test(entry.catalog_sha256||'')||
     !HEX.test(entry.review_sha256||'')||!HEX.test(entry.before_sha256||'')||!HEX.test(entry.after_sha256||'')||
     entry.art_approved!==false||entry.quality_verified!=='INDEPENDENT_SCOPE_ONLY'||
     typeof entry.lesson!=='string'||entry.lesson.length<35)throw Error('ART_REVIEW_UNTRUSTED_ENTRY');
 }
 return j;
}
export function ingestReviewed({journal,stagedEvent,feedback,review,beforeBytes,afterBytes}){
 validateJournal(journal);
 if(stagedEvent?.schema_version!==1||stagedEvent.kind!=='eldoria_art_reviewed_lesson'||
    stagedEvent.review_state!=='INDEPENDENT_EVIDENCE_RECORD_READY'||stagedEvent.art_approved!==false||
    stagedEvent.can_enter_canonical_learning!==false||
    stagedEvent.verified_files!==true)throw Error('ART_REVIEW_NOT_INDEPENDENTLY_STAGED');
 if(stagedEvent.source_event_sha256!==hash(feedback)||
    stagedEvent.independent_review_sha256!==hash(review)||
    stagedEvent.before_sha256!==hash(beforeBytes)||
    stagedEvent.after_sha256!==hash(afterBytes))throw Error('ART_REVIEW_ORIGINAL_EVIDENCE_HASH_MISMATCH');
 const raw=JSON.parse(review);
 if(raw.independent_reviewer!=='EXTERNAL_TO_AUTHOR'||raw.verdict!==stagedEvent.verdict||
   !raw.original_files_inspected||!raw.same_camera_verified||!raw.same_light_verified||
   !raw.same_resolution_verified||!HEX.test(raw.camera_matrix_sha256||'')||
   raw.before_sha256!==hash(beforeBytes)||raw.after_sha256!==hash(afterBytes))
   throw Error('ART_REVIEW_NO_INDEPENDENT_MATCHED_VISUAL_PROOF');
 const fb=JSON.parse(feedback);
 if(fb.art_approved!==false||fb.qwen_consumed_catalog!==true||!fb.actual_inference_sha256||
    fb.catalog_sha256!==stagedEvent.catalog_sha256)throw Error('ART_REVIEW_UNTRUSTED_AUTHOR_OR_CATALOG');
 const evidence_sha256=hash(Buffer.from(JSON.stringify(stagedEvent)));
 const id='L'+evidence_sha256.slice(0,20);
 if(journal.records.some(x=>x.id===id||x.evidence_sha256===evidence_sha256))throw Error('ART_REVIEW_ALREADY_ADMITTED');
 const next=structuredClone(journal);
 next.records.push({
  id,classification:raw.verdict==='ART_REJECTED'?'reviewed_negative':'reviewed_positive',
  lesson:stagedEvent.lesson,next_action:stagedEvent.next_action,
  knowledge_ids:stagedEvent.knowledge_ids,
  origin_request:fb.request_id,model:fb.requested_model,
  evidence_sha256,catalog_sha256:stagedEvent.catalog_sha256,
  review_sha256:stagedEvent.independent_review_sha256,
  before_sha256:stagedEvent.before_sha256,after_sha256:stagedEvent.after_sha256,
  camera_matrix_sha256:stagedEvent.camera_matrix_sha256,
  quality_verified:'INDEPENDENT_SCOPE_ONLY',art_approved:false,
  source_author_inference_sha256:fb.actual_inference_sha256
 });
 return validateJournal(next);
}
if(process.argv[1]&&path.resolve(process.argv[1])===new URL(import.meta.url).pathname){
 try{
  const [input,staged,feedback,review,before,after,out]=process.argv.slice(2);
  if(!out)throw Error('Usage: node reviewed-ledger.mjs ORIGINAL_JOURNAL.json STAGED.json FEEDBACK.json REVIEW.json before.png after.png OUT.json');
  if(fs.existsSync(out))throw Error('ART_REVIEW_REFUSE_OVERWRITE');
  const next=ingestReviewed({journal:JSON.parse(read(input)),stagedEvent:JSON.parse(read(staged)),
    feedback:read(feedback),review:read(review),beforeBytes:read(before),afterBytes:read(after)});
  fs.writeFileSync(out,JSON.stringify(next,null,2)+'\n',{flag:'wx'});
  console.log('ART_REVIEWED_LESSON_READY_FOR_CANONICAL_COMMIT '+next.records.at(-1).id);
 }catch(e){console.error('ART_REVIEW_ADMISSION_FAILED '+e.message);process.exitCode=1;}
}
