import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import { validChannel, propose } from './check-channel.mjs';
const full = propose({versionCode: 7, versionName: '1.7', bytes: 200000000,
  sourceSha: 'a'.repeat(40), sha256: 'b'.repeat(64)});
test('Published channel is disabled or a verified main-family release', () => {
  const published = JSON.parse(fs.readFileSync('updates/android/family.json', 'utf8'));
  assert.ok(validChannel(published, 0), 'Channel cannot point at unverified releases');
  assert.equal(JSON.stringify(published).includes('/construction/'), false);
});
test('Exact HTTPS release, pinned certificate, source SHA and package SHA-256', () => {
  assert.ok(validChannel(full, 6));
  for (const change of [
    { apkUrl: full.apkUrl.replace('github.com', 'fake.example') },
    { apkUrl: full.apkUrl + '?redirect=1' },
    { apkUrl: full.apkUrl.replace('code7', 'code6') },
    { certificateSha256: 'c'.repeat(64) },
    { sourceSha: 'z'.repeat(40) }, { sha256: 'x'.repeat(64) },
    { bytes: 500 * 1024 * 1024 }, { packageName: 'com.example.other' },
    { channel: 'experimental' }, { enabled: 'true' }, { versionName: '1.7.apk' }
  ]) assert.equal(validChannel({...full, ...change}, 6), false, JSON.stringify(change));
});
test('No downgrade or same-version update', () => {
  assert.equal(validChannel(full, 7), false);
  assert.equal(validChannel(full, 8), false);
  assert.ok(validChannel(propose({...full, versionCode: 8, versionName: '1.8'}), 7));
});
