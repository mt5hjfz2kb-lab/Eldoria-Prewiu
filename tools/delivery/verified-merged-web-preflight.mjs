import fs from 'node:fs';
import {fileURLToPath} from 'node:url';

const HEX=/^[a-f0-9]{40}$/;
const DIGEST=/^sha256:[a-f0-9]{64}$/;
const REPO='mt5hjfz2kb-lab/Eldoria-Prewiu';
export const cfgPath='pipeline/releases/pr163-apk-webgl-20261107.json';
const assert=(condition,message)=>{if(!condition)throw Error('RELEASE_GATE: '+message);};

// Native325 contains every runtime repair. Only this exact Editor-only capture
// assertion correction may differ; it never participates in a player assembly.
// The complete corrected native suite is required before publication; preparing
// an unpublished artifact can proceed while Windows QA is still running.
const CAPTURE_FIX={path:'Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/EarlyRetentionNativeCapture.cs',
 before:'64a644923fbe7849043da2f2d997051ca4cbd1d9',after:'2dcc4813b42a197dc093389285967ea15575d8c2'};
export async function verifyUnityTrees(original,current,getTree,prefix=''){
 if(original===current)return;
 const [a,b]=await Promise.all([getTree(original),getTree(current)]);
 assert(!a.truncated&&!b.truncated,'incomplete Unity tree');
 const left=new Map(a.tree.map(x=>[x.path,x]));
 const right=new Map(b.tree.map(x=>[x.path,x]));
 for(const name of new Set([...left.keys(),...right.keys()])){
  const x=left.get(name),y=right.get(name),path=prefix+name;
  assert(x&&y&&x.mode===y.mode&&x.type===y.type,'Unity path added/removed/type changed: '+path);
  if(x.sha===y.sha)continue;
  if(x.type==='tree')await verifyUnityTrees(x.sha,y.sha,getTree,path+'/');
  else assert(path===CAPTURE_FIX.path&&x.sha===CAPTURE_FIX.before&&y.sha===CAPTURE_FIX.after,'uncertified Unity change: '+path);
 }
}

export function verifyPromotion(c,fix,certificate,jobs,prepared,artifact){
 const p=c.prepared_web;
 assert(fix?.merged===true&&HEX.test(c.postmerge_ui_source)&&fix.head.sha===c.postmerge_ui_source,'PR177 exact repair not merged');
 assert(certificate?.status==='completed'&&certificate.conclusion==='success'&&certificate.path==='.github/workflows/ui-progression-cert.yml'&&certificate.head_sha===fix.head.sha,'exact PR177 native certificate missing');
 const native=jobs?.jobs?.find(x=>x.name==='certify');
 assert(native?.conclusion==='success','native certification job missing');
 for(const name of ['V4.1 real controls, persistent mailbox and approved-source captures','EditMode','PlayMode — UI and progression contract'])
  assert(native.steps?.some(x=>x.name===name&&x.conclusion==='success'),'native certification step not passed: '+name);
 assert(prepared?.status==='completed'&&prepared.conclusion==='success'&&prepared.head_sha===p.source_sha&&prepared.path==='.github/workflows/eldoria-post-pr163-web-release.yml','prepared package run mismatch');
 assert(artifact?.id===p.artifact_id&&artifact.name===p.artifact_name&&artifact.digest===p.artifact_digest&&artifact.workflow_run?.id===p.run_id&&!artifact.expired,'prepared artifact identity mismatch');
}

export function verifyPostBuildCertificate(c,run,jobs){
 assert(c.capture_fix?.source_sha==='acfdad16e4e38a86010e3f5a9977800c0b941be0'&&Number.isSafeInteger(c.capture_fix.run_id),'corrected capture certificate identity missing');
 assert(run?.head_sha===c.capture_fix.source_sha&&run.head_branch==='main'&&run.status==='completed'&&run.conclusion==='success'&&run.path==='.github/workflows/ui-progression-cert.yml','corrected complete native suite has not passed');
 const j=jobs?.jobs?.find(x=>x.name==='certify');
 assert(j?.conclusion==='success','actual corrected native job missing');
 for(const name of ['V4.1 real controls, persistent mailbox and approved-source captures','Real Diálogo 1 before/after — original HUD, three speakers, four formats','Verify retention reference composition and real guardian/discovery interaction','Verify three simultaneous marches in the actual integrated scene','EditMode','PlayMode — UI and progression contract'])
  assert(j.steps?.some(s=>s.name===name&&s.conclusion==='success'),'corrected native step missing: '+name);
}

