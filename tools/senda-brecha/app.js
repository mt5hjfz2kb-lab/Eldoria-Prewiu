(function () {
'use strict';
const game = new SendaBrecha.EventRun();
const $ = id => document.getElementById(id);
const canvas = $('arena'), ctx = canvas.getContext('2d', {alpha:false});
let W=390, H=500, last=0, frame=0, spriteX=0, toastTime=0, pointerStart=null;
let lastDom=-1, resultShown=false;
function resize(){
  const rect=canvas.getBoundingClientRect();
  W=Math.max(1,rect.width); H=Math.max(1,rect.height);
  const dpr=Math.min(devicePixelRatio||1,2);
  canvas.width=Math.max(1,Math.round(W*dpr));canvas.height=Math.max(1,Math.round(H*dpr));
  ctx.setTransform(dpr,0,0,dpr,0,0);
}
new ResizeObserver(resize).observe(canvas);
window.addEventListener('resize',resize);
function lerp(a,b,t){return a+(b-a)*t;}
function clamp(n,a,b){return Math.max(a,Math.min(b,n));}
function hash(n){return (Math.sin(n*127.1+62.13)*43758.5453)%1;}
function pseudo(n){return Math.abs(hash(n));}
function ellipse(x,y,rx,ry,color){ctx.beginPath();ctx.ellipse(x,y,rx,ry,0,0,Math.PI*2);ctx.fillStyle=color;ctx.fill();}
function line(x1,y1,x2,y2,col,width=1){ctx.beginPath();ctx.moveTo(x1,y1);ctx.lineTo(x2,y2);ctx.strokeStyle=col;ctx.lineWidth=width;ctx.stroke();}
function polygon(points,fill,stroke=null){ctx.beginPath();ctx.moveTo(points[0][0],points[0][1]);for(let i=1;i<points.length;i++)ctx.lineTo(points[i][0],points[i][1]);ctx.closePath();ctx.fillStyle=fill;ctx.fill();if(stroke){ctx.strokeStyle=stroke;ctx.stroke();}}
function roadHalf(y){let t=clamp((y-H*.24)/(H*.76),0,1);return lerp(W*.21,W*.66,Math.pow(t,1.05));}
function roadX(y,lane){return W/2+(lane===0?-1:1)*roadHalf(y)*.43;}
function sky(){
  const g=ctx.createLinearGradient(0,0,0,H);g.addColorStop(0,'#101725');g.addColorStop(.34,'#2a2937');g.addColorStop(.64,'#28303a');g.addColorStop(1,'#121a21');ctx.fillStyle=g;ctx.fillRect(0,0,W,H);
  const moon=ctx.createRadialGradient(W*.55,H*.13,2,W*.55,H*.13,W*.4);
  moon.addColorStop(0,'rgba(188,120,229,.22)');moon.addColorStop(.45,'rgba(165,96,205,.07)');moon.addColorStop(1,'rgba(165,96,205,0)');ctx.fillStyle=moon;ctx.fillRect(0,0,W,H*.8);
  ellipse(W*.60,H*.15,W*.031,W*.04,'#a7a4aa');
  ctx.fillStyle='#131a26';
  polygon([[0,H*.27],[W*.09,H*.18],[W*.23,H*.28],[W*.42,H*.18],[W*.58,H*.27],[W*.82,H*.17],[W,H*.28],[W,H*.6],[0,H*.6]],'#1a1e2b');
  polygon([[0,H*.31],[W*.14,H*.23],[W*.35,H*.33],[W*.6,H*.25],[W*.82,H*.32],[W,H*.24],[W,H*.58],[0,H*.58]],'#222431');
  // Breach altar silhouette / scenery at horizon.
  const fx=W*.52, fy=H*.28;polygon([[fx-23,fy+4],[fx-14,fy-31],[fx-6,fy-29],[fx-5,fy-57],[fx+1,fy-64],[fx+7,fy-28],[fx+17,fy-30],[fx+25,fy+4]],'#111621');
  line(fx,fy-55,fx+4,fy-11,'#8d55a577',2);ellipse(fx+1,fy-8,15,3,'#543461');
}
function pines(){
  const scroll=game.time*22;
  for(let side=0;side<2;side++){
    for(let i=0;i<15;i++){
      const p=((i*61+scroll+side*17)%(H*.88))/(H*.88), y=H*(.28+.72*p);
      const x=W/2+(side===0?-1:1)*(roadHalf(y)*(1.18+p*.08)+10+pseudo(i+side*51)*W*.16);
      const scale=lerp(.3,1.45,p)*(W/390);
      const trunk=18*scale, h=57*scale;
      ellipse(x,y+3,13*scale,4*scale,'#090f17b7');
      line(x,y-trunk*.2,x,y-trunk,'#3e3330',Math.max(1,5*scale));
      polygon([[x,y-h],[x-20*scale,y-16*scale],[x+20*scale,y-16*scale]],'#1a2728');
      polygon([[x,y-h*.81],[x-25*scale,y-10*scale],[x+25*scale,y-10*scale]],'#243538');
      line(x-4*scale,y-h*.47,x+4*scale,y-h*.46,'#7781794f',scale);
    }
  }
}
function ground(){
  let g=ctx.createLinearGradient(0,H*.27,0,H);g.addColorStop(0,'#313036');g.addColorStop(1,'#514139');
  polygon([[W/2-roadHalf(H*.26),H*.26],[W/2+roadHalf(H*.26),H*.26],[W/2+roadHalf(H),H],[W/2-roadHalf(H),H]],g,'#85725b');
  // Road perspective grooves, stone edges and continuously moving cobbles.
  for(let lane=-1;lane<=1;lane+=2){
    const a=W/2+lane*roadHalf(H*.26)*.95,b=W/2+lane*roadHalf(H)*.95;
    line(a,H*.26,b,H,'#66594b9c',2);
    line(a+lane*3,H*.26,b+lane*7,H,'#201e2079',1);
  }
  for(let j=0;j<20;j++){
    const t=(((j/20)+game.time*.055)%1),y=lerp(H*.29,H*1.04,t*t),rw=roadHalf(y),off=(pseudo(j*4+5)-.5)*1.55;
    ellipse(W/2+rw*off,y,lerp(1,8,t),lerp(.5,2.7,t),'#9b8d7661');
    if(j%3===0)line(W/2-rw*.11,y,W/2+rw*.16,y+1,'#aa988655',Math.max(1,t*2));
  }
  // Two readable lanes without bright arcade stripes.
  for(let j=0;j<9;j++){
    const t=(((j/9)+game.time*.045)%1); const y=H*(.3+.7*t*t);
    line(W/2,y,W/2,y+2+18*t,'#b6a17e55',Math.max(.5,1.6*t));
  }
  // Purple fissures off the road; decorative only.
  for(let i=0;i<7;i++){
    const t=((i*.181+game.time*.02)%1); const y=H*(.30+t*.68); const rw=roadHalf(y);
    const x=W/2+(i%2===0?-1:1)*(rw+9);
    line(x,y,x+(i%2===0?-1:1)*(6+12*t),y+13*t,'#9258b6a8',1.8);
  }
}
function banner(x,y,text,color,small=false){
  ctx.font=`700 ${small?10:12}px Georgia,serif`;
  const tw=ctx.measureText(text).width,w=Math.min(W*.75,tw+18),h=small?23:26;
  ctx.fillStyle='#10151fe5';ctx.fillRect(x-w/2,y-h/2,w,h);
  ctx.strokeStyle=color;ctx.lineWidth=1;ctx.strokeRect(x-w/2+.5,y-h/2+.5,w-1,h-1);
  ctx.fillStyle='#fff4df';ctx.textAlign='center';ctx.textBaseline='middle';ctx.fillText(text,x,y+1,w-9);
}
function shieldUnit(x,y,size,archer=false,enemy=false,ghost=false){
  const s=size; ctx.save();ctx.translate(x,y);
  ellipse(0,1,s*.53,s*.19,'#02060b86');
  const cloak=enemy?'#30243c':archer?'#486356':'#3d5362';
  polygon([[-s*.35,-s*.78],[s*.35,-s*.78],[s*.5,-s*.16],[-s*.50,-s*.16]],cloak);
  ctx.fillStyle=enemy?'#23222f':'#6f706b';ctx.fillRect(-s*.20,-s*.75,s*.40,s*.37);
  ellipse(0,-s*.82,s*.23,s*.21,enemy?'#57416b':'#9c9a8d');
  if(enemy){ellipse(-s*.08,-s*.85,s*.048,s*.042,'#ca8ff0');ellipse(s*.08,-s*.85,s*.048,s*.042,'#ca8ff0');}
  if(archer){line(s*.22,-s*.7,s*.49,-s*.95,'#c1ad82',Math.max(1,s*.08));line(s*.43,-s*.95,s*.54,-s*.62,'#e5dab0',1);}
  else{polygon([[-s*.36,-s*.65],[-s*.05,-s*.69],[-s*.07,-s*.19],[-s*.37,-s*.22]],enemy?'#4d355e':'#bba789',enemy?'#9b54be':'#dfcdb0');line(s*.34,-s*.55,s*.47,-s*.99,'#c8c7b1',Math.max(1,s*.08));}
  if(ghost){ctx.globalAlpha=.45}
  ctx.restore();
}
function squad(){
  const y=H*.85, laneTarget=roadX(y,game.lane);
  if(spriteX===0)spriteX=laneTarget;
  spriteX=lerp(spriteX,laneTarget,.10);
  const n=Math.min(game.soldiers,24), cols=Math.min(5,Math.max(1,Math.ceil(Math.sqrt(n))));
  for(let i=0;i<n;i++){
    const row=Math.floor(i/cols), col=i%cols;
    const x=spriteX+(col-(cols-1)/2)*(W*.062) + (row%2?W*.023:0);
    const sy=y-row*(H*.034), sz=lerp(W*.052,W*.038,row/5);
    shieldUnit(x,sy,sz,i>=Math.min(n,Math.round(n*(game.vanguard/Math.max(1,game.soldiers)))));
  }
  // Banner above unit group and hero marker.
  ellipse(spriteX,y+H*.065,W*.065,5,'#111622bb');
  banner(spriteX,y-H*.21,'VALORIA · '+game.soldiers,'#b99b6b',true);
  line(spriteX+W*.10,y-H*.13,spriteX+W*.10,y-H*.03,'#b9b1a3',2);
  polygon([[spriteX+W*.10,y-H*.13],[spriteX+W*.18,y-H*.115],[spriteX+W*.10,y-H*.09]],'#675476');
}
function stoneGate(x,y,scale,gate,selected){
  const w=clamp(W*.17*scale,18,110),h=clamp(H*.17*scale,35,150);
  const col=selected?'#e7c17a':'#a2aab2';
  ellipse(x,y+2,w*.65,w*.14,'#04080db7');
  ctx.save();ctx.shadowColor=selected?'#ffc675':'#a487bd';ctx.shadowBlur=selected?14:7;
  polygon([[x-w*.51,y],[x-w*.48,y-h*.75],[x-w*.36,y-h],[x+w*.36,y-h],[x+w*.48,y-h*.75],[x+w*.51,y]],'#3c3940',col);
  ctx.restore();
  polygon([[x-w*.22,y],[x-w*.20,y-h*.67],[x,y-h*.77],[x+w*.20,y-h*.67],[x+w*.22,y]],selected?'#765c61':'#4c4b60');
  ctx.save();ctx.font=`700 ${clamp(9*scale,9,20)}px Georgia,serif`;ctx.textAlign='center';ctx.fillStyle=selected?'#ffe4aa':'#e5dfd9';ctx.fillText(gate.icon,x,y-h*.46);ctx.restore();
  line(x-w*.45,y-h*.77,x+w*.45,y-h*.77,col,2);
}
function choiceScene(){
  const progress=clamp(game.phaseTime/SendaBrecha.GATE_SECONDS,0,1);
  const y=H*(.37+.28*progress),sc=.6+1.0*progress;
  const gates=game.round.gates;
  for(let n=0;n<2;n++){const x=roadX(y,n);stoneGate(x,y,sc,gates[n],game.lane===n);}
  if(progress>.67){
    const laneX=roadX(H*.77,game.lane);
    const glow=ctx.createRadialGradient(laneX,H*.77,1,laneX,H*.77,W*.19);
    glow.addColorStop(0,'rgba(213,178,101,.13)');glow.addColorStop(1,'rgba(213,178,101,0)');ctx.fillStyle=glow;ctx.fillRect(0,H*.57,W,H*.35);
  }
}
function enemies(){
  const f=game.enemyFraction;
  const boss=game.round.enemyKind==='boss';
  const baseY=H*.56, sz=boss?Math.min(90,W*.23):Math.min(42,W*.12);
  const x=W*.5;
  const halo=ctx.createRadialGradient(x,baseY-sz*.5,3,x,baseY-sz*.5,sz*1.9);
  halo.addColorStop(0,'rgba(183,91,215,.37)');halo.addColorStop(1,'rgba(166,73,225,0)');ctx.fillStyle=halo;ctx.fillRect(0,H*.26,W,H*.48);
  if(boss){
    ellipse(x,baseY+8,sz*.6,8,'#080b0fc7');
    polygon([[x-sz*.38,baseY],[x-sz*.43,baseY-sz*.92],[x-sz*.11,baseY-sz*1.48],[x+sz*.13,baseY-sz*1.48],[x+sz*.46,baseY-sz*.92],[x+sz*.39,baseY]],'#211a2c','#83529e');
    ellipse(x,baseY-sz*1.12,sz*.24,sz*.18,'#51415d');
    polygon([[x-sz*.22,baseY-sz*1.22],[x-sz*.38,baseY-sz*1.62],[x-sz*.1,baseY-sz*1.47]],'#33283d');
    polygon([[x+sz*.22,baseY-sz*1.22],[x+sz*.39,baseY-sz*1.62],[x+sz*.1,baseY-sz*1.47]],'#33283d');
    ellipse(x-sz*.09,baseY-sz*1.15,sz*.06,sz*.028,'#e1a5ee');ellipse(x+sz*.09,baseY-sz*1.15,sz*.06,sz*.028,'#e1a5ee');
    line(x+sz*.45,baseY-sz*.5,x+sz*.8,baseY-sz*1.45,'#a77fb4',4);
  }else{
    for(let i=0;i<4;i++){
      shieldUnit(x+(i-1.5)*sz*.53,baseY+(i%2)*sz*.34,sz*.78,false,true);
    }
  }
  // Light weapon trails suggest auto-combat while damage remains mathematical.
  const pulse=game.time*5;
  for(let i=0;i<4;i++){
    const t=(pulse*.75+i*.22)%1;
    const fromX=spriteX+(i-1.5)*W*.023, toX=x+(i-1.5)*sz*.12;
    const px=lerp(fromX,toX,t),py=lerp(H*.76,baseY-sz*.2,t);
    line(px,py,px-lerp(fromX,toX,.045)+fromX,py+4,'#f6c67d',Math.max(1,W*.004));
    ellipse(px,py,2,2,'#ffe4a7');
  }
  if(Math.sin(game.time*17)>.83){
    ctx.fillStyle='#e4a8ea';ctx.font='bold 12px system-ui';ctx.textAlign='center';
    ctx.fillText('−'+Math.ceil(game.attackDPS),x+sz*.5,baseY-sz*.6);
  }
  if(f<.3){
    const g=ctx.createRadialGradient(x,baseY,0,x,baseY,sz);
    g.addColorStop(0,'#bc7cd930');g.addColorStop(1,'#b064d900');ctx.fillStyle=g;ctx.fillRect(x-sz,baseY-sz,sz*2,sz*2);
  }
}
function vignette(){
  const g=ctx.createRadialGradient(W*.5,H*.57,Math.min(W,H)*.22,W*.5,H*.56,Math.max(W,H)*.8);
  g.addColorStop(0,'rgba(0,0,0,0)');g.addColorStop(1,'rgba(0,0,0,.65)');ctx.fillStyle=g;ctx.fillRect(0,0,W,H);
  ctx.fillStyle='#0d1014';ctx.fillRect(0,0,W,2);
}
function render(){
  if(!W||!H)return;
  ctx.clearRect(0,0,W,H);sky();ground();pines();
  if(game.phase==='choice')choiceScene();
  else if(game.phase==='combat'||game.phase==='victory'||game.phase==='defeat')enemies();
  squad();vignette();
}
function notify(text){$('toast').textContent=text;$('toast').classList.remove('hidden');toastTime=2.0;}
function select(side){
  if(game.select(side)){updateUI(true);}
}
function changeResult(){
  if(resultShown||!['victory','defeat'].includes(game.phase))return;
  resultShown=true;
  const won=game.phase==='victory';
  $('result-rune').textContent=won?'✦':'✕';
  $('result-kicker').textContent=won?'EXPEDICIÓN COMPLETADA':'EXPEDICIÓN INTERRUMPIDA';
  $('result-heading').textContent=won?'Victoria':'Derrota';
  $('result-description').textContent=won?'El destacamento ha despejado la ruta hasta Valoria.':game.reason;
  $('result-survivors').textContent=game.soldiers;
  $('result-time').textContent=formatTime(game.time);
  $('result-stages').textContent=game.roundIndex+1+'/3';
  $('reward').classList.toggle('hidden',!won);
  $('result').classList.remove('hidden');
  $('again').focus();
}
function formatTime(s){const n=Math.floor(Math.max(0,s));return String(Math.floor(n/60)).padStart(2,'0')+':'+String(n%60).padStart(2,'0');}
function updateUI(force=false){
  if(!force&&frame-lastDom<7)return;
  lastDom=frame;
  const s=game.snapshot();
  $('chapter').textContent=s.stage+' / III';
  $('soldiers').textContent=s.soldiers;
  $('attack').textContent=Math.round(s.attackDPS);
  $('fallen').textContent=s.lost;
  $('elapsed').textContent=formatTime(s.time);
  $('zone').innerHTML='<span>TRAMO '+s.stage+'</span> <strong>'+s.location+'</strong>';
  const choosing=s.phase==='choice';
  $('choice-hud').classList.toggle('hidden',!choosing);
  $('enemy-hud').classList.toggle('hidden',s.phase!=='combat');
  if(choosing){
    $('gate-time').textContent=Math.ceil(s.choiceLeft);
    const gates=s.gates;
    $('left-icon').textContent=gates[0].icon;$('right-icon').textContent=gates[1].icon;
    $('left-title').textContent=gates[0].name;$('right-title').textContent=gates[1].name;
    $('left-detail').textContent=gates[0].detail;$('right-detail').textContent=gates[1].detail;
    $('gate-left').classList.toggle('selected',s.lane===0);
    $('gate-right').classList.toggle('selected',s.lane===1);
    $('phase-info').textContent='Elige antes de cruzar la puerta.';
  }else if(s.phase==='combat'){
    $('enemy-label').textContent=s.enemyName;
    $('enemy-fill').style.width=Math.round(s.enemyFraction*100)+'%';
    $('phase-info').textContent='Combate automático · tu elección ya está fijada';
  }
  for(const id of ['left','right'])$(id).disabled=!choosing;
  changeResult();
}
function loop(now){
  const dt=last?Math.min((now-last)/1000,.07):0;last=now;frame++;
  if(!document.hidden){game.advance(dt);for(const msg of game.drainEvents())notify(msg);if(toastTime>0){toastTime-=dt;if(toastTime<=0)$('toast').classList.add('hidden');}}
  render();updateUI();requestAnimationFrame(loop);
}
$('start').addEventListener('click',()=>{game.start();$('intro').classList.add('hidden');$('right').focus();updateUI(true);});
$('again').addEventListener('click',()=>{game.reset();game.start();$('result').classList.add('hidden');resultShown=false;spriteX=0;toastTime=0;$('toast').classList.add('hidden');updateUI(true);});
$('left').addEventListener('click',()=>select(0));$('right').addEventListener('click',()=>select(1));
$('gate-left').addEventListener('click',()=>select(0));$('gate-right').addEventListener('click',()=>select(1));
window.addEventListener('keydown',e=>{
  if(['ArrowLeft','a','A'].includes(e.key)){select(0);e.preventDefault();}
  if(['ArrowRight','d','D'].includes(e.key)){select(1);e.preventDefault();}
});
canvas.addEventListener('pointerdown',e=>{pointerStart={x:e.clientX,y:e.clientY};try{canvas.setPointerCapture(e.pointerId)}catch{}});
canvas.addEventListener('pointerup',e=>{
  if(!pointerStart)return;
  const dx=e.clientX-pointerStart.x;
  if(Math.abs(dx)>22)select(dx>0?1:0);
  else select(e.clientX-canvas.getBoundingClientRect().left>W/2?1:0);
  pointerStart=null;
});
document.addEventListener('visibilitychange',()=>{last=0;});
window.__sendaTest={state:()=>game.snapshot(),step:(seconds)=>{let steps=Math.ceil(Math.min(180,Math.max(0,seconds))/.05);for(let i=0;i<steps;i++)game.advance(.05);updateUI(true);render();},select,reset:()=>{game.reset();game.start();resultShown=false;}};
resize();updateUI(true);requestAnimationFrame(loop);
})();
