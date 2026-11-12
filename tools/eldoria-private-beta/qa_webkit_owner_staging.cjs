// Diagnostic only: identity-pinned candidate, genuine desktop browsers, no mobile certification.
const fs = require('fs');
const {execFileSync,spawn} = require('child_process');
const {webkit, chromium} = require('playwright');
const {verifyWorldStart} = require('./webkit-world-variant.cjs');
const engine = process.env.QA_ENGINE || 'webkit';
const variant = process.env.QA_VARIANT || 'unchanged';
const out = process.env.QA_OUT || 'webkit-residency';
fs.mkdirSync(out, {recursive:true});
const report = {engine, variant, platform:process.platform, arch:process.arch, passed:false,
  scope:'Identity-pinned candidate; desktop engine comparison, NOT physical iPhone', samples:[], errors:[], navigations:[]};
const write = () => fs.writeFileSync(out+'/report.json', JSON.stringify(report,null,2));

const profileTasks=[],profiledPhases=new Set();
function nativeProfile(sample,phase){
 if(process.env.QA_NATIVE_PROFILE!=='1'||process.platform!=='darwin'||profiledPhases.has(phase))return;
 const p=(sample.processes||[]).filter(p=>p.name?.includes('WebContent')).sort((a,b)=>b.rssKiB-a.rssKiB)[0];
 if(!p||(phase!=='final'&&p.rssKiB<1800000))return;
 profiledPhases.add(phase);
 const prefix=out+'/native-'+phase;
 (report.nativeProfiles??=[]).push({phase,pid:p.pid,rssKiB:p.rssKiB,at:sample.at,prefix});
 function run(command,args,suffix){
  profileTasks.push(new Promise(resolve=>{
   const child=spawn(command,args);const output=fs.createWriteStream(prefix+suffix);
   child.stdout.pipe(output,{end:false});child.stderr.pipe(output,{end:false});
   const timer=setTimeout(()=>child.kill('SIGTERM'),20000);
   child.on('error',err=>{output.end(String(err));clearTimeout(timer);resolve();});
   child.on('close',code=>{output.end('\nEXIT_CODE='+code);clearTimeout(timer);resolve();});
  }));
 }
 run('/usr/bin/sample',[String(p.pid),'2','-file',prefix+'-sample.txt'],'-sample-status.txt');
 run('/usr/bin/vmmap',['-summary',String(p.pid)],'-vmmap.txt');
}

const log = event => fs.appendFileSync(out+'/events.jsonl', JSON.stringify({at:new Date().toISOString(),...event})+'\n');
// Baseline is captured BEFORE the Playwright browser is started. Exclude all
// existing WebContent PIDs, especially residue from the prior A/B arm.
const {parsePS,freshBrowserProcesses}=require('./webkit-process-isolation.cjs');
function allProcesses(){return parsePS(execFileSync('ps',['-axo','pid=,ppid=,rss=,comm='],{encoding:'utf8',timeout:3000}));}
const priorBrowserPids=new Set(allProcesses().map(p=>p.pid));
report.processProvenance={method:'fresh browser PIDs since prelaunch ps baseline',
  excludedPidCount:priorBrowserPids.size,samplesAreGlobal:false};
