import fs from 'node:fs';
import assert from 'node:assert/strict';
const workflow=fs.readFileSync(new URL('../../.github/workflows/ui-progression-cert.yml',import.meta.url),'utf8');
const marker='      - name: Reuse only an already completed full certificate for this exact commit';
const start=workflow.indexOf(marker);
assert.ok(start>=0,'Exact certificate guard is present');
const end=workflow.indexOf('      - name: Verify completed exact shared UI and Aldric native certificate',start);
assert.ok(end>start,'Historical fallback remains present');
const block=workflow.slice(start,end);
const body=block.slice(block.indexOf('          script: |\n')+'          script: |\n'.length);
const script=body.split('\n').filter(line=>line.startsWith('            ')).map(line=>line.slice(12)).join('\n');
const fn=new (Object.getPrototypeOf(async function(){}).constructor)('github','context','core',script);
const sha='a'.repeat(40),branch='fix/player-feedback-context-ui-20261030',id=42;
const names=script.match(/const required=\[([\s\S]*?)\];/)[1];
const required=Function('return ['+names+']')();
function fixture(){
 const run={id,head_sha:sha,head_branch:branch,event:'push',status:'completed',conclusion:'success',path:'.github/workflows/ui-progression-cert.yml'};
 const jobs=[{name:'certify',status:'completed',conclusion:'success',steps:required.map(name=>({name,status:'completed',conclusion:'success'}))}];
 const artifactNames=['eldoria-pr136-performance-'+sha,'eldoria-pr136-candidate-'+sha,'eldoria-ui-progression-cert-'+sha,'eldoria-aldric-shared-native-'+sha,'eldoria-v41-native-'+id,'eldoria-dialogue-v4-native-'+id,'eldoria-three-marches-native-'+id,'eldoria-retention-evolution-native-'+id,'eldoria-aldric-preflight-evidence-'+id];
 return {run,jobs,artifacts:artifactNames.map(name=>({name,expired:false,size_in_bytes:1000,workflow_run:{id},digest:'sha256:'+'b'.repeat(64)})),context:{eventName:'pull_request',repo:{owner:'o',repo:'r'},payload:{pull_request:{number:136,head:{ref:branch,sha,repo:{full_name:'o/r'}}}}}};
}
let count=0;
async function test(label,mutate,expected){const f=fixture();mutate(f);const outputs={};const core={setOutput:(k,v)=>outputs[k]=v,info:()=>{},summary:{addRaw(){return this},async write(){}}};const github={rest:{actions:{listWorkflowRuns:async()=>({data:{workflow_runs:[f.run]}}),listJobsForWorkflowRun:async()=>({data:{jobs:f.jobs}}),listWorkflowRunArtifacts:async()=>({data:{artifacts:f.artifacts}})}}};await fn(github,f.context,core);assert.equal(outputs.reused,expected?'true':'false',label);count++;}
await test('exact complete certificate',()=>{},true);
await test('same exact main commit',f=>{f.context={eventName:'push',ref:'refs/heads/main',sha,payload:{},repo:{owner:'o',repo:'r'}}},true);
await test('candidate push must execute native tests',f=>{f.context={eventName:'push',ref:'refs/heads/'+branch,sha,payload:{},repo:{owner:'o',repo:'r'}}},false);
await test('different source',f=>f.run.head_sha='c'.repeat(40),false);
await test('wrong branch',f=>f.run.head_branch='other',false);
await test('wrong workflow',f=>f.run.path='.github/workflows/other.yml',false);
await test('unbound PR run cannot prove canonical package',f=>f.run.event='pull_request',false);
await test('full canonical PR native certificate',f=>{f.run.event='pull_request';f.run.head_repository={full_name:'o/r'};f.run.pull_requests=[{number:136,head:{ref:branch}}]},true);
await test('foreign PR certificate',f=>{f.run.event='pull_request';f.run.head_repository={full_name:'foreign/r'};f.run.pull_requests=[{number:136,head:{ref:branch}}]},false);
await test('other PR certificate',f=>{f.run.event='pull_request';f.run.head_repository={full_name:'o/r'};f.run.pull_requests=[{number:137,head:{ref:branch}}]},false);
await test('canonical PR skipped build is insufficient',f=>{f.run.event='pull_request';f.run.head_repository={full_name:'o/r'};f.run.pull_requests=[{number:136,head:{ref:branch}}];f.jobs[0].steps.find(s=>s.name.startsWith('Compile canonical playable')).conclusion='skipped'},false);
assert.equal((workflow.match(/github.event.pull_request.number == 136 && github.event.pull_request.head.ref/g)||[]).length,4,'Both native performance/build and their uploads execute for canonical PR136');
await test('incomplete run',f=>f.run.status='in_progress',false);
await test('failed run',f=>f.run.conclusion='failure',false);
await test('native job absent',f=>f.jobs=[],false);
await test('reused/skipped native job',f=>f.jobs[0].conclusion='skipped',false);
await test('missing exact compiled commit identity guard',f=>f.jobs[0].steps=f.jobs[0].steps.filter(s=>s.name!=='Verify exact requested commit before any native execution'),false);
await test('missing PlayMode',f=>f.jobs[0].steps=f.jobs[0].steps.filter(s=>!s.name.startsWith('PlayMode')),false);
await test('skipped player build',f=>f.jobs[0].steps.find(s=>s.name.startsWith('Compile canonical playable')).conclusion='skipped',false);
await test('performance not executed',f=>f.jobs[0].steps.find(s=>s.name.startsWith('Compare reconciled')).conclusion='skipped',false);
await test('missing artifact',f=>f.artifacts.pop(),false);
await test('expired artifact',f=>f.artifacts[0].expired=true,false);
await test('artifact from other execution',f=>f.artifacts[0].workflow_run.id=7,false);
await test('unhashed artifact',f=>f.artifacts[0].digest=null,false);
await test('wrong PR',f=>f.context.payload.pull_request.number=137,false);
await test('foreign repository',f=>f.context.payload.pull_request.head.repo.full_name='foreign/repo',false);
function combatFixture(f){
 const combat='feature/eldoria-v41-contracts-20261030';
 f.run.head_branch=combat;f.run.event='pull_request';
 f.run.head_repository={full_name:'o/r'};f.run.pull_requests=[{number:134,head:{ref:combat}}];
 f.context.payload.pull_request.number=134;f.context.payload.pull_request.head.ref=combat;
}
await test('complete exact combat certificate',combatFixture,true);
await test('same exact combat on main',f=>{combatFixture(f);f.context={eventName:'push',ref:'refs/heads/main',sha,payload:{},repo:{owner:'o',repo:'r'}}},true);
await test('combat different source rejected',f=>{combatFixture(f);f.run.head_sha='c'.repeat(40)},false);
await test('combat foreign repository rejected',f=>{combatFixture(f);f.run.head_repository.full_name='foreign/r'},false);
await test('combat wrong PR rejected',f=>{combatFixture(f);f.run.pull_requests[0].number=136},false);
await test('combat skipped retention rejected',f=>{combatFixture(f);f.jobs[0].steps.find(s=>s.name.startsWith('Verify retention')).conclusion='skipped'},false);
await test('combat missing dialogue evidence rejected',f=>{combatFixture(f);f.artifacts=f.artifacts.filter(a=>!a.name.startsWith('eldoria-dialogue-'))},false);
await test('combat skipped build rejected',f=>{combatFixture(f);f.jobs[0].steps.find(s=>s.name.startsWith('Compile canonical playable')).conclusion='skipped'},false);
console.log(count+' exact-certificate positive/negative checks PASS');

assert.equal((workflow.match(/name: eldoria-(?:pr136-performance|pr136-candidate|ui-progression-cert|aldric-shared-native)-\$\{\{ github.event.pull_request.head.sha \|\| github.sha \}\}/g)||[]).length,4,'All source-bound artifacts use the exact checked-out head, not the synthetic PR merge SHA');

const nativeJob=workflow.slice(workflow.indexOf('  certify:\n'));
assert.ok(nativeJob.includes('ref: ${{ github.event.pull_request.head.sha || github.sha }}\n          clean: false'),'Native checkout pins the requested PR head');
assert.ok(!nativeJob.includes('$env:GITHUB_SHA'),'Native source recovery never switches back to the synthetic merge SHA');
assert.ok(nativeJob.indexOf('Verify exact requested commit before any native execution')<nativeJob.indexOf('Compile the exact Unity candidate before preparing unchanged art resources'),'Exact identity is checked before Unity starts');
