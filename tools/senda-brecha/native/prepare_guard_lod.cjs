// Build native guard from the exact normal-compatible Aldric. Retain every bone,
// animation, texture, vertex attribute and original binary byte; replace indices only.
const fs=require('fs'),path=require('path'),crypto=require('crypto'),assert=require('assert/strict');
const {MeshoptSimplifier}=require('meshoptimizer');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
(async()=>{
 const root=process.argv[2],src=fs.readFileSync(path.join(root,'SirAldric_Approved_RiggedAnimated_v1.glb'));
 assert.equal(sha(src),'d64f765625340365ad760e7e8d0639d640ebdd828d55c0635b3aefc0c974081b');
 const n=src.readUInt32LE(12),original=JSON.parse(src.subarray(20,20+n)),d=JSON.parse(JSON.stringify(original));
 const bin=src.subarray(28+n);assert.equal(bin.length,d.buffers[0].byteLength);
 await MeshoptSimplifier.ready;assert.ok(MeshoptSimplifier.supported);
 const chunks=[bin],stats=[];let offset=bin.length,total=0;
 for(let mi=0;mi<2;mi++){
  const mesh=d.meshes[mi];assert.ok(mi===0?mesh.name.includes('SirAldric_Tripo'):mesh.name.includes('Cape_Main'));
  const p=mesh.primitives[0],pa=d.accessors[p.attributes.POSITION],pv=d.bufferViews[pa.bufferView],ia=d.accessors[p.indices],iv=d.bufferViews[ia.bufferView];
  assert.equal(pa.componentType,5126);assert.ok([5123,5125].includes(ia.componentType));
  assert.ok(p.attributes.NORMAL!==undefined&&p.attributes.JOINTS_0!==undefined&&p.attributes.WEIGHTS_0!==undefined);
  const pos=new Float32Array(pa.count*3),ids=new Uint32Array(ia.count),start=(pv.byteOffset||0)+(pa.byteOffset||0),stride=pv.byteStride||12;
  for(let i=0;i<pa.count;i++)for(let k=0;k<3;k++)pos[i*3+k]=bin.readFloatLE(start+i*stride+4*k);
  const ix=(iv.byteOffset||0)+(ia.byteOffset||0);for(let i=0;i<ia.count;i++)ids[i]=ia.componentType===5125?bin.readUInt32LE(ix+i*4):bin.readUInt16LE(ix+i*2);
  const target=mi===0?5200:800;let [reduced]=MeshoptSimplifier.simplify(ids,pos,3,target*3,.10,[]);
  if(reduced.length>target*4.5)[reduced]=MeshoptSimplifier.simplify(ids,pos,3,target*3,.20,[]);
  assert.ok(reduced.length>0&&reduced.length%3===0&&reduced.every(x=>x<pa.count));
  const bytes=Buffer.alloc(reduced.length*4);for(let i=0;i<reduced.length;i++)bytes.writeUInt32LE(reduced[i],i*4);
  const view=d.bufferViews.length;d.bufferViews.push({buffer:0,byteOffset:offset,byteLength:bytes.length,target:34963});
  p.indices=d.accessors.length;d.accessors.push({bufferView:view,componentType:5125,count:reduced.length,type:'SCALAR'});
  chunks.push(bytes);offset+=bytes.length;total+=reduced.length/3;
  assert.deepEqual(p.attributes,original.meshes[mi].primitives[0].attributes);
  stats.push({name:mesh.name,originalTriangles:ids.length/3,triangles:reduced.length/3});
 }
 // Leave all node hierarchy intact, including animation targets. Remove the
 // eight redundant hero-specific mesh attachments, not nodes or bones.
 let removed=0;for(const node of d.nodes)if(node.mesh!==undefined&&node.mesh>1){delete node.mesh;delete node.skin;removed++}
 assert.equal(removed,8);assert.ok(total>1000&&total<=12000);assert.equal(d.skins[0].joints.length,26);assert.deepEqual(d.skins,original.skins);assert.deepEqual(d.animations,original.animations);
 d.buffers[0].byteLength=offset;const j0=Buffer.from(JSON.stringify(d)),j=Buffer.concat([j0,Buffer.alloc((4-j0.length%4)%4,32)]),b=Buffer.concat(chunks),header=Buffer.alloc(20),bh=Buffer.alloc(8);
 header.writeUInt32LE(0x46546c67,0);header.writeUInt32LE(2,4);header.writeUInt32LE(28+j.length+b.length,8);header.writeUInt32LE(j.length,12);header.writeUInt32LE(0x4e4f534a,16);bh.writeUInt32LE(b.length,0);bh.writeUInt32LE(0x004e4942,4);
 assert.ok(b.subarray(0,bin.length).equals(bin));
 const result=Buffer.concat([header,j,bh,b]);fs.writeFileSync(path.join(root,'Valoria_Guard_Rigged_LOD.glb'),result);
 const proof={sourceSha256:sha(src),derivedSha256:sha(result),triangles:total,skinJoints:26,animationClips:d.animations.length,originalBinaryPrefix:true,meshes:stats,artApproved:false};
 fs.writeFileSync(path.join(root,'guard-lod-proof.json'),JSON.stringify(proof,null,2));console.log('SENDA_NATIVE_GUARD_LOD_PASS',JSON.stringify(proof));
})().catch(e=>{console.error(e);process.exit(1)});
