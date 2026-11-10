'use strict';
// Low-residency browser transport for exact, source-verified beta10 -> corrected clock.
// The candidate engine is ISOLATED. No storage/cache/save mutations; no external CDN.
// Source and literals are cryptographically verified individually; the full target is
// pre-reconstructed and SHA-checked (all 31 parts) in CI with the same COPY/ADD ops.
// Explicit iPhone memory design: maximum 256 KiB output per ReadableStream pull,
// only ONE 16 MiB stable input cache, and no full 16 MiB target allocation/digest.
// Physical iPhone stability remains UNVERIFIED until the owner tests it.
(function(root){
  const shaRe=/^[a-f0-9]{64}$/;
  const MAX_OUTPUT=262144;
  const asHex=x=>Array.from(x,b=>b.toString(16).padStart(2,'0')).join('');
  const hash=async bytes=>asHex(new Uint8Array(await root.crypto.subtle.digest('SHA-256',bytes)));
  function safeURL(href,route){
    if(typeof href!=='string'||href.includes('\\')||href.includes('?')||href.includes('#'))
      throw Error('Eldoria invalid asset path');
    const u=new URL(href,root.location.href);
    if(u.origin!==root.location.origin||!u.pathname.includes(route))
      throw Error('Eldoria cross-origin or unapproved asset path');
    return u.href;
  }
  async function verifiedFetch(fetcher,href,length,sha){
    if(!Number.isSafeInteger(length)||length<0||!shaRe.test(sha))
      throw Error('Eldoria invalid SHA/length metadata');
    const response=await fetcher(href,{cache:'no-store',credentials:'omit'});
    if(!response.ok)throw Error('Eldoria exact asset not available');
    // Zero-byte literal files occur naturally between copied Unity segments.
    // Some real Safari versions expose a null response.body for an empty file.
    if(length===0){
      const empty=new Uint8Array(0);
      if(await hash(empty)!==sha)throw Error('Eldoria zero-byte literal hash mismatch');
      return empty;
    }
    if(!response.body)throw Error('Eldoria expected source stream missing');
    let bytes;
    if(href.endsWith('.gz')){
      const alreadyDecoded=(response.headers.get('Content-Encoding')||'').toLowerCase()==='gzip';
      const body=alreadyDecoded?response.body:response.body.pipeThrough(new root.DecompressionStream('gzip'));
      bytes=new Uint8Array(await new Response(body).arrayBuffer());
    }else bytes=new Uint8Array(await response.arrayBuffer());
    if(bytes.byteLength!==length||await hash(bytes)!==sha)
      throw Error('Eldoria exact source or literal SHA mismatch');
    return bytes;
  }
  function install(dataUrl,info){
    if(!info||info.format!=='ELDORIA_COPY_ADD_V1'||
       info.stable_source_sha!=='be49d62e3daf490f41dba0bf8123a8e2eee64480'||
       info.candidate_source_sha!=='004d3ef9c26e9070762e9e38ce84d69d525efd4f'||
       info.stable_part_size!==16777216||info.stable_size!==503830840||
       info.candidate_size!==503831429||
       !Array.isArray(info.stable_parts)||info.stable_parts.length!==31||
       !Array.isArray(info.target_parts)||info.target_parts.length!==31||
       !root.crypto?.subtle||typeof root.DecompressionStream!=='function')
      throw Error('Eldoria exact candidate metadata or browser API unavailable');
    const request=new URL(dataUrl,root.location.href).href;
    const originalFetch=root.fetch.bind(root);
    let active=false,baseIndex=-1,baseBytes=null;
    async function getBase(index){
      if(index<0||index>=31)throw Error('Eldoria source index out of range');
      if(index===baseIndex&&baseBytes)return baseBytes;
      const part=info.stable_parts[index];
      const bytes=await verifiedFetch(originalFetch,
        safeURL(part.url,'/beta-native325/payload-decoded/'),
        part.decoded_bytes,part.decoded_sha256);
      baseIndex=index;baseBytes=bytes;
      return bytes;
    }
    root.fetch=async function(input,init){
      const url=new URL(typeof input==='string'?input:input instanceof URL?input.href:input.url,root.location.href).href;
      if(url!==request)return originalFetch(input,init);
      if(active)throw Error('Eldoria concurrent streamed Unity data refused');
      active=true;
      let partIndex=0,partOffset=0,opIndex=0,opOffset=0,partLiteral=null,total=0;
      const release=()=>{active=false;baseIndex=-1;baseBytes=null;partLiteral=null;};
      const stream=new ReadableStream({
        async pull(controller){
          try{
            if(partIndex===31){
              if(total!==info.candidate_size)throw Error('Eldoria final data length mismatch');
              release();controller.close();return;
            }
            const part=info.target_parts[partIndex];
            if(!part||part.index!==partIndex||!Number.isSafeInteger(part.decoded_bytes)||
               part.decoded_bytes<1||part.decoded_bytes>16777216||
               !Number.isSafeInteger(part.literal_bytes)||part.literal_bytes<0||
               part.literal_bytes>6000000||!shaRe.test(part.decoded_sha256)||
               !Array.isArray(part.ops)||!part.ops.length)
              throw Error('Eldoria malformed target part');
            if(partLiteral===null)
              partLiteral=await verifiedFetch(originalFetch,
                safeURL(part.literal_url,'/beta-iphone-clock-ab/copy-literals/'),
                part.literal_bytes,part.literal_sha256);
            const size=Math.min(MAX_OUTPUT,part.decoded_bytes-partOffset);
            if(size<=0)throw Error('Eldoria invalid stream part offset');
            const chunk=new Uint8Array(size);
            let written=0;
            while(written<size){
              const op=part.ops[opIndex];
              if(!Array.isArray(op)||op.length!==3)
                throw Error('Eldoria missing or malformed operation');
              const [kind,position,length]=op;
              if(!Number.isSafeInteger(position)||!Number.isSafeInteger(length)||
                 position<0||length<=0||opOffset<0||opOffset>=length)
                throw Error('Eldoria operation out of range');
              const take=Math.min(length-opOffset,size-written);
              if(kind==='A'){
                if(position+opOffset+take>partLiteral.length)
                  throw Error('Eldoria literal beyond bounds');
                chunk.set(partLiteral.subarray(position+opOffset,position+opOffset+take),written);
              }else if(kind==='C'){
                if(position+opOffset+take>info.stable_size)
                  throw Error('Eldoria source copy beyond bounds');
                let src=position+opOffset,remaining=take,dst=written;
                while(remaining>0){
                  const index=Math.floor(src/info.stable_part_size);
                  const old=await getBase(index);
                  const offset=src-index*info.stable_part_size;
                  const count=Math.min(remaining,old.length-offset);
                  if(count<1)throw Error('Eldoria invalid source boundary');
                  chunk.set(old.subarray(offset,offset+count),dst);
                  src+=count;dst+=count;remaining-=count;
                }
              }else throw Error('Unapproved Eldoria byte operation');
              written+=take;opOffset+=take;
              if(opOffset===length){opIndex++;opOffset=0;}
            }
            partOffset+=written;total+=written;
            if(partOffset===part.decoded_bytes){
              if(opIndex!==part.ops.length||opOffset!==0)
                throw Error('Eldoria incomplete part operation program');
              partIndex++;partOffset=0;opIndex=0;opOffset=0;partLiteral=null;
            }
            controller.enqueue(chunk);
          }catch(error){release();controller.error(error);}
        },
        cancel(){release();}
      },{highWaterMark:1});
      const response=new Response(stream,{headers:{
        'Content-Type':'application/octet-stream',
        'Content-Length':String(info.candidate_size),
        'Cache-Control':'no-store'
      }});
      Object.defineProperty(response,'url',{value:request});
      return response;
    };
    return {active:true,parts:31,method:'low-memory verified COPY/ADD streaming',
      max_emitted_chunk_bytes:MAX_OUTPUT,stable_cache_parts:1};
  }
  root.installEldoriaCopyTransport=install;
})(window);
