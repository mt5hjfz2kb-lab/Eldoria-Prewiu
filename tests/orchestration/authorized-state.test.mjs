import {test} from 'node:test';import assert from 'node:assert/strict';
import {empty,evolve,pending} from '../../tools/eldoria-authorized-orchestration.mjs';
const id=(i)=>'event_'+i;
function step(s,i,type,extra={}){return evolve(s,{id:id(i),job_id:'job_A',type,...extra})}
const prop={owner:'department-code',scope:'tests-only',retry_limit:1};
const prepared=()=>step(step(empty(),1,'PROPOSE',prop),2,'APPROVE',{actor:'owner',authorization:'owner-approved-round-1'});
test('owner approval is mandatory',()=>assert.throws(()=>step(step(empty(),1,'PROPOSE',prop),3,'QUEUE',{actor:'coordinator'}),/UNAUTHORIZED/));
test('failure produces bounded correction, no automatic unapproved dispatch',()=>{
 let s=prepared();s=step(s,3,'QUEUE',{actor:'coordinator'});s=step(s,4,'START',{actor:'runner',run_id:'100'});s=step(s,5,'SUBMIT',{actor:'runner',artifact:'test-100',sha:'a'.repeat(40)});
 s=step(s,6,'REVIEW',{actor:'independent-qa',pass:false,proof:'qa-fail-100'});assert.equal(s.jobs[0].state,'CORRECTING');assert.deepEqual(pending(s)[0].phase,'CORRECTING');
 s=step(s,7,'QUEUE',{actor:'coordinator'});s=step(s,8,'START',{actor:'runner',run_id:'101'});s=step(s,9,'SUBMIT',{actor:'runner',artifact:'test-101',sha:'b'.repeat(40)});
 s=step(s,10,'REVIEW',{actor:'independent-qa',pass:true,proof:'qa-pass-101'});assert.equal(s.jobs[0].state,'ACCEPTED');
 s=step(s,11,'REPORT',{actor:'directorate',report:'report-101'});assert.equal(s.jobs[0].state,'REPORTED');assert.equal(s.jobs[0].attempt,2);
 assert.deepEqual(step(s,11,'REPORT',{actor:'directorate',report:'report-101'}),s);
 assert.throws(()=>step(s,12,'QUEUE',{actor:'coordinator'}),/JOB_NOT_ACTIVE/);
});
test('retry budget is enforced',()=>{
 let s=prepared();for(let i=0;i<2;i++){let n=3+i*4;s=step(s,n,'QUEUE',{actor:'coordinator'});s=step(s,n+1,'START',{actor:'runner',run_id:String(i)});s=step(s,n+2,'SUBMIT',{actor:'runner',artifact:'artifact'+i,sha:'c'.repeat(40)});s=step(s,n+3,'REVIEW',{actor:'independent-qa',pass:false,proof:'fail'+i})}
 assert.equal(s.jobs[0].state,'BLOCKED');assert.throws(()=>step(s,11,'QUEUE',{actor:'coordinator'}),/JOB_NOT_ACTIVE/);
});
test('independent QA and provenance required',()=>{
 let s=prepared();s=step(s,3,'QUEUE',{actor:'coordinator'});s=step(s,4,'START',{actor:'runner',run_id:'100'});
 assert.throws(()=>step(s,5,'SUBMIT',{actor:'runner',artifact:'a',sha:'short'}),/EVIDENCE_REQUIRED/);
 s=step(s,5,'SUBMIT',{actor:'runner',artifact:'a',sha:'a'.repeat(40)});
 assert.throws(()=>step(s,6,'REVIEW',{actor:'runner',pass:true,proof:'false-pass'}),/INDEPENDENT_REVIEW_REQUIRED/);
});
test('event replay collision protection',()=>{let s=step(empty(),1,'PROPOSE',prop);assert.deepEqual(step(s,1,'PROPOSE',prop),s);assert.throws(()=>step(s,1,'PROPOSE',{...prop,owner:'another'}),/EVENT_ID_COLLISION/)});
