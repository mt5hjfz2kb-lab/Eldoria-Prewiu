const {chromium}=require('playwright');
const fs=require('fs');
(async()=>{
 const out=process.env.QA_OUT||'qa-structural';
 fs.mkdirSync(out,{recursive:true});
 const url=process.env.QA_URL, revision=process.env.QA_REVISION;
 if(!url||!revision)throw Error('Explicit candidate URL and revision required');
 const scale=Number(process.env.QA_DPR||3);
 const browser=await chromium.launch({args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--disable-dev-shm-usage']});
 const context=await browser.newContext({viewport:{width:390,height:844},deviceScaleFactor:scale});
 const page=await context.newPage();
 const report={url,revision,deviceScaleFactor:scale,environment:'Chromium SwiftShader; NOT physical iPhone',passed:false,errors:[],logs:[],samples:[]};
 const write=stage=>{report.stage=stage;fs.writeFileSync(out+'/report.json',JSON.stringify(report,null,2));console.log(stage)};
 const timer=setTimeout(()=>{report.errors.push('Global 360-second deadline');write('watchdog');process.exit(1)},360000);
 page.on('pageerror',e=>report.errors.push(String(e)));
 page.on('crash',()=>report.errors.push('PAGE_CRASH'));
 page.on('console',m=>report.logs.push(m.text()));
 const snapshot=()=>page.evaluate(()=>({
  at:performance.now(),ready:!!window.valoriaGame,
  wasmCapacityBytes:window.valoriaGame?.Module?.HEAPU8?.byteLength||0,
  jsUsedBytes:performance.memory?.usedJSHeapSize||null,
  gameVisible:!document.getElementById('game')?.hidden,
  navigation:performance.getEntriesByType('navigation')[0]?.type
 }));
 try{
  write('load');
  await page.goto(url,{waitUntil:'domcontentloaded',timeout:30000});
  const identity=await page.evaluate(()=>fetch('release-identity.json',{cache:'no-store'}).then(r=>r.json()));
  if(identity.revision!==revision)throw Error('Unexpected candidate identity');
  report.identity=identity;
  await page.locator('#play').click();
  const deadline=Date.now()+240000;
  while(Date.now()<deadline){
   if(report.errors.length)throw Error('Runtime failure during startup');
   report.samples.push(await snapshot());
   if(report.logs.some(s=>s.includes('REGION1_CANON_WORLD_INSTALLED')) && report.samples.at(-1).ready)break;
   await page.waitForTimeout(1000);
  }
  if(!report.logs.some(s=>s.includes('REGION1_CANON_WORLD_INSTALLED')))throw Error('Exact world initialization never completed');
  if(!report.samples.at(-1).ready)throw Error('Unity instance not ready');
  write('world-initialization-complete');
  for(let i=0;i<60;i++){
   await page.waitForTimeout(1000);const sample=await snapshot();report.samples.push(sample);
   if(!sample.ready||!sample.gameVisible||report.errors.length)throw Error('Runtime left game during observation');
  }
  report.runtimeErrors=report.logs.filter(s=>/not readable|Read.Write|Exception:|out of memory|Could not allocate|RuntimeError:/i.test(s));
  if(report.runtimeErrors.length)throw Error('Unity runtime errors');
  report.peakWasmCapacityBytes=Math.max(...report.samples.map(s=>s.wasmCapacityBytes));
  // Genuine rendered output remains a required gate; a working JS object is insufficient.
  await page.screenshot({path:out+'/welcome.png',timeout:20000});
  report.passed=true;
  report.scope='Exact world initialization plus 60 seconds visible; does not certify reload, I-V gameplay, or iPhone';
 }catch(e){report.failure=String(e);}
 clearTimeout(timer);write('complete');
 const closing=setTimeout(()=>process.exit(report.passed?0:1),10000);
 await browser.close();clearTimeout(closing);
 if(!report.passed)process.exitCode=1;
})().catch(e=>{console.error(e);process.exit(1)});
