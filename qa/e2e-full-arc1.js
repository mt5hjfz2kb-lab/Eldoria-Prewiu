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
  let virtualWait=0;
  if((await state()).bastionLevel!==1)throw Error('Fresh save did not start at Bastion I');

  await building('sawmill'); await waitState(()=>window.ELDORIA_V023.state().sawmill===true,9000);
  await view('world');
  for(let i=0;i<3;i++){await node('forest');await waitState(()=>!window.ELDORIA_V023.state().tasks.some(t=>t.key==='gather-forest'),10000)}
  for(let i=0;i<4;i++){await node('quarry');await waitState(()=>!window.ELDORIA_V023.state().tasks.some(t=>t.key==='gather-quarry'),10000)}
  await node('camp');await p.waitForTimeout(2800);await closeAll();if(!(await state()).camp)throw Error('Corrupt camp did not resolve');

  await view('kingdom');await building('keep');await waitState(()=>window.ELDORIA_V023.state().bastionLevel===2,14000);await closeAll();
  await building('barracks');await waitState(()=>window.ELDORIA_V023.state().barracks===true,11000);await building('barracks');if((await state()).troops<41)throw Error('Troop recruitment did not apply');

  await view('world');await node('boss');let recruit=p.locator('.e22-cinema .btn');await recruit.waitFor({state:'visible'});await recruit.tap();await closeAll();if(!(await state()).lyra)throw Error('Lyra not recruited');
  await node('boss');await p.waitForTimeout(2800);await closeAll();if(!(await state()).boss)throw Error('Fissure not defeated');

  await view('kingdom');await building('keep');await waitState(()=>window.ELDORIA_V023.state().bastionLevel===3,18000);await closeAll();
  virtualWait+=await economy(180);await building('granary');await waitState(()=>window.ELDORIA_V023.state().granary===true,12000);
  await view('world');
  for(let i=0;i<4;i++){await node('meat');await waitState(()=>!window.ELDORIA_V023.state().tasks.some(t=>t.key==='gather-meat'),10000)}
  for(const id of ['wolf','boar']){await node(id);await p.waitForTimeout(2800);await closeAll()}

  await view('kingdom');virtualWait+=await economy(600);await building('keep');await waitState(()=>window.ELDORIA_V023.state().bastionLevel===4,20000);await closeAll();
  virtualWait+=await economy(600);await building('keep');await waitState(()=>window.ELDORIA_V023.state().bastionLevel===5,22000);await closeAll();
  await building('stoneworks');let confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await waitState(()=>window.ELDORIA_V023.state().graniteQuarry===true,15000);await closeAll();

  virtualWait+=await economy(600);await building('keep');confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await waitState(()=>window.ELDORIA_V023.state().bastionLevel===6,15000);await closeAll();
  await building('forge');await waitState(()=>window.ELDORIA_V023.state().forge===true,13000);await closeAll();

  virtualWait+=await economy(600);await building('keep');confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await waitState(()=>window.ELDORIA_V023.state().bastionLevel===7,15000);await closeAll();
  await view('world');await node('devourer');await p.waitForTimeout(1900);await closeAll();if(!(await state()).aetherEmber)throw Error('Aether Ember missing');

  await view('kingdom');await building('forge');confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await closeAll();if(!(await state()).inventory.some(x=>x&&x.slot==='weapon'))throw Error('Aether Blade missing');

  virtualWait+=await economy(600);await building('keep');confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await waitState(()=>window.ELDORIA_V023.state().bastionLevel===8,15000);await closeAll();
  await view('world');await node('nareth');await p.waitForTimeout(1900);await closeAll();if(!(await state()).maelis)throw Error('Maelis missing');

  await view('kingdom');virtualWait+=await economy(600);await building('keep');confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await waitState(()=>window.ELDORIA_V023.state().bastionLevel===9,15000);await closeAll();
  await view('heroes');const march=p.locator('#eldoria-core-loop [data-march]');await march.waitFor({state:'visible'});await march.tap();const partner=p.locator('[data-partner="maelis"]');await partner.tap();await closeAll();if(!(await state()).marchConfigured)throw Error('March configuration failed');

  await view('world');await node('trial');await p.waitForTimeout(1900);await closeAll();if(!(await state()).trialWon)throw Error('March Trial failed');
  await view('kingdom');virtualWait+=await economy(600);await building('keep');confirm=p.locator('.e22-overlay .btn:visible');await confirm.last().tap();await waitState(()=>window.ELDORIA_V023.state().bastionLevel===10,15000);await closeAll();
  await view('world');await node('final');await p.waitForTimeout(1900);const end=await state();if(!end.finalWon)throw Error('Final assault failed');
  if(virtualWait>4500)throw Error('Economy requires excessive passive wait: '+virtualWait+'s');
  console.log('FULL FRESH-SAVE ARC I PASS · accelerated passive time '+virtualWait+'s');
  await b.close();
})().catch(e=>{console.error(e);process.exit(1)});
