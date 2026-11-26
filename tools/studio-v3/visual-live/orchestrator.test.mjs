import {test} from "node:test";
import assert from "node:assert/strict";
import {readFileSync} from "node:fs";
import {runVisualTrial} from "./orchestrator.mjs";
import {SOURCE,REFERENCE,TARGET,assemble} from "../visual-benchmark/design.mjs";
import {RUBRIC} from "../visual-benchmark/role-contract.mjs";
const c=JSON.parse(readFileSync(new URL("../visual-benchmark/calibration.json",import.meta.url))).packets;
const e={primary_sha256:"a".repeat(64),secondary_sha256:"b".repeat(64),reference_text_sha256:"c".repeat(64)};
const j={primary_sha256:"d".repeat(64),secondary_sha256:"e".repeat(64),reference_text_sha256:e.reference_text_sha256};
const aliases=["baseline","variant-72","variant-19"];
function fixtureAdapter({failFirst=false,failAlways=false,wrongRender=false}={}){
 const slots=[],rounds=[],checkpoints=[],judges=[];
 function score(value){return {scores:Object.fromEntries(Object.keys(RUBRIC).map(k=>[k,value])),findings:["Fixture observation, not a visual judgment","Fixture only"],confidence:"low"}}
 return {slots,rounds,checkpoints,judges,
  call:async(slot,q)=>{slots.push(slot);let p=q.role==="single"?c[0]:q.role==="director"?c[1].direction:q.role==="illustrator"?c[1].art:q.role==="integrator"?c[1].layout:q.role==="repair"?c[1]:{baseline:score(6),"variant-72":score(7),"variant-19":score(8)};return {origin:"offline_fixture",receipt_ref:"fixture-"+slot,response:{request_digest:q.request_digest,payload:p}}},
  render:async(pack,n)=>{rounds.push(n);return {report:{mode:"external-design-packet",source_sha:SOURCE,reference_sha:REFERENCE,target:TARGET,actual_game_logic_unchanged:true,matched_fixtures:11,total_frames:33,errors:[],variants:aliases.map((variant,i)=>({variant,frames:11,all_controls:true,issues:i===1&&(failAlways||(failFirst&&n===1))?[{fixture:"phone-gates",issue:"small-control:#pause"}]:[]})),interactions:aliases.map(variant=>({variant,lane_buttons:true,pause:true,resume:true,replay:true}))},judgeEvidence:j,design_hashes:pack.packets.map((p,i)=>({alias:aliases[i+1],...assemble(p),css_sha256:wrongRender?"wrong":assemble(p).css_sha256}))}},
  registerJudgeEvidence:async(e,p)=>judges.push(e),
  checkpoint:async p=>checkpoints.push(p)
 }
}
test("complementary roles, actual render gate, common blind judges in order",async()=>{const a=fixtureAdapter(),r=await runVisualTrial(e,a);assert.deepEqual(a.slots,["single-1","director-1","illustrator-1","integrator-1","judge-1","judge-2"]);assert.deepEqual(a.rounds,[1]);assert.equal(a.judges.length,1);assert.equal(r.visual_decision,"fixture_only_not_a_quality_result");assert.equal(r.art_approved,false)});
test("a failed actual control gate is repaired once, rerenders both, then judges",async()=>{const a=fixtureAdapter({failFirst:true}),r=await runVisualTrial(e,a);assert.deepEqual(a.rounds,[1,2]);assert.ok(a.slots.includes("single-repair"));assert.ok(!a.slots.includes("studio-repair"));assert.equal(r.repair_performed,true);assert.equal(r.technical_gate_passed,true)});
test("persistent regression stops before paid blind judging",async()=>{const a=fixtureAdapter({failAlways:true}),r=await runVisualTrial(e,a);assert.equal(r.technical_gate_passed,false);assert.equal(r.judge_scores,null);assert.equal(a.slots.filter(x=>x.startsWith("judge")).length,0)});
test("reject rendered bytes not bound to the candidates",async()=>{const a=fixtureAdapter({wrongRender:true});await assert.rejects(()=>runVisualTrial(e,a),/Rendered different candidate bytes/);assert.equal(a.judges.length,0)});
test("cannot label fixture model outputs as live",async()=>{const a=fixtureAdapter();await assert.rejects(()=>runVisualTrial(e,a,{mode:"authorized-live"}),/authenticated role receipt/);assert.equal(a.rounds.length,0)});
