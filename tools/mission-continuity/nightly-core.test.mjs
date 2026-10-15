import test from 'node:test';
import assert from 'node:assert/strict';
import {checkRetry, localNight} from './nightly-core.mjs';
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
