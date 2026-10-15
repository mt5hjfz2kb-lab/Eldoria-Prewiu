import fs from 'node:fs';
import path from 'node:path';
import {checkRetry} from './nightly-core.mjs';

const root=process.env.GITHUB_WORKSPACE||process.cwd();
const policy=JSON.parse(fs.readFileSync(path.join(root,'pipeline/night-recovery-policy-v1.json'),'utf8'));
const registry=JSON.parse(fs.readFileSync(path.join(root,'pipeline/active-workstreams.json'),'utf8'));
const repo=process.env.GITHUB_REPOSITORY;
const token=process.env.GITHUB_TOKEN;
const dryRun=process.env.NIGHTLY_RECOVERY_DRY_RUN==='true';
if (!repo || !/^[a-z0-9_.-]+\/[a-z0-9_.-]+$/i.test(repo) || !token) throw Error('GitHub repository and token required');
if (policy.schema_version!==1 || policy.maximum_run_attempts!==1 || !Array.isArray(policy.tracks) || !Array.isArray(registry.active)) throw Error('Unsafe policy or missing registry');
const now=new Date().toISOString();
async function request(route,{method='GET',raw=false}={}){
  const res=await fetch('https://api.github.com/repos/'+repo+route,{
    method,headers:{'Authorization':'Bearer '+token,'Accept':'application/vnd.github+json','X-GitHub-Api-Version':'2022-11-28'},
    signal:AbortSignal.timeout(20000),redirect:'follow'
  });
  if(!res.ok)throw Error(method+' '+route+' HTTP '+res.status+' '+(await res.text()).slice(0,250));
  return raw ? res.text() : res.status===204 ? {} : res.json();
}
const findings=[];
let performed=false;
let problems=0;
let globalBusy=true;
try{
  const states=['in_progress','queued','pending','waiting','requested'];
  const pages=await Promise.all(states.map(s=>request('/actions/runs?status='+s+'&per_page=100')));
  globalBusy=pages.some(p=>p.total_count>0 || (p.workflow_runs||[]).length===100);
}catch(e){problems++;findings.push({track:'global',result:'CANNOT_VERIFY_RUNNER_IDLE',error:e.message});}
for(const track of policy.tracks){
  const row={track:track.id,branch:track.branch,checked_at:now,dry_run:dryRun};
  try{
    if(track.runner_type!=='windows' || !Array.isArray(track.workflow_names))throw Error('Track must be a named Windows workflow');
    const owner=registry.active.find(x=>x.id===track.id);
    const branch=await request('/branches/'+track.branch.split('/').map(encodeURIComponent).join('/'));
    const data=await request('/actions/runs?branch='+encodeURIComponent(track.branch)+'&per_page=100');
    const listed=(data.workflow_runs||[]).filter(x=>track.workflow_names.includes(x.name)).sort((a,b)=>Date.parse(b.created_at)-Date.parse(a.created_at));
    const latest=listed[0]||null;
    row.run_id=latest?.id||null;row.run_status=latest?.status||'none';row.run_conclusion=latest?.conclusion||null;
    row.owner=owner?.owner||null;
    let failedJobs=[],log='';
    // No automatic retry if an unrelated workflow is active anywhere in the repository.
    // Even if same workflow is still running, never dispatch a duplicate.
    const preflight=checkRetry({policy,track,owner,latest,branchSha:branch.commit.sha,globalBusy:globalBusy||performed,
      failedJobs,log,now});
    if(preflight==='NOT_EXACTLY_ONE_FAILED_JOB' && latest){
      const j=await request('/actions/runs/'+latest.id+'/jobs?per_page=100');
      if(j.total_count>100)throw Error('Too many jobs; fail closed');
      failedJobs=(j.jobs||[]).filter(x=>x.conclusion==='failure');
      if(failedJobs.length===1) {
        try{log=(await request('/actions/jobs/'+failedJobs[0].id+'/logs',{raw:true})).slice(-150000);}
        catch(e){row.log_unavailable=e.message;}
      }
    }
    const verdict=checkRetry({policy,track,owner,latest,branchSha:branch.commit.sha,
      globalBusy:globalBusy||performed,failedJobs,log,now});
    row.result=verdict;
    if(verdict==='ELIGIBLE'){
      row.job_id=failedJobs[0].id;
      if(dryRun){row.result='DRY_RUN_WOULD_RETRY';}
      else{
        // Reconfirm exact branch HEAD and no concurrent work before mutating any job.
        const after=await request('/branches/'+track.branch.split('/').map(encodeURIComponent).join('/'));
        const busyPages=await Promise.all(['in_progress','queued','pending','waiting','requested'].map(s=>request('/actions/runs?status='+s+'&per_page=100')));
        if(after.commit.sha!==branch.commit.sha||busyPages.some(p=>p.total_count>0||(p.workflow_runs||[]).length===100)){
          row.result='PRE_DISPATCH_RACE_STOP';
        }else{
          await request('/actions/jobs/'+row.job_id+'/rerun',{method:'POST'});
          row.result='ONE_TRANSIENT_JOB_RETRIED';
          performed=true;
        }
      }
    }
  }catch(e){row.result='MONITOR_ERROR';row.error=e.message;problems++;}
  findings.push(row);
}
const report={schema_version:1,checked_at:now,night_window:'22:00-08:00 Europe/Madrid',
 message:'ChatGPT chat sessions cannot be reconnected by GitHub. No source edits, aesthetic approvals, purchases or arbitrary production dispatch.',findings,errors:problems};
const dest=process.env.NIGHTLY_RECOVERY_OUTPUT||'night-recovery-report.json';
fs.writeFileSync(path.join(root,dest),JSON.stringify(report,null,2)+'\n');
if(process.env.GITHUB_STEP_SUMMARY){
  const s=['## Eldoria bounded night recovery','This is not automatic ChatGPT session reconnection or artistic approval.','','| Branch | Latest run | Gate / action |','| --- | --- | --- |'];
  for(const f of findings)s.push('| '+f.branch+' | '+(f.run_id||'—')+' | '+f.result+' |');
  fs.appendFileSync(process.env.GITHUB_STEP_SUMMARY,s.join('\n')+'\n');
}
console.log(JSON.stringify(report,null,2));
if(problems)process.exitCode=1;
