// Eldoria read-only workstream supervisor: detects stale claims without silently restarting work.
import fs from 'node:fs';
const data=JSON.parse(fs.readFileSync(process.argv[2]||'pipeline/active-workstreams.json','utf8'));
if(!Array.isArray(data.active))throw Error('INVALID_CANONICAL_REGISTRY');
const now=new Date(process.env.ELDORIA_CHECK_TIME||Date.now()).getTime();
const findings=[];
const director='D01-Direccion-General';
if(data.directorate_accountability?.owner!==director)throw Error('DIRECTORATE_ACCOUNTABILITY_MISSING');
const claims=new Map();
for(const w of data.active){
 if(!w.id||!w.owner||!['active','blocked'].includes(w.status))throw Error('INVALID_WORKSTREAM');
 if(w.accountable_director!==director || w.department_executor_owner!==w.owner) findings.push({id:w.id,status:'DIRECTORATE_UNASSIGNED',owner:w.owner,next:'Reconcile with DG without changing exclusive executor ownership',resources:w.resources||[]});
 if(w.status==='active')for(const resource of (w.resources||[])){
  if(claims.has(resource))findings.push({id:w.id,status:'RESOURCE_CONFLICT',owner:w.owner,next:'Resolve runner claim with '+claims.get(resource),resources:[resource]});
  else claims.set(resource,w.id);
 }
 if(w.status==='blocked'){findings.push({id:w.id,status:'BLOCKED',owner:w.owner,next:w.next||'',resources:w.resources||[]});continue}
 const since=Date.parse(w.claimed_at||'');if(!Number.isFinite(since))throw Error('INVALID_CLAIM_TIME');
 const hours=(now-since)/3600000;if(hours>=4)findings.push({id:w.id,status:'STALE_ACTIVE',owner:w.owner,hours:Math.floor(hours),next:w.next||'',resources:w.resources||[]});
}
const result={schema_version:1,source:'pipeline/active-workstreams.json',kind:'READ_ONLY_DG_TRIAGE',findings};
process.stdout.write(JSON.stringify(result)+'\n');