import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {readCatalog,selectKnowledge,writePrepared} from './select.mjs';

const source=readCatalog();
function req(profile,sector,diagnosis=''){return {profile,request_id:'test-art-memory-20261021',workstream_id:'test-art-memory-20261021',asset_or_sector:sector,diagnosis,allow_tripo:false,credit_authorization:{approved:false,authorized_credit_cost:0}};}
test('real catalog is valid and historical source SHAs are pinned',()=>{
 assert.ok(source.catalog.records.length>=54);
 assert.equal(source.catalog.safe_defaults.may_publish,false);
 for(const rec of source.catalog.records){for(const s of rec.historical_sources||[])assert.match(s.sha,/^[a-f0-9]{40}$/);}
});
test('forest request retrieves useful historical lessons and no rejected method as recommendation',()=>{
 const out=selectKnowledge(req('environment_composition','bosque y cantera, vegetación, árboles','mejorar pinos bosque y textura del terreno'),source);
 assert.equal(out.status,'MATCHED_EVIDENCE_READ_ONLY');
 assert.ok(out.selected_ids.length>0);
 assert.ok(out.recommendations.every(x=>x.evidence_status!=='art_rejected'));
 assert.ok(out.inventory_text.includes('SOURCE catalog SHA256'));
 assert.equal(out.qa_boundary.qwen_has_received_inventory,false);
 assert.equal(out.qa_boundary.art_approved,false);
 assert.equal(out.qa_boundary.paid_calls_authorized,false);
});
test('creature uses relevant creature lessons, with rejected microedits only in do-not-use',()=>{
 const x=selectKnowledge(req('animated_asset','Engendro de la Fisura criatura, rig y animación','rostro y zarpazo deben mantener pies y silhouette'),source);
 assert.ok(x.recommendations.some(i=>/creature|optimization|artificial_intelligence/.test(source.catalog.records.find(r=>r.id===i.id).family)));
 assert.ok(!x.recommendations.some(i=>i.evidence_status==='art_rejected'));
 assert.equal(x.qa_boundary.unity_modified,false);
});
test('no authority to invent artist approval on requests',()=>{
 const x=selectKnowledge(req('environment_new_geometry','pared','silueta poco convincente'),source);
 assert.equal(x.qa_boundary.art_approved,false);
 assert.equal(x.qa_boundary.artist_decisions_executed,false);
 assert.ok(x.catalog_sha256.length===64);
});
test('same request returns stable ids and provenance',()=>{
 const q=req('environment_surface','piedra de puente','material UV y glTF');
 const a=selectKnowledge(q,source),b=selectKnowledge(q,source);
 assert.deepEqual(a,b);
});
test('outputs are immutable and hashes match, never overwritten',()=>{
 const tmp=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-art-memory-'));
 try{
  const x=selectKnowledge(req('animated_asset','Jabalí, anima criatura'),source);
  const prefix=path.join(tmp,'unit');
  const created=writePrepared(x,prefix);
  assert.equal(created.length,2);
  assert.match(fs.readFileSync(prefix+'.knowledge.txt','utf8'),/ELDORIA HISTORICAL ART KNOWLEDGE/);
  assert.throws(()=>writePrepared(x,prefix),/Refuse overwrite/);
 }finally{fs.rmSync(tmp,{recursive:true,force:true});}
});
test('rejects unknown request profiles and malformed identifiers',()=>{
 assert.throws(()=>selectKnowledge(req('invalid','x'),source),/INVALID_PROFILE/);
 assert.throws(()=>selectKnowledge({profile:'animated_asset',request_id:'../../bad'},source),/INVALID_WORKSTREAM_ID/);
});

test('rejected technique remains visible in truncated 1800-character Qwen memory',()=>{
 const base=source.catalog.records.find(x=>x.evidence_status==='art_rejected')||source.catalog.records[0];
 const bad={...base,id:'K999',family:'environment',evidence_status:'art_rejected',
   problem:'bridge terrain shoreline camera world4x',
   observed_result:'Small bridge texture tweaks hide the composition failure',
   avoid:'NO_REPEAT_MICRO_BRIDGE_REPAINT: redesign the actual landmass silhouette',
   source_paths:['docs/evidence/verified-rejected-example.md']};
 const synthetic={...source,catalog:{...source.catalog,records:[bad,...source.catalog.records]}};
 const selection=selectKnowledge(req('environment_composition','bridge terrain shoreline camera world4x',
   'bridge terrain shoreline camera world4x'),synthetic);
 assert.ok(selection.rejected_ids.includes('K999'));
 assert.ok(!selection.selected_ids.includes('K999'));
 assert.match(selection.inventory_text.slice(0,1000),/NO_REPEAT_MICRO_BRIDGE_REPAINT/);
 assert.ok(selection.inventory_text.indexOf('REJECTED CASES TO AVOID')<
   selection.inventory_text.indexOf('SUGGESTED RELEVANT METHODS'));
 assert.equal(selection.qa_boundary.art_approved,false);
});

test('two independent, admitted negative reviews for the same request signal a structural pivot',()=>{
 const base=req('environment_composition','bosque pradera ribera','composición y terreno');
 const sample=(suffix,kind='reviewed_negative')=>({
   id:'L'+suffix.repeat(20),origin_request:base.workstream_id,classification:kind,
   evidence_sha256:suffix.repeat(64),review_sha256:suffix.repeat(64),
   lesson:'The same geometry-only micro-correction did not solve the real landscape composition.',
   next_action:'Change overall terrain composition and compare the same true Unity camera.',
   knowledge_ids:['K20'],art_approved:false,quality_verified:'INDEPENDENT_SCOPE_ONLY',
   catalog_sha256:'a'.repeat(64),before_sha256:'b'.repeat(64),after_sha256:'c'.repeat(64)
 });
 const modified={...source,journal:{...source.journal,records:[sample('1'),sample('2')]}};
 const selected=selectKnowledge(base,modified);
 assert.equal(selected.pivot_advisory.method_change_required,true);
 assert.deepEqual(selected.pivot_advisory.evidence_ids,['L'+'1'.repeat(20),'L'+'2'.repeat(20)]);
 assert.match(selected.inventory_text,/CRITICAL: Two independently reviewed attempts/);
 assert.equal(selected.qa_boundary.artist_decisions_executed,false);
 const withPositive=selectKnowledge(base,{...modified,journal:{...modified.journal,records:[sample('1'),sample('2'),sample('3','reviewed_positive')]}});
 assert.equal(withPositive.pivot_advisory.method_change_required,false,'Positive review interrupts the negative sequence');
 const foreign=selectKnowledge({...base,request_id:'foreign',workstream_id:'foreign'},modified);
 assert.equal(foreign.pivot_advisory.method_change_required,false,'Never transfer failures from another workstream');
 const duplicate=selectKnowledge(base,{...modified,journal:{...modified.journal,records:[sample('1'),{...sample('2'),review_sha256:'1'.repeat(64)}]}});
 assert.equal(duplicate.pivot_advisory.method_change_required,false,'Duplicated review evidence is not two independent failures');
});
