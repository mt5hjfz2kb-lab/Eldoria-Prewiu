import test from 'node:test';
import assert from 'node:assert/strict';
import {parseUnitySignal} from './signal-parser.mjs';
test('only authoritative authored Bastion I to V numeric markers are counted',()=>{
 for(let b=1;b<=5;b++){
  assert.deepEqual(parseUnitySignal('ELDORIA_BETA_SIGNAL|bastion_reached|bastion='+b),
   {name:'bastion_reached',payload:{bastion:b}});
 }
 assert.deepEqual(parseUnitySignal('ELDORIA_BETA_SIGNAL|end_of_slice|bastion=5'),
  {name:'end_of_slice',payload:{bastion:5}});
});
test('forbid fabricated level 6, false completion and unapproved payload',()=>{
 for(const line of [
  'ELDORIA_BETA_SIGNAL|bastion_reached|bastion=6',
  'ELDORIA_BETA_SIGNAL|end_of_slice|bastion=4',
  'ELDORIA_BETA_SIGNAL|build_completed|building_code=fake|level=2',
  'ELDORIA_BETA_SIGNAL|bastion_reached|bastion=1|player_name=Felipe',
  'ELDORIA_BETA_SIGNAL|bastion_reached|bastion=1|save=PlayerState',
  'ELDORIA_BETA_SIGNAL|bastion_reached|bastion=1|bastion=2',
  'VALORIA_PLAYTEST_CHUNK 1 0 2 eyJQbGF5ZXJOYW1lIjoifQ==',
  'ELDORIA_BETA_SIGNAL|card_acquired|bastion=2'
 ])assert.equal(parseUnitySignal(line),null,line);
});
test('event payload is limited to enumerated milestone codes',()=>{
 assert.deepEqual(parseUnitySignal('2026-11-09 ELDORIA_BETA_SIGNAL|build_completed|building_code=sawmill|level=2\nUnity log'),
 {name:'build_completed',payload:{building_code:'sawmill',level:2}});
 assert.deepEqual(parseUnitySignal('ELDORIA_BETA_SIGNAL|march_sent|march_slots_used=3'),
 {name:'march_sent',payload:{march_slots_used:3}});
 assert.deepEqual(parseUnitySignal('ELDORIA_BETA_SIGNAL|battle_resolved|outcome_code=win'),
 {name:'battle_resolved',payload:{outcome_code:'win'}});
});
