import fs from 'node:fs';
import {fileURLToPath} from 'node:url';

const HEX=/^[a-f0-9]{40}$/;
const DIGEST=/^sha256:[a-f0-9]{64}$/;
const REPO='mt5hjfz2kb-lab/Eldoria-Prewiu';
export const cfgPath='pipeline/releases/pr163-apk-webgl-20261107.json';
const assert=(condition,message)=>{if(!condition)throw Error('RELEASE_GATE: '+message);};

export function verifyConfig(c){
 assert(c?.schema_version===1 && c.workstream_id==='eldoria-post-pr163-apk-webgl-release-20261107','bad release workstream');
 assert(c.owner==='chatgpt-post-pr163-release-20261107' && c.budget_eur===0,'missing accountable zero-cost release');
 const p=c.provenance, w=c.web, a=c.android;
 assert(p?.revision==='native324' && HEX.test(p.source_sha) && HEX.test(p.merged_sha) && HEX.test(p.unity_tree),'invalid immutable Unity provenance');
 assert(Number.isSafeInteger(p.native_run)&&p.native_run>0&&p.native_artifact==='valoria-integrated-web-playtest-'+p.native_run&&DIGEST.test(p.native_digest),'invalid original native artifact');
 assert(Number.isSafeInteger(p.qa_run)&&p.qa_run>0&&HEX.test(p.qa_source_sha)&&p.qa_branch==='qa/pr163-webgl-evidence-20261107'&&p.qa_artifact==='pr163-real-windows-browser-'+p.qa_run&&DIGEST.test(p.qa_digest),'invalid real QA source');
 assert(p.qa_request_path==='tools/valoria-reference-clean/pr163-windows-browser-request.json'&&p.qa_expected_complete==='I-II','incorrect QA protocol');
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
 assert(w.owner===c.owner&&w.branch==='release/post-pr163-apk-webgl-20261107'&&w.status==='active'&&w.ownership_state==='claimed'&&w.write_scope_released===false&&w.ownership_revision>=1,'release owner or scope changed');
 assert(!w.resources?.includes('windows-runner-heavy'),'package stage must never seize reserved Windows');
 return true;
}
export function verifyRuns(c,native,qa,nativeArtifact,qaArtifact,qaRequest){
 const p=c.provenance;
 assert(native?.status==='completed'&&native.conclusion==='success'&&native.head_sha===p.source_sha&&native.head_branch==='fix/player-experience-audit-20261106'&&native.path==='.github/workflows/valoria-reference-clean.yml','not the exact certified native Unity artifact');
 assert(qa?.status==='completed'&&qa.conclusion==='success'&&qa.head_sha===p.qa_source_sha&&qa.head_branch===p.qa_branch&&qa.path==='.github/workflows/pr163-windows-browser-qa.yml','not the real normal-clock Chromium success');
 for(const [artifact,name,digest] of [[nativeArtifact,p.native_artifact,p.native_digest],[qaArtifact,p.qa_artifact,p.qa_digest]])
  assert(artifact&&artifact.name===name&&artifact.digest===digest&&!artifact.expired&&artifact.size_in_bytes>100000,'original evidence artifact missing or changed');
 assert(qaRequest?.run_id===p.native_run&&qaRequest.source_sha===p.source_sha&&qaRequest.revision===p.revision&&qaRequest.artifact_name===p.native_artifact&&qaRequest.allow_release===false&&qaRequest.allow_merge===false,'Chromium was not executed against original exact native bytes');
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
 assert(unity(sourceTree)===c.provenance.unity_tree && unity(currentTree)===c.provenance.unity_tree,'Unity changed since the certified PR163 player build; request new native QA before release');
 const merged=await api('pulls/163');
 assert(merged.merged===true&&merged.merge_commit_sha===c.provenance.merged_sha,'PR163 not merged at pinned source');
 const compare=await api('compare/'+c.provenance.merged_sha+'...'+mainSha);
 assert(['identical','ahead'].includes(compare.status),'main must descend from certified merge');
 const [native,qa,na,qaArtifacts,qaReqResponse]=await Promise.all([
  api('actions/runs/'+c.provenance.native_run),api('actions/runs/'+c.provenance.qa_run),
  api('actions/runs/'+c.provenance.native_run+'/artifacts?per_page=100'),api('actions/runs/'+c.provenance.qa_run+'/artifacts?per_page=100'),
  api('contents/'+c.provenance.qa_request_path+'?ref='+c.provenance.qa_source_sha)
 ]);
 const qaRequest=JSON.parse(Buffer.from(qaReqResponse.content,'base64').toString('utf8'));
 verifyRuns(c,native,qa,na.artifacts.find(x=>x.name===c.provenance.native_artifact),qaArtifacts.artifacts.find(x=>x.name===c.provenance.qa_artifact),qaRequest);
 if(c.web.publish_enabled){
  const fix=await api('pulls/177');
  assert(fix.merged===true,'postmerge V4.1 gate must be merged before publishing');
  const live=await api('git/ref/heads/main');
  assert(live.object.sha===mainSha,'main changed during release preflight');
  throw Error('RELEASE_GATE: publication not yet supported by this prepare-only workflow; do not upload without a separate verified promotion');
 }
 const output=process.env.GITHUB_OUTPUT;
 if(output)fs.appendFileSync(output,'source_sha='+c.provenance.source_sha+'\nnative_run='+c.provenance.native_run+'\nrevision='+c.provenance.revision+'\n');
 console.log('ELDORIA_MERGED_PR163_WEB_PROVENANCE_PASS '+JSON.stringify({main:mainSha,unity_tree:c.provenance.unity_tree,native_run:c.provenance.native_run,qa_run:c.provenance.qa_run,publish:false}));
}
