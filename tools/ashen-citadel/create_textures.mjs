// Original deterministic mobile tileable texture authoring: 128×128 PNG assets.
// Run from repo root: node tools/ashen-citadel/create_textures.mjs
import fs from "node:fs"; import path from "node:path";
const makeTextures=function makeTextures(){
 const paths={Stone:"AshenStone.png",Roof:"AshenRoof.png",Wood:"AshenWood.png",Moss:"AshenMoss.png",Metal:"AshenMetal.png",Cloth:"AshenCloth.png"};
 const W=128,H=128;
 function hash(x,y,z=0){
  let n=(Math.imul((x|0)+1009,374761393)+Math.imul((y|0)+829,668265263)+Math.imul((z|0)+3,2246822519))>>>0;
  n=(Math.imul(n^(n>>>13),1274126177))>>>0;return ((n^(n>>>16))>>>0)/4294967295;
 }
 function sample(kind,x,y){
   const grain=hash(Math.floor(x/5),Math.floor(y/6),3)*.07+hash(x,y,5)*.035;
   let v=.80+grain;
   if(kind==="Stone"){
     const row=Math.floor(y/32),shift=(row%2)*16;
     const xx=(x+shift+2*Math.sin(y*.19+row*2))%32,yy=y%32;
     const block=hash(Math.floor((x+shift)/32),row,11);
     // Broad brushed stone tones and softened irregular joints, not a wire grid.
     v=.82+block*.105+grain*.24+.024*Math.sin(x*.22+y*.13);
     if(xx<1.1||yy<1.1)v=.70+grain*.4;
     else if(xx>29.5||yy>29)v=.80+grain*.4;
     if(yy>3&&yy<6)v+=.025;
   } else if(kind==="Roof"){
     const row=Math.floor(y/16),xx=(x+(row%2)*16)%32,yy=y%16;
     v=.76+hash(Math.floor((x+(row%2)*16)/32),row,27)*.16+grain*.20;
     if(yy<2||xx<2)v=.62+grain*.5;
     if(yy>12)v+=.04;
   } else if(kind==="Wood"){
     const stripe=x%32,plank=Math.floor(x/32);
     v=.77+hash(plank,0,31)*.15+Math.sin(y*.35+x*.17)*.042+grain*.25;
     if(stripe<2||stripe>30)v=.42+grain*.5;
     if(stripe===10||stripe===22)v-=.06;
     if((y%48===6||y%48===7)&&(stripe===6||stripe===25))v=.43;
   } else if(kind==="Moss"){
     // Soft periodic painted meadow. A discrete hashed quilt produces
     // visible checkerboard patches when a large terrain cube is viewed at
     // 2.5D mobile scale; use continuous low-contrast variation instead.
     v=.85+.023*Math.sin(x*Math.PI/32)*Math.cos(y*Math.PI/32)+
       .018*Math.cos(x*Math.PI/64)*Math.cos(y*Math.PI/64)+
       (hash(x,y,63)-.5)*.020;
   } else if(kind==="Metal"){
     v=.84+.06*hash(1,y,88)+.035*Math.sin(x*.39);
     if(x%32<2)v=.72;
   } else if(kind==="Cloth"){
     v=.86+.04*hash(Math.floor(x/3),Math.floor(y/3),6);
     if(x%4===0||y%4===0)v-=.03;
     if(Math.abs(x-64)<2)v+=.02;
   }
   v=Math.max(.34,Math.min(.98,v));
   const l=Math.round(v*255);return[l,l,l,255];
 }
 function png(kind){
   const raw=new Uint8Array(H*(1+W*4));let i=0;
   for(let y=0;y<H;y++){raw[i++]=0;for(let x=0;x<W;x++){for(const v of sample(kind,x,y))raw[i++]=v;}}
   const blocks=[0x78,0x01];for(let k=0;k<raw.length;){const n=Math.min(65535,raw.length-k),final=k+n===raw.length;
     blocks.push(final?1:0,n&255,(n>>>8)&255,(~n)&255,((~n)>>>8)&255);
     for(let j=0;j<n;j++)blocks.push(raw[k+j]);k+=n;}
   let a=1,b=0;for(let x of raw){a=(a+x)%65521;b=(b+a)%65521;}
   let c=((b<<16)|a)>>>0;blocks.push((c>>>24)&255,(c>>>16)&255,(c>>>8)&255,c&255);
   const tab=[];for(let t=0;t<256;t++){let x=t;for(let k=0;k<8;k++)x=x&1?(0xedb88320^(x>>>1)):(x>>>1);tab[t]=x>>>0;}
   function u32(x){return[(x>>>24)&255,(x>>>16)&255,(x>>>8)&255,x&255];}
   function chunk(label,vals){const data=[...label].map(x=>x.charCodeAt(0)).concat(vals);let crc=0xffffffff;
     for(const x of data)crc=tab[(crc^x)&255]^(crc>>>8);return[...u32(vals.length),...data,...u32((crc^0xffffffff)>>>0)];}
   return Uint8Array.from([137,80,78,71,13,10,26,10,...chunk("IHDR",[...u32(W),...u32(H),8,6,0,0,0]),...chunk("IDAT",blocks),...chunk("IEND",[])]);
 }
 const out={};
 for(const [key,name] of Object.entries(paths))
   out["Unity/Assets/Eldoria/ExperimentalAshenCitadel/Textures/"+name]=png(key);
 return out;
};
for(const [dest,bytes] of Object.entries(makeTextures())){
 fs.mkdirSync(path.dirname(dest),{recursive:true});fs.writeFileSync(dest,Buffer.from(bytes));
 console.log(dest,bytes.length);
}
