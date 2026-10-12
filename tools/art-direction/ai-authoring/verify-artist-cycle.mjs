#!/usr/bin/env node
// Enforces traceable AI-directed Blender authorship before artistic approval.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
export const APPROVED=new Set(['ART_APPROVED','MOBILE_VALIDATED','GAMEPLAY_VALIDATED','RELEASED']);
const GEOMETRY=new Set(['sculpt_region','reprofile_crest','construct_mesh','remesh','mesh_cut','deform_curve','combine_meshes']);
const ACTIONS=new Set([...GEOMETRY,'assign_material','place_existing_asset','terrain_scatter','light_scene']);
const SHA=/^[a-f0-9]{64}$/i, nonempty=x=>typeof x==='string'&&x.trim().length>0;
export function verifyCycle(c,root=process.cwd()){
 const errors=[],need=(ok,msg)=>{if(!ok)errors.push(msg)};
 const file=(entry,label)=>{
  need(entry&&nonempty(entry.path)&&SHA.test(entry.sha256||''),label+': path and SHA256 required');
  if(!entry?.path||!SHA.test(entry.sha256||''))return;
  const resolved=path.resolve(root,entry.path),prefix=path.resolve(root)+path.sep;
  if(!resolved.startsWith(prefix)){errors.push(label+': file outside repository');return;}
  if(!fs.existsSync(resolved)){errors.push(label+': missing file '+entry.path);return;}
  need(crypto.createHash('sha256').update(fs.readFileSync(resolved)).digest('hex')===entry.sha256.toLowerCase(),label+': hash mismatch');
 };
 need(c?.schema_version===1,'schema_version=1 required');
 need(nonempty(c?.asset_id)&&nonempty(c?.workstream_id),'asset/workstream identity required');
 need(nonempty(c?.state),'cycle state required');
 if(!APPROVED.has(c?.state))return errors; // WIP and prototype explicitly are NOT artistic approvals.
 need(c?.authorship?.mode==='AI_DIRECTED_BLENDER','AI_DIRECTED_BLENDER required');
 need(c?.artist?.system_role==='system','Ollama artist profile must be actual system role');
 need(nonempty(c?.artist?.model),'local model identity required');
 need((c?.artist?.observed_defect||'').length>=35,'specific visual diagnosis required');
 need((c?.artist?.design_intent||'').length>=35,'artistic design intent required');
 need(['REUSE','MODIFY','NEW'].includes(c?.artist?.asset_decision),'reuse/modify/new decision required');
 need(nonempty(c?.artist?.reason_for_new_geometry),'asset reuse/new rationale required');
 for(const [name,item] of Object.entries({
  artist_system_prompt:c?.artist?.system_prompt,artist_inference:c?.artist?.inference_log,
  owner_reference:c?.reference,unity_before:c?.unity?.before,source_blend:c?.authorship?.source_blend,
  output_blend:c?.authorship?.result_blend,output_glb:c?.authorship?.result_glb,
  blender_execution:c?.authorship?.execution_log,unity_after:c?.unity?.after,unity_import:c?.unity?.import_log
 }))file(item,name);
 const planned=c?.authorship?.planned_actions, executed=c?.authorship?.executed_actions;
 need(Array.isArray(planned)&&planned.length>0,'prior artist actions required');
 need(Array.isArray(executed)&&executed.length>0,'real executed actions required');
 const ids=new Set();
 for(const op of planned||[]){
  need(nonempty(op.id)&&!ids.has(op.id),'unique planned action id required');ids.add(op.id);
  need(ACTIONS.has(op.type),'unsupported/preset-only action '+op.type);
  need(nonempty(op.target)&&(op.rationale||'').length>=15,'object and artistic rationale required');
  need(op.parameters&&Object.keys(op.parameters).length>0,'action parameters required');
 }
 const done=new Set();
 for(const op of executed||[]){
  need(ids.has(op.id)&&!done.has(op.id),'executed action not in prior decision/duplicate');done.add(op.id);
  need(op.applied===true&&nonempty(op.target)&&(Number(op.changed_vertices||0)>0||Number(op.changed_objects||0)>0),'measurable executed change required');
 }
 need(ids.size===done.size&&[...ids].every(x=>done.has(x)),'all approved actions must execute');
 const sourceHash=c?.authorship?.source_mesh_sha256,resultHash=c?.authorship?.result_mesh_sha256;
 need(SHA.test(sourceHash||'')&&SHA.test(resultHash||''),'mesh source/result hashes required');
 if(c?.geometry_gap_proven===true){
  need((planned||[]).some(x=>GEOMETRY.has(x.type)),'geometry gap requires structural modelling');
  need(sourceHash!==resultHash,'source/result mesh unchanged');
 }else{
  // Material/composition-only improvement may keep original mesh but must show independently reviewed change.
  need(sourceHash!==resultHash||(SHA.test(c?.authorship?.source_appearance_sha256||'')&&
    SHA.test(c?.authorship?.result_appearance_sha256||'')&&
    c.authorship.source_appearance_sha256!==c.authorship.result_appearance_sha256),
    'no demonstrable geometry or material/composition change');
 }
 need(c?.unity?.same_camera===true&&SHA.test(c?.unity?.camera_matrix_sha256||''),'matched-camera Unity A/B required');
 need(c?.reviews?.technical?.verdict==='PASS','technical verification required');
 need(c?.reviews?.visual?.verdict==='PASS'&&c.reviews.visual.independent===true&&nonempty(c.reviews.visual.visible_difference),'independent visual review required');
 need(nonempty(c?.license?.provenance),'provenance/license evidence required');
 if(['MOBILE_VALIDATED','GAMEPLAY_VALIDATED','RELEASED'].includes(c.state))need(c?.mobile?.verdict==='PASS','physical-device validation required');
 if(['GAMEPLAY_VALIDATED','RELEASED'].includes(c.state))need(c?.gameplay?.verdict==='PASS','gameplay regression required');
 if(c.state==='RELEASED')need(c?.release?.verified===true&&nonempty(c.release.artifact_sha256),'released artifact validation required');
 return errors;
}
if(process.argv[1]&&path.resolve(process.argv[1])===path.resolve(new URL(import.meta.url).pathname)){
 const file=process.argv[2];
 if(!file){console.error('Usage: node verify-artist-cycle.mjs artist-cycle.json [repo-root]');process.exit(2)}
 try{
  const c=JSON.parse(fs.readFileSync(file,'utf8')),problems=verifyCycle(c,process.argv[3]||process.cwd());
  if(problems.length){problems.forEach(p=>console.error('ART_CYCLE_FAIL: '+p));process.exit(1)}
  console.log(APPROVED.has(c.state)?'ART_CYCLE_EVIDENCE_PASS (visual merit remains independent)':'ART_CYCLE_WIP_NO_APPROVAL');
 }catch(e){console.error('ART_CYCLE_FAIL: '+e.message);process.exit(2)}
}
