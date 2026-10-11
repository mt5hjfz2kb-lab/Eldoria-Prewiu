#!/usr/bin/env node
import fs from "node:fs";
import path from "node:path";

const root=process.cwd();
const policyPath=path.join(root,"pipeline","production-art-source-policy.json");
const policy=JSON.parse(fs.readFileSync(policyPath,"utf8"));

const roots=[
  path.join(root,"Unity","Assets","Eldoria"),
  path.join(root,"tools")
];

const exts=new Set([".cs",".py",".js",".mjs"]);
const ignoreParts=["Library","Temp","obj","bin","node_modules",".git","playtest","tester-v0265"];
const hits=[];

function walk(dir){
  if(!fs.existsSync(dir))return;
  for(const ent of fs.readdirSync(dir,{withFileTypes:true})){
    if(ignoreParts.some(x=>ent.name===x))continue;
    const full=path.join(dir,ent.name);
    if(ent.isDirectory()) walk(full);
    else if(exts.has(path.extname(ent.name).toLowerCase())) scan(full);
  }
}

function scan(file){
  const text=fs.readFileSync(file,"utf8");
  const rel=path.relative(root,file).replaceAll("\\","/");
  const sigs=file.endsWith(".py")?policy.primitive_signatures.blender_python:policy.primitive_signatures.csharp;
  const found=sigs.filter(s=>text.includes(s));
  if(found.length) hits.push({path:rel,signatures:found});
}

for(const r of roots)walk(r);

const manifests=[];
function collectManifests(dir){
  if(!fs.existsSync(dir))return;
  for(const ent of fs.readdirSync(dir,{withFileTypes:true})){
    const full=path.join(dir,ent.name);
    if(ent.isDirectory())collectManifests(full);
    else if(ent.name.endsWith(".production-art.json")){
      try{
        const m=JSON.parse(fs.readFileSync(full,"utf8"));
        manifests.push({path:path.relative(root,full).replaceAll("\\","/"),manifest:m});
      }catch(e){
        manifests.push({path:path.relative(root,full).replaceAll("\\","/"),parse_error:String(e)});
      }
    }
  }
}
collectManifests(path.join(root,"art-source"));
collectManifests(path.join(root,"pipeline"));

const failures=[];
for(const item of manifests){
  if(item.parse_error){failures.push({path:item.path,reason:"manifest_parse_error"});continue;}
  const m=item.manifest;
  if(m.classification!=="PRODUCTION_ART_SOURCE")continue;
  if(m.source?.authoring_method==="primitive_only")failures.push({path:item.path,reason:"production_source_marked_primitive_only"});
  if(!m.source?.sha256 || !m.export?.sha256)failures.push({path:item.path,reason:"production_source_missing_source_or_export_sha"});
  if(!Array.isArray(m.materials)||!m.materials.length)failures.push({path:item.path,reason:"production_source_missing_materials"});
  if(!m.evidence?.zoom9 || !m.evidence?.mobile)failures.push({path:item.path,reason:"production_source_missing_zoom9_or_mobile_evidence"});
  if(m.source?.authoring_standard==="BLENDER_PROFESSIONAL_V1"){
    if(!Array.isArray(m.source?.tool_families) || !m.source.tool_families.length) failures.push({path:item.path,reason:"professional_blender_source_missing_tool_families"});
    if(!m.source?.primitive_role) failures.push({path:item.path,reason:"professional_blender_source_missing_primitive_role"});
    if(!m.art_review || m.art_review.standard!=="BLENDER_PROFESSIONAL_V1") failures.push({path:item.path,reason:"professional_blender_source_missing_art_review"});
    if(!Array.isArray(m.art_review?.preview_evidence) || !m.art_review.preview_evidence.length) failures.push({path:item.path,reason:"professional_blender_source_missing_preview_evidence"});
    if(m.art_review?.verdict!=="PASS") failures.push({path:item.path,reason:"professional_blender_source_art_review_not_pass"});
  }
}

const report={
  generated_at:new Date().toISOString(),
  policy:"pipeline/production-art-source-policy.json",
  primitive_signature_files:hits,
  production_manifests:manifests.map(x=>({path:x.path,classification:x.manifest?.classification,parse_error:x.parse_error})),
  failures,
  verdict:failures.length?"FAIL":"PASS"
};

const out=path.join(root,"pipeline","production-art-source-audit.json");
fs.writeFileSync(out,JSON.stringify(report,null,2)+"\n");
console.log(JSON.stringify(report,null,2));

if(process.argv.includes("--enforce") && failures.length)process.exit(1);
