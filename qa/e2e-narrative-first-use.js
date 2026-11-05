const {chromium}=require('playwright');
const assert=require('node:assert/strict');
const url=process.env.ELDORIA_URL||'http://127.0.0.1:4173/v0220/';
(async()=>{
 const browser=await chromium.launch({headless:true});
 const page=await browser.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 const problems=[],speakers=[];
 page.on('pageerror',e=>problems.push(String(e)));
 page.on('console',m=>{if(m.type()==='error')problems.push(m.text())});
 try{
  await page.goto(url,{waitUntil:'domcontentloaded'});
  await page.waitForFunction(()=>Boolean(window.ELDORIA_V023?.state),null,{timeout:15000});
  const get=()=>page.evaluate(()=>window.ELDORIA_V023.state());
  let stage=await get();assert.equal(stage.bastionLevel,1);
  let captures=0;
  for(let i=0;i<18;i++){
   const modal=page.locator('.aldric-cinematic:visible').first();
   if(!(await modal.count())){
    await page.waitForTimeout(450);
    stage=await get();
    if(stage.missionIntroSeen&&stage.tutorialSeen?.start)break;
    continue;
   }
   let who=await modal.locator('.aldric-name').first().innerText().catch(()=>'?');
   speakers.push(who);
   if(captures<4){
    await page.screenshot({path:'narrative-first-use-'+String(++captures).padStart(2,'0')+'.png',fullPage:true});
   }
   await page.waitForFunction(()=>Boolean(document.querySelector('.aldric-cinematic .aldric-dialogue.done')),null,{timeout:14000});
   await modal.locator('.aldric-continue').first().click({force:true});
   await page.waitForTimeout(240);
   stage=await get();
   if(stage.missionIntroSeen&&stage.tutorialSeen?.start)break;
  }
  stage=await get();
  console.log('NARRATIVE_PROBE',JSON.stringify({speakers,stage:{introSeen:stage.introSeen,missionIntroSeen:stage.missionIntroSeen,tutorialSeen:stage.tutorialSeen},problems}));
  assert.ok(stage.introSeen,'Aldric opening must be acknowledged');
  assert.ok(stage.missionIntroSeen,'Chapter panel must be taught after the real dialogue');
  assert.ok(stage.tutorialSeen?.start,'First construction action must be taught');
  assert.ok(speakers.length>=3,'Narrator, Aldric and first-use guidance should appear');
  assert.equal(problems.length,0,JSON.stringify(problems));
  console.log(JSON.stringify({result:'PASS',speakers,flags:{intro:stage.introSeen,missionIntro:stage.missionIntroSeen,firstAction:stage.tutorialSeen.start},captured:captures}));
 }finally{await browser.close();}
})().catch(e=>{console.error(e.stack||String(e));process.exitCode=1});
