import {createHash} from 'node:crypto';
export const STATES=['NEW','OPEN','ASSIGNED','BLOCKED','FIX_CANDIDATE','VERIFYING','RESOLVED','REOPENED'];
export const KINDS=['GAME','RENDERER','ASSET','PUBLISHED','PROBE','INFRA','UNKNOWN'];
export const fingerprint=x=>createHash('sha256').update(JSON.stringify([x.subsystem,x.scenario,x.error_code,x.failure_signature,x.kind])).digest('hex');
export const policy=Object.freeze({schema_version:1,priority:{P0:0,P1:1,P2:2,P3:3},severity:{fatal:'P0',blocking:'P1',degraded:'P2',minor:'P3'},departments:{GAME:'gameplay',RENDERER:'rendering',ASSET:'visual-art',PUBLISHED:'publishing',PROBE:'qa-automation',INFRA:'platform',UNKNOWN:'triage'}});
const nonempty=x=>typeof x==='string'&&x.length>0;
const sha=x=>nonempty(x)&&/^[a-f0-9]{40}$/i.test(x);
const allowed=new Set(['NEW>OPEN','OPEN>ASSIGNED','OPEN>BLOCKED','ASSIGNED>BLOCKED','BLOCKED>ASSIGNED','ASSIGNED>FIX_CANDIDATE','FIX_CANDIDATE>VERIFYING','VERIFYING>RESOLVED','VERIFYING>BLOCKED','RESOLVED>REOPENED','REOPENED>ASSIGNED','REOPENED>BLOCKED','BLOCKED>FIX_CANDIDATE','OPEN>FIX_CANDIDATE']);
export function validateFinding(f){
 if(!f||f.schema_version!==1||!nonempty(f.id)||!nonempty(f.subsystem)||!nonempty(f.scenario)||!nonempty(f.error_code)||!nonempty(f.failure_signature)||!sha(f.source_sha)||!nonempty(f.run_id)||!nonempty(f.artifact_id)||!nonempty(f.evidence_ref)||!nonempty(f.observed_at)||!['fatal','blocking','degraded','minor'].includes(f.impact)||!KINDS.includes(f.kind)||!['product','probe','unknown'].includes(f.origin)||!Number.isFinite(Date.parse(f.observed_at)))throw Error('INVALID_FINDING');
 if(f.origin==='probe'&&f.kind!=='PROBE'&&f.kind!=='INFRA')throw Error('PROBE_MISATTRIBUTED');
 if(/[\u0000-\u001f]/.test(f.error_code+f.failure_signature)||f.failure_signature.length>1024||f.error_code.length>128)throw Error('UNSAFE_FINDING');
 return true;
}
export function classify(f){validateFinding(f);return {kind:f.kind,department:policy.departments[f.kind],priority:policy.severity[f.impact],fingerprint:fingerprint(f),diagnosis:'UNCONFIRMED'};}
export function ingest(registry,f,ownership={active:[]}){
 validateFinding(f);if(!registry||registry.schema_version!==1||!Array.isArray(registry.incidents))throw Error('INVALID_REGISTRY');
 const result=classify(f);const current=registry.incidents.find(x=>x.fingerprint===result.fingerprint);
 const observation={id:f.id,source_sha:f.source_sha,run_id:f.run_id,artifact_id:f.artifact_id,evidence_ref:f.evidence_ref,scenario:f.scenario,result:'FAIL',observed_at:f.observed_at};
 if(current?.observations.some(x=>x.id===f.id)){const old=current.observations.find(x=>x.id===f.id);if(JSON.stringify(old)!==JSON.stringify(observation))throw Error('OBSERVATION_ID_COLLISION');return registry;}
 const incident=current?structuredClone(current):{id:'ELD-'+result.fingerprint.slice(0,16),fingerprint:result.fingerprint,kind:result.kind,subsystem:f.subsystem,scenario:f.scenario,department:result.department,priority:result.priority,status:'NEW',observations:[],history:[],policy_version:1};
 if(current?.status==='RESOLVED')incident.status='REOPENED';
 incident.observations.push(observation);
 if(policy.priority[result.priority]<policy.priority[incident.priority])incident.priority=result.priority;
 const blocked=(ownership.active||[]).some(w=>(w.resources||[]).includes(f.required_resource)|| (w.scope||[]).some(s=>(f.required_scope&&s===f.required_scope)));
 if(blocked&&incident.status!=='RESOLVED')incident.status='BLOCKED';
 incident.history.push({type:current?'REPEATED':'CREATED',observation_id:f.id,at:f.observed_at,status:incident.status});
 return {schema_version:1,incidents:[...registry.incidents.filter(x=>x.id!==incident.id),incident]};
}
export function transition(registry,id,to,{at,actor,proof,certificate}={}){
 if(!STATES.includes(to)||!nonempty(at)||!nonempty(actor))throw Error('INVALID_TRANSITION');
 const incident=registry.incidents.find(x=>x.id===id);if(!incident)throw Error('INCIDENT_NOT_FOUND');
 if(!allowed.has(incident.status+'>'+to))throw Error('ILLEGAL_TRANSITION');
 if(to==='VERIFYING'&&!proof?.candidate_sha)throw Error('MISSING_FIX_CANDIDATE');
 if(to==='RESOLVED'){
  if(!proof||!certificate||certificate.verified!==true||certificate.status!=='ACCEPTED'||certificate.source_sha!==proof.candidate_sha||certificate.gates?.[proof.required_gate]!=='PASS'||certificate.issuer===proof.executor_id||!proof.scenario_verified||proof.scenario!==incident.scenario||!proof.retest_ref)throw Error('NOT_RECERTIFIED');
 }
 const updated=structuredClone(incident);updated.status=to;updated.history.push({type:'TRANSITION',from:incident.status,to,at,actor,proof_ref:proof?.retest_ref||null});
 return {schema_version:1,incidents:registry.incidents.map(x=>x.id===id?updated:x)};
}
export function forM16(registry,ownership={active:[]}){return registry.incidents.map(i=>({incident_id:i.id,department:i.department,priority:i.priority,status:i.status,blocked:i.status==='BLOCKED',ready_for_assignment:['NEW','OPEN','REOPENED'].includes(i.status),owner_conflict:(ownership.active||[]).some(x=>x.id===i.owner_workstream)}));}
export function executiveReport(registry){const a=registry.incidents;return {schema_version:1,total:a.length,new:a.filter(x=>x.status==='NEW').length,critical:a.filter(x=>['P0','P1'].includes(x.priority)&&x.status!=='RESOLVED').length,repeated:a.filter(x=>x.observations.length>1).length,blocked:a.filter(x=>x.status==='BLOCKED').length,verifying:a.filter(x=>x.status==='VERIFYING').length,resolved:a.filter(x=>x.status==='RESOLVED').length,by_department:Object.fromEntries([...new Set(a.map(x=>x.department))].map(d=>[d,a.filter(x=>x.department===d).length]))};}
