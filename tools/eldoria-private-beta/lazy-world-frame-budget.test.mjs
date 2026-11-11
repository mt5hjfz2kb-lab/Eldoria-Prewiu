import {readFileSync} from 'node:fs';
import test from 'node:test';
import assert from 'node:assert/strict';
const placement=readFileSync('Unity/Assets/Eldoria/World4XCanon/RegionOneResourcePlacement.cs','utf8');
const world=readFileSync('Unity/Assets/Eldoria/World4XCanon/RegionOneCanonicalLandscape.cs','utf8');
const routing=readFileSync('Unity/Assets/Eldoria/World4XCanon/RegionOneTerrainRoutes.cs','utf8');
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
test('time and work bounded pauses preserve complete 32m rows and the same query order',()=>{
 assert.match(placement,/for\(int z=-1500;z<=1600;z\+=32\)[\s\S]*?for\(int x=-750;x<=600;x\+=32\)/);
 assert.match(placement,/sites\.Add\(new Site\{Id="region1:"[\s\S]*?if\(slice\.ShouldYield\(\)\)\{yield return null;slice\.Resume\(\);\}\s*}\s*}\s*if\(routes==null\)/);
 assert.match(placement,/using\(var tape=new TerrainQueryTape\(host,landscape\)\)/);
});
test('lazy opt-in yields real resources, while unflagged and original world still use eager parity',()=>{
 assert.match(world,/if\(ValoriaIntegratedWorld\.P0LazyWorldProbe\)\s+yield return RegionOneResourcePlacement\.InstallStaged\(root,host,geography\);/);
 assert.match(world,/else RegionOneResourcePlacement\.Install\(root,host,geography\);/);
 assert.match(world,/RegionOneCreaturePopulation\.Install\(root,host,placement\);/);
 assert.match(world,/REGION1_CANON_WORLD_INSTALLED/);
});

test('the full 449-row route-grid raycast tape remains complete with bounded work slices',()=>{
 assert.match(routing,/public static IEnumerator CreateStaged\([\s\S]*?yield return result\.BuildGrid\(terrain\);[\s\S]*?onReady\?\.Invoke\(result\)/);
 assert.match(routing,/IEnumerator BuildGrid\([\s\S]*?for\(int z=-224;z<=224;z\+\+\)\s*\{\s*for\(int x=-112;x<=112;x\+\+\)[\s\S]*?if\(slice\.ShouldYield\(\)\)\{yield return null;slice\.Resume\(\);\}\s*\}/);
 assert.equal((routing.match(/var ray=new Ray\(host\.WorldHome\+new Vector3\(xx,250,zz\),Vector3\.down\);/g)||[]).length,1,
   'do not fork or duplicate route-grid terrain queries');
 assert.match(routing,/var stages=BuildGrid\(terrain\);[\s\S]*?while\(stages\.MoveNext\(\)\)/,
   'eager route grid must drain the exact staged iterator synchronously');
 assert.match(placement,/if\(ValoriaIntegratedWorld\.P0LazyWorldProbe\)\s+yield return RegionOneTerrainRoutes\.CreateStaged\(host,surface,faults,ready=>routes=ready\);\s+else\s+routes=new RegionOneTerrainRoutes\(host,surface,faults\)/);
 assert.match(placement,/yield return RegionOneTerrainRoutes\.CreateStaged\([\s\S]*?tape\.Complete\(\)/,
   'response tape must stay alive until the complete route lattice is sampled');
});

const blender=readFileSync('Unity/Assets/Eldoria/World4XCanon/RegionOneBlenderLandscape.cs','utf8');
test('opt-in contact surface uses one unchanged bucketing loop with time and allocation-work bounds',()=>{
 const shared=blender.match(/IEnumerator BuildContactSurfaceStaged\(Vector3 origin\)[\s\S]*?activeSurface=this;/);
 assert.ok(shared,'a single authoritative contact-building loop must exist');
 assert.match(shared[0],/for\(int i=0;i<contactTriangles.Length;i\+=3\)[\s\S]*?list.Add\(i\);[\s\S]*?if\(\(\(i\/3\)\+1\)%256==0&&slice\.ShouldYield\(256\)\)\{\s*yield return null; slice\.Resume\(\);/);
 assert.equal((blender.match(/for\(int i=0;i<contactTriangles.Length;i\+=3\)/g)||[]).length,1,'never fork triangle bucket construction');
 assert.match(blender,/void BuildContactSurface\(Vector3 origin\)[\s\S]*?var stages=BuildContactSurfaceStaged\(origin\);[\s\S]*?while\(stages.MoveNext\(\)\)/);
 assert.match(blender,/if\(ValoriaIntegratedWorld.P0LazyWorldProbe\)\s+yield return instance.BuildContactSurfaceStaged\(host.WorldHome\);\s+else instance.BuildContactSurface\(host.WorldHome\);/);
 assert.match(shared[0],/activeSurface=this;/,'publish the query surface only after ALL contact buckets are ready');
});

test('every phase retains a hard work bound and resets its clock after the frame boundary',()=>{
 assert.match(blender,/var slice=new RegionOneFrameSlice\(16384\)/);
 assert.match(placement,/var slice=new RegionOneFrameSlice\(8\)/);
 assert.match(routing,/var slice=new RegionOneFrameSlice\(8\)/);
 for(const source of [blender,placement,routing])assert.match(source,/yield return null;\s*slice\.Resume\(\);/);
});

