import test from 'node:test';
import assert from 'node:assert/strict';
import {mkdtemp,writeFile,readFile,rm,mkdir} from 'node:fs/promises';
import {tmpdir} from 'node:os';
import {join} from 'node:path';
import {stagePilot} from './pilot-stage.mjs';
const REV='native325-mobile11-building-anchor';
const SHA='8dc875430241a49b4d417782dfeab2d4c7a4f315';
async function setup(){
 const dir=await mkdtemp(join(tmpdir(),'eldoria-stage-'));
 const site=join(dir,'site'),source=join(dir,'source');await mkdir(site);await mkdir(source);
 const html='<!doctype html><html><body><main>UNITY_CANVAS</main><script src="play-abcdef.js"></script></body></html>';
 const identity={revision:REV,source_sha:SHA,release_for_external_testers:false,native_journey_verified:true};
 await writeFile(join(site,'index.html'),html);await writeFile(join(site,'release-identity.json'),JSON.stringify(identity));
 await writeFile(join(site,'build-manifest.json'),'{}');
 for(const p of ['client.mjs','signal-parser.mjs','pilot-bootstrap.mjs','restart-diagnostic.mjs'])await writeFile(join(source,p),'// fake QA only '.repeat(50));
 return {dir,site,source,html,identity};
}
test('Stages only optional pilot, does not alter original Unity HTML',async()=>{
 const a=await setup();try{const m=await stagePilot(a.site,a.source);
  assert.equal((await readFile(join(a.site,'index.html'),'utf8')),a.html);
  const pilot=await readFile(join(a.site,'pilot.html'),'utf8');
  assert.ok(pilot.includes('<main>UNITY_CANVAS</main>'));
  assert.ok(pilot.includes('pilot-bootstrap.mjs'));
  assert.match(pilot,/pilot-bootstrap\.mjs\?v=[a-f0-9]{16}/,'Boot script must bypass stale iOS HTML caches');
  assert.ok(pilot.includes('noindex,nofollow'));
  assert.equal(m.source_sha,SHA);
  assert.equal(m.external_testers_enabled,false);
  assert.equal(m.telemetry_backend,'supabase_edge_qa_only');
  assert.equal(m.entry,'pilot.html?eldoriaBetaTelemetry=1');
 }finally{await rm(a.dir,{recursive:true,force:true})}});
test('Wrong or released build is never staged',async()=>{
 const a=await setup();try{
  await writeFile(join(a.site,'release-identity.json'),JSON.stringify({...a.identity,source_sha:'x'}));
  await assert.rejects(stagePilot(a.site,a.source),/wrong certified/);
  await writeFile(join(a.site,'release-identity.json'),JSON.stringify({...a.identity,release_for_external_testers:true}));
  await assert.rejects(stagePilot(a.site,a.source),/audience/);
 }finally{await rm(a.dir,{recursive:true,force:true})}});
test('Refuses credentials, duplicate injection and avoids side effects',async()=>{
 const a=await setup();try{
  await writeFile(join(a.source,'client.mjs'),'SUPABASE_SERVICE_ROLE_KEY='+ 'X'.repeat(600));
  await assert.rejects(stagePilot(a.site,a.source),/unsafe telemetry/);
  await assert.rejects(readFile(join(a.site,'pilot.html')));
 }finally{await rm(a.dir,{recursive:true,force:true})}});
