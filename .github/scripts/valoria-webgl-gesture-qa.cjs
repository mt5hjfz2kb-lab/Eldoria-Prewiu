'use strict';
const fs=require('node:fs');
const crypto=require('node:crypto');
const {spawn}=require('node:child_process');
const {chromium}=require('playwright');
const {PNG}=require('pngjs');
const pause=ms=>new Promise(resolve=>setTimeout(resolve,ms));
const must=(condition,message)=>{if(!condition)throw new Error(message);};
const panPattern=/VALORIA_CAMERA_PAN_VERIFIED x=([-0-9.]+) z=([-0-9.]+)/;
const zoomPattern=/VALORIA_CAMERA_ZOOM_VERIFIED size=([-0-9.]+)/;
const inputMarkers=logs=>logs.filter(x=>/VALORIA_CAMERA_(PAN|ZOOM)_VERIFIED/.test(x));
const imageStats=filename=>{
  const data=fs.readFileSync(filename);
  const png=PNG.sync.read(data);
  let sum=0,sum2=0,low=0,high=0,n=0;
  const colors=new Set();
  for(let y=0;y<png.height;y+=3)for(let x=0;x<png.width;x+=3){
    const k=(y*png.width+x)*4;
    const r=png.data[k],g=png.data[k+1],b=png.data[k+2];
    const l=.2126*r+.7152*g+.0722*b;
    sum+=l;sum2+=l*l;n++;
    if(l<35)low++;if(l>205)high++;
    colors.add(((r>>3)<<10)|((g>>3)<<5)|(b>>3));
  }
  const mean=sum/n,stdev=Math.sqrt(Math.max(0,sum2/n-mean*mean));
  return {width:png.width,height:png.height,bytes:data.length,mean:+mean.toFixed(2),stdev:+stdev.toFixed(2),colors:colors.size,darkFraction:+(low/n).toFixed(3),lightFraction:+(high/n).toFixed(3),sha256:crypto.createHash('sha256').update(data).digest('hex')};
};
const screenshot=async(page,session,path)=>{
  try{
    const result=await Promise.race([
      session.send('Page.captureScreenshot',{format:'png',captureBeyondViewport:false,fromSurface:true}),
      pause(45000).then(()=>{throw new Error('CDP screenshot timed out');})
    ]);
    fs.writeFileSync(path,Buffer.from(result.data,'base64'));
  }catch(err){
    console.log('CDP_SCREENSHOT_FALLBACK',String(err.message).slice(0,250));
    await page.screenshot({path,timeout:45000,animations:'allow'});
  }
  return imageStats(path);
};
async function dragMouse(page,box){
  const x=box.x+box.width*.47,y=box.y+box.height*.52;
  await page.mouse.move(x,y);
  await page.mouse.down();
  for(let i=1;i<=18;i++)await page.mouse.move(x+11*i,y+3*i,{steps:2});
  await page.mouse.up();
  await page.mouse.wheel(0,700);
  await pause(1400);
}
async function touchGestures(session,box){
  const x=Math.round(box.x+box.width*.53),y=Math.round(box.y+box.height*.48);
  await session.send('Emulation.setTouchEmulationEnabled',{enabled:true,maxTouchPoints:2});
  await session.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x,y,id:1}]});
  for(let i=1;i<=12;i++){
    await session.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{x:x+i*12,y:y+i*3,id:1}]});
    await pause(90);
  }
  await session.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
  await pause(700);
  await session.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:x-38,y,id:1},{x:x+38,y,id:2}]});
  for(let i=1;i<=12;i++){
    await session.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{x:x-38-i*6,y,id:1},{x:x+38+i*6,y,id:2}]});
    await pause(100);
  }
  await session.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
  await pause(1300);
}
(async()=>{
  const server=spawn('python3',['-m','http.server','8765','--directory','publication'],{stdio:'ignore'});
  let browser;
  try{
    await pause(1400);
    browser=await chromium.launch({headless:true,args:['--enable-webgl','--use-gl=angle','--use-angle=swiftshader','--enable-unsafe-swiftshader','--disable-dev-shm-usage']});
    const context=await browser.newContext({viewport:{width:1280,height:720},deviceScaleFactor:1,hasTouch:true});
    const page=await context.newPage();
    page.setDefaultTimeout(60000);
    const logs=[],errors=[],badResponses=[];
    page.on('console',m=>logs.push(m.text()));
    page.on('pageerror',e=>errors.push(e.message));
    page.on('response',r=>{if(r.status()>=400&&/valoria-experimental/.test(r.url()))badResponses.push(r.status()+' '+r.url());});
    await page.goto('http://127.0.0.1:8765/valoria-experimental/',{waitUntil:'domcontentloaded',timeout:60000});
    const canvas=page.locator('canvas').first();
    await canvas.waitFor({state:'visible',timeout:60000});
    const box=await canvas.boundingBox();
    must(box&&box.width>150&&box.height>150,'UNITY_CANVAS_NOT_VISIBLE');
    const session=await context.newCDPSession(page);
    let mouseLogs=[],ready=false;
    for(let attempt=1;attempt<=7;attempt++){
      await pause(attempt===1?24000:12000);
      const start=logs.length;
      await dragMouse(page,box);
      mouseLogs=logs.slice(start);
      if(mouseLogs.some(x=>panPattern.test(x))&&mouseLogs.some(x=>zoomPattern.test(x))){ready=true;break;}
      console.log('UNITY_GESTURE_RETRY',attempt,JSON.stringify(inputMarkers(mouseLogs).slice(-5)));
    }
    must(ready,'UNITY_RUNTIME_MOUSE_DRAG_AND_ZOOM_NOT_VERIFIED');
    const firstStats=await screenshot(page,session,'valoria-before-touch.png');
    const touchStart=logs.length;
    await touchGestures(session,box);
    const touchLogs=logs.slice(touchStart);
    const secondStats=await screenshot(page,session,'valoria-after-touch.png');
    console.log('MOUSE_INPUT_EVIDENCE',JSON.stringify(inputMarkers(mouseLogs).slice(-16)));
    console.log('TOUCH_INPUT_EVIDENCE',JSON.stringify(inputMarkers(touchLogs).slice(-16)));
    console.log('VISUAL_BEFORE',JSON.stringify(firstStats));
    console.log('VISUAL_AFTER',JSON.stringify(secondStats));
    console.log('BROWSER_ERRORS',JSON.stringify(errors));
    console.log('HTTP_ERRORS',JSON.stringify(badResponses));
    must(touchLogs.some(x=>panPattern.test(x)),'REAL_TOUCH_DRAG_NOT_VERIFIED');
    must(touchLogs.some(x=>zoomPattern.test(x)),'REAL_TOUCH_PINCH_NOT_VERIFIED');
    must(firstStats.sha256!==secondStats.sha256,'CAMERA_GESTURES_DID_NOT_CHANGE_RENDER');
    must(secondStats.width>=1000&&secondStats.height>=600,'SCREENSHOT_NOT_FULL_VIEWPORT');
    must(secondStats.colors>180&&secondStats.stdev>12,'VISUAL_FRAME_BLANK_OR_FLAT');
    must(secondStats.mean>10&&secondStats.mean<245,'VISUAL_LIGHTING_EXTREME');
    must(errors.length===0,'BROWSER_JAVASCRIPT_ERRORS');
    must(badResponses.length===0,'UNITY_ASSET_HTTP_ERRORS');
    fs.writeFileSync('valoria-browser-qa-report.json',JSON.stringify({pass:true,mouse:inputMarkers(mouseLogs),touch:inputMarkers(touchLogs),before:firstStats,after:secondStats,errors,badResponses},null,2));
    console.log('VALORIA_REAL_MOUSE_TOUCH_MATERIAL_LIGHTING_SCREENSHOT_QA_PASS');
  }finally{
    if(browser)await browser.close();
    server.kill();
  }
})().catch(e=>{console.error('VALORIA_BROWSER_QA_FAILURE',e.stack||e);process.exit(1)});
