import {test} from "node:test";
import assert from "node:assert/strict";
import {readFileSync} from "node:fs";
import {createCandidates,request,validateResult,blindScores,RUBRIC} from "./role-contract.mjs";
const fixture=JSON.parse(readFileSync(new URL("./calibration.json",import.meta.url))).packets;
const evidence={primary_sha256:"a".repeat(64),secondary_sha256:"b".repeat(64),reference_text_sha256:"c".repeat(64)};
test("same visual evidence reaches each distinct role and actual independent contributions assemble",async()=>{
 const seen=[];const r=await createCandidates(evidence,async q=>{seen.push(q);const p=q.role==="single"?fixture[0]:q.role==="director"?fixture[1].direction:q.role==="illustrator"?fixture[1].art:fixture[1].layout;return {request_digest:q.request_digest,payload:p}});
 assert.deepEqual(seen.map(x=>x.role),["single","director","illustrator","integrator"]);
 assert.ok(seen.every(q=>JSON.stringify(q.evidence)===JSON.stringify(evidence)));
 assert.deepEqual(seen[2].prior.direction,fixture[1].direction);assert.deepEqual(seen[3].prior.art,fixture[1].art);
 assert.deepEqual(r.packets,[fixture[0],fixture[1]]);assert.notEqual(r.render_hashes[0].art_sha256,r.render_hashes[1].art_sha256);
});
test("role identity and evidence are bound; stale output rejected",()=>{const q=request("director",evidence);assert.throws(()=>validateResult(q,{request_digest:"stale",payload:fixture[1].direction}));assert.throws(()=>request("director",evidence,{art:fixture[1].art}));assert.throws(()=>request("judge",{...evidence,primary_sha256:"missing"}))});
test("no cross-arm information allowed in common judge prompt",()=>{assert.throws(()=>request("judge",evidence,{single:fixture[0]}));const q=request("judge",evidence);assert.equal(Object.keys(q.prior).length,0)});
test("weighted scores require rubric and evidence, no invented score",()=>{const q=request("judge",evidence);const row={scores:Object.fromEntries(Object.keys(RUBRIC).map(k=>[k,7])),findings:["fixture only","not an actual rating"],confidence:"low"};const p={"baseline":row,"variant-72":row,"variant-19":row};assert.equal(blindScores(validateResult(q,{request_digest:q.request_digest,payload:p})).baseline.weighted_score,7);assert.throws(()=>validateResult(q,{request_digest:q.request_digest,payload:{...p,studio:row}}))});
test("repair requires failed actual check IDs and preserves schema",()=>{assert.throws(()=>request("repair",evidence,{candidate:fixture[0],feedback:[]}));const q=request("repair",evidence,{candidate:fixture[0],feedback:["compact-gates:small label"]},2);assert.deepEqual(validateResult(q,{request_digest:q.request_digest,payload:fixture[1]}),fixture[1])});
