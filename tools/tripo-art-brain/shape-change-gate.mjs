#!/usr/bin/env node
/**
 * Reject a fake 'sculpture' that primarily translates an existing creature.
 * This is a structural QA REJECTION gate, never a visual/ART approval.
 */
import fs from 'node:fs';
import crypto from 'node:crypto';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {loadCatalog} from './preflight.mjs';
const ROOT=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../..');
const SHA=b=>crypto.createHash('sha256').update(b).digest('hex');
const numeric=x=>typeof x==='number'&&Number.isFinite(x);
const vec=x=>Array.isArray(x)&&x.length===3&&x.every(numeric);
const distance=(a,b)=>Math.hypot(...a.map((z,i)=>z-b[i]));
const dims=s=>s.hi.map((v,i)=>v-s.lo[i]);
const center=s=>s.hi.map((v,i)=>(v+s.lo[i])/2);

export function evaluateCreatureShape(comparison,plan){
  const a=comparison?.source_geometry,b=comparison?.edited_geometry;
  if(!a||!b||!vec(a.lo)||!vec(a.hi)||!vec(b.lo)||!vec(b.hi)||
     !Number.isInteger(a.verts)||!Number.isInteger(b.verts)||
     !Number.isInteger(a.faces)||!Number.isInteger(b.faces)||
     !Array.isArray(plan)||!plan.length||a.verts<1000||
     !numeric(comparison?.camera_visibility?.changed_fraction))
    throw new Error('INVALID_AUTHENTIC_SHAPE_COMPARISON');
  const da=dims(a),db=dims(b);
  if(da.some(n=>n<=0)||db.some(n=>n<=0))throw new Error('INVALID_MESH_AABB');
  const diagonal=Math.hypot(...da);
  const centerShift=distance(center(a),center(b));
  const relativeExtentChange=Math.max(...da.map((d,i)=>Math.abs(db[i]-d)/d));
  const sameTopology=a.verts===b.verts&&a.faces===b.faces;
  const shapeOps=plan.filter(x=>x?.type==='sculpt_region'||x?.type==='sculpt_path');
  const overscale=shapeOps.some(op=>{
    const params=op.parameters||{};
    if(op.type==='sculpt_region')return numeric(params.radius)&&params.radius>diagonal*.6;
    if(op.type==='sculpt_path')return (params.knots||[]).some(k=>numeric(k.radius)&&k.radius>diagonal*.6);
    return false;
  });
  const translationLike=sameTopology&&shapeOps.length>0&&centerShift>diagonal*.12&&relativeExtentChange<.025;
  const verdict=overscale||translationLike?'REJECT_NOT_MEANINGFUL_CREATURE_SHAPE':'STRUCTURAL_CANDIDATE_INDEPENDENT_VISUAL_REVIEW_REQUIRED';
  return {schema_version:1,verdict,original_diagonal_m:diagonal,
    center_shift_m:centerShift,center_shift_ratio:centerShift/diagonal,
    max_relative_extent_change:relativeExtentChange,same_topology:sameTopology,
    overscale_radius:overscale,translation_like:translationLike,
    image_changed_fraction:comparison.camera_visibility.changed_fraction,
    art_approved:false,unity_art_approved:false,mobile_pass:false,
    note:verdict.startsWith('REJECT_')?
      'Moving nearly the full animal is not character sculpting. Preserve original and record lesson.':
      'This passes only a structural preflight, not independent art quality or Unity QA.'};
}

const main=process.argv[1]&&path.resolve(process.argv[1])===fileURLToPath(import.meta.url);
if(main){
  try{
    const [cmpPath,planPath,actualBlend,outPath]=process.argv.slice(2);
    if(!cmpPath||!planPath||!actualBlend||!outPath)throw new Error('USAGE: shape-change-gate.mjs comparison.json plan.json actual-output.blend OUT.json');
    const cmp=JSON.parse(fs.readFileSync(cmpPath,'utf8'));
    const plan=JSON.parse(fs.readFileSync(planPath,'utf8'));
    const catalog=loadCatalog(ROOT);
    if(cmp.source_file_sha256!==catalog.assets.spawnling.integration.editable.sha256||
       cmp.result_file_sha256!==SHA(fs.readFileSync(actualBlend)))
       throw new Error('SOURCE_OR_RESULT_BYTES_NOT_PINNED');
    const result=evaluateCreatureShape(cmp,plan);
    const out=path.resolve(outPath);
    if(out===ROOT||out.startsWith(ROOT+path.sep))throw new Error('OUTPUT_MUST_BE_OUTSIDE_REPO');
    fs.writeFileSync(out,JSON.stringify(result,null,2)+'\n',{flag:'wx'});
    console.log('CREATURE_SHAPE_GATE',result.verdict,'shift_ratio',result.center_shift_ratio.toFixed(3),
      'extent_drift',result.max_relative_extent_change.toFixed(4));
    if(result.verdict.startsWith('REJECT_'))process.exitCode=3;
  }catch(err){console.error('CREATURE_SHAPE_GATE_ERROR',err.message);process.exitCode=2}
}
