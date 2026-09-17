import fs from 'node:fs';
import path from 'node:path';
import zlib from 'node:zlib';
import crypto from 'node:crypto';

const root=process.cwd();
const parts=['d0.txt','r1.txt','r2.txt','r3.txt','r4.txt','r5a.txt','r5b.txt','r6a.txt','r6b.txt'];
const encoded=parts.map(f=>fs.readFileSync(path.join(root,'v0195',f),'utf8').trim()).join('');
let html=zlib.gunzipSync(Buffer.from(encoded,'base64')).toString('utf8');
for(const file of ['r2.json','r3.json','r4.json','r5.json','r6.json']){
  const patches=JSON.parse(fs.readFileSync(path.join(root,'v0195-clean',file),'utf8'));
  for(const [from,to] of patches){
    if(!html.includes(from)) throw new Error(`Missing clean integration marker in ${file}: ${from.slice(0,80)}`);
    html=html.replace(from,to);
  }
}
const cleanHash=crypto.createHash('sha256').update(html).digest('hex');
const expectedClean='03002b840f9a6e11212cb1825f014304f36c3531c3b59f656b39a7be795f03ba';
if(cleanHash!==expectedClean) throw new Error(`Validated r6 baseline mismatch: ${cleanHash}`);

const replace=(from,to)=>{ if(html.includes(from)) html=html.replace(from,to); };
replace("version:'0.19.5-r6'","version:'0.19.5-r7'");
replace("BUILD_SCHEMA='0195-r6'","BUILD_SCHEMA='0195-r7'");
replace('La corona desapareció. Las murallas resisten, pero el reino se apaga.','La corona desapareció. Valoria resiste entre cenizas.');
replace('Una herida violeta corrompe bosques, criaturas y caminos. Nadie conoce todavía su origen.','Una herida violeta está deformando el mundo. Nadie conoce su origen.');
replace('Los edificios principales son ruinas. Devuélvelos a la vida uno a uno y aprende qué sostiene tu reino.','Levanta de nuevo lo esencial. Después cruzaremos las puertas.');
replace('Reconstruir lleva tiempo. En el juego final, cada estructura tendrá su propia duración.','Valoria cambia con cada reconstrucción.');
replace('Valoria vuelve a sostenerse. Ahora debes aprender a fortalecerla, cruzar sus puertas y descubrir qué ocurre fuera.','Valoria vuelve a sostenerse. Fortalécela una vez más y cruza las puertas.');
replace('El Granero funciona, pero puede dar más. Mejóralo y después saldremos al mundo.','Mejora el Granero. Después salimos.');
replace('id="upgrade" class="primary">Mejorar ahora','id="upgrade" data-testid="upgrade-building" class="primary">Mejorar ahora');
replace('id="begin-auto" class="primary">Comenzar batalla semiautomática','id="begin-auto" data-testid="begin-auto-combat" class="primary">Comenzar batalla semiautomática');
replace('id="claim" class="primary">Guardar en el Archivo','id="claim" data-testid="claim-first-card" class="primary">Guardar en el Archivo');
replace('id="equip-relic" class="primary">Equipar reliquia','id="equip-relic" data-testid="equip-relic" class="primary">Equipar reliquia');
replace('id="equipment-continue" class="primary">Continuar','id="equipment-continue" data-testid="equipment-continue" class="primary">Continuar');
replace('id="close-power" class="secondary">Cerrar','id="close-power" data-testid="close-power" class="secondary">Cerrar');
html=html.replaceAll('id="enemy" class="enemy objective"','id="enemy" data-testid="expedition-enemy" class="enemy objective"').replaceAll('id="return" class="return"','id="return" data-testid="return-kingdom" class="return"');

