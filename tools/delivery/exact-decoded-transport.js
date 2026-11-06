"use strict";
// Fetch already exposes decoded gzip/Brotli bytes. Supply their verified size to Unity
// so its loader allocates once instead of estimating 1.6x and then slicing.
function installExactDecodedTransport(sizes, parts={}) {
 const originalFetch=window.fetch.bind(window);
 const lookup=new Map(Object.entries(sizes).map(([url,size])=>[new URL(url,location.href).href,size]));
 const segmented=new Map(Object.entries(parts).map(([url,list])=>[new URL(url,location.href).href,list]));
 window.fetch=async function(input,init) {
  const address=new URL(typeof input==="string"?input:input instanceof URL?input.href:input.url,location.href).href;
  const list=segmented.get(address);
  if(list) {
   let index=0, reader=null, received=0, total=0;
   const stream=new ReadableStream({
    async pull(controller) {
     try {
      while(true) {
       if(!reader) {
        if(index===list.length) {
         if(total!==lookup.get(address))throw Error('Decoded Unity total size mismatch');
         controller.close();return;
        }
        const part=await originalFetch(new URL(list[index].url,location.href).href,init);
        if(!part.ok || !part.body)throw Error('Unity data part failed: '+part.status);
        // https://itch.io/docs/creators/html5#compression: .br files receive
        // Content-Encoding:br. Fetch already decodes either HTTP encoding.
        // Preserve complete-gzip fallback for older packages/static QA servers.
        const encoding=part.headers.get('Content-Encoding')?.toLowerCase();
        const expectedEncoding=list[index].encoding || 'gzip';
        if(expectedEncoding==='br' && encoding!=='br')throw Error('Brotli CDN header missing');
        const body=encoding==='gzip' || encoding==='br'
         ?part.body:part.body.pipeThrough(new DecompressionStream('gzip'));
        reader=body.getReader();received=0;
       }
       const chunk=await reader.read();
       if(chunk.done) {
        if(received!==list[index].decoded_bytes)throw Error('Decoded Unity part size mismatch');
        reader.releaseLock();reader=null;index++;continue;
       }
       received+=chunk.value.byteLength;total+=chunk.value.byteLength;
       if(received>list[index].decoded_bytes)throw Error('Decoded Unity part overflow');
       controller.enqueue(chunk.value);return;
      }
     } catch(error) {if(reader)await reader.cancel(error).catch(()=>{});controller.error(error);}
    },
    async cancel(reason) {if(reader)await reader.cancel(reason);}
   });
   const response=new Response(stream,{headers:{'Content-Length':String(lookup.get(address)),'Content-Type':'application/octet-stream'}});
   Object.defineProperty(response,'url',{value:address});return response;
  }
  const response=await originalFetch(input,init);
  const size=lookup.get(address);
  if(!size || response.status!==200 || !response.body || !["gzip","br"].includes(response.headers.get("Content-Encoding")?.toLowerCase()))return response;
  const headers=new Headers(response.headers);
  headers.delete("Content-Encoding");
  headers.set("Content-Length",String(size));
  const decoded=new Response(response.body,{status:response.status,statusText:response.statusText,headers});
  Object.defineProperty(decoded,"url",{value:response.url||address});
  return decoded;
 };
}


