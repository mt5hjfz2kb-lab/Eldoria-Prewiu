import fs from 'node:fs';
import assert from 'node:assert/strict';

// Run BEFORE a costly Windows native Unity session. The other active
// workstream PR173 owns its own source: this guard READS its exact head,
// without editing it, merging drafts, or consuming the exclusive PC.
const repo=process.env.GITHUB_REPOSITORY||'mt5hjfz2kb-lab/Eldoria-Prewiu';
const token=process.env.GITHUB_TOKEN;
assert.ok(token,'GITHUB_TOKEN must be available for the private repo check');
const root='https://api.github.com/repos/'+repo;
async function api(path) {
  const response=await fetch(root+path,{
    headers:{
      Authorization:'Bearer '+token,
      Accept:'application/vnd.github+json',
      'X-GitHub-Api-Version':'2022-11-28',
      'User-Agent':'eldoria-treasures-compat-preflight'
    },
    signal:AbortSignal.timeout(15000)
  });
  if(!response.ok)throw new Error('GitHub source preflight HTTP '+response.status+
    ' for '+path.split('?')[0]);
  return response.json();
}
const pr=await api('/pulls/173');
assert.equal(pr.number,173);
assert.equal(pr.head.repo.full_name,repo,'Different source repo requires explicit review');
const ref=pr.head.sha;
assert.match(ref,/^[a-f0-9]{40}$/);
const path='Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs';
const file=await api('/contents/'+path+'?ref='+ref);
assert.equal(file.encoding,'base64');
const native=Buffer.from(file.content.replace(/\s/g,''),'base64').toString('utf8');
const treasury=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePanel.cs','utf8');
const royal=fs.readFileSync(
  'Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV3EarlyRetentionScreens.cs','utf8');

