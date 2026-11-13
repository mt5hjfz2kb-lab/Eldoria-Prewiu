/*
 * Eldoria native325 private tester gateway. Inactive until user-owned Cloudflare
 * account enables R2, Secrets and ACTIVE_RELEASE. Does NOT modify the Unity game.
 * Each real tester accepts analytics in advance and receives an anonymous link.
 * Never expose SUPABASE_SECRET_KEY or LINK_SECRET to the browser.
 */
import {EVENT_SET,DEVICES,FIELDS} from './events-catalog.mjs';
import {OBSERVER_JS} from './observer-source.mjs';
const RELEASE='native325-mobile11-building-anchor';
const COOKIE='__Host-eldoria_beta';
const SAFE={ 'X-Content-Type-Options':'nosniff', 'Referrer-Policy':'no-referrer',
  'X-Frame-Options':'DENY','Cross-Origin-Resource-Policy':'same-origin',
  'Permissions-Policy':'camera=(), microphone=(), geolocation=()'};
const UUID=/^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
const CLIENT_FIELDS=new Set(['client_event_id','session_id','event_name','device_type','elapsed_ms','payload','consent_given']);
function answer(s,status,headers={}){return new Response(s,{status,headers:{...SAFE,'Cache-Control':'no-store',...headers}})}
const approved=(id,s)=>String(s||'').split(/[,\s]+/).filter(Boolean).includes(id);
function base64url(bytes){let x='';for(const b of bytes)x+=String.fromCharCode(b);return btoa(x).replace(/\+/g,'-').replace(/\//g,'_').replace(/=+$/,'')}
function constantMatch(a,b){
 if(a.length!==b.length)return false;
 let x=0;for(let i=0;i<a.length;i++)x|=a.charCodeAt(i)^b.charCodeAt(i);
 return x===0;
}
async function checkInvite(token,env) {
 if(typeof token!=='string'||token.length>240||!token.includes('.'))return null;
 if(typeof env.LINK_SECRET!=='string'||env.LINK_SECRET.length<32)return null;
 const segments=token.split('.');if(segments.length!==2)return null;
 const [data,sig]=segments;
 if(!/^[A-Za-z0-9_-]{20,170}$/.test(data)||!/^[A-Za-z0-9_-]{43}$/.test(sig))return null;
 let decoded;
 try{decoded=atob(data.replace(/-/g,'+').replace(/_/g,'/'));}catch{return null}
 const parts=decoded.split(':');
 if(parts.length!==3||parts[0]!=='v1'||!UUID.test(parts[1]))return null;
 const exp=Number(parts[2]), now=Math.floor(Date.now()/1000);
 if(!Number.isSafeInteger(exp)||exp<=now||exp>now+30*86400)return null;
 const key=await crypto.subtle.importKey('raw',new TextEncoder().encode(env.LINK_SECRET),
  {name:'HMAC',hash:'SHA-256'},false,['sign']);
 const expected=base64url(new Uint8Array(await crypto.subtle.sign('HMAC',key,new TextEncoder().encode(decoded))));
 if(!constantMatch(sig,expected))return null;
 if(!approved(parts[1],env.CONSENTED_TESTER_IDS)||approved(parts[1],env.REVOKED_TESTER_IDS))return null;
 return {tester_id:parts[1],expires:exp};
}
function tokenFromCookie(req){
 const raw=req.headers.get('Cookie')||'';
 for(const entry of raw.split(';')){
  const pos=entry.indexOf('=');
  if(pos>0&&entry.slice(0,pos).trim()===COOKIE)return entry.slice(pos+1).trim();
 }
 return null;
}
function pathFromURL(url){
 const p=new URL(url).pathname;
 if(p.includes('%')||p.includes('\\')||p.includes('//')||p.length>350)return null;
 const path=p==='/'?'index.html':p.slice(1);
 if(!/^[A-Za-z0-9_+./-]+$/.test(path)||path.split('/').some(v=>!v||v==='.'||v==='..')||path.startsWith('__'))return null;
 return path;
}
function validPayload(p) {
 if(!p||typeof p!=='object'||Array.isArray(p))return false;
 const raw=JSON.stringify(p);
 if(raw.length>1500)return false;
 return Object.entries(p).every(([k,v])=>FIELDS.has(k)&&
  ((typeof v==='string'&&/^[a-zA-Z0-9_]{1,40}$/.test(v))||
   (typeof v==='boolean')||(typeof v==='number'&&Number.isSafeInteger(v)&&Math.abs(v)<=1_000_000_000)));
}
async function ingest(req,env,invite){
 if(req.headers.get('Origin')!==new URL(req.url).origin)
  return answer('Origin incorrecto',403);
 if(!/^application\/json(?:\s*;|$)/i.test(req.headers.get('Content-Type')||''))
  return answer('Formato incorrecto',415);
 if(Number(req.headers.get('Content-Length')||0)>4000)return answer('Demasiado grande',413);
 let body;
 try{const raw=await req.text();if(raw.length>4000)throw Error('length');body=JSON.parse(raw)}catch{return answer('JSON incorrecto',422)}
 if(!body||Object.keys(body).some(k=>!CLIENT_FIELDS.has(k))||
    !UUID.test(body.client_event_id)||!UUID.test(body.session_id)||
    !EVENT_SET.has(body.event_name)||!DEVICES.has(body.device_type)||
    !Number.isSafeInteger(body.elapsed_ms)||body.elapsed_ms<0||body.elapsed_ms>86_400_000||
    body.consent_given!==true||!validPayload(body.payload))
    return answer('Evento incorrecto',422);
 // Only the real, authored Bastion I–V pilot can be counted. Reject a forged
 // level VI, false terminal state, or fabricated building progress.
 if((body.event_name==='bastion_reached'&&
     (!Number.isInteger(body.payload.bastion)||body.payload.bastion<1||body.payload.bastion>5))||
    (body.event_name==='end_of_slice'&&body.payload.bastion!==5)||
    (body.event_name==='build_completed'&&
     (!Number.isInteger(body.payload.level)||body.payload.level<1||body.payload.level>5)))
   return answer('Fuera del alcance Bastion I-V',422);
 if(typeof env.SUPABASE_SECRET_KEY!=='string'||env.SUPABASE_SECRET_KEY.length<20||
    !/^https:\/\/[a-z0-9-]+\.supabase\.co\/?$/i.test(env.SUPABASE_URL||''))return answer('Medicion no habilitada',503);
 const event={...body,tester_id:invite.tester_id,build_version:RELEASE,
  source_sha:'8dc875430241a49b4d417782dfeab2d4c7a4f315',
  round_id:'eldoria-closed-beta-bastion-i-v-1',is_qa:env.QA_MODE!=='false'};
 const headers={'Content-Type':'application/json','apikey':env.SUPABASE_SECRET_KEY,'Prefer':'return=minimal'};
 if(env.SUPABASE_SECRET_KEY.startsWith('eyJ'))headers.Authorization='Bearer '+env.SUPABASE_SECRET_KEY;
 try{
  const response=await fetch(env.SUPABASE_URL.replace(/\/$/,'')+'/rest/v1/eldoria_unity_beta_events',{
   method:'POST',headers,body:JSON.stringify(event),redirect:'error'});
  if(response.ok||response.status===409)return answer('',202);
  return answer('El servidor no confirmo el evento',503);
 }catch{return answer('La medicion no esta disponible',503)}
}
export default {
 async fetch(req,env){
  if(!['GET','HEAD','POST'].includes(req.method))return answer('Metodo no permitido',405);
  if(req.method==='POST'&&new URL(req.url).pathname!=='/beta/events')return answer('Metodo no permitido',405);
  if(!env?.ELDORIA_BUCKET||env.ACTIVE_RELEASE!==RELEASE||
      typeof env.LINK_SECRET!=='string'||env.LINK_SECRET.length<32)return answer('Beta cerrada',503);
  const path=new URL(req.url).pathname;
  if(path.startsWith('/entrar/')){
   if(req.method!=='GET')return answer('Metodo no permitido',405);
   const invite=await checkInvite(path.slice('/entrar/'.length),env);
   if(!invite)return answer('Invitacion no valida',403);
   const ttl=invite.expires-Math.floor(Date.now()/1000);
   return answer('',302,{Location:'/?eldoriaBetaTelemetry=1',
    'Set-Cookie':COOKIE+'='+path.slice('/entrar/'.length)+'; Path=/; HttpOnly; Secure; SameSite=Lax; Max-Age='+ttl});
  }
  const invite=await checkInvite(tokenFromCookie(req),env);
  if(!invite)return answer('Solo mediante invitacion',403);
  if(path==='/beta/events'){
   if(req.method!=='POST')return answer('Metodo no permitido',405);
   return ingest(req,env,invite);
  }
  if(path==='/beta/session')return answer(JSON.stringify({tester_id:invite.tester_id,release:RELEASE,consent:true}),200,{'Content-Type':'application/json'});
  // The invitation allowlist is checked above: observer is never public.
  if(path==='/beta/observer.js'){
   if(req.method!=='GET'&&req.method!=='HEAD')return answer('Metodo no permitido',405);
   return answer(req.method==='HEAD'?null:OBSERVER_JS,200,{'Content-Type':'application/javascript; charset=utf-8'});
  }
  if(req.method!=='GET'&&req.method!=='HEAD')return answer('Metodo no permitido',405);
  const rel=pathFromURL(req.url);if(!rel)return answer('Ruta incorrecta',400);
  const key='releases/'+RELEASE+'/'+rel;
  const opts=req.headers.has('Range')?{range:req.headers}:undefined;
  const object=req.method==='HEAD'?await env.ELDORIA_BUCKET.head(key):await env.ELDORIA_BUCKET.get(key,opts);
  if(!object)return answer('No encontrado',404);
  const headers=new Headers(SAFE);
  object.writeHttpMetadata(headers);
  headers.set('ETag',object.httpEtag);
  headers.set('Cache-Control',/^(index\.html|play\.js|build-manifest\.json)$/.test(rel)?
   'private, no-store':'private, max-age=86400, no-transform');
  headers.set('Accept-Ranges','bytes');
  // The original 19 R2 objects remain byte-identical. In tester mode only
  // the *HTTP response* for the 1.8 KB index contains a passive script tag.
  // No Unity WASM, assets, game rules, tutorials or saved data are altered.
  if(rel==='index.html'&&req.method==='GET'&&env.OBSERVER_ENABLED==='true'){
   const html=await new Response(object.body).text();
   if(!html.includes('</body>') || html.length>10000)
     return answer('Index de beta invalido',500);
   const modified=html.replace('</body>','<script defer src="/beta/observer.js"></script></body>');
   headers.delete('ETag');
   headers.delete('Content-Length');
   headers.delete('Content-Encoding');
   headers.set('Content-Type','text/html; charset=utf-8');
   headers.set('Cache-Control','private, no-store');
   return new Response(modified,{status:200,headers});
  }
  const partial=!!(object.range&&opts);
  if(partial)headers.set('Content-Range','bytes '+object.range.offset+'-'+(object.range.offset+object.range.length-1)+'/'+object.size);
  return new Response(req.method==='HEAD'?null:object.body,{
   status:partial?206:200,headers,encodeBody:headers.has('Content-Encoding')?'manual':'automatic'});
 }
};
