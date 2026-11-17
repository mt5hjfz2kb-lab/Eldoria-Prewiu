#!/usr/bin/env node
// Headless Chromium smoke proof for a real compiled Unity 6.3 WebGL trial.
// A simulated iPhone viewport is not a physical iPhone performance test.
const fs=require('fs');
const crypto=require('crypto');
const { chromium }=require('playwright');
const url=process.env.CIUDAD2D_WEBPERF_URL||'http://127.0.0.1:8765/';
const out='ciudad2d-webperf-browser-proof';
fs.mkdirSync(out,{recursive:true});
const sleep=ms=>new Promise(r=>setTimeout(r,ms));
(async()=>{
 const errors=[],logs=[],failedRequests=[];
 const report={source:'Unity 6000.3 WebGL build',userAgent:'simulated iPhone / headless Chromium',
               physical_iphone_test:false,passed:false,engineReady:false,canvas:null,
               fpsSamples:[],playable_inputs_tested:false,errors,failedRequests};
 const browser=await chromium.launch({headless:true,args:['--use-angle=swiftshader',
                                                '--enable-unsafe-swiftshader']});
 try{
   const context=await browser.newContext({viewport:{width:390,height:844},
     isMobile:true,hasTouch:true,deviceScaleFactor:1,
     userAgent:'Mozilla/5.0 (iPhone; CPU iPhone OS 18_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.0 Mobile/15E148 Safari/604.1'});
   const page=await context.newPage();
   page.on('console',m=>logs.push({level:m.type(),text:m.text()}));
   page.on('pageerror',e=>errors.push(String(e)));
   page.on('requestfailed',r=>failedRequests.push({url:r.url(),failure:r.failure()}));
   await page.goto(url,{waitUntil:'domcontentloaded',timeout:60000});
   await page.waitForSelector('canvas',{timeout:30000});
   await page.waitForFunction(()=>{const c=document.querySelector('canvas');
     return c&&c.width>100&&c.height>100;},{timeout:120000});
   for(let i=0;i<90;i++){
     if(logs.some(x=>x.text.includes('CIUDAD2D_WEBPERF_V6_READY'))){report.engineReady=true;break;}
     if(errors.length)break;
     await sleep(2000);
   }
   report.canvas=await page.locator('canvas').evaluate(c=>({
     width:c.width,height:c.height,clientWidth:c.clientWidth,clientHeight:c.clientHeight}));
   if(!report.engineReady)
     throw new Error('Compiled Unity canvas exists but V6 runtime readiness was not observed');
   await sleep(6000);
   report.fpsSamples=logs.filter(x=>x.text.includes('CIUDAD2D_WEBPERF_V6_SAMPLE'))
                         .map(x=>x.text.slice(0,350));
   await page.screenshot({path:out+'/valoria_v6_webgl_chromium_mobile_emulation.png'});
   report.screenshot_sha256=crypto.createHash('sha256')
     .update(fs.readFileSync(out+'/valoria_v6_webgl_chromium_mobile_emulation.png')).digest('hex');
   report.passed=report.canvas.width>100&&report.canvas.height>100&&
     report.fpsSamples.length>0&&errors.length===0;
   if(!report.passed)throw new Error('Runtime loaded, but frame sampling/console check failed');
 }catch(e){report.error=String(e);throw e}
 finally{
   report.console_tail=logs.slice(-120);
   fs.writeFileSync(out+'/proof.json',JSON.stringify(report,null,2));
   await browser.close();
 }
})().catch(e=>{console.error('CIUDAD2D_WEBPERF_BROWSER_FAIL',String(e));process.exitCode=1;});
