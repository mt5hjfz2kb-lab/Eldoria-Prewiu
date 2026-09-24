const {chromium}=require('playwright');
const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1';
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(URL,{waitUntil:'domcontentloaded'});
 // Detailed tier/cost regression belongs to an advanced Barracks state.
 // Barracks Nv.1 intentionally uses the simplified v0.30.4 onboarding layout.
 await p.evaluate(()=>window.ELDORIA_V023.setQA({introSeen:true,view:'kingdom',bastion:2,bastion3:true,bastionLevel:4,barracks:true,granary:true,buildingLevels:{sawmill:2,barracks:4,granary:2,stoneworks:0,forge:0},wood:9999,stone:9999,food:9999,tasks:[],selectedAction:'barracks',recruitQty:5,recruitTier:2}));
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
 await b.close();console.log('v0.27 BARRACKS RECRUITMENT DETAILS PASS');
})().catch(e=>{console.error(e);process.exit(1)});