import fs from "node:fs";

const file=process.argv[2] || "pipeline/blender-professional-authoring-standard.json";
const s=JSON.parse(fs.readFileSync(file,"utf8"));
const required=["standard_id","applies_to","principles","presumptive_rejection_patterns","art_review_questions","hard_stop_questions","source_report_required_fields"];
const errors=[];
for(const k of required){ if(!(k in s)) errors.push("missing:"+k); }
if(s.standard_id!=="BLENDER_PROFESSIONAL_V1") errors.push("unexpected_standard_id");
if(!s.art_review_required) errors.push("art_review_must_be_required");
for(const q of s.hard_stop_questions||[]){ if(!(s.art_review_questions||[]).includes(q)) errors.push("hard_stop_not_in_questions:"+q); }
if(!(s.applies_to?.roles||[]).includes("PRIMARY") || !(s.applies_to?.roles||[]).includes("SECONDARY")) errors.push("visible_environment_roles_missing");
const out={standard:s.standard_id,errors,verdict:errors.length?"FAIL":"PASS"};
console.log(JSON.stringify(out,null,2));
if(errors.length) process.exit(2);
