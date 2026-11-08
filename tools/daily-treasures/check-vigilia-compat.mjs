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
  [/Resources\.Load<Sprite>\("VigiliaArt\/PremiumSurfaces\/profile_plate"\)/,
    'Vigilia reference plate location'],
  [/Resources\.Load<Sprite>\("VigiliaArt\/PremiumSurfaces\/burgundy_action"\)/,
    'Vigilia approved burgundy material'],
  [/VigiliaArt\/ResourceIcons\//,'canonical resource icon source']
];
for(const [expression,meaning] of sourceRequirements)
  assert.match(native,expression,
    'PR173 head '+ref.slice(0,12)+' changed contract: '+meaning);

const localRequirements=[
 [/GameObject\.Find\("VIGILIA \/ independent canvas"\)/,'source host discovery'],
 [/vigilia\.transform\.Find\("Safe composition"\)/,'same safe area'],
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
// Never leak a standalone extra Canvas into a game with one EventSystem.
assert.doesNotMatch(treasury,/new GameObject\([^\n]+typeof\(Canvas\)/);
assert.doesNotMatch(treasury,/new GameObject\([^\n]+typeof\(EventSystem\)/);
console.log('PASS cross-workstream UI contract: PR181 on PR173 HEAD '+
  ref.slice(0,12)+'; safe-area names, player-owned Canvas, materials, exact return');
