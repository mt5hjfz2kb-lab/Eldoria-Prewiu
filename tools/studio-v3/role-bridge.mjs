import {createHash} from "node:crypto";
import {canonical,runMission} from "./core.mjs";

// Transport is a trusted host boundary. This module performs no network calls,
// carries no credentials and does NOT grant a paid or execution permission.
// A digest binds bytes, not a model's identity; caller authenticates receipts.
const digest=x=>createHash("sha256").update(x,"utf8").digest("hex");
function need(ok,msg){if(!ok)throw new Error(msg)}
function exactKeys(o,keys){return o!==null&&typeof o==="object"&&!Array.isArray(o)&&Object.keys(o).sort().join(",")===keys.slice().sort().join(",")}
function copy(o){return JSON.parse(JSON.stringify(o))}
export function requestFor({spec,role,attempt,input}){
 need(spec.mode==="offline"&&spec.budget_usd===0,"Only offline role transport supported");
 need(["producer","auditor"].includes(role)&&Number.isInteger(attempt)&&attempt>=1&&attempt<=spec.max_attempts,"Invalid role or attempt");
 const body={schema:1,mode:"offline",mission_id:spec.id,target:spec.target,source_sha:spec.source_sha,role,identity:spec[role],attempt,input:copy(input)};
 need(Buffer.byteLength(canonical(body),"utf8")<=1200000,"Role request too large");
 return {...body,request_digest:digest(canonical(body))};
}
export function packetFor(req,payload,{receipt_ref,identity=req.identity,origin="offline_fixture"}={}){
 need(typeof receipt_ref==="string"&&receipt_ref.length>0&&receipt_ref.length<=300,"Missing receipt reference");
 return {schema:1,origin,identity,request_digest:req.request_digest,payload:copy(payload),payload_digest:digest(canonical(payload)),receipt_ref};
}
export function unwrap(req,packet){
 need(exactKeys(packet,["schema","origin","identity","request_digest","payload","payload_digest","receipt_ref"]),"Invalid role envelope");
 need(packet.schema===1&&["offline_fixture","archived_role_output"].includes(packet.origin),"Paid or unknown transport forbidden");
 need(packet.identity===req.identity&&packet.request_digest===req.request_digest,"Wrong identity or stale response");
 need(typeof packet.receipt_ref==="string"&&packet.receipt_ref.trim().length>0&&packet.receipt_ref.length<=300,"Missing receipt");
 need(Buffer.byteLength(canonical(packet.payload),"utf8")<=64000&&packet.payload_digest===digest(canonical(packet.payload)),"Response size/digest mismatch");
 const p=packet.payload;
 if(req.role==="producer"){
  need(exactKeys(p,["target","producer","strategy","operations"])&&p.target===req.target&&p.producer===req.identity&&Array.isArray(p.operations),"Invalid typed producer response");
 }else{
  need(exactKeys(p,["reviewer","candidate_digest","decision","reason","quality","evidence_refs"])&&p.reviewer===req.identity&&p.candidate_digest===req.input.candidate_digest,"Invalid typed reviewer response");
 }
 return copy(p);
}
export async function runWithOfflineRoles(spec,source,{transport,validate,checkpoint}){
 need(typeof transport==="function"&&typeof validate==="function"&&typeof checkpoint==="function","Trusted offline callbacks required");
 let attempt=0;const receipts=[],used=new Set();
 async function ask(role,input){
  const req=requestFor({spec,role,attempt,input});
  const packet=await transport(copy(req));
  const result=unwrap(req,packet);
  need(!used.has(req.request_digest)&&!used.has(packet.receipt_ref),"Duplicate role request or receipt");
  used.add(req.request_digest);used.add(packet.receipt_ref);
  receipts.push({role,attempt,identity:packet.identity,origin:packet.origin,request_digest:req.request_digest,payload_digest:packet.payload_digest,receipt_ref:packet.receipt_ref});
  return result;
 }
 const result=await runMission(spec,source,{digest,
  propose:({baseline,previous,required_pivot_from,lessons})=>{
   attempt++;
   return ask("producer",{objective:spec.objective,required_checks:spec.required_checks,baseline,previous,required_pivot_from,lessons});
  },
  validate,
  // Reviewer receives actual candidate, baseline and tests, not producer's reasoning.
  audit:({baseline,files,checks,candidate_digest})=>ask("auditor",{objective:spec.objective,required_checks:spec.required_checks,baseline,files,checks,candidate_digest}),
  checkpoint:r=>checkpoint({...r,role_receipts:copy(receipts)})
 });
 return {...result,role_receipts:copy(receipts)};
}
