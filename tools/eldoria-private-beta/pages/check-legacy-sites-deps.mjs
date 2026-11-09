// Read-only compatibility audit. Never loads Unity, telemetry or player saves.
// An old ChatGPT Sites shell currently references versioned WebGL binaries on a
// mutable GitHub Pages path. Replacing Pages can silently break that old shell.
import {pathToFileURL} from 'node:url';
const SITES='https://eldoria-valoria-beta-20261108.kbkvc5c45n.chatgpt.site/';
const SOURCE='be49d62e3daf490f41dba0bf8123a8e2eee64480';
const REVISION='native325-mobile10-pretest-ux';
const CDN='mt5hjfz2kb-lab.github.io';
export function extractDependencies(manifest,manifestUrl){
 if(!manifest||typeof manifest!=='object')throw Error('Missing legacy manifest');
 const base=new URL('.',manifestUrl),required=new Map();
 const add=(role,value)=>{
  if(typeof value!=='string'||!value.trim())throw Error('Missing asset '+role);
  const uri=new URL(value,base);
  if(uri.protocol!=='https:'||![CDN,base.hostname].includes(uri.hostname))
   throw Error('Unexpected legacy dependency host: '+uri.hostname);
  required.set(uri.href,[...(required.get(uri.href)||[]),role]);
 };
 for(const name of ['loader','framework','code'])add(name,manifest[name]);
 const parts=manifest.decoded_parts?.[manifest.data];
 if(!Array.isArray(parts)||!parts.length)throw Error('Missing original data chunks');
 add('data_part_0',parts[0].url);
 add('data_part_last',parts.at(-1).url);
 return [...required].map(([url,roles])=>({url,roles}));
}
export async function auditLegacyShell({fetchImpl=fetch,base=SITES,source=SOURCE,revision=REVISION}={}){
 const root=new URL(base);
 const json=async filename=>{
  const resp=await fetchImpl(new URL(filename,root).href,{method:'GET',redirect:'error',cache:'no-store'});
  if(!resp.ok)throw Error('Legacy '+filename+' failed HTTP '+resp.status);
  return resp.json();
 };
 const identity=await json('release-identity.json');
 if(identity.source_sha!==source||identity.revision!==revision)
  throw Error('Legacy shell no longer contains expected immutable build');
 const manifest=await json('build-manifest.json');
 const required=extractDependencies(manifest,new URL('build-manifest.json',root));
 const results=[];
 for(const item of required){
  try{
   const response=await fetchImpl(item.url,{method:'HEAD',redirect:'follow',cache:'no-store'});
   results.push({...item,available:response.ok,status:response.status});
  }catch(e){results.push({...item,available:false,status:null,error_type:e?.name||'FETCH_ERROR'})}
 }
 return {revision:identity.revision,source_sha:identity.source_sha,checked:results.length,
         missing:results.filter(r=>!r.available),pass:results.length>=4&&results.every(r=>r.available)};
}
if(process.argv[1]&&pathToFileURL(process.argv[1]).href===import.meta.url){
 if(!process.argv.includes('--live')){
  console.error('Usage: node check-legacy-sites-deps.mjs --live (read only)');
  process.exitCode=2;
 }else auditLegacyShell().then(x=>{
   console.log(JSON.stringify(x,null,2));
   if(!x.pass)process.exitCode=1;
 }).catch(err=>{console.error('ELDORIA_LEGACY_SITES_DEPENDENCY_FAILURE:',err.message);process.exitCode=2;});
}
