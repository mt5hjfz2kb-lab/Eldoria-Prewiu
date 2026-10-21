import {test} from 'node:test';
import assert from 'node:assert/strict';
import {assertRealCreatureMeshContext} from './verify-mesh-context.mjs';

const pinned='a'.repeat(64);
const original=()=>({
 schema_version:1,target:'Spawnling_Near_Authored',source_sha256:pinned,
 vertices:6000,faces:12000,
 local_aabb:{min:[-0.5,-0.4,-0.4],max:[0.5,0.4,0.4]},
 sample_vertices:Array.from({length:10},(_,index)=>({
  index,position:[-0.45+index*.09,-0.3+index*.06,-0.2+index*.04],
  role:'sampled_original_vertex'
 })),
 boundary_loops:[]
});

test('real indexed source points and exact source identity are required',()=>{
 const j=original();const x=assertRealCreatureMeshContext(j,pinned);
 assert.equal(x.status,'REAL_BLENDER_SOURCE_CONTEXT_PASS');
 assert.equal(x.art_approved,false);
 assert.equal(x.rigging_proven,false);
 assert.equal(x.qwen_model_called,false);
 assert.equal(x.sampled_real_vertices,10);
});
test('mismatched source SHA and target name always reject',()=>{
 assert.throws(()=>assertRealCreatureMeshContext(original(),'b'.repeat(64)),/WRONG_MESH_ID/);
 const z=original();z.target='fabricated';
 assert.throws(()=>assertRealCreatureMeshContext(z,pinned),/WRONG_MESH_ID/);
});
test('out-of-mesh vertices, repeated vertex IDs and broken bounds reject',()=>{
 const z=original();z.sample_vertices[0].position=[40,0,0];
 assert.throws(()=>assertRealCreatureMeshContext(z,pinned),/MESH_ANCHOR/);
 const a=original();a.sample_vertices[3].index=1;
 assert.throws(()=>assertRealCreatureMeshContext(a,pinned),/MESH_ANCHOR/);
 const b=original();b.local_aabb.max=[-0.6,0.4,0.4];
 assert.throws(()=>assertRealCreatureMeshContext(b,pinned),/INVALID_SOURCE/);
});
test('fabricated topology loops and undersized mesh data are not accepted',()=>{
 const z=original();z.boundary_loops=[{indices:[1,2,7000]}];
 assert.throws(()=>assertRealCreatureMeshContext(z,pinned),/UNTRUSTED_BOUNDARY_LOOP/);
 const j=original();j.vertices=5;
 assert.throws(()=>assertRealCreatureMeshContext(j,pinned),/PRODUCTION_SCALE/);
});
