const {chromium}=require('playwright');
const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1';
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(URL,{waitUntil:'domcontentloaded'});
 await p.evaluate(()=>window.ELDORIA_V023.setQA({introSeen:true,view:'kingdom',bastion:2,bastionLevel:2,barracks:true,buildingLevels:{sawmill:1,barracks:1,granary:0,stoneworks:0,forge:0},wood:9999,stone:9999,food:0,tasks:[],selectedAction:'barracks',recruitQty:5}));
 await p.locator('[data-testid="building-action-barracks"]').tap({force:true});
 const dialog=p.locator('[data-testid="recruit-dialog"]');await dialog.waitFor({state:'visible'});
 for(const q of [5,10,20]){
   const card=p.locator('[data-testid="recruit-choice-'+q+'"]');await card.waitFor({state:'visible'});
   const t=(await card.innerText()).replace(/\s+/g,' ');
   const wood=20*q,stone=12*q,sec=Math.max(7,Math.ceil(q*.8+4));
   for(const expected of [String(q)+' ARQUEROS',String(wood),String(stone),String(sec)+'s'])if(!t.includes(expected))throw Error('Recruit choice '+q+' missing visible '+expected+': '+t);
 }
 await p.locator('[data-testid="recruit-choice-20"]').tap({force:true});
 const summary=p.locator('[data-recruit-summary]');await summary.waitFor({state:'visible'});
 const st=(await summary.innerText()).replace(/\s+/g,' ');
 for(const expected of ['20 arqueros','400 madera','240 piedra','20s'])if(!st.includes(expected))throw Error('Recruit summary missing '+expected+': '+st);
 const style=await p.locator('[data-testid="recruit-choice-20"] .recruitChoiceMeta0265 strong').first().evaluate(el=>({color:getComputedStyle(el).color,display:getComputedStyle(el).display,rect:el.getBoundingClientRect().toJSON()}));
 if(!style.rect.width||!style.rect.height||style.color==='rgba(0, 0, 0, 0)')throw Error('Recruit detail is not visibly rendered '+JSON.stringify(style));
 await p.locator('[data-recruit-close]').tap({force:true});
 // Tier progression is driven by Barracks level and never replaces older troops.
 await p.evaluate(()=>window.ELDORIA_V023.setQA({introSeen:true,view:'kingdom',bastion:4,bastionLevel:4,barracks:true,buildingLevels:{sawmill:2,barracks:4,granary:2,stoneworks:0,forge:0},wood:9999,stone:9999,food:9999,tasks:[],selectedAction:null,recruitQty:5,recruitTier:2,troops:200,troopRoster:{archer:{1:140,2:60,3:0},paladin:{},warlock:{}}}));
 await p.locator('[data-testid="building-barracks"]').tap({force:true});
 await p.locator('[data-testid="building-action-barracks"]').tap({force:true});
 await p.locator('[data-testid="recruit-dialog"]').waitFor({state:'visible'});
 const tiers=(await p.locator('[data-testid="recruit-tier-picker"]').innerText()).replace(/\s+/g,' ');
 if(!tiers.includes('T1')||!tiers.includes('T2')||tiers.includes('T3 Cuartel 10+'))throw Error('Barracks 4 tier picker wrong: '+tiers);
 await p.locator('[data-recruit-tier="2"]').tap({force:true});
 await p.locator('[data-testid="recruit-start"]').tap({force:true});
 let tierState=await p.evaluate(()=>window.ELDORIA_V023.state());
 const tierTask=tierState.tasks.find(t=>/^recruit-archer-t2-/.test(t.key));
 if(!tierTask||tierTask.tier!==2)throw Error('T2 recruitment task not persisted');
 if(tierState.troopRoster.archer[1]!==140||tierState.troopRoster.archer[2]!==60)throw Error('Starting T2 recruitment changed existing tiers early');

 // Building upgrade cannot generate or convert troops.
 await p.evaluate(()=>window.ELDORIA_V023.setQA({view:'kingdom',bastionLevel:5,barracks:true,buildingLevels:{sawmill:2,barracks:3,granary:2,stoneworks:0,forge:0},wood:9999,stone:9999,tasks:[],troops:200,troopRoster:{archer:{1:140,2:60,3:0},paladin:{},warlock:{}},selectedAction:'barracks'}));
 const beforeUpgrade=await p.evaluate(()=>JSON.stringify(window.ELDORIA_V023.state().troopRoster));
 await p.locator('[data-testid="building-upgrade-barracks"]').tap({force:true});
 let upgradeState=await p.evaluate(()=>window.ELDORIA_V023.state());
 const upTask=upgradeState.tasks.find(t=>/^upgrade-barracks-4$/.test(t.key));
 if(!upTask)throw Error('Barracks upgrade task missing');
 if(JSON.stringify(upgradeState.troopRoster)!==beforeUpgrade)throw Error('Starting Barracks upgrade changed troops');

 await b.close();console.log('v0.27 BARRACKS RECRUITMENT DETAILS PASS');
})().catch(e=>{console.error(e);process.exit(1)});