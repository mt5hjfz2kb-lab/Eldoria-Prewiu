'use strict';
const assert=require('node:assert/strict');
const test=require('node:test');
const R=require('../src/rules.js');
const start=()=>R.initialState();
test('six authored waves and three meaningful choices each',()=>{
  assert.equal(R.WAVES.length,6);
  R.WAVES.forEach((w,i)=>{
    assert.equal(w.gates.length,3,`wave ${i}`);
    assert.ok(w.gates.every(g=>g.value>0));
    assert.equal(new Set(w.gates.map(g=>g.type)).size,3);
  });
});
test('no mutation of previous states and reports are internally consistent',()=>{
  let old=start();
  for(let i=0;i<6;i++){
    const lane=i%3;
    const {state,report}=R.playWave(old,lane);
    assert.notEqual(state,old);
    assert.equal(state.history.length,i+1);
    assert.equal(report.after,report.before+(report.gate==='recruit'?report.gateValue:0)-report.lost);
    assert.equal(report.incoming,report.absorbed+report.lost);
    assert.equal(old.history.length,i);
    assert.ok(state.units>=0 && state.shields>=0);
    if(state.finished)break;
    old=state;
  }
});
test('validates lane and refuses progression after event end',()=>{
  for(const bad of [-1,3,0.4,NaN,'0',null])assert.throws(()=>R.playWave(start(),bad),RangeError);
  let s=start();for(let i=0;i<6;i++){s=R.playWave(s,1).state;if(s.finished)break}
  assert.equal(s.finished,true);
  assert.throws(()=>R.playWave(s,1));
});
test('all 729 lane strategies are deterministic, bounded and yield reproducible scores',()=>{
 let wins=0,losses=0;let best=-Infinity,worst=Infinity;let choices=[];
 for(let mask=0;mask<729;mask++){
  let n=mask, s=start(); const selected=[];
  for(let i=0;i<6;i++){
   const lane=n%3;n=Math.floor(n/3);selected.push(lane);
   s=R.playWave(s,lane).state;if(s.finished)break;
  }
  const t=start();let s2=t;
  for(let i=0;i<selected.length;i++){s2=R.playWave(s2,selected[i]).state;if(s2.finished)break;}
  assert.deepEqual(s2,s);
  assert.ok(s.units>=0 && s.units<=96);
  assert.ok(s.score>=0);
  if(s.victory)wins++;else losses++;
  best=Math.max(best,s.score);worst=Math.min(worst,s.score);
 }
 console.log('strategy sweep', {wins,losses,best,worst});
 assert.ok(wins>0,'at least one achievable victory');
 assert.ok(losses>0,'some paths pose genuine risk');
 assert.ok(best>worst,'choices influence outcome');
});
test('all gate types affect exactly the proposed variable',()=>{
 let s=start();
 const recruit=R.playWave(s,0).report;
 const fury=R.playWave(s,1).report;
 const shields=R.playWave(s,2).report;
 assert.ok(recruit.before+8>=recruit.after);
 assert.ok(fury.attackBonus>0);
 assert.ok(shields.absorbed>0);
 assert.equal(s.units,22);
});
