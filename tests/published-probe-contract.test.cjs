'use strict';
const { test } = require('node:test');
const assert = require('node:assert/strict');
const { assertRuntimeClean, assertPersistedState, persistenceSnapshot,
  controlPoint, certifyMacroloop } = require('../tools/published-probe-contract.cjs');
const state = { revision:4,wood:590,stone:150,gatheredWood:360,gatheredStone:0,
  sawmill:0,bastion:1,march:'idle' };

test('rejects the Unity error observed in a previously green published probe', () => {
  assert.throws(() => assertRuntimeClean([{type:'error',text:
    "The file 'level0' is corrupted! Remove it and launch unity again!\n[Position out of bounds!]"}]),
    /Runtime errors prevent certification/);
});
test('rejects page errors, dialogs and Unity fail markers, but preserves warning distinction', () => {
  for (const entry of [{type:'pageerror',text:'crash'},{type:'dialog',text:'unsupported'},
    {type:'log',text:'VALORIA_WEBGL_BACKGROUND_LOAD_FAIL key=missing'}])
    assert.throws(() => assertRuntimeClean([entry]));
  assert.doesNotThrow(() => assertRuntimeClean([{type:'warning',text:'sync API deprecated'}]));
});
test('every persisted quantity and revision must match; excess progress is not silently accepted', () => {
  assert.doesNotThrow(() => assertPersistedState(state,{...state}));
  for (const field of Object.keys(state)) {
    const changed = {...state,[field]:typeof state[field]==='number'?state[field]+1:'outbound'};
    assert.throws(() => assertPersistedState(state,changed), /Persistence mismatch/);
  }
  const missing = {...state}; delete missing.stone;
  assert.throws(() => persistenceSnapshot(missing), /Missing persisted-state field/);
});
test('maps a named Unity control to one visible CSS target and rejects offscreen targets', () => {
  const box={x:10,y:20,width:844,height:390};
  const canvas={width:844,height:390};
  assert.deepEqual(controlPoint({x:630.5,unityY:32},box,canvas),{x:640.5,y:378});
  assert.throws(() => controlPoint({x:900,unityY:32},box,canvas),/outside/);
  assert.throws(() => controlPoint(null,box,canvas),/no valid/);
});
test('a successful macroloop cannot claim full progression, human play or visual quality', () => {
  const report={logs:[],checks:{beforeReload:{...state},afterReload:{...state}}};
  certifyMacroloop(report);
  assert.equal(report.checks.navigationGatherPersistencePass,true);
  assert.equal(report.checks.playablePass,undefined);
  assert.equal(report.coverage.fullBastionIToIIPass,false);
  assert.equal(report.coverage.visualQualityPass,false);
  assert.equal(report.coverage.independentHumanPlaythroughPass,false);
});
