const { chromium } = require('playwright');
const { PNG } = require('pngjs');
const pixelmatch = require('pixelmatch');
const fs = require('fs');
const { controlPoint, persistenceSnapshot, certifyMacroloop, fatalRuntimeLogs } = require(process.cwd() + '/tools/published-probe-contract.cjs');

const target=process.env.ELDORIA_PROBE_URL;
if(!target)throw new Error('Probe target was not resolved');
const sleep=ms=>new Promise(r=>setTimeout(r,ms));
const report={failure:null,logs:[],requests:[],checks:{navigationGatherPersistencePass:false},states:[],coverage:{fullBastionIToIIPass:false,visualQualityPass:false,independentHumanPlaythroughPass:false},scope:'Published limited macroloop probe: real touch pan/home, limited gather loop, Valoria↔World, Region 1 gather/reward, return, reload persistence, landscape+portrait screenshots. Chromium mobile emulation; physical iPhone owner test remains higher authority. Includes separate uninterrupted guided Bastion I-II and reset/reload flows in both orientations. Does not certify artistic quality or human playability.'};

report.delivery={kind:process.env.ELDORIA_PROBE_KIND,target,
  sourceSha:process.env.ELDORIA_PROBE_SOURCE_SHA,buildRunId:process.env.ELDORIA_PROBE_BUILD_RUN,
  candidateOnly:process.env.ELDORIA_PROBE_KIND==='candidate',publishedPass:false};

