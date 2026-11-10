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
const candidate=path.join(root,'beta-iphone-clock-ab');
const stable=path.join(root,'beta-native325');
const manifest=JSON.parse(fs.readFileSync(path.join(candidate,'build-manifest.json')));
const proof=JSON.parse(fs.readFileSync(path.join(root,'beta-iphone-clock-ab-proof.json')));
const info=manifest.clock_copy_delta;
const oldIdentity=JSON.parse(fs.readFileSync(path.join(stable,'release-identity.json')));
const newIdentity=JSON.parse(fs.readFileSync(path.join(candidate,'release-identity.json')));
assert.equal(oldIdentity.source_sha,'be49d62e3daf490f41dba0bf8123a8e2eee64480');
assert.equal(newIdentity.source_sha,'004d3ef9c26e9070762e9e38ce84d69d525efd4f');
assert.equal(proof.external_testers_enabled,false);
assert.equal(proof.physical_iphone_stability_verified,false);
assert.ok(proof.combined_bytes<650000000);
assert.equal(info.target_parts.length,31);
const markup=fs.readFileSync(path.join(candidate,'index.html'),'utf8');
assert.match(markup,/<script src="\.\/copy-transport\.js"><\/script>\s*<script src="play-[^"]+\.js"><\/script>/);
const play=fs.readFileSync(path.join(candidate,'play.js'),'utf8');
assert.match(play,/installEldoriaCopyTransport\(build\.data,build\.clock_copy_delta\)/);
const browserCode=fs.readFileSync(path.join(candidate,'copy-transport.js'),'utf8');
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
 const base=origin+'/Eldoria-Prewiu/beta-iphone-clock-ab/';
 const window={location:{href:base,origin},fetch:fetch.bind(globalThis),
               crypto:webcrypto,DecompressionStream};
 const context={window,ReadableStream,DecompressionStream,
                Response,URL,Uint8Array,console};
 vm.createContext(context);
 vm.runInContext(browserCode,context);
 assert.equal(typeof window.installEldoriaCopyTransport,'function');
 const result=window.installEldoriaCopyTransport(manifest.data,info);
 assert.equal(result.active,true);
 const url=new URL(manifest.data,base).href;
 const response=await window.fetch(url);
 assert.equal(response.status,200);
 assert.equal(response.url,url);
 assert.equal(Number(response.headers.get('Content-Length')),503831429);
 let index=0,total=0;
 const reader=response.body.getReader();
 while(true){
  const {value,done}=await reader.read();
  if(done)break;
  assert.equal(digest(value),info.target_parts[index].decoded_sha256,
               'Real HTTP Unity chunk mismatch '+index);
  total+=value.byteLength;index++;
  console.log('ELDORIA_OWNER_AB_REAL_HTTP_CHUNK_SHA_PASS',index,value.byteLength);
 }
 assert.equal(index,31);
 assert.equal(total,503831429);
 const unknown=await window.fetch(origin+'/Eldoria-Prewiu/unknown-id');
 assert.equal(unknown.status,404);
 console.log('ELDORIA_OWNER_AB_BROWSER_STREAM_31_PARTS_PASS',total,
             'stable beta10 unchanged; iPhone physical QA not performed');
}finally{
 await new Promise(resolve=>server.close(resolve));
}
