import {test} from "node:test";
import {strict as assert} from "node:assert";
import {createHash} from "node:crypto";
import {execFileSync} from "node:child_process";
import {fileURLToPath} from "node:url";
import {runWithOfflineRoles,requestFor} from "./role-bridge.mjs";
const adapter=fileURLToPath(new URL("./v2-role-adapter.py",import.meta.url));
const digest=s=>createHash("sha256").update(s,"utf8").digest("hex");
const src={"candidate.json":'{"value":1}'};
const spec={schema:1,id:"v2-bridge-integration",mode:"offline",budget_usd:0,source_sha:"1657b2caa6b1a955319538ce3a8b2906398599b3",target:"TEST",objective:"Reach 3; preserve all other data — verificación",max_attempts:3,quality_required:false,required_checks:["value"],producer:"programmer",auditor:"independent-auditor",files:[{path:"candidate.json",digest:digest(src["candidate.json"])}]};
function py(req){return JSON.parse(execFileSync("python3",[adapter,"--fixture-packet"],{input:JSON.stringify(req),encoding:"utf8",timeout:10000,maxBuffer:1024*1024,env:{PATH:process.env.PATH,LANG:"C.UTF-8",PYTHONDONTWRITEBYTECODE:"1"}}))}
test("actual pinned V2 payload/report and economics code accepts typed V3 packets",async()=>{
 const r=await runWithOfflineRoles(spec,src,{transport:py,validate:({files,candidate_digest})=>[{id:"value",candidate_digest,pass:JSON.parse(files["candidate.json"]).value===3,evidence:"actual parsed value"}],checkpoint:async()=>{}});
 assert.equal(r.accepted,true);assert.equal(r.rows.length,2);
 assert.equal(r.role_receipts.length,3);assert.ok(r.role_receipts.every(x=>x.origin==="offline_fixture"));
 assert.equal(r.candidate.files["candidate.json"],'{"value":3}');
 assert.equal(r.api_calls,0);assert.equal(r.art_approved,false);
});
test("V2 adapter rejects tampered V3 request digest",()=>{
 const r=requestFor({spec,role:"producer",attempt:1,input:{baseline:src}});
 assert.throws(()=>py({...r,request_digest:"invalid"}));
});
test("V2 adapter rejects unapproved live command",()=>{
 assert.throws(()=>execFileSync("python3",[adapter,"--live"],{encoding:"utf8",timeout:10000,env:{PATH:process.env.PATH,PYTHONDONTWRITEBYTECODE:"1"}}));
});
