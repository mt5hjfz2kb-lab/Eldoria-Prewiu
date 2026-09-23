const {chromium}=require('playwright');
const URL=(process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1')+(process.env.ELDORIA_URL?'&':'&')+'v030Manual=1';
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(URL,{waitUntil:'domcontentloaded'});
 await p.waitForFunction(()=>!!window.ELDORIA_V030&&!!window.ELDORIA_V023);
 const set=x=>p.evaluate(v=>window.ELDORIA_V023.setQA(v),x);
 const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());

 console.log('V030 CHECKPOINT hud');
 // Clock and compact settings live in the active HUD.
 await p.locator('[data-testid="world-clock"]').waitFor({state:'visible'});
 let clock=await p.locator('[data-testid="world-clock"]').innerText();
 if(!/SERVIDOR|SERVER/.test(clock)||!/UTC/.test(clock))throw Error('server clock missing '+clock);
 await p.locator('[data-testid="v030-settings"]').waitFor({state:'visible'});

 console.log('V030 CHECKPOINT choice');
 // Real development choice: mutually exclusive grant + persisted state.
 let s=await state();
 await set({...s,bastionLevel:4,bastion3:true,developmentChoices:{},wood:1000,stone:1000,food:500,chapterProgress:{...(s.chapterProgress||{}),current:4}});
 await p.evaluate(()=>window.ELDORIA_V030.openChoice(4));
 const choice=p.locator('[data-testid="v030-choice"]');await choice.waitFor({state:'visible'});
 const opts=choice.locator('[data-choice]');if(await opts.count()!==3)throw Error('development choice must expose 3 viable options');
 const army=choice.locator('[data-choice="army"]');
 await army.tap();await p.waitForTimeout(100);
 s=await state();if(s.developmentChoices?.[4]?.id!=='army'||s.power<1920)throw Error('development choice did not persist/apply '+JSON.stringify({choice:s.developmentChoices?.[4],power:s.power,wood:s.wood,stone:s.stone,food:s.food}));

 console.log('V030 CHECKPOINT autonomy');
 // Progressive autonomy: Bastion IX contains general objectives, no step-by-step jump button.
 s=await state();await set({...s,bastionLevel:9,developmentChoices:{4:{id:'army'},6:{id:'reserve'},8:{id:'balanced'},9:{id:'adapt'}},chapterProgress:{...(s.chapterProgress||{}),current:9,completedMissions:{},claimedChapters:{},missionRewards:{},chapterStarted:{9:Date.now()},counters:{gathered:{wood:0,stone:0,food:0},trained:0,hunts:0,wins:{spawnling:1,ashStalker:1,herald:0},speedupsUsed:0,relicDecisions:0,heroInterventions:0}},missionPanelOpen:false});
 await p.locator('[data-testid="chapter-compact"]').tap({force:true});await p.locator('[data-testid="chapter-drawer"]').waitFor({state:'visible'});await p.waitForTimeout(80);
 const drawer=p.locator('[data-testid="chapter-drawer"]');const dt=await drawer.innerText();if(!/OBJETIVOS GENERALES|GENERAL OBJECTIVES/.test(dt))throw Error('Bastion IX autonomy summary missing '+dt);if(await drawer.locator('[data-mission-go]').count())throw Error('Bastion IX still exposes step-by-step target jump');

 console.log('V030 CHECKPOINT rankings');
 // Rankings: all three categories, player and immediate rival are always visible.
 await p.evaluate(()=>window.ELDORIA_V030.openRankings('power'));
 let rank=p.locator('[data-testid="v030-ranking"]');await rank.waitFor({state:'visible'});
 if(await rank.locator('[data-rank-tab]').count()!==3)throw Error('expected three ranking tabs');
 if(await rank.locator('.rankRow.me').count()!==1||await rank.locator('[data-immediate-rival]').count()!==1)throw Error('player/immediate rival visibility missing');
 for(const tab of ['corrupt','relics']){await rank.locator('[data-rank-tab="'+tab+'"]').tap({force:true});rank=p.locator('[data-testid="v030-ranking"]');await rank.waitFor({state:'visible'});if(await rank.locator('.rankRow.me').count()!==1||await rank.locator('[data-immediate-rival]').count()!==1)throw Error('ranking context missing in '+tab)}
 await rank.locator('[data-close]').tap({force:true});

 console.log('V030 CHECKPOINT power');
 // First Power explanation is brief and separates Total Power from battle/march strength.
 await p.evaluate(()=>localStorage.removeItem('eldoria-v030-power-intro'));
 await p.locator('[data-testid="power-total"]').tap({force:true});const pi=p.locator('[data-testid="power-intro"]');await pi.waitFor({state:'visible'});let pit=(await pi.innerText()).toLowerCase();if(!pit.includes('poder total')||!pit.includes('combate'))throw Error('Power intro incomplete '+pit);await pi.locator('button').tap({force:true});

 console.log('V030 CHECKPOINT settings');
 // Settings + persistent English selector.
 const settingsButton=p.locator('[data-testid="v030-settings"]');
 await settingsButton.tap();let settings=p.locator('[data-testid="v030-settings-dialog"]');await settings.waitFor({state:'visible',timeout:5000});await settings.locator('[data-lang="en"]').tap();settings=p.locator('[data-testid="v030-settings-dialog"]');await settings.waitFor({state:'visible'});let st=await settings.innerText();if(!/SETTINGS/.test(st)||/AJUSTES/.test(st))throw Error('English settings localization mixed '+st);clock=await p.locator('[data-testid="world-clock"]').innerText();if(!/^SERVER/.test(clock))throw Error('clock did not localize '+clock);if(await p.evaluate(()=>localStorage.getItem('eldoria-v030-locale'))!=='en')throw Error('locale did not persist');
 const music=settings.locator('[data-music]'),sfx=settings.locator('[data-sfx]');await music.uncheck();await sfx.uncheck();const audio=await p.evaluate(()=>window.ELDORIA_V030.settings().audio);if(audio.music||audio.sfx)throw Error('audio controls failed');await settings.locator('[data-close]').tap();

 console.log('V030 CHECKPOINT battle-report');
 // Two-level battle report: summary + complete details without removing legacy report content.
 s=await state();await set({...s,locale:'en',view:'world',introSeen:true,bastionLevel:2,bastion:2,barracks:true,wood:9999,stone:9999,food:9999,troops:60,troopRoster:{archer:{1:60,2:0,3:0},paladin:{},warlock:{}},combatMarch:{troops:60,hero:'aldric'},enemyRespawns:{},selectedAction:null});
 const node=p.locator('[data-testid="world-node-spawnling"]');await node.waitFor({state:'visible'});await node.evaluate(el=>el.click());await p.waitForTimeout(70);await p.locator('[data-testid="world-action-spawnling"]').evaluate(el=>el.click());await p.locator('[data-testid="march-prep-spawnling"]').waitFor({state:'visible'});await p.locator('[data-testid="combat-launch-spawnling"]').tap({force:true});const report=p.locator('[data-testid="battle-report"]');await report.waitFor({state:'visible'});await p.locator('[data-testid="battle-report-summary"]').waitFor({state:'visible'});const details=p.locator('[data-testid="battle-report-details"]');await details.waitFor({state:'visible'});if(await details.locator(':scope > summary').count()!==1||await details.locator('details').count()!==0)throw Error('battle report must have one summary-to-details level');

 const audit=await p.evaluate(()=>window.ELDORIA_V030.audit());if(audit.breachExpansion||audit.pvpReal||audit.newHeroes||audit.newBuildings)throw Error('scope guard violated '+JSON.stringify(audit));
 await b.close();console.log('v0.30 DEPTH + AUTONOMY PASS');
})().catch(e=>{console.error(e);process.exit(1)});