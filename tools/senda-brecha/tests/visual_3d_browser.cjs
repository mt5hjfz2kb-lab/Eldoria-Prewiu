const {chromium} = require('playwright');
const fs=require('node:fs');
const path=require('node:path');
const assert=require('node:assert/strict');
(async()=>{
 const directory=process.env.SENDA_SCREENSHOTS||'senda-3d-evidence';
 fs.mkdirSync(directory,{recursive:true});
 const browser=await chromium.launch({headless:true,args:['--enable-webgl','--ignore-gpu-blocklist','--use-gl=angle','--use-angle=swiftshader','--enable-unsafe-swiftshader']});
 const checks=[],errors=[];
 for(const [name,width,height] of [['iphone',390,844],['small',320,700],['landscape',844,390],['desktop',1280,800]]){
  const page=await browser.newPage({viewport:{width,height},deviceScaleFactor:1,hasTouch:true});
  page.on('pageerror',e=>errors.push(name+':'+e.message));
  page.on('console',m=>{if(m.type()==='error')errors.push(name+':console:'+m.text())});
  await page.goto('http://127.0.0.1:8769/tools/senda-brecha/visual-3d/index.html',{waitUntil:'domcontentloaded',timeout:90000});
  await page.waitForFunction(()=>window.__senda3d&&window.__senda3d.state().hasWebGL,{timeout:90000});
  await page.waitForTimeout(1200);
  await page.screenshot({path:path.join(directory,name+'-intro.png')});
  await page.getByRole('button',{name:'COMENZAR EXPEDICIÓN'}).click();
  await page.waitForTimeout(900);
  const start=await page.evaluate(()=>window.__senda3d.state());
  assert.equal(start.started,true);assert.equal(start.soldiers,22);
  await page.locator('#left').click();assert.equal((await page.evaluate(()=>window.__senda3d.state())).lane,0);
  await page.locator('#right').click();assert.equal((await page.evaluate(()=>window.__senda3d.state())).lane,1);
  assert.equal(start.visibleActors,8,'Starting formation must be visually smaller than its 14-proxy cap');
  await page.locator('#left').click();
  await page.evaluate(()=>window.__senda3d.setPhase(0,4.05));
  await page.waitForTimeout(450);
  const recruited=await page.evaluate(()=>window.__senda3d.state());
  assert.equal(recruited.soldiers,32,'Left recruitment gate must add exactly ten virtual soldiers');
  assert.ok(recruited.visibleActors>start.visibleActors,'Actual THREE soldier objects must visibly increase');
  await page.screenshot({path:path.join(directory,name+'-recruits-visible.png')});
  await page.evaluate(()=>window.__senda3d.setPhase(0,2.8));await page.waitForTimeout(600);
  await page.screenshot({path:path.join(directory,name+'-gates.png')});
  await page.evaluate(()=>window.__senda3d.setPhase(2,7.2));await page.waitForTimeout(650);
  await page.screenshot({path:path.join(directory,name+'-battle.png')});
  const state=await page.evaluate(()=>window.__senda3d.state());
  checks.push({name,width,height,canvas:true,started:start.started,stage:state.stage,lane:state.lane,statsVisible:!!(await page.locator('#stats').isVisible())});
  await page.close();
 }
 // Record genuine WebGL frames, not AI artwork or offline mockup.
 const sequence=path.join(directory,'frames');fs.mkdirSync(sequence,{recursive:true});
 const videoPage=await browser.newPage({viewport:{width:390,height:844},deviceScaleFactor:1,hasTouch:true});
 videoPage.on('pageerror',e=>errors.push('clip:'+e.message));
 await videoPage.goto('http://127.0.0.1:8769/tools/senda-brecha/visual-3d/index.html',{waitUntil:'domcontentloaded',timeout:90000});
 await videoPage.waitForFunction(()=>window.__senda3d&&window.__senda3d.state().hasWebGL,{timeout:90000});
 await videoPage.getByRole('button',{name:'COMENZAR EXPEDICIÓN'}).click();
 await videoPage.evaluate(()=>window.__senda3d.setSpeed(4));
 // 80 software-rendered PNGs made the Chromium job exceed its practical GPU budget.
 // Four real viewports already test intro, gates, combat and added recruits;
 // the clip needs only a representative dozen frames, not 80 redundant renders.
 for(let frame=0;frame<12;frame++){
  if(frame===2) await videoPage.locator('#left').click();
  if(frame===9) await videoPage.locator('#right').click();
  await videoPage.waitForTimeout(50);
  await videoPage.screenshot({path:path.join(sequence,String(frame).padStart(3,'0')+'.png')});
 }
 await videoPage.close();
 await browser.close();
 const result={renderer:'THREE/WebGL',source:'isolated prototype',notUnity:true,notIphonePhysical:true,checks,errors};
 fs.writeFileSync(path.join(directory,'report.json'),JSON.stringify(result,null,2));
 assert.equal(errors.length,0,JSON.stringify(errors));
 console.log('SENDA_WEB3D_CHROMIUM_PASS '+JSON.stringify(checks));
})().catch(e=>{console.error(e);process.exitCode=1});