'use strict';
const test=require('node:test');
const assert=require('node:assert/strict');
const {PNG}=require('pngjs');
const {renderedGameFrame}=require('../tools/eldoria-rendered-frame-readiness.cjs');
function frame(pixel){
 const png=new PNG({width:120,height:80});
 for(let y=0;y<80;y++)for(let x=0;x<120;x++){
  const i=(y*120+x)*4,v=pixel(x,y);
  png.data[i]=v[0];png.data[i+1]=v[1];png.data[i+2]=v[2];png.data[i+3]=255;
 }
 return PNG.sync.write(png);
}
test('monochrome Unity splash cannot authorize input despite scene-loaded',()=>{
 const splash=frame((x,y)=>{const v=x>35&&x<85&&y>32&&y<48?110:31;return [v,v,v]});
 assert.equal(renderedGameFrame(splash).ready,false);
});
test('white or black missing frames remain blocked',()=>{
 for(const v of [0,255])assert.equal(renderedGameFrame(frame(()=>[v,v,v])).ready,false);
});
test('a coloured HTML loading bar outside the gameplay area is insufficient',()=>{
 const loading=frame((x,y)=>y>74?[40,200,90]:[31,31,31]);
 assert.equal(renderedGameFrame(loading).ready,false);
});
test('rendered coloured Valoria area passes frame prerequisite, not gesture acceptance',()=>{
 const game=frame((x,y)=>[(x*3)%140,100+(y%50),35]);
 assert.equal(renderedGameFrame(game).ready,true);
});
test('corrupt screenshot throws instead of approving absent evidence',()=>{
 assert.throws(()=>renderedGameFrame(Buffer.from('invalid png')));
});
