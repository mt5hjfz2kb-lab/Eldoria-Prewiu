#!/usr/bin/env node
// Production-direction guard: detects narrow repeated art edits in real Git history.
// Prevents treating a stream of micro-corrections as strategic visual progress.
// This is a decision-evidence check, never an automated aesthetic approval.
import fs from 'node:fs';
import path from 'node:path';
import {execFileSync} from 'node:child_process';

const root=path.resolve(process.argv[2]||'.');
const manifest='docs/evidence/world4x-canon-20261022/PRODUCTION_DIRECTION_CURRENT.json';
const errors=[];
function fail(message){errors.push(message);}
function validSentence(x,minimum=35){return typeof x==='string' && x.trim().length>=minimum;}
function committedHistory(){
  const args=['-C',root,'log','-n','35','--format=%H%x09%s','--','docs/evidence/world4x-canon-20261022/authoring'];
  const raw=execFileSync('git',args,{encoding:'utf8'}).trim();
  return raw?raw.split(/\r?\n/).map(row=>{const n=row.indexOf('\t');return {sha:row.slice(0,n),subject:row.slice(n+1)};}):[];
}
const categories=[
 {name:'bridge',re:/\b(bridge|castle.gate|deck|ramp|puente|pasarela)\b/i},
 {name:'forest',re:/\b(forest|conifer|canop|tree|pine|bosque|lod)\w*/i},
 {name:'meadow',re:/\b(meadow|prairie|pigment|grass|pradera|swath|suelo)\w*/i},
 {name:'panorama',re:/\b(panorama|regional|continent|composition|breach|brecha|coastline)\w*/i}
];
function category(s){return categories.find(x=>x.re.test(s))?.name||'other';}
function getPlan(){
  const full=path.resolve(root,manifest);
  if(!full.startsWith(root+path.sep)||!fs.existsSync(full)){fail('Missing strategic direction record '+manifest);return null;}
  try{return JSON.parse(fs.readFileSync(full,'utf8'));}catch(e){fail('Malformed production decision JSON: '+e.message);return null;}
}
function evaluate(){
  let history;
  try{history=committedHistory();}catch(e){fail('Cannot inspect actual authoring commit history: '+e.message);return;}
  const meaningful=history.filter(h=>/world4x|region-production|forest-lod|camera-pivot|blender/i.test(h.subject));
  if(meaningful.length===0){console.log('WORLD4X_DIRECTION_NO_ARTISTIC_HISTORY (no claims)');return;}
  const focus=category(meaningful[0].subject);
  const repeated=[];
  for(const item of meaningful){
    if(category(item.subject)===focus) repeated.push(item);
    else break;
  }
  const threshold=4;
  if(focus==='other'||repeated.length<threshold){
    console.log('WORLD4X_DIRECTION_OK focus='+focus+' consecutive='+repeated.length+' (no narrow-loop trigger)');
    return;
  }
  console.error('WORLD4X_NARROW_REWORK_DETECTED focus='+focus+' consecutive='+repeated.length);
  console.error('Evidence: '+repeated.slice(0,8).map(x=>x.sha.slice(0,10)+' '+x.subject).join(' | '));
  const p=getPlan();
  if(!p)return;
  if(p.schema_version!==1)fail('production strategy schema_version must be 1');
  if(p.workstream_id!=='eldoria-world-region-1-canon-20261022')fail('wrong workstream');
  if(p.repeated_topic!==focus)fail('repeated_topic must match the actual consecutive authoring focus: '+focus);
  if(p.responds_to_sha!==repeated[0].sha)fail('decision is stale: responds_to_sha must match latest repeated authoring commit '+repeated[0].sha);
  if(!['PIVOT_TO_HIGH_IMPACT','ONE_LAST_CRITICAL_FIX'].includes(p.decision))fail('decision must be PIVOT_TO_HIGH_IMPACT or ONE_LAST_CRITICAL_FIX');
  if(!validSentence(p.visual_evidence,55))fail('provide a concrete finding from real Unity screenshots / actual image comparison');
  if(!validSentence(p.root_cause,55))fail('explain the root cause rather than repeating the last tweak');
  if(!validSentence(p.reason_for_priority,55))fail('justify priority of the next work against nearby defects and the approved visual canon');
  if(!Array.isArray(p.competing_problems)||p.competing_problems.length<2)fail('compare at least two competing visual problems, not only the current bridge');
  else{
    const names=new Set();
    for(const [i,x] of p.competing_problems.entries()){
      if(!validSentence(x.defect,15))fail('competing_problems['+i+'].defect needs concrete visual impact');
      if(!['cercana','regional','panoramica','all'].includes(x.view))fail('competing_problems['+i+'].view invalid');
      if(!Number.isInteger(x.impact)||x.impact<1||x.impact>5)fail('competing_problems['+i+'].impact must be 1..5');
      names.add(x.view);
    }
    if(names.size<2)fail('assess more than one distinct view instead of fixing only near-camera details');
  }
  if(p.preserve_AF_AG!==true)fail('explicitly preserve AF/AG artistic core');
  if(p.heavy_run_rule!=='QUICK_VISUAL_PREVIEW_FIRST')fail('use cheap Blender/Unity capture inspection before another full native certification');
  if(p.decision==='PIVOT_TO_HIGH_IMPACT'){
    if(!Array.isArray(p.next_batch)||p.next_batch.length<2)fail('pivot must group >=2 related visual corrections with a shared outcome');
    if(!validSentence(p.shared_artistic_goal,50))fail('define a single coherent artistic goal for the grouped next pass');
    if(Array.isArray(p.next_batch)){
      const unique=new Set();
      for(const [i,x] of p.next_batch.entries()){
        if(!validSentence(x.visual_change,25))fail('next_batch['+i+'] lacks a concrete visual change');
        if(!['cercana','regional','panoramica','all'].includes(x.view))fail('next_batch['+i+'] must specify the affected view');
        unique.add(x.visual_change);
      }
      if(unique.size<2)fail('next_batch must contain distinct concrete improvements');
    }
  } else {
    if(repeated.length>threshold)fail('single critical fix exception exhausted after the first narrow rework threshold; pivot to a root-cause/visual batch instead');
    if(!validSentence(p.critical_blocker,60))fail('single-fix exception requires a genuinely blocking navigability/gameplay fault and evidence');
    if(!validSentence(p.one_retry_limit,40))fail('single-fix exception must state one remaining focused cycle maximum');
  }
  if(errors.length)return;
  console.log('WORLD4X_STRATEGIC_DIRECTION_REVIEW_PASS topic='+focus+' iterations='+repeated.length+' decision='+p.decision);
  console.log('Process guard only: the written plan is not proof of visual improvement or ART_APPROVED');
}
evaluate();
if(errors.length){console.error('WORLD4X_STRATEGY_GATE_FAIL');for(const e of errors)console.error(' - '+e);process.exitCode=1;}
