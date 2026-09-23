/* Eldoria v0.30 candidate — depth pass: autonomy, rankings, localization, clock, audio.
   This layer only deepens existing systems. It does not add or alter global Breach mechanics. */
(()=>{'use strict';
const API=()=>window.ELDORIA_V023;
const ROOT=()=>document.getElementById('eldoria-core-loop');
const QA=new URLSearchParams(location.search).get('qa')==='1';
const K={
 locale:'eldoria-v030-locale',
 audio:'eldoria-v030-audio',
 powerIntro:'eldoria-v030-power-intro',
 milestones:'eldoria-v030-power-milestones'
};
const cfg={
 locale:localStorage.getItem(K.locale)||'es',
 audio:Object.assign({music:true,sfx:true},(()=>{try{return JSON.parse(localStorage.getItem(K.audio)||'{}')}catch(_){return{}}})())
};
const tr=(es,en)=>cfg.locale==='en'?en:es;
const fmt=n=>new Intl.NumberFormat(cfg.locale==='en'?'en-US':'es-ES').format(Math.round(Number(n)||0));
const state=()=>API()?.state?.()||{};
const update=patch=>API()?.setQA?.(patch);
const htmlEscape=s=>String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));

/* ---------- audio: first lightweight layer, generated locally with WebAudio ---------- */
let AC=null, ambient=null;
function audioCtx(){if(!AC){const C=window.AudioContext||window.webkitAudioContext;if(C)AC=new C()}if(AC?.state==='suspended')AC.resume().catch(()=>{});return AC}
function tone(freq=440,dur=.08,gain=.025,type='sine'){if(!cfg.audio.sfx)return;const a=audioCtx();if(!a)return;const o=a.createOscillator(),g=a.createGain();o.type=type;o.frequency.value=freq;g.gain.setValueAtTime(gain,a.currentTime);g.gain.exponentialRampToValueAtTime(.0001,a.currentTime+dur);o.connect(g).connect(a.destination);o.start();o.stop(a.currentTime+dur)}
function stopAmbient(){if(ambient){try{ambient.forEach(x=>x.stop())}catch(_){}ambient=null}}
function startAmbient(){if(!cfg.audio.music||ambient)return;const a=audioCtx();if(!a)return;ambient=[110,164.81,220].map((f,i)=>{const o=a.createOscillator(),g=a.createGain();o.type=i===1?'triangle':'sine';o.frequency.value=f;g.gain.value=.004;o.connect(g).connect(a.destination);o.start();return o})}
function saveAudio(){localStorage.setItem(K.audio,JSON.stringify(cfg.audio));if(cfg.audio.music)startAmbient();else stopAmbient()}

/* ---------- server clock + compact HUD controls ---------- */
function ensureHudTools(){
 const tools=ROOT()?.querySelector('.hudTools'); if(!tools)return;
 if(!tools.querySelector('[data-testid="world-clock"]')){
   const el=document.createElement('span');el.className='worldClock030';el.dataset.testid='world-clock';tools.prepend(el);
 }
 if(!tools.querySelector('[data-v030-settings]')){
   const b=document.createElement('button');b.className='hudTool settings030';b.type='button';b.dataset.v030Settings='1';b.dataset.testid='v030-settings';b.setAttribute('aria-label',tr('Ajustes','Settings'));b.innerHTML='<span>⚙</span><small>'+tr('AJUSTES','SETTINGS')+'</small>';tools.appendChild(b);b.onclick=openSettings;
 }
 updateClock();
}
function updateClock(){
 const el=ROOT()?.querySelector('[data-testid="world-clock"]');if(!el)return;
 const d=new Date(),hh=String(d.getUTCHours()).padStart(2,'0'),mm=String(d.getUTCMinutes()).padStart(2,'0');
 const text=tr('SERVIDOR ','SERVER ')+hh+':'+mm+' UTC',title=tr('Hora del mundo de Eldoria, preparada para futuros eventos','Eldoria world time, ready for future events');
 if(el.textContent!==text)el.textContent=text;if(el.title!==title)el.title=title;
}
function openSettings(){
 tone(520,.06);const old=document.querySelector('[data-testid="v030-settings-dialog"]');if(old)old.remove();
 const o=document.createElement('div');o.className='e22-overlay v030Overlay';o.innerHTML='<div class="e22-dialog settingsCard030" data-testid="v030-settings-dialog"><small>'+tr('PREFERENCIAS','PREFERENCES')+'</small><h1>'+tr('AJUSTES','SETTINGS')+'</h1><div class="settingRow030"><div><b>'+tr('Idioma','Language')+'</b><small>'+tr('El ajuste se conserva en este dispositivo.','This setting persists on this device.')+'</small></div><div class="seg030"><button data-lang="es" class="'+(cfg.locale==='es'?'active':'')+'">ES</button><button data-lang="en" class="'+(cfg.locale==='en'?'active':'')+'">EN</button></div></div><label class="settingRow030"><div><b>'+tr('Música ambiental','Ambient music')+'</b><small>'+tr('Capa ligera de atmósfera.','Light atmospheric layer.')+'</small></div><input type="checkbox" data-music '+(cfg.audio.music?'checked':'')+'></label><label class="settingRow030"><div><b>'+tr('Efectos de sonido','Sound effects')+'</b><small>'+tr('Acciones, decisiones e hitos.','Actions, choices and milestones.')+'</small></div><input type="checkbox" data-sfx '+(cfg.audio.sfx?'checked':'')+'></label><button class="btn" data-close>'+tr('CERRAR','CLOSE')+'</button></div>';document.body.appendChild(o);
 o.querySelectorAll('[data-lang]').forEach(b=>b.onclick=()=>setLocale(b.dataset.lang));
 o.querySelector('[data-music]').onchange=e=>{cfg.audio.music=e.target.checked;saveAudio()};
 o.querySelector('[data-sfx]').onchange=e=>{cfg.audio.sfx=e.target.checked;saveAudio();tone(620,.07)};
 o.querySelector('[data-close]').onclick=()=>o.remove();o.onclick=e=>{if(e.target===o)o.remove()};
}
function setLocale(l){
 cfg.locale=l==='en'?'en':'es';localStorage.setItem(K.locale,cfg.locale);document.documentElement.lang=cfg.locale;
 const s=state();if(s.locale!==cfg.locale)update({locale:cfg.locale});
 document.querySelector('[data-testid="v030-settings-dialog"]')?.remove();localizePage();ensureHudTools();openSettings();
}