const sourceRequirements=[
  [/const string RootName\s*=\s*"VIGILIA \/ independent canvas"/,'Vigilia standalone Canvas name'],
  [/uiRoot\s*=\s*Rect\("Safe composition",transform\)/,'native safe area name'],
  [/canvas\.renderMode\s*=\s*RenderMode\.ScreenSpaceCamera/,'native UI render mode'],
  [/canvas\.sortingOrder\s*=\s*5000/,'native HUD sorting plane'],
  [/legacyCanvas\.enabled\s*=\s*false/,'legacy canvas is disabled but game object survives'],
  [/legacyGroup\.alpha\s*=\s*0/,'legacy UI is hidden'],
  [/host\.Hud\.SelectionChanged\s*\+=/,'canonical session owner is reused'],
  // PR173 head ba71afea now binds its visible profile/resource/upgrade
  // directly from CleanRoom. FreshArt remains live for wood/stone/food only.
  // Never keep a stale FreshArt surface assertion when the source actually
  // loads a different named painted original.
  [/Resources\.Load<Sprite>\("VigiliaArt\/CleanRoom\/profile"\)/,
    'Current Vigilia player identity uses approved single-piece CleanRoom profile'],
  [/Resources\.Load<Sprite>\(\s*"VigiliaArt\/CleanRoom\/resource"\)/,
    'Current Vigilia resource row uses original CleanRoom capsule'],
  [/Resources\.Load<Sprite>\("VigiliaArt\/CleanRoom\/upgrade"\)/,
    'Current upgrade CTA uses original CleanRoom art (never as a loyalty reward)'],
  [/actionPlate\.ApplyIndependentArt\(originalArt\)/,
    'One painted upgrade CTA rather than a stacked border'],
  [/actionPlate\.preserveAspect\s*=\s*true/,
    'Native painted action retains source aspect'],
  [/actionPlate\.raycastTarget\s*=\s*false/,
    'Painted overlay does not steal pointer input'],
  [/actionHit\.raycastTarget\s*=\s*true/,
    'True native action hit target'],
  [/resourceIllustrations\[i\]\.sprite=Resources\.Load<Sprite>\(/,
    'Canonical three resource sprites are drawn from real imported art']
];
for(const [expression,meaning] of sourceRequirements)
  assert.match(native,expression,
    'PR173 head '+ref.slice(0,12)+' changed contract: '+meaning);

// The current two independently painted families both exist in PR173's
// exact HEAD. CleanRoom supplies the actual identity/resource/action material;
// FreshArt supplies the three actual resource drawings. Validate their sprite
// importers, not deprecated FreshArt surface names or screenshot mockups.
for(const [family,name] of [
  ['CleanRoom','profile'],['CleanRoom','resource'],['CleanRoom','upgrade'],
  ['FreshArt','wood'],['FreshArt','stone'],['FreshArt','food']
]) {
  const path='Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/'+
    family+'/'+name+'.png';
  const [asset,meta]=await Promise.all([
    api('/contents/'+path+'?ref='+ref),
    api('/contents/'+path+'.meta?ref='+ref)
  ]);
  assert.equal(asset.type,'file','Live Vigilia source art disappeared: '+path);
  assert.ok(asset.size>5000,'Current painted source too small/missing: '+path);
  const info=Buffer.from(meta.content.replace(/\s/g,''),'base64').toString('utf8');
  assert.match(info,/textureType: 8/,'Current Vigilia material is not a Sprite: '+path);
  assert.match(info,/spriteMode: 1/,'Unexpected Sprite mode: '+path);
}

// The old bronze-outline cards are a deliberately SEMANTICALLY CLEAN
// separate reward treatment: unlike the current forge upgrade_button they
// do not have a permanent construction hammer baked into a reward CTA.
// Check that the actual runtime sources remain present, are single Sprites
// and have authentic 9-slice importers in PR173's exact HEAD.
const reusable=[
  'slate_resource','burgundy_action'
];
for(const name of reusable){
  const prefix='Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/PremiumSurfaces/'+name+'.png';
  const [asset,meta]=await Promise.all([
    api('/contents/'+prefix+'?ref='+ref),
    api('/contents/'+prefix+'.meta?ref='+ref)
  ]);
  assert.equal(asset.type,'file','Original reusable material disappeared: '+name);
  assert.ok(asset.size>5000,'Replacement material not real painted art: '+name);
  const info=Buffer.from(meta.content.replace(/\s/g,''),'base64').toString('utf8');
  assert.match(info,/textureType: 8/,'Material is no longer a Sprite: '+name);
  assert.match(info,/spriteMode: 1/,'Material sprite sheet unexpectedly changed: '+name);
  const border=/spriteBorder: \{x: (\d+), y: (\d+), z: (\d+), w: (\d+)\}/.exec(info);
  assert.ok(border && border.slice(1).every(n=>Number(n)>0),
    'Native nine-slice frame borders missing: '+name);
  assert.ok(treasury.includes('VigiliaArt/PremiumSurfaces/'+name),
    'Reward code no longer references compatible premium source: '+name);
}
for(const hammerAction of ['OwnerControls/upgrade_button',
                           'UnifiedButtons/action_gold']){
  assert.ok(!treasury.includes('VigiliaArt/'+hammerAction),
    'The forge upgrade hammer must not be burned onto a loyalty-claim action: '+
    hammerAction);
}

const localRequirements=[
 [/FindLiteralVigiliaCanvas\(\)/,'literal-name-safe Canvas discovery'],
 [/candidate\.gameObject\.name=="VIGILIA \/ independent canvas"/,'exact Canvas identity; slash is literal'],
 [/HasVisibleCanonicalHeroRoom\(\)/,'real Lyra presentation stays unobscured'],
 [/canvas\.transform\.Find\("Safe composition"\)/,'same safe area'],
 [/root\.SetParent\(safe,false\)/,'safe handoff'],
 [/root\.SetParent\(originalSafeArea,false\)/,'reversible handoff'],
 [/root\.SetAsLastSibling\(\)/,'ceremony above selectable building panels'],
 [/if\(onVigiliaCanvas && originalSafeArea\)/,'restores old UI if proof is destroyed'],
 [/if\(!dailyTreasures && safe\)/,'destroyed-panel recovery on old HUD'],
];
for(const [expression,meaning] of localRequirements) {
  assert.match(meaning==='destroyed-panel recovery on old HUD'?royal:treasury,
    expression,'PR181 missing cross-HUD capability: '+meaning);
}
// Unity 6000.3 interprets '/' in GameObject.Find as a hierarchy path.
// The two canonical Unity names have a LITERAL slash: never use Find.
assert.doesNotMatch(treasury,/GameObject\.Find\("VIGILIA \/ independent canvas"\)/);
assert.doesNotMatch(treasury,/GameObject\.Find\("Heroes \/ canonical destination"\)/);
// Never leak a standalone extra Canvas into a game with one EventSystem.
assert.doesNotMatch(treasury,/new GameObject\([^\n]+typeof\(Canvas\)/);
assert.doesNotMatch(treasury,/new GameObject\([^\n]+typeof\(EventSystem\)/);
console.log('PASS cross-workstream UI contract: PR181 on PR173 HEAD '+
  ref.slice(0,12)+'; safe area, currently painted Vigilia surfaces, '+
  'two physically present nine-sliced premium reward surfaces, no forge hammer');
