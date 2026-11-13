// Eldoria area handoff: opt-in, fail-closed. Does not cancel work, run Unity,
// infer human approval, merge a PR, or touch a production artifact.
import fs from 'node:fs';
import {createHash} from 'node:crypto';

const BUSY = ['queued', 'in_progress', 'pending', 'waiting', 'requested'];
const ID_PATTERN = /^[a-z0-9][a-z0-9_-]+$/;
const SHA_PATTERN = /^[0-9a-f]{40}$/i;

export function decideAreaRelease(registry, {run, branchHead, busyRuns} = {}) {
  if (!Array.isArray(registry?.active)) throw Error('Canonical active registry missing');
  const targets = registry.active.filter(w => w.area_handoff_request);
  if (!targets.length) return {state:'NO_REQUEST',reason:'No explicit area completion was requested'};
  if (targets.length > 1) return {state:'BLOCKED',reason:'Multiple requests: process only one explicit release at a time'};
  const w = targets[0], q = w.area_handoff_request;
  if (registry.active.filter(x => x.id === w.id).length !== 1 ||
      !ID_PATTERN.test(String(w.id)) || !q || q.status !== 'ready' ||
      q.completed !== true || typeof q.area !== 'string' || !q.area.trim())
    return {state:'BLOCKED',reason:'Invalid or incomplete area handoff request'};
  if (w.status !== 'active' || w.ownership_state !== 'claimed' ||
      w.write_scope_released === true || !w.owner || w.owner !== q.owner ||
      !Number.isSafeInteger(w.ownership_revision) ||
      w.ownership_revision !== q.revision || !SHA_PATTERN.test(String(q.branch_head)) ||
      !Number.isSafeInteger(q.run_id) || q.run_id <= 0)
    return {state:'BLOCKED',reason:'Request owner/revision/source does not match the live claim'};
  const leases = registry.active.filter(x => (x.resources || []).includes('windows-runner-heavy'));
  if (leases.length > 1 || (leases.length === 1 && leases[0].id !== w.id && (w.resources||[]).includes('windows-runner-heavy')))
    return {state:'BLOCKED',reason:'Conflicting Windows runner holders; do not modify'};
  if (!branchHead || branchHead !== q.branch_head)
    return {state:'PENDING',reason:'PR branch HEAD advanced or cannot be verified; renew the request after final commit'};
  if (!run) return {state:'PENDING',reason:'Exact-head evidence run is unavailable'};
  if (Number(run.id) !== q.run_id || run.head_sha !== q.branch_head ||
      run.head_branch !== w.branch)
    return {state:'BLOCKED',reason:'Unity evidence run does not match branch and exact HEAD'};
  if (run.status !== 'completed')
    return {state:'PENDING',reason:'Unity evidence run is not yet completed'};
  if (run.conclusion !== 'success')
    return {state:'BLOCKED',reason:'Unity evidence run failed; preserve ownership for repair'};
  if (!Array.isArray(busyRuns)) return {state:'PENDING',reason:'Live Actions queue could not be observed'};
  if (busyRuns.some(x => x && x.id !== q.run_id && BUSY.includes(x.status)))
    return {state:'PENDING',reason:'A queued or executing Actions run still targets this branch'};
  return {state:'READY',workstream:w.id,area:q.area,owner:q.owner,revision:q.revision,sha:q.branch_head,run_id:q.run_id};
}

export function applyAreaRelease(registry, decision, timestamp) {
  if (decision?.state !== 'READY') throw Error('Cannot mutate ownership without READY decision');
  const result = structuredClone(registry);
  const w = result.active.find(x => x.id === decision.workstream);
  if (!w || w.owner !== decision.owner || w.ownership_revision !== decision.revision ||
      w.area_handoff_request?.branch_head !== decision.sha)
    throw Error('Ownership changed before release; abort');
  const q = w.area_handoff_request;
  const finished = {area:q.area,owner:q.owner,revision:q.revision,
    source_sha:q.branch_head,unity_run_id:q.run_id,closed_at:timestamp,
    artistic_approval:q.artistic_approval || 'not_asserted',
    note:q.note || 'Area handoff only: no game publication or PR merge'};
  w.ownership_history = Array.isArray(w.ownership_history) ? w.ownership_history : [];
  w.ownership_history.push({owner:w.owner,ownership_revision:w.ownership_revision,
    transferred_at:timestamp,reason:'Completed area '+q.area+'; explicit verified handoff to next normal chat'});
  w.last_completed_area = finished;
  w.previous_owner = w.owner;
  w.owner = null;
  w.ownership_state = 'available';
  w.write_scope_released = true;
  w.resources = [];
  w.ownership_revision += 1;
  w.phase = 'AREA_COMPLETE_READY_FOR_NEXT_CHAT';
  w.next = 'Next owner may claim canonical workstream '+w.id+' on existing PR after live queue check; preserve '+q.branch_head+' and last_completed_area.';
  w.released_at = timestamp;
  delete w.area_handoff_request;
  result.updated_at = timestamp;
  return result;
}

