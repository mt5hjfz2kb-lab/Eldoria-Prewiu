"""Drive the actual mobile Unity canvas. Read only the optional localhost trace."""
import asyncio, json, os, sys, time, shutil
from pathlib import Path
from playwright.async_api import async_playwright

OUT=Path(os.environ.get("VALORIA_EVIDENCE","val-qa/evidence"))
OUT.mkdir(parents=True,exist_ok=True)
URL=sys.argv[1]
TIME_SHIFT='''(() => {
  const Base=Date;
  window.addElapsed=ms=>{sessionStorage.qaElapsed=Number(sessionStorage.qaElapsed||0)+ms};
  class TestDate extends Base {
    constructor(...args){if(args.length) super(...args);else super(TestDate.now())}
    static now(){return Base.now()+Number(sessionStorage.qaElapsed||0)}
  }
  window.Date=TestDate;
})()'''
async def main():
    async with async_playwright() as pw:
        browser=await pw.chromium.launch(executable_path=shutil.which('google-chrome') or shutil.which('chromium'),args=['--no-sandbox','--disable-dev-shm-usage','--use-gl=angle','--use-angle=swiftshader','--enable-unsafe-swiftshader'])
        ctx=await browser.new_context(viewport={'width':390,'height':844},has_touch=True,is_mobile=True,user_agent="Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Mobile Safari/537.36",device_scale_factor=1)
        await ctx.add_init_script(TIME_SHIFT)
        page=await ctx.new_page()
        state={}; log=[]; errors=[]; sequence=0
        def console(msg):
            nonlocal state,sequence
            if 'VALORIA_JOURNEY {' in msg.text:
                try: state=json.JSONDecoder().raw_decode(msg.text.split('VALORIA_JOURNEY ',1)[1])[0]; sequence+=1
                except ValueError: pass
            elif msg.type=='error': errors.append(msg.text[:600])
        page.on('console',console)
        page.on('pageerror',lambda e: errors.append(str(e)))
        cdp=await ctx.new_cdp_session(page)
        async def until(test,timeout=35):
            start=time.monotonic()
            while time.monotonic()-start<timeout:
                if test(): return
                await page.wait_for_timeout(160)
            raise RuntimeError(f"timeout {state} {errors[-3:]}")
        async def fresh():
            prev=sequence
            await until(lambda:sequence>prev)
        async def elapse(seconds):
            await page.evaluate('(ms)=>addElapsed(ms)',int(seconds*1000))
            await fresh();await fresh()
        async def swipe(x1,y1,x2,y2):
            await cdp.send('Input.dispatchTouchEvent',{'type':'touchStart','touchPoints':[{'x':x1,'y':y1}]})
            for i in range(1,13):
                await cdp.send('Input.dispatchTouchEvent',{'type':'touchMove','touchPoints':[{'x':x1+(x2-x1)*i/12,'y':y1+(y2-y1)*i/12}]})
                await page.wait_for_timeout(55)
            await cdp.send('Input.dispatchTouchEvent',{'type':'touchEnd','touchPoints':[]})
            await page.wait_for_timeout(300);await fresh()
        def find(prefix):
            return next((t for t in state['taps'] if t['name']==prefix or prefix.endswith('*') and t['name'].startswith(prefix[:-1])),None)
        async def tap(name,scroll=True):
            target=find(name)
            if not target and scroll:
                for a,b in [(470,680)]*3 + [(605,516)]*12:
                    await swipe(195,a,195,b)
                    target=find(name)
                    if target: break
            if not target: raise RuntimeError(f"Missing control {name}: {state}")
            box=await page.locator('canvas').bounding_box()
            x=box['x']+box['width']*target['x'];y=box['y']+box['height']*target['y']
            await cdp.send('Input.dispatchTouchEvent',{'type':'touchStart','touchPoints':[{'x':x,'y':y}]})
            await page.wait_for_timeout(175)
            await cdp.send('Input.dispatchTouchEvent',{'type':'touchEnd','touchPoints':[]})
            await fresh();await fresh()
        async def reload():
            nonlocal state
            before=state.copy();state={};await page.reload(wait_until='domcontentloaded')
            await until(lambda:state and 'taps'in state,240)
            for key in ['level','sawmill','barracks','granary','stoneworks']:
                assert state[key]==before[key],(key,before[key],state[key])
            log.append({'reload_level':state['level'],'revision':state['revision']})
        async def council():
            if not state['council']: await tap('Inspect city status',False)
            assert state['council']
        async def order(name,seconds=15):
            await council(); rev=state['revision']
            await tap(name)
            await until(lambda:state['revision']>rev,12)
            log.append({'command':name,'revision':state['revision'],'level':state['level']})
            print(log[-1],flush=True)
            await elapse(seconds)
            for _ in range(4):
                if not state['busy']: break
                await elapse(16)
        async def funds(wood,stone):
            for field,need,cmd in [('wood',wood,'Gather/forest-valoria'),('stone',stone,'Gather/quarry-valoria')]:
                for _ in range(18):
                    if state[field]>=need:break
                    await elapse(301); await order(cmd,20)
                assert state[field]>=need,(field,need,state)
        async def ascend(n,duration):
            await council();rev=state['revision']
            await tap('AdvanceBastion/bastion');await until(lambda:state['revision']>rev and state['busy'])
            log.append({'command':'AdvanceBastion/bastion','target_level':n,'revision':state['revision']})
            await reload();assert state['level']==n-1 and state['busy'],state
            await elapse(duration+1);await until(lambda:state['level']==n)
            assert state['level']==n,state
            await reload();await page.screenshot(path=OUT/f'bastion-{n}.png');await council()
        try:
            await page.goto(URL,wait_until='domcontentloaded');await until(lambda:bool(state),240)
            assert state['level']==1 and state['sawmill']==0 and state['wood']==230,state
            await page.wait_for_timeout(1800);await page.screenshot(path=OUT/'bastion-1.png')
            await tap('Toggle left activities',False);assert state['drawer']
            await tap('Current real construction objective',False);assert state['sawmillPanel']
            await tap('Close sawmill',False);await tap('Center camera',False)
            await tap('Inspect sawmill',False);assert state['sawmillPanel']
            await tap('Execute canonical Build command',False);await elapse(10)
            assert state['sawmill']==1 and state['wood']==150,state
            await reload();assert state['wood']==150
            for cmd in ['Gather/forest-valoria','Gather/forest-valoria','Gather/quarry-valoria','Fight/corrupt-scout']: await order(cmd)
            assert state['journey'],state
            await funds(450,300);await ascend(2,120)
            await funds(1000,1000);await order('Build/barracks')
            await order('Recruit/archer:t1:20',602);await order('ConfigureMarch/march-main')
            await order('Fight/engendro-valoria')
            await funds(650,500);await ascend(3,240)
            await funds(240,180);await order('Build/granary')
            await order('InvestigateFissure/fissure-valoria',1202)
            await order('ContactLyra/fissure-valoria');await order('Fight/fissure-valoria')
            assert state['lyra'] and state['fissure'],state
            await order('Fight/hunt-wolf');await order('Fight/hunt-boar')
            await order('Gather/meat-valoria');await funds(1200,1000)
            for target in ['sawmill','barracks','granary']: await order('UpgradeBuilding/'+target)
            await funds(850,700);await ascend(4,420)
            await order('ChooseDevelopment/civil');await elapse(301);await order('Gather/meat-valoria')
            await funds(1100,900);await ascend(5,600);await funds(500,0);await order('Build/stoneworks');await reload()
            assert state['sawmill']>=2 and state['granary']>=2 and state['barracks']>=2 and state['stoneworks']==1
            await page.screenshot(path=OUT/'bastion-5-completo.png')
            # The final camera tour starts in the persisted city, with the HUD
            # closed. Pinch inward in world units, then traverse via actual
            # browser touch gestures, capturing the Unity canvas throughout.
            await tap('Center camera',False)
            await page.screenshot(path=OUT/'bastion-5-panorama.png')
            await cdp.send('Input.dispatchTouchEvent',{'type':'touchStart','touchPoints':[{'x':170,'y':360,'id':0},{'x':220,'y':360,'id':1}]})
            for i in range(1,13):
                await cdp.send('Input.dispatchTouchEvent',{'type':'touchMove','touchPoints':[{'x':170-i*3,'y':360,'id':0},{'x':220+i*3,'y':360,'id':1}]})
                await page.wait_for_timeout(65)
            await cdp.send('Input.dispatchTouchEvent',{'type':'touchEnd','touchPoints':[]})
            await swipe(190,310,188,650)
            await page.screenshot(path=OUT/'bastion-5-nucleo.png')
            for n,(x1,y1,x2,y2) in enumerate([(145,530,320,350),(160,560,325,355),(319,540,70,402),(330,525,55,400),(218,560,276,360),(140,540,300,370)]):
                await swipe(x1,y1,x2,y2)
                await page.screenshot(path=OUT/f'bastion-5-recorrido-{n}.png')
            print('PASS',browser.version,state,flush=True)
        finally:
            (OUT/'journey.json').write_text(json.dumps({'browser':browser.version,'viewport':[390,844],'clock':'UTC elapsed by test, normal game timers','log':log,'last':state,'errors':errors},indent=2))
            await page.screenshot(path=OUT/'last.png');await browser.close()
asyncio.run(main())
