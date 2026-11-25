import * as THREE from 'three';
import {clone as cloneSkinned} from 'three/addons/utils/SkeletonUtils.js';
import {MeshoptSimplifier} from 'https://cdn.jsdelivr.net/npm/meshoptimizer@0.22.0/meshopt_simplifier.module.js';

// Derived from our own exact Aldric source. Do not alter the canonical hero.
// Retain texture coordinates, skin weights and original indices' vertex attributes.
function reduce(g,target){
 const n=g.attributes.position.count,pos=new Float32Array(n*3),pa=g.attributes.position;
 for(let i=0;i<n;i++){pos[i*3]=pa.getX(i);pos[i*3+1]=pa.getY(i);pos[i*3+2]=pa.getZ(i)}
 const ids=Uint32Array.from(g.index.array);
 let [indices]=MeshoptSimplifier.simplify(ids,pos,3,target*3,.10,[]);
 if(indices.length>target*4.5)[indices]=MeshoptSimplifier.simplify(ids,pos,3,target*3,.20,[]);
 const copy=g.clone();copy.setIndex(new THREE.BufferAttribute(indices,1));copy.computeBoundingSphere();return copy;
}
function helm(head,hostile){
 const metal=new THREE.MeshStandardMaterial({color:hostile?0x49414a:0x65717c,metalness:.65,roughness:.42});
 const trim=new THREE.MeshStandardMaterial({color:hostile?0xb16b40:0xbfa06c,metalness:.60,roughness:.45});
 const outline=[[-.07,.071],[-.045,.095],[.025,.10],[.075,.091],[.115,.061],[.14,.022],[.145,0]].map(([y,r])=>new THREE.Vector2(r,y));
 const helmet=new THREE.Mesh(new THREE.LatheGeometry(outline,16),metal);helmet.position.set(.018,.005,0);head.add(helmet);
 const rim=new THREE.Mesh(new THREE.TorusGeometry(.095,.006,6,24),trim);rim.rotation.x=Math.PI/2;rim.position.set(.018,.008,0);head.add(rim);
 const visor=new THREE.Mesh(new THREE.BoxGeometry(.018,.018,.13),new THREE.MeshStandardMaterial({color:0x111a20,roughness:1}));visor.position.set(.116,.025,0);head.add(visor);
 const nose=new THREE.Mesh(new THREE.BoxGeometry(.014,.057,.018),trim);nose.position.set(.126,-.002,0);head.add(nose);
 if(hostile)for(const side of [-1,1]){const horn=new THREE.Mesh(new THREE.ConeGeometry(.022,.10,7),trim);horn.position.set(.012,.075,side*.088);horn.rotation.x=side*.55;head.add(horn)}
}
function shield(chest,hostile){
 const sh=new THREE.Shape();sh.moveTo(-.14,.17);sh.lineTo(.14,.17);sh.lineTo(.13,-.02);sh.quadraticCurveTo(.10,-.16,0,-.23);sh.quadraticCurveTo(-.10,-.16,-.13,-.02);sh.closePath();
 const pivot=new THREE.Group();pivot.position.set(-.23,-.11,0);pivot.rotation.y=-Math.PI/2;chest.add(pivot);
 const a=new THREE.Mesh(new THREE.ExtrudeGeometry(sh,{depth:.014,bevelEnabled:true,bevelThickness:.008,bevelSize:.008,bevelSegments:1,steps:1}),new THREE.MeshStandardMaterial({color:0xbba06d,roughness:.50,metalness:.5}));pivot.add(a);
 const b=new THREE.Mesh(new THREE.ShapeGeometry(sh),new THREE.MeshStandardMaterial({color:hostile?0x6a2929:0x173d50,roughness:.75,metalness:.22}));b.position.z=.023;b.scale.set(.86,.87,1);pivot.add(b);
 const crest=new THREE.Mesh(new THREE.BoxGeometry(.045,.095,.006),new THREE.MeshStandardMaterial({color:0xd6b978,metalness:.45,roughness:.46}));crest.position.set(0,.02,.026);pivot.add(crest);
 const top=new THREE.Mesh(new THREE.BoxGeometry(.083,.025,.006),crest.material);top.position.set(0,.067,.026);pivot.add(top);
 for(let i=0;i<3;i++){const bit=new THREE.Mesh(new THREE.BoxGeometry(.017,.024,.006),crest.material);bit.position.set((i-1)*.029,.09,.026);pivot.add(bit)}
}
const qLeft=new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(1,0,0),-Math.PI*35/180);
const qRight=new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(1,0,0),Math.PI*35/180);
const iLeft=qLeft.clone().invert(),iRight=qRight.clone().invert();
export function correctArms(rig,undo=false){
 if(!rig.left||!rig.right)return;
 if(undo&&rig.poseCorrected){rig.left.quaternion.multiply(iLeft);rig.right.quaternion.multiply(iRight);rig.poseCorrected=false;}
 if(!undo&&!rig.poseCorrected){rig.left.quaternion.multiply(qLeft);rig.right.quaternion.multiply(qRight);rig.poseCorrected=true;}
}
export function equipWeapon(model,hero=false){
 const socket=model.getObjectByName('Eldoria_Aldric_weapon_R_socket');if(!socket)return;
 const wood=new THREE.MeshStandardMaterial({color:0x5b402b,roughness:.78}),iron=new THREE.MeshStandardMaterial({color:0xa4aab0,roughness:.39,metalness:.72}),gold=new THREE.MeshStandardMaterial({color:0xb99b63,roughness:.42,metalness:.55});
 const weapon=new THREE.Group();weapon.name=hero?'Valorian expedition sword':'Guard spear';socket.add(weapon);
 if(hero){
  const blade=new THREE.Mesh(new THREE.BoxGeometry(.028,.38,.009),iron);blade.position.y=.24;weapon.add(blade);
  const point=new THREE.Mesh(new THREE.ConeGeometry(.018,.055,4),iron);point.position.y=.457;weapon.add(point);
  const guard=new THREE.Mesh(new THREE.BoxGeometry(.12,.023,.022),gold);guard.position.y=.05;weapon.add(guard);
  const grip=new THREE.Mesh(new THREE.CylinderGeometry(.012,.012,.12,8),wood);grip.position.y=-.02;weapon.add(grip);
 }else{
  const shaft=new THREE.Mesh(new THREE.CylinderGeometry(.009,.009,.81,8),wood);shaft.position.y=.25;weapon.add(shaft);
  const socketBand=new THREE.Mesh(new THREE.CylinderGeometry(.014,.013,.055,8),gold);socketBand.position.y=.66;weapon.add(socketBand);
  const blade=new THREE.Mesh(new THREE.ConeGeometry(.031,.15,4),iron);blade.position.y=.74;weapon.add(blade);
 }
 weapon.rotation.z=.12;
}
export async function buildGuardArmy(source,walk,attack,squad,foes,fit){
 await MeshoptSimplifier.ready;if(!MeshoptSimplifier.supported)throw Error('Mesh simplifier unavailable');
 const base=cloneSkinned(source),kept=[];let triangles=0;
 base.traverse(o=>{
  if(!o.isSkinnedMesh)return;
  const target=o.name.includes('SirAldric_Tripo')?5200:o.name.includes('Cape_Main')?800:0;
  if(!target){o.visible=false;return}
  o.geometry=reduce(o.geometry,target);kept.push(o);triangles+=o.geometry.index.count/3;
 });
 if(kept.length!==2||triangles>12000)throw Error('Guard body/cape LOD budget: '+triangles+' meshes '+kept.length);
 const rigs=[];
 for(const hostile of [false,true]){
  const blueprint=cloneSkinned(base);
  blueprint.traverse(o=>{if(o.isSkinnedMesh&&o.visible){o.material=o.material.clone();o.material.color.multiply(new THREE.Color(hostile?0x97574c:0xaebfcd));if(o.name.includes('Cape_Main'))o.material.color.set(hostile?0x6b3330:0x214e68)}});
  const head=blueprint.getObjectByName('Eldoria_Aldric_head'),chest=blueprint.getObjectByName('Eldoria_Aldric_chest');
  if(!head||!chest)throw Error('Guard rig bones absent');helm(head,hostile);shield(chest,hostile);equipWeapon(blueprint,false);
  fit(blueprint,hostile?1.78:1.86);
  const units=hostile?foes:squad;
  for(let i=1;i<units.length;i++){
   const u=units[i];u.remove(u.userData.model);
   const model=cloneSkinned(blueprint),pivot=new THREE.Group();pivot.rotation.y=-Math.PI/2;u.add(pivot);pivot.add(model);
   const mixer=new THREE.AnimationMixer(model),w=mixer.clipAction(walk),a=mixer.clipAction(attack);w.play();
   const left=model.getObjectByName('Eldoria_Aldric_upperarm_L'),right=model.getObjectByName('Eldoria_Aldric_upperarm_R');
   rigs.push({unit:u,mixer,walk:w,attack:a,attackActive:false,lastTime:0,left,right});u.userData.guard=true;
  }
 }
 return {rigs,triangles};
}
export function updateGuardArmy(rigs,t,fighting){
 for(const g of rigs){
   if(!g.unit.visible)continue;
   if(fighting!==g.attackActive){g.attackActive=fighting;if(fighting){g.walk.stop();g.attack.reset().play()}else{g.attack.stop();g.walk.reset().play()}}
   const dt=Math.max(0,Math.min(.20,t-g.lastTime));g.lastTime=t;correctArms(g,true);g.mixer.update(dt);correctArms(g,false);
 }
}
