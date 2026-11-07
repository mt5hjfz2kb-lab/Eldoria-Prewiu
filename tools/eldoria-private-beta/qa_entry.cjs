const {chromium}=require('playwright');const fs=require('fs');
(async()=>{
fs.mkdirSync('qa-entry',{recursive:true});
const browser=await chromium.launch({args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--disable-dev-shm-usage']});
const context=await browser.newContext({viewport:{width:390,height:844},deviceScaleFactor:3});
const page=await context.newPage();const errors=[],logs=[];const report={physicalIphone:false,nickname:'Beta QA',errors,logs,stages:[]};
page.on('pageerror',e=>errors.push(String(e)));page.on('crash',()=>errors.push('PAGE_CRASH'));page.on('console',m=>logs.push(m.text()));
const checkpoint=(step)=>{report.lastStep=step;fs.writeFileSync('qa-entry/report.json',JSON.stringify(report,null,2));console.log('ENTRY_QA',step)};
const watchdog=setTimeout(()=>{report.failure='Entry QA exceeded 450-second deadline';report.passed=false;checkpoint('watchdog');process.exit(1)},450000);
const bounded=(promise,ms,label)=>{let timer;return Promise.race([promise,new Promise((_,reject)=>{timer=setTimeout(()=>reject(Error(label+' timeout')),ms)})]).finally(()=>clearTimeout(timer))};
const shot=async(name)=>{checkpoint('capture '+name);const cdp=await context.newCDPSession(page);const s=await bounded(cdp.send('Page.captureScreenshot',{format:'png',captureBeyondViewport:false,fromSurface:true}),20000,'capture '+name);fs.writeFileSync('qa-entry/'+name+'.png',Buffer.from(s.data,'base64'));await cdp.detach();};
try{
 checkpoint('load');await page.goto('http://127.0.0.1:8765/mobile/?eldoria_diag=1');await page.locator('#play').click();
 await page.waitForFunction(()=>!!window.valoriaGame,null,{timeout:240000});await page.waitForTimeout(45000);
 await shot('01-welcome');
 // Coordinates verified against 390x844 capture and RoyalPlayerNamePrompt anchors.
 checkpoint('enter-name');await page.mouse.click(195,490,{delay:200});await page.keyboard.type(report.nickname,{delay:140});await page.mouse.click(195,614,{delay:200});checkpoint('name-clicked');
 await page.waitForTimeout(60000);await shot('02-after-name');
 report.stages.push(await page.evaluate(()=>({stage:'after-name-60s',ready:!!window.valoriaGame,heapBytes:window.valoriaGame?.Module?.HEAPU8?.byteLength})));
 report.persistenceVerification='Separate OCR gate verifies visible nickname after actual browser reload';
 checkpoint('reload');await page.reload();await page.locator('#play').click();await page.waitForFunction(()=>!!window.valoriaGame,null,{timeout:240000});await page.waitForTimeout(45000);
 await shot('03-reloaded');report.stages.push(await page.evaluate(()=>({stage:'reload-45s',ready:!!window.valoriaGame,heapBytes:window.valoriaGame?.Module?.HEAPU8?.byteLength})));
 errors.push(...logs.filter(s=>/not readable|Read.Write|Exception:|out of memory|Could not allocate|RuntimeError:/i.test(s)));
 if(errors.length)throw Error('Runtime errors observed');
 report.passed=true;
}catch(e){report.failure=String(e);report.passed=false;await shot('failure').catch(()=>{});}
clearTimeout(watchdog);checkpoint('complete');await bounded(browser.close(),10000,'browser close');if(!report.passed)process.exitCode=1;
})().catch(e=>{console.error(e);process.exit(1)});
