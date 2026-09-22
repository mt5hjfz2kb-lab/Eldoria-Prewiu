const {chromium}=require('playwright');
const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1&chaptersqa=1';
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(URL,{waitUntil:'domcontentloaded'});
 const set=x=>p.evaluate(v=>window.ELDORIA_V023.setQA(v),x);
 const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());
 await p.waitForFunction(()=>window.ELDORIA?.chapters&&document.querySelector('[data-testid="chapter-compact"]'));
 // Old top-left guide is replaced visually by compact chapter progress.
 const old=await p.locator('.quest').first().evaluate(el=>getComputedStyle(el).display).catch(()=>null);if(old&&old!=='none')throw Error('legacy quest still visible');
 const compact=p.locator('[data-testid="chapter-compact"]');await compact.waitFor({state:'visible'});let box=await compact.boundingBox();if(!box||box.height>70||box.width>300)throw Error('chapter compact panel too large '+JSON.stringify(box));
 await compact.tap({force:true});const drawer=p.locator('[data-testid="chapter-drawer"]');await drawer.waitFor({state:'visible'});box=await drawer.boundingBox();if(!box||box.width>360||box.height>590)throw Error('chapter drawer violates mobile budget '+JSON.stringify(box));
 await drawer.locator('[data-chapter-toggle]').tap({force:true});
 // Complete Chapter I from real state facts; rewards must be idempotent and include 2 universal 1m speedups.
 await set({sawmill:true,forest:1,quarry:1,camp:1,bastion:2,bastionLevel:2,wood:700,stone:500});
 await p.waitForTimeout(1300);let s=await state();if(!s.chapterProgress?.claimedChapters?.['1'])throw Error('chapter I not completed');if((s.speedups?.m1||0)<2)throw Error('chapter I speedups missing');
 let events=(s.sessionLog||[]).map(x=>x.type);for(const e of ['chapter_started','mission_completed','chapter_completed','chapter_reward_claimed','speedup_received'])if(!events.includes(e))throw Error('analytics missing '+e);
 const beforeReward={wood:s.wood,stone:s.stone,m1:s.speedups.m1};await p.waitForTimeout(1200);s=await state();if(s.wood!==beforeReward.wood||s.stone!==beforeReward.stone||s.speedups.m1!==beforeReward.m1)throw Error('chapter reward duplicated');
 // Speedup reduces authoritative timestamp and survives save/reload.
 const now=Date.now(),task={key:'qa-timed-build',title:'MEJORANDO VALORIA',target:'keep',start:now,end:now+125000};
 await set({tasks:[task],speedups:{...s.speedups,m1:Math.max(1,s.speedups.m1)},view:'kingdom'});await p.waitForTimeout(350);
 const speed=p.locator('[data-speedup-task="qa-timed-build"][data-speedup-unit="m1"]').first();await speed.waitFor({state:'visible'});const pre=(await state()).tasks.find(t=>t.key==='qa-timed-build').end;await speed.tap({force:true});await p.waitForTimeout(250);s=await state();const post=s.tasks.find(t=>t.key==='qa-timed-build')?.end;if(!post||pre-post<59000)throw Error('speedup did not reduce timestamp');if(!(s.chapterProgress?.counters?.speedupsUsed>=1))throw Error('speedup usage not tracked');
 await p.reload({waitUntil:'domcontentloaded'});await p.waitForFunction(()=>window.ELDORIA_V023&&document.querySelector('[data-testid="chapter-compact"]'));s=await state();if(!s.chapterProgress?.claimedChapters?.['1']||!(s.speedups?.m1>=0))throw Error('chapter/speedup persistence failed');
 // Chapter II progression uses real troop count and the combat report signal.
 await set({...s,tasks:[],bastion:2,bastionLevel:3,bastion3:true,barracks:true,troops:60,lastBattleReport:{enemyId:'spawnling',win:true,playerStart:800,playerEnd:500,enemyStart:620,enemyEnd:0,rounds:[{round:1}],reasons:['Tu Defensa absorbió una parte importante del daño.']},chapterProgress:{...s.chapterProgress,current:2}});
 await p.waitForTimeout(1300);s=await state();if(!(s.chapterProgress?.counters?.wins?.spawnling>=1))throw Error('common enemy result not connected to missions');
 // Codex is connected to gameplay: two enemy victories can add two real relic discoveries after unlock.
 let cp=s.chapterProgress;cp.counters.wins.spawnling=Math.max(1,cp.counters.wins.spawnling||0);cp.counters.wins.ashStalker=1;
 await set({...s,bastionLevel:7,codexUnlocked:true,codex:[{id:'ash-sigil',name:'Sello de Ceniza',rarity:'Rara',quality:'Indestructible',values:{N:3,E:4,S:1,O:2}}],chapterProgress:cp});
 await p.waitForTimeout(1300);s=await state();const ids=(s.codex||[]).map(x=>x.id);for(const id of ['ash-sigil','threshold-shard','ash-claw'])if(!ids.includes(id))throw Error('Codex gameplay drop missing '+id);
 if(!(s.sessionLog||[]).some(x=>x.type==='relic_obtained'))throw Error('relic analytics missing');
 // No future troop family leaks into current player-facing recruitment.
 await set({...s,view:'kingdom',barracks:true,selectedAction:'barracks',tasks:[]});await p.locator('[data-testid="building-action-barracks"]').tap({force:true});const rt=(await p.locator('[data-testid="recruit-dialog"]').innerText()).toLowerCase();if(!rt.includes('arquer'))throw Error('archers missing');for(const bad of ['palad','brujo'])if(rt.includes(bad))throw Error('future troop leak '+bad);
 await b.close();console.log('v0.27 CHAPTERS + MISSIONS + SPEEDUPS PASS');
})().catch(e=>{console.error(e);process.exit(1)});