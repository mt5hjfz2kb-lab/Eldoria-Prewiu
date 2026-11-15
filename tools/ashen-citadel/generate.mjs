// Deterministic authored 3D geometry, original artwork. Run with Node from project root.
import fs from "node:fs"; import path from "node:path";
const author=function author(){
 const dir="Unity/Assets/Eldoria/ExperimentalAshenCitadel/Meshes/";
 const palette={basalt:"#4D5557",stone:"#666961",stoneLight:"#AAA38F",cut:"#86877E",iron:"#252A31",wood:"#715241",oak:"#987253",roof:"#3F4550",slate:"#5E6570",brass:"#C7A173",warm:"#E3B46A",banner:"#783F49",plaster:"#B1A48F",soil:"#5A5C4F",moss:"#4B604C",leaf:"#294D48",leafLight:"#3D6054",violet:"#7C6196"};
 let mat=["# ASHEN CITADEL ART PALETTE | proprietary original mesh artwork."];
 for(const [id,hex] of Object.entries(palette)){const rgb=[1,3,5].map(n=>(parseInt(hex.slice(n,n+2),16)/255).toFixed(4));mat.push("newmtl "+id,"Ka .08 .08 .08","Kd "+rgb.join(" "),"Ks .06 .06 .06","Ns 10","illum 2","");}
 const outputs={[dir+"AshenPalette.mtl"]:mat.join("\n")};
 function mesh(name){
  const vertices=[],faces={},materials=Object.keys(palette);for(const id of materials)faces[id]=[];
  function poly(a,m){const inds=[];for(const pos of a){vertices.push(pos);inds.push(vertices.length);}faces[m||"stone"].push(inds);}
  function box(x,y,z,w,h,d,m="stone"){
   if(w<=0||h<=0||d<=0)throw Error("Invalid box");
   const x0=x-w/2,x1=x+w/2,z0=z-d/2,z1=z+d/2,y0=y,y1=y+h;
   const v=[[x0,y0,z0],[x1,y0,z0],[x1,y0,z1],[x0,y0,z1],[x0,y1,z0],[x1,y1,z0],[x1,y1,z1],[x0,y1,z1]];
   for(const ii of [[0,1,5,4],[1,2,6,5],[2,3,7,6],[3,0,4,7],[4,5,6,7],[3,2,1,0]])poly(ii.map(i=>v[i]).reverse(),m);
  }
  function prism(x,y,z,r,h,n=8,m="stone"){
   const B=[],T=[];for(let i=0;i<n;i++){let a=2*Math.PI*i/n;B.push([x+Math.cos(a)*r,y,z+Math.sin(a)*r]);T.push([x+Math.cos(a)*r,y+h,z+Math.sin(a)*r]);}
   poly(B,m);poly([...T].reverse(),m);
   for(let i=0;i<n;i++)poly([T[i],T[(i+1)%n],B[(i+1)%n],B[i]],m);
  }
  function cone(x,y,z,r,h,n=8,m="roof"){const top=[x,y+h,z];for(let i=0;i<n;i++){let a=i*2*Math.PI/n,b=(i+1)*2*Math.PI/n;poly([[x+Math.cos(a)*r,y,z+Math.sin(a)*r],[x+Math.cos(b)*r,y,z+Math.sin(b)*r],top].reverse(),m);}}
  function frustum(x,y,z,bottom,top,height,n=9,m="leaf"){
    const low=[],high=[];
    for(let i=0;i<n;i++){
      const theta=2*Math.PI*i/n;
      const jag=1+.035*Math.sin(i*4.7+n);
      low.push([x+Math.cos(theta)*bottom*jag,y+.08*Math.sin(i*2.1),z+Math.sin(theta)*bottom*jag]);
      high.push([x+Math.cos(theta)*top,y+height,z+Math.sin(theta)*top]);
    }
    poly(low,m);poly([...high].reverse(),m);
    for(let i=0;i<n;i++)poly([high[i],high[(i+1)%n],low[(i+1)%n],low[i]],m);
  }
  function roof(x,y,z,w,d,raise=1.5){
   const A=[x-w/2,y,z-d/2],B=[x+w/2,y,z-d/2],C=[x+w/2,y,z+d/2],D=[x-w/2,y,z+d/2],E=[x,y+raise,z-d/2],F=[x,y+raise,z+d/2];
   poly([D,F,E,A],"roof");poly([E,F,C,B],"roof");poly([E,B,A],"slate");poly([F,D,C],"slate");
   box(x,y+raise,z,.14,.12,d+.2,"brass");
   for(let u=1;u<4;u++){const xx=w*u/8,yy=y+raise*(1-u/4);
     box(x-xx,yy,z,.10,.065,d+.13,"slate");box(x+xx,yy,z,.10,.065,d+.13,"slate");}
  }
  function window(x,y,z){
   box(x,y,z,.55,.88,.09,"iron");
   box(x,y+.14,z-.06,.29,.55,.08,"warm");
   box(x,y+.77,z-.08,.69,.14,.19,"cut");
   box(x,y-.13,z-.08,.72,.13,.18,"stoneLight");
   box(x-.02,y+.08,z-.12,.05,.54,.06,"iron");
  }
  function door(x,y,z,w=1.5,h=2.3){
   box(x,y,z,w+.45,h+.2,.3,"stoneLight");
   box(x,y+.03,z-.23,w,h-.1,.2,"wood");
   for(let k=-2;k<=2;k++)box(x+k*(w-.18)/5,y+.10,z-.36,.055,h-.25,.065,"oak");
   for(let h0 of [.6,1.8])box(x,y+h0,z-.40,w-.12,.12,.11,"iron");
   for(let k of [-.35,.35])box(x+k,y+1.12,z-.49,.07,.08,.10,"brass");
  }
  function banner(x,y,z){
   box(x,y,z,.085,2.9,.09,"brass");box(x+.44,y+2.72,z,.92,.08,.09,"brass");
   poly([[x+.08,y+2.65,z-.04],[x+1.02,y+2.65,z-.04],[x+.96,y+1.12,z-.04],[x+.52,y+1.32,z-.04],[x+.08,y+1.03,z-.04]],"banner");
   box(x+.51,y+1.99,z-.09,.14,.56,.05,"brass");
  }
  function torch(x,y,z){box(x,y,z,.11,.52,.11,"iron");prism(x,y+.45,z,.19,.12,6,"brass");cone(x,y+.55,z,.21,.50,6,"warm");}
  function battlements(x,y,z,len,axis="x"){
   const count=Math.ceil(len/.9);for(let i=0;i<count;i+=2){const off=(i+.5-count/2)*(len/count);
      box(x+(axis==="x"?off:0),y,z+(axis==="z"?off:0),axis==="x"?.63:.40,.5,axis==="z"?.63:.4,"cut");}
  }
  function tower(x,z,y,h,r=1.2,withRoof=true){
   prism(x,y,z,r,h,8,"stone");prism(x,y+h-.45,z,r*1.14,.42,8,"stoneLight");
   prism(x,y+h-.08,z,r*1.16,.21,8,"cut");
   if(withRoof){cone(x,y+h+.14,z,r*1.28,2.2,8,"roof");prism(x,y+h+2.26,z,.08,.40,8,"brass");}
   else for(let j=0;j<8;j+=2){const a=2*Math.PI*j/8;box(x+Math.cos(a)*r,y+h+.07,z+Math.sin(a)*r,.38,.46,.38,"cut");}
   window(x,y+h*.43,z-r+.03);
  }
  function hall(x,z,w,d,height,level){
   box(x,.34,z,w,height,d,"stone");box(x,.34+height-.30,z,w+.3,.38,d+.3,"cut");
   for(let i=1;i<5;i++){const yy=.34+height*i/6;box(x,yy,z-d/2-.07,w+.1,.08,.12,i%2?"cut":"stoneLight");}
   for(let yy=1.15;yy<height-.3;yy+=1.8){for(let xx=x-w/2+1.2;xx<x+w/2-.4;xx+=1.65)window(xx,yy,z-d/2-.10);}
   roof(x,.34+height,z,w+.68,d+.77,1.45+level*.16);
   for(let xx of [x-w/2+.4,x+w/2-.4])box(xx,.34,z-d/2-.20,.24,height,.32,"stoneLight");
  }
  function ring(w,d,h){
   box(-w/2,.50,0,.68,h,d,"stone");box(w/2,.50,0,.68,h,d,"stone");
   box(0,.50,d/2,w,h,.70,"stone");
   const gap=3.5,sw=(w-gap)/2;
   for(let sx of [-1,1])box(sx*(gap/2+sw/2),.50,-d/2,sw,h,.70,"stone");
   battlements(0,.5+h,d/2,w);battlements(-w/2,.5+h,0,d,"z");battlements(w/2,.5+h,0,d,"z");
   battlements(-gap/2-sw/2,.5+h,-d/2,sw);
   battlements(gap/2+sw/2,.5+h,-d/2,sw);
   door(0,.48,-d/2-.37,2.8,2.65);
   box(0,3.26,-d/2-.59,4.7,.48,1.14,"stoneLight");
  }
  // Basalt-edged, walkable organic foundation, replacing impossible square pads.
  function contourPlate(rx,rz,bottom,top,n,seed){
    const high=[],low=[];
    for(let i=0;i<n;i++){
      const a=2*Math.PI*i/n;
      const jitter=1+.046*Math.sin(i*1.7+seed)+.028*Math.cos(i*2.9+seed*.8);
      high.push([Math.cos(a)*rx*jitter,top,Math.sin(a)*rz*jitter]);
      low.push([Math.cos(a)*rx*jitter*1.05,bottom,Math.sin(a)*rz*jitter*1.05]);
    }
    for(let i=0;i<n;i++)poly([high[i],high[(i+1)%n],low[(i+1)%n],low[i]],"basalt");
    for(let i=0;i<n;i++)poly([[0,top,0],high[(i+1)%n],high[i]],"moss");
  }
  function base(){contourPlate(10.3,9.1,-.24,.045,24,1.2);
   box(0,.07,-8.0,3.4,.10,5.7,"cut");for(let i=0;i<5;i++)box(0,.10+i*.11,-11+i*.41,3.8-i*.3,.19,.50,"stoneLight");
  }
  function castle(level){
   base();
   if(level===1){
     hall(0,1.4,5.3,5.8,3.7,1);door(0,.32,-1.51,1.5,2.45);
     for(let x of [-6.2,6.2])for(let z=-5.9;z<=5.9;z+=.38){box(x,.13,z,.35,2.15,.35,"wood");cone(x,2.28,z,.30,.48,4,"oak");}
     for(let z of [-5.9,5.9])for(let x=-5.8;x<=5.8;x+=.38){if(z===-5.9&&Math.abs(x)<2)continue;box(x,.13,z,.34,2.15,.35,"wood");cone(x,2.28,z,.29,.48,4,"oak");}
     door(0,.18,-6.0,2.20,2.25);
     for(let x of [-4.1,4.1]){box(x,.16,0,2.4,1.8,2.7,"wood");roof(x,1.96,0,2.8,3.13,1.1);torch(x,1.7,-2);}
   }else{
     const w=level>=4?16.4:14.4,d=level>=4?14.0:12.1,wh=level>=4?3.8:3.0;
     ring(w,d,wh);
     hall(0,1.4,6.0+Math.max(0,level-3)*.52,5.9+Math.max(0,level-3)*.48,4.0+Math.max(0,level-3)*.62,level);
     for(let sx of [-1,1])tower(sx*w/2,-d/2,.2,wh+(level-2)*.45,level>=4?1.38:1.12,true);
     if(level>=3){
       for(let sx of [-1,1])tower(sx*w/2,d/2,.2,wh+.32,1.07,level>=4);
       for(let sx of [-1,1]){
         const x=sx*4.05;box(x,.26,2.3,2.40,2.78+(level-3)*.24,3.2,"stone");
         roof(x,3.05+(level-3)*.24,2.3,2.75,3.5,1.1);
         window(x,1.33,.65);
       }
       banner(-5.4,wh+1.55,-d/2+.2);banner(5.2,wh+1.55,-d/2+.2);
     }else{
       box(-3.8,.23,1.9,2.3,2.05,3.25,"wood");roof(-3.8,2.28,1.9,2.8,3.55,1.05);
     }
     for(const sx of [-1,1])torch(sx*2.75,1.6,-d/2-.65);
     if(level>=4){
       box(0,4.20,1.35,4.2,3.18,4.4,"stoneLight");roof(0,7.38,1.35,4.75,4.98,2.18);
       for(let x of [-1.30,1.30])window(x,5.25,-1.01);
       box(0,.16,-d/2-1.5,4.2,2.1,2.1,"stone");
       box(0,2.26,-d/2-1.5,4.62,.32,2.35,"stoneLight");
       for(let x of [-1.6,1.6])box(x,.15,-d/2-1.8,.63,2.2,.75,"cut");
     }
     if(level>=5){
       for(let x of [-2.85,2.85]){
         prism(x,6.05,1.40,1.35,2.50,8,"stoneLight");
         prism(x,8.54,1.4,1.55,.28,8,"brass");
         cone(x,8.82,1.4,1.65,2.74,8,"roof");
         banner(x-1,9,-.1);
       }
       prism(0,9.3,1.4,.99,.56,8,"stoneLight");cone(0,9.85,1.4,1.13,2.24,8,"roof");
       for(let x of [-5.3,-2.2,2.2,5.3]){box(x,.25,-2.85,.50,4.45,.90,"cut");}
       // A single scarce violet sign preserves the atmosphere without neon clutter.
       box(0,7.30,-1.0,.48,.57,.11,"violet");
     }
   }
  }
  function aux(kind){
   if(kind!=="Pine"&&kind!=="Rock")contourPlate(3.15,3.00,-.15,.19,12,.70);
   if(kind==="Sawmill"){
     box(0,.2,0,3.7,2.3,3.1,"wood");roof(0,2.52,0,4.2,3.6,1.55);
     for(let i=0;i<6;i++)box(-1.8+i*.7,.16,-2.15,.53,.30,1.6,i%2?"oak":"wood");
     prism(1.8,.42,1.1,.95,.18,12,"iron");prism(1.8,.60,1.1,.21,1.2,8,"wood");
     for(let i=0;i<8;i++){let a=2*Math.PI*i/8;box(1.8+Math.cos(a)*.8,.62,1.1+Math.sin(a)*.8,.23,.23,.40,"oak");}
   }else if(kind==="Barracks"){
     box(0,.2,0,3.8,2.9,3.9,"stone");roof(0,3.1,0,4.2,4.35,1.7);door(0,.2,-1.98,1.45,2.3);
     for(let x of [-1.52,1.52])box(x,.2,-2.05,.28,2.82,.25,"cut");banner(1.4,2.65,-1.45);
   }else if(kind==="Granary"){
     box(0,.48,0,3.6,2.2,4.0,"plaster");roof(0,2.68,0,4.1,4.52,1.82);
     for(let x of [-1.5,-.5,.5,1.5])box(x,.30,-2.06,.15,2.31,.2,"wood");
     for(let x of [-1.2,1.2])prism(x,.05,1.95,.65,.83,8,"oak");
   }else if(kind==="Stoneworks"){
     box(0,.20,0,3.5,2.4,3.0,"cut");roof(0,2.6,0,3.9,3.4,1.2);
     for(let i=0;i<10;i++)box((i%4)*1.06-1.6,.16,-1.9-Math.floor(i/4)*.66,.82,.4,.54,i%2?"stoneLight":"stone");
     prism(-1.7,.2,1.3,.65,.78,8,"stoneLight");torch(-1.6,1.9,-1.45);
   }else if(kind==="Pine"){
     prism(0,0,0,.17,5.2,7,"wood");
     // Asymmetric overlapping boughs: irregular silhouette, no stacked lampshades.
     for(let tier=0;tier<7;tier++){
       const y=.8+tier*.59,r=1.65-tier*.18;
       const pts=[];const n=13;
       for(let k=0;k<n;k++){
         const a=k*2*Math.PI/n+tier*.71,rr=r*(.78+.22*Math.sin(k*7.1+tier*3.7)**2);
         pts.push([Math.cos(a)*rr+.11*Math.sin(tier*2),y+.20*Math.sin(k*3.9+tier),Math.sin(a)*rr]);
       }
       const tip=[.12*Math.sin(tier),y+1.5,.10*Math.cos(tier*1.3)];
       for(let k=0;k<n;k++){
         poly([pts[(k+1)%n],pts[k],tip],k%5===0?"leafLight":"leaf");
         poly([[0,y+.2,0],pts[k],pts[(k+1)%n]],"leaf");
       }
     }
   }else if(kind==="Rock"){
     // Hand-shaped faceted fieldstone; no masonry texture on natural geology.
     const low=[],mid=[],high=[];const n=9;
     for(let i=0;i<n;i++){
       const a=i*2*Math.PI/n,r=1.25+.24*Math.sin(i*3.2)+.14*Math.cos(i*2.3);
       low.push([Math.cos(a)*r,-.14,Math.sin(a)*r*.74]);
       mid.push([Math.cos(a)*r*1.03,.48+.17*Math.sin(i*2.1),Math.sin(a)*r*.8]);
       high.push([Math.cos(a)*r*.61-.18,.95+.15*Math.cos(i*1.7),Math.sin(a)*r*.51]);
     }
     for(let i=0;i<n;i++){
       const j=(i+1)%n;
       poly([mid[i],mid[j],low[j],low[i]],"basalt");
       poly([high[i],high[j],mid[j],mid[i]],"basalt");
       poly([[-.18,1.09,0],high[j],high[i]],"basalt");
     }

   }
  }
  function finish(){
   const text=["# Ashen Citadel · authored 3D mesh, not code-only mockup","mtllib AshenPalette.mtl","o "+name];
   for(const v of vertices)text.push("v "+v.map(n=>n.toFixed(4)).join(" "));
   let fcount=0;for(const key of materials){if(!faces[key].length)continue;text.push("usemtl "+key);for(const f of faces[key]){text.push("f "+f.join(" "));fcount++;}}
   return {obj:text.join("\n")+"\n",stats:{vertices:vertices.length,polygons:fcount,trianglesUpperBound:fcount*4}};
  }
  return {castle,aux,finish};
 }
 let stats={};
 for(let i=1;i<=5;i++){const m=mesh("AshenBastion_"+i);m.castle(i);let result=m.finish();outputs[dir+`AshenBastion_${i}.obj`]=result.obj;stats[`bastion${i}`]=result.stats;}
 for(const kind of ["Sawmill","Barracks","Granary","Stoneworks","Pine","Rock"]){const m=mesh("Ashen_"+kind);m.aux(kind);let result=m.finish();outputs[dir+`Ashen_${kind}.obj`]=result.obj;stats[kind]=result.stats;}
 outputs["tools/ashen-citadel/art-manifest.json"]=JSON.stringify({name:"Ashen Citadel",state:"FIRST_AUTHORING_PASS_UNITY_UNTESTED",authorship:"Original 3D polygon meshes and 18-color material palette; no previous Bastion reused",levels:["Timber redoubt","Stone watch","Fortified ward","Royal stronghold","Ash crown"],coordinateSystem:"X/Y(up)/Z; front is -Z; unit metres",stats,palette,reuse:"Original architecture selected after inventory inspection; no legacy Bastion and no mismatched productions",limits:["Unity import/render not yet certified","Gameplay and mobile performance not yet tested"]},null,2)+"\n";
 return outputs;
};
for (const [p,content] of Object.entries(author())){fs.mkdirSync(path.dirname(p),{recursive:true});fs.writeFileSync(p,content,"utf8");console.log(p,Buffer.byteLength(content));}
