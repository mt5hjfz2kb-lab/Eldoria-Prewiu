const {chromium}=require('playwright');
const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1';
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(URL,{waitUntil:'domcontentloaded'});
 const set=x=>p.evaluate(v=>window.ELDORIA_V023.setQA(v),x),state=()=>p.evaluate(()=>window.ELDORIA_V023.state());
 const clear=()=>p.evaluate(()=>document.querySelectorAll('.aldric-cinematic,.e22-overlay').forEach(x=>x.remove()));
 const tapBuilding=async id=>{await clear();const el=p.locator('[data-testid="building-'+id+'"]');await el.waitFor({state:'visible'});await el.tap({force:true})};
 const dragWorld=async(dx=-110,dy=-55)=>{const wp=p.locator('[data-worldpan]'),box=await wp.boundingBox();if(!box)throw Error('worldpan missing');const sx=Math.min(320,box.x+250),sy=Math.min(540,box.y+330);await wp.dispatchEvent('pointerdown',{pointerId:31,clientX:sx,clientY:sy});await wp.dispatchEvent('pointermove',{pointerId:31,clientX:sx+dx,clientY:sy+dy});await wp.dispatchEvent('pointerup',{pointerId:31,clientX:sx+dx,clientY:sy+dy});await p.waitForTimeout(80);return wp.evaluate(el=>el.style.transform)};
 // 1. Shortage guidance is explicit and actionable.
 await set({view:'kingdom',introSeen:true,sawmill:true,bastion:2,bastionLevel:2,barracks:false,wood:10,stone:5,selectedAction:'barracks'});
 await p.locator('[data-testid="building-action-barracks"]').tap({force:true});
 const guide=p.locator('[data-testid="resource-guide"]');await guide.waitFor({state:'visible'});
 const guideText=await guide.innerText();for(const t of ['madera','piedra'])if(!guideText.toLowerCase().includes(t))throw Error('resource guide missing '+t+': '+guideText);
 if(!await guide.locator('[data-guide-go]').count())throw Error('resource guide has no destination action');
 await guide.locator('[data-guide-go]').first().tap({force:true});await p.waitForTimeout(80);
 if(await p.locator('[data-testid="resource-guide"]').isVisible().catch(()=>false))throw Error('resource shortage panel stayed visible after travelling to the World');
 let guideState=await state();if(guideState.view!=='world'||guideState.guideHint?.target?.kind!=='node')throw Error('resource destination focus was not preserved after travel '+JSON.stringify(guideState.guideHint));
 // 2. World selection/action never owns the camera; map remains draggable and transition-safe.
 await set({view:'world',introSeen:true,sawmill:true,bastion:2,bastionLevel:3,bastion3:true,barracks:true,granary:true,lyra:true,forest:1,quarry:1,forestRemain:900,wood:9999,stone:9999,food:9999,selectedAction:null,guideHint:null});
 const pan0=await dragWorld(-24,-12);
 const forest=p.locator('[data-testid="world-node-forest"]');await forest.evaluate(el=>el.click());await p.waitForTimeout(80);
 const panAfterSelect=await p.locator('[data-worldpan]').evaluate(el=>el.style.transform);if(panAfterSelect!==pan0)throw Error('node selection moved camera: '+pan0+' -> '+panAfterSelect);
 await p.locator('[data-testid="world-action-forest"]').evaluate(el=>el.click());await p.waitForTimeout(1900);
 if(!(await state()).tasks.some(t=>t.key==='gather-forest'))throw Error('gather task did not start');
 const panDuring=await dragWorld(-25,15);if(panDuring===panAfterSelect)throw Error('map cannot move while gathering');
 await p.locator('[data-testid="nav-kingdom"]').tap({force:true});await p.locator('[data-testid="nav-world"]').tap({force:true});
 const panReturn=await p.locator('[data-worldpan]').evaluate(el=>el.style.transform);if(panReturn!==panDuring)throw Error('world camera was reset by view transition');
 // 3. Barracks recruitment has its own timed flow and survives leaving/reloading.
 await set({view:'kingdom',introSeen:true,sawmill:true,bastion:2,bastionLevel:3,bastion3:true,barracks:true,buildingLevels:{sawmill:2,barracks:1,granary:2},wood:9999,stone:9999,food:9999,troops:36,tasks:[],selectedAction:null});
 await tapBuilding('barracks');await p.waitForTimeout(120);await p.locator('[data-testid="building-action-barracks"]').tap({force:true});await p.locator('[data-testid="recruit-dialog"]').waitFor({state:'visible'});
 const rt=await p.locator('[data-testid="recruit-dialog"]').innerText();for(const t of ['CANTIDAD','COSTE','TIEMPO'])if(!rt.includes(t))throw Error('recruit UI missing '+t);
 await p.locator('[data-recruit-qty="10"]').tap({force:true});await p.locator('[data-testid="recruit-start"]').tap({force:true});
 let st=await state(),task=st.tasks.find(t=>/^recruit-guards-/.test(t.key));if(!task||task.qty!==10||st.troops!==36)throw Error('recruit timer/task semantics wrong '+JSON.stringify({task,troops:st.troops}));
 await p.locator('[data-testid="nav-world"]').tap({force:true});
 await p.evaluate(()=>{const q=window.ELDORIA_V023.state(),now=Date.now();window.ELDORIA_V023.setQA({tasks:q.tasks.map(t=>/^recruit-guards-/.test(t.key)?{...t,start:now-1000,end:now+450}:t)})});
 await p.goto('about:blank');await p.waitForTimeout(700);await p.goto(URL,{waitUntil:'domcontentloaded'});
 st=await state();if(st.troops!==46||st.tasks.some(t=>/^recruit-guards-/.test(t.key)))throw Error('offline recruitment did not resolve '+JSON.stringify({troops:st.troops,tasks:st.tasks}));
 // 4. Barracks upgrade is distinct from recruitment.
 await set({...st,view:'kingdom',bastionLevel:3,barracks:true,buildingLevels:{...(st.buildingLevels||{}),barracks:1},wood:9999,stone:9999,tasks:[]});
 await tapBuilding('barracks');await p.waitForTimeout(120);const barracksCtx=p.locator('[data-testid="building-context-barracks"]');const barracksText=await barracksCtx.innerText();if(!/MEJORA EL CUARTEL A NIVEL 2/i.test(barracksText))throw Error('barracks objective still reads like troop recruitment: '+barracksText);const up=p.locator('[data-testid="building-upgrade-barracks"]');await up.waitFor({state:'visible'});const recruit=p.locator('[data-testid="building-action-barracks"]');const upBox=await up.boundingBox(),recruitBox=await recruit.boundingBox();if(!upBox||!recruitBox||upBox.y>=recruitBox.y)throw Error('barracks upgrade objective is not presented before recruitment');await up.tap({force:true});
 st=await state();let upTask=st.tasks.find(t=>/^upgrade-barracks-2/.test(t.key));if(!upTask)throw Error('barracks upgrade task missing');
 const timer=p.locator('[data-building-timer="barracks"] b');await timer.waitFor({state:'visible'});const t0=parseInt(await timer.innerText(),10);await p.waitForTimeout(1200);const t1=parseInt(await timer.innerText(),10);if(!(t1<t0))throw Error('building timer is frozen: '+t0+' -> '+t1);
 const dockRemaining=p.locator('[data-task-key="'+upTask.key+'"] [data-task-remaining]');if(await dockRemaining.count()){const d0=parseInt(await dockRemaining.innerText(),10);await p.waitForTimeout(1100);const d1=parseInt(await dockRemaining.innerText(),10);if(!(d1<d0))throw Error('queue timer is frozen: '+d0+' -> '+d1)}
 await p.evaluate(()=>{const q=window.ELDORIA_V023.state(),now=Date.now();window.ELDORIA_V023.setQA({tasks:q.tasks.map(t=>/^upgrade-barracks-2/.test(t.key)?{...t,end:now-1}:t)})});st=await state();if((st.buildingLevels?.barracks||0)!==2)throw Error('barracks did not upgrade to level 2');
 // 5. Bastion IV requires infrastructure and guides to the missing building.
 await set({view:'kingdom',introSeen:true,sawmill:true,barracks:true,granary:true,bastion:2,bastion3:true,bastionLevel:3,boss:true,lyra:true,wood:9999,stone:9999,food:9999,buildingLevels:{sawmill:1,barracks:1,granary:1},tasks:[],selectedAction:'keep',guideHint:null});
 const keepCtx=p.locator('[data-testid="building-context-keep"]');await keepCtx.waitFor({state:'visible'});const keepText=await keepCtx.innerText();if(!/INFRAESTRUCTURA/i.test(keepText))throw Error('Bastion IV context does not expose infrastructure requirements');
 await p.locator('[data-testid="building-action-keep"]').tap({force:true});st=await state();if(st.bastionLevel!==3||!st.guideHint?.target?.id)throw Error('Bastion IV did not block/guide missing infrastructure '+JSON.stringify(st.guideHint));
 await set({...st,view:'kingdom',wood:9999,stone:9999,food:9999,buildingLevels:{...st.buildingLevels,sawmill:2,barracks:2,granary:2},guideHint:null,selectedAction:'keep'});
 await p.locator('[data-testid="building-action-keep"]').tap({force:true});st=await state();if(!st.tasks.some(t=>t.key==='build-bastion4'))throw Error('Bastion IV did not start with infrastructure complete');
 // 6. Lyra exposes Heroes as a new system and acknowledgement clears the highlight.
 await set({view:'world',lyra:true,heroUnlockNotice:true,sawmill:true,bastionLevel:3,bastion3:true});
 const heroesNav=p.locator('[data-testid="nav-heroes"]');if(!await heroesNav.evaluate(el=>el.classList.contains('newUnlock0265')))throw Error('Heroes unlock is not highlighted');
 await heroesNav.tap({force:true});st=await state();if(st.heroUnlockNotice)throw Error('Heroes unlock notice did not clear after entering Heroes');
 // 7. Devourer produces a canonical narrative beat and a visually identified object.
 await clear();await set({view:'world',sawmill:true,bastionLevel:6,bastion3:true,barracks:true,granary:true,graniteQuarry:true,forge:true,lyra:true,devourerDefeated:false,aetherEmber:false,inventory:[],tasks:[],selectedAction:null});
 await p.locator('[data-testid="world-node-devourer"]').tap({force:true});await p.locator('[data-testid="world-action-devourer"]').tap({force:true});await p.waitForTimeout(1900);
 const story=p.locator('.aldric-cinematic[data-scene="story-dialogue"]');await story.waitFor({state:'visible',timeout:4000});if(!['NARRADOR','Lyra','Sir Aldric'].includes(await story.getAttribute('data-speaker')))throw Error('Devourer consequence did not use canonical dialogue');
 st=await state();if(!st.inventory.some(x=>x.id==='aether-ember'))throw Error('Aether Ember not stored in inventory');
 await clear();await set({...st,view:'chest'});const ember=p.locator('[data-testid="chest-item-aether-ember"]');await ember.waitFor({state:'visible'});if(!(await ember.innerText()).includes('🔥'))throw Error('Aether Ember lacks clear icon');
 // 8. All deep surfaces obey mobile vertical-scroll contract.
 const manyItems=Array.from({length:16},(_,i)=>({id:'qa-item-'+i,name:'Objeto QA '+i,type:'material',use:'Material de prueba'}));
 const cards=Array.from({length:7},(_,i)=>({id:'qa-card-'+i,name:'Reliquia QA '+i,rarity:i%2?'Rara':'Común',values:{N:2+i%3,E:3,S:1+i%2,O:4},copy:'Carta de prueba para validar scroll móvil.'}));
 await set({view:'chest',sawmill:true,lyra:true,chestUnlocked:true,codexUnlocked:true,bastionLevel:9,bastion3:true,inventory:manyItems,codex:cards,maelis:true});
 for(const [view,tid] of [['chest','chest-scroll'],['heroes',null],['codex','codex-scroll']]){await set({...await state(),view});const loc=tid?p.locator('[data-testid="'+tid+'"]'):p.locator('.heroArchiveScene');await loc.waitFor({state:'visible'});const css=await loc.evaluate(el=>({overflowY:getComputedStyle(el).overflowY,touchAction:getComputedStyle(el).touchAction,can:el.scrollHeight>el.clientHeight,before:el.scrollTop,sh:el.scrollHeight,ch:el.clientHeight}));if(!/auto|scroll/.test(css.overflowY)||!css.touchAction.includes('pan-y'))throw Error(view+' mobile scroll contract missing '+JSON.stringify(css));await loc.evaluate(el=>el.scrollTop=Math.min(180,el.scrollHeight-el.clientHeight));if((await loc.evaluate(el=>el.scrollTop))<=0&&css.can)throw Error(view+' cannot scroll vertically')}
 await set({...await state(),view:'kingdom',forge:true,aetherEmber:true,inventory:[{id:'aether-ember',name:'Ascua de Éter',type:'material'},...manyItems]});await p.locator('[data-testid="building-forge"]').tap({force:true});const forge=p.locator('[data-testid="forge-view"]');await forge.waitFor({state:'visible'});const fcss=await forge.evaluate(el=>({o:getComputedStyle(el).overflowY,t:getComputedStyle(el).touchAction}));if(!/auto|scroll/.test(fcss.o)||!fcss.t.includes('pan-y'))throw Error('Forge scroll contract missing '+JSON.stringify(fcss));
 await p.locator('[data-testid="power-total"]').tap({force:true});const power=p.locator('.e22-dialog.powerCard');await power.waitFor({state:'visible'});const pcss=await power.evaluate(el=>({o:getComputedStyle(el).overflowY,t:getComputedStyle(el).touchAction}));if(!/auto|scroll/.test(pcss.o)||!pcss.t.includes('pan-y'))throw Error('Power dialog scroll contract missing '+JSON.stringify(pcss));await clear();
 // 9. Codex hierarchy + real guided Duel with five temporary loan cards.
 const realCodex=[{id:'ash-sigil',name:'Sello de Ceniza',rarity:'Rara',quality:'Indestructible',values:{N:3,E:4,S:1,O:2},copy:'Una reliquia real del jugador.'}];
 await set({view:'codex',sawmill:true,lyra:true,chestUnlocked:true,codexUnlocked:true,bastionLevel:7,bastion3:true,codex:realCodex,cardsConsumed:[],duelTutorialComplete:false,duelSeen:false,cardChoices:{},cardCooldowns:{}});
 const codexText=await p.locator('[data-testid="codex-scroll"]').innerText();for(const t of ['DESCUBRIR','CONSERVAR','UTILIZAR','N 3','E 4','S 1','O 2'])if(!codexText.includes(t))throw Error('Codex UX missing '+t);
 await p.locator('[data-testid="open-relic-training"]').tap({force:true});const training=p.locator('[data-testid="duel-training"]');await training.waitFor({state:'visible'});
 if(await p.locator('[data-testid^="training-cell-"]').count()!==9)throw Error('training board is not 3x3');
 if(await p.locator('[data-testid="training-loan-count"]').innerText()!=='5')throw Error('five temporary cards were not lent');
 const grid=p.locator('[data-testid="training-board"]'),gb=await grid.boundingBox();if(!gb||gb.width>370)throw Error('training board does not fit mobile width '+JSON.stringify(gb));
 for(const [card,cell] of [[0,4],[1,2],[2,1],[3,7]]){await p.locator('[data-testid="training-card-'+card+'"]').tap({force:true});await p.locator('[data-testid="training-cell-'+cell+'"]').tap({force:true});await p.waitForTimeout(80)}
 await p.locator('[data-testid="duel-training-finish"]').waitFor({state:'visible'});await p.locator('[data-testid="duel-training-finish"]').tap({force:true});await clear();
 st=await state();if(!st.duelTutorialComplete||st.codex.length!==1||st.codex[0].id!=='ash-sigil'||st.codex.some(x=>String(x.id).startsWith('loan-')))throw Error('training cards leaked into permanent collection '+JSON.stringify({done:st.duelTutorialComplete,codex:st.codex}));
 if(st.view!=='codex')throw Error('training did not return to Codex');
 // 10. Genuine two-option decisions must not visually imply a correct answer.
 const neutral=await p.evaluate(()=>{const host=document.createElement('div');host.className='rift-choices';host.style.position='fixed';host.style.left='0';host.style.top='0';host.innerHTML='<button class="choice contain"><span>◇</span>A</button><button class="choice exploit"><span>◇</span>B</button>';document.body.appendChild(host);const [a,b]=host.querySelectorAll('button'),pick=x=>{const s=getComputedStyle(x);return{background:s.backgroundColor,border:s.borderColor,shadow:s.boxShadow,color:s.color}},out={a:pick(a),b:pick(b)};host.remove();return out});if(JSON.stringify(neutral.a)!==JSON.stringify(neutral.b))throw Error('two-option decision still has unequal visual weight '+JSON.stringify(neutral));
 await b.close();console.log('v0.26.5 OWNER IMPROVEMENT BLOCK PASS');
})().catch(e=>{console.error(e);process.exit(1)});