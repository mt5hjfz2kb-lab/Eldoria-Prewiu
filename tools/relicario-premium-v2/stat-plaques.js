/* Relicario v2.4: plaque borders and dynamic N/E/S/O values in ONE SVG 320x512 grid.
 * Covers baked old symbols in the approved reference. Never changes rarity, stats, card aspect or game state.
 */
'use strict';
const STAT_METALS=Object.freeze({
 common:{light:'#dbd5c6',mid:'#818582',dark:'#3b3c3d',rim:'#a4a39b',edge:'#c6bea8',text:'#f2ede2'},
 rare:{light:'#d9e7eb',mid:'#678da4',dark:'#263c51',rim:'#8aaebe',edge:'#b3d7e2',text:'#eef6fa'},
 epic:{light:'#d9c2a3',mid:'#8c6988',dark:'#402e45',rim:'#b99b80',edge:'#e1c5a1',text:'#f8e9e5'},
 legendary:{light:'#f6dfac',mid:'#b9843e',dark:'#6e471d',rim:'#e2bd76',edge:'#ffe5ac',text:'#fff3d4'}
});
function renderStatPlaques(values,rarity){
 const metal=STAT_METALS[rarity];if(!metal)throw Error('Unknown metal '+rarity);
 for(const d of ['N','S','E','O'])if(!Number.isInteger(values[d])||values[d]<1||values[d]>10)throw Error('Invalid '+d);
 const id='metal-'+rarity;
 const draw=(dir,outer,inner,cx,labelY,valueY)=>{
  const n=values[dir],digit=n===10?' two-digit':'';
  return `<g class="relic-stat stat-${dir.toLowerCase()}${digit}" data-dir="${dir}">
  <path class="plaque-outer" d="${outer}" fill="url(#${id})" stroke="${metal.rim}" stroke-width="1.35" stroke-linejoin="round"/>
  <path class="plaque-inner" d="${inner}" fill="url(#recess-${rarity})" stroke="${metal.dark}" stroke-width="1.3" stroke-linejoin="round"/>
  <path class="plaque-engraving" d="${inner}" fill="none" stroke="${metal.edge}" stroke-width=".55" stroke-linejoin="round" opacity=".43"/>
  <text class="compass" x="${cx}" y="${labelY}" text-anchor="middle">${dir}</text>
  <text class="value" x="${cx}" y="${valueY}" text-anchor="middle" font-size="${n===10?23:29}">${n}</text>
  </g>`;
 };
 return `<svg class="relic-stats-svg" viewBox="0 0 320 512" preserveAspectRatio="xMidYMid meet" aria-hidden="true" focusable="false" style="--stat-text:${metal.text}" xmlns="http://www.w3.org/2000/svg">
 <defs>
 <linearGradient id="${id}" x1="0%" y1="0%" x2="100%" y2="100%">
  <stop offset="0" stop-color="${metal.edge}"/><stop offset=".12" stop-color="${metal.light}"/>
  <stop offset=".37" stop-color="${metal.mid}"/><stop offset=".67" stop-color="${metal.dark}"/>
  <stop offset=".87" stop-color="${metal.mid}"/><stop offset="1" stop-color="${metal.edge}"/>
 </linearGradient>
 <linearGradient id="recess-${rarity}" x1="0%" y1="0%" x2="80%" y2="100%">
  <stop offset="0" stop-color="#1b2025"/><stop offset=".4" stop-color="#0b1017"/>
  <stop offset="1" stop-color="#151719"/>
 </linearGradient></defs>
 ${draw('N','M137 5 L183 5 L198 33 L193 70 L160 92 L127 70 L122 33 Z','M140 12 L180 12 L190 35 L187 66 L160 84 L133 66 L130 35 Z',160,31,66)}
 ${draw('O','M4 161 L29 146 L51 156 L51 234 L29 248 L4 232 Z','M10 164 L29 153 L45 161 L45 229 L29 239 L10 229 Z',27,180,213)}
 ${draw('E','M316 161 L291 146 L269 156 L269 234 L291 248 L316 232 Z','M310 164 L291 153 L275 161 L275 229 L291 239 L310 229 Z',293,180,213)}
 ${draw('S','M135 255 L160 244 L185 255 L191 304 L160 341 L129 304 Z','M141 263 L160 253 L179 263 L183 301 L160 331 L137 301 Z',160,281,312)}
 </svg>`;
}