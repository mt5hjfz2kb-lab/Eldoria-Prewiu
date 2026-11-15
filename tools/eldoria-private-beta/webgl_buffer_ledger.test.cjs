const {test}=require('node:test'),assert=require('node:assert/strict');
const {installWebGLBufferLedger}=require('./webgl_buffer_ledger.cjs');
test('counts measured replacements and explicit deletes once, preserves GL calls and exceptions',()=>{
 class GL {
  constructor(){this.sizes=new WeakMap();this.bound=null;this.calls=0;}
  getParameter(){return this.bound;}
  getBufferParameter(){return this.sizes.get(this.bound);}
  bufferData(_target,size){if(size<0)throw Error('native');this.calls++;this.sizes.set(this.bound,size);return 7;}
  deleteBuffer(buffer){this.sizes.delete(buffer);}
  drawElements(_mode,_count){this.calls++;return 9;}
 }
 class GL2 extends GL {}
 const root={WebGLRenderingContext:GL,WebGL2RenderingContext:GL2};installWebGLBufferLedger(root);
 const gl=new GL2(),buffer={};gl.bound=buffer;
 assert.equal(gl.bufferData(0x8892,100),7);gl.bufferData(0x8892,40);
 assert.equal(gl.drawElements(4,12),9);
 assert.throws(()=>gl.bufferData(0x8892,-1),/native/);
 let s=root.__eldoriaGpuLedger.snapshot();assert.equal(s.allocatedBytes,40);assert.equal(s.peakBytes,100);
 assert.equal(s.allocations,2);assert.equal(s.liveBuffers,1);assert.equal(s.drawCalls,1);assert.equal(s.submittedElements,12);
 gl.deleteBuffer(buffer);gl.deleteBuffer(buffer);s=root.__eldoriaGpuLedger.snapshot();
 assert.equal(s.allocatedBytes,0);assert.equal(s.liveBuffers,0);assert.equal(s.deletions,1);assert.equal(s.observerErrors,0);
});
