const {chromium}=require('playwright');
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 const url=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1';
 await p.goto(url,{waitUntil:'domcontentloaded'});
 await p.waitForFunction(()=>window.ELDORIA_V023?.state);
 const set=x=>p.evaluate(v=>window.ELDORIA_V023.setQA(v),x);
 const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());
 const chapter=(current,completed={})=>({version:27,current,completedMissions:completed,claimedChapters:{},missionRewards:{},chapterStarted:{[current]:Date.now()},counters:{gathered:{wood:0,stone:0,food:0},trained:20,hunts:0,wins:{spawnling:0,ashStalker:0,herald:0},speedupsUsed:0,relicDecisions:0,heroInterventions:0}});

 await set({introSeen:true,view:'kingdom',sawmill:true,barracks:true,chestUnlocked:true,bastion:2,bastionLevel:2,bastion3:true,wood:10000,stone:1250000,food:1000000000,troops:56,chapterProgress:chapter(2,{'c2-barracks':Date.now(),'c2-train':Date.now()}),missionPanelOpen:false});
 const hud=(await p.locator('[data-testid="resource-bar"]').innerText()).replace(/\s+/g,' ');
 if(!hud.includes('10K')||!hud.includes('1,25M')||!hud.includes('1B'))throw Error('Compact K/M/B HUD formatting missing: '+hud);
 const realm=await p.locator('.realm').innerText(),build=await p.locator('meta[name="eldoria-build"]').getAttribute('content');
 if(!realm.includes('v'+build))throw Error('Visible build version missing from HUD: '+realm);
 if(!await p.locator('[data-testid="hud-march"]').count())throw Error('Persistent March HUD access missing');

 const missionFont=await p.locator('[data-testid="chapter-compact"] em').evaluate(el=>parseFloat(getComputedStyle(el).fontSize));
 if(missionFont<10)throw Error('Closed mission copy still too small: '+missionFont);
 await p.locator('[data-testid="chapter-compact"]').tap({force:true});
 const drawer=(await p.locator('[data-testid="chapter-drawer"]').innerText()).toUpperCase();
 for(const term of ['¿CÓMO SUBE EL PODER DE EXPEDICIÓN?','HÉROES + TROPAS','CUARTEL','MARCHA'])if(!drawer.includes(term))throw Error('Chapter II Power explanation missing '+term);
 await p.locator('[data-mission-route="march"]').tap({force:true});
 await p.locator('[data-testid="march-screen"]').waitFor({state:'visible'});
 if(!await p.locator('[data-testid="c2-expedition-power-help"]').count())throw Error('March lacks Chapter II Power explanation');
 if(!await p.locator('[data-testid="march-close"]').count())throw Error('March close control missing');
 await p.locator('[data-testid="march-close"]').tap({force:true});
 if((await state()).view!=='kingdom')throw Error('March close did not return to kingdom');

 let s=await state();
 await set({...s,view:'kingdom',selectedAction:null,buildingLevels:{...(s.buildingLevels||{}),barracks:1}});
 await p.locator('[data-testid="building-barracks"]').tap({force:true});
 let barracksContext=p.locator('[data-testid="building-context-barracks"]');await barracksContext.waitFor({state:'visible'});
 if((await state()).selectedAction!=='barracks')throw Error('Real Barracks tap did not select the building');
 const coachClose=barracksContext.locator('[data-ux-coach-dismiss="barracks"]');
 if(await coachClose.count()){await coachClose.tap({force:true});barracksContext=p.locator('[data-testid="building-context-barracks"]');await barracksContext.waitFor({state:'visible'});}
 await p.waitForTimeout(120);
 const recruitAction=barracksContext.locator('[data-testid="building-action-barracks"]');await recruitAction.waitFor({state:'visible'});await recruitAction.tap();
 const recruit=p.locator('[data-testid="recruit-dialog"]');await recruit.waitFor({state:'visible',timeout:4000});
 if(!await recruit.evaluate(el=>el.classList.contains('early0304')))throw Error('Early Barracks did not enter simplified layout');
 for(const sel of ['.tierPicker027','.recruitChoiceMeta0265','.recruitSummary0265'])if(await recruit.locator(sel).count()&&await recruit.locator(sel).first().isVisible())throw Error('Early Barracks still exposes advanced clutter '+sel);
 const rt=(await recruit.innerText()).toUpperCase();for(const term of ['CUARTEL = CREAR TROPAS','ABRE MARCHA','PODER DE EXPEDICIÓN'])if(!rt.includes(term))throw Error('Early Barracks explanation missing '+term);
 await recruit.locator('[data-recruit-close]').tap({force:true});

 // First accelerator appears only when the player has a compatible timed task.
 s=await state();const speedNow=Date.now();
 await set({...s,view:'kingdom',bastionLevel:4,bastion3:true,speedups:{m1:0,m5:0,m15:0},speedupFirstGranted0304:false,speedupAwardIntroSeen:false,tasks:[{key:'upgrade-building-sawmill-2',title:'MEJORANDO ASERRADERO',target:'sawmill',start:speedNow,end:speedNow+30000,costPaid:true,cost:{wood:120,stone:78}}],chapterProgress:chapter(4,{})});
 await p.waitForTimeout(120);s=await state();if((s.speedups?.m1||0)!==1||!s.speedupFirstGranted0304)throw Error('Contextual first accelerator was not granted with first compatible task');

 s=await state();
 await set({...s,view:'forge',bastionLevel:6,forge:true,forgeLvl:1,aetherEmber:true,inventory:[{id:'aether-ember',name:'Ascua de Éter',type:'material'}]});
 await p.locator('[data-testid="forge-view"]').waitFor({state:'visible'});
 if(!await p.locator('[data-testid="forge-close"]').count())throw Error('Forge close control missing');
 const forgePurpose=(await p.locator('[data-testid="forge-purpose"]').innerText()).toUpperCase();
 for(const term of ['BASTIÓN VI','ASCUA DE ÉTER','DEVORADOR DE ÉTER','HOJA DE ÉTER','ARCÓN','HÉROES'])if(!forgePurpose.includes(term))throw Error('Forge purpose missing '+term);
 await p.locator('[data-testid="forge-craft-aether"]').tap({force:true});
 let forged=await state();if(!forged.firstForgeCeremonySeen||!forged.inventory.some(x=>x&&x.name==='Hoja de Éter'))throw Error('First forge milestone state missing');

 await set({...forged,view:'chest'});
 const chest=(await p.locator('[data-testid="chest-purpose"]').innerText()).toUpperCase();
 for(const term of ['QUÉ HAY EN EL ARCÓN','PARA QUÉ SIRVE','ACELERADORES','MATERIALES','EQUIPO'])if(!chest.includes(term))throw Error('Chest teaching missing '+term);

 await set({...await state(),view:'kingdom',bastionLevel:7,codexUnlocked:true,relicarioUnlocked:true,codex:[],relicDiscovered:[],cardsConsumed:[],enemyRespawns:{spawnling:0,ashStalker:0},chapterProgress:chapter(7,{'c7-codex':Date.now()}),missionPanelOpen:true});
 const c7=(await p.locator('[data-testid="chapter-drawer"]').innerText()).toUpperCase();
 for(const term of ['DÓNDE SE CONSIGUEN','ENGENDRO DE LA FISURA','ACECHADOR DE CENIZA','COMÚN 0,10','RARA 0,05','CAZA NUNCA'])if(!c7.includes(term))throw Error('Relic-source guidance missing '+term+': '+c7);
 await p.locator('[data-mission-go]').tap({force:true});
 let routed=await state();if(routed.view!=='relicario')throw Error('Relic mission did not route to independent Relicario: '+JSON.stringify({view:routed.view,selected:routed.selectedAction}));

 await set({...routed,troops:96,combatMarch:{troops:96,hero:'aldric'},view:'world',selectedAction:'spawnling'});
 await p.locator('[data-testid="world-action-spawnling"]').tap({force:true});
 await p.locator('[data-testid="march-prep-spawnling"]').waitFor({state:'visible'});
 await p.locator('[data-testid="combat-launch-spawnling"]').tap({force:true});
 await p.locator('[data-testid="world-march-animation"]').waitFor({state:'visible',timeout:1500});
 await p.locator('[data-testid="battle-report"]').waitFor({state:'visible',timeout:5000});

 await b.close();console.log('v0.30.4 OWNER UX PASS');
})().catch(e=>{console.error(e);process.exit(1)});