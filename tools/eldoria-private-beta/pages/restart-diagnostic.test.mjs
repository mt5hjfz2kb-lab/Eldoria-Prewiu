import test from 'node:test';
import assert from 'node:assert/strict';
import {startLocalRestartDiagnostic,RESTART_KEY} from './restart-diagnostic.mjs';
function harness({query='?eldoria_diag=1',data=new Map(),now=()=>10000,clock=()=>3000}={}){
 const events=new Map();
 const on=(name,handler)=>events.set(name,[...(events.get(name)||[]),handler]);
 const node=()=>({style:{},children:[],append(...v){this.children.push(...v)},
  addEventListener(kind,fn){this[kind]=fn},focus(){},select(){}});
 const doc={visibilityState:'visible',body:node(),createElement:node,
  addEventListener:on};
 const win={location:{search:query},localStorage:{
  getItem:k=>data.get(k)||null,setItem:(k,v)=>data.set(k,String(v))
 },performance:{now:clock},setInterval:()=>1,addEventListener:on,
 navigator:{clipboard:{writeText:async()=>{}}}};
 const emit=(kind,detail={})=>{for(const fn of events.get(kind)||[])fn(detail)};
 return {win,doc,emit,data,now,clock};
}
test('Requires explicit eldoria_diag=1 even if telemetry is separately enabled',()=>{
 const h=harness({query:'?eldoriaBetaTelemetry=1'});
 const result=startLocalRestartDiagnostic({...h,build:'mobile11'});
 assert.equal(result.active,false);
 assert.equal(h.data.size,0);
 assert.equal(h.doc.body.children.length,0);
});
test('Abrupt follow-up boot is a non-causal breadcrumb, without personal information',()=>{
 const h=harness();
 const p=startLocalRestartDiagnostic({...h,build:'mobile11'});
 assert.equal(p.active,true);
 assert.equal(p.getState().observation,'NO_PRIOR_RECORD');
 h.emit('webglcontextlost');h.emit('error',{message:'Name and saved game must NOT be copied'});
 const q=startLocalRestartDiagnostic({...h,build:'mobile11'});
 assert.equal(q.getState().observation,'PRIOR_DOCUMENT_WITHOUT_EXIT_RECORD');
 const report=JSON.parse(q.report());
 assert.equal(report.build,'mobile11');
 assert.equal(report.result,'PRIOR_DOCUMENT_WITHOUT_EXIT_RECORD');
 assert.ok(report.explanation.includes('NO confirma jetsam'));
 assert.equal(q.getState().events.at(-1)[0],'boot');
 assert.ok(q.getState().events.some(e=>e[0]==='webgl_lost'));
 assert.ok(!q.report().includes('Name and saved game'));
});
test('Pagehide normal exit and BFCache are distinct and never claimed as a crash',()=>{
 const h=harness();
 startLocalRestartDiagnostic({...h,build:'mobile11'});
 h.emit('pagehide',{persisted:true});
 h.emit('pageshow',{persisted:true});
 assert.equal(startLocalRestartDiagnostic({...h,build:'mobile11'}).getState().observation,
  'PRIOR_DOCUMENT_WITHOUT_EXIT_RECORD');
 h.emit('pagehide',{persisted:false});
 assert.equal(startLocalRestartDiagnostic({...h,build:'mobile11'}).getState().observation,
  'PREVIOUS_EXIT_RECORDED');
});
test('Has bounded records, uses only enums, and survives unavailable localStorage',()=>{
 const h=harness();
 const p=startLocalRestartDiagnostic({...h,build:'mobile11'});
 for(let i=0;i<100;i++)p.record('heartbeat');
 p.record('name=secret');
 assert.equal(p.getState().events.length,24);
 assert.ok(p.getState().events.every(e=>e.length===3&&e[0]!=='name=secret'));
 assert.ok(h.data.has(RESTART_KEY));
 const bad=harness();
 bad.win.localStorage={getItem(){throw Error('storage blocked')},setItem(){throw Error('storage blocked')}};
 const result=startLocalRestartDiagnostic({...bad,build:'mobile11'});
 assert.equal(result.active,true);
 assert.equal(JSON.parse(result.report()).persisted,false);
});
