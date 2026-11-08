// Diagnostic only: unchanged public mobile5, genuine desktop browsers, no mobile certification.
const fs = require('fs');
const {execFileSync} = require('child_process');
const {webkit, chromium} = require('playwright');
const engine = process.env.QA_ENGINE || 'webkit';
const variant = process.env.QA_VARIANT || 'unchanged';
const out = process.env.QA_OUT || 'webkit-residency';
fs.mkdirSync(out, {recursive:true});
const report = {engine, variant, platform:process.platform, arch:process.arch, passed:false,
  scope:'Published mobile5; desktop engine comparison, NOT physical iPhone', samples:[], errors:[], navigations:[]};
const write = () => fs.writeFileSync(out+'/report.json', JSON.stringify(report,null,2));
const log = event => fs.appendFileSync(out+'/events.jsonl', JSON.stringify({at:new Date().toISOString(),...event})+'\n');
function processes() {
  try {return execFileSync('ps',['-axo','pid=,ppid=,rss=,comm='],{encoding:'utf8',timeout:3000})
    .split('\n').filter(s=>/WebKit|MiniBrowser|Chromium|Google Chrome|chrome-headless/.test(s))
    .map(s=>{const m=s.trim().match(/^(\d+)\s+(\d+)\s+(\d+)\s+(.+)$/);return m?{pid:+m[1],ppid:+m[2],rssKiB:+m[3],name:m[4]}:null;}).filter(Boolean);
  } catch(e) {return [{error:String(e)}];}
}
(async()=>{
  let browser, page;
  const watchdog=setTimeout(()=>{report.errors.push('GLOBAL_DEADLINE');write();process.exit(1);},420000);
  try {
    if(!['webkit','chromium'].includes(engine))throw Error('Unknown engine');
    browser=await ({webkit,chromium}[engine]).launch({headless:true});
    report.version=browser.version();
    const context=await browser.newContext({viewport:{width:390,height:844},deviceScaleFactor:3});
    await context.addInitScript(({variant})=>{
      window.__growth=[];
      window.__glCalls=[];
      const remember=(name,args)=>{
        const a=Array.from(args).map(v=>typeof v==='number'||typeof v==='boolean'||typeof v==='string'?v:(v?.byteLength!==undefined?{byteLength:v.byteLength}:String(v)));
        window.__glCalls.push({at:performance.now(),name,args:a});
        if(window.__glCalls.length>50)window.__glCalls.shift();
      };
      const proto=WebGL2RenderingContext.prototype;
      if(variant==='astc-disabled'){
        const ext=proto.getExtension;
        proto.getExtension=function(name){return /compressed_texture_astc/i.test(name)?null:ext.call(this,name);};
        const supported=proto.getSupportedExtensions;
        proto.getSupportedExtensions=function(){return supported.call(this)?.filter(x=>!/compressed_texture_astc/i.test(x));};
      }
      for(const name of ['compressedTexImage2D','texImage2D','texStorage2D','bufferData','compileShader','linkProgram','drawElements','drawArrays']){
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
    },{variant});
    page=await context.newPage();
    page.on('console',m=>log({type:'console',level:m.type(),text:m.text()}));
    page.on('pageerror',e=>{report.errors.push(String(e));log({type:'pageerror',text:String(e)});write();});
    page.on('crash',()=>{report.errors.push('PAGE_CRASH');log({type:'crash'});write();});
    page.on('requestfailed',r=>log({type:'requestfailed',url:r.url(),error:r.failure()}));
    page.on('framenavigated',f=>{if(f===page.mainFrame()){report.navigations.push({at:Date.now(),url:f.url()});write();}});
    await page.goto('https://eldoria-valoria-beta-20261108.kbkvc5c45n.chatgpt.site/?eldoria_diag=1&build=mobile5',{waitUntil:'domcontentloaded',timeout:30000});
    report.identity=await page.evaluate(()=>fetch('release-identity.json',{cache:'no-store'}).then(r=>r.json()));
    if(report.identity.revision!=='native325-mobile5')throw Error('Public candidate changed');
    report.capabilities=process.env.QA_SKIP_PREFLIGHT==='1'?{webgl2:'not-probed',note:'Observe actual Unity context only'}:await page.evaluate(()=>{
      const c=document.createElement('canvas'),g=c.getContext('webgl2');
      if(!g)return {webgl2:false,userAgent:navigator.userAgent};
      const d=g.getExtension('WEBGL_debug_renderer_info');
      const result={webgl2:true,userAgent:navigator.userAgent,astc:!!g.getExtension('WEBGL_compressed_texture_astc'),renderer:g.getParameter(d?d.UNMASKED_RENDERER_WEBGL:g.RENDERER)};
      g.getExtension('WEBGL_lose_context')?.loseContext();return result;
    });
    write();
    if(!report.capabilities.webgl2)throw Error('ENVIRONMENT_NO_WEBGL2');
    await page.locator('#play').click();
    let readyAt=0;
    const started=Date.now();
    while(Date.now()-started<330000){
      if(report.errors.length)throw Error('Browser failure');
      const s=await page.evaluate(()=>{
        const game=window.valoriaGame;
        let metrics=null;try{metrics=game?.GetMetricsInfo?.();}catch(e){metrics={error:String(e)};}
        return {at:performance.now(),ready:!!game,metrics,growth:window.__growth,
          diagnostic:window.EldoriaLocalDiagnostic?.read?.(),
          visible:!document.getElementById('game').hidden,
          error:document.getElementById('error')?.textContent,
          wasmCapacityBytes:game?.Module?.HEAPU8?.byteLength||0};
      });
      s.processes=processes();report.samples.push(s);write();
      if(s.ready&&!readyAt){readyAt=Date.now();log({type:'unity-ready'});}
      if(readyAt&&(!s.ready||!s.visible))throw Error('Unity exited after ready');
      if(readyAt&&Date.now()-readyAt>90000)break;
      await page.waitForTimeout(1000);
    }
    if(!readyAt||Date.now()-readyAt<90000)throw Error('Did not survive 90s after ready');
    const events=fs.readFileSync(out+'/events.jsonl','utf8');
    if(!events.includes('REGION1_CANON_WORLD_INSTALLED'))throw Error('World initialization incomplete');
    if(/not readable|Could not allocate|out of memory|RuntimeError:|Exception:/i.test(events))throw Error('Unity runtime errors');
    await page.screenshot({path:out+'/game.png',timeout:15000});
    report.passed=true;
  }catch(e){report.failure=String(e);}
  finally{
    write();clearTimeout(watchdog);
    const closeTimer=setTimeout(()=>process.exit(report.passed?0:1),10000);
    if(browser)await browser.close().catch(()=>{});
    clearTimeout(closeTimer);
  }
  if(!report.passed)process.exitCode=1;
})();
