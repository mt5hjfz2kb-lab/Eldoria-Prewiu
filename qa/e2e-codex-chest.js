const {chromium}=require('playwright');
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 p.on('pageerror',e=>console.error('CODEX_QA_BROWSER_PAGE_ERROR',e.message));
 p.on('console',m=>{if(m.type()==='error')console.error('CODEX_QA_BROWSER_CONSOLE_ERROR',m.text())});
 await p.goto(process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1',{waitUntil:'domcontentloaded'});
 // Exercise Codex/Relicario independently on a genuine, coherent current save.
 // This UI regression does NOT certify late-game unlock/progression gates.
 const initialState=await p.evaluate(()=>window.ELDORIA_V023.state());
 const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());
 // Keep the original Chapter I state, advancing only isolated UI unlock flags.
 // Full Bastion VII fixtures have caused nonterminating scene initialization.
 const preset={...initialState,introSeen:true,view:'kingdom',chestUnlocked:true,inventory:[{id:'test-material',name:'Fragmento de prueba',type:'material',copy:'Material QA'}],codexUnlocked:true,relicarioUnlocked:true,codex:[{id:'estandarte-valoria',name:'Estandarte de Valoria',rarity:'rare',quality:'Indestructible',values:{N:6,S:5,E:3,O:6},effect:'Velocidad de marcha +20% durante 2 horas.'}],relicDiscovered:['estandarte-valoria'],relicIndestructibles:{'estandarte-valoria':true},relicEffects:{},cardsConsumed:[],cardChoices:{},cardCooldowns:{},relicTutorial:{started:true,firstReveal:true,rarities:true,decision:true,indestructible:true,sides:false,completed:true},relicTutorialComplete:true,relicTutorialSeen:true,relicSidesRevealed:false,relicarioTab:'collection',bastionLevel:initialState.bastionLevel};
 await p.evaluate(v=>localStorage.setItem('eldoria-v022-consistent-loop',JSON.stringify(v)),preset);
 console.log('CODEX_SAVE_PRE_RELOAD',await p.evaluate(()=>({level:JSON.parse(localStorage.getItem('eldoria-v022-consistent-loop')||'{}').bastionLevel})));
 await p.reload({waitUntil:'domcontentloaded'});
 console.log('CODEX_RUNTIME_AFTER_RELOAD',await p.evaluate(()=>({level:window.ELDORIA_V023?.state?.()?.bastionLevel,stored:JSON.parse(localStorage.getItem('eldoria-v022-consistent-loop')||'{}').bastionLevel,api:!!window.ELDORIA_V023})));
 await p.waitForFunction(()=>window.ELDORIA_V023?.state?.().bastionLevel===1,null,{timeout:5000});
 if((await state()).bastionLevel!==1)throw Error('Codex independent UI test original chapter state did not load');
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