function processes(){
 try {return freshBrowserProcesses(allProcesses(),priorBrowserPids);}
 catch(e) {report.errors.push('PID_ISOLATION_FAILED '+String(e));return [];}
}
(async()=>{
  let browser, page;
  // Longer genuine gameplay journeys must not be cut off by the 7-minute startup A/B watchdog.
  const globalDeadlineMs=Number(process.env.QA_GLOBAL_DEADLINE_MS||420000);
  if(!Number.isSafeInteger(globalDeadlineMs)||globalDeadlineMs<420000||globalDeadlineMs>1200000)
    throw Error('INVALID_QA_GLOBAL_DEADLINE_MS');
  report.globalDeadlineMs=globalDeadlineMs;
  const watchdog=setTimeout(()=>{report.errors.push('GLOBAL_DEADLINE');write();process.exit(1);},globalDeadlineMs);
  try {
    if(!['webkit','chromium'].includes(engine))throw Error('Unknown engine');
    if(process.env.QA_TELEMETRY_BUNDLE &&
       new URL(process.env.QA_URL).searchParams.get('eldoriaBetaTelemetry')!=='1')
      throw Error('ACTUAL_TELEMETRY_REQUIRES_EXPLICIT_NATIVE_OPT_IN_QUERY');
    browser=await ({webkit,chromium}[engine]).launch({headless:true});
    report.version=browser.version();
    report.perDrawTracing=process.env.QA_TRACE_GL==='1';
    const context=await browser.newContext({viewport:{width:390,height:844},deviceScaleFactor:3});
    await context.addInitScript(({variant,traceGL})=>{
      window.__growth=[];
      window.__preallocation=[];
      // Diagnostic A/B: grow the exported heap before Emscripten receives it.
      // No WASM byte, asset, gameplay or public deployment is changed.
      if(/^preallocated-(640|1024)$/.test(variant)){
        for(const method of ['instantiate','instantiateStreaming']){
          const instantiate=WebAssembly[method];if(!instantiate)continue;
          WebAssembly[method]=async function(...args){
            const result=await instantiate.apply(WebAssembly,args);
            const instance=result instanceof WebAssembly.Instance?result:result.instance;
            const memory=instance?.exports?.memory;
            if(memory instanceof WebAssembly.Memory){
              const before=memory.buffer.byteLength,target=Number(variant.split('-')[1])*1024*1024;
              if(before<target)memory.grow(Math.ceil((target-before)/65536));
              const record={at:performance.now(),before,after:memory.buffer.byteLength};
              window.__preallocation.push(record);console.log('ELDORIA_HEAP_PREALLOCATED '+JSON.stringify(record));
            }
            return result;
          };
        }
      }
      window.__glCalls=[];
      const remember=(name,args)=>{
        const a=Array.from(args).map(v=>typeof v==='number'||typeof v==='boolean'||typeof v==='string'?v:(v?.byteLength!==undefined?{byteLength:v.byteLength}:String(v)));
        window.__glCalls.push({at:performance.now(),name,args:a});
        if(window.__glCalls.length>50)window.__glCalls.shift();
      };
      const proto=WebGL2RenderingContext.prototype;
      if(['astc-disabled','s3tc-disabled'].includes(variant)){
        const denied=variant==='astc-disabled'?/compressed_texture_astc/i:/compressed_texture_s3tc/i;
        const ext=proto.getExtension;
        proto.getExtension=function(name){return denied.test(name)?null:ext.call(this,name);};
        const supported=proto.getSupportedExtensions;
        proto.getSupportedExtensions=function(){return supported.call(this)?.filter(x=>!denied.test(x));};
      }
      // Full draw tracing allocates on every draw and perturbs RSS. Opt in only
      // for graphics diagnosis, never for the normal memory comparison.
      if(traceGL)for(const name of ['compressedTexImage2D','compressedTexSubImage2D','texImage2D','texStorage2D','bufferData','compileShader','linkProgram','drawElements','drawArrays']){
        const original=proto[name];
        proto[name]=function(...args){remember(name,args);return original.apply(this,args);};
      }
      const getContext=HTMLCanvasElement.prototype.getContext;
      HTMLCanvasElement.prototype.getContext=function(type,...args){
        const gl=getContext.call(this,type,...args);
        if(this.id==='unity-canvas'&&gl&&type==='webgl2'&&!this.__observed){
          this.__observed=true;
          const ext=gl.getExtension('WEBGL_debug_renderer_info');
          console.log('ELDORIA_ACTUAL_GL '+JSON.stringify({at:performance.now(),astc:!!gl.getExtension('WEBGL_compressed_texture_astc'),renderer:gl.getParameter(ext?ext.UNMASKED_RENDERER_WEBGL:gl.RENDERER),uniformBindings:gl.getParameter(gl.MAX_UNIFORM_BUFFER_BINDINGS),attributes:gl.getContextAttributes()}));
          this.addEventListener('webglcontextlost',e=>console.error('ELDORIA_CONTEXT_LOST '+JSON.stringify({at:performance.now(),statusMessage:e.statusMessage,lastCalls:window.__glCalls})),{once:true});
        }
        return gl;
      };
      const original=WebAssembly.Memory.prototype.grow;
      WebAssembly.Memory.prototype.grow=function(pages){
        const x={at:performance.now(),before:this.buffer.byteLength,pages};
        window.__growth.push(x);
        try {const ret=original.call(this,pages);x.after=this.buffer.byteLength;return ret;}
        catch(e){x.error=String(e);throw e;}
      };
    },{variant,traceGL:process.env.QA_TRACE_GL==='1'});
    page=await context.newPage();
    if(process.env.QA_TELEMETRY_BUNDLE){
      report.telemetry={method:'QA-only exact pilot script injection into actual public owner HTML; no HTML or engine deployment',
        endpoint:'eldoria-mobile16-events-qa',installations:0,receipts:[],notPhysicalDevice:true};
      page.on('response',response=>{
        if(response.url()!=='https://eavaxwpxxcrwibooiimb.supabase.co/functions/v1/eldoria-mobile16-events-qa' ||
           response.request().method()!=='POST')return;
        try{
          const event=response.request().postDataJSON();
          report.telemetry.receipts.push({eventId:event.client_event_id,eventName:event.event_name,
            payload:event.payload,status:response.status()});write();
        }catch(error){report.errors.push('TELEMETRY_RECEIPT_PARSE '+String(error));}
      });
    }
    async function installOptInTelemetry(){
      if(!process.env.QA_TELEMETRY_BUNDLE)return;
      await page.addScriptTag({content:fs.readFileSync(process.env.QA_TELEMETRY_BUNDLE,'utf8')});
      if(report.telemetry.installations===0){
        await page.getByRole('button',{name:'Sí, compartir',exact:true}).click({timeout:15000});
      }
      await page.getByRole('button',{name:'Desactivar estadísticas',exact:true}).waitFor({state:'visible',timeout:15000});
      report.telemetry.installations++;write();
    }
    page.on('console',m=>log({type:'console',level:m.type(),text:m.text()}));
    page.on('pageerror',e=>{report.errors.push(String(e));log({type:'pageerror',text:String(e)});write();});
    page.on('crash',()=>{report.errors.push('PAGE_CRASH');log({type:'crash'});write();});
    page.on('requestfailed',r=>log({type:'requestfailed',url:r.url(),error:r.failure()}));
    page.on('framenavigated',f=>{if(f===page.mainFrame()){report.navigations.push({at:Date.now(),url:f.url()});write();}});
    await page.goto(process.env.QA_URL||'https://eldoria-valoria-beta-20261108.kbkvc5c45n.chatgpt.site/?eldoria_diag=1&build=mobile5',{waitUntil:'domcontentloaded',timeout:30000});
    report.identity=await page.evaluate(()=>fetch('release-identity.json',{cache:'no-store'}).then(r=>r.json()));
    if(report.identity.revision!==(process.env.QA_REVISION||'native325-mobile5'))throw Error('Candidate identity changed');
    report.capabilities=process.env.QA_SKIP_PREFLIGHT==='1'?{webgl2:'not-probed',note:'Observe actual Unity context only'}:await page.evaluate(()=>{
      const c=document.createElement('canvas'),g=c.getContext('webgl2');
      if(!g)return {webgl2:false,userAgent:navigator.userAgent};
      const d=g.getExtension('WEBGL_debug_renderer_info');
      const result={webgl2:true,userAgent:navigator.userAgent,astc:!!g.getExtension('WEBGL_compressed_texture_astc'),renderer:g.getParameter(d?d.UNMASKED_RENDERER_WEBGL:g.RENDERER)};
      g.getExtension('WEBGL_lose_context')?.loseContext();return result;
    });
    write();
    if(!report.capabilities.webgl2)throw Error('ENVIRONMENT_NO_WEBGL2');
    const expectedOrigin=process.env.QA_EXPECT_ORIGIN;
    const verifyOrigin=()=>{if(expectedOrigin&&new URL(page.url()).origin!==expectedOrigin)throw Error('QA_UNEXPECTED_ORIGIN '+page.url());};
    verifyOrigin();
    await installOptInTelemetry();
    await page.locator('#play').click();
    let readyAt=0,earlyEntry=false;
    const started=Date.now();
    while(Date.now()-started<330000){
      verifyOrigin();
      if(report.errors.length)throw Error('Browser failure');
      const s=await page.evaluate(()=>{
        const game=window.valoriaGame;
        let metrics=null;try{metrics=game?.GetMetricsInfo?.();}catch(e){metrics={error:String(e)};}
        return {at:performance.now(),ready:!!game,metrics,growth:window.__growth,preallocation:window.__preallocation,
          diagnostic:window.EldoriaLocalDiagnostic?.read?.(),
          p0LocalPhaseProfile:window.__eldoriaP0Postready?.active ? window.__eldoriaP0Postready.report() : null,
          visible:!document.getElementById('game').hidden,
          error:document.getElementById('error')?.textContent,
          wasmCapacityBytes:game?.Module?.HEAPU8?.byteLength||0,canvas:{width:document.getElementById('unity-canvas').width,height:document.getElementById('unity-canvas').height},stableCanvasChanges:window.__eldoriaStableCanvas?.changes};
      });
      s.processes=processes();report.samples.push(s);nativeProfile(s,'fresh');write();
      if(s.ready&&!readyAt){readyAt=Date.now();log({type:'unity-ready'});}
      if(readyAt&&(!s.ready||!s.visible))throw Error('Unity exited after ready');
      if(process.env.QA_EARLY_ENTRY==='1'&&readyAt&&!earlyEntry&&Date.now()-readyAt>2000){
        if(process.env.QA_EARLY_NO_SCREENSHOT!=='1')await page.screenshot({path:out+'/early-entry-before.png',timeout:15000});
        await page.mouse.click(195,490,{delay:200});
        if(process.env.QA_RESIZE_STRESS==='1'){
          report.resizeSequence=[];
          for(const height of [500,390,640,844]){
            await page.setViewportSize({width:390,height});await page.waitForTimeout(350);
            report.resizeSequence.push(await page.evaluate(()=>({viewport:{width:innerWidth,height:innerHeight},canvas:{width:document.getElementById('unity-canvas').width,height:document.getElementById('unity-canvas').height}})));
          }
        }
        await page.keyboard.type('Beta QA',{delay:140});await page.mouse.click(195,614,{delay:200});
        earlyEntry=true;report.earlyEntry={attempted:true,secondsAfterReady:(Date.now()-readyAt)/1000};write();
      }
      if(readyAt&&Date.now()-readyAt>90000)break;
      await page.waitForTimeout(1000);
    }
    if(!readyAt||Date.now()-readyAt<90000)throw Error('Did not survive 90s after ready');
    if(new URL(process.env.QA_URL||'http://localhost/').searchParams.get('eldoria_p0')==='1'){
      const last=report.samples.at(-1)?.p0LocalPhaseProfile;
      if(!last)throw Error('P0 local diagnostics did not install');
      const recorded=JSON.parse(last).events;
      if(!recorded.some(e=>e[1]==='unity-log'&&e[2].startsWith('ELDORIA_MEMORY_PHASE phase=prefab-before')))
        throw Error('P0 local diagnostics did not persist Unity memory phases');
      report.p0LocalPhaseRecorderVerified=true;
    }
    const events=fs.readFileSync(out+'/events.jsonl','utf8');
    // Validate genuine Unity logs for each URL variant. Lazy is a city-only
    // readiness measurement until the separately gated real-world test runs.
    Object.assign(report,verifyWorldStart({
      cityOnly:process.env.QA_CITY_ONLY==='1',
      lazyWorld:process.env.QA_LAZY_WORLD==='1',
      url:process.env.QA_URL||page.url(),
      events
    }));
    write();
    if(/not readable|Could not allocate|out of memory|RuntimeError:|Exception:|ELDORIA_CONTEXT_LOST|GL.End requires/i.test(events))throw Error('Unity runtime errors');
    await page.screenshot({path:out+'/game.png',timeout:15000});
    async function observeEntry(seconds,phase){
      if(!report.entrySamples)report.entrySamples=[];
      const deadline=Date.now()+seconds*1000;
      while(Date.now()<deadline){
        const sample=await page.evaluate(()=>({at:performance.now(),ready:!!window.valoriaGame,
          visible:!document.getElementById('game').hidden,
          metrics:window.valoriaGame?.GetMetricsInfo?.(),wasmCapacityBytes:window.valoriaGame?.Module?.HEAPU8?.byteLength||0}));
        sample.phase=phase;sample.processes=processes();report.entrySamples.push(sample);if(phase==='reloaded')nativeProfile(sample,phase);write();
        verifyOrigin();
        if(report.errors.length||!sample.ready||!sample.visible)throw Error('Unity failed during '+phase);
        await page.waitForTimeout(1000);
      }
    }
    if(process.env.QA_ENTRY==='1'){
      if(!earlyEntry){await page.mouse.click(195,490,{delay:200});await page.keyboard.type('Beta QA',{delay:140});await page.mouse.click(195,614,{delay:200});}
      await observeEntry(30,"after-name");await page.screenshot({path:out+'/after-name.png',timeout:15000});
      await page.evaluate(()=>window.valoriaGame.Quit());
      await page.reload({waitUntil:'domcontentloaded'});await installOptInTelemetry();await page.locator('#play').click();
      await page.waitForFunction(()=>!!window.valoriaGame,null,{timeout:120000});await observeEntry(45,"reloaded");
      await page.screenshot({path:out+'/reloaded.png',timeout:15000});
      const loadedIdentity=await page.evaluate(()=>fetch('release-identity.json',{cache:'no-store'}).then(r=>r.json()));
      if(loadedIdentity.source_sha!==report.identity.source_sha||loadedIdentity.revision!==report.identity.revision)throw Error('QA_RELOADED_SOURCE_IDENTITY_CHANGED');
      report.reloadedIdentity={source_sha:loadedIdentity.source_sha,revision:loadedIdentity.revision};
      report.entry={nickname:'Beta QA',typed:true,reloaded:true,observationSeconds:45,visualNameVerification:'pending image inspection'};
      if(process.env.QA_ADVANCE_STORY==='1'){
        report.storyAdvanceAttempt={actualCanvasClicks:[],realWorldEntryVerified:false};
        for(let step=0;step<5;step++){
          // Actual 390x844 canvas position from the original Mac screenshot.
          // A click attempt is not proof of completing a narrative or a quest.
          await page.mouse.click(300,735,{delay:120});await page.waitForTimeout(700);
          const file='story-step-'+step+'.png';
          await page.screenshot({path:out+'/'+file,timeout:15000});
          report.storyAdvanceAttempt.actualCanvasClicks.push({step,x:300,y:735,file});
          write();
        }
        await observeEntry(3,"after-story-clicks");
        if(process.env.QA_ACTUAL_WORLD_UI==='1'){
          report.actualWorldUi={method:'Apple Vision reads original browser pixels; clicks actual visible controls only',actions:[],realWorldEntryVerified:false,physicalIphoneVerified:false};
          const normalized=s=>s.normalize('NFD').replace(/[\u0300-\u036f]/g,'').toUpperCase().trim();
          async function readUi(phase){
            const file=out+'/'+phase+'.png';await page.screenshot({path:file,timeout:15000});
            const p=require('child_process').spawnSync(process.env.QA_OCR_EXECUTABLE,[file],{encoding:'utf8',timeout:20000});
            if(p.status!==0)throw Error('ACTUAL_UI_OCR_FAILED '+p.stderr);
            const rows=JSON.parse(p.stdout);fs.writeFileSync(out+'/'+phase+'-ocr.json',JSON.stringify(rows,null,2));return rows;
          }
          async function clickText(phase,label,filter=()=>true){
            const rows=await readUi(phase);const hits=rows.filter(r=>normalized(r.text)===label&&r.confidence>=.35&&filter(r));
            if(hits.length!==1)throw Error('VISIBLE_CONTROL_NOT_UNIQUE '+label+' '+JSON.stringify(hits));
            const hit=hits[0];await page.mouse.click(hit.x,hit.y,{delay:150});
            report.actualWorldUi.actions.push({phase,text:hit.text,x:hit.x,y:hit.y,fromActualScreenshot:phase+'.png'});write();await observeEntry(2,phase);
          }
          await readUi('city-intro-complete');
          // Reviewed untouched 1170x2532 image from actual run43158802987:
          // the shipped UX19 uses a circular arrow at CSS211,69 with "Ir",
          // rather than the longer label in the underlying vanilla binding.
          await page.mouse.click(211,69,{delay:150});
          report.actualWorldUi.actions.push({phase:'city-intro-complete',x:211,y:69,
            control:'visible canonical objective arrow',coordinateEvidenceRun:43158802987,
            coordinateEvidenceSha256:'bc6df280e6b396e88839ca30330f61f387a89e7cbe9a34232e67cd4a02d48901'});
          write();await observeEntry(3,'actual-objective-arrow');
          // Mission action selects the canonical sawmill. Exclude the bottom
          // navigation Build medallion when locating its real paid offer.
          await clickText('actual-sawmill-offer','CONSTRUIR',r=>r.y<750);
          // Stop the wait only when genuine on-screen NIVEL 1 is visible; still
          // fail closed if construction has not completed within 120 seconds.
          const sawmillStartedAt=Date.now();
          let sawmillLevelOne=false;
          for(let attempt=0;Date.now()-sawmillStartedAt<120000;attempt++){
            await observeEntry(4,'actual-sawmill-construction-'+attempt);
            const labels=await readUi('actual-sawmill-after-wait');
            if(labels.some(r=>normalized(r.text)==='NIVEL 1'&&r.confidence>=.35)){
              sawmillLevelOne=true;
              report.actualWorldUi.sawmillLevelOne={visible:true,elapsedSeconds:(Date.now()-sawmillStartedAt)/1000,proof:'actual-sawmill-after-wait.png'};
              write();break;
            }
          }
          if(!sawmillLevelOne)throw Error('SAWMILL_LEVEL1_NOT_VISIBLE_WITHIN_120S');
          // Closing a contextual building panel via genuine city navigation is
          // optional: only click the unique visible World medallion.
          await readUi('actual-world-control');
          // Actual run43160047196 shows the unlocked World compass at
          // CSS152,799. Its label is covered by the opt-in diagnostic badge;
          // the exposed compass itself remains a genuine clickable control.
          const worldEventCursor=fs.statSync(out+'/events.jsonl').size;
          await page.mouse.click(152,799,{delay:150});
          report.actualWorldUi.actions.push({phase:'actual-world-control',x:152,y:799,
            control:'visible unlocked World compass above diagnostic badge',
            coordinateEvidenceRun:43160047196,
            coordinateEvidenceSha256:'9c39cd34879c0f90e37fa78111430274330aaedbca628eb4f8f7b3325a09abd5'});
          write();
          await observeEntry(60,'after-real-world-click');
          const worldEvents=fs.readFileSync(out+'/events.jsonl').subarray(worldEventCursor).toString('utf8');
          report.actualWorldUi.realRegion1ConstructedAfterUserWorldClick=worldEvents.includes('REGION1_CANON_WORLD_INSTALLED')&&worldEvents.includes('ELDORIA_P0_LAZY_WORLD_ENTERED');
          if(!report.actualWorldUi.realRegion1ConstructedAfterUserWorldClick)throw Error('NO_REAL_REGION1_AFTER_ACTUAL_WORLD_CLICK');
          await readUi('actual-world-after-entry');
          report.realWorldEntryVerified=true;
          report.storyAdvanceAttempt.realWorldEntryVerified=true;
          report.actualWorldUi.realWorldEntryVerified=true;
          report.actualWorldUi.visualWorldVerification='pending exact screenshot inspection';write();
          report.actualWorldUi.worldIntroductionClicks=[];
          for(let step=0;step<8;step++){
            const phase='world-story-'+step,rows=await readUi(phase);
            const matches=rows.filter(r=>normalized(r.text)==='CONTINUAR'&&r.confidence>=.35);
            if(matches.length===0){report.actualWorldUi.worldIntroNoVisibleContinue=true;break;}
            if(matches.length!==1)throw Error('WORLD_CONTINUE_NOT_UNIQUE '+JSON.stringify(matches));
            const hit=matches[0];await page.mouse.click(hit.x,hit.y,{delay:150});
            report.actualWorldUi.worldIntroductionClicks.push({step,text:hit.text,x:hit.x,y:hit.y});
            write();await observeEntry(3,'world-story-'+step);
          }
          if(!report.actualWorldUi.worldIntroductionClicks.length ||
             !report.actualWorldUi.worldIntroNoVisibleContinue)
            throw Error('WORLD_INTRODUCTION_NOT_COMPLETED_THROUGH_VISIBLE_UI');
          await observeEntry(90,'world-after-introduction');
          await readUi('actual-world-ready');
          // The same exposed genuine navigation medallion is now CITY:
          // actual world screenshot43161378432 shows its castle at152,799.
          await page.mouse.click(152,799,{delay:150});
          report.actualWorldUi.actualCityReturnAttempted=true;write();
          await observeEntry(30,'actual-city-return');
          await readUi('actual-city-return');
          if(process.env.QA_NEXT_OBJECTIVE==='1'){
            report.nextObjectiveExploration={method:'Visible objective arrow and unique OCR-confirmed controls only; no save/time injection',actions:[],progressionCertified:false};
            // Same original canonical objective arrow already verified above.
            await page.mouse.click(211,69,{delay:150});
            report.nextObjectiveExploration.actions.push({control:'canonical objective arrow',x:211,y:69,coordinateEvidenceRun:43158802987});
            await observeEntry(3,'next-objective-focused');
            // Actual original run43192581903 shows "Recoger madera"
            // at194,747 after the real objective arrow, and its9-second
            // roundtrip. Read every new visible state; no command injection.
            const allowed=new Set(['RECOGER MADERA','RECOGER PIEDRA',
              'ATACAR EXPLORADOR','MEJORAR A BASTION II','CONSTRUIR']);
            const reachedTwo=()=>report.telemetry?.receipts.some(r=>r.status===202&&
              r.eventName==='bastion_reached'&&r.payload?.bastion===2);
            const builtBarracks=()=>report.telemetry?.receipts.some(r=>r.status===202&&
              r.eventName==='build_completed'&&r.payload?.building_code==='barracks'&&r.payload?.level===1);
            for(let step=0;step<16;step++){
              if(reachedTwo()&&(process.env.QA_BUILD_BARRACKS!=='1'||builtBarracks())){
                report.nextObjectiveExploration.actualBastionTwoReached=true;
                report.nextObjectiveExploration.actualBarracksBuilt=!!builtBarracks();
                report.nextObjectiveExploration.stopReason=builtBarracks()?
                  'actual_native_barracks_one_signal':'actual_native_bastion_two_signal';break;
              }
              if(step>0){
                await page.mouse.click(211,69,{delay:150});
                await observeEntry(2,'ordinary-objective-focus-'+step);
              }
              const phase='next-objective-'+step,rows=await readUi(phase);
              const mission=rows.filter(r=>r.y>=50&&r.y<=130&&r.x<195)
                .sort((a,b)=>a.y-b.y).map(r=>normalized(r.text)).join(' ');
              if(/REGRESA A.{0,24}VALORIA/.test(mission)){
                // Original exposed CITY compass already completed the exact
                // normal return in both43179910727 and43196662146.
                await page.mouse.click(152,799,{delay:150});
                report.nextObjectiveExploration.actions.push({control:'actual CITY compass',
                  x:152,y:799,evidence:phase+'.png',purpose:'ordinary return objective'});
                write();await observeEntry(4,phase+'-normal-return');continue;
              }
              const nodeMatches=rows.filter(r=>r.confidence>=.5&&r.y>730&&r.y<780&&
                allowed.has(normalized(r.text)));
              const continues=rows.filter(r=>r.confidence>=.5&&normalized(r.text)==='CONTINUAR');
              const matches=continues.length?continues:nodeMatches;
              if(matches.length!==1){report.nextObjectiveExploration.stopReason=
                matches.length?'ambiguous_visible_actions':'no_known_visible_action';break;}
              const hit=matches[0],label=normalized(hit.text);
              await page.mouse.click(hit.x,hit.y,{delay:150});
              report.nextObjectiveExploration.actions.push({text:hit.text,x:hit.x,y:hit.y,
                evidence:phase+'.png',ordinaryUiOnly:true});
              write();
              // Exact shipped4fd economy catalog authors120seconds for II.
              // Observe real elapsed time and native emission; never set a clock.
              await observeEntry(label==='MEJORAR A BASTION II'?130:
                label==='CONTINUAR'?3:12,phase+'-after-action');
            }
            report.nextObjectiveExploration.actualBastionTwoReached=!!reachedTwo();
            report.nextObjectiveExploration.actualRouteVictory=!!report.telemetry?.receipts.some(
              r=>r.status===202&&r.eventName==='battle_resolved'&&r.payload?.outcome_code==='win');
            await observeEntry(25,'next-objective-observation');
            await readUi('next-objective-final');write();
            if(process.env.QA_COMPLETE_CHAPTER_ONE==='1'&&
               (!report.nextObjectiveExploration.actualBastionTwoReached||
                !report.nextObjectiveExploration.actualRouteVictory))
              throw Error('ACTUAL_CHAPTER_ONE_NOT_COMPLETED '+JSON.stringify(report.nextObjectiveExploration));
            if(process.env.QA_BUILD_BARRACKS==='1'&&!builtBarracks())
              throw Error('ACTUAL_BARRACKS_NOT_BUILT_THROUGH_VISIBLE_UI');
            write();

          }

        }
      }
      const finalEvents=fs.readFileSync(out+'/events.jsonl','utf8');
      if(report.errors.length||/not readable|Could not allocate|out of memory|RuntimeError:|Exception:|ELDORIA_CONTEXT_LOST|GL.End requires/i.test(finalEvents))throw Error('Entry/reload runtime errors');
    }
    if(/^preallocated-(640|1024)$/.test(variant)&&!report.samples.some(s=>s.preallocation?.some(p=>p.after===Number(variant.split('-')[1])*1024*1024)))throw Error('Preallocation intervention not observed');
    if(process.env.QA_NATIVE_PROFILE==='1')nativeProfile({processes:processes(),at:Date.now()},'final');
    if(report.telemetry){
      await page.waitForTimeout(3000);
      const accepted=new Set(report.telemetry.receipts.filter(r=>r.status===202).map(r=>r.eventName));
      const required=['session_start','game_ready','return_visit','build_completed','world_opened'];
      report.telemetry.requiredActualJourneyEvents=required;
      report.telemetry.missing=required.filter(name=>!accepted.has(name));
      report.telemetry.rejected=report.telemetry.receipts.filter(r=>r.status!==202);
      if(report.telemetry.installations!==2 || report.telemetry.missing.length || report.telemetry.rejected.length)
        throw Error('ACTUAL_UNITY_TELEMETRY_CHAIN_INCOMPLETE '+JSON.stringify(report.telemetry));
      report.telemetry.actualJourneyBrowserReceiptsVerified=true;
      report.telemetry.databaseReceiptsVerified=false;
    }
    if(process.env.QA_EXPORT_ORDINARY_STATE==='1'){
      // Gameplay proof is already complete. End only THIS fresh disposable
      // QA game's document before discarding its regenerable UnityCache.
      // The actual earned /idbfs campaign database is never modified.
      report.gameplayPassed=true;
      const before=await page.evaluate(async()=>(await indexedDB.databases()).map(d=>({name:d.name,version:d.version})));
      if(!before.some(d=>d.name==='/idbfs')||
         before.some(d=>!['/idbfs','UnityCache','UnityPlayerPrefs'].includes(d.name)))
        throw Error('UNKNOWN_QA_DATABASE_DO_NOT_TOUCH');
      const origin=new URL(page.url()).origin;
      if(origin!=='https://mt5hjfz2kb-lab.github.io')throw Error('QA_SAVE_ORIGIN_MISMATCH');
      const identityUrl=new URL('./release-identity.json',page.url()).href;
      await page.evaluate(()=>window.valoriaGame.Quit());
      await page.goto(identityUrl,{waitUntil:'domcontentloaded',timeout:30000});
      const shown=JSON.parse(await page.locator('body').innerText());
      if(shown.source_sha!==report.identity.source_sha||shown.revision!==report.identity.revision)
        throw Error('QA_CHECKPOINT_GAME_IDENTITY_CHANGED');
      const deleted=await page.evaluate(()=>new Promise((resolve,reject)=>{
        const request=indexedDB.deleteDatabase('UnityCache');
        const timer=setTimeout(()=>reject(Error('QA_ASSET_CACHE_CLOSE_TIMEOUT')),15000);
        request.onerror=()=>{clearTimeout(timer);reject(Error('QA_ASSET_CACHE_DELETE_FAILED'));};
        request.onblocked=()=>{clearTimeout(timer);reject(Error('QA_ASSET_CACHE_CONNECTION_STILL_OPEN'));};
        request.onsuccess=()=>{clearTimeout(timer);resolve(true);};
      }));
      const after=await page.evaluate(async()=>(await indexedDB.databases()).map(d=>({name:d.name,version:d.version})));
      const savedBefore=before.find(d=>d.name==='/idbfs'),savedAfter=after.find(d=>d.name==='/idbfs');
      if(!savedAfter||savedAfter.version!==savedBefore.version||
         after.some(d=>!['/idbfs','UnityPlayerPrefs'].includes(d.name)))
        throw Error('EARNED_QA_SAVE_DATABASE_NOT_PRESERVED');
      // Official API since1.51; current runner is pinned1.55.
      const state=await page.context().storageState({indexedDB:true});
      if(state.cookies.length||state.origins.some(o=>o.origin!==origin))
        throw Error('QA_CHECKPOINT_UNEXPECTED_COOKIE_OR_ORIGIN');
      const bytes=JSON.stringify(state);
      if(Buffer.byteLength(bytes)>4000000)throw Error('QA_CHECKPOINT_UNEXPECTEDLY_LARGE');
      fs.writeFileSync(out+'/ordinary-qa-storage-state.json',bytes);
      report.ordinaryQaCheckpoint={exported:true,method:'PLAYWRIGHT_OFFICIAL_INDEXEDDB_SNAPSHOT_AFTER_QA_GAME_CLOSED',
        sourceEngine:report.identity.source_sha,sourceBundle:report.identity.candidate_bundle_sha256,
        origin,bytes:Buffer.byteLength(bytes),databaseBefore:before,databaseAfter:after,
        disposableAssetCacheDiscardedAfterGameClosed:deleted,earnedSaveDatabaseModified:false,
        ownerSaveAccessed:false,fakeStateCreated:false,
        sha256:require('crypto').createHash('sha256').update(bytes).digest('hex')};
      write();
      if(report.errors.length)throw Error('ERROR_DURING_ORDINARY_QA_SAVE_EXPORT');
    }
    report.passed=true;
  }catch(e){report.failure=String(e);}
  finally{
    await Promise.all(profileTasks);write();clearTimeout(watchdog);
    process.stdout.write(JSON.stringify({passed:report.passed,failure:report.failure,actualWorldUi:report.actualWorldUi,errorCount:report.errors.length})+"\\n");
    const closeTimer=setTimeout(()=>process.exit(report.passed?0:1),10000);
    if(browser)await browser.close().catch(()=>{});
    clearTimeout(closeTimer);
  }
  if(!report.passed)process.exitCode=1;
})();
