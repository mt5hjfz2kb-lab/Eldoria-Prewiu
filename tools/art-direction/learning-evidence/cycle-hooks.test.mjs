import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import zlib from 'node:zlib';
import {fileURLToPath} from 'node:url';
import {prepare,stage} from './cycle-hooks.mjs';
import {validate} from './learning.mjs';

const here=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(here,'../../..');
const db=path.join(here,'observations.json');
const reference=path.join(root,'references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg');
const digest='a'.repeat(40);
const camera='c'.repeat(64);

function crc(data){
 let c=0xffffffff;
 for(const b of data){c^=b;for(let i=0;i<8;i++)c=(c>>>1)^((c&1)?0xedb88320:0);}
 return (c^0xffffffff)>>>0;
}
function chunk(type,data){
 const name=Buffer.from(type),len=Buffer.alloc(4),check=Buffer.alloc(4);
 len.writeUInt32BE(data.length);
 check.writeUInt32BE(crc(Buffer.concat([name,data])));
 return Buffer.concat([len,name,data,check]);
}
function png(changed=false){
 const header=Buffer.alloc(13);
 header.writeUInt32BE(8,0);header.writeUInt32BE(8,4);header[8]=8;header[9]=2;
 const raw=Buffer.alloc(8*(1+8*3));
 if(changed)raw[1]=255;
 return Buffer.concat([Buffer.from('89504e470d0a1a0a','hex'),chunk('IHDR',header),
   chunk('IDAT',zlib.deflateSync(raw)),chunk('IEND',Buffer.alloc(0))]);
}
const work=()=>fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-learning-'));
test('preflight reads all six validated lessons with provenance, without touching source',()=>{
 const dir=work(),out=path.join(dir,'before.txt');
 const first=fs.readFileSync(db);
 const a=prepare({out,maxChars:3000});
 assert.match(a.message,/small-sculpt-repeated/);
 assert.match(a.message,/frustum-parity/);
 assert.equal(a.provenance.case_count,6);
 assert.ok(fs.existsSync(out+'.provenance.json'));
 assert.deepEqual(fs.readFileSync(db),first);
 assert.throws(()=>prepare({out}),/Refuse to overwrite/);
 fs.rmSync(dir,{recursive:true,force:true});
});
test('completed paired PNGs stage pending evidence, not an approved result',()=>{
 const dir=work(),before=path.join(dir,'before.png'),after=path.join(dir,'after.png');
 fs.writeFileSync(before,png());fs.writeFileSync(after,png(true));
 const out=path.join(dir,'staged.json'),options={out,runId:39000000000,gitSha:digest,reference,
   before,after,cameraSha:camera,diagnosis:'Visible trial has an insufficient cliff silhouette.',
   lesson:'Choose a camera-visible redesign rather than repeating invisible edits.',
   nextAction:'Inspect matched genuine Unity view before any independent approval.'};
 const staged=stage(options);
 const result=JSON.parse(fs.readFileSync(out,'utf8'));
 assert.deepEqual(validate(result),[]);
 assert.equal(staged.event.source.evidence_level,'pending_verification');
 assert.equal(staged.event.art_approved,false);
 assert.equal(staged.event.art_verdict,'ART_UNREVIEWED');
 assert.equal(staged.measurement.changed_pixels,1);
 assert.equal(result.cases.length,7);
 assert.equal(JSON.parse(fs.readFileSync(db,'utf8')).cases.length,6);
 assert.throws(()=>stage(options),/Refuse to overwrite/);
 fs.rmSync(dir,{recursive:true,force:true});
});
test('do not stage fabricated camera, unverified original images or forged reference',()=>{
 const dir=work(),before=path.join(dir,'before.png'),after=path.join(dir,'after.png');
 fs.writeFileSync(before,png());fs.writeFileSync(after,png());
 const basic={out:path.join(dir,'event.json'),runId:39000000001,gitSha:digest,reference,
   before,after,cameraSha:camera,diagnosis:'Correct and grounded architectural concern exists.',
   lesson:'Real improvements require matching the actual camera and geometry.',
   nextAction:'Collect new evidence with the exact approved Unity camera framing.'};
 assert.throws(()=>stage({...basic,cameraSha:'invalid'}),/camera identity/);
 fs.writeFileSync(after,Buffer.from('not a PNG'));
 assert.throws(()=>stage(basic),/genuine PNG/);
 fs.writeFileSync(after,png());
 assert.throws(()=>stage({...basic,reference:after}),/reference bytes\/hash mismatch/);
 assert.equal(fs.existsSync(basic.out),false);
 fs.rmSync(dir,{recursive:true,force:true});
});
test('reject duplicate or mismatched target and never silently overwrite',()=>{
 const dir=work(),out=path.join(dir,'context.txt');
 assert.throws(()=>prepare({target:'../escape',out}),/Invalid target/);
 assert.throws(()=>prepare({out:db}),/Never overwrite/);
 fs.rmSync(dir,{recursive:true,force:true});
});
