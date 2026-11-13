#!/usr/bin/env node
// Real SHA-256 test of every chunk through the exact browser transport,
// served by local HTTP: catches broken patch decoders and wrong HTML script.
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import http from 'node:http';
import vm from 'node:vm';
import {webcrypto,createHash} from 'node:crypto';

const root=path.resolve(process.argv[2]||'site');
const candidate=path.join(root,'beta-iphone-p0-lazy-world');
const stable=path.join(root,'beta-iphone-p0-city');
const manifest=JSON.parse(fs.readFileSync(path.join(candidate,'build-manifest.json')));
const proof=JSON.parse(fs.readFileSync(path.join(root,'beta-lazy-world-owner-proof.json')));
const info=manifest.clock_copy_delta;
const oldIdentity=JSON.parse(fs.readFileSync(path.join(stable,'release-identity.json')));
const newIdentity=JSON.parse(fs.readFileSync(path.join(candidate,'release-identity.json')));
assert.equal(oldIdentity.source_sha,'63fb2095412a3066e18f3a8a31c2a03f40ef4944');
assert.equal(newIdentity.source_sha,'4fd1cacc9c206cfc20e78b20738e4b5bc48e0e71');
assert.equal(newIdentity.candidate_bundle_sha256,'49a2d76277a02c7dad31cc1fd46c641d4f15144c2c02e2c6dba1b01ec8c9d66a');
assert.equal(proof.published,false);
assert.equal(proof.external_testers_enabled,false);
assert.equal(proof.physical_iphone_verified,false);
assert.ok(proof.combined_bytes<980000000);
assert.equal(info.target_parts.length,31);
const markup=fs.readFileSync(path.join(candidate,'index.html'),'utf8');
assert.match(markup,/<script src="\.\/copy-transport-[a-f0-9]{20}\.js"><\/script>\s*<script src="play-[^"]+\.js"><\/script>/);
const play=fs.readFileSync(path.join(candidate,'play.js'),'utf8');
assert.match(play,/installEldoriaCopyTransport\(build\.data,build\.clock_copy_delta\)/);
const transportFile=markup.match(/src="\.\/(copy-transport-[a-f0-9]{20}\.js)"/)[1];
const browserCode=fs.readFileSync(path.join(candidate,transportFile),'utf8');
assert.equal(transportFile,'copy-transport-'+createHash('sha256').update(browserCode).digest('hex').slice(0,20)+'.js');
assert.doesNotMatch(browserCode,/localStorage|sessionStorage|indexedDB|serviceWorker/);
const digest=bytes=>createHash('sha256').update(bytes).digest('hex');

const server=http.createServer((req,res)=>{
 try{
  const uri=new URL(req.url,'http://localhost');
  if(!uri.pathname.startsWith('/Eldoria-Prewiu/')){res.writeHead(404).end();return;}
  const relative=uri.pathname.slice('/Eldoria-Prewiu/'.length);
  if(relative.includes('..')||relative.includes('\\')){res.writeHead(400).end();return;}
  const file=path.resolve(root,relative);
  if(!file.startsWith(root+path.sep)||!fs.existsSync(file)||!fs.statSync(file).isFile()){
    res.writeHead(404).end();return;
  }
  res.writeHead(200,{'Content-Type':'application/octet-stream',
                     'Content-Length':fs.statSync(file).size});
  fs.createReadStream(file).pipe(res);
 }catch(_){res.writeHead(500).end();}
});
await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
try{
 const origin='http://127.0.0.1:'+server.address().port;
 const base=origin+'/Eldoria-Prewiu/beta-iphone-p0-lazy-world/';
 const window={location:{href:base,origin},fetch:fetch.bind(globalThis),
               crypto:webcrypto,DecompressionStream};
 const context={window,ReadableStream,DecompressionStream,
                Response,URL,Uint8Array,AbortController,console};
 vm.createContext(context);
 vm.runInContext(browserCode,context);
 assert.equal(typeof window.installEldoriaCopyTransport,'function');
 const result=window.installEldoriaCopyTransport(manifest.data,info);
 assert.equal(result.active,true);
 const url=new URL(manifest.data,base).href;
 const response=await window.fetch(url);
 assert.equal(response.status,200);
 assert.equal(response.url,url);
 assert.equal(Number(response.headers.get('Content-Length')),513657613);
 // The forward-only candidate emits sub-64KiB frames instead of 16MiB
 // target allocations. Verify every true 16MiB Unity part by incremental
 // SHA256 across frames, with real HTTP/gzip and exact frozen engine payload.
 let index=0,total=0,partBytes=0,frameCount=0,maxFrame=0;
 let currentHash=createHash('sha256');
 const reader=response.body.getReader();
 while(true){
  const {value,done}=await reader.read();
  if(done)break;
  assert.ok(value.byteLength>0 && value.byteLength<=65536,
     'Browser emitted oversized Unity memory frame '+value.byteLength);
  frameCount++;
  maxFrame=Math.max(maxFrame,value.byteLength);
  total+=value.byteLength;
  let off=0;
  while(off<value.byteLength){
    assert.ok(index<31,'Unexpected extra Unity bytes');
    const part=info.target_parts[index],left=part.decoded_bytes-partBytes;
    const take=Math.min(left,value.byteLength-off);
    currentHash.update(value.subarray(off,off+take));
    off+=take;partBytes+=take;
    if(partBytes===part.decoded_bytes){
      assert.equal(currentHash.digest('hex'),part.decoded_sha256,
         'Real HTTP low-memory Unity part SHA mismatch '+index);
      console.log('ELDORIA_LOW_MEMORY_REAL_HTTP_PART_SHA_PASS',index,partBytes);
      index++;partBytes=0;currentHash=createHash('sha256');
    }
  }
 }
 assert.equal(index,31);
 assert.equal(partBytes,0);
 assert.equal(total,513657613);
 assert.ok(frameCount>7000, 'Expected genuinely small streamed memory frames');
 console.log('ELDORIA_SMALL_FRAMES_VERIFIED',frameCount,'max',maxFrame);
 const unknown=await window.fetch(origin+'/Eldoria-Prewiu/unknown-id');
 assert.equal(unknown.status,404);
 console.log('ELDORIA_OWNER_AB_BROWSER_STREAM_31_PARTS_PASS',total,
             'stable beta10 unchanged; iPhone physical QA not performed');
}finally{
 await new Promise(resolve=>server.close(resolve));
}
