import {spawn} from "node:child_process";
import {createInterface} from "node:readline";
import {readFileSync,writeFileSync,mkdirSync,renameSync,existsSync} from "node:fs";
import {join,resolve} from "node:path";
import {loadEvidence,actualRenderer} from "./runtime.mjs";
import {runVisualTrial} from "./orchestrator.mjs";
import {sha} from "../visual-benchmark/design.mjs";
const need=(x,m)=>{if(!x)throw Error(m)};
const json=p=>JSON.parse(readFileSync(p,"utf8"));
const [mode,cal0,web0,out0]=process.argv.slice(2);
need(["--verify-input","--live"].includes(mode)&&cal0,"Explicit mode required");
const cal=resolve(cal0),grant=json("tools/studio-v3/visual-live/authorized-plan.json");
const prod=loadEvidence(cal,"production",grant.production_evidence);
if(mode==="--verify-input"){
 console.log("STUDIO_V3_VISUAL_INPUT_VERIFIED "+JSON.stringify({evidence:prod.evidence,frames:prod.report.total_frames}));process.exit(0)
}
need(web0&&out0,"Missing actual source/output");
const web=resolve(web0),out=resolve(out0);
const expected=prod.report;
need(sha(readFileSync(join(web,"tools/senda-brecha/visual-3d/index.html")))===expected.source_sha256&&sha(readFileSync(join(web,"tools/senda-brecha/visual-3d/reference/panel_obsidian.png")))===expected.material_sha256,"Materialized runtime differs from certified calibration");
mkdirSync(out,{recursive:false});
const child=spawn("python3",["-u","tools/studio-v3/visual-live/dispatch.py","--transport",cal,out],{stdio:["pipe","pipe","inherit"],env:process.env});
const lines=createInterface({input:child.stdout,crlfDelay:Infinity}),iterator=lines[Symbol.asyncIterator]();
const closed=new Promise((res,rej)=>{child.on("error",rej);child.on("close",code=>res(code))});
let stopped=false,latestRound=0,checkpoint=0,result=null,error=null;
function persist(name,value){
 const target=join(out,name),temp=target+".tmp";writeFileSync(temp,JSON.stringify(value,null,2)+"\n",{mode:0o600});renameSync(temp,target)
}
async function ask(m){
 need(!stopped,"Transport closed");
 child.stdin.write(JSON.stringify(m)+"\n");
 const line=await iterator.next();need(!line.done,"Transport terminated");
 const p=JSON.parse(line.value);need(p.ok===true,"Transport rejected: "+(p.error||"unknown"));return p
}
const renderActual=actualRenderer(web,out);
try{
 result=await runVisualTrial(prod.evidence,{
  call:async(slot,q)=>{
   console.log("VISUAL_SLOT_START "+slot);
   const p=await ask({action:"call",slot,req:q});
   console.log("VISUAL_SLOT_RESPONSE "+slot);
   return p
  },
  render:async(pack,round)=>{
   latestRound=round;
   console.log("VISUAL_RENDER_START "+round);
   const t=Date.now(),r=await renderActual(pack,round);
   persist("render-duration-"+round+".json",{elapsed_ms:Date.now()-t});
   console.log("VISUAL_RENDER_COMPLETE "+round+" "+JSON.stringify(r.report.variants.map(x=>({variant:x.variant,issues:x.issues.length,frames:x.frames}))));
   return r
  },
  registerJudgeEvidence:async(e)=>{await ask({action:"register",round:latestRound,evidence:e})},
  checkpoint:async(p)=>{persist("checkpoint-"+(++checkpoint)+".json",p);persist("last-checkpoint.json",p)}
 },{mode:"authorized-live"});
 persist("trial-result.json",result)
}catch(e){error={name:e.name,message:String(e.message).slice(0,300)};persist("trial-error.json",error)}
finally{stopped=true;child.stdin.end();await closed}
const lpath=join(out,"ledger/ledger.json"),ledger=existsSync(lpath)?json(lpath):null,entries=ledger?.entries||[];
const amounts=entries.map(e=>e.estimated_usd);
const sum=vals=>Math.round(vals.reduce((a,b)=>a+Number(b),0)*1e9)/1e9;
const complete=ledger!==null&&amounts.every(x=>x!==null&&x!==undefined);
const totals=Object.fromEntries(["single","studio","common-judge"].map(arm=>{
 const rows=entries.filter(e=>e.arm===arm);
 return [arm,{calls:rows.length,completed:rows.filter(e=>e.state==="completed").length,estimated_usd:rows.every(e=>e.estimated_usd!==null&&e.estimated_usd!==undefined)?sum(rows.map(e=>e.estimated_usd)):null,reserved_usd:sum(rows.map(e=>e.reserved_usd)),api_duration_ms:rows.reduce((a,b)=>a+(b.duration_ms||0),0)}]
}));
const report={schema:1,live_model_roles:entries.some(e=>e.response_id),error,technical_gate_passed:result?.technical_gate_passed??null,visual_decision:result?.visual_decision??"incomplete",judge_scores:result?.judge_scores??null,judge_differences:result?.judge_differences??null,rounds:result?.renders.map(x=>({round:x.round,report:x.report,design_hashes:x.design_hashes}))??null,repair_performed:result?.repair_performed??null,role_receipts:result?.role_receipts??null,call_count:entries.length,api_responses:entries.filter(e=>e.response_id).length,estimated_cost_usd:complete?sum(amounts):null,totals,ledger_entries:entries.map(({response_id,...e})=>({...e,api_response_received:!!response_id})),max_reservation_usd:"2.00",billing_verified:false,art_approved:false,promoted:false,physical_iphone:false};
persist("live-summary.json",report);
console.log("STUDIO_V3_VISUAL_LIVE_PROOF "+JSON.stringify(report));
if(error||!result||!result.technical_gate_passed||!result.judge_scores)process.exitCode=1;
