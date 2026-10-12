import test from 'node:test';
import assert from 'node:assert/strict';
import {ART_APPROVAL_STATES, requireMatchingApprovedCycle} from './approval-boundary.mjs';

const ws='unit-test-art-workstream';
const request=(state)=>({visual_gate_state:state,workstream_id:ws});
const cycle=(state)=>({schema_version:1,state,workstream_id:ws,asset_id:'valoria/cliff'});
for(const state of ART_APPROVAL_STATES) {
 test('approved production '+state+' needs same-state cycle belonging to same workstream',()=>{
   assert.equal(requireMatchingApprovedCycle(cycle(state),request(state),ws),true);
   for(const wip of ['REQUESTED','AUTHORING','TECH_VALIDATED','VISUAL_REJECTED','PROTOTYPE_NO_AI_AUTHORSHIP'])
     assert.throws(()=>requireMatchingApprovedCycle(cycle(wip),request(state),ws),/matching approved artist cycle/);
   assert.throws(()=>requireMatchingApprovedCycle(cycle('ART_APPROVED'),request('RELEASED'),ws),/matching approved artist cycle/);
 });
}
test('cannot recycle signed art from a different workstream',()=>{
 assert.throws(()=>requireMatchingApprovedCycle({...cycle('ART_APPROVED'),workstream_id:'other'},request('ART_APPROVED'),ws),/different workstream/);
 assert.throws(()=>requireMatchingApprovedCycle(cycle('ART_APPROVED'),{...request('ART_APPROVED'),workstream_id:'other'},ws),/different workstream/);
});
test('unknown and nonapproved gate statuses cannot claim approval',()=>{
 assert.throws(()=>requireMatchingApprovedCycle(cycle('ART_APPROVED'),request('TECH_VALIDATED'),ws),/not an approved/);
 assert.throws(()=>requireMatchingApprovedCycle(cycle('ART_APPROVED'),request('ART_APPROVE0'),ws),/not an approved/);
});
test('approved state without real asset identity is rejected',()=>{
 assert.throws(()=>requireMatchingApprovedCycle({...cycle('ART_APPROVED'),asset_id:''},request('ART_APPROVED'),ws),/asset identity/);
});
