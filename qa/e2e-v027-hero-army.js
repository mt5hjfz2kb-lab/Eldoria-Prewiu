const {chromium}=require('playwright');
const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1&preset=hero-army-base';
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(URL,{waitUntil:'domcontentloaded'});
 await p.waitForFunction(()=>window.ELDORIA?.heroArmy&&window.ELDORIA_V023?.state().view==='heroes');
 const result=await p.evaluate(()=>{
   const H=window.ELDORIA.heroArmy,C=window.ELDORIA.gameplay.combat,s=window.ELDORIA_V023.state();
   const synthetic={archer:{attack:100,defense:100,health:100,break:100,power:100},paladin:{attack:100,defense:100,health:100,break:100,power:100}};
   const mixed=H.buildMarch({heroIds:['aldric','lyra'],troops:{archer:10,paladin:10},bastionLevel:4,profileOverrides:synthetic});
   const aldricArchers=H.buildMarch({heroIds:['aldric'],troops:{archer:10},bastionLevel:4,profileOverrides:synthetic});
   const lyraPaladins=H.buildMarch({heroIds:['lyra'],troops:{paladin:10},bastionLevel:4,profileOverrides:synthetic});
   const lyraCombat=C.playerStats({troops:10,troopComposition:{archer:10,paladin:0},bastionLevel:4,hero:'lyra',heroes:['lyra']});
   return{
     heroes:H.HEROES,troops:H.TROOPS,ranks:H.SKILL_RANKS,
     mixed,aldricArchers,lyraPaladins,lyraCombat,state:s
   };
 });
 if(result.ranks.join(',')!=='I,II,III,IV,V')throw Error('skill rank scale missing');
 const a=result.heroes.aldric,l=result.heroes.lyra;
 if(a.role.id!=='tank'||a.affinity.troopType!=='paladin'||a.affinity.stat!=='defense'||a.affinity.modifier!==0.03)throw Error('Aldric role/affinity contract wrong');
 if(l.role.id!=='dps'||l.affinity.troopType!=='archer'||l.affinity.stat!=='attack'||l.affinity.modifier!==0.03)throw Error('Lyra role/affinity contract wrong');
 for(const h of [a,l]){
   if(h.skills.pve.length!==2||h.skills.pvp.length!==2)throw Error('hero skill slot count wrong');
   for(const slot of [...h.skills.pve,...h.skills.pvp])if(slot.maxRank!==5)throw Error('hero skill max rank wrong');
   if(!h.talents.tiers.length||h.talents.tiers.some(t=>t.exclusive!==true))throw Error('exclusive talent decision seam missing');
 }
 if(!result.troops.archer.profile||result.troops.archer.playerFacing!==true)throw Error('archer own stats/profile missing');
 if(result.troops.paladin.status!=='reserved'||result.troops.paladin.playerFacing!==false||result.troops.paladin.profile!==null)throw Error('paladin must stay reserved until balanced');
 if(result.mixed.families.archer.stats.attack!==103||result.mixed.families.archer.stats.defense!==100)throw Error('Lyra affinity did not affect archers only');
 if(result.mixed.families.paladin.stats.defense!==103||result.mixed.families.paladin.stats.attack!==100)throw Error('Aldric affinity did not affect paladins only');
 if(result.aldricArchers.affinities.length!==0||result.lyraPaladins.affinities.length!==0)throw Error('affinity incorrectly restricts/nonmatching troops');
 if(result.mixed.errors.length)throw Error('mixed configurable march failed '+result.mixed.errors.join(','));
 if(result.lyraCombat.troopType!=='archer'||result.lyraCombat.affinities.length!==1)throw Error('combat did not consume new march affinity model');
 if(result.state.maelis!==false||result.state.troopRoster.archer!==60||result.state.troopRoster.paladin!==0)throw Error('focused preset leaked future content or wrong roster');
 if(result.state.marchSetup.heroIds.join(',')!=='aldric,lyra')throw Error('march base state missing configured heroes');
 await p.locator('[data-testid="march-builder"]').waitFor({state:'visible'});
 await b.close();console.log('v0.27 HERO + TROOP + MARCH BASE PASS');
})().catch(e=>{console.error(e);process.exit(1)});
