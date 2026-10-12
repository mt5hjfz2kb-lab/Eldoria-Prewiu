import test from 'node:test';
import assert from 'node:assert/strict';
import {checkArtChangedFiles,isArtChange} from './check-art-changed-scope.mjs';

const normal = {
 id:'organic-cliff-production',status:'active',art_production_routing:'required',
 scope:['art-source/valoria/variants/**','Unity/Assets/Eldoria/Resources/Valoria/**']
};
const registry=(...workstreams)=>({active:workstreams});
const request=(w)=>({
 workstream_id:w.id,request_id:w.id,enabled:true,status:'ACTIVE',
 ai_authorship_policy:'required'
});
const assess=(files,rs,rf=request)=>checkArtChangedFiles(files,rs,rf);

test('no unclaimed asset may bypass permanent AI production',()=>{
 assert.match(assess(['art-source/valoria/variants/cliff.blend'],registry())[0],/without matching ACTIVE/);
});
test('no unclaimed Unity GLB may bypass by omitting the production request',()=>{
 assert.match(assess(['Unity/Assets/Eldoria/Resources/Valoria/cliff.glb'],registry())[0],/without matching ACTIVE/);
});
test('a legitimate new art workstream with required policy may stage WIP source changes',()=>{
 assert.deepEqual(assess(['art-source/valoria/variants/cliff.blend','Unity/Assets/Eldoria/Resources/Valoria/cliff.glb'],registry(normal)),[]);
});
test('registered workstream without AI mandatory policy must fail',()=>{
 assert.match(assess(['art-source/valoria/variants/cliff.glb'],registry(normal),w=>({...request(w),ai_authorship_policy:'none'}))[0],/no valid mandatory/);
});
test('not-applicable workstream cannot authorize source art edits',()=>{
 assert.match(assess(['art-source/valoria/variants/cliff.glb'],registry({...normal,art_production_routing:'not_applicable'}))[0],/no valid mandatory/);
});
test('failed or inactive request cannot cover source art',()=>{
 assert.match(assess(['art-source/valoria/variants/cliff.glb'],registry(normal),w=>({...request(w),status:'BLOCKED'}))[0],/no valid mandatory/);
});
test('grandfathered LIVE Bastion ownership remains valid but is not a universal exemption',()=>{
 const old={...normal,id:'valoria-bastion1-player-scene-integration-20261012',
  scope:['Unity/Assets/Eldoria/ArtTests/Bastion1Assembly/**']};
 assert.deepEqual(assess(['Unity/Assets/Eldoria/ArtTests/Bastion1Assembly/Textures/rock.png'],registry(old),w=>({...request(w),ai_authorship_policy:undefined})),[]);
 assert.ok(assess(['Unity/Assets/Eldoria/ArtTests/Bastion1Assembly/Textures/rock.png'],registry()).length);
});
test('gameplay-only, documentation and tester changes do not falsely require art authorship',()=>{
 const files=['Unity/Assets/Eldoria/Scripts/Presentation/SlicePresenter.cs','docs/README.md','qa/e2e-tester-build.js'];
 assert.deepEqual(assess(files,registry()),[]);
 assert.ok(files.every(x=>!isArtChange(x)));
});
test('scene and model edits are considered artistic changes',()=>{
 assert.equal(isArtChange('Unity/Assets/Eldoria/ProductionSlice/Runtime/Valoria.unity'),true);
 assert.equal(isArtChange('Unity/Assets/Eldoria/Families/Watchtower.prefab'),true);
 assert.equal(isArtChange('tools/valoria-bastion1-assembly/terrain_finish.py'),true);
});

test('new remote asset folders cannot hide artistic 3D textures or terrain',()=>{
 for(const p of [
  'Unity/Assets/Eldoria/NextRegion/Cliff/rocks.png',
  'Unity/Assets/Eldoria/NextRegion/Cliff/rock_normal.tga',
  'Unity/Assets/Eldoria/NextRegion/Cliff/Landscape.terrainlayer',
  'Unity/Assets/Eldoria/NextRegion/Scene/NewScene.unity',
  'Unity/Assets/Eldoria/NextRegion/Props/stone.shadergraph',
  'Unity/Assets/Eldoria/NextRegion/Meshes/terrain.asset',
  'Unity/Assets/Eldoria/NextRegion/Models/cliff.fbx'
 ]) {
  assert.ok(isArtChange(p),'missed future art asset: '+p);
  assert.match(assess([p],registry())[0],/without matching ACTIVE/);
 }
});
test('C# presentation behavior and QA tests stay outside art changed-file guard',()=>{
 for(const p of ['Unity/Assets/Eldoria/Scripts/UI/SomeBehavior.cs','Unity/Assets/Eldoria/Tests/PlayMode/PlayerMovement.cs'])
  assert.equal(isArtChange(p),false);
});
