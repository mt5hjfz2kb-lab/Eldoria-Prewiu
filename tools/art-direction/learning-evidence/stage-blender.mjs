#!/usr/bin/env node
// Stage a genuine Qwen/Blender clay comparison for later independently verified learning.
// Stage only: NEVER art approval, NEVER a live Unity comparison, NEVER automatic promotion.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {fileURLToPath} from 'node:url';
import {validate,appendPending} from './learning.mjs';
import {measureUnityVisualChange} from '../ai-authoring/unity_pixel_diff.mjs';

const HERE=path.dirname(fileURLToPath(import.meta.url));
const hex=/^[a-f0-9]{64}$/;
const gitsha=/^[a-f0-9]{40}$/;
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
const need=(ok,msg)=>{if(!ok)throw Error(msg)};
const read=p=>{need(p&&fs.existsSync(p),'Evidence missing: '+p);const s=fs.statSync(p);
  need(s.isFile()&&s.size<26000000,'Invalid evidence file: '+p);return fs.readFileSync(p)};
export function stageBlender({dbPath=path.join(HERE,'observations.json'),root,runId,gitSha,out}) {
 need(Number.isSafeInteger(runId)&&runId>0,'Real run ID required');
 need(gitsha.test(gitSha||''),'Real source SHA required');
 need(root&&out,'Existing artifact root and distinct output required');
 need(path.resolve(out)!==path.resolve(dbPath)&&!fs.existsSync(out),'Never overwrite canonical or prior evidence');
 const db=JSON.parse(read(dbPath));need(validate(db).length===0,'Invalid original memory catalog');
 const comparison=JSON.parse(read(path.join(root,'ab','comparison.json')));
 need(comparison.scope==='Blender clay A/B only, NOT Unity, NOT ART_APPROVED','Must use original Blender-only comparison');
 need(comparison.camera==='verified Bastion I player-equivalent orthographic camera','Unverified camera preset');
 need(comparison.player_frustum?.sensor_fit==='VERTICAL','Incorrect player camera projection');
 need(JSON.stringify(comparison.player_frustum?.pixels)==='[720,500]','Unexpected camera pixel dimensions');
 need(comparison.target==='Valoria natural playable hilltop and rock scarps','Unexpected artist target');
 const b=read(path.join(root,'ab','before-blender-clay.png'));
 const a=read(path.join(root,'ab','after-blender-clay.png'));
 need(sha(b)===comparison.before_image_sha256&&sha(a)===comparison.after_image_sha256,'Original clay frame hashes differ from actual images');
 const bSource=read(path.join(root,'terrain-source.blend'));
 const aSource=read(path.join(root,'result','artist-output.blend'));
 need(sha(bSource)===comparison.source_file_sha256&&sha(aSource)===comparison.result_file_sha256,'Original Blender model bytes differ from renderer manifest');
 const inference=JSON.parse(read(path.join(root,'model','artist-inference.json')));
 const plan=JSON.parse(read(path.join(root,'model','artist-plan.json')));
 need(inference.system_role==='system'&&inference.model.startsWith('qwen3-vl:'),'Missing genuine Qwen author');
 need(JSON.stringify(inference.direction?.planned_actions)===JSON.stringify(plan),'Saved plan does not match Qwen original');
 const ref=read('references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg');
 need(sha(ref)===db.reference_sha256,'Artist reference is not the approved source');
 // Re-measure 8-bit real PNGs independently. Blender's float threshold differs;
 // record the actual RGB threshold of THIS measurement, not the original one.
 const m=measureUnityVisualChange(b,a);
 need(m.width===720&&m.height===500,'Unexpected Blender frame size');
 const visible=m.gate==='INDEPENDENT_REVIEW_REQUIRED';
 const id='eldoria-bastion1-'+runId;
 const event={id,target:'bastion1',
   source:{run_id:runId,git_sha:gitSha,evidence_level:'pending_verification',
    url:'https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/'+runId},
   reference_sha256:db.reference_sha256,view_id:'bastion1-blender-clay-720x500',render_pass:'blender_clay',
   measurement:{kind:'camera_pixels',width:m.width,height:m.height,changed_pixels:m.changed_pixels,
     threshold_rgb:m.channel_difference_threshold},
   visibility_gate:visible?'REVIEW_REQUIRED':'REJECT_NOT_VISIBLE',
   art_verdict:'ART_UNREVIEWED',art_approved:false,
   lesson_id:visible?'blender-clay-requires-unity-review':'camera-invisible-qwen-intervention',
   diagnosis:visible
     ? 'El cambio Qwen se detecta en '+m.changed_pixels+' de '+m.total_pixels+' pixeles del render Blender, sin validar calidad en Unity.'
     : 'El cambio Qwen no alcanza 0,1% de cobertura visible en los renders Blender alineados: '+m.changed_pixels+'/'+m.total_pixels+'.',
   lesson:visible
     ? 'Conservar plan original y comparar con cámara y materiales idénticos en Unity antes de juzgar la calidad visual.'
     : 'No repetir una intervencion poco visible; elegir otra zona o estructura con Qwen y comprobar encuadre antes de Unity.',
   next_action:visible
     ? 'Solicitar al responsable inspeccion artistica independiente y captura nativa Unity de la misma zona.'
     : 'Volver a Qwen con esta evidencia, sin ejecutar un build ni una QA completa del candidato rechazado.',
   candidate_provenance:{
     qwen_model:inference.model,qwen_plan_sha256:sha(read(path.join(root,'model','artist-plan.json'))),
     original_renderer_manifest_sha256:sha(read(path.join(root,'ab','comparison.json'))),
     locked_blender_camera_preset_sha256:sha(Buffer.from(JSON.stringify({
       camera:comparison.camera,render_preset:comparison.render_preset,player_frustum:comparison.player_frustum}))),
     before_png_sha256:sha(b),after_png_sha256:sha(a),
     before_blend_sha256:sha(bSource),after_blend_sha256:sha(aSource),
     original_blender_float_gate:comparison.camera_visibility?.camera_visibility_gate,
     original_blender_float_changed_pixels:comparison.camera_visibility?.changed_pixels,
     proof_level:'BLENDER_CLAY_ONLY_UNITY_NOT_TESTED',recheck_pngs_required:true,
     independent_art_review_required:true}};
 const candidate=appendPending(db,event);
 fs.writeFileSync(out,JSON.stringify(event,null,2)+'\n',{flag:'wx'});
 return {event,candidate,measurement:m};
}
function args(input){const obj={};for(let i=0;i<input.length;i+=2){
 need(input[i]?.startsWith('--')&&input[i+1]&&!input[i+1].startsWith('--'),'Key/value arguments required');
 obj[input[i].slice(2)]=input[i+1];}return obj}
if(process.argv[1]&&path.resolve(process.argv[1])===fileURLToPath(import.meta.url)){
 try{const a=args(process.argv.slice(2));
 const result=stageBlender({dbPath:a.db,root:a.root,runId:Number(a['run-id']),gitSha:a['git-sha'],out:a.out});
 console.log('BASTION_ART_MEMORY_PENDING '+JSON.stringify({run:result.event.source.run_id,
   changed:result.measurement.changed_pixels,total:result.measurement.total_pixels,
   verdict:'ART_UNREVIEWED',visible:result.event.visibility_gate}));
 }catch(e){console.error('BASTION_ART_MEMORY_STAGE_FAIL '+e.message);process.exitCode=1}
}
