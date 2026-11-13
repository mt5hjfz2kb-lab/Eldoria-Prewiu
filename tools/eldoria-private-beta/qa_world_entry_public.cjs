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
 let consoleText='';
 page.on('console',m=>{consoleText+=m.text()+'\n';});
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
 let provisionsClicked=false;
 for(let step=0;step<35;step++){
  const rows=await screen('after-name-'+step);
  const next=rows.filter(r=>r.confidence>.4&&/^(CONTINUAR|CONTINUE|SIGUIENTE|NEXT|ENTENDIDO|COMENZAR|MOSTRAR TEXTO|REVEAL TEXT)$/.test(norm(r.text)));
  if(next.length===1){await page.mouse.click(next[0].x,next[0].y,{delay:150});report.actions.push({step,kind:'visible-dialogue',...next[0]});}
  else{
   const build=rows.filter(r=>r.confidence>.4&&norm(r.text)==='CONSTRUIR');
   const target=build.length===1?build[0]:null;
   if(target){await page.mouse.click(target.x,target.y,{delay:150});report.actions.push({step,kind:'visible-build-control',...target});}
   else if(!provisionsClicked){await page.mouse.click(221,776,{delay:150});provisionsClicked=true;report.actions.push({step,kind:'reviewed-beta19-provisions-navigation',x:221,y:776});}
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
  // Reviewed actual three-control navigation after the first sawmill unlock.
  await page.mouse.click(195,776,{delay:150});await page.waitForTimeout(1500);
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
  // Actual published a2 screenshot: three footer controls at x128/195/263.
  await page.mouse.click(263,776,{delay:180});
  const entry={visit,samples:[],entered:false};report.worldEntries.push(entry);save();
  const deadline=Date.now()+150000;
  while(Date.now()<deadline){
   if(report.errors.length)throw Error('World browser failure');
   const sample=await page.evaluate(()=>({at:performance.now(),ready:!!window.valoriaGame,
    metrics:window.valoriaGame?.GetMetricsInfo?.(),canvas:{width:document.getElementById('unity-canvas').width,height:document.getElementById('unity-canvas').height}}));
   sample.processes=execFileSync('ps',['-axo','pid=,ppid=,rss=,comm='],{encoding:'utf8',timeout:5000}).split('\n').filter(x=>/WebContent|playwright|MiniBrowser/.test(x));
   entry.samples.push(sample);save();
   if(consoleText.slice(logStart).includes('ELDORIA_P0_LAZY_WORLD_ENTERED')){entry.entered=true;break;}
   await page.waitForTimeout(1000);
  }
  if(!entry.entered)throw Error('World entry never reached canonical entered marker');
  await page.waitForTimeout(12000);await screen('world-visit-'+visit);
  await page.mouse.click(128,776,{delay:180});await page.waitForTimeout(4000);
  const city=await screen('city-return-'+visit);
  entry.returned=city.some(r=>norm(r.text)==='ENTRADA QA');
  if(!entry.returned)throw Error('City return lost visible saved nickname');
  save();
 }
 report.worldEntryAndReentryVerified=true;
 report.metrics=await page.evaluate(()=>window.valoriaGame?.GetMetricsInfo?.());
 report.completed=true;report.scope='Actual browser controls; screenshot review required, NOT physical iPhone';
 if(process.env.QA_REQUIRE_CONSTRUCTION==='1'&&!report.constructionVerified)throw Error('Actual post-nickname construction was not proven');
 if(report.errors.length)throw Error('Browser errors during continuation');
 report.passed=report.constructionVerified===true;save();
}catch(e){report.failure=String(e);save();process.exitCode=1;}finally{if(browser)await browser.close();}})();
