// Isolated source-pinned WebGL player: real Chromium canvas/UI evidence.
// This is not an iPhone performance, memory or artistic approval certificate.
import fs from 'node:fs';
import path from 'node:path';
import { spawn } from 'node:child_process';
import { createHash } from 'node:crypto';
import { chromium } from 'playwright-core';
import { PNG } from 'pngjs';

const [folder, target] = process.argv.slice(2);
if (!folder || !target) throw Error('Usage: node browser_smoke.mjs <WebGL-build-directory> <evidence-directory>');
const webRoot = path.resolve(folder), out = path.resolve(target);
if (!fs.existsSync(path.join(webRoot, 'index.html'))) throw Error('No Unity WebGL index.html; refuse screenshot simulation');
fs.mkdirSync(out, { recursive: true });
const chrome = [process.env.CHROME_PATH, '/usr/bin/google-chrome', '/usr/bin/google-chrome-stable', '/usr/bin/chromium']
  .filter(Boolean).find(fs.existsSync);
if (!chrome) throw Error('Chromium/Chrome not installed on ephemeral runner; browser QA blocked, not passed');

const port = 18757;
const url = 'http://127.0.0.1:' + port + '/index.html';
const server = spawn('python3', ['-m', 'http.server', String(port), '--bind', '127.0.0.1', '--directory', webRoot],
  { stdio: ['ignore', 'pipe', 'pipe'] });
