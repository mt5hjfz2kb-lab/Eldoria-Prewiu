#!/usr/bin/env node
// World4X source-only bridge geometry check: genuine Blender GLB + Unity camera metadata.
// Runs with Node 18+ on any OS. Does not need a Windows runner, Unity, Blender or network.
// A geometrically plausible projection is NEVER an ART_APPROVED or PlayMode PASS.
import fs from 'node:fs';
import crypto from 'node:crypto';
function fail(message){throw Error(message);}
const [,,glbFile,cameraFile,dxArg='3.3',dyArg='10.5']=process.argv;
if(!glbFile||!cameraFile)fail('Usage: node offline_bridge_projection.mjs INPUT.glb cercana-horizontal-source.json [shiftX=3.3] [shiftBlenderY=10.5]');
const glb=fs.readFileSync(glbFile),raw=fs.readFileSync(cameraFile);
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
if(glb.readUInt32LE(0)!==0x46546c67||glb.readUInt32LE(4)!==2||glb.readUInt32LE(8)!==glb.length)fail('Invalid GLB2');
const len=glb.readUInt32LE(12);
if(glb.readUInt32LE(16)!==0x4e4f534a)fail('First GLB chunk not JSON');
const gltf=JSON.parse(glb.subarray(20,20+len).toString('utf8'));
const cam=JSON.parse(raw.toString('utf8').replace(/^\uFEFF/,''));
if(cam.actualUnityCamera!==true||cam.playMode!==true||cam.label!=='cercana-horizontal')fail('Expected authentic near-horizontal Unity PlayMode camera metadata');
if(!Number.isFinite(cam.zoom)||cam.zoom<=0||!Number.isFinite(cam.height)||!Number.isFinite(cam.width))fail('Missing camera dimensions');
const node=gltf.nodes.find(n=>n.name==='BLD_Terrain');
if(!node||node.mesh===undefined||node.matrix||node.translation||node.rotation||node.scale)fail('Terrain has unsupported transform');
const mesh=gltf.meshes[node.mesh];
const matches=mesh.primitives.filter(p=>(gltf.materials[p.material]?.name||'')==='Bridge alternate weathered plank.001');
if(matches.length!==1)fail('Expected exactly one central bridge material primitive; got '+matches.length);
const p=matches[0],accessor=gltf.accessors[p.attributes.POSITION];
if(!accessor.min||!accessor.max)fail('No position bounds in GLB');
const min=accessor.min,max=accessor.max;
if(Math.abs((min[0]+max[0])/2)>25)fail('Selected deck is not central');
const q=cam.cameraRotation,pitch=2*Math.atan2(q.x,q.w);
if(Math.abs(pitch-42*Math.PI/180)>.02||Math.abs(q.y)>.001||Math.abs(q.z)>.001)fail('Unexpected camera rotation');
const scale=cam.height/(2*cam.zoom),dx=Number(dxArg),dy=Number(dyArg);
if(!Number.isFinite(dx)||!Number.isFinite(dy))fail('Non-numeric displacement');
const bin=20+len+8;
if(glb.readUInt32LE(20+len+4)!==0x004e4942||accessor.componentType!==5126||accessor.type!=='VEC3')fail('Expected float32 VEC3 geometry');
const bv=gltf.bufferViews[accessor.bufferView],stride=bv.byteStride||12,offset=bin+(bv.byteOffset||0)+(accessor.byteOffset||0);
let count=0,sumX=0,sumY=0;
for(let i=0;i<accessor.count;i++){
 const pos=offset+i*stride,x=glb.readFloatLE(pos),y=glb.readFloatLE(pos+4),z=glb.readFloatLE(pos+8);
 if(Math.abs(z-min[2])<.03){sumX+=x;sumY+=y;count++;}
}
if(count<2)fail('Could not identify castle-facing bridge edge');
const bridgeX=sumX/count,bridgeY=sumY/count;
function project(x,y,z){
 // For this pinned export, GLB Z is negative of Unity Z. Relative WorldHome is camera target.
 return [cam.width/2+scale*x,cam.height/2+scale*(Math.sin(pitch)*z-Math.cos(pitch)*y)];
}
const previous=project(bridgeX,bridgeY,min[2]),projected=project(bridgeX+dx,bridgeY,min[2]-dy);
const round=n=>Number(n.toFixed(3));
console.log(JSON.stringify({
 status:'OFFLINE_SOURCE_ONLY_NO_ART_APPROVAL',
 glb_sha256:sha(glb),camera_sha256:sha(raw),actual_unity_camera:true,
 central_deck_material:gltf.materials[p.material].name,source_vertices:accessor.count,
 bounds_glb_xyz_m:[min,max],castle_facing_border_vertices:count,
 previous_endpoint_projected_px:previous.map(round),
 shift_blender_x_m:dx,shift_blender_y_m:dy,
 predicted_endpoint_at_prior_height_px:projected.map(round),
 screen_delta_px:[projected[0]-previous[0],projected[1]-previous[1]].map(round),
 limitations:['Cannot verify real gate model from this GLB','Candidate adjusts ground-clamped height: actual camera pixels will differ','Native Unity screenshots and 57/26/44 still mandatory']
},null,2));
