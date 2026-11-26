// Trusted-host orchestration only. The provider and actual renderer are injected.
// No network, shell execution, or spending authorization in this module.
import {createCandidates,request,validateResult,blindScores} from "../visual-benchmark/role-contract.mjs";
import {assemble,SOURCE,REFERENCE,TARGET,sha} from "../visual-benchmark/design.mjs";
const need=(v,m)=>{if(!v)throw Error(m)};
const alias=["variant-72","variant-19"],method=["single","studio"];
const hash=x=>typeof x==="string"&&/^[a-f0-9]{64}$/.test(x);
const validEvidence=e=>e&&Object.keys(e).length===3&&["primary_sha256","secondary_sha256","reference_text_sha256"].every(k=>hash(e[k]));
function verifyRender(render,pack,productionEvidence){
 const p=render?.report;
 need(p&&p.source_sha===SOURCE&&p.reference_sha===REFERENCE&&p.target===TARGET&&p.mode==="external-design-packet"&&p.actual_game_logic_unchanged===true&&p.matched_fixtures===11&&p.total_frames===33&&Array.isArray(p.errors)&&p.errors.length===0,"No verified actual matched render");
 need(Array.isArray(p.variants)&&p.variants.map(x=>x.variant).join(",")==="baseline,variant-72,variant-19"&&p.variants.every(x=>x.frames===11&&Array.isArray(x.issues)),"Incomplete render metrics");
 need(Array.isArray(p.interactions)&&p.interactions.length===3&&p.interactions.every(x=>x.lane_buttons===true&&x.pause===true&&x.resume===true&&x.replay===true),"Gameplay regression");
 need(validEvidence(render.judgeEvidence)&&render.judgeEvidence.reference_text_sha256===productionEvidence.reference_text_sha256,"Judge evidence or common reference changed");
 need(Array.isArray(render.design_hashes)&&render.design_hashes.length===2,"No rendered candidate provenance");
 for(let i=0;i<2;i++){const d=assemble(pack.packets[i]),r=render.design_hashes[i];need(r.alias===alias[i]&&r.css_sha256===d.css_sha256&&r.art_sha256===d.art_sha256&&r.design_sha256===d.design_sha256,"Rendered different candidate bytes")}
 return p;
}
export async function runVisualTrial(productionEvidence,adapters,{mode="fixture"}={}){
 need(["fixture","authorized-live"].includes(mode)&&validEvidence(productionEvidence)&&["call","render","registerJudgeEvidence","checkpoint"].every(k=>typeof adapters[k]==="function"),"Missing trusted adapters");
 const calls=[],rounds=[],judgeRows=[];
 const record=async x=>adapters.checkpoint({...x,calls:[...calls],renders:rounds.map(r=>({round:r.round,variants:r.report.variants})),mode,art_approved:false,promoted:false});
 async function invoke(slot,q){
  const a=await adapters.call(slot,q);
  need(a&&a.origin===(mode==="fixture"?"offline_fixture":"real_api")&&typeof a.receipt_ref==="string"&&a.receipt_ref.length>0&&!calls.some(c=>c.receipt_ref===a.receipt_ref),"Missing/duplicate authenticated role receipt");
  const payload=validateResult(q,a.response);
  calls.push({slot,role:q.role,attempt:q.attempt,request_digest:q.request_digest,receipt_ref:a.receipt_ref,origin:a.origin});
  await record({phase:"role-completed",slot});
  return {request_digest:q.request_digest,payload};
 }
 const pack=await createCandidates(productionEvidence,q=>invoke(q.role+"-1",q));
 let latest;
 const doRender=async round=>{
  const out=await adapters.render({schema:1,target:TARGET,packets:pack.packets},round);
  const report=verifyRender(out,pack,productionEvidence);rounds.push({round,report,judgeEvidence:out.judgeEvidence,design_hashes:out.design_hashes});
  latest=out;await record({phase:"actual-render",round});return report;
 };
 let report=await doRender(1),repaired=false;
 for(let i=0;i<2;i++){
  const issues=report.variants[i+1].issues;
  if(issues.length===0&&report.variants[i+1].all_controls===true)continue;
  const feedback=issues.slice(0,20).map(x=>(x.fixture+": "+x.issue).slice(0,300));
  if(!feedback.length)feedback.push("Actual controls gate failed");
  const q=request("repair",productionEvidence,{candidate:pack.packets[i],feedback},2);
  const r=await invoke(method[i]+"-repair",q);
  pack.packets[i]=r.payload;repaired=true;
 }
 if(repaired)report=await doRender(2);
 const eligible=report.variants.slice(1).every(x=>x.issues.length===0&&x.all_controls===true);
 const result={schema:1,target:TARGET,mode,source_sha:SOURCE,source_evidence:productionEvidence,pack,role_receipts:calls,renders:rounds,technical_gate_passed:eligible,repair_performed:repaired,judge_scores:null,visual_decision:"not_scored",art_approved:false,promoted:false};
 if(!eligible){await record({phase:"technical-reject",result});return result}
 await adapters.registerJudgeEvidence(latest.judgeEvidence,report);
 for(let n=1;n<=2;n++){
  const q=request("judge",latest.judgeEvidence,{},n);
  const r=await invoke("judge-"+n,q);judgeRows.push(blindScores(r.payload));
 }
 const differences=judgeRows.map(j=>({studio_minus_single:+(j[alias[1]].weighted_score-j[alias[0]].weighted_score).toFixed(2),studio_vs_baseline:+(j[alias[1]].weighted_score-j.baseline.weighted_score).toFixed(2),single_vs_baseline:+(j[alias[0]].weighted_score-j.baseline.weighted_score).toFixed(2)}));
 const allStudio=differences.every(d=>d.studio_minus_single>=.5&&d.studio_vs_baseline>=.5);
 const allSingle=differences.every(d=>d.studio_minus_single<=-.5&&d.single_vs_baseline>=.5);
 result.judge_scores=judgeRows;result.judge_differences=differences;result.visual_decision=allStudio?"studio_preferred_in_this_trial":allSingle?"single_preferred_in_this_trial":"no_clear_agreement";
 // Fixture judges are fabricated regression inputs, never measured quality.
 if(mode==="fixture"){result.fixture_score_simulation=true;result.visual_decision="fixture_only_not_a_quality_result"}
 await record({phase:"closed",result});return result;
}
