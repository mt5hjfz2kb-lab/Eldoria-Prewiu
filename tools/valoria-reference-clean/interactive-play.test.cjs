'use strict';
const assert=require('node:assert/strict');
const {validate}=require('./interactive-play.cjs');
const base={session:'test',seq:1,at:new Date().toISOString(),type:'click',x:40,y:40};
const check=c=>validate(c,'test',0,{width:844,height:390});
assert.equal(check(base).type,'click');
for(const patch of [{session:'other'},{seq:0},{at:'2020-01-01'},{x:900},{y:-1},{type:'eval'},{type:'type',text:'name\ncommand'},{type:'key',key:'F12'},{type:'resize',width:1,height:1},{type:'wheel',delta:999999},{type:'drag',toX:-1,toY:10}])assert.throws(()=>check({...base,...patch}));
for(const patch of [{type:'observe'},{type:'finish'},{type:'type',text:'Neral'},{type:'resize',width:390,height:844},{type:'drag',toX:100,toY:120},{type:'key',key:'Enter'}])assert.doesNotThrow(()=>check({...base,...patch}));
console.log('Bounded manual inputs: valid player actions accepted; stale, arbitrary and out-of-view commands rejected.');
