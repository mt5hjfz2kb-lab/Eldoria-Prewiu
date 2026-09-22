/* Eldoria v0.26.6 gameplay rules shared by runtime and behavior QA. */
(()=>{'use strict';const E=window.ELDORIA=window.ELDORIA||{};
const BASTION_COSTS={2:[450,300,0],3:[650,500,0],4:[850,700,300],5:[1100,900,500],6:[1300,1050,650],7:[1500,1200,800],8:[1750,1400,950],9:[2050,1650,1100],10:[2400,1950,1300]};
const canAfford=(s,c)=>s.wood>=c[0]&&s.stone>=c[1]&&(s.food||0)>=c[2];

const ENEMIES={
  wolf:{id:'wolf',name:'Lobo ceniciento',level:1,kind:'hunt',stats:{attack:52,defense:42,health:360,break:18,power:1450}},
  boar:{id:'boar',name:'Jabalí de roca',level:1,kind:'hunt',stats:{attack:64,defense:58,health:470,break:12,power:2050}},
  spawnling:{id:'spawnling',name:'Engendro de la Fisura',level:1,kind:'common',stats:{attack:76,defense:64,health:620,break:28,power:2250}},
  ashStalker:{id:'ashStalker',name:'Acechador de Ceniza',level:3,kind:'uncommon',trait:{id:'ambush',name:'Emboscada',copy:'Golpea con fuerza al inicio, pero expone su defensa después del primer choque.'},stats:{attack:132,defense:68,health:980,break:54,power:3600}},
  herald:{id:'herald',name:'Heraldo de la Fisura',level:5,kind:'worldboss',trait:{id:'riftPulse',name:'Pulso de la Brecha',copy:'Sus pulsos castigan marchas mal preparadas. Una intervención de héroe puede cambiar el intercambio.'},stats:{attack:188,defense:122,health:2250,break:78,power:6200}}
};
const HERO_COMBAT={
  aldric:{name:'Sir Aldric',attack:78,defense:112,health:420,break:34,skill:{id:'bulwark',name:'Baluarte',copy:'Reduce el siguiente golpe enemigo un 45%.'}},
  lyra:{name:'Lyra',attack:116,defense:62,health:300,break:82,skill:{id:'piercingShot',name:'Disparo de Ruptura',copy:'Inflige daño inmediato y debilita la defensa enemiga.'}},
  maelis:{name:'Maelis',attack:72,defense:92,health:390,break:48,skill:{id:'ward',name:'Velo de Nareth',copy:'Recupera vida de la marcha y reduce daño del siguiente golpe.'}}
};
const TROOP_FAMILIES={archer:{id:'archer',name:'Arqueros',profile:t=>({attack:18+t*2,defense:11+t,health:34+t*3,break:9+t*2,power:24+t*3})}};
const troopProfile=(bastionLevel=1,type='archer')=>{const t=Math.max(1,Math.min(10,Number(bastionLevel)||1)),family=TROOP_FAMILIES[type]||TROOP_FAMILIES.archer;return{type:family.id,name:family.name,...family.profile(t)}};
const playerStats=({troops=1,bastionLevel=1,hero='aldric',gearPower=0}={})=>{
  const n=Math.max(1,Math.floor(Number(troops)||1)),tp=troopProfile(bastionLevel),h=HERO_COMBAT[hero]||HERO_COMBAT.aldric,scale=Math.sqrt(n);
  const attack=Math.round(tp.attack*scale+h.attack+gearPower*.16);
  const defense=Math.round(tp.defense*scale+h.defense+gearPower*.12);
  const health=Math.round(tp.health*scale+h.health+gearPower*.25);
  const brk=Math.round(tp.break*scale+h.break+gearPower*.08);
  const power=Math.round(n*tp.power+h.attack*4+h.defense*3+h.health+brk*4+gearPower);
  return{attack,defense,health,break:brk,power,troopType:'archer',troopName:'Arqueros',hero};
};
const effectiveHit=(attack,defense,brk,mult=1)=>Math.max(12,Math.round((attack*(1+Math.min(.6,brk/500))-(defense*.48))*mult));
const simulate=({enemyId,player,useSkill=false}={})=>{
  const enemy=ENEMIES[enemyId];if(!enemy)throw new Error('Unknown enemy '+enemyId);
  const p={...player},e={...enemy.stats};let pHp=p.health,eHp=e.health,opening=0,skillImpact=0,defDebuff=0,nextMitigation=0,rounds=[];
  if(enemy.trait?.id==='ambush'){opening=effectiveHit(e.attack,p.defense,e.break,1.42);pHp-=opening;defDebuff=.18;}
  if(enemy.kind==='worldboss'&&useSkill){
    if(p.hero==='aldric')nextMitigation=.45;
    else if(p.hero==='lyra'){skillImpact=Math.max(85,Math.round(p.attack*.72+p.break*.55));eHp-=skillImpact;defDebuff=.22;}
    else if(p.hero==='maelis'){const heal=Math.round(p.health*.18);pHp=Math.min(p.health,pHp+heal);skillImpact=-heal;nextMitigation=.25;}
  }
  for(let r=1;r<=6&&pHp>0&&eHp>0;r++){
    const enemyDefense=Math.round(e.defense*(1-defDebuff));
    const pd=effectiveHit(p.attack,enemyDefense,p.break,1+(r===1&&enemy.trait?.id==='ambush'?.08:0));
    eHp-=pd;
    if(eHp<=0){rounds.push({round:r,playerDamage:pd,enemyDamage:0});break}
    let ed=effectiveHit(e.attack,p.defense,e.break,1);
    if(nextMitigation){ed=Math.round(ed*(1-nextMitigation));nextMitigation=0}
    pHp-=ed;rounds.push({round:r,playerDamage:pd,enemyDamage:ed});
  }
  const win=eHp<=0&&pHp>0;
  const reasons=[];
  if(enemy.kind==='hunt')reasons.push(p.power>=e.power?'Tu Poder superó el umbral de la presa.':'La presa superó el Poder de tu expedición.');
  else{
    if(p.break>e.break)reasons.push('Tu Ruptura abrió mejor la defensa rival.');
    else reasons.push('La Ruptura enemiga redujo tu margen defensivo.');
    if(p.defense>=e.attack*.75)reasons.push('Tu Defensa absorbió una parte importante del daño.');
    else reasons.push('La presión enemiga superó tu Defensa.');
    if(enemy.trait?.id==='ambush')reasons.push(opening>0?'Emboscada aplicó '+opening+' de daño inicial; después el Acechador quedó con menos Defensa.':'Emboscada no llegó a activarse.');
    if(useSkill&&enemy.kind==='worldboss')reasons.push(skillImpact>=0?'La habilidad del héroe alteró el intercambio a tu favor.':'La habilidad recuperó vida de la marcha.');
  }
  return{enemyId,win,playerStart:p.health,playerEnd:Math.max(0,pHp),enemyStart:e.health,enemyEnd:Math.max(0,eHp),openingDamage:opening,skillImpact,rounds,reasons,player:p,enemy:e};
};
const huntResult=({enemyId,power}={})=>{const e=ENEMIES[enemyId];if(!e||e.kind!=='hunt')throw new Error('Not hunt');return{win:Number(power)>=e.stats.power,required:e.stats.power,actual:Number(power)||0}};

E.gameplay={
  BASTION_COSTS,canAfford,
  canBastion2:s=>canAfford(s,BASTION_COSTS[2])&&!!s.camp,
  canBastion3:s=>canAfford(s,BASTION_COSTS[3])&&!!s.barracks&&s.troops>=41,
  combat:{ENEMIES,HERO_COMBAT,TROOP_FAMILIES,troopProfile,playerStats,simulate,huntResult}
};
})();