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
const longEN=new Map(Object.entries({"Valoria vuelve a respirar entre ruinas. Recupera lo necesario para levantar un reino, no solo una muralla.":"Valoria breathes again among the ruins. Recover what is needed to raise a kingdom, not just a wall.","Reconstruye el Aserradero":"Rebuild the Sawmill","Recupera 600 madera":"Recover 600 wood","Recupera 500 piedra":"Recover 500 stone","Despeja la ruta de Corruptos":"Clear the route of Corrupts","Eleva el Bastión a nivel 2":"Raise the Bastion to level 2","Las murallas necesitan soldados y las primeras amenazas deben empezar a tener explicación.":"The walls need soldiers, and the first threats need to start making sense.","Construye el Cuartel":"Build the Barracks","Alcanza 2.250 de Poder de expedición":"Reach 2,250 Expedition Power","Derrota un Engendro de la Fisura":"Defeat a Rift Spawn","Eleva el Bastión a nivel 3":"Raise the Bastion to level 3","Valoria aprende a alimentarse, cazar y sostener una frontera más amplia.":"Valoria learns to feed itself, hunt, and sustain a wider frontier.","Construye el Granero":"Build the Granary","Reúne 500 comida":"Gather 500 food","Completa 2 cacerías":"Complete 2 hunts","Somete la Fisura":"Subdue the Fissure","Prepara la infraestructura de Valoria":"Prepare Valoria's infrastructure","Eleva el Bastión a nivel 4":"Raise the Bastion to level 4","Crecer ya no consiste en acumular: consiste en sostener varios sistemas a la vez.":"Growth is no longer about hoarding; it is about sustaining several systems at once.","Aserradero a nivel 2":"Sawmill to level 2","Granero a nivel 2":"Granary to level 2","Cuartel a nivel 2":"Barracks to level 2","Elige una prioridad para Valoria":"Choose a priority for Valoria","Eleva el Bastión a nivel 5":"Raise the Bastion to level 5","La economía se diversifica. Piedra, tropas y amenazas compiten por la misma atención.":"The economy diversifies. Stone, troops, and threats compete for the same attention.","Construye la Cantera de Valoria":"Build Valoria's Quarry","Mantén 1.200 piedra disponible":"Keep 1,200 stone available","Derrota al Acechador de Ceniza":"Defeat the Ash Stalker","Eleva el Bastión a nivel 6":"Raise the Bastion to level 6","La Forja convierte descubrimientos del Mundo en preparación real para la expedición.":"The Forge turns discoveries from the World into real expedition preparation.","Reconstruye la Forja":"Rebuild the Forge","Derrota al Devorador de Éter":"Defeat the Aether Devourer","Forja tu primer equipo":"Forge your first piece of gear","Alcanza 2.800 de Poder de expedición":"Reach 2,800 Expedition Power","Eleva el Bastión a nivel 7":"Raise the Bastion to level 7","El Códice registra lo que Eldoria aprende. El Relicario reúne las cartas y decisiones que nacen de esos descubrimientos.":"The Codex records what Eldoria learns. The Reliquary gathers the cards and decisions born from those discoveries.","Consulta el Códice de Eldoria":"Open the Eldoria Codex","Decide el destino de una Reliquia":"Decide the fate of a Relic","Completa la Práctica guiada":"Complete guided Practice","Eleva el Bastión a nivel 8":"Raise the Bastion to level 8","La composición empieza a importar más que la suma bruta de Poder.":"Composition starts to matter more than raw Power.","Investiga las ruinas de Nareth":"Investigate the ruins of Nareth","Equipa a un héroe":"Equip a hero","Alcanza 3.200 de Poder de expedición":"Reach 3,200 Expedition Power","Eleva el Bastión a nivel 9":"Raise the Bastion to level 9","Reino, ejército, héroes, equipo, Códice y Relicario deben empezar a funcionar como un único sistema.":"Kingdom, army, heroes, gear, Codex, and Reliquary must begin to work as one system.","Prepara una expedición completa":"Prepare a complete expedition","Alcanza 14.000 de Poder total":"Reach 14,000 Total Power","Mantén 3 Reliquias descubiertas":"Keep 3 Relics discovered","Supera la Prueba de Marcha":"Pass the March Trial","Eleva el Bastión a nivel 10":"Raise the Bastion to level 10","Ya no se trata de recolectar. Se trata de demostrar que Valoria sabe prepararse y luchar.":"This is no longer about gathering. It is about proving that Valoria knows how to prepare and fight.","Derrota al Heraldo de la Fisura":"Defeat the Rift Herald","Intervén con una habilidad de héroe":"Intervene with a hero ability","Completa el asalto final del Arco I":"Complete the final assault of Arc I","Golpea con fuerza al inicio, pero expone su defensa después del primer choque.":"Hits hard at the start, but exposes its defense after the first clash.","Sus pulsos castigan marchas mal preparadas. Una intervención de héroe puede cambiar el intercambio.":"Its pulses punish poorly prepared marches. A hero intervention can change the exchange.","Reduce el siguiente golpe enemigo un 45%.":"Reduces the next enemy hit by 45%.","Inflige daño inmediato y debilita la defensa enemiga.":"Deals immediate damage and weakens enemy defense.","Recupera vida de la marcha y reduce daño del siguiente golpe.":"Restores march health and reduces damage from the next hit.","Tu Poder superó el umbral de la presa.":"Your Power exceeded the prey threshold.","La presa superó el Poder de tu expedición.":"The prey exceeded your expedition Power.","Tu Ruptura abrió mejor la defensa rival.":"Your Break opened the enemy defense more effectively.","La Ruptura enemiga redujo tu margen defensivo.":"Enemy Break reduced your defensive margin.","Tu Defensa absorbió una parte importante del daño.":"Your Defense absorbed a significant part of the damage.","La presión enemiga superó tu Defensa.":"Enemy pressure overcame your Defense.","Emboscada no llegó a activarse.":"Ambush did not activate.","La habilidad del héroe alteró el intercambio a tu favor.":"The hero ability changed the exchange in your favor.","La habilidad recuperó vida de la marcha.":"The ability restored march health.","Valoria ha renacido. La Brecha sigue abierta más allá de la frontera.":"Valoria has been reborn. The Breach remains open beyond the frontier.","Prepara la marcha y enfrenta la amenaza que sostiene la Brecha.":"Prepare the march and face the threat sustaining the Breach.","Configura a Aldric con Lyra o Maelis y demuestra que la expedición está preparada.":"Configure Aldric with Lyra or Maelis and prove the expedition is ready.","Una señal de supervivientes llega desde las ruinas. Investígala.":"A signal from survivors reaches you from the ruins. Investigate it.","Una reliquia imposible ha aparecido en Valoria. Descubre dónde pertenece.":"An impossible relic has appeared in Valoria. Discover where it belongs.","Valoria puede reconstruir su Forja. El equipo abrirá una nueva capa de preparación.":"Valoria can rebuild its Forge. Gear will open a new layer of preparation.","Una criatura de élite custodia algo que la Forja podría trabajar.":"An elite creature guards something the Forge could work with.","Lleva la Ascua de Éter a la Forja y completa el ciclo de preparación.":"Take the Aether Ember to the Forge and complete the preparation cycle.","Bastión V permite construir la Cantera de Valoria para generar piedra de forma continua.":"Bastion V allows Valoria's Quarry to be built for continuous stone production.","El nivel IV consolida Valoria. Reúne recursos para alcanzar el V y desbloquear la Cantera.":"Level IV consolidates Valoria. Gather resources to reach V and unlock the Quarry.","Has abierto la frontera, alimentado Valoria y sometido la Fisura. Decide cuándo consolidar el reino.":"You have opened the frontier, fed Valoria, and subdued the Fissure. Decide when to consolidate the kingdom.","Bastión III abre la economía de comida. Construye el Granero y explora una frontera más amplia.":"Bastion III opens the food economy. Build the Granary and explore a wider frontier.","Algo ha cambiado en la frontera. Investiga la nueva Fisura y las huellas que la rodean.":"Something has changed on the frontier. Investigate the new Fissure and the traces around it.","Lyra conoce las marcas de la Brecha. Regresa con ella y asalta la Fisura.":"Lyra knows the marks of the Breach. Return with her and assault the Fissure.","Caza, explora y prepárate para la amenaza que ha surgido más allá de Valoria.":"Hunt, explore, and prepare for the threat that has arisen beyond Valoria.","El Bastión II ha abierto espacio para una nueva estructura militar. Construye el Cuartel.":"Bastion II has opened space for a new military structure. Build the Barracks.","Recluta tropas y reúne recursos para abrir una frontera mayor. Algo oscuro sigue creciendo más allá de las rutas conocidas.":"Recruit troops and gather resources to open a larger frontier. Something dark keeps growing beyond the known routes.","Toca el Aserradero en ruinas y devuelve vida al reino.":"Tap the ruined Sawmill and bring the kingdom back to life.","Valoria no puede crecer sola. Sal al mundo por recursos.":"Valoria cannot grow alone. Go out into the world for resources.","Los Corruptos bloquean la ruta. Tu ejército puede resolverlo.":"Corrupts block the route. Your army can clear it.","Ya tienes los recursos y la ruta asegurada. Levanta el Bastión.":"You have the resources and the route is secure. Raise the Bastion.","Consigue lo que falta aquí":"Get what is missing here","Ve al Mundo por recursos":"Go to the World for resources","Mejora este edificio":"Upgrade this building","Nuevo sistema: Héroes":"New system: Heroes","Nivel máximo":"Maximum level","Requisitos de progreso":"Progress requirements","Disponible para construir":"Available to build","Producción · +2 piedra/s":"Production · +2 stone/s","Coto de caza":"Hunting ground","Veta de piedra":"Stone vein","Jabalí de roca":"Rock Boar","Engendro de la Fisura":"Rift Spawn","Acechador de Ceniza":"Ash Stalker","Heraldo de la Fisura":"Rift Herald","Devorador de Éter":"Aether Devourer","Ruinas de Nareth":"Nareth Ruins","Prueba de Marcha":"March Trial","Corazón de la Brecha":"Heart of the Breach","Material anómalo":"Anomalous material","Ruta + héroe":"Route + hero","Progreso expedición":"Expedition progress","Un territorio mayor que una sola pantalla":"A territory larger than a single screen","Volver al reino":"Return to the kingdom","Poder recomendado":"Recommended Power","Amenaza temporal · intervención de héroe":"Timed threat · hero intervention","Élite de la Brecha · material desconocido":"Breach elite · unknown material","Señal de supervivientes · historia de Maelis":"Survivor signal · Maelis story","Composición de expedición":"Expedition composition","No hay contenido en esta pestaña":"There is no content in this tab","Las recompensas y objetos aparecerán aquí automáticamente según su utilidad.":"Rewards and items will appear here automatically according to their use.","Gestiona el equipamiento desde Héroes.":"Manage gear from Heroes.","Los aceleradores se aplican desde una tarea de construcción, mejora o entrenamiento que esté en curso.":"Speedups are applied from an active construction, upgrade, or recruitment task.","Selecciona un objeto":"Select an item","El Arcón ordena su contenido por utilidad. Los recursos básicos siguen en el HUD y las Reliquias pertenecen al Códice.":"The Chest organizes its contents by use. Basic resources remain in the HUD and Relics belong in the Codex.","Toca un objeto para ver qué es, de dónde viene y para qué sirve.":"Tap an item to see what it is, where it came from, and what it is for.","Una Ascua de Éter está lista para ser trabajada.":"An Aether Ember is ready to be worked.","RECETA DISPONIBLE":"RECIPE AVAILABLE","Hoja de Éter":"Aether Blade","Arma · +180 Poder cuando se equipa":"Weapon · +180 Power when equipped","EQUIPO FORJADO EN EL ARCÓN":"FORGED GEAR IN THE CHEST","Disponible para equipar en Héroes":"Available to equip in Heroes","Aún no has forjado ningún arma.":"You have not forged any weapons yet.","Necesitas una Ascua de Éter.":"You need an Aether Ember.","Hoja de Éter añadida al Arcón. Equípala desde Héroes.":"Aether Blade added to the Chest. Equip it from Heroes.","La primera marca":"The first mark","Una corrupción violeta altera rutas, ruinas y criaturas. Su origen sigue sin estar explicado.":"A violet corruption alters routes, ruins, and creatures. Its origin remains unexplained.","Símbolos recuperados junto a una Fisura. El Códice registra el hallazgo; su uso aún es desconocido.":"Symbols recovered near a Fissure. The Codex records the discovery; their use remains unknown.","Materia alterada que permanece después de algunos encuentros con la Brecha.":"Altered matter left behind after some encounters with the Breach.","Depredador de las rutas de Valoria. La caza enseña a medir Poder de expedición.":"Predator of Valoria's routes. Hunting teaches how to read Expedition Power.","Bestia resistente de las zonas pedregosas. Su fuerza exige una expedición preparada.":"A resilient beast from rocky areas. Its strength demands a prepared expedition.","Amenaza común nacida cerca de zonas corruptas. ATQ, DEF, VIDA y RUPTURA empiezan a importar.":"A common threat born near corrupted zones. ATK, DEF, HP, and BREAK start to matter.","Entidad de mayor rango vinculada a la Brecha. Su presencia altera el campo de batalla.":"A higher-rank entity tied to the Breach. Its presence changes the battlefield.","El reino que reconstruyes. Ciudad, rutas y producción forman el centro de tu expansión.":"The kingdom you rebuild. City, routes, and production form the center of your expansion.","Ruinas antiguas donde la historia de Eldoria comienza a conectarse con voces del pasado.":"Ancient ruins where Eldoria's history begins to connect with voices from the past.","Material extraño recuperado de amenazas mayores. La Forja puede darle forma.":"Strange material recovered from greater threats. The Forge can shape it.","Guardián de Valoria. Tanque y figura central de la reconstrucción.":"Guardian of Valoria. Tank and central figure of the reconstruction.","No reconstruiremos Valoria siguiendo órdenes sueltas. Dividiremos el camino en capítulos: objetivos reales que hagan crecer el reino y nos acerquen a entender la Brecha.":"We will not rebuild Valoria by following disconnected orders. We will divide the path into chapters: real objectives that grow the kingdom and bring us closer to understanding the Breach.","El panel de Capítulos muestra tu objetivo actual y su progreso. Tócalo para ver todas las misiones. Las acciones del Reino y del Mundo avanzan las misiones automáticamente; completar un capítulo concede una recompensa importante.":"The Chapters panel shows your current objective and progress. Tap it to see all missions. Kingdom and World actions advance missions automatically; completing a chapter grants an important reward.","Reduce tiempo real de construcción, mejora o entrenamiento. Esperar sigue siendo perfectamente válido.":"Reduces real construction, upgrade, or recruitment time. Waiting remains perfectly valid.","Reduce esta espera con un Acelerador Universal. Puedes guardarlos para más adelante.":"Reduce this wait with a Universal Speedup. You can save them for later."}));
const fallbackWords=[
[/\bBastión\b/g,'Bastion'],[/\bBrecha\b/g,'Breach'],[/\bFisura\b/g,'Fissure'],[/\bReino\b/g,'Kingdom'],[/\breino\b/g,'kingdom'],[/\bMundo\b/g,'World'],[/\bmundo\b/g,'world'],[/\bMundos\b/g,'Worlds'],[/\bmundos\b/g,'worlds'],[/\bHéroes\b/g,'Heroes'],[/\bhéroe\b/g,'hero'],[/\bhéroes\b/g,'heroes'],[/\bReliquias\b/g,'Relics'],[/\bReliquia\b/g,'Relic'],[/\breliquia\b/g,'relic'],[/\bCódice\b/g,'Codex'],[/\bArcón\b/g,'Chest'],[/\bCuartel\b/g,'Barracks'],[/\bGranero\b/g,'Granary'],[/\bCantera\b/g,'Quarry'],[/\bForja\b/g,'Forge'],[/\bAserradero\b/g,'Sawmill'],[/\bPoder\b/g,'Power'],[/\bpoder\b/g,'power'],[/\bmadera\b/g,'wood'],[/\bpiedra\b/g,'stone'],[/\bcomida\b/g,'food'],[/\bexpedición\b/g,'expedition'],[/\bmisiones\b/g,'missions'],[/\bmisión\b/g,'mission'],[/\bcapítulos\b/g,'chapters'],[/\bcapítulo\b/g,'chapter'],[/\bnivel\b/g,'level'],[/\btropas\b/g,'troops'],[/\bejército\b/g,'army'],[/\bdefensa\b/g,'defense'],[/\bataque\b/g,'attack'],[/\bvida\b/g,'health'],[/\bamenaza\b/g,'threat'],[/\bfrontera\b/g,'frontier'],[/\brecursos\b/g,'resources'],[/\brecurso\b/g,'resource'],[/\brecompensa\b/g,'reward'],[/\bequipo\b/g,'gear'],[/\bconstrucción\b/g,'construction'],[/\bmejora\b/g,'upgrade'],[/\bentrenamiento\b/g,'training'],[/\bpreparación\b/g,'preparation'],[/\bcorruptos\b/gi,'Corrupts'],[/\bCerrar\b/g,'Close'],[/\bVolver\b/g,'Back'],[/\bAbrir\b/g,'Open'],[/\bSelecciona\b/g,'Select'],[/\bDisponible\b/g,'Available'],[/\bBloqueado\b/g,'Locked'],[/\bComún\b/g,'Common'],[/\bÉpica\b/g,'Epic'],[/\bRara\b/g,'Rare'],[/\bLegendaria\b/g,'Legendary'],[/\bIndestructible\b/g,'Indestructible']
];
function translateText(s){
 let x=String(s),trim=x.trim();
 if(longEN.has(trim))x=x.replace(trim,longEN.get(trim));
 trim=x.trim();if(exact.has(trim))x=x.replace(trim,exact.get(trim));if(chapterEN[trim])x=x.replace(trim,chapterEN[trim]);
 for(const [a,b] of phrases)x=x.replace(a,b);for(const [a,b] of fallbackWords)x=x.replace(a,b);return x;
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
 const s=state(),eligible=[4,6,8,9].filter(level=>s.bastionLevel>=level&&!s.developmentChoices?.[level]);
 if(!eligible.length)return;
 if(QA&&new URLSearchParams(location.search).get('v030Manual')!=='1'){
   const dc=Object.assign({},s.developmentChoices||{}),patch={};let resourceDelta={wood:0,stone:0,food:0,power:0};
   for(const level of eligible){const def=choices[level],opt=def.options[2]||def.options[0];dc[level]={id:opt.id,at:Date.now(),qa:true};for(const [k,v] of Object.entries(opt.grant))resourceDelta[k]=(resourceDelta[k]||0)+v}
   patch.developmentChoices=dc;for(const [k,v] of Object.entries(resourceDelta))if(v)patch[k]=(Number(s[k])||0)+v;
   update(patch);return;
 }
 openChoice(eligible[0]);
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
 version:'0.30',
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