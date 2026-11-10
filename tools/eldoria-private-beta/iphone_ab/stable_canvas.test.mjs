import test from 'node:test';
import assert from 'node:assert/strict';
import {createRequire} from 'node:module';
const {install}=createRequire(import.meta.url)('./stable_canvas.js');
function fixture(){
 let box={width:390,height:844},pending,resize,w=300,h=150,writes=0;
 const canvas={getBoundingClientRect:()=>box,get width(){return w},set width(x){writes++;w=x},get height(){return h},set height(x){writes++;h=x}};
 const win={devicePixelRatio:3,setTimeout:f=>(pending=f,1),clearTimeout:()=>{pending=null},addEventListener:(_,f)=>{resize=f},removeEventListener:()=>{resize=null}};
 return {canvas,win,size:(width,height)=>{box={width,height};resize?.();pending?.()},writes:()=>writes};
}
test('keyboard/toolbar height changes do not repeatedly allocate render targets',()=>{
 const f=fixture(),c=install(f.canvas,f.win);const initial=f.writes();
 for(let i=0;i<100;i++)f.size(390,i%2?844:340);
 assert.equal(f.writes(),initial);assert.equal(f.canvas.width,585);assert.equal(f.canvas.height,1266);assert.equal(c.changes.length,1);
});
test('rotation changes resolution once and preserves the original pixel ratio cap',()=>{
 const f=fixture(),c=install(f.canvas,f.win);f.size(844,390);f.size(844,390);
 assert.equal(f.canvas.width,1266);assert.equal(f.canvas.height,585);assert.equal(c.changes.length,2);assert.equal(f.writes(),4);
});
test('disposal removes resize work',()=>{
 const f=fixture(),c=install(f.canvas,f.win);c.dispose();f.size(844,390);assert.equal(c.changes.length,1);
});
