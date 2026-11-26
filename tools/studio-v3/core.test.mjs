import {createHash} from "node:crypto";
import {test} from "node:test";
import {canonical,safePath,validateSpec,applyProposal,runMission,compare} from "./core.mjs";
function testsuite(digest,base){
 const source={"candidate.json":'{"a":1,"b":2}'};
 const s={schema:1,id:"test-cycle",mode:"offline",budget_usd:0,source_sha:base,files:[{path:"candidate.json",digest:digest(source["candidate.json"])}],target:"T",objective:"Improve candidate",max_attempts:4,quality_required:false,required_checks:["rules"],producer:"producer",auditor:"auditor"};
 const p=(before,after,strategy="local-edit")=>({target:"T",producer:"producer",strategy,operations:[{path:"candidate.json",before,after}]});
 const check=({files,candidate_digest})=>[{id:"rules",pass:JSON.parse(files["candidate.json"]).a===3,evidence:"parse + expected a",candidate_digest}];
 const audit=({candidate_digest})=>({reviewer:"auditor",candidate_digest,decision:"accept",reason:"Independent fixture acceptance",quality:"not_assessed"});
 const adapter=(ps,v=check,a=audit)=>{let i=0;return {digest,propose:async()=>ps[i++],validate:v,audit:a,checkpoint:async()=>{}}};
 const reject=fn=>{let threw=false;try{fn()}catch{threw=true}if(!threw)throw Error("Expected rejection")};
 const ok=(x,m)=>{if(!x)throw Error(m??"Assertion failed")};
 const arm={method:"single",source_sha:base,target:"T",objective:"same",protocol_digest:"p",time_budget_seconds:100,budget_usd:1,reviewer:"blind",required_checks_digest:"r",blind_review:true,accepted:true,elapsed_seconds:20,interventions:1,regressions:0,cost_usd:null,quality:null,evidence_ref:"run-1"};
 return [
 ["exact edit",()=>ok(applyProposal(s,source,p('"a":1','"a":3'))["candidate.json"]==='{"a":3,"b":2}')],
 ["scope traversal",()=>reject(()=>applyProposal(s,source,{...p("1","3"),operations:[{path:"../candidate.json",before:"1",after:"3"}]}))],
 ["duplicate match",()=>reject(()=>applyProposal(s,{"candidate.json":"aa"},p("a","b")))],
 ["target drift",()=>reject(()=>applyProposal(s,source,{...p("1","3"),target:"OTHER"}))],
 ["wrong source digest",()=>reject(()=>validateSpec(s,{"candidate.json":"other"},digest))],
 ["paid mode denied",()=>reject(()=>validateSpec({...s,budget_usd:1},source,digest))],
 ["reviewer independence",()=>reject(()=>validateSpec({...s,auditor:"producer"},source,digest))],
 ["no-op",()=>reject(()=>applyProposal(s,source,p("1","1")))],
 ["sensitive paths",()=>ok(!safePath("tools/budget_guard.py")&&!safePath(".github/workflows/test.yml")&&!safePath("foo/../bar.js"))],
 ["technical rejection then correction",async()=>{let r=await runMission(s,source,adapter([p('"a":1','"a":2'),p('"a":1','"a":3')]));ok(r.accepted&&r.rows.length===2&&r.rows[0].outcome==="technical_reject"&&r.lessons.length===1)}],
 ["pivot after two failures",async()=>{let r=await runMission(s,source,adapter([p('"a":1','"a":2'),p('"a":1','"a":4'),p('"a":1','"a":3'),p('"a":1','"a":3',"macro-pivot")]));ok(r.accepted&&r.rows[1].pivot_required&&r.rows[2].outcome==="rejected_proposal"&&r.rows[3].strategy==="macro-pivot")}],
 ["reject recycled candidate",async()=>{let r=await runMission({...s,max_attempts:2},source,adapter([p("1","2"),p("1","2","different")]));ok(!r.accepted&&r.rows[1].reason.includes("already evaluated"))}],
 ["wrong evidence binding",async()=>{let r=await runMission({...s,max_attempts:1},source,adapter([p("1","3")],()=>[{id:"rules",pass:true,candidate_digest:"wrong",evidence:"claimed"}]));ok(!r.accepted&&r.rows[0].outcome==="rejected_validation")}],
 ["auditor rejects valid candidate",async()=>{let r=await runMission({...s,max_attempts:1},source,adapter([p("1","3")],check,({candidate_digest})=>({...audit({candidate_digest}),decision:"reject",reason:"Not good enough"})));ok(!r.accepted&&r.rows[0].outcome==="quality_reject")}],
 ["quality cannot be skipped",async()=>{let r=await runMission({...s,max_attempts:1,quality_required:true},source,adapter([p("1","3")]));ok(!r.accepted&&r.rows[0].outcome==="rejected_audit")}],
 ["no game publication",async()=>{let r=await runMission(s,source,adapter([p("1","3")]));ok(r.accepted&&!r.art_approved&&!r.merged&&!r.published&&r.api_calls===0)}],
 ["checkpoint failure stops run",async()=>{let called=0;let failed=false;try{await runMission(s,source,{...adapter([p("1","2")]),checkpoint:async()=>{called++;throw Error("Disk unavailable")}})}catch{failed=true}ok(failed&&called===1)}],
 ["benchmark unknown metrics",()=>{let r=compare(arm,{...arm,method:"studio",elapsed_seconds:10});ok(r.time_saving_fraction===.5&&r.quality_delta===null&&r.cost_delta_usd===null)}],
 ["benchmark mismatch",()=>reject(()=>compare(arm,{...arm,method:"studio",target:"other"}))]
 ]
}
for(const [name,fn] of testsuite(s=>createHash("sha256").update(s,"utf8").digest("hex"),"1657b2caa6b1a955319538ce3a8b2906398599b3"))test(name,fn);