function parseState(text){
  const m=text.match(/ELDORIA_PLAYABLE_STATE tag=([^ ]+) scene=([^ ]+) revision=(\d+) wood=(\d+) stone=(\d+) gatheredWood=(\d+) gatheredStone=(\d+) march=([^ ]+) sawmill=(\d+) bastion=(\d+)/);
  if(!m)return null;
  const extra=text.match(/barracks=(\d+) archers=(\d+) trained=(\d+) configured=(True|False) scout=(True|False) engendro=(True|False) objective=([^\s]+)/);
  return {tag:m[1],scene:m[2],revision:+m[3],wood:+m[4],stone:+m[5],gatheredWood:+m[6],gatheredStone:+m[7],march:m[8],sawmill:+m[9],bastion:+m[10],...(extra?{barracks:+extra[1],archers:+extra[2],trained:+extra[3],configured:extra[4]==='True',scout:extra[5]==='True',engendro:extra[6]==='True',objective:extra[7]}:{})};
}
function parseUi(logs,id){
  for(let i=logs.length-1;i>=0;i--){
    const t=logs[i].text||'';
    const m=t.match(new RegExp('ELDORIA_PLAYABLE_UI id='+id+' x=([-0-9.]+) y=([-0-9.]+)'));
    if(m)return {x:+m[1],unityY:+m[2]};
  }
  return null;
}
function parseHotspot(logs,id){
  for(let i=logs.length-1;i>=0;i--){
    const t=logs[i].text||'';
    const m=t.match(new RegExp('ELDORIA_PLAYABLE_HOTSPOT id='+id+' x=([0-9.]+) y=([0-9.]+)'));
    if(m)return {x:+m[1],y:+m[2]};
  }
  return null;
}
async function waitLog(logs,predicate,timeout=60000,label='log',startIndex=0){
  const started=Date.now();
  while(Date.now()-started<timeout){
    for(let i=logs.length-1;i>=startIndex;i--) if(predicate(logs[i].text||'')) return logs[i].text;
    await sleep(250);
  }
  throw new Error('Timed out waiting for '+label);
}
async function waitState(logs,predicate,timeout=60000,label='state',startIndex=0){
  const started=Date.now();
  while(Date.now()-started<timeout){
    for(let i=logs.length-1;i>=startIndex;i--){
      const state=parseState(logs[i].text||'');
      if(state&&predicate(state)){report.states.push(state);return state;}
    }
    await sleep(250);
  }
  throw new Error('Timed out waiting for '+label);
}
function screenPoint(ui,box,canvasInfo){
  // Runtime logs use RectTransformUtility.WorldToScreenPoint, i.e. physical Unity
  // screen pixels with bottom-left origin. Map them directly into the CSS canvas box.
  return {
    x:box.x+(ui.x/canvasInfo.width)*box.width,
    y:box.y+((canvasInfo.height-ui.unityY)/canvasInfo.height)*box.height
  };
}
function hotspotPoint(hotspot,box,canvasInfo){
  return {
    x:box.x+(hotspot.x/canvasInfo.width)*box.width,
    y:box.y+(hotspot.y/canvasInfo.height)*box.height
  };
}
async function touchTap(cdp,x,y){
  await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x,y,radiusX:5,radiusY:5,force:1,id:1}]});
  await sleep(80);
  await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
}
async function touchDrag(cdp,x1,y1,x2,y2){
  await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:x1,y:y1,radiusX:5,radiusY:5,force:1,id:1}]});
  for(let i=1;i<=8;i++){
    const t=i/8;
    await cdp.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{x:x1+(x2-x1)*t,y:y1+(y2-y1)*t,radiusX:5,radiusY:5,force:1,id:1}]});
    await sleep(35);
  }
  await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
}
async function tapNamedNavigationControl(page,box,canvasInfo,logs,controlId,predicate,label,startIndex){
  const ui=parseUi(logs,controlId);
  const point=controlPoint(ui,box,canvasInfo);
  // One named control, one touch. Searching the entire navigation band can
  // hide displaced/offscreen buttons and must not be accepted as usability proof.
  await page.touchscreen.tap(point.x,point.y);
  await waitLog(logs,predicate,10000,label,startIndex);
  return {controlId,...point,attempts:1};
}
function imageStats(path){
  const png=PNG.sync.read(fs.readFileSync(path));
  let n=0, sum=0, sum2=0, white=0;
  const x0=Math.floor(png.width*.18), x1=Math.floor(png.width*.82);
  const y0=Math.floor(png.height*.12), y1=Math.floor(png.height*.88);
  for(let y=y0;y<y1;y++)for(let x=x0;x<x1;x++){
    const i=(y*png.width+x)*4;
    const r=png.data[i],g=png.data[i+1],b=png.data[i+2];
    const v=(r+g+b)/3; n++; sum+=v; sum2+=v*v;
    if(r>245&&g>245&&b>245)white++;
  }
  const mean=sum/n, variance=sum2/n-mean*mean;
  return {width:png.width,height:png.height,mean,stdev:Math.sqrt(Math.max(0,variance)),whiteRatio:white/n};
}
function primaryCtaBrightPixels(path,orientation){
  const png=PNG.sync.read(fs.readFileSync(path));
  const box=orientation==='portrait'?[.21,.86,.79,.89]:[.39,.72,.61,.78];
  let bright=0;
  for(let y=Math.floor(png.height*box[1]);y<Math.floor(png.height*box[3]);y++)
    for(let x=Math.floor(png.width*box[0]);x<Math.floor(png.width*box[2]);x++){
      const i=(y*png.width+x)*4;
      if(Math.max(png.data[i],png.data[i+1],png.data[i+2])>150)bright++;
    }
  return bright;
}
function diffRatio(aPath,bPath){
  const a=PNG.sync.read(fs.readFileSync(aPath)), b=PNG.sync.read(fs.readFileSync(bPath));
  if(a.width!==b.width||a.height!==b.height)throw new Error('Screenshot dimensions differ');
  const out=new PNG({width:a.width,height:a.height});
  const changed=pixelmatch(a.data,b.data,out.data,a.width,a.height,{threshold:.12});
  return changed/(a.width*a.height);
}

