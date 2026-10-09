'use strict';
// Real mobile Chromium test of the EXISTING Unity WebGL R2-B gameplay candidate.
// Never probes the stable /unity-owner/ path; use a local, isolated candidate package.
const fs=require('fs');
const {chromium}=require('playwright');
const url=process.env.ELDORIA_URL||'http://127.0.0.1:4173/';
const out=process.env.ELDORIA_EVIDENCE_DIR||'r2b-webgl-qa-evidence';
fs.mkdirSync(out,{recursive:true});
const results={url,engine:'real Unity WebGL, mobile Chromium emulation',sourceAuthority:'6e0e12396a98b04333b595fff704afa3e849c30a',cases:[],pass:false,errors:[]};
const pause=ms=>new Promise(r=>setTimeout(r,ms));
function locate(logs,re,start=0){for(let i=logs.length-1;i>=start;i--){const m=(logs[i]||'').match(re);if(m)return m;}return null;}
async function wait(logs,re,label,start=0,timeout=100000){const limit=Date.now()+timeout;while(Date.now()<limit){let m=locate(logs,re,start);if(m)return m;await pause(250);}throw Error('TIMEOUT '+label);}
function state(s){const m=(s||'').match(/ELDORIA_PLAYABLE_STATE tag=([^ ]+) scene=([^ ]+) revision=(\d+) wood=(\d+) stone=(\d+) gatheredWood=(\d+) gatheredStone=(\d+)/);return m?{tag:m[1],scene:m[2],rev:+m[3],wood:+m[4],stone:+m[5],gatheredWood:+m[6],gatheredStone:+m[7]}:null;}
async function waitState(logs,pred,label,st=0,timeout=100000){let end=Date.now()+timeout;while(Date.now()<end){for(let i=logs.length-1;i>=st;i--){const x=state(logs[i]);if(x&&pred(x))return x;}await pause(250);}throw Error('TIMEOUT '+label);}
function screenUi(m,box,cv){return{x:box.x+(+m[1]/cv.width)*box.width,y:box.y+((cv.height-(+m[2]))/cv.height)*box.height};}
function spotUi(m,box,cv){return{x:box.x+(+m[1]/cv.width)*box.width,y:box.y+(+m[2]/cv.height)*box.height};}
async function one(browser,which,viewport){
 const item={choice:which,viewport,pass:false};
 // Unity's default WebGL page chooses its responsive mobile canvas by navigator.userAgent.
 // Playwright isMobile/hasTouch alone do not identify Chromium as Android; the unmodified
 // desktop branch fixes the canvas at 960x600 and produces unreachable touches on 844x390.
 const ctx=await browser.newContext({viewport,hasTouch:true,isMobile:true,deviceScaleFactor:1,
  userAgent:'Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Mobile Safari/537.36'});
 const page=await ctx.newPage(),logs=[];
 page.on('console',m=>logs.push(m.text()));
 page.on('pageerror',e=>logs.push('PAGEERROR '+String(e)));
 page.on('requestfailed',r=>logs.push('FAILED_REQUEST '+r.url()+' '+r.failure()));
 try{
  await page.goto(url+'?r2b-check='+which+'-'+viewport.width,{waitUntil:'domcontentloaded',timeout:60000});
  let initial=await waitState(logs,s=>s.scene==='Valoria'||s.scene==='ValoriaWebGL','Valoria initial',0,180000);
  // A Unity state log can precede removal of the WebGL loading overlay. Real touch begins
  // only once createUnityInstance has completed and the player can see the game.
  await page.waitForFunction(()=>{const el=document.querySelector('#unity-loading-bar');return el&&getComputedStyle(el).display==='none';},null,{timeout:180000});
  await pause(300);
  const canvas=page.locator('canvas').first(),box=await canvas.boundingBox();
  if(!box)throw Error('no real Unity canvas');
  const cv=await canvas.evaluate(el=>({width:el.width,height:el.height}));
  if(!cv.width||!cv.height)throw Error('canvas dimensions zero');
  item.initial=initial;
  await page.screenshot({path:out+'/'+which+'-'+viewport.width+'-home.png'});
  const nav=await wait(logs,/ELDORIA_PLAYABLE_UI id=worldNav x=([-0-9.]+) y=([-0-9.]+)/,'World navigation');
  let at=screenUi(nav,box,cv),start=logs.length;
  item.worldNavGeometry={tap:at,canvas:box,canvasPixels:cv,viewport};
  if(at.x<0||at.x>=viewport.width||at.y<0||at.y>=viewport.height||at.x<box.x||at.x>box.x+box.width||at.y<box.y||at.y>box.y+box.height)
   throw Error('WORLD_NAV_TAP_OUTSIDE_VISIBLE_VIEWPORT '+JSON.stringify(item.worldNavGeometry));
  await page.touchscreen.tap(at.x,at.y);
  await waitState(logs,s=>s.scene==='Frontier','World region 1',start,45000);
  const hot=await wait(logs,/ELDORIA_PLAYABLE_HOTSPOT id=forest-valoria x=([0-9.]+) y=([0-9.]+)/,'forest hotspot',start,45000);
  await pause(500);
  const center=spotUi(hot,box,cv);start=logs.length;
  await page.touchscreen.tap(center.x,center.y);
  const btn=await wait(logs,/ELDORIA_PLAYABLE_UI id=buildingAction x=([-0-9.]+) y=([-0-9.]+)/,'forest contextual panel',start,15000);
  await pause(200);
  await page.screenshot({path:out+'/'+which+'-'+viewport.width+'-forest-before.png'});
  const action=screenUi(btn,box,cv);
  // WebGL real-frame QA #37862082996 proved the portrait 80px offset lands on
  // CERRAR (panel disappears without a ChooseRegionOneForest command). The actual
  // HARVEST row center in the 390x844 screenshot is ~68px below ACCIÓN.
  // Keep checking the real Unity command/reward/reload; never infer success from a tap.
  const scale=box.height/cv.height;
  const dy=which==='survey'?42:68;
  const click={x:action.x,y:action.y+dy*scale};
  if(click.x<box.x||click.x>box.x+box.width||click.y<box.y||click.y>box.y+box.height)
   throw Error('choice button not within Unity canvas');
  start=logs.length;
  await page.touchscreen.tap(click.x,click.y);
  await wait(logs,new RegExp('ELDORIA_PLAYABLE_COMMAND kind=ChooseRegionOneForest target=forest-valoria:'+which+' ok=True'),'choice action actual command',start,18000);
  const after=await waitState(logs,s=>s.tag.startsWith('command-ChooseRegionOneForest-'), 'post-choice persisted state',start,18000);
  const expect=which==='survey'?20:40;
  if(after.wood-initial.wood!==expect)throw Error('reward delta '+(after.wood-initial.wood)+' expected '+expect);
  item.after=after;item.reward=expect;
  await page.screenshot({path:out+'/'+which+'-'+viewport.width+'-after.png'});
  const prior=logs.length;
  await page.reload({waitUntil:'domcontentloaded',timeout:60000});
  const reloaded=await waitState(logs,s=>s.scene==='Valoria'||s.scene==='ValoriaWebGL','reload state',prior,180000);
  if(reloaded.wood!==after.wood)throw Error('wood not persisted after reload: '+reloaded.wood+' expected '+after.wood);
  item.reloaded=reloaded;
  await page.screenshot({path:out+'/'+which+'-'+viewport.width+'-reload.png'});
  item.pass=true;
 }catch(e){item.error=String(e);results.errors.push(which+' '+String(e));await page.screenshot({path:out+'/'+which+'-'+viewport.width+'-FAIL.png'}).catch(()=>{});}
 finally{item.logs=logs.filter(x=>/ELDORIA_PLAYABLE_(COMMAND|STATE|HOTSPOT|UI|NAV)|PAGEERROR|FAILED_REQUEST/.test(x)).slice(-150);await ctx.close();}
 results.cases.push(item);
}
(async()=>{
 const browser=await chromium.launch({args:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});
 try{await one(browser,'survey',{width:844,height:390});await one(browser,'harvest',{width:390,height:844});}
 finally{await browser.close();}
 results.pass=results.cases.length===2&&results.cases.every(x=>x.pass);
 fs.writeFileSync(out+'/report.json',JSON.stringify(results,null,2));
 console.log('ELDORIA_R2B_REAL_CANDIDATE_QA',JSON.stringify({pass:results.pass,cases:results.cases.map(x=>({choice:x.choice,pass:x.pass,error:x.error}))}));
 if(!results.pass)process.exitCode=1;
})().catch(e=>{console.error(e);process.exitCode=1;});