/* ---------- localization pass ---------- */
const exact=new Map(Object.entries({
'VALORIA':'VALORIA','Bastión':'Bastion','MADERA':'WOOD','PIEDRA':'STONE','COMIDA':'FOOD','PODER':'POWER','RANKING':'RANKINGS','ARCÓN':'CHEST',
'CIUDAD':'CITY','MUNDO':'WORLD','HÉROES':'HEROES','CÓDICE':'CODEX','RELICARIO':'RELIQUARY','AJUSTES':'SETTINGS',
'VOLVER':'BACK','CERRAR':'CLOSE','CANCELAR':'CANCEL','CONTINUAR':'CONTINUE','ATACAR':'ATTACK','INICIAR COMBATE':'START BATTLE','VOLVER AL MUNDO':'BACK TO WORLD',
'VICTORIA':'VICTORY','DERROTA':'DEFEAT','INFORME DE BATALLA':'BATTLE REPORT','CAZA · LECCIÓN DE PODER':'HUNT · POWER LESSON',
'TU PODER':'YOUR POWER','PODER DE LA PRESA':'PREY POWER','TU MARCHA':'YOUR MARCH','ENEMIGO':'ENEMY','Vida restante:':'Health remaining:',
'¿POR QUÉ OCURRIÓ?':'WHY DID IT HAPPEN?','VER INTERCAMBIOS':'VIEW EXCHANGES','PREPARAR MARCHA':'PREPARE MARCH','ARQUEROS':'ARCHERS','HÉROE':'HERO','RASGO':'TRAIT',
'PREVISIÓN CON ESTA COMPOSICIÓN':'FORECAST WITH THIS COMPOSITION','FAVORABLE':'FAVORABLE','DESFAVORABLE':'UNFAVORABLE','HABILIDAD ACTIVADA':'ABILITY ACTIVATED',
'CAPÍTULO COMPLETADO':'CHAPTER COMPLETE','RECOMPENSA DE CAPÍTULO':'CHAPTER REWARD','ACELERADORES UNIVERSALES':'UNIVERSAL SPEEDUPS','IR AL OBJETIVO':'GO TO OBJECTIVE',
'COLECCIÓN':'COLLECTION','PRÁCTICA':'PRACTICE','DUELO PVP':'PVP DUEL','PERSONAJES':'CHARACTERS','BESTIARIO':'BESTIARY',
'LA BRECHA':'THE BREACH','MUNDO':'WORLD','EQUIPO':'GEAR','HABILIDADES':'SKILLS','TALENTOS':'TALENTS','EQUIPAR':'EQUIP','CONSERVAR':'KEEP','USAR':'USE',
'PREFERENCIAS':'PREFERENCES','Idioma':'Language','Música ambiental':'Ambient music','Efectos de sonido':'Sound effects'
}));
const phrases=[
[/Bastión (\d+)/g,'Bastion $1'],[/Nv\.\s*(\d+)/g,'Lv. $1'],[/Ronda (\d+) · infliges (\d+) · recibes (\d+)/g,'Round $1 · dealt $2 · received $3'],
[/Vida estimada de la marcha:/g,'Estimated march health:'],[/arqueros/g,'archers'],[/Poder de expedición/g,'Expedition Power'],[/Poder total/g,'Total Power'],
[/PODER TOTAL/g,'TOTAL POWER'],[/CAPÍTULO (\d+)/g,'CHAPTER $1'],[/Misiones/g,'Missions'],[/Capítulo completado/g,'Chapter complete'],
[/RECOLECTANDO MADERA/g,'GATHERING WOOD'],[/EXTRAYENDO PIEDRA/g,'EXTRACTING STONE'],[/RECOLECTANDO PROVISIONES/g,'GATHERING PROVISIONS'],
[/RECOGIENDO CARNE/g,'GATHERING MEAT'],[/EN MARCHA/g,'MARCHING'],[/restantes/g,'remaining'],[/CONSTRUIR/g,'BUILD'],[/MEJORAR/g,'UPGRADE'],
[/FORTALEZA/g,'KEEP'],[/ASERRADERO/g,'SAWMILL'],[/GRANERO/g,'GRANARY'],[/CUARTEL/g,'BARRACKS'],[/CANTERA/g,'QUARRY'],[/FORJA/g,'FORGE'],
[/Alcanza/g,'Reach'],[/Derrota/g,'Defeat'],[/Construye/g,'Build'],[/Reconstruye/g,'Rebuild'],[/Eleva/g,'Raise'],[/Descubre/g,'Discover'],[/Completa/g,'Complete'],
[/Mantén/g,'Keep'],[/Prepara/g,'Prepare'],[/Equipa/g,'Equip'],[/Consulta/g,'Open'],[/Utiliza/g,'Use'],[/Entrena/g,'Train'],[/Recupera/g,'Recover'],[/Reúne/g,'Gather']
];
const chapterEN={
'LAS CENIZAS DE VALORIA':'THE ASHES OF VALORIA','ALGO QUE DEFENDER':'SOMETHING TO DEFEND','MÁS ALLÁ DE LAS MURALLAS':'BEYOND THE WALLS','EL PRECIO DEL CRECIMIENTO':'THE PRICE OF GROWTH','PIEDRA PARA UN REINO':'STONE FOR A KINGDOM','FUEGO ANTIGUO':'ANCIENT FIRE','CÓDICE Y RELICARIO':'CODEX AND RELIQUARY','VOCES DE NARETH':'VOICES OF NARETH','PREPARATIVOS DE GUERRA':'PREPARATIONS FOR WAR','LA PRIMERA BRECHA':'THE FIRST BREACH'
};
function translateText(s){
 let x=String(s);const trim=x.trim();if(exact.has(trim))x=x.replace(trim,exact.get(trim));if(chapterEN[trim])x=x.replace(trim,chapterEN[trim]);
 for(const [a,b] of phrases)x=x.replace(a,b);return x;
}
function localizePage(){
 document.documentElement.lang=cfg.locale;
 if(cfg.locale!=='en')return;
 const skip=new Set(['SCRIPT','STYLE','TEXTAREA','INPUT']);
 const w=document.createTreeWalker(document.body,NodeFilter.SHOW_TEXT);const nodes=[];while(w.nextNode())nodes.push(w.currentNode);
 nodes.forEach(n=>{if(skip.has(n.parentElement?.tagName))return;const v=n.nodeValue;if(v&&/[A-Za-zÁÉÍÓÚÑáéíóúñ¿¡]/.test(v)){const t=translateText(v);if(t!==v)n.nodeValue=t}});
 document.querySelectorAll('[aria-label]').forEach(el=>{const a=el.getAttribute('aria-label');if(a)el.setAttribute('aria-label',translateText(a))});
}

