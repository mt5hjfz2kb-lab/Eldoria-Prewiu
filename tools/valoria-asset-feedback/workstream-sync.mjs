#!/usr/bin/env node
// Side-effect-free preflight gate for the two active Valoria production workstreams.
// Reads *live main* registry/identity files downloaded by the workflow, not branch-stale snapshots.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {fileURLToPath} from 'node:url';
import {validateRegistry} from './guard.mjs';
const root=path.dirname(fileURLToPath(import.meta.url));
const fail=(condition,message)=>{if(!condition)throw Error(message)};
const read=p=>JSON.parse(fs.readFileSync(p,'utf8'));
const SHA=/^[0-9a-f]{64}$/;
export function validateWorkstream(workstream,plan,registry,ids,selected=null){
 validateRegistry(registry,ids);
 fail(plan.schema==='eldoria.valoria63.workstream-feedback.v1','feedback plan schema mismatch');
 fail(plan.workstream===workstream,'workstream mismatch');
 fail(['EXPERIMENT_ONLY','PROMOTION_REVIEW'].includes(plan.phase),'invalid phase');
 fail(plan.art_approved===false,'ART_APPROVED must not be inferred from CI');
 fail(/^[0-9a-f]{40}$/.test(plan.source_git_blob_sha1||''),'exact source file blob identity not acknowledged');
 fail(typeof plan.source_file==='string'&&plan.source_file.length>15,'missing tracked Blender/Unity source path');
 fail(Array.isArray(plan.corrections),'missing explicit corrections');
 const byId=new Map(registry.corrections.map(c=>[c.id,c]));
 const seen=new Set();
 for(const item of plan.corrections){
  const c=byId.get(item.id);
  fail(!!c&&!seen.has(item.id),'unexpected or repeated correction '+item.id);seen.add(item.id);
  fail(typeof item.rationale==='string'&&item.rationale.length>=24,'rationale missing for '+item.id);
  fail(item.evidence_url===c.proof,'evidence mismatch for '+item.id);
  const allowed=c.scope==='SCENE_LOCAL'?['scene_only','pending_review']:
   c.scope==='ASSEMBLY_CONTRACT'?['preserved_assembly','pending_review']:['applied','pending_review'];
  fail(allowed.includes(item.disposition),'illegal transfer of '+c.scope+' correction '+item.id);
  if(item.disposition==='applied')fail(/^https:\/\//.test(item.validation_ab_url||''),'applied improvement missing fixed-camera A/B '+item.id);
  if(plan.phase==='PROMOTION_REVIEW')fail(item.disposition!=='pending_review','unreviewed correction blocks promotion '+item.id);
 }
 for(const c of registry.corrections)fail(seen.has(c.id),'unacknowledged correction '+c.id);
 const pending=plan.corrections.filter(i=>i.disposition==='pending_review').length;
 if(workstream==='artist'){
  fail(plan.source_asset_count===63,'artist must preserve exact 63-asset inventory');
  if(plan.phase==='PROMOTION_REVIEW')fail(!!plan.blender_ab_review_url,'no Blender art review proof');
 }else if(workstream==='unity'){
  fail(selected&&selected.base_63?.expected_count===63,'missing 63-base selection evidence');
  fail(Array.isArray(selected.selected)&&Array.isArray(plan.selected_versions),'selected asset evidence unavailable');
  const pinned=new Map(selected.selected.map(x=>[x.id,x]));
  fail(selected.selected.length===pinned.size,'duplicate selected asset');
  fail(plan.selected_versions.length===selected.selected.length,'selected versions not all acknowledged');
  const planMap=new Map(plan.selected_versions.map(x=>[x.id,x]));
  fail(planMap.size===plan.selected_versions.length,'duplicate selected feedback record');
  const known=new Set(ids);
  for(const [id,item] of pinned){
   fail(known.has(id),'unrecognized selected module '+id);
   const a=planMap.get(id);
   fail(a&&SHA.test(item.sha256)&&a.sha256===item.sha256,'selected version SHA changed without feedback refresh '+id);
   fail(a.usage===item.usage,'selected usage changed without feedback refresh '+id);
  }
  if(plan.phase==='PROMOTION_REVIEW'){
   fail(selected.constraints?.art_approved===true,'cannot promote Unity candidate with art rejection');
   fail(/^https:\/\//.test(plan.unity_ab_review_url||''),'native Unity matched-camera visual evidence required');
  }
 }else throw Error('unknown workstream '+workstream);
 return {workstream,asset_count:ids.length,feedback_count:seen.size,pending,phase:plan.phase,selected:workstream==='unity'?selected.selected.length:ids.length,art_approved:false};
}
export function validateArtistOutput(outputDir,plan,registry,ids){
 const r=validateWorkstream('artist',plan,registry,ids);
 const status=read(path.join(outputDir,'individual_quality_status.json'));
 const entries=status.assets;
 fail(Array.isArray(entries)&&entries.length===63,'output has not exactly 63 asset statuses');
 const seen=new Set();
 const receipt=[];
 for(const x of entries){
  fail(ids.includes(x.asset)&&!seen.has(x.asset),'unregistered/duplicate produced asset '+x.asset);
  seen.add(x.asset);
  fail(x.technical==='GLB_EXPORTED'&&x.art_approved===false,'artist GLB output not safely staged '+x.asset);
  const file=path.join(outputDir,'glb',x.asset+'.glb');
  const digest=crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex');
  fail(x.glb_sha256===digest,'mismatched artifact hash '+x.asset);
  fail(fs.existsSync(path.join(outputDir,'previews',x.individual_front))&&fs.existsSync(path.join(outputDir,'previews',x.individual_rear)),'missing 2-view evidence '+x.asset);
  const corr=registry.assets.find(y=>y.id===x.asset);
  receipt.push({asset_id:x.asset,sha256:digest,correction_ids:corr.correction_ids,art_approved:false});
 }
 fail(seen.size===63,'missing source module');
 const report={...r,assets:receipt,correction_snapshot:registry.corrections.map(x=>x.id)};
 fs.writeFileSync(path.join(outputDir,'asset_feedback_receipt.json'),JSON.stringify(report,null,2));
 return {asset_count:receipt.length,corrections:r.feedback_count,art_approved:false};
}
export function verifySourceRevision(plan,filename){
 const content=fs.readFileSync(filename);
 const real=crypto.createHash('sha1').update(Buffer.from('blob '+content.length+'\\0')).update(content).digest('hex');
 fail(plan.source_git_blob_sha1===real,'source changed without per-asset feedback review: '+plan.source_file);
 return real;
}
const invoked=process.argv[1]&&path.resolve(process.argv[1])===path.resolve(fileURLToPath(import.meta.url));
if(invoked)try{
 const [mode,planFile,selectionFile,registryFile,idsFile,sourceFile]=process.argv.slice(2);
 fail(['artist','unity','artist-output'].includes(mode),'mode must be artist|unity|artist-output');
 fail(!!planFile,'feedback plan file required');
 const registry=read(registryFile||path.join(root,'registry.json'));
 const ids=read(idsFile||path.join(root,'asset-ids.json'));
 const plan=read(planFile);
 fail(!!sourceFile,'exact authored Blender/Unity source file is required');
 const source_blob_sha=verifySourceRevision(plan,sourceFile);
 const report=mode==='artist-output'?validateArtistOutput(selectionFile,plan,registry,ids):
   validateWorkstream(mode,plan,registry,ids,mode==='unity'?read(selectionFile):null);
 process.stdout.write('VALORIA63_FEEDBACK_GATE_PASS '+JSON.stringify({...report,source_blob_sha})+'\n');
}catch(e){console.error('VALORIA63_FEEDBACK_GATE_REJECT',e.message);process.exitCode=1}
