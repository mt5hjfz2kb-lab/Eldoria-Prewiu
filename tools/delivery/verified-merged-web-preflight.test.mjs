import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import {verifyConfig,verifyOwnership,verifyRuns,cfgPath} from './verified-merged-web-preflight.mjs';
const c=JSON.parse(fs.readFileSync(cfgPath,'utf8'));
const reg={active:[{id:c.workstream_id,owner:c.owner,branch:'release/post-pr163-apk-webgl-20261107',status:'active',ownership_state:'claimed',write_scope_released:false,ownership_revision:c.ownership_revision,resources:[]}]};
const n={status:'completed',conclusion:'success',head_sha:c.provenance.source_sha,head_branch:'fix/player-experience-audit-20261106',path:'.github/workflows/valoria-reference-clean.yml'};
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
test('runtime changes are rejected; only the exact certified Editor assertion is reusable',async()=>{
 const make=(sha)=>({tree:[{path:'EldoriaV41NativeCapture.cs',type:'blob',mode:'100644',sha}]});
 const original=make('old'), repair=make('8e4918b2b7a6e72d31cd9f9c7105be9d124d2dce');
 await verifyUnityTrees('a','b',async s=>s==='a'?original:repair,'Assets/Eldoria/Scripts/Editor/');
 await assert.rejects(verifyUnityTrees('a','b',async s=>s==='a'?original:make('another-repair'),'Assets/Eldoria/Scripts/Editor/'));
 await assert.rejects(verifyUnityTrees('a','b',async s=>s==='a'?original:repair,'Assets/Eldoria/Scripts/Domain/'));
 await assert.rejects(verifyUnityTrees('a','b',async s=>s==='a'?original:{tree:[]},'Assets/Eldoria/Scripts/Editor/'));
});
test('promotion requires actual native capture plus EditMode and PlayMode, merged repair and exact package',()=>{
 const fix={merged:true,head:{sha:c.postmerge_ui_source}};
 const cert={status:'completed',conclusion:'success',path:'.github/workflows/ui-progression-cert.yml',head_sha:fix.head.sha};
 const jobs={jobs:[{name:'certify',conclusion:'success',steps:['V4.1 real controls, persistent mailbox and approved-source captures','EditMode','PlayMode — UI and progression contract'].map(name=>({name,conclusion:'success'}))}]};
 const prepared={status:'completed',conclusion:'success',head_sha:c.prepared_web.source_sha,path:'.github/workflows/eldoria-post-pr163-web-release.yml'};
 const artifact={id:c.prepared_web.artifact_id,name:c.prepared_web.artifact_name,digest:c.prepared_web.artifact_digest,workflow_run:{id:c.prepared_web.run_id},expired:false};
 assert.doesNotThrow(()=>verifyPromotion(c,fix,cert,jobs,prepared,artifact));
 for(const bad of [{...fix,merged:false},{...fix,head:{sha:'wrong'}}])assert.throws(()=>verifyPromotion(c,bad,cert,jobs,prepared,artifact));
 assert.throws(()=>verifyPromotion(c,fix,{...cert,conclusion:'failure'},jobs,prepared,artifact));
 assert.throws(()=>verifyPromotion(c,fix,cert,{jobs:[]},prepared,artifact));
 assert.throws(()=>verifyPromotion(c,fix,cert,{jobs:[{...jobs.jobs[0],steps:jobs.jobs[0].steps.slice(1)}]},prepared,artifact));
 assert.throws(()=>verifyPromotion(c,fix,cert,jobs,prepared,{...artifact,digest:'wrong'}));
});
