// Chromium gesture QA on a real, immutable, SHA-pinned Unity WebGL artifact.
// This is a separate zero-Windows-cost probe. No fake camera transformations,
// no data read from the player save, and no physical-iPhone certification.
import fs from 'node:fs';
import http from 'node:http';
import path from 'node:path';
import { createHash } from 'node:crypto';
import { chromium } from 'playwright-core';
import { PNG } from 'pngjs';

const [sourcePath,outPath]=process.argv.slice(2);
if(!sourcePath||!outPath) throw Error('Usage: browser_camera_smoke.mjs <WebGL> <evidence>');
const source=path.resolve(sourcePath), output=path.resolve(outPath);
if(!fs.existsSync(path.join(source,'index.html')))
  throw Error('No real Unity WebGL build; camera gesture QA refused');
fs.mkdirSync(output,{recursive:true});
const binary=[process.env.CHROME_PATH,'/usr/bin/google-chrome','/usr/bin/google-chrome-stable','/usr/bin/chromium']
  .filter(Boolean).find(p=>fs.existsSync(p));
if(!binary)throw Error('Chromium unavailable; no fake touch/camera QA');
const mime={'.wasm':'application/wasm','.js':'application/javascript','.data':'application/octet-stream',
  '.html':'text/html','.json':'application/json','.css':'text/css','.png':'image/png','.ico':'image/x-icon'};
const server=http.createServer((req,res)=>{
  const url=new URL(req.url,'http://localhost');
  const relative=decodeURIComponent(url.pathname).replace(/^\/+/,'')||'index.html';
  const file=path.resolve(source,relative);
  if((file!==path.join(source,'index.html') && !file.startsWith(source+path.sep))||
     relative.includes('..')||!fs.existsSync(file)||!fs.statSync(file).isFile()){
    res.writeHead(404);res.end();return;
  }
  const comp=file.endsWith('.gz')?'.gz':file.endsWith('.br')?'.br':'';
  if(comp)res.setHeader('Content-Encoding',comp==='.gz'?'gzip':'br');
  const name=comp?file.slice(0,-comp.length):file;
  res.setHeader('Content-Type',mime[path.extname(name)]||'application/octet-stream');
  res.setHeader('Cache-Control','no-store');res.writeHead(200);
  fs.createReadStream(file).pipe(res);
});
await new Promise((resolve,reject)=>{server.once('error',reject);server.listen(0,'127.0.0.1',resolve)});
const url='http://127.0.0.1:'+server.address().port+'/index.html';
let browser;
const manifest={status:'NOT_TESTED',gameSourceSha:process.env.ELDORIA_PLAYER_SHA||'UNPINNED',
 testScriptSha:process.env.GITHUB_SHA||'UNPINNED',engine:'Actual Unity WebGL Chromium camera gestures',
 device:'Chromium synthetic mouse; NOT real iPhone',iPhoneTested:false,visualApproved:false,cases:[]};