export function verifyConfig(c){
 assert(c?.schema_version===1 && c.workstream_id==='eldoria-post-pr163-apk-webgl-release-20261107','bad release workstream');
 assert(typeof c.owner==='string' && c.owner.length>0 && Number.isSafeInteger(c.ownership_revision) && c.ownership_revision>0 && c.budget_eur===0,'missing accountable zero-cost release');
 const p=c.provenance, w=c.web, a=c.android;
 assert(p?.revision==='native325' && HEX.test(p.source_sha) && HEX.test(p.merged_sha) && HEX.test(p.unity_tree),'invalid immutable Unity provenance');
 assert(Number.isSafeInteger(p.native_run)&&p.native_run>0&&p.native_artifact==='valoria-integrated-web-playtest-'+p.native_run&&DIGEST.test(p.native_digest),'invalid original native artifact');
 assert(Number.isSafeInteger(p.qa_run)&&p.qa_run>0&&HEX.test(p.qa_source_sha)&&p.qa_branch==='qa/pr163-webgl-evidence-20261107'&&p.qa_artifact==='pr163-real-windows-browser-'+p.qa_run&&(DIGEST.test(p.qa_digest)||(!c.web?.publish_enabled&&p.qa_digest===null)),'invalid real QA source');
 assert(p.qa_request_path==='tools/valoria-reference-clean/pr163-windows-browser-request.json'&&p.qa_expected_complete==='I-II','incorrect QA protocol');
 assert(typeof w?.publish_enabled==='boolean','explicit publication flag required');
 assert(w?.provider==='itch.io'&&w.target==='chrismad/eldoria-pruebas'&&w.channel==='web'&&w.restricted===true&&w.max_total_bytes===500000000&&w.max_file_bytes===200000000,'invalid restricted HTML5 destination');
 assert(a?.existing_version_code===21&&a.new_version_code===22&&a.new_version_name==='1.22'&&HEX.test(a.existing_source_sha)&&DIGEST.test('sha256:'+a.original_signing_certificate_sha256),'unsigned or invalid version increment');
 assert(a.publish_enabled===false,'Android publishing must only use signed dedicated release pipeline');
 assert(c.dependencies?.require_pr177_merge_before_publish===true&&c.dependencies.pr177===177&&c.dependencies.require_no_competing_windows_lease===true,'release dependency checks disabled');
 return true;
}
export function verifyOwnership(registry,c){
 const claims=registry?.active?.filter(x=>x.id===c.workstream_id)||[];
 assert(claims.length===1,'release workstream missing or duplicated');
 const w=claims[0];
 assert(w.owner===c.owner&&w.branch==='release/post-pr163-apk-webgl-20261107'&&w.status==='active'&&w.ownership_state==='claimed'&&w.write_scope_released===false&&w.ownership_revision===c.ownership_revision,'release owner or scope changed');
 assert(!w.resources?.includes('windows-runner-heavy'),'package stage must never seize reserved Windows');
 return true;
}
export function verifyNativePackage(c,native,artifact,qaRequest){
 const p=c.provenance;
 assert(native?.status==='completed'&&native.conclusion==='success'&&native.head_sha===p.source_sha&&native.head_branch==='main'&&native.path==='.github/workflows/valoria-reference-clean.yml','not the exact certified native Unity artifact');
 assert(artifact&&artifact.name===p.native_artifact&&artifact.digest===p.native_digest&&!artifact.expired&&artifact.size_in_bytes>100000,'original native artifact missing or changed');
 assert(qaRequest?.run_id===p.native_run&&qaRequest.source_sha===p.source_sha&&qaRequest.revision===p.revision&&qaRequest.artifact_name===p.native_artifact&&qaRequest.allow_release===false&&qaRequest.allow_merge===false,'QA request does not pin original exact native bytes');
 return true;
}
export function verifyRuns(c,native,qa,nativeArtifact,qaArtifact,qaRequest){
 verifyNativePackage(c,native,nativeArtifact,qaRequest);
 const p=c.provenance;
 assert(qa?.status==='completed'&&qa.conclusion==='success'&&qa.head_sha===p.qa_source_sha&&qa.head_branch===p.qa_branch&&qa.path==='.github/workflows/pr163-windows-browser-qa.yml','not the real normal-clock Chromium success');
 assert(DIGEST.test(p.qa_digest)&&qaArtifact&&qaArtifact.name===p.qa_artifact&&qaArtifact.digest===p.qa_digest&&!qaArtifact.expired&&qaArtifact.size_in_bytes>100000,'original completed QA evidence missing or changed');
 return true;
}
const active=process.argv[1]===fileURLToPath(import.meta.url);
if(active){
 const c=JSON.parse(fs.readFileSync(cfgPath,'utf8'));
 verifyConfig(c);
 const token=process.env.GH_TOKEN||process.env.GITHUB_TOKEN;
 assert(token,'GitHub action token unavailable');
 const headers={Authorization:'Bearer '+token,Accept:'application/vnd.github+json','X-GitHub-Api-Version':'2022-11-28'};
 const api=async suffix=>{
  const response=await fetch('https://api.github.com/repos/'+REPO+'/'+suffix,{headers,signal:AbortSignal.timeout(25000)});
  if(!response.ok)throw Error('RELEASE_GATE: GitHub HTTP '+response.status+' on '+suffix.split('?')[0]);
  return response.json();
 };
 const mainRef=await api('git/ref/heads/main');
 const mainSha=mainRef?.object?.sha;
 assert(HEX.test(mainSha),'main commit missing');
 const registry=await api('contents/pipeline/active-workstreams.json?ref=main');
 const ownership=JSON.parse(Buffer.from(registry.content,'base64').toString('utf8'));
 verifyOwnership(ownership,c);
 const channel=await api('contents/updates/android/family.json?ref=main');
 const a=JSON.parse(Buffer.from(channel.content,'base64').toString('utf8'));
 assert(a.versionCode===c.android.existing_version_code && a.sourceSha===c.android.existing_source_sha && a.certificateSha256===c.android.original_signing_certificate_sha256,'installed Android channel advanced since release preparation');
 const source=await api('git/commits/'+c.provenance.source_sha);
 const current=await api('git/commits/'+mainSha);
 const sourceTree=await api('git/trees/'+source.tree.sha);
 const currentTree=await api('git/trees/'+current.tree.sha);
 const unity=t=>t.tree.find(x=>x.path==='Unity'&&x.type==='tree')?.sha;
 assert(unity(sourceTree)===c.provenance.unity_tree,'original certified Unity tree mismatch');
 await verifyUnityTrees(c.provenance.unity_tree,unity(currentTree),sha=>api('git/trees/'+sha));
 const merged=await api('pulls/163');
 assert(merged.merged===true&&merged.merge_commit_sha===c.provenance.merged_sha,'PR163 not merged at pinned source');
 const compare=await api('compare/'+c.provenance.merged_sha+'...'+mainSha);
 assert(['identical','ahead'].includes(compare.status),'main must descend from certified merge');
 const [native,na,qaReqResponse]=await Promise.all([
  api('actions/runs/'+c.provenance.native_run),api('actions/runs/'+c.provenance.native_run+'/artifacts?per_page=100'),
  api('contents/'+c.provenance.qa_request_path+'?ref='+c.provenance.qa_source_sha)
 ]);
 const qaRequest=JSON.parse(Buffer.from(qaReqResponse.content,'base64').toString('utf8'));
 const nativeArtifact=na.artifacts.find(x=>x.name===c.provenance.native_artifact);
 verifyNativePackage(c,native,nativeArtifact,qaRequest);
 if(c.web.publish_enabled){
  const [qa,qaArtifacts,captureRun,captureJobs]=await Promise.all([
   api('actions/runs/'+c.provenance.qa_run),api('actions/runs/'+c.provenance.qa_run+'/artifacts?per_page=100'),
   api('actions/runs/'+c.capture_fix?.run_id),api('actions/runs/'+c.capture_fix?.run_id+'/jobs?per_page=100')
  ]);
  verifyRuns(c,native,qa,nativeArtifact,qaArtifacts.artifacts.find(x=>x.name===c.provenance.qa_artifact),qaRequest);
  verifyPostBuildCertificate(c,captureRun,captureJobs);
  assert(Number.isSafeInteger(c.postmerge_ui_run)&&c.postmerge_ui_run>0,'exact PR177 certificate run required');
  const [fix,cert,jobs,prepared,artifact]=await Promise.all([
   api('pulls/177'),api('actions/runs/'+c.postmerge_ui_run),api('actions/runs/'+c.postmerge_ui_run+'/jobs?per_page=100'),
   api('actions/runs/'+c.prepared_web.run_id),api('actions/artifacts/'+c.prepared_web.artifact_id)
  ]);
  verifyPromotion(c,fix,cert,jobs,prepared,artifact);
  const live=await api('git/ref/heads/main');
  assert(live.object.sha===mainSha,'main changed during release preflight');
 }
 const output=process.env.GITHUB_OUTPUT;
 if(output)fs.appendFileSync(output,'prepared_run='+String(c.prepared_web?.run_id||'')+'\nprepared_artifact='+String(c.prepared_web?.artifact_name||'')+'\nsource_sha='+c.provenance.source_sha+'\nnative_run='+c.provenance.native_run+'\nrevision='+c.provenance.revision+'\npublish='+String(c.web.publish_enabled)+'\n');
 console.log((c.web.publish_enabled?'ELDORIA_VERIFIED_PUBLICATION_GATE_PASS ':'ELDORIA_NATIVE_PACKAGE_ONLY_QA_NOT_ASSERTED ')+JSON.stringify({main:mainSha,unity_tree:c.provenance.unity_tree,native_run:c.provenance.native_run,qa_run:c.provenance.qa_run,publish:c.web.publish_enabled}));
}
