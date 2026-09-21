/* Eldoria v0.24 gameplay rules shared by runtime and behavior QA. */
(()=>{'use strict';const E=window.ELDORIA=window.ELDORIA||{};
const BASTION_COSTS={2:[450,300,0],3:[650,500,0],4:[850,700,300],5:[1100,900,500],6:[1300,1050,650],7:[1500,1200,800],8:[1750,1400,950],9:[2050,1650,1100],10:[2400,1950,1300]};
const canAfford=(s,c)=>s.wood>=c[0]&&s.stone>=c[1]&&(s.food||0)>=c[2];
E.gameplay={BASTION_COSTS,canAfford,canBastion2:s=>canAfford(s,BASTION_COSTS[2])&&!!s.camp,canBastion3:s=>canAfford(s,BASTION_COSTS[3])&&!!s.barracks&&s.troops>=41};
})();