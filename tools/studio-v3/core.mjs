function req(ok,msg){if(!ok)throw new Error(msg)}

function obj(x){return x!==null&&typeof x==="object"&&!Array.isArray(x)}

function clone(x){return JSON.parse(JSON.stringify(x))}

function canonical(x){if(Array.isArray(x))return "["+x.map(canonical).join(",")+"]";if(obj(x))return "{"+Object.keys(x).sort().map(k=>JSON.stringify(k)+":"+canonical(x[k])).join(",")+"}";return JSON.stringify(x)}

function safePath(p){return typeof p==="string"&&p.length<=200&&p.split("/").every(s=>/^[A-Za-z0-9][A-Za-z0-9_.-]*$/.test(s)&&s!=="."&&s!=="..")&&/\.(json|md|txt|css|js|mjs|py|cs|shader|hlsl)$/.test(p)&&!/(^|\/)(AGENTS\.md|budget[^/]*|credentials[^/]*|secrets[^/]*|package\.json|package-lock\.json|requirements[^/]*)$/i.test(p)}

function validateSpec(s,src,hash){
 req(obj(s)&&s.schema===1&&/^[a-z0-9][a-z0-9-]{2,79}$/.test(s.id),"Invalid mission");
 req(s.mode==="offline"&&s.budget_usd===0,"Paid calls forbidden in V3 offline core");
 req(/^[a-f0-9]{40}$/.test(s.source_sha)&&obj(src)&&Array.isArray(s.files)&&s.files.length>=1&&s.files.length<=8,"Invalid source scope");
 req(typeof s.target==="string"&&s.target.length>0&&s.target.length<=100&&typeof s.objective==="string"&&s.objective.length>0&&s.objective.length<=2000,"Missing target/objective");
 req(Number.isInteger(s.max_attempts)&&s.max_attempts>=1&&s.max_attempts<=6&&typeof s.quality_required==="boolean","Invalid bounds/quality gate");
 req(Array.isArray(s.required_checks)&&s.required_checks.length>=1&&s.required_checks.length<=20&&new Set(s.required_checks).size===s.required_checks.length&&s.required_checks.every(k=>typeof k==="string"&&/^[a-z0-9_-]{1,80}$/.test(k)),"Invalid required checks");
 req(typeof s.producer==="string"&&s.producer.length>0&&typeof s.auditor==="string"&&s.auditor.length>0&&s.producer!==s.auditor,"Independent reviewer required");
 const names=s.files.map(f=>f.path);
 req(new Set(names).size===names.length&&Object.keys(src).length===names.length,"Duplicate/extra source file");
 for(const f of s.files)req(safePath(f.path)&&Object.hasOwn(src,f.path)&&typeof src[f.path]==="string"&&src[f.path].length<=250000&&typeof f.digest==="string"&&hash(src[f.path])===f.digest,"Unsafe source or digest mismatch");
 return clone(s)
}

function applyProposal(s,src,p){
 req(obj(p)&&p.target===s.target&&p.producer===s.producer&&typeof p.strategy==="string"&&/^[a-z0-9][a-z0-9-]{1,79}$/.test(p.strategy),"Producer/target/strategy mismatch");
 req(Array.isArray(p.operations)&&p.operations.length>=1&&p.operations.length<=12,"Invalid edit count");
 const files={...src};let size=0;
 for(const op of p.operations){
  req(obj(op)&&Object.keys(op).sort().join(",")==="after,before,path"&&safePath(op.path)&&Object.hasOwn(src,op.path),"Operation outside exact allowlist");
  req(typeof op.before==="string"&&op.before.length>0&&typeof op.after==="string"&&op.before!==op.after,"Invalid replacement");
  size+=op.before.length+op.after.length;req(size<=40000,"Edit budget exceeded");
  const raw=files[op.path],i=raw.indexOf(op.before);
  req(i>=0&&raw.indexOf(op.before,i+1)===-1,"Replacement must match exactly once");
  files[op.path]=raw.slice(0,i)+op.after+raw.slice(i+op.before.length);
  req(files[op.path].length<=250000,"Candidate too large");
 }
 req(canonical(files)!==canonical(src),"No actual change");
 return files
}

function verifyChecks(s,checks,d){
 req(Array.isArray(checks)&&checks.length===s.required_checks.length,"Missing validation results");
 const seen=new Set();
 for(const c of checks){
  req(obj(c)&&s.required_checks.includes(c.id)&&!seen.has(c.id)&&typeof c.pass==="boolean"&&c.candidate_digest===d&&typeof c.evidence==="string"&&c.evidence.trim().length>0&&c.evidence.length<=2000,"Invalid/unbound evidence");
  seen.add(c.id)
 }
 return checks.every(c=>c.pass)
}

function verifyAudit(s,a,d){
 req(obj(a)&&a.reviewer===s.auditor&&a.candidate_digest===d&&["accept","reject"].includes(a.decision)&&typeof a.reason==="string"&&a.reason.trim().length>0&&a.reason.length<=2000,"Invalid independent audit");
 req(["pass","fail","not_assessed"].includes(a.quality),"Invalid quality status");
 if(s.quality_required&&a.decision==="accept")req(a.quality==="pass"&&Array.isArray(a.evidence_refs)&&a.evidence_refs.length>0&&a.evidence_refs.every(x=>typeof x==="string"&&x.trim().length>0),"Quality acceptance requires evidence");
 return a.decision==="accept"&&a.quality!=="fail"&&(!s.quality_required||a.quality==="pass")
}

