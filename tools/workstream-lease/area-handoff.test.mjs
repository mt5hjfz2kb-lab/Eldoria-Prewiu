import {test} from 'node:test';
import assert from 'node:assert/strict';
import {decideAreaRelease,applyAreaRelease,readRegistry} from './area-handoff.mjs';
import fs from 'node:fs';

const id='valoria-master-plan-layered-construction-20261016';
const sha='a'.repeat(40);
const owner='area-current-chat';
const input=()=>({active:[{id,owner,ownership_revision:9,ownership_state:'claimed',
  status:'active',write_scope_released:false,branch:'fix/player-feedback-context-ui-20261030',
  scope:['Unity/Assets/Eldoria/HeroesUI/**'],resources:['windows-runner-heavy'],
  area_handoff_request:{status:'ready',completed:true,area:'Resumen Aldric y Lyra',
    owner,revision:9,branch_head:sha,run_id:12345,artistic_approval:'provisional',
    note:'Owner accepted for continuing next area'}}]});
const evidence=()=>({run:{id:12345,status:'completed',conclusion:'success',
  head_sha:sha,head_branch:'fix/player-feedback-context-ui-20261030'},
  branchHead:sha,busyRuns:[]});
test('no explicit request never releases a workstream',()=>{
  const r=input();delete r.active[0].area_handoff_request;
  assert.equal(decideAreaRelease(r,evidence()).state,'NO_REQUEST');
});
test('completed exact-head Unity gate and clean branch allow release',()=>{
  const d=decideAreaRelease(input(),evidence());
  assert.equal(d.state,'READY');
  const before=input();const after=applyAreaRelease(before,d,'2026-11-02T12:00:00Z');
  assert.equal(after.active[0].owner,null);
  assert.equal(after.active[0].ownership_state,'available');
  assert.equal(after.active[0].write_scope_released,true);
  assert.deepEqual(after.active[0].resources,[]);
  assert.equal(after.active[0].ownership_revision,10);
  assert.equal(after.active[0].last_completed_area.source_sha,sha);
  assert.equal(after.active[0].last_completed_area.artistic_approval,'provisional');
  assert.equal(after.active[0].area_handoff_request,undefined);
  assert.equal(before.active[0].owner,owner);
  assert.equal(after.active[0].ownership_history.at(-1).owner,owner);
});
test('queued, pending, running, waiting or requested branch jobs block release',()=>{
  for(const state of ['queued','pending','in_progress','waiting','requested']){
    const e=evidence();e.busyRuns.push({id:99999,status:state});
    assert.equal(decideAreaRelease(input(),e).state,'PENDING',state);
  }
});
test('corresponding evidence run itself may appear in historical queue only when completed',()=>{
  const e=evidence();e.busyRuns=[{id:12345,status:'completed'}];
  assert.equal(decideAreaRelease(input(),e).state,'READY');
  e.run.status='in_progress';assert.equal(decideAreaRelease(input(),e).state,'PENDING');
});
test('failed or mismatched evidence blocks release',()=>{
  const bad=evidence();bad.run.conclusion='failure';
  assert.equal(decideAreaRelease(input(),bad).state,'BLOCKED');
  const mismatched=evidence();mismatched.run.head_sha='b'.repeat(40);
  assert.equal(decideAreaRelease(input(),mismatched).state,'BLOCKED');
  const noqueue=evidence();delete noqueue.busyRuns;
  assert.equal(decideAreaRelease(input(),noqueue).state,'PENDING');
});
test('a newer PR HEAD or owner revision prevents a stale release',()=>{
  const advanced=evidence();advanced.branchHead='c'.repeat(40);
  assert.equal(decideAreaRelease(input(),advanced).state,'PENDING');
  const changed=input();changed.active[0].ownership_revision=10;
  assert.equal(decideAreaRelease(changed,evidence()).state,'BLOCKED');
  changed.active[0].ownership_revision=9;changed.active[0].owner='someone-else';
  assert.equal(decideAreaRelease(changed,evidence()).state,'BLOCKED');
});
test('without explicit completed flag or with conflicting Windows claims, no release',()=>{
  const r=input();r.active[0].area_handoff_request.completed=false;
  assert.equal(decideAreaRelease(r,evidence()).state,'BLOCKED');
  const conflict=input();conflict.active.push({id:'other',owner:'other',resources:['windows-runner-heavy']});
  assert.equal(decideAreaRelease(conflict,evidence()).state,'BLOCKED');
  assert.throws(()=>applyAreaRelease(input(),{state:'PENDING'},'2026-11-02'));
});
test('two handoff requests require separate safe decisions',()=>{
  const r=input();r.active.push({...structuredClone(r.active[0]),id:'second-area'});
  assert.equal(decideAreaRelease(r,evidence()).state,'BLOCKED');
});
test('workflow remains hosted and only runs for authorized push, completion or dispatch',()=>{
  // Repository architecture guard uses sparse checkout without workflow files.
  // The dedicated handoff CI includes this workflow and checks it fully.
  if(!fs.existsSync('.github/workflows/area-handoff.yml')) return;
  const y=fs.readFileSync('.github/workflows/area-handoff.yml','utf8');
  assert.match(y,/runs-on: ubuntu-latest/);
  assert.match(y,/node --test tools\/workstream-lease\/area-handoff.test.mjs/);
  assert.match(y,/workflow_dispatch:/);
  assert.match(y,/workflow_run:/);
  assert.match(y,/contents: write/);
  assert.doesNotMatch(y,/cancel-in-progress:\s*true/);
  assert.doesNotMatch(y,/runs-on: \[self-hosted/);
});

test('large canonical registry uses immutable blob and preserves CAS identity',async()=>{
  const {createHash}=await import('node:crypto');
  const bytes=Buffer.from(JSON.stringify({active:[],history:'x'.repeat(1100000)}));
  const sha=createHash('sha1').update('blob '+bytes.length+'\0').update(bytes).digest('hex');
  const calls=[];
  const value=await readRegistry('owner/repo','test',async(path)=>{
    calls.push(path);
    return calls.length===1?{sha,encoding:'none',content:''}:
      {sha,encoding:'base64',content:bytes.toString('base64')};
  });
  assert.equal(value.sha,sha);
  assert.equal(value.registry.history.length,1100000);
  assert.equal(calls[1],'/repos/owner/repo/git/blobs/'+sha);
  await assert.rejects(readRegistry('owner/repo','test',async()=>({sha,encoding:'base64',content:'e30='})),/digest mismatch/);
  let count=0;
  await assert.rejects(readRegistry('owner/repo','test',async()=>++count===1?
    {sha,encoding:'none'}:{sha:'a'.repeat(40),encoding:'base64',content:bytes.toString('base64')}),/mismatched/);
});
