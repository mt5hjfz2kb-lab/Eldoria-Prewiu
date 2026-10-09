'use strict';
// Independent D13/D10 real Unity WebGL save-load failure UX probe.
// Corruption is an isolated browser-storage test fixture, never a game/source defect.
const fs=require('node:fs'),assert=require('node:assert/strict');
const {chromium}=require('playwright');
const url=process.env.ELDORIA_URL||'http://127.0.0.1:4173/';
const out=process.env.ELDORIA_EVIDENCE_DIR||'local-save-playable-evidence';
fs.mkdirSync(out,{recursive:true});
const result={source_sha:process.env.ELDORIA_SOURCE_SHA,url,engine:'Actual Unity WebGL player',independent_reviewer:'D13/D10 hosted browser QA',scenarios:[],pass:false};
const sleep=ms=>new Promise(r=>setTimeout(r,ms));
async function wait(logs,match,label,start=0){for(let i=0;i<720;i++){const x=logs.slice(start).findLast(l=>match.test(l));if(x)return x;await sleep(250);}throw Error('Unity timeout: '+label);}
async function storage(page,mutate){
 return page.evaluate(async mutate=>{
  const found=[],inventory=[];
  const corrupt='{"SchemaVersion":999,"testFixture":"isolated-browser-save-recovery"}';
  function change(v){
   if(typeof v==='string'&&v.includes('"SchemaVersion"'))return corrupt;
   if(v instanceof ArrayBuffer||ArrayBuffer.isView(v)){
    const bytes=v instanceof ArrayBuffer?new Uint8Array(v):new Uint8Array(v.buffer,v.byteOffset,v.byteLength);
    if(bytes.byteLength>65536)return null;
    // Observed actual Unity6000 FILE_DATA/PlayerPrefs record contains a binary envelope.
    // Change only the saved JSON schema digit 1→9, preserving envelope length and all other bytes.
    const patches=new Set();
    for(const literal of ['"SchemaVersion"','&quot;SchemaVersion&quot;','\\"SchemaVersion\\"']){
      for(const stride of [1,2]){
        const ascii=new TextEncoder().encode(literal),pattern=stride===1?ascii:Uint8Array.from(Array.from(ascii).flatMap(x=>[x,0]));
        for(let i=0;i<=bytes.length-pattern.length;i++){
          if(!pattern.every((x,k)=>bytes[i+k]===x))continue;
          let at=i+pattern.length;
          while([9,10,13,32].includes(bytes[at]))at+=stride;
          if(bytes[at]!==58)continue;at+=stride;
          while([9,10,13,32].includes(bytes[at]))at+=stride;
          if(bytes[at]===49||bytes[at]===57)patches.add(at);
        }
      }
    }
    if(patches.size!==1)return null;
    const copy=new Uint8Array(bytes);for(const at of patches)copy[at]=57;
    if(v instanceof ArrayBuffer)return copy.buffer;
    if(v instanceof Uint8Array)return copy;
    return null;
   }
   if(v&&typeof v==='object'){
    const copy=Array.isArray(v)?[...v]:{...v};let yes=false;
    for(const k of Object.keys(copy)){const next=change(copy[k]);if(next!==null){copy[k]=next;yes=true;}}
    return yes?copy:null;
   }
   return null;
  }
  for(let i=0;i<localStorage.length;i++){
   const key=localStorage.key(i),v=localStorage.getItem(key);inventory.push({type:'localStorage',key,preview:v?.slice(0,180)});
   if(v&&v.includes('"SchemaVersion"')){if(mutate)localStorage.setItem(key,corrupt);found.push({type:'localStorage',key,value:mutate?corrupt:v});}
  }
  for(const info of await indexedDB.databases()){
   if(!info.name)continue;
   const db=await new Promise((resolve,reject)=>{const r=indexedDB.open(info.name);r.onsuccess=()=>resolve(r.result);r.onerror=()=>reject(r.error);});
   for(const store of Array.from(db.objectStoreNames)){
    const entries=await new Promise((resolve,reject)=>{const data=[];const tx=db.transaction(store,'readonly'),c=tx.objectStore(store).openCursor();c.onsuccess=()=>{const x=c.result;if(x){data.push({key:x.key,value:x.value});x.continue();}else resolve(data);};c.onerror=()=>reject(c.error);});
    for(const e of entries){
     const next=typeof e.key==='string'&&e.key.endsWith('/PlayerPrefs')?change(e.value):null;inventory.push({db:info.name,store,key:e.key,type:typeof e.value,preview:typeof e.value==='string'?e.value.slice(0,180):JSON.stringify({keys:Object.keys(e.value||{}).slice(0,8),bytes:e.value?.byteLength,contentsBytes:e.value?.contents?.byteLength})});
     if(next!==null){
      if(mutate)await new Promise((resolve,reject)=>{const tx=db.transaction(store,'readwrite');const os=tx.objectStore(store);const req=os.keyPath?os.put(next):os.put(next,e.key);req.onerror=()=>reject(req.error);tx.oncomplete=resolve;tx.onerror=()=>reject(tx.error);});
      found.push({db:info.name,store,key:e.key,value:((mutate?next:e.value).contents instanceof ArrayBuffer?Array.from(new Uint8Array((mutate?next:e.value).contents)):Array.from((mutate?next:e.value).contents||[]))});
     }
    }
   }
   db.close();
  }
  return {found,inventory};
 },mutate);
}
async function scenario(browser,label,viewport,dpr=1){
 const initialViewport={...viewport};
 const context=await browser.newContext({viewport,hasTouch:true,isMobile:true,deviceScaleFactor:dpr,userAgent:'Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Mobile Safari/537.36'});
 const page=await context.newPage(),logs=[];page.on('console',m=>logs.push(m.text()));page.on('pageerror',e=>logs.push('PAGEERROR '+e));
 const item={label,viewport:{...viewport},dpr,pass:false};
 try{
  await page.goto(url,{waitUntil:'domcontentloaded'});
  await wait(logs,/ELDORIA_PLAYABLE_STATE tag=scene-loaded/,'healthy playable boot');
  await page.waitForFunction(()=>{const e=document.querySelector('#unity-loading-bar');return e&&getComputedStyle(e).display==='none';},null,{timeout:180000});await sleep(500);
  assert(!logs.some(l=>l.includes('ELDORIA_SAVE_RECOVERY_NOTICE shown=true')),'Healthy save displays erroneous warning');
  await page.screenshot({path:out+'/'+label+'-healthy-before.png'});
  // Genuine touch: ordinary world navigation creates/persists the player's real state.
  const nav=await wait(logs,/ELDORIA_PLAYABLE_UI id=worldNav x=([-0-9.]+) y=([-0-9.]+)/,'ordinary world button');
  const m=nav.match(/worldNav x=([-0-9.]+) y=([-0-9.]+)/),canvas=page.locator('canvas').first(),box=await canvas.boundingBox(),cv=await canvas.evaluate(e=>({width:e.width,height:e.height}));
  const point={x:box.x+Number(m[1])*box.width/cv.width,y:box.y+(cv.height-Number(m[2]))*box.height/cv.height};assert(point.x>=0&&point.x<viewport.width&&point.y>=0&&point.y<viewport.height,'World control unreachable');
  let start=logs.length;await page.touchscreen.tap(point.x,point.y);
  await wait(logs,/ELDORIA_PLAYABLE_STATE.*scene=Frontier/,'genuine navigation',start);
  await sleep(500);
  const hot=await wait(logs,/ELDORIA_PLAYABLE_HOTSPOT id=forest-valoria x=([0-9.]+) y=([0-9.]+)/,'real forest choice hotspot',start);
  const hm=hot.match(/forest-valoria x=([0-9.]+) y=([0-9.]+)/);
  start=logs.length;await page.touchscreen.tap(box.x+Number(hm[1])*box.width/cv.width,box.y+Number(hm[2])*box.height/cv.height);
  const action=await wait(logs,/ELDORIA_PLAYABLE_UI id=buildingAction x=([-0-9.]+) y=([-0-9.]+)/,'real forest choice panel',start);
  const am=action.match(/buildingAction x=([-0-9.]+) y=([-0-9.]+)/);
  const hudScale=Math.sqrt(cv.width/(viewport.width>viewport.height?844:390)*cv.height/(viewport.width>viewport.height?390:844));
  start=logs.length;await page.touchscreen.tap(box.x+Number(am[1])*box.width/cv.width,box.y+(cv.height-Number(am[2])+68*hudScale)*box.height/cv.height);
  await wait(logs,/ELDORIA_PLAYABLE_COMMAND kind=ChooseRegionOneForest target=forest-valoria:harvest ok=True/,'actual state-committing harvest command',start);
  await wait(logs,/ELDORIA_PLAYABLE_STATE tag=command-ChooseRegionOneForest-/,'actual authoritative commit',start);
  item.real_save_preparation_command=true;
  await sleep(800);
  await page.goto('about:blank');
  const storagePage=await context.newPage();await storagePage.goto(url+'isolated-storage-fixture.html',{waitUntil:'domcontentloaded'});
  const fixture=await storage(storagePage,true);item.isolated_fixture=fixture;
  await storagePage.close();
  assert(fixture.found.length>0,'Cannot prepare isolated corrupt save through existing browser storage; inspect inventory, do not invent a PASS');
  // Close the running player before relaunch so a dirty in-memory store cannot overwrite the fixture.
  start=logs.length;await page.goto(url,{waitUntil:'domcontentloaded'});
  const recoveryStart=start;
  await wait(logs,/ELDORIA_SAVE_RECOVERY_NOTICE shown=true/,'real load-failure notice',start);
  await page.waitForFunction(()=>{const e=document.querySelector('#unity-loading-bar');return e&&getComputedStyle(e).display==='none';},null,{timeout:180000});await sleep(700);
  const layout=await wait(logs,/ELDORIA_SAVE_RECOVERY_LAYOUT width=([0-9.]+) height=([0-9.]+) font=([0-9.]+)(?: scale=([0-9.]+))? screen=([0-9]+)x([0-9]+)/,'actual Unity layout',start);
  const l=layout.match(/width=([0-9.]+) height=([0-9.]+) font=([0-9.]+)(?: scale=([0-9.]+))? screen=([0-9]+)x([0-9]+)/),noticeScale=Number(l[4]||1);
  assert(Number(l[1])<=Number(l[5])-32&&Number(l[2])<=Number(l[6])-32,'Modal clipped in viewport');
  assert(Number(l[3])*box.width/cv.width>=16,'Actual notice font is unreadable on high-DPI device');
  await page.screenshot({path:out+'/'+label+'-notice.png'});
  // Exercise genuine underlying navigation/reset controls while the warning is open.
  const blockedStart=logs.length;
  for(const id of ['worldNav','reset','reset']){
    const control=await wait(logs,new RegExp('ELDORIA_PLAYABLE_UI id='+id+' x=([-0-9.]+) y=([-0-9.]+)'),'underlying '+id,recoveryStart);
    const cm=control.match(/ x=([-0-9.]+) y=([-0-9.]+)/);
    const bx=await canvas.boundingBox(),px=await canvas.evaluate(e=>({width:e.width,height:e.height}));
    const pt={x:bx.x+Number(cm[1])*bx.width/px.width,y:bx.y+(px.height-Number(cm[2]))*bx.height/px.height};
    assert(pt.x>=0&&pt.x<viewport.width&&pt.y>=0&&pt.y<viewport.height,'Underlying control fixture out of viewport');
    await page.touchscreen.tap(pt.x,pt.y);await sleep(250);
  }
  assert(!logs.slice(blockedStart).some(x=>/ELDORIA_PLAYABLE_(COMMAND|RESET)|ELDORIA_PLAYABLE_STATE.*scene=Frontier|ELDORIA_SAVE_RECOVERY_NOTICE acknowledged=true/.test(x)),'Underlying gameplay/reset executed before notice acknowledgement');
  item.modal_blocks_underlying_input=true;
  if(label==='landscape'){
    viewport={width:390,height:844};await page.setViewportSize(viewport);await sleep(1000);
    await page.screenshot({path:out+'/'+label+'-rotated-notice.png'});
    item.real_orientation_change={from:initialViewport,to:viewport,requires_independent_visual_review:true};
  }
  const before=await storage(page,false);assert(before.found.length>0);assert(before.found.every(x=>Array.isArray(x.value)&&x.value.length>0),'Preservation proof must include actual original PlayerPrefs bytes');assert(JSON.stringify(before.found)===JSON.stringify(fixture.found),'Original corrupted save was overwritten at load');
  const rect=await canvas.boundingBox(),pixels=await canvas.evaluate(e=>({width:e.width,height:e.height}));
  const ack={x:rect.x+rect.width/2,y:rect.y+rect.height/2+86*noticeScale*rect.height/pixels.height};assert(ack.y<viewport.height,'Acknowledgement button unreachable');
  start=logs.length;await page.touchscreen.tap(ack.x,ack.y);await wait(logs,/ELDORIA_SAVE_RECOVERY_NOTICE acknowledged=true/,'real notice acknowledgement',start);await sleep(500);
  await page.screenshot({path:out+'/'+label+'-acknowledged.png'});
  const after=await storage(page,false);assert.deepEqual(after.found,before.found,'Acknowledgement changed the original save');
  // Acknowledgement must restore actual gameplay; volatile commits must preserve the failed original.
  const resumed=await wait(logs,/ELDORIA_PLAYABLE_UI id=worldNav x=([-0-9.]+) y=([-0-9.]+)/,'resumed world control',recoveryStart);
  const rm=resumed.match(/worldNav x=([-0-9.]+) y=([-0-9.]+)/);
  const resumedBox=await canvas.boundingBox(),resumedCanvas=await canvas.evaluate(e=>({width:e.width,height:e.height}));
  const resumePoint={x:resumedBox.x+Number(rm[1])*resumedBox.width/resumedCanvas.width,y:resumedBox.y+(resumedCanvas.height-Number(rm[2]))*resumedBox.height/resumedCanvas.height};
  start=logs.length;await page.touchscreen.tap(resumePoint.x,resumePoint.y);
  await wait(logs,/ELDORIA_PLAYABLE_STATE.*scene=Frontier/,'real gameplay after notice acknowledgement',start);
  await sleep(800);
  const played=await storage(page,false);assert.deepEqual(played.found,before.found,'Volatile gameplay overwrote the original incompatible save');
  item.real_gameplay_resumed=true;item.volatile_gameplay_preserved_original=true;
  start=logs.length;await page.reload({waitUntil:'domcontentloaded'});await wait(logs,/ELDORIA_SAVE_RECOVERY_NOTICE shown=true/,'reload keeps failure explanation',start);
  item.pass=true;item.original_save_preserved=true;item.real_touch_acknowledged=true;item.reload_repeats_notice=true;item.healthy_no_false_warning=true;item.layout=layout;
 }catch(e){item.error=String(e);await page.screenshot({path:out+'/'+label+'-FAIL.png'}).catch(()=>{});}
 finally{item.logs=logs.filter(l=>/ELDORIA_SAVE_RECOVERY|ELDORIA_PLAYABLE_STATE|PAGEERROR|save load failed/.test(l)).slice(-50);result.scenarios.push(item);await context.close();}
}
(async()=>{const browser=await chromium.launch({args:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});try{await scenario(browser,'portrait',{width:390,height:844});await scenario(browser,'landscape',{width:844,height:390},2);}finally{await browser.close();}
 result.pass=result.scenarios.length===2&&result.scenarios.every(s=>s.pass);fs.writeFileSync(out+'/report.json',JSON.stringify(result,null,2));
 console.log('REAL_UNITY_SAVE_NOTICE_QA',JSON.stringify({pass:result.pass,scenarios:result.scenarios.map(s=>({label:s.label,pass:s.pass,error:s.error,inventory:s.error?s.isolated_fixture:undefined}))}));
 if(!result.pass)process.exitCode=1;
})().catch(e=>{console.error(e);process.exitCode=1;});
