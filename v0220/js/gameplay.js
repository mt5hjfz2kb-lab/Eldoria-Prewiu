/* Eldoria v0.27 gameplay rules shared by runtime and behavior QA. */
(()=>{'use strict';const E=window.ELDORIA=window.ELDORIA||{};
const BASTION_COSTS={2:[450,300,0],3:[650,500,0],4:[850,700,300],5:[1100,900,500],6:[1450,1200,700],7:[1850,1550,950],8:[2350,2000,1250],9:[3000,2600,1650],10:[3900,3400,2200]};
const canAfford=(s,c)=>s.wood>=c[0]&&s.stone>=c[1]&&(s.food||0)>=c[2];
const CHAPTERS=[
{id:1,title:'LAS CENIZAS DE VALORIA',context:'Valoria vuelve a respirar entre ruinas. Recupera lo necesario para levantar un reino, no solo una muralla.',missions:[
{id:'c1-sawmill',title:'Reconstruye el Aserradero',type:'flag',path:'sawmill',value:1,reward:{wood:120}},
{id:'c1-wood',title:'Recupera 600 madera',type:'counter',counter:'gathered.wood',value:600,target:{kind:'node',id:'forest'},reward:{stone:80}},
{id:'c1-stone',title:'Recupera 500 piedra',type:'counter',counter:'gathered.stone',value:500,target:{kind:'node',id:'quarry'},reward:{wood:120}},
{id:'c1-route',title:'Despeja la ruta de Corruptos',type:'flag',path:'camp',value:1,target:{kind:'node',id:'camp'},reward:{wood:90,stone:55}},
{id:'c1-bastion',title:'Eleva el Bastión a nivel 2',type:'state',path:'bastionLevel',value:2,target:{kind:'poi',id:'keep'},reward:{power:80}}],chapterReward:{wood:180,stone:140,power:120}},
{id:2,title:'ALGO QUE DEFENDER',context:'Las murallas necesitan soldados y las primeras amenazas deben empezar a tener explicación.',missions:[
{id:'c2-barracks',title:'Construye el Cuartel',type:'flag',path:'barracks',value:1,target:{kind:'poi',id:'barracks'},reward:{wood:140,stone:90}},
{id:'c2-train',title:'Entrena 20 arqueros',type:'counter',counter:'trained',value:20,target:{kind:'poi',id:'barracks'},reward:{wood:220,stone:150}},
{id:'c2-power',title:'Alcanza 2.250 de Poder de expedición',type:'expedition',value:2250,target:{kind:'nav',id:'march'},reward:{power:100}},
{id:'c2-spawnling',title:'Derrota un Engendro de la Fisura',type:'counter',counter:'wins.spawnling',value:1,target:{kind:'node',id:'spawnling'},reward:{wood:120,stone:120}},
{id:'c2-bastion',title:'Eleva el Bastión a nivel 3',type:'state',path:'bastionLevel',value:3,target:{kind:'poi',id:'keep'},reward:{food:180}}],chapterReward:{wood:240,stone:200,food:220,power:180}},
{id:3,title:'MÁS ALLÁ DE LAS MURALLAS',context:'Valoria aprende a alimentarse, cazar y sostener una frontera más amplia.',missions:[
{id:'c3-granary',title:'Construye el Granero',type:'flag',path:'granary',value:1,target:{kind:'poi',id:'granary'},reward:{wood:160,stone:110}},
{id:'c3-food',title:'Reúne 500 comida',type:'resource',path:'food',value:500,target:{kind:'node',id:'meat'},reward:{wood:120}},
{id:'c3-hunt',title:'Completa 2 cacerías',type:'counter',counter:'hunts',value:2,target:{kind:'node',id:'wolf'},reward:{food:220}},
{id:'c3-fissure',title:'Somete la Fisura',type:'flag',path:'boss',value:1,target:{kind:'node',id:'boss'},reward:{stone:260,power:100}},
{id:'c3-infra',title:'Prepara la infraestructura de Valoria',type:'infra2',value:3,target:{kind:'poi',id:'sawmill'},reward:{wood:220,stone:180}},
{id:'c3-bastion',title:'Eleva el Bastión a nivel 4',type:'state',path:'bastionLevel',value:4,target:{kind:'poi',id:'keep'},reward:{food:180}}],chapterReward:{wood:280,stone:240,food:260,power:220}},
{id:4,title:'EL PRECIO DEL CRECIMIENTO',context:'Crecer ya no consiste en acumular: consiste en sostener varios sistemas a la vez.',missions:[
{id:'c4-sawmill2',title:'Aserradero a nivel 2',type:'building',path:'sawmill',value:2,target:{kind:'poi',id:'sawmill'},reward:{wood:180}},
{id:'c4-granary2',title:'Granero a nivel 2',type:'building',path:'granary',value:2,target:{kind:'poi',id:'granary'},reward:{food:220}},
{id:'c4-barracks2',title:'Cuartel a nivel 2',type:'building',path:'barracks',value:2,target:{kind:'poi',id:'barracks'},reward:{stone:180}},
{id:'c4-priority',title:'Elige una prioridad para Valoria',type:'flag',path:'developmentChoices.4',value:1,reward:{power:80}},
{id:'c4-bastion',title:'Eleva el Bastión a nivel 5',type:'state',path:'bastionLevel',value:5,target:{kind:'poi',id:'keep'},reward:{power:220}}],chapterReward:{wood:360,stone:300,food:280,speedup5:1,power:260}},
{id:5,title:'PIEDRA PARA UN REINO',context:'La economía se diversifica. Piedra, tropas y amenazas compiten por la misma atención.',missions:[
{id:'c5-stoneworks',title:'Construye la Cantera de Valoria',type:'flag',path:'graniteQuarry',value:1,target:{kind:'poi',id:'stoneworks'},reward:{wood:220,food:180}},
{id:'c5-stone',title:'Mantén 1.200 piedra disponible',type:'resource',path:'stone',value:1200,target:{kind:'node',id:'quarry3'},reward:{wood:240}},
{id:'c5-army',title:'Alcanza 60 arqueros',type:'state',path:'troops',value:60,target:{kind:'poi',id:'barracks'},reward:{wood:260,stone:180}},
{id:'c5-rare',title:'Derrota al Acechador de Ceniza',type:'counter',counter:'wins.ashStalker',value:1,target:{kind:'node',id:'ashStalker'},reward:{food:260,stone:180}},
{id:'c5-bastion',title:'Eleva el Bastión a nivel 6',type:'state',path:'bastionLevel',value:6,target:{kind:'poi',id:'keep'},reward:{power:260}}],chapterReward:{wood:420,stone:360,food:320,speedup5:1,power:300}},
{id:6,title:'FUEGO ANTIGUO',context:'La Forja convierte descubrimientos del Mundo en preparación real para la expedición.',missions:[
{id:'c6-forge',title:'Reconstruye la Forja',type:'flag',path:'forge',value:1,target:{kind:'poi',id:'forge'},reward:{wood:240,stone:180}},
{id:'c6-devourer',title:'Derrota al Devorador de Éter',type:'flag',path:'devourerDefeated',value:1,target:{kind:'node',id:'devourer'},reward:{power:180}},
{id:'c6-gear',title:'Forja tu primer equipo',type:'equipment',value:1,target:{kind:'poi',id:'forge'},reward:{food:260}},
{id:'c6-expedition',title:'Alcanza 2.800 de Poder de expedición',type:'expedition',value:2800,target:{kind:'nav',id:'march'},reward:{power:180}},
{id:'c6-bastion',title:'Eleva el Bastión a nivel 7',type:'state',path:'bastionLevel',value:7,target:{kind:'poi',id:'keep'},reward:{power:300}}],chapterReward:{wood:480,stone:420,food:360,speedup5:1,power:340}},
{id:7,title:'CÓDICE Y RELICARIO',context:'El Códice registra lo que Eldoria aprende. El Relicario reúne las cartas y decisiones que nacen de esos descubrimientos.',missions:[
{id:'c7-codex',title:'Consulta el Códice de Eldoria',type:'flag',path:'codexUnlocked',value:1,target:{kind:'nav',id:'codex'},reward:{power:160}},
{id:'c7-relics',title:'Descubre tu primera Reliquia',type:'relics',value:1,target:{kind:'nav',id:'relicario'},reward:{stone:260}},
{id:'c7-decision',title:'Decide el destino de una Reliquia',type:'counter',counter:'relicDecisions',value:1,target:{kind:'nav',id:'relicario'},reward:{speedup1:2}},
{id:'c7-duel',title:'Comprende el Relicario',type:'flag',path:'relicTutorialComplete',value:1,target:{kind:'nav',id:'relicario'},reward:{power:220}},
{id:'c7-bastion',title:'Eleva el Bastión a nivel 8',type:'state',path:'bastionLevel',value:8,target:{kind:'poi',id:'keep'},reward:{power:320}}],chapterReward:{wood:520,stone:460,food:400,speedup15:1,power:420}},
{id:8,title:'VOCES DE NARETH',context:'La composición empieza a importar más que la suma bruta de Poder.',missions:[
{id:'c8-nareth',title:'Investiga las ruinas de Nareth',type:'flag',path:'narethRescued',value:1,target:{kind:'node',id:'nareth'},reward:{food:300}},
{id:'c8-maelis',title:'Incorpora a Maelis',type:'flag',path:'maelis',value:1,target:{kind:'nav',id:'heroes'},reward:{power:220}},
{id:'c8-equip',title:'Equipa a un héroe',type:'equipped',value:1,target:{kind:'nav',id:'heroes'},reward:{stone:260}},
{id:'c8-expedition',title:'Alcanza 3.200 de Poder de expedición',type:'expedition',value:3200,target:{kind:'nav',id:'march'},reward:{power:180}},
{id:'c8-bastion',title:'Eleva el Bastión a nivel 9',type:'state',path:'bastionLevel',value:9,target:{kind:'poi',id:'keep'},reward:{power:360}}],chapterReward:{wood:600,stone:520,food:460,speedup5:1,power:460}},
{id:9,title:'PREPARATIVOS DE GUERRA',context:'Reino, ejército, héroes, equipo, Códice y Relicario deben empezar a funcionar como un único sistema.',missions:[
{id:'c9-march',title:'Prepara una expedición completa',type:'flag',path:'marchConfigured',value:1,target:{kind:'nav',id:'march'},reward:{food:320}},
{id:'c9-power',title:'Alcanza 14.000 de Poder total',type:'totalPower',value:14000,reward:{power:200}},
{id:'c9-collection',title:'Mantén 3 Reliquias descubiertas',type:'relics',value:3,target:{kind:'nav',id:'relicario'},reward:{speedup5:1}},
{id:'c9-trial',title:'Supera la Prueba de Marcha',type:'flag',path:'trialWon',value:1,target:{kind:'node',id:'trial'},reward:{power:260}},
{id:'c9-bastion',title:'Eleva el Bastión a nivel 10',type:'state',path:'bastionLevel',value:10,target:{kind:'poi',id:'keep'},reward:{power:420}}],chapterReward:{wood:680,stone:600,food:520,speedup15:1,power:520}},
{id:10,title:'LA PRIMERA BRECHA',context:'Ya no se trata de recolectar. Se trata de demostrar que Valoria sabe prepararse y luchar.',missions:[
{id:'c10-herald',title:'Derrota al Heraldo de la Fisura',type:'counter',counter:'wins.herald',value:1,target:{kind:'node',id:'herald'},reward:{power:300}},
{id:'c10-skill',title:'Intervén con una habilidad de héroe',type:'counter',counter:'heroInterventions',value:1,target:{kind:'node',id:'herald'},reward:{speedup5:1}},
{id:'c10-final',title:'Completa el asalto final del Arco I',type:'flag',path:'finalWon',value:1,target:{kind:'node',id:'final'},reward:{power:500}}],chapterReward:{wood:900,stone:800,food:700,speedup15:2,power:900}}
];


const ENEMIES={
  wolf:{id:'wolf',name:'Lobo ceniciento',level:1,kind:'hunt',stats:{attack:52,defense:42,health:360,break:18,power:1450}},
  boar:{id:'boar',name:'Jabalí de roca',level:1,kind:'hunt',stats:{attack:64,defense:58,health:470,break:12,power:2050}},
  spawnling:{id:'spawnling',name:'Engendro de la Fisura',level:1,kind:'common',stats:{attack:76,defense:64,health:620,break:28,power:2250}},
  ashStalker:{id:'ashStalker',name:'Acechador de Ceniza',level:3,kind:'uncommon',trait:{id:'ambush',name:'Emboscada',copy:'Golpea con fuerza al inicio, pero expone su defensa después del primer choque.'},stats:{attack:132,defense:68,health:920,break:54,power:3600}},
  devourer:{id:'devourer',name:'Devorador de Éter',level:4,kind:'elite',trait:{id:'aetherShell',name:'Caparazón de Éter',copy:'Refuerza su Defensa durante el primer intercambio; después la coraza pierde estabilidad.'},stats:{attack:142,defense:92,health:1050,break:52,power:3900}},
  herald:{id:'herald',name:'Heraldo de la Fisura',level:5,kind:'worldboss',trait:{id:'riftPulse',name:'Pulso de la Brecha',copy:'Sus pulsos castigan marchas mal preparadas. Una intervención de héroe puede cambiar el intercambio.'},stats:{attack:188,defense:122,health:2250,break:78,power:6200}}
};
const HA=E.heroArmy;
const HERO_COMBAT={
  aldric:{name:'Sir Aldric',...HA.HEROES.aldric.baseStats,role:HA.HEROES.aldric.role,affinity:HA.HEROES.aldric.affinity,skill:{id:'bulwark',name:'Baluarte',copy:'Reduce el siguiente golpe enemigo un 45%.'}},
  lyra:{name:'Lyra',...HA.HEROES.lyra.baseStats,role:HA.HEROES.lyra.role,affinity:HA.HEROES.lyra.affinity,skill:{id:'piercingShot',name:'Disparo de Ruptura',copy:'Inflige daño inmediato y debilita la defensa enemiga.'}},
  maelis:{name:'Maelis',attack:72,defense:92,health:390,break:48,role:{id:'support',name:'Soporte'},affinity:null,skill:{id:'ward',name:'Velo de Nareth',copy:'Recupera vida de la marcha y reduce daño del siguiente golpe.'}}
};
const TROOP_FAMILIES=HA.TROOPS;
const troopProfile=(tier=1,type='archer',heroIds=[])=>{const family=TROOP_FAMILIES[type];if(!family)throw new Error('Unknown troop family '+type);const composition={archer:{1:0,2:0,3:0}};if(type==='archer')composition.archer[tier]=1;const stats=HA.tierStats(type,tier,heroIds,composition);if(!stats)throw new Error('Troop family '+type+' tier '+tier+' has no balanced stats yet');return{type:family.id,name:family.name,tier,...stats}};
const playerStats=({troops=1,troopComposition=null,bastionLevel=1,hero='aldric',heroes=null,gearPower=0,gearPowerByHero=null}={})=>{
  const heroIds=HA.normalizeHeroes(heroes||[hero]),activeHero=heroIds[0]||'aldric';
  const composition=troopComposition?HA.normalizeRoster(troopComposition):HA.normalizeRoster(null,Math.max(1,Math.floor(Number(troops)||1)));
  const gearMap=gearPowerByHero||{[activeHero]:gearPower};
  const march=HA.buildMarch({heroIds,troops:composition,gearPowerByHero:gearMap});
  if(!march.valid)throw new Error('Invalid march composition');
  return{...march.stats,troopType:'archer',troopName:'Arqueros',hero:activeHero,heroes:heroIds,troopComposition:march.troops,affinities:march.affinities,troopBreakdown:march.troopStats.details,heroBreakdown:march.heroStats};
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
    const enemyDefense=Math.round(e.defense*(1-defDebuff)*(enemy.trait?.id==='aetherShell'&&r===1?1.18:1));
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
    if(enemy.trait?.id==='aetherShell')reasons.push('Caparazón de Éter reforzó su Defensa solo durante el primer intercambio; después perdió estabilidad.');
    if(useSkill&&enemy.kind==='worldboss')reasons.push(skillImpact>=0?'La habilidad del héroe alteró el intercambio a tu favor.':'La habilidad recuperó vida de la marcha.');
  }
  return{enemyId,win,playerStart:p.health,playerEnd:Math.max(0,pHp),enemyStart:e.health,enemyEnd:Math.max(0,eHp),openingDamage:opening,skillImpact,rounds,reasons,player:p,enemy:e};
};
const huntResult=({enemyId,power}={})=>{const e=ENEMIES[enemyId];if(!e||e.kind!=='hunt')throw new Error('Not hunt');return{win:Number(power)>=e.stats.power,required:e.stats.power,actual:Number(power)||0}};

E.gameplay={
  BASTION_COSTS,CHAPTERS,canAfford,
  canBastion2:s=>canAfford(s,BASTION_COSTS[2])&&!!s.camp,
  canBastion3:s=>canAfford(s,BASTION_COSTS[3])&&!!s.barracks&&s.troops>=41,
  combat:{ENEMIES,HERO_COMBAT,TROOP_FAMILIES,troopProfile,playerStats,simulate,huntResult}
};
})();