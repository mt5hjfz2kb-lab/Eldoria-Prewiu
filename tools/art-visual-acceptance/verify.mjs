#!/usr/bin/env node
/**
 * Independent artistic ACCEPTANCE evidence verifier.
 * Checks declared provenance, complete matched-camera A/B bytes and truthful
 * scoring. NEVER evaluates aesthetics, trains Qwen or auto-approves work.
 * Canonical declaration directory: pipeline/art-visual-approvals/*.json
 */
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { fileURLToPath } from 'node:url';

export const RUBRIC = Object.freeze({
  geometry: 0.25, architecture: 0.25, materials: 0.20,
  style: 0.20, camera: 0.10
});
const HEX = /^[a-f0-9]{64}$/i;
const SHA = /^[a-f0-9]{40}$/i;
const PNG_SIGNATURE = Buffer.from('89504e470d0a1a0a', 'hex');
function ok(condition, message, errors) { if (!condition) errors.push(message); return !!condition; }
function hash(buffer) { return crypto.createHash('sha256').update(buffer).digest('hex'); }
function score(value, context, errors) {
  if (!value || typeof value !== 'object' || Array.isArray(value)) {
    errors.push(context + ': missing structured visual rubric'); return null;
  }
  let sum=0;
  for (const [key,weight] of Object.entries(RUBRIC)) {
    const n=value[key];
    if (typeof n !== 'number' || !Number.isFinite(n) || n < 0 || n > 10) {
      errors.push(context + ': invalid score ' + key); return null;
    }
    sum += n*weight;
  }
  return sum;
}
function verifyPngAb(pair, context, root, errors) {
  if (!ok(pair && typeof pair === 'object',context+': missing A/B evidence',errors)) return;
  ok(HEX.test(pair.camera_sha256 || ''),context+': missing same-camera SHA',errors);
  ok(typeof pair.render_profile === 'string' && pair.render_profile.length >= 3,
    context+': missing renderer/lighting profile',errors);
  ok(Number.isInteger(pair.width) && pair.width > 0 &&
     Number.isInteger(pair.height) && pair.height > 0,
    context+': missing exact width/height',errors);
  for (const which of ['before','after']) {
    const p=pair[which+'_path'];
    const expected=pair[which+'_sha256'];
    if (!ok(HEX.test(expected || ''),context+': missing '+which+' SHA256',errors)) continue;
    if (!ok(typeof p === 'string' && p.length > 4 && !path.isAbsolute(p),
      context+': invalid relative '+which+' PNG path',errors)) continue;
    const full=path.resolve(root,p);
    if (!ok(full.startsWith(path.resolve(root)+path.sep),context+': PNG path escapes checkout',errors)) continue;
    if (!ok(fs.existsSync(full) && fs.statSync(full).isFile(),
      context+': original '+which+' PNG bytes not materialized at '+p,errors)) continue;
    const b=fs.readFileSync(full);
    ok(b.length >= 45 && b.subarray(0,8).equals(PNG_SIGNATURE) &&
       b.toString('ascii',12,16)==='IHDR' && b.includes(Buffer.from('IEND')),
      context+': not a structurally identifiable PNG file '+p,errors);
    ok(hash(b)===expected.toLowerCase(),context+': PNG bytes SHA mismatch '+p,errors);
  }
  ok(pair.before_sha256 !== pair.after_sha256,
    context+': identical before/after PNG; no demonstrated intervention',errors);
}
export function validateManifest(m, root=process.cwd()) {
  const errors=[];
  if (!ok(m && typeof m==='object' && !Array.isArray(m),'manifest must be JSON object',errors)) return errors;
  ok(m.schema_version===1,'schema_version must be 1',errors);
  ok(typeof m.asset_id==='string' && m.asset_id.length>=4,'missing stable asset_id',errors);
  ok(HEX.test(m.reference_sha256||''),'missing reference SHA256',errors);
  ok(HEX.test(m.frozen_original_asset_sha256||''),'missing original source SHA256',errors);
  ok(m.status==='ART_APPROVED'||m.status==='CALIBRATION_VALIDATED',
    'Only formal ART_APPROVED or CALIBRATION_VALIDATED declarations are permitted here',errors);
  const iterations=m.iterations;
  if (!ok(Array.isArray(iterations)&&iterations.length>0&&iterations.length<=40,
    'missing bounded iteration history',errors)) return errors;
  if (m.status==='CALIBRATION_VALIDATED') ok(iterations.length>=3,
    'CALIBRATION_VALIDATED needs >=3 consecutive accepted interventions',errors);
  let previous=m.frozen_original_asset_sha256;
  let previousScore=null;
  let firstScore=null;
  iterations.forEach((it,i)=>{
    const prefix='iteration '+(i+1);
    if(!ok(it && typeof it==='object',prefix+': invalid iteration',errors))return;
    ok(Number.isInteger(it.run_id)&&it.run_id>0,prefix+': missing real run_id',errors);
    ok(SHA.test(it.commit_sha||''),prefix+': missing exact Git commit SHA',errors);
    ok(typeof it.model==='string'&&it.model.length>=3,prefix+': missing model',errors);
    ok(HEX.test(it.original_qwen_plan_sha256||''),prefix+': missing original Qwen plan hash',errors);
    ok(HEX.test(it.input_asset_sha256||''),prefix+': missing input asset SHA',errors);
    ok(HEX.test(it.output_asset_sha256||''),prefix+': missing output asset SHA',errors);
    ok(it.input_asset_sha256===previous,prefix+': broken same-asset cumulative SHA chain',errors);
    ok(it.output_asset_sha256!==it.input_asset_sha256,prefix+': unchanged output asset',errors);
    previous=it.output_asset_sha256;
    verifyPngAb(it.blender_ab,prefix+' Blender',root,errors);
    const rev=it.independent_review;
    if(!ok(rev&&typeof rev==='object',prefix+': missing independent review',errors))return;
    ok(rev.independent===true&&typeof rev.reviewer_id==='string'&&
       rev.reviewer_id.length>=4&&rev.reviewer_id!==it.model,
       prefix+': self-approval or unidentified reviewer',errors);
    ok(rev.accepted===true&&rev.protected_regions_ok===true&&
       Array.isArray(rev.severe_regressions)&&rev.severe_regressions.length===0,
       prefix+': acceptance denied or protected/severe visual regressions',errors);
    ok(typeof rev.diagnosis==='string'&&rev.diagnosis.trim().length>=20,
       prefix+': missing specific visual diagnosis',errors);
    ok(typeof rev.observed_improvement==='string'&&rev.observed_improvement.trim().length>=20,
       prefix+': missing concrete observed visual improvement',errors);
    const before=score(rev.before,prefix+' before',errors);
    const after=score(rev.after,prefix+' after',errors);
    if(before!==null&&after!==null){
      if(i===0)firstScore=before;
      ok(after-before>=0.25-1e-8,prefix+': weighted visual improvement <0.25/10',errors);
      if(previousScore!==null)ok(Math.abs(previousScore-before)<0.011,
        prefix+': score chain discontinuity (not review of previous accepted output)',errors);
      for(const key of Object.keys(RUBRIC))
        ok(rev.after[key] + 0.500001 >= rev.before[key],
          prefix+': unacceptable regression in '+key,errors);
      previousScore=after;
    }
    if(i===iterations.length-1){
      verifyPngAb(it.unity_ab,prefix+' final Unity',root,errors);
      ok(it.unity_ab?.engine==='Unity',prefix+': final A/B must be real Unity capture',errors);
      ok(it.unity_ab?.source_asset_sha256===it.output_asset_sha256,
         prefix+': Unity A/B does not certify exact output asset',errors);
    }
  });
  if(previousScore!==null)ok(previousScore>=8-1e-8,
    'ART_APPROVED needs final weighted visual score >=8/10',errors);
  if(firstScore!==null&&previousScore!==null)
    ok(previousScore>firstScore,'no demonstrated cumulative progress',errors);
  return errors;
}
function filesAt(dir) {
  if(!fs.existsSync(dir))return [];
  return fs.readdirSync(dir).filter(x=>x.endsWith('.json')).map(x=>path.join(dir,x));
}
export function runCli(argv=process.argv.slice(2)) {
  const ix=argv.indexOf('--manifest');
  const id=argv.indexOf('--directory');
  if((ix>=0)===(id>=0)) throw Error('Pass exactly one of --manifest FILE or --directory DIR');
  const root=process.cwd();
  const paths=ix>=0 ? [argv[ix+1]] : filesAt(argv[id+1]);
  if(ix>=0&&!paths[0])throw Error('missing manifest path');
  if(paths.length===0){console.log('NO_ART_APPROVAL_DECLARED: no manifests, no ART_APPROVED claim');return 0;}
  let fail=false;
  for(const file of paths){
    try{
      const errors=validateManifest(JSON.parse(fs.readFileSync(file,'utf8')),root);
      if(errors.length){fail=true;console.error('ART_VISUAL_ACCEPTANCE_FAIL '+file+'\n- '+errors.join('\n- '));}
      else console.log('ART_VISUAL_EVIDENCE_CONTRACT_PASS '+file+' (NOT automatic aesthetic approval)');
    }catch(e){fail=true;console.error('ART_VISUAL_ACCEPTANCE_FAIL '+file+': '+e.message);}
  }
  return fail?1:0;
}
if(process.argv[1] && fileURLToPath(import.meta.url)===path.resolve(process.argv[1])){
  try{process.exitCode=runCli();}catch(e){console.error(e.message);process.exitCode=2;}
}
