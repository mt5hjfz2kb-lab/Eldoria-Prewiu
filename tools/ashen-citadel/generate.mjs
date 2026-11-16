// Deterministic authored 3D geometry, original artwork. Run with Node from project root.
import fs from "node:fs"; import path from "node:path";
const author=function author(){
 const dir="Unity/Assets/Eldoria/ExperimentalAshenCitadel/Meshes/";
 const palette={basalt:"#4D5557",stone:"#89877E",stoneLight:"#B9AC94",cut:"#86877E",iron:"#252A31",wood:"#715241",oak:"#987253",roof:"#34465D",slate:"#52667C",brass:"#C7A173",warm:"#E3B46A",banner:"#703A4E",plaster:"#B1A48F",soil:"#5A5C4F",moss:"#4B604C",leaf:"#294D48",leafLight:"#3D6054",violet:"#7C6196"};
 let mat=["# ASHEN CITADEL ART PALETTE | proprietary original mesh artwork."];
 for(const [id,hex] of Object.entries(palette)){const rgb=[1,3,5].map(n=>(parseInt(hex.slice(n,n+2),16)/255).toFixed(4));mat.push("newmtl "+id,"Ka .08 .08 .08","Kd "+rgb.join(" "),"Ks .06 .06 .06","Ns 10","illum 2","");}
 const outputs={[dir+"AshenPalette.mtl"]:mat.join("\n")};
 function mesh(name){
  const vertices=[],faces={},materials=Object.keys(palette);for(const id of materials)faces[id]=[];
  function poly(a,m){const inds=[];for(const pos of a){vertices.push(pos);inds.push(vertices.length);}faces[m||"stone"].push(inds);}
  function box(x,y,z,w,h,d,m="stone"){
   if(w<=0||h<=0||d<=0)throw Error("Invalid box");
   const x0=x-w/2,x1=x+w/2,z0=z-d/2,z1=z+d/2,y0=y<=.55?-.10:y,y1=y+h;
   const v=[[x0,y0,z0],[x1,y0,z0],[x1,y0,z1],[x0,y0,z1],[x0,y1,z0],[x1,y1,z0],[x1,y1,z1],[x0,y1,z1]];
   for(const ii of [[0,1,5,4],[1,2,6,5],[2,3,7,6],[3,0,4,7],[4,5,6,7],[3,2,1,0]])poly(ii.map(i=>v[i]).reverse(),m);
  }
  function prism(x,y,z,r,h,n=8,m="stone"){
   const topY=y+h;if(y<=.20)y=-.10;h=topY-y;
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
  function base(){ // No surrounding plinth: structural footings meet the continuous meadow.
   box(0,.07,-8.0,3.4,.10,5.7,"cut");for(let i=0;i<5;i++)box(0,.10+i*.11,-11+i*.41,3.8-i*.3,.19,.50,"stoneLight");
  }
  // Ashen identity: the surviving west watchtower, split-chevron stonework,
  // steep slate roofs and an inhabited lower hall; progression grows around it.
  function crest(x,y,z,size=.65){
    poly([[x,y+size,z],[x+size*.62,y,z],[x,y-size*.45,z],[x-size*.62,y,z]],"brass");
    poly([[x,y+size*.58,z-.02],[x+size*.24,y+.02,z-.02],[x,y-size*.2,z-.02],[x-size*.24,y+.02,z-.02]],"banner");
  }
  function buttress(x,z,h,w=.55){
    // Tapered stone shoulder instead of a narrow decorative vertical strip.
    const a=[[x-w,.2,z-.65],[x+w,.2,z-.65],[x+w*.55,.2,z+.35],[x-w*.55,.2,z+.35]],
          b=[[x-w*.45,h,z-.12],[x+w*.45,h,z-.12],[x+w*.45,h,z+.35],[x-w*.45,h,z+.35]];
    poly(a,"cut");poly([...b].reverse(),"stoneLight");
    for(let i=0;i<4;i++)poly([b[i],b[(i+1)%4],a[(i+1)%4],a[i]],i===0?"stoneLight":"stone");
  }
  function watchtower(level){
    const x=-2.15,z=2.7,h=4.9+level*.72;
    box(x,.2,z,3.0,h,3.35,"stone");
    for(let dx of [-1.42,1.42])buttress(x+dx,z-1.8,h*.70,.35);
    box(x,h-.35,z,3.32,.35,3.66,"stoneLight");
    box(x,h,z,3.48,.54,3.82,"cut");
    for(let dx of [-.9,.9])window(x+dx,h-1.6,z-1.74);
    // Tall four-plane hipped cap; distinct from the round corner turrets.
    const y=h+.55,rx=2.05,rz=2.20,top=y+2.4+level*.10;
    const A=[x-rx,y,z-rz],B=[x+rx,y,z-rz],C=[x+rx,y,z+rz],D=[x-rx,y,z+rz],
      E=[x,y+2.4+level*.10,z-.66],F=[x,y+2.4+level*.10,z+.66];
    poly([A,E,B],"roof");poly([B,E,F,C],"slate");poly([C,F,D],"roof");poly([D,F,E,A],"roof");
    box(x,top,z,.12,.20,1.75,"brass");
    crest(x,h+.03,z-1.94,.40);
    banner(x-.55,top-.15,z-.10);
    if(level>=4){
      // Projecting upper oriel makes the tower identifiable from the player camera.
      box(x,h-2.3,z-1.94,1.52,1.6,.72,"stoneLight");
      roof(x,h-.7,z-2.03,1.78,.95,.68);window(x,h-2.1,z-2.35);
    }
  }
  function castle(level){
   base();
   const w=level>=4?16.4:14.4,d=level>=4?14:12.1,wh=level>=4?3.8:3;
   if(level===1){
     for(let x of [-6.2,6.2])for(let z=-5.9;z<=5.9;z+=.38){box(x,.13,z,.35,2.15,.35,"wood");cone(x,2.28,z,.30,.48,4,"oak");}
     for(let z of [-5.9,5.9])for(let x=-5.8;x<=5.8;x+=.38){if(z===-5.9&&Math.abs(x)<2)continue;box(x,.13,z,.34,2.15,.35,"wood");cone(x,2.28,z,.29,.48,4,"oak");}
     door(0,.18,-6.0,2.20,2.25);
   }else{
     ring(w,d,wh);
     tower(-w/2,-d/2,.2,wh+.48,1.25,false);
     tower(w/2,-d/2,.2,wh+.82,1.18,true);
     if(level>=3){tower(-w/2,d/2,.2,wh+.22,1.12,false);tower(w/2,d/2,.2,wh+.15,.95,false);}
   }
   // The old tower stays west/back at every tier; the great hall grows beside it.
   watchtower(level);
   const hh=2.6+level*.46;
   hall(1.35,-.20,4.25+level*.18,5.20,hh,level);
   door(1.35,.32,-2.91,1.35,2.25);
   for(let x of [-.88,3.58])buttress(x,-2.95,hh*.84,.32);
   crest(1.35,hh-.25,-2.88,.46);
   if(level>=2){
     box(3.65,.22,3.9,2.6,2.15,2.5,"wood");roof(3.65,2.37,3.9,3.05,2.95,1.0);
   }
   if(level>=3){
     // Enclosed gallery with rhythmic supports, lower than the landmark tower.
     box(3.8,2.0,2.1,1.85,1.24,3.8,"plaster");roof(3.8,3.24,2.1,2.3,4.2,1.15);
     for(let z of [.5,2.1,3.7])box(4.5,.22,z,.3,2.1,.32,"stoneLight");
   }
   const front=level===1?-6:-d/2;
   for(let x of [-2.55,2.55])torch(x,1.6,front-.7);
   if(level>=4){
     // Deep gatehouse with side piers; entry remains visible rather than walled over.
     for(let x of [-2.0,2.0]){box(x,.2,front-.9,1.15,3.85,2.2,"stone");buttress(x,front-2.15,2.9,.48);}
     box(0,3.12,front-.9,5.35,1.20,2.2,"stoneLight");
     roof(0,4.32,front-.9,5.85,2.72,1.15);crest(0,3.72,front-2.04,.58);
   }
   if(level===5){
     // One vertical accent on the opposite rear wall balances, not duplicates, the keep.
     tower(6.6,5.5,.2,6.6,1.08,true);
     for(let x of [-5.5,5.3])banner(x,wh+.4,front+.15);
     for(let z of [-3.3,.3,3.9])buttress(-w/2-.10,z,wh*.87,.30);
   }
  }
  function aux(kind){
   // No display plinths: posts, walls and feet extend slightly into terrain.
   if(kind==="Sawmill"){
     // Open timber workshop; posts, saw bench and stacked sawn boards visible under roof.
     box(0,.18,1.15,3.5,2.30,.35,"wood");
     for(let x of [-1.7,1.7])for(let z of [-1.25,1.15]){box(x,.20,z,.25,2.55,.25,"oak");}
     roof(0,2.75,0,4.35,3.65,1.25);
     box(0,1.15,-.25,2.60,.22,.95,"oak");
     for(let x of [-1,1])box(x,.2,-.25,.20,.95,.65,"wood");
     for(let i=0;i<5;i++)box(-1.55+i*.70,.20,-2.1,.48,.30,1.45,i%2?"oak":"wood");
     for(let i=0;i<3;i++)box(2.1,.2+i*.26,.15,1.0,.22,2.55,"oak");
     crest(0,2.92,-1.84,.43);
     // Saw blade and timber frame give the workshop a readable working identity.
     box(.1,1.37,-.24,1.65,.40,.055,"iron");
     for(let i=0;i<9;i++)cone(-.62+i*.18,1.77,-.24,.055,.10,3,"iron");
     box(-1.75,2.22,-1.45,.1,.12,1.1,"oak");
     box(-1.75,1.65,-1.95,.72,.55,.08,"wood");crest(-1.75,1.86,-2.02,.24);
   }else if(kind==="Barracks"){
     box(0,.2,0,3.8,2.9,3.9,"stone");roof(0,3.1,0,4.4,4.45,1.55);door(0,.2,-1.98,1.45,2.3);
     for(let x of [-1.52,1.52])buttress(x,-2.05,2.65,.35);
     // Low fortified side watch platform, shield emblem, paired spear rack.
     box(2.05,.20,.65,1.3,3.6,2.6,"stone");battlements(2.05,3.8,-.65,1.5);battlements(2.05,3.8,1.95,1.5);
     crest(0,2.95,-2.06,.64);banner(-1.45,2.6,-1.55);
     box(1.18,3.15,1.1,.62,2.05,.68,"basalt");box(1.18,5.1,1.1,.82,.22,.88,"cut");
     for(let x of [-1.05,1.05]){box(x,1.15,-2.3,.42,.73,.1,"banner");crest(x,1.60,-2.38,.26);}
     for(let x of [-.65,-.22,.22,.65]){box(x,.18,-2.70,.055,1.7,.07,"wood");cone(x,1.88,-2.70,.105,.3,4,"iron");}
   }else if(kind==="Granary"){
     // Raised timber crib on stone feet; ventilation gap clearly visible.
     for(let x of [-1.35,1.35])for(let z of [-1.55,1.55]){prism(x,.05,z,.35,.95,6,"stone");prism(x,.85,z,.50,.16,6,"cut");}
     box(0,1.04,0,3.65,.22,4.1,"oak");box(0,1.26,0,3.4,1.85,3.85,"plaster");
     roof(0,3.11,0,4.15,4.65,1.9);
     for(let x of [-1.6,0,1.6])box(x,1.26,-1.98,.16,1.85,.18,"wood");
     for(let y of [1.42,2.85])box(0,y,-1.99,3.45,.13,.18,"wood");
     for(let x of [-.8,.8]){box(x,2.1,-2.01,.54,.53,.06,"iron");for(let y=2.12;y<2.64;y+=.14)box(x,y,-2.07,.62,.065,.09,"oak");}
     for(let i=0;i<4;i++)box(.15,.2+i*.22,-2.9+i*.25,1.10,.23,.38,"wood");
     crest(0,3.25,-1.99,.43);prism(2,.20,.8,.48,.82,9,"oak");
     roof(0,5.0,0,1.20,1.45,.45);box(0,4.6,0,.8,.5,1.05,"wood");
     for(let z of [-.6,1.65]){prism(2.15,.18,z,.42,.70,9,"oak");prism(2.15,.27,z,.44,.09,9,"iron");}
   }else if(kind==="Stoneworks"){
     // Low open mason's shelter and a timber lifting gantry identify its purpose.
     box(0,.2,.8,3.35,1.85,1.65,"cut");roof(0,2.05,.8,3.9,2.25,1.05);
     for(let x of [-1.65,1.65])box(x,.20,-1.5,.26,3.5,.28,"wood");
     box(0,3.5,-1.5,4.1,.32,.35,"oak");box(.5,1.25,-1.5,.06,2.3,.06,"iron");
     box(.5,.85,-1.5,.95,.65,.65,"stoneLight");
     for(let i=0;i<6;i++)box((i%3)*.8-1.25,.20+Math.floor(i/3)*.43,-2.45,.7,.40,.68,i%2?"stoneLight":"stone");
     crest(0,2.13,-.35,.45);
     prism(-1.25,.18,-3.1,.55,.26,8,"cut");box(-1.25,.48,-3.1,.55,.65,.6,"stoneLight");
     box(1.2,.95,-1.7,.8,.10,.18,"iron");
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
 for(const kind of ["Sawmill","Barracks","Granary","Stoneworks","Pine","Rock"]){
  if(kind==="Pine"){
    // This source is authored in Blender, not regenerated by the architecture tool.
    const text=fs.readFileSync(dir+"Ashen_Pine.obj","utf8"),ls=text.split("\n"),faces=ls.filter(x=>x.startsWith("f "));
    stats.Pine={vertices:ls.filter(x=>x.startsWith("v ")).length,polygons:faces.length,triangles:faces.reduce((a,f)=>a+f.trim().split(/\s+/).length-3,0)};continue;
  }
  const m=mesh("Ashen_"+kind);m.aux(kind);let result=m.finish();outputs[dir+`Ashen_${kind}.obj`]=result.obj;stats[kind]=result.stats;}
 outputs["tools/ashen-citadel/art-manifest.json"]=JSON.stringify({name:"Ashen Citadel",state:"GROUNDED_SETTLEMENT_CANDIDATE_NATIVE_PENDING",authorship:"Directed west watchtower / lower hall architecture and functional auxiliaries; original 18-color palette; geometry source code, Blender review and editable export",levels:["Old watch redoubt","Stone enclosure","Garrison ward","Gatehouse stronghold","Ashen watch citadel"],coordinateSystem:"X/Y(up)/Z; front is -Z; unit metres",stats,palette,reuse:"Original architecture selected after inventory inspection; no legacy Bastion and no mismatched productions",limits:["Unity import/render not yet certified","Gameplay and mobile performance not yet tested"]},null,2)+"\n";
 return outputs;
};
for (const [p,content] of Object.entries(author())){fs.mkdirSync(path.dirname(p),{recursive:true});fs.writeFileSync(p,content,"utf8");console.log(p,Buffer.byteLength(content));}
