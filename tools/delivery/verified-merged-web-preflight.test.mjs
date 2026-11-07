import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import {verifyConfig,verifyOwnership,verifyRuns,cfgPath} from './verified-merged-web-preflight.mjs';
const actualConfig=JSON.parse(fs.readFileSync(cfgPath,'utf8'));
// Unit-test receipts are simulated; the real executable preflight fetches GitHub.
const c={...actualConfig,provenance:{...actualConfig.provenance,qa_digest:actualConfig.provenance.qa_digest||'sha256:'+'d'.repeat(64)}};
const reg={active:[{id:c.workstream_id,owner:c.owner,branch:'release/post-pr163-apk-webgl-20261107',status:'active',ownership_state:'claimed',write_scope_released:false,ownership_revision:c.ownership_revision,resources:[]}]};
const n={status:'completed',conclusion:'success',head_sha:c.provenance.source_sha,head_branch:'main',path:'.github/workflows/valoria-reference-clean.yml'};
const q={status:'completed',conclusion:'success',head_sha:c.provenance.qa_source_sha,head_branch:c.provenance.qa_branch,path:'.github/workflows/pr163-windows-browser-qa.yml'};
const a={name:c.provenance.native_artifact,digest:c.provenance.native_digest,expired:false,size_in_bytes:504705781};
const b={name:c.provenance.qa_artifact,digest:c.provenance.qa_digest,expired:false,size_in_bytes:50261434};
const req={run_id:c.provenance.native_run,source_sha:c.provenance.source_sha,revision:c.provenance.revision,artifact_name:c.provenance.native_artifact,allow_release:false,allow_merge:false};
test('verified baseline and owner',()=>{assert.equal(verifyConfig(c),true);assert.equal(verifyOwnership(reg,c),true);assert.equal(verifyRuns(c,n,q,a,b,req),true);});
test('reject wrong source/run/receipt',()=>{assert.throws(()=>verifyRuns(c,{...n,head_sha:'bad'},q,a,b,req));assert.throws(()=>verifyRuns(c,n,q,{...a,digest:'bad'},b,req));assert.throws(()=>verifyRuns(c,n,q,a,b,{...req,source_sha:'bad'}));});
test('reject stolen Windows lease, wrong owner and public destination',()=>{assert.throws(()=>verifyOwnership({active:reg.active.map(x=>({...x,resources:['windows-runner-heavy']}))},c));assert.throws(()=>verifyOwnership({active:reg.active.map(x=>({...x,owner:'another'}))},c));assert.throws(()=>verifyConfig({...c,web:{...c.web,restricted:false}}));});
test('fail-closed publishing while PR177 and native Android not certified',()=>{assert.throws(()=>verifyConfig({...c,android:{...c.android,publish_enabled:true}}));});

test('reject stale ownership revision after an authorized handoff',()=>{assert.throws(()=>verifyOwnership({active:reg.active.map(x=>({...x,ownership_revision:x.ownership_revision-1}))},c));});

const {verifyUnityTrees,verifyPromotion}=await import('./verified-merged-web-preflight.mjs');
test('unverified Unity changes are rejected after the fresh native build',async()=>{
 const make=(sha)=>({tree:[{path:'EldoriaV41NativeCapture.cs',type:'blob',mode:'100644',sha}]});
 await verifyUnityTrees('same','same',async()=>{throw Error('identical tree must not be fetched');});
 for(const prefix of ['Assets/Eldoria/Scripts/Editor/','Assets/Eldoria/Scripts/Domain/'])
  await assert.rejects(verifyUnityTrees('a','b',async s=>make(s),prefix));
 await assert.rejects(verifyUnityTrees('a','b',async s=>s==='a'?make('old'):{tree:[]}));
});
test('promotion requires actual native capture plus EditMode and PlayMode, merged repair and exact package',()=>{
 const promotionConfig={...c,prepared_web:c.prepared_web||{run_id:1,source_sha:'a'.repeat(40),artifact_id:2,artifact_name:'unit-test-only-package',artifact_digest:'sha256:'+ 'b'.repeat(64)}};
 const fix={merged:true,head:{sha:c.postmerge_ui_source}};
 const cert={status:'completed',conclusion:'success',path:'.github/workflows/ui-progression-cert.yml',head_sha:fix.head.sha};
 const jobs={jobs:[{name:'certify',conclusion:'success',steps:['V4.1 real controls, persistent mailbox and approved-source captures','EditMode','PlayMode — UI and progression contract'].map(name=>({name,conclusion:'success'}))}]};
 const prepared={status:'completed',conclusion:'success',head_sha:promotionConfig.prepared_web.source_sha,path:'.github/workflows/eldoria-post-pr163-web-release.yml'};
 const artifact={id:promotionConfig.prepared_web.artifact_id,name:promotionConfig.prepared_web.artifact_name,digest:promotionConfig.prepared_web.artifact_digest,workflow_run:{id:promotionConfig.prepared_web.run_id},expired:false};
 assert.doesNotThrow(()=>verifyPromotion(promotionConfig,fix,cert,jobs,prepared,artifact));
 for(const bad of [{...fix,merged:false},{...fix,head:{sha:'wrong'}}])assert.throws(()=>verifyPromotion(promotionConfig,bad,cert,jobs,prepared,artifact));
 assert.throws(()=>verifyPromotion(promotionConfig,fix,{...cert,conclusion:'failure'},jobs,prepared,artifact));
 assert.throws(()=>verifyPromotion(promotionConfig,fix,cert,{jobs:[]},prepared,artifact));
 assert.throws(()=>verifyPromotion(promotionConfig,fix,cert,{jobs:[{...jobs.jobs[0],steps:jobs.jobs[0].steps.slice(1)}]},prepared,artifact));
 assert.throws(()=>verifyPromotion(promotionConfig,fix,cert,jobs,prepared,{...artifact,digest:'wrong'}));
});

