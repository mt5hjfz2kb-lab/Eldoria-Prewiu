import test from 'node:test';
import assert from 'node:assert/strict';
import {startPilot} from './pilot-bootstrap.mjs';
import {RELEASE,SOURCE} from './client.mjs';

const KEY='eldoria_pages_qa_v1:'+RELEASE;
function createHarness({consent=true}={}){
 const data=new Map(consent?[[KEY+':consent','yes']]:[]);
 const handlers=new Map();
 let nextId=0;
 const storage={
  getItem:key=>data.get(key)??null,
  setItem:(key,value)=>data.set(key,String(value)),
  removeItem:key=>data.delete(key)
 };
 const element=tag=>({
  tagName:tag,style:{},children:[],textContent:'',value:'',
  appendChild(child){this.children.push(child);return child},
  append(...children){this.children.push(...children)},
  remove(){this.removed=true},
  addEventListener(){},
 });
 const body=element('body');
 const doc={
  body,visibilityState:'visible',
  createElement:element,
  getElementById:()=>null,
  addEventListener(){},
 };
 const win={
  localStorage:storage,
  crypto:{randomUUID:()=>('qa-id-'+(++nextId))},
  navigator:{userAgent:'Mozilla/5.0 (iPhone; CPU iPhone OS 18_0)'},
  performance:{now:()=>12500},
  console:{log(){},warn(){}},
  innerWidth:390,innerHeight:844,
  requestAnimationFrame(){},
  setInterval(){return ++nextId},
  clearInterval(){},
  addEventListener(kind,handler){
   const existing=handlers.get(kind)||[];
   existing.push(handler);handlers.set(kind,existing);
  }
 };
 const fetchImpl=async url=>url==='./release-identity.json'
  ? {ok:true,json:async()=>({revision:RELEASE,source_sha:SOURCE})}
  : {status:503}; // Simulate offline; evidence must survive in the queue.
 const emit=(kind,event)=>{
  for(const handler of handlers.get(kind)||[])handler(event);
 };
 const events=()=>JSON.parse(data.get(KEY+':queue')||'[]');
 return {win,doc,fetchImpl,emit,events};
}
const drain=async()=>{for(let i=0;i<6;i++)await new Promise(resolve=>setImmediate(resolve))};

test('BFCache return resumes opted-in measurements without a second session',async()=>{
 const h=createHarness();
 const state=await startPilot(h);await drain();
 assert.equal(state.active,true);
 assert.equal(state.consented,true);
 const initial=h.events();
 assert.ok(initial.some(x=>x.event_name==='session_start'));
 const originalSession=initial.find(x=>x.event_name==='session_start').session_id;

 h.emit('pagehide',{persisted:true});await drain();
 assert.equal(h.events().filter(x=>x.event_name==='visibility_state'&&x.payload.state_code==='background').length,1);
 h.emit('pageshow',{persisted:true});await drain();
 const after=h.events();
 assert.equal(after.filter(x=>x.event_name==='visibility_state'&&x.payload.state_code==='foreground').length,1);
 assert.equal(after.filter(x=>x.event_name==='session_start').length,1,'restoring is not a new session');
 assert.equal(after.at(-1).session_id,originalSession);

 h.emit('pagehide',{persisted:false});await drain();
 const size=h.events().length;
 h.emit('pageshow',{persisted:true});await drain();
 assert.equal(h.events().length,size,'a real exit never restarts telemetry');
});

test('Without consent, BFCache events never create or send telemetry',async()=>{
 const h=createHarness({consent:false});
 const state=await startPilot(h);
 assert.equal(state.active,true);
 assert.equal(state.consented,false);
 h.emit('pagehide',{persisted:true});
 h.emit('pageshow',{persisted:true});
 await drain();
 assert.equal(h.events().length,0);
});
