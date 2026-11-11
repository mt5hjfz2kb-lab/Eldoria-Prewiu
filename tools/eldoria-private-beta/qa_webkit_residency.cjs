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
  const watchdog=setTimeout(()=>{report.errors.push('GLOBAL_DEADLINE');write();process.exit(1);},420000);
  try {
    if(!['webkit','chromium'].includes(engine))throw Error('Unknown engine');
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
      await page.reload({waitUntil:'domcontentloaded'});await page.locator('#play').click();
      await page.waitForFunction(()=>!!window.valoriaGame,null,{timeout:120000});await observeEntry(45,"reloaded");
      await page.screenshot({path:out+'/reloaded.png',timeout:15000});
      const loadedIdentity=await page.evaluate(()=>fetch('release-identity.json',{cache:'no-store'}).then(r=>r.json()));
      if(loadedIdentity.source_sha!==report.identity.source_sha||loadedIdentity.revision!==report.identity.revision)throw Error('QA_RELOADED_SOURCE_IDENTITY_CHANGED');
      report.reloadedIdentity={source_sha:loadedIdentity.source_sha,revision:loadedIdentity.revision};
      report.entry={nickname:'Beta QA',typed:true,reloaded:true,observationSeconds:45,visualNameVerification:'pending image inspection'};
      const finalEvents=fs.readFileSync(out+'/events.jsonl','utf8');
      if(report.errors.length||/not readable|Could not allocate|out of memory|RuntimeError:|Exception:|ELDORIA_CONTEXT_LOST|GL.End requires/i.test(finalEvents))throw Error('Entry/reload runtime errors');
    }
    if(/^preallocated-(640|1024)$/.test(variant)&&!report.samples.some(s=>s.preallocation?.some(p=>p.after===Number(variant.split('-')[1])*1024*1024)))throw Error('Preallocation intervention not observed');
    if(process.env.QA_NATIVE_PROFILE==='1')nativeProfile({processes:processes(),at:Date.now()},'final');
    report.passed=true;
  }catch(e){report.failure=String(e);}
  finally{
    await Promise.all(profileTasks);write();clearTimeout(watchdog);
    const closeTimer=setTimeout(()=>process.exit(report.passed?0:1),10000);
    if(browser)await browser.close().catch(()=>{});
    clearTimeout(closeTimer);
  }
  if(!report.passed)process.exitCode=1;
})();
