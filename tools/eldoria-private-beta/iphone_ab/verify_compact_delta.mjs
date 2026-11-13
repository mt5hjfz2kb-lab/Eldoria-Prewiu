#!/usr/bin/env node
// Verify locally bundled browser decoder against every real compiled Unity
// .data byte, then exercise the exact ReadableStream-over-fetch in a Node
// browser shim. This is a CI test, NOT a physical iOS certification.
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import vm from 'node:vm';
import http from 'node:http';
import crypto,{webcrypto} from 'node:crypto';
import zlib from 'node:zlib';
import {createRequire} from 'node:module';

const root=path.resolve(process.argv[2]||'site');
const modules=path.resolve(process.argv[3]||'vendor');
const require=createRequire(import.meta.url);
const library=require(path.join(modules,'node_modules/@ably/vcdiff-decoder'));
const decode=library.decode||library.default?.decode;
assert.equal(typeof decode,'function','Pinned browser VCDIFF decoder does not implement decode');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
const site=path.join(root,'beta-iphone-clock-ab');
const old=path.join(root,'beta-native325');
const manifest=JSON.parse(fs.readFileSync(path.join(site,'build-manifest.json'),'utf8'));
const stable=JSON.parse(fs.readFileSync(path.join(old,'build-manifest.json'),'utf8'));
const part=manifest.clock_delta_parts;
assert.equal(part.length,31);
const original=stable.decoded_parts[stable.data];
assert.equal(original.length,31);
let total=0,patchTotal=0;
for(let i=0;i<31;i++){
  const before=zlib.gunzipSync(fs.readFileSync(path.join(old,original[i].url)));
  const patch=fs.readFileSync(path.join(site,part[i].patch_url));
  assert.equal(sha(patch),part[i].patch_sha256,'VCDIFF patch integrity mismatch '+i);
  const result=decode(new Uint8Array(patch),new Uint8Array(before));
  assert.ok(result instanceof Uint8Array);
  assert.equal(result.byteLength,part[i].decoded_bytes,'Decoded length '+i);
  assert.equal(sha(result),part[i].decoded_sha256,'Browser decoded Unity SHA mismatch '+i);
  total+=result.byteLength;patchTotal+=patch.byteLength;
  console.log('BROWSER_LIB_VCDIFF_EXACT_UNITY_PART_PASS',i,result.byteLength,patch.byteLength);
}
assert.equal(total,manifest.decoded_sizes[manifest.data]);
assert.ok(patchTotal<20_000_000,'Delta transport too large');
console.log('BROWSER_LIB_VCDIFF_ALL_31_PARTS_EXACT_PASS',total,patchTotal);

// Dynamic stream QA: run actual browser transport in an isolated VM with
// same-origin static HTTP. No game progress/state is read or changed.
const js=fs.readFileSync(path.join(site,'clock_delta_transport.js'),'utf8');
assert.ok(!/localStorage|sessionStorage|indexedDB|serviceWorker/.test(js),
  'Delta transport must not touch player persistence');
const serve=http.createServer((req,res)=>{
  const url=new URL(req.url,'http://localhost');
  const prefix='/Eldoria-Prewiu/';
  if(!url.pathname.startsWith(prefix)){res.writeHead(404);res.end();return;}
  const p=url.pathname.slice(prefix.length);
  if(p.includes('..')||p.includes('\\')){res.writeHead(400);res.end();return;}
  const f=path.join(root,p);
  if(!f.startsWith(root+path.sep)||!fs.existsSync(f)||!fs.statSync(f).isFile()){
    res.writeHead(404);res.end();return;
  }
  res.writeHead(200,{'Content-Type':'application/octet-stream','Content-Length':fs.statSync(f).size});
  fs.createReadStream(f).pipe(res);
});
await new Promise(resolve=>serve.listen(0,'127.0.0.1',resolve));
try{
  const origin='http://127.0.0.1:'+serve.address().port;
  const base=origin+'/Eldoria-Prewiu/beta-iphone-clock-ab/';
  const window={
    fetch:fetch.bind(globalThis),location:{href:base,origin},
    crypto:webcrypto,EldoriaVCDIFF:{decode},
  };
  const env={window,Response,Headers,ReadableStream,DecompressionStream,Uint8Array,URL,console};
  vm.createContext(env);vm.runInContext(js,env);
  assert.equal(typeof window.installEldoriaClockDeltaTransport,'function');
  const state=window.installEldoriaClockDeltaTransport(manifest.data,part,total);
  assert.equal(state.active,true);
  assert.equal(state.parts,31);
  const target=new URL(manifest.data,base).href;
  const resp=await window.fetch(target);
  assert.equal(resp.status,200);
  assert.equal(resp.url,target);
  assert.equal(Number(resp.headers.get('Content-Length')),total);
  let seen=0,index=0;
  const reader=resp.body.getReader();
  while(true){
    const {value,done}=await reader.read();
    if(done)break;
    assert.equal(sha(value),part[index].decoded_sha256,'HTTP real transport chunk SHA '+index);
    seen+=value.byteLength;index++;
  }
  assert.equal(index,31);
  assert.equal(seen,total);
  const blocked=await window.fetch(origin+'/Eldoria-Prewiu/missing-chunk');
  assert.equal(blocked.status,404,'Unknown URLs must preserve normal HTTP status');
  console.log('BROWSER_HTTP_STREAM_31_REAL_CHUNKS_PASS',seen);
}finally{
  serve.close();
}
