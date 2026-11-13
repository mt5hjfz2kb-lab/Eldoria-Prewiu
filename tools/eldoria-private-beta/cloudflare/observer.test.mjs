import test from 'node:test';
import assert from 'node:assert/strict';
import {randomUUID,createHmac} from 'node:crypto';
import {Script} from 'node:vm';
import worker from './worker.mjs';
import {OBSERVER_JS} from './observer-source.mjs';
const RELEASE='native325-mobile11-building-anchor';
const id=randomUUID();
const secret='ONLY_FOR_TESTS_123456789012345678901234567890';
const bytes=Buffer.from('v1:'+id+':'+(Math.floor(Date.now()/1000)+3600));
const token=bytes.toString('base64url')+'.'+createHmac('sha256',secret).update(bytes).digest('base64url');
const cookie='__Host-eldoria_beta='+token;
const html='<!doctype html><html lang="es"><body><button id="play">Entrar en Valoria</button><script src="play.js"></script></body></html>';
const bucket={
 async get(key){
  const path=key.replace('releases/'+RELEASE+'/','');
  if(path!=='index.html'&&path!=='Build/game.wasm.unityweb')return null;
  const v=path==='index.html'?html:'fake compressed binary';
  return {size:v.length,httpEtag:'"exact-frozen-object"',
   body:new Response(v).body,writeHttpMetadata(h){
     h.set('Content-Type',path==='index.html'?'text/html; charset=utf-8':'application/wasm');
     if(path!=='index.html')h.set('Content-Encoding','gzip');
   }};
 },
 async head(key){return this.get(key)}
};
const env={ACTIVE_RELEASE:RELEASE,LINK_SECRET:secret,
 ELDORIA_BUCKET:bucket,CONSENTED_TESTER_IDS:id,QA_MODE:'true',OBSERVER_ENABLED:'true'};
const request=(path,auth=true)=>new Request('https://beta.eldoria.test'+path,
 {headers:auth?{Cookie:cookie}:{}});
test('observer compiles and contains only browser lifecycle probes',()=>{
 assert.doesNotThrow(()=>new Script(OBSERVER_JS));
 assert.match(OBSERVER_JS,/game_ready/);
 assert.match(OBSERVER_JS,/frame_summary/);
 assert.doesNotMatch(OBSERVER_JS,/card_acquired|codex_state|service_role|supabase_secret/i);
});
test('observer cannot be accessed without tester link',async()=>{
 assert.equal((await worker.fetch(request('/beta/observer.js',false),env)).status,403);
});
test('observer served as JS only after signed consented invite',async()=>{
 const r=await worker.fetch(request('/beta/observer.js'),env);
 assert.equal(r.status,200);
 assert.match(r.headers.get('Content-Type'),/application\/javascript/);
 assert.equal(await r.text(),OBSERVER_JS);
});
test('default freeze host sends original HTML unchanged',async()=>{
 const r=await worker.fetch(request('/'),{...env,OBSERVER_ENABLED:'false'});
 assert.equal(await r.text(),html);
 assert.equal(r.headers.get('ETag'),'"exact-frozen-object"');
});
test('optional observer changes HTTP index only, never R2 stored object',async()=>{
 const before=html;
 const r=await worker.fetch(request('/'),env);
 const h=await r.text();
 assert.equal(r.status,200);
 assert.match(h,/<script defer src="\/beta\/observer\.js"><\/script><\/body>/);
 assert.ok(h.startsWith(before.split('</body>')[0]));
 assert.equal(r.headers.get('ETag'),null);
 assert.equal(r.headers.get('Content-Encoding'),null);
 assert.equal(r.headers.get('Cache-Control'),'private, no-store');
 assert.equal(html,before);
});
test('WASM response stays unchanged when observer enabled',async()=>{
 const r=await worker.fetch(request('/Build/game.wasm.unityweb'),env);
 assert.equal(r.status,200);
 assert.equal(r.headers.get('Content-Encoding'),'gzip');
 assert.equal(await r.text(),'fake compressed binary');
});
test('observer unavailable to unauthorized tester even with enabled flag',async()=>{
 assert.equal((await worker.fetch(request('/',false),env)).status,403);
});