/* ---------- meaningful economic choices ---------- */
const choices={
 4:{title:['PRIORIDAD DE RECONSTRUCCIÓN','REBUILDING PRIORITY'],copy:['Valoria ya sostiene varios frentes. Decide qué recibe primero los recursos recuperados.','Valoria now sustains several fronts. Decide where recovered resources go first.'],options:[
  {id:'works',icon:'🏗️',name:['Obras del reino','Kingdom works'],copy:['+320 madera · +220 piedra','+320 wood · +220 stone'],grant:{wood:320,stone:220}},
  {id:'supply',icon:'🌾',name:['Reservas y abastecimiento','Stores and supplies'],copy:['+220 madera · +320 comida','+220 wood · +320 food'],grant:{wood:220,food:320}},
  {id:'army',icon:'🏹',name:['Preparación militar','Military readiness'],copy:['+160 piedra · +160 comida · +120 Poder','+160 stone · +160 food · +120 Power'],grant:{stone:160,food:160,power:120}}
 ]},
 6:{title:['RUTA DE INVERSIÓN','INVESTMENT PATH'],copy:['La Forja abre nuevas demandas. No puedes priorizarlo todo a la vez.','The Forge creates new demands. You cannot prioritize everything at once.'],options:[
  {id:'industry',icon:'⚒️',name:['Industria','Industry'],copy:['+420 madera · +360 piedra','+420 wood · +360 stone'],grant:{wood:420,stone:360}},
  {id:'expedition',icon:'⚔️',name:['Expedición','Expedition'],copy:['+240 comida · +220 piedra · +160 Poder','+240 food · +220 stone · +160 Power'],grant:{food:240,stone:220,power:160}},
  {id:'reserve',icon:'⏳',name:['Reserva estratégica','Strategic reserve'],copy:['+260 de cada recurso','+260 of each resource'],grant:{wood:260,stone:260,food:260}}
 ]},
 8:{title:['NARETH CAMBIA EL PLAN','NARETH CHANGES THE PLAN'],copy:['Con más sistemas activos, el valor está en elegir dónde profundizar.','With more systems active, value comes from choosing where to deepen.'],options:[
  {id:'realm',icon:'🏰',name:['Fortificar Valoria','Fortify Valoria'],copy:['+520 madera · +420 piedra','+520 wood · +420 stone'],grant:{wood:520,stone:420}},
  {id:'march',icon:'🛡️',name:['Preparar la marcha','Prepare the march'],copy:['+420 comida · +220 Poder','+420 food · +220 Power'],grant:{food:420,power:220}},
  {id:'balanced',icon:'⚖️',name:['Mantener flexibilidad','Stay flexible'],copy:['+300 madera · +300 piedra · +300 comida','+300 wood · +300 stone · +300 food'],grant:{wood:300,stone:300,food:300}}
 ]},
 9:{title:['AUTONOMÍA DE VALORIA','VALORIA AUTONOMY'],copy:['Ya no hay una única ruta correcta. Elige la preparación que mejor encaje con tu estado actual.','There is no single correct route now. Choose the preparation that best matches your current state.'],options:[
  {id:'economy',icon:'📦',name:['Economía primero','Economy first'],copy:['+620 madera · +520 piedra','+620 wood · +520 stone'],grant:{wood:620,stone:520}},
  {id:'force',icon:'⚔️',name:['Fuerza primero','Force first'],copy:['+480 comida · +260 Poder','+480 food · +260 Power'],grant:{food:480,power:260}},
  {id:'adapt',icon:'🧭',name:['Adaptación','Adaptation'],copy:['+360 de cada recurso','+360 of each resource'],grant:{wood:360,stone:360,food:360}}
 ]}
};
function openChoice(level){
 const s=state(),def=choices[level];if(!def||s.developmentChoices?.[level])return false;
 const active=document.querySelector('[data-testid="v030-choice"]');
 if(active&&Number(active.dataset.level)===Number(level))return true;
 active?.closest('.e22-overlay')?.remove();
 const o=document.createElement('div');o.className='e22-overlay v030Overlay';o.innerHTML='<div class="e22-dialog choice030" data-testid="v030-choice" data-level="'+level+'"><small>'+tr('DECISIÓN DE DESARROLLO','DEVELOPMENT DECISION')+' · '+tr('BASTIÓN','BASTION')+' '+level+'</small><h1>'+def.title[cfg.locale==='en'?1:0]+'</h1><p>'+def.copy[cfg.locale==='en'?1:0]+'</p><div class="choiceGrid030">'+def.options.map(x=>'<button data-choice="'+x.id+'"><span>'+x.icon+'</span><b>'+x.name[cfg.locale==='en'?1:0]+'</b><small>'+x.copy[cfg.locale==='en'?1:0]+'</small></button>').join('')+'</div><p class="choiceFoot030">'+tr('Las tres rutas son viables. Esta elección cambia tu siguiente prioridad, no bloquea contenido.','All three routes are viable. This choice changes your next priority; it does not lock content.')+'</p></div>';document.body.appendChild(o);
 o.querySelectorAll('[data-choice]').forEach(b=>b.onclick=()=>applyChoice(level,b.dataset.choice,o));return true;
}
function applyChoice(level,id,o){
 const s=state(),def=choices[level],opt=def?.options.find(x=>x.id===id);if(!opt||s.developmentChoices?.[level])return false;
 const dc=Object.assign({},s.developmentChoices||{}, {[level]:{id,at:Date.now()}});
 const patch={developmentChoices:dc};
 for(const [k,v] of Object.entries(opt.grant)){patch[k]=(Number(s[k])||0)+v}
 update(patch);tone(660,.12,.035,'triangle');o?.remove();showToast(tr('DECISIÓN APLICADA','CHOICE APPLIED'),opt.name[cfg.locale==='en'?1:0]);return true;
}
function delegatedChoiceEvent(e){
 const b=e.target?.closest?.('[data-choice]');if(!b)return;
 const dialog=b.closest('[data-testid="v030-choice"]');if(!dialog)return;
 const level=Number(dialog.dataset.level);if(!choices[level])return;
 e.preventDefault();e.stopPropagation();
 applyChoice(level,b.dataset.choice,dialog.closest('.e22-overlay'));
}
function maybeChoice(){
 if(document.querySelector('[data-testid="v030-choice"]'))return;
 const s=state();for(const level of [4,6,8,9])if(s.bastionLevel>=level&&!s.developmentChoices?.[level]){if(QA&&new URLSearchParams(location.search).get('v030Manual')!=='1'){const def=choices[level],opt=def.options[2]||def.options[0],dc=Object.assign({},s.developmentChoices||{}, {[level]:{id:opt.id,at:Date.now(),qa:true}}),patch={developmentChoices:dc};for(const [k,v] of Object.entries(opt.grant))patch[k]=(Number(s[k])||0)+v;update(patch)}else openChoice(level);break}
}

