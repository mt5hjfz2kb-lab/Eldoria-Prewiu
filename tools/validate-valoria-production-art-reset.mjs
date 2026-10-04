#!/usr/bin/env node
import fs from "node:fs";

const requiredFiles=[
  "docs/VALORIA_VISUAL_BIBLE.md",
  "docs/ELDORIA_PRODUCTION_ART_SOURCE_PIPELINE.md",
  "docs/VALORIA_FINAL_LOOK_AND_CAMERA_AB_PLAN.md",
  "pipeline/valoria-production-material-stack.json",
  "pipeline/valoria-production-art-source-schema.json",
  "pipeline/production-art-source-policy.json",
  "pipeline/valoria-final-look-profiles.json",
  "pipeline/valoria-camera-ab-spec.json",
  "pipeline/valoria-density-life-plan.json",
  "pipeline/valoria-production-art-rollout.json",
  "tools/check-production-art-source.mjs",
  "tools/valoria-production-art-starter/build_starter_family_v1.py"
];

const failures=[];
for(const p of requiredFiles) if(!fs.existsSync(p)) failures.push("missing:"+p);

function json(p){try{return JSON.parse(fs.readFileSync(p,"utf8"))}catch(e){failures.push("bad_json:"+p+":"+e.message);return null}}
const stack=json("pipeline/valoria-production-material-stack.json");
const schema=json("pipeline/valoria-production-art-source-schema.json");
const policy=json("pipeline/production-art-source-policy.json");
const look=json("pipeline/valoria-final-look-profiles.json");
const cam=json("pipeline/valoria-camera-ab-spec.json");
const density=json("pipeline/valoria-density-life-plan.json");
const rollout=json("pipeline/valoria-production-art-rollout.json");

for(const fam of ["Eldoria_Stone","Eldoria_Timber","Eldoria_Slate","Eldoria_Ground","Eldoria_Rock","Eldoria_Metal_Accent"]){
  if(!stack?.families?.[fam])failures.push("missing_material_family:"+fam);
}
if(!schema?.properties?.classification)failures.push("source_schema_missing_classification");
if(!policy?.classifications?.includes("PRODUCTION_ART_SOURCE"))failures.push("policy_missing_production_class");
if(!look?.profiles?.FINAL_LOOK_CANDIDATE)failures.push("final_look_missing_candidate");
if(cam?.canonical_until_proven!=="ORTHOGRAPHIC")failures.push("camera_canonical_not_orthographic");
if(!Array.isArray(density?.reject)||!density.reject.length)failures.push("density_missing_reject_rules");
if(!Array.isArray(rollout?.sequence)||rollout.sequence.length<5)failures.push("rollout_too_short");

const src=fs.readFileSync("tools/valoria-production-art-starter/build_starter_family_v1.py","utf8");
for(const token of ["boolean_difference","add_window_recess","add_door_recess","add_arch_ring","roof_gable_solid","Valoria_StarterFamily_v1.blend"]){
  if(!src.includes(token))failures.push("starter_builder_missing:"+token);
}
if(!src.includes('"classification":"TEMPORARY"')) failures.push("starter_candidates_must_begin_temporary");
if(src.includes('"classification":"PRODUCTION_ART_SOURCE"')) failures.push("starter_builder_premature_production_classification");

const bible=fs.readFileSync("docs/VALORIA_VISUAL_BIBLE.md","utf8");
for(const token of ["premium medieval fantasy","Flat Citadel","maquette","PRODUCTION ART SOURCE"]){
  if(!bible.toLowerCase().includes(token.toLowerCase())) failures.push("visual_bible_missing:"+token);
}

const result={checked_at:new Date().toISOString(),required_files:requiredFiles.length,failures,verdict:failures.length?"FAIL":"PASS"};
fs.mkdirSync("pipeline/evidence",{recursive:true});
fs.writeFileSync("pipeline/evidence/valoria-production-art-reset-static-gate.json",JSON.stringify(result,null,2)+"\n");
console.log(JSON.stringify(result,null,2));
if(failures.length)process.exit(1);
