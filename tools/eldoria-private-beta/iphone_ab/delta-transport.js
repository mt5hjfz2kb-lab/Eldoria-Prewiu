'use strict';
/*
 * Eldoria P0 owner-only iPhone clock A/B: immutable beta10 data + verified
 * reconstruction of the exact Unity mobile12 data bytes. No save access,
 * no telemetry, no remote origin, no mutation of the stable Pages route.
 * Do NOT activate public testers without a genuine 30-minute phone test.
 */
async function installEldoriaDeltaTransport(build) {
  const cfg = build.eldoria_delta_transport;
  if (!cfg || cfg.schema !== 'eldoria-candidate-delta-from-frozen-beta10-v1') {
    throw Error('Unknown Eldoria candidate delta transport');
  }
  const originalFetch = window.fetch.bind(window);
  const mresp = await originalFetch(new URL(cfg.manifest, location.href), {cache:'no-cache'});
  if (!mresp.ok) throw Error('Delta manifest unavailable');
  const manifest = await mresp.json();
  if (manifest.schema !== cfg.schema ||
      manifest.stable_source_sha !== 'be49d62e3daf490f41dba0bf8123a8e2eee64480' ||
      manifest.candidate_source_sha !== '004d3ef9c26e9070762e9e38ce84d69d525efd4f' ||
      manifest.candidate_revision !== build.revision ||
      manifest.candidate_parts.length !== 31 || manifest.base_parts.length !== 31 ||
      manifest.chunk_bytes !== 16777216 ||
      manifest.candidate_decoded_bytes !== build.decoded_sizes[build.data]) {
    throw Error('Wrong immutable Eldoria build or delta manifest');
  }
  const baseIdentityResponse = await originalFetch(new URL('../beta-native325/release-identity.json', location.href), {cache:'no-cache'});
  if (!baseIdentityResponse.ok) throw Error('Stable beta10 identity unavailable');
  const baseIdentity=await baseIdentityResponse.json();
  if (baseIdentity.source_sha!==manifest.stable_source_sha || baseIdentity.revision!=='native325-mobile10-pretest-ux') {
    throw Error('Stable beta10 source changed, candidate delta cannot start');
  }
  const target=new URL(build.data,location.href).href;
  const size=build.decoded_sizes[build.data];
  let baseChunkId=-1,baseChunk=null;
  async function getDecoded(url,expectedSize,expectedSha) {
    const response=await originalFetch(new URL(url,location.href), {cache:'force-cache'});
    if (!response.ok || !response.body) throw Error('Missing Eldoria immutable data chunk '+response.status);
    let body=response.body;
    const enc=(response.headers.get('Content-Encoding')||'').toLowerCase();
    if (!enc) body=body.pipeThrough(new DecompressionStream('gzip'));
    else if (enc!=='gzip') throw Error('Unsupported Eldoria compression '+enc);
    const bytes=new Uint8Array(await new Response(body).arrayBuffer());
    if (bytes.byteLength!==expectedSize) throw Error('Eldoria exact decoded byte count mismatch');
    const digest=Array.from(new Uint8Array(await crypto.subtle.digest('SHA-256',bytes)),v=>v.toString(16).padStart(2,'0')).join('');
    if (digest!==expectedSha) throw Error('Eldoria immutable chunk hash mismatch');
    return bytes;
  }
  async function getBaseChunk(index) {
    if (index===baseChunkId) return baseChunk;
    const d=manifest.base_parts[index];
    if (!d) throw Error('Eldoria delta invalid source chunk index');
    baseChunk=null;
    baseChunkId=-1;
    const chunk=await getDecoded(d.url,d.bytes,d.sha256);
    baseChunk=chunk;baseChunkId=index;
    return chunk;
  }
  async function buildPart(index) {
    const d=manifest.candidate_parts[index];
    if (!d || !Array.isArray(d.ops) || d.ops.length===0 || d.size<=0) throw Error('Eldoria invalid candidate part');
    const result=new Uint8Array(d.size);
    const literals=d.literal_url ? await getDecoded(d.literal_url,d.literal_bytes,d.literal_sha256) : null;
    let cursor=0;
    for (const op of d.ops) {
      let sourceOffset=op.offset;
      let left=op.length;
      if (!Number.isSafeInteger(left)||left<=0||!Number.isSafeInteger(sourceOffset)||sourceOffset<0 || left+cursor>d.size) {
        throw Error('Eldoria invalid delta copy instruction');
      }
      if(op.kind==='literal') {
        if(!literals||sourceOffset+left>literals.byteLength)throw Error('Eldoria delta literal out of range');
        result.set(literals.subarray(sourceOffset,sourceOffset+left),cursor);
        cursor+=left;
      } else if(op.kind==='copy') {
        while(left>0) {
          const k=Math.floor(sourceOffset/manifest.chunk_bytes);
          const local=sourceOffset%manifest.chunk_bytes;
          const source=await getBaseChunk(k);
          const n=Math.min(left,source.length-local);
          if(n<=0)throw Error('Eldoria invalid source copy range');
          result.set(source.subarray(local,local+n),cursor);
          sourceOffset+=n;cursor+=n;left-=n;
        }
      }else throw Error('Eldoria unknown delta opcode');
    }
    if(cursor!==result.byteLength) throw Error('Eldoria candidate delta truncated');
    const digest=Array.from(new Uint8Array(await crypto.subtle.digest('SHA-256',result)),v=>v.toString(16).padStart(2,'0')).join('');
    if(digest!==d.sha256)throw Error('Eldoria candidate data SHA mismatch');
    return result;
  }
  window.fetch=async function(input,init) {
    const url=new URL(typeof input==='string'?input:input instanceof URL?input.href:input.url,location.href).href;
    if(url!==target)return originalFetch(input,init);
    let index=0,bytesDelivered=0;
    const stream=new ReadableStream({
      async pull(controller){
        try {
          if(index===manifest.candidate_parts.length){
            if(bytesDelivered!==size) throw Error('Eldoria candidate data total mismatch');
            controller.close();baseChunk=null;return;
          }
          const part=await buildPart(index++);
          bytesDelivered+=part.byteLength;
          controller.enqueue(part);
        } catch(error) {baseChunk=null;controller.error(error);}
      },
      cancel(){baseChunk=null;}
    },{highWaterMark:0});
    const result=new Response(stream,{status:200,headers:{'Content-Length':String(size),'Content-Type':'application/octet-stream'}});
    Object.defineProperty(result,'url',{value:url});return result;
  };
  return true;
}
if(typeof module!=='undefined'&&module.exports)module.exports={installEldoriaDeltaTransport};
