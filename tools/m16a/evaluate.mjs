import {readFileSync} from 'node:fs';
import {fileURLToPath} from 'node:url';
import {verify as verifyM03} from '../m03/certify.mjs';
const unique=a=>new Set(a).size===a.length;
const states=new Set(['APPROVED','ACTIVE','ACCEPTED','FAILED','CANCELLED','BLOCKED']);
const resourceConflict=(a,b)=>a.some(x=>b.some(y=>x===y));
const scopeConflict=(a,b)=>a.some(x=>b.some(y=>x===y||x.endsWith('/**')&&y.startsWith(x.slice(0,-3))||y.endsWith('/**')&&x.startsWith(y.slice(0,-3))));
export function evaluate(catalog, ownership={active:[]}, certifications={certificates:[]}) {
 const errors=[], items=catalog.jobs||[], departments=catalog.departments||[];
 if(catalog.schema_version!==1||!Array.isArray(catalog.jobs)||!Array.isArray(departments)) errors.push('Invalid catalog structure');
 if(!unique(items.map(j=>j.id)))errors.push('Duplicate job id');
 if(!unique(departments.map(d=>d.id)))errors.push('Duplicate department id');
 const byId=new Map(items.map(j=>[j.id,j]));const dept=new Map(departments.map(d=>[d.id,d]));
 for(const j of items){
  if(!j||typeof j.id!=='string'||!/^M16-[A-Z0-9-]+$/.test(j.id)||typeof j.department!=='string'||!dept.has(j.department)||!states.has(j.status)||!Array.isArray(j.depends_on)||!Array.isArray(j.scopes)||!Array.isArray(j.resources)||!['github_workflow','human_chat'].includes(j.executor)||!j.acceptance_policy||typeof j.acceptance_policy!=='string') {errors.push('Invalid job: '+String(j?.id));continue;}
  if(j.executor==='github_workflow'&&!dept.get(j.department).capabilities?.includes('github_workflow'))errors.push('Unsupported executor: '+j.id);
  for(const x of j.depends_on){if(!x||typeof x.job_id!=='string'||!byId.has(x.job_id)&&!x.certificate_id||!x.required_gate)errors.push('Invalid dependency: '+j.id);}
 }
 const visiting=new Set(),visited=new Set();
 function walk(id){if(visiting.has(id)){errors.push('Dependency cycle: '+id);return;}if(visited.has(id))return;visiting.add(id);for(const d of byId.get(id)?.depends_on||[])if(byId.has(d.job_id))walk(d.job_id);visiting.delete(id);visited.add(id);}
 for(const j of items)walk(j.id);
 if(errors.length)return {ok:false,errors};
 const certs=new Map((certifications.certificates||[]).map(c=>[c.id,c]));
 const result=items.map(j=>{
  const reasons=[];const current=['ACTIVE','ACCEPTED','FAILED','CANCELLED'].includes(j.status);
  if(!current)for(const d of j.depends_on){
   const c=certs.get(d.certificate_id||byId.get(d.job_id)?.certificate_id);
   if(!c||c.status!=='ACCEPTED'||!c.gates||c.gates[d.required_gate]!=='PASS'||c.issued_by===j.id||(certifications.schema_version===1&&(!verifyM03(c)||c.issuer===j.id||c.candidate?.id!==(d.job_id||c.candidate?.id))))reasons.push('DEPENDENCY_UNCERTIFIED:'+d.job_id);
  }
  if(!current)for(const w of ownership.active||[]){
   if(resourceConflict(j.resources,w.resources||[])||scopeConflict(j.scopes,w.scope||[]))reasons.push('OWNER_CONFLICT:'+w.id);
  }
  let readiness=current?j.status:reasons.length?'BLOCKED':j.executor==='human_chat'?'HANDOFF':'READY';
  return {id:j.id,department:j.department,executor:j.executor,readiness,reasons};
 });
 const summary=Object.fromEntries(['READY','HANDOFF','BLOCKED','ACTIVE','ACCEPTED','FAILED','CANCELLED'].map(s=>[s,result.filter(x=>x.readiness===s).length]));
 return {ok:true,results:result,summary};
}
export function render(report){if(!report.ok)return 'M16-A VALIDATION FAIL\n'+report.errors.join('\n');return ['M16-A DIRECCION GENERAL',...Object.entries(report.summary).map(([k,v])=>k+': '+v),...report.results.map(j=>j.id+' | '+j.department+' | '+j.readiness+(j.reasons.length?' | '+j.reasons.join(', '):''))].join('\n');}
if(process.argv[1]&&fileURLToPath(import.meta.url)===process.argv[1]){const [jobs,owners,certs]=process.argv.slice(2);if(!jobs){console.error('Usage: node tools/m16a/evaluate.mjs <catalog.json> [workstreams.json] [certificates.json]');process.exit(2);}const read=p=>p?JSON.parse(readFileSync(p,'utf8')):{};const r=evaluate(read(jobs),read(owners),read(certs));console.log(render(r));if(!r.ok)process.exitCode=1;}
