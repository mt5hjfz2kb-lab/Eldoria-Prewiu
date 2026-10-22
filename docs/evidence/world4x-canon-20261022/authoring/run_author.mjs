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
  'REGION 1 WORLD MAP, NOT VALORIA CLIFF OR CITY. Frozen actual Unity terrain scaled: 1 Blender unit=10 Unity metres. Blender X east, Y north, Z up. Bounds X +/-91 Y +/-78. Core X +/-42.5 Y +/-26. Existing broad low hills, no large mountains. Preserve central radius 12 around (0,0), ground Breach (0.8,6.1), rivers and coastal water; preserve XY and UV, edit Z only. Reference is an art direction, not a texture to project. Choose ONE coherent macro relief improvement in outer land, not scattered noise, color tweaks or new buildings. Max 3 actions, use measured anchors. Prior repeated procedural microsteps and coastal ribbons failed visual parity. This is a COPY; never self-approve.');
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
