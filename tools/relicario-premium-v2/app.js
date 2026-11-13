'use strict';
/** Premium V2 study — standalone, no real-game mutations. All data keyed to existing catalog. */
const RARITIES=[{id:'common',label:'Común',material:'Hierro bruñido'},{id:'rare',label:'Rara',material:'Plata azur'},{id:'epic',label:'Épica',material:'Amatista y oro'},{id:'legendary',label:'Legendaria',material:'Oro solar'}];
const CARD=window.ELDORIA_RELICARIO_DATA;
if(!CARD || CARD.id!=='bosque-valoria')throw Error('Missing generated card data');
let activeImage=CARD.illustration;
const asset=path=>window.__ELDORIA_ASSETS__?.[path]||path;
const safe=s=>String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
function cardHTML(card=CARD,rarity=card.rarity,indestructible=false,compact=false){
 if(!RARITIES.find(x=>x.id===rarity))throw Error('Unknown rarity');
 for(const d of ['N','E','S','O'])if(!Number.isInteger(card.values[d])||card.values[d]<1||card.values[d]>10)throw Error('Bad value: '+d);
 const name=safe(card.name),image=safe(asset(card.id===CARD.id?activeImage:card.illustration)),r=RARITIES.find(x=>x.id===rarity);
 return `<article class="relic-card ${compact?'compact':''} ${indestructible?'is-indestructible':''}" data-rarity="${rarity}" data-indestructible="${indestructible}" data-card-id="${safe(card.id)}" aria-label="${name}; ${r.label}; N ${card.values.N}, E ${card.values.E}, S ${card.values.S}, O ${card.values.O}. ${safe(card.effect)} ${indestructible?'Indestructible':''}">
 <div class="relic-art"><img src="${image}" alt="" loading="eager"></div>
 <img class="relic-chrome" src="${asset('assets/frame_'+rarity+'.webp')}" alt="" aria-hidden="true">
 <span class="relic-rarity">${safe(r.label)}</span>
 ${[['N','n'],['E','e'],['S','s'],['O','o']].map(([dir,c])=>`<b class="relic-stat stat-${c} ${card.values[dir]>=10?'two-digit':''}"><span class="compass">${dir}</span><span class="value">${card.values[dir]}</span></b>`).join('')}
 <div class="relic-title ${card.name.length>24?'long-name':''}">${name}</div>
 <div class="relic-effect"><img src="${asset(card.effectIcon)}" alt="" aria-hidden="true"><div><strong>AL USARLA, RECIBES</strong><span>${safe(card.effectShort)}</span></div></div>
 ${indestructible?`<img class="relic-indestructible" src="${asset('assets/overlay_indestructible.svg')}" alt="" aria-hidden="true">`:''}
 </article>`;
}
function gridCard(r,indestructible){return `<div class="card-frame">${cardHTML(CARD,r,indestructible)}</div>`}
function gallery(){document.getElementById('app').innerHTML=`<header class="top"><div><p class="eyebrow">ELDORIA · DIRECCIÓN ARTÍSTICA · V2</p><h1>Relicario <em>/</em> acabado premium</h1><p class="dek">Materiales y ornamentos extraídos de la referencia artística aprobada. Cuatro marcos, un tamaño, una ilustración intercambiable y un sello independiente.</p></div><div class="status"><b>V2 · REFERENCIA DE PRODUCCIÓN</b><br>Fototextura de marco + capas de datos reales.<br>Sin cambios en Unity ni en la beta.</div></header>
 <div class="rarity-grid">${RARITIES.map(r=>`<section class="rarity-column" data-column="${r.id}"><h2>${r.label.toUpperCase()}</h2><p>${r.material}</p>${gridCard(r.id,false)}<span class="tag">MISMA CARTA · INDESTRUCTIBLE</span>${gridCard(r.id,true)}</section>`).join('')}</div>
 <footer class="footer"><p><b>PROPORCIÓN INVARIABLE</b><span>640 × 1024 px · 5:8 en todas las rarezas</span></p><p><b>UN DATO, OCHO ACABADOS</b><span>N 6 · E 4 · S 3 · O 2 · madera 20 minutos</span></p><p><b>MAQUETA, NO PUBLICACIÓN</b><span>Arte y HTML aislados · importación Unity pendiente</span></p></footer>`;}
function mobile(){document.getElementById('app').innerHTML=`<div class="mobile-shell"><p class="eyebrow">ELDORIA · MÓVIL</p><h1>Relicario</h1><p class="mobile-explain">La misma carta en cuatro materiales, con un sello reutilizable.</p><main class="mobile-stage" id="preview">${gridCard('common',false)}</main><div class="controls"><label>Rareza<select id="rarity">${RARITIES.map(x=>`<option value="${x.id}">${x.label}</option>`).join('')}</select></label><label class="toggle"><input id="indestructible" type="checkbox">Indestructible</label><label>Ilustración interior<input id="file" type="file" accept="image/*"></label></div><p class="note">La propiedad indestructible mantiene valores, efecto, ilustración y rareza; solo añade su sello.</p></div>`;
 const redraw=()=>document.querySelector('#preview').innerHTML=gridCard(document.querySelector('#rarity').value,document.querySelector('#indestructible').checked);
 document.querySelector('#rarity').onchange=redraw;document.querySelector('#indestructible').onchange=redraw;
 document.querySelector('#file').onchange=e=>{const f=e.target.files?.[0];if(!f||!f.type.startsWith('image/'))return;const r=new FileReader();r.onload=()=>{activeImage=r.result;redraw()};r.readAsDataURL(f)};
}
function board(){document.getElementById('app').innerHTML=`<div class="mobile-shell board-shell"><p class="eyebrow">ELDORIA · PRÁCTICA DE ORIN</p><h1>Tablero 3 × 3</h1><p class="mobile-explain">Prueba de escala: solo ilustración y valores, sin panel descriptivo.</p><div class="board-grid">${Array.from({length:9},(_,i)=>`<div class="board-cell" data-cell="${i}">${i===4||i===8?'':cardHTML(CARD,RARITIES[i%4].id,i%3===1,true)}</div>`).join('')}</div></div>`}
const view=new URLSearchParams(location.search).get('view')||'gallery';({gallery,mobile,board}[view]||gallery)();
window.ELDORIA_RELICARIO_V2={CARD,RARITIES,cardHTML,view,version:'2.1.0-typography-polish'};
