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
  if(!Number.isSafeInteger(globalDeadlineMs)||globalDeadlineMs<420000||globalDeadlineMs>5400000)
    throw Error('INVALID_QA_GLOBAL_DEADLINE_MS');
  report.globalDeadlineMs=globalDeadlineMs;
  const watchdog=setTimeout(()=>{report.deadlineReached=true;write();},globalDeadlineMs);
  try {
    if(!['webkit','chromium'].includes(engine))throw Error('Unknown engine');
    if(process.env.QA_TELEMETRY_BUNDLE &&
       new URL(process.env.QA_URL).searchParams.get('eldoriaBetaTelemetry')!=='1')
      throw Error('ACTUAL_TELEMETRY_REQUIRES_EXPLICIT_NATIVE_OPT_IN_QUERY');
    browser=await ({webkit,chromium}[engine]).launch({headless:true});
    report.version=browser.version();
    report.perDrawTracing=process.env.QA_TRACE_GL==='1';
    const statePath=process.env.QA_EARNED_STATE;
    const stateBytes=fs.readFileSync(statePath);
    const digest=require('crypto').createHash('sha256').update(stateBytes).digest('hex');
    const inputProof=JSON.parse(fs.readFileSync(process.env.QA_CONTROL_INPUT_REPORT,'utf8'));
    const cp=inputProof.ordinaryQaCheckpoint;
    if(!inputProof.passed||!cp?.exported||cp.sourceEngine!=='4fd1cacc9c206cfc20e78b20738e4b5bc48e0e71'||
       cp.sourceBundle!=='49a2d76277a02c7dad31cc1fd46c641d4f15144c2c02e2c6dba1b01ec8c9d66a'||
       !/^[a-f0-9]{64}$/.test(cp.sha256)||cp.ownerSaveAccessed!==false||cp.fakeStateCreated!==false)
      throw Error('UNPROVEN_EARNED_CAMPAIGN_INPUT');
    if(digest!==cp.sha256)
      throw Error('EARNED_CHECKPOINT_BYTES_CHANGED');
    const earned=JSON.parse(stateBytes);
    if(stateBytes.length!==cp.bytes||stateBytes.length>4000000||earned.cookies.length||earned.origins.length!==1||
       earned.origins[0].origin!=='https://mt5hjfz2kb-lab.github.io'||
       earned.origins[0].indexedDB.length!==1||earned.origins[0].indexedDB[0].name!=='/idbfs')
      throw Error('EARNED_CHECKPOINT_SCHEMA_CHANGED');
    report.resumedEarnedCampaign={sourceRun:Number(process.env.QA_EARNED_RUN),sourceSha:process.env.QA_EARNED_SOURCE,sha256:digest,modified:false};
    const context=await browser.newContext({viewport:{width:390,height:844},deviceScaleFactor:3,storageState:statePath});
    // Isolated candidate compatibility test: preserve the public-origin IDBFS
    // database bytes while serving only the exact unpublished engine17 artifact.
    // This changes no public file, save, gameplay clock or owner session.
    const candidate17=process.env.QA_CANDIDATE_MODE==='engine17-save-compat';
    if(process.env.QA_CANDIDATE_MODE&&!candidate17)throw Error('UNKNOWN_CANDIDATE_MODE');
    if(candidate17){
      if(process.env.QA_TELEMETRY_BUNDLE||new URL(process.env.QA_URL).searchParams.has('eldoriaBetaTelemetry'))
        throw Error('CANDIDATE_COMPATIBILITY_MUST_NOT_SEND_TELEMETRY');
      const path=require('path'),root=path.resolve(process.env.QA_CANDIDATE_ROOT||'');
      if(!process.env.QA_CANDIDATE_ROOT)throw Error('MISSING_CANDIDATE_ROOT');
      const identity=JSON.parse(fs.readFileSync(path.join(root,'release-identity.json'),'utf8'));
      if(identity.source_sha!=='a8b0ee4febf85d20eff3280133a37cac17fae967'||
         identity.candidate_bundle_sha256!=='ee8eb5ab636ddc7381b9a8a355580b509a9f9f79a442f1b4987140bb58a25ad5'||
         identity.revision!=='native325-mobile17-caption-profile-owner-qa')
        throw Error('UNPINNED_UNPUBLISHED_CANDIDATE');
      const prefix='https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/beta-iphone-p0-lazy-world/';
      if(!process.env.QA_URL.startsWith(prefix))throw Error('CANDIDATE_ORIGIN_PATH_CHANGED');
      report.unpublishedCandidateCompatibility={publicDeploymentChanged:false,
        earnedInputModified:false,sourceEngine:cp.sourceEngine,sourceBundle:cp.sourceBundle,
        candidateEngine:identity.source_sha,candidateBundle:identity.candidate_bundle_sha256,
        sourceArtifactRun:43220783570,sourceArtifactId:13944774610,
        sourceArtifactSha256:'276f4c7a9f5cbd2c0b7d520063fa06b33dc5174d0dc4e376cecd25619eb089ad',
        physicalIphone:false,routeFiles:[]};
      await context.route(prefix+'**',async route=>{
        const requested=route.request().url();
        const relative=decodeURIComponent(new URL(requested).pathname.slice(new URL(prefix).pathname.length))||'index.html';
        const file=path.resolve(root,relative);
        if(!file.startsWith(root+path.sep)||!fs.existsSync(file)||!fs.statSync(file).isFile()){
          report.errors.push('CANDIDATE_ASSET_NOT_FOUND '+relative);
          await route.fulfill({status:404,body:'Unpublished QA candidate asset missing'});return;
        }
        const body=fs.readFileSync(file);
        const sha256=require('crypto').createHash('sha256').update(body).digest('hex');
        report.unpublishedCandidateCompatibility.routeFiles.push({path:relative,bytes:body.length,sha256});
        const mime=file.endsWith('.html')?'text/html':file.endsWith('.js')||file.endsWith('.mjs')?'application/javascript':
          file.endsWith('.json')?'application/json':file.endsWith('.css')?'text/css':'application/octet-stream';
        await route.fulfill({status:200,body,headers:{'content-type':mime,'cache-control':'no-store',
          'cross-origin-opener-policy':'same-origin','cross-origin-embedder-policy':'require-corp'}});
      });
    }

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
      if(report.telemetry.installations===0 && !await page.getByRole('button',{name:'Desactivar estadísticas',exact:true}).isVisible()){
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
    const expectedEngine=candidate17?'a8b0ee4febf85d20eff3280133a37cac17fae967':cp.sourceEngine;
    const expectedBundle=candidate17?'ee8eb5ab636ddc7381b9a8a355580b509a9f9f79a442f1b4987140bb58a25ad5':cp.sourceBundle;
    if(report.identity.source_sha!==expectedEngine||report.identity.candidate_bundle_sha256!==expectedBundle)
      throw Error('ENGINE_DIFFERS_FROM_PINNED_EARNED_OR_COMPATIBILITY_PROVENANCE');
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


    // One foreground test session. This is not an automation or a scheduled
    // supervisor. Commands can only operate observed UI in this disposable QA
    // player; no game-state API, eval, clock adjustment or payment is exposed.
    const {DefaultArtifactClient}=require('@actions/artifact');
    const artifact=new DefaultArtifactClient();
    const path=require('path'),crypto=require('crypto');
    const controllerBranch='qa/eldoria-earned-campaign-20261112';
    const commandPath='tools/eldoria-private-beta/qa-control-command.json';
    const runId=Number(process.env.GITHUB_RUN_ID);
    if(!Number.isSafeInteger(runId)||!process.env.GH_TOKEN)throw Error('NO_SCOPED_ACTION_CONTEXT');
    report.controlledSession={runId,source:process.env.GITHUB_SHA,scheduledTaskCreated:false,
      actions:[],snapshots:[],lastCommandSequence:0};
    report.actualWorldUi={method:'Fresh original screenshots and Apple Vision; controlled actual UI only',actions:[]};
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


    let snapshotNumber=0,lastProof=null,finished=false;
    async function publishSnapshot(reason,error){
      verifyOrigin();
      const number=snapshotNumber++,phase='control-'+String(number).padStart(3,'0');
      const rows=await readUi(phase);
      const png=fs.readFileSync(out+'/'+phase+'.png');
      const sha256=crypto.createHash('sha256').update(png).digest('hex');
      const folder=path.resolve(out,phase);fs.mkdirSync(folder,{recursive:true});
      fs.writeFileSync(folder+'/frame.png',png);
      fs.writeFileSync(folder+'/ocr.json',JSON.stringify(rows,null,2));
      const state={schema:1,runId,snapshot:number,proofSha256:sha256,reason,
        commandError:error||null,lastCommandSequence:report.controlledSession.lastCommandSequence,
        url:page.url(),identity:report.identity,actualEarnedInput:report.resumedEarnedCampaign,
        acceptedNativeEvents:report.telemetry.receipts.filter(r=>r.status===202),
        browserErrors:report.errors,physicalIphoneVerified:false,
        source:process.env.GITHUB_SHA,artOrReleaseApproval:false,at:new Date().toISOString()};
      fs.writeFileSync(folder+'/state.json',JSON.stringify(state,null,2));
      const upload=await artifact.uploadArtifact('eldoria-control-'+String(number).padStart(3,'0'),
        ['frame.png','ocr.json','state.json'].map(f=>path.join(folder,f)),folder,
        {retentionDays:14});
      if(!upload.id)throw Error('ORIGINAL_UI_SNAPSHOT_NOT_PRESERVED');
      lastProof={sha256,rows,number,artifactId:upload.id};
      report.controlledSession.snapshots.push({...lastProof,rows:undefined,reason});
      write();console.log(JSON.stringify({actualSnapshot:number,artifactId:upload.id,sha256,reason}));
    }
    async function readCommand(){
      const url='https://api.github.com/repos/'+process.env.GITHUB_REPOSITORY+
        '/contents/'+commandPath+'?ref='+encodeURIComponent(controllerBranch);
      const response=await fetch(url,{headers:{Authorization:'Bearer '+process.env.GH_TOKEN,
        Accept:'application/vnd.github+json','Cache-Control':'no-cache'}});
      if(!response.ok)throw Error('CONTROL_READ_HTTP_'+response.status);
      const result=await response.json();
      if(result.type!=='file'||result.encoding!=='base64'||result.size>12000)
        throw Error('UNEXPECTED_COMMAND_DOCUMENT');
      const bytes=Buffer.from(result.content.replace(/\s/g,''),'base64');
      if(crypto.createHash('sha1').update(Buffer.concat([
        Buffer.from('blob '+bytes.length+'\0'),bytes])).digest('hex')!==result.sha)
        throw Error('COMMAND_BLOB_IDENTITY_CHANGED');
      return JSON.parse(bytes.toString('utf8'));
    }
    async function applyObservedCommand(command){
      if(command.schema!==1||command.run_id!==runId||
         command.sequence!==report.controlledSession.lastCommandSequence+1||
         command.sequence>300||command.proof_sha256!==lastProof.sha256)
        throw Error('STALE_OR_UNPROVEN_CONTROL_COMMAND');
      const rows=await readUi('before-command-'+command.sequence);
      const allowed=new Set(['snapshot','click_text','click_point','wait','key','type_digits','finish']);
      if(!allowed.has(command.action))throw Error('UNKNOWN_UI_ACTION_REFUSED');
      if(command.action==='click_text'){
        const label=normalized(String(command.label||''));
        const matches=rows.filter(r=>r.confidence>=.35&&normalized(r.text)===label&&
          (command.min_y===undefined||r.y>=command.min_y)&&
          (command.max_y===undefined||r.y<=command.max_y));
        if(matches.length!==1)throw Error('ACTUAL_VISIBLE_LABEL_NOT_UNIQUE '+label);
        await page.mouse.click(matches[0].x,matches[0].y,{delay:150});
      }else if(command.action==='click_point'){
        const guards=command.guard_texts;
        if(!Array.isArray(guards)||guards.length<2||
           guards.some(t=>!rows.some(r=>r.confidence>=.35&&normalized(r.text)===normalized(t)))||
           !Number.isFinite(command.x)||!Number.isFinite(command.y)||
           command.x<0||command.x>390||command.y<0||command.y>844||
           typeof command.reason!=='string'||command.reason.length<10)
          throw Error('UNOBSERVED_POINT_REFUSED');
        await page.mouse.click(command.x,command.y,{delay:150});
      }else if(command.action==='wait'){
        if(!Number.isInteger(command.seconds)||command.seconds<1||command.seconds>45)
          throw Error('WAIT_OUT_OF_BOUNDS');
        await observeEntry(command.seconds,'actual-normal-wait-'+command.sequence);
      }else if(command.action==='key'){
        if(!['Enter','Backspace','ArrowLeft','ArrowRight','Home','End','Meta+A','Control+A'].includes(command.key))
          throw Error('UNSUPPORTED_KEY_REFUSED');
        await page.keyboard.press(command.key);
      }else if(command.action==='type_digits'){
        if(!/^[0-9]{1,3}$/.test(command.text||''))throw Error('ONLY_OBSERVED_QUANTITY_DIGITS_ALLOWED');
        await page.keyboard.type(command.text,{delay:100});
      }else if(command.action==='finish')finished=true;
      if(!['wait','snapshot','finish'].includes(command.action))
        await observeEntry(2,'after-observed-command-'+command.sequence);
      report.controlledSession.lastCommandSequence=command.sequence;
      report.controlledSession.actions.push({sequence:command.sequence,action:command.action,
        label:command.label,x:command.x,y:command.y,key:command.key,text:command.text,
        reason:command.reason,priorProof:command.proof_sha256,at:new Date().toISOString()});
      await publishSnapshot('after-command-'+command.sequence);
    }
    await publishSnapshot('actual-earned-session-ready');
    const stopAt=Date.now()+Math.min(globalDeadlineMs-180000,5100000);
    let transientReads=0;
    while(!finished&&!report.deadlineReached&&Date.now()<stopAt&&!report.errors.length){
      let command;
      try{command=await readCommand();transientReads=0;}
      catch(error){
        transientReads++;log({type:'control-read',error:String(error)});
        if(transientReads>=8)throw Error('CONTROL_CHANNEL_UNAVAILABLE_AFTER_BOUNDED_RETRIES');
      }
      if(command?.run_id===runId && command.sequence>report.controlledSession.lastCommandSequence){
        // Consume a rejected command once, publish a fresh original frame,
        // and retain the actual browser session for a corrected next command.
        try{await applyObservedCommand(command);}
        catch(error){
          if(Number.isInteger(command.sequence)&&command.sequence===report.controlledSession.lastCommandSequence+1)
            report.controlledSession.lastCommandSequence=command.sequence;
          await publishSnapshot('rejected-command-'+command.sequence,String(error));
        }
      }
      await observeEntry(8,'live-foreground-session');
    }
    report.controlledSession.finishedByObservedCommand=finished;
    report.controlledSession.progressionCertified=false;
    report.passed=finished&&!report.errors.length&&!report.deadlineReached;
  }catch(e){report.failure=String(e);}
  finally{
    // A rejected UI assertion must never discard an already earned campaign.
    // Export only this QA origin after its actual game document has closed,
    // using the same official API proven in43203180584.
    try{
      if(browser&&page&&!page.isClosed()){
        const expectedOrigin='https://mt5hjfz2kb-lab.github.io';
        if(new URL(page.url()).origin!==expectedOrigin)throw Error('EXPORT_ORIGIN_CHANGED');
        await page.evaluate(async()=>{if(window.valoriaGame)await window.valoriaGame.Quit();});
        await page.goto(new URL('./release-identity.json',page.url()).href,
          {waitUntil:'domcontentloaded',timeout:30000});
        const identity=JSON.parse(await page.locator('body').innerText());
        if(identity.source_sha!==report.identity.source_sha||identity.revision!==report.identity.revision)
          throw Error('EXPORT_ENGINE_IDENTITY_CHANGED');
        const before=await page.evaluate(async()=>await indexedDB.databases());
        if(!before.some(d=>d.name==='/idbfs')||before.some(d=>!['/idbfs','UnityCache','UnityPlayerPrefs'].includes(d.name)))
          throw Error('UNKNOWN_QA_DATABASE_REFUSED');
        await page.evaluate(()=>new Promise((resolve,reject)=>{
          const request=indexedDB.deleteDatabase('UnityCache');
          const timer=setTimeout(()=>reject(Error('DISPOSABLE_ASSET_CACHE_STILL_OPEN')),15000);
          request.onsuccess=()=>{clearTimeout(timer);resolve(true);};
          request.onerror=request.onblocked=()=>{clearTimeout(timer);reject(Error('QA_ASSET_CACHE_NOT_CLOSED'));};
        }));
        const state=await page.context().storageState({indexedDB:true});
        const bytes=JSON.stringify(state);
        if(Buffer.byteLength(bytes)>4000000||state.cookies.length||
           state.origins.some(o=>o.origin!==expectedOrigin)||
           state.origins.some(o=>o.indexedDB?.some(d=>!['/idbfs','UnityPlayerPrefs'].includes(d.name))))
          throw Error('UNEXPECTED_QA_CHECKPOINT_CONTENT');
        fs.writeFileSync(out+'/ordinary-qa-storage-state.json',bytes);
        report.ordinaryQaCheckpoint={exported:true,
          sourceEngine:report.identity.source_sha,sourceBundle:report.identity.candidate_bundle_sha256,
          bytes:Buffer.byteLength(bytes),sha256:require('crypto').createHash('sha256').update(bytes).digest('hex'),
          method:'OFFICIAL_PLAYWRIGHT_EARNED_STATE_AFTER_ACTUAL_QA_GAME_CLOSED',
          ownerSaveAccessed:false,fakeStateCreated:false,earnedSaveDatabaseModified:false};
      }
    }catch(error){report.checkpointFailure=String(error);report.passed=false;}
    await Promise.all(profileTasks);write();clearTimeout(watchdog);
    console.log(JSON.stringify({passed:report.passed,failure:report.failure,checkpoint:report.ordinaryQaCheckpoint}));
    if(browser)await browser.close().catch(()=>{});
  }
  if(!report.passed)process.exitCode=1;
})();
