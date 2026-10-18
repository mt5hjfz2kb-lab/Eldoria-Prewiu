#!/usr/bin/env node
// Professional gate: historical orthographic XYZ cannot masquerade as the player SHARP camera.
// Evaluates actual source and evidence, never automatically approves visuals.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
const load = p => fs.readFileSync(path.resolve(process.cwd(),p),'utf8');
const sha = p => crypto.createHash('sha256').update(fs.readFileSync(path.resolve(process.cwd(),p))).digest('hex');
const check = (value,message) => {if(!value)throw Error('VALORIA_CAMERA_AUTHORITY_FAIL '+message);};
const layout=JSON.parse(load('Unity/Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/Layout/certified-legacy-skeleton.json'));
const camera=load('Unity/Assets/Eldoria/Scripts/Presentation/ValoriaParcelPresentation.cs');
const host=load('Unity/Assets/Eldoria/Scripts/Presentation/SlicePresenter.cs');
const qa=load('Unity/Assets/Eldoria/Scripts/Presentation/VisualWorld.cs');
const native=JSON.parse(load('docs/evidence/valoria-layered-construction-20261016/terrain-v1/terrain-v1-unity.json'));
const certified=JSON.parse(load('docs/evidence/valoria-visual-authority-reset-v1/review.json'));
const resetBlockout=JSON.parse(load('docs/evidence/valoria-art-production-reset-v1/target-spec.json'));
const approvalSha='8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689';
check(sha('references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg')===approvalSha,'approved art reference changed');
check(layout.classification==='LEGACY_QA_CERTIFIED_TOPOLOGY_NOT_CANONICAL_SHARP_GEOMETRY','historical topology mislabeled');
check(layout.canonical_sharp_camera.projection==='perspective','SHARP perspective authority changed');
check(layout.expected_footprint_count===22 && layout.stair.count===12,'historical master topological count changed');
check(camera.includes('public float HomeFov = 44.42281f'),'SHARP FOV no longer matches known source');
check(camera.includes('public const float PresentationHomeScale=1.00f'),'SHARP presentation scale no longer matches source');
check(camera.includes('ProductionCamera.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);'),'player camera Home() authority changed');
check(camera.includes('ProductionCamera.fieldOfView=PresentationHomeFov;'),'player camera FOV reset changed');
check(camera.includes('public void Pan(Vector2 delta)'),'player pan authority unavailable');
check(host.includes('GetComponentInChildren<ValoriaParcelPresentation>'),'SlicePresenter player runtime source changed');
check(host.includes('productionParcels.Home()'),'player Home invocation changed');
check(qa.includes('camera.orthographic=true') && qa.includes('new Vector3(18.2f,14.6f,-25.8f)'),'historical orthographic position changed');
check(certified.reference_authority_sha256===approvalSha && certified.criteria.COMPOSITION==='PASS' && certified.interaction.raycasts_pass===45,'previous certified SHARP reference no longer matches');
check(resetBlockout.reference.sha256===approvalSha &&
      resetBlockout.camera.projection==='orthographic' &&
      resetBlockout.camera.orthographic_size===24 &&
      resetBlockout.camera.yaw===20 && resetBlockout.camera.pitch===35,
      'Approved third art blockout camera source changed');