function lastState(logs,start=0){
  for(let i=logs.length-1;i>=start;i--){const state=parseState(logs[i].text||'');if(state)return state;}
  return null;
}
function detailedUi(logs,id,start=0){
  for(let i=logs.length-1;i>=start;i--){
    const m=(logs[i].text||'').match(new RegExp('ELDORIA_PLAYABLE_UI id='+id+' x=([-0-9.]+) y=([-0-9.]+) enabled=(True|False) visible=(True|False) label=([^\\n]*)'));
    if(m)return {x:+m[1],unityY:+m[2],enabled:m[3]==='True',visible:m[4]==='True',label:m[5]};
  }
  return null;
}
async function tapLoggedUi(page,logs,id,start=0){
  let ui,lastMappingError=null;
  const locator=page.locator('canvas');
  const begin=Date.now();
  while(Date.now()-begin<90000){
    ui=detailedUi(logs,id,start);
    if(ui&&ui.enabled&&ui.visible){
      const box=await locator.boundingBox();
      const dimensions=await locator.evaluate(c=>({width:c.width,height:c.height}));
      try{
        const point=controlPoint(ui,box,dimensions);
        await page.touchscreen.tap(point.x,point.y);
        return {...ui,css:point};
      }catch(error){
        // After a mobile orientation resize Unity can briefly expose the
        // previous frame's UI coordinates. Do not accept those stale
        // coordinates, but wait for the next runtime geometry publication.
        lastMappingError=error;
      }
    }
    await sleep(200);
  }
  throw new Error('No usable named control '+id+(lastMappingError?': '+lastMappingError.message:''));
}
function fullSnapshot(s){
  return Object.fromEntries(['revision','wood','stone','gatheredWood','gatheredStone','march','sawmill','bastion','barracks','archers','trained','configured','scout','engendro','objective'].map(k=>[k,s[k]]));
}
async function resetPublishedGame(page,logs){
  // A deliberate two-step reset is part of the real owner UX. Observe the first
  // touch before deciding whether it armed confirmation or already completed a
  // previously armed reset, then require explicit execution evidence.
  const start=logs.length;
  await tapLoggedUi(page,logs,'reset');
  const canonical=s=>s.revision===0&&s.sawmill===0&&s.bastion===1&&s.objective==='b1.build-sawmill';
  const firstDeadline=Date.now()+2500;
  let armed=false;
  while(Date.now()<firstDeadline){
    for(let i=logs.length-1;i>=start;i--){
      const text=logs[i].text||'';
      const state=parseState(text);
      if(state&&canonical(state)){report.states.push(state);await sleep(1200);return state;}
      if(text.includes('ELDORIA_PLAYABLE_RESET armed=True'))armed=true;
    }
    if(armed)break;
    await sleep(100);
  }
  if(!armed)throw new Error('Reset first touch neither armed confirmation nor executed reset');
  const executeStart=logs.length;
  await tapLoggedUi(page,logs,'reset');
  await waitLog(logs,t=>t.includes('ELDORIA_PLAYABLE_RESET execute=True'),10000,'reset execution',executeStart);
  const state=await waitState(logs,canonical,30000,'canonical reset',executeStart);
  await sleep(1200);
  return state;
}
async function fullPublishedFlow(page,logs,orientation){
  const result={orientation,steps:[],selectedBuildings:[],pass:false,persistencePass:false,resetPass:false};
  report.fullFlows=report.fullFlows||[];report.fullFlows.push(result);
  await page.setViewportSize(orientation==='portrait'?{width:390,height:844}:{width:844,height:390});
  await sleep(1000);
  const fresh=await resetPublishedGame(page,logs);
  if(fresh.wood!==230||fresh.stone!==150||fresh.barracks!==0||fresh.engendro)throw new Error('Reset is not canonical initial state');
  const flowStart=0;
  await page.screenshot({path:'webgl-startup-evidence/'+orientation+'-fresh-bastion-i.png'});
  let final;
  for(let step=0;step<70;step++){
    let state=lastState(logs,flowStart);
    if(state&&state.objective==='b2.complete'&&state.march==='idle'&&state.scene!=='Frontier'){final=state;break;}
    // Wait through genuine timers. Never inject state, advance clocks or invoke gateway methods.
    const readyStart=Date.now();
    let ui;
    while(Date.now()-readyStart<90000){
      ui=detailedUi(logs,'primary',flowStart);
      state=lastState(logs,flowStart);
      if(state&&state.objective==='b2.complete'&&state.march==='idle'&&state.scene!=='Frontier'){final=state;break;}
      if(ui&&ui.enabled&&ui.visible)break;
      await sleep(250);
    }
    if(final)break;
    if(!ui||!ui.enabled||!ui.visible)throw new Error('Guided objective is unavailable: '+JSON.stringify(state));
    state=lastState(logs,flowStart);
    const before=logs.length;
    const tapped=await tapLoggedUi(page,logs,'primary',flowStart);
    await sleep(750);
    const action=detailedUi(logs,'buildingAction',before);
    let buildingTap=null;
    if(action&&action.visible){
      const buildingId=({'b1.build-sawmill':'sawmill','b1.ascend':'bastion','b2.build-barracks':'barracks','b2.train-archers':'barracks'})[state.objective];
      if(buildingId&&!result.selectedBuildings.includes(buildingId)){
        // Independently select each real building at its visible presented-frame position.
        await tapLoggedUi(page,logs,'buildingClose',before);await sleep(500);
        const hotspot=parseHotspot(logs,buildingId);
        const canvas=page.locator('canvas'),hotspotBox=await canvas.boundingBox();
        const dim=await canvas.evaluate(c=>({width:c.width,height:c.height}));
        if(!hotspot)throw new Error('Presented '+buildingId+' geometry missing');
        const point=hotspotPoint(hotspot,hotspotBox,dim);
        if(point.x<hotspotBox.x||point.x>hotspotBox.x+hotspotBox.width||point.y<hotspotBox.y||point.y>hotspotBox.y+hotspotBox.height)throw new Error('Guided focus left '+buildingId+' outside viewport');
        const selectStart=logs.length;
        await page.touchscreen.tap(point.x,point.y);
        await waitLog(logs,t=>t.includes('ELDORIA_PLAYABLE_UI id=buildingAction ')&&/visible=True/.test(t),10000,'visible '+buildingId+' selection',selectStart);
        result.selectedBuildings.push(buildingId);
        result.presentedBuildingSelectionPass=result.selectedBuildings.length===3;
        await page.screenshot({path:'webgl-startup-evidence/'+orientation+'-selected-'+buildingId+'.png'});
      }
      buildingTap=await tapLoggedUi(page,logs,'buildingAction',before);await sleep(500);
    }
    const changed=await waitState(logs,s=>s.revision!==state.revision||s.scene!==state.scene||s.objective!==state.objective,30000,'guided step effect '+state.objective,before);
    result.steps.push({step,objective:state.objective,scene:state.scene,primary:tapped,buildingAction:buildingTap,after:changed});
    const stepImage='webgl-startup-evidence/'+orientation+'-step-'+String(step).padStart(2,'0')+'-'+state.objective+'.png';
    await page.screenshot({path:stepImage});
    if(state.objective==='b2.train-archers'){
      result.trainingCtaBrightPixels=primaryCtaBrightPixels(stepImage,orientation);
      // Static legacy-UI crop is not a valid test of the compact mesh HUD.
    }
    const quiet=Date.now();
    while(Date.now()-quiet<90000){
      const current=lastState(logs,flowStart),button=detailedUi(logs,'primary',before);
      if(current&&current.objective==='b2.complete'&&current.scene!=='Frontier'&&current.march==='idle')break;
      if(current&&button&&button.enabled&&current.march==='idle')break;
      await sleep(250);
    }
  }
  if(!final)throw new Error('Full '+orientation+' flow never reached city chapter-II completion');
  if(!result.presentedBuildingSelectionPass)throw new Error('Presented city selection unverified');
  if(final.bastion!==2||final.sawmill!==1||final.barracks!==1||final.trained!==20||!final.configured||!final.scout||!final.engendro||final.gatheredWood<600||final.gatheredStone<500)throw new Error('Full final state incomplete: '+JSON.stringify(final));
  result.final=fullSnapshot(final);
  const completedImage='webgl-startup-evidence/'+orientation+'-bastion-ii-complete.png';
  await page.screenshot({path:completedImage});
  result.completionCtaBrightPixels=primaryCtaBrightPixels(completedImage,orientation);
  // Compact HUD contrast is inspected from the genuine screenshots.
  const beforeReload=logs.length;
  await page.reload({waitUntil:'domcontentloaded',timeout:60000});
  await page.waitForFunction(()=>document.body.dataset.eldoriaEngineReady==='true',{},{timeout:300000});
  const restored=await waitState(logs,s=>s.scene!=='Frontier'&&s.objective==='b2.complete',90000,'full final reload',beforeReload);
  if(JSON.stringify(fullSnapshot(restored))!==JSON.stringify(result.final))throw new Error('Full final save changed across reload');
  result.persistencePass=true;
  await sleep(1500);
  const reset=await resetPublishedGame(page,logs);
  const resetStart=logs.length;
  await page.reload({waitUntil:'domcontentloaded',timeout:60000});
  await page.waitForFunction(()=>document.body.dataset.eldoriaEngineReady==='true',{},{timeout:300000});
  const resetReloaded=await waitState(logs,s=>s.revision===0&&s.objective==='b1.build-sawmill',90000,'reset survives reload',resetStart);
  if(JSON.stringify(fullSnapshot(resetReloaded))!==JSON.stringify(fullSnapshot(reset)))throw new Error('Reset state changed after reload');
  result.resetPass=true;result.pass=true;
}


