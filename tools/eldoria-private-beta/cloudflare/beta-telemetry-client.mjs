/*
 * Modulo SIN integrar todavia en Unity. La WebGL final invocara track() en sus hitos.
 * No registra nada sin que el jugador haya dado consentimiento explicito.
 * El Worker, NO el navegador, aporta tester_id, version, commit y la clave Supabase.
 */
import {EVENT_NAMES, EVENT_SET, ALLOWED_PAYLOAD_KEYS} from './events-catalog.mjs';
const MAX_QUEUE=200;
const QUEUE_KEY='eldoria_beta_event_queue_v1';
export const BETA_EVENTS=EVENT_NAMES;
const eventSet=EVENT_SET;
export function makeBetaTelemetry({send, storage, uuid, now, deviceType='other'}={}) {
  if (typeof send!=='function' || typeof uuid!=='function' || typeof now!=='function' || !storage)
    throw new Error('Faltan dependencias seguras de telemetria');
  const sessionId=uuid();
  const started=now();
  let consent=false; // deliberadamente NO se recupera automaticamente de un guardado.
  let busy=false;
  let memoryQueue=[];
  let durable=true;
  const read=()=>{
    try {
      const q=JSON.parse(storage.getItem(QUEUE_KEY)||'[]');
      const fromStorage=Array.isArray(q)?q.slice(-MAX_QUEUE):[];
      // Si localStorage fallo en una escritura previa pero vuelve, no perder memoria temporal.
      const known=new Set(fromStorage.map(x=>x.client_event_id));
      for(const item of memoryQueue){
        if(!known.has(item.client_event_id)){
          fromStorage.push(item);known.add(item.client_event_id);
        }
      }
      memoryQueue=fromStorage.slice(-MAX_QUEUE);
      return [...memoryQueue];
    } catch {durable=false;return [...memoryQueue]}
  };
  const write=q=>{
    memoryQueue=[...q.slice(-MAX_QUEUE)];
    try {storage.setItem(QUEUE_KEY,JSON.stringify(memoryQueue))}
    catch {durable=false}
  };
  const api={
    sessionId,
    isConsented:()=>consent,
    pending:()=>read().length,
    hasDurableQueue:()=>durable,
    setConsent(value){
      consent=value===true;
      if(!consent){memoryQueue=[];try{storage.removeItem(QUEUE_KEY)}catch{durable=false}}
    },
    async track(eventName,payload={}){
      if(!consent) return {accepted:false,reason:'no_consent'};
      if(!eventSet.has(eventName) || !payload || typeof payload!=='object' || Array.isArray(payload))
        return {accepted:false,reason:'invalid_event'};
      // Before local persistence, reject unknown fields/free text, so no PII ends up offline.
      if(Object.entries(payload).some(([k,v])=>!ALLOWED_PAYLOAD_KEYS.has(k) ||
          (typeof v==='string' && !/^[a-zA-Z0-9_]{1,40}$/.test(v)) ||
          (typeof v!=='string' && typeof v!=='boolean' && !Number.isSafeInteger(v))) ||
          JSON.stringify(payload).length>1300)
        return {accepted:false,reason:'invalid_payload'};
      const event={
        client_event_id:uuid(),session_id:sessionId,device_type:deviceType,
        event_name:eventName,elapsed_ms:Math.max(0,Math.floor(now()-started)),
        payload,consent_given:true
      };
      const queue=read();
      if(queue.length>=MAX_QUEUE) return {accepted:false,reason:'queue_full',pending:queue.length};
      queue.push(event);write(queue);
      const status=await api.flush();
      return {accepted:true,durable,...status};
    },
    async flush(){
      if(!consent || busy) return {sent:0,pending:read().length};
      busy=true;
      let sent=0,droppedInvalid=0;
      try {
        while(consent){
          const first=read()[0];
          if(!first)break;
          let response;
          try {response=await send(first);} catch {break}
          // El servidor solo reconoce 202 como confirmacion persistente.
          if(response?.status===422){
            // An irreparably invalid event must not starve all valid queued events.
            const current=read();
            const index=current.findIndex(x=>x.client_event_id===first.client_event_id);
            if(index>=0){current.splice(index,1);write(current);droppedInvalid++}
            continue;
          }
          if(response?.status!==202)break;
          // Otro evento pudo encolarse durante el await: quitar solo el confirmado.
          const current=read();
          const index=current.findIndex(x=>x.client_event_id===first.client_event_id);
          if(index>=0){current.splice(index,1);write(current)}
          sent++;
        }
        return {sent,droppedInvalid,pending:read().length};
      } finally {busy=false;}
    }
  };
  return api;
}
/* Adaptador de transporte para el navegador: sin token de Supabase ni ID de tester. */
export function browserSend(event){
  return fetch('/beta/events',{method:'POST',credentials:'same-origin',
    headers:{'Content-Type':'application/json'},body:JSON.stringify(event),cache:'no-store'});
}
