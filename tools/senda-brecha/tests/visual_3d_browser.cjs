const {chromium} = require('playwright');
const fs=require('node:fs');
const path=require('node:path');
const assert=require('node:assert/strict');
(async()=>{
 const directory=process.env.SENDA_SCREENSHOTS||'senda-3d-evidence';
 fs.mkdirSync(directory,{recursive:true});
 const browser=await chromium.launch({headless:true,args:['--enable-webgl','--ignore-gpu-blocklist','--use-gl=angle','--use-angle=swiftshader','--enable-unsafe-swiftshader']});
 try {
 const checks=[],errors=[];
 for(const [name,width,height] of [['iphone',390,844],['small',320,700],['landscape',844,390],['desktop',1280,800]]){
  const page=await browser.newPage({viewport:{width,height},deviceScaleFactor:1,hasTouch:true});
  page.on('pageerror',e=>errors.push(name+':'+e.message));
  page.on('console',m=>{if(m.type()==='error')errors.push(name+':console:'+m.text())});
  await page.goto('http://127.0.0.1:8769/tools/senda-brecha/visual-3d/index.html?test',{waitUntil:'domcontentloaded',timeout:90000});
  await page.waitForFunction(()=>window.__senda3d&&window.__senda3d.state().canonicalReady,undefined,{timeout:90000});
  await page.waitForTimeout(800);
  assert.equal(await page.locator('#crest').evaluate(i=>i.complete&&i.naturalWidth===64),true,'Generated HUD crest must decode');
  assert.ok(await page.evaluate(()=>{let x=__senda3d.state().gateScreenX;return x[0]<x[1]&&x[1]<x[2]}),'Physical gates ordered left-to-right');
  await page.screenshot({path:path.join(directory,name+'-intro.png')});
  await page.getByRole('button',{name:'COMENZAR EXPEDICIÓN'}).click();
  await page.waitForTimeout(900);
  const start=await page.evaluate(()=>window.__senda3d.state());
  assert.equal(start.canonicalPieces,3);assert.equal(start.canonicalSources.length,10);assert.equal(start.canonicalTerrain,2);assert.ok(start.canonicalFoliage>25);assert.equal(start.enemyCanonical,true);assert.equal(start.authoredGuards,22);assert.equal(start.environmentPolish,true);assert.ok(start.guardTriangles>1000&&start.guardTriangles<=12000,'Derived armor geometry LOD budget');assert.ok(start.heroSkins>0);assert.ok(start.heroAnimated&&start.heroWalkTime>0,'Actual approved Aldric skin and walk clip must render');
  assert.equal(start.started,true);assert.equal(start.soldiers,22);
  await page.locator('#left').click();assert.equal((await page.evaluate(()=>window.__senda3d.state())).lane,0);
  await page.evaluate(()=>window.__senda3d.advance(.6));
  assert.ok((await page.evaluate(()=>window.__senda3d.state())).screenLaneX<0,'Left control must move visibly to screen left');
  await page.locator('#right').click();assert.equal((await page.evaluate(()=>window.__senda3d.state())).lane,1);
  assert.equal(start.visibleActors,8,'Starting formation must be visually smaller than its 14-proxy cap');
  await page.locator('#left').click();
  await page.evaluate(()=>{window.__senda3d.setSpeed(0);window.__senda3d.setPhase(0,4.05);window.__senda3d.advance(.20)});
  const recruited=await page.evaluate(()=>window.__senda3d.state());
  assert.equal(recruited.soldiers,32,'Left recruitment gate must add exactly ten virtual soldiers');
  assert.ok(recruited.visibleActors>start.visibleActors,'Actual THREE soldier objects must visibly increase');
  await page.screenshot({path:path.join(directory,name+'-recruits-visible.png')});
  await page.evaluate(()=>{window.__senda3d.setPhase(0,2.8);window.__senda3d.advance(.15)});
  await page.screenshot({path:path.join(directory,name+'-gates.png')});
  await page.evaluate(()=>{window.__senda3d.setPhase(2,7.2);window.__senda3d.advance(.35)});
  await page.screenshot({path:path.join(directory,name+'-battle.png')});
  const state=await page.evaluate(()=>window.__senda3d.state());
  assert.deepEqual(state.gateLabels,[['Power',45],['Recruit',13],['Trap',9]],'Gate label must match selected stage');
  assert.ok(state.batches>0,'Repeated geometry must be batched');assert.ok(state.drawCalls<350,'Mobile rendering budget exceeded: '+state.drawCalls);
  await page.evaluate(()=>window.__senda3d.start());
  assert.deepEqual((await page.evaluate(()=>window.__senda3d.state())).gateLabels,[['Recruit',10],['Power',35],['Trap',5]],'Replay resets physical gate labels');
  checks.push({name,width,height,canvas:true,started:start.started,stage:state.stage,lane:state.lane,statsVisible:!!(await page.locator('#stats').isVisible()),drawCalls:state.drawCalls,triangles:state.triangles,batches:state.batches,canonicalPieces:state.canonicalPieces,guardTriangles:state.guardTriangles,authoredGuards:state.authoredGuards,canonicalTerrain:state.canonicalTerrain,canonicalFoliage:state.canonicalFoliage,enemyCanonical:state.enemyCanonical,heroSkins:state.heroSkins,heroAnimated:state.heroAnimated,canonicalSources:state.canonicalSources,screenLaneDirection:true,gateReset:true});
  await page.close();
 }
 // Run complete deterministic live browser routes through the real update loop,
 // not the setPhase debug shortcut. Use actual lane buttons and check pause/replay.
 const routePage=await browser.newPage({viewport:{width:390,height:844},deviceScaleFactor:1,hasTouch:true});
 routePage.on('pageerror',e=>errors.push('routes:'+e.message));
 await routePage.goto('http://127.0.0.1:8769/tools/senda-brecha/visual-3d/index.html?test',{waitUntil:'domcontentloaded',timeout:30000});
 await routePage.waitForFunction(()=>window.__senda3d&&window.__senda3d.state().canonicalReady,undefined,{timeout:90000});
 await routePage.getByRole('button',{name:'COMENZAR EXPEDICIÓN'}).click();
 await routePage.locator('#pause').click();
 const beforePause=await routePage.evaluate(()=>__senda3d.state().stepTime);
 await routePage.waitForTimeout(250);
 assert.equal(await routePage.evaluate(()=>__senda3d.state().stepTime),beforePause,'Pause must freeze time');
 await routePage.locator('#pause').click();
 await routePage.evaluate(()=>__senda3d.setSpeed(0));
 const playRoute=async (route)=>{
   await routePage.evaluate(()=>__senda3d.start());
   for(const target of route){
     let state=await routePage.evaluate(()=>__senda3d.state());if(state.finished)break;
     for(let i=state.lane;i>target;i--)await routePage.locator('#left').click();
     for(let i=state.lane;i<target;i++)await routePage.locator('#right').click();
     await routePage.evaluate(()=>__senda3d.advance(11.05));
   }
   return await routePage.evaluate(()=>__senda3d.state());
 };
 const victory=await playRoute([1,0,1,0,1,0]);
 assert.equal(victory.finished,true);assert.ok(victory.soldiers>0);assert.equal(victory.stage,5);
 assert.match(await routePage.locator('#endTitle').textContent(),/BRECHA SELLADA/);
 await routePage.screenshot({path:path.join(directory,'iphone-victory.png')});
 const defeat=await playRoute([0,0,0,0,0,0]);
 assert.equal(defeat.finished,true);assert.equal(defeat.soldiers,0);assert.equal(defeat.visibleActors,0);
 assert.match(await routePage.locator('#endTitle').textContent(),/CAÍDO/);
 await routePage.screenshot({path:path.join(directory,'iphone-defeat.png')});
 await routePage.locator('#again').click();
 const replay=await routePage.evaluate(()=>__senda3d.state());
 assert.equal(replay.soldiers,22);assert.equal(replay.stage,0);assert.equal(replay.lane,1);
 assert.deepEqual(replay.gateLabels,[['Recruit',10],['Power',35],['Trap',5]]);
 await routePage.close();
 // Record genuine WebGL frames, not AI artwork or offline mockup.
 const sequence=path.join(directory,'frames');fs.mkdirSync(sequence,{recursive:true});
 const videoPage=await browser.newPage({viewport:{width:390,height:844},deviceScaleFactor:1,hasTouch:true});
 videoPage.on('pageerror',e=>errors.push('clip:'+e.message));
 await videoPage.goto('http://127.0.0.1:8769/tools/senda-brecha/visual-3d/index.html?test',{waitUntil:'domcontentloaded',timeout:90000});
 await videoPage.waitForFunction(()=>window.__senda3d&&window.__senda3d.state().canonicalReady,undefined,{timeout:90000});
 await videoPage.getByRole('button',{name:'COMENZAR EXPEDICIÓN'}).click();
 await videoPage.evaluate(()=>window.__senda3d.setSpeed(0));
 // 80 software-rendered PNGs made the Chromium job exceed its practical GPU budget.
 // Four real viewports already test intro, gates, combat and added recruits;
 // the clip needs only a representative dozen frames, not 80 redundant renders.
 for(let frame=0;frame<12;frame++){
  if(frame===2) await videoPage.locator('#left').click();
  if(frame===9) await videoPage.locator('#right').click();
  await videoPage.evaluate(()=>window.__senda3d.advance(.32));
  await videoPage.screenshot({path:path.join(sequence,String(frame).padStart(3,'0')+'.png')});
 }
 await videoPage.close();
 const result={renderer:'THREE/WebGL',source:'isolated prototype',notUnity:true,notIphonePhysical:true,checks,errors,victoryRouteChecked:true,defeatRouteChecked:true,pauseAndReplayChecked:true};
 fs.writeFileSync(path.join(directory,'report.json'),JSON.stringify(result,null,2));
 assert.equal(errors.length,0,JSON.stringify(errors));
 console.log('SENDA_WEB3D_CHROMIUM_PASS '+JSON.stringify(checks));
 } finally {await browser.close()}
})().catch(e=>{console.error(e);process.exit(1)});