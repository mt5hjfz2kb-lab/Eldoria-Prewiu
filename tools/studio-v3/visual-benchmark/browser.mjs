import {createRequire} from "node:module";
import {readFileSync,writeFileSync,mkdirSync,copyFileSync,statSync} from "node:fs";
import {join,resolve} from "node:path";
import {createHash} from "node:crypto";
import assert from "node:assert/strict";
import {assemble,SOURCE,REFERENCE,MATERIAL_COMMIT,sha} from "./design.mjs";
const require=createRequire(import.meta.url);
const {chromium}=require("playwright");
const root=resolve(process.env.V3_VISUAL_OUT||"v3-visual-evidence"), web=resolve(process.env.V3_VISUAL_WEBROOT||"");
assert.ok(process.env.V3_VISUAL_WEBROOT&&process.env.V3_VISUAL_OUT);
mkdirSync(root,{recursive:false});
const pack=JSON.parse(readFileSync(process.env.V3_VISUAL_PACK||new URL("./calibration.json",import.meta.url),"utf8"));
assert.equal(pack.packets.length,2);
const designs=pack.packets.map(assemble);
const blind=join(root,"blind-review"),privateRoot=join(root,"private-receipts");
mkdirSync(blind);mkdirSync(privateRoot);
const actualSource=readFileSync(join(web,"tools/senda-brecha/visual-3d/index.html"));
const material=join(web,"tools/senda-brecha/visual-3d/reference/panel_obsidian.png");
const materialBytes=readFileSync(material);
assert.ok(materialBytes.subarray(0,8).equals(Buffer.from([137,80,78,71,13,10,26,10])),"Real pinned reference PNG required");
copyFileSync(material,join(blind,"reference-panel.png"));
copyFileSync(join(web,"reference-art-direction.md"),join(blind,"reference-art-direction.md"));
copyFileSync(join(web,"reference-shared-standard.md"),join(blind,"reference-shared-standard.md"));
// Arbitrary anonymous label assignment, fixed before screenshots. Mapping is
// separate from judge input. Calibration labels are not live model roles.
const aliases=["variant-72","variant-19"];
for(let i=0;i<designs.length;i++){
 const d=designs[i];writeFileSync(join(privateRoot,aliases[i]+".css"),d.css);writeFileSync(join(privateRoot,aliases[i]+".svg"),d.svg);
}
writeFileSync(join(privateRoot,"identity-map.json"),JSON.stringify({calibration:!process.env.V3_VISUAL_PACK,aliases:aliases.map((alias,i)=>({alias,packet_index:i,...Object.fromEntries(Object.entries(designs[i]).filter(([k])=>k.endsWith("sha256")))}))},null,2));
const variants=[{name:"baseline",css:null},...designs.map((d,i)=>({name:aliases[i],css:d.css}))];
const viewports=[["phone",390,844],["compact",320,700],["horizontal",844,390]];
const all=[],errors=[],interactions=[];
const invariant=(s)=>JSON.stringify(Object.fromEntries(["started","paused","finished","stage","stepTime","lane","sideX","soldiers","damage","armor","score","visibleActors","gateLabels","screenLaneX","gateScreenX","worldScroll","canonicalReady","canonicalPieces","canonicalSources","canonicalError"].map(k=>[k,s[k]])));
const browser=await chromium.launch({headless:true,args:["--enable-webgl","--ignore-gpu-blocklist","--use-gl=angle","--use-angle=swiftshader","--enable-unsafe-swiftshader"]});
async function setVariant(page,v){
 await page.evaluate(()=>document.querySelector("#v3-visual-candidate")?.remove());
 if(v.css){await page.addStyleTag({content:v.css});await page.evaluate(()=>{const nodes=document.querySelectorAll("style");nodes[nodes.length-1].id="v3-visual-candidate"})}
 await page.evaluate(async()=>{await document.fonts.ready;await new Promise(r=>requestAnimationFrame(()=>requestAnimationFrame(r)))});
}
async function metrics(page,phase){
 return await page.evaluate(phase=>{
  const $=s=>document.querySelector(s),game=$("#game").getBoundingClientRect();
  const rect=r=>({x:+(r.x-game.x).toFixed(2),y:+(r.y-game.y).toFixed(2),w:+r.width.toFixed(2),h:+r.height.toFixed(2)});
  const visible=e=>{if(!e)return false;let r=e.getBoundingClientRect(),s=getComputedStyle(e);return r.width>0&&r.height>0&&s.visibility!=="hidden"&&s.display!=="none"};
  const within=r=>r.left>=game.left-.5&&r.right<=game.right+.5&&r.top>=game.top-.5&&r.bottom<=game.bottom+.5;
  const issue=[],bounds={},labels=[],buttons=[];
  const ids=phase==="intro"?["#play"]:phase==="victory"||phase==="defeat"?["#again"]:["#left","#right","#pause"];
  for(const id of ids){const e=$(id),r=e.getBoundingClientRect(),hit=document.elementFromPoint(r.x+r.width/2,r.y+r.height/2);
   const q={id,...rect(r),visible:visible(e),inside:within(r),hit:hit===e||e.contains(hit)};buttons.push(q);
   if(!q.visible||!q.inside||!q.hit)issue.push("blocked-control:"+id);
   if(r.width<43.9||r.height<43.9)issue.push("small-control:"+id);
  }
  let list=phase==="victory"||phase==="defeat"?["#endTitle","#endBody","#finalScore","#again"]:["#hud h1","#chapter","#roundlabel","#timer","#stats b","#stats small"];
  if(phase==="intro")list.push("#modal h2","#modal p","#play");
  for(const id of list)for(const e of document.querySelectorAll(id)){
   if(!visible(e)){issue.push("hidden-label:"+id);continue}
   const r=e.getBoundingClientRect(),range=document.createRange();range.selectNodeContents(e);const t=range.getBoundingClientRect(),style=getComputedStyle(e),parent=e.closest("#stats>span")||e;
   const p=parent.getBoundingClientRect(),fits=t.left>=p.left-1&&t.right<=p.right+1&&t.top>=p.top-1&&t.bottom<=p.bottom+1;
   labels.push({id,text:e.textContent,font_px:+parseFloat(style.fontSize).toFixed(1),fits,inside:within(r),...rect(r)});
   if(!fits||!within(r))issue.push("clipped-label:"+id+":"+e.textContent);
   if(id==="#stats small"&&parseFloat(style.fontSize)<9)issue.push("small-stat-label:"+e.textContent);
  }
  for(const id of ["#hud","#stats","#foot","#modal","#end>div","#pause"]){const e=$(id);if(visible(e))bounds[id]=rect(e.getBoundingClientRect())}
  const overlap=(a,b)=>a&&b&&Math.min(a.x+a.w,b.x+b.w)-Math.max(a.x,b.x)>2&&Math.min(a.y+a.h,b.y+b.h)-Math.max(a.y,b.y)>2;
  if((phase==="gates"||phase==="battle"||phase==="intro")&&overlap(bounds["#stats"],bounds["#pause"]))issue.push("stats-pause-overlap");
  if(phase==="intro"&&overlap(bounds["#modal"],bounds["#stats"]))issue.push("modal-stats-overlap");
  if(phase==="intro"&&overlap(bounds["#modal"],bounds["#foot"]))issue.push("modal-footer-overlap");
  return {issues:[...new Set(issue)],bounds,labels,buttons,scene_covered_by_hud_pct:bounds["#hud"]?+(100*bounds["#hud"].w*bounds["#hud"].h/(game.width*game.height)).toFixed(1):null,game:rect(game)};
 },phase)
}
async function capture(page,viewport,phase){
 const before=invariant(await page.evaluate(()=>__senda3d.state()));
 for(const v of variants){
  await setVariant(page,v);
  const after=invariant(await page.evaluate(()=>__senda3d.state()));
  assert.equal(after,before,"Presentation candidate changed exact frozen fixture");
  const m=await metrics(page,phase);
  const name=viewport+"-"+phase+"-"+v.name+".png",file=join(blind,name);
  await page.screenshot({path:file,animations:"disabled",fullPage:false});
  all.push({viewport,phase,variant:v.name,sha256:sha(readFileSync(file)),state_digest:sha(before),metrics:m});
 }
 await setVariant(page,variants[0]);
}
async function playRoute(page,route){
 await page.evaluate(()=>{__senda3d.start();__senda3d.setSpeed(0)});
 for(const target of route){
  const s=await page.evaluate(()=>__senda3d.state());
  if(s.finished)break;
  for(let i=s.lane;i>target;i--)await page.locator("#left").click();
  for(let i=s.lane;i<target;i++)await page.locator("#right").click();
  await page.evaluate(()=>__senda3d.advance(11.05))
 }
 return await page.evaluate(()=>__senda3d.state())
}
try{
 for(const [name,width,height] of viewports){
  const page=await browser.newPage({viewport:{width,height},deviceScaleFactor:1,hasTouch:true});
  page.on("pageerror",e=>errors.push(name+":page:"+e.message));
  page.on("console",m=>{if(m.type()==="error")errors.push(name+":console:"+m.text().slice(0,350))});
  await page.goto("http://127.0.0.1:8773/tools/senda-brecha/visual-3d/index.html?test",{waitUntil:"domcontentloaded",timeout:90000});
  await page.waitForFunction(()=>window.__senda3d?.state().canonicalReady,undefined,{timeout:90000});
  await page.evaluate(()=>__senda3d.setSpeed(0));
  const original=await page.evaluate(()=>__senda3d.state());assert.equal(original.canonicalPieces,3);assert.equal(original.canonicalSources.length,10);assert.equal(original.heroAnimated,true);
  // Load the original asset explicitly, not silently substitute a procedural texture.
  assert.equal(await page.evaluate(async()=>{let i=new Image();i.src="reference/panel_obsidian.png";await i.decode();return i.naturalWidth>0}),true);
  await capture(page,name,"intro");
  await page.locator("#play").click();
  await page.evaluate(()=>{__senda3d.setPhase(0,2.8);__senda3d.advance(.15)});
  await capture(page,name,"gates");
  await page.evaluate(()=>{__senda3d.setPhase(2,7.2);__senda3d.advance(.35)});
  await capture(page,name,"battle");
  if(name==="phone"){
   for(const v of variants){
    await setVariant(page,v);await page.evaluate(()=>__senda3d.start());
    await page.locator("#left").click();assert.equal((await page.evaluate(()=>__senda3d.state())).lane,0);
    await page.evaluate(()=>__senda3d.advance(.6));assert.ok((await page.evaluate(()=>__senda3d.state())).screenLaneX<0);
    await page.locator("#right").click();assert.equal((await page.evaluate(()=>__senda3d.state())).lane,1);
    await page.locator("#pause").click();assert.equal((await page.evaluate(()=>__senda3d.state())).paused,true);
    const p=await page.evaluate(()=>__senda3d.state());await page.evaluate(()=>__senda3d.advance(.2));assert.equal((await page.evaluate(()=>__senda3d.state())).stepTime,p.stepTime);
    await page.locator("#pause").click();assert.equal((await page.evaluate(()=>__senda3d.state())).paused,false);
    interactions.push({variant:v.name,lane_buttons:true,pause:true,resume:true})
   }
   await setVariant(page,variants[0]);
   const victory=await playRoute(page,[1,0,1,0,1,0]);assert.equal(victory.finished,true);assert.ok(victory.soldiers>0);assert.match(await page.locator("#endTitle").textContent(),/BRECHA SELLADA/);
   await capture(page,name,"victory");
   const defeat=await playRoute(page,[0,0,0,0,0,0]);assert.equal(defeat.finished,true);assert.equal(defeat.soldiers,0);assert.match(await page.locator("#endTitle").textContent(),/CAÍDO/);
   await capture(page,name,"defeat");
   for(const v of variants){
    await setVariant(page,v);const r=await page.evaluate(()=>__senda3d.state());
    if(!r.finished){await setVariant(page,variants[0]);await playRoute(page,[0,0,0,0,0,0]);await setVariant(page,v)}
    await page.locator("#again").click();const reset=await page.evaluate(()=>__senda3d.state());
    assert.equal(reset.soldiers,22);assert.equal(reset.stage,0);assert.equal(reset.lane,1);
    interactions.find(x=>x.variant===v.name).replay=true;
   }
  }
  await page.close();
 }
 assert.equal(all.length,33,"Expected all three variants across eleven fixtures");
 const summaries=variants.map(v=>{const rows=all.filter(x=>x.variant===v.name);return {variant:v.name,frames:rows.length,issues:rows.flatMap(r=>r.metrics.issues.map(issue=>({fixture:r.viewport+"-"+r.phase,issue}))),hud_pct:rows.filter(r=>r.phase==="gates").map(r=>({viewport:r.viewport,pct:r.metrics.scene_covered_by_hud_pct})),all_controls:rows.flatMap(r=>r.metrics.buttons).every(b=>b.visible&&b.inside&&b.hit),min_stat_label_px:Math.min(...rows.flatMap(r=>r.metrics.labels).filter(l=>l.id==="#stats small").map(l=>l.font_px))}});
 const proof={schema:1,target:"SENDA_UI_VISUAL_SYSTEM",mode:process.env.V3_VISUAL_PACK?"external-design-packet":"hand-authored-calibration",engine:"actual Senda Three.js WebGL in Chromium",source_sha:SOURCE,source_sha256:sha(actualSource),reference_sha:REFERENCE,material_source_sha:MATERIAL_COMMIT,material_sha256:sha(materialBytes),actual_game_logic_unchanged:true,matched_fixtures:11,total_frames:all.length,viewports,variants:summaries,interactions,errors,live_model_roles:false,blind_scoring_performed:false,visual_scores:null,art_approved:false,production_promoted:false,physical_iphone:false};
 writeFileSync(join(blind,"frames-manifest.json"),JSON.stringify(all.map(({viewport,phase,variant,sha256,state_digest})=>({viewport,phase,variant,sha256,state_digest})),null,2));
 writeFileSync(join(privateRoot,"layout-metrics.json"),JSON.stringify(all,null,2));
 writeFileSync(join(root,"report.json"),JSON.stringify(proof,null,2));
 console.log("STUDIO_V3_VISUAL_PROOF "+JSON.stringify(proof));
 assert.equal(errors.length,0,JSON.stringify(errors));
 // Baseline defects are observed, not gate failures. Reject a calibration
 // candidate on hidden/clipped controls or candidate technical regressions.
 for(const s of summaries.slice(1))assert.equal(s.issues.length,0,JSON.stringify(s.issues));
}finally{await browser.close()}
