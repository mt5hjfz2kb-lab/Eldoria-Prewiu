import {test} from 'node:test';
import assert from 'node:assert/strict';
import {summarize,collectLive,markdown,jobTiming,versionStates} from './report.mjs';

const registry={active:[{id:'one',title:'UI',branch:'ui/one',owner:'existing',status:'active',resources:[],phase:'QA_PENDING'}],history:[{id:'old',status:'active',secret:'DO_NOT_COPY_HISTORY'}]};
test('bounded view never reactivates history or labels an untested delivery current',()=>{
  const report=summarize({registry,delivery:{state:'PROCESSING',candidate:{revision:'new'},published:{revision:'old',remote_active_confirmed:true},uploaded:{revision:'new',remote_active_confirmed:false},credential_setup:{private:'DO_NOT_COPY_CREDENTIALS'}}});
  assert.equal(report.active.length,1);assert.equal(report.active[0].live_branch.status,'not_checked');
  assert.equal(report.web.confirmed_revision,'old');assert.equal(report.web.uploaded_activation_confirmed,false);
  assert.equal(report.windows.observation,'not_checked');
  assert.ok(!JSON.stringify(report).includes('DO_NOT_COPY'));assert.ok(!markdown(report).includes('undefined'));
});
test('jobs reveal busy Windows even when every declared resource list is empty',async()=>{
  const github={rest:{repos:{getBranch:async()=>({data:{commit:{sha:'source'}}})},actions:{
    listWorkflowRunsForRepo:async a=>({data:{total_count:1,workflow_runs:a.status?[{id:7,head_branch:'ui/one'}]:[{id:6,head_sha:'source',name:'Native',status:'in_progress',conclusion:null}]}}),
    listJobsForWorkflowRun:async()=>({data:{total_count:2,jobs:[{id:70,name:'native',status:'in_progress',labels:['self-hosted','Windows']},{id:71,name:'docs',status:'in_progress',labels:['ubuntu-latest']}]}})
  }}};
  const live=await collectLive({github,repo:{owner:'o',repo:'r'},active:registry.active});
  const report=summarize({registry,live});
  assert.equal(report.windows.running.length,1);assert.equal(report.active[0].declared_resources.length,0);
  assert.equal(report.active[0].live_branch.checks[0].conclusion,null);
});
test('API failures remain unknown/partial and cannot be interpreted as a free runner',async()=>{
  const no=async()=>{throw Error('offline');};
  const github={rest:{repos:{getBranch:no},actions:{listWorkflowRunsForRepo:no,listJobsForWorkflowRun:no}}};
  const live=await collectLive({github,repo:{owner:'o',repo:'r'},active:registry.active});
  assert.equal(live.windows.observation,'partial');assert.ok(live.windows.errors.length);
  assert.equal(live.branches['ui/one'].status,'unavailable');
});
test('invalid live registry fails rather than producing a misleading empty status',()=>{
  assert.throws(()=>summarize({registry:{history:[]}}),/active array missing/);
});

const at=s=>'2026-10-21T00:00:'+String(s).padStart(2,'0')+'Z';
test('timing preserves unknowns, unfinished and skipped steps instead of inventing zero duration',()=>{
  const t=jobTiming({created_at:at(0),started_at:at(5),completed_at:at(45),status:'completed',steps:[
    {name:'Checkout',status:'completed',conclusion:'success',started_at:at(5),completed_at:at(10)},
    {name:'Build Unity',status:'completed',conclusion:'success',started_at:at(10),completed_at:at(20)},
    {name:'QA tests',status:'completed',conclusion:'success',started_at:at(20),completed_at:at(30)},
    {name:'Upload artifacts',status:'completed',conclusion:'success',started_at:at(30),completed_at:at(35)},
    {name:'Skipped build',status:'completed',conclusion:'skipped',started_at:at(35),completed_at:at(40)},
    {name:'Invalid dates',status:'completed',started_at:at(40),completed_at:at(39)}]});
  assert.equal(t.queue_seconds,5);assert.equal(t.elapsed_seconds,40);
  assert.deepEqual(t.stage_seconds,{preparation:5,build:10,test:10,delivery:5,other:0});
  assert.equal(t.unmeasured_steps,2);
  const unknown=jobTiming({status:'queued'},at(20));
  assert.equal(unknown.queue_seconds,null);assert.equal(unknown.stage_seconds,null);assert.equal(unknown.elapsed_seconds,null);
  assert.equal(jobTiming({status:'queued',created_at:at(5)},at(20)).observed_wait_seconds,15);
  const ongoing=jobTiming({status:'in_progress',started_at:at(5)},at(20));
  assert.equal(ongoing.elapsed_seconds,15);assert.equal(ongoing.duration_final,false);
});
test('version states never promote mismatched QA, an upload, disabled Android or a green HEAD check',()=>{
  const delivery={candidate:{revision:'new'},current_revision_qa:{revision:'old',native_unity_pass:true,windows_gpu_webgl_pass:true},published:{revision:'old',remote_active_confirmed:true},uploaded:{revision:'new',remote_active_confirmed:false}};
  const v=versionStates({head:'new-source',delivery,android:{enabled:false,versionName:'hidden'}});
  assert.equal(v.web_candidate.technical_qa_recorded,false);assert.equal(v.web_available.revision,'old');assert.equal(v.android_channel.version,null);
  delivery.current_revision_qa.revision='new';
  assert.equal(versionStates({delivery}).web_candidate.technical_qa_recorded,true);
  assert.equal(versionStates({}).web_candidate.technical_qa_recorded,false);
});
test('old run timing is kept separate from HEAD checks and requests are bounded per distinct branch',async()=>{
  let calls=0;
  const github={rest:{repos:{getBranch:async()=>({data:{commit:{sha:'current'}}})},actions:{
    listWorkflowRunsForRepo:async a=>({data:{workflow_runs:a.status?[]:[{id:3,name:'Unity native',head_sha:'older',run_attempt:2}]}}),
    listJobsForWorkflowRun:async()=>{calls++;return {data:{total_count:101,jobs:[{id:4,name:'build',status:'completed',started_at:at(5),completed_at:at(20)}]}};}
  }}};
  const live=await collectLive({github,repo:{owner:'o',repo:'r'},active:[...registry.active,...registry.active]});
  const b=live.branches['ui/one'];
  assert.equal(calls,1);assert.equal(b.checks.length,0);assert.equal(b.timing_sample.source_sha,'older');assert.equal(b.timing_sample.status,'partial');assert.equal(b.timing_sample.run_attempt,2);
});
test('delivery definition is allowlisted and absent definitions do not imply completion',()=>{
  const active=[{...registry.active[0],delivery_definition:{deliverable:'Result',evidence:'Tests',handoff:'Commit',private:'DO_NOT_COPY'}}];
  const r=summarize({registry:{active}});
  assert.equal(r.active[0].delivery_definition.deliverable,'Result');assert.ok(!JSON.stringify(r).includes('DO_NOT_COPY'));
  assert.match(markdown(summarize({registry})),/sin definición explícita/);
});
