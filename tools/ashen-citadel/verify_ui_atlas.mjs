// Verify the actual committed image bytes, not an authored palette constant.
import fs from "node:fs";
import path from "node:path";
import {inflateSync} from "node:zlib";
import {createHash} from "node:crypto";

const root="Unity/Assets/Eldoria/ExperimentalAshenCitadel/Resources";
const manifest=JSON.parse(fs.readFileSync("tools/ashen-citadel/ui-atlas-manifest.json","utf8"));
if(manifest.assets.length!==14)throw Error("Expected 12 premium UI images");
function parsePng(file){
 const b=fs.readFileSync(file);
 if(!b.subarray(0,8).equals(Buffer.from([137,80,78,71,13,10,26,10])))throw Error("Bad PNG signature "+file);
 let width=0,height=0,offset=8,idat=[];
 while(offset<b.length){
  const length=b.readUInt32BE(offset);
  const type=b.toString("ascii",offset+4,offset+8);
  const data=b.subarray(offset+8,offset+8+length);
  if(type==="IHDR"){
   width=data.readUInt32BE(0);height=data.readUInt32BE(4);
   if(data[8]!==8||data[9]!==6)throw Error("Expected RGBA8 for "+file);
  }
  if(type==="IDAT")idat.push(data);
  offset+=12+length;
  if(type==="IEND")break;
 }
 const data=inflateSync(Buffer.concat(idat));
 if(data.length!==height*(1+width*4))throw Error("Corrupted raw data "+file);
 const out=new Uint8Array(width*height*4);
 for(let y=0;y<height;y++){
  const pos=y*(1+width*4);
  if(data[pos]!==0)throw Error("Source generator must emit filter 0");
  out.set(data.subarray(pos+1,pos+1+width*4),y*width*4);
 }
 return {width,height,pixels:out,bytes:b};
}
let details=[];
for(const item of manifest.assets){
 const p=path.join(root,item.name);
 const {width,height,pixels,bytes}=parsePng(p);
 if(width!==item.width||height!==item.height)throw Error("Dimension mismatch "+p);
 if(createHash("sha256").update(bytes).digest("hex")!==item.sha256)throw Error("SHA mismatch "+p);
 let alpha=0,fill=0;
 for(let k=0;k<pixels.length;k+=4){if(pixels[k+3]>24)alpha++;if(pixels[k+3]>220)fill++;}
 const fraction=alpha/(width*height);
 if(item.name==="AshenPremiumPanel.png"){
  const pos=((height/2)*width+width/2)*4;
  const rgb=[...pixels.slice(pos,pos+3)];
  if(Math.max(...rgb)>95)throw Error("UI panel center must be DARK, actual: "+rgb.join(","));
  if(fraction<.9)throw Error("UI panel incorrectly transparent");
 }
 if(item.name.includes("Glyph_")){
  if(fraction<.005||fraction>.65)
   throw Error("Glyph must contain both transparent background and legible stroke: "+item.name+" alphaFraction="+fraction);
 }
 if(item.name.includes("Button")&&(fraction<.60||fraction>.99))
  throw Error("Button alpha bounds invalid: "+item.name);
 details.push({name:item.name,alphaFraction:Math.round(fraction*1000)/10,opaqueFill:fill});
}
console.log("ASHEN_PREMIUM_UI_RASTER_QA_PASS",JSON.stringify(details));
