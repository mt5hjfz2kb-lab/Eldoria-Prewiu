import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {evaluateCreatureShape} from './shape-change-gate.mjs';
const ROOT=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../..');

function actualRejected(){
 const j=JSON.parse(fs.readFileSync(path.join(ROOT,
 'pipeline/tripo-art-brain/verified-failures/spawnling-qwen2b-global-move-20261021.json'),'utf8'));
 return {j,comparison:{source_geometry:{lo:j.geometry.original_aabb_min,hi:j.geometry.original_aabb_max,verts:j.action.total_target_vertices,faces:12000},
 edited_geometry:{lo:j.geometry.edited_aabb_min,hi:j.geometry.edited_aabb_max,verts:j.action.total_target_vertices,faces:12000},
 camera_visibility:{changed_fraction:j.visual.changed_fraction}},
 plan:[{type:j.action.type,parameters:{center:j.action.center,radius:j.action.radius,delta:j.action.delta}}]};
}
test('authentic Qwen run 39628068735 must be REJECTED despite 2.8% changed camera pixels',()=>{
 const {j,comparison,plan}=actualRejected();
 assert.equal(j.source.repo_run,39628068735);
 const x=evaluateCreatureShape(comparison,plan);
 assert.equal(x.verdict,'REJECT_NOT_MEANINGFUL_CREATURE_SHAPE');
 assert.equal(x.overscale_radius,true);
 assert.equal(x.translation_like,true);
 assert.ok(x.center_shift_ratio>.5);
 assert.equal(x.art_approved,false);
 assert.equal(x.image_changed_fraction,0.02802409);
});
test('scoped head reshaping with preserved global center is not falsely rejected',()=>{
 const {comparison,plan}=actualRejected();
 plan[0].parameters.radius=.18;
 plan[0].parameters.delta=[0,.018,-.025];
 comparison.edited_geometry={...comparison.edited_geometry,lo:[...comparison.source_geometry.lo],
    hi:[comparison.source_geometry.hi[0]+.04,comparison.source_geometry.hi[1],comparison.source_geometry.hi[2]]};
 const x=evaluateCreatureShape(comparison,plan);
 assert.equal(x.verdict,'STRUCTURAL_CANDIDATE_INDEPENDENT_VISUAL_REVIEW_REQUIRED');
 assert.equal(x.overscale_radius,false);
 assert.equal(x.translation_like,false);
 assert.equal(x.art_approved,false);
});
test('cannot call malformed evidence or fake counts successful art',()=>{
 const {comparison,plan}=actualRejected();
 const bad=structuredClone(comparison);bad.edited_geometry.lo=[NaN,0,0];
 assert.throws(()=>evaluateCreatureShape(bad,plan),/INVALID_AUTHENTIC_SHAPE_COMPARISON/);
 const another=structuredClone(comparison);another.source_geometry.hi=[0,0,0];
 another.source_geometry.lo=[0,0,0];
 assert.throws(()=>evaluateCreatureShape(another,plan),/INVALID_MESH_AABB/);
});
