#!/usr/bin/env node
// Real smoke tests: complete fixture PASS, changed camera FAIL, source drift FAIL, absent record FAIL.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import crypto from 'node:crypto';
import zlib from 'node:zlib';
import {execFileSync,spawnSync} from 'node:child_process';
const verifier=path.resolve('tools/world4x-artistic-process/verify.mjs');
const tmp=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-art-qa-'));
const root=path.join(tmp,'candidate'), E='docs/evidence/world4x-canon-20261022';
function f(rel,bytes){const file=path.join(root,rel);fs.mkdirSync(path.dirname(file),{recursive:true});fs.writeFileSync(file,bytes);return file;}
function sh(v){return crypto.createHash('sha256').update(v).digest('hex');}
function git(...a){return execFileSync('git',['-C',root,...a],{encoding:'utf8'}).trim();}
function crc(b){let c=0xffffffff;for(const byte of b){c^=byte;for(let j=0;j<8;j++)c=(c>>>1)^((c&1)?0xedb88320:0);}return (c^0xffffffff)>>>0;}
function ch(name,data){const n=Buffer.from(name,'ascii'),size=Buffer.alloc(4),sum=Buffer.alloc(4);size.writeUInt32BE(data.length);sum.writeUInt32BE(crc(Buffer.concat([n,data])));return Buffer.concat([size,n,data,sum]);}
function png(rgb){const ih=Buffer.alloc(13);ih.writeUInt32BE(1,0);ih.writeUInt32BE(1,4);ih[8]=8;ih[9]=6;const id=zlib.deflateSync(Buffer.from([0,...rgb,255]));return Buffer.concat([Buffer.from([137,80,78,71,13,10,26,10]),ch('IHDR',ih),ch('IDAT',id),ch('IEND',Buffer.alloc(0))]);}
function run(shouldPass,tag,required){const r=spawnSync(process.execPath,[verifier,root],{encoding:'utf8'});const combined=(r.stdout||'')+(r.stderr||'');if((r.status===0)!==shouldPass||!combined.includes(required))throw Error(tag+': unexpected outcome '+r.status+' '+combined.slice(0,1000));console.log('SMOKE_PASS '+tag);}
try{
  fs.mkdirSync(root,{recursive:true});execFileSync('git',['init',root],{stdio:'ignore'});git('config','user.email','art-qa@example.invalid');git('config','user.name','Art QA Fixture');
  const prefix=E+'/sources/native-';
  const beforeDir=prefix+'100000/review',afterDir=prefix+'200000/review';
  const labels=['cercana-horizontal','cercana-vertical','regional-horizontal','regional-vertical','panoramica-horizontal','panoramica-vertical'];
  for(const label of labels){for(const [dir,rgb] of [[beforeDir,[20,40,60]],[afterDir,[26,46,68]]]){
    const b=png(rgb),name=dir+'/'+label;
    f(name+'-unity.png',b);
    f(name+'-source.json',JSON.stringify({label,actualUnityCamera:true,playMode:true,width:1,height:1,sha256:sh(b),zoom:36,aspect:1,cameraPosition:{x:1,y:2,z:3},cameraRotation:{x:0,y:0,z:0,w:1},worldTarget:{x:0,y:0,z:0}}));
  }}
  const sourceA=Buffer.from('BLENDER-v1 source dummy'),sourceB=Buffer.from('BLENDER-v2 edited dummy');
  const input=E+'/sources/native-100000/artist-source.blend',output=E+'/sources/native-200000/artist-final.blend';
  f(input,sourceA);f(output,sourceB);f(E+'/authoring/model.py','original procedural helper');
  const review=E+'/artistic-review.md';
  f(review,'Observación: el suelo era plano y la silueta lejana pequeña. Decisión: cambiar la composición, no solo el ruido. Corrección: editar en Blender y revisar cámara Unity. Veredicto: conservar mejora, aún quedan defectos en costa y bosques. '.repeat(2));
  git('add','.');git('commit','-m','baseline source');const sourceSHA=git('rev-parse','HEAD');
  const record={
    schema_version:1,workstream_id:'eldoria-world-region-1-canon-20261022',certified_source_commit:sourceSHA,
    blender:{method:'blender-bpy-reviewed',mcp_status:'not-used',mcp_preflight:'Se comprobó la disponibilidad de MCP y no se utilizó durante este ensayo simulado; hay capturas revisadas.',mcp_reason:'MCP no se empleó porque es una prueba automatizada del verificador; se simula Blender bpy con inspección.',artistic_decision_log:review,input_blend:input,input_sha256:sh(sourceA),output_blend:output,output_sha256:sh(sourceB)},
    baseline:{run_id:100000,directory:beforeDir},candidate:{run_id:200000,directory:afterDir},visual_review_path:review,
    visual_judgment:{outcome:'RETAINED',reviewer:'reviewer-fixture',observed_improvement:'La imagen posterior mejora visiblemente el color en las seis cámaras emparejadas de Unity.',remaining_gaps:'La composición del terreno todavía necesita diversidad forestal en el siguiente ciclo.'}
  };
  const recordFile=E+'/ARTISTIC_PROCESS_CURRENT.json';f(recordFile,JSON.stringify(record,null,2));git('add','.');git('commit','-m','save artistic process evidence');
  run(true,'valid six-view evidence','WORLD4X_ARTISTIC_PROCESS_EVIDENCE_PASS');
  const cameraFile=afterDir+'/regional-horizontal-source.json',orig=fs.readFileSync(path.join(root,cameraFile),'utf8'),changed=JSON.parse(orig);changed.cameraPosition.x=99;f(cameraFile,JSON.stringify(changed));run(false,'camera mismatch blocked','A/B camera cameraPosition not identical');f(cameraFile,orig);
  f(E+'/authoring/model.py','new unverified visual code');git('add','.');git('commit','-m','unverified art drift');run(false,'uncertified art drift blocked','Art code changed since certified source commit');
  git('reset','--hard','HEAD~1');fs.unlinkSync(path.join(root,recordFile));run(false,'missing manifest blocked','WORLD4X_ARTISTIC_PROCESS_FAIL');
  console.log('ARTISTIC_PROCESS_VALIDATOR_SMOKE_ALL_PASS');
}finally{fs.rmSync(tmp,{recursive:true,force:true});}
