
const rankKinds={TotalPower:{label:"Poder total",icon:"power",metric:"Poder",metal:"#d8b77b",field:"#3b312b",image:"hall"},Kingdom:{label:"Reino",icon:"kingdom",metric:"Poder del reino",metal:"#c6ba88",field:"#34433c",image:"hall"},Army:{label:"Ejército",icon:"army",metric:"Poder militar",metal:"#b6bac7",field:"#333c48",image:"relic"},Vigilants:{label:"Vigilantes",icon:"vigilants",metric:"Etapa superada",metal:"#d1a46e",field:"#49362d",image:"hall"},Hunters:{label:"Cazadores",icon:"hunters",metric:"Puntos de caza",metal:"#bf926a",field:"#4b342e",image:"relic"},Explorers:{label:"Exploradores",icon:"explorers",metric:"Hallazgos",metal:"#8cc8b8",field:"#27423f",image:"relic"}};
const qaNames=Array.from({length:8},(_,i)=>"Valoria QA "+String(i+1).padStart(2,"0"));
const qaScores={TotalPower:[24520,22380,20170,19000,18410,17320,16000,14120],Kingdom:[6500,6020,5810,5500,5000,4900,4200,3800],Army:[9800,9210,8900,8500,8000,7400,6900,6200],Vigilants:[25,24,23,22,20,19,17,15],Hunters:[420,389,352,310,288,260,240,210],Explorers:[4,4,4,3,3,3,2,1]};
let rankScroll=0;
function showRankDetail(kind,fixture=false){
 if(!Object.hasOwn(rankKinds,kind)||section!=="Rankings")return;
 const k=rankKinds[kind],main=document.querySelector("main"),fmt=n=>n.toLocaleString("es-ES");
 const rankRows=fixture?qaNames.map((name,i)=>({name,score:qaScores[kind][i],position:i+1})):[];
 const medal=["#e4bf73","#b7c1d2","#bf8858"];
 let body="";
 if(fixture){
  body='<div class="rank-podium">'+[1,0,2].map(i=>{const r=rankRows[i];return '<div class="rank-champion" data-place="'+r.position+'" style="--medal:'+medal[i]+'"><span class="rank-medal">'+r.position+'</span><b>'+esc(r.name)+'</b><strong>'+fmt(r.score)+'</strong><small>'+esc(k.metric)+'</small></div>'}).join('')+'</div>';
  body+='<div class="rank-ledger"><div class="rank-colhead"><span>Pos.</span><span>Jugador</span><span class="rank-score">'+esc(k.metric)+'</span></div>'+rankRows.slice(3).map(r=>'<div class="rank-player" data-position="'+r.position+'"><span class="rank-number">'+r.position+'</span><b>'+esc(r.name)+'</b><span class="rank-score">'+fmt(r.score)+'</span></div>').join('')+'</div><div class="rank-own"><div><b>Tu posición</b><small>Valoria QA 08</small></div><strong>8 · '+fmt(rankRows[7].score)+'</strong></div>';
 }else{body='<div class="rank-offline"><img src="assets/seal.png" alt=""><h2>Clasificación no disponible</h2><p>El servicio competitivo todavía no está conectado. No hay posiciones públicas que mostrar.</p></div><div class="rank-own"><div><b>Tu posición</b><small>Sin datos del servidor</small></div><strong>—</strong></div>'}
 main.innerHTML='<div class="rank-detail" data-ranking="'+kind+'" data-fixture="'+fixture+'" style="--metal:'+k.metal+';--field:'+k.field+';--rank-image:url(assets/'+k.image+'.jpg)"><button class="rank-back">Volver a clasificaciones</button><div class="rank-banner"><img class="rank-icon" src="assets/'+k.icon+'.png" alt=""><div><small>Valoria · Clasificaciones</small><h1>'+esc(k.label)+'</h1><p>'+esc(k.metric)+' · '+(fixture?'Periodo QA':'Servicio no conectado')+'</p></div></div><div class="rank-qa"><span>'+(fixture?'DATOS FICTICIOS · SOLO QA':'Vista sin servidor')+'</span><button class="rank-toggle">'+(fixture?'Ver sin servidor':'Ver ejemplo QA')+'</button></div>'+body+'<div class="rank-foot">'+(fixture?'Ejemplo de diseño con jugadores de prueba. No es una clasificación real.':'Los resultados reales se mostrarán al conectar la autoridad del juego.')+'</div></div>';
 main.querySelector(".rank-back").onclick=()=>{render();main.scrollTop=rankScroll};
 main.querySelector(".rank-toggle").onclick=()=>showRankDetail(kind,!fixture);main.scrollTop=0;
}
