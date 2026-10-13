#!/usr/bin/env node
// Mirror of ZoneOneWorld.cs. Zero dependencies; emits deterministic logical cells.
const fs=require('node:fs'),path=require('node:path');
const SIZE=128,CHUNK=16,SEED=20261013;
function hash(x,z,salt){
 let h=(Math.imul(x,374761393)+Math.imul(z,668265263)+salt+SEED)>>>0;
 h=Math.imul((h^(h>>>13))>>>0,1274126177)>>>0;
 return ((h^(h>>>16))>>>0)/4294967295;
}
function cell(x,z){
 if(x<0||z<0||x>=SIZE||z>=SIZE)throw Error('outside world');
 const dx=x-63.5,dz=z-63.5,broad=hash(Math.trunc(x/4),Math.trunc(z/4),1),fine=hash(x,z,2);
 const breach=Math.hypot(dx,dz)<14+(broad-.5)*5;
 const sea=z>SIZE-9+Math.round((broad-.5)*6);
 const terrain=breach?'breach':sea?'sea':broad>.84?'mountain':broad>.70?'forest':'plain';
 let feature=null;
 if(terrain==='plain'){
   if(fine<.008)feature='city';else if(fine<.020)feature='enemy_camp';
   else if(fine<.030)feature='ruin';else if(fine<.11)feature=['wood','stone','food'][Math.min(2,Math.floor(hash(x,z,3)*3))];
 }
 return {x,z,terrain,...(feature?{feature}:{})};
}
function chunk(cx,cz){if(cx<0||cz<0||cx>=8||cz>=8)return [];const out=[];for(let z=cz*CHUNK;z<(cz+1)*CHUNK;z++)for(let x=cx*CHUNK;x<(cx+1)*CHUNK;x++)out.push(cell(x,z));return out;}
function visibleChunks(cx,cz,r=1){const out=[];for(let z=Math.max(0,cz-r);z<=Math.min(7,cz+r);z++)for(let x=Math.max(0,cx-r);x<=Math.min(7,cx+r);x++)out.push([x,z]);return out;}
function verify(){
 const counts={},ids=new Set();for(let cz=0;cz<8;cz++)for(let cx=0;cx<8;cx++)for(const c of chunk(cx,cz)){
   const id=c.x+','+c.z;if(ids.has(id))throw Error('duplicate '+id);ids.add(id);
   counts[c.terrain]=(counts[c.terrain]||0)+1;if(c.feature)counts[c.feature]=(counts[c.feature]||0)+1;
   if(c.terrain==='sea'&&c.z<116)throw Error('sea not at southern edge');
   if(c.terrain==='breach'&&Math.hypot(c.x-63.5,c.z-63.5)>17)throw Error('breach outside center');
   if(c.feature&&c.terrain!=='plain')throw Error('feature on blocked terrain');
 }
 if(ids.size!==SIZE*SIZE||counts.plain<SIZE*SIZE*.55||!counts.city||!counts.wood||!counts.stone||!counts.food)throw Error('world invariants');
 if(JSON.stringify(chunk(3,3))!==JSON.stringify(chunk(3,3)))throw Error('nondeterminism');
 if(visibleChunks(0,0,1).length!==4||visibleChunks(4,4,1).length!==9)throw Error('chunk bounds');
 return counts;
}
if(require.main===module){
 const counts=verify(),out=process.argv[2];if(out){const cells=[];for(let cz=0;cz<8;cz++)for(let cx=0;cx<8;cx++)cells.push(...chunk(cx,cz));fs.mkdirSync(path.dirname(out),{recursive:true});fs.writeFileSync(out,JSON.stringify({schema:'eldoria-zone1-logical-v2',size:SIZE,chunkSize:CHUNK,seed:SEED,cells}));}
 console.log(JSON.stringify({ok:true,counts,output:out||null},null,2));
}
module.exports={cell,chunk,visibleChunks,verify};
