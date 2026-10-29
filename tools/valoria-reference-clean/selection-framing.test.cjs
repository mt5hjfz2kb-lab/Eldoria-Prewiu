'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const {stableSelectionView} = require('./playtest_web.cjs');
const state = () => ({selected:'animated-creature', inWorld:true, width:540, height:960,
  narrative:false, camera:{x:925,y:109,z:-109,zoom:64},
  framing:{pass:true, projected:[.37,.30,.62,.45], visible:[.015,.23,.985,.79]},
  controls:{primary:{x:270,y:266,enabled:true}}});
test('animated bounds and HUD text can vary while fully contained and actionable', () => {
  const a=state(), b=state(); b.framing.projected=[.375,.299,.625,.451]; b.framing.visible[3]=.795;
  assert.equal(stableSelectionView(a,b),true);
});
test('clipping or an engine framing failure rejects either observation', () => {
  for (const key of ['clipped','engine']) {
    const a=state(), b=state();
    if(key==='clipped') b.framing.projected[1]=.22; else b.framing.pass=false;
    assert.equal(stableSelectionView(a,b),false);
    assert.equal(stableSelectionView(b,a),false);
  }
});
test('moving camera, moving control and changed viewport still reject stability', () => {
  for(const change of [s=>s.camera.x+=.02,s=>s.controls.primary.x+=1,s=>s.height=950]) {
    const a=state(),b=state();change(b);assert.equal(stableSelectionView(a,b),false);
  }
});
