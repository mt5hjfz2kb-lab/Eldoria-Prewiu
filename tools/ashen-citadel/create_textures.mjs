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
     const row=Math.floor(y/32),shift=(row%2)*16,xx=(x+shift)%32,yy=y%32;
     const block=hash(Math.floor((x+shift)/32),row,11);
     v=.80+block*.13+grain*.44;
     if(xx<=1||yy<=1)v=.57+grain*.5;
     else if(xx>=29||yy>=29)v=.73+grain;
     if(hash(x,y,20)>.991)v=.65;
   } else if(kind==="Roof"){
     const row=Math.floor(y/16),xx=(x+(row%2)*16)%32,yy=y%16;
     v=.73+hash(Math.floor((x+(row%2)*16)/32),row,27)*.17+grain*.32;
     if(yy<2||xx<2)v=.49+grain*.8;
     if(yy>12)v+=.04;
   } else if(kind==="Wood"){
     const stripe=x%32,plank=Math.floor(x/32);
     v=.77+hash(plank,0,31)*.15+Math.sin(y*.35+x*.17)*.042+grain*.25;
     if(stripe<2||stripe>30)v=.42+grain*.5;
     if(stripe===10||stripe===22)v-=.06;
     if((y%48===6||y%48===7)&&(stripe===6||stripe===25))v=.43;
   } else if(kind==="Moss"){
     v=.78+hash(Math.floor(x/7),Math.floor(y/7),16)*.14+
       hash(Math.floor(x/21),Math.floor(y/21),47)*.08;
     if(hash(x,y,63)>.986)v=.51;
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
