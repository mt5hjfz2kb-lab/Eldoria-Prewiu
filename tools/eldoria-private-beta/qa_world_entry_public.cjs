// Read the actual public player and operate only visible controls in a fresh QA profile.
const fs=require('fs'),{execFileSync}=require('child_process'),{webkit}=require('playwright');
const out=process.env.QA_OUT||'postnickname-evidence';fs.mkdirSync(out,{recursive:true});
const report={source:null,physicalIphone:false,actions:[],errors:[],screens:[],completed:false};
const save=()=>fs.writeFileSync(out+'/report.json',JSON.stringify(report,null,2));
const norm=s=>s.normalize('NFD').replace(/[\u0300-\u036f]/g,'').toUpperCase().trim();
(async()=>{let browser;try{
 browser=await webkit.launch({headless:true});
 const context=await browser.newContext({viewport:{width:390,height:844},deviceScaleFactor:3});
 const page=await context.newPage();
 // Read-only frame pacing observer: identical for control and candidate. It
 // never calls Unity methods or changes saved state, camera or game time.
 await page.addInitScript(()=>{
  window.__eldoriaWorldQaFrames={active:false,last:0,phase:'idle',gaps:{}};
  const observe=now=>{
   const s=window.__eldoriaWorldQaFrames;
   if(s.active&&s.last){const a=s.gaps[s.phase]||(s.gaps[s.phase]=[]);if(a.length<36000)a.push(now-s.last);}
   s.last=now;requestAnimationFrame(observe);
  };
  requestAnimationFrame(observe);
 });
 let consoleText='';
 page.on('console',m=>{
  const text=m.text();consoleText+=text+'\n';
  if(/Could not allocate|out of memory|RuntimeError:|NullReferenceException|ELDORIA_CONTEXT_LOST|CONTEXT_LOST_WEBGL/i.test(text)){
   report.errors.push('UNITY_RUNTIME: '+text);save();
  }
 });
 page.on('crash',()=>{report.errors.push('PAGE_CRASH');save();});
 page.on('console',m=>fs.appendFileSync(out+'/console.log',m.text()+'\n'));
 page.on('pageerror',e=>{report.errors.push(String(e));save();});
 await page.goto(process.env.QA_URL||'https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/beta-integrated-vigilia/?eldoria_p0_lazy_world=1&eldoria_diag=1',{waitUntil:'domcontentloaded'});
 const identity=await page.evaluate(()=>fetch('release-identity.json',{cache:'no-store'}).then(r=>r.json()));
 if(identity.source_sha!==(process.env.QA_EXPECT_SOURCE||'fb8727d3ac45e31e61ab8ba244fa395477eff16e'))throw Error('Public source changed');
 report.source=identity.source_sha;save();
 await page.locator('#play').click();
 await page.waitForFunction(()=>!!window.valoriaGame,null,{timeout:150000});
 async function screen(name){
  const file=out+'/'+name+'.png';await page.screenshot({path:file});
  const rows=JSON.parse(execFileSync(process.env.QA_OCR_EXECUTABLE,[file],{encoding:'utf8',timeout:20000}));
  fs.writeFileSync(out+'/'+name+'.json',JSON.stringify(rows,null,2));
  report.screens.push({name,text:rows.map(r=>r.text)});save();return rows;
 }
 await page.waitForTimeout(4000);await screen('before-name');
 // Exact nickname control coordinates from reviewed beta19 startup at 390x844.
 await page.mouse.click(195,490,{delay:150});await page.keyboard.type('Entrada QA',{delay:100});
 await screen('typed-name');await page.mouse.click(195,614,{delay:150});
 await page.waitForTimeout(3000);await screen('saved-nickname-before-reload');
 // Reproduce a returning player whose nickname was saved before story advancement.
 await page.reload({waitUntil:'domcontentloaded'});await page.locator('#play').click();
 await page.waitForFunction(()=>!!window.valoriaGame,null,{timeout:150000});
 await page.waitForTimeout(4000);
 const returning=await screen('returning-nickname');
 report.nicknameReloadVerified=returning.some(r=>norm(r.text)==='ENTRADA QA');
 if(!report.nicknameReloadVerified)throw Error('Saved nickname did not survive real reload');
 const storyLine=returning.find(r=>r.confidence>.4&&norm(r.text).includes('BRECHA SE ABRIO'));
 if(!storyLine)throw Error('Actual returning-player story line was not found for pixel readability proof');
 const {measureStoryInk}=await import('./story_pixel_readability.mjs');
 report.storyInk=measureStoryInk(out+'/returning-nickname.png',storyLine);save();
 if(!report.storyInk.passed)throw Error('Rendered story ink remains dark on Vigilia stone');
 let constructionSiteClicked=false;
 for(let step=0;step<35;step++){
  const rows=await screen('after-name-'+step);
  const next=rows.filter(r=>r.confidence>.4&&/^(CONTINUAR|CONTINUE|SIGUIENTE|NEXT|ENTENDIDO|COMENZAR|MOSTRAR TEXTO|REVEAL TEXT)$/.test(norm(r.text)));
  if(next.length===1){await page.mouse.click(next[0].x,next[0].y,{delay:150});report.actions.push({step,kind:'visible-dialogue',...next[0]});}
  else{
   const build=rows.filter(r=>r.confidence>.4&&norm(r.text)==='CONSTRUIR');
   const target=build.length===1?build[0]:null;
   if(target){await page.mouse.click(target.x,target.y,{delay:150});report.actions.push({step,kind:'visible-build-control',...target});}
   else if(!constructionSiteClicked){
    const sites=rows.filter(r=>r.confidence>.4&&/^CONSTRUIR AQUI:\s*ASERRADERO$/.test(norm(r.text)));
    if(sites.length!==1)throw Error('Expected one visible sawmill construction site');
    await page.mouse.click(sites[0].x,sites[0].y,{delay:150});constructionSiteClicked=true;
    report.actions.push({step,kind:'visible-sawmill-construction-site',...sites[0]});
   }
   else if(rows.some(r=>/^(ASERRADERO|SAWMILL)$/.test(norm(r.text))) && rows.some(r=>/^(NIVEL|LEVEL)\s*1$/.test(norm(r.text)))){
    report.constructionVerified=report.actions.some(a=>norm(a.text||'')==='CONSTRUIR');break;
   }
  }
  save();await page.waitForTimeout(2500);
 }
 await page.waitForTimeout(10000);await screen('final');
 if(report.constructionVerified){
  await page.reload({waitUntil:'domcontentloaded'});await page.locator('#play').click();
  await page.waitForFunction(()=>!!window.valoriaGame,null,{timeout:150000});
  await page.waitForTimeout(4000);await screen('reloaded-built-city');
  // The saved sawmill legitimately unlocks Aldric's next story beat on return.
  // Finish that visible canonical dialogue before touching city navigation.
  // The earlier probe clicked behind it and falsely called this save loss.
  for(let beat=0;beat<12;beat++){
   const rows=await screen('returning-built-story-'+beat);
   const next=rows.filter(r=>r.confidence>.4&&/^(CONTINUAR|CONTINUE|SIGUIENTE|NEXT|ENTENDIDO|COMENZAR|MOSTRAR TEXTO|REVEAL TEXT)$/.test(norm(r.text)));
   if(next.length===0)break;
   if(next.length!==1)throw Error('Ambiguous visible returning-player narrative control');
   await page.mouse.click(next[0].x,next[0].y,{delay:150});
   report.actions.push({kind:'returning-built-visible-dialogue',beat,...next[0]});save();
   await page.waitForTimeout(2500);
   if(beat===11)throw Error('Returning-player story failed to finish');
  }
  // Actual sawmill roof in reviewed returning-built-story-1 at 390x844.
  // The bag medallion belongs to inventory and must never select a building.
  await page.mouse.click(26,282,{delay:150});await page.waitForTimeout(1500);
  report.actions.push({kind:'visible-built-sawmill',x:26,y:282});
  const persisted=await screen('reloaded-sawmill');
  report.constructionReloadVerified=persisted.some(r=>/^(ASERRADERO|SAWMILL)$/.test(norm(r.text))) &&
    persisted.some(r=>/^(NIVEL|LEVEL)\s*1$/.test(norm(r.text))) &&
    persisted.some(r=>norm(r.text)==='ENTRADA QA');
  if(!report.constructionReloadVerified)throw Error('Built sawmill did not survive real reload');
 }
 if(!report.constructionReloadVerified)throw Error('World regression requires genuinely earned and reloaded sawmill');
 report.worldEntries=[];
 for(let visit=0;visit<2;visit++){
  const logStart=consoleText.length;
  await page.evaluate(()=>Object.assign(window.__eldoriaWorldQaFrames,
   {active:true,last:performance.now(),phase:'world-entry',gaps:{}}));
  // Actual published a2 screenshot: three footer controls at x128/195/263.
  await page.mouse.click(263,776,{delay:180});
  const entry={visit,samples:[],entered:false,requestedAt:Date.now()};report.worldEntries.push(entry);save();
  const deadline=Date.now()+150000;
  while(Date.now()<deadline){
   if(report.errors.length)throw Error('World browser failure');
   const sample=await page.evaluate(()=>({at:performance.now(),ready:!!window.valoriaGame,
    metrics:window.valoriaGame?.GetMetricsInfo?.(),canvas:{width:document.getElementById('unity-canvas').width,height:document.getElementById('unity-canvas').height}}));
   sample.processes=execFileSync('ps',['-axo','pid=,ppid=,rss=,comm='],{encoding:'utf8',timeout:5000}).split('\n').filter(x=>/WebContent|WebKit.GPU|GPUProcess|playwright|MiniBrowser/.test(x));
   entry.samples.push(sample);save();
   if(consoleText.slice(logStart).includes('ELDORIA_P0_LAZY_WORLD_ENTERED')){entry.entered=true;entry.observedEntryWaitMs=Date.now()-entry.requestedAt;break;}
   await page.waitForTimeout(1000);
  }
  if(!entry.entered)throw Error('World entry never reached canonical entered marker');
  // Visible models stream AFTER the entered marker. Sampling only up to
  // that marker misses the very allocation peak this P0 candidate changes.
  await page.evaluate(()=>window.__eldoriaWorldQaFrames.phase='world-stream-settling');
  const settleDeadline=Date.now()+12000;
  while(Date.now()<settleDeadline){
   if(report.errors.length)throw Error('World browser failure during streamed model settling');
   const sample=await page.evaluate(()=>({at:performance.now(),ready:!!window.valoriaGame,
    metrics:window.valoriaGame?.GetMetricsInfo?.(),phase:'world-stream-settling'}));
   sample.processes=execFileSync('ps',['-axo','pid=,ppid=,rss=,comm='],{encoding:'utf8',timeout:5000}).split('\n').filter(x=>/WebContent|WebKit.GPU|GPUProcess|playwright|MiniBrowser/.test(x));
   entry.samples.push(sample);save();await page.waitForTimeout(500);
  }
  await screen('world-visit-'+visit);
  if(visit===0){
   // Integrated Valoria disables ZoneOneStrategicCamera.LateUpdate and owns
   // real input in SlicePresenter. Keyboard 1/2/3 therefore cannot test zoom.
   // Use actual wheel events through that production input path instead.
   const {decodeUnityPng}=await import('../art-direction/ai-authoring/unity_pixel_diff.mjs');
   let previousView=out+'/world-visit-'+visit+'.png';
   report.zoomPixelChanges=[];
   for(const [ticks,delta,view] of [[6,240,'out'],[32,240,'overview'],[70,-240,'in']]){
    await page.evaluate(view=>window.__eldoriaWorldQaFrames.phase='zoom-'+view,view);
    // Keep the pointer off the central Entrar en Valoria button as it moves during zoom.
    await page.mouse.move(350,200);
    for(let tick=0;tick<ticks;tick++){
     await page.mouse.wheel(0,delta);await page.waitForTimeout(250);
     const sample=await page.evaluate(()=>({at:performance.now(),ready:!!window.valoriaGame,
      metrics:window.valoriaGame?.GetMetricsInfo?.()}));
     sample.phase='zoom-'+view+'-gesture';
     sample.processes=execFileSync('ps',['-axo','pid=,ppid=,rss=,comm='],{encoding:'utf8',timeout:5000}).split('\n').filter(x=>/WebContent|WebKit.GPU|GPUProcess|playwright|MiniBrowser/.test(x));
     entry.samples.push(sample);
    }
    report.actions.push({kind:'actual-mouse-wheel',ticks,delta,view});
    const viewDeadline=Date.now()+6500;
    while(Date.now()<viewDeadline){
     if(report.errors.length)throw Error('Runtime error while streaming zoom '+view);
     const sample=await page.evaluate(()=>({at:performance.now(),ready:!!window.valoriaGame,
      metrics:window.valoriaGame?.GetMetricsInfo?.()}));
     sample.phase='zoom-'+view;
     sample.processes=execFileSync('ps',['-axo','pid=,ppid=,rss=,comm='],{encoding:'utf8',timeout:5000}).split('\n').filter(x=>/WebContent|WebKit.GPU|GPUProcess|playwright|MiniBrowser/.test(x));
     entry.samples.push(sample);save();await page.waitForTimeout(500);
    }
    await screen('world-zoom-'+view);
    const currentView=out+'/world-zoom-'+view+'.png';
    const before=decodeUnityPng(fs.readFileSync(previousView)),after=decodeUnityPng(fs.readFileSync(currentView));
    if(before.width!==after.width||before.height!==after.height)throw Error('Zoom screenshot dimensions changed');
    let changed=0,total=0;
    for(let y=Math.floor(after.height*.2);y<after.height*.8;y+=4){
     for(let x=Math.floor(after.width*.1);x<after.width*.9;x+=4){
      const i=(y*after.width+x)*3;total++;
      if(Math.max(...[0,1,2].map(c=>Math.abs(before.rgb[i+c]-after.rgb[i+c])))>24)changed++;
     }
    }
    const change={view,changedFraction:changed/total,sampledPixels:total};
    report.zoomPixelChanges.push(change);save();
    if(change.changedFraction<.1)throw Error('Wheel did not visibly change the actual world view: '+view);
    previousView=currentView;
   }
   report.zoomInputsSent=true;report.zoomViewsChanged=true;report.zoomViewsRequireImageReview=true;save();
  }
  entry.framePacing=await page.evaluate(()=>{
   const s=window.__eldoriaWorldQaFrames;s.active=false;
   const phases={};for(const [phase,raw] of Object.entries(s.gaps)){
    const values=raw.filter(x=>x>=0).sort((a,b)=>a-b);
    phases[phase]={frames:values.length,maxGapMs:values.at(-1)||0,
     p95GapMs:values[Math.floor((values.length-1)*.95)]||0,
     over100ms:values.filter(x=>x>100).length,over250ms:values.filter(x=>x>250).length};
   }
   return {visibility:document.visibilityState,phases};
  });save();
  // Both approved comparison sources now retain the published beta19 UI:
  // two world footer controls, with the actual city button at225,776.
  // The rejected588 candidate used an obsolete three-button layout.
  const returnX=225;
  await page.mouse.click(returnX,776,{delay:180});await page.waitForTimeout(4000);
  await screen('city-return-'+visit);
  await page.mouse.click(26,282,{delay:150});await page.waitForTimeout(1500);
  const city=await screen('city-return-sawmill-'+visit);
  entry.returned=city.some(r=>norm(r.text)==='ENTRADA QA')&&
   city.some(r=>/^(ASERRADERO|SAWMILL)$/.test(norm(r.text)))&&
   city.some(r=>/^(NIVEL|LEVEL)\s*1$/.test(norm(r.text)));
  if(!entry.returned)throw Error('City return did not expose the genuinely saved sawmill');
  save();
 }
 if(process.env.QA_REQUIRE_WORLD_JOURNAL==='1'){
  const journal=await page.evaluate(()=>window.EldoriaLocalDiagnostic?.readWorldReport?.());
  if(!journal?.persistent)throw Error('World journal not persisted');
  for(const phase of ['world-requested','world-built','world-ready','world-entered','world-frame-2'])
   if(!journal.current.some(e=>e.phase===phase))throw Error('Missing actual Unity journal phase: '+phase);
  fs.writeFileSync(out+'/world-journal.json',JSON.stringify(journal,null,2));
  await page.reload({waitUntil:'domcontentloaded'});
  const recovered=await page.evaluate(()=>window.EldoriaLocalDiagnostic?.readWorldReport?.());
  if(!recovered?.previous.some(e=>e.phase==='world-entered'))throw Error('World journal lost after real reload');
  fs.writeFileSync(out+'/world-journal-recovered.json',JSON.stringify(recovered,null,2));
  report.worldJournalVerified=true;
 }
 report.worldEntryAndReentryVerified=true;
 report.metrics=await page.evaluate(()=>window.valoriaGame?.GetMetricsInfo?.());
 report.completed=true;report.scope='Actual browser controls; screenshot review required, NOT physical iPhone';
 if(process.env.QA_REQUIRE_CONSTRUCTION==='1'&&!report.constructionVerified)throw Error('Actual post-nickname construction was not proven');
 if(report.errors.length)throw Error('Browser errors during continuation');
 report.passed=report.constructionVerified===true;save();
}catch(e){report.failure=String(e);save();process.exitCode=1;}finally{if(browser)await browser.close();}})();
