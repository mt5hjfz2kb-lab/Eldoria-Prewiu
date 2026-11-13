/*
 * Eldoria GitHub Pages → Supabase telemetry endpoint (QA by default).
 * The browser uses a PUBLIC publishable API key; only the Edge runtime
 * reads SUPABASE_SECRET_KEYS / SUPABASE_SERVICE_ROLE_KEY.
 * No Cloudflare, game save upload, username, email or private auth required.
 */
const ORIGIN='https://mt5hjfz2kb-lab.github.io';
const BUILD='native325-mobile11-building-anchor';
const SHA='8dc875430241a49b4d417782dfeab2d4c7a4f315';
const ROUND='eldoria-closed-beta-bastion-i-v-1';
const EVENTS=new Set([
 'session_start','game_ready','session_heartbeat','return_visit',
 'load_stage','visibility_state','orientation_state','client_error',
 'frame_summary','bastion_reached','end_of_slice','world_opened',
 'build_completed','march_sent','battle_resolved','lyra_state',
 'fissure_state','blocked','survey_completed','survey_answer'
]);
const DEVICES=new Set(['ios_safari','android_chrome','desktop_chrome','desktop_edge','desktop_firefox','other']);
const KEYS=new Set(['client_event_id','tester_id','session_id','event_name','device_type','elapsed_ms','payload','consent_given']);
const FIELDS=new Set(['bastion','level','building_code','outcome_code','state_code','march_slots_used',
 'stage_code','duration_ms','orientation','fps_p50','fps_p05','error_code','question_id','answer_code','reason_code']);
const UUID=/^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
const rate=new Map();
const respond=(body,status)=>new Response(status===204?null:body,{status,headers:{
 'Access-Control-Allow-Origin':ORIGIN,'Access-Control-Allow-Methods':'POST,OPTIONS',
 'Access-Control-Allow-Headers':'apikey,content-type','Vary':'Origin','Cache-Control':'no-store',
 'Content-Type':'text/plain; charset=utf-8','X-Content-Type-Options':'nosniff'
}});
function allowedPayload(o){
 if(!o||typeof o!=='object'||Array.isArray(o)||JSON.stringify(o).length>1200)return false;
 return Object.entries(o).every(([k,v])=>FIELDS.has(k)&&(
 (typeof v==='string'&&/^[A-Za-z0-9_]{1,36}$/.test(v))||
 (typeof v==='number'&&Number.isSafeInteger(v)&&v>=0&&v<=100000000)||
 (typeof v==='boolean')));
}
function validate(b){
 if(!b||typeof b!=='object'||Array.isArray(b)||Object.keys(b).some(k=>!KEYS.has(k)))return false;
 if(!UUID.test(b.client_event_id)||!UUID.test(b.tester_id)||!UUID.test(b.session_id))return false;
 if(b.consent_given!==true||!EVENTS.has(b.event_name)||!DEVICES.has(b.device_type))return false;
 if(!Number.isInteger(b.elapsed_ms)||b.elapsed_ms<0||b.elapsed_ms>86400000||!allowedPayload(b.payload))return false;
 if(b.event_name==='bastion_reached'&&(!Number.isInteger(b.payload.bastion)||b.payload.bastion<1||b.payload.bastion>5))return false;
 if(b.event_name==='end_of_slice'&&b.payload.bastion!==5)return false;
 if(b.event_name==='build_completed'&&(!['sawmill','barracks','granary','stoneworks'].includes(b.payload.building_code)||!Number.isInteger(b.payload.level)||b.payload.level<1||b.payload.level>5))return false;
 return true;
}
function keys(env){
 try{
  let pub=env.SUPABASE_PUBLISHABLE_KEYS&&JSON.parse(env.SUPABASE_PUBLISHABLE_KEYS);
  let priv=env.SUPABASE_SECRET_KEYS&&JSON.parse(env.SUPABASE_SECRET_KEYS);
  return {public:pub?.default||env.SUPABASE_PUBLISHABLE_KEY,
   secret:priv?.default||env.SUPABASE_SERVICE_ROLE_KEY};
 }catch{return {}}
}
export async function handler(req,env={},send=fetch,{qa=true}={}){
 if(req.headers.get('Origin')!==ORIGIN)return respond('origin',403);
 if(req.method==='OPTIONS')return respond('',204);
 if(req.method!=='POST')return respond('method',405);
 let k=keys(env);
 if(!k.public||req.headers.get('apikey')!==k.public)return respond('client_key',401);
 if(!/^application\/json(?:\s*;|$)/i.test(req.headers.get('Content-Type')||''))return respond('content_type',415);
 if(Number(req.headers.get('Content-Length')||0)>4000)return respond('size',413);
 let b;
 try{const str=await req.text();if(str.length>4000)return respond('size',413);b=JSON.parse(str)}
 catch{return respond('json',422)}
 if(!validate(b))return respond('event',422);
 const id=b.tester_id,minute=Math.floor(Date.now()/60000),record=rate.get(id);
 const count=(record?.minute===minute?record.count:0)+1;
 if(count>75)return respond('rate',429);
 if(rate.size>2000)rate.clear();rate.set(id,{minute,count});
 if(!env.SUPABASE_URL||!k.secret)return respond('unavailable',503);
 const row={...b,round_id:ROUND,build_version:BUILD,source_sha:SHA,is_qa:qa===true};
 try{
  const result=await send(env.SUPABASE_URL.replace(/\/$/,'')+'/rest/v1/eldoria_unity_beta_events?on_conflict=client_event_id',{
   method:'POST',headers:{'Content-Type':'application/json','apikey':k.secret,
    'Authorization':'Bearer '+k.secret,
    'Prefer':'resolution=ignore-duplicates,return=minimal'},
   body:JSON.stringify(row),redirect:'error'
  });
  return respond('',result.ok?202:503);
 }catch{return respond('unavailable',503)}
}
if(typeof Deno!=='undefined'){
 Deno.serve(req=>handler(req,{
  SUPABASE_URL:Deno.env.get('SUPABASE_URL'),
  SUPABASE_PUBLISHABLE_KEYS:Deno.env.get('SUPABASE_PUBLISHABLE_KEYS'),
  SUPABASE_SECRET_KEYS:Deno.env.get('SUPABASE_SECRET_KEYS'),
  SUPABASE_PUBLISHABLE_KEY:Deno.env.get('SUPABASE_PUBLISHABLE_KEY'),
  SUPABASE_SERVICE_ROLE_KEY:Deno.env.get('SUPABASE_SERVICE_ROLE_KEY')
 }));
}
