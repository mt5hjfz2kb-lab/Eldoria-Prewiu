const fs=require('fs'),path=require('path'),crypto=require('crypto');
const {chromium}=require('playwright');
const root=path.resolve(process.env.ELDORIA_QA_ROOT||'.');
const site=path.join(root,'site'),state=path.join(root,'state/deferred/ordinary-qa-storage-state.json');
const out=path.join(root,'browser-proof');fs.mkdirSync(out,{recursive:true});
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
const prefix='https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/beta-iphone-p0-lazy-world/';
const report={method:'ACTUAL_PUBLIC_ENGINE18_EARNED_CONTINUATION_WINDOWS_EDGE_NOT_PHYSICAL_IPHONE',passed:false,physicalIphone:false,desktopMac:false,ownerSaveAccessed:false,fakeStateCreated:false,sourceEngine:'de63660d25bee9a32d7b804d6245744ed1f0dbca',sourceBundle:'43e2b883d8667c455b5cc9be37ff3fb5abddc6815776e2c7d01ea47c7f87da5a',errors:[],routes:[],workflowRun:process.env.GITHUB_RUN_ID,workflowSource:process.env.GITHUB_SHA,startedAt:new Date().toISOString()};
const write=()=>fs.writeFileSync(path.join(out,'report.json'),JSON.stringify(report,null,2));
async function fileHash(file){const h=crypto.createHash('sha256');for await(const b of fs.createReadStream(file))h.update(b);return h.digest('hex');}
(async()=>{
 let browser,page;
 try{
  const bytes=fs.readFileSync(state);report.inputSha256=sha(bytes);
  if(report.inputSha256!=='6b1df345a48681145ecde90ea8ce94b7cc4f4b8909531c8dfed5196b2c65434d'||bytes.length!==130908)throw Error('Unproven earned checkpoint bytes');
  const proof=JSON.parse(fs.readFileSync(path.join(root,'state/deferred/report.json')));
  report.inputEngine=proof.ordinaryQaCheckpoint.sourceEngine;report.inputBundle=proof.ordinaryQaCheckpoint.sourceBundle;
  if(!proof.passed||proof.ordinaryQaCheckpoint.ownerSaveAccessed!==false||proof.ordinaryQaCheckpoint.fakeStateCreated!==false)throw Error('Unproven input provenance');
  const identityResponse=await fetch(prefix+'release-identity.json?qa='+Date.now(),{cache:'no-store'});
  if(!identityResponse.ok)throw Error('Public identity unavailable');
  const identity=await identityResponse.json();
  if(identity.source_sha!==report.sourceEngine||identity.candidate_bundle_sha256!==report.sourceBundle||identity.release_for_external_testers!==false)throw Error('Public candidate identity differs');
  const publication=await (await fetch(new URL('../beta-mobile18-owner-publication-proof.json?qa='+Date.now(),prefix))).json();
  if(publication.source_sha!==report.sourceEngine||publication.external_testers_enabled!==false||publication.preserved_files!==146)throw Error('Public publication gate differs');
  report.actualPublicIdentity=identity;report.actualPublicPublication=publication;
  report.siteIntegrityMethod='Actual public HTTP without routing, fulfilment or local asset substitution';
  const edge=process.env.ELDORIA_QA_EDGE;
  if(!edge||!fs.existsSync(edge))throw Error('Installed Edge executable unavailable');
  browser=await chromium.launch({executablePath:edge,headless:true});
  report.browserVersion=browser.version();
  const context=await browser.newContext({viewport:{width:390,height:844},deviceScaleFactor:3,storageState:state});
  await context.addInitScript(()=>{window.__localQaContextLost=false;document.addEventListener('webglcontextlost',()=>{window.__localQaContextLost=true;},true);});
  page=await context.newPage();let stateSeen=false;
  page.on('pageerror',e=>{report.errors.push(String(e));write();});
  page.on('crash',()=>{report.errors.push('PAGE_CRASH');write();});
  page.on('console',m=>{const text=m.text();fs.appendFileSync(path.join(out,'console.jsonl'),JSON.stringify({at:new Date().toISOString(),level:m.type(),text})+'\n');if(text.includes('ELDORIA_PLAYABLE_STATE')&&/bastion=5\b/.test(text)&&/archers=60\b/.test(text)&&text.includes('objective=b5.complete')){stateSeen=true;report.actualReadOnlyState=text.trim();}});
  await page.goto(prefix+'?eldoria_diag=1&eldoria_profile=1&eldoria_p0_lazy_world=1',{waitUntil:'domcontentloaded',timeout:30000});
  const play=page.locator('#play');await play.waitFor({state:'visible',timeout:20000});report.actualLaunchLabel=await play.innerText();await play.click();
  const started=Date.now();while(!stateSeen&&Date.now()-started<150000&&!report.errors.length)await page.waitForTimeout(1000);
  await page.waitForTimeout(6000);
  await page.screenshot({path:path.join(out,'game.png'),timeout:15000});
  report.actualCanvasClicks=[];
  if(stateSeen&&!report.errors.length){
    {const hit=await page.evaluate(()=>{const e=document.elementFromPoint(153,790);return {tag:e?.tagName,id:e?.id};});(report.canvasHitChecks??=[]).push(hit);if(hit.tag!=='CANVAS')throw Error('World/city tap intercepted by HTML '+JSON.stringify(hit));}
    await page.mouse.click(153,790,{delay:150});
    report.actualCanvasClicks.push({x:153,y:790,purpose:'Observed World icon above diagnostic overlay',at:new Date().toISOString()});
    await page.waitForTimeout(45000);
    await page.screenshot({path:path.join(out,'after-world-control.png'),timeout:15000});
    {const hit=await page.evaluate(()=>{const e=document.elementFromPoint(153,790);return {tag:e?.tagName,id:e?.id};});(report.canvasHitChecks??=[]).push(hit);if(hit.tag!=='CANVAS')throw Error('World/city tap intercepted by HTML '+JSON.stringify(hit));}
    await page.mouse.click(153,790,{delay:150});
    report.actualCanvasClicks.push({x:153,y:790,purpose:'Same alternating world/city control',at:new Date().toISOString()});
    await page.waitForTimeout(15000);
    await page.screenshot({path:path.join(out,'after-return-control.png'),timeout:15000});
    report.worldAndReturnImagesRequireVisualReview=true;
  }
  report.worldEntryVerified=false;
  report.actualProgressSeen=stateSeen;report.contextLost=await page.evaluate(()=>window.__localQaContextLost===true);
  report.inputBytesUnchanged=sha(fs.readFileSync(state))===report.inputSha256;
  report.screenshotSha256=sha(fs.readFileSync(path.join(out,'game.png')));
  report.passed=stateSeen&&report.inputBytesUnchanged&&!report.contextLost&&!report.errors.length;
 }catch(e){report.failure=String(e);if(page)try{await page.screenshot({path:path.join(out,'failure.png'),timeout:5000});}catch{}}
 finally{report.finishedAt=new Date().toISOString();write();if(browser)await browser.close();console.log(JSON.stringify({passed:report.passed,failure:report.failure,progress:report.actualProgressSeen,errors:report.errors,browser:report.browserVersion,screenshot:path.join(out,'game.png'),report:path.join(out,'report.json')}));}
 process.exitCode=report.passed?0:1;
})();
