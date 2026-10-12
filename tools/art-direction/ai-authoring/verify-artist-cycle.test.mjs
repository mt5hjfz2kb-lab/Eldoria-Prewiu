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
  fs.mkdirSync(path.dirname(full),{recursive:true});const bytes=Buffer.from('evidence for '+p);fs.writeFileSync(full,bytes);
  return {path:rel,sha256:crypto.createHash('sha256').update(bytes).digest('hex')};
 };
 const c={schema_version:1,asset_id:'cliff-fix',workstream_id:'valoria',state:'ART_APPROVED',geometry_gap_proven:true,
  artist:{model:'qwen3-vl:4b-instruct',system_role:'system',observed_defect:'A rectangular cliff is interrupting organic landscape silhouettes.',design_intent:'Replace the slab with a coherent artist sculpted cliff bank and terrain joins.',asset_decision:'MODIFY',reason_for_new_geometry:'Old plateau end is structurally rectangular',system_prompt:proof('prompt'),inference_log:proof('inference')},
  reference:proof('target'),unity:{before:proof('before'),after:proof('after'),import_log:proof('unity-log'),same_camera:true,camera_matrix_sha256:'3'.repeat(64)},
  authorship:{mode:'AI_DIRECTED_BLENDER',source_blend:proof('source'),result_blend:proof('blend'),result_glb:proof('glb'),execution_log:proof('execution'),source_mesh_sha256:'1'.repeat(64),result_mesh_sha256:'2'.repeat(64),
   planned_actions:[{id:'rock1',type:'sculpt_region',target:'rock',rationale:'Deeply reshape large cliff silhouette',parameters:{center:[0,0,0],radius:5,delta:[0,0,2]}}],
   executed_actions:[{id:'rock1',applied:true,target:'rock',changed_vertices:200}]},
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
test('Material-only cycle can preserve mesh if material appearance evidence changed',()=>{const {root,c}=fixture();c.geometry_gap_proven=false;c.authorship.result_mesh_sha256=c.authorship.source_mesh_sha256;c.authorship.source_appearance_sha256='a'.repeat(64);c.authorship.result_appearance_sha256='b'.repeat(64);c.authorship.planned_actions[0].type='assign_material';assert.deepEqual(verifyCycle(c,root),[])});
test('Release gate refuses no real mobile QA',()=>{const {root,c}=fixture();c.state='RELEASED';assert.ok(verifyCycle(c,root).some(x=>x.includes('physical-device')))});
