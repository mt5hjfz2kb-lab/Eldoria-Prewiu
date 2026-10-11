#!/usr/bin/env node
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

const [, , inputPath, assetName, expectedShaArg=''] = process.argv;
if (!inputPath || !assetName) {
  console.error('Usage: node tools/persist-tripo-exact-input.mjs <image> <asset_name> [expected_sha256]');
  process.exit(2);
}
if (!/^[A-Za-z0-9._-]+$/.test(assetName)) {
  throw new Error('Unsafe asset_name.');
}
const bytes=fs.readFileSync(inputPath);
const sha=crypto.createHash('sha256').update(bytes).digest('hex');
const expected=expectedShaArg.trim().toLowerCase();
if (expected && sha!==expected) throw new Error(`SHA mismatch expected=${expected} actual=${sha}`);

const outDir=path.join('pipeline','exact-inputs',assetName);
fs.mkdirSync(outDir,{recursive:true});
for (const name of fs.readdirSync(outDir)) if (/^part_\d+\.b64$/.test(name)) fs.unlinkSync(path.join(outDir,name));
const b64=bytes.toString('base64');
const chunkChars=120000;
let parts=0;
for(let offset=0;offset<b64.length;offset+=chunkChars){
  fs.writeFileSync(path.join(outDir,`part_${String(parts).padStart(3,'0')}.b64`),b64.slice(offset,offset+chunkChars));
  parts++;
}
const manifest={
  asset_name:assetName,
  file_name:path.basename(inputPath),
  sha256:sha,
  size_bytes:bytes.length,
  parts,
  source:'chat_exact_input_persisted_before_tripo'
};
fs.writeFileSync(path.join(outDir,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');
console.log(JSON.stringify({ok:true,...manifest},null,2));
