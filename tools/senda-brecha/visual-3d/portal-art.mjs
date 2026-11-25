import * as THREE from 'three';
// Construct solid 3D masonry from the measured opaque contour. Original
// orthographic surface art is UV mapped to the actual front/back triangles;
// the sides are separate stone material and retain physical thickness.
export async function buildStudioPortals(gates,stone){
 const root=new URL('./art/',import.meta.url);
 const r=await fetch(new URL('senda-valoria-portal.mesh.json',root));if(!r.ok)throw Error('Portal contour missing');
 const d=await r.json();
 if(d.schema!==1||d.world_width!==2.04||d.world_height!==3.02||d.depth!==.42||d.outer.length<20||d.outer.length>300||d.opening.length<10||d.opening_clear_fraction<.97)throw Error('Invalid portal shape');
 const t=await fetch(new URL('senda-valoria-portal.png',root));if(!t.ok)throw Error('Portal art missing');const bytes=await t.arrayBuffer();
 const hash=Array.from(new Uint8Array(await crypto.subtle.digest('SHA-256',bytes)),x=>x.toString(16).padStart(2,'0')).join('');
 if(hash!==d.texture_sha256||hash!=='d748cef798a69403f3b49fc48725e0c0eb1524b299b1b963f7eac5a8f57d33d9')throw Error('Portal art digest mismatch');
 const url=URL.createObjectURL(new Blob([bytes],{type:'image/png'}));let tex;
 try{tex=await new THREE.TextureLoader().loadAsync(url)}finally{URL.revokeObjectURL(url)}
 tex.colorSpace=THREE.SRGBColorSpace;tex.anisotropy=4;
 const points=arr=>arr.map(([x,y])=>new THREE.Vector2(x*d.world_width,y*d.world_height));
 const shape=new THREE.Shape(points(d.outer));
 const solid=new THREE.ExtrudeGeometry(shape,{depth:d.depth,bevelEnabled:false,curveSegments:1,steps:1});
 solid.translate(0,0,-d.depth*.5);
 const pos=solid.attributes.position,uv=solid.attributes.uv,[x,y,w,h]=d.bounds;
 for(const group of solid.groups)if(group.materialIndex===0)for(let i=group.start;i<group.start+group.count;i++){
   uv.setXY(i,(x+(pos.getX(i)/d.world_width+.5)*(w-1))/1024,1-(y+(1-pos.getY(i)/d.world_height)*(h-1))/1024);
 }
 uv.needsUpdate=true;solid.computeBoundingBox();
 if(solid.boundingBox.max.x-solid.boundingBox.min.x>2.06||solid.boundingBox.min.y<-.01||solid.boundingBox.max.z-solid.boundingBox.min.z<.40)throw Error('Portal outside lane or thickness lost');
 const front=new THREE.MeshStandardMaterial({map:tex,alphaTest:.32,color:0xffffff,roughness:.86,metalness:.05});
 const side=new THREE.MeshStandardMaterial({map:stone,roughness:.96,color:0x70675e});
 const aperture=new THREE.ShapeGeometry(new THREE.Shape(points(d.opening)),1);
 const trimPoints=points(d.opening).map(p=>new THREE.Vector3(p.x,p.y,-.218));
 const trimGeo=new THREE.BufferGeometry().setFromPoints(trimPoints);
 for(const g of gates){
   g.baseFrame=g.group.children.filter(o=>o!==g.label.plane&&o!==g.ring&&o!==g.aura);
   const frame=new THREE.Mesh(solid,[front,side]);frame.name='Valoria carved masonry / actual extruded contour';
   const energy=new THREE.Mesh(aperture,g.panel.material);energy.position.z=.03;energy.name='Energy in measured aperture';
   const trim=new THREE.LineLoop(trimGeo,new THREE.LineBasicMaterial({color:g.ring.material.color}));trim.name='Live magical rim';
   g.studioFrame=[frame,energy,trim];g.studioTrim=trim;g.group.add(...g.studioFrame);
 }
 return {meshVertices:solid.attributes.position.count,openingWidth:d.opening_width_mid,width:d.world_width,depth:d.depth,assetHash:hash};
}
export function useStudioPortals(gates,on){
 for(const g of gates){
   if(!g.studioFrame)throw Error('Portal pair not loaded');
   for(const m of g.baseFrame)m.visible=!on;
   for(const m of g.studioFrame)m.visible=on;
 }
}
