/* Eldoria v0.27 — Chapters, missions, rewards and universal speedups. */
(()=>{'use strict';
const E=window.ELDORIA=window.ELDORIA||{};
const defs=()=>E.gameplay?.CHAPTERS||[];
const get=(o,p)=>String(p||'').split('.').reduce((v,k)=>v==null?undefined:v[k],o);
const blankCounters=()=>({gathered:{wood:0,stone:0,food:0},trained:0,hunts:0,wins:{spawnling:0,ashStalker:0,herald:0},speedupsUsed:0,relicDecisions:0,heroInterventions:0});
function normalizeState(s){
 if(!s.chapterProgress||typeof s.chapterProgress!=='object')s.chapterProgress={version:27,current:Math.max(1,Math.min(10,s.bastionLevel||1)),completedMissions:{},claimedChapters:{},missionRewards:{},chapterStarted:{},counters:blankCounters()};
 const p=s.chapterProgress;p.version=27;p.completedMissions=p.completedMissions||{};p.claimedChapters=p.claimedChapters||{};p.missionRewards=p.missionRewards||{};p.chapterStarted=p.chapterStarted||{};p.counters=p.counters||blankCounters();p.counters.gathered=p.counters.gathered||{wood:0,stone:0,food:0};p.counters.wins=p.counters.wins||{spawnling:0,ashStalker:0,herald:0};
 for(const k of ['trained','hunts','speedupsUsed','relicDecisions','heroInterventions'])if(!Number.isFinite(p.counters[k]))p.counters[k]=0;
 s.speedups=s.speedups&&typeof s.speedups==='object'?s.speedups:{m1:0,m5:0,m15:0};for(const k of ['m1','m5','m15'])if(!Number.isFinite(s.speedups[k]))s.speedups[k]=0;
 if(s.missionIntroSeen==null)s.missionIntroSeen=false;if(s.missionPanelOpen==null)s.missionPanelOpen=false;if(s.speedupIntroSeen==null)s.speedupIntroSeen=false;
 return s;
}
function progress(s,m,h={}){normalizeState(s);let cur=0;
 if(m.type==='flag')cur=get(s,m.path)?1:0;
 else if(m.type==='state'||m.type==='resource')cur=Number(get(s,m.path)||0);
 else if(m.type==='building')cur=Number((s.buildingLevels||{})[m.path]||0);
 else if(m.type==='counter')cur=Number(get(s.chapterProgress.counters,m.counter)||0);
 else if(m.type==='infra2')cur=['sawmill','barracks','granary'].filter(x=>Number((s.buildingLevels||{})[x]||0)>=2).length;
 else if(m.type==='expedition')cur=Number(h.marchPower?.()||0);
 else if(m.type==='totalPower')cur=Number(h.totalPower?.()||0);
 else if(m.type==='equipment')cur=(s.inventory||[]).some(x=>x&&x.slot)?1:0;
 else if(m.type==='equipped')cur=Object.values(s.equipped||{}).some(g=>g&&Object.values(g).some(Boolean))?1:0;
 else if(m.type==='relics')cur=(s.codex||[]).length+(s.cardsConsumed||[]).length;
 return{current:Math.min(cur,m.value),raw:cur,target:m.value,done:cur>=m.value};
}
function currentChapter(s){normalizeState(s);let id=Math.max(1,Math.min(10,s.chapterProgress.current||s.bastionLevel||1));return defs().find(x=>x.id===id)||defs()[0]}
function currentMission(s,h){let ch=currentChapter(s);return ch?.missions.find(m=>!s.chapterProgress.completedMissions[m.id])||null}
function rewardText(r){let a=[];if(!r)return'';if(r.wood)a.push('🌲 '+r.wood);if(r.stone)a.push('🪨 '+r.stone);if(r.food)a.push('🍖 '+r.food);if(r.power)a.push('⚔ '+r.power);if(r.speedup1)a.push('⏱ '+r.speedup1+'×1m');if(r.speedup5)a.push('⏱ '+r.speedup5+'×5m');if(r.speedup15)a.push('⏱ '+r.speedup15+'×15m');if(r.specialRelic)a.push('✦ Reliquia especial');return a.join(' · ')}
function grant(s,reward,source,api={}){if(!reward)return;normalizeState(s);
 for(const [k,v] of Object.entries(reward)){if(['wood','stone','food'].includes(k))s[k]=(s[k]||0)+v;else if(k==='power')s.power=(s.power||0)+v;
 else if(k==='speedup1'){s.speedups.m1+=v;api.record?.('speedup_received',{unit:'1m',qty:v,source})}
 else if(k==='speedup5'){s.speedups.m5+=v;api.record?.('speedup_received',{unit:'5m',qty:v,source})}
 else if(k==='speedup15'){s.speedups.m15+=v;api.record?.('speedup_received',{unit:'15m',qty:v,source})}
 else if(k==='specialRelic'&&v){let id='valoria-dawn';s.codex=s.codex||[];if(!s.codex.some(x=>x.id===id)){s.codex.push({id,name:'Alba de Valoria',rarity:'Legendaria',quality:'Indestructible',values:{N:6,E:5,S:6,O:5},copy:'Reliquia del primer arco. Conserva la memoria del reino que sobrevivió a la Brecha.'});api.record?.('relic_obtained',{id,source})}}}
}
function sync(s,h={},api={},ceremony=false){normalizeState(s);let changed=false,ch=defs().find(x=>x.id===s.chapterProgress.current)||defs()[0];if(!ch)return false;
 if(!s.chapterProgress.chapterStarted[ch.id]){s.chapterProgress.chapterStarted[ch.id]=Date.now();api.record?.('chapter_started',{chapter:ch.id,title:ch.title});changed=true}
 for(const m of ch.missions){let p=progress(s,m,h);if(p.done&&!s.chapterProgress.completedMissions[m.id]){s.chapterProgress.completedMissions[m.id]=Date.now();api.record?.('mission_completed',{chapter:ch.id,mission:m.id,title:m.title});if(!s.chapterProgress.missionRewards[m.id]){grant(s,m.reward,'mission:'+m.id,api);s.chapterProgress.missionRewards[m.id]=Date.now()}changed=true}}
 let all=ch.missions.every(m=>s.chapterProgress.completedMissions[m.id]);
 if(all&&!s.chapterProgress.claimedChapters[ch.id]){grant(s,ch.chapterReward,'chapter:'+ch.id,api);s.chapterProgress.claimedChapters[ch.id]=Date.now();api.record?.('chapter_completed',{chapter:ch.id,title:ch.title});api.record?.('chapter_reward_claimed',{chapter:ch.id,reward:ch.chapterReward});s.chapterProgress.current=Math.min(10,ch.id+1);changed=true;
   if(!api.isQA?.())setTimeout(()=>api.cinema?.('✦','CAPÍTULO COMPLETADO','<b>'+ch.title+'</b><br><br>Valoria avanza porque sus sistemas, ejército y descubrimientos empiezan a sostenerse entre sí.','RECOMPENSA · '+rewardText(ch.chapterReward),'CONTINUAR',()=>{api.save?.();api.render?.()}),80);
 }
 if(changed)api.persist?.();return changed;
}
function panel(s,h={}){normalizeState(s);let ch=currentChapter(s),m=currentMission(s,h),done=ch.missions.filter(x=>s.chapterProgress.completedMissions[x.id]).length,mp=m?progress(s,m,h):null;
 let closed='<button class="chapterCompact0267" data-chapter-toggle data-testid="chapter-compact"><small>CAPÍTULO '+ch.id+' · '+ch.title+'</small><span><b>'+done+'/'+ch.missions.length+'</b><em>'+(m?m.title:'Capítulo completado')+'</em></span>'+(mp&&!mp.done?'<i>'+Math.floor(mp.current)+' / '+mp.target+'</i>':'')+'</button>';
 if(!s.missionPanelOpen)return closed;
 let rows=ch.missions.map(x=>{let p=progress(s,x,h),ok=!!s.chapterProgress.completedMissions[x.id];return '<article class="'+(ok?'done':x===m?'current':'')+'"><span>'+(ok?'✓':'◇')+'</span><div><b>'+x.title+'</b><small>'+Math.floor(p.current)+' / '+p.target+'</small><em>'+rewardText(x.reward)+'</em></div></article>'}).join('');
 return closed+'<aside class="chapterDrawer0267" data-testid="chapter-drawer"><button class="chapterClose0267" data-chapter-toggle aria-label="Cerrar">×</button><small>CAPÍTULO '+ch.id+'</small><h2>'+ch.title+'</h2><p>'+ch.context+'</p><div class="missionList0267">'+rows+'</div><div class="speedWallet0267"><small>ACELERADORES UNIVERSALES</small><span>1m ×'+s.speedups.m1+'</span><span>5m ×'+s.speedups.m5+'</span><span>15m ×'+s.speedups.m15+'</span></div><footer><small>RECOMPENSA DE CAPÍTULO</small><b>'+rewardText(ch.chapterReward)+'</b></footer></aside>';
}
function missionTarget(s,view,h={}){let m=currentMission(s,h);if(!m?.target)return null;let t=m.target;
 if(t.kind==='node')return view==='world'?{kind:'node',id:t.id,copy:m.title}:{kind:'nav',id:'world',copy:'Ve al Mundo · '+m.title};
 if(t.kind==='poi')return view==='kingdom'?{kind:'poi',id:t.id,copy:m.title}:{kind:'nav',id:'kingdom',copy:'Vuelve a Valoria · '+m.title};
 if(t.kind==='nav')return{kind:'nav',id:t.id,copy:m.title};return null;
}
function taskControls(s,t){normalizeState(s);if(!t||/^gather-/.test(t.key))return'';let a=[];if(s.speedups.m1)a.push('<button data-speedup-task="'+t.key+'" data-speedup-unit="m1">ACELERAR 1m</button>');if(s.speedups.m5)a.push('<button data-speedup-task="'+t.key+'" data-speedup-unit="m5">5m</button>');if(s.speedups.m15)a.push('<button data-speedup-task="'+t.key+'" data-speedup-unit="m15">15m</button>');return a.length?'<div class="speedActions0267">'+a.join('')+'</div>':''}
function bind(root,s,api={}){normalizeState(s);
 root.querySelectorAll('[data-chapter-toggle]').forEach(b=>b.onclick=e=>{e.preventDefault();e.stopPropagation();s.missionPanelOpen=!s.missionPanelOpen;api.save?.();api.render?.()});
 root.querySelectorAll('[data-speedup-task]').forEach(b=>b.onclick=e=>{e.preventDefault();e.stopPropagation();applySpeedup(s,b.dataset.speedupTask,b.dataset.speedupUnit,api)});
}
function applySpeedup(s,key,unit,api={}){normalizeState(s);let task=(s.tasks||[]).find(x=>x.key===key),seconds={m1:60,m5:300,m15:900}[unit];if(!task||!seconds||/^gather-/.test(task.key)||!(s.speedups[unit]>0))return false;
 s.speedups[unit]--;task.end=Math.max(Date.now(),task.end-seconds*1000);s.chapterProgress.counters.speedupsUsed++;api.record?.('speedup_used',{task:key,unit:unit==='m1'?'1m':unit==='m5'?'5m':'15m'});
 if(!s.speedupIntroSeen){s.speedupIntroSeen=true;api.toast?.('ACELERADOR UNIVERSAL','Reduce tiempo real de construcción, mejora o entrenamiento. Esperar sigue siendo perfectamente válido.')}
 api.save?.();api.recover?.();sync(s,{marchPower:api.marchPower,totalPower:api.totalPower},api,true);api.render?.();return true;
}
function intro(s,api={}){normalizeState(s);if(s.missionIntroSeen||api.isQA?.())return;s.missionIntroSeen=true;api.save?.();setTimeout(()=>api.story?.([
 ['Sir Aldric','No reconstruiremos Valoria siguiendo órdenes sueltas. Dividiremos el camino en capítulos: objetivos reales que hagan crecer el reino y nos acerquen a entender la Brecha.'],
 ['NARRADOR','El panel de Capítulos muestra tu objetivo actual y su progreso. Tócalo para ver todas las misiones. Las acciones del Reino y del Mundo avanzan las misiones automáticamente; completar un capítulo concede una recompensa importante.']
],()=>{api.save?.();api.render?.()}),260)}
function count(s,path,amount=1){normalizeState(s);let parts=String(path).split('.'),o=s.chapterProgress.counters;for(let i=0;i<parts.length-1;i++)o=o[parts[i]]||(o[parts[i]]={});let k=parts.at(-1);o[k]=(Number(o[k])||0)+amount}
function installStyle(){if(document.getElementById('chapters-v027'))return;let st=document.createElement('style');st.id='chapters-v027';st.textContent=`
#eldoria-core-loop .chapterCompact0267{position:absolute;z-index:55;left:10px;top:58px;width:min(270px,62vw);min-height:48px;padding:7px 9px;border:1px solid #d5b86966;border-radius:8px;background:#0d1318ef;color:#eee;text-align:left;box-shadow:0 8px 20px #0008}
#eldoria-core-loop .chapterCompact0267 small{display:block;font:700 6px Arial;letter-spacing:.08em;color:#d5b869}#eldoria-core-loop .chapterCompact0267 span{display:grid;grid-template-columns:auto 1fr;gap:8px;align-items:center;margin-top:3px}#eldoria-core-loop .chapterCompact0267 span b{font:900 10px Arial;color:#f0d58c}#eldoria-core-loop .chapterCompact0267 em{font:700 8px/1.2 Arial;font-style:normal;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}#eldoria-core-loop .chapterCompact0267>i{display:block;margin-top:3px;font:700 7px Arial;color:#aeb9bd;font-style:normal}
#eldoria-core-loop .chapterDrawer0267{position:fixed;z-index:120;left:8px;top:112px;width:min(390px,88vw);max-height:68dvh;overflow-y:auto;padding:14px;border:1px solid #d5b86966;border-radius:10px;background:#0b1116f7;color:#eee;box-shadow:0 18px 45px #000d;touch-action:pan-y}
#eldoria-core-loop .chapterDrawer0267 h2{margin:3px 0 6px;font:900 16px Georgia;color:#e7cd8d}#eldoria-core-loop .chapterDrawer0267>p{font:9px/1.4 Arial;color:#b9c3c7}.chapterClose0267{position:sticky;float:right;top:0;width:34px;height:34px;border-radius:17px;border:1px solid #ffffff30;background:#121920;color:#fff;font-size:20px}
#eldoria-core-loop .missionList0267{display:grid;gap:5px;margin:10px 0}.missionList0267 article{display:grid;grid-template-columns:22px 1fr;gap:5px;padding:8px;border:1px solid #ffffff14;background:#10171c}.missionList0267 article.current{border-color:#d5b86988;background:#19170f}.missionList0267 article.done{opacity:.55}.missionList0267 article span{color:#d5b869}.missionList0267 article b,.missionList0267 article small,.missionList0267 article em{display:block}.missionList0267 article b{font:8px Arial}.missionList0267 article small{margin-top:3px;font:7px Arial;color:#9ca7ab}.missionList0267 article em{margin-top:2px;font:6px Arial;color:#b79fc3;font-style:normal}
#eldoria-core-loop .speedWallet0267{display:flex;gap:6px;flex-wrap:wrap;padding:8px;border:1px solid #8e62a444;background:#12101a}.speedWallet0267 small{width:100%;font:700 6px Arial;color:#c8a4dc}.speedWallet0267 span{font:700 8px Arial;color:#ddd}
#eldoria-core-loop .chapterDrawer0267 footer{margin-top:10px;padding:9px;border-left:3px solid #d5b869;background:#17150f}.chapterDrawer0267 footer small,.chapterDrawer0267 footer b{display:block}.chapterDrawer0267 footer small{font:6px Arial;color:#d5b869}.chapterDrawer0267 footer b{margin-top:4px;font:8px/1.35 Arial}.speedActions0267{display:flex;gap:4px;flex-wrap:wrap;margin-top:5px}.speedActions0267 button{min-height:28px;padding:4px 7px;border:1px solid #a86ac866;background:#1a1020;color:#e9d6ef;font:700 6px Arial}#eldoria-core-loop .quest{display:none!important}.missionFocus027{outline:2px solid #d5b869!important;outline-offset:3px!important}.missionGo027{width:100%;min-height:36px;margin-top:9px;border:1px solid #d5b86988;background:#231e12;color:#f0d58c;font:800 8px Arial}.missionIntro027,.speedToast027{position:fixed;z-index:190;left:12px;top:112px;width:min(330px,86vw);padding:12px;border:1px solid #d5b86977;border-radius:9px;background:#0c1217f7;color:#eee;box-shadow:0 15px 35px #000c}.missionIntro027 small,.missionIntro027 b,.speedToast027 b,.speedToast027 span{display:block}.missionIntro027 small{font:6px Arial;color:#d5b869}.missionIntro027 b{margin-top:4px;font:900 11px Georgia}.missionIntro027 p,.speedToast027 span{font:8px/1.4 Arial;color:#c6ced1}.missionIntro027 button{width:100%;min-height:34px;border:1px solid #d5b86966;background:#1b1810;color:#f0d58c}.chapterCeremony027{position:fixed;z-index:210;inset:0;display:grid;place-items:center;background:#05080bb8}.chapterCeremony027>div{width:min(420px,88vw);padding:20px;border:1px solid #d5b86988;background:#10151a;text-align:center;color:#eee}.chapterCeremony027 small,.chapterCeremony027 b{display:block}.chapterCeremony027 small{font:7px Arial;color:#d5b869}.chapterCeremony027 h2{margin:5px 0;font:900 20px Georgia}.chapterCeremony027 b{font:12px Georgia;color:#efd995}.chapterCeremony027 p{font:9px/1.4 Arial}.chapterCeremony027 button{min-height:40px;padding:0 18px;border:1px solid #d5b86988;background:#241e11;color:#f2d993}
@media(max-width:620px){#eldoria-core-loop .chapterCompact0267{left:7px;top:54px;width:min(250px,68vw);min-height:44px}#eldoria-core-loop .chapterDrawer0267{left:6px;top:104px;width:min(340px,90vw);max-height:66dvh;padding:11px}}
`;document.head.appendChild(st)}
installStyle();

let runtimeSyncing027=false,runtimeScheduled027=false,lastRenderSig027='',introTimer027=0;
function isChapterQA027(){return new URLSearchParams(location.search).get('chaptersqa')==='1'}
function isGeneralQA027(){return new URLSearchParams(location.search).get('qa')==='1'}
function runtimeBridge027(){let api=window.ELDORIA_V023;if(!api||typeof api.state!=='function'||typeof api.setQA!=='function')return null;return api}
function runtimeHelpers027(api){return{marchPower:()=>{try{return Number(api.sessionSummary().power?.expedition||0)}catch(_){return 0}},totalPower:()=>{try{return Number(api.sessionSummary().power?.total||0)}catch(_){return 0}}}}
function emit027(s,type,data={}){s.sessionLog=Array.isArray(s.sessionLog)?s.sessionLog:[];s.sessionLog.push({at:Date.now(),type,data});if(s.sessionLog.length>240)s.sessionLog=s.sessionLog.slice(-240)}
function chapterUnlocked027(s){return Math.max(1,Math.min(10,Number(s.bastionLevel)||1))}
function recomputeCurrent027(s){let max=chapterUnlocked027(s),candidate=max;for(const ch of defs().filter(x=>x.id<=max)){let all=ch.missions.every(m=>s.chapterProgress.completedMissions[m.id]);if(!all){candidate=ch.id;break}}s.chapterProgress.current=candidate}
function deriveCounters027(s){
 let p=s.chapterProgress,c=p.counters;
 c.gathered.wood=Math.max(Number(c.gathered.wood)||0,(Number(s.forest)||0)*1250);
 c.gathered.stone=Math.max(Number(c.gathered.stone)||0,(Number(s.quarry)||0)*5200);
 c.trained=Math.max(Number(c.trained)||0,Math.max(0,(Number(s.troops)||36)-36));
 c.hunts=Math.max(Number(c.hunts)||0,(Number(s.wolves)||0)+(Number(s.boars)||0));
 c.relicDecisions=Math.max(Number(c.relicDecisions)||0,Object.keys(s.cardChoices||{}).length);
 let br=s.lastBattleReport;
 if(br&&br.win){let sig=[br.enemyId,br.playerStart,br.enemyStart,br.playerEnd,br.enemyEnd,(br.rounds||[]).length].join('|');if(sig&&sig!==p.lastBattleSeen){p.lastBattleSeen=sig;if(['spawnling','ashStalker','herald'].includes(br.enemyId))c.wins[br.enemyId]=(Number(c.wins[br.enemyId])||0)+1;if((br.reasons||[]).some(x=>/habilidad/i.test(String(x))))c.heroInterventions=(Number(c.heroInterventions)||0)+1;}}
}
function ensureRelicDrops027(s){
 if(!s.codexUnlocked)return false;let changed=false,win=s.chapterProgress.counters.wins||{},drops=[
  {need:'spawnling',id:'threshold-shard',name:'Fragmento del Umbral',rarity:'Común',values:{N:2,E:3,S:2,O:1},copy:'Un fragmento que vibra cuando una Fisura cercana cambia de intensidad.'},
  {need:'ashStalker',id:'ash-claw',name:'Garra de Ceniza',rarity:'Rara',values:{N:4,E:2,S:3,O:2},copy:'La huella mineral de una criatura que aprendió a esconderse dentro de la corrupción.'}
 ];s.codex=s.codex||[];for(const d of drops){if((win[d.need]||0)>0&&!s.codex.some(x=>x.id===d.id)){s.codex.push({id:d.id,name:d.name,rarity:d.rarity,values:d.values,copy:d.copy});emit027(s,'relic_obtained',{id:d.id,source:d.need});changed=true}}return changed
}
function grantRuntime027(s,reward,source){if(!reward)return;for(const [k,v] of Object.entries(reward)){if(['wood','stone','food'].includes(k))s[k]=(s[k]||0)+v;else if(k==='power')s.power=(s.power||0)+v;else if(k==='speedup1'){s.speedups.m1+=v;emit027(s,'speedup_received',{unit:'1m',qty:v,source})}else if(k==='speedup5'){s.speedups.m5+=v;emit027(s,'speedup_received',{unit:'5m',qty:v,source})}else if(k==='speedup15'){s.speedups.m15+=v;emit027(s,'speedup_received',{unit:'15m',qty:v,source})}else if(k==='specialRelic'&&v){let id='valoria-dawn';s.codex=s.codex||[];if(!s.codex.some(x=>x.id===id)){s.codex.push({id,name:'Alba de Valoria',rarity:'Legendaria',quality:'Indestructible',values:{N:6,E:5,S:6,O:5},copy:'Reliquia del primer arco. Conserva la memoria del reino que sobrevivió a la Brecha.'});emit027(s,'relic_obtained',{id,source})}}}}
function syncRuntimeState027(api,s,showCeremony=false){
 normalizeState(s);deriveCounters027(s);let changed=false,helpers=runtimeHelpers027(api),newChapter=null;
 if(s.codexUnlocked&&!s.chapterProgress.codexLogged){s.chapterProgress.codexLogged=Date.now();emit027(s,'codex_unlocked',{chapter:7});changed=true}
 let known=new Set(s.chapterProgress.knownRelics||[]);for(const card of s.codex||[]){if(!known.has(card.id)){known.add(card.id);emit027(s,'relic_obtained',{id:card.id,source:'gameplay'});changed=true}}s.chapterProgress.knownRelics=[...known];
 if(s.duelTutorialComplete&&!s.chapterProgress.duelCompletedLogged){s.chapterProgress.duelCompletedLogged=Date.now();emit027(s,'duel_completed',{training:true});changed=true}
 if(ensureRelicDrops027(s))changed=true;
 recomputeCurrent027(s);
 for(const ch of defs()){if(ch.id>chapterUnlocked027(s))break;if(!s.chapterProgress.chapterStarted[ch.id]){s.chapterProgress.chapterStarted[ch.id]=Date.now();emit027(s,'chapter_started',{chapter:ch.id,title:ch.title});changed=true}
  for(const m of ch.missions){let pr=progress(s,m,helpers);if(pr.done&&!s.chapterProgress.completedMissions[m.id]){s.chapterProgress.completedMissions[m.id]=Date.now();emit027(s,'mission_completed',{chapter:ch.id,mission:m.id,title:m.title});if(!s.chapterProgress.missionRewards[m.id]){grantRuntime027(s,m.reward,'mission:'+m.id);s.chapterProgress.missionRewards[m.id]=Date.now()}changed=true}}
  let all=ch.missions.every(m=>s.chapterProgress.completedMissions[m.id]);if(all&&!s.chapterProgress.claimedChapters[ch.id]){grantRuntime027(s,ch.chapterReward,'chapter:'+ch.id);s.chapterProgress.claimedChapters[ch.id]=Date.now();emit027(s,'chapter_completed',{chapter:ch.id,title:ch.title});emit027(s,'chapter_reward_claimed',{chapter:ch.id,reward:ch.chapterReward});newChapter=ch;changed=true}}
 recomputeCurrent027(s);return{changed,newChapter}
}
function runtimePatch027(api,s){runtimeSyncing027=true;try{api.setQA({chapterProgress:s.chapterProgress,speedups:s.speedups,missionIntroSeen:s.missionIntroSeen,missionPanelOpen:s.missionPanelOpen,speedupIntroSeen:s.speedupIntroSeen,wood:s.wood,stone:s.stone,food:s.food,power:s.power,codex:s.codex,sessionLog:s.sessionLog,tasks:s.tasks})}finally{setTimeout(()=>runtimeSyncing027=false,0)}}
function showCeremony027(ch){if(!ch||isGeneralQA027())return;document.querySelectorAll('.chapterCeremony027').forEach(x=>x.remove());let o=document.createElement('div');o.className='chapterCeremony027';o.innerHTML='<div><small>VALORIA AVANZA</small><h2>CAPÍTULO COMPLETADO</h2><b>'+ch.title+'</b><p>'+rewardText(ch.chapterReward)+'</p><button>CONTINUAR</button></div>';document.body.appendChild(o);o.querySelector('button').onclick=()=>o.remove()}
function showMissionIntro027(api,s){if(s.missionIntroSeen||isGeneralQA027()||document.querySelector('.missionIntro027'))return;if(introTimer027)return;introTimer027=setTimeout(()=>{introTimer027=0;let o=document.createElement('div');o.className='missionIntro027';o.innerHTML='<small>SIR ALDRIC · RECONSTRUCCIÓN</small><b>Valoria se levantará por capítulos.</b><p>El objetivo compacto muestra tu siguiente misión. Tócalo para abrir el capítulo completo. Las acciones reales avanzan las misiones automáticamente y cada capítulo entrega una recompensa importante.</p><button>ENTENDIDO</button>';document.body.appendChild(o);o.querySelector('button').onclick=()=>{o.remove();let now=api.state();normalizeState(now);now.missionIntroSeen=true;runtimePatch027(api,now)}} ,550)}
function speedupRuntime027(api,s,key,unit){let seconds={m1:60,m5:300,m15:900}[unit],task=(s.tasks||[]).find(x=>x.key===key);if(!seconds||!task||/^gather-/.test(task.key)||!(s.speedups?.[unit]>0))return;s.speedups[unit]--;task.end=Math.max(Date.now()-1,task.end-seconds*1000);s.chapterProgress.counters.speedupsUsed=(Number(s.chapterProgress.counters.speedupsUsed)||0)+1;emit027(s,'speedup_used',{task:key,unit:unit==='m1'?'1m':unit==='m5'?'5m':'15m'});if(!s.speedupIntroSeen){s.speedupIntroSeen=true;setTimeout(()=>{let t=document.createElement('div');t.className='speedToast027';t.innerHTML='<b>ACELERADOR UNIVERSAL</b><span>Reduce construcción, mejoras o entrenamiento. Guardarlo también es una decisión válida.</span>';document.body.appendChild(t);setTimeout(()=>t.remove(),3200)},40)}runtimePatch027(api,s)}
function goMission027(api,s,m){if(!m?.target)return;let t=m.target,patch={missionPanelOpen:false};if(t.kind==='node'){patch.view='world';patch.selectedAction=t.id}else if(t.kind==='poi'){patch.view='kingdom';patch.selectedAction=t.id}else if(t.kind==='nav'){patch.view=t.id;patch.selectedAction=null}runtimeSyncing027=true;try{api.setQA(patch)}finally{setTimeout(()=>runtimeSyncing027=false,0)}}
function injectRuntimeUI027(api,s){let core=document.getElementById('eldoria-core-loop'),host=core?.querySelector('.e22');if(!host)return;core.querySelectorAll('[data-chapter-runtime-host]').forEach(x=>x.remove());let wrap=document.createElement('div');wrap.dataset.chapterRuntimeHost='1';wrap.innerHTML=panel(s,runtimeHelpers027(api));while(wrap.firstChild)host.appendChild(wrap.firstChild);
 host.querySelectorAll('[data-chapter-toggle]').forEach(b=>b.onclick=e=>{e.preventDefault();e.stopPropagation();let now=api.state();normalizeState(now);runtimeSyncing027=true;try{api.setQA({missionPanelOpen:!now.missionPanelOpen})}finally{setTimeout(()=>runtimeSyncing027=false,0)}});
 let m=currentMission(s,runtimeHelpers027(api)),drawer=host.querySelector('[data-testid="chapter-drawer"]');if(drawer&&m?.target){let go=document.createElement('button');go.className='missionGo027';go.textContent='IR AL OBJETIVO';go.dataset.missionGo='1';drawer.appendChild(go);go.onclick=e=>{e.preventDefault();e.stopPropagation();goMission027(api,api.state(),m)}}
 host.querySelectorAll('[data-speedup-task]').forEach(b=>b.onclick=e=>{e.preventDefault();e.stopPropagation();let now=api.state();normalizeState(now);speedupRuntime027(api,now,b.dataset.speedupTask,b.dataset.speedupUnit)});
 core.querySelectorAll('.missionFocus027').forEach(x=>x.classList.remove('missionFocus027'));let t=missionTarget(s,s.view,runtimeHelpers027(api));if(t){let el=t.kind==='node'?core.querySelector('[data-node="'+t.id+'"]'):t.kind==='poi'?core.querySelector('[data-poi="'+t.id+'"]'):core.querySelector('[data-view="'+t.id+'"]');el?.classList.add('missionFocus027')}
 // Inject speedup controls in the real queue without changing task/offline semantics.
 host.querySelectorAll('.taskchip[data-task-key]').forEach(chip=>{if(chip.querySelector('.speedActions0267'))return;let key=chip.dataset.taskKey,task=(s.tasks||[]).find(x=>x.key===key),html=taskControls(s,task);if(html)chip.insertAdjacentHTML('beforeend',html)});
 host.querySelectorAll('.taskchip [data-speedup-task]').forEach(b=>b.onclick=e=>{e.preventDefault();e.stopPropagation();let now=api.state();normalizeState(now);speedupRuntime027(api,now,b.dataset.speedupTask,b.dataset.speedupUnit)});
}
function observeDuel027(api,s){let open=!!document.querySelector('[data-testid="duel-training"],.duelFormal026');if(open&&!s.chapterProgress.duelOpenLogged){s.chapterProgress.duelOpenLogged=Date.now();emit027(s,'duel_started',{training:!s.duelTutorialComplete});return true}return false}
function runtimeTick027(){if(runtimeSyncing027)return;let api=runtimeBridge027();if(!api)return;let s;try{s=api.state()}catch(_){return}normalizeState(s);
 let qa=isGeneralQA027()&&!isChapterQA027();let res={changed:false,newChapter:null};if(!qa){res=syncRuntimeState027(api,s,true);if(observeDuel027(api,s))res.changed=true;if(res.changed)runtimePatch027(api,s)}
 injectRuntimeUI027(api,s);showMissionIntro027(api,s);if(res.newChapter)showCeremony027(res.newChapter);
}
function scheduleRuntime027(){if(runtimeScheduled027)return;runtimeScheduled027=true;setTimeout(()=>{runtimeScheduled027=false;runtimeTick027()},30)}
const obs027=new MutationObserver(scheduleRuntime027);let root027=document.getElementById('eldoria-core-loop');if(root027)obs027.observe(root027,{childList:true,subtree:true});setInterval(runtimeTick027,1000);setTimeout(runtimeTick027,60);

E.chapters={normalizeState,progress,currentChapter,currentMission,panel,missionTarget,taskControls,bind,applySpeedup,intro,sync,count,rewardText,runtimeTick:runtimeTick027};
})();