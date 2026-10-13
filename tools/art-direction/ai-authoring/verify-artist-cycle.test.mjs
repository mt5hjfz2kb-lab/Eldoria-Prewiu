import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import crypto from 'node:crypto';
import zlib from 'node:zlib';
import {measureUnityVisualChange,certifyUnityAB} from './unity_pixel_diff.mjs';
// Real, checksummed 8-bit PNG fixtures. A signature-only imitation MUST NOT pass.
function crc32(bytes){
 let n=0xffffffff;
 for(const b of bytes){n^=b;for(let k=0;k<8;k++)n=(n>>>1)^((n&1)?0xedb88320:0);}
 return(n^0xffffffff)>>>0;
}
function pngChunk(type,data){
 const name=Buffer.from(type,'ascii'),b=Buffer.alloc(12+data.length);
 b.writeUInt32BE(data.length,0);name.copy(b,4);data.copy(b,8);
 b.writeUInt32BE(crc32(b.subarray(4,8+data.length)),8+data.length);
 return b;
}
function validPng(mode='baseline',width=40,height=30){
 const header=Buffer.alloc(13);header.writeUInt32BE(width,0);header.writeUInt32BE(height,4);
 header[8]=8;header[9]=2; // PNG 8-bit RGB with filter=0, no interlace
 const scan=Buffer.alloc((width*3+1)*height);
 for(let y=0;y<height;y++)for(let x=0;x<width;x++){
  const pos=y*(width*3+1)+1+x*3;
  const changed=mode==='visible'?(x<9&&y<9):mode==='tiny'?(x===0&&y===0):false;
  scan.fill(changed?220:80,pos,pos+3);
 }
 return Buffer.concat([Buffer.from('89504e470d0a1a0a','hex'),
  pngChunk('IHDR',header),pngChunk('IDAT',zlib.deflateSync(scan)),pngChunk('IEND',Buffer.alloc(0))]);
}

