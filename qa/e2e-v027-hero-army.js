const {chromium}=require('playwright');
const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1&preset=hero-army-base';
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(URL,{waitUntil:'domcontentloaded'});
 await p.waitForFunction(()=>window.ELDORIA?.heroArmy&&sessionStorage.getItem('eldoria-qa-active-preset')==='hero-army-base'&&window.ELDORIA_V023?.state().view==='heroes'&&!window.ELDORIA_V023.state().heroDetailOpen);
 const data=await p.evaluate(()=>{
   const H=window.ELDORIA.heroArmy,s=window.ELDORIA_V023.state();
   const roster=H.normalizeRoster(s.troopRoster,s.troops);
   const base=H.tierStats('archer',1,[],roster);
   const lyra=H.tierStats('archer',1,['lyra'],roster);
   const aldric=H.tierStats('archer',1,['aldric'],roster);
   const mixed=H.buildMarch({heroIds:['aldric','lyra'],troops:{archer:{1:40,2:60,3:0}}});
   const aldricArchers=H.buildMarch({heroIds:['aldric'],troops:{archer:{1:40,2:0,3:0}}});
   const paladinAffinity=H.affinityBonuses(['aldric'],{archer:{1:0,2:0,3:0},paladin:{1:20}});
   const promoted=H.promoteTroops({archer:{1:140,2:60,3:0}},'archer',1,2,20);
   return{
     heroes:H.HEROES,
     ranks:H.SKILL_RANKS,
     unlocks:[H.unlockedTiers(1),H.unlockedTiers(4),H.unlockedTiers(10)],
     tiers:H.ARCHER_TIERS,
     base,lyra,aldric,mixed,aldricArchers,paladinAffinity,promoted,
     roster,state:s
   };
 });
 if(data.ranks.join(',')!=='I,II,III,IV,V')throw Error('skill rank scale missing');
 const a=data.heroes.aldric,l=data.heroes.lyra;
 if(a.role.id!=='tank'||a.affinity.troopType!=='paladin'||a.affinity.stat!=='defense'||a.affinity.modifier!==0.03)throw Error('Aldric role/affinity contract wrong');
 if(l.role.id!=='dps'||l.affinity.troopType!=='archer'||l.affinity.stat!=='attack'||l.affinity.modifier!==0.03)throw Error('Lyra role/affinity contract wrong');
 for(const h of [a,l]){
   if(h.skills.pve.length!==2||h.skills.pvp.length!==2)throw Error('hero skill slot count wrong');
   for(const slot of [...h.skills.pve,...h.skills.pvp])if(slot.maxRank!==5)throw Error('hero skill max rank wrong');
   if(h.talents.mode!=='exclusive-choice'||!Array.isArray(h.talents.choices))throw Error('exclusive talent seam missing');
   if(!Array.isArray(h.equipment.slots)||h.equipment.slots.length<4)throw Error('flexible equipment slots missing');
 }
 if(data.unlocks[0].join(',')!=='1'||data.unlocks[1].join(',')!=='1,2'||data.unlocks[2].join(',')!=='1,2,3')throw Error('barracks tier gates wrong '+JSON.stringify(data.unlocks));
 if(!data.tiers[1].stats||!data.tiers[2].stats||!data.tiers[3].stats)throw Error('archer tier stats missing');
 if(data.lyra.attack!==data.base.attack*1.03||data.lyra.defense!==data.base.defense)throw Error('Lyra affinity must affect archer attack only');
 if(data.aldric.attack!==data.base.attack||data.aldric.defense!==data.base.defense)throw Error('Aldric must not modify archers');
 if(data.paladinAffinity.length!==1||data.paladinAffinity[0].troopType!=='paladin'||data.paladinAffinity[0].stat!=='defense')throw Error('Aldric paladin affinity seam wrong');
 if(!data.aldricArchers.valid)throw Error('affinity incorrectly restricts Aldric + archers');
 if(!data.promoted.ok||data.promoted.roster.archer[1]!==120||data.promoted.roster.archer[2]!==80)throw Error('manual troop promotion seam wrong');
 if(data.roster.archer[1]!==140||data.roster.archer[2]!==60||data.roster.archer[3]!==0)throw Error('tiered roster fixture wrong');
 if(data.mixed.heroes.length!==2||data.mixed.stats.power<=0)throw Error('mixed march model invalid');
 await p.locator('[data-testid="hero-hall"]').waitFor({state:'visible'});
 await p.locator('[data-testid="army-inventory"]').waitFor({state:'visible'});
 for(const id of ['army-archer-t1','army-archer-t2','army-archer-t3'])await p.locator('[data-testid="'+id+'"]').waitFor({state:'visible'});
 await p.locator('[data-testid="hero-aldric"]').tap({force:true});
 await p.locator('[data-testid="hero-profile-aldric"]').waitFor({state:'visible'});
 let txt=(await p.locator('[data-testid="hero-profile-aldric"]').innerText()).replace(/\s+/g,' ');
 for(const needle of ['SIR ALDRIC','PODER','TANQUE','PALADINES','HABILIDADES','TALENTOS'])if(!txt.toUpperCase().includes(needle))throw Error('Aldric profile missing '+needle);
 await p.locator('[data-hero-tab="talents"]').tap({force:true});
 if(!(await p.locator('.heroTabBody027').innerText()).toUpperCase().includes('EXCLUYENTE'))throw Error('talent choice structure not visible');
 await p.locator('[data-hero-back]').tap({force:true});
 await p.locator('[data-testid="hero-hall"]').waitFor({state:'visible'});
 await p.locator('[data-testid="hero-lyra"]').tap({force:true});
 await p.locator('[data-testid="hero-profile-lyra"]').waitFor({state:'visible'});
 txt=(await p.locator('[data-testid="hero-profile-lyra"]').innerText()).replace(/\s+/g,' ').toUpperCase();
 for(const needle of ['LYRA','DPS','ARQUEROS','+3% ATQ'])if(!txt.includes(needle))throw Error('Lyra profile missing '+needle);
 const overflow=await p.locator('[data-testid="hero-profile-lyra"]').evaluate(el=>({w:el.scrollWidth,cw:el.clientWidth,h:el.scrollHeight,ch:el.clientHeight}));
 if(overflow.w>overflow.cw+3)throw Error('hero profile horizontal overflow on mobile '+JSON.stringify(overflow));
 await b.close();console.log('v0.27 HERO + TROOP + MARCH BASE PASS');
})().catch(e=>{console.error(e);process.exit(1)});
