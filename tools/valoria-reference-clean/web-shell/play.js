"use strict";
const play=document.getElementById("play"), welcome=document.getElementById("welcome");
const loading=document.getElementById("loading"), statusText=document.getElementById("status");
const progress=document.getElementById("progress"), errorText=document.getElementById("error");
const canvas=document.getElementById("unity-canvas"), game=document.getElementById("game");
let starting=false;
// Large Unity payloads are transported as small static pieces. The Unity loader
// receives the original byte stream and original URL; no game data is rewritten.
function installChunkTransport(manifest){
 const originalFetch=window.fetch.bind(window),lookup=new Map();
 for(const [name,entry] of Object.entries(manifest))lookup.set(new URL(name,location.href).href,entry);
 window.fetch=async function(input,init){
  const address=new URL(typeof input==="string"?input:input instanceof URL?input.href:input.url,location.href).href;
  const entry=lookup.get(address);
  if(!entry)return originalFetch(input,init);
  const headers={"Content-Type":"application/octet-stream","Content-Length":String(entry.bytes),"Cache-Control":"public, max-age=31536000, immutable"};
  if(entry.sha256)headers.ETag='"'+entry.sha256+'"';
  if((init?.method||(typeof input==="object"&&input.method)||"GET").toUpperCase()==="HEAD"){
   const head=new Response(null,{status:200,headers});Object.defineProperty(head,"url",{value:address});return head;
  }
  let index=0,reader=null,received=0,cancelled=false;
  const stream=new ReadableStream({
   async pull(controller){
    try{
     while(!cancelled){
      if(!reader){
       if(index===entry.parts.length){
        if(received!==entry.bytes)throw new Error("Descarga incompleta");
        controller.close();return;
       }
       const response=await originalFetch(entry.parts[index++],{signal:init?.signal,cache:"force-cache"});
       if(!response.ok||!response.body)throw new Error("No se pudo descargar una parte del juego");
       reader=response.body.getReader();
      }
      const chunk=await reader.read();
      if(chunk.done){reader.releaseLock();reader=null;continue;}
      received+=chunk.value.byteLength;controller.enqueue(chunk.value);return;
     }
    }catch(error){controller.error(error);}
   },
   async cancel(reason){cancelled=true;if(reader)await reader.cancel(reason);}
  });
  const response=new Response(stream,{status:200,headers});
  Object.defineProperty(response,"url",{value:address});
  return response;
 };
}
function showError(error){
 console.error("Valoria playtest:",error);
 document.body.classList.remove("playing");game.hidden=true;welcome.hidden=false;loading.hidden=true;
 errorText.hidden=false;errorText.textContent="No se ha podido abrir Valoria. Vuelve a intentarlo con una conexión estable. Tu progreso guardado no se borra.";
 play.hidden=false;play.textContent="Volver a cargar";play.onclick=()=>location.reload();
}
play.addEventListener("click",async()=>{
 if(starting)return;starting=true;play.hidden=true;loading.hidden=false;
 try{
  const response=await fetch("build-manifest.json",{cache:"no-cache"});
  if(!response.ok)throw new Error("Configuración del juego no disponible");
  const build=await response.json();installChunkTransport(build.chunks);
  const script=document.createElement("script");script.src=build.loader;
  await new Promise((resolve,reject)=>{script.onload=resolve;script.onerror=()=>reject(new Error("No se pudo descargar el motor"));document.body.appendChild(script);});
  game.hidden=false;
  // Keep the real canvas measurable during startup without covering the loader.
  game.style.visibility="hidden";
  const instance=await createUnityInstance(canvas,{
   dataUrl:build.data,frameworkUrl:build.framework,codeUrl:build.code,
   streamingAssetsUrl:"StreamingAssets",companyName:"Eldoria",productName:"Valoria",productVersion:build.revision,
   devicePixelRatio:Math.min(window.devicePixelRatio||1,1.5),
   matchWebGLToCanvasSize:true,
   showBanner:(message,type)=>{if(type==="error")showError(new Error(message));else console.warn(message);}
  },value=>{
   progress.value=value;statusText.textContent=value<.9?"Cargando Valoria… "+Math.round(value*100)+" %":"Entrando en la ciudad…";
  });
  window.valoriaGame=instance;welcome.hidden=true;game.style.visibility="visible";document.body.classList.add("playing");canvas.focus();
 }catch(error){showError(error);}
});
