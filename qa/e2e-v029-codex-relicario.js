const {chromium}=require('playwright');
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 const url=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1&preset=relicario-v029';
 await p.goto(url,{waitUntil:'domcontentloaded'});
 await p.waitForFunction(()=>sessionStorage.getItem('eldoria-qa-active-preset')==='relicario-v029'&&window.ELDORIA_V023?.state().view==='relicario');
 const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());

 // Relicario has the three required tabs and Codex is no longer the card manager.
 await p.locator('[data-testid="relicario"]').waitFor({state:'visible'});
 const tabText=(await p.locator('.relicTabs027').innerText()).toUpperCase();
 for(const term of ['COLECCIÓN','PRÁCTICA','DUELO PVP','FUTURO'])if(!tabText.includes(term))throw Error('Relicario tabs missing '+term);
 await p.locator('[data-view="codex"]').first().evaluate(el=>el.click());
 await p.locator('[data-testid="codex-scroll"]').waitFor({state:'visible'});
 for(const id of ['breach','bestiary','world','characters'])if(!await p.locator('[data-codex-section="'+id+'"]').count())throw Error('Codex section missing '+id);
 if(await p.locator('[data-card-use],[data-card-keep],.relicCard026').count())throw Error('Codex still manages relic cards');
 if(!await p.locator('[data-testid="open-relicario"]').count())throw Error('Codex lacks Relicario entry point');

 // Phase 1: one relic => rarity/effect/use-keep, no side values or board teaching.
 await p.evaluate(()=>window.ELDORIA_V023.setQA({view:'relicario',relicarioTab:'collection',codex:[{id:'ash-sigil',name:'Sello de Ceniza',rarity:'Rara',quality:'Indestructible',values:{N:3,E:4,S:1,O:2},copy:'Prueba QA'}],cardsConsumed:[],cardChoices:{},cardCooldowns:{},relicTutorialSeen:true,relicSidesRevealed:false,duelTutorialComplete:false}));
 await p.locator('[data-testid="relicario-collection"]').waitFor({state:'visible'});
 const oneText=(await p.locator('[data-testid="relicario-collection"]').innerText()).toUpperCase();
 for(const term of ['RARA','EFECTO','USAR','CONSERVAR','INDESTRUCTIBLE'])if(!oneText.includes(term))throw Error('First relic surface missing '+term);
 if(/\bN\s*3\b|\bE\s*4\b|\bS\s*1\b|\bO\s*2\b|TABLERO|ECO 1\/1\/1\/1/.test(oneText))throw Error('Board language revealed before five relics');
 await p.locator('[data-relic-tab="practice"]').evaluate(el=>el.click());
 const locked=(await p.locator('[data-testid="relicario-practice"]').innerText()).toUpperCase();
 if(!locked.includes('1/5')||!locked.includes('AÚN NO ES EL MOMENTO'))throw Error('Practice not gated before threshold');

 // Indestructible use: effect applies, card remains, cooldown starts.
 await p.locator('[data-relic-tab="collection"]').evaluate(el=>el.click());
 const before=await state();
 await p.locator('[data-testid="card-use-ash-sigil"]').evaluate(el=>el.click());
 await p.locator('.e22-overlay .btn').evaluate(el=>el.click()).catch(()=>{});
 const after=await state();
 if(after.stone<before.stone+260)throw Error('Indestructible effect missing');
 if(!after.codex.some(x=>x.id==='ash-sigil'))throw Error('Indestructible relic disappeared');
 if(!(after.cardCooldowns['ash-sigil']>Date.now()))throw Error('Indestructible cooldown missing');

 // Normal relic is consumed and recorded.
 await p.evaluate(()=>window.ELDORIA_V023.setQA({view:'relicario',relicarioTab:'collection',codex:[...window.ELDORIA_V023.state().codex,{id:'normal-qa',name:'Reliquia normal',rarity:'Común',values:{N:2,E:2,S:2,O:2},copy:'Consumible QA'}],cardCooldowns:{}}));
 await p.locator('[data-testid="card-use-normal-qa"]').evaluate(el=>el.click());
 await p.locator('.e22-overlay .btn').evaluate(el=>el.click()).catch(()=>{});
 const consumed=await state();
 if(consumed.codex.some(x=>x.id==='normal-qa')||!consumed.cardsConsumed.includes('normal-qa'))throw Error('Normal relic consumption contract broken');

 // Phase 2: at five discoveries values appear and Practice opens.
 await p.evaluate(()=>window.ELDORIA_V023.setQA({view:'relicario',relicarioTab:'collection',codex:[
 {id:'ash-sigil',name:'Sello de Ceniza',rarity:'Rara',quality:'Indestructible',values:{N:3,E:4,S:1,O:2},copy:'QA'},
 {id:'r2',name:'R2',rarity:'Común',values:{N:2,E:3,S:2,O:1},copy:'QA'},
 {id:'r3',name:'R3',rarity:'Épica',values:{N:4,E:2,S:5,O:3},copy:'QA'}],
 cardsConsumed:['r4','r5'],cardChoices:{},cardCooldowns:{},relicSidesRevealed:true,relicTutorialSeen:true,duelTutorialComplete:false}));
 await p.locator('[data-testid="relicario-collection"]').waitFor({state:'visible'});
 const fiveText=(await p.locator('[data-testid="relicario-collection"]').innerText()).toUpperCase();
 if(!fiveText.includes('N 3')||!fiveText.includes('E 4'))throw Error('Side values not revealed at threshold');
 await p.locator('[data-relic-tab="practice"]').evaluate(el=>el.click());
 await p.locator('[data-testid="open-relic-training"]').waitFor({state:'visible'});
 const beforePractice=await state();const beforeIds=beforePractice.codex.map(x=>x.id).sort().join('|');
 await p.locator('[data-testid="open-relic-training"]').evaluate(el=>el.click());
 await p.locator('[data-testid="duel-training"]').waitFor({state:'visible'});
 if(await p.locator('[data-testid^="training-cell-"]').count()!==9)throw Error('Practice board is not 3x3');
 for(const [card,cell] of [[0,4],[1,2],[2,1],[3,7]]){await p.locator('[data-testid="training-card-'+card+'"]').evaluate(el=>el.click());await p.locator('[data-testid="training-cell-'+cell+'"]').evaluate(el=>el.click());await p.waitForTimeout(40)}
 await p.locator('[data-testid="duel-training-finish"]').evaluate(el=>el.click());
 await p.evaluate(()=>document.querySelectorAll('.aldric-cinematic').forEach(x=>x.remove()));
 await p.locator('[data-testid="relicario-practice"]').waitFor({state:'visible'});
 const afterPractice=await state();const afterIds=afterPractice.codex.map(x=>x.id).sort().join('|');
 if(!afterPractice.duelTutorialComplete||afterPractice.view!=='relicario'||afterPractice.relicarioTab!=='practice')throw Error('Guided practice completion state wrong');
 if(beforeIds!==afterIds)throw Error('Practice permanently changed relic collection');

 // PvP is visible but explicitly inactive.
 await p.locator('[data-relic-tab="pvp"]').evaluate(el=>el.click());
 const pvp=await p.locator('[data-testid="relicario-pvp"]').innerText();
 if(!/PRÓXIMAMENTE/i.test(pvp)||!/BLOQUEADO/i.test(pvp))throw Error('PvP future lock unclear');
 if(await p.locator('[data-testid="relicario-pvp"] button:not([disabled])').count())throw Error('PvP exposes active functionality');

 // Mobile horizontal overflow guard.
 const overflow=await p.evaluate(()=>({w:innerWidth,sw:document.documentElement.scrollWidth}));
 if(overflow.sw>overflow.w+3)throw Error('Codex/Relicario horizontal overflow '+JSON.stringify(overflow));
 await b.close();console.log('v0.29 CODEX KNOWLEDGE + RELICARIO PASS');
})().catch(e=>{console.error(e);process.exit(1)});
