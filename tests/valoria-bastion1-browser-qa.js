const { chromium }=require('playwright');
const { PNG }=require('pngjs');
const fs=require('fs'), path=require('path');
const dir='bastion1-browser-evidence';
fs.mkdirSync(dir,{recursive:true});
const report={exactBuildRun:38314377442,sourceSha:'8758911e81f12b147391a07d7d5cd5d79eb9f4c3',webglBuildPass:true,browserStartupPass:false,cameraPanPass:false,sawmillSelectionPass:false,worldReturnPass:false,reloadPass:false,zeroFatalErrors:false,visualPass:false,physicalPhonePass:false,checks:[],errors:[],events:[],state:[],shots:[],accepted:false};
const sleep=ms=>new Promise(resolve=>setTimeout(resolve,ms));
let browser;
function state(m){const a=m.match(/ELDORIA_PLAYABLE_STATE tag=([^ ]+) scene=([^ ]+) revision=(\d+) wood=(\d+) stone=(\d+).*sawmill=(\d+) bastion=(\d+) barracks=(\d+)/);return a?{tag:a[1],scene:a[2],revision:+a[3],wood:+a[4],stone:+a[5],sawmill:+a[6],bastion:+a[7],barracks:+a[8]}:null;}
function findId(id){for(let i=report.events.length-1;i>=0;i--){let s=report.events[i];if(!s.includes('ELDORIA_PLAYABLE_UI id='+id+' '))continue;let m=s.match(/x=([-0-9.]+) y=([-0-9.]+)/);if(m)return {x:+m[1],y:+m[2]};}return null;}
function hotspot(id){for(let i=report.events.length-1;i>=0;i--){let s=report.events[i];if(!s.includes('ELDORIA_PLAYABLE_HOTSPOT id='+id+' '))continue;let m=s.match(/x=([-0-9.]+) y=([-0-9.]+)/);if(m)return {x:+m[1],y:+m[2]};}return null;}
async function waitUntil(fn,timeout=80000){const end=Date.now()+timeout;while(Date.now()<end){if(fn())return true;await sleep(450);}return false;}
async function snapshot(page,name){const file=path.join(dir,name+'.png');await page.screenshot({path:file,timeout:30000});report.shots.push(file);const png=PNG.sync.read(fs.readFileSync(file));let hot=0;for(let i=0;i<png.data.length;i+=4){const a=png.data[i],b=png.data[i+1],c=png.data[i+2];if(a>210&&b<90&&c>210)hot++;}const ratio=hot/(png.width*png.height);report.checks.push({shot:name,pinkRatio:ratio,width:png.width,height:png.height});return ratio;}
async function gesture(cdp,x,y,dx,dy){await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x,y,id:1}]});for(let i=1;i<=9;i++){await cdp.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{x:x+dx*i/9,y:y+dy*i/9,id:1}]});await sleep(40);}await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});await sleep(450);}
(async()=>{
 try{
  browser=await chromium.launch({args:['--use-angle=swiftshader','--enable-unsafe-swiftshader','--ignore-gpu-blocklist']});
  const context=await browser.newContext({viewport:{width:844,height:390},isMobile:true,hasTouch:true,deviceScaleFactor:1});
  const page=await context.newPage(),cdp=await context.newCDPSession(page);
  page.on('console',m=>{const s=m.text();if(s.includes('ELDORIA_PLAYABLE_')){report.events.push(s);const st=state(s);if(st)report.state.push(st);}if(m.type()==='error'&&!s.includes('WebGL: INVALID_ENUM'))report.errors.push('console: '+s.slice(0,700));});
  page.on('pageerror',e=>report.errors.push('page: '+String(e).slice(0,700)));
  page.on('requestfailed',r=>report.errors.push('request: '+r.url()+' '+r.failure()));
  await page.goto('http://127.0.0.1:8765/',{waitUntil:'domcontentloaded',timeout:40000});
  report.browserStartupPass=await waitUntil(()=>report.state.some(s=>s.scene==='ValoriaWebGL'),115000);
  report.checks.push({startup:report.browserStartupPass,state:report.state[0]||null});
  if(!report.browserStartupPass)throw Error('No ValoriaWebGL playable state emitted after 115 seconds');
  await sleep(1800);
  const pink=await snapshot(page,'01-initial-landscape');
  const pans0=report.events.filter(s=>s.includes('ELDORIA_PLAYABLE_TOUCH_PAN')).length;
  for(const [dx,dy] of [[110,0],[-110,0],[0,80],[0,-80]])await gesture(cdp,420,205,dx,dy);
  const pans=report.events.filter(s=>s.includes('ELDORIA_PLAYABLE_TOUCH_PAN')).length-pans0;
  report.cameraPanPass=pans>=2;report.checks.push({panLogCount:pans});
  const home=findId('home');if(home){await page.touchscreen.tap(home.x,home.y);await sleep(700);}
  await snapshot(page,'02-pan-home');
  const target=hotspot('sawmill');report.checks.push({sawmillTarget:target});
  if(target&&target.x>=0&&target.x<844&&target.y>=0&&target.y<390){
    await page.touchscreen.tap(target.x,target.y);await sleep(1200);
    report.sawmillSelectionPass=!!findId('buildingAction');
    report.checks.push({buildingAction:findId('buildingAction')});
    await snapshot(page,'03-sawmill-selection');
    const action=findId('buildingAction');
    if(action){const initialRev=report.state.at(-1)?.revision;await page.touchscreen.tap(action.x,action.y);await sleep(1500);report.checks.push({sawmillRevisionBefore:initialRev,after:report.state.at(-1)?.revision});}
  }
  const world=findId('worldNav');report.checks.push({worldNav:world});
  if(world){await page.touchscreen.tap(world.x,world.y);await waitUntil(()=>report.state.some(s=>s.scene==='Frontier'),25000);await snapshot(page,'04-world');const city=findId('cityNav');report.checks.push({cityNav:city});if(city){await page.touchscreen.tap(city.x,city.y);report.worldReturnPass=await waitUntil(()=>report.state.filter(s=>s.scene==='ValoriaWebGL').length>=2,30000);}}
  await page.reload({waitUntil:'domcontentloaded',timeout:35000});
  const count=report.state.length;
  report.reloadPass=await waitUntil(()=>report.state.length>count&&report.state.at(-1).scene==='ValoriaWebGL',110000);
  await snapshot(page,'05-reload');
  report.zeroFatalErrors=report.errors.length===0;
  report.visualPass=false; // Requires human inspection against owner reference; never assert art PASS from a screenshot.
  report.accepted=report.browserStartupPass&&report.cameraPanPass&&report.sawmillSelectionPass&&report.worldReturnPass&&report.reloadPass&&report.zeroFatalErrors&&pink<.055;
 }catch(e){report.errors.push('PROBE: '+String(e).slice(0,1000));}
 finally{if(browser)await browser.close().catch(()=>{});fs.writeFileSync(path.join(dir,'browser-qa-result.json'),JSON.stringify(report,null,2));console.log('BASTION1_BROWSER_QA',JSON.stringify({accepted:report.accepted,startup:report.browserStartupPass,pan:report.cameraPanPass,selection:report.sawmillSelectionPass,worldReturn:report.worldReturnPass,reload:report.reloadPass,errors:report.errors.slice(0,5),shots:report.shots}));if(!report.accepted)process.exitCode=1;}
})();