/* ---------- progressive autonomy in chapter UI ---------- */
function tuneGuidance(){
 const s=state(),ch=Number(s.chapterProgress?.current||s.bastionLevel||1),root=ROOT();if(!root)return;
 root.dataset.guidance=ch>=9?'autonomous':ch>=8?'light':ch>=6?'reduced':'guided';
 if(ch>=6)root.querySelectorAll('.tutorialHand,.missionFocus027').forEach(el=>{el.classList.remove('missionFocus027');if(el.classList.contains('tutorialHand'))el.style.display='none'});
 if(ch>=8)root.querySelectorAll('[data-mission-go]').forEach(el=>el.remove());
 if(ch>=9){
   root.querySelectorAll('.guideHint0265').forEach(el=>el.remove());
   const drawer=root.querySelector('[data-testid="chapter-drawer"]');if(drawer&&!drawer.dataset.autonomy030){
     drawer.dataset.autonomy030='1';const list=drawer.querySelector('.missionList0267');if(list){
       const title=tr('OBJETIVOS GENERALES','GENERAL OBJECTIVES'),items=ch===9?
       [tr('Preparar una expedición completa','Prepare a complete expedition'),tr('Consolidar al menos 14.000 de Poder Total','Consolidate at least 14,000 Total Power'),tr('Superar la Prueba de Marcha','Pass the March Trial'),tr('Elevar el Bastión a nivel 10','Raise the Bastion to level 10')]:
       [tr('Derrotar al Heraldo','Defeat the Herald'),tr('Usar una intervención de héroe cuando aporte valor','Use a hero intervention when it adds value'),tr('Completar el asalto final','Complete the final assault')];
       list.innerHTML='<section class="generalObjectives030"><small>'+title+'</small>'+items.map(x=>'<p>◇ '+x+'</p>').join('')+'<em>'+tr('El juego ya no marca una ruta paso a paso. Tú decides el orden.','The game no longer marks a step-by-step route. You choose the order.')+'</em></section>';
     }
   }
 }
}

