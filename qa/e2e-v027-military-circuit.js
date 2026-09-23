const {chromium}=require('playwright');
const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1&preset=hero-army-base';
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(URL,{waitUntil:'domcontentloaded'});
 await p.waitForFunction(()=>sessionStorage.getItem('eldoria-qa-active-preset')==='hero-army-base'&&window.ELDORIA_V023?.state().view==='heroes'&&!window.ELDORIA_V023.state().heroDetailOpen);
 const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());
 // March composition must be tier-aware and use one source of truth.
 const globalPowerBefore=await p.evaluate(()=>window.ELDORIA_V023.state().power);
 const before=await p.locator('[data-testid="march-summary"]').innerText();
 const expectedBefore=await p.evaluate(()=>{const s=window.ELDORIA_V023.state(),H=window.ELDORIA.heroArmy,m=H.buildMarch({heroIds:s.marchSetup.heroIds,troops:s.marchSetup.troops,gearPowerByHero:Object.fromEntries(s.marchSetup.heroIds.map(id=>[id,Object.values((s.equipped||{})[id]||{}).filter(Boolean).reduce((n,g)=>n+(g.power||0),0)]))});return m.stats});
 if(!before.includes(String(expectedBefore.power)))throw Error('rendered march power diverges from domain source');
 const t1Plus=p.locator('[data-march-tier="1"][data-delta="10"]');if(await t1Plus.isDisabled())throw Error('T1 increment unexpectedly disabled');await t1Plus.tap({force:true});
 const changed=await p.locator('[data-testid="march-summary"]').innerText();if(changed===before)throw Error('tier composition did not change march stats');
 let s=await state();if(s.marchSetup.troops.archer[1]!==50||s.marchSetup.troops.archer[2]!==60)throw Error('tier quantities not persisted independently');
 if(s.power!==globalPowerBefore)throw Error('configuring a march changed global power / double counted power');
 // Barracks upgrade must unlock T2 without generating or converting troops.
 await p.evaluate(()=>window.ELDORIA_V023.setQA({view:'kingdom',bastionLevel:4,bastion3:true,barracks:true,wood:9999,stone:9999,food:9999,troops:20,troopRoster:{archer:{1:20,2:0,3:0},paladin:{},warlock:{}},buildingLevels:{sawmill:2,barracks:3,granary:2,stoneworks:0,forge:0},tasks:[],selectedAction:null}));
 await p.locator('[data-testid="building-barracks"]').tap({force:true});
 await p.locator('[data-testid="building-upgrade-barracks"]').evaluate(el=>el.click());
 await p.waitForFunction(()=>window.ELDORIA_V023.state().tasks.some(t=>/^upgrade-barracks-4/.test(t.key)));
 await p.evaluate(()=>{const q=window.ELDORIA_V023.state(),now=Date.now();window.ELDORIA_V023.setQA({tasks:q.tasks.map(t=>/^upgrade-barracks-4/.test(t.key)?{...t,end:now-1}:t)})});
 await p.waitForFunction(()=>window.ELDORIA_V023.state().buildingLevels.barracks===4,{timeout:4000});
 s=await state();
 if(s.troopRoster.archer[1]!==20||s.troopRoster.archer[2]!==0||s.troops!==20)throw Error('barracks upgrade generated/converted troops '+JSON.stringify(s.troopRoster));
 // Recruitment at barracks 4 exposes only Archer T1/T2 and preserves T1.
 await p.locator('[data-testid="building-barracks"]').evaluate(el=>el.click());
 await p.locator('[data-testid="building-action-barracks"]').evaluate(el=>el.click());
 const dlg=p.locator('[data-testid="recruit-dialog"]');await dlg.waitFor({state:'visible'});
 const tiers=await dlg.locator('[data-recruit-tier]').evaluateAll(xs=>xs.map(x=>x.dataset.recruitTier));
 if(tiers.join(',')!=='1,2')throw Error('barracks level 4 tier exposure wrong '+tiers.join(','));
 if((await dlg.innerText()).toLowerCase().includes('paladin')||(await dlg.innerText()).toLowerCase().includes('brujo'))throw Error('future troop family leaked into recruitment');
 await dlg.locator('[data-recruit-tier="2"]').evaluate(el=>el.click());
 await p.locator('[data-testid="recruit-choice-5"]').evaluate(el=>el.click());
 await p.locator('[data-testid="recruit-start"]').evaluate(el=>el.click());
 await p.waitForFunction(()=>window.ELDORIA_V023.state().tasks.some(t=>/^recruit-archer-t2-/.test(t.key)));
 await p.evaluate(()=>{const q=window.ELDORIA_V023.state(),now=Date.now();window.ELDORIA_V023.setQA({tasks:q.tasks.map(t=>/^recruit-archer-t2-/.test(t.key)?{...t,end:now-1}:t)})});
 await p.waitForFunction(()=>window.ELDORIA_V023.state().troopRoster.archer[2]===5,{timeout:4000});
 s=await state();
 if(s.troopRoster.archer[1]!==20||s.troopRoster.archer[2]!==5||s.troops!==25)throw Error('tier recruitment destroyed prior tier '+JSON.stringify(s.troopRoster));
 // Hero Hall opens/closes cleanly through navigation after military actions.
 await p.locator('[data-testid="nav-heroes"]').tap({force:true});await p.locator('[data-testid="hero-hall"]').waitFor({state:'visible'});
 await p.locator('[data-testid="hero-aldric"]').tap({force:true});await p.locator('[data-testid="hero-profile-aldric"]').waitFor({state:'visible'});
 await p.locator('[data-hero-back]').tap({force:true});await p.locator('[data-testid="hero-hall"]').waitFor({state:'visible'});
 await b.close();console.log('v0.27 INTEGRATED MILITARY CIRCUIT PASS');
})().catch(e=>{console.error(e);process.exit(1)});
