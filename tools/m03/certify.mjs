import {createHash} from 'node:crypto';
import {readFileSync} from 'node:fs';
import {fileURLToPath} from 'node:url';
export const GATES=['TECH','FUNCTIONAL','PUBLISHED','MOBILE_INTERACTION','VISUAL','EXPERIENCE_REVIEW'];
export const RESULTS=['PASS','FAIL','NOT_VERIFIED','BLOCKED','NOT_APPLICABLE'];
const obj=x=>x!==null&&typeof x==='object'&&!Array.isArray(x);
const str=x=>typeof x==='string'&&x.length>0;
const sha=x=>str(x)&&/^[a-f0-9]{40}$/i.test(x),digest=x=>str(x)&&/^[a-f0-9]{64}$/i.test(x);
const date=x=>str(x)&&!Number.isNaN(Date.parse(x))&&/Z$|[+-]\d\d:\d\d$/.test(x);
export const hash=x=>createHash('sha256').update(JSON.stringify(x)).digest('hex');
export function validate(candidate,policy,evidence){
 const errors=[];
 if(!obj(candidate)||candidate.schema_version!==1||!str(candidate.id)||!sha(candidate.source_sha)||!str(candidate.build_id)||!digest(candidate.build_sha256)||!str(candidate.artifact_id)||!str(candidate.executor_id)||!date(candidate.built_at))errors.push('INVALID_CANDIDATE');
 if(!obj(policy)||policy.schema_version!==1||!str(policy.id)||!Number.isInteger(policy.version)||policy.version<1||!date(policy.frozen_at)||!sha(policy.frozen_for_sha)||!Array.isArray(policy.required_gates)||!obj(policy.requirements))errors.push('INVALID_POLICY');
 if(!Array.isArray(evidence))errors.push('INVALID_EVIDENCE_SET');
 if(errors.length)return errors;
 if(policy.frozen_for_sha!==candidate.source_sha||Date.parse(policy.frozen_at)>Date.parse(candidate.built_at))errors.push('POLICY_NOT_FROZEN');
 if(!policy.required_gates.every(g=>GATES.includes(g))||new Set(policy.required_gates).size!==policy.required_gates.length)errors.push('INVALID_REQUIRED_GATES');
 for(const g of GATES){const r=policy.requirements[g];if(!obj(r)||typeof r.required!=='boolean'||!Array.isArray(r.scenarios)||!r.scenarios.every(str)||new Set(r.scenarios).size!==r.scenarios.length)errors.push('INVALID_RULE:'+g);}
 for(const g of policy.required_gates)if(!policy.requirements[g]?.required)errors.push('REQUIRED_GATE_CONTRADICTION:'+g);
 const ids=new Set();
 for(const e of evidence){if(!obj(e)||!str(e.id)||!GATES.includes(e.gate)||!str(e.scenario)||!RESULTS.includes(e.result)||!date(e.observed_at)||!date(e.expires_at)||!sha(e.source_sha)||!str(e.build_id)||!digest(e.build_sha256)||!str(e.artifact_id)||!str(e.producer_id)||!str(e.run_id)||!str(e.reference)||!['CI','INDEPENDENT_REVIEW','VERIFIED_DEPLOY'].includes(e.provenance))errors.push('INVALID_EVIDENCE');if(e?.id&&ids.has(e.id))errors.push('DUPLICATE_EVIDENCE:'+e.id);ids.add(e?.id);}
 return errors;
}
export function evaluate({candidate,policy,evidence,now='2026-10-08T12:00:00Z',issuer='independent-m03'}){
 const errors=validate(candidate,policy,evidence),gates=Object.fromEntries(GATES.map(g=>[g,'NOT_VERIFIED'])),insufficient=[];
 if(!date(now)||!str(issuer))errors.push('INVALID_EVALUATION_CONTEXT');
 if(errors.length)return {schema_version:1,status:'REJECTED',gates,errors,insufficient};
 const usable=[];
 for(const e of evidence){
  if(e.source_sha!==candidate.source_sha||e.build_id!==candidate.build_id||e.build_sha256!==candidate.build_sha256||e.artifact_id!==candidate.artifact_id){errors.push('CANDIDATE_MISMATCH:'+e.id);continue;}
  if(Date.parse(e.expires_at)<=Date.parse(now)||Date.parse(e.observed_at)>Date.parse(now)){errors.push('EXPIRED_OR_FUTURE_EVIDENCE:'+e.id);continue;}
  if(e.producer_id===candidate.executor_id||e.producer_id===issuer){errors.push('SELF_CERTIFICATION:'+e.id);continue;}
  if(e.gate==='PUBLISHED'&&(!obj(e.publication)||!str(e.publication.deployment_id)||e.publication.observed_build_sha256!==candidate.build_sha256||e.publication.observed_source_sha!==candidate.source_sha)){errors.push('PUBLICATION_MISMATCH:'+e.id);continue;}
  usable.push(e);
 }
 for(const g of GATES){const rule=policy.requirements[g];if(!rule.required){gates[g]='NOT_APPLICABLE';continue;}
  const rows=rule.scenarios.map(s=>usable.filter(e=>e.gate===g&&e.scenario===s));
  if(rows.some(a=>a.some(e=>e.result==='FAIL'))){gates[g]='FAIL';continue;}
  if(rows.some(a=>a.some(e=>e.result==='BLOCKED'))){gates[g]='BLOCKED';continue;}
  if(rows.some(a=>!a.some(e=>e.result==='PASS'))){gates[g]='NOT_VERIFIED';insufficient.push(g);continue;}
  gates[g]='PASS';
 }
 const status=errors.length?'REJECTED':GATES.some(g=>gates[g]==='FAIL')?'REJECTED':GATES.some(g=>gates[g]==='BLOCKED')?'BLOCKED':policy.required_gates.every(g=>gates[g]==='PASS')?'ACCEPTED':'BLOCKED';
 return {schema_version:1,status,gates,errors,insufficient,candidate_id:candidate.id,source_sha:candidate.source_sha,build_sha256:candidate.build_sha256,policy_id:policy.id,policy_version:policy.version,policy_digest:hash(policy),evidence_digest:hash(evidence),issuer,evaluated_at:now};
}
export function certify(input){const r=evaluate(input);return {...r,certificate_id:r.candidate_id?'m03-'+hash([r.candidate_id,r.build_sha256,r.policy_digest,r.evidence_digest]).slice(0,24):null,candidate:input.candidate,policy:input.policy,evidence:input.evidence};}
export function verify(c){if(!obj(c)||c.schema_version!==1||!obj(c.candidate)||!obj(c.policy)||!Array.isArray(c.evidence)||!date(c.evaluated_at)||!str(c.issuer))return false;const r=certify({candidate:c.candidate,policy:c.policy,evidence:c.evidence,now:c.evaluated_at,issuer:c.issuer});return r.status==='ACCEPTED'&&['certificate_id','status','source_sha','build_sha256','policy_digest','evidence_digest','candidate_id'].every(k=>c[k]===r[k])&&GATES.every(g=>c.gates?.[g]===r.gates[g])&&c.issuer!==c.candidate.executor_id;}
export function report(c){return ['M03 CERTIFICATION','candidate='+String(c.candidate_id),'status='+c.status,...GATES.map(g=>g+'='+c.gates[g]),'errors='+c.errors.join(','),'insufficient='+c.insufficient.join(',')].join('\n');}
if(process.argv[1]&&fileURLToPath(import.meta.url)===process.argv[1]){const p=process.argv[2];if(!p){console.error('Usage: node tools/m03/certify.mjs <input.json>');process.exit(2);}const c=certify(JSON.parse(readFileSync(p,'utf8')));console.log(JSON.stringify(c,null,2));console.error(report(c));if(c.status!=='ACCEPTED')process.exitCode=1;}
