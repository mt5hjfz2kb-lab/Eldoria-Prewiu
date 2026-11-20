// Isolated source-pinned WebGL player: real Chromium canvas/UI evidence.
// This is not an iPhone performance, memory or artistic approval certificate.
import fs from 'node:fs';
import path from 'node:path';
import http from 'node:http';
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

// Unity gzip/Brotli builds require explicit Content-Encoding and wasm MIME;
// Python http.server lacks these. Host only the exact downloaded artifact.
const mime = {
  '.html': 'text/html; charset=utf-8',
  '.js': 'application/javascript',
  '.wasm': 'application/wasm',
  '.data': 'application/octet-stream',
  '.json': 'application/json',
  '.css': 'text/css',
  '.png': 'image/png',
  '.ico': 'image/x-icon'
};
const server = http.createServer((req, res) => {
  let relative;
  try { relative = decodeURIComponent(new URL(req.url, 'http://localhost').pathname).replace(/^\/+/, ''); }
  catch { res.writeHead(400); res.end(); return; }
  const file = path.resolve(webRoot, relative || 'index.html');
  if (!file.startsWith(webRoot + path.sep) || !fs.existsSync(file) || !fs.statSync(file).isFile()) {
    res.writeHead(404); res.end('File not in source-pinned WebGL artifact'); return;
  }
  const compressed = file.endsWith('.gz') ? '.gz' : file.endsWith('.br') ? '.br' : '';
  const base = compressed ? file.slice(0, -compressed.length) : file;
  if (compressed) res.setHeader('Content-Encoding', compressed === '.gz' ? 'gzip' : 'br');
  res.setHeader('Content-Type', mime[path.extname(base)] || 'application/octet-stream');
  res.setHeader('Cache-Control', 'no-store');
  res.writeHead(200);
  fs.createReadStream(file).pipe(res);
});
await new Promise((resolve, reject) => {
  server.once('error', reject);
  server.listen(0, '127.0.0.1', resolve);
});
const url = 'http://127.0.0.1:' + server.address().port + '/index.html';
const manifest = {
  status: 'NOT_YET_TESTED',
  sourceSha: process.env.GITHUB_SHA || 'UNPINNED',
  engine: 'Unity WebGL real browser player',
  browser: 'Chrome/Chromium headless with SwiftShader',
  hosting: 'temporary same-origin HTTP server with Unity gzip/Brotli and WebAssembly MIME headers',
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
  const ready = await fetch(url);
  if (!ready.ok) throw Error('Isolated WebGL local HTTP returned ' + ready.status);
  browser = await chromium.launch({
    headless: true, executablePath: chrome,
    args: ['--no-sandbox', '--disable-dev-shm-usage', '--use-gl=angle', '--use-angle=swiftshader',
      '--enable-unsafe-swiftshader']
  });
  for (const [width, height] of [[390, 844], [1170, 2532]]) {
    const ctx = await browser.newContext({
      viewport: { width, height }, isMobile: true, hasTouch: true, deviceScaleFactor: 1,
      // Unity's default WebGL template switches to responsive mobile CSS by UA.
      // isMobile alone changes viewport emulation, not navigator.userAgent.
      userAgent: 'Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Mobile Safari/537.36'
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
    if (box.width > width * 1.05 || box.height > height * 1.05)
      throw Error(width + 'x' + height + ': WebGL canvas overflows responsive mobile viewport');
    const capture = async label => {
      const file = path.join(out, width + 'x' + height + '-' + label + '.png');
      const bytes = await page.screenshot({ path: file, animations: 'disabled', timeout: 50_000 });
      if (bytes.length < 10_000) throw Error('Empty/broken WebGL screenshot: ' + file);
      return { filename: path.basename(file), sha256: sha(bytes), bytes };
    };
    const before = await capture('real-world-hud');
    // Regression for the real 390x844 WebGL screenshot from run 44651309304:
    // the top half of BASTIÓN I was cropped by an off-screen HUD strip.
    // It was visible as a dark full-width bar touching screen row zero.
    // A mobile safe-area-compliant panel must not cover the first 16 px.
    const first=PNG.sync.read(before.bytes);
    let blackSamples=0, totalTop=0;
    for(let yy=0; yy<Math.min(16,first.height); yy+=2)
      for(let xx=Math.floor(first.width*.08);xx<Math.floor(first.width*.92);xx+=3) {
        const offset=(yy*first.width+xx)*4;
        if(first.data[offset]<48 && first.data[offset+1]<48 && first.data[offset+2]<48)
          blackSamples++;
        totalTop++;
      }
    const topBlackRatio=blackSamples/Math.max(1,totalTop);
    if(topBlackRatio>.3)
      throw Error(width+'x'+height+': HUD intrudes into top safe-area; dark top pixels '+topBlackRatio.toFixed(3));
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
      noPageError: true, noRequestFailures: true, topBlackRatio,
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
  await new Promise(resolve => server.close(resolve));
  fs.writeFileSync(path.join(out, 'browser-evidence.json'), JSON.stringify(manifest, null, 2) + '\n');
}
