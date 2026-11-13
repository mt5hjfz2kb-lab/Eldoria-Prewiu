'use strict';
// Forward-only COPY/ADD transport. Unlike the rejected cache/digest approach,
// this retains just the current network chunk, never a 16MiB base/target buffer.
// Exact target SHA256 is checked independently for all 31 parts in HTTP CI.
// Runtime enforces same-origin pinned paths, monotonic operations and byte sizes;
// it does not claim to hash streamed bytes in the browser. No game saves touched.
(function(root){
 const MAX_OUTPUT=65536;
 function safeURL(href,route){
  if(typeof href!=='string'||href.includes('\\')||href.includes('?')||href.includes('#'))throw Error('Invalid asset path');
  const u=new URL(href,root.location.href);
  if(u.origin!==root.location.origin||!u.pathname.includes(route))throw Error('Unapproved asset origin/path');
  return u.href;
 }
 function install(dataUrl,info){
  if(!info||info.format!=='ELDORIA_COPY_ADD_V1'||
     info.stable_source_sha!=='be49d62e3daf490f41dba0bf8123a8e2eee64480'||
     info.candidate_source_sha!=='004d3ef9c26e9070762e9e38ce84d69d525efd4f'||
     info.stable_part_size!==16777216||info.stable_size!==503830840||info.candidate_size!==503831429||
     !Array.isArray(info.stable_parts)||info.stable_parts.length!==31||
     !Array.isArray(info.target_parts)||info.target_parts.length!==31||typeof root.DecompressionStream!=='function')
   throw Error('Unapproved candidate metadata');
  // Reject any backward seek before fetching. Only the pinned forward program
  // can be streamed without caching prior Unity data.
  let end=0,total=0;
  for(let i=0;i<31;i++){
   const p=info.target_parts[i];let size=0,literalEnd=0;
   if(p.index!==i||!Number.isSafeInteger(p.decoded_bytes)||p.decoded_bytes<1||p.decoded_bytes>16777216||
      !Number.isSafeInteger(p.literal_bytes)||p.literal_bytes<0||p.literal_bytes>6000000||!Array.isArray(p.ops)||!p.ops.length)
    throw Error('Invalid target part');
   for(const op of p.ops){
    if(!Array.isArray(op)||op.length!==3)throw Error('Invalid operation');
    const [kind,pos,n]=op;
    if(!Number.isSafeInteger(pos)||pos<0||!Number.isSafeInteger(n)||n<=0)throw Error('Invalid operation bounds');
    if(kind==='C'){
     if(pos<end||pos+n>info.stable_size)throw Error('Non-monotonic source operation');end=pos+n;
    }else if(kind==='A'){
     if(pos!==literalEnd||pos+n>p.literal_bytes)throw Error('Non-monotonic literal operation');literalEnd=pos+n;
    }else throw Error('Unapproved operation');
    size+=n;
   }
   if(size!==p.decoded_bytes||literalEnd!==p.literal_bytes)throw Error('Invalid part extent');
   total+=size;
  }
  if(total!==info.candidate_size)throw Error('Invalid candidate extent');
  const request=new URL(dataUrl,root.location.href).href;
  const originalFetch=root.fetch.bind(root);let active=false;
  root.fetch=async function(input,init){
   const url=new URL(typeof input==='string'?input:input instanceof URL?input.href:input.url,root.location.href).href;
   if(url!==request)return originalFetch(input,init);
   if(active)throw Error('Concurrent Unity data stream refused');
   active=true;let cancelled=false;
   const abort=new AbortController();
   function source(parts,route){
    let index=0,reader=null,chunk=null,offset=0,received=0,cursor=0;
    async function close(){chunk=null;if(reader){const r=reader;reader=null;await r.cancel().catch(()=>{});}}
    async function next(){
     while(!cancelled){
      if(!reader){
       if(index===parts.length)return false;
       const p=parts[index];
       const response=await originalFetch(safeURL(p.url,route),{cache:'no-store',credentials:'omit',signal:abort.signal});
       if(!response.ok)throw Error('Source HTTP failure');
       received=0;
       if(p.decoded_bytes===0){index++;continue;}
       if(!response.body)throw Error('Missing source body');
       const compressed=p.url.endsWith('.gz')&&response.headers.get('Content-Encoding')?.toLowerCase()!=='gzip';
       reader=(compressed?response.body.pipeThrough(new root.DecompressionStream('gzip')):response.body).getReader();
      }
      const r=await reader.read();
      if(cancelled)return false;
      if(r.done){
       if(received!==parts[index].decoded_bytes)throw Error('Source length mismatch');
       reader.releaseLock();reader=null;index++;continue;
      }
      received+=r.value.byteLength;
      if(received>parts[index].decoded_bytes)throw Error('Source overflow');
      if(!r.value.byteLength)continue;
      chunk=r.value;offset=0;return true;
     }
     return false;
    }
    async function take(at,limit){
     if(at<cursor)throw Error('Backward source read');
     while(true){
      if(!chunk||offset===chunk.byteLength){chunk=null;if(!await next())throw Error('Truncated source');}
      const skip=Math.min(at-cursor,chunk.byteLength-offset);cursor+=skip;offset+=skip;
      if(cursor<at||offset===chunk.byteLength)continue;
      const n=Math.min(limit,MAX_OUTPUT,chunk.byteLength-offset);
      const result=chunk.subarray(offset,offset+n);cursor+=n;offset+=n;
      return result;
     }
    }
    async function finish(){
     if(chunk&&offset!==chunk.byteLength)throw Error('Unconsumed literal bytes');
     chunk=null;
     if(await next())throw Error('Extra literal bytes');
    }
    return {take,close,finish};
   }
   const base=source(info.stable_parts,'/beta-native325/payload-decoded/');
   let literal=null,partIndex=0,opIndex=0,opOffset=0,emitted=0;
   async function cleanup(){cancelled=true;abort.abort();await Promise.all([base.close(),literal?.close()]);literal=null;active=false;}
   const stream=new ReadableStream({
    async pull(controller){
     try{
      if(partIndex===31){
       if(emitted!==info.candidate_size)throw Error('Final size mismatch');
       await cleanup();controller.close();return;
      }
      const p=info.target_parts[partIndex];
      const [kind,pos,n]=p.ops[opIndex];
      if(kind==='A'&&!literal)literal=source([{url:p.literal_url,decoded_bytes:p.literal_bytes}],'/beta-iphone-clock-ab/copy-literals/');
      const bytes=await (kind==='C'?base:literal).take(pos+opOffset,n-opOffset);
      if(cancelled)return;
      opOffset+=bytes.byteLength;emitted+=bytes.byteLength;
      if(opOffset===n){opIndex++;opOffset=0;}
      if(opIndex===p.ops.length){
       if(literal){await literal.finish();await literal.close();literal=null;}
       partIndex++;opIndex=0;
      }
      controller.enqueue(bytes);
     }catch(e){await cleanup();controller.error(e);}
    },
    cancel(){return cleanup();}
   },{highWaterMark:1});
   const response=new Response(stream,{headers:{'Content-Type':'application/octet-stream','Content-Length':String(info.candidate_size),'Cache-Control':'no-store'}});
   Object.defineProperty(response,'url',{value:request});return response;
  };
  return {active:true,parts:31,method:'forward-only COPY/ADD streaming',max_emitted_chunk_bytes:MAX_OUTPUT,stable_cache_parts:0};
 }
 root.installEldoriaCopyTransport=install;
})(window);
