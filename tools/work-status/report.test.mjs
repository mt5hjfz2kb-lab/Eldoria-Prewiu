import {test} from 'node:test';
import assert from 'node:assert/strict';
import {summarize,collectLive,markdown} from './report.mjs';

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