/* ---------- Power: first explanation, feedback and restrained milestones ---------- */
let lastPower=null;
const thresholds=[5000,10000,15000,20000];
function powerFromHud(){const t=ROOT()?.querySelector('[data-testid="power-total"] b')?.textContent||'';return Number(t.replace(/[^0-9]/g,''))||Number(state().power)||0}
function showToast(title,copy){
 const old=document.querySelector('.v030Toast');if(old)old.remove();const d=document.createElement('div');d.className='v030Toast';d.innerHTML='<b>'+htmlEscape(title)+'</b><small>'+htmlEscape(copy||'')+'</small>';document.body.appendChild(d);requestAnimationFrame(()=>d.classList.add('show'));setTimeout(()=>d.classList.remove('show'),2200);setTimeout(()=>d.remove(),2600)
}
function monitorPower(){
 const p=powerFromHud();if(lastPower==null){lastPower=p;return}if(p>lastPower){
   const delta=p-lastPower,hook=ROOT()?.querySelector('[data-testid="power-total"]');hook?.classList.add('powerRise030');setTimeout(()=>hook?.classList.remove('powerRise030'),900);
   if(delta>=80)showToast('+'+fmt(delta)+' '+tr('PODER','POWER'),tr('Tu desarrollo permanente ha aumentado.','Your permanent development has increased.'));
   tone(480+Math.min(320,delta/4),.08);
   let seen=[];try{seen=JSON.parse(localStorage.getItem(K.milestones)||'[]')}catch(_){}
   const hit=thresholds.find(x=>lastPower<x&&p>=x&&!seen.includes(x));if(hit){seen.push(hit);localStorage.setItem(K.milestones,JSON.stringify(seen));showPowerMilestone(hit)}
 }lastPower=p;
}
function showPowerMilestone(n){
 tone(740,.18,.04,'triangle');const o=document.createElement('div');o.className='powerMilestone030';o.innerHTML='<small>'+tr('HITO DE PODER','POWER MILESTONE')+'</small><b>⚔ '+fmt(n)+'</b><span>'+tr('Valoria alcanza una nueva escala de desarrollo.','Valoria reaches a new scale of development.')+'</span>';document.body.appendChild(o);setTimeout(()=>o.classList.add('show'),20);setTimeout(()=>o.remove(),2800)
}
let powerIntroPending=false;
function interceptPower(e){
 const b=e.target.closest?.('[data-power]');if(!b||localStorage.getItem(K.powerIntro))return;
 localStorage.setItem(K.powerIntro,'1');powerIntroPending=true;tone(560,.08);
}
function enhancePowerCard(){
 const card=document.querySelector('.e22-dialog.powerCard');if(!card||!powerIntroPending||card.querySelector('[data-testid="power-intro"]'))return;
 const primer=document.createElement('section');primer.className='powerIntro030';primer.dataset.testid='power-intro';
 primer.innerHTML='<small>'+tr('PODER TOTAL','TOTAL POWER')+'</small><b>'+tr('Tu progreso global en una cifra.','Your overall progression in one number.')+'</b><span>'+tr('Resume reino, ejército, héroes, equipo y colección. No decide por sí solo un combate: la composición y el Poder de Marcha siguen importando.','It summarizes kingdom, army, heroes, gear and collection. It does not decide a battle by itself: composition and March Power still matter.')+'</span>';
 const h=card.querySelector('h1');if(h)h.after(primer);else card.prepend(primer);powerIntroPending=false;
}

