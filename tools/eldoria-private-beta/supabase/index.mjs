import {EVENT_SET,DEVICES,FIELDS} from './events-catalog.mjs';
const ORIGIN='https://mt5hjfz2kb-lab.github.io';
const PUBLIC_KEY="sb_publishable_cfpa9W5f2Iq251Sb4GE5VQ_5dIOQ_bI"; // Public client key, never a service-role secret.
const UUID=/^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
const KEYS=new Set(['client_event_id','tester_id','session_id','event_name','device_type','elapsed_ms','payload','consent_given']);
const rates=new Map();
function response(body,status){return new Response(body,{status,headers:{'Access-Control-Allow-Origin':ORIGIN,'Access-Control-Allow-Headers':'apikey,content-type','Access-Control-Allow-Methods':'POST,OPTIONS','Vary':'Origin','Cache-Control':'no-store'}})}
function payload(p){return p&&typeof p==='object'&&!Array.isArray(p)&&JSON.stringify(p).length<=1500&&Object.entries(p).every(([k,v])=>FIELDS.has(k)&&((typeof v==='string'&&/^[a-zA-Z0-9_]{1,40}$/.test(v))||typeof v==='boolean'||(Number.isSafeInteger(v)&&Math.abs(v)<=1e9)))}
export async function handler(req,env,send=fetch){
 if(req.headers.get('Origin')!==ORIGIN)return response('origin',403);
 if(req.method==='OPTIONS')return response(null,204);
 if(req.method!=='POST')return response('method',405);
 // Authentication is performed here because publishable API keys are not JWTs.
 // This identifies the public client, not a person or trusted gameplay session.
 if(req.headers.get('apikey')!==PUBLIC_KEY)return response('client_key',401);
 if(!/^application\/json(?:\s*;|$)/i.test(req.headers.get('Content-Type')||''))return response('content_type',415);
 let b;try{const raw=await req.text();if(raw.length>4000)return response('size',413);b=JSON.parse(raw)}catch{return response('json',422)}
 if(!b||Object.keys(b).some(k=>!KEYS.has(k))||!UUID.test(b.client_event_id)||!UUID.test(b.tester_id)||!UUID.test(b.session_id)||!EVENT_SET.has(b.event_name)||!DEVICES.has(b.device_type)||!Number.isSafeInteger(b.elapsed_ms)||b.elapsed_ms<0||b.elapsed_ms>86400000||b.consent_given!==true||!payload(b.payload))return response('event',422);
 const minute=Math.floor(Date.now()/60000);const prev=rates.get(b.tester_id);const count=prev?.minute===minute?prev.count+1:1;if(count>120)return response('rate',429);
 if(rates.size>10000)rates.clear();rates.set(b.tester_id,{minute,count});
 const url=env.SUPABASE_URL,key=env.SUPABASE_SERVICE_ROLE_KEY;
 if(!url||!key)return response('unavailable',503);
 const row={...b,source_sha:'948ffa9837168a0b68dab80be76b66350422b23a',build_version:'native325-mobile-candidate1',round_id:'eldoria-public-link-qa-1',is_qa:true};
 try{const res=await send(url+'/rest/v1/eldoria_unity_beta_events',{method:'POST',headers:{'Content-Type':'application/json',apikey:key,Authorization:'Bearer '+key,Prefer:'return=minimal'},body:JSON.stringify(row),redirect:'error'});return response('',res.ok||res.status===409?202:503)}catch{return response('unavailable',503)}
}
if(typeof Deno!=='undefined')Deno.serve(req=>handler(req,{SUPABASE_URL:Deno.env.get('SUPABASE_URL'),SUPABASE_SERVICE_ROLE_KEY:Deno.env.get('SUPABASE_SERVICE_ROLE_KEY')}));