check(Object.keys(resetBlockout.target_landmarks).length>=8,'Art target original semantic anchors missing');
check(native.camera_authority==='LEGACY_QA_NOT_SHARP' && native.gameplay_status==='NOT_CERTIFIED','native terrain screenshot falsely claims player camera');
check(native.art_status==='REVIEW_REQUIRED' && native.bridge_rays===5 && native.stair_rays===12,'native terrain evidence changed');
const output={
  status:'CAMERA_AUTHORITIES_IDENTIFIED_MAPPING_STILL_UNVERIFIED',
  approved_reference_sha256:approvalSha,
  historical_orthographic:{
    source:'VisualWorld.cs / ValoriaMobileNavigableCityV1.cs',
    position:layout.approved_qa_camera.position,
    target:layout.approved_qa_camera.target,
    zoom:layout.approved_qa_camera.zoom_sizes,
    projection:'orthographic'
  },
  actual_player:{
    source:'ValoriaParcelPresentation.cs / SlicePresenter.cs',
    appearance:'SHARP',
    projection:'perspective',
    home_camera_position:[0,0,0],
    home_fov_degrees:44.42281,
    certified_previous_run:37474695980,
    certified_previous_raycasts:45
  },
  candidate:{
    native_unity_run:38979099838,
    scene:native.scene,
    technical_route:'PASS_HISTORICAL',
    full_scene_art:'REJECTED',
    original_gameplay_integration:'NOT_TESTED'
  },
  stage_gate:{stage_1:'SOURCE_PASS',stage_2:'NOT_PASSED_NO_3D_TO_SHARP_MAPPING',stage_3:'TECH_PASS_ART_FAIL',
              stage_4:'GREYBOX_ONLY',stage_5:'LOCAL_BRIDGE_GATE_CANDIDATE',stage_6:'HOLD_UPPER_FORTIFICATION',
              stage_8:'NOT_TESTED',stage_9:'NOT_TESTED'},
  no_promote:true
};

const anchorEvidence=JSON.parse(load('docs/evidence/valoria-visual-authority-reset-v1/production-slice-evidence.json'));
check(anchorEvidence.home_anchors.length===9,'Player semantic source anchor count changed');
check(anchorEvidence.camera.fov===44.42281,'Source screen-space reference FOV changed');
check(anchorEvidence.camera.width===1230&&anchorEvidence.camera.height===845,'Source crop dimensions changed');
check(anchorEvidence.home_anchors.every(a=>Number.isFinite(a.x)&&Number.isFinite(a.y) &&
  a.x>=0&&a.x<=1&&a.y>=0&&a.y<=1),'Player source anchors invalid');
output.approved_reset_art_camera={
   source:'docs/evidence/valoria-art-production-reset-v1/target-spec.json',
   projection:'orthographic',yaw_degrees:resetBlockout.camera.yaw,
   pitch_degrees:resetBlockout.camera.pitch,
   orthographic_size:resetBlockout.camera.orthographic_size,
   vertical_span:resetBlockout.camera.vertical_span,
   reference_resolution:resetBlockout.reference.dimensions,
   target_landmarks:Object.entries(resetBlockout.target_landmarks).map(([name,data])=>({name,anchor:data.anchor,bbox:data.bbox})),
   equivalent_to_historical_qa:false,equivalent_to_player_SHARP:false
 };
