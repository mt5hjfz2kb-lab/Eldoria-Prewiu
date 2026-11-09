import {createTelemetry,sendToQA,RELEASE,SOURCE} from './client.mjs';
import {parseUnitySignal} from './signal-parser.mjs';
import {startLocalRestartDiagnostic} from './restart-diagnostic.mjs';

// Pages-only QA preview: no login, APK, Cloudflare or saved-game mutation.
// The pilot overlay is not injected into the stable public game by default.
export async function startPilot({win=window,doc=document,fetchImpl=fetch}={}){
 const release=await fetchImpl('./release-identity.json',{cache:'no-store'}).then(r=>r.ok?r.json():null).catch(()=>null);
 if(!release||release.revision!==RELEASE||release.source_sha!==SOURCE){
  console.warn('ELDORIA_PILOT_INSTRUMENTATION_DISABLED_BUILD_MISMATCH');
  return {active:false,reason:'build_mismatch'};
 }
 // Local P0 diagnostics are separate from Supabase consent: only ?eldoria_diag=1 activates them.
 startLocalRestartDiagnostic({win,doc,build:RELEASE});
 const store=win.localStorage,uuid=()=>win.crypto.randomUUID();
 const agent=win.navigator.userAgent;
 const device=/iphone|ipad|ipod/i.test(agent)?'ios_safari':
  /android/i.test(agent)?'android_chrome':
  /firefox/i.test(agent)?'desktop_firefox':
  /edg\//i.test(agent)?'desktop_edge':'desktop_chrome';
 const client=createTelemetry({
  send:e=>sendToQA(e,fetchImpl),storage:store,uuid,
  clock:()=>win.performance.now(),device
 });
 let done=false,ready=false,lastOrientation='',lastHeartbeat=0,frameLast=0;
 let frameSamples=[],hadConsent=client.hasExistingConsent();
 const seen=new Set();
 const style=(node,properties)=>Object.assign(node.style,properties);
 const button=(label,parent,onClick)=>{const b=doc.createElement('button');b.textContent=label;
  b.type='button';style(b,{padding:'8px 10px',borderRadius:'8px',fontSize:'13px',
   background:'#2d4052',border:'1px solid #899cad',color:'#fff',cursor:'pointer'});
  b.addEventListener('click',onClick);parent.appendChild(b);return b;};
 const toolbar=doc.createElement('div');
 style(toolbar,{position:'fixed',right:'8px',bottom:'8px',zIndex:'999999',
  padding:'4px',display:'none',gap:'4px',fontFamily:'system-ui,sans-serif'});
 doc.body.appendChild(toolbar);
 const revoke=button('Desactivar estadísticas',toolbar,()=>{
  client.setConsent(false);done=true;toolbar.remove();
 });
 function emit(name,payload={}){if(!done)void client.track(name,payload)}
 function observeSignal(message){
  const parsed=parseUnitySignal(message);
  if(!parsed)return;
  const key=parsed.name+':'+JSON.stringify(parsed.payload);
  if(parsed.name==='bastion_reached'||parsed.name==='end_of_slice'){
   if(seen.has(key))return;seen.add(key);
  }
  emit(parsed.name,parsed.payload);
  if(parsed.name==='end_of_slice')showSurvey();
 }
 function intercept(){
  for(const method of ['log','warn']){
   const original=win.console[method];
   win.console[method]=function(...args){
    try{for(const a of args)if(typeof a==='string')observeSignal(a)}catch(_){}
    return original.apply(this,args);
   };
  }
 }
 const orientation=()=>{
  const next=win.innerWidth>=win.innerHeight?'landscape':'portrait';
  if(next!==lastOrientation){lastOrientation=next;emit('orientation_state',{orientation:next})}
 };
 function frames(ts){
  if(done)return;
  if(ready&&doc.visibilityState==='visible'&&frameLast){
   const ms=ts-frameLast;if(ms>0&&ms<1000){
    frameSamples.push(Math.max(0,Math.min(240,1000/ms)));if(frameSamples.length>400)frameSamples.shift();
   }
  }
  frameLast=ts;win.requestAnimationFrame(frames);
 }
 function frameSummary(){
  if(frameSamples.length<25)return;
  frameSamples.sort((a,b)=>a-b);
  const pct=p=>Math.round(frameSamples[Math.floor((frameSamples.length-1)*p)]);
  emit('frame_summary',{fps_p50:pct(.50),fps_p05:pct(.05)});
  frameSamples=[];
 }
 function readyCheck(){
  if(ready||!win.valoriaGame||done)return;
  ready=true;emit('game_ready');emit('load_stage',{stage_code:'ready',
   duration_ms:Math.round(win.performance.now())});
 }
 function showSurvey(){
  if(doc.getElementById('eldoria-pilot-survey'))return;
  const box=doc.createElement('div');box.id='eldoria-pilot-survey';
  style(box,{position:'fixed',inset:'auto 8px 65px auto',zIndex:'999999',
   maxWidth:'300px',padding:'12px',color:'white',borderRadius:'12px',
   background:'#1d2939',fontFamily:'system-ui,sans-serif',fontSize:'13px'});
  const title=doc.createElement('strong');title.textContent='Bastión V completado · Opinión opcional';
  box.appendChild(title);
  const questions=[['clarity','¿Has entendido qué hacer?'],['fun','¿Te ha entretenido?'],['continue','¿Seguirías jugando?']];
  const selects=[];
  for(const [id,q] of questions){
   const row=doc.createElement('label');row.textContent=q;row.style.display='block';row.style.marginTop='8px';
   const select=doc.createElement('select');select.style.display='block';select.style.width='100%';
   for(const [value,label] of [['0','Prefiero no responder'],['1','No'],['2','No estoy seguro'],['3','Sí']]){
    const option=doc.createElement('option');option.value=value;option.textContent=label;select.appendChild(option);
   }row.appendChild(select);box.appendChild(row);selects.push([id,select]);
  }
  button('Enviar respuestas anónimas',box,async()=>{
   for(const [id,select] of selects)if(select.value!=='0')
    await client.track('survey_answer',{question_id:id,answer_code:select.value});
   await client.track('survey_completed');
   box.remove();
  });
  button('Cerrar',box,()=>box.remove());doc.body.appendChild(box);
 }
 function begin(){
  client.setConsent(true);
  toolbar.style.display='flex';
  intercept();orientation();win.requestAnimationFrame(frames);
  emit('session_start');
  if(hadConsent)emit('return_visit');
  win.addEventListener('resize',orientation,{passive:true});
  doc.addEventListener('visibilitychange',()=>{
   emit('visibility_state',{state_code:doc.visibilityState==='visible'?'foreground':'background'});
   if(doc.visibilityState==='visible')void client.flush();
  });
  win.addEventListener('error',()=>emit('client_error',{error_code:'JS_ERROR'}));
  win.addEventListener('unhandledrejection',()=>emit('client_error',{error_code:'PROMISE_ERROR'}));
  doc.addEventListener('webglcontextlost',()=>emit('client_error',{error_code:'WEBGL_CONTEXT_LOST'}),true);
  win.addEventListener('pagehide',(event)=>{
   frameSummary();emit('visibility_state',{state_code:'background'});
   // A BFCache navigation suspends the same document; it is not a final exit.
   // If persisted, retain the queue, listeners and consent for pageshow.
   // No sendBeacon: it cannot include the required published client key header.
   if(!event?.persisted)done=true;
  });
  win.addEventListener('pageshow',(event)=>{
   if(done||!event?.persisted)return;
   // Resume the original opted-in session without creating a fake new visitor.
   // Reset frame timing so the frozen interval is never counted as a low FPS.
   frameLast=0;lastHeartbeat=win.performance.now();
   orientation();emit('visibility_state',{state_code:'foreground'});
   void client.flush();
  });
  const begun=win.performance.now();
  const interval=win.setInterval(()=>{
   if(done){win.clearInterval(interval);return}
   readyCheck();
   if(doc.visibilityState==='visible'&&win.performance.now()-lastHeartbeat>=45000){
    lastHeartbeat=win.performance.now();emit('session_heartbeat');
   }
   frameSummary();void client.flush();
  },15000);
  const quick=win.setInterval(()=>{
   if(done||ready){win.clearInterval(quick);return}
   readyCheck();
  },1000);
 }
 if(hadConsent){begin();return {active:true,client,consented:true}}
 const panel=doc.createElement('div');
 style(panel,{position:'fixed',left:'8px',bottom:'8px',zIndex:'999999',
   maxWidth:'360px',padding:'12px',color:'#fff',background:'#1d2939',
   borderRadius:'12px',boxShadow:'0 3px 12px #0009',
   fontFamily:'system-ui,sans-serif',fontSize:'13px'});
 const label=doc.createElement('strong');label.textContent='Prueba de Eldoria · Estadísticas opcionales';
 const explanation=doc.createElement('p');
 explanation.textContent='¿Compartir datos anónimos de rendimiento y progreso Bastión I–V? No enviamos nombres, guardados ni mensajes. Puedes jugar sin estadísticas.';
 panel.append(label,explanation);
 const options=doc.createElement('div');style(options,{display:'flex',gap:'7px',flexWrap:'wrap'});
 button('Sí, compartir',options,()=>{panel.remove();begin()});
 button('Jugar sin estadísticas',options,()=>{panel.remove()});
 panel.appendChild(options);doc.body.appendChild(panel);
 return {active:true,client,consented:false};
}
if(typeof window!=='undefined'&&typeof document!=='undefined'){
 void startPilot().catch(()=>console.warn('ELDORIA_PILOT_INSTRUMENTATION_NOT_AVAILABLE'));
}