const apiBase = 'https://api.github.com';
async function api(url, token, options={}) {
  const resp=await fetch(apiBase+url,{...options,headers:{
    Authorization:'Bearer '+token,Accept:'application/vnd.github+json',
    'X-GitHub-Api-Version':'2022-11-28','User-Agent':'eldoria-safe-area-handoff',
    ...options.headers}});
  if(!resp.ok)throw Error('GitHub HTTP '+resp.status+' '+url);
  return resp.json();
}
export async function readRegistry(repo,token,request=api) {
  const body=await request('/repos/'+repo+'/contents/pipeline/active-workstreams.json?ref=main',token);
  if(!SHA_PATTERN.test(body.sha||''))throw Error('Missing canonical JSON blob SHA');
  // Contents omits inline base64 above 1 MB. Read the immutable blob
  // identified by that response, retaining the same SHA for the later CAS.
  const blob=body.encoding==='base64'&&body.content?body:
    await request('/repos/'+repo+'/git/blobs/'+body.sha,token);
  if(blob.sha!==body.sha||blob.encoding!=='base64'||typeof blob.content!=='string')
    throw Error('Canonical registry blob missing or mismatched');
  const bytes=Buffer.from(blob.content.replace(/\s/g,''),'base64');
  const actual=createHash('sha1').update('blob '+bytes.length+'\0').update(bytes).digest('hex');
  if(actual!==body.sha)throw Error('Canonical registry blob digest mismatch');
  return {sha:body.sha,registry:JSON.parse(bytes.toString('utf8'))};
}
async function activeRuns(repo,token,branch) {
  const runs = new Map();
  for(const status of BUSY) {
    for(let page=1;page<=5;page++) {
      const p=await api('/repos/'+repo+'/actions/runs?branch='+encodeURIComponent(branch)+
        '&status='+status+'&per_page=100&page='+page,token);
      if(!Array.isArray(p.workflow_runs))throw Error('Incomplete Actions queue response');
      for(const run of p.workflow_runs) runs.set(run.id,run);
      if(p.workflow_runs.length<100)break;
      if(page===5)throw Error('Actions queue pagination truncated: refuse release');
    }
  }
  return [...runs.values()];
}
async function execute() {
  const repo=process.env.GITHUB_REPOSITORY,token=process.env.GH_TOKEN;
  if(!/^[\w.-]+\/[\w.-]+$/.test(repo||'')||!token)throw Error('Repository/token missing');
  const {registry,sha}=await readRegistry(repo,token);
  const requested=registry.active?.filter(w=>w.area_handoff_request)||[];
  if(!requested.length){console.log('HANDOFF_NO_REQUEST');return;}
  if(requested.length!==1){console.log('HANDOFF_BLOCKED: multiple requests');return;}
  const w=requested[0],q=w.area_handoff_request;
  // Never query or mutate another owner's job. Only the explicitly requesting claim.
  let run=null,head=null,busy=null;
  if(q?.branch_head && q?.run_id && w.branch) {
    run=await api('/repos/'+repo+'/actions/runs/'+q.run_id,token);
    const encoded=w.branch.split('/').map(encodeURIComponent).join('/');
    const ref=await api('/repos/'+repo+'/git/ref/heads/'+encoded,token);
    head=ref.object?.sha;
    busy=await activeRuns(repo,token,w.branch);
  }
  const decision=decideAreaRelease(registry,{run,branchHead:head,busyRuns:busy});
  console.log('AREA_HANDOFF_'+decision.state+': '+decision.reason);
  if(process.env.GITHUB_STEP_SUMMARY)
    fs.appendFileSync(process.env.GITHUB_STEP_SUMMARY,
      '### Area handoff: '+decision.state+'\n'+(decision.workstream||w.id)+' — '+(decision.reason||decision.area)+'\n');
  if(decision.state!=='READY')return;
  const next=applyAreaRelease(registry,decision,new Date().toISOString());
  // GitHub contents API is a blob-SHA CAS; concurrent owner edits fail rather than overwrite.
  const result=await api('/repos/'+repo+'/contents/pipeline/active-workstreams.json',token,{
    method:'PUT',headers:{'Content-Type':'application/json'},
    body:JSON.stringify({message:'coordination: safely release completed area '+decision.area+' after exact Unity run',
      branch:'main',sha,content:Buffer.from(JSON.stringify(next,null,2)+'\n','utf8').toString('base64')})
  });
  console.log('HANDOFF_RELEASED: '+decision.workstream+' commit='+result.commit?.sha);
}
if(process.argv[1]&&/area-handoff\.mjs$/.test(process.argv[1]))
  execute().catch(e=>{console.error('HANDOFF_FAIL_CLOSED:',e.message);process.exitCode=1;});
