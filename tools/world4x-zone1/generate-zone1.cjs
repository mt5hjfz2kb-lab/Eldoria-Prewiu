#!/usr/bin/env node
// Zero-dependency deterministic logical Zone 1 prototype. No Unity production files touched.
const fs=require('node:fs');
const size=128, seed=20261013;
function noise(x,y,s=0){let n=Math.imul(x+seed+s*97,374761393)^Math.imul(y-seed,668265263);n=Math.imul(n^(n>>>13),1274126177);return ((n^(n>>>16))>>>0)/4294967295}
const cells=[], cx=(size-1)/2,cy=(size-1)/2;
for(let y=0;y<size;y++)for(let x=0;x<size;x++){
 const r=Math.hypot(x-cx,y-cy), n=noise(x,y), n2=noise(x>>2,y>>2,1);
 const breach=r<14+(n2-.5)*5, coast=y>size-9+Math.round((n2-.5)*6);
 let terrain=breach?'breach':coast?'sea':n2>.82?'mountain':n2>.66?'forest':'plain';
 let entity=null;
 if(!breach&&!coast&&terrain!=='mountain'){
   if(n<.009)entity='player_city';else if(n<.025)entity='enemy_camp';else if(n<.038)entity='ruins';else if(n<.115)entity=['wood','stone','food'][Math.floor(noise(x,y,3)*3)];
 }
 cells.push({x,y,terrain,...(entity?{entity}:{})});
}
const data={schema:'eldoria-zone1-logical-v1',seed,size,breachCenter:{x:cx,y:cy},southSea:true,cells};
const out=process.argv[2];if(out){fs.mkdirSync(require('node:path').dirname(out),{recursive:true});fs.writeFileSync(out,JSON.stringify(data));}
const counts={};for(const c of cells){counts[c.terrain]=(counts[c.terrain]||0)+1;if(c.entity)counts[c.entity]=(counts[c.entity]||0)+1;}
console.log(JSON.stringify({size,seed,counts,output:out||null},null,2));
