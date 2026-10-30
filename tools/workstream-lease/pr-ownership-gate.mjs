// Read-only PR admission guard. Runs from trusted main on pull_request_target.
// Never executes, merges, closes or checks out source from a proposed PR.
import fs from 'node:fs';
const LEGACY_LAST_PR=116;
const LEGACY_UI='eldoria-ui-v3-management-completion-20261021';
export function claimId(pr){
  const match=String(pr.body||'').match(/^\s*Workstream[-_ ]?ID\s*:\s*`?([a-z0-9][a-z0-9_-]*)`?\s*$/im);
  if(match)return match[1];
  if(Number(pr.number)<=LEGACY_LAST_PR &&
    (/cr[oó]nica\s*real|cronicareal/i.test(String(pr.title||'')) ||
    /cronicareal|cronica-real/i.test(String(pr.head?.ref||''))))return LEGACY_UI;
  return null;
}
const claimed=w=>w && w.status==='active' &&
  w.ownership_state==='claimed' && typeof w.owner==='string' && w.owner.trim().length>0;
export function globMatch(glob,file){
  let expr='^';
  for(let i=0;i<glob.length;i++){
    const c=glob[i];
    if(c==='*'){
      if(glob[i+1]==='*'){expr+='.*';i++;}else expr+='[^/]*';
    }else expr+=/[\\^$+?.()|[\]{}]/.test(c)?'\\'+c:c;
  }
  return new RegExp(expr+'$').test(file);
}
const rank=p=>String(p.created_at||'9999').padEnd(24,'0')+'-'+String(p.number).padStart(10,'0');
export function evaluate({current,openPulls,registry,changedFiles=[]}){
  const errors=[],notes=[];
  if(!current||!Number.isSafeInteger(Number(current.number))||Number(current.number)<1)
    return {ok:false,errors:['PR identity missing'],notes};
  if(!Array.isArray(openPulls)||!Array.isArray(registry?.active))
    return {ok:false,errors:['Open PR list or live ownership registry missing'],notes};
  const id=claimId(current);
  if(!id){
    if(Number(current.number)>LEGACY_LAST_PR)
      errors.push('PR #'+current.number+': add Workstream-ID: <claimed canonical ID> to description before starting a second PR');
    else notes.push('Legacy unmarked PR #'+current.number+' left unchanged');
    return {ok:errors.length===0,id,errors,notes};
  }
  const workstream=registry.active.find(w=>w.id===id);
  if(!workstream||!claimed(workstream))
    errors.push('Workstream '+id+' must have one live claimed owner in main before PR admission');
  for(const other of openPulls){
    if(Number(other.number)===Number(current.number)||claimId(other)!==id)continue;
    if(rank(other)<rank(current))
      errors.push('PR #'+current.number+' duplicates canonical workstream '+id+' already active in PR #'+other.number+'; continue or hand off in the first PR');
    else notes.push('PR #'+other.number+' is a later duplicate; current PR remains canonical');
  }
  if(Number(current.number)>LEGACY_LAST_PR){
    if(!Array.isArray(changedFiles))errors.push('Incomplete changed-file list');
    for(const w of registry.active){
      if(w.id===id||!claimed(w))continue;
      const hits=changedFiles.filter(file=>(w.scope||[]).some(pattern=>globMatch(pattern,file)));
      if(!hits.length)continue;
      // A deliberate owner-authorized handoff may combine two existing
      // canonical workstreams in one PR. Protect against a DIFFERENT writer,
      // not against two claimed scopes held by the SAME live owner.
      if(claimed(workstream)&&workstream.owner===w.owner){
        notes.push('Same live owner retains responsibility for '+w.id+
          ' (shared files '+hits.slice(0,3).join(', ')+')');
        continue;
      }
      errors.push('Touched '+hits.slice(0,3).join(', ')+' in another claimed workstream '+w.id+'; reconcile ownership before proceeding');
    }
  }
  return {ok:errors.length===0,id,errors,notes};
}
async function api(path,token){
  const response=await fetch('https://api.github.com'+path,{headers:{
    Authorization:'Bearer '+token,Accept:'application/vnd.github+json',
    'X-GitHub-Api-Version':'2022-11-28','User-Agent':'eldoria-pr-owner-guard'}});
  if(!response.ok)throw Error('GitHub API HTTP '+response.status+' for '+path);
  return response.json();
}
async function paginate(path,token,limit=10){
  const all=[];
  for(let page=1;page<=limit;page++){
    const list=await api(path+(path.includes('?')?'&':'?')+'per_page=100&page='+page,token);
    if(!Array.isArray(list))throw Error('Non-array API response');
    all.push(...list);
    if(list.length<100)return all;
  }
  throw Error('GitHub PR list truncated after '+limit*100+' items; refuse incomplete decision');
}
async function main(){
  const n=Number(process.env.GUARD_PR_NUMBER),token=process.env.GH_TOKEN,repo=process.env.GITHUB_REPOSITORY;
  if(!Number.isSafeInteger(n)||n<1||!token||!/^[-\w.]+\/[-\w.]+$/.test(repo||''))
    throw Error('PR number, token or repository invalid/missing');
  const base='/repos/'+repo;
  const [current,openPulls,registryFile]=await Promise.all([
    api(base+'/pulls/'+n,token),paginate(base+'/pulls?state=open',token),
    api(base+'/contents/pipeline/active-workstreams.json?ref=main',token)]);
  if(registryFile.encoding!=='base64'||!registryFile.content)throw Error('Live registry unavailable');
  const registry=JSON.parse(Buffer.from(registryFile.content.replace(/\s/g,''),'base64').toString('utf8'));
  const changedFiles=n>LEGACY_LAST_PR?
    (await paginate(base+'/pulls/'+n+'/files',token,30)).map(f=>f.filename):[];
  const result=evaluate({current,openPulls,registry,changedFiles});
  for(const note of result.notes)console.log('PR_OWNER_NOTE:',note);
  for(const error of result.errors)console.error('PR_OWNER_FAIL:',error);
  console.log('PR_OWNER_'+(result.ok?'PASS':'FAIL')+': #'+n+', workstream='+(result.id||'legacy'));
  if(process.env.GITHUB_STEP_SUMMARY)
    fs.appendFileSync(process.env.GITHUB_STEP_SUMMARY,'### PR owner '+(result.ok?'PASS':'FAIL')+': #'+n+'\n'+
      'Workstream: '+(result.id||'legacy unmarked')+'\n\n'+
      result.errors.map(x=>'- '+x+'\n').join('')+result.notes.map(x=>'- '+x+'\n').join('')+'\n');
  if(!result.ok)process.exitCode=1;
}
if(process.argv[1]&&/pr-ownership-gate\.mjs$/.test(process.argv[1]))
  main().catch(e=>{console.error('PR_OWNER_FAIL_CLOSED:',e.message);process.exitCode=1;});
