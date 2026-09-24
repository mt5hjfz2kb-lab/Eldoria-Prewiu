const {chromium}=require('playwright');
const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1';
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(URL,{waitUntil:'domcontentloaded'});
 const set=x=>p.evaluate(v=>window.ELDORIA_V023.setQA(v),x);
 const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());
 const base=await state();

 await set({...base,view:'kingdom',introSeen:true,bastionLevel:1,bastion:1,sawmill:false,missionIntroSeen:true,missionPanelOpen:false,chapterProgress:{version:27,current:1,completedMissions:{},claimedChapters:{},missionRewards:{},chapterStarted:{1:Date.now()},counters:{gathered:{wood:0,stone:0,food:0},trained:0,hunts:0,wins:{spawnling:0,ashStalker:0,herald:0},speedupsUsed:0,relicDecisions:0,heroInterventions:0}},speedups:{m1:0,m5:0,m15:0}});
 const compact=p.locator('[data-testid="chapter-compact"]');await compact.waitFor({state:'visible'});
 if(await p.locator('[data-testid="guided-step"]').count())throw Error('legacy guided-step remains visible');
 let txt=await compact.innerText();if(!/CAPÍTULO 1/i.test(txt)||!/Reconstruye el Aserradero/i.test(txt))throw Error('wrong compact mission '+txt);
 await compact.tap({force:true});const drawer=p.locator('[data-testid="chapter-drawer"]');await drawer.waitFor({state:'visible'});
 let box=await drawer.boundingBox();if(!box||box.width>360||box.height>590)throw Error('chapter drawer exceeds mobile budget '+JSON.stringify(box));
 txt=await drawer.innerText();if(!/RECOMPENSA DE CAPÍTULO/i.test(txt)||!/ACELERADORES UNIVERSALES/i.test(txt))throw Error('chapter details incomplete');
 await drawer.locator('[data-chapter-toggle]').tap({force:true});

 // Numeric mission progress.
 let s=await state();s.chapterProgress.counters.gathered.wood=340;
 await set({...s,sawmill:true,missionPanelOpen:false});await p.waitForTimeout(80);txt=await compact.innerText();if(!/340 \/ 600/.test(txt))throw Error('numeric mission progress missing '+txt);

 // Complete Chapter I from real facts + counters; rewards are idempotent.
 s=await state();s.chapterProgress.counters.gathered.wood=600;s.chapterProgress.counters.gathered.stone=500;
 await set({...s,sawmill:true,camp:1,bastion:2,bastionLevel:2});await p.waitForTimeout(100);
 s=await state();if(!s.chapterProgress.claimedChapters['1'])throw Error('chapter I did not complete');
 if((s.speedups.m1||0)!==0)throw Error('chapter I must not stockpile speedups before they are usable');
 let events=(s.sessionLog||[]).map(x=>x.type);for(const e of ['chapter_started','mission_completed','chapter_completed','chapter_reward_claimed'])if(!events.includes(e))throw Error('analytics missing '+e);
 const rewardSnapshot={m1:s.speedups.m1,claim:s.chapterProgress.claimedChapters['1'],rewardEvents:(s.sessionLog||[]).filter(x=>x.type==='chapter_reward_claimed'&&x.data?.chapter===1).length};await set({...s});await p.waitForTimeout(80);s=await state();if(s.speedups.m1!==rewardSnapshot.m1||s.chapterProgress.claimedChapters['1']!==rewardSnapshot.claim||(s.sessionLog||[]).filter(x=>x.type==='chapter_reward_claimed'&&x.data?.chapter===1).length!==rewardSnapshot.rewardEvents)throw Error('chapter reward duplicated');

 // Speedup modifies authoritative timestamp, resolves eligible task, persists one leftover.
 const now=Date.now();s.chapterProgress.current=4;s.chapterProgress.counters.speedupsUsed=0;
 await set({...s,view:'kingdom',bastionLevel:4,bastion:2,bastion3:true,developmentChoices:{4:{id:'balanced',at:now,qa:true}},sawmill:true,barracks:true,granary:true,buildingLevels:{sawmill:1,barracks:1,granary:1,stoneworks:0,forge:0},speedups:{m1:2,m5:0,m15:0},tasks:[{key:'upgrade-building-sawmill-2',title:'MEJORANDO ASERRADERO',target:'sawmill',start:now,end:now+30000,costPaid:true,cost:{wood:120,stone:78}}]});
 await p.locator('[data-queue]').tap({force:true});await p.locator('[data-testid="speedup-open"]').evaluate(el=>{const d=el.closest('details');if(d)d.open=true});const acc=p.locator('[data-speedup-task="upgrade-building-sawmill-2"][data-speedup-unit="m1"]');await acc.waitFor({state:'visible'});await acc.tap({force:true});await p.waitForTimeout(100);
 s=await state();if(s.speedups.m1!==1)throw Error('first speedup did not leave one spare');if(s.chapterProgress.counters.speedupsUsed!==1)throw Error('speedup usage not counted');if((s.buildingLevels||{}).sawmill!==2)throw Error('speedup did not resolve timestamp task');if(!(s.sessionLog||[]).some(x=>x.type==='speedup_used'))throw Error('speedup_used analytics missing');

 // Gathering tasks must never expose accelerator controls.
 const gnow=Date.now();await set({...s,tasks:[{key:'gather-forest',title:'RECOLECTANDO MADERA',target:'forest',start:gnow,end:gnow+30000}]});await p.locator('[data-queue]').tap({force:true});if(await p.locator('[data-speedup-task="gather-forest"]').count())throw Error('gathering incorrectly accepts speedups');

 // Codex chapter is main progression, and real gameplay discoveries count.
 s=await state();await set({...s,tasks:[],view:'kingdom',bastionLevel:7,developmentChoices:{...(s.developmentChoices||{}),4:{id:'balanced',at:Date.now(),qa:true},6:{id:'reserve',at:Date.now(),qa:true}},codexUnlocked:true,codex:[
 {id:'ash-sigil',name:'Sello de Ceniza',rarity:'Rara',quality:'Indestructible',values:{N:3,E:4,S:1,O:2}},
 {id:'rift-shard',name:'Fragmento de Fisura',rarity:'Común',values:{N:2,E:3,S:2,O:1}},
 {id:'ash-veil',name:'Velo de Ceniza',rarity:'Épica',values:{N:4,E:2,S:5,O:3}}
 ],chapterProgress:{version:27,current:7,completedMissions:{},claimedChapters:{},missionRewards:{},chapterStarted:{7:Date.now()},counters:{gathered:{wood:0,stone:0,food:0},trained:0,hunts:0,wins:{spawnling:0,ashStalker:0,herald:0},speedupsUsed:0,relicDecisions:0,heroInterventions:0}},missionPanelOpen:false});
 await p.locator('[data-testid="chapter-compact"]').evaluate(el=>el.click());await p.waitForTimeout(120);console.log('V027 CH7 DRAWER DIAG',await p.evaluate(()=>({missionPanelOpen:window.ELDORIA_V023.state().missionPanelOpen,current:window.ELDORIA_V023.state().chapterProgress?.current,compact:document.querySelector('[data-testid="chapter-compact"]')?.outerHTML,drawer:document.querySelector('[data-testid="chapter-drawer"]')?.outerHTML||null,overlays:[...document.querySelectorAll('.e22-overlay')].map(x=>x.textContent.slice(0,80))})));await p.locator('[data-testid="chapter-drawer"]').waitFor({state:'visible',timeout:5000});txt=await p.locator('[data-testid="chapter-drawer"]').innerText();if(!/EL CÓDICE/i.test(txt)||!/Descubre 3 Reliquias/i.test(txt))throw Error('Codex chapter missing');

 // Persistence survives reload.
 await p.reload({waitUntil:'domcontentloaded'});await p.locator('[data-testid="chapter-compact"]').waitFor({state:'visible'});s=await state();if(s.chapterProgress?.version!==27||s.speedups.m1!==1)throw Error('chapter/speedup persistence failed');

 // Current troop surface remains archers-only.
 await set({...s,view:'kingdom',barracks:true,selectedAction:'barracks',tasks:[],missionPanelOpen:false});await p.locator('[data-testid="building-action-barracks"]').evaluate(el=>el.click());const rt=(await p.locator('[data-testid="recruit-dialog"]').innerText()).toLowerCase();if(!rt.includes('arquer'))throw Error('archers missing');for(const bad of ['palad','brujo'])if(rt.includes(bad))throw Error('future troop leak '+bad);

 await b.close();console.log('v0.27 CHAPTERS + MISSIONS + SPEEDUPS PASS');
})().catch(e=>{console.error(e);process.exit(1)});