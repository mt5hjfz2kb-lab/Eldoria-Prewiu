/* Forja Oscura: RGBA PNG sprite production from deterministic first-party art.
 * Zero third-party packages; no gameplay assets, player names, or UI screenshot crops.
 * node tools/forja-oscura/build-premium-sprites.js [output-directory]
 */
"use strict";
const fs=require("node:fs"),path=require("node:path"),zlib=require("node:zlib"),crypto=require("node:crypto");
const {makeAsset,variants}=require("./forja-detail-art.js");
const crctab=new Uint32Array(256);
for(let i=0;i<256;i++){let c=i;for(let k=0;k<8;k++)c=(c&1)?(0xedb88320^(c>>>1)):(c>>>1);crctab[i]=c>>>0;}
const crc=bytes=>{let c=0xffffffff;for(const b of bytes)c=crctab[(c^b)&255]^(c>>>8);return (c^0xffffffff)>>>0;};
function chunk(kind,data){const name=Buffer.from(kind),s=Buffer.alloc(4),e=Buffer.alloc(4);s.writeUInt32BE(data.length);e.writeUInt32BE(crc(Buffer.concat([name,data])));return Buffer.concat([s,name,data,e]);}
function toRGBA(bytes){
 let at=8,width,height,plte,alpha,compressed=[];
 const png=Buffer.from(bytes);
 if(png.subarray(0,8).toString("hex")!=="89504e470d0a1a0a")throw Error("Not a PNG");
 while(at<png.length){const n=png.readUInt32BE(at),tag=png.toString("ascii",at+4,at+8),d=png.subarray(at+8,at+8+n);at+=12+n;
   if(tag==="IHDR"){width=d.readUInt32BE(0);height=d.readUInt32BE(4);if(d[8]!==8||d[9]!==3)throw Error("Expected 8-bit indexed RGBA palette");}
   if(tag==="PLTE")plte=d;if(tag==="tRNS")alpha=d;if(tag==="IDAT")compressed.push(d);
 }
 if(!plte||!alpha||!width||!height)throw Error("Missing alpha/palette");
 const raw=zlib.inflateSync(Buffer.concat(compressed));if(raw.length!==height*(width+1))throw Error("Unexpected pixel length");
 const rgba=Buffer.alloc(height*(1+4*width));
 for(let y=0;y<height;y++)for(let x=0;x<width;x++){
   const idx=raw[y*(width+1)+1+x],src=idx*3,dst=y*(width*4+1)+1+x*4;
   rgba[dst]=plte[src];rgba[dst+1]=plte[src+1];rgba[dst+2]=plte[src+2];rgba[dst+3]=alpha[idx];
 }
 const ihdr=Buffer.alloc(13);ihdr.writeUInt32BE(width,0);ihdr.writeUInt32BE(height,4);ihdr[8]=8;ihdr[9]=6;
 return Buffer.concat([png.subarray(0,8),chunk("IHDR",ihdr),chunk("IDAT",zlib.deflateSync(rgba,{level:9})),chunk("IEND",Buffer.alloc(0))]);
}
function main(){
 const out=path.resolve(process.argv[2]||"forja-premium-art");
 fs.mkdirSync(out,{recursive:true});const manifest=[];
 for(const [name,kind,w,h,seed]of variants()){
   const generated=makeAsset(kind,w,h,seed),rgba=toRGBA(generated.png),filename=name+".png";
   fs.writeFileSync(path.join(out,filename),rgba);
   manifest.push({filename,width:w,height:h,bytes:rgba.length,sha256:crypto.createHash("sha256").update(rgba).digest("hex"),origin:"first-party deterministic art; not a Unity native capture"});
 }
 fs.writeFileSync(path.join(out,"manifest.json"),JSON.stringify({status:"ART_CANDIDATE_NON_NATIVE",pieces:manifest},null,2));
 console.log(JSON.stringify({result:"PNG_RGBA_GENERATED_PREVIEW_ONLY",count:manifest.length,totalBytes:manifest.reduce((s,x)=>s+x.bytes,0),destination:out},null,2));
}
if(require.main===module)main();
module.exports={toRGBA};
