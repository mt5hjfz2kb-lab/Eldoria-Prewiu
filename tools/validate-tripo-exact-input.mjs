import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

const manifestPath = process.argv[2];
if (!manifestPath) {
  console.error('usage: node tools/validate-tripo-exact-input.mjs <manifest.json>');
  process.exit(2);
}

const manifest = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
const repoRoot = process.cwd();
const partsDir = path.resolve(repoRoot, manifest.parts_dir);
if (!fs.existsSync(partsDir)) {
  throw new Error('Exact-input parts directory does not exist: ' + partsDir);
}

const parts = fs.readdirSync(partsDir)
  .filter(name => /^part_\d+\.b64$/i.test(name))
  .sort();

if (!parts.length) throw new Error('No exact-input base64 parts found in ' + partsDir);

const b64 = parts.map(name => fs.readFileSync(path.join(partsDir, name), 'utf8').trim()).join('');
let bytes;
try {
  bytes = Buffer.from(b64, 'base64');
} catch (error) {
  throw new Error('Base64 decode failed: ' + error.message);
}

const roundtrip = bytes.toString('base64').replace(/=+$/,'');
const normalized = b64.replace(/\s+/g,'').replace(/=+$/,'');
if (roundtrip !== normalized) throw new Error('Base64 payload is malformed or incomplete.');

const sha256 = crypto.createHash('sha256').update(bytes).digest('hex');
const actual = { parts: parts.length, bytes: bytes.length, sha256 };
const expected = {
  bytes: Number(manifest.expected_size_bytes),
  sha256: String(manifest.expected_sha256 || '').toLowerCase()
};

if (actual.bytes !== expected.bytes || actual.sha256 !== expected.sha256) {
  console.error(JSON.stringify({ ok:false, expected, actual }, null, 2));
  process.exit(1);
}

console.log(JSON.stringify({ ok:true, asset_name:manifest.asset_name, expected, actual }, null, 2));
