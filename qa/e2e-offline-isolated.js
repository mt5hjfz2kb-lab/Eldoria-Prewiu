const {chromium}=require('playwright');
(async()=>{
 const b=await chromium.launch({headless:true});
 const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
 await p.goto(process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1',{waitUntil:'domcontentloaded'});
 const now=Date.now();
 await p.evaluate(({now})=>window.ELDORIA_V023.setQA({wood:100,forestRemain:1250,sawmill:true,lastPassiveAt:now,tasks:[{key:'gather-forest',title:'RECOLECTANDO MADERA',target:'forest',start:now-120000,end:now-60000}]}),{now});
 const pre=await p.evaluate(()=>({api:window.ELDORIA_V023.state(),raw:JSON.parse(localStorage.getItem('eldoria-v022-consistent-loop')||'{}')}));
 await p.reload({waitUntil:'domcontentloaded'});
 const post=await p.evaluate(()=>({api:window.ELDORIA_V023.state(),raw:JSON.parse(localStorage.getItem('eldoria-v022-consistent-loop')||'{}')}));
 console.log('OFFLINE ISOLATED',JSON.stringify({pre,post}));
 if(post.api.tasks.some(t=>t.key==='gather-forest'))throw Error('Expired gather remained pending');
 if(post.api.wood<=pre.api.wood)throw Error('Expired gather did not credit wood');
 if(post.api.forestRemain>=pre.api.forestRemain)throw Error('Expired gather did not reduce node');
 await b.close();
 console.log('OFFLINE ISOLATED PASS');
})().catch(e=>{console.error(e);process.exit(1)});