#!/usr/bin/env node
// Low-cost bridge for persistent, evidence-backed artist lessons.
// Does not train model weights, mutate the source catalog, approve art or use Unity.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {fileURLToPath} from 'node:url';
import {validate,brief,appendPending} from './learning.mjs';
import {measureUnityVisualChange} from '../ai-authoring/unity_pixel_diff.mjs';

const HERE=path.dirname(fileURLToPath(import.meta.url));
const DEFAULT_DB=path.join(HERE,'observations.json');
const HEX=/^[a-f0-9]{64}$/;
const GIT_SHA=/^[a-f0-9]{40}$/;
const ID=/^[a-zA-Z0-9_-]{5,110}$/;
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
const assert=(x,m)=>{if(!x)throw Error(m);};
function catalog(dbPath){
 const c=JSON.parse(fs.readFileSync(dbPath,'utf8'));
 const errors=validate(c);
 assert(!errors.length,'Untrusted persistent learning catalog: '+errors.join('; '));
 return c;
}
function safeOut(file,src){
 assert(path.resolve(file)!==path.resolve(src),'Never overwrite persistent source catalog');
 assert(!fs.existsSync(file),'Refuse to overwrite prior evidence: '+file);
}
export function prepare({dbPath=DEFAULT_DB,target='bastion1',out,maxChars=1800}){
 assert(ID.test(target),'Invalid target');
 assert(Number.isInteger(maxChars)&&maxChars>=300&&maxChars<=6000,'Invalid memory brief bound');
 const c=catalog(dbPath),message=brief(c,target,maxChars);
 const raw=fs.readFileSync(dbPath);
 const provenance={schema_version:1,target,catalog_sha256:sha(raw),reference_sha256:c.reference_sha256,
   case_count:c.cases.filter(x=>x.target===target).length,brief_sha256:sha(Buffer.from(message)),
   status:'HISTORIC_ADVICE_NOT_MODEL_TRAINING_OR_ART_PASS'};
 if(out){
   safeOut(out,dbPath);
   fs.writeFileSync(out,message,{flag:'wx'});
   fs.writeFileSync(out+'.provenance.json',JSON.stringify(provenance,null,2)+'\n',{flag:'wx'});
 }
 return {message,provenance};
}
function verifiedFile(p,label){
 assert(typeof p==='string'&&fs.existsSync(p),label+' missing');
 const stat=fs.statSync(p);
 assert(stat.isFile()&&stat.size<25_000_000,label+' must be a bounded regular file');
 return fs.readFileSync(p);
}
export function stage({dbPath=DEFAULT_DB,target='bastion1',out,runId,gitSha,reference,before,after,
 cameraSha,viewId='unity-player-pbr',renderPass='unity_pbr',diagnosis,lesson,nextAction,lessonId}){
 assert(out,'Staged output path required');
 assert(ID.test(target)&&ID.test(viewId)&&ID.test(renderPass),'Invalid target/view/pass');
 assert(Number.isSafeInteger(runId)&&runId>0,'Authentic GitHub run ID required');
 assert(GIT_SHA.test(gitSha||''),'Authentic 40-digit source commit required');
 assert(HEX.test(cameraSha||''),'Exact camera identity hash required');
 for(const [k,v] of Object.entries({diagnosis,lesson,nextAction})){
   assert(typeof v==='string'&&v.trim().length>=12&&v.length<=1200,k+' must contain a real observation');
 }
 const c=catalog(dbPath);
 const ref=verifiedFile(reference,'Approved reference');
 assert(sha(ref)===c.reference_sha256,'Approved reference bytes/hash mismatch');
 const left=verifiedFile(before,'Original capture'),right=verifiedFile(after,'Candidate capture');
 const measured=measureUnityVisualChange(left,right);
 // Matching dimensions and claimed fixed-camera provenance are necessary but
 // do not independently prove camera equality; every event remains unverified.
 const observed=measured.gate==='VISUAL_REJECTED';
 const event={id:'eldoria-'+target+'-'+runId,target,source:{
   run_id:runId,git_sha:gitSha,evidence_level:'pending_verification',
   url:'https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/'+runId
 },reference_sha256:c.reference_sha256,view_id:viewId,render_pass:renderPass,
 measurement:{kind:'camera_pixels',width:measured.width,height:measured.height,
   changed_pixels:measured.changed_pixels,threshold_rgb:measured.channel_difference_threshold},
 visibility_gate:observed?'REJECT_NOT_VISIBLE':'REVIEW_REQUIRED',art_verdict:'ART_UNREVIEWED',
 art_approved:false,lesson_id:lessonId||'pending-'+target,
 diagnosis,lesson,next_action:nextAction,
 candidate_provenance:{
   before_png_sha256:sha(left),after_png_sha256:sha(right),
   camera_matrix_sha256:cameraSha,claimed_same_camera:true,
   review_required:true,original_artifact_verification_pending:true
 }};
 const next=appendPending(c,event);
 safeOut(out,dbPath);
 fs.writeFileSync(out,JSON.stringify(next,null,2)+'\n',{flag:'wx'});
 fs.writeFileSync(out+'.event.json',JSON.stringify(event,null,2)+'\n',{flag:'wx'});
 return {event,measurement:measured};
}
function parse(args){
 const o={};for(let i=0;i<args.length;i++){
   const a=args[i];assert(a.startsWith('--')&&args[i+1]&&!args[i+1].startsWith('--'),'Expected value for '+a);
   assert(!(a.slice(2) in o),'Duplicate flag '+a);o[a.slice(2)]=args[++i];
 }return o;
}
if(process.argv[1]&&path.resolve(process.argv[1])===fileURLToPath(import.meta.url)){
 try{
  const [op,...rest]=process.argv.slice(2),a=parse(rest);
  if(op==='prepare'){
   const p=prepare({dbPath:a.db,target:a.target,out:a.out,maxChars:a['max-chars']?Number(a['max-chars']):1800});
   if(!a.out)process.stdout.write(p.message);
   console.log('ART_HISTORY_PREPARED cases='+p.provenance.case_count+' catalog_sha256='+p.provenance.catalog_sha256);
  }else if(op==='stage'){
   const p=stage({dbPath:a.db,target:a.target,out:a.out,runId:Number(a['run-id']),gitSha:a['git-sha'],
    reference:a.reference,before:a.before,after:a.after,cameraSha:a['camera-sha'],viewId:a['view-id'],
    renderPass:a['render-pass'],diagnosis:a.diagnosis,lesson:a.lesson,nextAction:a['next-action'],lessonId:a['lesson-id']});
   console.log('ART_HISTORY_STAGED_PENDING run='+p.event.source.run_id+' changed_pixels='+p.measurement.changed_pixels);
  }else throw Error('Usage: cycle-hooks.mjs prepare|stage --flags');
 }catch(e){console.error('ART_HISTORY_HOOK_FAIL '+e.message);process.exitCode=1}
}