(async()=>{
 const browser=await chromium.launch({args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--ignore-gpu-blocklist']});
 try {
 const context=await browser.newContext({viewport:{width:844,height:390},isMobile:true,hasTouch:true,deviceScaleFactor:1});
 const page=await context.newPage();
 const logs=report.logs;
 page.on('console',m=>logs.push({type:m.type(),text:m.text()}));
 page.on('pageerror',e=>logs.push({type:'pageerror',text:String(e)}));
 await page.route(url=>url.href.split('?')[0]===target,async route=>{
   const response=await route.fetch();const html=await response.text();const token='.then((unityInstance) => {';
   if(!html.includes(token))throw new Error('Unity loader callback changed');
   await route.fulfill({response,body:html.replace(token,token+" document.body.dataset.eldoriaEngineReady='true';")});
 });
 await page.goto(target,{waitUntil:'domcontentloaded',timeout:60000});
 await page.waitForFunction(()=>document.body.dataset.eldoriaEngineReady==='true',{},{timeout:300000});
 await waitState(logs,s=>s.scene==='ValoriaWebGL',90000,'candidate startup');
 await sleep(3000);
 await fullPublishedFlow(page,logs,'landscape');
 await fullPublishedFlow(page,logs,'portrait');
 const fatal=fatalRuntimeLogs(logs);
 if(fatal.length)throw new Error('Runtime errors '+JSON.stringify(fatal));
 report.coverage.fullBastionIToIIPass=report.fullFlows.every(f=>f.pass);
 report.scope='Exact mesh candidate uninterrupted real touch Bastion I-II flow in both orientations; no state injection or clock manipulation. Chromium emulation, not physical phone or independent human approval.';
 } catch(e){report.failure=String(e);process.exitCode=1;}
 finally {await browser.close();fs.writeFileSync('webgl-startup-evidence/full-player-result.json',JSON.stringify(report,null,2));console.log('BASTION_FULL_PLAYER_FLOW',JSON.stringify({failure:report.failure,flows:report.fullFlows?.map(x=>({orientation:x.orientation,pass:x.pass,final:x.final}))}));}
})();
