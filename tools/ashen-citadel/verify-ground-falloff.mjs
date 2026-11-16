// Ashen courtyard/path color-falloff regression check.
// Static source guards plus independently evaluated numeric boundary tests.
// Does NOT compile Unity or validate the rendered artistic result.
import fs from "node:fs";
import assert from "node:assert/strict";

const src=fs.readFileSync("Unity/Assets/Eldoria/ExperimentalAshenCitadel/Editor/AshenCitadelSceneBuilder.cs","utf8");
assert.match(src,/float worn=1f-Mathf\.SmoothStep\(0f,1f,Mathf\.InverseLerp\(\.55f,1\.9f\+\(detail-\.5f\)\*\.8f,dist\)\);/);
assert.match(src,/float yard=1f-Mathf\.SmoothStep\(0f,1f,Mathf\.InverseLerp\(\.84f,1\.13f\+\(detail-\.5f\)\*\.12f,yardEdge\)\);/);
assert.match(src,/var homes=new\[\]\{new Vector2\(-17\.2f,-6\.2f\)/);
assert.equal(src.match(/var homes=new\[\]/g)?.length,1);

const clamp01=x=>Math.max(0,Math.min(1,x));
const smoothstep=t=>{t=clamp01(t);return t*t*(3-2*t)};
const falloff=(a,b,x)=>1-smoothstep((x-a)/(b-a));
const close=(a,b)=>Math.abs(a-b)<1e-8;

for (const detail of [0,.25,.5,.75,1]){
 const pathFar=1.9+(detail-.5)*.8;
 const yardFar=1.13+(detail-.5)*.12;
 for (const [near,far] of [[.55,pathFar],[.84,yardFar]]){
  assert(far>near);
  assert(close(falloff(near,far,0),1),"Centre must retain full wear");
  assert(close(falloff(near,far,far+.5),0),"Outside must have no wear");
  let previous=1;
  for(let i=0;i<=100;i++){
   const t=near+(far-near)*i/100, v=falloff(near,far,t);
   assert(v>=0&&v<=1&&v<=previous+1e-9,"Fade must be bounded and monotonic");
   previous=v;
  }
 }
}
console.log("ASHEN_GROUND_FALLOFF_CONTRACT_PASS: source guards and 5 x 2 x 101 monotonic/boundary cases");
