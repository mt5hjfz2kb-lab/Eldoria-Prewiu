const {chromium}=require('playwright');
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 const url=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1&preset=relicario-v029';
 await p.goto(url,{waitUntil:'domcontentloaded'});
 await p.waitForFunction(()=>sessionStorage.getItem('eldoria-qa-active-preset')==='relicario-v029'&&window.ELDORIA_V023?.state().view==='relicario');
 const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());

 // Códice and Relicario remain independent peer systems.
 await p.locator('[data-testid="relicario"]').waitFor({state:'visible'});
 if(!await p.locator('[data-testid="nav-codex"]').count()||!await p.locator('[data-testid="nav-relicario"]').count())throw Error('Codex/Relicario do not have independent primary navigation');
 const tabText=(await p.locator('.relicTabs027').innerText()).toUpperCase();
 for(const term of ['COLECCIÓN','PRÁCTICA','DUELO PVP','FUTURO'])if(!tabText.includes(term))throw Error('Relicario tabs missing '+term);
 await p.locator('[data-testid="nav-codex"]').evaluate(el=>el.click());
 await p.locator('[data-testid="codex-scroll"]').waitFor({state:'visible'});
 for(const id of ['breach','bestiary','world','characters'])if(!await p.locator('[data-codex-section="'+id+'"]').count())throw Error('Codex section missing '+id);
 if(await p.locator('[data-card-use],[data-card-keep],.relicCard026').count())throw Error('Codex still manages relic cards');
 if(await p.locator('[data-testid="open-relicario"],.relicarioPortal027').count())throw Error('Relicario is still nested inside Codex');
 await p.locator('[data-testid="nav-relicario"]').evaluate(el=>el.click());
 await p.locator('[data-testid="relicario"]').waitFor({state:'visible'});
 if(await p.locator('.relicarioToCodex027').count())throw Error('Relicario still presents Codex as parent');

 // One discovered card: its exact card data is visible, but Practice remains gated.
 await p.evaluate(()=>window.ELDORIA_V023.setQA({view:'relicario',relicarioTab:'collection',codex:[{id:'bosque-valoria',name:'Bosque de Valoria',rarity:'common',values:{N:6,S:3,E:4,O:2},effect:'20 minutos de producción actual de madera.'}],relicDiscovered:['bosque-valoria'],relicIndestructibles:{},cardsConsumed:[],cardChoices:{},cardCooldowns:{},relicTutorial:{started:true,firstReveal:true,rarities:true,decision:false,indestructible:false,sides:false,completed:true},relicTutorialComplete:true,relicSidesRevealed:false}));
 await p.locator('[data-testid="relicario-collection"]').waitFor({state:'visible'});
 const oneText=(await p.locator('[data-testid="relicario-collection"]').innerText()).toUpperCase();
 for(const term of ['COMÚN','USO','USAR','CONSERVAR','N 6','S 3','E 4','O 2'])if(!oneText.includes(term))throw Error('First relic surface missing '+term);
 await p.locator('[data-relic-tab="practice"]').evaluate(el=>el.click());
 const locked=(await p.locator('[data-testid="relicario-practice"]').innerText()).toUpperCase();
 if(!locked.includes('1/3')||!locked.includes('AÚN NO ES EL MOMENTO'))throw Error('Practice not gated before threshold');

 // Indestructible: same rarity/effect, no consumption and no extra cooldown.
 await p.evaluate(()=>window.ELDORIA_V023.setQA({...window.ELDORIA_V023.state(),view:'relicario',relicarioTab:'collection',marchConfigured:true,codex:[...window.ELDORIA_V023.state().codex,{id:'estandarte-valoria',name:'Estandarte de Valoria',rarity:'rare',quality:'Indestructible',values:{N:6,S:5,E:3,O:6},effect:'+20 % de velocidad de marcha durante exactamente 2 horas.'}],relicDiscovered:['bosque-valoria','estandarte-valoria'],relicIndestructibles:{'estandarte-valoria':true},cardCooldowns:{}}));
 await p.locator('[data-testid="card-use-estandarte-valoria"]').evaluate(el=>el.click());
 await p.locator('.e22-overlay .btn').evaluate(el=>el.click()).catch(()=>{});
 let after=await state();
 if(!after.codex.some(x=>x.id==='estandarte-valoria'&&String(x.quality).toLowerCase()==='indestructible'))throw Error('Indestructible relic disappeared');
 if((after.cardCooldowns?.['estandarte-valoria']||0)>Date.now()||!(after.relicEffects?.marchSpeedPct?.end>Date.now()))throw Error('Indestructible gained extra cooldown or lost its effect');

 // Normal copy is repeatable and consumed one copy at a time.
 await p.evaluate(()=>{window.ELDORIA_V023.relicario.grant('bosque-valoria',false);window.ELDORIA_V023.relicario.grant('bosque-valoria',false)});
 let before=await state(),normalBefore=before.codex.filter(x=>x.id==='bosque-valoria'&&!x.quality).length;
 await p.locator('[data-testid="card-use-bosque-valoria"]').evaluate(el=>el.click());
 await p.locator('.e22-overlay .btn').evaluate(el=>el.click()).catch(()=>{});
 let consumed=await state(),normalAfter=consumed.codex.filter(x=>x.id==='bosque-valoria'&&!x.quality).length;
 if(normalAfter!==normalBefore-1||!consumed.cardsConsumed.includes('bosque-valoria'))throw Error('Normal duplicate consumption contract broken');

 // Three genuine discoveries unlock Orin Practice; training never mutates permanent collection.
 await p.evaluate(()=>window.ELDORIA_V023.setQA({...window.ELDORIA_V023.state(),view:'relicario',relicarioTab:'collection',codex:[
 {id:'bosque-valoria',name:'Bosque de Valoria',rarity:'common',values:{N:6,S:3,E:4,O:2}},
 {id:'engendro-fisura',name:'Engendro de la Fisura',rarity:'common',values:{N:2,S:7,E:3,O:2}},
 {id:'acechador-ceniza',name:'Acechador de Ceniza',rarity:'rare',values:{N:8,S:3,E:5,O:3}}],
 relicDiscovered:['bosque-valoria','engendro-fisura','acechador-ceniza'],cardsConsumed:[],cardChoices:{},cardCooldowns:{},relicSidesRevealed:true,duelTutorialComplete:false}));
 await p.locator('[data-relic-tab="practice"]').evaluate(el=>el.click());
 await p.locator('[data-testid="open-relic-training"]').waitFor({state:'visible'});
 const beforePractice=await state(),beforeIds=beforePractice.codex.map(x=>x.id).sort().join('|');
 await p.locator('[data-testid="open-relic-training"]').evaluate(el=>el.click());
 await p.locator('[data-testid="duel-training"]').waitFor({state:'visible'});
 if(await p.locator('[data-testid^="training-cell-"]').count()!==9)throw Error('Practice board is not 3x3');
 for(const [card,cell] of [[0,4],[1,2],[2,1],[3,7]]){await p.locator('[data-testid="training-card-'+card+'"]').evaluate(el=>el.click());await p.locator('[data-testid="training-cell-'+cell+'"]').evaluate(el=>el.click());await p.waitForTimeout(40)}
 await p.locator('[data-testid="duel-training-finish"]').evaluate(el=>el.click());
 await p.evaluate(()=>document.querySelectorAll('.aldric-cinematic').forEach(x=>x.remove()));
 await p.locator('[data-testid="relicario-practice"]').waitFor({state:'visible'});
 const afterPractice=await state(),afterIds=afterPractice.codex.map(x=>x.id).sort().join('|');
 if(!afterPractice.duelTutorialComplete||beforeIds!==afterIds)throw Error('Guided practice state/collection contract broken');

 await p.locator('[data-relic-tab="pvp"]').evaluate(el=>el.click());
 const pvp=await p.locator('[data-testid="relicario-pvp"]').innerText();
 if(!/PRÓXIMAMENTE/i.test(pvp)||!/BLOQUEADO/i.test(pvp)||await p.locator('[data-testid="relicario-pvp"] button:not([disabled])').count())throw Error('PvP future lock unclear');

 const overflow=await p.evaluate(()=>({w:innerWidth,sw:document.documentElement.scrollWidth}));
 if(overflow.sw>overflow.w+3)throw Error('Codex/Relicario horizontal overflow '+JSON.stringify(overflow));
 await b.close();console.log('v0.29/v0.31 CODEX KNOWLEDGE + RELICARIO PASS');
})().catch(e=>{console.error(e);process.exit(1)});
