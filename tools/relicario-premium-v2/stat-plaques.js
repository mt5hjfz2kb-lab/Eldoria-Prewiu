/* Relicario v2.2: plaque borders and dynamic N/E/S/O values in ONE SVG 320x512 grid.
 * Covers baked old symbols in the approved reference. Never changes rarity, stats, card aspect or game state.
 */
'use strict';
const STAT_METALS=Object.freeze({
 common:{light:'#f6f0e4',mid:'#a9afb9',dark:'#5e656e',rim:'#c5c9cb'},
 rare:{light:'#ecfcff',mid:'#61b6df',dark:'#296080',rim:'#94e4ff'},
 epic:{light:'#ffe3e9',mid:'#c477c9',dark:'#643777',rim:'#eda6ef'},
 legendary:{light:'#fff6d5',mid:'#ffcc68',dark:'#9d5e21',rim:'#ffe1a3'}
});
function renderStatPlaques(values,rarity){
 const metal=STAT_METALS[rarity];if(!metal)throw Error('Unknown metal '+rarity);
 for(const d of ['N','S','E','O'])if(!Number.isInteger(values[d])||values[d]<1||values[d]>10)throw Error('Invalid '+d);
 const id='metal-'+rarity;
 const draw=(dir,outer,inner,cx,labelY,valueY)=>{
  const n=values[dir],digit=n===10?' two-digit':'';
  return \`<g class="relic-stat stat-\${dir.toLowerCase()}\${digit}" data-dir="\${dir}">
  <path class="plaque-outer" d="\${outer}" fill="url(#\${id})" stroke="\${metal.rim}" stroke-width="1.45" stroke-linejoin="round"/>
  <path class="plaque-inner" d="\${inner}" fill="#0e1117" stroke="\${metal.dark}" stroke-width="1.15" stroke-linejoin="round"/>
  <text class="compass" x="\${cx}" y="\${labelY}" text-anchor="middle">\${dir}</text>
  <text class="value" x="\${cx}" y="\${valueY}" text-anchor="middle" font-size="\${n===10?23:29}">\${n}</text>
  </g>\`;
 };
 return \`<svg class="relic-stats-svg" viewBox="0 0 320 512" preserveAspectRatio="xMidYMid meet" aria-hidden="true" focusable="false" xmlns="http://www.w3.org/2000/svg">
 <defs><linearGradient id="\${id}" x1="0" y1="0" x2="1" y2="1">
  <stop offset="0" stop-color="\${metal.light}"/><stop offset=".28" stop-color="\${metal.mid}"/>
  <stop offset=".63" stop-color="\${metal.dark}"/><stop offset="1" stop-color="\${metal.light}"/>
 </linearGradient></defs>
 \${draw('N','M137 5 L183 5 L198 33 L193 70 L160 92 L127 70 L122 33 Z','M140 12 L180 12 L190 35 L187 66 L160 84 L133 66 L130 35 Z',160,31,66)}
 \${draw('O','M4 161 L29 146 L51 156 L51 234 L29 248 L4 232 Z','M10 164 L29 153 L45 161 L45 229 L29 239 L10 229 Z',27,180,213)}
 \${draw('E','M316 161 L291 146 L269 156 L269 234 L291 248 L316 232 Z','M310 164 L291 153 L275 161 L275 229 L291 239 L310 229 Z',293,180,213)}
 \${draw('S','M135 255 L160 244 L185 255 L191 304 L160 341 L129 304 Z','M141 263 L160 253 L179 263 L183 301 L160 331 L137 301 Z',160,281,312)}
 </svg>\`;
}