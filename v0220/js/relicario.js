/* Eldoria v0.31 Relicario domain. Canonical collection/drop/effect rules. */
(()=>{'use strict';const E=window.ELDORIA=window.ELDORIA||{};
const CATALOG=[
{id:'bosque-valoria',name:'Bosque de Valoria',rarity:'Común',values:{N:6,S:3,E:4,O:2},glyph:'🌲',use:'Obtiene inmediatamente madera equivalente a 20 minutos de tu producción actual.',duel:null,unlock:s=>!!s.sawmill,effect:{kind:'production',resource:'wood',seconds:1200}},
{id:'cantera-valoria',name:'Cantera de Valoria',rarity:'Común',values:{N:3,S:6,E:2,O:4},glyph:'⛏️',use:'Obtiene inmediatamente piedra equivalente a 20 minutos de tu producción actual.',duel:null,unlock:s=>!!s.graniteQuarry,effect:{kind:'production',resource:'stone',seconds:1200}},
{id:'granero-valoria',name:'Granero de Valoria',rarity:'Común',values:{N:4,S:3,E:6,O:2},glyph:'🌾',use:'Obtiene inmediatamente comida equivalente a 20 minutos de tu producción actual.',duel:null,unlock:s=>!!s.granary,effect:{kind:'production',resource:'food',seconds:1200}},
{id:'arqueros-valoria-t1',name:'Arqueros de Valoria T1',rarity:'Común',values:{N:6,S:2,E:5,O:3},glyph:'🏹',use:'Entrega inmediatamente 20 Arqueros T1.',duel:null,unlock:s=>!!s.barracks,effect:{kind:'archers',tier:1,qty:20}},
{id:'engendro-fisura',name:'Engendro de la Fisura',rarity:'Común',values:{N:2,S:7,E:3,O:2},glyph:'✦',use:'La próxima victoria contra un enemigo de la Brecha entrega +10 % de recompensas.',duel:null,unlock:s=>Number(s.chapterProgress?.counters?.wins?.spawnling||0)>0,effect:{kind:'nextReward',scope:'breach',pct:10}},
{id:'corrupto-nv1',name:'Corrupto Nv.1',rarity:'Común',values:{N:2,S:4,E:7,O:2},glyph:'☠️',use:'La próxima expedición PvE cuesta 15 % menos de energía. El efecto desaparece al iniciar dicha expedición.',duel:null,unlock:s=>!!s.camp,effect:{kind:'nextPveEnergy',pct:15}},
{id:'acechador-ceniza',name:'Acechador de Ceniza',rarity:'Rara',values:{N:8,S:3,E:5,O:3},glyph:'🜂',use:'La próxima victoria contra un enemigo poco común entrega +25 % de recompensas.',duel:null,unlock:s=>Number(s.chapterProgress?.counters?.wins?.ashStalker||0)>0,effect:{kind:'nextReward',scope:'uncommon',pct:25}},
{id:'corrupto-nv15',name:'Corrupto Nv.15',rarity:'Rara',values:{N:3,S:8,E:5,O:3},glyph:'☣',use:'+25 % de capacidad de carga de expediciones durante exactamente 2 horas.',duel:null,unlock:s=>!!s.corrupt15Discovered,effect:{kind:'timed',key:'expeditionCapacityPct',value:25,duration:7200000}},
{id:'forja-valoria',name:'Forja de Valoria',rarity:'Rara',values:{N:5,S:4,E:7,O:3},glyph:'🔥',use:'Reduce inmediatamente 25 % del tiempo restante de una tarea activa de Forja.',duel:null,unlock:s=>!!s.forge,effect:{kind:'forgeRemaining',pct:25}},
{id:'estandarte-valoria',name:'Estandarte de Valoria',rarity:'Rara',values:{N:6,S:5,E:3,O:6},glyph:'⚑',use:'+20 % de velocidad de marcha durante exactamente 2 horas.',duel:null,unlock:s=>!!s.marchConfigured,effect:{kind:'timed',key:'marchSpeedPct',value:20,duration:7200000}},
{id:'sir-aldric',name:'Sir Aldric',rarity:'Épica',values:{N:8,S:6,E:4,O:5},glyph:'🛡️',use:'+30 % de defensa de expedición durante exactamente 4 horas.',duel:{name:'Guardia',copy:'Mientras Sir Aldric permanezca en mesa: +2 al lado izquierdo.'},unlock:s=>true,effect:{kind:'timed',key:'expeditionDefensePct',value:30,duration:14400000}},
{id:'lyra',name:'Lyra',rarity:'Épica',values:{N:5,S:4,E:9,O:5},glyph:'🏹',use:'+35 % de velocidad de entrenamiento de Arqueros durante exactamente 4 horas.',duel:null,unlock:s=>!!s.lyra,effect:{kind:'timed',key:'archerTrainingSpeedPct',value:35,duration:14400000}},
{id:'primera-brecha',name:'La Primera Brecha',rarity:'Épica',values:{N:6,S:8,E:5,O:5},glyph:'✦',use:'+35 % a las recompensas obtenidas contra enemigos de la Brecha durante exactamente 4 horas.',duel:{name:'Distorsión',copy:'Al jugarla reduce en 1 un lado aleatorio de una carta rival válida, nunca por debajo de 1.'},unlock:s=>!!s.finalWon,effect:{kind:'timed',key:'breachRewardPct',value:35,duration:14400000}},
{id:'corona-valoria',name:'Corona de Valoria',rarity:'Legendaria',values:{N:8,S:7,E:6,O:7},glyph:'♛',use:'+50 % de velocidad de construcción durante exactamente 4 horas.',duel:null,unlock:s=>!!s.finalWon,effect:{kind:'timed',key:'constructionSpeedPct',value:50,duration:14400000}},
{id:'heraldo-fisura',name:'Heraldo de la Fisura',rarity:'Legendaria',values:{N:7,S:9,E:8,O:6},glyph:'ϟ',use:'Durante exactamente 4 horas: +50 % de recompensas contra enemigos de la Brecha y +25 % de capacidad de carga de expediciones.',duel:{name:'Corrupción',copy:'Al enfrentarse directamente, intercambia Este y Oeste de la carta rival durante ese enfrentamiento.'},unlock:s=>Number(s.chapterProgress?.counters?.wins?.herald||0)>0,effect:{kind:'multiTimed',effects:[['breachRewardPct',50],['expeditionCapacityPct',25]],duration:14400000}}
];
const BY_ID=Object.fromEntries(CATALOG.map(x=>[x.id,x]));
const RARITY_CLASS={'Común':'common','Rara':'rare','Épica':'epic','Legendaria':'legendary'};
const DROP={common:.001,rare:.0005};
const INDESTRUCTIBLE={'Común':.0001,'Rara':.00005,'Épica':.00001,'Legendaria':.000001};
const legacyMap={'ash-sigil':'bosque-valoria','rift-shard':'engendro-fisura','ash-veil':'acechador-ceniza','valoria-dawn':'corona-valoria'};
const clone=x=>JSON.parse(JSON.stringify(x));
function normalize(s){
 if(!s.relicCollection||typeof s.relicCollection!=='object'||Array.isArray(s.relicCollection))s.relicCollection={};
 if(!s.relicDiscovered||typeof s.relicDiscovered!=='object')s.relicDiscovered={};
 if(!s.relicConsumed||typeof s.relicConsumed!=='object'||Array.isArray(s.relicConsumed))s.relicConsumed={};
 if(!s.relicEffects||typeof s.relicEffects!=='object')s.relicEffects={};
 if(!s.relicPoolUnlocked||typeof s.relicPoolUnlocked!=='object')s.relicPoolUnlocked={};
 if(!s.relicTutorial||typeof s.relicTutorial!=='object')s.relicTutorial={intro:false,rarity:false,sides:false,use:false,indestructible:false,duel:false};
 if(!Array.isArray(s.relicRevealQueue))s.relicRevealQueue=[];
 if(!s.relicV031Migrated){
  for(const old of (s.codex||[])){const id=legacyMap[old?.id];if(!id||!BY_ID[id])continue;const slot=s.relicCollection[id]||(s.relicCollection[id]={normal:0,indestructible:false});if((old.quality||'').toLowerCase()==='indestructible')slot.indestructible=true;else slot.normal++;s.relicDiscovered[id]=true}
  for(const old of (s.cardsConsumed||[])){const id=legacyMap[old]||old;if(BY_ID[id]){s.relicDiscovered[id]=true;s.relicConsumed[id]=(s.relicConsumed[id]||0)+1}}
  s.codex=[];s.cardsConsumed=[];s.relicV031Migrated=true;
 }
 for(const id of Object.keys(s.relicCollection)){let x=s.relicCollection[id];if(!x||typeof x!=='object')x=s.relicCollection[id]={normal:0,indestructible:false};x.normal=Math.max(0,Math.floor(Number(x.normal)||0));x.indestructible=!!x.indestructible;if(x.normal||x.indestructible)s.relicDiscovered[id]=true}
 refreshPool(s);return s;
}
function refreshPool(s){for(const c of CATALOG)s.relicPoolUnlocked[c.id]=!!c.unlock(s);return s.relicPoolUnlocked}
function card(id){return BY_ID[id]||null}
function unlocked(s,rarity){normalize(s);return CATALOG.filter(c=>c.rarity===rarity&&s.relicPoolUnlocked[c.id])}
function discoverCount(s){normalize(s);return Object.keys(s.relicDiscovered).filter(id=>s.relicDiscovered[id]&&BY_ID[id]).length}
function ownedCount(s,id){normalize(s);let x=s.relicCollection[id];return x?(x.normal||0)+(x.indestructible?1:0):0}
function owned(s){normalize(s);return CATALOG.filter(c=>ownedCount(s,c.id)>0).map(c=>({...c,normal:s.relicCollection[c.id].normal||0,indestructible:!!s.relicCollection[c.id].indestructible,consumed:s.relicConsumed[c.id]||0}))}
function discovered(s){normalize(s);return CATALOG.filter(c=>s.relicDiscovered[c.id])}
function add(s,id,{indestructible=false,source='unknown',queue=true}={}){normalize(s);const c=BY_ID[id];if(!c)return null;const slot=s.relicCollection[id]||(s.relicCollection[id]={normal:0,indestructible:false});if(indestructible){if(slot.indestructible)return null;slot.indestructible=true}else slot.normal++;s.relicDiscovered[id]=true;refreshPool(s);const result={...clone(c),indestructible:!!indestructible,source};if(queue)s.relicRevealQueue.push(result);return result}
function choose(list,rng=Math.random){if(!list.length)return null;return list[Math.min(list.length-1,Math.floor(Math.max(0,Math.min(.999999999,Number(rng())||0))*list.length))]}
function rollIndestructible(s,c,rng=Math.random){const chance=INDESTRUCTIBLE[c.rarity]||0;if((Number(rng())||0)>=chance)return{card:c,indestructible:false};if(!s.relicCollection[c.id]?.indestructible)return{card:c,indestructible:true};const alt=unlocked(s,c.rarity).filter(x=>!s.relicCollection[x.id]?.indestructible);if(alt.length)return{card:choose(alt,rng),indestructible:true};return{card:c,indestructible:false,duplicateFallback:true}}
function rollEnemyDrop(s,{source='world-enemy',rng=Math.random}={}){normalize(s);const r=Number(rng())||0;let rarity=null;if(r<DROP.rare)rarity='Rara';else if(r<DROP.rare+DROP.common)rarity='Común';else return null;const pool=unlocked(s,rarity);if(!pool.length)return null;const ind=rollIndestructible(s,choose(pool,rng),rng);return add(s,ind.card.id,{indestructible:ind.indestructible,source,queue:true})}
function grantIntro(s){normalize(s);if(s.relicTutorialIntroGranted)return null;s.relicTutorialIntroGranted=true;return add(s,'bosque-valoria',{source:'relicario-intro',queue:true})}
function grantEpicRandom(s,{source='future-epic-reward',rng=Math.random}={}){normalize(s);const pool=unlocked(s,'Épica');if(!pool.length)return null;const ind=rollIndestructible(s,choose(pool,rng),rng);return add(s,ind.card.id,{indestructible:ind.indestructible,source,queue:true})}
function modifiers(s,now=Date.now()){normalize(s);const out={};for(const [k,e] of Object.entries(s.relicEffects)){if(e&&Number(e.endsAt)>now)out[k]=Number(e.value)||0}return out}
function setTimed(s,key,value,duration,now=Date.now()){s.relicEffects[key]={value,startedAt:now,endsAt:now+duration}}
function applyUse(s,id,ctx={}){normalize(s);const c=BY_ID[id],slot=s.relicCollection[id];if(!c||!slot||(!slot.normal&&!slot.indestructible))return{ok:false,reason:'missing'};let applied=true,detail=c.use,now=ctx.now||Date.now(),e=c.effect;
 if(e.kind==='production'){const rate=(E.economy?.passiveRates(s)||{})[e.resource]||0,qty=Math.max(0,Math.floor(rate*e.seconds));s[e.resource]=(s[e.resource]||0)+qty;detail='+'+qty+' '+e.resource}
 else if(e.kind==='archers'){s.troopRoster=E.heroArmy.normalizeRoster(s.troopRoster,s.troops||0);s.troopRoster.archer[e.tier]=(s.troopRoster.archer[e.tier]||0)+e.qty;s.troops=E.heroArmy.totalFamily(s.troopRoster,'archer');detail='+'+e.qty+' Arqueros T'+e.tier}
 else if(e.kind==='nextReward')s.relicEffects[e.scope==='uncommon'?'nextUncommonRewardPct':'nextBreachRewardPct']={value:e.pct,uses:1,startedAt:now};
 else if(e.kind==='nextPveEnergy')s.relicEffects.nextPveEnergyDiscountPct={value:e.pct,uses:1,startedAt:now};
 else if(e.kind==='forgeRemaining'){const task=(s.tasks||[]).find(t=>t&&String(t.key||'').includes('forge')&&Number(t.end)>now);if(!task){applied=false;detail='No hay una tarea activa de Forja.'}else{const remaining=Math.max(0,task.end-now),cut=Math.floor(remaining*e.pct/100);task.end=Math.max(now,task.end-cut);detail='-'+Math.ceil(cut/1000)+'s de Forja'}}
 else if(e.kind==='timed')setTimed(s,e.key,e.value,e.duration,now);
 else if(e.kind==='multiTimed')for(const [key,value] of e.effects)setTimed(s,key,value,e.duration,now);
 if(!applied)return{ok:false,reason:'condition',detail};
 const ind=!!slot.indestructible;if(!ind){slot.normal--;s.relicConsumed[id]=(s.relicConsumed[id]||0)+1}
 s.relicDiscovered[id]=true;return{ok:true,indestructible:ind,detail,card:clone(c)}
}
function victoryRewardMultiplier(s,enemy){normalize(s);let pct=0,mods=modifiers(s);if(enemy?.kind!=='hunt'&&mods.breachRewardPct)pct+=mods.breachRewardPct;if(enemy?.kind!=='hunt'&&s.relicEffects.nextBreachRewardPct?.uses>0){pct+=Number(s.relicEffects.nextBreachRewardPct.value)||0;s.relicEffects.nextBreachRewardPct.uses--;if(s.relicEffects.nextBreachRewardPct.uses<=0)delete s.relicEffects.nextBreachRewardPct}if(enemy?.kind==='uncommon'&&s.relicEffects.nextUncommonRewardPct?.uses>0){pct+=Number(s.relicEffects.nextUncommonRewardPct.value)||0;s.relicEffects.nextUncommonRewardPct.uses--;if(s.relicEffects.nextUncommonRewardPct.uses<=0)delete s.relicEffects.nextUncommonRewardPct}return 1+pct/100}
function trainingSeconds(s,seconds){const pct=modifiers(s).archerTrainingSpeedPct||0;return Math.max(1,Math.ceil(seconds/(1+pct/100)))}
function constructionSeconds(s,seconds,key=''){if(!/^build-|^upgrade-|^rebuild-|^upgrade-bastion-/.test(String(key)))return seconds;const pct=modifiers(s).constructionSpeedPct||0;return Math.max(1,Math.ceil(seconds/(1+pct/100)))}
function marchDuration(s,ms=700){const pct=modifiers(s).marchSpeedPct||0;return Math.max(220,Math.round(ms/(1+pct/100)))}
function expeditionDefense(s,defense){const pct=modifiers(s).expeditionDefensePct||0;return Math.round(defense*(1+pct/100))}
E.relics={CATALOG,BY_ID,RARITY_CLASS,DROP,INDESTRUCTIBLE,normalize,refreshPool,card,unlocked,discoverCount,ownedCount,owned,discovered,add,rollEnemyDrop,grantIntro,grantEpicRandom,applyUse,modifiers,victoryRewardMultiplier,trainingSeconds,constructionSeconds,marchDuration,expeditionDefense};
})();