/* ---------- rankings: three simulated ladders, player + immediate rival always visible ---------- */
const profiles=['Casa Varyn','Orden del Alba','Bastión Nareth','Lobos de Khar','Guardia Umbría','Dominio Aster','Vigías de Orun','Casa Serev'];
function metrics(s){
 const wins=s.chapterProgress?.counters?.wins||{};return{
   power:powerFromHud(),
   corrupt:(Number(wins.spawnling)||0)+(Number(wins.ashStalker)||0)+(Number(wins.herald)||0)+(s.camp?1:0)+(s.boss?1:0)+(s.devourerDefeated?1:0)+(s.trialWon?1:0)+(s.finalWon?1:0),
   relics:new Set([...(s.codex||[]).map(x=>x?.id),...(s.cardsConsumed||[]).map(x=>x?.id||x)].filter(Boolean)).size
 }}
function ladder(type,value){
 const offs=type==='power'?[2300,1100,420,-280,-760,-1500,-2600,-3900]:type==='corrupt'?[6,3,1,-1,-2,-3,-5,-7]:[5,3,1,-1,-2,-3,-4,-6];
 let rows=profiles.map((name,i)=>({name,value:Math.max(type==='power'?100:0,value+offs[i])}));rows.push({name:tr('VALORIA · TÚ','VALORIA · YOU'),value,me:true});rows.sort((a,b)=>b.value-a.value);return rows
}
let rankTab='power';
function openRankings(tab=rankTab){
 rankTab=tab;document.querySelector('[data-testid="v030-ranking"]')?.closest('.e22-overlay')?.remove();const s=state(),m=metrics(s),v=m[tab],rows=ladder(tab,v),me=rows.findIndex(x=>x.me),neighbor=rows[me-1]||rows[me+1];
 const labels={power:[tr('Poder total','Total power'),'⚔'],corrupt:[tr('Corruptos eliminados','Corrupts defeated'),'☠'],relics:[tr('Colección de reliquias','Relic collection'),'✦']};
 const o=document.createElement('div');o.className='e22-overlay v030Overlay';o.innerHTML='<div class="e22-dialog rankingCard ranking030" data-testid="v030-ranking"><small>'+tr('PERFILES SIMULADOS · PROTOTIPO 4X','SIMULATED PROFILES · 4X PROTOTYPE')+'</small><h1>'+tr('CLASIFICACIONES','RANKINGS')+'</h1><div class="rankingTabs030">'+Object.keys(labels).map(k=>'<button data-rank-tab="'+k+'" class="'+(k===tab?'active':'')+'">'+labels[k][1]+' '+labels[k][0]+'</button>').join('')+'</div><div class="rivalStrip030"><small>'+tr('RIVAL INMEDIATO','IMMEDIATE RIVAL')+'</small><b>'+(neighbor?htmlEscape(neighbor.name)+' · '+fmt(neighbor.value):'—')+'</b></div><div class="rankingList">'+rows.map((r,i)=>'<div class="rankRow '+(r.me?'me':'')+'" '+(r===neighbor?'data-immediate-rival':'')+'><strong>#'+(i+1)+'</strong><span>'+htmlEscape(r.name)+'</span><b>'+labels[tab][1]+' '+fmt(r.value)+'</b></div>').join('')+'</div><p class="simNote030">'+tr('Estos perfiles son simulados. Comparan progreso; no predicen resultados de combate ni representan jugadores reales.','These profiles are simulated. They compare progression; they do not predict combat outcomes or represent real players.')+'</p><button class="btn" data-close>'+tr('VOLVER','BACK')+'</button></div>';document.body.appendChild(o);
 o.querySelectorAll('[data-rank-tab]').forEach(b=>b.onclick=()=>openRankings(b.dataset.rankTab));o.querySelector('[data-close]').onclick=()=>o.remove()
}
function interceptRank(e){const b=e.target.closest?.('[data-rank]');if(!b)return;e.preventDefault();e.stopPropagation();e.stopImmediatePropagation();tone(520,.06);openRankings()}

