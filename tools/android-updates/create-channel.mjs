import assert from 'node:assert/strict';
import fs from 'node:fs';
import { createHash } from 'node:crypto';
import { validChannel, propose, PACKAGE, CERTIFICATE } from './check-channel.mjs';

const [candidateFile, identityFile, channelFile] = process.argv.slice(2);
assert.ok(candidateFile && identityFile && channelFile, 'Expected APK, identity JSON, channel JSON');
const build = JSON.parse(fs.readFileSync(identityFile, 'utf8').replace(/^\uFEFF/, ''));
const current = JSON.parse(fs.readFileSync(channelFile, 'utf8'));
const bytes = fs.statSync(candidateFile).size;
const hash = createHash('sha256');
for await (const part of fs.createReadStream(candidateFile)) hash.update(part);
const sha256 = hash.digest('hex');
assert.equal(build.schema, 1);
assert.equal(build.channel, 'family');
assert.equal(build.packageName, PACKAGE);
assert.equal(build.certificateSha256, CERTIFICATE);
assert.equal(build.nativeSignatureVerified, true);
assert.equal(build.installerPermissionVerified, true);
assert.equal(build.userDataReset, false);
assert.ok(build.versionCode >= 7 && Number.isInteger(build.versionCode));
assert.equal(build.bytes, bytes);
assert.equal(build.sha256, sha256);
assert.match(build.sourceSha, /^[0-9a-f]{40}$/);
if (process.env.ELDORIA_EXPECTED_SHA) assert.equal(build.sourceSha, process.env.ELDORIA_EXPECTED_SHA);
if (process.env.ELDORIA_EXPECTED_CODE) assert.equal(build.versionCode, Number(process.env.ELDORIA_EXPECTED_CODE));
const previousCode = current.enabled ? current.versionCode : 0;
assert.ok(build.versionCode > previousCode, 'Never overwrite/downgrade promoted version');
const next = propose({
  versionCode: build.versionCode, versionName: build.versionName, sourceSha: build.sourceSha,
  sha256, bytes, message: 'Nueva versión de Eldoria disponible. Conserva tu partida y confirma la instalación en Android.'
});
assert.ok(validChannel(next, previousCode));
fs.writeFileSync(channelFile, JSON.stringify(next, null, 2) + '\n');
console.log('ELDORIA_FAMILY_PROMOTION_MANIFEST_VERIFIED', build.versionCode, sha256);
