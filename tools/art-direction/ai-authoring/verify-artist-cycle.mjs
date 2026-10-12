#!/usr/bin/env node
// Enforces traceable AI-directed Blender authorship before artistic approval.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
export const APPROVED=new Set(['ART_APPROVED','MOBILE_VALIDATED','GAMEPLAY_VALIDATED','RELEASED']);
const GEOMETRY=new Set(['sculpt_region','reprofile_crest','sculpt_path','bridge_boundary_loops','construct_mesh']);
const ACTIONS=new Set([...GEOMETRY,'assign_material','place_existing_asset']);
const SHA=/^[a-f0-9]{64}$/i, nonempty=x=>typeof x==='string'&&x.trim().length>0;
// Low-cost Linux preflight: reject malformed Qwen spatial plans BEFORE using Blender/Unity.
export function preflightArtistPlan(ops,target,mode='existing'){
 const errors=[],bad=x=>errors.push(x);
 const number=(v,min,max)=>typeof v==='number'&&Number.isFinite(v)&&v>=min&&v<=max;
 const vec=(v,size=3,min=-10000,max=10000)=>Array.isArray(v)&&v.length===size&&v.every(x=>number(x,min,max));
 if(!['existing','new'].includes(mode))bad('Unknown target mode');
 if(!nonempty(target))bad('Blender target name required');
 if(!Array.isArray(ops)||ops.length<1||ops.length>8)return ['Plan must have 1..8 actions',...errors];
 const ids=new Set();
 for(const [index,op] of ops.entries()){
  if(!op||typeof op!=='object'||Array.isArray(op)){bad('Action must be object');continue;}
  const p=op.parameters;
  if(!nonempty(op.id)||ids.has(op.id))bad('Duplicate/blank action id');
  ids.add(op.id);
  if(op.target!==target)bad('Action targets a different Blender object');
  if(!ACTIONS.has(op.type))bad('Unsupported action '+op.type);
  if(!nonempty(op.rationale)||op.rationale.length<15)bad('Missing artist rationale');
  if(!p||typeof p!=='object'||Array.isArray(p)){bad('Missing action parameters');continue;}
  if(op.type==='construct_mesh'){
   if(mode!=='new'||index!==0)bad('New mesh must be first action in NEW mode');
   const vertices=p.vertices,faces=p.faces;
   if(!Array.isArray(vertices)||vertices.length<4||vertices.length>256||!vertices.every(v=>vec(v)))bad('Invalid mesh vertices');
   if(!Array.isArray(faces)||faces.length<3||faces.length>512||
     !faces.every(face=>Array.isArray(face)&&face.length>=3&&face.length<=12&&face.every(i=>Number.isInteger(i)&&i>=0&&i<(vertices?.length||0))))bad('Invalid mesh faces');
  }
  if(op.type==='sculpt_region'&&(!vec(p.center)||!vec(p.delta)||!number(p.radius,.01,100)||
    p.delta.every(x=>x===0)))bad('Invalid sculpt region');
  if(op.type==='sculpt_path'){
   if(!Array.isArray(p.knots)||p.knots.length<2||p.knots.length>24||
    !p.knots.every(k=>k&&vec(k.center)&&vec(k.delta)&&number(k.radius,.05,60) &&
      Math.hypot(...k.delta)>=.005&&Math.hypot(...k.delta)<=20))bad('Invalid artist sculpt path');
  }
  if(op.type==='bridge_boundary_loops'){
   const a=p.loop_a,b=p.loop_b;
   if(!Array.isArray(a)||!Array.isArray(b)||a.length<3||a.length>48||a.length!==b.length||
      ![...a,...b].every(i=>Number.isInteger(i)&&i>=0)||new Set([...a,...b]).size!==a.length+b.length)
      bad('Invalid artist boundary loops');
  }
  if(op.type==='reprofile_crest'){
   if(!['x','y'].includes(p.axis)||!['y','z'].includes(p.height_axis)||p.axis===p.height_axis||
    !Array.isArray(p.control_points)||p.control_points.length<3||p.control_points.length>24||
    !p.control_points.every(x=>vec(x,2)))bad('Invalid artist crest reprofiling');
  }
  if(op.type==='assign_material'&&(!vec(p.rgb,3,0,1)||!number(p.roughness??.85,0,1)))bad('Invalid material settings');
  if(op.type==='place_existing_asset'&&(!nonempty(p.source_object)||!vec(p.location)||!vec(p.rotation)||!vec(p.scale)||
    p.scale.some(x=>Math.abs(x)<.0001)))bad('Invalid existing asset placement');
 }
 if(mode==='new'&&ops[0]?.type!=='construct_mesh')bad('NEW mode requires authored mesh construction');
 if(mode==='existing'&&ops.some(op=>op?.type==='construct_mesh'))bad('Existing source cannot be replaced by new mesh');
 return errors;
}
export function verifyCycle(c,root=process.cwd()){
 const errors=[],need=(ok,msg)=>{if(!ok)errors.push(msg)};
 const file=(entry,label)=>{
  need(entry&&nonempty(entry.path)&&SHA.test(entry.sha256||''),label+': path and SHA256 required');
  if(!entry?.path||!SHA.test(entry.sha256||''))return;
  const resolved=path.resolve(root,entry.path),prefix=path.resolve(root)+path.sep;
  if(!resolved.startsWith(prefix)){errors.push(label+': file outside repository');return;}
  if(!fs.existsSync(resolved)){errors.push(label+': missing file '+entry.path);return;}
  const realRoot=fs.realpathSync(root),realFile=fs.realpathSync(resolved);
  if(!realFile.startsWith(realRoot+path.sep)){errors.push(label+': symlink escapes repository');return;}
  if(!fs.statSync(realFile).isFile()){errors.push(label+': not a regular evidence file');return;}
  const bytes=fs.readFileSync(realFile);
  // Hashes alone cannot reject text files masquerading as Unity frames or 3D models.
  if(['owner_reference','unity_before','unity_after'].includes(label)){
    const png=bytes.length>=24&&bytes.subarray(0,8).equals(Buffer.from('89504e470d0a1a0a','hex'));
    const jpeg=bytes.length>=10&&bytes[0]===255&&bytes[1]===216&&bytes[bytes.length-2]===255&&bytes[bytes.length-1]===217;
    need(png||jpeg,label+': PNG/JPEG image signature required');
  }
  if(['source_blend','output_blend'].includes(label))
    need(bytes.length>=12&&bytes.subarray(0,7).toString()==='BLENDER',label+': Blender file signature required');
  if(label==='output_glb')
    need(bytes.length>=20&&bytes.subarray(0,4).toString()==='glTF'&&bytes.readUInt32LE(4)===2&&bytes.readUInt32LE(8)===bytes.length,label+': GLB v2 header/length required');
  need(crypto.createHash('sha256').update(bytes).digest('hex')===entry.sha256.toLowerCase(),label+': hash mismatch');
 };
 need(c?.schema_version===1,'schema_version=1 required');
 need(nonempty(c?.asset_id)&&nonempty(c?.workstream_id),'asset/workstream identity required');
 need(nonempty(c?.state),'cycle state required');
 const NON_APPROVED=new Set(['REQUESTED','AUTHORING','TECH_VALIDATED','VISUAL_REJECTED','PROTOTYPE_NO_AI_AUTHORSHIP']);
 need(APPROVED.has(c?.state)||NON_APPROVED.has(c?.state),'unknown artist cycle state');
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
  artist_plan:c?.authorship?.plan_file,
  blender_execution:c?.authorship?.execution_log,unity_after:c?.unity?.after,unity_import:c?.unity?.import_log
 }))file(item,name);
 const parseProof=(entry,label)=>{
  if(!entry?.path||!SHA.test(entry.sha256||''))return null;
  const full=path.resolve(root,entry.path),base=fs.realpathSync(root)+path.sep;
  if(!full.startsWith(path.resolve(root)+path.sep)||!fs.existsSync(full)||!fs.realpathSync(full).startsWith(base))return null;
  try {return JSON.parse(fs.readFileSync(full,'utf8'));}
  catch {need(false,label+': structured JSON evidence required');return null;}
 };
 const inference=parseProof(c?.artist?.inference_log,'artist_inference');
 const plan=parseProof(c?.authorship?.plan_file,'artist_plan');
 const execution=parseProof(c?.authorship?.execution_log,'blender_execution');
 if(inference){
  need(inference.system_role==='system'&&inference.model===c?.artist?.model,'artist inference role/model mismatch');
  need(inference.system_prompt_sha256===c?.artist?.system_prompt?.sha256,'artist inference profile hash mismatch');
  need(inference.reference_sha256===c?.reference?.sha256&&inference.unity_before_sha256===c?.unity?.before?.sha256,'artist inference input images mismatch');
  need(JSON.stringify(inference.direction?.planned_actions)===JSON.stringify(c?.authorship?.planned_actions),'recorded Qwen decisions mismatch planned actions');
 }
 if(plan)need(JSON.stringify(plan)===JSON.stringify(c?.authorship?.planned_actions),'saved pre-execution plan mismatch');
 if(execution){
  need(execution.plan_sha256===c?.authorship?.plan_file?.sha256,'Blender did not execute exact pinned pre-edit plan');
  need(execution.source_sha256===c?.authorship?.source_blend?.sha256,'Blender source bytes mismatch');
  need(execution.result_blend_sha256===c?.authorship?.result_blend?.sha256&&execution.result_glb_sha256===c?.authorship?.result_glb?.sha256,'Blender outputs differ from executor manifest');
  need(execution.source_mesh_sha256===c?.authorship?.source_mesh_sha256&&execution.result_mesh_sha256===c?.authorship?.result_mesh_sha256,'mesh provenance mismatches Blender execution');
  need(JSON.stringify(execution.executed_actions)===JSON.stringify(c?.authorship?.executed_actions),'artist operations mismatch actual Blender execution');
 }
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
 const plannedById=new Map((planned||[]).filter(x=>nonempty(x.id)).map(x=>[x.id,x]));
 const done=new Set();
 for(const op of executed||[]){
  need(ids.has(op.id)&&!done.has(op.id),'executed action not in prior decision/duplicate');done.add(op.id);
  const selected=plannedById.get(op.id);
  need(!selected||(op.target===selected.target&&op.type===selected.type),'executed action type/target must match prior artist decision');
  need(op.applied===true&&nonempty(op.target)&&(Number(op.changed_vertices||0)>0||Number(op.changed_faces||0)>0||Number(op.changed_objects||0)>0),'measurable executed change required');
  if(selected&&GEOMETRY.has(selected.type))need(Number(op.changed_vertices||0)>0||Number(op.changed_faces||0)>0,'structural artist action must change vertices/faces');
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
 if(process.argv[2]==='--preflight-plan'){
  try{
   const ops=JSON.parse(fs.readFileSync(process.argv[3],'utf8'));
   const errors=preflightArtistPlan(ops,process.argv[4],process.argv[5]||'existing');
   if(errors.length){errors.forEach(e=>console.error('ART_PLAN_PREFLIGHT_FAIL: '+e));process.exit(1);}
   console.log('ART_PLAN_PREFLIGHT_PASS');process.exit(0);
  }catch(e){console.error('ART_PLAN_PREFLIGHT_FAIL: '+e.message);process.exit(2);}
 }
 const file=process.argv[2];
 if(!file){console.error('Usage: node verify-artist-cycle.mjs artist-cycle.json [repo-root]');process.exit(2)}
 try{
  const c=JSON.parse(fs.readFileSync(file,'utf8')),problems=verifyCycle(c,process.argv[3]||process.cwd());
  if(problems.length){problems.forEach(p=>console.error('ART_CYCLE_FAIL: '+p));process.exit(1)}
  console.log(APPROVED.has(c.state)?'ART_CYCLE_EVIDENCE_PASS (visual merit remains independent)':'ART_CYCLE_WIP_NO_APPROVAL');
 }catch(e){console.error('ART_CYCLE_FAIL: '+e.message);process.exit(2)}
}
