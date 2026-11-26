import {test} from "node:test";
import assert from "node:assert/strict";
import {readFileSync} from "node:fs";
import {assemble} from "../visual-benchmark/design.mjs";
const read=p=>JSON.parse(readFileSync(new URL(p,import.meta.url),"utf8"));
const r=read("./evidence/visual-paired-live-20261126.json");
const c=read("./evidence/visual-paired-candidates-20261126.json");
test("archived actual role packets reconstruct exact rendered visual assets",()=>{
 assert.equal(c.source_run,r.run_id);
 assert.equal(c.not_promoted,true);
 for(const [i,p] of c.packets.entries()){
  const a=assemble(p),hashes=r.proof.rounds[0].design_hashes[i];
  assert.equal(a.css_sha256,hashes.css_sha256);
  assert.equal(a.art_sha256,hashes.art_sha256);
  assert.equal(a.design_sha256,hashes.design_sha256);
 }
});
test("archived result retains two disagreeing judges and no promotion",()=>{
 assert.equal(r.proof.visual_decision,"no_clear_agreement");assert.equal(r.proof.art_approved,false);assert.equal(r.proof.promoted,false);
 assert.equal(r.proof.api_responses,6);assert.equal(r.proof.rounds[0].report.total_frames,33);
 assert.deepEqual(r.proof.rounds[0].report.variants.map(x=>x.issues.length),[44,0,0]);
 const avg=(key)=>r.proof.judge_scores.reduce((a,x)=>a+x[key].weighted_score,0)/2;
 assert.ok(Math.abs(avg("baseline")-6.95)<1e-10);assert.ok(Math.abs(avg("variant-72")-7.5)<1e-10);assert.ok(Math.abs(avg("variant-19")-7.565)<1e-10);
});
