const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),os=require('node:os'),path=require('node:path'),crypto=require('node:crypto');
const {authorize,verify,conflicts,ALIASES}=require('../tools/eldoria-local-recovery-v1.cjs');
const owner='chat-work-local-recovery-20261009',id='eldoria-local-agent-preflight-v1',sha='a'.repeat(40);
const req=()=>({mode:'local-recovery-v1',owner,workstream_id:id,model:'qwen2.5-coder:3b',budget_eur:0,max_attempts:2,expires_at:new Date(Date.now()+60000).toISOString(),authorization:'owner explicit',order_id:'recovery-v1',dg_authorized_loan:{authorization:'owner-executive-recovery-20261009',source_reservation:'r2-b-strategic-choice',no_game_changes:true},tasks:[{kind:'runner-resource-aliases'},{kind:'runner-conflict-cases'}]});
const reg=()=>({active:[{id,owner,status:'active',scope:['pipeline/local-runner-resource-aliases.json','tests/fixtures/local-runner-conflicts-v1.json']},{id:'r2-b-strategic-choice',status:'active',resources:['windows-runner-heavy']}]});
test('M16 recognizes both real Windows resource names',()=>{for(const alias of Object.keys(ALIASES))assert.equal(conflicts({active:[{id:'other',status:'active',resources:[alias]}]},id).length,1)});
test('task requires current owner and authorized destination',()=>{let r=reg();r.active[0].owner='someone';assert.throws(()=>authorize(r,req(),sha),/ownership/);r=reg();r.active[0].scope=[];assert.throws(()=>authorize(r,req(),sha),/destination/)});
test('other productive reservations cannot be borrowed',()=>{const r=reg();r.active.push({id:'art',status:'active',resources:['windows-self-hosted-unity-6000-3-23f1']});assert.throws(()=>authorize(r,req(),sha),/reservation/)});
test('expiry and budget cannot silently expand',()=>{let q=req();q.expires_at='2000-01-01';assert.throws(()=>authorize(reg(),q,sha),/expired/);q=req();q.budget_eur=1;assert.throws(()=>authorize(reg(),q,sha))});
test('independent verifier rejects forged hash, SHA and cleanup',()=>{
 const dir=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-local-qa-'));
 try{
  const task=authorize(reg(),req(),sha),candidate=Buffer.from(JSON.stringify({aliases:ALIASES}));
  const proof={source_sha:sha,run_id:'1',attempt:1,model:task.model,digest:task.model_digest,done:true,eval_count:20,external_api_calls:0,generated_code_executed:false,candidate_sha256:crypto.createHash('sha256').update(candidate).digest('hex')};
  const clean={listener_closed:true,owned_process_stopped:true,lock_released:true};
  const save=(n,v)=>fs.writeFileSync(path.join(dir,n),JSON.stringify(v));
  save('task.json',task);save('proof.json',proof);save('cleanup.json',clean);fs.writeFileSync(path.join(dir,'candidate.json'),candidate);
  assert.equal(verify(dir,sha,'1',1).pass,true);
  save('cleanup.json',{...clean,listener_closed:false});assert.throws(()=>verify(dir,sha,'1',1));save('cleanup.json',clean);
  save('proof.json',{...proof,candidate_sha256:'fake'});assert.throws(()=>verify(dir,sha,'1',1));save('proof.json',proof);
  assert.throws(()=>verify(dir,'b'.repeat(40),'1',1));assert.throws(()=>verify(dir,sha,'2',1));
 }finally{fs.rmSync(dir,{recursive:true,force:true})}
});
test('independent model-produced regression data contributes actual gate coverage when present',{skip:!fs.existsSync(path.resolve(__dirname,'fixtures/local-runner-conflicts-v1.json'))},()=>{
 const f=path.resolve(__dirname,'fixtures/local-runner-conflicts-v1.json');
 const {cases}=JSON.parse(fs.readFileSync(f,'utf8'));
 for(const c of cases)assert.equal(conflicts({active:[{id:'other',status:'active',resources:c.resources}]},id).length>0,c.conflict);
 assert.equal(cases.length,7);
});

