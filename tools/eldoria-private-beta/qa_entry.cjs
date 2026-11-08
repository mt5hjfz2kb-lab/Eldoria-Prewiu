const {chromium}=require('playwright');const fs=require('fs');
(async()=>{
fs.mkdirSync('qa-entry',{recursive:true});
const launchArgs=process.platform==='win32'?['--no-sandbox','--use-angle=d3d11','--ignore-gpu-blocklist','--enable-webgl']:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--disable-dev-shm-usage'];
const browser=await chromium.launch({headless:true,executablePath:process.env.VALORIA_CHROMIUM_EXECUTABLE||undefined,args:launchArgs});
const context=await browser.newContext({viewport:{width:390,height:844},deviceScaleFactor:3});
const page=await context.newPage();const errors=[],logs=[];const report={physicalIphone:false,nickname:'Beta QA',errors,logs,stages:[]};
page.on('pageerror',e=>errors.push(String(e)));page.on('crash',()=>errors.push('PAGE_CRASH'));page.on('console',m=>logs.push(m.text()));
const checkpoint=(step)=>{report.lastStep=step;fs.writeFileSync('qa-entry/report.json',JSON.stringify(report,null,2));console.log('ENTRY_QA',step)};
const watchdog=setTimeout(()=>{report.failure='Entry QA exceeded 450-second deadline';report.passed=false;checkpoint('watchdog');process.exit(1)},450000);
const bounded=(promise,ms,label)=>{let timer;return Promise.race([promise,new Promise((_,reject)=>{timer=setTimeout(()=>reject(Error(label+' timeout')),ms)})]).finally(()=>clearTimeout(timer))};
const shot=async(name)=>{
 checkpoint('capture '+name);
 const data=await bounded(page.evaluate(()=>new Promise((resolve,reject)=>{
  const original=window.requestAnimationFrame;let captured=false;
  const restore=()=>{window.requestAnimationFrame=original;};
  const timer=setTimeout(()=>{restore();reject(Error('No rendered frame for capture'))},15000);
  window.requestAnimationFrame=function(callback){
   return original.call(window,function(time){
    callback(time);
    if(!captured && callback.name==='Browser_mainLoop_runner'){
     const canvas=document.getElementById('unity-canvas');const probe=document.createElement('canvas');probe.width=probe.height=16;const ctx=probe.getContext('2d');ctx.drawImage(canvas,0,0,16,16);const pixels=ctx.getImageData(0,0,16,16).data;let low=255,high=0;for(let i=0;i<pixels.length;i+=4){low=Math.min(low,pixels[i],pixels[i+1],pixels[i+2]);high=Math.max(high,pixels[i],pixels[i+1],pixels[i+2]);}if(high-low<15)return;
     captured=true;restore();clearTimeout(timer);
     try{resolve(document.getElementById('unity-canvas').toDataURL('image/png'))}catch(e){reject(e);}
    }
   });
  };
 })),20000,'rendered-frame readback');
 if(!data.startsWith('data:image/png;base64,'))throw Error('Invalid rendered frame');
 fs.writeFileSync('qa-entry/'+name+'.png',Buffer.from(data.split(',')[1],'base64'));
 report.captureMethod='Read actual Unity canvas synchronously after its animation-frame callback; original scheduler restored';
};
try{
 checkpoint('load');await page.goto(process.env.QA_URL||'http://127.0.0.1:8765/mobile/?eldoria_diag=1');await page.locator('#play').click();
 await page.waitForFunction(()=>!!window.valoriaGame,null,{timeout:240000});await page.waitForTimeout(45000);
 checkpoint('welcome-ready');
 // Coordinates verified against 390x844 capture and RoyalPlayerNamePrompt anchors.
 checkpoint('enter-name');await page.mouse.click(195,490,{delay:200});await page.keyboard.type(report.nickname,{delay:140});await page.mouse.click(195,614,{delay:200});checkpoint('name-clicked');
 await page.waitForTimeout(60000);checkpoint('after-name-60s');
 report.stages.push(await page.evaluate(()=>({stage:'after-name-60s',ready:!!window.valoriaGame,heapBytes:window.valoriaGame?.Module?.HEAPU8?.byteLength})));
 report.persistenceVerification='Separate OCR gate verifies visible nickname after actual browser reload';
 checkpoint('quit-runtime');await bounded(page.evaluate(()=>window.valoriaGame.Quit()),30000,'Unity Quit');
 report.reopenMethod='Documented Unity Quit then actual page reload; raw reload timed out in separate SwiftShader run42423325534';
 checkpoint('reload');await page.reload();await page.locator('#play').click();await page.waitForFunction(()=>!!window.valoriaGame,null,{timeout:240000});await page.waitForTimeout(45000);
 await shot('03-reloaded');report.stages.push(await page.evaluate(()=>({stage:'reload-45s',ready:!!window.valoriaGame,heapBytes:window.valoriaGame?.Module?.HEAPU8?.byteLength})));
 errors.push(...logs.filter(s=>/not readable|Read.Write|Exception:|out of memory|Could not allocate|RuntimeError:/i.test(s)));
 if(errors.length)throw Error('Runtime errors observed');
 report.passed=true;
}catch(e){report.failure=String(e);report.passed=false;await shot('failure').catch(()=>{});}
clearTimeout(watchdog);checkpoint('complete');await bounded(browser.close(),10000,'browser close');if(!report.passed)process.exitCode=1;
})().catch(e=>{console.error(e);process.exit(1)});
