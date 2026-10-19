"use strict";
// Fetch already exposes decoded gzip bytes. Supply their verified size to Unity
// so its loader allocates once instead of estimating 1.6x and then slicing.
function installExactDecodedTransport(sizes) {
 const originalFetch=window.fetch.bind(window);
 const lookup=new Map(Object.entries(sizes).map(([url,size])=>[new URL(url,location.href).href,size]));
 window.fetch=async function(input,init) {
  const address=new URL(typeof input==="string"?input:input instanceof URL?input.href:input.url,location.href).href;
  const response=await originalFetch(input,init);
  const size=lookup.get(address);
  if(!size || response.status!==200 || !response.body || response.headers.get("Content-Encoding")?.toLowerCase()!=="gzip")return response;
  const headers=new Headers(response.headers);
  headers.delete("Content-Encoding");
  headers.set("Content-Length",String(size));
  const decoded=new Response(response.body,{status:response.status,statusText:response.statusText,headers});
  Object.defineProperty(decoded,"url",{value:response.url||address});
  return decoded;
 };
}
