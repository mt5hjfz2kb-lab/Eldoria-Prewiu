import test from 'node:test';
import assert from 'node:assert/strict';
import {randomUUID} from 'node:crypto';
import {handler} from './edge-function.mjs';
const ORIGIN='https://mt5hjfz2kb-lab.github.io';
const pub='sb_publishable_local_qa_no_secret';
const secret='sb_secret_fake_local_only_never_a_real_key';
const env={
 SUPABASE_URL:'https://example.supabase.co',
 SUPABASE_PUBLISHABLE_KEYS:JSON.stringify({default:pub}),
 SUPABASE_SECRET_KEYS:JSON.stringify({default:secret})
};
const row=()=>({
 client_event_id:randomUUID(),tester_id:randomUUID(),session_id:randomUUID(),
 event_name:'bastion_reached',device_type:'ios_safari',
 elapsed_ms:21000,payload:{bastion:3},consent_given:true
});
const req=(body,method='POST',origin=ORIGIN,key=pub)=>new Request('https://example.supabase.co/functions/v1/eldoria-pages-events-qa',{
 method,headers:{Origin:origin,'Content-Type':'application/json',apikey:key},
 ...(method!=='OPTIONS'?{body:JSON.stringify(body)}:{})
});
test('CORS refuses other origins; public key checked independently',async()=>{
 assert.equal((await handler(req(row(),'POST','https://evil.invalid'),env)).status,403);
 assert.equal((await handler(req(row(),'POST',ORIGIN,'incorrect'),env)).status,401);
 assert.equal((await handler(req(row(),'OPTIONS'),env)).status,204);
});
test('rejects no consent, free text, PII, fabricated Bastion VI and false end',async()=>{
 for(const bad of [
 {...row(),consent_given:false},
 {...row(),payload:{bastion:6}},
 {...row(),event_name:'end_of_slice',payload:{bastion:4}},
 {...row(),payload:{email:'person@example.org'}},
 {...row(),payload:{save:'entire PlayerState'}},
 {...row(),event_name:'build_completed',payload:{building_code:'fake',level:2}},
 {...row(),event_name:'card_acquired'},
 ])assert.equal((await handler(req(bad),env)).status,422);
});
test('only the server adds release SHA, round and QA label; no privileged key in the client',async()=>{
 let call;
 const fake=async(url,opts)=>{call={url,opts,body:JSON.parse(opts.body)};return new Response('',{status:201})};
 const b=row();
 let result=await handler(req(b),env,fake);
 assert.equal(result.status,202);
 assert.equal(call.body.client_event_id,b.client_event_id);
 assert.equal(call.body.build_version,'native325-mobile11-building-anchor');
 assert.equal(call.body.source_sha,'8dc875430241a49b4d417782dfeab2d4c7a4f315');
 assert.equal(call.body.round_id,'eldoria-closed-beta-bastion-i-v-1');
 assert.equal(call.body.is_qa,true);
 assert.ok(call.opts.headers.Authorization.includes(secret));
 assert.ok(!JSON.stringify(b).includes(secret));
 assert.match(call.opts.headers.Prefer,/ignore-duplicates/);
});
test('live eligibility cannot be chosen by the browser and is QA until separate certified deployment',async()=>{
 const evil={...row(),is_qa:false,source_sha:'f'.repeat(40)};
 assert.equal((await handler(req(evil),env)).status,422);
 const missing={...env,SUPABASE_SECRET_KEYS:'{}',SUPABASE_SERVICE_ROLE_KEY:undefined};
 assert.equal((await handler(req(row()),missing)).status,503);
});
test('transport errors fail closed without acknowledging an unpersisted event',async()=>{
 assert.equal((await handler(req(row()),env,async()=>new Response('',{status:500}))).status,503);
 assert.equal((await handler(req(row()),env,async()=>{throw Error('offline')})).status,503);
});
