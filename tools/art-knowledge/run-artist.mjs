#!/usr/bin/env node
// Canonical Qwen consumer adapter: prepares historical context then invokes the
// EXISTING author_brief.py via a fixed argv. Never calls Tripo, Blender or Unity.
import fs from 'node:fs';
import crypto from 'node:crypto';
import { spawnSync } from 'node:child_process';
import {readCatalog,selectKnowledge,writePrepared} from './select.mjs';

const hash=bytes=>crypto.createHash('sha256').update(bytes).digest('hex');
const PYTHON='tools/art-direction/ai-authoring/author_brief.py';
const assert=(condition,msg)=>{if(!condition)throw Error(msg)};
function argsToMap(list){
 const result={};
 for(let i=0;i<list.length;i++){
  assert(list[i].startsWith('--')&&list[i+1]&&!list[i+1].startsWith('--'),'Expected --key value pairs for Qwen');
  const key=list[i++];
  assert(!['--inventory','--endpoint','--failed-plan','--failed-review'].includes(key),'Use canonical memory inventory; complex prior-failure inputs must stay in their owning workflow');
  assert(!(key in result),'Duplicate Qwen option '+key);
  result[key]=list[i];
 }
 return result;
}
export function prepareInvocation({requestFile,prefix,artistArgs}){
 assert(requestFile&&prefix,'Require request and memory prefix');
 const req=JSON.parse(fs.readFileSync(requestFile,'utf8'));
 const selected=selectKnowledge(req,readCatalog());
 const args=argsToMap(artistArgs);
 for(const field of ['--reference','--unity-before','--target-object','--out']){
  assert(args[field],field+' required for authentic artist');
 }
 const created=writePrepared(selected,prefix);
 const inventory=created[0];
 const command=['python3',PYTHON,...artistArgs,'--inventory',inventory];
 return {selected,command,inventory,out:args['--out'],manifest:prefix+'.knowledge-consumption.json'};
}
export function verifyArtistConsumption(invocation){
 const evidencePath=invocation.out+'/artist-inference.json';
 assert(fs.existsSync(evidencePath),'Real Qwen inference evidence not found');
 const evidence=JSON.parse(fs.readFileSync(evidencePath,'utf8'));
 const bytes=fs.readFileSync(invocation.inventory,'utf8').slice(0,4000);
 const actual=hash(Buffer.from(bytes,'utf8'));
 assert(evidence.asset_inventory_sha256===actual,'Qwen did NOT receive exact selected memory inventory');
 assert(evidence.self_approval===false,'Artist self-approval forbidden');
 assert(evidence.artist_brief_sha256&&evidence.model&&evidence.system_prompt_sha256,'Incomplete real artist provenance');
 const manifest={
  schema_version:1,status:'EXACT_QWEN_INVENTORY_CONSUMED',
  selected_ids:invocation.selected.selected_ids,
  rejected_ids:invocation.selected.rejected_ids,
  catalog_sha256:invocation.selected.catalog_sha256,
  source_inventory_sha256:actual,
  artist_inference_sha256:hash(fs.readFileSync(evidencePath)),
  qwen_model:evidence.model,
  qwen_prompt_sha256:evidence.artist_brief_sha256,
  art_approved:false,unity_verified:false,
  warnings:['Real Qwen consumption proven only, not executable art acceptance or independent visual success']
 };
 fs.writeFileSync(invocation.manifest,JSON.stringify(manifest,null,2)+'\n',{flag:'wx'});
 return manifest;
}
if(process.argv[1]&&import.meta.url.endsWith('/'+process.argv[1].split('/').pop())){
 try{
  const a=process.argv.slice(2),sep=a.indexOf('--');
  if(sep<0)throw Error('Usage: node tools/art-knowledge/run-artist.mjs --request REQUEST --memory-prefix PREFIX [--dry-run|--run] -- QWEN_ARGS');
  const before=a.slice(0,sep);let requestFile,prefix,run=false;
  for(let i=0;i<before.length;i++){
   if(before[i]==='--request')requestFile=before[++i];
   else if(before[i]==='--memory-prefix')prefix=before[++i];
   else if(before[i]==='--run')run=true;
   else if(before[i]==='--dry-run')run=false;
   else throw Error('Unrecognized adapter argument '+before[i]);
  }
  const invocation=prepareInvocation({requestFile,prefix,artistArgs:a.slice(sep+1)});
  console.log('ART_KNOWLEDGE_ADAPTER='+(!run?'DRY_RUN_PREPARED':'CALLING_EXISTING_QWEN'));
  console.log(JSON.stringify({argv:invocation.command,selected_ids:invocation.selected.selected_ids,rejected_ids:invocation.selected.rejected_ids,memory_sha256:invocation.selected.catalog_sha256}));
  if(run){
   const [command,...cmdArgs]=invocation.command;
   const p=spawnSync(command,cmdArgs,{stdio:'inherit',shell:false,timeout:570000});
   if(p.error)throw p.error;
   if(p.status!==0)throw Error('Existing Qwen author rejected or failed: '+p.status);
   console.log(JSON.stringify(verifyArtistConsumption(invocation)));
  }
 }catch(e){console.error('ART_KNOWLEDGE_ADAPTER_FAIL '+e.message);process.exitCode=1;}
}
