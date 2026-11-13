import fs from 'node:fs';
import assert from 'node:assert/strict';

// Native Unity 6000.3.23f1 already verified the authored audio is NOT
// compilable if this built-in module is switched off. A pure C# test that
// uses no AudioClip types cannot prove the full player assembly compiles.
const manifest=JSON.parse(fs.readFileSync('Unity/Packages/manifest.json','utf8'));
assert.equal(manifest.dependencies?.['com.unity.modules.audio'],'1.0.0',
  'Native Unity AudioModule must be enabled for original premium chest Foley');
const panel=fs.readFileSync(
  'Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePanel.cs','utf8');
assert.match(panel,/AudioSource originalSfx/);
assert.match(panel,/AudioClip chestKnock,chestChime,chestGrandChime/);
assert.match(panel,/DailyTreasureFoley\.Render\(cue,rate\)/);
assert.doesNotMatch(panel,/#if\s+UNITY_EDITOR[\s\S]*?AudioClip/,
  'Sounds cannot be restricted to Editor: WebGL/Android player needs music-free Foley');
console.log('PASS exact Unity 6000.3 built-in AudioModule dependency: original wooden latch + two bronze cues remain available in players');
