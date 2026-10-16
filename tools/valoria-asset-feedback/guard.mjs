#!/usr/bin/env node
// Eldoria Valoria 63 source-independent feedback guard. No network, Unity, Blender or paid actions.
// Usage: node guard.mjs validate [registry.json] [asset-ids.json]
//        node guard.mjs check <artist|unity> candidate.json [registry.json] [asset-ids.json]
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
const modulePath=fileURLToPath(import.meta.url);
const base=path.dirname(modulePath);
const load=(p)=>JSON.parse(fs.readFileSync(p,'utf8'));
const regDefault=path.join(base,'registry.json'),idsDefault=path.join(base,'asset-ids.json');
const SHA=/^[a-f0-9]{64}$/;
function requireValid(ok,msg){if(!ok)throw Error(msg)}
export function validateRegistry(reg,ids) {
 requireValid(reg.schema==='eldoria.valoria63.asset_feedback.v1','bad schema');
 requireValid(Array.isArray(ids)&&ids.length===63&&new Set(ids).size===63,'canonical catalog must contain 63 unique assets');
 requireValid(Array.isArray(reg.assets)&&reg.assets.length===63,'registry must contain 63 per-asset entries');
 const map=new Map();
 for(const a of reg.assets){
  requireValid(typeof a.id==='string'&&ids.includes(a.id),'unknown asset '+a.id);
  requireValid(!map.has(a.id),'duplicate asset '+a.id);
  requireValid(Array.isArray(a.correction_ids),'missing correction IDs on '+a.id);
  if(a.preferred_version)requireValid(SHA.test(a.preferred_version.sha256),'bad preferred sha '+a.id);
  map.set(a.id,a);
 }
 requireValid(ids.every(id=>map.has(id)),'missing per-asset record(s)');
 requireValid(Array.isArray(reg.corrections),'missing corrections');
 const cm=new Map();
 for(const c of reg.corrections){
  requireValid(typeof c.id==='string'&&!cm.has(c.id),'duplicate/bad correction id');
  requireValid(map.has(c.asset_id),'unknown correction asset '+c.id);
  requireValid(['ASSET_GLOBAL','ASSEMBLY_CONTRACT','SCENE_LOCAL'].includes(c.scope),'invalid scope '+c.id);
  requireValid(['TRIAL_PENDING','OBSERVED_NEEDS_REVIEW','STRUCTURE_VERIFIED','ART_ACCEPTED'].includes(c.state),'invalid state '+c.id);
  requireValid(SHA.test(c.source_sha256)&&/^[a-f0-9]{40}$/.test(c.commit),'invalid source provenance '+c.id);
  requireValid(Number.isInteger(c.unity_run_id)&&c.unity_run_id>0,'missing Unity run '+c.id);
  requireValid(typeof c.summary==='string'&&c.summary.length>15&&typeof c.regression==='string'&&c.regression.length>10,'missing rationale '+c.id);
  requireValid(/^https:\/\//.test(c.proof),'missing evidence link '+c.id);
  cm.set(c.id,c);
 }
 for(const a of reg.assets){
  requireValid(new Set(a.correction_ids).size===a.correction_ids.length,'duplicate correction reference '+a.id);
  for(const id of a.correction_ids)requireValid(cm.has(id)&&cm.get(id).asset_id===a.id,'invalid correction ref '+a.id+'/'+id);
 }
 for(const c of reg.corrections)requireValid(map.get(c.asset_id).correction_ids.includes(c.id),'orphan correction '+c.id);
 return {assets:reg.assets.length,corrections:cm.size};
}
export function checkCandidate(phase,cand,reg,ids){
 validateRegistry(reg,ids);
 requireValid(['artist','unity'].includes(phase),'phase must be artist or unity');
 const a=reg.assets.find(a=>a.id===cand.asset_id);
 requireValid(!!a,'unknown candidate asset');
 requireValid(SHA.test(cand.sha256),'candidate missing exact GLB SHA-256');
 requireValid(/^[a-f0-9]{40}$/.test(cand.commit),'candidate missing 40-char authoring commit');
 requireValid(/^https:\/\//.test(cand.blender_ab_url),'candidate missing independently inspectable Blender A/B');
 requireValid(Array.isArray(cand.decisions),'candidate requires correction decision array');
 const seen=new Set();
 for(const d of cand.decisions){
  requireValid(a.correction_ids.includes(d.id)&&!seen.has(d.id),'unexpected/duplicate correction decision '+d.id);seen.add(d.id);
  const c=reg.corrections.find(x=>x.id===d.id);
  requireValid(['applied','scene_only','preserved_assembly','pending_review'].includes(d.disposition),'invalid disposition '+d.id);
  requireValid(/^https:\/\//.test(d.evidence_url),'missing evidence URL '+d.id);
  if(c.scope==='SCENE_LOCAL')requireValid(['scene_only','pending_review'].includes(d.disposition),'do not transfer scene-level fix globally '+d.id);
  if(c.scope==='ASSEMBLY_CONTRACT')requireValid(['preserved_assembly','pending_review'].includes(d.disposition),'assembly contract must be checked separately '+d.id);
  if(c.scope==='ASSET_GLOBAL')requireValid(['applied','pending_review'].includes(d.disposition),'asset-level defect must be explicitly addressed '+d.id);
  if(d.disposition==='applied')requireValid(!!d.validation_ab_url&&/^https:\/\//.test(d.validation_ab_url),'applied change needs A/B '+d.id);
 }
 for(const id of a.correction_ids)requireValid(seen.has(id),'unacknowledged correction '+id);
 const pending=cand.decisions.filter(d=>d.disposition==='pending_review');
 if(cand.ready_for_unity)requireValid(pending.length===0,'unresolved defect; cannot mark candidate ready for Unity');
 if(phase==='unity'){
  const u=cand.unity;
  requireValid(u&&u.same_camera===true,'Unity same-camera check required');
  requireValid(Number.isInteger(u.run_id)&&u.run_id>0&&Number.isInteger(u.artifact_id)&&u.artifact_id>0,'Unity exact run/artifact required');
  requireValid(/^https:\/\//.test(u.before_url)&&/^https:\/\//.test(u.after_url),'Unity before/after URLs required');
  requireValid(u.art_approved===false || (u.art_approved===true&&u.visual_review==='PASS'&&pending.length===0),'no art approval from technical success or unreviewed changes');
 }
 return {asset_id:cand.asset_id,phase,corrections_checked:a.correction_ids.length,pending:pending.length,ready_for_unity:!!cand.ready_for_unity};
}
const invoked=process.argv[1]&&path.resolve(process.argv[1])===path.resolve(modulePath);
if(invoked)try{
 const [mode,...argv]=process.argv.slice(2);
 if(mode==='validate'){const r=validateRegistry(load(argv[0]||regDefault),load(argv[1]||idsDefault));console.log('PASS',JSON.stringify(r));}
 else if(mode==='check'){const [phase,file,registryPath,idsPath]=argv;requireValid(!!file,'usage: check <artist|unity> candidate.json');const r=checkCandidate(phase,load(file),load(registryPath||regDefault),load(idsPath||idsDefault));console.log('PASS',JSON.stringify(r));}
 else throw Error('usage: validate [registry] [ids] | check <artist|unity> candidate.json [registry] [ids]');
}catch(error){console.error('FAIL',error.message);process.exitCode=1;}
