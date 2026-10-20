import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const source = fs.readFileSync('Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/EldoriaAndroidAutoUpdater.cs', 'utf8');
function between(start, end) {
  const left = source.indexOf(start), right = source.indexOf(end, left + start.length);
  assert.ok(left >= 0 && right > left, 'Missing updater source anchors: ' + start);
  return source.slice(left, right);
}
test('Only pinned, newer, same-package releases can be selected', () => {
  const p = between('public static bool Accept(', 'public static bool VerifyApk(');
  for (const token of ['candidate.versionCode <= installedCode', 'candidate.certificateSha256 != Certificate',
    'candidate.packageName != Package', 'candidate.sha256', 'candidate.sourceSha', 'expected'])
    assert.ok(p.includes(token), 'Missing update policy: ' + token);
  assert.match(source, /raw[.]githubusercontent[.]com\/mt5hjfz2kb-lab\/Eldoria-Prewiu\/main\/updates\/android\/family[.]json/);
});
test('Opening the game starts transfer, not installation', () => {
  const check = between('IEnumerator Check()', 'int InstalledVersionCode()');
  assert.match(check, /StartCoroutine\(DownloadUpdate\(true\)\)/);
  assert.doesNotMatch(check, /LaunchInstaller\s*\(/);
  const boot = between('IEnumerator Start()', 'void OnApplicationFocus(');
  assert.match(boot, /yield return Check\(\)/);
  assert.doesNotMatch(boot, /LaunchInstaller\s*\(/);
});
test('Automatic Wi-Fi only and explicit mobile-data opt-in', () => {
  const downloader = between('IEnumerator DownloadUpdate(bool automatic)', 'void LaunchInstaller()');
  assert.match(downloader, /automatic && !HasUnmeteredNetwork\(\)/);
  assert.match(downloader, /setAllowedOverMetered", !automatic/);
  assert.match(downloader, /setDestinationInExternalFilesDir/);
  assert.doesNotMatch(downloader, /LaunchInstaller\s*\(/);
  const ui = between('void OnGUI()', 'IEnumerator DownloadUpdate(bool automatic)');
  assert.match(ui, /if \(downloaded\) LaunchInstaller\(\)/);
  assert.match(ui, /DownloadUpdate\(false\)/);
});
test('Resume transfer without touching game saves', () => {
  assert.match(source, /download-id/);
  assert.match(source, /DownloadStatus\(manager, downloadId,/);
  assert.match(source, /Task[.]Run\(/);
  assert.match(source, /EldoriaAndroidUpdatePolicy[.]VerifyApk\(apkPath, candidate\)/);
  assert.doesNotMatch(source, /PlayerPrefs|Gateway[.]|LocalGateway|GameState[.]|DeleteAll/);
});
test('Installer needs explicit Android permission and the channel is not silently enabled', () => {
  const installer = source.slice(source.indexOf('void LaunchInstaller()'));
  assert.match(installer, /canRequestPackageInstalls/);
  assert.match(installer, /getUriForDownloadedFile/);
  assert.match(installer, /startActivity/);
  const channel = JSON.parse(fs.readFileSync('updates/android/family.json', 'utf8'));
  if (channel.enabled !== false) assert.ok(channel.versionCode >= 8, 'Cannot activate stale release');
});

test('Update prompt uses an opaque modal card rather than Unity transparent default skin', () => {
  const panel = between('static void DrawSolid(', 'IEnumerator DownloadUpdate(bool automatic)');
  assert.match(panel, /GUI[.]DrawTexture\(area,\s*Texture2D[.]whiteTexture\)/);
  assert.doesNotMatch(panel, /GUI[.]Box\s*\(/, 'Default GUI.Box skin allows world HUD to show through');
  assert.match(panel, /new Color\(0[.]075f,\s*0[.]095f,\s*0[.]135f,\s*1f\)/, 'Solid dark card with full alpha');
  assert.match(panel, /new Color\(0[.]76f,\s*0[.]60f,\s*0[.]35f,\s*1f\)/, 'Solid gold outline');
  assert.match(panel, /headline[.]normal[.]textColor = Color[.]white/);
  assert.match(panel, /description[.]normal[.]textColor/);
  assert.match(panel, /bool landscape = Screen[.]width > Screen[.]height/);
  assert.match(panel, /GUI[.]matrix = previousMatrix/);
  assert.match(panel, /GUI[.]depth = previousDepth/);
  assert.match(panel, /GUI[.]Button\(secondary,\s*"Ahora no"/);
  assert.match(panel, /if \(downloaded\) LaunchInstaller\(\)/);
});
