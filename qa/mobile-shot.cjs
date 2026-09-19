const {chromium}=require('playwright');
(async()=>{
 const b=await chromium.launch({headless:true});
 const shot=async(name,state,selector)=>{
   const p=await b.newPage({viewport:{width:390,height:844},deviceScaleFactor:2});
   await p.goto('http://127.0.0.1:4173/?qa=1'); await p.waitForFunction(()=>window.ELDORIA_V022_QA);
   await p.evaluate(s=>window.ELDORIA_V022_QA.setState(s),state); await p.waitForFunction(sel=>{const el=document.querySelector(sel);return !!el&&getComputedStyle(el).display!=='none'},selector);
   await p.screenshot({path:'qa/'+name+'.png',fullPage:true}); await p.close();
 };
 await shot('mobile-review',{introSeen:true,view:'kingdom',bastionLevel:1,bastion:1,sawmill:false,wood:520,stone:360,food:0},'#app>.e22 [data-testid="kingdom"]');
 await shot('mobile-review-b10',{introSeen:true,view:'kingdom',bastionLevel:10,bastion:10,sawmill:true,sawmillLvl:10,barracks:true,barracksLvl:10,granary:true,granaryLvl:10,graniteQuarry:true,quarryLvl:10,forge:true,forgeLvl:10,lyra:true,maelis:true,trial9Done:true,troops:23},'#app>.e22 [data-testid="kingdom"]');
 await shot('mobile-review-world',{introSeen:true,view:'world',bastionLevel:9,bastion:9,sawmill:true,barracks:true,granary:true,graniteQuarry:true,forge:true,lyra:true,maelis:true,marchConfigured:true,marchSlots:['aldric','lyra'],trial9Done:false},'#app>.e22 .world');
 await b.close();
})().catch(e=>{console.error(e);process.exit(1)});