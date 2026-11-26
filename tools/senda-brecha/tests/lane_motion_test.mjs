import assert from 'node:assert/strict';
import {stepLane} from '../visual-3d/lane-motion.mjs';
const limit=2.12;
for(const hz of [15,30,60,120]){
 let x=0,v=0,t=0;while(t<.3-1e-10){const dt=Math.min(1/hz,.3-t),old=x;[x,v]=stepLane(x,v,limit,dt,limit);assert.ok(x>=old-1e-8&&x<=limit);t+=dt;}
 assert.ok(x>2.08&&v>=0&&v<.5);
}
let [x,v]=stepLane(0,0,limit,.075,limit);
const initial=[x,v];[x,v]=stepLane(x,v,-limit,0,limit);assert.deepEqual([x,v],initial);
for(let i=0;i<60;i++){[x,v]=stepLane(x,v,-limit,1/120,limit);assert.ok(Math.abs(x)<=limit);}
assert.ok(x< -2.08);
let a=stepLane(0,0,limit,.20,limit),b=[0,0];for(let i=0;i<12;i++)b=stepLane(...b,limit,1/60,limit);
assert.ok(Math.abs(a[0]-b[0])<1e-9&&Math.abs(a[1]-b[1])<1e-9);
console.log('SENDA_LANE_MOTION_PASS 15/30/60/120Hz, reversal continuity, bounded, frame invariance');