async function runMission(input,source,adapters){
 req(obj(adapters)&&["digest","propose","validate","audit","checkpoint"].every(k=>typeof adapters[k]==="function"),"Trusted adapters required");
 const digest=adapters.digest,s=validateSpec(input,source,digest),baseline=clone(source),baseDigest=digest(canonical(baseline)),seen=new Set([baseDigest]),failures=new Map(),rows=[],lessons=[];
 let accepted=null,previous=null,pivot=null;
 const report=(terminal)=>({schema:1,id:s.id,source_sha:s.source_sha,baseline_digest:baseDigest,terminal,accepted:!!accepted,candidate:accepted,rows,lessons,api_calls:0,published:false,merged:false,art_approved:false});
 for(let attempt=1;attempt<=s.max_attempts;attempt++){
  let phase="proposal",row={attempt},p,d=null;
  try{
   p=await adapters.propose(clone({spec:s,baseline,previous,required_pivot_from:pivot,lessons}));
   req(obj(p)&&p.producer===s.producer,"Unexpected producer");
   if(typeof p.strategy==="string")row.strategy=p.strategy;
   req((failures.get(p.strategy)??0)<2,"Repeated ineffective strategy: pivot required");
   const files=applyProposal(s,baseline,p);
   d=digest(canonical(files));row.candidate_digest=d;req(!seen.has(d),"Candidate already evaluated");seen.add(d);
   phase="validation";
   const checks=await adapters.validate(clone({spec:s,files,candidate_digest:d}));
   const tech=verifyChecks(s,checks,d);row.checks=checks;
   if(!tech){row.outcome="technical_reject";row.reason="Required check failed"}
   else{
    phase="audit";
    const a=await adapters.audit(clone({spec:s,baseline,files,checks,candidate_digest:d}));row.audit=a;
    if(verifyAudit(s,a,d)){row.outcome="accepted_candidate";row.reason=a.reason;accepted={files:clone(files),digest:d}}
    else{row.outcome="quality_reject";row.reason=a.reason}
   }
  }catch(e){row.outcome="rejected_"+phase;row.reason=String(e?.message??e).slice(0,2000)}
  if(!accepted){
   if(typeof row.strategy==="string"&&/^[a-z0-9][a-z0-9-]{1,79}$/.test(row.strategy)){
    const n=(failures.get(row.strategy)??0)+1;failures.set(row.strategy,n);
    if(n>=2){pivot=row.strategy;row.pivot_required=true}
   }
   if(d&&row.outcome==="technical_reject")lessons.push({status:"session_evidence",candidate_digest:d,strategy:row.strategy,failed_checks:row.checks.filter(c=>!c.pass).map(c=>c.id),rule:"Do not repeat unchanged candidate; address failing checks."});
   previous=clone(row)
  }
  rows.push(clone(row));await adapters.checkpoint(clone(report("in_progress")));
  if(accepted)break
 }
 const result=clone(report(accepted?"candidate_ready":"exhausted"));
 await adapters.checkpoint(clone(result));return result
}

function compare(a,b){
 req(obj(a)&&obj(b)&&a.method==="single"&&b.method==="studio","Expected two arms");
 for(const k of ["source_sha","target","objective","protocol_digest","time_budget_seconds","budget_usd","reviewer","required_checks_digest"])req(a[k]!==undefined&&a[k]===b[k],"Noncomparable: "+k);
 req(a.blind_review===true&&b.blind_review===true&&Number.isFinite(a.time_budget_seconds)&&a.time_budget_seconds>0&&Number.isFinite(a.budget_usd)&&a.budget_usd>=0,"Invalid protocol");
 for(const r of [a,b]){
  req(typeof r.accepted==="boolean"&&Number.isFinite(r.elapsed_seconds)&&r.elapsed_seconds>0&&r.elapsed_seconds<=r.time_budget_seconds&&Number.isInteger(r.interventions)&&r.interventions>=0&&Number.isInteger(r.regressions)&&r.regressions>=0,"Invalid observed metrics");
  req(r.cost_usd===null||(Number.isFinite(r.cost_usd)&&r.cost_usd>=0&&r.cost_usd<=r.budget_usd),"Invalid cost");
  req(r.quality===null||(Number.isFinite(r.quality)&&r.quality>=0&&r.quality<=10),"Invalid quality");
  req(typeof r.evidence_ref==="string"&&r.evidence_ref.length>0,"Missing evidence")
 }
 return {quality_delta:a.quality===null||b.quality===null?null:b.quality-a.quality,time_saving_fraction:(a.elapsed_seconds-b.elapsed_seconds)/a.elapsed_seconds,cost_delta_usd:a.cost_usd===null||b.cost_usd===null?null:b.cost_usd-a.cost_usd,intervention_delta:b.interventions-a.interventions,regression_delta:b.regressions-a.regressions,both_accepted:a.accepted&&b.accepted,conclusion:"One paired trial is not proof of general productivity."}
}

export {canonical,safePath,validateSpec,applyProposal,runMission,compare};
