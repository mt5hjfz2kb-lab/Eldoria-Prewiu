'use strict';
// Owner-only, SHA-verified COPY + literal reconstruction. Does not read/write
// browser persistence or change the stable beta10 engine/game/data files.
(function(root){
 const shaRe=/^[a-f0-9]{64}$/;
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
   if(!response.ok||!response.body)throw Error('Eldoria exact asset not available');
   let bytes;
   if(href.endsWith('.gz')){
     const encoded=(response.headers.get('Content-Encoding')||'').toLowerCase()==='gzip';
     const body=encoded?response.body:response.body.pipeThrough(new root.DecompressionStream('gzip'));
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
   let active=false;
   const cache=new Map();
   async function getBase(index){
     if(index<0||index>=31)throw Error('Eldoria source index out of range');
     if(cache.has(index)){
       const b=cache.get(index);cache.delete(index);cache.set(index,b);return b;
     }
     const part=info.stable_parts[index];
     const bytes=await verifiedFetch(originalFetch,
       safeURL(part.url,'/beta-native325/payload-decoded/'),
       part.decoded_bytes,part.decoded_sha256);
     cache.set(index,bytes);
     while(cache.size>2)cache.delete(cache.keys().next().value);
     return bytes;
   }
   root.fetch=async function(input,init){
     const url=new URL(typeof input==='string'?input:input instanceof URL?input.href:input.url,root.location.href).href;
     if(url!==request)return originalFetch(input,init);
     if(active)throw Error('Eldoria concurrent streamed Unity data refused');
     active=true;
     let partIndex=0,total=0;
     const release=()=>{active=false;cache.clear();};
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
              part.literal_bytes>6000000||!Array.isArray(part.ops)||!part.ops.length)
             throw Error('Eldoria malformed target part');
           const literal=await verifiedFetch(originalFetch,
             safeURL(part.literal_url,'/beta-iphone-clock-ab/copy-literals/'),
             part.literal_bytes,part.literal_sha256);
           const target=new Uint8Array(part.decoded_bytes);
           let written=0;
           for(const op of part.ops){
             if(!Array.isArray(op)||op.length!==3)throw Error('Eldoria malformed operation');
             const [kind,position,length]=op;
             if(!Number.isSafeInteger(position)||!Number.isSafeInteger(length)||
                position<0||length<=0||written+length>target.length)
               throw Error('Eldoria operation out of range');
             if(kind==='A'){
               if(position+length>literal.length)throw Error('Eldoria literal beyond bounds');
               target.set(literal.subarray(position,position+length),written);
             }else if(kind==='C'){
               if(position+length>info.stable_size)throw Error('Eldoria old stream beyond bounds');
               let src=position,remaining=length,dst=written;
               while(remaining>0){
                 const idx=Math.floor(src/info.stable_part_size);
                 const old=await getBase(idx);
                 const offset=src-idx*info.stable_part_size;
                 const count=Math.min(remaining,old.length-offset);
                 if(count<1)throw Error('Eldoria invalid source boundary');
                 target.set(old.subarray(offset,offset+count),dst);
                 src+=count;dst+=count;remaining-=count;
               }
             }else throw Error('Unapproved Eldoria byte operation');
             written+=length;
           }
           if(written!==target.length||await hash(target)!==part.decoded_sha256)
             throw Error('Reconstructed Unity chunk SHA mismatch');
           total+=target.length;partIndex++;controller.enqueue(target);
         }catch(error){release();controller.error(error);}
       },
       cancel(){release();}
     },{highWaterMark:1});
     const response=new Response(stream,{headers:{
       'Content-Type':'application/octet-stream','Content-Length':String(info.candidate_size),
       'Cache-Control':'no-store'}});
     Object.defineProperty(response,'url',{value:request});
     return response;
   };
   return {active:true,parts:31,method:'SHA-verified copy/add'};
 }
 root.installEldoriaCopyTransport=install;
})(window);
