#!/usr/bin/env node
// Eldoria's opt-in future-workstream entrypoint for the EXISTING Qwen author.
// Fails closed on missing memory; never executes Blender/Tripo/Unity or approves art.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {spawnSync} from 'node:child_process';
import {readCatalog,selectKnowledge} from './select.mjs';
import {recordInference} from './cycle-feedback.mjs';

const hash=data=>crypto.createHash('sha256').update(data).digest('hex');
const PYTHON='tools/art-direction/ai-authoring/author_brief.py';
function assert(condition,message){if(!condition)throw Error(message);}
function options(args){
 const a={};
 for(let i=0;i<args.length;){
  const key=args[i++];
  assert(key?.startsWith('--'),'Expect only explicit Qwen --options');
  assert(key!=='--inventory','Qwen inventory is selected by the central art-memory adapter');
  if(key==='--cpu-compact'){a[key]=true;continue;}
  assert(i<args.length&&!args[i].startsWith('--'),'Missing Qwen value for '+key);
  if(key==='--failed-plan'||key==='--failed-review'){(a[key]??=[]).push(args[i++]);continue;}
  assert(!(key in a),'Duplicate Qwen option '+key);
  a[key]=args[i++];
 }
 return a;
}
function uniqueWrite(p,data){
 if(fs.existsSync(p))throw Error('ART_MEMORY_REFUSE_OVERWRITE '+p);
 fs.mkdirSync(path.dirname(p),{recursive:true});
 fs.writeFileSync(p,data,{flag:'wx'});
}
function composedInventory(knowledge,source,compact,subject){
 // The real author_brief.py accepts 4000 characters and then truncates to
 // 1800 terrain / 3200 creature when --cpu-compact is used.
 const cap=compact?(subject==='terrain'?1800:3200):4000;
 const prior=source?fs.readFileSync(source,'utf8'):'';
 if(prior.length>250000)throw Error('Unbounded external source inventory');
 const sourceBound=Math.min(cap===1800?580:1250,Math.max(0,cap-500));
 const intro=prior?'EXISTING SOURCE / IMMUTABLE FACTS (verify hashes):\n'+prior.slice(0,sourceBound)+'\n':'';
 const header='ELDORIA CASE MEMORY (advisory; reference and active owner prevail):\n';
 const remaining=cap-intro.length-header.length;
 assert(remaining>=500,'Insufficient Qwen context capacity for verified lessons');
 return (intro+header+knowledge.inventory_text.slice(0,remaining)).slice(0,cap);
}
export function prepareInvocation({requestFile,prefix,artistArgs,sourceInventoryPath=null}){
 assert(typeof requestFile==='string'&&typeof prefix==='string','Require request and unique memory prefix');
 const request=JSON.parse(fs.readFileSync(requestFile,'utf8'));
 const selected=selectKnowledge(request,readCatalog());
 const flags=options(artistArgs);
 for(const field of ['--reference','--unity-before','--target-object','--out']){
  assert(flags[field],field+' required by existing author');
 }
 const subject=flags['--subject-kind']||'terrain',compact=!!flags['--cpu-compact'];
 const inventoryText=composedInventory(selected,sourceInventoryPath,compact,subject);
 const inv=prefix+'.knowledge.txt',manifest=prefix+'.knowledge.json';
 for(const p of [inv,manifest,prefix+'.cycle-feedback.json'])assert(!fs.existsSync(p),'Refuse overwrite '+p);
 uniqueWrite(inv,inventoryText+'\n');
 const record={schema_version:2,request_id:selected.request_id,catalog_sha256:selected.catalog_sha256,
   source_inventory_sha256:sourceInventoryPath?hash(fs.readFileSync(sourceInventoryPath)):null,
   selected_ids:selected.selected_ids,rejected_ids:selected.rejected_ids,
   exact_qwen_inventory_sha256:hash(Buffer.from(inventoryText,'utf8')),
   evidence_status:'PREPARED_NOT_CONSUMED',
   art_approved:false,paid_calls_authorized:false, may_mutate_unity:false};
 uniqueWrite(manifest,JSON.stringify(record,null,2)+'\n');
 return {selected,command:['python3',PYTHON,...artistArgs,'--inventory',inv],
   inventory:inv,expected_inventory_sha256:record.exact_qwen_inventory_sha256,
   out:flags['--out'],manifest:prefix+'.knowledge-consumption.json',
   feedback:prefix+'.cycle-feedback.json',source_inventory_sha256:record.source_inventory_sha256};
}
export function verifyArtistConsumption(invocation){
 const file=path.join(invocation.out,'artist-inference.json');
 assert(fs.existsSync(file),'Real Qwen inference evidence not found');
 const evidence=JSON.parse(fs.readFileSync(file,'utf8'));
 const data=fs.readFileSync(invocation.inventory,'utf8');
 const actual=hash(Buffer.from(data.slice(0,data.endsWith('\n')?-1:undefined),'utf8'));
 assert(actual===invocation.expected_inventory_sha256,'Qwen inventory generated hash changed');
 assert(evidence.asset_inventory_sha256===actual,'Qwen did NOT receive exact knowledge+source inventory');
 assert(evidence.self_approval===false,'Artistic self-approval forbidden');
 assert(evidence.artist_brief_sha256&&evidence.model&&evidence.system_prompt_sha256,'Missing real Qwen identity');
 const result={schema_version:2,status:'EXACT_QWEN_INVENTORY_CONSUMED',
   selected_ids:invocation.selected.selected_ids,rejected_ids:invocation.selected.rejected_ids,
   catalog_sha256:invocation.selected.catalog_sha256,inventory_sha256:actual,
   inference_sha256:hash(fs.readFileSync(file)),model:evidence.model,
   prompt_sha256:evidence.artist_brief_sha256,
   independent_visual_review:false,art_approved:false};
 uniqueWrite(invocation.manifest,JSON.stringify(result,null,2)+'\n');
 return result;
}
if(process.argv[1]&&path.resolve(process.argv[1])===new URL(import.meta.url).pathname){
 try{
  const a=process.argv.slice(2),sep=a.indexOf('--');assert(sep>=0,'Expected -- separator before Qwen args');
  let requestFile,prefix,run=false,sourceInventoryPath=null;
  for(let i=0;i<sep;i++){
   if(a[i]==='--request')requestFile=a[++i];
   else if(a[i]==='--memory-prefix')prefix=a[++i];
   else if(a[i]==='--source-inventory')sourceInventoryPath=a[++i];
   else if(a[i]==='--run')run=true;
   else if(a[i]==='--dry-run')run=false;
   else throw Error('Unrecognized adapter argument '+a[i]);
  }
  const p=prepareInvocation({requestFile,prefix,sourceInventoryPath,artistArgs:a.slice(sep+1)});
  console.log('ELDORIA_MEMORY_ENTRYPOINT='+(!run?'PREPARED_ONLY':'CALLING_EXISTING_LOCAL_QWEN'));
  console.log(JSON.stringify({selected_ids:p.selected.selected_ids,rejected_ids:p.selected.rejected_ids,
   inventory_sha256:p.expected_inventory_sha256,source_inventory_sha256:p.source_inventory_sha256}));
  if(run){
   const [cmd,...params]=p.command;
   const proc=spawnSync(cmd,params,{stdio:'inherit',shell:false,timeout:1_250_000});
   const status=proc.status===null?1:proc.status;
   // Every completed/failed attempt leaves an immutable pending lesson; never auto-approves.
   const feedback=recordInference({invocation:p,outFile:p.feedback,exitStatus:status});
   console.log('ART_MEMORY_PENDING_CYCLE_RECORDED '+feedback.status);
   if(proc.error)throw proc.error;
   if(status!==0)throw Error('Existing Qwen author failed or rejected candidate: '+status);
   console.log(JSON.stringify(verifyArtistConsumption(p)));
  }
 }catch(e){console.error('ELDORIA_MEMORY_ENTRYPOINT_FAIL '+e.message);process.exitCode=1;}
}
