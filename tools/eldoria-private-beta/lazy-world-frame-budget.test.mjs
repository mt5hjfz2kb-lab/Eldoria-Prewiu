import {readFileSync} from 'node:fs';
import test from 'node:test';
import assert from 'node:assert/strict';
const placement=readFileSync('Unity/Assets/Eldoria/World4XCanon/RegionOneResourcePlacement.cs','utf8');
const world=readFileSync('Unity/Assets/Eldoria/World4XCanon/RegionOneCanonicalLandscape.cs','utf8');
test('eager and opt-in deferred installation reuse ONE authoritative terrain scan',()=>{
 const eager=placement.match(/public static void Install\([\s\S]*?\n        }/);
 const staged=placement.match(/public static IEnumerator InstallStaged\([\s\S]*?\n        }/);
 assert.ok(eager&&staged);
 assert.match(eager[0],/service\.Scan\(landscape\);/);
 assert.match(staged[0],/yield return service\.ScanStaged\(landscape\);/);
 assert.match(placement,/void Scan\(RegionOneBlenderLandscape landscape\)[\s\S]*?var stages=ScanStaged\(landscape\);/);
 assert.equal((placement.match(/new Site\{Id="region1:"/g)||[]).length,1,'never fork site generation');
 assert.match(placement,/tape\.Complete\(\)/);
});
test('one frame boundary after each untouched 32m Z row, never in inner raycast sequence',()=>{
 assert.match(placement,/for\(int z=-1500;z<=1600;z\+=32\)[\s\S]*?for\(int x=-750;x<=600;x\+=32\)/);
 assert.match(placement,/sites\.Add\(new Site\{Id="region1:"[\s\S]*?yield return null;\s*}\s*}\s*if\(routes==null\)/);
 assert.match(placement,/using\(var tape=new TerrainQueryTape\(host,landscape\)\)/);
});
test('lazy opt-in yields real resources, while unflagged and original world still use eager parity',()=>{
 assert.match(world,/if\(ValoriaIntegratedWorld\.P0LazyWorldProbe\)\s+yield return RegionOneResourcePlacement\.InstallStaged\(root,host,geography\);/);
 assert.match(world,/else RegionOneResourcePlacement\.Install\(root,host,geography\);/);
 assert.match(world,/RegionOneCreaturePopulation\.Install\(root,host,placement\);/);
 assert.match(world,/REGION1_CANON_WORLD_INSTALLED/);
});
