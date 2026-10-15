import test from 'node:test';
import assert from 'node:assert/strict';
import {checkRetry, localNight, otherActiveRuns} from './nightly-core.mjs';
const policy = {enabled:true,zone:'Europe/Madrid',window_start_hour:22,window_end_hour:8,minimum_idle_minutes:20,maximum_run_attempts:1};
const track={id:'reference-first',branch:'test/pilot',workflow_names:['[PILOT] Valoria reference-first isolated Unity captures'],retry_enabled:true,requires_owner:true,runner_type:'windows'};
const owner={id:'reference-first',branch:'test/pilot',status:'active',resources:['windows-runner-heavy']};
const latest={id:12,name:track.workflow_names[0],status:'completed',conclusion:'failure',event:'push',head_sha:'abc',run_attempt:1,updated_at:'2026-10-15T20:30:00Z'};
const base={policy,track,owner,latest,branchSha:'abc',globalBusy:false,failedJobs:[{id:9}],log:'GitHub Actions service unavailable HTTP 503',now:'2026-10-15T21:01:00Z'};
test('Madrid night gates including DST',()=>{assert.equal(localNight('2026-10-15T21:01:00Z'),true);assert.equal(localNight('2026-10-15T10:00:00Z'),false);assert.equal(localNight('2026-12-15T06:59:00Z'),true);assert.equal(localNight('2026-12-15T09:00:00Z'),false);});
test('exactly one eligible transient recovery',()=>assert.equal(checkRetry(base),'ELIGIBLE'));
test('rejects daytime, stale source, nonowner and busy runner',()=>{
 assert.equal(checkRetry({...base,now:'2026-10-15T10:00:00Z'}),'NOT_NIGHT');
 assert.equal(checkRetry({...base,branchSha:'def'}),'STALE_SOURCE');
 assert.equal(checkRetry({...base,owner:null}),'OWNER_NOT_ACTIVE');
 assert.equal(checkRetry({...base,globalBusy:true}),'RUNNER_OR_REPO_BUSY');
});
test('never repeats, fixes code failures or auto-approves art',()=>{
 assert.equal(checkRetry({...base,latest:{...latest,run_attempt:2}}),'RETRY_LIMIT');
 assert.equal(checkRetry({...base,log:'error CS0234: missing thing; HTTP 503'}),'SOURCE_OR_ART_ERROR');
 assert.equal(checkRetry({...base,log:'Unity compiler error'}),'NOT_TRANSIENT_INFRA_FAILURE');
 assert.equal(checkRetry({...base,track:{...track,retry_enabled:false}}),'RETRY_DISABLED');
});
test('requires exact single failed job and fresh completed failure',()=>{
 assert.equal(checkRetry({...base,failedJobs:[]}), 'NOT_EXACTLY_ONE_FAILED_JOB');
 assert.equal(checkRetry({...base,latest:{...latest,conclusion:'success'}}),'NOT_FAILED');
 assert.equal(checkRetry({...base,latest:{...latest,event:'pull_request'}}),'UNAUTHORIZED_EVENT');
 assert.equal(checkRetry({...base,now:'2026-10-15T20:35:00Z'}),'GRACE_PERIOD');
});

test('second modular track is protected by its own resource and explicit idle-only runner delegation', () => {
  const artist={...track,id:'valoria-63-individual-quality-uplift-20261015',
    owner_resource:'valoria-63-module-art',windows_delegation:'idle_only_no_lease_transfer',
    workflow_names:['[ART V4] 12 CC0 nature assets individual Blender finishing']};
  const artOwner={...owner,id:artist.id,resources:['valoria-63-module-art']};
  const artRun={...latest,name:artist.workflow_names[0]};
  assert.equal(checkRetry({...base,track:artist,owner:artOwner,latest:artRun}),'ELIGIBLE');
  assert.equal(checkRetry({...base,track:artist,owner:artOwner,latest:artRun,globalBusy:true}),'RUNNER_OR_REPO_BUSY');
  assert.equal(checkRetry({...base,track:{...artist,windows_delegation:undefined},owner:artOwner,latest:artRun}),'NO_IDLE_WINDOWS_DELEGATION');
  assert.equal(checkRetry({...base,track:artist,owner:{...artOwner,resources:[]},latest:artRun}),'WINDOWS_NOT_CLAIMED');
});
test('live watchdog own run is ignored but any other GitHub workflow blocks, including queued', () => {
  const own=88, blank={total_count:0,workflow_runs:[]};
  const pages=[{total_count:1,workflow_runs:[{id:own}]},blank,blank,blank,blank];
  assert.equal(otherActiveRuns(pages,own),false);
  assert.equal(otherActiveRuns(pages,0),true);
  assert.equal(otherActiveRuns([{total_count:2,workflow_runs:[{id:own},{id:99}]},blank,blank,blank,blank],own),true);
  assert.equal(otherActiveRuns([pages[0],{total_count:1,workflow_runs:[{id:99}]},blank,blank,blank],own),true);
  assert.equal(otherActiveRuns([{total_count:4,workflow_runs:[{id:own}]},blank,blank,blank,blank],own),true);
});
