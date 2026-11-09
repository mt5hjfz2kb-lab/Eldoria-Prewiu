import test from 'node:test';
import assert from 'node:assert/strict';
import {randomUUID,createHmac} from 'node:crypto';
import worker from './worker.mjs';
const secret='NEVER-USE-IN-PRODUCTION-LOCAL-TEST-000000000000000';
const id=randomUUID();
const RELEASE='native325-mobile11-building-anchor';
const files={
 'index.html':{content:'<!doctype html><title>Valoria</title>',type:'text/html; charset=utf-8'},
 'payload-decoded/data-0.bin.br':{content:'fake brotli binary',type:'application/octet-stream',encoding:'br'},
 'Build/ValoriaWebPlaytest.wasm.unityweb':{content:'fake wasm',type:'application/wasm',encoding:'gzip'}
};
const bucket={
 async head(key){return this.get(key)},
 async get(key){
  const f=files[key.slice(('releases/'+RELEASE+'/').length)];
  if(!f)return null;
  return {body:new Response(f.content).body,size:f.content.length,httpEtag:'"test-etag"',
   writeHttpMetadata(h){h.set('Content-Type',f.type);if(f.encoding)h.set('Content-Encoding',f.encoding)}};
 }
};
const env={ACTIVE_RELEASE:RELEASE,LINK_SECRET:secret,ELDORIA_BUCKET:bucket,CONSENTED_TESTER_IDS:id,QA_MODE:'true'};
function token(expires=Math.floor(Date.now()/1000)+3600,uid=id){
 const bytes=Buffer.from('v1:'+uid+':'+expires,'utf8');
 return bytes.toString('base64url')+'.'+createHmac('sha256',secret).update(bytes).digest('base64url');
}
const cookie='__Host-eldoria_beta='+token();
function req(path,method='GET',extra={}){
 return new Request('https://beta.eldoria.test'+path,{method,...extra,headers:{...(extra.headers||{}),Cookie:cookie}});
}
test('no configuration -> beta closed',async()=>{
 const response=await worker.fetch(req('/'),{...env,ACTIVE_RELEASE:''});
 assert.equal(response.status,503);
});
test('unapproved tester cannot enter even with signed token',async()=>{
 const response=await worker.fetch(req('/entrar/'+token()),{...env,CONSENTED_TESTER_IDS:''});
 assert.equal(response.status,403);
});
test('one-click signed invitation redirects with secure HttpOnly cookie',async()=>{
 const response=await worker.fetch(req('/entrar/'+token()),env);
 assert.equal(response.status,302);assert.equal(response.headers.get('Location'),'/?eldoriaBetaTelemetry=1');
 assert.match(response.headers.get('Set-Cookie'),/HttpOnly; Secure; SameSite=Lax/);
});
test('index and Unity assets remain private and byte-preserving',async()=>{
 const noCookie=await worker.fetch(new Request('https://beta.eldoria.test/'),env);
 assert.equal(noCookie.status,403);
 const game=await worker.fetch(req('/'),env);
 assert.equal(game.status,200);assert.match(await game.text(),/Valoria/);
 assert.equal(game.headers.get('Content-Type'),'text/html; charset=utf-8');
 const br=await worker.fetch(req('/payload-decoded/data-0.bin.br'),env);
 assert.equal(br.status,200);assert.equal(br.headers.get('Content-Encoding'),'br');
 assert.equal(await br.text(),'fake brotli binary');
 const gz=await worker.fetch(req('/Build/ValoriaWebPlaytest.wasm.unityweb'),env);
 assert.equal(gz.headers.get('Content-Encoding'),'gzip');
});
test('forged, expired, future and revoked links denied',async()=>{
 const corrupt=token().slice(0,-1)+'x';
 assert.equal((await worker.fetch(req('/entrar/'+corrupt),env)).status,403);
 assert.equal((await worker.fetch(req('/entrar/'+token(Math.floor(Date.now()/1000)-1)),env)).status,403);
 assert.equal((await worker.fetch(req('/entrar/'+token(Math.floor(Date.now()/1000)+32*86400)),env)).status,403);
 assert.equal((await worker.fetch(req('/'),{...env,REVOKED_TESTER_IDS:id})).status,403);
});
test('path traversal and other HTTP methods blocked',async()=>{
 for(const path of ['/some%2Fthing','/secret//file','/__admin','/../secret']){
  const r=await worker.fetch(req(path),env);
  assert.notEqual(r.status,200);
 }
 assert.equal((await worker.fetch(req('/','POST'),env)).status,405);
});
test('only explicit consent and allowed events pass; no card events',async()=>{
 const uuid=randomUUID();
 const input={client_event_id:uuid,session_id:randomUUID(),event_name:'bastion_reached',
  device_type:'android_chrome',elapsed_ms:40000,payload:{bastion:2},consent_given:true};
 const base={'Content-Type':'application/json','Origin':'https://beta.eldoria.test'};
 const send=async body=>worker.fetch(req('/beta/events','POST',{body:JSON.stringify(body),headers:base}),env);
 assert.equal((await send(input)).status,503); // secret missing, correct fail closed
 assert.equal((await send({...input,consent_given:false})).status,422);
 assert.equal((await send({...input,event_name:'card_acquired'})).status,422);
 assert.equal((await send({...input,payload:{playerName:'PII'}})).status,422);
 assert.equal((await send({...input,event_name:'bastion_reached',payload:{bastion:6}})).status,422);
 assert.equal((await send({...input,event_name:'end_of_slice',payload:{bastion:4}})).status,422);
 const x=await worker.fetch(req('/beta/events','POST',{body:JSON.stringify(input),
  headers:{'Content-Type':'application/json','Origin':'https://evil.test'}}),env);
 assert.equal(x.status,403);
});
test('server sets source, round, tester ID and QA; never trusts client identity',async()=>{
 const json={client_event_id:randomUUID(),session_id:randomUUID(),event_name:'vigilant_result',
  device_type:'desktop_chrome',elapsed_ms:12000,payload:{stage_number:5,outcome_code:'win'},consent_given:true};
 const original=globalThis.fetch;let captured;
 globalThis.fetch=async (url,options)=>{captured={url,body:JSON.parse(options.body),headers:options.headers};
  return new Response('',{status:201});};
 try{
  const r=await worker.fetch(req('/beta/events','POST',{
   body:JSON.stringify(json),headers:{Origin:'https://beta.eldoria.test','Content-Type':'application/json'}}),
   {...env,SUPABASE_URL:'https://example.supabase.co',SUPABASE_SECRET_KEY:'sb_secret_only_a_server_must_know_123456789'});
  assert.equal(r.status,202);
  assert.equal(captured.body.tester_id,id);
  assert.equal(captured.body.build_version,RELEASE);
  assert.equal(captured.body.source_sha,'8dc875430241a49b4d417782dfeab2d4c7a4f315');
  assert.equal(captured.body.is_qa,true);
  assert.ok(!JSON.stringify(json).includes('sb_secret'));
  assert.equal(captured.headers.apikey,'sb_secret_only_a_server_must_know_123456789');
 }finally{globalThis.fetch=original}
});
