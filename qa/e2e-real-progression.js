// Real touch/DOM journey for the approved Bastion III sequence:
// twenty-minute investigation -> contact Lyra about her lost brother ->
// assault together -> recruit AFTER victory -> unique manuscript in chest.
// This replaces the retired pre-III shortcut in which inspection recruited her.
const {chromium}=require('playwright');
(async()=>{
 const browser=await chromium.launch({headless:true});
 const page=await browser.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await page.goto(process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1',{waitUntil:'domcontentloaded'});
 await page.evaluate(()=>{
  localStorage.setItem('eldoria-aldric-story-r7-v3','1');
  document.querySelectorAll('.aldric-cinematic,.e22-overlay').forEach(x=>x.remove());
 });
 await page.waitForFunction(()=>!!window.ELDORIA_V023);
 const state=()=>page.evaluate(()=>window.ELDORIA_V023.state());
 const set=value=>page.evaluate(x=>window.ELDORIA_V023.setQA(x),value);
 const boss=page.locator('[data-testid="world-node-boss"]');
 const action=page.locator('[data-testid="world-action-boss"]');
 const select=async()=>{await boss.waitFor({state:'attached'});await boss.evaluate(el=>el.click());await action.waitFor({state:'visible',timeout:5000});};
 let s=await state();
 await set({...s,introSeen:true,view:'world',sawmill:true,bastion:2,bastionLevel:3,bastion3:true,
  barracks:true,granary:true,forest:1,quarry:1,camp:1,lyra:false,lyraContacted:false,
  fissureExplored:false,boss:false,tasks:[],breachManuscript:false,inventory:[],
  wood:9999,stone:9999,food:9999});
 await select();
 if(!(await action.innerText()).includes('EXPLORAR'))throw Error('Fissure must offer research first');
 await action.evaluate(el=>el.click());
 // The authentic world action animates Aldric's route before dispatching the
 // research task. Do not inspect state in the same click frame.
 await page.waitForFunction(()=>window.ELDORIA_V023.state().tasks.some(t=>t.key==='explore-fissure'),null,{timeout:12000});
 s=await state();
 const research=(s.tasks||[]).filter(t=>t.key==='explore-fissure');
 if(research.length!==1||Math.abs((research[0].end-research[0].start)-1200000)>1000)
  throw Error('Investigation did not create one persistent 20-minute task');
 if(s.lyra||s.lyraContacted||s.boss||s.breachManuscript)
  throw Error('Investigation must not preemptively recruit Lyra or award manuscript');
 // Advance the fixture to the moment the exact 20-minute task completes;
 // 20-minute/offline/idempotence mechanics are separately proven by domain QA.
 await set({...s,tasks:(s.tasks||[]).filter(t=>t.key!=='explore-fissure'),fissureExplored:true,view:'world'});
 await select();
 if(!(await action.innerText()).includes('HABLAR CON LYRA'))throw Error('Research must unlock CONTACT, not an immediate boss battle');
 await action.evaluate(el=>el.click());
 const advance=async speaker=>{
  const dialogue=page.locator('.aldric-cinematic[data-scene="story-dialogue"][data-speaker="'+speaker+'"]');
  await dialogue.waitFor({state:'visible',timeout:7000});
  const next=dialogue.locator('.aldric-continue');
  await next.evaluate(button=>button.click());await page.waitForTimeout(50);
  if(await next.count())await next.evaluate(button=>button.click());
 };
 for(const speaker of ['NARRADOR','Sir Aldric','Lyra','Sir Aldric','Lyra','Sir Aldric'])await advance(speaker);
 await page.waitForFunction(()=>window.ELDORIA_V023.state().lyraContacted===true,null,{timeout:4000});
 s=await state();
 if(s.lyra||s.boss||s.breachManuscript)throw Error('Contact cannot recruit Lyra or grant victory loot');
 await select();
 if(!(await action.innerText()).includes('ASALTAR CON LYRA'))throw Error('Real post-contact assault missing');
 await action.evaluate(el=>el.click());
 const cinematic=page.locator('.e22-cinema .e22-dialog');
 for(let i=0;i<45;i++){
  if(await cinematic.count())break;
  await page.waitForTimeout(180);
  const next=page.locator('.aldric-cinematic:visible .aldric-continue');
  if(await next.count()){
   await next.last().evaluate(el=>el.click());await page.waitForTimeout(30);
   if(await next.count())await next.last().evaluate(el=>el.click());
  }
 }
 await cinematic.waitFor({state:'visible',timeout:7000});
 let label=(await cinematic.innerText()).toUpperCase();
 if(!label.includes('NUEVO HÉROE')||!label.includes('LYRA'))
  throw Error('Post-assault hero ceremony missing: '+label);
 await cinematic.locator('button').click();
 await cinematic.waitFor({state:'visible',timeout:7000});
 label=(await cinematic.innerText()).toUpperCase();
 if(!label.includes('MANUSCRITO DE LA FISURA')||!label.includes('MISTERIO'))
  throw Error('Unique manuscript ceremony missing: '+label);
 s=await state();
 if(!s.lyra||!s.boss||!s.breachManuscript||!(s.inventory||[]).some(x=>x&&x.id==='breach-manuscript'))
  throw Error('Victory missing Lyra, fissure closure or manuscript');
 if((s.inventory||[]).filter(x=>x&&x.id==='breach-manuscript').length!==1)
  throw Error('Manuscript awarded more than once');
 if(!s.heroUnlockNotice)throw Error('Lyra did not highlight the Heroes section');
 const heroes=page.locator('[data-testid="nav-heroes"]');
 if(!(await heroes.evaluate(el=>el.classList.contains('newUnlock0265'))))
  throw Error('Heroes navigation is not visibly marked after recruitment');
 await browser.close();
 console.log('REAL BASTION III FISSURE LYRA PROGRESSION PASS');
})().catch(e=>{console.error(e);process.exit(1)});
