'use strict';
// Exact isolated remediation lease. Does not grant publication or process access.
const fs=require('node:fs');
function requireLease(snapshot,live){
 const id='valoria-master-plan-layered-construction-20261016';
 const owner='codex-player-experience-fixes-23287cf1c126';
 const branch='fix/player-experience-audit-20261106';
 const claim=(registry,label)=>{
  const matches=registry.active?.filter(x=>x.id===id)||[];
  if(matches.length!==1)throw Error(label+': canonical claim missing/duplicated');
  const w=matches[0];
  if(w.owner!==owner||w.branch!==branch||w.status!=='active'||w.ownership_state!=='claimed'||w.write_scope_released||
     !Number.isSafeInteger(w.ownership_revision)||w.ownership_revision<155||!w.phase?.startsWith('PLAYER_AUDIT_'))throw Error(label+': remediation ownership changed');
  const holders=registry.active.filter(x=>x.resources?.includes('windows-runner-heavy'));
  if(holders.length!==1||holders[0]!==w)throw Error(label+': exclusive Windows reservation missing');
  return w;
 };
 const queued=claim(snapshot,'Snapshot'),current=claim(live,'Live main');
 if(queued.ownership_revision!==current.ownership_revision||JSON.stringify(queued.scope)!==JSON.stringify(current.scope))throw Error('Stale remediation scope/revision');
 return {id,owner,branch,revision:current.ownership_revision};
}
module.exports={requireLease};
if(require.main===module){try{
 const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
 console.log('PLAYER_EXPERIENCE_LEASE_PASS '+JSON.stringify(requireLease(read(process.argv[2]),read(process.argv[3]))));
}catch(e){console.error(e.message);process.exitCode=1;}}
