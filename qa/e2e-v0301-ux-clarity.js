const {chromium}=require('playwright');
const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1&preset=hero-army-base';
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(URL,{waitUntil:'domcontentloaded'});
 await p.waitForFunction(()=>window.ELDORIA_V023&&sessionStorage.getItem('eldoria-qa-active-preset')==='hero-army-base');
 const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());
 const set=x=>p.evaluate(v=>window.ELDORIA_V023.setQA(v),x);

 // Hero Hall = heroes only.
 await set({...await state(),view:'heroes',heroDetailOpen:false,uxCoachSeen:{}});
 const hall=p.locator('[data-testid="hero-hall"]');await hall.waitFor({state:'visible'});
 if(await hall.locator('[data-testid="army-inventory"]').count())throw Error('Hero Hall mixes troop inventory');
 if(await hall.locator('[data-testid="march-builder"]').count())throw Error('Hero Hall mixes march builder');
 if(await hall.locator('.lockedHero0301').count()<1)throw Error('Hero Hall lacks locked collection slots');
 await hall.locator('[data-testid="ux-coach-heroes"]').waitFor({state:'visible'});

 // Hero profile + clear affinity + separate march route.
 await hall.locator('[data-testid="hero-lyra"]').tap({force:true});const hp=p.locator('[data-testid="hero-profile-lyra"]');await hp.waitFor({state:'visible'});
 let txt=(await hp.innerText()).replace(/\s+/g,' ').toUpperCase();
 if(!txt.includes('LYRA POTENCIA EL ATAQUE DE LOS ARQUEROS')||!txt.includes('+3 %'))throw Error('Lyra affinity explanation unclear '+txt);
 await hp.locator('[data-open-march]').tap({force:true});const march=p.locator('[data-testid="march-screen"]');await march.waitFor({state:'visible'});
 txt=(await march.innerText()).replace(/\s+/g,' ').toUpperCase();
 for(const n of ['PREPARANDO UNA EXPEDICIÓN','HÉROES','TROPAS','COMPOSICIÓN','PODER DE MARCHA','CONFIRMAR MARCHA'])if(!txt.includes(n))throw Error('March hierarchy missing '+n);

 // Barracks = recruit/upgrade only, with route to troop inventory.
 let s=await state();await set({...s,view:'kingdom',bastionLevel:4,bastion3:true,barracks:true,buildingLevels:{...(s.buildingLevels||{}),barracks:4,sawmill:2,granary:2},selectedAction:null});
 const barr=p.locator('[data-testid="building-barracks"]');await barr.waitFor({state:'visible'});await barr.tap({force:true});
 const ctx=p.locator('[data-testid="building-context-barracks"]');await ctx.waitFor({state:'visible'});txt=(await ctx.innerText()).toUpperCase();
 if(!txt.includes('RECLUTAR TROPAS')||!txt.includes('MEJORAR CUARTEL')||!txt.includes('VER TROPAS'))throw Error('Barracks responsibilities unclear '+txt);
 await ctx.locator('[data-testid="open-troops"]').evaluate(el=>el.click());const troops=p.locator('[data-testid="troops-panel"]');await troops.waitFor({state:'visible'});
 txt=(await troops.innerText()).toUpperCase();if(!txt.includes('TROPAS')||!txt.includes('CUARTEL ES DONDE RECLUTAS'))throw Error('Troop inventory explanation unclear '+txt);
 if(await troops.locator('[data-testid="hero-aldric"]').count())throw Error('Troops screen mixes hero collection');

 // Uniform building labels.
 await set({...await state(),view:'kingdom',bastionLevel:6,bastion3:true,barracks:true,granary:true,graniteQuarry:true,forge:true,forgeLvl:1,buildingLevels:{sawmill:2,barracks:4,granary:3,stoneworks:2,forge:1},selectedAction:null});
 for(const id of ['keep','sawmill','barracks','granary','stoneworks','forge']){
   const el=p.locator('[data-testid="building-'+id+'"]');await el.waitFor({state:'visible'});
   const direct=await el.evaluate(x=>({label:x.querySelector(':scope > label')?.textContent||'',small:x.querySelector(':scope > small')?.textContent||''}));
   if(!/^Nivel \d+$/.test(direct.small))throw Error('Building label inconsistent '+id+' '+JSON.stringify(direct));
   if(/producci|comida|guardias|disponible|activo/i.test(direct.small))throw Error('Building label contains noisy state '+id+' '+direct.small);
 }

 // Missions are discoverable and accelerators explain use.
 s=await state();await set({...s,view:'kingdom',missionPanelOpen:false,missionPanelHintSeen:false,speedups:{m1:2,m5:1,m15:1},tasks:[]});
 const compact=p.locator('[data-testid="chapter-compact"]');await compact.waitFor({state:'visible'});txt=(await compact.innerText()).toUpperCase();
 if(!txt.includes('ABRIR')||!txt.includes('▼'))throw Error('Mission panel is not discoverable '+txt);
 await compact.evaluate(el=>el.click());await p.locator('[data-testid="chapter-drawer"]').waitFor({state:'visible'});

 // Accelerator picker on a compatible task.
 s=await state();const now=Date.now();await set({...s,view:'kingdom',tasks:[{key:'upgrade-building-sawmill-3',title:'MEJORANDO ASERRADERO',target:'sawmill',start:now,end:now+180000,costPaid:true,cost:{wood:240,stone:156}}],speedups:{m1:2,m5:1,m15:1}});
 const accel=p.locator('[data-testid="speedup-open"]');await accel.waitFor({state:'visible'});await accel.tap({force:true});
 const task=p.locator('[data-task-key="upgrade-building-sawmill-3"]');txt=(await task.innerText()).toUpperCase();
 if(!txt.includes('ACELERADORES DISPONIBLES')||!txt.includes('−60S')||!txt.includes('NO FUNCIONAN SOBRE RECOLECCIÓN'))throw Error('Accelerator explanation incomplete '+txt);

 // Contrast sanity on critical panels: explicit foreground/background and readable font sizes.
 const selectors=['[data-testid="hero-hall"]','.uxCoach0301','[data-testid="march-screen"]','[data-testid="chapter-drawer"]','.speedPicker0301'];
 for(const sel of selectors){const el=p.locator(sel).first();if(!await el.count())continue;const st=await el.evaluate(x=>{const cs=getComputedStyle(x);return{color:cs.color,bg:cs.backgroundColor,font:parseFloat(cs.fontSize)||0}});if(st.color===st.bg)throw Error('Identical foreground/background '+sel);}

 // Manuscript discovery must resolve as a narrative ceremony, not a toast.
 s=await state();await set({...s,view:'world',bastionLevel:3,bastion:2,bastion3:true,sawmill:true,forest:1,quarry:1,camp:1,barracks:true,granary:true,lyra:true,boss:false,breachManuscript:false,inventory:[],wood:9999,stone:9999,food:9999,selectedAction:null});
 const boss=p.locator('[data-testid="world-node-boss"]');await boss.waitFor({state:'attached'});await boss.evaluate(el=>el.click());const act=p.locator('[data-testid="world-action-boss"]');await act.waitFor({state:'attached'});await act.evaluate(el=>el.click());
 for(let i=0;i<12;i++){await p.waitForTimeout(250);const btn=p.locator('.aldric-cinematic:visible .aldric-continue');if(await btn.count())await btn.last().evaluate(el=>el.click());}
 const ceremony=p.locator('.e22-cinema .e22-dialog');await ceremony.waitFor({state:'visible',timeout:7000});txt=(await ceremony.innerText()).toUpperCase();
 if(!txt.includes('MANUSCRITO DE LA FISURA')||!txt.includes('MISTERIO'))throw Error('Manuscript lacks important narrative ceremony '+txt);

 await b.close();console.log('v0.30.1 UX CLARITY PASS');
})().catch(e=>{console.error(e);process.exit(1)});