output.actual_player.semantic_anchors=anchorEvidence.home_anchors;
output.anchor_comparison={
  method:'two independent coordinate spaces only; NO fitted 3D-to-SHARP transform',
  canonical_screenspace:'SHARP source 1230x845 intrinsic home, normalized bottom-left',
  historic_worldspace:'VisualWorld XZ metres; geometry origin / orientation DIFFERENT',
  equivalent_world_transform_verified:false,
  screen_anchors_count:anchorEvidence.home_anchors.length,
  world_footprints_count:layout.footprints.length,
  mandatory_next_gate:'Measured anchors from identical meshes in actual ValoriaParcelPresentation camera. Never substitute historic QA screenshots.'
};
// A two-panel map makes the missing relationship inspectable without faking a world transform.
const escaped=s=>String(s).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;');
const svg=['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1560 850" width="1560" height="850">',
'<rect width="1560" height="850" fill="#f8f8f4"/>',
'<text x="40" y="43" font-size="27" fill="#222">Valoria | dos espacios de coordenadas NO equivalentes</text>',
'<text x="40" y="70" font-size="15" fill="#743c2b">DIAGNÓSTICO, NO APROBACIÓN DE CÁMARA: no se ha demostrado el traslado de estas mallas al runtime SHARP</text>',
'<text x="48" y="107" font-size="18" fill="#27373a">IZQUIERDA: planta XYZ de VisualWorld, ortográfica histórica</text>',
'<text x="807" y="107" font-size="18" fill="#27373a">DERECHA: anclas de pantalla del SHARP aprobado</text>',
'<rect x="45" y="127" width="680" height="655" fill="#eaf0e8" stroke="#73848a" stroke-width="2"/>',
'<rect x="807" y="127" width="708" height="655" fill="#e7eef4" stroke="#73848a" stroke-width="2"/>'];
// Historical world extents fixed by recovered layout and available expansion.
const leftX=x=>Math.round(85+(x+32)*8.9);
const leftZ=z=>Math.round(750-(z+29)*9.7);
const ordered=[...layout.footprints].sort((a,b)=>{
 const pri=k=>k.startsWith('terrain')?0:k.startsWith('reserve')?1:k.startsWith('route')?3:k.startsWith('plot')?4:2;
 return pri(a.kind)-pri(b.kind);
});
for(const f of ordered){
 const [x,,z]=f.center,[w,,d]=f.size;
 if(f.kind==='gate_hotspot_volume')continue;
 const tint=f.kind.startsWith('route')?'#89999e':f.kind.startsWith('plot')?'#c7ad88':
  f.kind.startsWith('reserve')?'#b5ccb5':f.kind.startsWith('floor')?'#b4b9a5':'#91a599';
 svg.push('<rect x="'+leftX(x-w/2)+'" y="'+leftZ(z+d/2)+'" width="'+(w*8.9)+'" height="'+(d*9.7)+
  '" fill="'+tint+'" fill-opacity=".47" stroke="#62716e"><title>'+escaped(f.id)+'</title></rect>');
}
for(const [id,short] of [
  ['VPD · lower entry apron','Gate/entry'],['VPD · L0 main street','Road'],['VPD · L1 landing','Upper landing'],
  ['VPD · L0 west plot','West plot'],['VPD · L0 east plot','East plot'],
  ['VPD · master west district','West grow'],['VPD · master east district','East grow']]){
 const f=layout.footprints.find(f=>f.id===id);
 check(Boolean(f),'QA source landmark missing '+id);
 svg.push('<circle cx="'+leftX(f.center[0])+'" cy="'+leftZ(f.center[2])+'" r="5.5" fill="#182e38"/>');
 svg.push('<text x="'+(leftX(f.center[0])+9)+'" y="'+(leftZ(f.center[2])-5)+
   '" font-size="14" fill="#182e38">'+escaped(short)+'</text>');
}
svg.push('<text x="70" y="804" font-size="14" fill="#71362b">Son metros XYZ; no equivalen a píxeles SHARP.</text>');
// SHARP coordinates are independent normalized screen evidence, not projections of left-side XYZ.
for(let i=0;i<anchorEvidence.home_anchors.length;i++){
 const a=anchorEvidence.home_anchors[i];
 const x=Math.round(807+a.x*708),y=Math.round(127+(1-a.y)*655);
 const labelX=Math.min(1450,x+8),labelY=Math.max(143,Math.min(775,y+(i%2===0?-6:15)));
 svg.push('<circle cx="'+x+'" cy="'+y+'" r="6.8" fill="#174563" stroke="#fff" stroke-width="1.5"/>');
 svg.push('<text x="'+labelX+'" y="'+labelY+'" font-size="14" fill="#174563" font-weight="bold">'+escaped(a.id)+'</text>');
}
svg.push('<text x="805" y="804" font-size="14" fill="#71362b">Anclas reales de pantalla 1230×845, cámara en perspectiva FOV 44.42281.</text>');
svg.push('<text x="44" y="835" font-size="15" fill="#7b3220">NO calcular homografía o distorsionar las mallas sin correspondencias 3D medidas.</text>');
svg.push('</svg>');
const comparisonPath='docs/evidence/valoria-layered-construction-20261016/camera-coordinate-authority-comparison.svg';
fs.writeFileSync(comparisonPath,svg.join('\n')+'\n');

const dest='docs/evidence/valoria-layered-construction-20261016/player-camera-authority-gate.json';
fs.writeFileSync(dest,JSON.stringify(output,null,2)+'\n');
console.log('VALORIA_CAMERA_AUTHORITY_SOURCE_PASS: canonical SHARP perspective vs QA orthographic distinguished');
console.log('VALORIA_NO_REAL_CAMERA_MATCH_PASS: 3D-to-SHARP geometry is unverified; terrain art rejected, further upper art blocked');
