'use strict';
const {PNG}=require('pngjs');
function renderedGameFrame(buffer){
 const frame=PNG.sync.read(buffer);let chromatic=0,total=0;
 // The Unity splash is monochrome. Require actual coloured game pixels before
 // sending input; scene-loaded and the HTML loader do not prove a rendered frame.
 for(let y=Math.floor(frame.height*.15);y<frame.height*.85;y+=3){
  for(let x=Math.floor(frame.width*.1);x<frame.width*.9;x+=3){
   const i=(y*frame.width+x)*4,values=[frame.data[i],frame.data[i+1],frame.data[i+2]];
   total++;if(Math.max(...values)-Math.min(...values)>25)chromatic++;
  }
 }
 return {chromaticRatio:chromatic/total,ready:chromatic/total>.03};
}
module.exports={renderedGameFrame};
