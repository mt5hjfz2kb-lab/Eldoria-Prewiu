import test from 'node:test';
import assert from 'node:assert/strict';
import crypto from 'node:crypto';
import {validateJournal,ingestReviewed} from './reviewed-ledger.mjs';
import {readCatalog,selectKnowledge} from './select.mjs';
const hash=bytes=>crypto.createHash('sha256').update(bytes).digest('hex');
test('reviewed lesson journal is an empty, honest real baseline',()=>{
 const x=readCatalog();
 assert.equal(x.journal.records.length,0);
 assert.ok(x.journal_sha256?.length===64);
});
test('independently reviewed negative is usable as future warning, never automatically approved',()=>{
 const before=Buffer.from('PNG image bytes original'),after=Buffer.from('PNG image bytes after');
 const fb={schema_version:1,kind:'eldoria_art_cycle_feedback',request_id:'test-artist',
  requested_model:'qwen3-vl:2b-instruct',actual_inference_sha256:'f'.repeat(64),
  qwen_consumed_catalog:true,catalog_sha256:'c'.repeat(64),art_approved:false};
 const review={verdict:'ART_REJECTED',independent_reviewer:'EXTERNAL_TO_AUTHOR',
  original_files_inspected:true,same_camera_verified:true,same_light_verified:true,
  same_resolution_verified:true,camera_matrix_sha256:'d'.repeat(64),
  before_sha256:hash(before),after_sha256:hash(after)};
 const feedback=Buffer.from(JSON.stringify(fb)),reviewBytes=Buffer.from(JSON.stringify(review));
 const staged={schema_version:1,kind:'eldoria_art_reviewed_lesson',
  review_state:'INDEPENDENT_EVIDENCE_RECORD_READY',art_approved:false,
  can_enter_canonical_learning:false,verified_files:true,
  source_event_sha256:hash(feedback),independent_review_sha256:hash(reviewBytes),
  before_sha256:hash(before),after_sha256:hash(after),
  camera_matrix_sha256:'d'.repeat(64),catalog_sha256:'c'.repeat(64),
  knowledge_ids:['K19'],verdict:'ART_REJECTED',
  lesson:'No volver a repetir el desplazamiento global del Engendro desde cámara estratégica',
  next_action:'Comprobar anatomía local real y cambio visible'};
 const empty={schema_version:1,records:[]};
 const journal=ingestReviewed({journal:empty,stagedEvent:staged,feedback,review:reviewBytes,beforeBytes:before,afterBytes:after});
 assert.equal(journal.records[0].classification,'reviewed_negative');
 assert.equal(journal.records[0].art_approved,false);
 assert.throws(()=>ingestReviewed({journal,stagedEvent:staged,feedback,review:reviewBytes,beforeBytes:before,afterBytes:after}),/ALREADY_ADMITTED/);
 const source=readCatalog();source.journal=journal;
 const result=selectKnowledge({profile:'animated_asset',request_id:'bench-engendro',asset_or_sector:'Engendro',diagnosis:'desplazamiento global anatomía'},source);
 assert.ok(result.reviewed_ids.length>0);
 assert.match(result.inventory_text,/INDEPENDENTLY REVIEWED NEW OBSERVATIONS/);
 assert.ok(result.qa_boundary.art_approved===false);
 const tampered=Buffer.from('altered');
 assert.throws(()=>ingestReviewed({journal:empty,stagedEvent:staged,feedback,review:reviewBytes,beforeBytes:before,afterBytes:tampered}),/HASH_MISMATCH/);
});