const {verifyPostBuildCertificate}=await import('./verified-merged-web-preflight.mjs');
test('only the reviewed Editor capture assertion blobs may differ',async()=>{
 const path='Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/';
 const make=sha=>({tree:[{path:'EarlyRetentionNativeCapture.cs',type:'blob',mode:'100644',sha}]});
 const old='64a644923fbe7849043da2f2d997051ca4cbd1d9', fresh='2dcc4813b42a197dc093389285967ea15575d8c2';
 await verifyUnityTrees('a','b',async s=>make(s==='a'?old:fresh),path);
 await assert.rejects(verifyUnityTrees('a','b',async s=>make(s==='a'?old:'unknown'),path));
 await assert.rejects(verifyUnityTrees('a','b',async s=>make(s==='a'?old:fresh),'Assets/Eldoria/Runtime/'));
});
test('capture-only source exception requires the complete real main suite',()=>{
 const cfg={capture_fix:{source_sha:'acfdad16e4e38a86010e3f5a9977800c0b941be0',run_id:42}};
 const run={head_sha:cfg.capture_fix.source_sha,head_branch:'main',status:'completed',conclusion:'success',path:'.github/workflows/ui-progression-cert.yml'};
 const names=['V4.1 real controls, persistent mailbox and approved-source captures','Real Diálogo 1 before/after — original HUD, three speakers, four formats','Verify retention reference composition and real guardian/discovery interaction','Verify three simultaneous marches in the actual integrated scene','EditMode','PlayMode — UI and progression contract'];
 const job={name:'certify',conclusion:'success',steps:names.map(name=>({name,conclusion:'success'}))};
 assert.doesNotThrow(()=>verifyPostBuildCertificate(cfg,run,{jobs:[job]}));
 assert.throws(()=>verifyPostBuildCertificate(cfg,{...run,head_sha:'old'},{jobs:[job]}));
 assert.throws(()=>verifyPostBuildCertificate(cfg,run,{jobs:[{...job,steps:job.steps.filter(s=>!s.name.startsWith('Verify retention'))}]}));
 assert.throws(()=>verifyPostBuildCertificate(cfg,run,{jobs:[{...job,conclusion:'failure'}]}));
});

test('unfinished QA permits unpublished preparation but never publication',()=>{
 const pending={...c,provenance:{...c.provenance,qa_digest:null},web:{...c.web,publish_enabled:false}};
 assert.doesNotThrow(()=>verifyConfig(pending));
 assert.throws(()=>verifyConfig({...pending,web:{...pending.web,publish_enabled:true}}));
 assert.throws(()=>verifyRuns(pending,n,{...q,status:'queued',conclusion:null},a,b,req));
 assert.throws(()=>verifyRuns(pending,n,q,a,{...b,digest:null},req));
});

test('completed delivery must disable publication explicitly',()=>{
 assert.throws(()=>verifyConfig({...c,web:{...c.web,delivery_complete:true,publish_enabled:true}}));
 assert.doesNotThrow(()=>verifyConfig({...c,web:{...c.web,delivery_complete:true,publish_enabled:false}}));
});
