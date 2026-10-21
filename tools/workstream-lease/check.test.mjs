import { test } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { spawnSync } from 'node:child_process';
import { requireLiveLease } from './lease.mjs';

const id = 'valoria-master-plan-layered-construction-20261016';
const copy = x => structuredClone(x);
const registry = (owner = 'new-art-chat') => ({ active: [{ id, owner, status: 'active', branch: 'main',
  resources: ['windows-runner-heavy'], scope: ['Unity/canonical-art/**'], ownership_revision: 3 }], history: [] });
test('a new owner can run after claiming the same workstream; names are not hardcoded', () => {
  for (const owner of ['new-art-chat', 'future-chat', 'chatgpt-eldoria-visual-recovery-20261020']) {
    const r = registry(owner);
    assert.equal(requireLiveLease(r, copy(r), id).owner, owner);
  }
});
test('an available canonical workstream is not an execution lease', () => {
  for (const patch of [{owner:null}, {owner:''}, {owner:'  '}, {ownership_state:'available'}, {write_scope_released:true}, {resources:[]}]) {
    const r = registry(); Object.assign(r.active[0], patch);
    assert.throws(() => requireLiveLease(r, r, id));
  }
});
test('an old queued run fails after transfer, scope change or same-name reclaim', () => {
  const queued = registry();
  for (const patch of [{owner:'next-art-chat'}, {ownership_revision:4}, {scope:['Unity/different/**']}]) {
    const live = copy(queued); Object.assign(live.active[0], patch);
    assert.throws(() => requireLiveLease(queued, live, id), /predates/);
  }
});
test('two resource claims, duplicate IDs and missing registries fail closed', () => {
  const r = registry(), competing = copy(r);
  competing.active.push({...copy(r.active[0]),id:'other-work',owner:'other-chat'});
  assert.throws(() => requireLiveLease(r, competing, id), /conflicting/);
  const duplicate = copy(r); duplicate.active.push(copy(r.active[0]));
  assert.throws(() => requireLiveLease(r, duplicate, id), /expected one/);
  for (const broken of [{}, {active:[]}, {active:null}]) assert.throws(() => requireLiveLease(r, broken, id));
});
test('blocked claims, wrong branches and invalid revisions cannot execute', () => {
  for (const patch of [{status:'blocked'}, {branch:'old-prototype'}, {ownership_revision:-1}, {ownership_revision:'3'}]) {
    const r=registry(); Object.assign(r.active[0],patch);
    assert.throws(() => requireLiveLease(r,r,id));
  }
});
test('history and independent no-resource work do not block a valid lease', () => {
  const r=registry(); r.history.push({...copy(r.active[0]),owner:'old-chat'});
  r.active.push({id:'independent-tripo-policy',owner:'another-chat',resources:[]});
  assert.equal(requireLiveLease(r,copy(r),id).revision,3);
  delete r.active[0].ownership_revision;
  assert.equal(requireLiveLease(r,copy(r),id).revision,0);
});
test('real CLI reads BOM registries, rejects corrupt data and never writes inputs', () => {
  const dir=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-lease-'));
  try {
    const a=path.join(dir,'snapshot.json'),b=path.join(dir,'live.json');
    const original='\uFEFF'+JSON.stringify(registry()); fs.writeFileSync(a,original);fs.writeFileSync(b,original);
    const run=()=>spawnSync(process.execPath,['tools/workstream-lease/check.mjs',a,b,id],{encoding:'utf8'});
    assert.equal(run().status,0);assert.equal(fs.readFileSync(a,'utf8'),original);
    fs.writeFileSync(b,'{broken');const failed=run();assert.notEqual(failed.status,0);assert.match(failed.stderr,/WORKSTREAM_LEASE_FAIL/);
    assert.equal(fs.readFileSync(a,'utf8'),original);
  } finally {fs.rmSync(dir,{recursive:true,force:true});}
});
test('both production entry points bind pinned ownership to fresh main and retain process guards', () => {
  for (const [file,liveDir] of [['valoria-reference-clean.yml','valoria-clean-live-main'],['valoria-windows-chromium-qa.yml','valoria-qa-live-main']]) {
    const text=fs.readFileSync('.github/workflows/'+file,'utf8');
    assert.ok(text.includes('path: '+liveDir));assert.ok(text.includes('ref: main'));
    assert.ok(text.includes('node tools/workstream-lease/check.mjs pipeline/active-workstreams.json'));
    assert.ok(text.includes(liveDir+'/pipeline/active-workstreams.json'));
    assert.ok(text.includes('if($LASTEXITCODE)'));
    assert.ok(text.includes('Get-Process Unity,blender'));
    assert.ok(!text.includes("owner -eq 'chatgpt-eldoria-visual-recovery-20261020'"));
  }
});
