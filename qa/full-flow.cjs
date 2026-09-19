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
 assert.equal(await page.locator('body.v022-core-active > .e22').count(),1);
 assert.equal(await page.locator('#game:visible').count(),0);
 await page.click('[data-poi="sawmill"]');
 await page.waitForSelector('[data-context-confirm]:visible');
 assert.match(await page.locator('[data-context-action]').innerText(),/80 madera/i);
 await page.click('[data-context-confirm]');
 await page.waitForFunction(()=>window.ELDORIA_V022_QA.getState().state.sawmill===true);
 assert.equal((await page.evaluate(()=>window.ELDORIA_V022_QA.getState().state.view)),'world');
 let before=await page.evaluate(()=>window.ELDORIA_V022_QA.getState().state.wood);
 await page.click('[data-node="forest"]'); await page.waitForSelector('[data-node-confirm]:visible');
 let mid=await page.evaluate(()=>window.ELDORIA_V022_QA.getState().state.wood); assert.equal(mid,before);
 await page.click('[data-node-confirm]');
 await page.waitForFunction(w=>window.ELDORIA_V022_QA.getState().state.wood>w,before);
 await page.click('[data-node="valoria"]'); await page.waitForSelector('.kingdom .keep:visible');
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
 await page.evaluate(()=>window.ELDORIA_V022_QA.setState({view:'world'})); await page.waitForSelector('[data-node="final"]:visible');
 assert.equal(errors.length,0,'browser errors: '+errors.join(' | '));
 console.log('ELDORIA_FULL_FLOW_SMOKE_OK');
 await browser.close();
})().catch(e=>{console.error(e);process.exit(1)});