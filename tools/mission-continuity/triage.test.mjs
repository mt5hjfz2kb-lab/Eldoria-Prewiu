import test from 'node:test';
import assert from 'node:assert/strict';
import {triageRun} from './triage.mjs';

const report=(over={})=>({id:'eldoria-world-region-1-canon-20261022',
 observation_mode:'report_only_no_issues',state:'WAITING_EVIDENCE',
 source_matches_branch_head:true,latest_run:{id:40316305875,status:'completed',conclusion:'success'},...over});
const evidence=()=>({jobs:{total_count:1,jobs:[{id:9,name:'Native Unity test',steps:[{name:'PlayMode',conclusion:'success'}]}]},
 artifacts:{total_count:1,artifacts:[{id:12,name:'actual-native-captures',expired:false,size_in_bytes:1000}]}});
test('green CI leads to evidence review, not art pass or completion',()=>{
 const result=triageRun({report:report(),...evidence()});
 assert.equal(result.status,'REVIEW_VERIFIED_RUN_OUTPUTS');
 assert.equal(result.art_approved,false);assert.equal(result.dispatch_allowed,false);
 assert.deepEqual(result.artifacts[0].name,'actual-native-captures');
});
test('failure includes only bounded step metadata, never raw logs or immediate retry',()=>{
 const e=evidence();e.jobs.jobs[0].steps.push({name:'Compile shader',conclusion:'failure',private_logs:'DO_NOT_COPY'});
 const v=triageRun({report:report({state:'NEEDS_RECOVERY',latest_run:{id:20,status:'completed',conclusion:'failure'}}),...e});
 assert.equal(v.status,'DIAGNOSE_FAILED_STEPS');assert.equal(v.failed_steps[0].step,'Compile shader');
 assert.ok(!JSON.stringify(v).includes('DO_NOT_COPY'));assert.match(v.next_safe_action,/No blind retry/);
});
test('older-source CI is not usable to certify current branch',()=>{
 const v=triageRun({report:report({source_matches_branch_head:false}),...evidence()});
 assert.equal(v.status,'STALE_SOURCE');assert.equal(v.source_matches_head,false);
});
test('GitHub action_required always needs authorized external review',()=>{
 const v=triageRun({report:report({state:'GITHUB_APPROVAL_REQUIRED',latest_run:{id:1,status:'completed',conclusion:'action_required'}}),...evidence()});
 assert.equal(v.status,'EXTERNAL_APPROVAL');
});
test('running Actions job does not imply a free lease or justify duplicate execution',()=>{
 const v=triageRun({report:report({state:'EXTERNAL_RUNNING',latest_run:{id:10,status:'in_progress'}}),...evidence()});
 assert.equal(v.status,'WAIT_FOR_SAME_RUN');assert.equal(v.dispatch_allowed,false);
});
test('missing and truncated API data fail closed',()=>{
 assert.equal(triageRun({report:report(),jobs:null,artifacts:null}).status,'EVIDENCE_UNAVAILABLE');
 const e=evidence();e.jobs.total_count=1001;
 assert.equal(triageRun({report:report(),...e}).status,'EVIDENCE_PARTIAL');
 assert.equal(triageRun({report:report({latest_run:null}),...e}).status,'NO_MATCHING_RUN');
});
test('never triage a legacy issue-producing mission as an executable agent',()=>{
 assert.throws(()=>triageRun({report:report({observation_mode:'recovery_issue'}),...evidence()}),/No autonomous triage/);
});
