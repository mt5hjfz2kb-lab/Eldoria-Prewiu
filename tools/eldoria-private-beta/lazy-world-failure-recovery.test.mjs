import {readFileSync} from 'node:fs';
import assert from 'node:assert/strict';
import test from 'node:test';
const world=readFileSync('Unity/Assets/Eldoria/World4XCanon/RegionOneCanonicalLandscape.cs','utf8');
const host=readFileSync('Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs','utf8');
test('only opt-in full lazy world uses guarded nested coroutine instead of unsafe Unity orphan load',()=>{
 assert.match(world,/StartCoroutine\(GuardedDeferredBuild\(\)\)/);
 assert.match(world,/if\(failed\)return false;/);
 assert.match(world,/stack.Push\(BuildAndInstall\(\)\)/);
 assert.match(world,/if\(yielded is IEnumerator inner\)\{stack.Push\(inner\);continue;\}/);
 assert.match(world,/catch\(Exception ex\)\{failure=ex;break;\}/);
 assert.match(world,/while\(stack.Count>0\)[\s\S]*?\.Dispose\(\)/);
 assert.match(world,/Time.realtimeSinceStartup>deadline/);
 assert.match(world,/REGION1_P0_LAZY_BUILD_FAILED/);
 assert.match(world,/else partialWorldRoot=null;/);
 assert.match(world,/public bool HasFailed=>failed;/);
});
test('failed lazy load releases ONLY temporary Region1 presentation; no save, hud or gameplay mutation',()=>{
 assert.match(world,/if\(partialWorldRoot\)Destroy\(partialWorldRoot.gameObject\);/);
 assert.match(world,/if\(originals\[i\]\)originals\[i\]\.enabled=states\[i\]/);
 assert.match(world,/if\(lods\[i\]\)lods\[i\]\.enabled=lodStates\[i\]/);
 assert.doesNotMatch(world,/new LocalGateway|DeleteAll|DeleteKey|PlayerPrefs\.Set|Save\(/);
 assert.match(host,/!worldPresentation.IsReady&&!worldPresentation.HasFailed/);
 assert.match(host,/if\(worldPresentation.HasFailed\)[\s\S]*?return false;/);
});
test('unflagged and city-only modes retain original behavior',()=>{
 assert.match(world,/if \(ValoriaIntegratedWorld\.P0CityOnlyProbe\)[\s\S]*?yield break;/);
 assert.match(world,/if \(ValoriaIntegratedWorld\.P0LazyWorldProbe\)[\s\S]*?yield break;/);
 assert.match(world,/yield return BuildAndInstall\(\);/);
 assert.match(host,/if\(open&&P0CityOnlyProbe\)/);
});
