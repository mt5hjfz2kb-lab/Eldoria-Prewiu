const { chromium } = require('playwright');
const assert = require('assert');
(async()=>{
 const browser=await chromium.launch({headless:true});
 const page=await browser.newPage({viewport:{width:1280,height:800}});
 const errors=[]; page.on('pageerror',e=>errors.push(e.message)); page.on('console',m=>{if(m.type()==='error')errors.push(m.text())});
 await page.goto('http://127.0.0.1:4173/?qa=1',{waitUntil:'load'});
 await page.waitForFunction(()=>window.ELDORIA_V022_QA);
 await page.evaluate(()=>window.ELDORIA_V022_QA.setState({introSeen:true,view:'kingdom'}));
 await page.waitForSelector('.e22Hud',{state:'visible'});
 await page.waitForSelector('.keep',{state:'visible'});
 assert.equal(await page.locator('body.v022-core-active > #app > .e22').count(),1);
 assert.equal(await page.locator('#game:visible').count(),0);
 await page.click('[data-poi="sawmill"]');
 await page.waitForSelector('[data-context-confirm]:visible');
 assert.match(await page.locator('[data-context-action]').innerText(),/80 madera/i);
 await page.click('[data-context-confirm]');
 await page.waitForTimeout(1200); const afterBuild=await page.evaluate(()=>window.ELDORIA_V022_QA.getState().state); console.log('AFTER_BUILD',JSON.stringify({sawmill:afterBuild.sawmill,view:afterBuild.view,tasks:afterBuild.tasks,wood:afterBuild.wood})); assert.equal(afterBuild.sawmill,true,'sawmill construction did not complete');
 assert.equal((await page.evaluate(()=>window.ELDORIA_V022_QA.getState().state.view)),'world');
 let before=await page.evaluate(()=>window.ELDORIA_V022_QA.getState().state.wood);
 assert.match(await page.locator('.mapHint').innerText(),/arrastra|mover/i); await page.locator('[data-node="forest"]').evaluate(el=>el.click()); await page.waitForSelector('[data-node-confirm]');
 let mid=await page.evaluate(()=>window.ELDORIA_V022_QA.getState().state.wood); assert.equal(mid,before);
 await page.locator('[data-node-confirm]').evaluate(el=>el.click());
 await page.waitForFunction(w=>window.ELDORIA_V022_QA.getState().state.wood>w,before);
 await page.locator('[data-node="valoria"]').evaluate(el=>el.click()); await page.waitForSelector('.kingdom .keep:visible');
 const stages=[
  {p:{bastionLevel:2,bastion:2,sawmill:true,sawmillLvl:2,camp:1,lyra:false,view:'world'},q:/Lyra/i},
  {p:{bastionLevel:3,bastion:3,bastion3:true,sawmill:true,sawmillLvl:3,camp:1,lyra:true,boss:1,granary:false,view:'kingdom'},q:/Granero/i},
  {p:{bastionLevel:5,bastion:5,bastion3:true,sawmill:true,sawmillLvl:5,camp:1,lyra:true,boss:1,granary:true,granaryLvl:5,graniteQuarry:false,view:'kingdom'},q:/Cantera/i},
  {p:{bastionLevel:6,bastion:6,bastion3:true,sawmill:true,sawmillLvl:6,camp:1,lyra:true,boss:1,granary:true,granaryLvl:6,graniteQuarry:true,quarryLvl:6,forge:false,view:'world'},q:/hierro|Forja/i},
  {p:{bastionLevel:7,bastion:7,bastion3:true,sawmill:true,sawmillLvl:7,camp:1,lyra:true,boss:1,granary:true,granaryLvl:7,graniteQuarry:true,quarryLvl:7,forge:true,forgeLvl:7,barracks:true,barracksLvl:7,troops:17,aetherHuntDone:false,view:'world'},q:/Devorador/i},
  {p:{bastionLevel:8,bastion:8,bastion3:true,sawmill:true,sawmillLvl:8,camp:1,lyra:true,boss:1,granary:true,granaryLvl:8,graniteQuarry:true,quarryLvl:8,forge:true,forgeLvl:8,barracks:true,barracksLvl:8,troops:20,aetherHuntDone:true,aetherUnlocked:true,aetherShard:2,codexUnlocked:true,relicDecision:true,equipment:1,gearInventory:[{uid:'qa1',type:'weapon',name:'Hoja QA',rarity:'RARA',atk:8,def:0,hp:0,power:120,temper:0}],heroEquipment:{aldric:{weapon:'qa1'},lyra:{},maelis:{}},maelis:false,view:'world'},q:/Maelis|Nareth/i},
  {p:{bastionLevel:9,bastion:9,bastion3:true,sawmill:true,camp:1,lyra:true,boss:1,granary:true,graniteQuarry:true,forge:true,barracks:true,troops:23,aetherHuntDone:true,codexUnlocked:true,relicDecision:true,equipment:1,maelis:true,marchSlots:['aldric','lyra'],marchConfigured:true,trial9Done:false,view:'heroes'},q:/Prueba/i},
  {p:{bastionLevel:10,bastion:10,bastion3:true,sawmill:true,camp:1,lyra:true,boss:1,granary:true,graniteQuarry:true,forge:true,barracks:true,troops:23,aetherHuntDone:true,codexUnlocked:true,relicDecision:true,equipment:1,maelis:true,marchSlots:['aldric','lyra'],marchConfigured:true,trial9Done:true,finalVictory:false,view:'world'},q:/Asalto final/i}
 ];
 for(const [idx,st] of stages.entries()){
   await page.evaluate(p=>window.ELDORIA_V022_QA.setState(p),st.p);
   await page.waitForSelector('.scene:visible');
   const txt=await page.locator('.e22Quest').innerText(); assert.match(txt,st.q,'stage '+(idx+1)+': '+txt);
   const box=await page.locator('.scene').boundingBox(); assert(box&&box.width>600&&box.height>400,'scene collapsed at stage '+(idx+1));
 }
 await page.evaluate(()=>window.ELDORIA_V022_QA.setState({view:'heroes'})); await page.waitForSelector('[data-testid="hero-hall"]:visible');
 // Exercise Bastion IX composition trial with a deliberately strong QA company.
 await page.evaluate(()=>window.ELDORIA_V022_QA.setState({bastionLevel:9,bastion:9,view:'world',introSeen:true,sawmill:true,barracks:true,granary:true,graniteQuarry:true,forge:true,lyra:true,maelis:true,troops:100,aldricLvl:40,lyraLvl:40,marchSlots:['aldric','lyra'],marchConfigured:true,trial9Done:false,heroEquipment:{aldric:{},lyra:{},maelis:{}}}));
 await page.waitForSelector('[data-node="trial9"]:visible');
 await page.locator('[data-node="trial9"]').evaluate(el=>el.click());
 await page.waitForSelector('[data-testid="cinematic"]:visible');
 await page.locator('[data-testid="cinematic"] button').evaluate(el=>el.click()); await page.waitForSelector('[data-testid="march-trial"]:visible');
 for(let i=0;i<40 && !(await page.evaluate(()=>window.ELDORIA_V022_QA.getState().state.trial9Done));i++){
   await page.evaluate(()=>document.querySelector('body > .e22-overlay[data-testid="march-trial"] [data-testid="trial-guard"]')?.click());
   await page.waitForTimeout(100);
   await page.evaluate(()=>document.querySelector('body > .e22-overlay[data-testid="march-trial"] [data-testid="trial-action"]:not([disabled])')?.click());
   await page.waitForTimeout(140);
 }
 assert.equal(await page.evaluate(()=>window.ELDORIA_V022_QA.getState().state.trial9Done),true,'Bastion IX trial could not be completed through player controls');
 // Close trial victory cinematic.
 if(await page.locator('[data-testid="cinematic"]:visible').count())await page.locator('[data-testid="cinematic"] button').evaluate(el=>el.click());
 // Exercise the real two-phase final encounter through visible controls.
 await page.evaluate(()=>window.ELDORIA_V022_QA.setState({bastionLevel:10,bastion:10,view:'world',troops:120,aldricLvl:24,lyraLvl:24,trial9Done:true,finalVictory:false,marchSlots:['aldric','lyra'],marchConfigured:true}));
 await page.waitForSelector('[data-node="final"]:visible'); await page.locator('[data-node="final"]').evaluate(el=>el.click());
 await page.waitForSelector('.aldric-cinematic:visible'); await page.locator('.aldric-cinematic').click({position:{x:5,y:5}});await page.waitForTimeout(30);await page.locator('.aldric-cinematic button').click();
 await page.waitForSelector('[data-testid="final-combat"]:visible');
 for(let i=0;i<120 && !(await page.evaluate(()=>window.ELDORIA_V022_QA.getState().state.finalVictory));i++){
   if(await page.locator('[data-testid="cinematic"]:visible').count()){await page.locator('[data-testid="cinematic"] button').evaluate(el=>el.click());await page.waitForTimeout(40);continue}
   await page.evaluate(()=>{const m=document.querySelector('body > .e22-overlay[data-testid="final-combat"]');m?.querySelector('[data-testid="final-specialist"]:not([disabled])')?.click();document.querySelector('body > .e22-overlay[data-testid="final-combat"] [data-testid="final-guard"]:not([disabled])')?.click();document.querySelector('body > .e22-overlay[data-testid="final-combat"] [data-testid="final-break"]:not([disabled])')?.click()});
   await page.waitForTimeout(80);
 }
 assert.equal(await page.evaluate(()=>window.ELDORIA_V022_QA.getState().state.finalVictory),true,'Final encounter could not be completed through player controls');
 // Mobile sanity: HUD/nav/scene remain usable and contextual actions stay on-screen.
 const mobile=await browser.newPage({viewport:{width:390,height:844}});
 const mobileErrors=[];mobile.on('pageerror',e=>mobileErrors.push(e.message));
 await mobile.goto('http://127.0.0.1:4173/?qa=1',{waitUntil:'load'});await mobile.waitForFunction(()=>window.ELDORIA_V022_QA);
 await mobile.evaluate(()=>window.ELDORIA_V022_QA.setState({introSeen:true,view:'kingdom',sawmill:false,bastionLevel:1,bastion:1,wood:600,stone:300}));
 await mobile.waitForSelector('[data-poi="sawmill"]:visible');await mobile.locator('[data-poi="sawmill"]').click();await mobile.waitForSelector('[data-context-action]:visible');
 const mb=await mobile.locator('[data-context-action]').boundingBox();assert(mb&&mb.x>=0&&mb.x+mb.width<=390&&mb.y>=0&&mb.y<844,'mobile contextual action leaves viewport');
 assert.equal(await mobile.locator('.e22Nav:visible').count(),1);assert.equal(mobileErrors.length,0,'mobile browser errors: '+mobileErrors.join(' | '));await mobile.close();
 assert.equal(errors.length,0,'browser errors: '+errors.join(' | '));
 console.log('ELDORIA_FULL_FLOW_SMOKE_OK');
 await browser.close();
})().catch(e=>{console.error(e);process.exit(1)});