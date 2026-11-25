/* Interactive visual prototype of La Senda de la Brecha. All art procedural, no remote assets. */
(function () {
 'use strict';
 const R=window.SendaRules;
 if(!R) throw Error('Falta el motor de decisiones de La Senda');
 const $=id=>document.getElementById(id);
 const canvas=$('battlefield'),ctx=canvas.getContext('2d',{alpha:false});
 const startOverlay=$('startOverlay'),pauseOverlay=$('pauseOverlay'),endOverlay=$('endOverlay');
 const laneButtons=[...document.querySelectorAll('.lane')];
 let run=R.initialState(),selectedLane=1,elapsed=0,stepTime=9.6,viewStage=0;
 let running=false,paused=false,locked=false,resolved=false,notification='',notificationTimer=0;
 let animationTime=0,lastFrame=0,canvasSize={w:390,h:844,dpr:1},pointerHeld=false;
 const ATK_SYMBOL={recruit:'♟',power:'⚔',ward:'◇',sabotage:'✣'};
 function clamp(v,min,max){return Math.max(min,Math.min(max,v))}
 function rand(i){const n=Math.sin(i*127.1+91.7)*43758.5453;return n-Math.floor(n)}
 function show(element,visible){element.classList.toggle('hidden',!visible)}
 function resize(){const r=canvas.getBoundingClientRect();const dpr=Math.min(window.devicePixelRatio||1,2);canvas.width=Math.round(r.width*dpr);canvas.height=Math.round(r.height*dpr);ctx.setTransform(dpr,0,0,dpr,0,0);canvasSize={w:r.width,h:r.height,dpr};}
 function updateStats(){
   $('troops').textContent=run.units;$('attack').textContent='+'+run.attackBonus+'%';$('shield').textContent=run.shields;
   $('zoneLabel').textContent=R.WAVES[Math.min(viewStage,5)].name;
   $('roundLabel').textContent='TRAMO '+Math.min(viewStage+1,6)+' / 6';
   const bars=[...$('progressTrack').children];bars.forEach((b,i)=>b.className=i<run.stage?'done':i===run.stage?'current':'');
 }
 function makeCards(){
   const wave=R.WAVES[Math.min(run.stage,5)];
   $('enemyCount').textContent=wave.enemy;
   $('gates').replaceChildren();
   wave.gates.forEach((g,i)=>{
     const b=document.createElement('button');b.type='button';b.className='gate';b.dataset.type=g.type;b.dataset.lane=String(i);
     b.setAttribute('aria-label',`Carril ${i+1}: ${R.TYPES[g.type]} ${g.title} ${g.hint}`);
     const symbol=document.createElement('span');symbol.className='g-symbol';symbol.textContent=ATK_SYMBOL[g.type];
     const strong=document.createElement('strong');strong.textContent=g.title;
     const label=document.createElement('span');label.className='g-type';label.textContent=R.TYPES[g.type];
     const hint=document.createElement('span');hint.className='g-hint';hint.textContent=g.hint;
     b.append(symbol,strong,label,hint);b.addEventListener('click',()=>choose(i));$('gates').append(b);
   });
   updateSelection();updateStats();
 }
 function updateSelection(){
   laneButtons.forEach((b,i)=>{b.classList.toggle('selected',i===selectedLane);b.setAttribute('aria-pressed',i===selectedLane?'true':'false');b.disabled=locked||!running||paused});
   [...$('gates').children].forEach((b,i)=>{b.classList.toggle('selected',i===selectedLane);b.disabled=locked||!running||paused;});
 }
 function choose(lane){if(!running||paused||locked||!Number.isInteger(lane)||lane<0||lane>2)return false;
   selectedLane=lane;updateSelection();announce(`Carril ${lane+1} seleccionado`,0.75);return true;}
 function announce(text,seconds=2){notification=text;notificationTimer=seconds;$('combatMessage').textContent=text;$('combatMessage').classList.add('show')}
 function start(){run=R.initialState();selectedLane=1;elapsed=0;viewStage=0;animationTime=0;lastFrame=performance.now();running=true;paused=false;locked=false;resolved=false;notification='';notificationTimer=0;
   show(startOverlay,false);show(pauseOverlay,false);show(endOverlay,false);$('pause').textContent='Ⅱ';$('pause').setAttribute('aria-label','Pausar partida');$('gateHeader').classList.remove('hidden-stage');$('instruction').textContent='ELIGE TU PORTAL';$('tip').textContent='DESLIZA O TOCA UN CARRIL PARA ELEGIR';
   makeCards();updateSelection();}
 function pause(){if(!running||run.finished)return;paused=!paused;show(pauseOverlay,paused);$('pause').textContent=paused?'▶':'Ⅱ';$('pause').setAttribute('aria-label',paused?'Reanudar partida':'Pausar partida');updateSelection()}
 function finish(){running=false;paused=false;locked=true;updateSelection();
   const s=R.summary(run);
   $('endTitle').textContent=s.victory?'¡BRECHA SELLADA!':'EXPEDICIÓN CAÍDA';
   $('endLabel').textContent=s.victory?'VICTORIA · GRADO '+s.grade:'DERROTA · VUELVE A INTENTARLO';
   $('endBody').textContent=s.victory?'Tu guardia ha derrotado al Guardián. Los supervivientes regresan a Valoria.':'La corrupción ha detenido a tu guardia. Prueba otra combinación de portales.';
   $('endCrest').querySelector('span').textContent=s.victory?'✦':'⚔';
   $('resultStages').textContent=s.wavesCleared+'/6';$('resultTroops').textContent=s.units;$('resultScore').textContent=s.score.toLocaleString('es-ES');
   $('resultHistory').replaceChildren();run.history.forEach(r=>{
     const item=document.createElement('div'),l=document.createElement('span'),v=document.createElement('span');
     l.textContent=r.stage+'. '+r.zone;
     v.textContent=(r.survived?'SUPERADO':'CAÍDA')+' · −'+r.lost+' tropas';v.className=r.survived?'win':'lost';
     item.append(l,v);$('resultHistory').append(item);
   });show(endOverlay,true);
 }
 function advance(dt){if(!running||paused)return;
   dt=clamp(dt,0,0.2);animationTime+=dt;elapsed+=dt;
   const phase=clamp(elapsed/stepTime,0,1);
   if(notificationTimer>0){notificationTimer-=dt;if(notificationTimer<=0)$('combatMessage').classList.remove('show')}
   if(!locked && phase>=0.49){locked=true;$('gateHeader').classList.add('hidden-stage');$('tip').textContent='¡PREPARA EL ENFRENTAMIENTO!';updateSelection();}
   if(!resolved && phase>=0.76){resolved=true;
     const previous=run;
     const output=R.playWave(run,selectedLane);run=output.state;const rep=output.report;
     const bonus=R.WAVES[previous.stage].gates[selectedLane];
     announce((rep.survived?'OLEADA SUPERADA · ':'CAÍDA · ')+R.TYPES[bonus.type]+' · −'+rep.lost+' TROPAS',2.4);
     updateStats();$('encounterInfo').style.opacity='0';
   }
   if(phase>=1){
     if(run.finished){finish();return}
     elapsed=0;viewStage=run.stage;locked=false;resolved=false;$('gateHeader').classList.remove('hidden-stage');$('tip').textContent='ELIGE TU PRÓXIMO PORTAL';$('encounterInfo').style.opacity='1';
     $('instruction').textContent=run.stage===5?'EL GUARDIÁN SE ACERCA':'ELIGE TU PORTAL';
     makeCards();
   }
   $('gateTimer').firstElementChild.style.transform='scaleX('+Math.max(0,1-phase/0.49)+')';
 }
 function poly(points,fill,stroke,width=1){ctx.beginPath();points.forEach(([x,y],i)=>i?ctx.lineTo(x,y):ctx.moveTo(x,y));ctx.closePath();if(fill){ctx.fillStyle=fill;ctx.fill()}if(stroke){ctx.strokeStyle=stroke;ctx.lineWidth=width;ctx.stroke()}}
 function ellipse(x,y,rx,ry,color){ctx.beginPath();ctx.ellipse(x,y,rx,ry,0,0,Math.PI*2);ctx.fillStyle=color;ctx.fill()}
 function line(points,color,width=1){ctx.beginPath();points.forEach(([x,y],i)=>i?ctx.lineTo(x,y):ctx.moveTo(x,y));ctx.strokeStyle=color;ctx.lineWidth=width;ctx.stroke()}
 function roadHalfWidth(y){const {w,h}=canvasSize;return w*(0.19+0.34*clamp((y-h*0.18)/(h*0.78),0,1));}
 function laneX(lane,y){return canvasSize.w/2+(lane-1)*roadHalfWidth(y)*0.56}
 function drawSky(){const {w,h}=canvasSize;
   let gr=ctx.createLinearGradient(0,0,0,h);gr.addColorStop(0,'#090b18');gr.addColorStop(.34,'#192333');gr.addColorStop(.73,'#25222d');gr.addColorStop(1,'#0b111a');ctx.fillStyle=gr;ctx.fillRect(0,0,w,h);
   ellipse(w*.56,h*.19,w*.29,h*.08,'#764a7a17');ellipse(w*.56,h*.20,w*.20,h*.05,'#a27eaf0d');
   for(let i=0;i<22;i++){const x=rand(i+32)*w,y=h*.1+rand(i+211)*h*.45,sz=.5+rand(i+13)*1.6;ellipse(x,y,sz,sz,'#af88d2'+(i%3?'48':'73'))}
   for(let s=0;s<2;s++){
    let p=[[0,h*.30+s*h*.12]];for(let j=0;j<=10;j++){let x=j*w/10,y=h*(.23+s*.09)+rand(j+s*47)*h*.085;p.push([x,y])}p.push([w,h*.82],[0,h*.82]);poly(p,s===0?'#171d2a':'#22232b');
   }
   for(let i=0;i<7;i++){const x=i*w/6+rand(i*11)*17;const ty=h*(.28+rand(i*23)*.15),size=8+rand(i*9)*25;
     poly([[x-size*.65,ty+size],[x-size*.52,ty],[x-size*.2,ty],[x-size*.1,ty-size*.7],[x+size*.08,ty-size*.85],[x+size*.1,ty],[x+size*.5,ty],[x+size*.68,ty+size]],'#101621');
     ctx.fillStyle='#5d49854a';ctx.fillRect(x-size*.06,ty-size*.38,size*.13,size*.22);
   }
 }
 function drawRoad(){const {w,h}=canvasSize,t=animationTime,top=h*.19,bottom=h*1.15;
   const hTop=roadHalfWidth(top),hBottom=roadHalfWidth(bottom);
   const roadGrad=ctx.createLinearGradient(0,top,0,bottom);roadGrad.addColorStop(0,'#37313a');roadGrad.addColorStop(.6,'#38343b');roadGrad.addColorStop(1,'#342a32');
   poly([[w/2-hTop,top],[w/2+hTop,top],[w/2+hBottom,bottom],[w/2-hBottom,bottom]],roadGrad,'#a5845d44',3);
   // Perspective seam marks, paced as if walking forward
   for(let i=0;i<31;i++){
    const z=((i/31 + t*.075)%1);const y=top+(h-top)*z*z;
    const wAt=roadHalfWidth(y);const x=w/2+(rand(i*7+14)-.5)*wAt*1.8;
    const r=1+z*7;
    poly([[x-r,y],[x-r*.6,y-r*.3],[x+r*.8,y-r*.2],[x+r,y+r*.3],[x-r*.5,y+r*.25]],i%2?'#73626a34':'#9a837b25');
   }
   for(let j=1;j<3;j++){
     const xx=(j-1.5)*2;
     for(let i=0;i<13;i++){
       const z=((i/13+t*.16)%1);const y=top+(h-top)*z*z;const half=roadHalfWidth(y);const x=w/2+xx*half*.33;
       if(y>top+12)line([[x,y],[x,y+2+z*11]],'#b69d9b35',.7+z*1.6);
     }
   }
   // Soil, stone, roots and violet fissure on the edges
   for(const side of [-1,1]){
     for(let i=0;i<22;i++){
       const z=((i/22+t*.06)%1),y=top+(h-top)*z;
       const edge=w/2+side*(roadHalfWidth(y)+4+z*12),rx=2+z*8;
       poly([[edge-side*rx,y-rx],[edge+side*rx*.8,y-rx*.55],[edge+side*rx*1.4,y+rx],[edge-side*rx*.6,y+rx*.8]],i%3?'#26202e':'#39323b','#72547a40',.7);
       if(i%4===0){line([[edge,y],[edge+side*(3+z*9),y-(6+z*15)],[edge+side*(6+z*12),y-(9+z*22)]],'#b762d0a3',1+z*2);ellipse(edge,y,1+z*2,1+z*2,'#c177e2')}
     }
   }
   // Ancient ritual fissure at the distant end
   line([[w*.50,top],[w*.48,top+20],[w*.52,top+37],[w*.49,top+52]],'#b661d6ad',2);
 }
 function drawWorldProps(){const {w,h}=canvasSize,t=animationTime;
   for(let i=0;i<18;i++){
     const z=((i/18+t*.055)%1),y=h*.23+(h*.71)*z;
     const side=i%2?-1:1,x=w/2+side*(roadHalfWidth(y)+9+rand(i+33)*38*z);
     const size=(4+z*18)*(0.72+rand(i+40)*.5);
     // corrupted standing stone with violet crystal
     poly([[x-size*.45,y],[x-size*.6,y-size*.48],[x,y-size*1.3],[x+size*.65,y-size*.55],[x+size*.4,y]],'#262634','#535364',.7);
     poly([[x-size*.2,y-size*.5],[x,y-size*1.2],[x+size*.25,y-size*.5],[x,y-size*.13]],i%3?'#7f6aa18f':'#b35cbaaa');
     ellipse(x,y-size*.5,size*.5,size*.38,'#a260a01a');
   }
 }
 function drawPortal(x,y,scale,selected,kind){ctx.save();ctx.translate(x,y);ctx.scale(scale,scale);
   const c={recruit:'#dfc38e',power:'#e59b7b',ward:'#bc8ee5',sabotage:'#8fd1bc'}[kind]||'#a98ac9';
   ellipse(0,3,27,10,selected?'#e3b77844':'#8b6bac29');
   ctx.shadowColor=c;ctx.shadowBlur=selected?22:8;
   line([[-17,0],[-18,-37],[-11,-56],[0,-63],[11,-56],[18,-37],[17,0]],c,selected?4:2.5);
   ctx.shadowBlur=0;line([[-9,-4],[-9,-39],[0,-48],[9,-39],[9,-4]],'#b28ec4aa',1.3);
   ellipse(0,-31,selected?8:5,selected?10:7,selected?'#d9b58a90':'#7b75a955');
   ctx.restore();}
 function drawGateProps(phase){if(phase>=.6)return;const w=canvasSize.w,h=canvasSize.h,stage=Math.min(viewStage,5);
   const y=h*(.39+phase*.42);
   for(let j=0;j<3;j++)drawPortal(laneX(j,y),y,.58+phase*1.5,j===selectedLane,R.WAVES[stage].gates[j].type);
 }
 function drawSoldier(x,y,scale,side,leader=false){ctx.save();ctx.translate(x,y);ctx.scale(scale,scale);
   ellipse(1,5,8,3,'#050b13aa');
   if(side==='enemy'){
     poly([[-7,2],[-6,-11],[0,-15],[6,-11],[8,3]],'#382532','#a5586d',.8);
     poly([[-5,-8],[-8,-16],[-2,-19],[0,-15],[6,-19],[10,-13],[4,-8]],'#595061','#7a566e',.8);
     ellipse(-2,-10,1.6,1,'#f47887');ellipse(3,-10,1.6,1,'#f47887');
     ellipse(-7,-2,5,7,'#5b424d');ellipse(-7,-2,3,5,'#9b576e');
     line([[6,-4],[10,-22],[13,-24]],'#e8c6bc',1.7);
   }else{
     poly([[-7,3],[-9,-6],[0,-13],[7,-6],[8,4]],leader?'#854852':'#4a5c70','#8d7680',.7);
     poly([[-4,-8],[-5,-14],[0,-17],[5,-14],[4,-8]],leader?'#d3b073':'#a2a8a8','#d9c6af',.7);
     ellipse(-6,-2,4,6,leader?'#bc9b73':'#82929c');ellipse(-6,-2,2.4,4,leader?'#d5c293':'#b8c8c7');
     line([[7,-4],[12,-22],[11,-25]],leader?'#e8cb92':'#b6c4cb',1.7);
   }ctx.restore()}
 function drawSquad(phase){const {w,h}=canvasSize;const y=h*(h<490?.68:.724);let lane=selectedLane;
   const x=laneX(lane,y),t=animationTime;
   const visible=Math.min(run.units,22);
   for(let i=visible-1;i>=0;i--){const row=Math.floor(i/5),col=i%5;
      const sx=x+(col-2)*13+(row%2?5:0);const sy=y+row*13-18+Math.sin(t*9+i*.6)*1.4;
      drawSoldier(sx,sy,.64+(row*.025),'ally',false)}
   drawSoldier(x,y-20+Math.sin(t*9)*1.4,1.4,'ally',true);
   if(phase>=.76&&phase<.94){
      for(let i=0;i<12;i++){const px=x+(rand(i)*2-1)*40,py=y-65-rand(i+33)*34+(animationTime*80+i*7)%30;ellipse(px,py,2+rand(i+4)*2,2,'#f7b98f90')}
   }
 }
 function drawEnemies(phase){const {w,h}=canvasSize;
   if(phase<.29 || phase>.98)return;
   const y=h*(.38+(phase-.29)*.38);const x=laneX(selectedLane,y);
   const count=Math.min(15,R.WAVES[Math.min(viewStage,5)].enemy),boss=R.WAVES[Math.min(viewStage,5)].boss;
   for(let i=count-1;i>=0;i--){const row=Math.floor(i/5),col=i%5;
     drawSoldier(x+(col-2)*14+(row%2?5:0),y+row*12-10,.70,'enemy')}
   if(boss){ctx.save();ctx.shadowBlur=20;ctx.shadowColor='#ba55e8';drawSoldier(x,y-55,2.7,'enemy');ctx.restore();
      ellipse(x,y-62,24,29,'#a14cd51b');}
   if(phase>.70){for(let k=0;k<12;k++){
      const sy=y+20+rand(k+41)*35,ex=x+((rand(k+8)-.5)*80);
      line([[ex,sy],[ex+8,sy-17]],k%2?'#eab07d88':'#c66bd388',1.4);
   }}
 }
 function drawAsh(){const {w,h}=canvasSize,t=animationTime;
   for(let i=0;i<45;i++){
     const x=((rand(i*31)+t*(i%2?0.004:-0.004))%1+1)%1*w;
     const y=(((rand(i*43+70)+t*(0.009+rand(i*91)*.010))%1)+1)%1*h;
     const r=.5+rand(i+78)*2.1;ellipse(x,y,r,r, i%5?'#a38cac30':'#dfc99a45');
   }
 }
 function draw(){const {w,h}=canvasSize;if(!w||!h)return;
   const phase=elapsed/stepTime;drawSky();drawRoad();drawWorldProps();drawGateProps(phase);drawEnemies(phase);drawSquad(phase);drawAsh();
   if(phase>.72&&phase<.94){ctx.fillStyle=`rgba(216,116,172,${0.07*Math.sin((phase-.72)/.22*Math.PI)})`;ctx.fillRect(0,0,w,h)}
 }
 function frame(now){if(!lastFrame)lastFrame=now;const delta=Math.min(.05,Math.max(0,(now-lastFrame)/1000));lastFrame=now;advance(delta);draw();requestAnimationFrame(frame)}
 laneButtons.forEach(b=>b.addEventListener('click',()=>choose(Number(b.dataset.lane))));
 $('start').addEventListener('click',start);$('retry').addEventListener('click',start);$('pause').addEventListener('click',pause);$('resume').addEventListener('click',pause);$('restartFromPause').addEventListener('click',start);
 window.addEventListener('keydown',e=>{if(!running||paused||locked)return;
   if(['ArrowLeft','a','A'].includes(e.key)){e.preventDefault();choose(clamp(selectedLane-1,0,2))}
   else if(['ArrowRight','d','D'].includes(e.key)){e.preventDefault();choose(clamp(selectedLane+1,0,2))}
   else if(['1','2','3'].includes(e.key)){e.preventDefault();choose(Number(e.key)-1)}
   else if(e.key===' '){e.preventDefault();pause()}
 });
 function laneFromPointer(e){const r=canvas.getBoundingClientRect();const x=e.clientX-r.left;return clamp(Math.floor(x/r.width*3),0,2)}
 canvas.addEventListener('pointerdown',e=>{pointerHeld=true;try{canvas.setPointerCapture(e.pointerId)}catch{}choose(laneFromPointer(e))});
 canvas.addEventListener('pointermove',e=>{if(pointerHeld)choose(laneFromPointer(e))});
 canvas.addEventListener('pointerup',()=>pointerHeld=false);canvas.addEventListener('pointercancel',()=>pointerHeld=false);
 document.addEventListener('visibilitychange',()=>{if(document.hidden && running && !paused)pause();lastFrame=performance.now()});
 window.addEventListener('resize',resize);resize();
 $('progressTrack').replaceChildren(...Array.from({length:6},()=>document.createElement('span')));makeCards();
 // QA-only synchronously advances the same event state machine; unavailable in normal play.
 if(new URLSearchParams(location.search).has('qa')||window.__sendaQA)window.__sendaTest={
   start,choose, state:()=>structuredClone(run), phase:()=>elapsed/stepTime,
   advanceSeconds:seconds=>{let n=Math.ceil(seconds/.045);for(let i=0;i<n;i++)advance(seconds/n);draw();},
   forceFinish:()=>{if(run.finished)finish();},
 };
 requestAnimationFrame(frame);
})();
