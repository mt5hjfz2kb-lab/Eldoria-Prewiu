// Isolated feature-branch native work: compare the same canonical live claim.
// The generic historical helper accepts main-only production dispatches.
import fs from 'node:fs';
const [snapshotFile,liveFile]=process.argv.slice(2);
const id='valoria-master-plan-layered-construction-20261016';
const branch=process.env.GITHUB_REF_NAME;
if(!['feature/bastion-iv-v-migration-20261029','feature/eldoria-web-parity-camera-20261029'].includes(branch))throw Error('Unapproved verification branch');
const resource='windows-runner-heavy';
function claim(file){
 const registry=JSON.parse(fs.readFileSync(file,'utf8').replace(/^\uFEFF/,''));
 const matches=registry.active.filter(x=>x.id===id);
 if(matches.length!==1)throw Error('Missing or duplicate canonical claim');
 const current=matches[0];
 if(current.status!=='active'||current.ownership_state!=='claimed'||current.write_scope_released===true||!current.owner||current.branch!==branch)throw Error('Canonical feature claim is not active');
 if(!Number.isSafeInteger(current.ownership_revision))throw Error('Invalid ownership revision');
 const holders=registry.active.filter(x=>x.resources?.includes(resource));
 if(holders.length!==1||holders[0].id!==id)throw Error('Exclusive Windows resource is not owned by this claim');
 return current;
}
const queued=claim(snapshotFile),live=claim(liveFile);
for(const key of ['owner','ownership_revision','scope','branch','resources'])if(JSON.stringify(queued[key])!==JSON.stringify(live[key]))throw Error('Stale handoff: '+key);
console.log('BASTION_IV_V_LIVE_LEASE_PASS',live.owner,live.ownership_revision);
