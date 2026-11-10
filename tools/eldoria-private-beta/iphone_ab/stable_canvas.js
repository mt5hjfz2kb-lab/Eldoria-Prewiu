// Candidate only: keep Unity's render target stable while Safari's keyboard and
// toolbar change viewport height. Width changes (including rotation) still resize.
// No gameplay, storage, automatic navigation, or browser-history changes.
(function(root){
 'use strict';
 function install(canvas,win=root){
  let cssWidth=0,timer=null,disposed=false;
  const changes=[];
  function apply(force){
   if(disposed)return;
   const box=canvas.getBoundingClientRect();
   const w=Math.round(box.width),h=Math.round(box.height);
   if(w<1||h<1)return;
   if(!force&&w===cssWidth)return;
   cssWidth=w;
   const dpr=Math.min(Math.max(Number(win.devicePixelRatio)||1,1),1.5);
   const width=Math.round(w*dpr),height=Math.round(h*dpr);
   // Assigning an unchanged canvas dimension can reallocate the render target.
   if(canvas.width!==width)canvas.width=width;
   if(canvas.height!==height)canvas.height=height;
   changes.push({width,height});if(changes.length>16)changes.shift();
  }
  function resize(){win.clearTimeout(timer);timer=win.setTimeout(()=>apply(false),250);}
  apply(true);win.addEventListener('resize',resize,{passive:true});
  return {changes,dispose(){disposed=true;win.clearTimeout(timer);win.removeEventListener('resize',resize);}};
 }
 root.installEldoriaStableCanvas=install;
 if(typeof module==='object')module.exports={install};
})(typeof window==='object'?window:globalThis);
