#!/usr/bin/env node
// Stage an opt-in telemetry preview beside an immutable verified Eldoria WebGL release.
// This does NOT approve external testers, compile Unity or change the default page.
import {readFile,writeFile,access} from 'node:fs/promises';
import {resolve,join} from 'node:path';
import {createHash} from 'node:crypto';
import {fileURLToPath} from 'node:url';
const REVISION='native325-mobile11-building-anchor';
const UNITY_SOURCE='8dc875430241a49b4d417782dfeab2d4c7a4f315';
const MOBILE16={revision:'native325-mobile16-full-lazy-profile-owner-qa',
 source:'4fd1cacc9c206cfc20e78b20738e4b5bc48e0e71',
 bundle:'49a2d76277a02c7dad31cc1fd46c641d4f15144c2c02e2c6dba1b01ec8c9d66a',
 api:'https://eavaxwpxxcrwibooiimb.supabase.co/functions/v1/eldoria-mobile16-events-qa'};
const CLIENTS=['client.mjs','signal-parser.mjs','pilot-bootstrap.mjs','restart-diagnostic.mjs'];
const hash=(b)=>createHash('sha256').update(b).digest('hex');
export async function stagePilot(sitePath,sourcePath){
 const site=resolve(sitePath),source=resolve(sourcePath);
 const original=await readFile(join(site,'index.html'));
 const body=original.toString('utf8');
 const identity=JSON.parse(await readFile(join(site,'release-identity.json'),'utf8'));
 const mobile16=identity.revision===MOBILE16.revision && identity.source_sha===MOBILE16.source;
 if(!mobile16 && (identity.revision!==REVISION || identity.source_sha!==UNITY_SOURCE))
  throw Error('Refuse pilot: wrong certified Unity engine');
 const revision=mobile16?MOBILE16.revision:REVISION;
 const unitySource=mobile16?MOBILE16.source:UNITY_SOURCE;
 if(mobile16){
  if(identity.candidate_bundle_sha256!==MOBILE16.bundle || identity.owner_only!==true ||
     identity.real_lazy_unity_playmode_passed!==true || identity.real_native_i_v_journey_passed!==true)
   throw Error('Refuse pilot: mobile16 native identity/provenance mismatch');
  const proof=JSON.parse(await readFile(join(site,'../beta-lazy-world-owner-proof.json'),'utf8'));
  if(proof.source_sha!==MOBILE16.source || proof.native_bundle_sha256!==MOBILE16.bundle ||
     proof.original_files_preserved!==144 || proof.shared_exact_streaming_assets!==true ||
     proof.external_testers_enabled!==false || !Number.isSafeInteger(proof.combined_bytes) ||
     proof.combined_bytes<=0 || proof.combined_bytes>=980000000)
   throw Error('Refuse pilot: mobile16 preserved owner-site proof mismatch');
 }
 if(identity.release_for_external_testers!==false || identity.native_journey_verified!==true)
  throw Error('Refuse pilot: release audience or journey certification mismatch');
 if(!/<html\b/i.test(body)||!/<\/body>/i.test(body)||body.match(/<\/body>/gi)?.length!==1)
  throw Error('Refuse pilot: real Unity document missing valid body');
 if(body.includes('pilot-bootstrap.mjs')||body.includes('ELDORIA_PILOT_QA'))
  throw Error('Refuse pilot: game entry is already instrumented');
 if(!mobile16)await access(join(site,'build-manifest.json'));
 const staged=[];
 for(const f of CLIENTS){
  const originalBytes=await readFile(join(source,f));
  let bytes=originalBytes;
  if(bytes.length<500 || bytes.length>20000 || /SUPABASE_(?:SECRET|SERVICE_ROLE)|CLOUDFLARE_API_TOKEN|LINK_SECRET|PASSWORD=/.test(bytes.toString()))
   throw Error('Refuse unsafe telemetry preview source '+f);
  if(mobile16 && f==='client.mjs'){
   let text=bytes.toString('utf8');
   for(const [key,value] of [['SOURCE',MOBILE16.source],['RELEASE',MOBILE16.revision],['API',MOBILE16.api]]){
    const pattern=new RegExp("export const "+key+"='[^']+';");
    if(!pattern.test(text))throw Error('Refuse pilot: missing exact client profile constant '+key);
    text=text.replace(pattern,()=> "export const "+key+"='"+value+"';");
   }
   bytes=Buffer.from(text);
  }
  staged.push({filename:f,bytes,sha256:hash(bytes),original_sha256:hash(originalBytes)});
 }
 const originalHash=hash(original);
 const injection='<!-- ELDORIA_PILOT_QA: opt-in preview only; the default game remains unmodified. -->'+
  '\n<meta name="robots" content="noindex,nofollow">'+
  '\n<script type="module" src="./pilot-bootstrap.mjs?v='+staged.find(s=>s.filename==='pilot-bootstrap.mjs').sha256.slice(0,16)+'"></script>\n';
 const modified=body.replace(/<\/body>/i,injection+'</body>');
 if(modified.length>=body.length+800)throw Error('Unexpected preview HTML modification');
 for(const f of staged)await writeFile(join(site,f.filename),f.bytes,{flag:'wx'});
 await writeFile(join(site,'pilot.html'),modified,{flag:'wx'});
 if(hash(await readFile(join(site,'index.html')))!==originalHash)
  throw Error('Default production game altered during staging');
 const manifest={
  schema:1,role:'OWNER_QA_ONLY',external_testers_enabled:false,
  entry:'pilot.html?eldoriaBetaTelemetry=1',
  source_sha:unitySource,build_revision:revision,
  default_html_sha256:originalHash,pilot_html_sha256:hash(Buffer.from(modified)),
  source_files:staged.map(({filename,sha256,original_sha256})=>({filename,sha256,original_sha256})),
  telemetry_profile:mobile16?'mobile16-owner-qa':'legacy-mobile11-owner-qa',
  telemetry_endpoint:mobile16?MOBILE16.api:'https://eavaxwpxxcrwibooiimb.supabase.co/functions/v1/eldoria-pages-events-qa',
  backend_current_profile_verified:false,
  platform_certification:'macOS WebKit desktop, NOT physical iPhone or Android',
  gameplay_markers_in_published_unity:'PENDING_NATIVE_PROOF',
  telemetry_backend:'supabase_edge_qa_only',
  note:'Do not invite external testers. Source JS alone cannot prove real Bastion I-V telemetry.'
 };
 await writeFile(join(site,'pilot-preview-manifest.json'),JSON.stringify(manifest,null,2)+'\n',{flag:'wx'});
 return manifest;
}
if(process.argv[1] && fileURLToPath(import.meta.url)===resolve(process.argv[1])){
 const [site,source]=process.argv.slice(2);
 if(!site||!source)throw Error('Usage: node pilot-stage.mjs WEBGL_SITE_DIR PILOT_SOURCE_DIR');
 stagePilot(site,source).then(m=>console.log('ELDORIA_PAGES_OWNER_QA_PILOT_STAGED',m.source_sha,m.default_html_sha256)).catch(err=>{console.error(err);process.exitCode=1;});
}

