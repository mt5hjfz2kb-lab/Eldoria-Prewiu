import test from 'node:test';import assert from 'node:assert/strict';import {evaluate,render} from '../../tools/m16a/evaluate.mjs';
const base=()=>({schema_version:1,departments:[{id:'qa',capabilities:['github_workflow']},{id:'design',capabilities:[]}],jobs:[{id:'M16-A',department:'qa',status:'APPROVED',executor:'github_workflow',acceptance_policy:'m03',depends_on:[],scopes:['tests/m16a/**'],resources:[]}]});
const job=(id)=>({id,department:'qa',status:'APPROVED',executor:'github_workflow',acceptance_policy:'m03',depends_on:[],scopes:[],resources:[]});
test('1 valid job',()=>assert.equal(evaluate(base()).results[0].readiness,'READY'));
test('2 incomplete rejected',()=>{let c=base();delete c.jobs[0].department;assert.equal(evaluate(c).ok,false)});
test('3 unresolved dependency blocked',()=>{let c=base();c.jobs.push({...job('M16-B'),depends_on:[{job_id:'M16-A',required_gate:'TECH'}]});assert.equal(evaluate(c).results[1].readiness,'BLOCKED')});
test('4 certified dependency ready',()=>{let c=base();c.jobs[0].certificate_id='cert-A';c.jobs.push({...job('M16-B'),depends_on:[{job_id:'M16-A',required_gate:'TECH'}]});assert.equal(evaluate(c,{}, {certificates:[{id:'cert-A',status:'ACCEPTED',gates:{TECH:'PASS'},issued_by:'independent-m03'}]}).results[1].readiness,'READY')});
test('5 cycle rejected',()=>{let c=base();c.jobs[0].depends_on=[{job_id:'M16-B',required_gate:'TECH'}];c.jobs.push({...job('M16-B'),depends_on:[{job_id:'M16-A',required_gate:'TECH'}]});assert.equal(evaluate(c).ok,false)});
test('6 duplicate rejected',()=>{let c=base();c.jobs.push({...c.jobs[0]});assert.equal(evaluate(c).ok,false)});
test('7 occupied resources blocked',()=>{let c=base();c.jobs[0].resources=['unity'];assert.equal(evaluate(c,{active:[{id:'other',resources:['unity'],scope:[]}]}).results[0].readiness,'BLOCKED')});
test('8 human handoff',()=>{let c=base();c.jobs[0].executor='human_chat';assert.equal(evaluate(c).results[0].readiness,'HANDOFF')});
test('9 no self-certification',()=>{let c=base();c.jobs[0].certificate_id='self';c.jobs.push({...job('M16-B'),depends_on:[{job_id:'M16-A',required_gate:'TECH'}]});assert.equal(evaluate(c,{}, {certificates:[{id:'self',status:'ACCEPTED',gates:{TECH:'PASS'},issued_by:'M16-B'}]}).results[1].readiness,'BLOCKED')});
test('10 DG report reflects statuses',()=>{let c=base();c.jobs.push({...job('M16-B'),executor:'human_chat'});let r=evaluate(c);assert.equal(r.summary.READY,1);assert.equal(r.summary.HANDOFF,1);assert.match(render(r),/DIRECCION GENERAL/)});

test('11 two independent authorized departments are READY simultaneously',()=>{
 const c=base();c.departments.push({id:'audio',capabilities:['github_workflow']});
 c.jobs.push({...job('M16-B'),department:'audio',scopes:['audio/**'],resources:['audio-cpu']});
 c.jobs[0].resources=['qa-cpu'];
 const result=evaluate(c,{active:[]});
 assert.equal(result.summary.READY,2);
 assert.deepEqual(result.results.map(x=>x.readiness),['READY','READY']);
});
test('12 incompatible resources block only affected job, never unrelated department',()=>{
 const c=base();c.departments.push({id:'audio',capabilities:['github_workflow']});
 c.jobs.push({...job('M16-B'),department:'audio',scopes:['audio/**'],resources:['audio-cpu']});
 c.jobs.push({...job('M16-C'),department:'qa',scopes:['unity/**'],resources:['windows-runner']});
 c.jobs[0].resources=['qa-cpu'];
 const result=evaluate(c,{active:[{id:'external-owner',scope:['unity/**'],resources:['windows-runner']}]});
 assert.equal(result.summary.READY,2);assert.equal(result.summary.BLOCKED,1);
 assert.deepEqual(result.results.map(x=>x.readiness),['READY','READY','BLOCKED']);
 assert.match(result.results[2].reasons.join(' '),/OWNER_CONFLICT:external-owner/);
});
