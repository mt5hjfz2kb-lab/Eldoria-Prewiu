import test from 'node:test';
import assert from 'node:assert/strict';
import {randomUUID} from 'node:crypto';
import {makeBetaTelemetry,BETA_EVENTS} from './beta-telemetry-client.mjs';
function localStorageMock(){const m=new Map();return {getItem(k){return m.get(k)||null},setItem(k,v){m.set(k,v)},removeItem(k){m.delete(k)},contents:m}}
function client(send=async()=>({status:202}),storage=localStorageMock()){
 let clock=1000;
 const c=makeBetaTelemetry({send,storage,uuid:randomUUID,now:()=>clock,deviceType:'ios_safari'});
 return {c,advance:n=>{clock+=n},storage};
}
test('sin consentimiento no encola ni transmite nada',async()=>{
 let sent=0;const {c}=client(async()=>{sent++;return {status:202}});
 const r=await c.track('game_ready');assert.equal(r.accepted,false);assert.equal(c.pending(),0);assert.equal(sent,0);
});
test('tras aceptacion real transmite identificador efimero sin identidad ni credencial',async()=>{
 const rows=[];const {c,advance}=client(async e=>{rows.push(e);return{status:202}});
 c.setConsent(true);advance(3500);const r=await c.track('march_sent',{bastion:2,screen:'world'});
 assert.equal(r.sent,1);assert.equal(c.pending(),0);assert.equal(rows[0].elapsed_ms,3500);
 assert.equal(rows[0].consent_given,true);assert.equal(rows[0].device_type,'ios_safari');
 assert.ok(!('tester_id' in rows[0]));assert.ok(!('email' in rows[0]));
 assert.ok(!('build_version' in rows[0]));assert.ok(!('apikey' in rows[0]));
});
test('sin red mantiene cola y reintenta sin duplicar evento',async()=>{
 const storage=localStorageMock();const sends=[];
 const {c}=client(async e=>{sends.push(e);throw new Error('offline')},storage);
 c.setConsent(true);await c.track('game_ready');assert.equal(c.pending(),1);
 const d=client(async e=>{sends.push(e);return{status:202}},storage).c;
 d.setConsent(true);assert.equal((await d.flush()).sent,1);assert.equal(d.pending(),0);
 assert.equal(sends[0].client_event_id,sends[1].client_event_id);
});
test('retirar consentimiento borra cola y bloquea nuevos envios',async()=>{
 const {c}=client(async()=>({status:502}));
 c.setConsent(true);await c.track('blocked',{reason_code:'fallo_tecnico'});
 assert.equal(c.pending(),1);c.setConsent(false);assert.equal(c.pending(),0);
 assert.equal((await c.track('game_ready')).accepted,false);
});
test('eventos no reconocidos se rechazan',async()=>{
 const {c}=client();c.setConsent(true);
 assert.equal((await c.track('credit_card_number')).accepted,false);
 assert.equal(c.pending(),0);
 assert.equal(BETA_EVENTS.length,67);
});
test('solo un envio de HTTP 202 permite quitar evento',async()=>{
 const {c}=client(async()=>({status:200}));c.setConsent(true);
 const r=await c.track('game_ready');assert.equal(r.sent,0);assert.equal(c.pending(),1);
});
test('sin almacenamiento disponible no inventa datos persistidos',async()=>{
 const storage={getItem(){throw Error('blocked')},setItem(){throw Error('blocked')},removeItem(){throw Error('blocked')}};
 const {c}=client(async()=>({status:202}),storage);c.setConsent(true);
 const r=await c.track('game_ready');assert.equal(r.accepted,true);
 assert.equal(r.sent,1);assert.equal(c.pending(),0);assert.equal(r.durable,false);
});

test('dos eventos concurrentes no se pierden durante un envio lento',async()=>{
  const records=[];
  let unblock;
  const firstSend=new Promise(resolve=>{unblock=resolve});
  let count=0;
  const {c}=client(async event=>{records.push(event);if(++count===1)await firstSend;return {status:202}});
  c.setConsent(true);
  const p1=c.track('game_ready');
  await new Promise(resolve=>setImmediate(resolve));
  const p2=c.track('march_sent');
  unblock();
  await Promise.all([p1,p2]);
  assert.equal(c.pending(),0);
  assert.deepEqual(records.map(x=>x.event_name),['game_ready','march_sent']);
});
test('sin almacenamiento y sin red no finge persistencia',async()=>{
 const storage={getItem(){throw Error('blocked')},setItem(){throw Error('blocked')},removeItem(){throw Error('blocked')}};
 const {c}=client(async()=>{throw new Error('offline')},storage);
 c.setConsent(true);
 const r=await c.track('game_ready');
 assert.equal(r.sent,0);assert.equal(r.durable,false);assert.equal(c.pending(),1);
});
test('cliente rechaza sin guardar PII o texto libre antes del envio',async()=>{
 const arr=[];const {c}=client(async event=>{arr.push(event);return {status:202}});c.setConsent(true);
 for(const payload of [{email:'a@example.com'},{nick:'elena'},{screen:'pantalla que contiene un nombre'}]){
  const r=await c.track('screen_viewed',payload);
  assert.equal(r.accepted,false);assert.equal(r.reason,'invalid_payload');
 }
 assert.equal(c.pending(),0);assert.equal(arr.length,0);
});
test('un 422 del servidor descarta solo el evento irreparable y no bloquea el siguiente',async()=>{
 const seen=[];const {c}=client(async ev=>{seen.push(ev.event_name);return {status:ev.event_name==='battle_resolved'?422:202}});
 c.setConsent(true);
 const bad=await c.track('battle_resolved',{outcome_code:'me_name'});
 assert.equal(bad.accepted,true);assert.equal(bad.droppedInvalid,1);assert.equal(c.pending(),0);
 const good=await c.track('game_ready');assert.equal(good.sent,1);
 assert.deepEqual(seen,['battle_resolved','game_ready']);
});
test('cola al limite no descarta silenciosamente eventos anteriores',async()=>{
 const {c}=client(async()=>{throw Error('no network')});c.setConsent(true);
 for(let i=0;i<200;i++){
  const r=await c.track('session_heartbeat',{count:i});
  assert.equal(r.accepted,true);
 }
 const extra=await c.track('session_heartbeat',{count:201});
 assert.equal(extra.accepted,false);assert.equal(extra.reason,'queue_full');assert.equal(c.pending(),200);
});
