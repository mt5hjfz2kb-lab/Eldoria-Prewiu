#!/usr/bin/env node
/*
 * Forja Oscura: verify pixel-by-pixel that the generator's fixed
 * engravings/rivets survive Unity nine-slice resizing.
 * This is an ASSET test, not a Unity native screenshot or artistic approval.
 * No dependencies beyond Node.js.
 */
"use strict";
const assert = require("node:assert/strict");
const zlib = require("node:zlib");
const {makeAsset, variants} = require("./forja-detail-art.js");

const borders = {
  panel_dark: [56,20,56,20],
  panel_quest: [56,22,56,22],
  resource_chip: [36,14,36,14],
  nav_tile_normal: [48,22,48,22],
  nav_tile_selected: [48,22,48,22],
  button_primary_normal: [56,15,56,15],
  button_primary_hover: [56,15,56,15],
  button_primary_pressed: [56,15,56,15],
  button_primary_disabled: [56,15,56,15]
};

function indexedPixels(bytes, width, height) {
  const data = Buffer.from(bytes);
  assert.equal(data.subarray(0,8).toString("hex"),"89504e470d0a1a0a");
  const chunks = [];
  let off = 8;
  while (off < data.length) {
    const n = data.readUInt32BE(off);
    const type = data.toString("ascii", off+4, off+8);
    assert.ok(off+n+12 <= data.length, "PNG chunk truncated");
    if (type === "IHDR") {
      assert.equal(data.readUInt32BE(off+8), width);
      assert.equal(data.readUInt32BE(off+12), height);
      assert.equal(data[off+16],8,"indexed source depth");
      assert.equal(data[off+17],3,"indexed source color");
    }
    if (type === "IDAT") chunks.push(data.subarray(off+8,off+8+n));
    off += n+12;
  }
  const raw = zlib.inflateSync(Buffer.concat(chunks));
  assert.equal(raw.length,(width+1)*height,"source indexed stride");
  const pixels = new Uint8Array(width*height);
  for(let y=0;y<height;y++){
    assert.equal(raw[y*(width+1)],0,"expected unfiltered first-party PNG row");
    pixels.set(raw.subarray(y*(width+1)+1,(y+1)*(width+1)),y*width);
  }
  return pixels;
}

const results = [];
for (const [name, kind, width, height, seed] of variants()) {
  const border = borders[name];
  assert.ok(border, "Border table missing " + name);
  const [left,bottom,right,top] = border;
  const withMarks = makeAsset(kind,width,height,seed,true);
  const plain = makeAsset(kind,width,height,seed,false);
  assert.ok(withMarks.bevel <= Math.min(left,bottom,right,top),
    name + " bevel cuts into stretchable edge strips");
  const illustrated = indexedPixels(withMarks.png,width,height);
  const neutral = indexedPixels(plain.png,width,height);
  const count = [0,0,0,0];
  for(let y=0;y<height;y++)for(let x=0;x<width;x++){
    const p=y*width+x;
    if(illustrated[p]===neutral[p]) continue;
    const cornerX=x<left?0:x>=width-right?1:-1;
    const cornerY=y<top?0:y>=height-bottom?1:-1;
    assert.ok(cornerX>=0 && cornerY>=0,
      name + " artist detail enters stretchable nine-slice strip at "+x+","+y);
    count[cornerY*2+cornerX]++;
  }
  assert.ok(count.every(v=>v>8),name+" is missing an intentional rivet in a fixed corner: "+count);
  results.push({name, fixedCornerDetailPixels:count.reduce((a,b)=>a+b,0),bevel:withMarks.bevel,border});
}
assert.equal(results.length,9);
console.log("FORJA_NINE_SLICE_ORNAMENT_SAFETY_PASS",JSON.stringify(results));
