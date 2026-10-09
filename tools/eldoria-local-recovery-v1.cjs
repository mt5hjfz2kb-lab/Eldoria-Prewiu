'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const ID='eldoria-local-agent-preflight-v1',OWNER='chat-work-local-recovery-20261009';
const ALIASES={'windows-runner-heavy':'windows-runner-heavy','windows-self-hosted-unity-6000-3-23f1':'windows-runner-heavy'};
function normalize(r,aliases=ALIASES){return aliases[r]||r;}
function acceptedAliases(){
 const policy=path.resolve('pipeline/local-runner-resource-aliases.json');
 if(!fs.existsSync(policy))return ALIASES; // Conservative bootstrap before first independent acceptance.
 const aliases=JSON.parse(fs.readFileSync(policy,'utf8')).aliases;
 assert.deepEqual(aliases,ALIASES,'Accepted resource policy corrupted');return aliases;
}
function conflicts(reg,id,aliases=acceptedAliases()){return reg.active.filter(w=>w.id!==id&&w.status==='active'&&(w.resources||[]).some(r=>normalize(r,aliases)==='windows-runner-heavy'));}
function authorize(reg,request,sha,taskIndex=0){
 assert.match(sha,/^[a-f0-9]{40}$/);assert.equal(request.mode,'local-recovery-v1');
 assert.equal(request.owner,OWNER);assert.equal(request.workstream_id,ID);
 assert.equal(request.model,'qwen2.5-coder:3b');assert.equal(request.budget_eur,0);
 assert.equal(request.max_attempts,2);assert(Date.parse(request.expires_at)>Date.now(),'Order expired');
 const w=reg.active.find(w=>w.id===ID);assert(w&&w.owner===OWNER&&w.status==='active','Live ownership absent');
 const c=conflicts(reg,ID);
 assert(c.every(w=>w.id==='r2-b-strategic-choice'),'Unapproved Windows reservation');
 assert.equal(request.dg_authorized_loan.source_reservation,'r2-b-strategic-choice');
 assert.equal(request.dg_authorized_loan.authorization,'owner-executive-recovery-20261009');
 assert.equal(request.dg_authorized_loan.no_game_changes,true);
 assert.equal(request.tasks.length,2);assert([0,1].includes(taskIndex));
 const kind=['runner-resource-aliases','runner-conflict-cases'][taskIndex];assert.equal(request.tasks[taskIndex].kind,kind);
 const target=['pipeline/local-runner-resource-aliases.json','tests/fixtures/local-runner-conflicts-v1.json'][taskIndex];
 assert(w.scope.includes(target),'Candidate destination outside claim');
 return {schema_version:1,task_id:request.order_id+'-'+taskIndex,kind,department:'D03',issuer:'eldoria-coordinator-v1',owner:OWNER,workstream_id:ID,source_sha:sha,model:request.model,model_digest:'f72c60cabf6237b07f6e632b2c48d533cef25eda2efbd34bed21c5e9c01e6225',max_attempts:2,budget_eur:0,target,authorized_by:request.authorization,aliases:ALIASES,live_reservations:c.map(w=>({id:w.id,owner:w.owner,resources:w.resources})),prompt:kind==='runner-resource-aliases'?
 'Produce a runner resource alias policy used to prevent two Eldoria departments using the same Windows PC. Return only JSON with one key aliases, an object mapping BOTH windows-runner-heavy and windows-self-hosted-unity-6000-3-23f1 to windows-runner-heavy. No other keys, no commands. This is operational configuration, not prose.':
 'Produce regression data for the Eldoria Windows exclusion gate. Return a JSON object cases containing 7 distinct objects with resources (array of strings) and conflict (boolean). The Windows PC is reserved if and only if resources contains the exact string windows-runner-heavy or windows-self-hosted-unity-6000-3-23f1. Determine the conflict boolean for each of these exact resource arrays:\n[]\n["windows-runner-heavy"]\n["windows-self-hosted-unity-6000-3-23f1"]\n["github-hosted-blender"]\n["windows-runner-heavy","windows-self-hosted-unity-6000-3-23f1"]\n["linux-qa","windows-runner-heavy"]\n["linux-qa","github-hosted-blender"]\nUse only the exact identifiers in those arrays. No placeholders, prose, commands or source code.'};
}
function verify(folder,sha,run,attempt){
 const read=n=>JSON.parse(fs.readFileSync(path.join(folder,n),'utf8').replace(/^\uFEFF/,''));
 const t=read('task.json'),p=read('proof.json'),clean=read('cleanup.json');
 const bytes=fs.readFileSync(path.join(folder,'candidate.json')),candidate=JSON.parse(bytes.toString('utf8').replace(/^\uFEFF/,''));
 assert.equal(t.issuer,'eldoria-coordinator-v1');assert.equal(t.owner,OWNER);assert.equal(t.source_sha,sha);
 assert.equal(p.source_sha,sha);assert.equal(String(p.run_id),String(run));assert.equal(p.attempt,attempt);
 assert.equal(p.model,t.model);assert.equal(p.digest,t.model_digest);assert.equal(p.done,true);assert(p.eval_count>0);
 assert.equal(p.external_api_calls,0);assert.equal(p.generated_code_executed,false);
 assert.equal(clean.listener_closed,true);assert.equal(clean.owned_process_stopped,true);assert.equal(clean.lock_released,true);
 assert.equal(p.candidate_sha256,crypto.createHash('sha256').update(bytes).digest('hex'));
 let checks=0;
 if(t.kind==='runner-resource-aliases'){
   assert.deepEqual(Object.keys(candidate),['aliases']);assert.deepEqual(candidate.aliases,ALIASES);
   const cases=[{r:[],want:false},{r:['windows-runner-heavy'],want:true},{r:['windows-self-hosted-unity-6000-3-23f1'],want:true},{r:['github-hosted-blender'],want:false}];
   for(const x of cases){assert.equal(conflicts({active:[{id:'other',status:'active',resources:x.r}]},ID,candidate.aliases).length>0,x.want);checks++;}
   assert.equal(conflicts({active:[{id:ID,status:'active',resources:['windows-runner-heavy']},{id:'closed',status:'completed',resources:['windows-runner-heavy']}]},ID,candidate.aliases).length,0);checks++;
 }else{
   assert.equal(t.kind,'runner-conflict-cases');assert.deepEqual(Object.keys(candidate),['cases']);assert.equal(candidate.cases.length,7);
   const signatures=new Set();
   for(const c of candidate.cases){
     assert.deepEqual(Object.keys(c).sort(),['conflict','resources']);assert(Array.isArray(c.resources));assert(c.resources.every(r=>typeof r==='string'&&r.length<80));assert.equal(typeof c.conflict,'boolean');
     const key=[...c.resources].sort().join('|');assert(!signatures.has(key),'Duplicate coverage');signatures.add(key);
     // Independent oracle: exact known identifiers, never candidate-supplied labels or rules.
     const expected=c.resources.includes('windows-runner-heavy')||c.resources.includes('windows-self-hosted-unity-6000-3-23f1');
     assert.equal(c.conflict,expected,'Wrong conflict for resources '+JSON.stringify(c.resources)+': received '+c.conflict+', expected '+expected+'. Empty and non-Windows resource lists do not reserve the Windows PC.');assert.equal(conflicts({active:[{id:'other',status:'active',resources:c.resources}]},ID).length>0,expected);checks++;
   }
   for(const key of ['', 'windows-runner-heavy','windows-self-hosted-unity-6000-3-23f1','github-hosted-blender','windows-runner-heavy|windows-self-hosted-unity-6000-3-23f1','linux-qa|windows-runner-heavy','github-hosted-blender|linux-qa'])assert(signatures.has(key),'Missing critical coverage '+key);
 }
 return {schema_version:1,task_id:t.task_id,kind:t.kind,source_sha:sha,run_id:String(run),attempt,pass:true,independent_checks:checks,candidate_sha256:p.candidate_sha256,model:p.model,digest:p.digest,target:t.target,scope:'Operational data only; no Unity, art, gameplay or subjective visual certification'};
}
module.exports={authorize,verify,conflicts,ALIASES};
if(require.main===module){
 const [mode,...args]=process.argv.slice(2);
 if(mode==='verify'){
  let report;try{report=verify(args[0],args[1],args[2],Number(args[3]));}catch(e){report={pass:false,errors:[e.message],run_id:args[2],source_sha:args[1],attempt:Number(args[3])};try{const prior=fs.readFileSync(path.join(args[0],'candidate.json'),'utf8');if(prior.length<8000)report.rejected_candidate=JSON.parse(prior.replace(/^\uFEFF/,''));}catch{}}
  fs.writeFileSync(args[4],JSON.stringify(report,null,2));console.log(JSON.stringify(report));
 }else throw Error('Use existing M16 prepare mode to issue orders');
}

