// Reproducible X/Y-up OBJ planar UV0 atlas. Execute AFTER generate.mjs.
// Each original polygon owns its vertex indices, so per-face projection is
// distortion-free and does not change geometry or ownership.
import fs from "node:fs";
const groups={
 Stone:["basalt","stone","stoneLight","cut","plaster"],
 Roof:["roof","slate"],Wood:["wood","oak"],
 Moss:["soil","moss","leaf","leafLight"],
 Metal:["iron","brass","warm"],Cloth:["banner","violet"]
};
export function unwrap(text){
 const lines=text.trimEnd().split("\n"),coords=[[0,0,0]],uv=[];
 for(const line of lines)if(line.startsWith("v "))coords.push(line.slice(2).trim().split(/\s+/).map(Number));
 const out=[];
 for(const line of lines){
  if(line.startsWith("vt "))continue;
  if(!line.startsWith("f ")){out.push(line);continue;}
  const ids=line.slice(2).trim().split(/\s+/).map(s=>Number(s.split("/")[0]));
  if(ids.length<3||ids.some(i=>i<1||i>=coords.length))throw Error("Invalid source face");
  const [a,b,c]=ids.slice(0,3).map(i=>coords[i]),d=b.map((x,j)=>x-a[j]),e=c.map((x,j)=>x-a[j]);
  const n=[d[1]*e[2]-d[2]*e[1],d[2]*e[0]-d[0]*e[2],d[0]*e[1]-d[1]*e[0]].map(Math.abs);
  const axis=n[1]>=n[0]&&n[1]>=n[2]?"y":n[0]>n[2]?"x":"z";
  for(const i of ids){
   const p=coords[i],u=axis==="y"?[p[0],p[2]]:axis==="x"?[p[2],p[1]]:[p[0],p[1]];
   const pair=u.map(v=>(v*.46).toFixed(5));
   if(uv[i]&&uv[i].join("/")!==pair.join("/"))throw Error("Nonunique UV face mapping");
   uv[i]=pair;
  }
  out.push("f "+ids.map(i=>i+"/"+i).join(" "));
 }
 if(uv.slice(1).length!==coords.length-1||uv.length!==coords.length)throw Error("Unbound UV vertex");
 const pos=out.findIndex(l=>l.startsWith("usemtl "));if(pos<0)throw Error("Missing MTL");
 out.splice(pos,0,...uv.slice(1).map(p=>"vt "+p.join(" ")));
 return out.join("\n")+"\n";
}
export function bindMtl(text){
 let key=null,out=[];
 for(const line of text.trimEnd().split("\n")){
  out.push(line);
  if(line.startsWith("newmtl "))key=line.slice(7).trim();
  if(line.trim()==="illum 2"&&key){
   const family=Object.keys(groups).find(g=>groups[g].includes(key));
   if(!family)throw Error("Unknown material "+key);
   out.push("map_Kd ../Textures/Ashen"+family+".png");
  }
 }
 return out.join("\n")+"\n";
}
const root="Unity/Assets/Eldoria/ExperimentalAshenCitadel/Meshes/";
for(const name of [...Array.from({length:5},(_,i)=>"AshenBastion_"+(i+1)),
      ..."Sawmill Barracks Granary Stoneworks Pine Rock".split(" ").map(n=>"Ashen_"+n)]){
 const file=root+name+".obj";
 fs.writeFileSync(file,unwrap(fs.readFileSync(file,"utf8")));
 console.log("ASHEN_UV_OK",name);
}
const palette=root+"AshenPalette.mtl";
fs.writeFileSync(palette,bindMtl(fs.readFileSync(palette,"utf8")));
console.log("ASHEN_TEXTURE_REFERENCES_OK");
