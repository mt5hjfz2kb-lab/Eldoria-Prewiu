import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import zlib from 'node:zlib';
import crypto from 'node:crypto';
import {fileURLToPath} from 'node:url';
import {stageBlender} from './stage-blender.mjs';
import {validate} from './learning.mjs';

const here=path.dirname(fileURLToPath(import.meta.url));
const db=path.join(here,'observations.json');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
function crc(b){let c=0xffffffff;for(const v of b){c^=v;for(let i=0;i<8;i++)c=(c>>>1)^((c&1)?0xedb88320:0);}return (c^0xffffffff)>>>0;}
function chunk(t,b){const n=Buffer.from(t),l=Buffer.alloc(4),c=Buffer.alloc(4);l.writeUInt32BE(b.length);c.writeUInt32BE(crc(Buffer.concat([n,b])));return Buffer.concat([l,n,b,c]);}
function png(changed){
 const head=Buffer.alloc(13);head.writeUInt32BE(720,0);head.writeUInt32BE(500,4);head[8]=8;head[9]=2;
 const raw=Buffer.alloc(500*(1+720*3));if(changed)raw[1]=255;
 return Buffer.concat([Buffer.from('89504e470d0a1a0a','hex'),chunk('IHDR',head),
  chunk('IDAT',zlib.deflateSync(raw)),chunk('IEND',Buffer.alloc(0))]);
}
function fixture(){
 const dir=fs.mkdtempSync(path.join(os.tmpdir(),'bastion-learning-'));
 for(const folder of ['ab','result','model'])fs.mkdirSync(path.join(dir,folder));
 const before=png(false),after=png(true),source=Buffer.from('BLENDER-frozen-source'),target=Buffer.from('BLENDER-result-source');
 const write=(p,b)=>fs.writeFileSync(path.join(dir,p),b);
 write('ab/before-blender-clay.png',before);write('ab/after-blender-clay.png',after);
 write('terrain-source.blend',source);write('result/artist-output.blend',target);
 const plan=[{id:'original-model-action',type:'sculpt_region',target:'Valoria natural playable hilltop and rock scarps',
   parameters:{center:[1,1,1],radius:10,delta:[1,0,0]},rationale:'Source example only, not real 3D scene evidence'}];
 write('model/artist-plan.json',JSON.stringify(plan));
 write('model/artist-inference.json',JSON.stringify({system_role:'system',model:'qwen3-vl:8b-instruct',direction:{planned_actions:plan}}));
 const comparison={scope:'Blender clay A/B only, NOT Unity, NOT ART_APPROVED',
   camera:'verified Bastion I player-equivalent orthographic camera',
   player_frustum:{sensor_fit:'VERTICAL',pixels:[720,500]},render_preset:'matched controlled clay',
   target:'Valoria natural playable hilltop and rock scarps',
   source_file_sha256:sha(source),result_file_sha256:sha(target),
   before_image_sha256:sha(before),after_image_sha256:sha(after),
   camera_visibility:{changed_pixels:1,camera_visibility_gate:'REJECT_NOT_VISIBLE'}};
 write('ab/comparison.json',JSON.stringify(comparison));
 return {dir,comparison};
}
test('Blender-only Qwen records stage pending with authentic frame remeasurement',()=>{
 const {dir}=fixture(),out=path.join(dir,'staged.json');
 try{
  const v=stageBlender({dbPath:db,root:dir,runId:39999000000,gitSha:'a'.repeat(40),out});
  assert.equal(v.event.art_approved,false);
  assert.equal(v.event.art_verdict,'ART_UNREVIEWED');
  assert.equal(v.event.source.evidence_level,'pending_verification');
  assert.equal(v.event.visibility_gate,'REJECT_NOT_VISIBLE');
  assert.equal(v.event.measurement.changed_pixels,1);
  assert.deepEqual(validate(v.candidate),[]);
  assert.equal(v.candidate.cases.length,7);
  assert.equal(JSON.parse(fs.readFileSync(db,'utf8')).cases.length,6);
  assert.throws(()=>stageBlender({dbPath:db,root:dir,runId:39999000000,gitSha:'a'.repeat(40),out}),/overwrite/);
 }finally{fs.rmSync(dir,{recursive:true,force:true})}
});
test('stage refuses mismatched PNG and camera preset (no false source evidence)',()=>{
 const {dir,comparison}=fixture(),out=path.join(dir,'staged.json');
 try{
  fs.writeFileSync(path.join(dir,'ab','after-blender-clay.png'),png(false));
  assert.throws(()=>stageBlender({dbPath:db,root:dir,runId:39999000001,gitSha:'a'.repeat(40),out}),/frame hashes/);
  fs.writeFileSync(path.join(dir,'ab','after-blender-clay.png'),png(true));
  comparison.camera='free camera';fs.writeFileSync(path.join(dir,'ab','comparison.json'),JSON.stringify(comparison));
  assert.throws(()=>stageBlender({dbPath:db,root:dir,runId:39999000001,gitSha:'a'.repeat(40),out}),/Unverified camera/);
  assert.equal(fs.existsSync(out),false);
 }finally{fs.rmSync(dir,{recursive:true,force:true})}
});
