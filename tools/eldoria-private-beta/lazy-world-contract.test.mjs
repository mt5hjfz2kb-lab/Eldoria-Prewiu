import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';

const world=readFileSync('Unity/Assets/Eldoria/World4XCanon/RegionOneCanonicalLandscape.cs','utf8');
const host=readFileSync('Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs','utf8');

test('deferred real world is strictly opt-in and city-only never initiates region',()=>{
  assert.match(host,/#elif UNITY_WEBGL\s+return !P0CityOnlyProbe && UnityEngine\.Application\.absoluteURL\.Contains\("eldoria_p0_lazy_world=1"\)/);
  assert.match(world,/if \(ValoriaIntegratedWorld\.P0CityOnlyProbe\)[\s\S]*?WORLD_INITIALIZATION_SKIPPED[\s\S]*?yield break;/);
  assert.match(world,/if \(ValoriaIntegratedWorld\.P0LazyWorldProbe\)[\s\S]*?ELDORIA_P0_LAZY_WORLD_WAITING_FOR_ENTRY[\s\S]*?yield break;/);
  assert.match(world,/yield return BuildAndInstall\(\);/);
});

test('the real staged map installs once and gates camera exposure',()=>{
  assert.match(world,/public bool BeginDeferredLoad\(\)[\s\S]*?if\(built\|\|building\)return true;/);
  // A nested stage exception previously abandoned the live Unity coroutine,
  // leaving IsLoading stuck and a partial world resident forever.
  assert.match(world,/StartCoroutine\(GuardedDeferredBuild\(\)\);/);
  assert.match(world,/stack.Push\(BuildAndInstall\(\)\);/);
  assert.match(world,/if\(failed\)return false;/);
  assert.match(world,/if\(yielded is IEnumerator inner\)/);
  assert.match(world,/if\(failure!=null\)[\s\S]*?building=false;built=false;/);
  assert.match(world,/yield return RegionOneBlenderLandscape\.InstallStaged\(/);
  assert.match(world,/RegionOneCreaturePopulation\.Install\(/);
  assert.match(world,/REGION1_CANON_WORLD_INSTALLED/);
  const openGate=host.indexOf('public bool TrySetWorld(bool open)');
  const progressGate=host.indexOf('if(open&&!CanEnterWorld',openGate);
  const asyncGate=host.indexOf('if(open&&P0LazyWorldProbe)',openGate);
  const cameraSwitch=host.indexOf('WorldRoot.SetActive(open)',openGate);
  assert.ok(0<openGate&&openGate<progressGate&&progressGate<asyncGate&&asyncGate<cameraSwitch,
    'sawmill progression and full readiness must precede activating world camera');
  assert.match(host,/EnterWorldAfterDeferredBuild\(\+\+deferredWorldEpoch\)/);
  assert.match(host,/!worldPresentation\.IsReady/);
  assert.match(host,/if\(!open&&deferredWorldEntryQueued\)/);
});

test('the launch path preserves gameplay authority, HUD and save schemas',()=>{
  assert.match(host,/BindSession\(\);Hud\.SelectionChanged\+=SelectionChanged;/);
  assert.match(host,/bool deferredWorldEntryQueued;/);
  assert.doesNotMatch(host,/PlayerPrefs\.DeleteAll\(|DeleteFile\(/);
  assert.doesNotMatch(world,/new LocalGateway\(/);
});
