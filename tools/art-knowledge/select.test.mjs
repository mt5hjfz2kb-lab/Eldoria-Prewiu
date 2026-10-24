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
