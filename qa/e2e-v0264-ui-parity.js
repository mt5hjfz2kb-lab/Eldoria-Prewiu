const {chromium}=require('playwright');
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1';
 await p.goto(URL,{waitUntil:'domcontentloaded'});
 const set=x=>p.evaluate(v=>window.ELDORIA_V023.setQA(v),x),state=()=>p.evaluate(()=>window.ELDORIA_V023.state());
 // Locked progression parity.
 await set({view:'kingdom',sawmill:false,lyra:false,chestUnlocked:false,codexUnlocked:false,rankUnlocked:false});
 if(!await p.locator('[data-testid="nav-world"]:disabled').count())throw Error('World lock parity lost');
 if(await p.locator('[data-testid="nav-heroes"],[data-testid="valoria-chest"],[data-testid="nav-codex"]').count())throw Error('Locked primary destinations exposed early');
 // Fully unlocked navigation, exact primary order, ranking preserved outside main nav.
 await set({view:'kingdom',sawmill:true,lyra:true,maelis:true,chestUnlocked:true,codexUnlocked:true,rankUnlocked:true,bastion3:true,bastionLevel:9,inventory:[{id:'iron-brace',name:'Abrazadera de hierro antiguo',type:'material',use:'Mejora Aserradero'},{id:'qa-blade',name:'Hoja QA',slot:'weapon',power:10}],codex:[{id:'ash-sigil',name:'Sello de Ceniza',rarity:'Rara',quality:'Indestructible',values:{N:3,E:4,S:1,O:2},copy:'QA'}],narethRescued:true,troops:41,marchConfigured:true,marchSlots:['aldric','maelis'],trialWon:false});
 const navText=(await p.locator('[data-testid="primary-nav"] button').allInnerTexts()).map(x=>x.trim());
 const expected=['⌂\nCIUDAD','◎\nMUNDO','♞\nHÉROES','▣\nARCÓN','✦\nCÓDICE'];
 if(JSON.stringify(navText)!==JSON.stringify(expected))throw Error('Primary navigation parity/order mismatch: '+JSON.stringify(navText));
 if(await p.locator('[data-testid="primary-nav"] [data-testid="nav-ranking"]').count())throw Error('Ranking remained in primary nav');
 if(!await p.locator('[data-testid="nav-ranking"]').count())throw Error('Ranking access disappeared');
 // Full-screen destinations.
 for(const [tid,view,selector] of [['nav-heroes','heroes','.heroArchiveScene'],['valoria-chest','chest','.chestScene'],['nav-codex','codex','.codexScene']]){
   await p.locator('[data-testid="'+tid+'"]').tap({force:true});await p.locator(selector).waitFor({state:'visible'});
   if((await state()).view!==view)throw Error(view+' navigation failed');
   if(await p.locator('.e22-overlay:visible').count())throw Error(view+' regressed to overlay');
 }
 // Chest card -> in-screen detail; no lost item information.
 await p.locator('[data-testid="valoria-chest"]').tap({force:true});
 await p.locator('[data-testid="chest-item-iron-brace"]').tap({force:true});
 const detail=await p.locator('[data-testid="chest-item-detail"]').innerText();
 for(const term of ['Abrazadera','MATERIAL','CANTIDAD','UTILIDAD','PROCEDENCIA'])if(!detail.includes(term))throw Error('Chest detail missing '+term);
 // Forge is a full screen and keeps craft functionality.
 await set({view:'kingdom',bastionLevel:9,forge:true,forgeLvl:1,aetherEmber:true,inventory:[{id:'aether-ember',name:'Ascua de Éter',type:'material'}]});
 await p.locator('[data-testid="building-forge"]').tap({force:true});await p.locator('[data-testid="building-action-forge"]').tap({force:true});
 await p.locator('[data-testid="forge-view"]').waitFor({state:'visible'});if(await p.locator('.e22-overlay:visible').count())throw Error('Forge is not full screen');
 await p.locator('[data-testid="forge-craft-aether"]').tap({force:true});const forged=await state();if(!forged.inventory.some(x=>x&&x.slot==='weapon'&&x.name==='Hoja de Éter'))throw Error('Forge craft functionality lost');if(forged.view!=='forge')throw Error('Forge left management screen after craft');
 // Building context sheet stays above nav.
 await set({view:'kingdom',bastionLevel:3,bastion:2,bastion3:true,sawmill:true,granary:true,buildingLevels:{sawmill:1,granary:1},wood:2000,stone:2000,food:1000,selectedAction:'sawmill'});
 await p.waitForTimeout(100);const bp=p.locator('[data-testid="building-context-sawmill"]'),nav=p.locator('[data-testid="primary-nav"]');const [bb,nb]=await Promise.all([bp.boundingBox(),nav.boundingBox()]);if(!bb||!nb||bb.y+bb.height>nb.y+1)throw Error('Building context sheet overlaps bottom nav');
 // World context sheet outside pannable map; action remains reachable.
 await set({view:'world',bastionLevel:9,bastion3:true,sawmill:true,lyra:true,maelis:true,narethRescued:true,marchConfigured:true,marchSlots:['aldric','maelis'],trialWon:false,forestRemain:900,selectedAction:'forest'});
 await p.waitForTimeout(100);const sheet=p.locator('[data-testid="world-context-forest"]');await sheet.waitFor({state:'visible'});if(await sheet.evaluate(el=>!!el.closest('[data-worldpan]')))throw Error('World context remains trapped inside pannable map');const [sb,nb2]=await Promise.all([sheet.boundingBox(),p.locator('[data-testid="primary-nav"]').boundingBox()]);if(!sb||!nb2||sb.y+sb.height>nb2.y+1)throw Error('World context overlaps bottom nav');if(!await sheet.getByText('RECOLECTAR').count())throw Error('Gather action disappeared');
 // Map responds immediately to drag and preserves node actions.
 const wp=p.locator('[data-worldpan]');const before=await wp.evaluate(el=>el.style.transform);await wp.dispatchEvent('pointerdown',{pointerId:7,clientX:300,clientY:420});await wp.dispatchEvent('pointermove',{pointerId:7,clientX:180,clientY:360});await wp.dispatchEvent('pointerup',{pointerId:7,clientX:180,clientY:360});await p.waitForTimeout(120);const after=await wp.evaluate(el=>el.style.transform);if(before===after)throw Error('World pan did not follow pointer');
 // Category cues remain present.
 for(const kind of ['resource','fauna','threat','breach'])if(!await p.locator('[data-node-kind="'+kind+'"]').count())throw Error('World node category cue missing '+kind);
 // Touch targets: primary nav and contextual primary actions >=44px.
 for(const el of await p.locator('[data-testid="primary-nav"] button:visible,.contextSheet .primaryAction:visible').all()){const r=await el.boundingBox();if(r&&(r.height<44||r.width<44))throw Error('Touch target below 44px: '+JSON.stringify(r))}
 // Critical functional surface IDs must remain.
 const critical=['player-menu','power-total','nav-kingdom','nav-world','nav-heroes','valoria-chest','nav-codex','building-keep','building-sawmill','building-barracks','building-granary','building-forge','building-hall','world-node-forest','world-node-quarry','world-node-trial','trial-objective-dock'];
 for(const tid of critical){if(!await p.locator('[data-testid="'+tid+'"]').count())throw Error('Functional parity element missing '+tid)}
 await b.close();console.log('v0.26.4 UI PARITY + MOBILE UX PASS');
})().catch(e=>{console.error(e);process.exit(1)});