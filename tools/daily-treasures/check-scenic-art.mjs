import fs from 'node:fs';
import assert from 'node:assert/strict';

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
console.log('PASS three authentic reused painterly milestone scenes, original metadata, UV crop and non-blocking overlay');
