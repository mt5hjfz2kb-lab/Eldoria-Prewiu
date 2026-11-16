// Ashen Citadel UI sprites — deterministic authored PNG from JavaScript source.
// This intentionally bypasses Unity 6000 batchmode Texture2D.SetPixels32,
// which returned white centre pixels in two genuine native runs even though
// the mathematical palette source predicted dark pixels. No paid assets.
// Run: node tools/ashen-citadel/generate_ui_atlas.mjs
import fs from "node:fs";
import path from "node:path";
import {deflateSync} from "node:zlib";
import {createHash} from "node:crypto";
const root="Unity/Assets/Eldoria/ExperimentalAshenCitadel/Resources";
fs.mkdirSync(root,{recursive:true});
const clamp=(v,lo,hi)=>Math.max(lo,Math.min(hi,v));
const byte=v=>Math.round(clamp(v,0,255));
const vec=(r,g,b,a=255)=>[byte(r),byte(g),byte(b),byte(a)];
const fract=v=>v-Math.floor(v);
function noise(x,y){
  let h=(Math.imul(x,374761393)+Math.imul(y,668265263)+0x9e3779b9)>>>0;
  h=Math.imul(h^(h>>>13),1274126177)>>>0;
  return ((h^(h>>>16))&255)/255;
}
function panel(x,y){
  let e=Math.min(x,255-x,y,255-y);
  if(e<=1)return vec(8,13,17);
  if(e===2)return vec(80,62,40);
  if(e<=4)return vec(124,94,56,244);
  if(e===5)return vec(57,52,42,253);
  if(e<9)return vec(24,31,38,250);
  if((x<16||x>239)&&(y<16||y>239)&&e<12&&(x+y)%5<2)
    return vec(153,116,63,253);
  let n=(noise(x,y)-.5)*5,vg=Math.pow(Math.abs(x-128)/128,1.5)*4,c=4.5*y/255;
  return vec(17+c+n-vg,25+c+n-vg,32+c+n-vg,248);
}
function button(x,y,main){
  const cx=x-127.5,cy=y-35.5,qx=Math.abs(cx)-122,qy=Math.abs(cy)-30;
  const d=Math.hypot(Math.max(qx,0),Math.max(qy,0))+Math.min(Math.max(qx,qy),0);
  if(d>5)return vec(0,0,0,0);
  if(d>3)return vec(8,14,18,240);
  if(d>1.8)return main?vec(163,124,65,252):vec(99,84,52,252);
  if(d>.1)return main?vec(64,56,39,246):vec(49,51,49,246);
  let t=y/71,n=(noise(x,y)-.5)*3;
  return main?vec(51+10*t+n,42+8*t+n,36+7*t+n,250)
    :vec(31+6*t+n,41+7*t+n,49+7*t+n,244);
}
function crest(x,y){
  const dx=x-63.5,dy=63.5-y,r=Math.hypot(dx,dy);
  if(r>61)return vec(0,0,0,0);
  if(r>57)return vec(12,20,25,0);
  if(r>54)return vec(147,111,58,246);
  if(r>50)return vec(32,35,33,254);
  if(r>46)return vec(93,75,48,253);
  if(r>43)return vec(17,27,34,252);
  const ax=Math.abs(x-64),yp=128-y;
  const wall=ax<22&&yp>35&&yp<79;
  const crenels=yp>=79&&yp<88&&ax<23&&(Math.floor(x/9)%2===0);
  const roof=ax<15&&yp>=87&&yp<93;
  const portal=ax<7&&yp>35&&yp<61;
  const slit=ax>11&&ax<15&&yp>60&&yp<72;
  if(portal)return vec(15,26,32,255);
  if(wall||crenels||roof)return vec(185,157,103,254);
  if(slit)return vec(24,38,43,255);
  if(ax<31&&yp>28&&yp<34)return vec(132,98,53,247);
  return vec(19,31,38,250);
}
const box=(x,y,cx,cy,hx,hy)=>Math.hypot(Math.max(Math.abs(x-cx)-hx,0),Math.max(Math.abs(y-cy)-hy,0))
 +Math.min(Math.max(Math.abs(x-cx)-hx,Math.abs(y-cy)-hy),0);
