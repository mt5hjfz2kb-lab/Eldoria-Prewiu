#!/usr/bin/env node
/*
 * Exact production parity for the NINE deterministic metal textures.
 * Source tests alone are not sufficient: fail if Unity still imports older
 * flattened/distorted PNGs. All 20 illustrated icon PNGs stay untouched.
 * This is a file/CRC test, never Unity visual or artistic acceptance.
 */
"use strict";
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const crypto = require("node:crypto");
const {makeAsset, variants} = require("./forja-detail-art.js");
const {toRGBA} = require("./build-premium-sprites.js");
const root = path.resolve(__dirname,"../../Unity/Assets/Eldoria/VisualKit/Resources/ForjaOscura/Surfaces");
const details = [];
for (const [name,kind,width,height,seed] of variants()) {
  const output = path.join(root,name+".png");
  assert.ok(fs.existsSync(output),"Missing real Unity production PNG: "+output);
  assert.ok(fs.existsSync(output+".meta"),"Missing retained Unity GUID for "+output);
  const actual = fs.readFileSync(output);
  const expected = toRGBA(makeAsset(kind,width,height,seed,true).png);
  assert.ok(actual.equals(expected),
    "Production PNG does not match the stretch-safe first-party generator: "+name);
  details.push({
    name, bytes:actual.length,
    sha256:crypto.createHash("sha256").update(actual).digest("hex")
  });
}
assert.equal(details.length,9);
console.log("FORJA_PRODUCTION_NINE_SPRITES_EXACT_SOURCE_PARITY_PASS",JSON.stringify(details));
