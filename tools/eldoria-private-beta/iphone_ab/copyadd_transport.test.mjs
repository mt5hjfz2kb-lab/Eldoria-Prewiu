import test from 'node:test';
import assert from 'node:assert/strict';
import vm from 'node:vm';
import fs from 'node:fs';
const code=fs.readFileSync(new URL('./copyadd_transport.js',import.meta.url),'utf8');
function fixture(){
 const size=503831429,stable=503830840,part=16777216;
 const parts=Array.from({length:31},(_,i)=>({index:i,decoded_bytes:Math.min(part,size-i*part),literal_bytes:i===30?589:0,literal_url:'copy-literals/'+i+'.bin',ops:i===30?[['C',i*part,stable-i*part],['A',0,589]]:[['C',i*part,part]]}));
 return {format:'ELDORIA_COPY_ADD_V1',stable_source_sha:'be49d62e3daf490f41dba0bf8123a8e2eee64480',candidate_source_sha:'004d3ef9c26e9070762e9e38ce84d69d525efd4f',stable_part_size:part,stable_size:stable,candidate_size:size,target_parts:parts,stable_parts:parts.map((_,i)=>({url:'../beta-native325/payload-decoded/'+i+'.bin',decoded_bytes:Math.min(part,stable-i*part)}))};
}
function setup({truncated=false,metadata=fixture()}={}){
 const state={requests:0,cancels:0};
 const win={location:{href:'https://game.test/beta-iphone-clock-ab/',origin:'https://game.test'},DecompressionStream,
 fetch:async function(url,init){
  state.requests++;if(url.endsWith('other'))return new Response('normal');
  const match=url.match(/\/(\d+)\.bin$/);let remaining=truncated?0:(url.includes('copy-literals')?589:metadata.stable_parts[+match[1]].decoded_bytes);
  return new Response(new ReadableStream({pull(c){if(!remaining){c.close();return;}const n=Math.min(65536,remaining);remaining-=n;c.enqueue(new Uint8Array(n).fill(url.includes('copy-literals')?2:1));},cancel(){state.cancels++;}}));
 }};
 vm.runInNewContext(code,{window:win,ReadableStream,Response,URL,Uint8Array,AbortController});
 const install=()=>win.installEldoriaCopyTransport('data.bin',metadata);
 return {win,install,state};
}
test('full forward-only stream stays within 64KiB and emits exact source/literal lengths',async()=>{
 const {win,install}=setup();assert.equal(install().stable_cache_parts,0);
 const response=await win.fetch('data.bin'),reader=response.body.getReader();let total=0,ones=0,twos=0;
 while(true){const {value,done}=await reader.read();if(done)break;assert.ok(value.length<=65536);total+=value.length;if(value[0]===1){ones+=value.length;assert.equal(value.at(-1),1);}else{twos+=value.length;assert.equal(value.at(-1),2);}}
 assert.equal(total,503831429);assert.equal(ones,503830840);assert.equal(twos,589);
 assert.equal(await (await win.fetch('other')).text(),'normal');
});
test('rejects backward COPY and malformed extents before network access',()=>{
 const metadata=fixture();metadata.target_parts[1].ops[0][1]=0;
 const {install,state}=setup({metadata});assert.throws(install,/Non-monotonic/);assert.equal(state.requests,0);
});
test('truncated input fails rather than returning a successful partial Unity payload',async()=>{
 const {win,install}=setup({truncated:true});install();const reader=(await win.fetch('data.bin')).body.getReader();await assert.rejects(reader.read(),/length mismatch/);
});
test('cancellation releases stream and permits a clean subsequent load',async()=>{
 const {win,install,state}=setup();install();const response=await win.fetch('data.bin');const reader=response.body.getReader();await reader.read();await reader.cancel();assert.ok(state.cancels>0);
 const next=await win.fetch('data.bin');await next.body.cancel();
});
test('runtime never materializes or digests full source buffers',()=>{
 assert.doesNotMatch(code,/\.arrayBuffer\(|crypto\.subtle|new Uint8Array\(/);
});
