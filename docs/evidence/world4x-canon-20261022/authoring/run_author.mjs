// Task adapter for the EXISTING memory selector and typed Blender author.
// No cloud inference, paid models, arbitrary generated Python or art approval.
import fs from 'node:fs';
import path from 'node:path';
import {pathToFileURL} from 'node:url';
import {spawnSync} from 'node:child_process';
const [rootArg,capturesArg,outputArg,model]=process.argv.slice(2);
const root=path.resolve(rootArg),captures=path.resolve(capturesArg),output=path.resolve(outputArg);
process.chdir(root);
const {prepareInvocation,verifyArtistConsumption}=await import(pathToFileURL(path.join(root,'tools/art-knowledge/run-artist.mjs')));
const {recordInference}=await import(pathToFileURL(path.join(root,'tools/art-knowledge/cycle-feedback.mjs')));
const inventory=path.join(output,'source-inventory.txt');
const brief="ACTUAL Unity terrain in Blender: Z UP; 1 unit=10 Unity metres. Author ONE visible existing mountain ridge, target RegionOneCanonicalTerrain. Source landmarks XY: NW[-33,19], NE[32.6,21.5], SW[-32,-22], SE[26.7,-17]. Measured nearest protected water distances: NW11.56, NE25.21, SW4.16, SE6.44 units. Water must remain untouched. Use sculpt_region or sculpt_path only, unique STRING id. Each point: center[x,y,0], radius8..22, delta[0,0,h] with h1..3. NO lateral displacement. abs(x)<=42,abs(y)<=26; sqrt(x*x+y*y)-radius>=12 protects gameplay. Preserve XY/topology/UV. Pick actual geometry from images, not arbitrary compass bumps. Prior rejected edits: sideways delta[0,.2,0]; central bumps; offscreen x=-90; imperceptible compass-edge hills. Do not repeat. Terrain atlas is preserved in Blender. One coherent reference-linked landform; no city, new objects, fog tricks or self-approval.";
if(brief.length>1100)throw Error('Terrain constraints exceed verified source context budget');
fs.writeFileSync(inventory,brief);
const invocation=prepareInvocation({
  requestFile:'pipeline/art-production-requests/eldoria-world-region-1-canon-20261022.json',
  prefix:path.join(output,'memory'),sourceInventoryPath:inventory,
  artistArgs:['--reference','docs/visual-reference/world-map/03_vista_panoramica.png',
    '--unity-before',path.join(captures,'panoramica-horizontal-unity.png'),
    '--target-object','RegionOneCanonicalTerrain','--target-mode','existing',
    '--subject-kind','terrain','--mesh-context',path.join(output,'topology/mesh-context.json'),
    '--out',path.join(output,'direction'),'--model',model,
    '--num-ctx','8192','--num-predict','1800','--max-actions','1',
    '--repair-attempts','1','--request-timeout','420']
});
const consumedInventory=fs.readFileSync(invocation.inventory,'utf8');
if(!consumedInventory.includes(brief))
  throw Error('Canonical adapter truncated essential terrain constraints; refuse inference');
const python=process.env.ELDORIA_REGION1_PYTHON;
if(!python||!fs.existsSync(python))throw Error('Verified existing Blender Python required; do not invoke Windows Store alias');
const proc=spawnSync(python,invocation.command.slice(1),{stdio:'inherit',shell:false,timeout:950000});
recordInference({invocation,outFile:invocation.feedback,exitStatus:proc.status??1});
if(proc.error)throw proc.error;
if(proc.status!==0)throw Error('Typed Qwen author failed; no geometry or approval inferred');
console.log(JSON.stringify(verifyArtistConsumption(invocation)));
// Reject the demonstrated coordinate/visibility error before Blender executes.
// Values remain Qwen's decisions; the task supplies only the protected envelope.
const plan=JSON.parse(fs.readFileSync(path.join(output,'direction/artist-plan.json'),'utf8'));
for(const op of plan){
  const points=op.type==='sculpt_region'?[op.parameters]:op.type==='sculpt_path'?op.parameters.knots:null;
  if(!Array.isArray(points)||!points.length)throw Error('Only bounded terrain sculpt operations allowed');
  for(const p of points){
    const [x,y]=p.center||[],[dx,dy,dz]=p.delta||[];
    if(![x,y,dx,dy,dz,p.radius].every(Number.isFinite)||Math.abs(x)>42||Math.abs(y)>26||
       dx!==0||dy!==0||dz<1||dz>3||p.radius<8||p.radius>22)
      throw Error('Artist operation outside visible region or not a bounded Z-up relief edit');
    if(Math.hypot(x,y)-p.radius<12)
      throw Error('Artist operation intersects protected central gameplay circle');
  }
}

const measured=JSON.parse(fs.readFileSync(path.join(captures,'terrain-source.json'),'utf8'));
if(!Array.isArray(measured.protectedWater))throw Error('Measured water exclusions missing');
for(const op of plan){
  const points=op.type==='sculpt_region'?[op.parameters]:op.parameters.knots;
  for(const p of points)for(let i=0;i<measured.vertices.length;i++){
    if(!measured.protectedWater[i])continue;
    const v=measured.vertices[i];
    const x=(v.x-measured.origin.x)/10,y=(v.z-measured.origin.z)/10,z=v.y/10;
    if(Math.hypot(x-p.center[0],y-p.center[1],z-p.center[2])<p.radius)
      throw Error('Artist operation intersects measured protected water vertex '+i);
  }
}
