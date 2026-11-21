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
  // The newly top-pinned RESOURCE panels begin at reference y=8.
  // Compare only genuinely exposed grass pixels ABOVE that UI; sampling
  // y=8..15 would treat a dark resource capsule as a cropped world frame.
  const rows=Math.max(1,Math.floor(h/844)); // Above the 2-point safe-top inset.
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
    const image=PNG.sync.read(bytes);
    let sum=0,squares=0,count=0;
    for(let y=Math.floor(height*.28);y<height*.32;y+=3){
      for(let x=Math.floor(width*.032);x<width*.968;x+=3){
        if(x>width*.072 && x<width*.928)continue;
        const i=(y*width+x)*4;
        const l=.21*image.data[i]+.72*image.data[i+1]+.07*image.data[i+2];
        sum+=l;squares+=l*l;count++;
      }
    }
    const edgeLuma=sum/count;
    const deviation=Math.sqrt(Math.max(0,squares/count-edgeLuma*edgeLuma));
    const sample={edgeLuma,deviation};
    reads.push(+sample.edgeLuma.toFixed(1));
    if(deviation>3 && sample.edgeLuma>=78 && previous!==null && previous>=78 &&
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
// Geometrical visual acceptance with actual captured Unity browser pixels:
// the originally approved 7afd HUD had no painted resources in the upper-right
// 12..53px region at 390x844 (0% pixels below luma80); the new six-slot
// composition must put actual ornate resource capsules into the top row.
// This deliberately tests placement, not mechanical text or CSS mocks.
function topPinnedResources(bytes) {
  const png=PNG.sync.read(bytes);
  const w=png.width,h=png.height;
  let dark=0,total=0;
  for(let y=Math.round(h*.015);y<Math.round(h*.065);y+=2)
    for(let x=Math.round(w*.52);x<Math.round(w*.96);x+=2) {
      const i=(y*w+x)*4;
      const l=(png.data[i]+png.data[i+1]+png.data[i+2])/3;
      if(l<80)dark++;
      total++;
    }
  return {darkSamples:dark,totalSamples:total,
          darkFraction:dark/Math.max(1,total)};
}
function woodBalancePixels(a, b) {
  const original=PNG.sync.read(a), current=PNG.sync.read(b);
  if(original.width!==current.width || original.height!==current.height)
    throw Error('Wood label screenshot dimensions changed');
  const w=original.width,h=original.height;
  // Slender ONE-row economy strip: the original wood amount appears in
  // cell 0 at x~159..192, y~16..41 for the 390x844 reference.
  // This is a candidate crop from actual Unity source UI geometry;
  // exact WebGL screenshots must confirm 230 -> 150.
  const left=Math.round(w*.405),right=Math.round(w*.495);
  const top=Math.round(h*.018),bottom=Math.round(h*.050);
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
// The native left drawer slides out of x=51..297 in 390x844 and
// occupies the middle screen y~330..515. Compare real pixels only.
function drawerPixels(a,b) {
  const x=PNG.sync.read(a),y=PNG.sync.read(b);
  if(x.width!==y.width||x.height!==y.height)
    throw Error('Drawer screenshots have different dimensions');
  const w=x.width,h=x.height;
  let changed=0,total=0;
  for(let py=Math.floor(h*.34);py<Math.floor(h*.66);py+=3)
    for(let px=Math.floor(w*.15);px<Math.floor(w*.75);px+=3) {
      const k=(py*w+px)*4;
      const delta=Math.abs(x.data[k]-y.data[k])+
        Math.abs(x.data[k+1]-y.data[k+1])+
        Math.abs(x.data[k+2]-y.data[k+2]);
      if(delta>85)changed++;
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
  for (const [width, height, density] of [[390, 844, 1], [1170, 2532, 1], [390, 844, 3]]) {
    const ctx = await browser.newContext({
      viewport: { width, height }, isMobile: true, hasTouch: true, deviceScaleFactor: density,
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
    const firstReadiness=await awaitUnfadedWorld(page,width*density,height*density,'before-build');
    const scene = await page.evaluate(() => {
      const canvas = document.querySelector('canvas');
      const rect = canvas?.getBoundingClientRect();
      return {
        canvasWidth: rect?.width || 0,
        renderWidth: canvas?.width || 0,
        renderHeight: canvas?.height || 0,
        devicePixelRatio: window.devicePixelRatio, canvasHeight: rect?.height || 0,
        webglContext: Boolean(canvas && (canvas.getContext('webgl2') || canvas.getContext('webgl'))),
        visible: Boolean(rect && rect.width > 120 && rect.height > 200)
      };
    });
    if (!scene.visible || !scene.webglContext) throw Error(width + 'x' + height + ': no real visible Unity WebGL canvas/context');
    if(scene.renderWidth!==width*density || scene.renderHeight!==height*density)
      throw Error('Render target does not match mobile pixel density '+JSON.stringify(scene));
    const box = await page.locator('canvas').boundingBox();
    if (!box) throw Error('Actual Unity canvas has no on-screen bounds');
    if (box.width > width * 1.05 || box.height > height * 1.05)
      throw Error(width + 'x' + height + ': WebGL canvas overflows responsive mobile viewport');
    const capture = async label => {
      const file = path.join(out, width + 'x' + height + (density>1?'-dpr'+density:'') + '-' + label + '.png');
      const bytes = await page.screenshot({ path: file, animations: 'disabled', timeout: 50_000 });
      if (bytes.length < 10_000) throw Error('Empty/broken WebGL screenshot: ' + file);
      return { filename: path.basename(file), sha256: sha(bytes), bytes };
    };
    const before = await capture('real-world-hud');
    const economyTop=topPinnedResources(before.bytes);
    if(economyTop.darkFraction<.18)
      throw Error(width+'x'+height+': Corona economy is NOT visually attached to the top edge: '+
        JSON.stringify(economyTop));
    // A cropped HUD makes the *middle* of the top row darker than meadow
    // at both edges. Absolute black pixels are not evidence of clipping:
    // the 44697658660 browser failure was a partially faded game frame.
    const top=topRowStats(before.bytes);
    if(top.edgeMinusCenter>19)
      throw Error(width+'x'+height+': HUD top edge appears cropped: '+
        JSON.stringify(top));
    // Target the CENTER of the repositioned uGUI controls in 390x844 reference units,\n    // with bottom safe margin 36. These are still real Unity screen-space clicks.
    // Eight-slot HUD: OBRAS occupies the last stable cell (slot 7).
    // Source UI NavigationCell(7,390,0,0,8,false) has x center ~360
    // and y center ~56 from the bottom. Click its REAL Unity button.
    await page.mouse.click(box.x + box.width * (360 / 390),
                           box.y + box.height * (1 - 56 / 844));
    await page.waitForTimeout(650);
    const opened = await capture('real-sawmill-build-panel');
    const change = pixelChange(before.bytes, opened.bytes);
    if (change.changedSamples < Math.max(120, Math.floor(change.totalSamples * .04)))
      throw Error(width + 'x' + height + ': real ASERRADERO click did not reveal its HUD panel (pixel delta too small)');
    // Click the actual CANONICAL command inside that same panel, not a mock API.
    // Corona's large separately framed detail sheet sits 26 reference pixels above Umbral.
    // Its real 42px command hitbox center is around y=145 from the safe bottom.
    await page.mouse.click(box.x + box.width * .5, box.y + box.height * (1 - 145 / 844));
    await page.waitForTimeout(800);
    const building = await capture('real-build-command');
    // A successful authoritative Build must dismiss the panel. Check the
    // ORIGINAL real screenshot surface at the panel's center changed back
    // to the rendered world, while wood was spent exactly once.
    const closed= pixelChange(opened.bytes,building.bytes);
    if(closed.changedSamples<Math.max(120,Math.floor(closed.totalSamples*.04)))
      throw Error(width+'x'+height+': successful Build failed to dismiss the actual construction panel');
    const paid=woodBalancePixels(opened.bytes,building.bytes);
    if(paid.changed<25 || paid.ratio<.034)
      throw Error(width+'x'+height+': Build did not change the real wood balance glyphs: '+paid.ratio.toFixed(3));
    // User-requested real left-side construction drawer. It must stay
    // CLOSED after Build, open only by tapping the narrow visible handle,
    // and then collapse again without a second game command or cost.
    await page.mouse.click(box.x+box.width*(26/390),box.y+box.height*.5);
    await page.waitForTimeout(450);
    const drawerOpened=await capture('real-left-construction-drawer-open');
    const drawerSpread=drawerPixels(building.bytes,drawerOpened.bytes);
    if(drawerSpread.ratio<.12)
      throw Error(width+'x'+height+': left OBRA drawer did not expand from its hidden position: '+drawerSpread.ratio.toFixed(3));
    await page.mouse.click(box.x+box.width*(26/390),box.y+box.height*.5);
    await page.waitForTimeout(400);
    const drawerClosed=await capture('real-left-construction-drawer-closed');
    const drawerResidual=drawerPixels(building.bytes,drawerClosed.bytes);
    if(drawerResidual.ratio>drawerSpread.ratio*.52)
      throw Error(width+'x'+height+': left OBRA drawer remained over the city after collapsing');
    await page.waitForTimeout(2500); // PlayerPrefs.Save and browser storage sync
    await page.reload({waitUntil:'domcontentloaded',timeout:60_000});
    await page.waitForFunction(()=>{
      const loading=document.querySelector('#unity-loading-bar');
      const canvas=document.querySelector('canvas');
      return canvas&&canvas.getBoundingClientRect().width>120 &&
        (!loading || getComputedStyle(loading).display==='none');
    },null,{timeout:200_000});
    const reloadReadiness=await awaitUnfadedWorld(page,width*density,height*density,'after-reload');
    const reloaded=await capture('real-after-browser-reload');
    // Capture the real finished architecture after the normal six-second construction.
    // Chromium reload and startup may already have consumed the entire interval.
    await page.waitForTimeout(7000);
    const completed=await capture('real-completed-sawmill');
    await page.mouse.click(box.x+box.width*(352/390),box.y+box.height*(1-56/844));
    await page.waitForTimeout(500);
    const completedPanel=await capture('real-completed-sawmill-panel');
    const retained=woodBalancePixels(building.bytes,reloaded.bytes);
    const distinctFromStartingBalance=woodBalancePixels(before.bytes,reloaded.bytes);
    if(retained.changed>=paid.changed*.5 || distinctFromStartingBalance.changed<paid.changed*.5)
      throw Error(width+'x'+height+': Browser reload lost or altered paid wood balance: '+retained.ratio.toFixed(3));
        if (errors.length || failures.length)
      throw Error(width + 'x' + height + ': browser errors: ' + [...errors, ...failures].slice(0, 10).join(' | '));
    manifest.resolutions.push({
      width, height, density, context: scene, sawmillPanelPixelChange: change,
      inputActions: ['screen-space ASERRADERO button', 'screen-space CONSTRUIR button'],
      screenshots: [before, opened, building, drawerOpened, drawerClosed, reloaded, completed, completedPanel].map(({ filename, sha256 }) => ({ filename, sha256 })),
      collapsedByDefaultAndDrawerToggle: {opened:drawerSpread,closed:drawerResidual},
      woodBalanceBuildChange: paid, browserReloadWoodRetention: retained,
      successfulBuildClosedModal: closed,
      noPageError: true, noRequestFailures: true, topBarContrast: top,
      topPinnedPaintedResourceCoverage:economyTop,
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


