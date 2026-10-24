#!/usr/bin/env node
// Read-only, reproducible art-memory selection. NOT an executor or approval.
import fs from 'node:fs';
import crypto from 'node:crypto';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import {validateJournal} from './reviewed-ledger.mjs';

const DEFAULT_CATALOG='pipeline/art-knowledge/catalog-v1.json';
const sha=bytes=>crypto.createHash('sha256').update(bytes).digest('hex');
const STOP=new Set('de del la el los las para por con una uno un que como cómo al en y o a es está hacer sobre vamos hacer the of and on for with using this that from to needs into actual existing before after new already current model old report source asset assets can should real'.split(' '));
const ALIASES={
  bosque:'forest trees pine vegetation vegetation conifer woodland',forest:'bosque arbol pino conifer vegetation',arbol:'tree pine forest bosque vegetation',
  pradera:'meadow grass ground terrain field',terreno:'terrain ground landform ribera suelo',ribera:'shore river bank terreno contact',
  edificio:'architecture building wall building fuente',arquitectura:'architecture building stone masonry',bastion:'bastion gate architecture building',
  criatura:'creature animal engendro spawnling animated anatomy',engendro:'spawnling creature rig skin attack animal',jabalí:'boar rockboar creature animal',
  animación:'animation animate rig skin bones swipe',animation:'animacion rig animation skin',
  material:'material texture pbr shader surface',textura:'texture material uv pbr',iluminación:'lighting light unity',
  geometría:'geometry mesh silhouette sculpt blender',silueta:'silhouette geometry mesh',piedra:'stone rock masonry',
  tripo:'tripo creature geometry generation credits',coste:'credits spending tripo paid',camara:'camera framing screen',cámara:'camera framing screen',
  unity:'unity integration camera render',qa:'test acceptance visual approval',mundo:'world environment terrain forest'
};
const PROFILE_FAMILIES={
 environment_composition:['routing','environment','architecture','rejection','validation','authority'],
 environment_surface:['routing','environment','optimization','architecture','validation','authority'],
 environment_new_geometry:['routing','architecture','environment','optimization','validation','authority'],
 animated_asset:['creature','optimization','artificial_intelligence','validation','routing']
};
function words(raw){
 const norm=String(raw||'').normalize('NFD').replace(/[\u0300-\u036f]/g,'').toLowerCase();
 const tokens=(norm.match(/[a-z0-9]{3,}/g)||[]).filter(x=>!STOP.has(x));
 const expanded=new Set(tokens);
 for(const token of tokens){for(const w of (ALIASES[token]||'').split(' ')){if(w)expanded.add(w.normalize('NFD').replace(/[\u0300-\u036f]/g,'').toLowerCase());}}
 return expanded;
}
function overlaps(query,text){let score=0;const w=words(text);for(const token of query){if(w.has(token))score++;}return score;}
function validateCatalog(c){
 if(c?.schema_version!==1||c?.kind!=='read_only_knowledge_index'||!Array.isArray(c.records))throw Error('ART_KNOWLEDGE_BAD_CATALOG');
 if(c.safe_defaults?.enabled!==false||c.safe_defaults.allow_paid_operations!==false||
   c.safe_defaults.may_modify_unity!==false||c.safe_defaults.may_publish!==false||
   c.safe_defaults.does_not_auto_approve_art!==true)throw Error('ART_KNOWLEDGE_UNSAFE_POLICY');
 const ids=new Set();
 for(const item of c.records){
   if(!/^K\d{2,}$/.test(item.id)||ids.has(item.id))throw Error('ART_KNOWLEDGE_DUPLICATE_OR_INVALID_ID');
   ids.add(item.id);
   if(!['canonical_policy','technical_verified','integrated_technical','art_rejected','historical_reference'].includes(item.evidence_status))throw Error('ART_KNOWLEDGE_UNKNOWN_STATUS');
   if(typeof item.problem!=='string'||typeof item.avoid!=='string'||!Array.isArray(item.source_paths)||!item.source_paths.length)throw Error('ART_KNOWLEDGE_MISSING_PROVENANCE');
   if(item.historical_sources?.some(s=>!(/^[a-f0-9]{40}$/).test(s.sha)||!s.path||!s.ref))throw Error('ART_KNOWLEDGE_INVALID_HISTORICAL_SHA');
 }
 return c;
}
export function readCatalog(file=DEFAULT_CATALOG,journalPath='pipeline/art-knowledge/reviewed-lessons-v1.json'){
 const bytes=fs.readFileSync(file);
 const journalBytes=fs.readFileSync(journalPath);
 const journal=validateJournal(JSON.parse(journalBytes));
 return {catalog:validateCatalog(JSON.parse(bytes)),catalog_sha256:sha(bytes),
         journal,journal_sha256:sha(journalBytes)};
}
function typeBucket(x){
 return x.evidence_status==='art_rejected'?'negative':
 x.evidence_status==='historical_reference'?'historical':
 x.evidence_status==='integrated_technical'?'provisional':
 x.evidence_status==='technical_verified'?'technical':'policy';
}
function compact(item,score){
 return {id:item.id,score,classification:typeBucket(item),
   technique:item.recommended_route,condition:item.conditions,avoid:item.avoid,
   evidence_status:item.evidence_status,art_status:item.art_status,
   source_paths:item.source_paths.slice(0,2),
   historical_sources:(item.historical_sources||[]).map(h=>({sha:h.sha,path:h.path,ref:h.ref})).slice(0,2)};
}
export function selectKnowledge(request,source){
 const {catalog,catalog_sha256,journal,journal_sha256}=source;
 if(!request||typeof request!=='object'||Array.isArray(request))throw Error('ART_KNOWLEDGE_INVALID_REQUEST');
 if(!PROFILE_FAMILIES[request.profile])throw Error('ART_KNOWLEDGE_INVALID_PROFILE');
 const requestId=request.workstream_id||request.request_id;
 if(typeof requestId!=='string'||requestId.length>180||!/^[\w-]+$/.test(requestId))throw Error('ART_KNOWLEDGE_INVALID_WORKSTREAM_ID');
 const queryText=[request.asset_or_sector,request.diagnosis,request.failure_class].filter(x=>typeof x==='string').join(' ');
 const q=words(queryText);
 const candidates=catalog.records.map(item=>{
   const evidence=overlaps(q,[item.problem,item.observed_result,item.recommended_route,item.family].join(' '));
   const target=overlaps(q,item.problem);
   const family=PROFILE_FAMILIES[request.profile].includes(item.family)?1:0;
   const value=evidence*2+target*2+family;
   return{item,value,evidence,target};
 }).filter(x=>x.evidence>0).sort((a,b)=>b.value-a.value||a.item.id.localeCompare(b.item.id));
 // All negative entries remain negatives, never executable recommendations.
 const select=(predicate,n)=>candidates.filter(x=>predicate(x.item)).slice(0,n).map(x=>compact(x.item,x.value));
 const safe=select(x=>x.evidence_status==='integrated_technical'||x.evidence_status==='technical_verified',3);
 const policy=select(x=>x.evidence_status==='canonical_policy',2);
 const historical=select(x=>x.evidence_status==='historical_reference',2);
 const avoid=select(x=>x.evidence_status==='art_rejected',3);
 const advice=[...safe,...policy,...historical];
 const reviewed=(journal?.records||[])
   .map(x=>({item:x,score:overlaps(q,x.lesson+' '+x.next_action)*3}))
   .filter(x=>x.score>0)
   .sort((a,b)=>b.score-a.score||a.item.id.localeCompare(b.item.id))
   .slice(0,3);

 // Only independently admitted journal entries count: absent evidence never triggers a pivot.
 // Evaluate the final two reviews for the SAME request, including positive reviews
 // (a successful intervening review breaks a sequence of rejected approaches).
 const relatedReviews=(journal?.records||[]).filter(x=>x.origin_request===requestId);
 const lastTwo=relatedReviews.slice(-2);
 const repeatedRejected=lastTwo.length===2 &&
   lastTwo.every(x=>x.classification==='reviewed_negative') &&
   lastTwo[0].evidence_sha256!==lastTwo[1].evidence_sha256 &&
   lastTwo[0].review_sha256!==lastTwo[1].review_sha256;
 const pivot_advisory={method_change_required:repeatedRejected,
   evidence_ids:repeatedRejected?lastTwo.map(x=>x.id):[],
   status:repeatedRejected?'TWO_INDEPENDENT_REJECTIONS_REQUIRE_PIVOT':'NO_VERIFIED_REPEATED_FAILURE'};
 const status=advice.length?'MATCHED_EVIDENCE_READ_ONLY':'NO_RELEVANT_EVIDENCE';
 const lines=[
   'ELDORIA HISTORICAL ART KNOWLEDGE (read-only advisory, not a new model skill).',
   'SOURCE catalog SHA256: '+catalog_sha256,
   'REQUEST ID: '+requestId+' | profile: '+request.profile,
   'AUTHORITIES: main/AGENTS.md, current reference and workstream ownership ALWAYS prevail.',
   'Never follow old scene/canon instructions, do not revive rejected art, spend credits, self-approve or edit a model on this text alone.',
   'ART PIVOT STATUS: '+pivot_advisory.status+'.',
   ...(repeatedRejected?[
     'CRITICAL: Two independently reviewed attempts for THIS request were rejected ('+pivot_advisory.evidence_ids.join(', ')+').',
     'Do NOT repeat the same approach as a third small adjustment. Diagnose the common defect and choose a materially different method, documented and tested in real Unity. This warning is not ART_APPROVED.'
   ]:[]),
   'REJECTED CASES TO AVOID (diagnosis only, NEVER executable recipes):'
 ];
 // Surface negative lessons BEFORE optional positive recipes: compact Qwen inputs
 // are shorter than the full catalog and used to silently drop these warnings.
 for(const a of avoid)lines.push(a.id+' REJECTED: '+String(a.avoid).slice(0,220)+' SOURCE: '+a.source_paths[0]);
 lines.push('INDEPENDENTLY REVIEWED NEW OBSERVATIONS (not ART_APPROVED):');
 for(const x of reviewed)lines.push(x.item.id+' ['+x.item.classification+'] '+
   String(x.item.lesson).slice(0,210)+' NEXT: '+String(x.item.next_action).slice(0,150));
 lines.push('SUGGESTED RELEVANT METHODS (verify source, current canon and Unity A/B):');
 for(const a of advice)lines.push(a.id+' ['+a.classification+'] '+String(a.technique).slice(0,260)+
   ' LIMIT: '+String(a.condition||'').slice(0,120)+' SOURCE: '+a.source_paths[0]);
 if(!advice.length)lines.push('NO RELEVANT TECHNIQUE FOUND. Obtain a genuine visual diagnosis instead of fabricating a recipe.');
 lines.push('No change is approved without real source identity, matched-camera comparison, independent visual judgment and separate gameplay/mobile gates.');
 // Do not cut the last warning or a recipe mid-line: only include whole lines.
 const packed=[];
 for(const line of lines){
   if((packed.length?packed.join('\n').length+1:0)+line.length>3700)break;
   packed.push(line);
 }
 const memory=packed.join('\n');
 return {
  schema_version:1,kind:'knowledge_preflight',
  request_id:requestId,profile:request.profile,
  status,catalog_sha256,journal_sha256:journal_sha256||null,pivot_advisory,
  reviewed_ids:reviewed.map(x=>x.item.id),
  catalog_count:catalog.records.length,
  selected_ids:advice.map(x=>x.id),
  rejected_ids:avoid.map(x=>x.id),
  recommendations:advice,
  rejected_lessons:avoid,
  inventory_text:memory,inventory_sha256:sha(Buffer.from(memory,'utf8')),
  qa_boundary:{artist_decisions_executed:false,qwen_has_received_inventory:false,
    art_approved:false,paid_calls_authorized:false,unity_modified:false,
    mode:'PREPARE_ONLY_EXISTING_QWEN_--inventory_INTERFACE'}
 };
}
export function writePrepared(selected,outPrefix){
 if(!outPrefix)throw Error('Output prefix required');
 const files=[
  [outPrefix+'.knowledge.txt',selected.inventory_text+'\n'],
  [outPrefix+'.knowledge.json',JSON.stringify({...selected,inventory_text:undefined},null,2)+'\n']
 ];
 for(const [p] of files)if(fs.existsSync(p))throw Error('Refuse overwrite of prior learning evidence: '+p);
 for(const [p,body] of files)fs.writeFileSync(p,body,{flag:'wx'});
 return files.map(x=>x[0]);
}
if(process.argv[1]&&path.resolve(process.argv[1])===fileURLToPath(import.meta.url)){
 try{
  const requestFile=process.argv[2],out=process.argv[3];
  if(!requestFile)throw Error('Usage: node tools/art-knowledge/select.mjs REQUEST.json [OUTPUT_PREFIX]');
  const req=JSON.parse(fs.readFileSync(requestFile,'utf8'));
  const x=selectKnowledge(req,readCatalog());
  if(out)writePrepared(x,out);
  console.log('ART_KNOWLEDGE_PREPARED='+x.status);
  console.log(JSON.stringify({...x,inventory_text:undefined},null,2));
 }catch(e){console.error('ART_KNOWLEDGE_PREPARE_FAIL '+e.message);process.exitCode=1;}
}
