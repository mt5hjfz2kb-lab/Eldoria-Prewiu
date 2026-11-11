'use strict';
const assert=require('node:assert/strict');
const {install,KEY}=require('./postready_profile.js');
function target(){
 const events={};
 return {addEventListener(type,fn){(events[type]??=[]).push(fn);},
 removeEventListener(type,fn){events[type]=(events[type]||[]).filter(x=>x!==fn);},
 emit(type,data={}){for(const fn of events[type]||[])fn(data);}};
}
function env(query='?eldoria_p0=1'){
 const storage=new Map(),doc=Object.assign(target(),{visibilityState:'visible',
 getElementById(){return {width:600,height:1000};}});
 const original=()=>{};
 const win=Object.assign(target(),{
 location:{search:query},
 localStorage:{getItem:k=>storage.get(k)||null,setItem:(k,v)=>storage.set(k,v)},
 console:{log:original,warn:original,error:original},document:doc,
 innerWidth:400,innerHeight:700,
 performance:{getEntriesByType:()=>[{type:'navigate'}]}
 });
 return {win,doc,storage,original};
}
{
 const {win,storage}=env('?other=1');const r=install(win);
 assert.equal(r.active,false);assert.equal(storage.size,0);
}
{
 const {win,storage,original}=env();const a=install(win);
 assert.equal(a.active,true);
 win.console.log('ELDORIA_MEMORY_PHASE phase=prefab-before allocated=3');
 win.console.error('private nickname');
 assert.equal(storage.get(KEY).includes('prefab-before'),true);
 assert.equal(storage.get(KEY).includes('private nickname'),false);
 win.emit('resize');win.innerWidth=401;win.emit('resize');
 const data=JSON.parse(storage.get(KEY));
 assert.equal(data.events.filter(x=>x[1]==='resize').length,1);
 const wrapped=win.console.log;a.dispose();
 assert.notEqual(wrapped,original);assert.equal(win.console.log,original);
 const b=install(win);
 assert.equal(b.report().includes('previousUnclosed=true'),true);
 win.emit('pagehide',{persisted:false});b.dispose();
 const c=install(win);assert.equal(c.report().includes('previousUnclosed=false'),true);c.dispose();
}
{
 const {win,storage}=env();const a=install(win);
 for(let i=0;i<100;i++)a.mark('phase',String(i));
 const result=JSON.parse(storage.get(KEY));
 assert.equal(result.events.length,48);
 assert.equal(result.events.at(-1)[2],'99');
 a.dispose();
}
console.log('P0_POSTREADY_PROFILE_TESTS_PASS three suites');
