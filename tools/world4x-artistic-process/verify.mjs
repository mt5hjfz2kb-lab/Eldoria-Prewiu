#!/usr/bin/env node
// Validate PROCESS evidence; this is NOT an automatic artistic approval.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {execFileSync} from 'node:child_process';

const root=path.resolve(process.argv[2] || '.');
const evidence='docs/evidence/world4x-canon-20261022/ARTISTIC_PROCESS_CURRENT.json';
const hex=/^[a-f0-9]{64}$/i, commit=/^[a-f0-9]{40}$/i;
const errors=[];
function insist(ok,message){if(!ok)errors.push(message);return Boolean(ok);}
function ownedFile(p,description){
  if(!insist(typeof p==='string' && p.length>0,description+': missing path'))return null;
  const resolved=path.resolve(root,p);
  if(!insist(resolved.startsWith(root+path.sep) && fs.existsSync(resolved) && fs.statSync(resolved).isFile(),description+': missing/unsafe file '+p))return null;
  return resolved;
}
function readJson(p,description){
  const file=ownedFile(p,description);if(!file)return null;
  try{return JSON.parse(fs.readFileSync(file,'utf8'));}catch(e){errors.push(description+': invalid JSON: '+e.message);return null;}
}
function sha(file){return crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex');}
function pngInfo(file){
  const b=fs.readFileSync(file);
  if(!insist(b.length>45 && b.subarray(0,8).equals(Buffer.from([137,80,78,71,13,10,26,10])) && b.toString('ascii',12,16)==='IHDR','not a valid PNG: '+file))return null;
  return {width:b.readUInt32BE(16),height:b.readUInt32BE(20),hash:crypto.createHash('sha256').update(b).digest('hex')};
}
function nontrivial(s,n){return typeof s==='string' && s.trim().length>=n;}
const record=readJson(evidence,'Artistic process record');
if(record){
  insist(record.schema_version===1,'schema_version must be 1');
  insist(record.workstream_id==='eldoria-world-region-1-canon-20261022','wrong workstream identity');
  insist(commit.test(record.certified_source_commit||''),'certified_source_commit must be exact Git SHA');
  if(commit.test(record.certified_source_commit||'')){
    try{
      execFileSync('git',['-C',root,'merge-base','--is-ancestor',record.certified_source_commit,'HEAD'],{stdio:'ignore'});
      const files=execFileSync('git',['-C',root,'diff','--name-only',record.certified_source_commit,'HEAD','--','Unity/Assets/Eldoria/World4XCanon','docs/evidence/world4x-canon-20261022/authoring'],{encoding:'utf8'}).trim();
      insist(!files,'Art code changed since certified source commit without a new A/B cycle: '+files.slice(0,350));
    }catch(e){errors.push('certified_source_commit not in current branch history or git unavailable');}
  }
  const b=record.blender||{};
  insist(['blender-mcp-interactive','blender-bpy-reviewed'].includes(b.method),'record real Blender method (MCP or reviewed bpy)');
  insist(['verified','unavailable','not-used'].includes(b.mcp_status),'MCP preflight/status missing');
  insist(nontrivial(b.mcp_preflight,25),'MCP preflight must document an actual check and decision');
  if(b.method==='blender-mcp-interactive')insist(b.mcp_status==='verified','MCP method requires verified interactive connection');
  if(b.method==='blender-bpy-reviewed')insist(nontrivial(b.mcp_reason,30),'bpy fallback needs a concrete reason and visual review');
  const trace=ownedFile(b.artistic_decision_log,'real artistic decision/inspect/correct log');
  if(trace){
    const t=fs.readFileSync(trace,'utf8').toLowerCase();
    for(const word of ['observ','decisi','correc','veredic'])insist(t.includes(word),'artistic decision log lacks stage: '+word);
  }
  for(const side of ['input','output']){
    let p=ownedFile(b[side+'_blend'],'Blender '+side+' .blend');
    insist(!p || p.endsWith('.blend'),'Blender '+side+' source must be an editable .blend');
    insist(hex.test(b[side+'_sha256']||''),'Blender '+side+' SHA256 missing');
    if(p && hex.test(b[side+'_sha256']||''))insist(sha(p)===b[side+'_sha256'],'Blender '+side+' source SHA256 mismatch');
  }
  const review=ownedFile(record.visual_review_path,'visual verdict/review');if(review){
    insist(fs.statSync(review).size>150,'visual review is too short to document a real decision');
  }
  const v=record.visual_judgment||{};
  insist(['RETAINED','REJECTED','PENDING'].includes(v.outcome),'visual outcome must be explicit');
  insist(nontrivial(v.observed_improvement,40),'visual change must be described, not just TECH PASS');
  insist(nontrivial(v.remaining_gaps,30),'remaining visual defects must be documented');
  insist(nontrivial(v.reviewer,3),'visual reviewer not identified');
  const before=record.baseline||{},after=record.candidate||{};
  insist(Number.isInteger(before.run_id)&&before.run_id>0,'baseline run_id missing');
  insist(Number.isInteger(after.run_id)&&after.run_id>0&&after.run_id!==before.run_id,'different candidate native run_id missing');
  const labels=['cercana-horizontal','cercana-vertical','regional-horizontal','regional-vertical','panoramica-horizontal','panoramica-vertical'];
  let changes=0;
  for(const label of labels){
    let a={};
    for(const [key,snap] of [['before',before],['after',after]]){
      const dirname=snap.directory;
      const base=typeof dirname==='string'?dirname+'/'+label:null;
      const pic=base?ownedFile(base+'-unity.png',key+' '+label+' genuine Unity image'):null;
      const metadata=base?readJson(base+'-source.json',key+' '+label+' camera proof'):null;
      if(pic&&metadata){
        const info=pngInfo(pic);
        if(info){
          insist(metadata.actualUnityCamera===true&&metadata.playMode===true,key+' '+label+': not marked real Unity PlayMode camera');
          insist(metadata.label===label,key+' '+label+': wrong capture label');
          insist(metadata.width===info.width&&metadata.height===info.height,key+' '+label+': image dimensions mismatch');
          insist(metadata.sha256===info.hash,key+' '+label+': camera manifest PNG SHA256 mismatch');
          a[key]={metadata,hash:info.hash};
        }
      }
    }
    if(a.before&&a.after){
      for(const field of ['zoom','aspect','cameraPosition','cameraRotation','worldTarget']){
        const x=JSON.stringify(a.before.metadata[field]),y=JSON.stringify(a.after.metadata[field]);
        insist(x===y,label+': A/B camera '+field+' not identical; re-render matched control cameras');
      }
      if(a.before.hash!==a.after.hash)changes++;
    }
  }
  insist(changes>0,'A/B comparison contains no changed pixels in all six matched Unity views');
  // A genuine process check can pass for a REVIEW_REQUIRED candidate; NEVER infer ART_APPROVED.
  insist(v.outcome==='RETAINED','candidate not retained after visual review; not eligible for closeout');
}
if(errors.length){
  console.error('WORLD4X_ARTISTIC_PROCESS_FAIL');
  for(const msg of errors)console.error(' - '+msg);
  process.exitCode=1;
}else{
  console.log('WORLD4X_ARTISTIC_PROCESS_EVIDENCE_PASS (NOT ART_APPROVED)');
  console.log('Verified: actual editable Blender source, MCP preflight, inspect/decide/correct log, current committed art and six authentic same-camera Unity A/B pairs.');
}