// Narrative guidance layer: Aldric provides short, contextual motivation without blocking play.
const css=`\n/* r7 consolidated polish */\nbutton:focus-visible{outline:2px solid currentColor;outline-offset:3px}.modal{overscroll-behavior:contain}.reward-modal{animation:rewardArrival .32s cubic-bezier(.2,.9,.25,1.15)}@keyframes rewardArrival{from{transform:translateY(12px) scale(.985);opacity:.4}to{transform:none;opacity:1}}.gate.objective{animation:objectivePulse 1.8s ease-in-out infinite}@keyframes objectivePulse{50%{filter:drop-shadow(0 0 13px rgba(215,180,91,.52));transform:translateY(-2px)}}.aldric-guide{position:fixed;left:12px;right:12px;bottom:max(12px,env(safe-area-inset-bottom));z-index:28;display:flex;gap:11px;align-items:flex-start;max-width:560px;margin:auto;padding:12px 14px;border:1px solid rgba(215,180,91,.42);border-radius:15px;background:linear-gradient(145deg,rgba(20,18,24,.97),rgba(35,27,34,.97));box-shadow:0 12px 35px rgba(0,0,0,.45);animation:aldricIn .25s ease-out}.aldric-guide .portrait{width:42px;height:42px;flex:0 0 42px;border-radius:50%;display:grid;place-items:center;background:radial-gradient(circle at 35% 30%,#d7b45b,#705129 55%,#261d1b);border:1px solid #d7b45b;font-size:21px}.aldric-guide .speech{font-size:13px;line-height:1.42}.aldric-guide .speech b{display:block;color:#e7c878;letter-spacing:.04em;margin-bottom:2px}.aldric-guide .dismiss{margin-left:auto;padding:2px 5px;border:0;background:transparent;color:#aaa;font-size:18px;min-width:auto}.aldric-guide .reason{display:block;color:#bfb7ae;margin-top:3px}.quest-context{color:#d7b45b;font-weight:600}@keyframes aldricIn{from{transform:translateY(16px);opacity:0}to{transform:none;opacity:1}}@media(max-width:430px){.modal{max-height:88dvh;overflow:auto}.combat-actions{position:sticky;bottom:0;z-index:3;padding-bottom:max(8px,env(safe-area-inset-bottom));backdrop-filter:blur(8px)}.intent-choices button{min-height:68px}.hero-unit{scroll-margin-top:12px}.aldric-guide{left:8px;right:8px;padding:10px 11px}.aldric-guide .speech{font-size:12.5px}}@media(prefers-reduced-motion:reduce){*,*::before,*::after{animation-duration:.01ms!important;animation-iteration-count:1!important;transition-duration:.01ms!important;scroll-behavior:auto!important}}\n`;
html=html.replace('</style>',css+'</style>');