/* ---------- battle report: clear summary first, full detail on demand ---------- */
function enhanceBattleReport(){
 const r=document.querySelector('[data-testid="battle-report"]');if(!r||r.dataset.v030)return;r.dataset.v030='1';
 const h=r.querySelector('h1'),why=r.querySelector('.battleWhy0266'),compare=r.querySelector('.battleCompare0266'),rounds=r.querySelector('details');
 const reasonLines=[...(why?.querySelectorAll('p')||[])].map(x=>x.textContent.replace(/^•\s*/,''));const mainReason=reasonLines[0]||tr('Consulta los detalles para ver estadísticas e intercambios.','Open details to view stats and exchanges.');const intervention=reasonLines.find(x=>/habilidad|intervenci[oó]n|ability|skill/i.test(x));
 const summary=document.createElement('section');summary.className='battleSummary030';summary.dataset.testid='battle-report-summary';summary.innerHTML='<small>'+tr('RESUMEN','SUMMARY')+'</small><b>'+htmlEscape(h?.textContent||tr('Resultado resuelto','Resolved result'))+'</b><span>'+htmlEscape(mainReason)+'</span>'+(intervention?'<em>'+htmlEscape(intervention)+'</em>':'');
 h?.after(summary);
 if(compare||why||rounds){const d=document.createElement('details');d.className='battleDetails030';d.dataset.testid='battle-report-details';d.innerHTML='<summary>'+tr('VER DETALLES COMPLETOS','VIEW FULL DETAILS')+'</summary><div></div>';const box=d.querySelector('div');[compare,why].filter(Boolean).forEach(x=>box.appendChild(x));if(rounds){const log=rounds.querySelector('.roundLog0266');if(log){const wrap=document.createElement('section');wrap.className='roundsFull030';wrap.innerHTML='<b>'+tr('INTERCAMBIOS','EXCHANGES')+'</b>';wrap.appendChild(log);box.appendChild(wrap)}rounds.remove()}summary.after(d)}
}

/* ---------- ceremonies audit: module only creates milestone ceremony, routine feedback stays compact ---------- */
function bindSfx(){
 document.addEventListener('click',e=>{if(e.target.closest('[data-choice]'))return;const b=e.target.closest('button');if(b&&cfg.audio.sfx&&!b.closest('[data-testid="v030-settings-dialog"]'))tone(360,.035,.012)},true);
}

