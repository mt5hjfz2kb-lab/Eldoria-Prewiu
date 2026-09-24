const {chromium}=require('playwright');
const medical=require('../v0220/js/military-medical.js');
const BASE=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1';
const root=BASE.replace(/\?.*$/,'');
(async()=>{
 if(medical.COMBAT_POLICY.pve.canKill!==false||medical.COMBAT_POLICY.pve.canWound!==true)throw Error('PvE casualty policy broken');
 if(medical.COMBAT_POLICY.pvp.canKill!==true||medical.COMBAT_POLICY.pvp.implemented!==false)throw Error('Future PvP separation missing');
 for(const sample of [
  {participated:90,playerStart:1000,playerEnd:800,enemyPower:6200,marchPower:7000,win:true,persist:true},
  {participated:20,playerStart:500,playerEnd:0,enemyPower:4000,marchPower:2500,win:false,persist:true},
  {participated:50,playerStart:500,playerEnd:100,enemyPower:9000,marchPower:3000,win:false,persist:false}
 ]){const x=medical.pveOutcome(sample);if(x.dead!==0)throw Error('PvE produced permanent deaths '+JSON.stringify(x))}
 const b=await chromium.launch({headless:true});

 // Save migration: old saves without medical fields must keep existing progress/resources/troops.
 const migration=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await migration.goto(root+'?qa=1',{waitUntil:'domcontentloaded'});await migration.waitForFunction(()=>window.ELDORIA_V023?.state);
 const before=await migration.evaluate(()=>{window.ELDORIA_V023.loadState('lyra');const x=window.ELDORIA_V023.state();x.wood=4321;x.stone=3210;x.food=2222;x.lastPassiveAt=Date.now();x.troopRoster={archer:{1:77,2:5,3:0},paladin:{},warlock:{}};x.troops=82;x.inventory=[...(x.inventory||[]),{id:'migration-keepsake',name:'Objeto de migración',type:'lore'}];const snapshot={wood:x.wood,stone:x.stone,food:x.food,bastion:x.bastionLevel,troops:x.troops,lyra:x.lyra,sawmill:x.sawmill,camp:x.camp,inventory:x.inventory.map(i=>i.id).sort().join('|')};delete x.woundedRoster;delete x.hospitalUnlocked;delete x.hospitalTutorialSeen;delete x.hospitalFirstTreatmentComplete;delete x.pvpCasualties;delete x.marchOperation;localStorage.setItem('eldoria-v022-consistent-loop',JSON.stringify(x));return snapshot});
 await migration.reload({waitUntil:'domcontentloaded'});await migration.waitForFunction(()=>window.ELDORIA_V023?.state);
 const migrated=await migration.evaluate(()=>window.ELDORIA_V023.state());
 if(migrated.wood!==before.wood||migrated.stone!==before.stone||migrated.food!==before.food||migrated.bastionLevel!==before.bastion||migrated.troops!==before.troops||migrated.lyra!==before.lyra||migrated.sawmill!==before.sawmill||migrated.camp!==before.camp||migrated.inventory.map(i=>i.id).sort().join('|')!==before.inventory)throw Error('Old-save migration changed existing progress '+JSON.stringify({before,after:{wood:migrated.wood,stone:migrated.stone,food:migrated.food,bastion:migrated.bastionLevel,troops:migrated.troops,lyra:migrated.lyra,sawmill:migrated.sawmill,camp:migrated.camp,inventory:migrated.inventory.map(i=>i.id).sort().join('|')}}));
 if(!migrated.woundedRoster||windowNaN(migrated.woundedRoster?.archer?.[1]))throw Error('Wounded roster not migrated');
 if(medical.total(migrated.woundedRoster)!==0||migrated.hospitalUnlocked!==false)throw Error('Old save did not initialize medical state safely');
 if(migrated.marchOperation?.mode!=='solo'||migrated.marchOperation?.contributionId!=='player-main')throw Error('March operation migration missing');
 await migration.close();

 // Real Bastion X Herald: PvE wear creates wounded but never deaths and unlocks Hospital.
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(root+'?qa=1&preset=worldboss',{waitUntil:'domcontentloaded'});
 await p.waitForSelector('[data-testid="world-node-herald"]');
 await p.locator('[data-testid="world-node-herald"]').click();
 await p.locator('[data-testid="world-context-herald"] [data-object-action="herald"]').click();
 await p.waitForSelector('[data-testid="march-prep-herald"]');
 await p.locator('[data-testid="combat-launch-herald"]').click();
 await p.waitForSelector('[data-testid="worldboss-combat"]');
 const skill=p.locator('[data-worldboss-skill]');await skill.waitFor({state:'visible'});await p.waitForFunction(()=>{const b=document.querySelector('[data-worldboss-skill]');return b&&!b.disabled});
 await skill.click();await p.waitForSelector('[data-testid="battle-report"]');
 const report=(await p.locator('[data-testid="battle-report"]').innerText()).toUpperCase();
 for(const term of ['PARTICIPARON','HERIDOS','BAJAS PERMANENTES','0'])if(!report.includes(term))throw Error('PvE troop report missing '+term);
 let state=await p.evaluate(()=>window.ELDORIA_V023.state());
 if(!state.lastBattleReport?.win)throw Error('QA Herald did not win');
 if(state.lastBattleReport.troopOutcome?.dead!==0)throw Error('Herald PvE created permanent deaths');
 if(!(state.lastBattleReport.troopOutcome?.wounded>0)||medical.total(state.woundedRoster)<=0)throw Error('Herald did not create persistent wounded');
 const woundedAfter=medical.total(state.woundedRoster),availableAfter=state.troops;
 if(availableAfter+woundedAfter<90)throw Error('Wounded troops stopped belonging to player');
 await p.locator('[data-battle-close]').click();
 const dialog=p.locator('.aldric-cinematic');if(await dialog.count()){const btn=dialog.locator('.aldric-continue');await btn.click();await btn.click();}
 await p.waitForSelector('[data-testid="building-hospital"]');
 const op=await p.evaluate(()=>window.ELDORIA_V023.medical.operationContract());
 if(op.ownerId!=='player'||op.marchId!=='main'||op.operationId!==null)throw Error('Solo march is not an independent future-operation contribution '+JSON.stringify(op));
 if(await p.locator('[data-join-operation],[data-join-rally],[data-testid*="concentration-join"]').count())throw Error('Fake multiplayer UI introduced');

 // Focused Hospital UI and March availability/power.
 await p.goto(root+'?qa=1&preset=b10-hospital-v032',{waitUntil:'domcontentloaded'});await p.waitForSelector('[data-testid="hospital-screen"]');
 if((await p.locator('[data-testid="hospital-wounded"]').innerText()).trim()!=='12')throw Error('Focused Hospital wounded count wrong');
 const overflow=await p.evaluate(()=>({w:document.documentElement.clientWidth,sw:document.documentElement.scrollWidth}));if(overflow.sw>overflow.w+2)throw Error('Hospital mobile horizontal overflow '+JSON.stringify(overflow));
 state=await p.evaluate(()=>window.ELDORIA_V023.state());if(state.troops!==78||medical.total(state.woundedRoster)!==12)throw Error('Available/wounded separation wrong');
 await p.locator('[data-hospital-back]').click();await p.waitForSelector('[data-testid="building-hospital"]');
 await p.evaluate(()=>window.ELDORIA_V023.setQA({...window.ELDORIA_V023.state(),view:'march'}));await p.waitForSelector('[data-testid="march-builder"]');
 const duringPower=Number((await p.locator('[data-testid="march-summary"] .power b').innerText()).replace(/\D/g,''));
 if(!await p.locator('[data-testid="march-wounded-note"]').count())throw Error('March does not explain wounded unavailability');
 const chosen=await p.evaluate(()=>window.ELDORIA_V023.state().marchSetup.troops.archer[1]);if(chosen>78)throw Error('Wounded leaked into selected march');

 // Start treatment, persist it over a reload, then complete and restore availability.
 await p.evaluate(()=>window.ELDORIA_V023.setQA({...window.ELDORIA_V023.state(),view:'hospital'}));await p.locator('[data-testid="hospital-heal"]').click();
 state=await p.evaluate(()=>window.ELDORIA_V023.state());let task=state.tasks.find(x=>x.key==='heal-hospital');if(!task||task.qty!==12)throw Error('Hospital timer not started');
 await p.evaluate(()=>{let s=window.ELDORIA_V023.state(),t=s.tasks.find(x=>x.key==='heal-hospital');t.end=Date.now()+900;window.ELDORIA_V023.setQA({...s,tasks:s.tasks})});
 await p.goto(root+'?qa=1',{waitUntil:'domcontentloaded'});await p.waitForFunction(()=>window.ELDORIA_V023?.state);await p.waitForTimeout(1250);
 state=await p.evaluate(()=>window.ELDORIA_V023.state());
 if(medical.total(state.woundedRoster)!==0||state.troops!==90||!state.hospitalFirstTreatmentComplete)throw Error('Persistent Hospital healing did not restore troops '+JSON.stringify({troops:state.troops,wounded:medical.total(state.woundedRoster),complete:state.hospitalFirstTreatmentComplete}));
 if(state.pvpCasualties?.total!==0||state.pvpCasualties?.reserved!==true)throw Error('PvP casualty placeholder mutated by PvE');

 // Rebuild recovered March and verify Expedition Power can return after healing.
 await p.evaluate(()=>{let s=window.ELDORIA_V023.state();window.ELDORIA_V023.setQA({...s,view:'march',marchSetup:{...s.marchSetup,troops:{archer:{1:90,2:0,3:0},paladin:{},warlock:{}}}})});
 await p.waitForSelector('[data-testid="march-summary"]');const afterPower=Number((await p.locator('[data-testid="march-summary"] .power b').innerText()).replace(/\D/g,''));
 if(!(afterPower>duringPower))throw Error('Expedition Power did not recover with healed troops '+JSON.stringify({duringPower,afterPower}));
 await b.close();console.log('v0.32 HOSPITAL + WOUNDED + MARCH OPERATION CONTRACT PASS');
})().catch(e=>{console.error(e);process.exit(1)});
function windowNaN(v){return Number.isNaN(Number(v))}
