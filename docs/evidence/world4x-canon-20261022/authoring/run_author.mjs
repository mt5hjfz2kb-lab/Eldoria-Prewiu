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
fs.writeFileSync(inventory,
  'MEASURED SOURCE ANCHORS: existing mountain landmarks are [-33,19], [32.6,21.5], [-32,-22], [26.7,-17] in Blender XY. Improve ONE existing visible ridge, not new arbitrary compass-edge bumps. Previous 39830118330 edits at [-42,0], [42,0], [0,26] were technically valid but visually insufficient. The atlas is now preserved in Blender for honest geographic review. Existing water vertices are protected and a plan intersecting them fails. Choose a coherent sculpt_path or region around an existing mountain anchor; heights stay Z-up. LATEST REJECTION 39829726922: ridge_center at [-1,0,0] radius12 intersects protected gameplay and was NOT executed. NO CENTRAL RIDGE. Every centre/radius MUST satisfy sqrt(x*x+y*y)-radius >=12. Choose peripheral existing landforms, not the centre and not river. One coherent peripheral edit is enough; do not fill max-actions with central bumps. SCHEMA FEEDBACK from actual cycle 39829192368: every action id MUST be a unique nonempty STRING, e.g. "ridge_west", NEVER numeric 1. Keep parameters nested. The schema rejected numeric ids twice; do not repeat. REJECTED REAL CYCLE 39828569968: center[-90,0,0],radius15,delta[0,0.5,0] moved terrain sideways OUTSIDE camera; zero useful relief. DO NOT REPEAT. Blender Z is UP: delta must be [0,0,positiveHeight], NEVER [0,height,0]. Author visible low connected hills IN the photographed core: centers X[-42,42],Y[-26,26]; coordinates beyond this are OFFSCREEN, even if mesh exists. WORLD MAP, not Valoria cliff/city. 1 Blender unit=10 Unity metres. Preserve XY, UV and central circle radius12. Choose precise visible land centres from reference/screenshot, radius8..22 and vertical lift1..3 Blender units, avoiding coast/rivers and protected centre. Use sculpt_region or sculpt_path only, up to3 coherent actions. No color noise, new objects or enormous peaks. Actual source extended mesh X+/-91 Y+/-78 is NOT the visible region. Prior procedural microsteps/coastal ribbons failed. This is a COPY; never self-approve.');
const invocation=prepareInvocation({
  requestFile:'pipeline/art-production-requests/eldoria-world-region-1-canon-20261022.json',
  prefix:path.join(output,'memory'),sourceInventoryPath:inventory,
  artistArgs:['--reference','docs/visual-reference/world-map/03_vista_panoramica.png',
    '--unity-before',path.join(captures,'panoramica-horizontal-unity.png'),
    '--target-object','RegionOneCanonicalTerrain','--target-mode','existing',
    '--subject-kind','terrain','--mesh-context',path.join(output,'topology/mesh-context.json'),
    '--out',path.join(output,'direction'),'--model',model,
    '--num-ctx','8192','--num-predict','1800','--max-actions','3',
    '--repair-attempts','1','--request-timeout','420']
});
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
