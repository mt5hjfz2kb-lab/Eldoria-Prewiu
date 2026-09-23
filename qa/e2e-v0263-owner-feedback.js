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

 // 2) Hero Hall is a dedicated full-screen view, with understandable march builder.
 await set({view:'heroes',bastionLevel:9,lyra:true,maelis:true,troops:41,troopRoster:{archer:{1:41,2:0,3:0},paladin:{},warlock:{}},marchSetup:{heroIds:['aldric'],troops:{archer:{1:41,2:0,3:0}}},marchSlots:['aldric'],marchConfigured:false,inventory:[],heroSelected:'aldric',heroDetailOpen:false});
 await p.locator('.heroArchiveScene').waitFor({state:'visible',timeout:3000});
 if(await p.locator('.e22-overlay:visible').count())throw Error('Hero Hall regressed to overlay');
 const hall=await p.locator('.heroArchiveScene').innerText();
 for(const term of ['ARQUEROS','T1','T2','T3','PREPARACIÓN DE MARCHA','ATQ','DEF','VIDA','RUP'])if(!hall.toUpperCase().includes(term))throw Error('Hero Hall military contract missing '+term);
 await p.locator('[data-march-hero="maelis"]').tap({force:true});
 const march=await state();
 if(!march.marchSetup?.heroIds?.includes('maelis'))throw Error('March hero selection did not persist from full-screen Hall');

 // 3) IX must expose a robust attack action on 390x844 mobile after march configuration.
 await set({view:'world',bastionLevel:9,lyra:true,maelis:true,narethRescued:true,marchConfigured:true,marchSlots:['aldric','maelis'],trialWon:false,troops:41,selectedAction:null});
 const dock=p.locator('[data-testid="trial-objective-dock"]');
 await dock.waitFor({state:'visible',timeout:3000});
 const box=await dock.boundingBox();
 if(!box||box.y+box.height>844)throw Error('Bastion IX attack CTA is off-screen on mobile');
 if(!await dock.getByText('ATACAR').count())throw Error('Bastion IX attack CTA missing');

 // 4) Codex still has the explicit use/conserve education surface.
 await set({view:'codex',bastionLevel:7,codexUnlocked:true,codexTutorialSeen:true,codex:[{id:'ash-sigil',name:'Sello de Ceniza',rarity:'Rara',quality:'Indestructible',values:{N:3,S:1,E:4,O:2},copy:'QA'}],cardsConsumed:[],cardChoices:{},cardCooldowns:{}});
 const exp=await p.locator('[data-testid="codex-use-keep-explainer"]').innerText();
 for(const term of ['USAR','CONSERVAR','INDESTRUCTIBLE','Eco 1/1/1/1'])if(!exp.toLowerCase().includes(term.toLowerCase()))throw Error('Codex explanation missing '+term);

 await b.close();
 console.log('v0.26.3 OWNER FEEDBACK QA PASS');
})().catch(e=>{console.error(e);process.exit(1)});