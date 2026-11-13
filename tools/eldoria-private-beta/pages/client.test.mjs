import test from 'node:test';
import assert from 'node:assert/strict';
import {randomUUID} from 'node:crypto';
import {createTelemetry,sendToQA,RELEASE,SOURCE,API,PUBLISHABLE} from './client.mjs';
const storage=()=>{const m=new Map();return {getItem:k=>m.get(k)??null,setItem:(k,v)=>m.set(k,v),removeItem:k=>m.delete(k),m}};
const create=(send=async()=>({status:202}),s=storage())=>{
 let clock=100;const c=createTelemetry({send,storage:s,uuid:randomUUID,clock:()=>clock,device:'ios_safari'});
 return {c,s,tick:d=>{clock+=d}};
};
test('no consent = no network and no durable browser pseudonym',async()=>{
 let calls=0;let {c,s}=create(async()=>{calls++;return {status:202}});
 assert.equal((await c.track('session_start')).accepted,false);
 assert.equal(c.testerId(),null);assert.equal(s.m.size,0);assert.equal(calls,0);
});
test('opt-in creates only anonymous IDs; all five Bastion levels have numeric contract',async()=>{
 const saved=[];let {c,tick}=create(async e=>{saved.push(e);return {status:202}});
 c.setConsent(true);
 for(let b=1;b<=5;b++){tick(1000);let r=await c.track('bastion_reached',{bastion:b});assert.equal(r.sent,1)}
 await c.track('end_of_slice',{bastion:5});
 assert.deepEqual(saved.filter(x=>x.event_name==='bastion_reached').map(x=>x.payload.bastion),[1,2,3,4,5]);
 assert.ok(saved.every(x=>x.consent_given&&x.device_type==='ios_safari'));
 assert.ok(saved.every(x=>!('playerName' in x)&&!('email' in x)&&!('save' in x)));
 assert.equal(RELEASE,'native325-mobile11-building-anchor');
 assert.equal(SOURCE,'8dc875430241a49b4d417782dfeab2d4c7a4f315');
});
test('Bastion VI, free text, names and false end are discarded before local persistence',async()=>{
 let {c}=create();c.setConsent(true);
 for(const [name,payload] of [
  ['bastion_reached',{bastion:6}],['end_of_slice',{bastion:4}],
  ['bastion_reached',{bastion:0}],['game_ready',{player_name:'Cristian'}],
  ['game_ready',{save:'entire PlayerState'}],['game_ready',{comment:'unrestricted text'}]
 ])assert.equal((await c.track(name,payload)).accepted,false);
 assert.equal(c.pending(),0);
});
test('offline queue survives reload and uses the same event UUID for retry',async()=>{
 const s=storage(),sent=[];
 let {c}=create(async e=>{sent.push(e);throw Error('offline')},s);
 c.setConsent(true);await c.track('game_ready');
 const before=c.testerId();assert.equal(c.pending(),1);
 let resumed=create(async e=>{sent.push(e);return {status:202}},s).c;
 resumed.setConsent(true);assert.equal(resumed.testerId(),before);
 assert.equal((await resumed.flush()).sent,1);
 assert.equal(resumed.pending(),0);
 assert.equal(sent[0].client_event_id,sent[1].client_event_id);
});
test('withdrawal clears all queued anonymous data and disables transmission',async()=>{
 let {c,s}=create(async()=>({status:503}));c.setConsent(true);
 await c.track('session_start');assert.equal(c.pending(),1);
 c.setConsent(false);assert.equal(c.pending(),0);assert.equal(c.testerId(),null);
 assert.equal((await c.track('game_ready')).reason,'no_consent');
 assert.equal([...s.m.keys()].length,0);
});
test('public browser client sends to Supabase Edge QA without private credentials',async()=>{
 let called;const fake=async(url,opts)=>{called={url,opts};return {status:202}};
 const r=await sendToQA({client_event_id:randomUUID()},fake);
 assert.equal(r.status,202);assert.equal(called.url,API);
 assert.equal(called.opts.headers.apikey,PUBLISHABLE);
 assert.equal(called.opts.credentials,'omit');
 assert.equal(new Request(called.url,called.opts).keepalive,true);
 assert.ok(!JSON.stringify(called).includes('service_role'));
 assert.ok(!JSON.stringify(called).includes('CLOUDFLARE'));
});

test('oversized direct transport cannot consume the unload keepalive budget',async()=>{
 let calls=0;
 await assert.rejects(sendToQA({payload:'x'.repeat(5000)},async()=>{calls++;return {status:202}}),/transport bound/);
 assert.equal(calls,0);
});
