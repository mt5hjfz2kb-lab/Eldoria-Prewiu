const {chromium}=require('playwright');
const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1&preset=b6-forge';
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 const goto=async preset=>{await p.goto((process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/')+'?qa=1&preset='+preset,{waitUntil:'domcontentloaded'});await p.waitForFunction(id=>window.ELDORIA_V023&&sessionStorage.getItem('eldoria-qa-active-preset')===id,preset)};
 const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());
 const set=x=>p.evaluate(v=>window.ELDORIA_V023.setQA(v),x);
 const close=async()=>{for(let i=0;i<8;i++){const x=p.locator('.e22-overlay .btn:visible,.aldric-cinematic:visible .aldric-continue');if(!await x.count())break;await x.last().evaluate(el=>el.click());await p.waitForTimeout(60)}};

 // Bastion II: a valid expedition can be prepared with Aldric alone.
 await goto('b2-spawnling');let s=await state();await set({...s,view:'march',marchSetup:{heroIds:['aldric'],troops:s.troopRoster},marchSlots:['aldric'],marchConfigured:false});
 let confirm=p.locator('[data-testid="march-confirm"]');await confirm.waitFor({state:'visible'});
 if(await confirm.isDisabled())throw Error('Bastion II still requires an unavailable second hero');
 let marchText=(await p.locator('[data-testid="march-screen"]').innerText()).replace(/\s+/g,' ');
 if(/al menos dos héroes/i.test(marchText))throw Error('Bastion II presents impossible two-hero guidance');
 await confirm.tap();if(!(await state()).marchConfigured)throw Error('Aldric-only Bastion II march did not save');

 // Lyra affinity is sourced consistently and never exposes the internal "archer" key.
 await goto('hero-army-base');await set({...await state(),view:'march'});
 const march=p.locator('[data-testid="march-screen"]');await march.waitFor({state:'visible'});marchText=(await march.innerText()).replace(/\s+/g,' ');
 if(!/Lyra potencia el Ataque de Arqueros en \+3 %/i.test(marchText))throw Error('Lyra march affinity is inconsistent: '+marchText);
 if(/\barcher\b/i.test(marchText))throw Error('Internal troop key leaked into Spanish UI: '+marchText);

 // Bastion III: decorative Bestias label cannot steal the boar hitbox.
 await goto('b3-lyra');s=await state();await set({...s,view:'world',boss:false,boars:0,boarRespawnAt:0,selectedAction:null});
 const boar=p.locator('[data-testid="world-node-boar"]');await boar.waitFor({state:'visible'});
 await p.evaluate(()=>{const wp=document.querySelector('[data-worldpan]'),b=document.querySelector('[data-testid="world-node-boar"]');if(!wp||!b)return;wp.style.transform='translate3d(0px,0px,0px)';const r=b.getBoundingClientRect(),dx=Math.round(innerWidth*.5-(r.left+r.width/2)),dy=Math.round(innerHeight*.48-(r.top+r.height/2));wp.style.transform='translate3d('+dx+'px,'+dy+'px,0px)'});
 await p.waitForTimeout(80);const box=await boar.boundingBox();if(!box)throw Error('Boar has no hitbox');
 const ghostAudit=await p.evaluate(()=>[...document.querySelectorAll('.ghost')].map(g=>({text:(g.textContent||'').trim(),pointer:getComputedStyle(g).pointerEvents})));if(ghostAudit.some(g=>g.pointer!=='none'))throw Error('Decorative world label can intercept taps: '+JSON.stringify(ghostAudit));
 const topAtCenter=await p.evaluate(({x,y})=>{const el=document.elementFromPoint(x,y);return el&&({node:el.closest('[data-node]')?.dataset.node||null,cls:el.className||'',text:(el.textContent||'').trim().slice(0,80)})},{x:box.x+box.width/2,y:box.y+box.height/2});
 if(topAtCenter?.node!=='boar')throw Error('Boar center is intercepted: '+JSON.stringify(topAtCenter));
 await boar.tap({position:{x:box.width/2,y:box.height/2}});await p.locator('[data-testid="world-action-boar"]').waitFor({state:'visible'});

 // Bastion VI: Forge action remains reachable even beside Granary.
 await goto('b6-forge');const forge=p.locator('[data-testid="building-forge"]');await forge.waitFor({state:'visible'});await forge.tap();
 const forgeAction=p.locator('[data-testid="building-action-forge"]');await forgeAction.waitFor({state:'visible'});
 const fbox=await forgeAction.boundingBox();if(!fbox)throw Error('Forge action has no hitbox');
 const ftop=await p.evaluate(({x,y})=>document.elementFromPoint(x,y)?.closest('[data-testid="building-action-forge"]')?.getAttribute('data-testid')||null,{x:fbox.x+fbox.width/2,y:fbox.y+fbox.height/2});
 if(ftop!=='building-action-forge')throw Error('Forge action center is intercepted');
 await forgeAction.tap();await p.waitForFunction(()=>window.ELDORIA_V023.state().tasks.some(t=>t.key==='build-forge'),null,{timeout:4000});

 // Devorador uses modeled preparation/forecast and never leaks invalid data.
 s=await state();await set({...s,view:'world',forge:true,forgeLvl:1,buildingLevels:{...(s.buildingLevels||{}),forge:1},devourerDefeated:false,aetherEmber:false,selectedAction:null,tasks:[],combatMarch:{troops:82,hero:'lyra'},lyra:true});
 const dev=p.locator('[data-testid="world-node-devourer"]');await dev.waitFor({state:'visible'});await dev.tap({force:true});const act=p.locator('[data-testid="world-action-devourer"]');await act.waitFor({state:'visible'});await act.tap();
 const prep=p.locator('[data-testid="march-prep-devourer"]');await prep.waitFor({state:'visible'});let t=(await prep.innerText()).replace(/\s+/g,' ');
 for(const need of ['PODER','PREVISIÓN','CAPARAZÓN DE ÉTER','ATAQUE','DEFENSA','RUPTURA'])if(!t.toUpperCase().includes(need))throw Error('Devourer prep missing '+need+': '+t);
 if(/undefined|null|\barcher\b/i.test(t))throw Error('Technical token leaked in Devourer prep: '+t);
 await prep.locator('[data-testid="combat-launch-devourer"]').tap();const report=p.locator('[data-testid="battle-report"]');await report.waitFor({state:'visible'});t=(await report.innerText()).replace(/\s+/g,' ');
 for(const need of ['PARTICIPANTES','DAÑO INFLIGIDO','¿POR QUÉ OCURRIÓ?','CAPARAZÓN DE ÉTER'])if(!t.toUpperCase().includes(need))throw Error('Devourer report missing '+need+': '+t);
 if(/undefined|null/i.test(t))throw Error('Technical token leaked in Devourer report: '+t);
 await report.locator('[data-battle-close]').tap();await close();

 // Exact reported regression: an equipped forged weapon remains 1/1 and unlocks Bastion VII.
 s=await state();await set({...s,view:'kingdom',wood:9999,stone:9999,food:9999,bastionLevel:6,devourerDefeated:true,aetherEmber:false,inventory:[],equipped:{...s.equipped,aldric:{...(s.equipped?.aldric||{}),weapon:{id:'aether-blade-regression',name:'Hoja de Éter',slot:'weapon',power:180}}},chapterProgress:{...(s.chapterProgress||{}),current:6,completedMissions:{...((s.chapterProgress||{}).completedMissions||{}),'c6-forge':true,'c6-devourer':true,'c6-expedition':true,'c6-gear':true}}});
 if(!await p.evaluate(()=>window.ELDORIA.heroArmy.ownedEquipment(window.ELDORIA_V023.state(),'weapon')))throw Error('Equipped weapon is not canonical ownership');
 const keep=p.locator('[data-testid="building-keep"]');await keep.waitFor({state:'visible'});await keep.tap();const up=p.locator('[data-testid="building-action-keep"]');await up.waitFor({state:'visible'});await up.tap();
 const blocked=p.locator('.e22-overlay',{hasText:'completa el ciclo de Bastión VI'});if(await blocked.count())throw Error('Bastion VII gate still rejects equipped forged weapon');

 await b.close();console.log('v0.30.2 ARC I STABILITY PASS');
})().catch(e=>{console.error(e);process.exit(1)});
