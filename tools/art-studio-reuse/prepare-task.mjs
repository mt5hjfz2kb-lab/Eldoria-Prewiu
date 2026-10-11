#!/usr/bin/env node
// Eldoria AI art studio: isolated reuse readiness + new task manifest.
// No paid calls, CI dispatch, branch promotion or Unity changes.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
const args = process.argv.slice(2);
const option = n => {const i=args.indexOf(n);return i < 0 ? null : args[i+1];};
const root=process.cwd();
const required=[
 'AGENTS.md',
 'pipeline/active-workstreams.json',
 'pipeline/art-studio-recovery-v1.json',
 'tools/art-studio-recovery/REPRODUCE.md',
 'tools/art-studio-recovery/produce_rock_study.py',
 'tools/art-studio-recovery/decision_policy.py',
 'tools/art-studio-recovery/review_with_local_vision.py',
 'tools/art-studio-recovery/mcp_repair_contacts.py',
 'tools/art-studio-recovery/author_cluster_v2.py',
 'tools/art-studio-recovery/ValoriaStudioRockUnityProof.cs',
 '.github/workflows/art-studio-recovery-v1.yml',
 'tools/plan-art-production.mjs',
 'tools/check-art-production-governance.mjs'
];
const checks=required.map(file=>({file,exists:fs.existsSync(path.join(root,file))}));
const config=JSON.parse(fs.readFileSync('pipeline/art-studio-recovery-v1.json','utf8'));
const safety={
 zeroPaid:config.budget_eur===0 && config.paid_generation_allowed===false,
 autoPromoteDisabled:config.auto_promote===false,
 runtimeProtected:config.unity_runtime_protected===true,
 localVision:/local/i.test(config.existing_vision_transport||''),
 rejectionEnabled:Array.isArray(config.allowed_techniques)&&config.allowed_techniques.includes('reject_all'),
};
const ready=checks.every(x=>x.exists)&&Object.values(safety).every(Boolean);
const workflow=fs.readFileSync('.github/workflows/art-studio-recovery-v1.yml','utf8');
const warnings=[];
if(!/workflow_dispatch:/.test(workflow)) warnings.push('Experimental workflow is not manually dispatchable; push scoped changes on isolated task branch.');
warnings.push('AI evaluation is advisory, not ART PASS.');
warnings.push('Unity integration, mobile performance and always-on agent routing are NOT validated.');
const report={schema_version:1,ready_for_isolated_reuse:ready,checks,safety,warnings};
if(args.includes('--check')){console.log(JSON.stringify(report,null,2));process.exit(ready?0:1);}
const family=option('--family'),reference=option('--reference'),camera=option('--camera'),output=option('--out');
if(!ready){console.error(JSON.stringify(report,null,2));process.exit(1);}
if(!family||!reference||!camera||!output){console.error('Usage: node tools/art-studio-reuse/prepare-task.mjs --family rock|architecture|vegetation --reference path --camera description --out path');process.exit(2);}
if(!['rock','architecture','vegetation'].includes(family)){console.error('Unsupported family; do not silently claim scripts generalize to new asset families.');process.exit(2);}
const refPath=path.resolve(root,reference);
if(!fs.existsSync(refPath)||!fs.statSync(refPath).isFile()){console.error('Reference not found: '+reference);process.exit(2);}
const sha256=crypto.createHash('sha256').update(fs.readFileSync(refPath)).digest('hex');
const request={schema_version:1,task_family:family,reference_path:path.relative(root,refPath),reference_sha256:sha256,requested_camera:camera,policy:'ISOLATED_RESEARCH_ONLY',auto_accept:false,auto_integrate_unity:false,paid_generation_allowed:false,required_gates:['real_blender_render','GLB_audit','human_art_review','isolated_unity_strategic_camera','mobile_performance_if_targeted'],reuse_proven_pipeline:family==='rock',known_runner_constraints:'Consult pipeline/active-workstreams.json before runner access',notes:family==='rock'?'Existing art-studio-recovery scripts are usable subject to owner/branch verification.':'No validated autonomous Blender recipe for this family; require explicit technical adaptation before use.'};
fs.mkdirSync(path.dirname(path.resolve(root,output)),{recursive:true});
fs.writeFileSync(path.resolve(root,output),JSON.stringify(request,null,2)+'\n',{flag:'wx'});
console.log(JSON.stringify({created:output,reference_sha256:sha256,autonomous_production_ready:family==='rock'},null,2));
