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
   // Small vertical masonry arrises, no random vertex displacement.
   if(["stone","stoneLight","plaster","cut"].includes(m)&&w>.6&&d>.4&&h>.6){
     const b=Math.min(.13,w*.06,d*.06),foot=[[x0+b,z0],[x1-b,z0],[x1,z0+b],[x1,z1-b],[x1-b,z1],[x0+b,z1],[x0,z1-b],[x0,z0+b]];
     const low=foot.map(([xx,zz])=>[xx,y0,zz]),high=foot.map(([xx,zz])=>[xx,y1,zz]);
     poly(low,m);poly([...high].reverse(),m);
     for(let i=0;i<8;i++){const j=(i+1)%8;poly([high[i],high[j],low[j],low[i]],m);}return;
   }
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
  function roofShell(point,nu,nv,m,edge,thick=.09){
    const lower=p=>[p[0],p[1]-thick,p[2]];
    for(let i=0;i<nu;i++)for(let j=0;j<nv;j++){
      const a=point(i/nu,j/nv),b=point((i+1)/nu,j/nv),c=point((i+1)/nu,(j+1)/nv),d=point(i/nu,(j+1)/nv);
      poly([a,d,c,b],m);poly([lower(a),lower(b),lower(c),lower(d)],m==="roof"?"wood":m);
    }
    const rim=[];for(let i=0;i<nu;i++)rim.push(point(i/nu,0));for(let j=0;j<nv;j++)rim.push(point(1,j/nv));
    for(let i=nu;i>0;i--)rim.push(point(i/nu,1));for(let j=nv;j>0;j--)rim.push(point(0,j/nv));
    for(let i=0;i<rim.length;i++){const a=rim[i],b=rim[(i+1)%rim.length];poly([a,b,lower(b),lower(a)],edge);}
  }
  function roof(x,y,z,w,d,raise=1.5,m="roof"){
    // Unequal ridge and slightly swept eaves form one continuous roof shell.
    const point=(u,v)=>{const q=u*2-1,a=Math.abs(q);return[x+w*q/2+w*.035*(1-a),y+raise*(1-a-.20*Math.sin(Math.PI*a))-.055*Math.sin(Math.PI*v),z+d*(v-.5)];};
    roofShell(point,8,4,m,m==="roof"?"slate":"wood",.11);
    for(let j=0;j<4;j++){const a=point(.5,j/4),b=point(.5,(j+1)/4);a[1]+=.055;b[1]+=.055;beam(a,b,.075,m==="roof"?"brass":"wood");}
    // Closed gable cheeks follow the shell, rather than leaving roof pieces floating.
    for(const v of [0,1])for(let i=0;i<8;i++){
      const a=point(i/8,v),b=point((i+1)/8,v),c=[b[0],y-.11,b[2]],d=[a[0],y-.11,a[2]];
      poly(v===0?[d,c,b,a]:[a,b,c,d],m==="roof"?"slate":"wood");
    }
  }
  function profileRound(x,y,z,profile,n,m){
    const rings=profile.map(([yy,r])=>Array.from({length:n},(_,i)=>[x+Math.cos(i*2*Math.PI/n)*r,y+yy,z+Math.sin(i*2*Math.PI/n)*r]));
    poly(rings[0],m==="roof"?"wood":m);poly([...rings.at(-1)].reverse(),m);
    for(let k=1;k<rings.length;k++)for(let i=0;i<n;i++){const j=(i+1)%n;poly([rings[k][i],rings[k][j],rings[k-1][j],rings[k-1][i]],m);}
  }
  function window(x,y,z){
   box(x,y,z,.55,.88,.09,"iron");
   box(x,y+.14,z-.06,.29,.55,.08,"warm");
   box(x,y+.77,z-.08,.69,.14,.19,"cut");
   box(x,y-.13,z-.08,.72,.13,.18,"stoneLight");
   box(x-.02,y+.08,z-.12,.05,.54,.06,"iron");
  }
  function extrudeFront(points,front,back,m){
    const a=points.map(([x,y])=>[x,y,front]),b=points.map(([x,y])=>[x,y,back]);
    poly([...a].reverse(),m);poly(b,m);
    for(let i=0;i<a.length;i++){const j=(i+1)%a.length;poly([a[i],a[j],b[j],b[i]],m);}
  }
  function door(x,y,z,w=1.5,h=2.3){
    const spring=y+h*.70,rr=w/2,rise=h*.30;
    for(const side of [-1,1])box(x+side*(rr+.13),y,z,.26,h*.70,.35,"stoneLight");
    for(let i=0;i<9;i++){
      const a=Math.PI*i/9+.012,b=Math.PI*(i+1)/9-.012;
      const p=t=>[x+Math.cos(t)*(rr+.27),spring+Math.sin(t)*(rise+.27)];
      const q=t=>[x+Math.cos(t)*rr,spring+Math.sin(t)*rise];
      extrudeFront([q(a),p(a),p(b),q(b)],z-.20,z+.18,i%3===0?"cut":"stoneLight");
    }
    const outline=[[x-rr,y],[x+rr,y]];
    for(let i=0;i<=12;i++){const a=Math.PI*i/12;outline.push([x+Math.cos(a)*rr,spring+Math.sin(a)*rise]);}
    extrudeFront(outline,z-.26,z-.08,"wood");
    for(let k=-2;k<=2;k++){
      const xx=k*(w-.18)/5,top=spring+rise*Math.sqrt(Math.max(0,1-xx*xx/(rr*rr)));
      box(x+xx,y+.10,z-.32,.045,top-y-.15,.055,"oak");
    }
    for(let t of [.25,.58])box(x,y+h*t,z-.36,w-.12,.10,.09,"iron");
    for(let k of [-.20,.20])box(x+k,y+h*.45,z-.43,.07,.10,.09,"brass");
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
   prism(x,y,z,r,h,12,"stone");prism(x,y+h-.45,z,r*1.14,.42,12,"stoneLight");
   prism(x,y+h-.08,z,r*1.16,.21,12,"cut");
   if(withRoof){profileRound(x,y+h+.14,z,[[0,r*1.28],[.18,r*1.16],[.75,r*.73],[1.45,r*.35],[2.2,.025]],12,"roof");prism(x,y+h+2.26,z,.08,.40,8,"brass");}
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
  // Craft architecture uses exposed joints and work equipment, not miniature keeps.
  function beam(a,b,r,m="wood",n=4,end=m){
    const d=b.map((v,i)=>v-a[i]),len=Math.hypot(...d);for(let i=0;i<3;i++)d[i]/=len;
    let u=Math.abs(d[1])>.98?[1,0,0]:[-d[2],0,d[0]];const ul=Math.hypot(...u);u=u.map(v=>v/ul);
    const v=[d[1]*u[2]-d[2]*u[1],d[2]*u[0]-d[0]*u[2],d[0]*u[1]-d[1]*u[0]];
    const rings=[a,b].map(p=>Array.from({length:n},(_,i)=>p.map((q,j)=>q+r*(u[j]*Math.cos(2*Math.PI*i/n+Math.PI/4)+v[j]*Math.sin(2*Math.PI*i/n+Math.PI/4)))));
    poly([...rings[0]].reverse(),end);poly(rings[1],end);
    for(let i=0;i<n;i++){let j=(i+1)%n;poly([rings[0][i],rings[0][j],rings[1][j],rings[1][i]],m);}
  }
  function shed(x,y,z,w,d,rise,m="wood"){
    const cloth=m==="banner",sag=cloth?.28:.12;
    const point=(u,v)=>[x+w*(u-.5),y+rise*v-sag*Math.sin(Math.PI*u)*(.35+.65*Math.sin(Math.PI*v)),z+d*(v-.5)];
    roofShell(point,6,4,m,cloth?"banner":"wood",cloth?.025:.10);
    for(let j=0;j<4;j++){beam(point(0,j/4),point(0,(j+1)/4),.09,"wood");beam(point(1,j/4),point(1,(j+1)/4),.09,"wood");}
    // Cloth hangs between supports; wooden roofs have two integrated battens.
    for(const v of cloth?[0,1]:[0,.40,.75,1])for(let i=0;i<6;i++)beam(point(i/6,v),point((i+1)/6,v),cloth?.025:.055,cloth?"iron":"oak");
  }
  function barrel(x,z,r=.4,h=.8){prism(x,.08,z,r,h,10,"oak");for(let y of [.22,h*.75])prism(x,y,z,r+.025,.075,10,"iron");}
  function aux(kind){
   // No display plinths: posts, walls and feet extend slightly into terrain.
   if(kind==="Sawmill"){
     // Long open sawing bay with a high rear roof and a lower working canopy.
     for(let x of [-2.45,2.45])for(let z of [-.8,1.8])box(x,.12,z,.25,2.82,.27,"wood");
     box(0,.12,1.8,.25,3.56,.27,"wood");
     box(0,.15,1.85,5.2,2.6,.18,"oak");
     roof(0,2.9,.65,5.7,3.45,.85,"wood");
     const bow=[[-2.45,2.65,-.8],[-1.7,3.0,-.8],[-.8,3.40,-.8],[.2,3.48,-.8],[1.15,3.22,-.8],[2.45,2.65,-.8]];
     for(let i=1;i<bow.length;i++)beam(bow[i-1],bow[i],.13,"wood");shed(-.35,2.35,-1.7,4.9,1.55,.5,"oak");
     for(let x of [-2.45,2.45]){box(x,.12,-2.4,.22,2.35,.22,"wood");beam([x,1.6,-2.4],[x,2.6,-1.4],.11,"oak");}
     for(let x of [-1.8,1.8])box(x,.08,-.2,.30,1.0,.8,"wood");box(0,1.0,-.2,4.4,.25,1.05,"oak");
     beam([-1.9,1.48,-.2],[1.7,1.48,-.2],.30,"wood",10,"oak");
     for(let i=0;i<3;i++)beam([-2.1+i*.62,.32,-3.05],[-2.1+i*.62,.32,-1.45],.26,"wood",9,"oak");
     for(let i=0;i<3;i++)box(2.2,.12+i*.20,-1.8,1.1,.17,2.0,"oak");
     box(.6,1.28,-.2,.045,1.15,1.15,"iron");
     beam([-.65,1.5,-.84],[1.45,1.5,-.84],.075,"wood");
     box(-2.5,2.45,-2.3,1.0,.65,.09,"wood");crest(-2.5,2.74,-2.39,.30);
   }else if(kind==="Barracks"){
     // Inhabited garrison longhouse, low stone base and timber upper storey.
     box(-.45,.10,.55,5.15,1.15,3.7,"stone");box(-.45,1.25,.55,5.15,1.55,3.7,"plaster");
     for(let x of [-2.92,-1.25,.45,2.02])box(x,1.16,-1.34,.15,1.7,.16,"wood");
     box(-.45,1.25,-1.35,5.2,.16,.18,"wood");box(-.45,2.67,-1.35,5.2,.18,.18,"wood");
     roof(-.45,2.8,.55,5.75,4.25,1.30);door(-.9,.10,-1.44,1.2,2.0);
     for(let x of [-2.10,.65])window(x,1.6,-1.44);
     // Lower entrance porch and a projecting wing replace the tower silhouette.
     box(2.0,.10,-1.25,1.5,2.05,2.6,"wood");shed(2.0,2.05,-1.25,1.95,3.0,.30,"slate");
     for(let x of [-1.7,-.1])box(x,.10,-2.55,.18,2.0,.18,"wood");shed(-.9,2.05,-2.1,2.1,1.6,.45,"banner");
     box(-1.8,2.8,1.05,.65,1.8,.7,"basalt");box(-1.8,4.55,1.05,.88,.2,.93,"cut");
     banner(1.25,1.9,-1.4);crest(-.9,2.65,-1.55,.48);
     // Practice target and arms rack stand on earth, not on a courtyard slab.
     box(-2.7,.10,-2.65,.16,1.5,.18,"wood");
     beam([-2.7,1.3,-2.72],[-2.7,1.3,-2.85],.48,"oak",12,"oak");
     beam([-2.7,1.3,-2.86],[-2.7,1.3,-2.91],.24,"banner",12,"banner");
     for(let x of [.25,.55,.85]){box(x,.10,-2.85,.05,1.55,.06,"wood");cone(x,1.65,-2.85,.09,.22,4,"iron");}
   }else if(kind==="Granary"){
     // Broad low storehouse with a tall timber silo: the silhouette reads agriculture.
     for(let x of [-2.2,.25])for(let z of [-1.25,1.4])prism(x,.05,z,.30,.65,6,"stone");
     box(-.95,.65,.1,3.2,.2,3.45,"oak");box(-.95,.85,.1,3.0,1.85,3.2,"plaster");
     shed(-.95,2.75,.1,3.65,3.85,1.0,"wood");
     for(let x of [-2.38,-.95,.48])box(x,.85,-1.55,.14,1.9,.18,"wood");
     for(let y of [1.0,2.4])box(-.95,y,-1.55,3.0,.13,.17,"oak");
     box(-.95,.85,-1.64,1.0,1.5,.12,"wood");
     for(let i=0;i<3;i++)box(-.95,.10+i*.24,-2.8+i*.28,1.35,.26,.42,"wood");
     prism(1.8,.08,.75,1.04,.55,12,"stone");
     const profile=[[0,.95],[.55,1.08],[1.85,1.12],[3.1,1.04],[3.95,.92]];
     profileRound(1.8,.63,.75,profile,14,"oak");
     for(const [yy,r] of profile.slice(1))prism(1.8,.63+yy-.05,.75,r+.025,.10,14,"iron");
     profileRound(1.8,4.58,.75,[[0,1.24],[.17,1.19],[.48,.88],[.95,.38],[1.35,.045]],14,"wood");
     prism(1.8,5.90,.75,.06,.20,6,"iron");
     for(let i=0;i<7;i++)box(1.8,.55+i*.47,-.39,.65,.08,.12,"wood");
     for(let x of [1.42,2.18])box(x,.10,-.39,.08,3.7,.09,"wood");
     barrel(2.35,-1.7,.44,.88);barrel(1.25,-2.05,.34,.65);
     crest(-.95,2.55,-1.69,.33);
   }else if(kind==="Stoneworks"){
     // Open masonry yard: stone bins, canvas shelter and a braced lifting derrick.
     box(-1.3,.10,.9,2.4,1.05,2.0,"basalt");
     for(let x of [-2.6,-.05])box(x,.10,1.7,.18,2.55,.20,"wood");
     for(let x of [-2.6,-.05])box(x,.10,-.7,.18,2.1,.20,"wood");
     shed(-1.3,2.15,.5,3.0,2.9,.5,"banner");
     box(1.7,.10,.4,.36,4.15,.4,"wood");
     beam([1.7,.20,-.9],[1.7,3.0,.4],.16,"oak");beam([.35,.20,.4],[1.7,2.8,.4],.16,"oak");
     beam([1.7,4.1,1.4],[1.7,3.65,-2.5],.19,"wood");
     beam([1.7,4.1,.4],[1.7,3.65,-2.1],.045,"iron");
     box(1.7,1.25,-2.1,.05,2.45,.05,"iron");box(1.7,.65,-2.1,.85,.60,.85,"stoneLight");
     for(let i=0;i<7;i++)box(-2.15+(i%3)*.73,.10+Math.floor(i/3)*.43,-2.15,.65,.41,.8,i%3===0?"basalt":"cut");
     box(-.45,.10,-2.95,1.45,.65,.72,"stoneLight");
     box(-.55,.8,-2.95,.7,.09,.13,"wood");box(-.2,.75,-2.95,.22,.25,.24,"iron");
     crest(-1.3,2.2,-1.02,.33);
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
 outputs["tools/ashen-citadel/art-manifest.json"]=JSON.stringify({name:"Ashen Citadel",state:"ORGANIC_ARCHITECTURE_CANDIDATE_NATIVE_PENDING",authorship:"Directed west watchtower / lower hall architecture and functional auxiliaries; original 18-color palette; geometry source code, Blender review and editable export",levels:["Old watch redoubt","Stone enclosure","Garrison ward","Gatehouse stronghold","Ashen watch citadel"],coordinateSystem:"X/Y(up)/Z; front is -Z; unit metres",stats,palette,reuse:"Original architecture selected after inventory inspection; no legacy Bastion and no mismatched productions",limits:["Unity import/render not yet certified","Gameplay and mobile performance not yet tested"]},null,2)+"\n";
 return outputs;
};
for (const [p,content] of Object.entries(author())){fs.mkdirSync(path.dirname(p),{recursive:true});fs.writeFileSync(p,content,"utf8");console.log(p,Buffer.byteLength(content));}
