// Real source task with an allowlisted source edit. No arbitrary model code executes.
import {createHash} from "node:crypto";
import {execFileSync,spawn} from "node:child_process";
import {readFileSync,writeFileSync,mkdirSync} from "node:fs";
import {join} from "node:path";
import {runInNewContext} from "node:vm";
import {createInterface} from "node:readline";
import {performance} from "node:perf_hooks";
import {canonical,runMission} from "./core.mjs";
import {requestFor,unwrap} from "./role-bridge.mjs";

export const SOURCE="e1d3b9892e1a75a9e7bfbafd0db225de7aacba74";
export const PATH="tools/senda-brecha/src/rules.js";
export const MARK="  }\n  function playWave(state, lane) {";
export const digest=s=>createHash("sha256").update(s,"utf8").digest("hex");
const checkIds=["safe-edit","malformed-states","route-parity"];
// Each clause is fixed trusted source. Candidate text is reconstructed from
// this list before running it. No arbitrary generated expressions are evaluated.
export const CLAUSES=[
 "!Number.isSafeInteger(source.units)", "source.units > 96",
 "!Number.isSafeInteger(source.attackBonus)", "source.attackBonus < 0", "source.attackBonus > 125",
 "!Number.isSafeInteger(source.shields)", "source.shields < 0", "source.shields > 50",
 "!Number.isSafeInteger(source.totalLost)", "source.totalLost < 0",
 "!Number.isSafeInteger(source.totalRecruited)", "source.totalRecruited < 0",
 "!Number.isSafeInteger(source.score)", "source.score < 0",
 "!Array.isArray(source.history)", "source.history.length !== source.stage",
 "source.finished !== false", "source.victory !== false",
 // plausible but overrestrictive alternatives intentionally available
 "source.units > 22", "source.shields > 6", "source.attackBonus > 22", "source.score > 0"
];
export const OBJECTIVE="Fix checkedState in the real Senda rules so active states require safe integer units 1..96, attackBonus 0..125, shields 0..50, nonnegative safe integer totalLost,totalRecruited,score, array history of length stage, finished=false and victory=false. Preserve ALL 729 valid route outcomes and the original error semantics. Only one literal insertion at the end of checkedState. Set operations[0].path to "+PATH+". Exact before is "+JSON.stringify(MARK)+". After is \"    if (\" + selected clauses joined with \" || \" + \") throw new Error('Estado de la expedición inválido');\\n\" + before. Use only these exact safe clauses, select appropriate subset and order to short-circuit null history: "+CLAUSES.join("; ")+". No other code. Correct using actual checks if a previous attempt failed.";
export const REQ_PATH="tools/studio-v3/live-requests/senda-paired-20261126.json";
export const PLAN_PATH="tools/studio-v3/senda-pilot-plan.json";
export const BUDGET_PATH="tools/studio-v3/senda-pilot-budget.json";
export const TIME_LIMIT=210;
export function frozenSource(){return execFileSync("git",["show",SOURCE+":"+PATH],{encoding:"utf8",maxBuffer:250000})}
function need(x,msg){if(!x)throw Error(msg)}
export function planFor(src){
 need(src.includes(MARK)&&src.indexOf(MARK)===src.lastIndexOf(MARK),"Frozen source marker mismatch");
 need(OBJECTIVE.length<=2000,"Objective too long");
 const grants=["a","b"].map(label=>({schema:1,id:"v3-senda-"+label,mission_id:"senda-pilot-"+label,source_sha:SOURCE,target:"SENDA_STATE_VALIDATION",objective:OBJECTIVE,producer:"programmer",auditor:"independent-reviewer",required_checks:checkIds,files:{[PATH]:digest(src)},max_attempts:2,cap_usd:"0.525",routing_profile:"standard",approved_public_game_evidence:true,expires_on:"2026-12-01"}));
 return {schema:1,id:"senda-paired-20261126",source_sha:SOURCE,source_sha256:digest(src),source_path:PATH,time_budget_seconds:TIME_LIMIT,max_total_reservation_usd:"1.05",arms:[{method:"single",grant:grants[0]},{method:"studio",grant:grants[1]}],judging:"identical deterministic blind checks; technical only; independent text-model audit in Studio arm; no visual score"};
}
export function makeEdit(src,clauses){
 need(Array.isArray(clauses)&&clauses.length>0&&new Set(clauses).size===clauses.length&&clauses.every(c=>CLAUSES.includes(c)),"Unsafe clause");
 const line="    if ("+clauses.join(" || ")+") throw new Error('Estado de la expedición inválido');\n";
 return src.replace(MARK,line+MARK);
}
export function guardCandidate(src,candidate){
 need(typeof candidate==="string"&&candidate.length<=src.length+4000,"Invalid candidate");
 const at=src.indexOf(MARK);
 need(at>=0&&candidate.startsWith(src.slice(0,at))&&candidate.endsWith(src.slice(at)),"Edit outside checkedState insertion");
 const insertion=candidate.slice(at,candidate.length-src.length+at);
 const start="    if (",end=") throw new Error('Estado de la expedición inválido');\n";
 need(insertion.startsWith(start)&&insertion.endsWith(end),"Unapproved guard syntax");
 const clauses=insertion.slice(start.length,-end.length).split(" || ");
 need(makeEdit(src,clauses)===candidate,"Unapproved candidate bytes");
 // Ensure history check protects property access from null/undefined. Reversed
 // ordering is safe code but could throw TypeError; reject at static gate.
 const len=clauses.indexOf("source.history.length !== source.stage"),arr=clauses.indexOf("!Array.isArray(source.history)");
 need(len<0||(arr>=0&&arr<len),"Unsafe history guard order");
 return clauses;
}
function loadRules(code){
 const sandbox={module:{exports:{}}};
 runInNewContext(code,sandbox,{timeout:500,contextCodeGeneration:{strings:false,wasm:false}});
 return sandbox.module.exports;
}
function json(x){return JSON.stringify(x)}
function routes(R){
 const rows=[];
 for(let mask=0;mask<729;mask++){
  let s=R.initialState(),n=mask;
  for(let i=0;i<6&&!s.finished;i++){const lane=n%3;n=Math.floor(n/3);s=R.playWave(s,lane).state}
  rows.push(s);
 }
 return {digest:digest(json(rows)),wins:rows.filter(x=>x.victory).length,losses:rows.filter(x=>!x.victory).length,count:rows.length};
}
const malformed=[
 ["fractional-units",{units:1.5}],["excess-units",{units:97}],
 ["fractional-attack",{attackBonus:0.5}],["negative-attack",{attackBonus:-1}],["excess-attack",{attackBonus:126}],
 ["fractional-shields",{shields:0.5}],["negative-shields",{shields:-1}],["excess-shields",{shields:51}],
 ["missing-lost",{totalLost:undefined}],["negative-lost",{totalLost:-1}],["fractional-lost",{totalLost:1.5}],
 ["missing-recruited",{totalRecruited:undefined}],["negative-recruited",{totalRecruited:-1}],["fractional-recruited",{totalRecruited:1.5}],
 ["missing-score",{score:undefined}],["negative-score",{score:-1}],["fractional-score",{score:1.5}],
 ["nonarray-history",{history:""}],["null-history",{history:null}],["history-length",{history:[{}]}],
 ["missing-finished",{finished:undefined}],["ongoing-victory",{victory:true}]
];
function badStates(R){
 const failed=[];
 for(const [id,change] of malformed){
  try{R.playWave({...R.initialState(),...change},0);failed.push(id)}
  catch(e){if(e?.name!=="Error"||e.message!=="Estado de la expedición inválido")failed.push(id)}
 }
 return {count:malformed.length,failed};
}
export function baselineEvidence(src){
 const r=loadRules(src);
 return {routes:routes(r),invalid:badStates(r)};
}
export function validator(src,baseline){
 return ({files,candidate_digest})=>{
  const row=(id,pass,evidence)=>({id,pass,evidence,candidate_digest});
  const candidate=files[PATH];
  try{guardCandidate(src,candidate)}catch(e){return [row("safe-edit",false,"Candidate rejected by exact safe-guard allowlist"),row("malformed-states",false,"Not run: unsafe candidate"),row("route-parity",false,"Not run: unsafe candidate")]}
  // Run only the identical pinned source plus the trusted canonical guard.
  const c=loadRules(candidate),inv=badStates(c),rr=routes(c);
  return [row("safe-edit",true,"Exact single allowlisted guard insertion"),row("malformed-states",inv.failed.length===0,"Rejected "+(inv.count-inv.failed.length)+"/"+inv.count+" invalid states; failed IDs: "+inv.failed.join(",")),row("route-parity",rr.digest===baseline.routes.digest,"All 729 terminal states and reports hash "+rr.digest+"; wins "+rr.wins+"; losses "+rr.losses)];
 };
}
function specFor(g){return {schema:1,id:g.mission_id,mode:"offline",budget_usd:0,source_sha:g.source_sha,target:g.target,objective:g.objective,max_attempts:g.max_attempts,quality_required:false,required_checks:g.required_checks,producer:g.producer,auditor:g.auditor,files:[{path:PATH,digest:g.files[PATH]}]}}
function write(path,object){writeFileSync(path,JSON.stringify(object,null,2)+"\n",{flag:"wx",mode:0o600})}
async function armRun(method,grant,src,baseline,ask,root){
 const spec=specFor(grant),start=performance.now();let attempt=0,checkpoint=0;
 const receipts=[];
 function elapsed(){return performance.now()-start}
 async function invoke(role,input){
  need(elapsed()<TIME_LIMIT*1000,"Arm time limit");
  const req=requestFor({spec,role,attempt,input});
  const packet=await ask(grant.id,req);
  const payload=unwrap(req,packet);
  receipts.push({role,attempt,request_digest:req.request_digest,payload_digest:packet.payload_digest,receipt_ref:packet.receipt_ref,origin:packet.origin});
  return payload
 }
 const result=await runMission(spec,{[PATH]:src},{digest,
  propose:({baseline,previous,required_pivot_from,lessons})=>{attempt++;return invoke("producer",{objective:spec.objective,required_checks:spec.required_checks,baseline,previous,required_pivot_from,lessons})},
  validate:validator(src,baseline),
  audit:({baseline,files,checks,candidate_digest})=>method==="studio"
    ? invoke("auditor",{objective:spec.objective,required_checks:spec.required_checks,baseline,files,checks,candidate_digest})
    : {reviewer:spec.auditor,candidate_digest,decision:"accept",reason:"Method-blind deterministic acceptance; no extra model review",quality:"not_assessed"},
  checkpoint:r=>write(join(root,grant.id+"-checkpoint-"+(++checkpoint)+".json"),{...r,api_calls:undefined,role_receipts:receipts})
 });
 // Core does not perform API calls itself. Actual network accounting comes
 // only from the external persistent Python ledger, never the core's zero.
 delete result.api_calls;
 const candidate=result.candidate?.files?.[PATH]??null;
 if(candidate!==null){
  guardCandidate(src,candidate);
  writeFileSync(join(root,grant.id+"-candidate.js"),candidate,{flag:"wx",mode:0o600});
 }
 const last=result.rows.at(-1);
 return {method,grant_id:grant.id,accepted:result.accepted,terminal:result.terminal,elapsed_ms:Math.round(elapsed()),attempts:result.rows.length,rows:result.rows,role_receipts:receipts,candidate_sha256:candidate===null?null:digest(candidate),quality_score:null,manual_interventions:0,technical_checks:last?.checks??null};
}
function transport(root,live){
 const child=spawn("python3",["tools/studio-v3/pilot-dispatch.py",live?"--transport":"--fixture-transport",root],{stdio:["pipe","pipe","inherit"],env:process.env});
 const lines=createInterface({input:child.stdout,crlfDelay:Infinity});
 const iter=lines[Symbol.asyncIterator]();
 const closed=new Promise((resolve,reject)=>{child.on("error",reject);child.on("close",code=>code===0?resolve():reject(Error("Transport stopped "+code)))});
 let ended=false;
 return {
  ask:async(id,req)=>{need(!ended,"Transport closed");child.stdin.write(JSON.stringify({id,req})+"\n");const line=await iter.next();need(!line.done,"Transport ended");const p=JSON.parse(line.value);need(p.ok===true,"Transport blocked: "+(p.code??"unknown"));return p.packet},
  close:async()=>{if(!ended){ended=true;child.stdin.end()}await closed}
 };
}
export async function paired(root,live){
 const src=frozenSource(),plan=planFor(src),base=baselineEvidence(src);
 need(base.invalid.failed.length>0,"No reproduced baseline defect");
 mkdirSync(root,{recursive:false});
 write(join(root,"baseline-proof.json"),base);
 const tr=transport(root,live),results=[];
 try{for(const a of plan.arms)results.push(await armRun(a.method,a.grant,src,base,tr.ask,root))}
 finally{await tr.close()}
 const ledgers=JSON.parse(readFileSync(join(root,"ledger-summary.json"),"utf8"));
 const final=results.map(r=>{
  const l=ledgers.find(x=>x.grant_id===r.grant_id);need(l,"Missing role ledger");
  const entries=l.entries;
  need(r.role_receipts.every(rr=>entries.some(e=>e.receipt_ref===rr.receipt_ref&&e.packet_digest===rr.payload_digest)),"Receipt mismatch");
  return {...r,api_calls:live?entries.length:0,fixture_calls:live?0:entries.length,estimated_cost_usd:live?(entries.every(e=>e.estimated_usd!==null)?entries.reduce((n,e)=>n+Number(e.estimated_usd),0):null):0,reserved_usd:l.reserved_usd,halted:l.halted,ledger_entries:entries};
 });
 const report={schema:1,id:plan.id,live_model_roles:live,source_sha:SOURCE,source_sha256:digest(src),baseline:base,arms:final,max_total_reservation_usd:plan.max_total_reservation_usd,both_accepted:final.every(x=>x.accepted),independent_common_judge:true,quality_score:null,art_approved:false,promoted:false,published:false,billing_verified:false,limitations:"One paired technical task; constrained edit grammar, same budget/time ceiling but additional model audit only in Studio. No aesthetic or general productivity inference."};
 write(join(root,"paired-report.json"),report);
 console.log("STUDIO_V3_PAIRED_PROOF "+JSON.stringify(report));
 return report
}
if(process.argv[1]?.endsWith("senda-pilot.mjs")){
 const cmd=process.argv[2];
 if(cmd==="--plan"){const s=frozenSource(),p=planFor(s);console.log("STUDIO_V3_PLAN "+JSON.stringify({plan:p,plan_sha256:digest(canonical(p)),grant_hashes:p.arms.map(a=>({id:a.grant.id,digest:digest(canonical(a.grant))})),baseline:baselineEvidence(s)}))}
 else if(cmd==="--fixture"||cmd==="--live")await paired(process.argv[3],cmd==="--live");
 else throw Error("Unknown pilot mode")
}
