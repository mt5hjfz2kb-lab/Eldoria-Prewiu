/* Eldoria v0.32 military medical + future operation contracts. */
(function(root,factory){const api=factory();if(typeof module==='object'&&module.exports)module.exports=api;if(root){root.ELDORIA=root.ELDORIA||{};root.ELDORIA.militaryMedical=api;}})(typeof window!=='undefined'?window:null,function(){
'use strict';
const n=v=>Math.max(0,Math.floor(Number(v)||0));
const COMBAT_POLICY=Object.freeze({
 pve:Object.freeze({canWound:true,canKill:false}),
 pvp:Object.freeze({canWound:true,canKill:true,implemented:false})
});
const emptyRoster=()=>({archer:{1:0,2:0,3:0},paladin:{},warlock:{}});
function normalizeRoster(input){
 const out=emptyRoster(),src=input&&typeof input==='object'?input:{};
 for(const type of ['archer','paladin','warlock']){
  const tiers=src[type]&&typeof src[type]==='object'?src[type]:{};
  for(const [tier,count] of Object.entries(tiers))out[type][tier]=n(count);
 }
 return out;
}
function addRoster(a,b){
 const out=normalizeRoster(a),src=normalizeRoster(b);
 for(const type of Object.keys(out))for(const [tier,count] of Object.entries(src[type]||{}))out[type][tier]=n(out[type][tier])+n(count);
 return out;
}
function total(roster){return Object.values(normalizeRoster(roster)).reduce((sum,tiers)=>sum+Object.values(tiers).reduce((a,b)=>a+n(b),0),0)}
function clampComposition(chosen,available){
 const out=emptyRoster(),c=normalizeRoster(chosen),a=normalizeRoster(available);
 for(const type of Object.keys(out))for(const [tier,count] of Object.entries(c[type]||{}))out[type][tier]=Math.min(n(count),n(a[type]?.[tier]));
 return out;
}
function normalizeState(s){
 s.woundedRoster=normalizeRoster(s.woundedRoster);
 if(s.hospitalUnlocked==null)s.hospitalUnlocked=false;
 if(s.hospitalTutorialSeen==null)s.hospitalTutorialSeen=false;
 if(s.hospitalFirstTreatmentComplete==null)s.hospitalFirstTreatmentComplete=false;
 if(!s.marchOperation||typeof s.marchOperation!=='object')s.marchOperation={mode:'solo',operationId:null,contributionId:'player-main'};
 else s.marchOperation={mode:s.marchOperation.mode==='operation'?'operation':'solo',operationId:s.marchOperation.operationId||null,contributionId:s.marchOperation.contributionId||'player-main'};
 if(!s.pvpCasualties||typeof s.pvpCasualties!=='object')s.pvpCasualties={reserved:true,total:0};
 return s;
}
function operationContribution({ownerId='player',marchId='main',heroIds=[],troops={},operationId=null}={}){
 return{ownerId,marchId,operationId,heroIds:[...heroIds],troops:normalizeRoster(troops)};
}
function pveOutcome({participated=0,playerStart=1,playerEnd=1,enemyPower=0,marchPower=1,win=true,persist=true}={}){
 const p=n(participated);if(!p||!persist)return{mode:'pve',participated:p,available:p,wounded:0,dead:0};
 const hpLoss=Math.max(0,Math.min(1,1-(Math.max(0,Number(playerEnd)||0)/Math.max(1,Number(playerStart)||1))));
 const pressure=Math.max(.5,Math.min(1.8,(Number(enemyPower)||0)/Math.max(1,Number(marchPower)||1)));
 const severity=Math.max(.02,Math.min(win?.18:.28,hpLoss*.22+pressure*.025));
 const wounded=Math.max(1,Math.min(p-1,Math.round(p*severity)));
 return{mode:'pve',participated:p,available:p-wounded,wounded,dead:0,severity:Number(severity.toFixed(3))};
}
function woundAvailableRoster(available,wounded,qty){
 const a=normalizeRoster(available),w=normalizeRoster(wounded);let left=Math.min(n(qty),total(a));
 const order=[3,2,1];
 for(const tier of order){if(!left)break;const have=n(a.archer[tier]),take=Math.min(have,left);a.archer[tier]=have-take;w.archer[tier]=n(w.archer[tier])+take;left-=take}
 return{available:a,wounded:w,moved:n(qty)-left};
}
function healWoundedRoster(available,wounded,qty=Infinity){
 const a=normalizeRoster(available),w=normalizeRoster(wounded);let left=Number.isFinite(qty)?n(qty):total(w),moved=0;
 for(const tier of [1,2,3]){if(!left)break;const have=n(w.archer[tier]),take=Math.min(have,left);w.archer[tier]=have-take;a.archer[tier]=n(a.archer[tier])+take;left-=take;moved+=take}
 return{available:a,wounded:w,moved};
}
function healingSeconds(qty){return Math.max(8,Math.min(30,n(qty)*2))}
return{COMBAT_POLICY,emptyRoster,normalizeRoster,addRoster,total,clampComposition,normalizeState,operationContribution,pveOutcome,woundAvailableRoster,healWoundedRoster,healingSeconds};
});
