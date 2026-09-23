/* Eldoria v0.27 military domain: heroes, troop tiers, affinities and march composition. */
(function(root,factory){const api=factory();if(typeof module==='object'&&module.exports)module.exports=api;if(root){root.ELDORIA=root.ELDORIA||{};root.ELDORIA.heroArmy=api;}})(typeof window!=='undefined'?window:null,function(){
'use strict';
const SKILL_RANKS=['I','II','III','IV','V'];
const emptySkill=(slot)=>({slot,id:null,name:null,rank:1,maxRank:5,status:'reserved',effects:[],requirements:[],costs:[]});
const skillTrack=(first=null)=>[first?{slot:1,rank:1,maxRank:5,status:'active',effects:[],requirements:[],costs:[],...first}:emptySkill(1),emptySkill(2)];
const HEROES={
 aldric:{id:'aldric',name:'Sir Aldric',role:{id:'tank',name:'Tanque'},affinity:{troopType:'paladin',label:'Paladines',stat:'defense',modifier:0.03,activeWhen:'paladin'},statsSource:'existing-v0266-combat',baseStats:{attack:78,defense:112,health:420,break:34},skills:{pve:skillTrack({id:'bulwark',name:'Baluarte',effectId:'next_hit_mitigation'}),pvp:skillTrack()},talents:{mode:'exclusive-choice',choices:[]},equipment:{slots:['weapon','armor','head','accessory']}},
 lyra:{id:'lyra',name:'Lyra',role:{id:'dps',name:'DPS'},affinity:{troopType:'archer',label:'Arqueros',stat:'attack',modifier:0.03,activeWhen:'archer'},statsSource:'existing-v0266-combat',baseStats:{attack:116,defense:62,health:300,break:82},skills:{pve:skillTrack({id:'piercingShot',name:'Disparo de Ruptura',effectId:'direct_damage_defense_break'}),pvp:skillTrack()},talents:{mode:'exclusive-choice',choices:[]},equipment:{slots:['weapon','armor','head','accessory']}}
};
const legacyProfile=t=>({attack:18+t*2,defense:11+t,health:34+t*3,break:9+t*2,power:24+t*3});
const ARCHER_TIERS={
 1:{tier:1,label:'T1',unlockBarracks:1,stats:legacyProfile(1),statsSource:'existing-v0266-profile@bastion1'},
 2:{tier:2,label:'T2',unlockBarracks:4,stats:legacyProfile(4),statsSource:'existing-v0266-profile@bastion4'},
 3:{tier:3,label:'T3',unlockBarracks:10,stats:legacyProfile(10),statsSource:'existing-v0266-profile@bastion10'}
};
const TROOPS={
 archer:{id:'archer',name:'Arqueros',status:'active',playerFacing:true,tiers:ARCHER_TIERS},
 paladin:{id:'paladin',name:'Paladines',status:'reserved',playerFacing:false,tiers:{}},
 warlock:{id:'warlock',name:'Brujos',status:'reserved',playerFacing:false,tiers:{}}
};
const n=v=>Math.max(0,Math.floor(Number(v)||0));
const emptyRoster=()=>({archer:{1:0,2:0,3:0},paladin:{},warlock:{}});
function normalizeRoster(input,legacyTotal=0){
 const out=emptyRoster(),src=input&&typeof input==='object'?input:{};
 if(typeof src.archer==='number')out.archer[1]=n(src.archer);
 else if(src.archer&&typeof src.archer==='object')for(const t of [1,2,3])out.archer[t]=n(src.archer[t]??src.archer['T'+t]);
 for(const type of ['paladin','warlock'])if(src[type]&&typeof src[type]==='object')for(const [tier,count] of Object.entries(src[type]))out[type][tier]=n(count);
 if(!Object.values(out.archer).some(Boolean)&&legacyTotal)out.archer[1]=n(legacyTotal);
 return out;
}
const totalFamily=(roster,type)=>Object.values(roster?.[type]||{}).reduce((a,b)=>a+n(b),0);
const totalTroops=roster=>Object.keys(roster||{}).reduce((sum,type)=>sum+totalFamily(roster,type),0);
const unlockedTiers=(barracksLevel=1,type='archer')=>Object.values(TROOPS[type]?.tiers||{}).filter(x=>barracksLevel>=x.unlockBarracks).map(x=>x.tier);
const highestUnlockedTier=(barracksLevel=1,type='archer')=>Math.max(...unlockedTiers(barracksLevel,type),1);
function addTroops(roster,type,tier,qty){const out=normalizeRoster(roster);out[type]=out[type]||{};out[type][tier]=n(out[type][tier])+n(qty);return out}
function promoteTroops(roster,type,fromTier,toTier,qty){const out=normalizeRoster(roster);qty=n(qty);if(toTier<=fromTier||n(out[type]?.[fromTier])<qty)return{ok:false,roster:out};out[type][fromTier]-=qty;out[type][toTier]=n(out[type][toTier])+qty;return{ok:true,roster:out}}
function normalizeHeroes(ids){const arr=Array.isArray(ids)?ids:[ids].filter(Boolean);return [...new Set(arr.filter(id=>HEROES[id]||id==='maelis'))].slice(0,3)}
function normalizeComposition(input,legacyArchers=0){
 const roster=normalizeRoster(input,legacyArchers),out={archer:{}};
 for(const t of [1,2,3])out.archer[t]=n(roster.archer[t]);
 return out;
}
function affinityBonuses(heroIds,composition){
 const heroes=normalizeHeroes(heroIds),roster=normalizeRoster(composition),bonuses=[];
 for(const id of heroes){const h=HEROES[id],a=h?.affinity;if(!a)continue;const target=a.activeWhen||a.troopType,count=totalFamily(roster,target);if(count)bonuses.push({heroId:id,heroName:h.name,troopType:target,stat:a.stat,modifier:a.modifier,count});}
 return bonuses;
}
function tierStats(type,tier,heroIds=[],composition={}){
 const spec=TROOPS[type]?.tiers?.[tier];if(!spec?.stats)return null;const out={...spec.stats};
 for(const b of affinityBonuses(heroIds,composition))if(b.troopType===type&&Number.isFinite(out[b.stat]))out[b.stat]*=(1+b.modifier);
 return out;
}
function aggregateTroops(composition,heroIds=[]){
 const roster=normalizeRoster(composition),tot={attack:0,defense:0,health:0,break:0,power:0,count:0},details=[];
 for(const [type,tiers] of Object.entries(roster))for(const [tierKey,countRaw] of Object.entries(tiers)){const count=n(countRaw);if(!count)continue;const tier=+tierKey,stats=tierStats(type,tier,heroIds,roster);if(!stats){details.push({type,tier,count,stats:null});continue}const scale=Math.sqrt(count);tot.attack+=stats.attack*scale;tot.defense+=stats.defense*scale;tot.health+=stats.health*scale;tot.break+=stats.break*scale;tot.power+=stats.power*count;tot.count+=count;details.push({type,tier,count,stats});}
 for(const k of ['attack','defense','health','break','power'])tot[k]=Math.round(tot[k]);
 return{...tot,details};
}
function heroPower(id,gearPower=0){const h=HEROES[id];if(!h)return 0;return Math.round(h.baseStats.attack*4+h.baseStats.defense*3+h.baseStats.health+h.baseStats.break*4+gearPower)}
function buildMarch({heroIds=[],troops={},gearPowerByHero={}}={}){
 const heroes=normalizeHeroes(heroIds),roster=normalizeRoster(troops),troopStats=aggregateTroops(roster,heroes);
 let heroStats={attack:0,defense:0,health:0,break:0,power:0};
 for(const id of heroes){const h=HEROES[id];if(!h)continue;const g=Number(gearPowerByHero[id])||0;heroStats.attack+=h.baseStats.attack+g*.16;heroStats.defense+=h.baseStats.defense+g*.12;heroStats.health+=h.baseStats.health+g*.25;heroStats.break+=h.baseStats.break+g*.08;heroStats.power+=heroPower(id,g)}
 for(const k of ['attack','defense','health','break','power'])heroStats[k]=Math.round(heroStats[k]);
 const stats={attack:Math.round(troopStats.attack+heroStats.attack),defense:Math.round(troopStats.defense+heroStats.defense),health:Math.round(troopStats.health+heroStats.health),break:Math.round(troopStats.break+heroStats.break),power:Math.round(troopStats.power+heroStats.power)};
 return{heroes,troops:roster,troopStats,heroStats,stats,affinities:affinityBonuses(heroes,roster),valid:troopStats.count>0&&heroes.length>0&&heroes.length<=3,errors:[]};
}
return{SKILL_RANKS,HEROES,TROOPS,ARCHER_TIERS,emptyRoster,normalizeRoster,totalFamily,totalTroops,unlockedTiers,highestUnlockedTier,addTroops,promoteTroops,normalizeHeroes,normalizeComposition,affinityBonuses,tierStats,aggregateTroops,heroPower,buildMarch,hero:id=>HEROES[id]||null,troop:id=>TROOPS[id]||null};
});
