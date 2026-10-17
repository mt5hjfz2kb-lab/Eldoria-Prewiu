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
const dest='docs/evidence/valoria-layered-construction-20261016/player-camera-authority-gate.json';
fs.writeFileSync(dest,JSON.stringify(output,null,2)+'\n');
console.log('VALORIA_CAMERA_AUTHORITY_SOURCE_PASS: canonical SHARP perspective vs QA orthographic distinguished');
console.log('VALORIA_NO_REAL_CAMERA_MATCH_PASS: 3D-to-SHARP geometry is unverified; terrain art rejected, further upper art blocked');
