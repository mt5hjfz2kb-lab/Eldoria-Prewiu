const {chromium}=require('playwright');
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1',{waitUntil:'domcontentloaded'});
 const set=x=>p.evaluate(v=>window.ELDORIA_V023.setQA(v),x);
 const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());

 // 1) Construction spend is immediate, not deferred.
 await set({view:'kingdom',wood:1000,stone:1000,food:1000,sawmill:false,bastionLevel:1,bastion:1,tasks:[],selectedAction:'sawmill'});
 const before=await state();
 await p.locator('[data-testid="building-action-sawmill"]').tap({force:true});
 await p.waitForTimeout(120);
 const started=await state();
 if(started.wood!==before.wood-80)throw Error('Sawmill cost was not deducted at construction start');
 if(!started.tasks.some(t=>t.key==='build-sawmill'&&t.costPaid))throw Error('Construction task did not persist immediate-spend marker');

 // 2) Hero Hall is hero-only; troops and march live in separate full-screen surfaces.
 await set({view:'heroes',bastionLevel:9,lyra:true,maelis:true,troops:41,troopRoster:{archer:{1:41,2:0,3:0},paladin:{},warlock:{}},marchSetup:{heroIds:['aldric'],troops:{archer:{1:41,2:0,3:0}}},marchSlots:['aldric'],marchConfigured:false,inventory:[],heroSelected:'aldric',heroDetailOpen:false,uxCoachSeen:{heroes:true}});
 await p.locator('[data-testid="hero-hall"]').waitFor({state:'visible',timeout:3000});
 if(await p.locator('.e22-overlay:visible').count())throw Error('Hero Hall regressed to overlay');
 const hall=await p.locator('[data-testid="hero-hall"]').innerText();
 for(const forbidden of ['ARQUEROS T1','PREPARACIÓN DE MARCHA','PODER DE MARCHA'])if(hall.toUpperCase().includes(forbidden))throw Error('Hero Hall mixes military systems: '+forbidden);
 if(await p.locator('[data-testid="army-inventory"],[data-testid="march-builder"]').count())throw Error('Troops or march leaked into Hero Hall');
 await p.locator('[data-testid="hero-maelis"]').tap({force:true});
 await p.locator('[data-testid="hero-profile-maelis"]').waitFor({state:'visible'});
 await p.locator('[data-open-march]').evaluate(el=>el.click());
 await p.locator('[data-testid="march-screen"]').waitFor({state:'visible'});
 for(const term of ['HÉROES','TROPAS','COMPOSICIÓN','PODER DE MARCHA','CONFIRMAR MARCHA'])if(!(await p.locator('[data-testid="march-screen"]').innerText()).toUpperCase().includes(term))throw Error('Separated march screen missing '+term);
 await p.locator('[data-march-hero="maelis"]').tap({force:true});
 const march=await state();
 if(!march.marchSetup?.heroIds?.includes('maelis'))throw Error('March hero selection did not persist from march screen');

 // 3) IX must expose a robust attack action on 390x844 mobile after march configuration.
 await set({view:'world',bastionLevel:9,lyra:true,maelis:true,narethRescued:true,marchConfigured:true,marchSlots:['aldric','maelis'],trialWon:false,troops:41,selectedAction:null});
 const dock=p.locator('[data-testid="trial-objective-dock"]');
 await dock.waitFor({state:'visible',timeout:3000});
 const box=await dock.boundingBox();
 if(!box||box.y+box.height>844)throw Error('Bastion IX attack CTA is off-screen on mobile');
 if(!await dock.getByText('ATACAR').count())throw Error('Bastion IX attack CTA missing');

 // 4) Codex is knowledge-only; Relicario owns use/conserve decisions.
 await set({view:'codex',bastionLevel:7,codexUnlocked:true,codex:[{id:'ash-sigil',name:'Sello de Ceniza',rarity:'Rara',quality:'Indestructible',values:{N:3,S:1,E:4,O:2},copy:'QA'}],cardsConsumed:[],cardChoices:{},cardCooldowns:{},relicTutorialSeen:true,relicSidesRevealed:false,relicarioTab:'collection'});
 const codex=await p.locator('[data-testid="codex-scroll"]').innerText();
 for(const term of ['CÓDICE DE ELDORIA','LA BRECHA','BESTIARIO','MUNDO','PERSONAJES'])if(!codex.toUpperCase().includes(term))throw Error('Codex knowledge surface missing '+term);
 if(await p.locator('[data-card-use],[data-card-keep]').count())throw Error('Relic decisions leaked into Codex');
 await p.locator('[data-testid="open-relicario"]').evaluate(el=>el.click());
 const exp=await p.locator('[data-testid="relicario-collection"]').innerText();
 for(const term of ['USAR','CONSERVAR','INDESTRUCTIBLE'])if(!exp.toUpperCase().includes(term))throw Error('Relicario explanation missing '+term);

 await b.close();
 console.log('v0.26.3 OWNER FEEDBACK QA PASS');
})().catch(e=>{console.error(e);process.exit(1)});