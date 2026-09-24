const {chromium}=require('playwright');
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1';
 await p.goto(URL,{waitUntil:'domcontentloaded'});
 const set=x=>p.evaluate(v=>window.ELDORIA_V023.setQA(v),x),state=()=>p.evaluate(()=>window.ELDORIA_V023.state());
 // Locked progression parity.
 await set({view:'kingdom',sawmill:false,lyra:false,chestUnlocked:false,codexUnlocked:false,relicarioUnlocked:false,rankUnlocked:false});
 if(!await p.locator('[data-testid="nav-world"]:disabled').count())throw Error('World lock parity lost');
 if(await p.locator('[data-testid="nav-heroes"],[data-testid="valoria-chest"],[data-testid="nav-codex"],[data-testid="nav-relicario"]').count())throw Error('Locked primary destinations exposed early');
 // Fully unlocked navigation, exact primary order, ranking preserved outside main nav.
 await set({view:'kingdom',sawmill:true,lyra:true,maelis:true,chestUnlocked:true,codexUnlocked:true,relicarioUnlocked:true,rankUnlocked:true,bastion3:true,bastionLevel:9,inventory:[{id:'iron-brace',name:'Abrazadera de hierro antiguo',type:'material',use:'Mejora Aserradero'},{id:'qa-blade',name:'Hoja QA',slot:'weapon',power:10}],codex:[{id:'ash-sigil',name:'Sello de Ceniza',rarity:'Rara',quality:'Indestructible',values:{N:3,E:4,S:1,O:2},copy:'QA'}],narethRescued:true,troops:41,marchConfigured:true,marchSlots:['aldric','maelis'],trialWon:false});
 const navText=(await p.locator('[data-testid="primary-nav"] button').allInnerTexts()).map(x=>x.trim());
 const expected=['⌂\nCIUDAD','◎\nMUNDO','♞\nHÉROES','▣\nARCÓN','⌘\nCÓDICE','✦\nRELICARIO'];
 if(JSON.stringify(navText)!==JSON.stringify(expected))throw Error('Primary navigation parity/order mismatch: '+JSON.stringify(navText));
 if(await p.locator('[data-testid="primary-nav"] [data-testid="nav-ranking"]').count())throw Error('Ranking remained in primary nav');
 if(!await p.locator('[data-testid="nav-ranking"]').count())throw Error('Ranking access disappeared');
 // Full-screen destinations.
 for(const [tid,view,selector] of [['nav-heroes','heroes','.heroArchiveScene'],['valoria-chest','chest','.chestScene'],['nav-codex','codex','.codexScene'],['nav-relicario','relicario','[data-testid="relicario"]']]){
   await p.locator('[data-testid="'+tid+'"]').evaluate(el=>el.click());await p.locator(selector).waitFor({state:'visible'});
   if((await state()).view!==view)throw Error(view+' navigation failed');
   if(await p.locator('.e22-overlay:visible').count())throw Error(view+' regressed to overlay');
 }
 // Chest card -> in-screen detail; no lost item information.
 await p.locator('[data-testid="valoria-chest"]').evaluate(el=>el.click());
 await p.locator('[data-testid="chest-item-iron-brace"]').tap({force:true});
 const detail=await p.locator('[data-testid="chest-item-detail"]').innerText();
 for(const term of ['Abrazadera','MATERIAL','CANTIDAD','UTILIDAD','PROCEDENCIA'])if(!detail.includes(term))throw Error('Chest detail missing '+term);
 // Forge is a full screen and keeps craft functionality.
 await set({view:'kingdom',bastionLevel:9,forge:true,forgeLvl:1,aetherEmber:true,inventory:[{id:'aether-ember',name:'Ascua de Éter',type:'material'}]});
 await p.locator('[data-testid="building-forge"]').tap({force:true});
 await p.locator('[data-testid="forge-view"]').waitFor({state:'visible',timeout:5000});if(await p.locator('.e22-overlay:visible').count())throw Error('Forge is not full screen');
 await p.locator('[data-testid="forge-craft-aether"]').tap({force:true});const forged=await state();if(!forged.inventory.some(x=>x&&x.slot==='weapon'&&x.name==='Hoja de Éter'))throw Error('Forge craft functionality lost');if(forged.view!=='forge')throw Error('Forge left management screen after craft');
 // Building context stays object-local and above nav.
 await set({view:'kingdom',bastionLevel:3,bastion:2,bastion3:true,sawmill:true,granary:true,buildingLevels:{sawmill:1,granary:1},wood:2000,stone:2000,food:1000,selectedAction:'sawmill'});
 await p.waitForTimeout(100);const bp=p.locator('[data-testid="building-context-sawmill"]'),nav=p.locator('[data-testid="primary-nav"]');const [bb,nb,sawBox]=await Promise.all([bp.boundingBox(),nav.boundingBox(),p.locator('[data-testid="building-sawmill"]').boundingBox()]);if(!bb||!nb||!sawBox||bb.y+bb.height>nb.y+1)throw Error('Building context sheet overlaps bottom nav');const sawBelow=bb.y>=sawBox.y+sawBox.height-2&&bb.y-(sawBox.y+sawBox.height)<=10,sawAbove=sawBox.y>=bb.y+bb.height-2&&sawBox.y-(bb.y+bb.height)<=10;if(!sawBelow&&!sawAbove)throw Error('Sawmill context is not attached locally to its building: '+JSON.stringify({building:sawBox,context:bb}));
 await set({view:'kingdom',bastionLevel:3,bastion:2,bastion3:true,sawmill:true,granary:true,buildingLevels:{sawmill:1,granary:1},wood:2000,stone:2000,food:1000,selectedAction:'keep'});
 await p.waitForTimeout(100);const keep=p.locator('[data-testid="building-keep"]'),keepContext=p.locator('[data-testid="building-context-keep"]');const [kb,kcb]=await Promise.all([keep.boundingBox(),keepContext.boundingBox()]);if(!kb||!kcb)throw Error('Bastion local context missing');if(kcb.y<kb.y+kb.height-2||kcb.y-(kb.y+kb.height)>10)throw Error('Bastion context is not attached directly below the Bastion like other buildings: '+JSON.stringify({building:kb,context:kcb}));
 // World context stays attached to the selected marker; action remains compact and reachable.
 await set({view:'world',bastionLevel:9,bastion3:true,sawmill:true,lyra:true,maelis:true,narethRescued:true,marchConfigured:true,marchSlots:['aldric','maelis'],trialWon:false,forestRemain:900,selectedAction:'forest'});
 await p.waitForTimeout(100);const sheet=p.locator('[data-testid="world-context-forest"]');await sheet.waitFor({state:'visible'});if(!await sheet.evaluate(el=>!!el.closest('[data-node-wrap]')))throw Error('World context is no longer anchored to its selected node');const [sb,nb2,nodeBox]=await Promise.all([sheet.boundingBox(),p.locator('[data-testid="primary-nav"]').boundingBox(),p.locator('[data-testid="world-node-forest"]').boundingBox()]);if(!sb||!nb2||!nodeBox||sb.y+sb.height>nb2.y+1)throw Error('World context overlaps bottom nav');if(sb.y<nodeBox.y+nodeBox.height-2)throw Error('World context is not positioned beneath the selected marker');if(!await sheet.getByText('RECOLECTAR').count())throw Error('Gather action disappeared');
 if(sb.width>150||sb.height>150)throw Error('World local context regressed to oversized panel: '+JSON.stringify(sb));
 if(await sheet.locator('.contextMore0264:visible').count())throw Error('Secondary context info is permanently visible');const more=sheet.locator('[data-context-more]:visible');if(await more.count()){const details=more.locator('xpath=..');await details.evaluate(el=>el.open=true);if(!await sheet.locator('.contextMore0264:visible').count())throw Error('On-demand context info cannot be revealed');await details.evaluate(el=>el.open=false)}
 // Hard mobile composition budgets: HUD/nav/tutorial stay compact while the map keeps most of the viewport.
 const vh=844;
 const hudBox=await p.locator('.hud').boundingBox();if(hudBox&&hudBox.height>vh*.12)throw Error('HUD exceeds 12% viewport: '+hudBox.height);
 const resourceIcons=await p.locator('[data-testid="resource-bar"] .resChip i').allInnerTexts();if(resourceIcons[0]!=='🌲'||resourceIcons[1]!=='🪨'||(resourceIcons[2]&&resourceIcons[2]!=='🍖'))throw Error('Clear resource pictograms regressed: '+JSON.stringify(resourceIcons));
 const topEls=await p.locator('.hud > .crest,.hud > .res,.hud > .hudTools').evaluateAll(els=>els.map(el=>{const r=el.getBoundingClientRect();return {x:r.x,y:r.y,w:r.width,h:r.height,right:r.right,bottom:r.bottom}}));for(let i=0;i<topEls.length-1;i++){if(topEls[i].w&&topEls[i+1].w&&topEls[i].right>topEls[i+1].x+1)throw Error('HUD groups overlap: '+JSON.stringify(topEls))}
 const hudOverflow=await p.locator('.hud').evaluate(el=>el.scrollWidth-el.clientWidth);if(hudOverflow>1)throw Error('HUD overflows viewport by '+hudOverflow+'px');
 const navBox=await p.locator('[data-testid="primary-nav"]').boundingBox();if(navBox&&navBox.height>vh*.10)throw Error('Nav exceeds 10% viewport: '+navBox.height);
 const questBox=await p.locator('[data-testid="chapter-compact"]:visible').boundingBox();if(questBox&&questBox.height>vh*.08)throw Error('Chapter mission compact exceeds 8% viewport: '+questBox.height);
 const sceneBox=await p.locator('.scene.world').boundingBox();if(sceneBox&&sceneBox.height/vh<.65)throw Error('World scene visible share below 65%: '+(sceneBox.height/vh).toFixed(3));
 // Map responds immediately to drag and preserves node actions.
 const wp=p.locator('[data-worldpan]');const before=await wp.evaluate(el=>el.style.transform);await wp.dispatchEvent('pointerdown',{pointerId:7,clientX:300,clientY:420});await wp.dispatchEvent('pointermove',{pointerId:7,clientX:180,clientY:360});await wp.dispatchEvent('pointerup',{pointerId:7,clientX:180,clientY:360});await p.waitForTimeout(120);const after=await wp.evaluate(el=>el.style.transform);if(before===after)throw Error('World pan did not follow pointer');
 // Category cues remain present.
 for(const kind of ['resource','fauna','threat','breach'])if(!await p.locator('[data-node-kind="'+kind+'"]').count())throw Error('World node category cue missing '+kind);
 // Touch targets: primary nav and contextual primary actions >=44px.
 for(const el of await p.locator('[data-testid="primary-nav"] button:visible').all()){const r=await el.boundingBox();if(r&&(r.height<44||r.width<44))throw Error('Nav touch target below 44px: '+JSON.stringify(r))}for(const el of await p.locator('.contextSheet .primaryAction:visible').all()){const r=await el.boundingBox();if(r&&(r.height<34||r.width<44))throw Error('Compact contextual action target too small: '+JSON.stringify(r))}
 // Critical functional surfaces must remain in their real screens.
 const always=['player-menu','power-total','nav-kingdom','nav-world','nav-heroes','valoria-chest','nav-codex'];
 for(const tid of always){if(!await p.locator('[data-testid="'+tid+'"]').count())throw Error('Functional parity element missing '+tid)}
 await set({view:'kingdom',bastionLevel:9,bastion:2,bastion3:true,sawmill:true,barracks:true,granary:true,graniteQuarry:true,forge:true,lyra:true,chestUnlocked:true,codexUnlocked:true});
 for(const tid of ['building-keep','building-sawmill','building-barracks','building-granary','building-stoneworks','building-forge','building-hall']){if(!await p.locator('[data-testid="'+tid+'"]').count())throw Error('City parity element missing '+tid)}
 await set({view:'world',bastionLevel:9,bastion3:true,sawmill:true,lyra:true,maelis:true,narethRescued:true,marchConfigured:true,marchSlots:['aldric','maelis'],trialWon:false,forestRemain:900,quarryRemain:900});
 for(const tid of ['world-node-forest','world-node-quarry','world-node-trial','trial-objective-dock']){if(!await p.locator('[data-testid="'+tid+'"]').count())throw Error('World parity element missing '+tid)}
 await b.close();console.log('v0.26.4 UI PARITY + MOBILE UX PASS');
})().catch(e=>{console.error(e);process.exit(1)});