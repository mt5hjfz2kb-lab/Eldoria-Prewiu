'use strict';
// Independent real WebGL interaction diagnosis. A source-only check cannot certify selection.
const fs=require('node:fs');
const {chromium}=require('playwright');
const {renderedGameFrame}=require('../tools/eldoria-rendered-frame-readiness.cjs');
const url=process.env.ELDORIA_URL;
const dir=process.env.ELDORIA_EVIDENCE_DIR||'r2b-building-selection-qa';
if(!url||!/^https:\/\/mt5hjfz2kb-lab\.github\.io\/Eldoria-Prewiu\/r2-candidates\//.test(url))throw Error('Exact isolated R2-B URL required');
fs.mkdirSync(dir,{recursive:true});
const results={url,source_sha:process.env.ELDORIA_SOURCE_SHA,scope:'Real touch on logged projected canonical building target centres; no automated subjective UX signoff',cases:[]};
const pause=ms=>new Promise(r=>setTimeout(r,ms));
async function caseFor(browser,label,viewport,id){
 const context=await browser.newContext({viewport,hasTouch:true,isMobile:true,deviceScaleFactor:1});
 const page=await context.newPage();const logs=[];page.on('console',m=>logs.push(m.text()));page.on('pageerror',e=>logs.push('PAGEERROR '+String(e)));
 const report={label,id,viewport,pass:false};
 try{
  await page.goto(url+'?building-probe='+label,{waitUntil:'domcontentloaded',timeout:60000});
  const readyUntil=Date.now()+180000;
  while(Date.now()<readyUntil&&!logs.some(x=>/ELDORIA_PLAYABLE_STATE tag=scene-loaded scene=ValoriaWebGL/.test(x)))await pause(250);
  if(!logs.some(x=>/ELDORIA_PLAYABLE_STATE tag=scene-loaded scene=ValoriaWebGL/.test(x)))throw Error('NO_UNITY_CITY_STATE');
  const frameUntil=Date.now()+30000;
  while(Date.now()<frameUntil){const sample=renderedGameFrame(await page.screenshot());if(sample.ready){report.rendered_frame=sample;break;}await pause(300);}
  if(!report.rendered_frame)throw Error('NO_RENDERED_CITY_FRAME');
  const geometry=logs.filter(x=>x.includes('ELDORIA_PLAYABLE_HOTSPOT id='+id+' ')).at(-1);
  const m=geometry?.match(/x=([-\d.]+) y=([-\d.]+)/);if(!m)throw Error('NO_CANONICAL_HOTSPOT_GEOMETRY');
  const canvas=page.locator('canvas').first(),box=await canvas.boundingBox();const pixels=await canvas.evaluate(c=>({width:c.width,height:c.height}));
  report.target={x:+m[1],y:+m[2]};report.canvas={box,pixels};
  report.browser=await page.evaluate(()=>({innerWidth,innerHeight,devicePixelRatio,visualViewport:{width:visualViewport?.width,height:visualViewport?.height,scale:visualViewport?.scale},metaViewport:document.querySelector('meta[name="viewport"]')?.content||null,canvasStyle:document.querySelector('canvas')?.getAttribute('style'),containerStyle:document.querySelector('#unity-container')?.getAttribute('style')}));
  if(!box)throw Error('NO_CANVAS');
  const point={x:box.x+(report.target.x/pixels.width)*box.width,y:box.y+(report.target.y/pixels.height)*box.height};
  report.touch=point;
  report.on_screen=point.x>=box.x&&point.x<box.x+box.width&&point.y>=box.y&&point.y<box.y+box.height&&point.x>=0&&point.x<viewport.width&&point.y>=0&&point.y<viewport.height;
  if(!report.on_screen){report.finding='CANONICAL_BUILDING_TARGET_OFFSCREEN';return;}
  report.dom_target=await page.evaluate(({x,y})=>{const el=document.elementFromPoint(x,y);return {tag:el?.tagName,id:el?.id};},point);
  if(report.dom_target.id!=='unity-canvas'){report.finding='CANONICAL_BUILDING_TARGET_OCCLUDED';return;}
  await page.screenshot({path:dir+'/'+label+'-before.png'});
  const before=logs.length;
  const cdp=await context.newCDPSession(page);
  await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:point.x,y:point.y,radiusX:5,radiusY:5,force:1,id:1}]});
  await pause(90);await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
  await pause(1000);
  await page.screenshot({path:dir+'/'+label+'-after.png'});
  report.after_logs=logs.slice(before).filter(x=>/ELDORIA_PLAYABLE_(UI|HOTSPOT|NAV|RESET|TOUCH_SOURCE)|PAGEERROR/.test(x)).slice(-40);
  report.panel_open=report.after_logs.some(x=>/ELDORIA_PLAYABLE_UI id=buildingAction/.test(x));
  report.wrong_navigation=report.after_logs.some(x=>/ELDORIA_PLAYABLE_NAV|ELDORIA_PLAYABLE_RESET/.test(x));
  report.pass=report.panel_open&&!report.wrong_navigation;
  report.finding=report.pass?'BUILDING_SELECTION_OBSERVED':report.wrong_navigation?'WRONG_UI_ACTION':'BUILDING_SELECTION_NOT_OBSERVED';
 }catch(e){report.finding='PROBE_BLOCKED';report.error=String(e);await page.screenshot({path:dir+'/'+label+'-error.png'}).catch(()=>{});}finally{results.cases.push(report);await context.close();}
}
(async()=>{const browser=await chromium.launch({args:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});try{
 await caseFor(browser,'landscape-sawmill',{width:844,height:390},'sawmill');
 await caseFor(browser,'portrait-bastion',{width:390,height:844},'bastion');
}finally{await browser.close();}
results.pass=results.cases.every(c=>c.pass);fs.writeFileSync(dir+'/report.json',JSON.stringify(results,null,2));console.log('ELDORIA_R2B_BUILDING_SELECTION',JSON.stringify(results));if(!results.pass)process.exitCode=1;})().catch(e=>{console.error(e);process.exitCode=1;});
