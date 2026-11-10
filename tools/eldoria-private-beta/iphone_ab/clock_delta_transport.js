"use strict";
// Eldoria owner-only A/B. Exact, incremental reconstruction of Unity .data,
// using pre-verified VCDIFF patches over the already-published beta10 chunks.
// No telemetry, no saves, no Unity runtime changes. Fail closed on any bad hash.
// Browser decoder is bundled locally from @ably/vcdiff-decoder@1.0.6 (Apache-2.0).
(function(root){
  function hex(bytes){
    return Array.from(bytes,b=>b.toString(16).padStart(2,'0')).join('');
  }
  async function decodeGzip(response){
    if(!response.ok||!response.body)throw Error('Eldoria stable chunk not available');
    const enc=(response.headers.get('Content-Encoding')||'').toLowerCase();
    const decoded=enc==='gzip'?response.body:
       response.body.pipeThrough(new DecompressionStream('gzip'));
    return new Uint8Array(await new Response(decoded).arrayBuffer());
  }
  function makeTransport(dataUrl,items,expectedTotal,opts={}){
    if(!Array.isArray(items)||items.length!==31||
       !Number.isSafeInteger(expectedTotal)||expectedTotal<500000000||
       !root.EldoriaVCDIFF || typeof root.EldoriaVCDIFF.decode!=='function'||
       !root.crypto || !root.crypto.subtle)throw Error('Eldoria exact delta unavailable');
    const target=new URL(dataUrl,root.location.href).href;
    const baselineFetch=root.fetch.bind(root);let inUse=false;
    root.fetch=async function(input,init){
      const candidate=new URL(typeof input==='string'?input:input instanceof URL?input.href:input.url,root.location.href).href;
      if(candidate!==target)return baselineFetch(input,init);
      if(inUse)throw Error('Eldoria simultaneous data reconstruction refused');
      inUse=true;
      let index=0,total=0,closed=false;
      const finish=()=>{if(!closed){closed=true;inUse=false}};
      const stream=new ReadableStream({
        async pull(controller){
          try{
            if(index===items.length){
              if(total!==expectedTotal)throw Error('Eldoria exact data length mismatch');
              finish();controller.close();return;
            }
            const item=items[index];
            if(!item || !Number.isSafeInteger(item.decoded_bytes) ||
               item.decoded_bytes<=0 || item.decoded_bytes>16777216 ||
               !/^[a-f0-9]{64}$/.test(item.decoded_sha256) ||
               typeof item.base_url!=='string'||typeof item.patch_url!=='string')
              throw Error('Eldoria invalid delta metadata');
            const sourceUrl=new URL(item.base_url,root.location.href);
            const patchUrl=new URL(item.patch_url,root.location.href);
            if(sourceUrl.origin!==root.location.origin ||
               patchUrl.origin!==root.location.origin ||
               !sourceUrl.pathname.includes('/beta-native325/payload-decoded/') ||
               !patchUrl.pathname.includes('/beta-iphone-clock-ab/delta-patches/'))
               throw Error('Eldoria cross-origin or unapproved delta path');
            const [baseResponse,patchResponse]=await Promise.all([
              baselineFetch(sourceUrl.href,{cache:'no-store',credentials:'omit'}),
              baselineFetch(patchUrl.href,{cache:'no-store',credentials:'omit'})
            ]);
            if(!patchResponse.ok)throw Error('Eldoria patch not available');
            const base=await decodeGzip(baseResponse);
            const patch=new Uint8Array(await patchResponse.arrayBuffer());
            if(patch.byteLength===0||patch.byteLength>3000000)throw Error('Eldoria patch size mismatch');
            const decoded=root.EldoriaVCDIFF.decode(patch,base);
            if(!(decoded instanceof Uint8Array) || decoded.byteLength!==item.decoded_bytes)
              throw Error('Eldoria reconstructed chunk size mismatch');
            const digest=hex(new Uint8Array(await root.crypto.subtle.digest('SHA-256',decoded)));
            if(digest!==item.decoded_sha256)throw Error('Eldoria reconstructed chunk SHA mismatch');
            total+=decoded.byteLength;index++;
            controller.enqueue(decoded);
          }catch(error){
            finish();controller.error(error);
          }
        },
        cancel(){finish();}
      },{highWaterMark:1});
      const response=new Response(stream,{headers:{
        'Content-Type':'application/octet-stream',
        'Content-Length':String(expectedTotal),
        'Cache-Control':'no-store'
      }});
      Object.defineProperty(response,'url',{value:target});
      return response;
    };
    return {active:true,total:expectedTotal,parts:items.length};
  }
  root.installEldoriaClockDeltaTransport=makeTransport;
})(window);
