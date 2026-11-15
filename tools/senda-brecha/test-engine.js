'use strict';
const assert = require('node:assert/strict');
const { EventRun, ROUNDS, GATE_SECONDS } = require('./engine.js');
function run(sides, step = 0.05) {
  const e = new EventRun(); e.start();
  let prevStage = 0;
  for(let n=0; n < 30000 && !['victory','defeat'].includes(e.phase); n++) {
    if(e.phase === 'choice') e.select(sides[e.roundIndex]);
    e.advance(step);
    const snap = e.snapshot();
    assert(snap.soldiers >= 0 && Number.isFinite(snap.attackDPS));
    assert(snap.enemyHP >= 0);
    assert(snap.stage >= prevStage);
    prevStage = snap.stage;
  }
  assert(['victory','defeat'].includes(e.phase));
  return e;
}
assert.equal(ROUNDS.length,3);
const all = [];
for(let n=0;n<8;n++){
  const sides = [n&1,(n>>1)&1,(n>>2)&1];
  const e = run(sides);
  all.push({sides:sides.join(''),result:e.phase,time:Math.round(e.time),survivors:e.soldiers,lost:e.totalLost});
  assert(e.time>GATE_SECONDS*3 && e.time<140);
  assert.equal(e.chosen.length,3);
  assert.equal(!!e.rewards, e.phase==='victory');
  const snapshot=e.snapshot(); assert.equal(snapshot.selected.length,3);
  const stable={...snapshot};
  e.advance(5000); assert.deepEqual(e.snapshot(),stable);
}
assert(all.some(r=>r.result==='victory'),'At least one winning path');
assert(all.some(r=>r.result==='defeat'),'At least one losing path makes decisions meaningful');
const e=new EventRun();
assert.equal(e.phase,'intro'); assert.equal(e.select(1),false);
assert.equal(e.start(),true); assert.equal(e.start(),false);
assert.equal(e.select(1),true); assert.equal(e.select(-1),false);
e.advance(6); assert.equal(e.phase,'choice'); // dt spike bounded
for(let n=0;n<120;n++)e.advance(0.05);
assert.equal(e.phase,'combat'); assert.equal(e.chosen[0].side,1);
assert.equal(e.select(0),false);
assert.equal(e.resolveGate(),false);
assert.equal(e.drainEvents().length,2);
assert.equal(e.drainEvents().length,0);
e.reset(); assert.equal(e.phase,'intro'); assert.equal(e.rewards,null);
console.log('PASS: 8 complete deterministic playthroughs, choice invariants, event receipt and no permanent rewards');
console.table(all);
