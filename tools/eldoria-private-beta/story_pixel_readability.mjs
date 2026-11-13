import fs from 'node:fs';
import {decodeUnityPng} from '../art-direction/ai-authoring/unity_pixel_diff.mjs';

// Inspect the actual OCR text bounds in an unmodified browser PNG. A property
// value or OCR recognition alone does not prove light ink reached the GPU.
export function measureStoryInk(file, row) {
  const png=decodeUnityPng(fs.readFileSync(file));
  const sx=png.width/390,sy=png.height/844;
  const x0=Math.max(0,Math.floor((row.x-row.width/2)*sx));
  const x1=Math.min(png.width,Math.ceil((row.x+row.width/2)*sx));
  const y0=Math.max(0,Math.floor((row.y-row.height/2)*sy));
  const y1=Math.min(png.height,Math.ceil((row.y+row.height/2)*sy));
  const values=[];
  for(let y=y0;y<y1;y++)for(let x=x0;x<x1;x++){
    const i=(y*png.width+x)*3;
    values.push((.2126*png.rgb[i]+.7152*png.rgb[i+1]+.0722*png.rgb[i+2])/255);
  }
  if(values.length<20)throw Error('Narrative OCR crop is empty or too small');
  values.sort((a,b)=>a-b);
  const p90=values[Math.floor((values.length-1)*.90)];
  const brightFraction=values.filter(v=>v>=.65).length/values.length;
  return {text:row.text,p90,brightFraction,pixels:values.length,
    bounds:{x0,y0,x1,y1},passed:p90>=.65&&brightFraction>=.10,
    scope:'Rendered ivory-on-stone regression, not a full accessibility certificate'};
}
