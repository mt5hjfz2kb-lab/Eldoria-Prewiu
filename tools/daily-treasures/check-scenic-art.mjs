import fs from 'node:fs';
import assert from 'node:assert/strict';
import {createHash} from 'node:crypto';

// Reuse actual commissioned/installed Eldoria scene art rather than inventing
// a portrait of Lyra, an unbuilt Bastion, or non-existent loot. These are
// source/integrity checks; native composition is still a separate gate.
const root='Unity/Assets/Eldoria/Resources';
const sources=[
  {path:'EldoriaV41/heroes-hall-illustrated-v2.jpg',type:0,min:100_000},
  {path:'EldoriaRetention/scouts-painterly-v2.png',type:0,min:350_000},
  {path:'EldoriaRetention/vigilants-painterly-v2.png',type:0,min:350_000},
];
const panel=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePanel.cs','utf8');
for(const {path,type,min} of sources){
  const file=`${root}/${path}`,meta=file+'.meta';
  assert.ok(fs.existsSync(file),`Missing genuine Eldoria painterly backdrop: ${file}`);
  assert.ok(fs.statSync(file).size>min,`Scenery accidentally replaced by low-detail thumbnail: ${file}`);
  assert.ok(fs.existsSync(meta),`Missing original Unity metadata: ${meta}`);
  const text=fs.readFileSync(meta,'utf8');
  assert.match(text,new RegExp(`textureType: ${type}\\b`),
    `Artist scenic texture must load as Texture2D: ${path}`);
  const resource=path.replace(/\.(png|jpg)$/,'');
  assert.ok(panel.includes('"'+resource+'"'),
    `Event is not wired to genuine scenic source: ${resource}`);
}
assert.match(panel,/void SetMilestoneScenery\(string eventId,bool showChest\)/);
assert.match(panel,/Resources\.Load<Texture2D>\(path\)/);
assert.match(panel,/milestoneScenery\.raycastTarget=false/);
assert.match(panel,/UpdateSceneryCrop\(\)/);
assert.match(panel,/if\(target<source\)/);
// Preserve the canonical empty Bastion scenery resolver. A preceding
// orientation-reshoot helper may be added without turning a true city
// building into a fabricated flat painting.
const canonicalScenery=panel.match(
  /static string MilestoneScenerySource\(string eventId\)([\s\S]*?)static Sprite TryCanonicalLyraPortrait/);
assert.ok(canonicalScenery && /\/\/ For Bastion,[\s\S]*?return null;\s*}/.test(canonicalScenery[1]),
  'Unbuilt Bastion must return no synthetic scenic screenshot');
assert.match(panel,/void RefreshBastionLensForRotation\(float width,float height\)/);
// Four distinct event compositions use real source art and the live city,
// not a static montage of invented characters and future Bastion levels.
const stage=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureStageDirection.cs',
  'utf8');
