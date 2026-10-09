import {verify as verifyM03} from './certify.mjs';
import {evaluate as m16} from '../m16a/evaluate.mjs';
export async function githubGet(url,{token=process.env.GITHUB_TOKEN,fetcher=fetch}={}){
 const u=new URL(url);
 if(u.protocol!=='https:'||u.host!=='api.github.com'||!/^\/repos\/[^/]+\/[^/]+\//.test(u.pathname))throw Error('DISALLOWED_GITHUB_URL');
 const r=await fetcher(u.href,{method:'GET',headers:{Accept:'application/vnd.github+json',...(token?{Authorization:'Bearer '+token}:{})}});
 if(!r.ok)throw Error('GITHUB_HTTP_'+r.status);
 return r.json();
}
export async function verifyGitHub(c,{repository,reader=githubGet}={}){
 const failures=[];
 if(!/^[\w.-]+\/[\w.-]+$/.test(repository||''))return {verified:false,failures:['INVALID_REPOSITORY']};
 if(!verifyM03(c))return {verified:false,failures:['INVALID_LOCAL_CERTIFICATE']};
 const prefix='https://api.github.com/repos/'+repository;
 const get=path=>reader(prefix+path);
 const sha=c.candidate.source_sha,artifact=String(c.candidate.artifact_id);
 try{
  const commit=await get('/commits/'+sha);
  if(commit.sha!==sha)failures.push('COMMIT_MISMATCH');
  for(const id of new Set(c.evidence.map(e=>String(e.run_id)))){
   if(!/^\d+$/.test(id)){failures.push('BAD_RUN_ID');continue;}
   const run=await get('/actions/runs/'+id);
   if(String(run.id)!==id||run.head_sha!==sha||run.status!=='completed'||run.conclusion!=='success'||run.repository?.full_name!==repository)failures.push('RUN_MISMATCH_OR_NOT_SUCCESS:'+id);
   const response=await get('/actions/runs/'+id+'/artifacts?per_page=100');
   const a=(response.artifacts||[]).find(x=>String(x.id)===artifact);
   if(!a||a.expired||a.workflow_run?.id!==Number(id)||a.workflow_run?.head_sha!==sha||a.digest!=='sha256:'+c.candidate.build_sha256||response.total_count>100)failures.push('ARTIFACT_MISMATCH_OR_EXPIRED:'+id);
  }
  for(const e of c.evidence.filter(e=>e.gate==='PUBLISHED'&&e.result==='PASS')){
   const deployments=await get('/deployments?sha='+sha+'&per_page=100');
   const found=deployments.find(d=>String(d.id)===String(e.publication?.deployment_id)&&d.sha===sha);
   if(!found)failures.push('DEPLOYMENT_MISMATCH');
   else {const states=await get('/deployments/'+found.id+'/statuses?per_page=100');if(!states.some(s=>s.state==='success'))failures.push('DEPLOYMENT_NOT_SUCCESS');}
   failures.push('PUBLISHED_BYTES_NOT_VERIFIED'); // API metadata cannot attest browser-observed bytes
  }
 }catch(e){failures.push('PROVIDER_ERROR:'+e.message)}
 return {verified:failures.length===0,failures,candidate_id:c.candidate.id,certificate_id:c.certificate_id,repository};
}
export async function evaluateM16Verified(catalog,ownership,certificates,options){
 const accepted=[];
 for(const c of certificates){const v=await verifyGitHub(c,options);if(v.verified)accepted.push(c);}
 return m16(catalog,ownership,{schema_version:1,certificates:accepted});
}