const stroke=(x,y,ax,ay,bx,by,r)=>{
  let dx=bx-ax,dy=by-ay,t=clamp(((x-ax)*dx+(y-ay)*dy)/(dx*dx+dy*dy),0,1);
  return Math.hypot(x-(ax+t*dx),y-(ay+t*dy))-r;
};
const circle=(x,y,r,w)=>Math.abs(Math.hypot(x,y)-r)-w;
function glyphDistance(key,x,y){
  let d=10,add=v=>d=Math.min(d,v);
  if(key==="castle"){
    d=box(x,y,0,-.1,.59,.46);for(let i=-2;i<=2;i++)add(box(x,y,i*.23,.45,.075,.19));
    return Math.max(d,-box(x,y,0,-.40,.15,.34));
  }
  if(key==="workshop"){
    add(stroke(x,y,-.74,-.04,0,.61,.075));add(stroke(x,y,0,.61,.78,-.06,.075));
    add(box(x,y,0,-.35,.59,.36));add(box(x,y,-.24,-.33,.13,.35));return d;
  }
  if(key==="army"){
    add(stroke(x,y,-.66,-.60,.62,.61,.095));add(stroke(x,y,-.62,.63,.66,-.58,.095));
    add(circle(x,y,.12,.07));return d;
  }
  if(key==="compass"){
    add(circle(x,y,.65,.07));add(stroke(x,y,-.39,-.39,.39,.39,.06));
    add(stroke(x,y,-.39,.39,.39,-.39,.06));add(box(x,y,0,0,.1,.1));return d;
  }
  if(key==="mail"){
    add(Math.abs(box(x,y,0,0,.69,.45))-.065);
    add(stroke(x,y,-.66,.34,0,-.16,.065));add(stroke(x,y,0,-.16,.66,.34,.065));return d;
  }
  if(key==="bag"){
    add(circle(x,y+.20,.54,.1));add(box(x,y,0,-.33,.5,.32));
    add(stroke(x,y,-.28,.60,.25,.60,.10));return d;
  }
  if(key==="hero"){
    for(const [a,b,c,d1] of [[-.65,.54,0,.72],[0,.72,.65,.54],[-.65,.54,-.52,-.32],
      [.65,.54,.52,-.32],[-.52,-.32,0,-.72],[.52,-.32,0,-.72]])
        add(stroke(x,y,a,b,c,d1,.09));
    return d;
  }
  if(key==="quest"){
    for(const [a,b,c,d1] of [[-.43,-.67,-.43,.69],[-.43,.60,.57,.47],
      [.57,.47,.28,.07],[.28,.07,-.43,.22]])
      add(stroke(x,y,a,b,c,d1,.085));
    return d;
  }
  throw Error("Unknown glyph "+key);
}
function glyph(key,x,y){
  const px=(x-31.5)/27,py=(31.5-y)/27;
  let d=glyphDistance(key,px,py),a=byte(clamp((.08-d)/.08,0,1)*250);
  return vec(190,154,96,a);
}
const crcTable=new Uint32Array(256);
for(let n=0;n<256;n++){let v=n;for(let k=0;k<8;k++)v=v&1?(0xedb88320^(v>>>1)):(v>>>1);crcTable[n]=v>>>0;}
function crc(bytes){let c=0xffffffff;for(const v of bytes)c=crcTable[(c^v)&255]^(c>>>8);return (c^0xffffffff)>>>0;}
function u32(v){const o=Buffer.alloc(4);o.writeUInt32BE(v>>>0);return o;}
function chunk(name,body){
 const type=Buffer.from(name,"ascii");return Buffer.concat([u32(body.length),type,body,u32(crc(Buffer.concat([type,body])))]);
}
function png(width,height,sample){
 const raw=Buffer.alloc(height*(1+4*width));let p=0;
 for(let y=0;y<height;y++){
   raw[p++]=0;
   for(let x=0;x<width;x++)for(const v of sample(x,y))raw[p++]=v;
 }
 const ihdr=Buffer.concat([u32(width),u32(height),Buffer.from([8,6,0,0,0])]);
 return Buffer.concat([Buffer.from([137,80,78,71,13,10,26,10]),
   chunk("IHDR",ihdr),chunk("IDAT",deflateSync(raw,{level:9})),chunk("IEND",Buffer.alloc(0))]);
}
const output=[
 ["AshenPremiumPanel.png",256,256,panel],
 ["AshenPremiumButtonPrimary.png",256,72,(x,y)=>button(x,y,true)],
 ["AshenPremiumButtonSecondary.png",256,72,(x,y)=>button(x,y,false)],
 ["AshenPremiumCrest.png",128,128,crest],
 ...["castle","workshop","army","compass","mail","bag","hero","quest"].map(k=>
   ["AshenPremiumGlyph_"+k+".png",64,64,(x,y)=>glyph(k,x,y)])
];
const result=[];
for(const [name,w,h,sample] of output){
 const bytes=png(w,h,sample);fs.writeFileSync(path.join(root,name),bytes);
 const sha256=createHash("sha256").update(bytes).digest("hex");
 result.push({name,width:w,height:h,size:bytes.length,sha256});
}
const centre=panel(128,128);
if(centre[0]>=90||centre[1]>=90||centre[2]>=100)
  throw Error("Dark slate panel source failed its centre-pixel gate: "+centre.join(","));
const manifest={generator:"ashen-premium-ui-atlas-v1",assets:result};
fs.writeFileSync("tools/ashen-citadel/ui-atlas-manifest.json",
  JSON.stringify(manifest,null,2)+"\n");
console.log("ASHEN_PREMIUM_UI_ATLAS_PASS",result.length,"PNG Sprites; centre RGB",
  centre.slice(0,3).join(","),"; SHA256 manifest saved");
