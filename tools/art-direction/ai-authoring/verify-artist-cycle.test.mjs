import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import crypto from 'node:crypto';
import {verifyCycle} from './verify-artist-cycle.mjs';
function fixture(){
 const root=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-authoring-')),proof=p=>{
  const rel='proof/'+p+'.txt',full=path.join(root,rel);
  fs.mkdirSync(path.dirname(full),{recursive:true});
  let bytes=Buffer.from('synthetic fixture for '+p);
  if(['target','before','after'].includes(p))bytes=Buffer.concat([Buffer.from('89504e470d0a1a0a','hex'),Buffer.alloc(24,0)]);
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
 };return{root,c};
}
test('Complete traceable artist-driven cycle passes evidence gate',()=>{const {root,c}=fixture();assert.deepEqual(verifyCycle(c,root),[])});
test('A procedural candidate is not visually approved',()=>{const {root,c}=fixture();c.state='PROTOTYPE_NO_AI_AUTHORSHIP';c.artist=null;assert.deepEqual(verifyCycle(c,root),[])});
test('Qwen parameter-only changes cannot certify modelling',()=>{const {root,c}=fixture();c.authorship.planned_actions[0].type='noise_amplitude';assert.ok(verifyCycle(c,root).some(x=>x.includes('preset-only')))});
test('MCP execution missing does not count as actual authoring',()=>{const {root,c}=fixture();c.authorship.executed_actions=[];assert.ok(verifyCycle(c,root).some(x=>x.includes('executed actions')))});
test('Artist system profile must have been transmitted as system role',()=>{const {root,c}=fixture();c.artist.system_role='user';assert.ok(verifyCycle(c,root).some(x=>x.includes('system role')))});
test('Tampered Unity capture fails file hash gate',()=>{const {root,c}=fixture();fs.writeFileSync(path.join(root,c.unity.after.path),'tampered');assert.ok(verifyCycle(c,root).some(x=>x.includes('hash mismatch')))});
test('Independent Unity visual gate cannot be self-approved',()=>{const {root,c}=fixture();c.reviews.visual.independent=false;assert.ok(verifyCycle(c,root).some(x=>x.includes('independent visual')))});
test('Material-only cycle can preserve mesh if material appearance evidence changed',()=>{const {root,c}=fixture();c.geometry_gap_proven=false;c.authorship.result_mesh_sha256=c.authorship.source_mesh_sha256;c.authorship.source_appearance_sha256='a'.repeat(64);c.authorship.result_appearance_sha256='b'.repeat(64);c.authorship.planned_actions[0].type='assign_material';c.authorship.executed_actions[0].type='assign_material';c.authorship.executed_actions[0].changed_vertices=0;c.authorship.executed_actions[0].changed_objects=1;assert.deepEqual(verifyCycle(c,root),[])});
test('Release gate refuses no real mobile QA',()=>{const {root,c}=fixture();c.state='RELEASED';assert.ok(verifyCycle(c,root).some(x=>x.includes('physical-device')))});

test('Reject an operation whose executed type differs from the model design',()=>{const {root,c}=fixture();c.authorship.executed_actions[0].type='assign_material';assert.ok(verifyCycle(c,root).some(x=>x.includes('type/target')))});
test('Reject fake Unity frame with matching hash but invalid image bytes',()=>{const {root,c}=fixture();const bytes=Buffer.from('This is not a Unity image');fs.writeFileSync(path.join(root,c.unity.after.path),bytes);c.unity.after.sha256=crypto.createHash('sha256').update(bytes).digest('hex');assert.ok(verifyCycle(c,root).some(x=>x.includes('image signature')))});
test('Reject a symlink to evidence outside the project',()=>{const {root,c}=fixture();const out=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-external-'));const dest=path.join(out,'image.png');fs.copyFileSync(path.join(root,c.reference.path),dest);const link=path.join(root,'proof','linked.png');fs.symlinkSync(dest,link);c.reference.path='proof/linked.png';assert.ok(verifyCycle(c,root).some(x=>x.includes('symlink escapes')))});
test('Reject a sculpt operation claimed without any changed vertices',()=>{const {root,c}=fixture();c.authorship.executed_actions[0].changed_vertices=0;c.authorship.executed_actions[0].changed_objects=1;assert.ok(verifyCycle(c,root).some(x=>x.includes('must change vertices')))});
test('Reject a supposedly real Blender source that is only text',()=>{const {root,c}=fixture();const bytes=Buffer.from('fake blend model');fs.writeFileSync(path.join(root,c.authorship.source_blend.path),bytes);c.authorship.source_blend.sha256=crypto.createHash('sha256').update(bytes).digest('hex');assert.ok(verifyCycle(c,root).some(x=>x.includes('Blender file signature')))});
