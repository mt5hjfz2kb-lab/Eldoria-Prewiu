import fs from 'node:fs';
import assert from 'node:assert/strict';

// Unity's special Editor folder does NOT isolate scripts beneath a parent
// .asmdef. Without this nested Editor-only assembly, native QA code referencing
// UnityEditor would enter the player DLL and break Android/WebGL compilation.
// Unity documentation: assembly-definitions-creating (Unity 6000).
const prefix='Unity/Assets/Eldoria/DailyTreasures';
const runtime=JSON.parse(fs.readFileSync(
  prefix+'/Eldoria.DailyTreasures.UI.asmdef','utf8'));
const editor=JSON.parse(fs.readFileSync(
  prefix+'/Editor/Eldoria.DailyTreasures.Editor.asmdef','utf8'));
assert.equal(runtime.name,'Eldoria.DailyTreasures.UI');
assert.equal(editor.name,'Eldoria.DailyTreasures.Editor');
assert.deepEqual(editor.includePlatforms,['Editor']);
assert.ok(!editor.excludePlatforms?.length);
assert.ok(!runtime.includePlatforms?.length,
  'Runtime reward panel must remain available in Android and WebGL');
assert.ok(!runtime.references.includes(editor.name),
  'Runtime assembly may never reference editor-only tests');
for(const name of [runtime.name,'Eldoria.Domain',
                    'Eldoria.Application','Unity.ugui'])
  assert.ok(editor.references.includes(name),
    'Native QA editor assembly must explicitly reference '+name);
const native=fs.readFileSync(
  prefix+'/Editor/DailyTreasureNativeEvidence.cs','utf8');
assert.match(native,/#if UNITY_EDITOR\s+using System;/);
assert.match(native,/using UnityEditor;/);
const runtimeScripts=fs.readdirSync(prefix+'/Runtime')
  .filter(f=>f.endsWith('.cs'));
assert.ok(runtimeScripts.length>=10);
for(const source of runtimeScripts){
  const c=fs.readFileSync(prefix+'/Runtime/'+source,'utf8');
  assert.doesNotMatch(c,/^using UnityEditor;/m,
    'Player runtime would try to link editor APIs: '+source);
}
assert.match(fs.readFileSync(prefix+'/Editor/Eldoria.DailyTreasures.Editor.asmdef.meta','utf8'),
    /^guid: [a-f0-9]{32}$/m);
console.log('PASS real Unity assembly boundary: runtime safe for Android/WebGL, Editor art capture isolated and properly referenced');
