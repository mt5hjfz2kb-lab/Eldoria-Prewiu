#!/usr/bin/env node
/**
 * Validate an ACTUAL Blender --inspect manifest for the already-integrated
 * Engendro. Never creates geometry, guesses a face index or approves art.
 * The upstream authoring engine remains execute_artist_plan.py.
 */
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { loadCatalog } from './preflight.mjs';

const ROOT=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../..');
const sha=bytes=>crypto.createHash('sha256').update(bytes).digest('hex');
const vec3=x=>Array.isArray(x)&&x.length===3&&x.every(n=>typeof n==='number'&&Number.isFinite(n));

export function assertRealCreatureMeshContext(ctx, pinnedSourceSha, expectedTarget='Spawnling_Near_Authored') {
  if(!ctx||ctx.schema_version!==1||ctx.target!==expectedTarget||ctx.source_sha256!==pinnedSourceSha)
    throw new Error('WRONG_MESH_ID_OR_SOURCE_SHA');
  if(!Number.isInteger(ctx.vertices)||ctx.vertices<1000||ctx.vertices>1_000_000 ||
     !Number.isInteger(ctx.faces)||ctx.faces<500||ctx.faces>2_000_000)
    throw new Error('MESH_COUNT_OUTSIDE_ACTUAL_PRODUCTION_SCALE');
  const low=ctx.local_aabb?.min, high=ctx.local_aabb?.max;
  if(!vec3(low)||!vec3(high)||low.some((n,i)=>n>=high[i]||high[i]-n>100))
    throw new Error('INVALID_SOURCE_LOCAL_BOUNDS');
  if(!Array.isArray(ctx.sample_vertices)||ctx.sample_vertices.length<8||ctx.sample_vertices.length>50)
    throw new Error('MISSING_OR_TOO_MANY_REAL_MESH_ANCHORS');
  const seen=new Set();
  for(const a of ctx.sample_vertices){
    if(!Number.isInteger(a.index)||a.index<0||a.index>=ctx.vertices||seen.has(a.index)||
       !vec3(a.position)||a.position.some((n,i)=>n<low[i]-0.001||n>high[i]+0.001))
       throw new Error('MESH_ANCHOR_NOT_ON_VERIFIED_SOURCE');
    seen.add(a.index);
  }
  if(!Array.isArray(ctx.boundary_loops)||ctx.boundary_loops.length>12)
    throw new Error('UNTRUSTED_BOUNDARY_LOOP_LIST');
  for(const l of ctx.boundary_loops){
    if(!Array.isArray(l.indices)||l.indices.length<3||l.indices.length>48||
       new Set(l.indices).size!==l.indices.length||
       l.indices.some(n=>!Number.isInteger(n)||n<0||n>=ctx.vertices))
       throw new Error('UNTRUSTED_BOUNDARY_LOOP');
  }
  return {status:'REAL_BLENDER_SOURCE_CONTEXT_PASS',art_approved:false,rigging_proven:false,
    qwen_model_called:false,target:ctx.target,source_sha256:ctx.source_sha256,
    vertices:ctx.vertices,faces:ctx.faces,local_bounds:ctx.local_aabb,
    sampled_real_vertices:ctx.sample_vertices.length,verified_boundary_loops:ctx.boundary_loops.length};
}

const directly=process.argv[1]&&path.resolve(process.argv[1])===fileURLToPath(import.meta.url);
if(directly){
  try{
    const [src,out]=process.argv.slice(2);
    if(!src||!out)throw new Error('USAGE verify-mesh-context.mjs /actual/mesh-context.json /outside/repo/report.json');
    const data=fs.readFileSync(src);
    const ctx=JSON.parse(data.toString('utf8'));
    const catalog=loadCatalog(ROOT);
    const report=assertRealCreatureMeshContext(ctx,catalog.assets.spawnling.integration.editable.sha256);
    report.mesh_context_sha256=sha(data);
    const output=path.resolve(out);
    if(output===ROOT||output.startsWith(ROOT+path.sep))throw new Error('OUTPUT_MUST_BE_OUTSIDE_REPO');
    fs.mkdirSync(path.dirname(output),{recursive:true});
    fs.writeFileSync(output,JSON.stringify(report,null,2)+'\n',{flag:'wx'});
    console.log(JSON.stringify(report,null,2));
  }catch(err){console.error('SOURCE_MESH_CONTEXT_FAIL',err.message);process.exitCode=2;}
}