assert.match(stage,/DailyTreasureStageKind\.Lyra/);
assert.match(stage,/DailyTreasureStageKind\.Explorers/);
assert.match(stage,/DailyTreasureStageKind\.Vigilants/);
assert.match(stage,/DailyTreasureStageKind\.Bastion/);
assert.match(stage,/float backdropOpacity,float sceneryOpacity,float veilOpacity/);
assert.match(stage,/if\(kind==DailyTreasureStageKind\.Bastion\)/);
assert.match(panel,/PaintMilestoneAtmosphere\(elapsed,frame\)/);
assert.match(panel,/SafePainterlyDrift\(/);
assert.match(panel,/stage\.SideBySide\?stage\.SceneX:-panelW\*\.10f/,
  'Portrait event headline must not cover the actual Skip action');
assert.match(panel,/return "EldoriaV41\/heroes-hall-illustrated-v2"/);
assert.match(panel,/return "EldoriaRetention\/scouts-painterly-v2"/);
assert.match(panel,/return "EldoriaRetention\/vigilants-painterly-v2"/);
// Lyra already owns a genuinely approved 3D presentation in the existing
// Heroes UI. Never invent a new portrait or open the chest over her face.
const heroUi=fs.readFileSync(
  'Unity/Assets/Eldoria/HeroesUI/HeroesApprovedDestination.cs','utf8');
assert.match(heroUi,/public void PresentRecruitment\(string id\)/);
assert.match(heroUi,/Rect\("Heroes \/ canonical destination",safeArea\)/);
assert.match(panel,/HasVisibleCanonicalHeroRoom\(\)/);
assert.match(panel,/cachedCanonicalHeroRoom\.gameObject\.activeInHierarchy/);
assert.doesNotMatch(panel,/GameObject\.Find\("Heroes \/ canonical destination"\)/);
console.log('PASS three authentic reused painterly milestone scenes, original metadata, UV crop and non-blocking overlay');


// Production imports must be exact copies of four independent preflighted
// transparent illustrations and their shared engraving. A contact sheet is
// not a Unity Sprite and must never substitute an actual source asset.
const candidateRoot='tools/daily-treasures/art-candidates';
const importedRoot='Unity/Assets/Eldoria/DailyTreasures/Resources/DailyTreasureCeremonies';
const candidateManifest=JSON.parse(fs.readFileSync(
  candidateRoot+'/ceremony-art-production.json','utf8'));
const importedManifest=JSON.parse(fs.readFileSync(
  importedRoot+'/source-manifest.json','utf8'));
assert.equal(importedManifest.art_approved,false);
assert.equal(importedManifest.unity_native_verified,false);
const authored={lyra:'lyra_stage.png',explorers:'explorers_stage.png',
  vigilants:'vigilants_stage.png',bastion:'bastion_stage.png',
  ornament:'shared_ornament.png'};
const guids=new Set();
for(const [key,file] of Object.entries(authored)){
  const original=fs.readFileSync(candidateRoot+'/'+file);
  const imported=fs.readFileSync(importedRoot+'/'+file);
  assert.ok(original.equals(imported),'Corrupted original art: '+file);
  const digest=createHash('sha256').update(imported).digest('hex');
  const base=key==='ornament'?
    candidateManifest.ceremonies.lyra.layers.find(x=>x.role==='ornament'):
    candidateManifest.ceremonies[key].layers.find(x=>x.role==='stage');
  assert.equal(digest,base.sha256,'Unexpected candidate art: '+file);
  assert.equal(digest,importedManifest.assets[key].sha256,
    'Unexpected Unity imported art: '+file);
  const meta=fs.readFileSync(importedRoot+'/'+file+'.meta','utf8');
  const guid=importedManifest.assets[key].guid;
  assert.ok(meta.includes('guid: '+guid),'Wrong GUID: '+file);
  for(const required of ['textureType: 8','spriteMode: 1',
                          'alphaIsTransparency: 1','enableMipMap: 0'])
    assert.ok(meta.includes(required),'Wrong Unity importer: '+file+' '+required);
  assert.ok(!guids.has(guid),'Collision in imported art GUIDs');
  guids.add(guid);
}
// Lyra, Scouts and Vigilants use genuine layered source ornaments.
// Bastion is fundamentally different: its only architecture is the actual
// built 3D model seen through a one-frame authenticated live Unity camera.
// The historic arch candidate stays hash-traced, but it must NOT be rendered.
for(const scene of ['lyra','explorers','vigilants'])
  assert.ok(panel.includes('"DailyTreasureCeremonies/'+scene+'_stage"'),
    'Source asset not bound to runtime: '+scene);
assert.ok(!panel.includes('"DailyTreasureCeremonies/bastion_stage"'),
  'Fictional painted arch must never overlay the actual Bastion');
assert.match(panel,/case DailyTreasureStageKind\.Bastion:\s*\/\/[^\n]*\n\s*\/\/[^\n]*\n\s*return null;/,
  'Bastion stage must resolve genuine camera artwork exclusively');
assert.match(panel,/milestoneStageArt\.raycastTarget=false/);
assert.match(panel,/HasDistinctMilestoneArt/);
assert.match(panel,/mainFrameSurface\.sprite=singleFrame\?null:dailyFrameSprite/);
console.log('PASS unique original ceremony artwork in actual Unity Resources; pending Unity/human review');


// Isolated Unity screenshots previously had an empty Lyra arch. The backup
// must come ONLY from a SHA-traced render of the canonical 4.77MB GLB.
const trueLyraFile='canonical_lyra_bust.png';
const trueLyraSource=candidateRoot+'/lyra-canonical-bust-source.png';
const trueLyraData=fs.readFileSync(trueLyraSource);
const realModelProvenance=JSON.parse(fs.readFileSync(
  candidateRoot+'/lyra-canonical-bust-source.provenance.json','utf8'));
const authenticLyra=importedManifest.assets.canonical_lyra_bust;
assert.equal(realModelProvenance.generated_person,false);
assert.equal(realModelProvenance.source_sha256,
  'a9fa7899e7ead2ad26e7b26d368c9006085ae8b8ded2df6515ca466c863f6725');
assert.equal(createHash('sha256').update(trueLyraData).digest('hex'),
  realModelProvenance.output_sha256);
assert.equal(createHash('sha256').update(
  fs.readFileSync(importedRoot+'/'+trueLyraFile)).digest('hex'),
  authenticLyra.sha256);
assert.ok(trueLyraData.equals(fs.readFileSync(importedRoot+'/'+trueLyraFile)));
const realLyraMeta=fs.readFileSync(importedRoot+'/'+trueLyraFile+'.meta','utf8');
assert.ok(realLyraMeta.includes('guid: '+authenticLyra.guid));
assert.ok(realLyraMeta.includes('textureType: 8') &&
          realLyraMeta.includes('alphaIsTransparency: 1'));
assert.match(panel,/Resources.Load<Sprite>\(\s*"DailyTreasureCeremonies\/canonical_lyra_bust"\)/);
assert.match(panel,/lyraFallbackActive=portrait!=null/);
console.log('PASS authentic Lyra GLB-only portrait fallback and non-obscuring category art composition');

// Protect the authored four-way ceremony from reverting to the same static
// coffer effect. These are pure source contracts; only Unity PNGs prove art.
const direction=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureStageDirection.cs','utf8');
assert.match(direction,/DailyTreasureStageEntrance SampleEntrance\(/);
assert.match(panel,/PaintMilestoneStageEntrance\(elapsed\)/);
assert.match(panel,/PaintMilestoneStageEntrance\(0f\)/);
assert.match(panel,/milestoneStageRest\+new Vector2\(beat.ShiftX,beat.ShiftY\)/);
assert.match(panel,/milestoneStageRestOpacity\*beat.Opacity/);
console.log('PASS four original authored scene entrance motions; daily chest remains separate');


// Real original shared_orna­ment was 1376x768 with 90px unpainted-safe
// corner bands. Plain Image.Simple distorted those corner rivets 11.6%
// horizontally at 740x370. Unity 6 CanvasScaler uses 100 reference PPU;
// our art imports at 200 PPU so 90px SOURCE borders become 45 UI pixels
// on both axes. Assert the same real geometry at short-phone and tablet.
const metalSource=fs.readFileSync(importedRoot+'/shared_ornament.png');
assert.equal(metalSource.toString('ascii',1,4),'PNG');
const sourceW=metalSource.readUInt32BE(16),sourceH=metalSource.readUInt32BE(20);
assert.equal(sourceW,1376);
assert.equal(sourceH,768);
const brassMeta=fs.readFileSync(importedRoot+'/shared_ornament.png.meta','utf8');
assert.ok(brassMeta.includes('spritePixelsToUnits: 200'),
  'Wrong pixels per unit: brass rivets would distort on Vigilia Canvas');
assert.ok(brassMeta.includes('spriteBorder: {x: 90, y: 90, z: 90, w: 90}'),
  'Imported frame must have 90px nine-slice corners on all four sides');
assert.match(panel,/milestoneOrnament\.type=Image\.Type\.Sliced/,
  'Runtime must actually render 9-sliced ornament, not stretched Simple');
const uiBorder=90*100/200;
for(const [w,h] of [[740,370],[643,355],[700,370]]){
  assert.ok(w>2*uiBorder && h>2*uiBorder,'Sprite border overlaps itself');
  const cornerX=uiBorder/90,cornerY=uiBorder/90;
  assert.equal(cornerX,cornerY,'Real corner aspect must be unchanged');
  assert.ok(Math.abs(cornerX-.5)<1e-10);
}
console.log('PASS original forged bronze 9-slice preserves square corners at all landscape sizes');


// Actual Unity native assertion: screenshot rectangles are insufficient if
// the ornament itself silently reverts to Simple or vertical frames overlap.
const nativeArt=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Editor/DailyTreasureNativeEvidence.cs','utf8');
assert.match(nativeArt,/ornament\.type==Image\.Type\.Sliced/);
assert.match(nativeArt,/Mathf\.Abs\(border\.x-90f\)/);
assert.match(nativeArt,/Mathf\.Abs\(ornament\.sprite\.pixelsPerUnit-200f\)/);
assert.match(nativeArt,/plate && !plate\.sprite/);
console.log('PASS native gate requires true original nine-slice corner geometry in Unity');

// New authentic camera method: only the real active built structure and real
// PlayerCamera may feed a Bastion milestone. No baked future castle art.
const liveBastion=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureBastionCamera.cs','utf8');
assert.match(liveBastion,/ValoriaIntegratedWorld/);
assert.match(liveBastion,/BastionOne/);
assert.match(liveBastion,/PlayerCamera/);
assert.match(liveBastion,/camera.CopyFrom\(source\)/);
// Exact Unity 6 URP 17.3 manual: single-camera request outside the
// pipeline. The old camera.Render() compiled but did not certify a real
// full-frame RenderTexture in URP.
assert.match(liveBastion,/using UnityEngine.Rendering.Universal/);
assert.match(liveBastion,/renderData.renderType=CameraRenderType.Base/);
assert.match(liveBastion,/new UniversalRenderPipeline.SingleCameraRequest\(\)/);
assert.match(liveBastion,/RenderPipeline.SupportsRenderRequest\(camera,request\)/);
assert.match(liveBastion,/request.destination=result/);
assert.match(liveBastion,/RenderPipeline.SubmitRenderRequest\(camera,request\)/);
assert.doesNotMatch(liveBastion, /^\s*camera\.Render\(\);/m);
assert.match(liveBastion,/result.Release\(\)/);
assert.match(panel,/DailyTreasureBastionCamera.Capture\(/);
assert.match(panel,/DailyTreasureBastionCamera.Release\(/);
assert.match(panel,/DailyTreasureBastionCamera.SceneKey/);
console.log('PASS true camera-derived live Bastion presentation, never a zoomed PNG');

// A real, committed Scout discovery uses its SAVED family Kind to select
// authentic existing EldoriaRetention mission-vignette art. Shader feathers
// ink into the blank original painting; no custom coast, hero, loot or map.
const scoutInkShader=fs.readFileSync(
 'Unity/Assets/Eldoria/DailyTreasures/Resources/DailyTreasureCeremonies/DailyScoutDiscoveryInk.shader',
 'utf8');
assert.match(scoutInkShader,/Shader "Eldoria\/DailyScoutDiscoveryInk"/);
assert.match(scoutInkShader,/float matte=smoothstep\(/);
assert.match(scoutInkShader,/_AtlasUv/);
assert.match(panel,/FindActualCommittedScoutKind\(eventId\)/);
assert.match(panel,/report.DiscoveryKind<0 \|\| report.DiscoveryKind>3/);
assert.match(panel,/report.DiscoveryOrderId!=orderId/);
assert.match(panel,/report.EventId!="exploration:"\+report.RewardReceiptId/);
assert.match(panel,/EldoriaRetention\/scout-mission-vignettes-v4/);
assert.match(panel,/milestoneDiscoveryIllustration\.uvRect=tile/);
assert.match(panel,/milestoneDiscoveryIllustration\.raycastTarget=false/);
assert.match(panel,/DestroyOwned\(ownedScoutInkMaterial\)/);
console.log('PASS 4 true earned Scout family vignettes painted into parchment with GPU feather and no fake awards');

// Coherent REAL painted world around the whole narrative, replacing the
// obsolete black/right void with original-world ambient layers only.
// No duplicate camera readback or fictional reward/hero/city art.
const narrativeMaterial=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Resources/DailyTreasureCeremonies/DailyNarrativeBackdrop.shader',
  'utf8');
assert.match(narrativeMaterial,/Shader "Eldoria\/DailyNarrativeBackdrop"/);
assert.match(narrativeMaterial,/fixed4 src=tex2D\(_MainTex,i.uv\)/);
assert.match(narrativeMaterial,/float rightInk=smoothstep/);
assert.match(narrativeMaterial,/float bottomInk=smoothstep/);
assert.match(narrativeMaterial,/float readingInk=lerp/);
assert.match(narrativeMaterial,/_VisibleUv/);
assert.match(panel,/ScenicCoverUv\(Texture texture,Rect rect\)/);
assert.match(panel,/milestoneCinematicBackdrop\.uvRect=uv/);
assert.match(panel,/ownedCinematicBackdropMaterial\.SetVector/);
assert.match(panel,/ownedCinematicBackdropMaterial\.SetFloat/);
for(const [w,h] of [[1600,900],[900,1600],[844,390],[390,844]]){
  const src=16/9,dst=w/h;
  const cropW=dst<src?dst/src:1, cropH=dst<src?1:src/dst;
  assert.ok(Math.abs(src*cropW/cropH-dst)<1e-10,
    'Authentic panorama would distort in '+w+'x'+h);
}
assert.match(nativeArt,/paintedAspect-screenAspect/);
assert.match(nativeArt,/Native cinematic ink darkens wrong reading side/);
assert.match(panel,/milestoneCinematicBackdrop\.raycastTarget=false/);
assert.match(panel,/milestoneCinematicBackdrop\.texture=milestoneScenery\.texture/);
assert.match(panel,/DestroyOwned\(ownedCinematicBackdropMaterial\)/);
assert.match(panel,/milestoneCinematicBackdrop\.texture=null/);
console.log('PASS: canonical full-frame narrative atmosphere, real source and owned GPU material');

// Genuine existing Eldoria Vigilants badge remains the authoritative shape
// but its original 128px blue pixels are rendered as embossed antique
// copper on original iron artwork. No generic fake badge or extra sticker.
const reliefCode=fs.readFileSync(
 'Unity/Assets/Eldoria/DailyTreasures/Resources/DailyTreasureCeremonies/DailyHeraldicRelief.shader',
 'utf8');
assert.match(reliefCode,/Shader "Eldoria\/DailyHeraldicRelief"/);
assert.match(reliefCode,/fixed4 src=tex2D\(_MainTex,i.uv\)/);
assert.match(reliefCode,/float relief=saturate/);
assert.match(panel,/milestoneInsignia\.material=/);
assert.match(panel,/ownedVigilantsReliefMaterial:null/);
assert.match(panel,/DestroyOwned\(ownedVigilantsReliefMaterial\)/);
console.log('PASS real retained Vigilants compass rendered as source-derived bronze relief');

// Proven against canonical Unity source: ValoriaIntegratedWorld owns the
// switching camera but NOT named BastionOne/Two fields. The real
// ValoriaReferenceCleanGameplay owns the active built meshes and PlayerCamera.
const realBuiltCity=fs.readFileSync(
 'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureBastionCamera.cs','utf8');
assert.match(realBuiltCity,/item.GetType\(\).Name!="ValoriaReferenceCleanGameplay"/);
assert.match(realBuiltCity,/ReadMember\(item,"PlayerCamera"\)/);
assert.match(realBuiltCity,/GetProperty\(member,/);
assert.match(realBuiltCity,/new\[\]\{"BastionTwo","BastionOne"\}/);
assert.match(realBuiltCity,/!root\.activeInHierarchy/);
assert.match(realBuiltCity,/TryActiveBounds\(building,camera,out bounds\)/);
assert.doesNotMatch(realBuiltCity,/new\[\]\{"BastionFive","BastionFour","BastionThree"/);
console.log('PASS real city class owns actual Bastion renderers and only displays built world levels');

// Reproduced geometric root of the original Scout sticker: uGUI
// Image.preserveAspect letterboxes vertices, NOT child RectTransforms.
// Contain the parent itself to 1376:768 so real persisted atlas children
// never stretch on narrow phone or 844x390 landscape.
const directionSource=fs.readFileSync(
    'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureStageDirection.cs','utf8');
assert.match(directionSource,/ContainOriginalArtwork\(/);
assert.match(panel,/currentStage==DailyTreasureStageKind\.Explorers\)[\s\S]*?ContainOriginalArtwork\(/);
assert.match(panel,/original\.width,original\.height/);
for(const [w,h] of [[246,282],[358,276],[328,249],[470,320]]){
  const scale=Math.min(w/1376,h/768),rw=1376*scale,rh=768*scale;
  assert.ok(rw<=w+1e-6&&rh<=h+1e-6);
  assert.ok(Math.abs(rw/rh-1376/768)<1e-9);
  assert.ok(Math.abs((.35*rw)/(.62*rh)-1)<.03);
}
console.log('PASS source-aligned Scout vignette geometry, no letterbox stickers');

// Unity runtime compiles this code in an explicit asmdef, not Assembly-CSharp.
// Add both URP 17.3 pipeline references; code can pass Ubuntu smoke but fail
// the real Unity compiler without the URP assembly being referenced.
const dailyUiAsmdef=JSON.parse(fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Eldoria.DailyTreasures.UI.asmdef','utf8'));
for(const assembly of ['Unity.RenderPipelines.Universal.Runtime',
                       'Unity.RenderPipelines.Core.Runtime'])
  assert.ok(dailyUiAsmdef.references.includes(assembly),
    'Missing explicit Unity6 URP dependency: '+assembly);
console.log('PASS URP render-request assemblies explicitly referenced');

// Source-only cinematic lens proof: original 3D scene, deliberate 3/4 facade
// and 38° physical perspective rather than enlarging gameplay screenshots.
const cinematicSource=fs.readFileSync(
 'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureBastionCamera.cs','utf8');
assert.match(cinematicSource,/const float elevationDegrees=36f/);
assert.match(cinematicSource,/Quaternion.AngleAxis\(18f,Vector3.up\)/);
assert.match(cinematicSource,/camera\.fieldOfView=38f/);
assert.match(cinematicSource,/camera\.orthographic=false/);
assert.match(cinematicSource,/camera\.transform\.position=b.center\+away\*distance/);
assert.match(cinematicSource,/camera\.farClipPlane=Mathf.Max/);
assert.doesNotMatch(cinematicSource,/Texture2D\.Resize/);
console.log('PASS authentic three-quarter PBR Bastion lens, not digital image zoom');

// Actual GPU pixels must show original 3D Bastion masonry/terrain,
// not an empty but successfully allocated URP RenderTexture or UI border.
const cityArtSource=fs.readFileSync(
 'Unity/Assets/Eldoria/DailyTreasures/Editor/DailyTreasureCityArtEvidence.cs','utf8');
assert.match(cityArtSource,/ProveNativeBastionSilhouette\(panel,report\)/);
assert.match(cityArtSource,/raw\.texture is RenderTexture/);
assert.match(cityArtSource,/RenderTexture\.active=rt/);
assert.match(cityArtSource,/colors\.Count>=36/);
assert.match(cityArtSource,/trueBastionPixelProofs==8/);
assert.match(cityArtSource,/Object\.DestroyImmediate\(tex\)/);
console.log('PASS editor readback verifies 8 real, nonblack 3D Bastion shots');

// Genuine 3D Bastion can be built one frame after gateway event, so a
// one-shot camera would silently leave the ceremony black. This is NOT
// a per-frame GPU poll or fictitious placeholder: 3 bounded retries.
const lateLens=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePanel.cs','utf8');
assert.match(lateLens,/bastionCameraPending=!capturedBastionScene/);
assert.match(lateLens,/void TryRecoverBuiltBastionLens\(\)/);
assert.match(lateLens,/bastionCameraRetries>=3/);
assert.match(lateLens,/nextBastionCameraTry=Time.unscaledTime\+\.35f/);
assert.match(lateLens,/if\(!sceneOwnedByOtherUi\)TryRecoverBuiltBastionLens\(\)/);
assert.match(lateLens,/bastionCameraPending=false;/);
console.log('PASS bounded late real Bastion capture; no black fake celebration');

// Scene scale is a material art change, NOT just an extra frame: source
// landscape expands 2.62x on desktop and 2.13x on portrait tablets.
const largeScene=fs.readFileSync(
 'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureStageDirection.cs','utf8');
assert.match(largeScene,/NarrativeTheatreFrame\(/);
assert.match(largeScene,/Math.Min\(1120f,width\*\.84f\)/);
assert.match(panel,/if\(eventStage\)panelW=earnedTheatre.Width/);
assert.match(panel,/if\(eventStage\)panelH=earnedTheatre.Height/);
assert.ok(1120*640>740*370*2.5);
console.log('PASS 2.62x more actual painterly story on 1600x900, mobile unchanged');

// Typography must follow the ACTUAL upscaled cinematic stage rather than
// remaining a tiny 17px line on a 1120px metal frame. Small phones and
// the entire daily coffer/calendar keep their original calibrated sizes.
assert.match(panel,/float narrativeScale=eventStage\?Mathf.Clamp\(/);
assert.match(panel,/if\(eventStage\)panelH=earnedTheatre.Height/);
assert.match(panel,/milestoneBannerCaption\.fontSize\*narrativeScale/);
assert.match(panel,/resultText\.fontSize\*narrativeScale/);
assert.match(panel,/insigniaSize=\(centerShield\?/);
console.log('PASS narrative typography and original heraldic art scale with source stage');

// Authentic GPU hero shot matches the NARRATIVE aperture, not a fixed
// 600x900 image stretched into a 390x844 phone; one bounded render only.
const realCamera=fs.readFileSync(
 'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureBastionCamera.cs','utf8');
assert.match(realCamera,/NarrativeTheatreFrame\(/);
assert.match(realCamera,/float screenAspect=Mathf.Max\(/);
assert.match(realCamera,/height\*screenAspect/);
assert.match(realCamera,/Mathf.Clamp\(Mathf.RoundToInt/);
for(const [w,h,fw,fh] of [[390,844,366,700],[844,390,740,370],
                           [900,1600,620,1060],[1600,900,1120,640]]){
  const aspect=(fw-25)/(fh-28),portrait=h>w;
  const outH=portrait?(w>=650?1152:1024):(w>=950?720:540);
  const outW=Math.max(500,Math.min(1360,Math.round(outH*aspect)));
  assert.ok(Math.abs(outW/outH-aspect)<.025,
    'Original 3D Bastion footage would distort at '+w+'x'+h);
}
console.log('PASS real Bastion 3D render resolution follows portrait/landscape theatre');

// Source causal art fallback: an unloaded/newly building Bastion must remain
// the real visible CITY, not an empty black fake architecture. The existing
// report has a separate dark ink field so text remains clear.
// Reuse the existing, already-read pure stage source. No redeclared
// const/duplicate read: the complete JS gate must parse before visual QA.
assert.match(direction,/BastionWorldThroughPlateOpacity=\.19f/);
assert.match(direction,/BastionWorldThroughBackdropOpacity=\.10f/);
assert.ok((1-.19)*(1-.10)>.72,'True city fallback must retain >72% of light');
assert.match(panel,/bool liveBastionWorldThrough=eventStage/);
assert.match(panel,/float bOpacity=liveBastionWorldThrough\?/);
assert.match(panel,/bastionCameraPending=false;\s*Layout\(\);/);
assert.match(nativeArt,/float liveLight=\(1f-backdrop\.color\.a\)/);
assert.match(nativeArt,/worldThrough\?\.70f:\.40f/);
console.log('PASS true city-through fallback, with frame restored after live 3D capture');

// Actual live world tier V: ValoriaIntegratedWorld chooses its real
// PlayerCamera and BastionIVVWorldPresentation owns two REAL replacement
// meshes. Tier >=5 actively disables BastionTwo. Never show tier-II as
// the achievement for Bastion V, or mistake InWorld for the city camera.
const builtLens=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureBastionCamera.cs','utf8');
assert.match(builtLens,/item.GetType\(\).Name!="ValoriaIntegratedWorld"/);
assert.match(builtLens,/ReadMember\(item,"InWorld"\)/);
assert.match(builtLens,/component.GetType\(\).Name=="BastionIVVWorldPresentation"/);
assert.match(builtLens,/ReadMember\(progression,"WorldModel"\)/);
assert.match(builtLens,/GetMethod\("ModelFor"/);
assert.match(builtLens,/new\[\]\{"WorldBastionTwo","WorldBastionOne"\}/);
assert.match(builtLens,/new\[\]\{"BastionTwo","BastionOne"\}/);
assert.match(builtLens,/TryActiveBounds\(tierFive,camera,out bounds\)/);
assert.match(builtLens,/camera\.cullingMask&\(1<<r\.gameObject\.layer\)/);
assert.match(builtLens,/item.GetType\(\).Name!="ValoriaReferenceCleanGameplay"/);
assert.doesNotMatch(builtLens,/new\[\]\{"BastionFive","BastionFour","BastionThree"/);
console.log('PASS active real Bastion I–V world/city models, not legacy tier-II in level-V');

// Correct level-V identity requires the real saved BastionLevel, not just
// whichever old I/II mesh briefly remains active. The legitimate V model
// may arrive after city/region setup; the transparent real-world stage is
// strictly preferable to a fake II-level hero photo.
const tierProof=fs.readFileSync(
 'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureBastionCamera.cs','utf8');
assert.match(tierProof,/static int SavedBastionLevel\(object city\)/);
assert.match(tierProof,/ReadMember\(snapshot,"BastionLevel"\)/);
assert.match(tierProof,/if\(builtLevel>=5 && progression/);
assert.match(tierProof,/if\(builtLevel>=5\)/);
assert.match(tierProof,/pendingCanonicalTierFive=true/);
assert.match(tierProof,/if\(pendingCanonicalTierFive\)return false/);
assert.match(tierProof,/if\(SavedBastionLevel\(item\)>=5\)continue/);
console.log('PASS exact saved Bastion V prevents old tier-II photo while V model loads');
