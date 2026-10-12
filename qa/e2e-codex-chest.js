const {chromium}=require('playwright');
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1',{waitUntil:'domcontentloaded'});
 await p.addScriptTag({path:require('node:path').resolve(__dirname,'../v0220/js/qa-fixtures.js')});
 const fullBastion7=await p.evaluate(()=>window.ELDORIA_QA_FIXTURES.get('b7-codex'));
 const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());
 // Rehydrate a complete, coherent Bastion VII SAVE before the game boots.
 // Calling setQA() to jump from Chapter I to Chapter VII during render causes
 // a nonterminating browser main-thread transition and is not a valid playthrough.
 const preset={...fullBastion7,introSeen:true,view:'kingdom',chestUnlocked:true,inventory:[{id:'test-material',name:'Fragmento de prueba',type:'material',copy:'Material QA'}],codexUnlocked:true,relicarioUnlocked:true,codex:[{id:'estandarte-valoria',name:'Estandarte de Valoria',rarity:'rare',quality:'Indestructible',values:{N:6,S:5,E:3,O:6},effect:'Velocidad de marcha +20% durante 2 horas.'}],relicDiscovered:['estandarte-valoria'],relicIndestructibles:{'estandarte-valoria':true},relicEffects:{},cardsConsumed:[],cardChoices:{},cardCooldowns:{},relicTutorial:{started:true,firstReveal:true,rarities:true,decision:true,indestructible:true,sides:false,completed:true},relicTutorialComplete:true,relicTutorialSeen:true,relicSidesRevealed:false,relicarioTab:'collection',bastionLevel:7};
 await p.evaluate(v=>localStorage.setItem('eldoria-v022-consistent-loop',JSON.stringify(v)),preset);
 await p.reload({waitUntil:'domcontentloaded'});
 await p.waitForFunction(()=>window.ELDORIA_V023?.state?.().bastionLevel===7,null,{timeout:5000});
 if((await state()).bastionLevel!==7)throw Error('Codex test canonical save did not load');
 await p.locator('[data-testid="valoria-chest"]').tap({force:true});
 await p.locator('.archiveScene.chestScene').waitFor({state:'visible',timeout:4000});
 if((await state()).view!=='chest')throw Error('Chest did not become a dedicated view');
 await p.locator('[data-testid="nav-codex"]').tap({force:true});
 await p.locator('[data-testid="codex-scroll"]').waitFor({state:'visible'});
 if(await p.locator('[data-testid="codex-scroll"] [data-card-use],[data-testid="codex-scroll"] [data-card-keep]').count())throw Error('Codex still contains card management');
 const codexText=(await p.locator('[data-testid="codex-scroll"]').innerText()).toUpperCase();
 for(const term of ['CÓDICE DE ELDORIA','LA BRECHA','BESTIARIO','MUNDO','PERSONAJES','CONOCIMIENTO'])if(!codexText.includes(term))throw Error('Knowledge Codex missing '+term);
 if(codexText.includes('ABRIR RELICARIO')||await p.locator('[data-testid="open-relicario"],.relicarioPortal027').count())throw Error('Relicario is still nested inside Codex');
 if(!await p.locator('[data-testid="nav-relicario"]').count())throw Error('Independent Relicario navigation is missing');
 await p.locator('[data-testid="nav-relicario"]').tap({force:true});
 await p.locator('[data-testid="relicario"]').waitFor({state:'visible'});
 const rel=(await p.locator('[data-testid="relicario"]').innerText()).toUpperCase();
 for(const term of ['COLECCIÓN','PRÁCTICA','DUELO PVP','USAR','CONSERVAR'])if(!rel.includes(term))throw Error('Relicario missing '+term);
 const before=await state();
 await p.locator('[data-testid="card-use-estandarte-valoria"]').evaluate(el=>el.click());
 await p.locator('.e22-overlay .btn').evaluate(el=>el.click()).catch(()=>{});
 const after=await state();
 if(!after.codex.some(x=>x.id==='estandarte-valoria')||(after.cardCooldowns?.['estandarte-valoria']>Date.now())||!(after.relicEffects?.marchSpeedPct?.end>Date.now()))throw Error('Indestructible relic contract failed');
 await b.close();console.log('CODEX KNOWLEDGE + RELICARIO + CHEST PASS');
})().catch(e=>{console.error(e);process.exit(1)});
