// Opt-in desktop QA observer. No Unity calls, saves, resource deletion or GL error consumption.
// BUFFER_SIZE is the API allocation size, not physical GPU/process memory. Explicit deletes
// are counted; GC-only disposal is not observable. Queries can perturb timing: not an FPS test.
function installWebGLBufferLedger(root=globalThis){
 const states=new WeakMap(),installed=new Set(),wrappedMethods=new WeakSet();
 const total={allocatedBytes:0,peakBytes:0,liveBuffers:0,allocations:0,deletions:0,
  drawCalls:0,submittedElements:0,observerErrors:0,contextsLost:0};
 function state(gl){
  if(!states.has(gl)){
   states.set(gl,{buffers:new WeakMap()});
   gl.canvas?.addEventListener?.('webglcontextlost',()=>{total.contextsLost++;});
  }
  return states.get(gl);
 }
 const bindings=new Map([[0x8892,0x8894],[0x8893,0x8895],[0x8f36,0x8f36],
  [0x8f37,0x8f37],[0x88eb,0x88ed],[0x88ec,0x88ef],[0x8c8e,0x8c8f],[0x8a11,0x8a28]]);
 function wrap(proto,name,observe){
  const original=proto[name];if(typeof original!=='function'||wrappedMethods.has(original))return;
  const wrapped=function(...args){
   const result=Reflect.apply(original,this,args);
   try{observe(this,args);}catch(_){total.observerErrors++;}
   return result;
  };
  wrappedMethods.add(wrapped);
  Object.defineProperty(proto,name,{value:wrapped,writable:true,configurable:true});
 }
 for(const ctor of [root.WebGLRenderingContext,root.WebGL2RenderingContext]){
  const proto=ctor?.prototype;if(!proto||installed.has(proto))continue;installed.add(proto);
  wrap(proto,'bufferData',(gl,args)=>{
   const binding=bindings.get(args[0]);if(binding===undefined)return;
   const buffer=gl.getParameter(binding);if(!buffer)return;
   const size=gl.getBufferParameter(args[0],0x8764);
   if(!Number.isFinite(size)||size<0)return;
   const buffers=state(gl).buffers,prior=buffers.get(buffer);
   total.allocatedBytes+=size-(prior||0);if(prior===undefined)total.liveBuffers++;
   buffers.set(buffer,size);total.peakBytes=Math.max(total.peakBytes,total.allocatedBytes);
   total.allocations++;
  });
  wrap(proto,'deleteBuffer',(gl,args)=>{
   const buffers=state(gl).buffers,prior=buffers.get(args[0]);
   if(prior!==undefined){total.allocatedBytes-=prior;total.liveBuffers--;buffers.delete(args[0]);total.deletions++;}
  });
  for(const [name,count,instances] of [['drawArrays',2,null],['drawElements',1,null],
   ['drawArraysInstanced',2,3],['drawElementsInstanced',1,4],['drawRangeElements',3,null]])
   wrap(proto,name,(_gl,args)=>{total.drawCalls++;total.submittedElements+=args[count]*(instances===null?1:args[instances]);});
 }
 root.__eldoriaGpuLedger={snapshot:()=>({...total,at:root.performance?.now?.()||0,
  scope:'WEBGL_BUFFER_ALLOCATIONS_NOT_PHYSICAL_GPU_MEMORY',timingPerturbed:true})};
}
module.exports={installWebGLBufferLedger};