import {verifyCycle,preflightArtistPlan} from './verify-artist-cycle.mjs';
function fixture(){
 const root=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-authoring-')),proof=p=>{
  const rel='proof/'+p+'.txt',full=path.join(root,rel);
  fs.mkdirSync(path.dirname(full),{recursive:true});
  let bytes=Buffer.from('synthetic fixture for '+p);
  if(['target','before','after'].includes(p))bytes=validPng(p==='after'?'visible':'baseline');
  if(['source','blend'].includes(p))bytes=Buffer.concat([Buffer.from('BLENDER-v300'),Buffer.alloc(20)]);
  if(p==='glb'){bytes=Buffer.alloc(20);bytes.write('glTF',0);bytes.writeUInt32LE(2,4);bytes.writeUInt32LE(20,8);bytes.writeUInt32LE(4,12);bytes.write('JSON',16);}
  fs.writeFileSync(full,bytes);
  return {path:rel,sha256:crypto.createHash('sha256').update(bytes).digest('hex')};
 };
 const c={schema_version:1,asset_id:'cliff-fix',workstream_id:'valoria',state:'ART_APPROVED',geometry_gap_proven:true,
  artist:{model:'qwen3-vl:4b-instruct',system_role:'system',observed_defect:'A rectangular cliff is interrupting organic landscape silhouettes.',design_intent:'Replace the slab with a coherent artist sculpted cliff bank and terrain joins.',asset_decision:'MODIFY',reason_for_new_geometry:'Old plateau end is structurally rectangular',system_prompt:proof('prompt'),inference_log:proof('inference')},
  reference:proof('target'),unity:{before:proof('before'),after:proof('after'),import_log:proof('unity-log'),same_camera:true,camera_matrix_sha256:'3'.repeat(64)},
  authorship:{mode:'AI_DIRECTED_BLENDER',source_blend:proof('source'),result_blend:proof('blend'),result_glb:proof('glb'),execution_log:proof('execution'),source_mesh_sha256:'1'.repeat(64),result_mesh_sha256:'2'.repeat(64),
   planned_actions:[{id:'rock1',type:'sculpt_region',target:'rock',rationale:'Deeply reshape large cliff silhouette',parameters:{center:[0,0,0],radius:5,delta:[0,0,2]}}],
   executed_actions:[{id:'rock1',type:'sculpt_region',applied:true,target:'rock',changed_vertices:200}]},
  reviews:{technical:{verdict:'PASS'},visual:{verdict:'PASS',independent:true,visible_difference:'Eliminated the hard rectangular front in the genuine Unity scene.'}},
  license:{provenance:'Original modelling in Blender using artist plan'}
 }; const writeLog=(name,data)=>{
  const rel='proof/'+name+'.json',full=path.join(root,rel),bytes=Buffer.from(JSON.stringify(data,null,2));
  fs.writeFileSync(full,bytes);
  return {path:rel,sha256:crypto.createHash('sha256').update(bytes).digest('hex')};
 };
 const syncLogs=()=>{
  c.authorship.plan_file=writeLog('plan',c.authorship.planned_actions);
  c.artist.inference_log=writeLog('inference',{
   system_role:'system',model:c.artist.model,system_prompt_sha256:c.artist.system_prompt.sha256,
   reference_sha256:c.reference.sha256,unity_before_sha256:c.unity.before.sha256,
   direction:{planned_actions:c.authorship.planned_actions}
  });
  c.authorship.execution_log=writeLog('execution',{
   plan_sha256:c.authorship.plan_file.sha256,
   source_sha256:c.authorship.source_blend.sha256,
   result_blend_sha256:c.authorship.result_blend.sha256,
   result_glb_sha256:c.authorship.result_glb.sha256,
   source_mesh_sha256:c.authorship.source_mesh_sha256,
   result_mesh_sha256:c.authorship.result_mesh_sha256,
   executed_actions:c.authorship.executed_actions
  });
 };
 syncLogs();return{root,c,syncLogs};
}
test('Complete traceable artist-driven cycle passes evidence gate',()=>{const {root,c}=fixture();assert.deepEqual(verifyCycle(c,root),[])})
test('Unity screenshot original and Qwen candidate with imperceptible difference are rejected even with matching SHA256s and review PASS',()=>{
 const {root,c}=fixture();
 const newBytes=validPng('tiny'),full=path.join(root,c.unity.after.path);
 fs.writeFileSync(full,newBytes);c.unity.after.sha256=crypto.createHash('sha256').update(newBytes).digest('hex');
 assert.ok(verifyCycle(c,root).some(e=>e.includes('UNITY_AB_VISUAL_REJECTED')));
});
test('Identical but differently named real Unity frames cannot justify ART_APPROVED',()=>{
 const {root,c}=fixture();
 const original=fs.readFileSync(path.join(root,c.unity.before.path));
 fs.writeFileSync(path.join(root,c.unity.after.path),original);
 c.unity.after.sha256=crypto.createHash('sha256').update(original).digest('hex');
 assert.ok(verifyCycle(c,root).some(e=>e.includes('UNITY_AB_VISUAL_REJECTED')));
});
test('Unity A/B pixel classifier rejects material differences below the camera threshold',()=>{
 const a=validPng('baseline'),b=validPng('tiny');
 assert.equal(measureUnityVisualChange(a,b).changed_pixels,1);
 assert.equal(measureUnityVisualChange(a,b).gate,'VISUAL_REJECTED');
 assert.throws(()=>certifyUnityAB(a,b),/UNITY_AB_VISUAL_REJECTED/);
 assert.equal(certifyUnityAB(a,validPng('visible')).gate,'INDEPENDENT_REVIEW_REQUIRED');
});
test('Forged image headers, size differences and damaged CRCs cannot bypass Unity visual gate',()=>{
 const a=validPng('baseline');
 assert.throws(()=>measureUnityVisualChange(a,Buffer.concat([Buffer.from('89504e470d0a1a0a','hex'),Buffer.alloc(24)])),/genuine PNG/);
 assert.throws(()=>measureUnityVisualChange(a,validPng('visible',41,30)),/identical image dimensions/);
 const b=validPng('visible');b[b.length-16]^=1;
 assert.throws(()=>measureUnityVisualChange(a,b),/checksum invalid/);
});
;
test('A procedural candidate is not visually approved',()=>{const {root,c}=fixture();c.state='PROTOTYPE_NO_AI_AUTHORSHIP';c.artist=null;assert.deepEqual(verifyCycle(c,root),[])});
test('Qwen parameter-only changes cannot certify modelling',()=>{const {root,c}=fixture();c.authorship.planned_actions[0].type='noise_amplitude';assert.ok(verifyCycle(c,root).some(x=>x.includes('preset-only')))});
test('MCP execution missing does not count as actual authoring',()=>{const {root,c}=fixture();c.authorship.executed_actions=[];assert.ok(verifyCycle(c,root).some(x=>x.includes('executed actions')))});
test('Artist system profile must have been transmitted as system role',()=>{const {root,c}=fixture();c.artist.system_role='user';assert.ok(verifyCycle(c,root).some(x=>x.includes('system role')))});
test('Tampered Unity capture fails file hash gate',()=>{const {root,c}=fixture();fs.writeFileSync(path.join(root,c.unity.after.path),'tampered');assert.ok(verifyCycle(c,root).some(x=>x.includes('hash mismatch')))});
test('Independent Unity visual gate cannot be self-approved',()=>{const {root,c}=fixture();c.reviews.visual.independent=false;assert.ok(verifyCycle(c,root).some(x=>x.includes('independent visual')))});
test('Material-only cycle can preserve mesh if material appearance evidence changed',()=>{const {root,c,syncLogs}=fixture();c.geometry_gap_proven=false;c.authorship.result_mesh_sha256=c.authorship.source_mesh_sha256;c.authorship.source_appearance_sha256='a'.repeat(64);c.authorship.result_appearance_sha256='b'.repeat(64);c.authorship.planned_actions[0].type='assign_material';c.authorship.executed_actions[0].type='assign_material';c.authorship.executed_actions[0].changed_vertices=0;c.authorship.executed_actions[0].changed_objects=1;syncLogs();assert.deepEqual(verifyCycle(c,root),[])});
test('Release gate refuses no real mobile QA',()=>{const {root,c}=fixture();c.state='RELEASED';assert.ok(verifyCycle(c,root).some(x=>x.includes('physical-device')))});

