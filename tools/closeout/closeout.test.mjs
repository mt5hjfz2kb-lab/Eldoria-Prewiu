import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, writeFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { createHash } from 'node:crypto';
import { triage, browserPreflight, main } from './closeout.mjs';

const validSource='2210c14593f142ef144be6d628df7e873512e218';

test('actual native139 return-to-city touch failure remains an unresolved GAME/QA interaction, never PASS',()=>{
  const log='2026-10-19 VALORIA_WEB_PLAYTEST '+JSON.stringify({status:'FAIL', failures:['Error: Return to city: timed out after 10s'], runtime_errors:[], last_action:{kind:'touch-hold-release',label:'world'}});
  const r=triage({logText:log,change:'qa_only'});
  assert.equal(r.verdict,'BLOCKED_NOT_CLOSED');
  assert.ok(r.issues.some(x=>x.category==='INTERACTION_UNRESOLVED'));
  assert.equal(r.next_build,'REUSE_EXACT_ARTIFACT_IF_SHA_AND_PROVENANCE_MATCH');
  assert.equal(r.release_approved,false);
});

test('separate environment FFmpeg failure and observable navigation failure',()=>{
  const r=triage({logText:'VALORIA_WEB_PLAYTEST '+JSON.stringify({status:'FAIL',failures:['World pan up did not move camera','Genuine Chromium video frames missing: spawnSync ffmpeg ENOENT']})});
  assert.deepEqual(new Set(r.issues.map(x=>x.category)),new Set(['ENVIRONMENT_FFMPEG','INTERACTION_UNRESOLVED']));
});

test('compile failure demands real rebuild after a product change',()=>{
  const r=triage({logText:'Unity/Editor error CS1069 AndroidJavaObject missing from AndroidJNIModule\nBuild Failed, Result: Failure',change:'product'});
  assert.ok(r.issues.some(x=>x.category==='PLATFORM_COMPILE'));
  assert.equal(r.next_build,'REQUIRED_AFTER_GAME_CHANGE');
});

test('input fingerprint mismatch fails closed and cannot silently approve artifact',()=>{
  const r=triage({logText:'Input manifest differs from native proof; source fingerprint mismatch'});
  assert.equal(r.verdict,'BLOCKED_NOT_CLOSED');
  // The classified issue must prevent release even if a wrapper forgot to set exit code.
  assert.equal(r.issues[0].category,'SOURCE_INTEGRITY');
  assert.equal(r.release_approved,false);
});

test('Editor indexation timeout classified separately from gameplay and unknown fails closed',()=>{
  const r=triage({logText:'Exception UnityEditor.Search.SearchDatabase.IndexationOnStartup\n##[error] Process completed with exit code 1'});
  assert.ok(r.issues.some(x=>x.category==='ENVIRONMENT_UNITY_EDITOR'));
  assert.equal(r.verdict,'BLOCKED_NOT_CLOSED');
  const unknown=triage({report:{status:'FAIL',failures:['New error without known signature']}});
  assert.equal(unknown.issues[0].category,'UNCLASSIFIED_FAILURE');
});

test('passing technical report cannot certify release automatically',()=>{
  const r=triage({report:{status:'PASS',checks:{control:true},runtime_errors:[]},change:'qa_only'});
  assert.equal(r.verdict,'NO_FAILURE_ESTABLISHED_NOT_CERTIFIED');
  assert.equal(r.release_approved,false);
});

test('browser preflight rejects wrong artifact digest and source drift before test',()=>{
  const directory=mkdtempSync(path.join(tmpdir(),'eldoria-closeout-'));
  try {
    const file=path.join(directory,'candidate.zip');writeFileSync(file,'some build bytes');
    const output=browserPreflight({artifact:file,artifactSha256:'0'.repeat(64),sourceSha:validSource,validatedSourceSha:'1'.repeat(40),
      findBrowser:()=>'/fake/edge',findFfmpeg:()=>'/fake/ffmpeg',findPlaywright:()=>'/fake/playwright'});
    assert.equal(output.verdict,'BLOCKED_BEFORE_EXPENSIVE_QA');
    assert.ok(output.blocked.includes('source_provenance'));
    assert.ok(output.blocked.includes('artifact_bytes'));
  } finally { rmSync(directory,{recursive:true,force:true}); }
});

test('browser preflight passes only readiness checks with pinned inputs and prerequisites',()=>{
  const directory=mkdtempSync(path.join(tmpdir(),'eldoria-closeout-'));
  try {
    const file=path.join(directory,'candidate.zip');writeFileSync(file,'fixed candidate bytes');
    const digest=createHash('sha256').update('fixed candidate bytes').digest('hex');
    const result=browserPreflight({artifact:file,artifactSha256:digest,sourceSha:validSource,validatedSourceSha:validSource,
      scriptPath:file, checkScript:()=>true,findBrowser:()=>'/fake/edge',findFfmpeg:()=>'/fake/ffmpeg',findPlaywright:()=>'/fake/playwright'});
    assert.equal(result.verdict,'READY_FOR_TARGETED_QA_NOT_CERTIFIED');
    assert.deepEqual(result.blocked,[]);
    assert.equal(result.release_approved,false);
  } finally { rmSync(directory,{recursive:true,force:true}); }
});

test('missing browser/ffmpeg/playwright prevents expensive run even with same artifact',()=>{
  const directory=mkdtempSync(path.join(tmpdir(),'eldoria-closeout-'));
  try {
    const file=path.join(directory,'candidate.zip');writeFileSync(file,'fixed candidate bytes');
    const digest=createHash('sha256').update('fixed candidate bytes').digest('hex');
    const r=browserPreflight({artifact:file,artifactSha256:digest,sourceSha:validSource,validatedSourceSha:validSource,
      findBrowser:()=>null,findFfmpeg:()=>null,findPlaywright:()=>null});
    assert.deepEqual(new Set(r.blocked),new Set(['playwright_package','chromium_or_edge','ffmpeg']));
  } finally { rmSync(directory,{recursive:true,force:true}); }
});

test('CLI errors on missing evidence without creating any status files',()=>{
  assert.equal(main(['triage']),2);
  assert.equal(main(['preflight-browser']),2);
});