function sha(x){return createHash('sha256').update(x).digest('hex')}
function imageDifference(before,after,area){
  const a=PNG.sync.read(before),b=PNG.sync.read(after);
  if(a.width!==b.width||a.height!==b.height)throw Error('Different viewport dimensions');
  const [x0,y0,x1,y1]=area;let samples=0,changed=0,sum=0;
  for(let y=y0;y<Math.min(y1,a.height);y+=3)
  for(let x=x0;x<Math.min(x1,a.width);x+=3){
    const i=(y*a.width+x)*4;
    const d=Math.abs(a.data[i]-b.data[i])+Math.abs(a.data[i+1]-b.data[i+1])+
      Math.abs(a.data[i+2]-b.data[i+2]);
    if(d>70)changed++;
    sum+=d/3;samples++;
  }
  return {meanRgbDifference:sum/Math.max(1,samples),changedFraction:changed/Math.max(1,samples),samples};
}
function edgeBrightness(bytes){
  const im=PNG.sync.read(bytes);let sum=0,n=0;
  for(let y=1;y<12;y+=2)for(let x=10;x<im.width-10;x+=7){
    const i=(y*im.width+x)*4;
    sum+=.21*im.data[i]+.72*im.data[i+1]+.07*im.data[i+2];n++;
  }
  return sum/Math.max(1,n);
}
async function capture(page,label,width,height){
  const file=path.join(output,width+'x'+height+'-'+label+'.png');
  const bytes=await page.screenshot({path:file,animations:'disabled',timeout:50_000});
  if(bytes.length<10_000)throw Error('Empty actual browser screenshot '+label);
  return {bytes,name:path.basename(file),sha256:sha(bytes)};
}
async function waitWorldReady(page){
  await page.waitForFunction(()=>{
    const c=document.querySelector('canvas'),l=document.querySelector('#unity-loading-bar');
    return c && c.getBoundingClientRect().width>120 &&
       (!l||getComputedStyle(l).display==='none');
  },null,{timeout:200_000});
  let previous=0;
  for(let i=0;i<24;i++){
    await page.waitForTimeout(750);
    const current=edgeBrightness(await page.screenshot({animations:'disabled'}));
    if(current>75 && previous>75 && Math.abs(current-previous)<10)return;
    previous=current;
  }
  throw Error('Unity boot fade not over; refuse camera comparison');
}
try{
  if(!(await fetch(url)).ok)throw Error('HTTP did not serve actual WebGL');
  browser=await chromium.launch({headless:true,executablePath:binary,
    args:['--no-sandbox','--disable-dev-shm-usage','--use-gl=angle',
      '--use-angle=swiftshader','--enable-unsafe-swiftshader']});
  for(const [width,height] of [[390,844],[844,390]]){
    const ctx=await browser.newContext({viewport:{width,height},isMobile:true,hasTouch:true,deviceScaleFactor:1,
      userAgent:'Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Mobile Safari/537.36'});
    const page=await ctx.newPage(),errors=[];
    page.on('pageerror',e=>errors.push(e.message));
    page.on('console',m=>{if(m.type()==='error')errors.push(m.text())});
    await page.goto(url,{waitUntil:'domcontentloaded',timeout:60_000});
    await waitWorldReady(page);
    const box=await page.locator('canvas').boundingBox();
    if(!box || Math.abs(box.width-width)>3 || Math.abs(box.height-height)>3)
      throw Error('Canvas does not fit exact mobile viewport '+width+'x'+height);
    const before=await capture(page,'camera-home',width,height);
    // Real browser mouse drag. The game must handle input in its own Update.
    const x=Math.round(width*.53),y=Math.round(height*.42);
    await page.mouse.move(x,y);await page.mouse.down();
    await page.mouse.move(x+Math.round(width*.19),y+Math.round(height*.13),{steps:16});
    await page.mouse.up();await page.waitForTimeout(700);
    const panned=await capture(page,'camera-panned',width,height);
    await page.mouse.move(Math.round(width*.49),Math.round(height*.42));
    await page.mouse.wheel(0,-460);await page.waitForTimeout(900);
    const zoomed=await capture(page,'camera-zoomed',width,height);
    const roi=[Math.round(width*.09),Math.round(height*.22),Math.round(width*.91),Math.round(height*.83)];
    const p= imageDifference(before.bytes,panned.bytes,roi);
    const z= imageDifference(before.bytes,zoomed.bytes,roi);
    if(p.changedFraction<.11 || p.meanRgbDifference<9)
      throw Error(width+'x'+height+': dragging did not move actual world enough: '+JSON.stringify(p));
    if(z.changedFraction<.13 || z.meanRgbDifference<10)
      throw Error(width+'x'+height+': wheel zoom did not change world enough: '+JSON.stringify(z));
    let home=null,h=null;
    if(height>width){
      // Actual bottom-right uGUI CENTRAR button; no synthetic camera Home call.
      await page.mouse.click(Math.round(width*.78),Math.round(height*.917));
      await page.waitForTimeout(800);
      home=await capture(page,'camera-home-restored',width,height);
      h=imageDifference(before.bytes,home.bytes,roi);
      if(h.changedFraction>=Math.min(p.changedFraction,z.changedFraction)*.64)
        throw Error('Real CENTRAR did not restore home frame: '+JSON.stringify({p,z,h}));
    }
    if(errors.length)throw Error('Browser/Unity JS error: '+errors.slice(0,5).join(' | '));
    manifest.cases.push({width,height,panning:p,zoom:z,returnHome:h,
      assertions:'real mouse drag and scroll + physical uGUI center action in portrait',
      frames:[before,panned,zoomed,...(home?[home]:[])].map(({name,sha256})=>({name,sha256}))});
    await ctx.close();
  }
  manifest.status='REAL_BROWSER_PAN_ZOOM_AND_HOME_PASS_NO_IPHONE_CERTIFICATION';
}catch(error){
  manifest.status='FAILED';manifest.failure=String(error?.stack||error);throw error;
}finally{
  if(browser)await browser.close();
  await new Promise(resolve=>server.close(resolve));
  fs.writeFileSync(path.join(output,'camera-evidence.json'),JSON.stringify(manifest,null,2)+'\n');
}
