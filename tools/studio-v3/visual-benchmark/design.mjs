import {createHash} from "node:crypto";
export const SOURCE="e1d3b9892e1a75a9e7bfbafd0db225de7aacba74";
export const REFERENCE="91b8de9874aed87219d733efa6e8e0aa56207f1e";
export const MATERIAL_COMMIT="5ef2aee67cf8fde57ba2fc4b3d929b517ee67944";
export const MATERIAL_PATH="Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D/UI/CoronaBrechaPremium/panel_obsidian.png";
export const TARGET="SENDA_UI_VISUAL_SYSTEM";
export const FIELDS={
 direction:["palette","identity","title_font","frame","texture"],
 palette:["ink","surface","light","trim","text","muted","action","action_text","shield"],
 art:["paths","placement"],
 layout:["gutter","gap","title","label","value","stat_height","control","modal_width","modal_padding","footer_gap","hud_pad","stat_style","icon_position","panel_align"]
};
const need=(x,m)=>{if(!x)throw Error(m)};
const exact=(o,k)=>o&&typeof o==="object"&&!Array.isArray(o)&&Object.keys(o).sort().join(",")===k.slice().sort().join(",");
const choice=(x,a)=>a.includes(x);
const integer=(x,min,max)=>Number.isInteger(x)&&x>=min&&x<=max;
const hex=x=>typeof x==="string"&&/^#[a-fA-F0-9]{6}$/.test(x);
export const sha=x=>createHash("sha256").update(x).digest("hex");
export function validateDirection(d){
 need(exact(d,FIELDS.direction)&&exact(d.palette,FIELDS.palette)&&Object.values(d.palette).every(hex),"Direction schema/palette");
 need(typeof d.identity==="string"&&d.identity.length>0&&d.identity.length<=350,"Art rationale");
 need(choice(d.title_font,["serif","system"])&&choice(d.frame,["bevel","inset","angular"])&&choice(d.texture,["obsidian","matte","grain"]),"Direction choices");
 return d;
}
export function validateArt(a){
 need(exact(a,FIELDS.art)&&choice(a.placement,["corners","crest","crest-and-corners"])&&Array.isArray(a.paths)&&a.paths.length>=2&&a.paths.length<=10,"Art schema");
 let bytes=0;
 for(const p of a.paths){
  need(exact(p,["d","fill","stroke","width"]),"Vector path schema");
  // Restricted coordinates and SVG path commands; no markup, URL, filter, event or script.
  need(typeof p.d==="string"&&p.d.length<=300&&/^[MLHVQCZmlhvqcz0-9., +\-]+$/.test(p.d)&&/^[Mm]/.test(p.d),"Unsafe path syntax");
  const numbers=p.d.match(/[-+]?\d+(?:\.\d+)?/g)||[];
  need(numbers.length>=4&&numbers.length<=90&&numbers.every(n=>Number.isFinite(Number(n))&&Math.abs(Number(n))<=150),"Path geometry bounds");
  need(choice(p.fill,["none","trim","light","surface","action","shield"])&&choice(p.stroke,["none","trim","light","surface","action","shield"])&&integer(p.width,0,4),"Vector paint");
  bytes+=p.d.length;
 }
 need(bytes<=2200,"Ornament too large");return a;
}
export function validateLayout(l){
 need(exact(l,FIELDS.layout),"Layout schema");
 for(const [k,a,b] of [["gutter",8,18],["gap",3,10],["title",17,26],["label",9,12],["value",16,24],["stat_height",42,62],["control",44,60],["modal_width",270,380],["modal_padding",12,25],["footer_gap",6,14],["hud_pad",6,14]])need(integer(l[k],a,b),"Invalid "+k);
 need(choice(l.stat_style,["strip","tiles"])&&choice(l.icon_position,["left","top"])&&choice(l.panel_align,["center","left"]),"Layout choices");return l;
}
export function assemble(packet){
 need(exact(packet,["schema","target","direction","art","layout"])&&packet.schema===1&&packet.target===TARGET,"Packet schema or target");
 const d=validateDirection(packet.direction),a=validateArt(packet.art),l=validateLayout(packet.layout),p=d.palette;
 const color=k=>k==="none"?"none":p[k];
 const svg='<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100">'+a.paths.map(v=>'<path d="'+v.d+'" fill="'+color(v.fill)+'" stroke="'+color(v.stroke)+'" stroke-width="'+v.width+'" stroke-linecap="round" stroke-linejoin="round"/>').join("")+'</svg>';
 const data='url("data:image/svg+xml,'+encodeURIComponent(svg)+'")';
 const texture=d.texture==="obsidian"?'linear-gradient(150deg,'+p.surface+'e8,'+p.ink+'eb),url("reference/panel_obsidian.png")':d.texture==="grain"?'repeating-linear-gradient(125deg,#ffffff06 0px,#ffffff06 1px,transparent 1px,transparent 5px),linear-gradient(155deg,'+p.light+','+p.ink+')':'linear-gradient(155deg,'+p.surface+','+p.ink+')';
 const frame=d.frame==="angular"?"0":d.frame==="bevel"?"6px":"12px";
 const shadow=d.frame==="inset"?"inset 0 0 0 2px "+p.ink+",inset 0 0 0 3px "+p.trim+"77,0 6px 18px #0009":"inset 0 1px #ffffff26,inset 0 0 0 2px "+p.ink+",0 5px 12px #0008";
 const font=d.title_font==="serif"?"Georgia,serif":"system-ui,sans-serif";
 const svgAsCorner=a.placement!=="crest";
 const svgAsCrest=a.placement!=="corners";
 const css=`
/* Generated visual presentation only. DOM, state and event handlers untouched. */
#game{--v3-ink:${p.ink};--v3-surface:${p.surface};--v3-trim:${p.trim};--v3-text:${p.text};--v3-muted:${p.muted};--v3-action:${p.action};--v3-crest:${data};color:var(--v3-text)}
#hud{left:0;right:0;width:auto;max-width:none;box-sizing:border-box;padding:max(${l.hud_pad}px,env(safe-area-inset-top)) ${l.gutter}px 12px;background:linear-gradient(180deg,${p.ink}fa 0%,${p.ink}de 80%,${p.ink}00 100%);text-shadow:0 2px 3px #0009}
#hudcrest{left:${l.gutter}px;top:max(${l.hud_pad}px,env(safe-area-inset-top));height:50px;width:35px}
#eyebrow,#hud h1,#chapter{padding-left:43px;margin-right:46px}
#eyebrow{font:700 9px/1.2 system-ui,sans-serif;letter-spacing:.045em;color:${p.muted}}
#hud h1{font:700 clamp(14px,4.7vw,${l.title}px)/1.15 ${font};color:${p.text};letter-spacing:.012em;max-width:none;margin-top:4px;margin-bottom:3px;overflow-wrap:normal;text-shadow:0 2px 2px #000c}
#chapter{font:650 11px/1.25 system-ui,sans-serif;letter-spacing:.08em;color:${p.trim};margin-top:0}
#round{margin-top:9px;gap:8px;font-size:11px;color:${p.text};letter-spacing:.02em}
#track{height:5px;background:${p.light};border-radius:1px}#bar{background:linear-gradient(90deg,${p.shield},${p.action})}
#stats{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:${l.stat_style==="strip"?0:l.gap}px;margin-top:7px;${l.stat_style==="strip"?"border:1px solid "+p.trim+";background:"+texture+";background-size:cover;border-radius:"+frame+";overflow:hidden":""}}
#stats>span,#pause,#foot .arrow,#hint,#modal,#end>div{clip-path:none;border:1px solid ${p.trim};border-radius:${frame};box-shadow:${shadow};background:${texture};background-size:cover;background-position:center;color:${p.text}}
#stats>span{display:grid;position:relative;min-width:0;min-height:${l.stat_height}px;grid-template-columns:${l.icon_position==="top"?"1fr":"19px minmax(0,1fr)"};grid-template-rows:${l.icon_position==="top"?"17px auto auto":"auto auto"};align-content:center;align-items:center;justify-items:center;gap:1px;padding:4px 3px;overflow:hidden;${l.stat_style==="strip"?"border:0;border-right:1px solid "+p.trim+"55;border-radius:0;box-shadow:none;background:none":""}}
#stats>span:last-child{${l.stat_style==="strip"?"border-right:0":""}}
#stats>span:before,#stats>span:after,#pause:before,#foot .arrow:before,#hint:before{content:none}
#stats .statIcon{display:block;grid-column:1;grid-row:1;width:18px;height:18px;margin:0}
#stats b{grid-column:${l.icon_position==="top"?1:2};grid-row:${l.icon_position==="top"?2:1};font:800 clamp(15px,4.7vw,${l.value}px)/1.05 system-ui,sans-serif;letter-spacing:-.025em;color:${p.text};white-space:nowrap;text-shadow:none}
#stats b.amethyst{color:${p.shield}}
#stats small{grid-column:1/-1;grid-row:${l.icon_position==="top"?3:2};font:700 ${l.label}px/1.15 system-ui,sans-serif;letter-spacing:0;color:${p.muted};margin:0;white-space:nowrap;text-shadow:none}
#pause{position:absolute;top:max(${l.hud_pad}px,env(safe-area-inset-top));right:${l.gutter}px;min-width:44px;width:44px;height:44px;font-size:21px;color:${p.text}}
#foot{display:flex;left:0;right:0;gap:${l.footer_gap}px;bottom:max(12px,env(safe-area-inset-bottom));padding:0 ${l.gutter}px;align-items:center}
#foot .arrow{position:relative;width:${l.control+8}px;min-width:44px;height:${l.control}px;color:${p.text};font-size:25px;flex:none}
#hint{position:relative;min-width:0;max-width:230px;flex:1;font:650 11px/1.25 system-ui,sans-serif;letter-spacing:0;text-shadow:none;padding:9px 10px;text-align:center;color:${p.muted}}
#modal,#end>div{box-sizing:border-box;position:relative;width:min(calc(100% - 2 * ${l.gutter}px),${l.modal_width}px);padding:${l.modal_padding}px;text-align:${l.panel_align};isolation:isolate;background-size:cover;background-position:center}
#modal{position:absolute;left:50%;bottom:max(${l.control+38}px,env(safe-area-inset-bottom) + 86px);transform:translateX(-50%);backdrop-filter:none}
#modal h2,#end h2{font:700 clamp(19px,5.2vw,26px)/1.14 ${font};letter-spacing:.018em;color:${p.text};margin:0 0 9px;text-shadow:0 2px 3px #0008}
#modal p,#end p{font:500 14px/1.45 system-ui,sans-serif;color:${p.muted};margin:8px 0}
#loadingArt{font-size:11px!important;line-height:1.3!important;color:${p.muted}!important}
#crest{width:56px;height:56px;margin:0 auto 12px;display:block}
#play,#again{width:100%;min-height:48px;display:block;margin-top:14px;padding:10px 12px;border:1px solid ${p.trim};border-radius:${frame};background:linear-gradient(155deg,${p.action},${p.action}dd);color:${p.action_text};font:850 14px/1.2 system-ui,sans-serif;letter-spacing:.02em;box-shadow:inset 0 1px #ffffff44,0 4px 10px #0006}
#end{background:${p.ink}d9;padding:${l.gutter}px}
#end b{font:800 29px/1.1 system-ui,sans-serif;color:${p.text};margin:15px 0}
#end>div{min-height:0}
#notice{font:750 19px/1.15 ${font};color:${p.text};max-width:94%;white-space:normal}
${svgAsCorner?"#modal:before,#end>div:before,#modal:after,#end>div:after{content:\"\";position:absolute;pointer-events:none;width:34px;height:34px;background:"+data+" center/contain no-repeat;opacity:.8;z-index:-1}#modal:before,#end>div:before{top:6px;left:6px}#modal:after,#end>div:after{top:6px;right:6px;transform:scaleX(-1)}":""}
${svgAsCrest?"#modal #crest{background:"+data+" center/contain no-repeat;padding:4px}":""}
@media(max-width:360px){#hud{padding-left:8px;padding-right:8px}#hudcrest{left:8px;width:30px;height:44px}#eyebrow,#hud h1,#chapter{padding-left:36px;margin-right:43px}#hud h1{font-size:14px}#stats>span{grid-template-columns:1fr;grid-template-rows:15px auto auto;min-height:48px;padding:3px 1px}#stats .statIcon{grid-column:1;grid-row:1;width:14px;height:14px}#stats b{grid-column:1;grid-row:2;font-size:16px}#stats small{grid-row:3;font-size:9px}#foot{padding:0 8px;gap:6px}#hint{font-size:10px}#modal,#end>div{padding:13px}}
@media(max-height:530px){#hud{max-width:min(440px,66%);padding:6px 8px 8px}#hudcrest,#eyebrow{display:none}#hud h1,#chapter{padding-left:0;margin-right:0}#hud h1{font-size:17px;line-height:1.1}#chapter{font-size:10px}#round{margin-top:4px;font-size:10px}#stats{margin-top:4px}#stats>span{min-height:39px;grid-template-columns:14px 1fr;grid-template-rows:17px 12px;padding:2px}#stats .statIcon{grid-row:1;grid-column:1;width:13px;height:13px}#stats b{grid-row:1;grid-column:2;font-size:15px}#stats small{grid-row:2;font-size:9px}#pause{top:8px;right:12px}#foot{bottom:8px;justify-content:center}#foot .arrow{height:44px;width:60px}#hint{display:none}#modal{left:auto;right:16px;bottom:64px;transform:none;width:min(46%,330px);padding:12px}#modal h2{font-size:18px;margin:0 0 5px}#modal p{font-size:12px;line-height:1.25;margin:5px 0}#modal #crest{display:none}#play{min-height:44px;margin-top:7px;padding:8px;font-size:13px}#end>div{max-width:390px;padding:14px}#end h2{font-size:22px}#end p{font-size:13px}#end b{font-size:23px;margin:8px 0}}
@media(prefers-reduced-motion:reduce){#play,#again,#pause,#foot .arrow{transition:none}}
`;
 need(css.length<20000&&!/javascript:|@import|<script/i.test(css),"Unsafe CSS");
 return {css,svg,css_sha256:sha(css),art_sha256:sha(svg),design_sha256:sha(JSON.stringify(packet))};
}
