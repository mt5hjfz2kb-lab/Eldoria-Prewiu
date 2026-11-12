/*
 * Eldoria Github Pages opt-in telemetry client. Browser IDs are pseudonyms,
 * never player identity, and cannot prove a unique human participant.
 * The Edge server, not the browser, stamps version, source and QA/live status.
 */
export const SOURCE='8dc875430241a49b4d417782dfeab2d4c7a4f315';
export const RELEASE='native325-mobile11-building-anchor';
export const API='https://eavaxwpxxcrwibooiimb.supabase.co/functions/v1/eldoria-pages-events-qa';
export const PUBLISHABLE='sb_publishable_cfpa9W5f2Iq251Sb4GE5VQ_5dIOQ_bI';
const EVENTS=new Set(['session_start','game_ready','session_heartbeat','return_visit',
 'load_stage','visibility_state','orientation_state','client_error','frame_summary',
 'bastion_reached','end_of_slice','world_opened','build_completed','march_sent',
 'battle_resolved','lyra_state','fissure_state','blocked','survey_completed','survey_answer']);
const FIELDS=new Set(['bastion','level','building_code','outcome_code','state_code',
 'march_slots_used','stage_code','duration_ms','orientation','fps_p50','fps_p05',
 'error_code','question_id','answer_code','reason_code']);
const PREFIX='eldoria_pages_qa_v1:';
export function createTelemetry({send,storage,uuid,clock,device='other',release=RELEASE}){
 if(typeof send!=='function'||!storage||typeof uuid!=='function'||typeof clock!=='function')
   throw new Error('Missing telemetry dependencies');
 const current=clock(),k=PREFIX+release;
 let accepted=false,busy=false,durable=true,anonymousId=null;
 let pending=[];
 const get=(key)=>{try{return storage.getItem(key)}catch{durable=false;return null}};
 const put=(key,value)=>{try{storage.setItem(key,value)}catch{durable=false}};
 const remove=(key)=>{try{storage.removeItem(key)}catch{durable=false}};
 const safePayload=(data)=>data&&typeof data==='object'&&!Array.isArray(data)&&
  JSON.stringify(data).length<1100&&Object.entries(data).every(([key,value])=>
   FIELDS.has(key)&&((typeof value==='string'&&/^[A-Za-z0-9_]{1,36}$/.test(value))||
   (typeof value==='number'&&Number.isSafeInteger(value)&&value>=0&&value<=100000000)||
   typeof value==='boolean'));
 const ensureId=()=>{if(!anonymousId)anonymousId=get(k+':tester')||uuid();put(k+':tester',anonymousId);return anonymousId};
 const session=uuid();
 const read=()=>{
  try{const raw=JSON.parse(get(k+':queue')||'[]');if(Array.isArray(raw)){
    const known=new Set(raw.map(x=>x.client_event_id));
    for(const e of pending)if(!known.has(e.client_event_id)){raw.push(e);known.add(e.client_event_id)}
    pending=raw.slice(-150);return [...pending];}
  }catch{durable=false}
  return [...pending];
 };
 const write=(items)=>{pending=items.slice(-150);put(k+':queue',JSON.stringify(pending))};
 return {
  isConsented:()=>accepted,
  pending:()=>read().length,
  isDurable:()=>durable,
  testerId:()=>accepted?ensureId():null,
  sessionId:()=>session,
  hasExistingConsent:()=>get(k+':consent')==='yes',
  setConsent(value){
   accepted=value===true;
   if(accepted){ensureId();put(k+':consent','yes')}
   else{pending=[];remove(k+':consent');remove(k+':queue');remove(k+':tester');anonymousId=null}
  },
  async track(eventName,payload={}){
   if(!accepted)return {accepted:false,reason:'no_consent'};
   if(!EVENTS.has(eventName)||!safePayload(payload))return {accepted:false,reason:'invalid_payload'};
   if(eventName==='bastion_reached'&&(!Number.isInteger(payload.bastion)||payload.bastion<1||payload.bastion>5))
    return {accepted:false,reason:'invalid_bastion'};
   if(eventName==='end_of_slice'&&payload.bastion!==5)return {accepted:false,reason:'not_bastion_v'};
   const q=read();if(q.length>=150)return {accepted:false,reason:'queue_full'};
   const event={
    client_event_id:uuid(),tester_id:ensureId(),session_id:session,
    event_name:eventName,device_type:device,
    elapsed_ms:Math.min(86400000,Math.max(0,Math.floor(clock()-current))),
    payload,consent_given:true
   };
   q.push(event);write(q);
   const status=await this.flush();return {accepted:true,durable,...status};
  },
  async flush(){
   if(!accepted||busy)return {sent:0,pending:read().length};
   busy=true;let sent=0,discarded=0;
   try{
    while(accepted){
     const first=read()[0];if(!first)break;
     let response;try{response=await send(first)}catch{break}
     if(response?.status!==202 && response?.status!==422)break;
     const q=read();const i=q.findIndex(x=>x.client_event_id===first.client_event_id);
     if(i>=0){q.splice(i,1);write(q)}
     if(response.status===422)discarded++;else sent++;
    }
   }finally{busy=false}
   return {sent,discarded,pending:read().length};
  }
 };
}
export async function sendToQA(event,fetchImpl=fetch){
 // A pagehide/reload must not cancel the one bounded event in flight.
 // Keep normal CORS and the public API key; no beacon/auth shortcut.
 const body=JSON.stringify(event);
 if(new TextEncoder().encode(body).byteLength>4000)throw Error('QA event body exceeds transport bound');
 return fetchImpl(API,{method:'POST',mode:'cors',cache:'no-store',keepalive:true,
  headers:{'Content-Type':'application/json',apikey:PUBLISHABLE},
  body,credentials:'omit',redirect:'error'});
}
