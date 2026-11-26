import {test} from "node:test";
import {strict as assert} from "node:assert";
import {createHash} from "node:crypto";
import {packetFor,requestFor,unwrap,runWithOfflineRoles} from "./role-bridge.mjs";
const digest=x=>createHash("sha256").update(x,"utf8").digest("hex");
const file="candidate.json",source={[file]:'{"value":1}'};
const spec={schema:1,id:"role-bridge-test",mode:"offline",budget_usd:0,source_sha:"1657b2caa6b1a955319538ce3a8b2906398599b3",target:"TEST",objective:"Value must become 3",files:[{path:file,digest:digest(source[file])}],max_attempts:3,quality_required:false,required_checks:["value"],producer:"programmer",auditor:"independent-auditor"};
const proposal=(value)=>({target:spec.target,producer:spec.producer,strategy:"fix-value",operations:[{path:file,before:'"value":1',after:'"value":'+value}]});
const req=requestFor({spec,role:"producer",attempt:1,input:{baseline:source}});
const make=()=>packetFor(req,proposal(3),{receipt_ref:"test-receipt-1"});
test("valid typed role packet",()=>assert.deepEqual(unwrap(req,make()),proposal(3)));
test("reject stale request",()=>assert.throws(()=>unwrap({...req,request_digest:"stale"},make()),/stale/));
test("reject changed payload",()=>assert.throws(()=>unwrap(req,{...make(),payload:proposal(4)}),/digest/));
test("reject impersonation",()=>assert.throws(()=>unwrap(req,{...make(),identity:spec.auditor}),/identity/));
test("reject live transport",()=>assert.throws(()=>unwrap(req,{...make(),origin:"live_api"}),/Paid/));
test("reject paid spec",()=>assert.throws(()=>requestFor({spec:{...spec,budget_usd:1},role:"producer",attempt:1,input:{}}),/offline/));
test("reject command payload",()=>assert.throws(()=>unwrap(req,packetFor(req,{...proposal(3),shell:"echo bad"},{receipt_ref:"invalid-command"})),/typed/));
test("role bridge correction, validated digest, independent audit and receipts",async()=>{
 let stored=[];let reviewerInput;
 const result=await runWithOfflineRoles(spec,source,{
  transport:async r=>{
   let p;if(r.role==="producer")p=proposal(r.input.previous?3:2);
   else{reviewerInput=r.input;p={reviewer:spec.auditor,candidate_digest:r.input.candidate_digest,decision:"accept",quality:"not_assessed",reason:"Exact value checked",evidence_refs:["fixture-check"]}}
   return packetFor(r,p,{receipt_ref:r.role+"-"+r.attempt});
  },
  validate:({files,candidate_digest})=>[{id:"value",pass:JSON.parse(files[file]).value===3,candidate_digest,evidence:"Parsed actual candidate"}],
  checkpoint:async r=>stored.push(r)
 });
 assert.equal(result.accepted,true);
 assert.equal(result.rows.length,2);
 assert.equal(result.role_receipts.length,3);
 assert.deepEqual(result.role_receipts.map(r=>r.role),["producer","producer","auditor"]);
 assert.equal(result.api_calls,0);
 assert.equal(stored.at(-1).role_receipts.length,3);
 assert.equal(reviewerInput.previous,undefined);
 assert.equal(reviewerInput.files[file],'{"value":3}');
});