/* ---------- styles ---------- */
function style(){
 if(document.getElementById('v030-depth-style'))return;const st=document.createElement('style');st.id='v030-depth-style';st.textContent=`
#eldoria-core-loop .worldClock030{display:grid;place-items:center;min-height:28px;padding:3px 7px;border:1px solid #ffffff14;border-radius:7px;background:#091014b8;color:#aab5b8;font:700 6px Arial;letter-spacing:.06em;white-space:nowrap}
#eldoria-core-loop .settings030{min-width:44px}.v030Overlay{z-index:3200!important;pointer-events:auto!important}.v030Overlay .e22-dialog,.v030Overlay button,.v030Overlay input{pointer-events:auto!important}.settingsCard030,.choice030,.ranking030,.powerIntro030{max-width:560px!important}
.settingRow030{display:flex;align-items:center;justify-content:space-between;gap:14px;padding:12px 0;border-bottom:1px solid #ffffff14}.settingRow030 b,.settingRow030 small{display:block}.settingRow030 small{margin-top:3px;color:#9ca7aa;font:9px Arial}.settingRow030 input{width:22px;height:22px}.seg030{display:flex}.seg030 button{min-width:46px;padding:8px;border:1px solid #ffffff22;background:#10161b;color:#aaa}.seg030 button.active{border-color:#d5b869;background:#332815;color:#f4d98c}
.choiceGrid030{display:grid;grid-template-columns:repeat(3,1fr);gap:8px;margin:16px 0}.choiceGrid030 button{min-height:150px;padding:14px 9px;border:1px solid #ffffff22;border-radius:9px;background:#10171c;color:#e9e4d9;text-align:center}.choiceGrid030 button:hover,.choiceGrid030 button:focus{border-color:#d5b869;background:#201b10}.choiceGrid030 span,.choiceGrid030 b,.choiceGrid030 small{display:block}.choiceGrid030 span{font-size:30px}.choiceGrid030 b{margin:8px 0;color:#f0d58c}.choiceGrid030 small{font:9px/1.4 Arial;color:#afb8bb}.choiceFoot030{font-size:10px!important;color:#9ea7aa!important}
.rankingTabs030{display:flex;gap:5px;overflow-x:auto;margin:12px 0}.rankingTabs030 button{flex:0 0 auto;padding:8px;border:1px solid #ffffff22;background:#10161a;color:#aeb5b6;font:8px Arial}.rankingTabs030 button.active{border-color:#d5b869;background:#2c2313;color:#f4d98b}.rivalStrip030{padding:9px;border-left:3px solid #b67bd6;background:#15111a}.rivalStrip030 small,.rivalStrip030 b{display:block}.rivalStrip030 small{font:6px Arial;color:#bda2cb}.rivalStrip030 b{margin-top:3px;font:10px Arial}.ranking030 [data-immediate-rival]{outline:1px dashed #b67bd677}.simNote030{font-size:9px!important;color:#8f999d!important}
.powerIntro030{margin:10px 0;padding:10px;border:1px solid #d5b86955;background:#15130d;text-align:left}.powerIntro030 small,.powerIntro030 b,.powerIntro030 span{display:block}.powerIntro030 small{font:7px Arial;color:#d5b869}.powerIntro030 b{margin:4px 0;font:12px Arial;color:#f0dc9e}.powerIntro030 span{font:9px/1.45 Arial;color:#c2c9c8}
.battleSummary030{margin:12px 0;padding:12px;border:1px solid #d5b86955;background:#15130d}.battleSummary030 small,.battleSummary030 b,.battleSummary030 span{display:block}.battleSummary030 small{font:7px Arial;color:#d5b869}.battleSummary030 b{margin:4px 0;font:15px Georgia}.battleSummary030 span{font:9px/1.45 Arial;color:#c5cbca}.battleDetails030{margin:10px 0;border:1px solid #ffffff19;background:#0c1115}.battleDetails030>summary{padding:11px;color:#e7cb87;font:800 8px Arial;cursor:pointer}.battleDetails030>div{padding:0 10px 10px}
.generalObjectives030{padding:10px;border:1px solid #d5b86955;background:#12150f}.generalObjectives030 small{color:#d5b869;font:700 7px Arial}.generalObjectives030 p{margin:8px 0!important;font:9px/1.35 Arial!important}.generalObjectives030 em{display:block;margin-top:10px;color:#9fa8a9;font:italic 8px/1.35 Arial}
#eldoria-core-loop[data-guidance="reduced"] .chapterCompact0267{opacity:.92}#eldoria-core-loop[data-guidance="light"] .chapterCompact0267{width:min(230px,60vw);opacity:.84}#eldoria-core-loop[data-guidance="autonomous"] .chapterCompact0267{width:min(215px,58vw);opacity:.78}
.v030Toast{position:fixed;z-index:1000;left:50%;top:82px;transform:translate(-50%,-12px);opacity:0;min-width:210px;max-width:80vw;padding:9px 13px;border:1px solid #d5b86977;border-radius:9px;background:#0a1015f2;color:#eee;text-align:center;transition:.22s}.v030Toast.show{opacity:1;transform:translate(-50%,0)}.v030Toast b,.v030Toast small{display:block}.v030Toast small{margin-top:3px;color:#aeb7b8;font:8px Arial}
.powerRise030{animation:powerRise030 .85s}.powerMilestone030{position:fixed;z-index:1000;left:50%;top:18%;transform:translate(-50%,-10px) scale(.96);opacity:0;padding:16px 24px;border:1px solid #d5b86988;background:#15130df5;text-align:center;transition:.3s}.powerMilestone030.show{opacity:1;transform:translate(-50%,0) scale(1)}.powerMilestone030 small,.powerMilestone030 b,.powerMilestone030 span{display:block}.powerMilestone030 small{font:7px Arial;color:#d5b869}.powerMilestone030 b{margin:6px;font:24px Georgia;color:#f3dc99}.powerMilestone030 span{font:9px Arial;color:#bbc1bf}@keyframes powerRise030{40%{filter:brightness(1.6);transform:scale(1.08);box-shadow:0 0 20px #d5b86977}}
body:has(.qaLaunchBtn) #eldoria-core-loop .settings030{margin-right:44px}
@media(max-width:620px){#eldoria-core-loop .worldClock030{position:absolute;right:6px;top:46px;min-height:20px;padding:2px 5px;font-size:5px;opacity:.78}#eldoria-core-loop .settings030 small{display:none}.choiceGrid030{grid-template-columns:1fr}.choiceGrid030 button{min-height:88px;display:grid;grid-template-columns:40px 1fr;grid-template-rows:auto auto;text-align:left;column-gap:8px}.choiceGrid030 button span{grid-row:1/3;align-self:center}.choiceGrid030 button b{margin:0}.choice030{max-height:88dvh!important;overflow:auto!important}.ranking030{max-height:88dvh!important;overflow:auto!important}}
`;document.head.appendChild(st)
}

/* ---------- public QA seam ---------- */
window.ELDORIA_V030={
 version:'candidate',
 openChoice,openRankings,setLocale,
 metrics:()=>metrics(state()),
 settings:()=>JSON.parse(JSON.stringify(cfg)),
 audit:()=>({breachExpansion:false,pvpReal:false,newHeroes:false,newBuildings:false,choices:[4,6,8,9],rankings:['power','corrupt','relics'],battleReportLevels:2})
};

function cycle(){
 ensureHudTools();tuneGuidance();enhancePowerCard();enhanceBattleReport();monitorPower();localizePage();maybeChoice()
}
style();bindSfx();document.documentElement.lang=cfg.locale;
document.addEventListener('pointerdown',()=>{if(cfg.audio.music)startAmbient()},{once:true});
document.addEventListener('pointerup',delegatedChoiceEvent,true);
document.addEventListener('click',e=>{delegatedChoiceEvent(e);interceptRank(e);interceptPower(e)},true);
const mo=new MutationObserver(()=>queueMicrotask(cycle));mo.observe(document.body,{subtree:true,childList:true});
setInterval(()=>{updateClock();monitorPower()},1000);
setTimeout(cycle,50);
})();