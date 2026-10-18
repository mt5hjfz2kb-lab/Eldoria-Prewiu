import assert from 'node:assert/strict';
import fs from 'node:fs';

export const PACKAGE = 'com.eldoria.valoria.v5test';
export const CERTIFICATE = 'f479ca92ccfa256a991936b23d0153ffad247b352a8a5ed569457db8274443d8';
export const MAX_BYTES = 450 * 1024 * 1024;
export function expectedUrl(code) {
  return 'https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code' +
    code + '/Eldoria-Valoria-code' + code + '.apk';
}
export function validChannel(m, installedCode = 6) {
  if (!m || m.schema !== 1 || m.channel !== 'family' || m.packageName !== PACKAGE) return false;
  if (m.enabled === false) return true; // Safe, intentionally inactive bootstrap channel.
  if (m.enabled !== true || !Number.isInteger(m.versionCode) || m.versionCode <= installedCode ||
      m.versionCode > 999999) return false;
  if (!Number.isSafeInteger(m.bytes) || m.bytes < 1048576 || m.bytes > MAX_BYTES) return false;
  if (m.certificateSha256 !== CERTIFICATE || !/^[0-9a-f]{64}$/.test(m.sha256 ?? '') ||
      !/^[0-9a-f]{40}$/.test(m.sourceSha ?? '') ||
      !/^[0-9]+[.][0-9]+([.][0-9]+)?$/.test(m.versionName ?? '')) return false;
  return m.apkUrl === expectedUrl(m.versionCode);
}
export function propose({ versionCode, versionName, bytes, sha256, sourceSha, message }) {
  const proposed = {
    schema: 1, enabled: true, channel: 'family', packageName: PACKAGE,
    versionCode, versionName, bytes, sha256, sourceSha,
    certificateSha256: CERTIFICATE, apkUrl: expectedUrl(versionCode),
    message: message || 'Nueva versión integrada de Eldoria disponible. Tu partida se conserva.'
  };
  assert.ok(validChannel(proposed, versionCode - 1), 'Manifest incompatible or unsafe');
  return proposed;
}
if (process.argv[1] && import.meta.url.endsWith(process.argv[1].replaceAll('\\', '/'))) {
  const m = JSON.parse(fs.readFileSync(process.argv[2] || 'updates/android/family.json', 'utf8'));
  assert.ok(validChannel(m, 0), 'Android update channel invalid');
  console.log('ELDORIA_ANDROID_CHANNEL_POLICY_PASS', m.enabled ? m.versionCode : 'disabled');
}
