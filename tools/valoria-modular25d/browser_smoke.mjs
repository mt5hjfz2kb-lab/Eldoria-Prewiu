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
  sourceSha: process.env.ELDORIA_PLAYER_SHA || process.env.GITHUB_SHA || 'UNPINNED',
  testScriptSha: process.env.GITHUB_SHA || 'UNPINNED',
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
// This is a readiness probe, not a grade for the scene's visual quality.
// Use corner grass colours to detect the startup fade in real Unity WebGL.
function topRowStats(bytes) {
  const png=PNG.sync.read(bytes), w=png.width, h=png.height;
  let edge=0,edgeN=0,center=0,centerN=0;
  const edgeW=Math.max(6,Math.floor(w*.025));
  const rows=Math.max(10,Math.floor(h*.018));
  for(let y=0;y<rows;y+=2)for(let x=0;x<w;x+=2) {
    const i=(y*w+x)*4;
    const l=(png.data[i]+png.data[i+1]+png.data[i+2])/3;
    if(x<edgeW || x>=w-edgeW){edge+=l;edgeN++;}
    else if(x>=w*.18 && x<=w*.82){center+=l;centerN++;}
  }
  const edgeLuma=edge/Math.max(1,edgeN);
  const centerLuma=center/Math.max(1,centerN);
  return {edgeLuma,centerLuma,edgeMinusCenter:edgeLuma-centerLuma};
}
async function awaitUnfadedWorld(page,width,height,label) {
  // Real evidence from previous runs: Native meadow corner is ~100 luma;
  // browser startup fade ranged ~40–66 and brightened above 85.
  // A dark boot frame is NOT a black header or acceptable gameplay image.
  const reads=[];
  let previous=null;
  await page.locator('canvas').focus();
  for(let i=0;i<25;i++) {
    await page.waitForTimeout(750);
    const bytes=await page.screenshot({animations:'disabled',timeout:50_000});
    const sample=topRowStats(bytes);
    reads.push(+sample.edgeLuma.toFixed(1));
    if(sample.edgeLuma>=78 && previous!==null && previous>=78 &&
       Math.abs(sample.edgeLuma-previous)<10) {
      console.log('WEBGL_REAL_RENDER_UNFADED',width,height,label,
        'samples='+reads.join(','));
      return {final:sample,readings:reads};
    }
    previous=sample.edgeLuma;
  }
  throw Error(width+'x'+height+': World render stayed dark or unstable during '+label+
    ' after Unity loading finished; edge luminance='+reads.join(','));
}
const delay = ms => new Promise(ok => setTimeout(ok, ms));
// Compare exactly the on-canvas wood balance glyphs, without OCR or
// reading private Unity state. The top bar is at 390x844 reference safe
// coordinates; after Build, the balance must change and survive a REAL reload.
function woodBalancePixels(a, b) {
  const original=PNG.sync.read(a), current=PNG.sync.read(b);
  if(original.width!==current.width || original.height!==current.height)
    throw Error('Wood label screenshot dimensions changed');
  const w=original.width,h=original.height;
  // The REAL numeric wood glyph occupies x~.13-.23, not the entire capsule.
  // Verified against 44770528023 original build 230 -> 150 screenshots.
  const left=Math.round(w*.13),right=Math.round(w*.24);
  const top=Math.round(h*.10),bottom=Math.round(h*.13);
  let changed=0,total=0;
  for(let y=top;y<bottom;y++)for(let x=left;x<right;x++) {
    const i=(y*w+x)*4;
    const delta=Math.abs(original.data[i]-current.data[i])+
      Math.abs(original.data[i+1]-current.data[i+1])+
      Math.abs(original.data[i+2]-current.data[i+2]);
    if(delta>80)changed++;
    total++;
  }
  return {changed,total,ratio:changed/Math.max(1,total)};
}
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
    page.on('console', msg => {if(msg.type()==='error') errors.push('UNITY_BROWSER_CONSOLE: '+msg.text());});
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
    const firstReadiness=await awaitUnfadedWorld(page,width,height,'before-build');
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
    // A cropped HUD makes the *middle* of the top row darker than meadow
    // at both edges. Absolute black pixels are not evidence of clipping:
    // the 44697658660 browser failure was a partially faded game frame.
    const top=topRowStats(before.bytes);
    if(top.edgeMinusCenter>19)
      throw Error(width+'x'+height+': HUD top edge appears cropped: '+
        JSON.stringify(top));
    // Target the CENTER of the repositioned uGUI controls in 390x844 reference units,\n    // with bottom safe margin 36. These are still real Unity screen-space clicks.
    await page.mouse.click(box.x + box.width * (88 / 390), box.y + box.height * (1 - 50 / 844));
    await page.waitForTimeout(650);
    const opened = await capture('real-sawmill-build-panel');
    const change = pixelChange(before.bytes, opened.bytes);
    if (change.changedSamples < Math.max(120, Math.floor(change.totalSamples * .04)))
      throw Error(width + 'x' + height + ': real ASERRADERO click did not reveal its HUD panel (pixel delta too small)');
    // Click the actual CANONICAL command inside that same panel, not a mock API.
    await page.mouse.click(box.x + box.width * .5, box.y + box.height * (1 - 111 / 844));
    await page.waitForTimeout(800);
    const building = await capture('real-build-command');
    // Both frames have the identical open modal and full brightness;
    // only the Wood digits should change, not a startup fade.
    const paid=woodBalancePixels(opened.bytes,building.bytes);
    if(paid.changed<25 || paid.ratio<.034)
      throw Error(width+'x'+height+': Build did not change the real wood balance glyphs: '+paid.ratio.toFixed(3));
    await page.waitForTimeout(2500); // allow PlayerPrefs.Save + browser storage sync
    await page.reload({waitUntil:'domcontentloaded',timeout:60_000});
    await page.waitForFunction(()=>{
      const loading=document.querySelector('#unity-loading-bar');
      const canvas=document.querySelector('canvas');
      return canvas&&canvas.getBoundingClientRect().width>120 &&
        (!loading || getComputedStyle(loading).display==='none');
    },null,{timeout:200_000});
    const reloadReadiness=await awaitUnfadedWorld(page,width,height,'after-reload');
    const reloaded=await capture('real-after-browser-reload');
    const retained=woodBalancePixels(building.bytes,reloaded.bytes);
    if(retained.ratio>.055)
      throw Error(width+'x'+height+': Browser reload lost or altered paid wood balance: '+retained.ratio.toFixed(3));
        if (errors.length || failures.length)
      throw Error(width + 'x' + height + ': browser errors: ' + [...errors, ...failures].slice(0, 10).join(' | '));
    manifest.resolutions.push({
      width, height, context: scene, sawmillPanelPixelChange: change,
      inputActions: ['screen-space ASERRADERO button', 'screen-space CONSTRUIR button'],
      screenshots: [before, opened, building, reloaded].map(({ filename, sha256 }) => ({ filename, sha256 })),
      woodBalanceBuildChange: paid, browserReloadWoodRetention: retained,
      noPageError: true, noRequestFailures: true, topBarContrast: top,
      bootReadiness: firstReadiness, reloadReadiness,
      gameplayBuildPersistence: 'SCREEN_VERIFIED_WOOD_BALANCE_AFTER_REAL_BROWSER_RELOAD_NOT_DIRECT_IDBFS_INSPECTION'
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
