import test from 'node:test';
import assert from 'node:assert/strict';
import {randomUUID} from 'node:crypto';
import {Script} from 'node:vm';
import {OBSERVER_JS} from './observer-source.mjs';

function createHarness({testerId=randomUUID(),allow=true,storage=new Map(),sendStatus=202}={}){
 const rows=[],listeners=new Map(),logs=[];
 const localStorage={
  getItem:k=>storage.get(k)??null,
  setItem:(k,v)=>storage.set(k,String(v)),
  removeItem:k=>storage.delete(k)
 };
 const c={
  innerWidth:390,innerHeight:844,
  navigator:{userAgent:'Mozilla/5.0 iPhone Safari'},
  performance:{now:()=>15000},
  crypto:{randomUUID},
  localStorage,
  document:{visibilityState:'visible',addEventListener:(name,fn)=>listeners.set(name,fn)},
  setInterval(){return 1},
  requestAnimationFrame(){},
  addEventListener:(name,fn)=>listeners.set(name,fn),
  console:{log:(...x)=>logs.push(x),warn:(...x)=>logs.push(x)},
  Response,Blob,
  async fetch(url,options={}){
   if(url==='/beta/session')
    return allow?new Response(JSON.stringify({tester_id:testerId,release:'native325-mobile11-building-anchor',consent:true}),
       {status:200,headers:{'Content-Type':'application/json'}}):new Response('denied',{status:403});
   assert.equal(url,'/beta/events');
   const e=JSON.parse(options.body);rows.push(e);
   return new Response('',{status:sendStatus});
  }
 };
 c.window=c;
 new Script(OBSERVER_JS).runInNewContext(c);
 const tick=async()=>{await new Promise(resolve=>setImmediate(resolve));await new Promise(resolve=>setImmediate(resolve));};
 return {c,rows,logs,listeners,storage,tick};
}

test('no authenticated consented invitation means no browser telemetry',async()=>{
 const h=createHarness({allow:false});
 await h.tick();
 h.c.console.log('ELDORIA_BETA_SIGNAL|bastion_reached|bastion=4');
 await h.tick();
 assert.equal(h.c.__eldoriaBetaObserver.consented,false);
 assert.equal(h.rows.length,0);
 assert.equal(h.storage.size,0);
});
test('explicitly consented invitation emits anonymous browser and real numeric Bastion I-V milestones',async()=>{
 const h=createHarness();
 await h.tick();
 assert.ok(h.rows.some(x=>x.event_name==='session_start'));
 assert.ok(h.rows.some(x=>x.event_name==='orientation_state'));
 const old=h.rows.length;
 for(const level of [1,2,3,4,5])
  h.c.console.log('ELDORIA_BETA_SIGNAL|bastion_reached|bastion='+level);
 h.c.console.warn('ELDORIA_BETA_SIGNAL|end_of_slice|bastion=5');
 h.c.console.log('ELDORIA_BETA_SIGNAL|build_completed|building_code=sawmill|level=2');
 h.c.console.log('ELDORIA_BETA_SIGNAL|march_sent|march_slots_used=1');
 h.c.console.log('ELDORIA_BETA_SIGNAL|battle_resolved|outcome_code=win');
 await h.tick();
 assert.ok(h.rows.length>old);
 assert.deepEqual(h.rows.filter(x=>x.event_name==='bastion_reached').map(x=>x.payload.bastion),[1,2,3,4,5]);
 assert.ok(h.rows.some(x=>x.event_name==='end_of_slice'&&x.payload.bastion===5));
 assert.ok(h.rows.some(x=>x.event_name==='march_sent'&&x.payload.march_slots_used===1));
 assert.ok(h.rows.every(x=>x.consent_given===true&&x.device_type==='ios_safari'));
 assert.ok(h.rows.every(x=>!('tester_id' in x)&&!('player_name' in x)&&!('save' in x)));
});
test('out-of-scope level VI or free text cannot produce analytics',async()=>{
 const h=createHarness();await h.tick();const first=h.rows.length;
 for(const line of [
  'ELDORIA_BETA_SIGNAL|bastion_reached|bastion=6',
  'ELDORIA_BETA_SIGNAL|end_of_slice|bastion=4',
  'ELDORIA_BETA_SIGNAL|bastion_reached|bastion=2|player_name=Someone',
  'VALORIA_PLAYTEST_CHUNK 4 0 2 bmlja25hbWU=',
  'ELDORIA_BETA_SIGNAL|unknown_event|bastion=3'
 ])h.c.console.log(line);
 await h.tick();
 assert.equal(h.rows.length,first);
});
test('a shared browser does not send player A stored events under player B invitation',async()=>{
 const storage=new Map(),idA=randomUUID(),idB=randomUUID();
 const a=createHarness({testerId:idA,storage,sendStatus:503});await a.tick();
 a.c.console.log('ELDORIA_BETA_SIGNAL|bastion_reached|bastion=2');await a.tick();
 const countA=[...storage.keys()].filter(x=>x.includes(idA));
 assert.ok(countA.length>0,'A has its own durable queue and seen flag');
 const b=createHarness({testerId:idB,storage});await b.tick();
 assert.ok(b.rows.every(x=>x.event_name!=='bastion_reached'),'no A gameplay data sent by B');
 assert.ok([...storage.keys()].some(x=>x.includes(idA)));
 assert.ok([...storage.keys()].some(x=>x.includes(idB)));
});
test('return visits are scoped to the same anonymous invitation, not an unrelated person',async()=>{
 const storage=new Map(),id=randomUUID();
 const first=createHarness({testerId:id,storage});await first.tick();
 const second=createHarness({testerId:id,storage});await second.tick();
 assert.ok(second.rows.some(x=>x.event_name==='return_visit'));
 const third=createHarness({testerId:randomUUID(),storage});await third.tick();
 assert.equal(third.rows.some(x=>x.event_name==='return_visit'),false);
});