const guideScript=`\n<script>\n(()=>{\n const KEY='eldoria-aldric-guide-r7';\n let seen={};try{seen=JSON.parse(localStorage.getItem(KEY)||'{}')}catch{}\n const guides=[\n  {id:'rebuild',match:/Reconstru|ruinas|Granero|Aserradero|Cantera|Forja|Cuartel/i,text:'Valoria no sobrevivirá solo con murallas.',reason:'Recupera primero alimento, madera, piedra y tropas. Cada edificio devuelve una función al reino.'},\n  {id:'first-expedition',match:/expedici|salir al mundo|cruzar.*puerta/i,text:'Las murallas ya aguantan. Ahora necesitamos saber qué hay fuera.',reason:'Prepara la expedición y sigue la amenaza de la Brecha.'},\n  {id:'combat',match:/batalla|combate|enemigo/i,text:'No necesitas dirigir cada golpe.',reason:'Tus tropas luchan solas. Intervén cuando una habilidad pueda cambiar el combate.'},\n  {id:'rift',match:/Brecha responde|contener|explotar|herida violeta/i,text:'Esto no es una ruina cualquiera. La Brecha reacciona a nosotros.',reason:'Tu decisión cambia la recompensa y empieza a definir cómo gobiernas Valoria.'},\n  {id:'card',match:/carta|Archivo|vestigio|reliquia/i,text:'Guárdala. Estos vestigios conservan poder de la Brecha.',reason:'Las reliquias servirán fuera del combate y más adelante tendrán que demostrar su valor.'},\n  {id:'heroes',match:/Salón de Héroes|Esencia|Resonancia|ALDRIC/i,text:'Un reino no crece solo con edificios.',reason:'La experiencia sube niveles; la Esencia mejora habilidades; las Resonancias especializan a cada héroe.'},\n  {id:'global',match:/Tu reino no está solo|mapa global|Primera marcha/i,text:'Valoria es solo una pieza del mundo.',reason:'Empieza por el Bosque seguro. Después veremos quién controla los recursos que necesitamos.'},\n  {id:'quarry',match:/Cantera disputada|recurso tiene dueño/i,text:'Aquí termina la seguridad de nuestras tierras.',reason:'Inspecciona la Cantera y decide cómo responder cuando otro reino ocupa un recurso.'},\n  {id:'global-rift',match:/Brecha también es global|Zona de la Brecha/i,text:'La misma corrupción aparece lejos de Valoria.',reason:'Resuelve la zona para entender que la amenaza no pertenece a un solo reino.'},\n  {id:'bastion',match:/bastión renace|Fortaleza/i,text:'Ya sabemos qué existe más allá de las puertas.',reason:'Refuerza el Bastión con lo recuperado y prepara una segunda incursión.'},\n  {id:'duel',match:/Duelo de Reliquias/i,text:'Ahora veremos si sabes leer una reliquia, no solo coleccionarla.',reason:'Mira los valores N/E/S/O, juega una pieza y observa qué valor enfrenta al rival.'},\n  {id:'sea',match:/MAR DE CRISTAL|Mar de Cristal/i,text:'Valoria vuelve a respirar, pero la Brecha no termina aquí.',reason:'Más allá de estas tierras nos espera el Mar de Cristal.'}\n ];\n function text(){return (document.querySelector('.quest')?.innerText||'')+' '+(document.querySelector('.modal')?.innerText||'')+' '+document.body.innerText.slice(0,3500)}\n function show(g){if(seen[g.id]||document.querySelector('.aldric-guide'))return;seen[g.id]=1;localStorage.setItem(KEY,JSON.stringify(seen));const el=document.createElement('div');el.className='aldric-guide';el.dataset.guide=g.id;el.innerHTML='<div class="portrait">⚔</div><div class="speech"><b>SIR ALDRIC</b>'+g.text+'<span class="reason">'+g.reason+'</span></div><button class="dismiss" aria-label="Cerrar">×</button>';el.querySelector('.dismiss').onclick=()=>el.remove();document.body.appendChild(el);setTimeout(()=>el.remove(),11000)}\n function scan(){const t=text();for(const g of guides){if(g.match.test(t)){show(g);break}}}\n new MutationObserver(()=>{clearTimeout(window.__aldricScan);window.__aldricScan=setTimeout(scan,180)}).observe(document.documentElement,{subtree:true,childList:true,characterData:true});\n setTimeout(scan,900);\n window.addEventListener('keydown',e=>{if(e.shiftKey&&e.key==='R'){localStorage.removeItem(KEY);seen={};}});\n})();\n</script>\n`;
html=html.replace('</body>',guideScript+'</body>');
html=html.replace('<head>','<head><meta name="eldoria-build" content="0.19.5-r7-guided">');

const required=['onboardingDone','showGlobalWorldIntro','Duelo de Reliquias','CAPÍTULO III','MAR DE CRISTAL','0.19.5-r7-guided','aldric-guide','SIR ALDRIC'];
for(const marker of required) if(!html.includes(marker)) throw new Error(`r7 guided output missing marker: ${marker}`);
for(const forbidden of ['../v0195-r6/index.html','r7patch.js']) if(html.includes(forbidden)) throw new Error(`Runtime chain leaked into consolidated output: ${forbidden}`);

const out=path.join(root,'playtest');
fs.rmSync(out,{recursive:true,force:true});
fs.mkdirSync(out,{recursive:true});
fs.writeFileSync(path.join(out,'index.html'),html);
const finalHash=crypto.createHash('sha256').update(html).digest('hex');
fs.writeFileSync(path.join(out,'build.json'),JSON.stringify({version:'0.19.5',revision:'r7-guided',sha256:finalHash,builtAt:new Date().toISOString()},null,2));
console.log(`Eldoria r7 guided: ${html.length} bytes sha256=${finalHash}`);
