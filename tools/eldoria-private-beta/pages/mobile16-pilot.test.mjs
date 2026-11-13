import test from 'node:test';
import assert from 'node:assert/strict';
import {mkdtemp,mkdir,readFile,writeFile,rm} from 'node:fs/promises';
import {tmpdir} from 'node:os';
import {join} from 'node:path';
import {fileURLToPath} from 'node:url';
import {stagePilot} from './pilot-stage.mjs';
import {handler} from './edge-mobile16-qa.mjs';
const REV='native325-mobile16-full-lazy-profile-owner-qa';
const SHA='4fd1cacc9c206cfc20e78b20738e4b5bc48e0e71';
const BUNDLE='49a2d76277a02c7dad31cc1fd46c641d4f15144c2c02e2c6dba1b01ec8c9d66a';
async function fixture(){
 const dir=await mkdtemp(join(tmpdir(),'eldoria-mobile16-pilot-')),site=join(dir,'beta-iphone-p0-lazy-world');
 await mkdir(site);const html='<!doctype html><html><body><canvas id="unity-canvas"></canvas><script src="real-game.js"></script></body></html>';
 const identity={revision:REV,source_sha:SHA,candidate_bundle_sha256:BUNDLE,owner_only:true,
 release_for_external_testers:false,native_journey_verified:true,real_lazy_unity_playmode_passed:true,real_native_i_v_journey_passed:true};
 const proof={source_sha:SHA,native_bundle_sha256:BUNDLE,original_files_preserved:144,
 shared_exact_streaming_assets:true,external_testers_enabled:false,combined_bytes:972044910};
 await writeFile(join(site,'index.html'),html);await writeFile(join(site,'release-identity.json'),JSON.stringify(identity));
 await writeFile(join(dir,'beta-lazy-world-owner-proof.json'),JSON.stringify(proof));
 return {dir,site,html,identity,proof};
}
test('Mobile16 stages its own exact profile without changing legacy source, default entry or engine bytes',async()=>{
 const f=await fixture(),source=fileURLToPath(new URL('.',import.meta.url));
 try{
  const legacy=await readFile(join(source,'client.mjs'),'utf8');
  const wasm=Buffer.from('IMMUTABLE_UNIT_FIXTURE_NOT_A_GAME');await writeFile(join(f.site,'engine.wasm'),wasm);
  const m=await stagePilot(f.site,source),client=await readFile(join(f.site,'client.mjs'),'utf8');
  assert.equal(await readFile(join(f.site,'index.html'),'utf8'),f.html);
  assert.deepEqual(await readFile(join(f.site,'engine.wasm')),wasm);
  assert.equal(await readFile(join(source,'client.mjs'),'utf8'),legacy);
  assert.match(legacy,/native325-mobile11-building-anchor/);
  assert.ok(client.includes("export const SOURCE='"+SHA+"'"));
  assert.ok(client.includes("export const RELEASE='"+REV+"'"));
  assert.match(client,/functions\/v1\/eldoria-mobile16-events-qa/);
  assert.equal(m.source_sha,SHA);assert.equal(m.build_revision,REV);
  assert.equal(m.backend_current_profile_verified,false);
  assert.equal(m.external_testers_enabled,false);
  assert.ok(m.source_files.every(x=>x.sha256&&x.original_sha256));
 }finally{await rm(f.dir,{recursive:true,force:true})}
});
test('Mobile16 rejects a wrong native bundle and a preserved-site identity mismatch before writing',async()=>{
 const source=fileURLToPath(new URL('.',import.meta.url)),f=await fixture();
 try{
  await writeFile(join(f.site,'release-identity.json'),JSON.stringify({...f.identity,candidate_bundle_sha256:'0'.repeat(64)}));
  await assert.rejects(stagePilot(f.site,source),/provenance/);
  await writeFile(join(f.site,'release-identity.json'),JSON.stringify(f.identity));
  await writeFile(join(f.dir,'beta-lazy-world-owner-proof.json'),JSON.stringify({...f.proof,source_sha:'0'.repeat(40)}));
  await assert.rejects(stagePilot(f.site,source),/preserved owner/);
  for(const combined_bytes of [null,undefined,0,-1,972044910.5,980000000]){
   await writeFile(join(f.dir,'beta-lazy-world-owner-proof.json'),JSON.stringify({...f.proof,combined_bytes}));
   await assert.rejects(stagePilot(f.site,source),/preserved owner/);
  }
  await assert.rejects(readFile(join(f.site,'pilot.html')));
  assert.equal(await readFile(join(f.site,'index.html'),'utf8'),f.html);
 }finally{await rm(f.dir,{recursive:true,force:true})}
});
function request(extra={}){
 return new Request('https://example.test',{method:'POST',headers:{Origin:'https://mt5hjfz2kb-lab.github.io',apikey:'public-fixture','Content-Type':'application/json'},
 body:JSON.stringify({client_event_id:'dc615dbe-28b0-4a58-88ef-f5f72c05ae61',tester_id:'ff615dbe-28b0-4a58-88ef-f5f72c05ae61',session_id:'ee615dbe-28b0-4a58-88ef-f5f72c05ae61',
 event_name:'world_opened',device_type:'desktop_chrome',elapsed_ms:17,payload:{bastion:1},consent_given:true,...extra})});
}
const env={SUPABASE_URL:'https://example.supabase.co',SUPABASE_PUBLISHABLE_KEY:'public-fixture',SUPABASE_SERVICE_ROLE_KEY:'server-fixture'};
test('Dedicated Mobile16 QA server stamps exact current engine identity and QA status',async()=>{
 let saved=null;const response=await handler(request(),env,async(url,options)=>{saved=JSON.parse(options.body);return new Response(null,{status:201})});
 assert.equal(response.status,202);assert.equal(saved.build_version,REV);assert.equal(saved.source_sha,SHA);assert.equal(saved.is_qa,true);
});
test('A browser cannot spoof the server engine identity or send an unconsented event',async()=>{
 let writes=0;const send=async()=>{writes++;return new Response(null,{status:201})};
 assert.equal((await handler(request({source_sha:'forged'}),env,send)).status,422);
 assert.equal((await handler(request({consent_given:false}),env,send)).status,422);
 assert.equal(writes,0);
});

