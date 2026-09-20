const {chromium}=require('playwright');
const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1';
(async()=>{const b=await chromium.launch({headless:true});const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});await p.goto(URL,{waitUntil:'domcontentloaded'});
const result=await p.evaluate(()=>{
 const E=window.ELDORIA,api=window.ELDORIA_V023,fail=[],ok=(x,m)=>{if(!x)fail.push(m)};
 ok(!!E?.economy&&!!E?.tasks&&!!E?.dialogue&&!!E?.tutorial&&!!E?.ui&&!!E?.gameplay,'module-boundaries');
 const base={wood:500,stone:350,food:0,camp:true,lyra:false,boss:false,sawmill:true,graniteQuarry:true,granary:true,buildingLevels:{sawmill:2,stoneworks:2,granary:2},lastPassiveAt:Date.now()-10000};
 const r=E.economy.passiveRates(base);ok(r.wood===2&&r.stone===4&&r.food===2,'passive-rates');
 const before=[base.wood,base.stone,base.food];E.economy.accrue(base,base.lastPassiveAt+10000);ok(base.wood===before[0]+20&&base.stone===before[1]+40&&base.food===before[2]+20,'offline-accrual');
 const t=E.tasks.create('qa','QA','sawmill',5,1000);ok(t.end===6000&&E.tasks.remaining(t,2000)===4,'timestamp-task');
 ok(E.gameplay.canBastion2({wood:450,stone:300,food:0,camp:true}),'bastion2-rule');ok(!E.gameplay.canBastion2({wood:450,stone:300,food:0,camp:false}),'bastion2-gate');
 ok(E.gameplay.canBastion3({wood:650,stone:500,food:0,lyra:true,boss:true}),'bastion3-rule');
 const a=api.assert();ok(a&&typeof a==='object','runtime-assertions');return{fail,a,version:api.version};
});console.log('BEHAVIOR REGRESSION '+JSON.stringify(result));await b.close();if(result.fail.length)process.exit(2);
})().catch(e=>{console.error(e);process.exit(1)});