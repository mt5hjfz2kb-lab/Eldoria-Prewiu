const {chromium}=require('playwright');const fs=require('fs');
(async()=>{
fs.mkdirSync('qa-entry',{recursive:true});
const browser=await chromium.launch({args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--disable-dev-shm-usage']});
const context=await browser.newContext({viewport:{width:390,height:844},deviceScaleFactor:0.5});
const page=await context.newPage();const errors=[],logs=[];const report={physicalIphone:false,nickname:'Beta QA',errors,logs,stages:[]};
page.on('pageerror',e=>errors.push(String(e)));page.on('crash',()=>errors.push('PAGE_CRASH'));page.on('console',m=>logs.push(m.text()));
const shot=async(name)=>{const cdp=await context.newCDPSession(page);const s=await cdp.send('Page.captureScreenshot',{format:'png',captureBeyondViewport:false,fromSurface:true});fs.writeFileSync('qa-entry/'+name+'.png',Buffer.from(s.data,'base64'));await cdp.detach();};
try{
 await page.goto('http://127.0.0.1:8765/mobile/?eldoria_diag=1');await page.locator('#play').click();
 await page.waitForFunction(()=>!!window.valoriaGame,null,{timeout:240000});await page.waitForTimeout(45000);
 await shot('01-welcome');
 // Coordinates verified against 390x844 capture and RoyalPlayerNamePrompt anchors.
 await page.mouse.click(195,490);await page.keyboard.type(report.nickname,{delay:140});await page.mouse.click(195,614);
 await page.waitForTimeout(60000);await shot('02-after-name');
 report.stages.push(await page.evaluate(()=>({stage:'after-name-60s',ready:!!window.valoriaGame,heapBytes:window.valoriaGame?.Module?.HEAPU8?.byteLength})));
 report.persistence=await page.evaluate(async()=>{
  const result=[];for(const info of await indexedDB.databases()){
   const db=await new Promise((resolve,reject)=>{const r=indexedDB.open(info.name);r.onsuccess=()=>resolve(r.result);r.onerror=()=>reject(r.error)});
   for(const name of Array.from(db.objectStoreNames)){
    const values=await new Promise((resolve,reject)=>{const r=db.transaction(name,'readonly').objectStore(name).getAll();r.onsuccess=()=>resolve(r.result);r.onerror=()=>reject(r.error)});
    function contains(v){if(typeof v==='string')return v.includes('Beta QA');if(v instanceof ArrayBuffer)return new TextDecoder().decode(v).includes('Beta QA');if(ArrayBuffer.isView(v))return new TextDecoder().decode(v).includes('Beta QA');if(v&&typeof v==='object')return Object.values(v).some(contains);return false;}
    result.push({database:info.name,store:name,records:values.length,nicknameFound:values.some(contains)});
   }db.close();
  }return result;
 });
 if(!report.persistence.some(x=>x.nicknameFound))throw Error('Nickname not observed in browser-backed saved state');
 await page.reload();await page.locator('#play').click();await page.waitForFunction(()=>!!window.valoriaGame,null,{timeout:240000});await page.waitForTimeout(45000);
 await shot('03-reloaded');report.stages.push(await page.evaluate(()=>({stage:'reload-45s',ready:!!window.valoriaGame,heapBytes:window.valoriaGame?.Module?.HEAPU8?.byteLength})));
 errors.push(...logs.filter(s=>/not readable|Read.Write|Exception:|out of memory|Could not allocate|RuntimeError:/i.test(s)));
 if(errors.length)throw Error('Runtime errors observed');
 report.passed=true;
}catch(e){report.failure=String(e);report.passed=false;await shot('failure').catch(()=>{});}
fs.writeFileSync('qa-entry/report.json',JSON.stringify(report,null,2));await browser.close();if(!report.passed)process.exitCode=1;
})().catch(e=>{console.error(e);process.exit(1)});
