import assert from 'node:assert/strict';
import fs from 'node:fs';
import http from 'node:http';
import vm from 'node:vm';
import crypto from 'node:crypto';
import zlib from 'node:zlib';
const root=process.argv[2];
const manifest=JSON.parse(fs.readFileSync(root+'/build-manifest.json','utf8'));
const helper=fs.readFileSync(new URL('./exact-decoded-transport.js',import.meta.url),'utf8');
const loader=fs.readFileSync(root+'/'+manifest.loader.split('?')[0],'utf8');
const reader=loader.slice(loader.indexOf('b.readBodyWithProgress='),loader.indexOf(',b.fetchWithProgress='));
assert(reader.startsWith('b.readBodyWithProgress='));
const server=http.createServer((req,res)=>{
 const name=req.url.split('?')[0];
 const file=root+name;
 if(!fs.existsSync(file)){res.writeHead(404);res.end();return;}
 const bytes=fs.statSync(file).size;
 res.writeHead(200,{'Content-Encoding':'gzip','Content-Length':bytes});
 fs.createReadStream(file).pipe(res);
});
await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
const base='http://127.0.0.1:'+server.address().port+'/';
try {
 const allocations=[];
 class TrackedArray extends Uint8Array {
  constructor(...args){super(...args);if(typeof args[0]==='number')allocations.push(args[0]);}
 }
 const scope={window:{fetch},location:{href:base},URL,Headers,Response,Map,console,Uint8Array:TrackedArray,b:{}};
 vm.createContext(scope);vm.runInContext(reader,scope);vm.runInContext(helper,scope);
 scope.installExactDecodedTransport(manifest.decoded_sizes);
 for(const key of ['framework','code','data']){
  const url=new URL(manifest[key],base).href;
  allocations.length=0;
  const response=await scope.window.fetch(url);
  assert.equal(response.url,url);
  assert.equal(response.headers.get('Content-Encoding'),null);
  assert.equal(Number(response.headers.get('Content-Length')),manifest.decoded_sizes[manifest[key]]);
  const result=await scope.b.readBodyWithProgress(response,()=>{},false);
  assert.deepEqual(allocations,[manifest.decoded_sizes[manifest[key]]],'Unity should allocate exactly once');
  const expectedHash=crypto.createHash('sha256');
  await new Promise((resolve,reject)=>fs.createReadStream(root+'/'+manifest[key].split('?')[0]).pipe(zlib.createGunzip()).on('data',b=>expectedHash.update(b)).on('end',resolve).on('error',reject));
  assert.equal(crypto.createHash('sha256').update(result.parsedBody).digest('hex'),expectedHash.digest('hex'));
  console.log(key+': exact decoded bytes, single allocation '+allocations[0]+' PASS');
 }
 const missing=await scope.window.fetch(base+'missing');
 assert.equal(missing.status,404);
 console.log('Error status passthrough PASS');
 const play=fs.readFileSync(root+'/play.js','utf8');
 assert(play.includes('cacheControl:()=>"no-store"'));
 console.log('Unity download-cache clone disabled; save code untouched PASS');
} finally {server.close();}