const manifest = {
  status: 'NOT_YET_TESTED',
  sourceSha: process.env.GITHUB_SHA || 'UNPINNED',
  engine: 'Unity WebGL real browser player',
  browser: 'Chrome/Chromium headless with SwiftShader',
  realGameView: false,
  actualBrowserHUD: false,
  physicalIphone: 'NOT_TESTED',
  artApproved: false,
  memoryFpsCertified: false,
  caveats: 'Rendered browser canvas, not Editor GameView or physical Safari; HUD click smoke only, no complete gameplay traversal',
  resolutions: []
};
let browser;
const sha = bytes => createHash('sha256').update(bytes).digest('hex');
const delay = ms => new Promise(ok => setTimeout(ok, ms));
function pixelChange(a, b) {
  const old = PNG.sync.read(a), next = PNG.sync.read(b);
  if (old.width !== next.width || old.height !== next.height) throw Error('Screenshot sizes differ');
  let changes = 0, checked = 0;
  // Actual animated world alone cannot paint an opaque central build panel:
  // count only its fixed central, lower-middle screen region.
  for (let y = Math.floor(old.height * .64); y < Math.floor(old.height * .82); y += 3) {
    for (let x = Math.floor(old.width * .13); x < Math.floor(old.width * .87); x += 3) {
      const i = (y * old.width + x) * 4;
      const diff = Math.abs(old.data[i] - next.data[i]) +
        Math.abs(old.data[i + 1] - next.data[i + 1]) +
        Math.abs(old.data[i + 2] - next.data[i + 2]);
      if (diff > 65) changes++;
      checked++;
    }
  }
  return { changedSamples: changes, totalSamples: checked, ratio: changes / Math.max(1, checked) };
}
try {
  let ready = false;
  for (let n = 0; n < 90; n++) {
    if (server.exitCode !== null) throw Error('Local HTTP server failed before browser test');
    try {
      const r = await fetch(url);
      if (r.ok) { ready = true; break; }
    } catch {}
    await delay(250);
  }
  if (!ready) throw Error('WebGL local HTTP server did not respond');
  browser = await chromium.launch({
    headless: true, executablePath: chrome,
    args: ['--no-sandbox', '--disable-dev-shm-usage', '--use-gl=angle', '--use-angle=swiftshader',
      '--enable-unsafe-swiftshader']
  });
  for (const [width, height] of [[390, 844], [1170, 2532]]) {
    const ctx = await browser.newContext({
      viewport: { width, height }, isMobile: true, hasTouch: true, deviceScaleFactor: 1
    });
    const page = await ctx.newPage();
    const errors = [], failures = [];
    page.on('pageerror', e => errors.push(e.message));
    page.on('requestfailed', req => failures.push(req.url() + ': ' + (req.failure()?.errorText || 'network failure')));
    page.on('response', resp => {
      if (resp.status() >= 400 && !resp.url().endsWith('/favicon.ico'))
        failures.push(resp.status() + ': ' + resp.url());
    });
    await page.goto(url, { timeout: 60_000, waitUntil: 'domcontentloaded' });
    await page.waitForFunction(() => {
      const loading = document.querySelector('#unity-loading-bar');
      const canvas = document.querySelector('canvas');
      return canvas && canvas.getBoundingClientRect().width > 120 &&
        (!loading || getComputedStyle(loading).display === 'none');
    }, null, { timeout: 200_000 });
    await page.waitForTimeout(2000);
    const scene = await page.evaluate(() => {
      const canvas = document.querySelector('canvas');
      const rect = canvas?.getBoundingClientRect();
      return {
        canvasWidth: rect?.width || 0, canvasHeight: rect?.height || 0,
        webglContext: Boolean(canvas && (canvas.getContext('webgl2') || canvas.getContext('webgl'))),
        visible: Boolean(rect && rect.width > 120 && rect.height > 200)
      };
    });
    if (!scene.visible || !scene.webglContext) throw Error(width + 'x' + height + ': no real visible Unity WebGL canvas/context');
    const box = await page.locator('canvas').boundingBox();
    if (!box) throw Error('Actual Unity canvas has no on-screen bounds');
    const capture = async label => {
      const file = path.join(out, width + 'x' + height + '-' + label + '.png');
      const bytes = await page.screenshot({ path: file, animations: 'disabled', timeout: 50_000 });
      if (bytes.length < 10_000) throw Error('Empty/broken WebGL screenshot: ' + file);
      return { filename: path.basename(file), sha256: sha(bytes), bytes };
    };
    const before = await capture('real-world-hud');
    // Coordinates come from the actual 390x844 Unity uGUI anchors, not HTML or fake controls.
    await page.mouse.click(box.x + box.width * (88 / 390), box.y + box.height * (1 - 47 / 844));
    await page.waitForTimeout(650);
    const opened = await capture('real-sawmill-build-panel');
    const change = pixelChange(before.bytes, opened.bytes);
    if (change.changedSamples < Math.max(120, Math.floor(change.totalSamples * .04)))
      throw Error(width + 'x' + height + ': real ASERRADERO click did not reveal its HUD panel (pixel delta too small)');
    // Click the actual CANONICAL command inside that same panel, not a mock API.
    await page.mouse.click(box.x + box.width * .5, box.y + box.height * (1 - 111 / 844));
    await page.waitForTimeout(800);
    const building = await capture('real-build-command');
    if (errors.length || failures.length)
      throw Error(width + 'x' + height + ': browser errors: ' + [...errors, ...failures].slice(0, 10).join(' | '));
    manifest.resolutions.push({
      width, height, context: scene, sawmillPanelPixelChange: change,
      inputActions: ['screen-space ASERRADERO button', 'screen-space CONSTRUIR button'],
      screenshots: [before, opened, building].map(({ filename, sha256 }) => ({ filename, sha256 })),
      noPageError: true, noRequestFailures: true,
      gameplayBuildPersistence: 'NOT_VERIFIED_BY_THIS_BROWSER_TEST'
    });
    await ctx.close();
  }
  manifest.status = 'BROWSER_UI_SMOKE_PASSED_NOT_DEVICE_QA';
  manifest.actualBrowserHUD = true;
} catch (e) {
  manifest.status = 'FAILED';
  manifest.failure = String(e?.stack || e);
  throw e;
} finally {
  if (browser) await browser.close();
  server.kill('SIGTERM');
  fs.writeFileSync(path.join(out, 'browser-evidence.json'), JSON.stringify(manifest, null, 2) + '\n');
}
