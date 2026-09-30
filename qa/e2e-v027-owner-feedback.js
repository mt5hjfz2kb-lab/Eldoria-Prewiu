const {chromium}=require('playwright');
const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1';
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(URL,{waitUntil:'domcontentloaded'});
 const set=x=>p.evaluate(v=>window.ELDORIA_V023.setQA(v),x);
 const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());

 // Regression: Chapter II asks for Spawnling before Bastion III, so it must exist in Bastion II.
 const completed={'c2-barracks':Date.now(),'c2-train':Date.now(),'c2-power':Date.now()};
 await set({view:'world',introSeen:true,sawmill:true,bastion:2,bastionLevel:2,barracks:true,forest:1,quarry:1,camp:1,troops:56,wood:9999,stone:9999,selectedAction:null,enemyRespawns:{},chapterProgress:{version:27,current:2,completedMissions:completed,claimedChapters:{1:Date.now()},missionRewards:{},chapterStarted:{2:Date.now()},counters:{gathered:{wood:600,stone:500,food:0},trained:20,hunts:0,wins:{spawnling:0,ashStalker:0,herald:0},speedupsUsed:0,relicDecisions:0,heroInterventions:0}},speedups:{m1:3,m5:1,m15:2}});
 const spawn=p.locator('[data-testid="world-node-spawnling"]');await spawn.waitFor({state:'visible'});const spawnBox=await spawn.boundingBox();const sceneBox=await p.locator('.scene.world').boundingBox();if(!spawnBox||!sceneBox||spawnBox.x<sceneBox.x||spawnBox.x+spawnBox.width>sceneBox.x+sceneBox.width||spawnBox.y<sceneBox.y||spawnBox.y+spawnBox.height>sceneBox.y+sceneBox.height)throw Error('Chapter II Spawnling exists but is not visible in the initial frontier viewport: '+JSON.stringify({spawnBox,sceneBox}));
 await spawn.evaluate(el=>el.click());await p.locator('[data-testid="world-action-spawnling"]').waitFor({state:'visible'});
 const chapter=p.locator('[data-testid="chapter-compact"]');const chapterText=await chapter.innerText();if(!/Engendro de la Fisura/i.test(chapterText))throw Error('Chapter II current objective is not Spawnling: '+chapterText);

 // Chest exposes utility tabs and universal speedups with quantities.
 await set({...await state(),view:'chest',chestUnlocked:true,chestTab:'all',inventory:[
   {id:'qa-mat',name:'Material QA',type:'material',use:'Material de prueba'},
   {id:'qa-gear',name:'Equipo QA',slot:'weapon',power:10},
   {id:'qa-lore',name:'Objeto QA',type:'lore',copy:'Objeto especial de prueba'}
 ]});
 const tabs=p.locator('[data-testid="chest-tabs"]');await tabs.waitFor({state:'visible'});
 for(const id of ['all','speedups','materials','equipment','special'])if(!await p.locator('[data-testid="chest-tab-'+id+'"]').count())throw Error('Missing chest tab '+id);
 await p.locator('[data-testid="chest-tab-speedups"]').tap({force:true});
 for(const id of ['m1','m5','m15'])await p.locator('[data-testid="chest-item-speedup-'+id+'"]').waitFor({state:'visible'});
 let text=await p.locator('[data-testid="chest-scroll"]').innerText();for(const t of ['ACELERADORES','×3','×1','×2'])if(!text.includes(t))throw Error('Speedup wallet missing '+t+': '+text);
 await p.locator('[data-testid="chest-item-speedup-m5"]').tap({force:true});let detail=await p.locator('[data-testid="chest-item-detail"]').innerText();if(!/5 minuto/i.test(detail)||!/CANTIDAD\s*1/i.test(detail.replace(/\n/g,' ')))throw Error('Speedup detail is incomplete: '+detail);
 await p.locator('[data-testid="chest-tab-materials"]').tap({force:true});if(!await p.locator('[data-testid="chest-item-qa-mat"]').isVisible())throw Error('Material tab lost material');
 if(await p.locator('[data-testid="chest-item-qa-gear"]').count())throw Error('Equipment leaked into Materials tab');

 await b.close();console.log('v0.27 OWNER FEEDBACK SPAWNLING + CHEST PASS');
})().catch(e=>{console.error(e);process.exit(1)});