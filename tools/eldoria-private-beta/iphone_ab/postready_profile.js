/* Owner-only, opt-in diagnostic for beta13 WebGL post-ready restarts.
 * No gameplay mutations, identity collection, telemetry uploads or navigation.
 * Install before createUnityInstance; no pagehide is NOT proof of jetsam.
 */
(function(root){
 'use strict';
 const KEY='eldoria_p0_postready_profile_v1',MAX_EVENTS=48,MAX_TEXT=170;
 function install(win=root,doc=win.document){
  const params=new URLSearchParams((win.location&&win.location.search)||'');
  if(params.get('eldoria_p0')!=='1')return {active:false,dispose(){}};
  let previous=null,enabled=true,disposed=false;
  try{previous=JSON.parse(win.localStorage.getItem(KEY)||'null');}catch(_){enabled=false;}
  const same=previous&&previous.v===1&&previous.build==='beta13'&&Array.isArray(previous.events);
  const state={v:1,build:'beta13',previousUnclosed:!!(same&&previous.closed===false),
   closed:false,events:(same?previous.events:[]).slice(-(MAX_EVENTS-1))};
  const initial=win.performance&&win.performance.getEntriesByType?.('navigation')?.[0];
  const now=()=>Date.now();
  const scrub=message=>String(message).replace(/[\r\n]+/g,' ').slice(0,MAX_TEXT);
  function mark(kind,detail=''){
   if(disposed)return;
   state.events.push([now(),kind,scrub(detail)]);
   if(state.events.length>MAX_EVENTS)state.events.splice(0,state.events.length-MAX_EVENTS);
   try{win.localStorage.setItem(KEY,JSON.stringify(state));}catch(_){enabled=false;}
  }
  mark('boot','nav='+(initial?.type||'unknown')+' previousUnclosed='+state.previousUnclosed);
  const listeners=[];
  function on(obj,type,handler,options){
   if(!obj?.addEventListener)return;
   obj.addEventListener(type,handler,options);listeners.push([obj,type,handler,options]);
  }
  function dimensions(){
   const el=doc?.getElementById?.('unity-canvas');
   return 'viewport='+(win.innerWidth||0)+'x'+(win.innerHeight||0)+' canvas='+(el?.width||0)+'x'+(el?.height||0);
  }
  let lastDimensions=dimensions();mark('dimensions',lastDimensions);
  on(win,'resize',()=>{const next=dimensions();if(next!==lastDimensions){lastDimensions=next;mark('resize',next);}}, {passive:true});
  on(win,'pagehide',event=>{mark('pagehide','persisted='+!!event?.persisted);state.closed=true;try{win.localStorage.setItem(KEY,JSON.stringify(state));}catch(_){enabled=false;}});
  on(win,'pageshow',event=>mark('pageshow','persisted='+!!event?.persisted));
  on(doc,'visibilitychange',()=>mark('visibility',doc.visibilityState||'unknown'));
  on(doc,'webglcontextlost',()=>mark('webglcontextlost'),true);
  on(doc,'webglcontextrestored',()=>mark('webglcontextrestored'),true);
  on(win,'error',()=>mark('js-error'),true);
  on(win,'unhandledrejection',()=>mark('promise-error'),true);
  const old={};
  const allowed=/^(?:ELDORIA_MEMORY_PHASE\b|REGION1_CANON_WORLD_INSTALLED\b|REGION1_BLENDER_IMPORTED\b|ELDORIA_TERRAIN_QUERY_TAPE\b|REGION1_IMPORTED_PIGMENT_CONTACT\b|ELDORIA_CONTEXT_LOST\b)/;
  for(const level of ['log','warn','error']){
   const original=win.console?.[level];if(typeof original!=='function')continue;
   old[level]=original;
   win.console[level]=function(...args){
    try{const message=typeof args[0]==='string'?args[0]:'';if(allowed.test(message))mark('unity-'+level,message);}catch(_){}
    return original.apply(this,args);
   };
  }
  const api={active:true,report:()=>JSON.stringify({...state,persistenceAvailable:enabled},null,2),
   mark,dispose(){if(disposed)return;
    for(const [target,type,fn,opt] of listeners)target.removeEventListener?.(type,fn,opt);
    for(const level of Object.keys(old))win.console[level]=old[level];disposed=true;}};
  // Show persisted phase breadcrumbs after an involuntary reload without
  // interrupting the game or disclosing them to any server.
  if(doc?.body&&typeof doc.createElement==='function'){
   const button=doc.createElement('button');button.type='button';button.textContent='Perfil P0';
   Object.assign(button.style,{position:'fixed',right:'8px',top:'8px',zIndex:'999997',
    font:'12px monospace',padding:'5px',background:'#14212c',color:'#fff',
    border:'1px solid #b9a476',borderRadius:'4px'});
   const panel=doc.createElement('div');
   Object.assign(panel.style,{display:'none',position:'fixed',right:'8px',top:'42px',
    width:'min(92vw,440px)',maxHeight:'55vh',zIndex:'999997',background:'#14212c',
    color:'#fff',border:'1px solid #b9a476',padding:'8px'});
   const note=doc.createElement('p');
   note.textContent='Registro local y privado. No prueba jetsam. No se envía automáticamente.';
   const output=doc.createElement('textarea');output.readOnly=true;
   Object.assign(output.style,{width:'100%',height:'190px',font:'11px monospace'});
   button.addEventListener('click',()=>{panel.style.display=panel.style.display==='none'?'block':'none';output.value=api.report();});
   panel.append(note,output);doc.body.append(button,panel);
  }
  win.__eldoriaP0Postready=api;
  return api;
 }
 root.installEldoriaPostreadyProfile=install;
 if(typeof module==='object')module.exports={install,KEY};
})(typeof window==='object'?window:globalThis);
