'use strict';
// Real published Unity WebGL interaction diagnostic; never tests source code as if it were played.
// R2-B candidate is immutable: a failure is an input to owning D09/D10/D13, NOT an approval.
const fs=require('node:fs');
const {chromium}=require('playwright');
const output=process.env.ELDORIA_EVIDENCE_DIR||'eldoria-gameplay-camera-experience';
const site=process.env.ELDORIA_URL||'https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/r2-candidates/r2-b-6e0e1239/';
fs.mkdirSync(output,{recursive:true});
const results={schema_version:1,source_candidate_sha:process.env.ELDORIA_SOURCE_SHA||'6e0e12396a98b04333b595fff704afa3e849c30a',url:site,independent_review:'AUTOMATED_TOUCH_GEOMETRY_ONLY',scenarios:[],visual_experience_accepted:false,gameplay_defect_certified:false};
async function run(browser,label,viewport,axis,cssOverride=false){
 const context=await browser.newContext({viewport,hasTouch:true,isMobile:true,deviceScaleFactor:1,userAgent:'Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Mobile Safari/537.36'});
 const page=await context.newPage(); const logs=[];const item={label,viewport,axis,css_override_experiment:cssOverride,pass:false};
 page.on('console',m=>logs.push(m.text())); page.on('pageerror',e=>logs.push('PAGEERROR '+e));
 try {
  await page.goto(site+'?exp-probe='+label,{waitUntil:'domcontentloaded',timeout:60000});
  await page.waitForFunction(()=>{const bar=document.querySelector('#unity-loading-bar');return bar&&getComputedStyle(bar).display==='none';},null,{timeout:180000});
  // Unity's loading overlay can disappear before SlicePresenter announces its playable state.
  const readyUntil=Date.now()+120000;
  while(Date.now()<readyUntil&&!logs.some(x=>/ELDORIA_PLAYABLE_STATE tag=scene-loaded/.test(x))){await page.waitForTimeout(300);}
  if(!logs.some(x=>/ELDORIA_PLAYABLE_STATE tag=scene-loaded/.test(x)))throw Error('BLOCKED_UNITY_PLAYABLE_STATE_NOT_OBSERVED');
  if(cssOverride)await page.addStyleTag({content:'html,body,#unity-container,#unity-canvas,canvas{touch-action:none!important;overscroll-behavior:none!important}'});
  const canvas=page.locator('canvas').first(),rect=await canvas.boundingBox();if(!rect)throw Error('NO_UNITY_CANVAS');
  item.web_input_css=await canvas.evaluate(el=>({canvasTouchAction:getComputedStyle(el).touchAction,canvasPointerEvents:getComputedStyle(el).pointerEvents,canvasStyle:el.getAttribute('style'),parents:[el.parentElement,el.parentElement?.parentElement].filter(Boolean).map(x=>({tag:x.tagName,touchAction:getComputedStyle(x).touchAction,overflow:getComputedStyle(x).overflow})),bodyTouchAction:getComputedStyle(document.body).touchAction,bodyOverflow:getComputedStyle(document.body).overflow,postbuildMarkerPresent:!!document.getElementById('eldoria-webgl-touch-action-v1')}));
  if(process.env.ELDORIA_REQUIRE_TOUCH_FIX==='1' && (!item.web_input_css.postbuildMarkerPresent || item.web_input_css.canvasTouchAction!=='none'))throw Error('UNITY_WEBGL_TOUCH_ACTION_FIX_NOT_IN_BUILT_CANDIDATE');
  await page.screenshot({path:output+'/'+label+'-before.png'});
  await page.evaluate(()=>{
    window.__eldoriaInputTrace=[];
    for(const type of ['touchstart','touchmove','touchend','touchcancel','pointerdown','pointermove','pointerup']){
      window.addEventListener(type,e=>{
        const trace=window.__eldoriaInputTrace;
        if(trace.length<250)trace.push({type,target:e.target?.tagName||'',id:e.target?.id||'',defaultPrevented:e.defaultPrevented,points:e.touches?.length??null});
      },{capture:true,passive:true});
    }
  });
  const start={x:rect.x+rect.width*.52,y:rect.y+rect.height*.46};
  const finish=axis==='horizontal'?{x:start.x-85,y:start.y}:{x:start.x,y:start.y-85};
  const cdp=await context.newCDPSession(page);
  const dispatch=async (type,x,y)=>cdp.send('Input.dispatchTouchEvent',{type,touchPoints:type==='touchEnd'?[]:[{x,y}]});
  await dispatch('touchStart',start.x,start.y);
  // Let Unity InputSystem sample the initial pressed frame before streaming moves.
  await page.waitForTimeout(300);
  for(let i=1;i<=12;i++){const f=i/12;await dispatch('touchMove',start.x+(finish.x-start.x)*f,start.y+(finish.y-start.y)*f);await page.waitForTimeout(80);}
  await dispatch('touchEnd',finish.x,finish.y);
  await page.waitForTimeout(700);
  await page.screenshot({path:output+'/'+label+'-after.png'});
  item.dom_input_trace=await page.evaluate(()=>window.__eldoriaInputTrace||[]);
  item.dom_input_counts=Object.fromEntries(['touchstart','touchmove','touchend','touchcancel','pointerdown','pointermove','pointerup'].map(k=>[k,item.dom_input_trace.filter(x=>x.type===k).length]));
  const touchLogEnd=logs.length;
  // Independent diagnostic input channel: mouse events use a different WebGL path.
  // Its success must NEVER be counted as mobile touch success.
  await page.mouse.move(start.x,start.y);await page.mouse.down();await page.waitForTimeout(250);
  for(let i=1;i<=12;i++){const f=i/12;await page.mouse.move(start.x+(finish.x-start.x)*f,start.y+(finish.y-start.y)*f);await page.waitForTimeout(55);}
  await page.mouse.up();await page.waitForTimeout(400);
  item.mouse_pan_event_count=logs.slice(touchLogEnd).filter(x=>x.includes('ELDORIA_PLAYABLE_TOUCH_PAN')).length;
  const events=logs.slice(0,touchLogEnd).filter(x=>x.includes('ELDORIA_PLAYABLE_TOUCH_PAN'));
  const positions=events.map(x=>{const m=x.match(/x=([-0-9.]+) y=([-0-9.]+)/);return m?{x:+m[1],y:+m[2]}:null}).filter(Boolean);
  const delta=positions.length>1?Math.abs(positions.at(-1)[axis==='horizontal'?'x':'y']-positions[0][axis==='horizontal'?'x':'y']):0;
  item.pan_event_count=events.length;item.observed_axis_delta=delta;item.pass=events.length>0&&delta>=.015;
  item.console_count=logs.length;item.playable_state_count=logs.filter(x=>x.includes('ELDORIA_PLAYABLE_STATE')).length;
  item.finding=item.pass?'GESTURE_REACTED':(item.playable_state_count===0?'PROBE_INCONCLUSIVE_NO_GAME_STATE':'GESTURE_NOT_OBSERVED_NOT_YET_GAME_DEFECT');
  item.logs=logs.filter(x=>/ELDORIA_PLAYABLE|PAGEERROR|FAILED_REQUEST/.test(x)).slice(-90);
  item.console_sample=logs.slice(0,15);
 }catch(err){item.finding='BLOCKED_PROBE';item.error=String(err);await page.screenshot({path:output+'/'+label+'-error.png'}).catch(()=>{});}finally{results.scenarios.push(item);await context.close();}
}
(async()=>{const browser=await chromium.launch({args:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});try{await run(browser,'landscape-horizontal',{width:844,height:390},'horizontal');await run(browser,'portrait-vertical',{width:390,height:844},'vertical');await run(browser,'landscape-css-override-touch',{width:844,height:390},'horizontal',true);}finally{await browser.close();}results.all_gestures_observed=results.scenarios.filter(x=>!x.css_override_experiment).every(x=>x.pass);results.css_experiment_restored=results.scenarios.find(x=>x.css_override_experiment)?.pass===true;fs.writeFileSync(output+'/report.json',JSON.stringify(results,null,2));console.log('ELDORIA_REAL_TOUCH_EXPERIENCE',JSON.stringify(results));if(!results.all_gestures_observed)process.exitCode=1;})().catch(e=>{console.error(e);process.exitCode=1;});
