const {chromium}=require('playwright');
const URL=process.env.ELDORIA_URL||'http://127.0.0.1:4173/playtest/?qa=1';
(async()=>{
  const b=await chromium.launch({headless:true});
  const p=await b.newPage({viewport:{width:390,height:844},isMobile:true,hasTouch:true});
  await p.goto(URL,{waitUntil:'domcontentloaded'});
  const state=()=>p.evaluate(()=>window.ELDORIA_V023.state());
  const closeAll=async()=>{let empty=0;for(let i=0;i<10&&empty<2;i++){await p.waitForTimeout(180);const x=p.locator('.e22-overlay .btn:visible');if(!await x.count()){empty++;continue}empty=0;await x.last().tap({force:true})}};
  const view=async name=>{await closeAll();const n=p.locator('#eldoria-core-loop [data-view="'+name+'"]');await n.first().click({force:true});await p.waitForTimeout(120);};
  const building=async id=>{await closeAll();const n=p.locator('#eldoria-core-loop [data-testid="building-'+id+'"]');await n.waitFor({state:'visible'});const a=p.locator('#eldoria-core-loop [data-testid="building-action-'+id+'"]');await n.tap();await a.waitFor({state:'visible',timeout:4000});await a.tap();};
  const node=async id=>{await closeAll();const n=p.locator('#eldoria-core-loop [data-testid="world-node-'+id+'"]');await n.waitFor({state:'visible'});await n.tap({force:true});const a=p.locator('#eldoria-core-loop [data-testid="world-action-'+id+'"]');await a.waitFor({state:'visible'});await a.tap({force:true});};
  const waitState=async(fn,timeout=25000)=>p.waitForFunction(fn,null,{timeout});
  const economy=async seconds=>{await p.evaluate(s=>window.ELDORIA_V023.advanceEconomy(s),seconds);return seconds};
  const upgradeProd=async(id,target)=>{for(let guard=0;guard<12;guard++){let q=await state(),lvl=(q.buildingLevels||{})[id]||0;if(lvl>=Math.min(target,q.bastionLevel))return;let need=120*lvl,stone=Math.round(need*.65);if(q.wood<need||q.stone<stone){let wr=Math.max(1,(q.buildingLevels||{}).sawmill||1),sr=Math.max(2,((q.buildingLevels||{}).stoneworks||1)*2),sec=Math.max(60,Math.ceil(Math.max((need-q.wood)/wr,(stone-q.stone)/sr)));virtualWait+=await economy(sec);continue}await building(id);await p.waitForTimeout(150)}throw Error('Production upgrade guard exceeded: '+id)};
  let virtualWait=0;
  const sweep=[];
  const checkpoint=async(label,fn,recover)=>{try{await fn();sweep.push({label,ok:true})}catch(e){let snap=null;try{snap=await state()}catch{};sweep.push({label,ok:false,error:String(e&&e.message||e),state:snap});console.error('SWEEP BLOCKER '+label+' '+String(e&&e.message||e));try{await p.screenshot({path:'qa-failure-'+label.replace(/[^a-z0-9]+/gi,'-').toLowerCase()+'.png',fullPage:true})}catch{};if(recover)await recover(snap,e);else throw e}};
  if((await state()).bastionLevel!==1)throw Error('Fresh save did not start at Bastion I');

  await building('sawmill'); await p.waitForTimeout(6500);await p.reload({waitUntil:'domcontentloaded'});await waitState(()=>window.ELDORIA_V023.state().sawmill===true,4000);
  await view('world');
  for(let i=0;i<3;i++){await node('forest');await waitState(()=>!window.ELDORIA_V023.state().tasks.some(t=>t.key==='gather-forest'),10000)}
  for(let i=0;i<4;i++){await node('quarry');await waitState(()=>!window.ELDORIA_V023.state().tasks.some(t=>t.key==='gather-quarry'),10000)}
  await node('camp');await p.waitForTimeout(2800);await closeAll();if(!(await state()).camp)throw Error('Corrupt camp did not resolve');

  await view('kingdom');await building('keep');await waitState(()=>window.ELDORIA_V023.state().bastionLevel===2,14000);await closeAll();
  await building('barracks');await waitState(()=>window.ELDORIA_V023.state().barracks===true,11000);await building('barracks');if((await state()).troops<41)throw Error('Troop recruitment did not apply');

  await view('world');await node('boss');let recruit=p.locator('.e22-cinema .btn');await recruit.waitFor({state:'visible'});await recruit.tap();await closeAll();if(!(await state()).lyra)throw Error('Lyra not recruited');
  await node('boss');await p.waitForTimeout(2800);await closeAll();if(!(await state()).boss)throw Error('Fissure not defeated');

  await view('kingdom');await building('keep');await p.waitForTimeout(14500);await p.reload({waitUntil:'domcontentloaded'});await waitState(()=>window.ELDORIA_V023.state().bastionLevel===3,4000);await closeAll();
  virtualWait+=await economy(210);await building('granary');await waitState(()=>window.ELDORIA_V023.state().granary===true,12000).catch(async e=>{throw new Error('Granary build timeout: '+JSON.stringify(await state()))});
  await view('world');
  for(let i=0;i<4;i++){await node('meat');await waitState(()=>!window.ELDORIA_V023.state().tasks.some(t=>t.key==='gather-meat'),10000)}
  for(const id of ['wolf','boar']){await node(id);await p.waitForTimeout(2800);await closeAll()}

  await view('kingdom');await upgradeProd('sawmill',3);await upgradeProd('granary',3);virtualWait+=await economy(300);await checkpoint('bastion-iv',async()=>{await building('keep');await waitState(()=>window.ELDORIA_V023.state().bastionLevel===4,20000).catch(async()=>{throw new Error('Bastion IV timeout: '+JSON.stringify(await state()))});await closeAll()},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({wood:5000,stone:5000,food:5000,bastionLevel:4}));await closeAll()});
  await upgradeProd('sawmill',4);await upgradeProd('granary',4);virtualWait+=await economy(300);await checkpoint('bastion-v',async()=>{await building('keep');await waitState(()=>window.ELDORIA_V023.state().bastionLevel===5,22000).catch(async()=>{throw new Error('Bastion V timeout: '+JSON.stringify(await state()))});await closeAll()},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({wood:5000,stone:5000,food:5000,bastionLevel:5}));await closeAll()});
  virtualWait+=await economy(180);let confirm=p.locator('.e22-overlay .btn:visible');await checkpoint('stoneworks-build',async()=>{await building('stoneworks');confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await waitState(()=>window.ELDORIA_V023.state().graniteQuarry===true,15000).catch(async()=>{throw new Error('Stoneworks timeout: '+JSON.stringify(await state()))});await closeAll()},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({wood:5000,stone:5000,food:5000,graniteQuarry:true,buildingLevels:{...window.ELDORIA_V023.state().buildingLevels,stoneworks:1}}));await closeAll()});

  await checkpoint('bastion-vi',async()=>{virtualWait+=await economy(600);await building('keep');confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await waitState(()=>window.ELDORIA_V023.state().bastionLevel===6,15000).catch(async()=>{throw new Error('Bastion VI timeout: '+JSON.stringify(await state()))});await closeAll()},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({wood:5000,stone:5000,food:5000}));await building('keep');let x=p.locator('.e22-overlay .btn:visible');if(await x.count())await x.last().tap();await waitState(()=>window.ELDORIA_V023.state().bastionLevel===6,15000);await closeAll()});
  await checkpoint('forge-build',async()=>{await building('forge');await waitState(()=>window.ELDORIA_V023.state().forge===true,13000);await closeAll()},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({wood:5000,stone:5000,food:5000,forge:true,forgeLvl:1}));await closeAll()});

  await checkpoint('bastion-vii',async()=>{virtualWait+=await economy(600);await building('keep');confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await waitState(()=>window.ELDORIA_V023.state().bastionLevel===7,15000).catch(async()=>{throw new Error('Bastion VII timeout: '+JSON.stringify(await state()))});await closeAll()},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({wood:5000,stone:5000,food:5000,bastionLevel:7}));await closeAll()});
  await checkpoint('devourer-aether',async()=>{await view('world');await node('devourer');await p.waitForTimeout(1900);await closeAll();if(!(await state()).aetherEmber)throw Error('Aether Ember missing')},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({devourerDefeated:true,aetherEmber:true,inventory:[...window.ELDORIA_V023.state().inventory,{id:'aether-ember',name:'Ascua de Éter',slot:'material'}]}));await closeAll()});

  await checkpoint('aether-blade',async()=>{await view('kingdom');await building('forge');confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await closeAll();if(!(await state()).inventory.some(x=>x&&x.slot==='weapon'))throw Error('Aether Blade missing')},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({inventory:[...window.ELDORIA_V023.state().inventory,{id:'qa-aether-blade',name:'Hoja de Éter',slot:'weapon',power:20}]}));await closeAll()});

  await checkpoint('bastion-viii',async()=>{virtualWait+=await economy(600);await building('keep');confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await waitState(()=>window.ELDORIA_V023.state().bastionLevel===8,15000).catch(async()=>{throw new Error('Bastion VIII timeout: '+JSON.stringify(await state()))});await closeAll()},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({wood:5000,stone:5000,food:5000,bastionLevel:8}));await closeAll()});
  await checkpoint('nareth-maelis',async()=>{await view('world');await node('nareth');await p.waitForTimeout(1900);await closeAll();if(!(await state()).maelis)throw Error('Maelis missing')},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({narethRescued:true,maelis:true}));await closeAll()});

  await checkpoint('bastion-ix',async()=>{await view('kingdom');virtualWait+=await economy(600);await building('keep');confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await waitState(()=>window.ELDORIA_V023.state().bastionLevel===9,15000).catch(async()=>{throw new Error('Bastion IX timeout: '+JSON.stringify(await state()))});await closeAll()},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({wood:5000,stone:5000,food:5000,bastionLevel:9}));await closeAll()});
  await checkpoint('gear-and-march',async()=>{await view('heroes');await p.evaluate(()=>{let s=window.ELDORIA_V023.state();s.inventory.push({id:'qa-old-blade',name:'Hoja Antigua',slot:'weapon',power:10},{id:'qa-new-blade',name:'Hoja Nueva',slot:'weapon',power:20});window.ELDORIA_V023.setQA(s)});await p.locator('#eldoria-core-loop [data-hero="aldric"]').tap();let eq=p.locator('.e22-overlay [data-equip]');await eq.filter({hasText:'Hoja Antigua'}).tap();await eq.filter({hasText:'Hoja Nueva'}).tap();let gearState=await state();if(gearState.equipped.aldric.weapon?.id!=='qa-new-blade'||!gearState.inventory.some(x=>x?.id==='qa-old-blade'))throw Error('Replaced gear was not returned to inventory');await p.locator('.e22-overlay [data-close]').tap();const march=p.locator('#eldoria-core-loop [data-march]');await march.waitFor({state:'visible'});await march.tap();const partner=p.locator('[data-partner="maelis"]');await partner.tap();await closeAll();if(!(await state()).marchConfigured)throw Error('March configuration failed')},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({marchConfigured:true,marchSlots:['aldric','maelis']}));await closeAll()});

  await checkpoint('march-trial',async()=>{await view('world');await node('trial');await p.waitForTimeout(1900);await closeAll();if(!(await state()).trialWon)throw Error('March Trial failed')},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({trialWon:true,marchConfigured:true}));await closeAll()});
  await checkpoint('bastion-x',async()=>{await view('kingdom');virtualWait+=await economy(600);await building('keep');confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await waitState(()=>window.ELDORIA_V023.state().bastionLevel===10,15000);await closeAll()},async()=>{await p.evaluate(()=>window.ELDORIA_V023.setQA({wood:5000,stone:5000,food:5000,bastionLevel:10}));await closeAll()});
  await checkpoint('final-assault',async()=>{await view('world');await node('final');const aldric=p.locator('.aldric-cinematic .aldric-continue');await aldric.waitFor({state:'attached',timeout:5000});await p.waitForFunction(()=>document.querySelector('.aldric-cinematic .aldric-dialogue')?.classList.contains('done'),null,{timeout:15000});await aldric.click({force:true});await p.waitForTimeout(2200);const end=await state();if(!end.finalWon)throw Error('Final assault failed')});
  if(virtualWait>4500)sweep.push({label:'economy-pacing',ok:false,error:'Economy requires excessive passive wait: '+virtualWait+'s'});
  const blockers=sweep.filter(x=>!x.ok);
  console.log((blockers.length?'FULL ARC I SWEEP COMPLETE WITH '+blockers.length+' BLOCKER(S)':'FULL FRESH-SAVE ARC I PASS')+' · accelerated passive time '+virtualWait+'s');
  console.log('SWEEP SUMMARY '+JSON.stringify(sweep));
  if(blockers.length)process.exitCode=2;
  await b.close();
})().catch(e=>{console.error(e);process.exit(1)});
