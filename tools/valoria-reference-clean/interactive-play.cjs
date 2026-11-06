'use strict';
// A bounded, manually directed player session. No eval, save writes, engine API,
// clock override, console commands or arbitrary navigation are accepted.
const fs=require('node:fs'),path=require('node:path');
const {serveBuild}=require('./playtest_web.cjs');
const sleep=ms=>new Promise(r=>setTimeout(r,ms));
const ROOT='docs/evidence/player-audit-20261106';
const OWNER='codex-player-audit-23287cf1c126';
function validate(c,session,last,view){
 if(c.session!==session||!Number.isSafeInteger(c.seq)||c.seq<=last)throw Error('Stale/wrong session command');
 if(!Number.isFinite(Date.parse(c.at))||Math.abs(Date.now()-Date.parse(c.at))>900000)throw Error('Expired command');
 const point=(x,y)=>{if(!Number.isFinite(x)||!Number.isFinite(y)||x<0||y<0||x>=view.width||y>=view.height)throw Error('Point outside viewport');};
 if(['click','drag','wheel'].includes(c.type))point(c.x,c.y);
 if(c.type==='drag')point(c.toX,c.toY);
 if(c.type==='wheel'&&(!Number.isFinite(c.delta)||Math.abs(c.delta)>1200))throw Error('Invalid wheel');
 if(c.type==='type'&&(typeof c.text!=='string'||c.text.length>32||/[\r\n\x00-\x1f]/.test(c.text)))throw Error('Invalid player text');
 if(c.type==='key'&&!['Enter','Escape','Backspace','Tab','ArrowUp','ArrowDown','ArrowLeft','ArrowRight','Control+a'].includes(c.key))throw Error('Invalid key');
 if(c.type==='resize'&&![[844,390],[390,844],[960,540]].some(v=>v[0]===c.width&&v[1]===c.height))throw Error('Unsupported viewport');
 if(!['click','drag','wheel','type','key','observe','resize','reload','finish'].includes(c.type))throw Error('Unsupported visible action');
 return c;
}
async function main(){
 const cfg=JSON.parse(fs.readFileSync('tools/valoria-reference-clean/browser-qa-request.json','utf8'));
 const session=cfg.manual_session,branch=cfg.controls_branch;
 if(cfg.interaction_mode!=='manual'||session!=='native299-player-20261106'||branch!=='qa/player-audit-controls-20261106')throw Error('Manual session not explicitly configured');
 const token=process.env.GITHUB_TOKEN,repo=process.env.GITHUB_REPOSITORY;
 if(!token||repo!=='mt5hjfz2kb-lab/Eldoria-Prewiu')throw Error('Repository session unavailable');
 const api=async(route,body)=>{const r=await fetch('https://api.github.com/repos/'+repo+route,{method:body?'PUT':'GET',headers:{Authorization:'Bearer '+token,Accept:'application/vnd.github+json','X-GitHub-Api-Version':'2022-11-28'},body:body?JSON.stringify(body):undefined,signal:AbortSignal.timeout(25000)});if(!r.ok)throw Error('Repository request failed '+r.status+' '+route.split('?')[0]);return r.json();};
 const read=async(p,ref)=>{const d=await api('/contents/'+p+'?ref='+encodeURIComponent(ref));return {sha:d.sha,value:JSON.parse(Buffer.from(d.content,'base64').toString('utf8'))};};
 const lease=async()=>{const {value:r}=await read('pipeline/active-workstreams.json','main');const w=r.active.find(x=>x.id==='valoria-master-plan-layered-construction-20261016');if(w?.owner!==OWNER||w.ownership_revision!==151||w.write_scope_released||!w.resources.includes('windows-runner-heavy'))throw Error('Manual session lease changed');};
 await lease();
 const out=path.resolve(process.env.VALORIA_PLAYTEST_OUT);fs.mkdirSync(out,{recursive:true});
 const site=await serveBuild(path.resolve(process.env.VALORIA_WEB_ROOT));
 const {chromium}=require('playwright');
 const context=await chromium.launchPersistentContext(path.join(out,'isolated-player-profile'),{headless:true,executablePath:process.env.VALORIA_CHROMIUM_EXECUTABLE||undefined,args:['--no-sandbox','--use-angle=d3d11','--ignore-gpu-blocklist','--enable-webgl'],viewport:{width:844,height:390},deviceScaleFactor:1,recordVideo:{dir:path.join(out,'video'),size:{width:960,height:844}}});
 const page=context.pages()[0]||await context.newPage();page.setDefaultTimeout(30000);
 const log=(file,data)=>fs.appendFileSync(path.join(out,file),JSON.stringify({at:new Date().toISOString(),...data})+'\n');
 page.on('pageerror',e=>log('runtime-errors.ndjson',{message:e.message}));
 page.on('console',m=>{if(m.type()==='error'||m.type()==='warning')log('console.ndjson',{level:m.type(),message:m.text()});});
 page.on('requestfailed',r=>log('network-errors.ndjson',{url:r.url(),failure:r.failure()}));
 let seq=0,view={width:844,height:390},lastLease=Date.now(),lastAction=Date.now();const started=Date.now();
 const publish=async(state,error=null)=>{const png=await page.screenshot({timeout:30000});const filename=String(seq).padStart(4,'0')+'-'+state+'.png';fs.writeFileSync(path.join(out,filename),png);const {sha}=await read(ROOT+'/frame.json',branch);const frame={session,seq,state,at:new Date().toISOString(),view,error,image_base64:png.toString('base64'),filename,run:process.env.GITHUB_RUN_ID,source:process.env.VALORIA_SOURCE_SHA};await api('/contents/'+ROOT+'/frame.json',{branch,sha,message:'Player session frame '+seq+' [skip ci]',content:Buffer.from(JSON.stringify(frame)).toString('base64')});console.log('PLAYER_FRAME '+JSON.stringify({seq,state,filename}));};
 let result='INTERRUPTED';
 try{
  await page.goto(site.url,{waitUntil:'domcontentloaded'});await publish('ready');
  while(Date.now()-started<3*60*60*1000){
   if(Date.now()-lastLease>30000){await lease();lastLease=Date.now();}
   if(Date.now()-lastAction>25*60*1000)throw Error('No player command for 25 minutes; release session');
   const {value:c}=await read(ROOT+'/command.json',branch);
   if(c.session===session&&Number.isSafeInteger(c.seq)&&c.seq>seq){
    validate(c,session,seq,view);seq=c.seq;lastAction=Date.now();log('actions.ndjson',c);
    if(c.type==='finish'){result='MANUAL_SESSION_ENDED';await publish('finished');break;}
    if(c.type==='click')await page.mouse.click(c.x,c.y);
    else if(c.type==='drag'){await page.mouse.move(c.x,c.y);await page.mouse.down();await page.mouse.move(c.toX,c.toY,{steps:18});await page.mouse.up();}
    else if(c.type==='wheel'){await page.mouse.move(c.x,c.y);await page.mouse.wheel(0,c.delta);}
    else if(c.type==='type')await page.keyboard.type(c.text);
    else if(c.type==='key')await page.keyboard.press(c.key);
    else if(c.type==='resize'){view={width:c.width,height:c.height};await page.setViewportSize(view);}
    else if(c.type==='reload')await page.reload({waitUntil:'domcontentloaded'});
    await sleep(800);await publish(c.type);
   }
   await sleep(2500);
  }
 }catch(e){log('session-errors.ndjson',{message:e.message});try{await publish('error',e.message);}catch{}throw e;}
 finally{fs.writeFileSync(path.join(out,'manual-session.json'),JSON.stringify({status:result,session,seq,started:new Date(started).toISOString(),ended:new Date().toISOString(),source:process.env.VALORIA_SOURCE_SHA,note:'No automatic gameplay verdict; reviewer must assess captured actions and results.'},null,2));await context.close();await site.close();}
}
module.exports={validate};
if(require.main===module)main().catch(e=>{console.error(e.message);process.exitCode=1;});
