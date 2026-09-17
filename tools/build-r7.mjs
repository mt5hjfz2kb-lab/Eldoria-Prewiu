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

// r7 is now materialized at build time over the validated r6 baseline.
// Historical runtime loaders/patches are not used by the published playtest.
const replace=(from,to)=>{ if(html.includes(from)) html=html.replace(from,to); };
replace("version:'0.19.5-r6'","version:'0.19.5-r7'");
replace('BUILD_SCHEMA=\'0195-r6\'','BUILD_SCHEMA=\'0195-r7\'');
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
const css=`\n/* r7 consolidated polish */\nbutton:focus-visible{outline:2px solid currentColor;outline-offset:3px}.modal{overscroll-behavior:contain}.reward-modal{animation:rewardArrival .32s cubic-bezier(.2,.9,.25,1.15)}@keyframes rewardArrival{from{transform:translateY(12px) scale(.985);opacity:.4}to{transform:none;opacity:1}}.gate.objective{animation:objectivePulse 1.8s ease-in-out infinite}@keyframes objectivePulse{50%{filter:drop-shadow(0 0 13px rgba(215,180,91,.52));transform:translateY(-2px)}}@media(max-width:430px){.modal{max-height:88dvh;overflow:auto}.combat-actions{position:sticky;bottom:0;z-index:3;padding-bottom:max(8px,env(safe-area-inset-bottom));backdrop-filter:blur(8px)}.intent-choices button{min-height:68px}.hero-unit{scroll-margin-top:12px}}@media(prefers-reduced-motion:reduce){*,*::before,*::after{animation-duration:.01ms!important;animation-iteration-count:1!important;transition-duration:.01ms!important;scroll-behavior:auto!important}}\n`;
html=html.replace('</style>',css+'</style>');
html=html.replace('<head>','<head><meta name="eldoria-build" content="0.19.5-r7-consolidated">');

const required=['onboardingDone','showGlobalWorldIntro','Duelo de Reliquias','CAPÍTULO III','MAR DE CRISTAL','0.19.5-r7-consolidated'];
for(const marker of required) if(!html.includes(marker)) throw new Error(`r7 consolidated output missing marker: ${marker}`);
for(const forbidden of ['../v0195-r6/index.html','r7patch.js']) if(html.includes(forbidden)) throw new Error(`Runtime chain leaked into consolidated output: ${forbidden}`);

const out=path.join(root,'playtest');
fs.rmSync(out,{recursive:true,force:true});
fs.mkdirSync(out,{recursive:true});
fs.writeFileSync(path.join(out,'index.html'),html);
const finalHash=crypto.createHash('sha256').update(html).digest('hex');
fs.writeFileSync(path.join(out,'build.json'),JSON.stringify({version:'0.19.5',revision:'r7-consolidated',sha256:finalHash,builtAt:new Date().toISOString()},null,2));
console.log(`Eldoria r7 consolidated: ${html.length} bytes sha256=${finalHash}`);
