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
assert.match(panel,/return null;\s*}\s*void SetMilestoneScenery/);
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
  assert.ok(meta.includes('guid: '+guid+'\\n'),'Wrong GUID: '+file);
  for(const required of ['textureType: 8','spriteMode: 1',
                          'alphaIsTransparency: 1','enableMipMap: 0'])
    assert.ok(meta.includes(required),'Wrong Unity importer: '+file+' '+required);
  assert.ok(!guids.has(guid),'Collision in imported art GUIDs');
  guids.add(guid);
}
for(const scene of ['lyra','explorers','vigilants','bastion'])
  assert.ok(panel.includes('"DailyTreasureCeremonies/'+scene+'_stage"'),
    'Source asset not bound to runtime: '+scene);
assert.match(panel,/milestoneStageArt\.raycastTarget=false/);
assert.match(panel,/HasDistinctMilestoneArt/);
assert.match(panel,/mainFrameSurface\.sprite=singleFrame\?null:dailyFrameSprite/);
console.log('PASS unique original ceremony artwork in actual Unity Resources; pending Unity/human review');
