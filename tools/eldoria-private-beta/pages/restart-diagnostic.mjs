// Local, opt-in iPhone/Safari restart breadcrumbs for P0 #96.
// No analytics calls, user identity, saved games, browser fingerprint or error messages.
// An unclosed prior record is NOT proof of jetsam or an engine crash.
export const RESTART_KEY='eldoria_owner_restart_p0_v1';
const MAX_EVENTS=24;
export function startLocalRestartDiagnostic({
 win=window,doc=document,storage=win.localStorage,
 now=()=>Date.now(),clock=()=>win.performance.now(),build='unknown'
}={}){
 const enabled=new URLSearchParams(win.location?.search||'').get('eldoria_diag')==='1';
 if(!enabled)return {active:false,reason:'not_opted_in'};
 let prior=null,persistent=true;
 try{prior=JSON.parse(storage.getItem(RESTART_KEY)||'null')}catch(_){persistent=false}
 const valid=prior&&prior.v===1&&Array.isArray(prior.events)&&prior.build===build;
 const previous=valid?prior:null;
 const observation=!previous?'NO_PRIOR_RECORD':
  previous.closed?'PREVIOUS_EXIT_RECORDED':
  'PRIOR_DOCUMENT_WITHOUT_EXIT_RECORD';
 const events=(valid?prior.events:[]).slice(-(MAX_EVENTS-1))
  .filter(e=>Array.isArray(e)&&e.length===3);
 const state={v:1,build,closed:false,observation,boot_at:now(),events};
 const started=clock();
 const accepted=new Set(['boot','heartbeat','background','foreground',
  'pagehide','bfcache_pause','bfcache_resume','webgl_lost','webgl_restored',
  'js_error','promise_error']);
 function record(kind){
  if(!accepted.has(kind))return;
  const elapsed=Math.max(0,Math.round((clock()-started)/1000));
  state.events.push([kind,elapsed,now()]);
  state.events=state.events.slice(-MAX_EVENTS);
  if(kind==='pagehide')state.closed=true;
  try{storage.setItem(RESTART_KEY,JSON.stringify(state))}catch(_){persistent=false}
 }
 record('boot');
 const on=(target,event,handler,options)=>target?.addEventListener?.(event,handler,options);
 on(doc,'visibilitychange',()=>record(doc.visibilityState==='hidden'?'background':'foreground'));
 on(win,'pagehide',e=>record(e?.persisted?'bfcache_pause':'pagehide'));
 on(win,'pageshow',e=>{if(e?.persisted)record('bfcache_resume')});
 on(doc,'webglcontextlost',()=>record('webgl_lost'),true);
 on(doc,'webglcontextrestored',()=>record('webgl_restored'),true);
 on(win,'error',()=>record('js_error'));
 on(win,'unhandledrejection',()=>record('promise_error'));
 const interval=win.setInterval?.(()=>{if(doc.visibilityState!=='hidden'&&!state.closed)record('heartbeat')},10000);
 interval?.unref?.();
 function report(){
  return JSON.stringify({
   diagnostic:'ELDORIA_P0_LOCAL_OWNER_ONLY',build,
   result:state.observation,
   explanation:'Sin señal de salida NO confirma jetsam ni fallo de memoria: puede ser navegación, cierre del sistema o varias pestañas.',
   events:state.events,persisted:persistent
  },null,2);
 }
 // Do not cover Unity game controls: the owner's diagnostic tab lives top-left.
 if(doc?.body&&doc.createElement){
  const btn=doc.createElement('button');btn.type='button';btn.textContent='Diagnóstico local';
  Object.assign(btn.style,{position:'fixed',top:'6px',left:'6px',zIndex:'999998',
   font:'12px system-ui',padding:'5px 7px',background:'#172533',color:'white',
   border:'1px solid #8ba',borderRadius:'5px',opacity:'0.85'});
  const box=doc.createElement('div');
  Object.assign(box.style,{display:'none',position:'fixed',top:'40px',left:'6px',
   right:'6px',maxWidth:'420px',maxHeight:'55vh',overflow:'auto',
   zIndex:'999998',background:'#15202b',color:'white',padding:'10px',
   border:'1px solid #8ba',font:'12px system-ui'});
  const label=doc.createElement('p');
  label.textContent='Solo en este dispositivo. No se envía nada. Copia el diagnóstico después del reinicio. Las causas son aún desconocidas.';
  const area=doc.createElement('textarea');
  Object.assign(area.style,{width:'96%',height:'170px',font:'11px monospace'});
  area.readOnly=true;
  const copy=doc.createElement('button');copy.type='button';copy.textContent='Copiar registro';
  copy.addEventListener('click',async()=>{
   area.value=report();
   try{
    if(!win.navigator.clipboard?.writeText)throw Error('Clipboard unsupported');
    await win.navigator.clipboard.writeText(area.value);copy.textContent='Copiado';
   }catch(_){copy.textContent='Selecciona y copia el texto';area.focus?.();area.select?.();}
  });
  box.append(label,area,copy);
  btn.addEventListener('click',()=>{
   box.style.display=box.style.display==='none'?'block':'none';
   area.value=report();
  });
  doc.body.append(btn,box);
 }
 return {active:true,record,report,getState:()=>state};
}
