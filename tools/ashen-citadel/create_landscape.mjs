// Original Ashen Citadel sculpted heightfield + forest-edge decorations (real OBJ polygons).
// Usage: node tools/ashen-citadel/create_landscape.mjs. The result is physically
// imported into Blender, can be edited in Blender, and then Unity URP.
import fs from "node:fs";
const makeLandscape=function makeLandscape(){
  const lines=["# ASHEN CITADEL · authored terrain, paths, indigenous undergrowth and rocks","mtllib AshenPalette.mtl","o AshenLandscape"];
  let vs=0, fs=0;
  const byMat={};
  let seed=28811;
  function rand(){seed=(Math.imul(seed,1664525)+1013904223)>>>0;return seed/4294967296;}
  function noise(x,z){return Math.sin(x*.41+Math.cos(z*.18))*Math.cos(z*.47)*.5 + Math.sin(x*.11+z*.24)*.38 + Math.sin(x*.82-z*.60)*.12;}
  function height(x,z){
    let rad=Math.hypot(x*.85,z),s=Math.max(0,Math.min(1,(rad-18)/29));
    let lev=(noise(x,z)*.55+Math.sin(x*.12-z*.14)*.35)*s + s*s*2.2;
    return -.060+lev;
  }
  function emit(p,mat,uv){
    let idx=[];
    for(let i=0;i<p.length;i++){
      vs++;idx.push(vs);
      let v=p[i];lines.push(`v ${v[0].toFixed(4)} ${v[1].toFixed(4)} ${v[2].toFixed(4)}`);
      let coords=uv?uv[i]:[v[0]*.46,v[2]*.46];
      byMat[mat]??=[];
      byMat[mat].push({vertex:vs,uv:coords});
    }
    // uv later per-vertex in strict vertex order by recording an array
    return idx;
  }
  let coordUV=[null];const faces=[];
  function tri(a,b,c,mat,forceUV){
    const idx=[];
    for(const v of [a,b,c]){
      lines.push("v "+v.map(q=>q.toFixed(4)).join(" "));
      vs++; idx.push(vs);
      const uv=forceUV ? forceUV[idx.length-1] : [v[0]*.20,v[2]*.20];
      coordUV.push(uv);
    }
    faces.push([mat,idx]);fs++;
  }
  const extent=90,cell=3.6,n=50;
  for(let i=0;i<n;i++)for(let j=0;j<n;j++){
    const x=-extent+i*cell,z=-extent+j*cell;
    const p=(dx,dz)=>[x+dx,height(x+dx,z+dz),z+dz];
    const a=p(0,0),b=p(cell,0),c=p(cell,cell),d=p(0,cell);
    const xc=x+cell/2,zc=z+cell/2;
    const pathCenter=Math.sin(zc*.12)*.70;
    const route=(zc<-6 && zc>-36 && Math.abs(xc-pathCenter)<3.1);
    const material="moss";
    tri(a,c,b,material);tri(a,d,c,material);
  }
  // Footpath is a continuous hand-curved 3d ribbon with broken edges, no rectangular tiles.
  for(let k=0;k<32;k++){
    const p=k/32,q=(k+1)/32;
    const path=(t,dir)=>{
      const z=-8-34*t,x=Math.sin((z+8)*.15)*.66 + Math.sin((z+8)*.52)*.17;
      const width=2.05+.12*Math.sin(t*17)+.15*Math.sin(t*46);
      const xx=x+dir*width, yy=height(xx,z)+.045;
      return [xx,yy,z];
    };
    const a=path(p,-1),b=path(p,1),c=path(q,1),d=path(q,-1);
    tri(a,b,c,"soil");tri(a,c,d,"soil");
  }
  // Sculpted grassy hummocks and beds of undergrowth, strongly less repetitive
  // than the tree-ring algorithm; all triangles are actual 3D geometry.
  const excluded=(x,z)=> (Math.abs(x)<11&&Math.abs(z)<11)|| (Math.abs(x)<4.5&&z<0&&z>-42)||
    [[-17.2,-6.2],[16.8,-7.5],[-16.6,7.8],[17.4,8.8]].some(([a,b])=>Math.abs(x-a)<4.7&&Math.abs(z-b)<4.7);
  let undergrowth=0;
  for(let i=0;i<325;i++){
    const x=(rand()-.5)*86,z=(rand()-.5)*78;
    if(excluded(x,z))continue;
    const rr=.22+.48*rand(),yy=height(x,z);
    const count=3+(i%3);
    for(let k=0;k<count;k++){
      const a=k*2.39996+rand()*.3,h=rr*(.7+rand()*.7);
      const mid=[x+Math.cos(a)*.09,yy+h,z+Math.sin(a)*.09];
      const v1=[x+Math.cos(a-.45)*rr*.4,yy-.009,z+Math.sin(a-.45)*rr*.4],
            v2=[x+Math.cos(a+.45)*rr*.4,yy-.009,z+Math.sin(a+.45)*rr*.4];
      tri(v1,mid,v2,i%7===0?"leafLight":"leaf");undergrowth++;
    }
  }
  // Art-directed pockets of low shrubs, never an even scatter or a ring of plots.
  const pockets=[[-12.7,4.2],[-11.9,-6.7],[11.9,6.4],[12.3,-4.8],[-20.7,1.2],[21.3,1.1],[-8.5,-13.5],[7.2,-15.5],[-21,14],[20,16],[-26,-14],[27,-11]];
  for(let k=0;k<pockets.length;k++){
    const [px,pz]=pockets[k];
    for(let b=0;b<6;b++){
      const x=px+(rand()-.5)*3.1,z=pz+(rand()-.5)*2.1,y=height(x,z);
      const rx=.28+rand()*.40,rz=.25+rand()*.35,h=.22+rand()*.40;
      // Small overlapping leaves, not closed faceted domes that read as green rocks.
      for(let j=0;j<22;j++){
        const theta=j*2.39996,ring=Math.sqrt(rand()),xx=x+Math.cos(theta)*rx*ring,zz=z+Math.sin(theta)*rz*ring;
        const yy=y+.10+h*(1-ring*.7)+rand()*.07;
        const len=.13+rand()*.16,w=.055+rand()*.075;
        const ux=Math.cos(theta),uz=Math.sin(theta),vx=-uz,vz=ux;
        const base=[xx-ux*len,yy-.045,zz-uz*len],tip=[xx+ux*len,yy+.07,zz+uz*len];
        const left=[xx+vx*w,yy+.025,zz+vz*w],right=[xx-vx*w,yy+.025,zz-vz*w];
        const tone=j%7===0?"leafLight":"leaf";tri(base,left,tip,tone);tri(base,tip,right,tone);
      }
      for(let j=0;j<5;j++){
        const a=j*2.4,xx=x+Math.cos(a)*rx*1.4,zz=z+Math.sin(a)*rz*1.4;
        tri([xx-.04,y,zz],[xx+.07,y+.22+rand()*.18,zz+.04],[xx+.06,y,zz+.08],"leafLight");undergrowth++;
      }
    }
  }
  // Broken flagstones disappear into the approach rather than outlining a platform.
  for(let i=0;i<34;i++){
    const z=-12-rand()*18,x=(rand()-.5)*3.7+Math.sin(z*.15)*.5,y=height(x,z)+.072;
    const w=.12+rand()*.25,d=.10+rand()*.25;
    const a=[x-w,y,z-d],b=[x-w*.7,y,z+d],c=[x+w,y,z+d*.6],e=[x+w*.8,y,z-d*.9];
    tri(a,b,c,i%4===0?"basalt":"cut");tri(a,c,e,i%4===0?"basalt":"cut");
  }
  // Strata: natural basalt outcrops, mesh (not random cubes) with stone palettes.
  let stones=0;
  for(let i=0;i<70;i++){
    const x=(rand()-.5)*94,z=(rand()-.5)*91;
    if(excluded(x,z))continue;
    const r=.32+rand()*.80,ry=.18+rand()*.38,y=height(x,z)-.06;
    const nside=6+(i%3),base=[],top=[];
    for(let k=0;k<nside;k++){
      const a=k/nside*2*Math.PI,rx=Math.cos(a)*r,rz=Math.sin(a)*r*.7;
      base.push([x+rx,y,z+rz]);
      top.push([x+rx*.64,y+ry,z+rz*.64]);
    }
    const mat=i%3===0?"stone":i%3===1?"cut":"basalt";
    for(let k=0;k<nside;k++){
      const j=(k+1)%nside;
      tri(top[j],base[j],base[k],mat);
      tri(top[k],top[j],base[k],mat);
      tri([x,y+ry*1.40,z],top[j],top[k],i%2?"stone":mat);
    }
    stones++;
  }
  // Need uv array after all vertices but before material sections.
  const vnLines=coordUV.slice(1).map(u=>`vt ${u[0].toFixed(4)} ${u[1].toFixed(4)}`);
  lines.push(...vnLines);
  let current="";
  for(const [mat,inds] of faces){
    if(mat!==current){lines.push("usemtl "+mat);current=mat;}
    lines.push("f "+inds.map(i=>`${i}/${i}`).join(" "));
  }
  return {obj:lines.join("\n")+"\n",stats:{vertices:vs,faces:fs,triangles:fs,undergrowth_blades:undergrowth,boulders:stones,materials:[...new Set(faces.map(x=>x[0]))].sort()}};
};
const result=makeLandscape();
const dst="Unity/Assets/Eldoria/ExperimentalAshenCitadel/Meshes/AshenLandscape.obj";
fs.writeFileSync(dst,result.obj);fs.writeFileSync("tools/ashen-citadel/landscape-manifest.json",JSON.stringify(result.stats,null,2)+"\n");
console.log("ASHEN_LANDSCAPE_AUTHORED",JSON.stringify(result.stats));
