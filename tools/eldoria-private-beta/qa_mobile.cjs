const {chromium}=require('playwright');const fs=require('fs');const {PNG}=require('pngjs');
(async()=>{
fs.mkdirSync('qa',{recursive:true});const reports=[];
const browser=await chromium.launch({args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--disable-dev-shm-usage']});
for(const name of ['original','mobile']){
 const context=await browser.newContext({viewport:{width:390,height:844},deviceScaleFactor:0.5});const page=await context.newPage();const logs=[];const errors=[];page.on('console',m=>logs.push(m.text()));page.on('pageerror',e=>errors.push(String(e)));page.on('crash',()=>errors.push('PAGE_CRASH'));const start=Date.now();let failed=null;
 try{await page.goto('http://127.0.0.1:8765/'+name+'/?eldoria_diag=1');await page.locator('#play').click();await page.waitForFunction(()=>!!window.valoriaGame,null,{timeout:240000});await page.waitForTimeout(45000);const cdp=await context.newCDPSession(page);const shot=await cdp.send('Page.captureScreenshot',{format:'png',captureBeyondViewport:false,fromSurface:true});fs.writeFileSync('qa/'+name+'.png',Buffer.from(shot.data,'base64'));const p=PNG.sync.read(fs.readFileSync('qa/'+name+'.png'));let sum=0,sq=0,n=0;for(let i=0;i<p.data.length;i+=4){let v=(p.data[i]+p.data[i+1]+p.data[i+2])/3;sum+=v;sq+=v*v;n++;}if(Math.sqrt(sq/n-(sum/n)**2)<15)throw Error('blank game capture');}
 catch(e){failed=String(e);await page.screenshot({path:'qa/'+name+'-failed.png',timeout:5000}).catch(()=>{});}
 let state=await page.evaluate(()=>({ready:!!window.valoriaGame,heapBytes:window.valoriaGame?.Module?.HEAPU8?.byteLength||null,diagnostic:window.EldoriaLocalDiagnostic?.read(),error:document.getElementById('error')?.textContent})).catch(()=>null);
 errors.push(...logs.filter(s=>/not readable|Read.Write|Exception:|out of memory|Could not allocate|RuntimeError:/i.test(s)));reports.push({name,observationAfterReadySeconds:45,seconds:(Date.now()-start)/1000,failed,errors,logs,state,physicalIphone:false});fs.writeFileSync('qa/report.json',JSON.stringify(reports,null,2));await context.close();
}
await browser.close();if(reports.some(r=>r.failed||r.errors.length))process.exit(1);
})().catch(e=>{console.error(e);process.exit(1)});
