/* Eldoria v0.27 hero + troop + march domain model. Pure data/logic; no final Hero Hall UI decisions live here. */
(function(root,factory){const api=factory();if(typeof module==='object'&&module.exports)module.exports=api;if(root){root.ELDORIA=root.ELDORIA||{};root.ELDORIA.heroArmy=api;}})(typeof window!=='undefined'?window:null,function(){
'use strict';

const SKILL_RANKS=['I','II','III','IV','V'];
const emptySkill=(slot)=>({slot,id:null,name:null,rank:null,maxRank:5,status:'reserved'});
const skillTrack=(first=null)=>[
  first?{slot:1,rank:1,maxRank:5,status:'active',...first}:emptySkill(1),
  emptySkill(2)
];

const HEROES={
  aldric:{
    id:'aldric',
    name:'Sir Aldric',
    role:{id:'tank',name:'Tanque'},
    affinity:{troopType:'paladin',label:'Paladines',stat:'defense',modifier:0.03},
    statsSource:'existing-v0266-combat',
    baseStats:{attack:78,defense:112,health:420,break:34},
    skills:{
      pve:skillTrack({id:'bulwark',name:'Baluarte',effectId:'next_hit_mitigation'}),
      pvp:skillTrack()
    },
    talents:{mode:'exclusive-choice',choices:[]}
  },
  lyra:{
    id:'lyra',
    name:'Lyra',
    role:{id:'dps',name:'DPS'},
    affinity:{troopType:'archer',label:'Arqueros',stat:'attack',modifier:0.03},
    statsSource:'existing-v0266-combat',
    baseStats:{attack:116,defense:62,health:300,break:82},
    skills:{
      pve:skillTrack({id:'piercingShot',name:'Disparo de Ruptura',effectId:'direct_damage_defense_break'}),
      pvp:skillTrack()
    },
    talents:{mode:'exclusive-choice',choices:[]}
  }
};

const TROOPS={
  archer:{
    id:'archer',
    name:'Arqueros',
    status:'active',
    playerFacing:true,
    statsSource:'existing-v0266-profile',
    profile:(bastionLevel=1)=>{
      const t=Math.max(1,Math.min(10,Number(bastionLevel)||1));
      return{attack:18+t*2,defense:11+t,health:34+t*3,break:9+t*2,power:24+t*3};
    }
  },
  paladin:{
    id:'paladin',
    name:'Paladines',
    status:'reserved',
    playerFacing:false,
    statsSource:'pending-balance',
    profile:null
  }
};

const normalizeCount=v=>Math.max(0,Math.floor(Number(v)||0));
function normalizeComposition(input,legacyArchers=0){
  if(typeof input==='number')return{archer:normalizeCount(input)};
  const src=input&&typeof input==='object'?input:{archer:legacyArchers};
  const out={};
  for(const [id,v] of Object.entries(src))out[id]=normalizeCount(v);
  if(!Object.keys(out).length)out.archer=normalizeCount(legacyArchers);
  return out;
}
function normalizeHeroes(ids){
  const arr=Array.isArray(ids)?ids:[ids].filter(Boolean);
  return [...new Set(arr.filter(id=>HEROES[id]))];
}
function affinityBonuses(heroIds,composition){
  const heroes=normalizeHeroes(heroIds),troops=normalizeComposition(composition);
  const bonuses=[];
  for(const id of heroes){
    const h=HEROES[id],a=h.affinity;
    if(!a||!troops[a.troopType])continue;
    bonuses.push({heroId:id,heroName:h.name,troopType:a.troopType,stat:a.stat,modifier:a.modifier,count:troops[a.troopType]});
  }
  return bonuses;
}
function applyAffinityToStats(troopType,stats,heroIds,composition){
  let out={...stats};
  for(const b of affinityBonuses(heroIds,composition)){
    if(b.troopType!==troopType||!Number.isFinite(out[b.stat]))continue;
    out[b.stat]=out[b.stat]*(1+b.modifier);
  }
  return out;
}
function resolveTroopProfile(troopType,bastionLevel,heroIds,composition,profileOverrides={}){
  const family=TROOPS[troopType];
  if(!family)throw new Error('Unknown troop family '+troopType);
  const raw=profileOverrides[troopType]||(family.profile?family.profile(bastionLevel):null);
  if(!raw)return null;
  return applyAffinityToStats(troopType,raw,heroIds,composition);
}
function buildMarch({heroIds=[],troops={},bastionLevel=1,profileOverrides={}}={}){
  const heroes=normalizeHeroes(heroIds),composition=normalizeComposition(troops);
  const families={};
  for(const [type,count] of Object.entries(composition)){
    if(!count)continue;
    const family=TROOPS[type];if(!family)throw new Error('Unknown troop family '+type);
    families[type]={id:type,name:family.name,count,stats:resolveTroopProfile(type,bastionLevel,heroes,composition,profileOverrides),status:family.status};
  }
  const errors=Object.values(families).filter(x=>!x.stats).map(x=>'Stats pending for '+x.name);
  return{
    heroes,
    troops:composition,
    families,
    affinities:affinityBonuses(heroes,composition),
    valid:errors.length===0,
    errors
  };
}
function hero(id){return HEROES[id]||null}
function troop(id){return TROOPS[id]||null}

return{SKILL_RANKS,HEROES,TROOPS,normalizeComposition,normalizeHeroes,affinityBonuses,applyAffinityToStats,resolveTroopProfile,buildMarch,hero,troop};
});