test('Reject an operation whose executed type differs from the model design',()=>{const {root,c}=fixture();c.authorship.executed_actions[0].type='assign_material';assert.ok(verifyCycle(c,root).some(x=>x.includes('type/target')))});
test('Reject fake Unity frame with matching hash but invalid image bytes',()=>{const {root,c}=fixture();const bytes=Buffer.from('This is not a Unity image');fs.writeFileSync(path.join(root,c.unity.after.path),bytes);c.unity.after.sha256=crypto.createHash('sha256').update(bytes).digest('hex');assert.ok(verifyCycle(c,root).some(x=>x.includes('image signature')))});
test('Reject a symlink to evidence outside the project',()=>{const {root,c}=fixture();const out=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-external-'));const dest=path.join(out,'image.png');fs.copyFileSync(path.join(root,c.reference.path),dest);const link=path.join(root,'proof','linked.png');fs.symlinkSync(dest,link);c.reference.path='proof/linked.png';assert.ok(verifyCycle(c,root).some(x=>x.includes('symlink escapes')))});
test('Reject a sculpt operation claimed without any changed vertices',()=>{const {root,c}=fixture();c.authorship.executed_actions[0].changed_vertices=0;c.authorship.executed_actions[0].changed_objects=1;assert.ok(verifyCycle(c,root).some(x=>x.includes('must change vertices')))});
test('Reject a supposedly real Blender source that is only text',()=>{const {root,c}=fixture();const bytes=Buffer.from('fake blend model');fs.writeFileSync(path.join(root,c.authorship.source_blend.path),bytes);c.authorship.source_blend.sha256=crypto.createHash('sha256').update(bytes).digest('hex');assert.ok(verifyCycle(c,root).some(x=>x.includes('Blender file signature')))});

test('Artist-directed sculpt path can prove structural mesh changes',()=>{const {root,c,syncLogs}=fixture();c.authorship.planned_actions[0].type='sculpt_path';c.authorship.planned_actions[0].parameters={knots:[{center:[0,0,0],delta:[0,1,1],radius:5},{center:[6,0,3],delta:[0,0,2],radius:7}]};c.authorship.executed_actions[0].type='sculpt_path';syncLogs();assert.deepEqual(verifyCycle(c,root),[])});
test('Boundary bridge with actual faces can prove topological change',()=>{const {root,c,syncLogs}=fixture();c.authorship.planned_actions[0].type='bridge_boundary_loops';c.authorship.planned_actions[0].parameters={loop_a:[1,2,3],loop_b:[8,9,10]};c.authorship.executed_actions[0].type='bridge_boundary_loops';c.authorship.executed_actions[0].changed_vertices=0;c.authorship.executed_actions[0].changed_faces=3;syncLogs();assert.deepEqual(verifyCycle(c,root),[])});
test('Topology bridge with no changed faces or vertices fails',()=>{const {root,c,syncLogs}=fixture();c.authorship.planned_actions[0].type='bridge_boundary_loops';c.authorship.executed_actions[0].type='bridge_boundary_loops';c.authorship.executed_actions[0].changed_vertices=0;c.authorship.executed_actions[0].changed_faces=0;syncLogs();assert.ok(verifyCycle(c,root).some(e=>e.includes('must change vertices/faces')))});
test('Replaced Qwen inference plan fails even with internally consistent cycle metadata',()=>{const {root,c}=fixture();c.authorship.planned_actions[0].rationale='A different later script decision, not from the actual model';assert.ok(verifyCycle(c,root).some(e=>e.includes('recorded Qwen decisions mismatch')))});
test('A purported Blender export inconsistent with execution log fails',()=>{const {root,c}=fixture();c.authorship.result_mesh_sha256='c'.repeat(64);assert.ok(verifyCycle(c,root).some(e=>e.includes('mesh provenance mismatches')))});
test('Executable action not selected by approved allowlist fails',()=>{const {root,c,syncLogs}=fixture();c.authorship.planned_actions[0].type='mesh_cut';c.authorship.executed_actions[0].type='mesh_cut';syncLogs();assert.ok(verifyCycle(c,root).some(e=>e.includes('unsupported/preset-only')))});

test('Preflight accepts a model-authored organic terrain path',()=>{const ops=[{id:'arc1',type:'sculpt_path',target:'Terrain',rationale:'Replace squared cliff shape with long curving geometry',parameters:{knots:[{center:[0,1,1],delta:[.3,0,1],radius:5},{center:[3,2,1],delta:[.6,0,2],radius:6}]}}];assert.deepEqual(preflightArtistPlan(ops,'Terrain','existing'),[])});
test('Preflight rejects unsafe generated geometry at no Blender cost',()=>{const ops=[{id:'bad',type:'sculpt_path',target:'Terrain',rationale:'Move the entire cliff using infinite displacement',parameters:{knots:[{center:[0,0,0],delta:[Infinity,0,0],radius:9},{center:[1,0,0],delta:[0,1,0],radius:9}]}}];assert.ok(preflightArtistPlan(ops,'Terrain','existing').some(s=>s.includes('Invalid artist sculpt path')))});
test('Preflight requires validated distinct boundary indices',()=>{const ops=[{id:'bad-seam',type:'bridge_boundary_loops',target:'Terrain',rationale:'Bridge two distinct cliff segments without their actual borders',parameters:{loop_a:[1,2,3],loop_b:[2,9,10]}}];assert.ok(preflightArtistPlan(ops,'Terrain','existing').some(s=>s.includes('boundary loops')))});
test('Preflight prohibits duplicate meshes in existing-object mode',()=>{const ops=[{id:'dup',type:'construct_mesh',target:'Terrain',rationale:'Duplicate the mountain instead of modifying the original',parameters:{vertices:[[0,0,0],[1,0,0],[0,1,0],[0,0,1]],faces:[[0,1,2],[0,1,3],[0,2,3]]}}];assert.ok(preflightArtistPlan(ops,'Terrain','existing').some(s=>s.includes('Existing source cannot')))});
