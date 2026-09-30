/* Eldoria v0.24 canonical state/economy service. Pure helpers; runtime adapter remains compatible. */
(()=>{'use strict';
const E=window.ELDORIA=window.ELDORIA||{};
const clampOfflineSeconds=n=>Math.max(0,Math.min(3600,Number(n)||0));
const passiveRates=s=>{const b=s.buildingLevels||{};return{wood:s.sawmill?Math.max(1,b.sawmill||1):0,stone:s.graniteQuarry?Math.max(2,(b.stoneworks||1)*2):0,food:s.granary?Math.max(1,b.granary||1):0}};
const accrue=(s,now=Date.now())=>{const dt=clampOfflineSeconds((now-(s.lastPassiveAt||now))/1000),r=passiveRates(s);s.wood+=dt*r.wood;s.stone+=dt*r.stone;s.food+=dt*r.food;s.lastPassiveAt=now;return{seconds:dt,rates:r}};
E.economy={clampOfflineSeconds,passiveRates,accrue};
})();