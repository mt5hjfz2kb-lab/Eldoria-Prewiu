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
        recording = {'record_video_dir':str(OUT/'video'),'record_video_size':{'width':390,'height':844}} if os.environ.get('VALORIA_TEST_REGION')=='1' else {}
        ctx=await browser.new_context(**recording,viewport={'width':390,'height':844},has_touch=True,is_mobile=True,user_agent="Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Mobile Safari/537.36",device_scale_factor=int(os.environ.get('VALORIA_DPR','1')))
        await ctx.add_init_script(TIME_SHIFT)
        page=await ctx.new_page()
        state={}; log=[]; errors=[]; sequence=0
        world_view={}
        def console(msg):
            nonlocal state,sequence
            if 'VALORIA_JOURNEY {' in msg.text:
                try: state=json.JSONDecoder().raw_decode(msg.text.split('VALORIA_JOURNEY ',1)[1])[0]; sequence+=1
                except ValueError: pass
            elif 'VALORIA_REGION {' in msg.text:
                try: world_view.update(json.JSONDecoder().raw_decode(msg.text.split('VALORIA_REGION ',1)[1])[0])
                except ValueError: pass
            elif msg.text.startswith('ELDORIA_MEMORY_PHASE') or 'REGION1_P0_LAZY' in msg.text:
                log.append({'region_phase':msg.text[:350],'browser_seconds':round(time.monotonic(),2)})
                print(log[-1],flush=True)
                if msg.type=='error': errors.append(msg.text[:600])
            elif msg.type=='error':
                errors.append(msg.text[:600])
                log.append({'console_error':msg.text[:200],'scene':state.get('activeScene'),
                            'room':state.get('room'),'level':state.get('level')})
        page.on('response', lambda res: log.append({'http_error':res.status,'url':res.url})
                if res.status>=400 else None)
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
        async def pinch_city(start, end):
            def points(radius):
                return [{'x':195-radius,'y':225,'id':0},{'x':195+radius,'y':225,'id':1}]
            await cdp.send('Input.dispatchTouchEvent',{'type':'touchStart','touchPoints':points(start)})
            await page.wait_for_timeout(450)
            for step in range(1,13):
                await cdp.send('Input.dispatchTouchEvent',{'type':'touchMove',
                    'touchPoints':points(start+(end-start)*step/12)})
                await page.wait_for_timeout(150)
            await cdp.send('Input.dispatchTouchEvent',{'type':'touchEnd','touchPoints':[]})
            await fresh()

        def find(prefix):
            return next((t for t in state['taps'] if t['name']==prefix or prefix.endswith('*') and t['name'].startswith(prefix[:-1])),None)
        async def tap(name,scroll=True):
            target=find(name)
            if not target and scroll:
                # Reset towards the first Council row, then traverse the real
                # masked list far enough to pass accumulated reward packets.
                # A complete I-V journey can legitimately expose more than
                # twenty orders before the canonical system destinations.
                for a,b in [(430,690)]*4 + [(655,300)]*28:
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
            if not state['council']: await tap('Bastion world nameplate',False)
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
        process_session=await browser.new_browser_cdp_session()
        async def record_memory(scene):
            # Fresh Chromium instance per DPR. RSS may include shared pages.
            processes=(await process_session.send('SystemInfo.getProcessInfo'))['processInfo']
            resident_kib={}
            for process in processes:
                try:
                    status=Path('/proc',str(process['id']),'status').read_text()
                except (FileNotFoundError,PermissionError):
                    continue
                for line in status.splitlines():
                    if line.startswith('VmRSS:'):
                        kind=process['type']
                        resident_kib[kind]=resident_kib.get(kind,0)+int(line.split()[1])
                        break
            heap=await cdp.send('Runtime.getHeapUsage')
            log.append({'memory_scene':scene,'bastion':state.get('level'),
                        'chromium_process_group_rss_KiB':resident_kib,'v8_bytes':heap})
        async def travel_round_trip():
            if state['council']:
                await tap('Close state',False)
            world_view.clear()
            control=find('Visitar Región 1')
            assert control,state
            box=await page.locator('canvas').bounding_box()
            async def touch(control):
                await page.touchscreen.tap(box['x']+control['x']*box['width'],box['y']+control['y']*box['height'])
            await record_memory('ciudad-antes-del-viaje')
            before=state.copy()
            await touch(control)
            await until(lambda:bool(world_view),200)
            assert world_view['level']==before['level'] and world_view['sawmill']==before['sawmill']
            await page.wait_for_timeout(1500)
            # World entry can legitimately open the canonical narrative and,
            # after Lyra is recruited, her real hero presentation. Traverse
            # the visible action/close controls in order; never click through,
            # disable or silently skip authored progression content.
            async def finish_visible_world_prompts():
                for _ in range(64 if before["level"]==5 else 24):
                    blocker=world_view.get('blocker','')
                    if blocker:
                        sample=world_view.get('sample',0)
                        await touch({'x':world_view['blockerX'],'y':world_view['blockerY']})
                        await until(lambda:world_view.get('sample',0)>sample,15)
                        await page.wait_for_timeout(260)
                        continue
                    if world_view.get('canTravel'):
                        if before['level']==5:
                            # The real narrative presenter schedules the following
                            # introduction 1.5 s after the previous acknowledgement.
                            await page.wait_for_timeout(2350)
                            if world_view.get('blocker'): continue
                        break
                    sample=world_view.get('sample',0)
                    await until(lambda:world_view.get('sample',0)>sample,15)
                assert not world_view.get('blocker') and world_view.get('canTravel'),world_view
            await finish_visible_world_prompts()
            await record_memory('region')
            await page.screenshot(path=OUT/f'ciudad-a-region-{state["level"]}.png')
            if state['level']==5:
                # Beyond stills, verify that the real Unity world responds to
                # mobile pan and pinch after gameplay has unlocked every region
                # system. Do not invoke debug camera setters or create fake
                # scene states: these are actual two-finger browser events.
                for attempt in range(3):
                    await finish_visible_world_prompts()
                    initial_zoom=world_view['zoom']
                    # Start over open ground, clear of the city entry target.
                    await cdp.send('Input.dispatchTouchEvent',{'type':'touchStart','touchPoints':[
                        {'x':130,'y':255,'id':0},{'x':250,'y':255,'id':1}]})
                    for step in range(1,13):
                        await cdp.send('Input.dispatchTouchEvent',{'type':'touchMove','touchPoints':[
                            {'x':130+step*3,'y':255,'id':0},
                            {'x':250-step*3,'y':255,'id':1}]})
                        await page.wait_for_timeout(90)
                    await cdp.send('Input.dispatchTouchEvent',{'type':'touchEnd','touchPoints':[]})
                    released=world_view['sample']
                    await until(lambda: world_view.get('sample',0)>released and
                        (world_view.get('blocker') or world_view['zoom']>initial_zoom*1.05),30)
                    if world_view.get('blocker'):
                        log.append({'pinch_interrupted_by':world_view['blocker'],'attempt':attempt+1})
                        continue
                    zoom_after=world_view['zoom']
                    break
                else:
                    raise AssertionError(('world pinch repeatedly interrupted',world_view))
                await page.screenshot(path=OUT/'region-1-pinch-real-bastion-5.png')
                before_sample=world_view.get('sample',0)
                await cdp.send('Input.dispatchTouchEvent',{'type':'touchStart','touchPoints':[
                    {'x':190,'y':515,'id':0}]})
                for step in range(1,13):
                    await cdp.send('Input.dispatchTouchEvent',{'type':'touchMove','touchPoints':[
                        {'x':190+step*7,'y':515-step*9,'id':0}]})
                    await page.wait_for_timeout(65)
                await cdp.send('Input.dispatchTouchEvent',{'type':'touchEnd','touchPoints':[]})
                await until(lambda:world_view.get('sample',0)>before_sample,30)
                await page.screenshot(path=OUT/'region-1-pan-real-bastion-5.png')
                log.append({'world_real_touch_tour':True,'level':5,
                            'zoom_before':initial_zoom,'zoom_after':zoom_after})
            seq=sequence
            # Confirm the actual Unity scene, not merely the next city log.
            # The old city can emit a final sample as the loading curtain
            # opens. That sample is NOT proof that the world-to-city journey
            # has completed. A second entry at Bastion II exposed this false
            # positive in the archived mobile evidence.
            def city_arrived():
                return (sequence > seq and
                    state.get('activeScene')=='BastionI_Modular25D' and
                    not state.get('travelCurtainVisible', True))
            for _ in range(24):
                if city_arrived(): break
                blocker=world_view.get('blocker','')
                if blocker:
                    sample=world_view.get('sample',0)
                    await touch({'x':world_view['blockerX'],'y':world_view['blockerY']})
                    await until(lambda:city_arrived() or
                                world_view.get('sample',0)>sample,30)
                    continue
                if not world_view.get('canTravel'):
                    sample=world_view.get('sample',0)
                    await until(lambda:city_arrived() or
                                world_view.get('sample',0)>sample,30)
                    continue
                # Press the real world HUD once. During LoadSceneAsync there
                # are no world samples. Do not click repeatedly through its
                # opaque transition canvas.
                await touch(world_view)
                await until(lambda:city_arrived() or
                            bool(world_view.get('blocker')),150)
            await until(city_arrived,150)
            await page.wait_for_timeout(800)
            world_view.clear()
            assert state.get('activeScene')=='BastionI_Modular25D' and not state.get('travelCurtainVisible')
            for key in ['level','sawmill','granary','barracks','stoneworks','lyra','fissure']:
                assert state[key]==before[key],(key,before,state)
            await record_memory('ciudad-tras-regresar')
            await page.screenshot(path=OUT/f'regreso-a-valoria-{state["level"]}.png')
            log.append({'world_round_trip':True,'level':state['level']})
        async def funds(wood,stone):
            for field,need,cmd in [('wood',wood,'Gather/forest-valoria'),('stone',stone,'Gather/quarry-valoria')]:
                for _ in range(18):
                    if state[field]>=need:break
                    await elapse(301)
                    remaining = 'forestRemaining' if field=='wood' else 'quarryRemaining'
                    if os.environ.get('VALORIA_TEST_REGION')=='1' and state[remaining]<=0:
                        await travel_round_trip()
                    await order(cmd,20)
                assert state[field]>=need,(field,need,state)
        async def ascend(n,duration):
            await council();rev=state['revision']
            await tap('AdvanceBastion/bastion');await until(lambda:state['revision']>rev and state['busy'])
            log.append({'command':'AdvanceBastion/bastion','target_level':n,'revision':state['revision']})
            await reload();assert state['level']==n-1 and state['busy'],state
            await elapse(duration+1);await until(lambda:state['level']==n)
            assert state['level']==n,state
            await reload();await page.wait_for_timeout(1400);await page.screenshot(path=OUT/f'bastion-{n}.png');await council()
        try:
            await page.goto(URL,wait_until='domcontentloaded');await until(lambda:bool(state),240)
            display=await page.locator('canvas').evaluate('(c)=>({dpr:devicePixelRatio,width:c.width,height:c.height})')
            assert display['dpr']==int(os.environ.get('VALORIA_DPR','1')), display
            log.append({'display_at_launch':display})
            assert state['level']==1 and state['sawmill']==0 and state['wood']==230,state
            if os.environ.get('VALORIA_TEST_REGION')=='1':
                await until(lambda:find('Confirm your name'))
                await page.screenshot(path=OUT/'nombre-inicial.png')
                # The canonical prompt focuses its real InputField when opened.
                await page.keyboard.type('ValoriaQA',delay=100)
                await page.keyboard.press('Enter')
                await tap('Confirm your name',False)
                await until(lambda:not find('Confirm your name'))
            await page.wait_for_timeout(1800);await page.screenshot(path=OUT/'bastion-1.png')
            assert abs(state['halfWidth']-6.04)<.03, state['halfWidth']
            await pinch_city(52,85)
            await until(lambda:state['halfWidth']<5.85)
            near=state['halfWidth']
            await pinch_city(92,34)
            await until(lambda:state['halfWidth']>6.19)
            log.append({'city_entry_half_width':6.04,'pinch_in':near,'pinch_out':state['halfWidth']})
            await tap('Toggle left activities',False);assert state['drawer']
            await tap('Current real construction objective',False);assert state['sawmillPanel']
            await tap('Close sawmill',False)
            await tap('Sawmill world nameplate',False);assert state['sawmillPanel']
            assert state.get('handVisible'),state
            await page.screenshot(path=OUT/'guia-iniciar-obra.png')
            await tap('Execute canonical Build command',False)
            await until(lambda:state.get('sawmillClockVisible'),5)
            await page.screenshot(path=OUT/'temporizador-en-parcela.png')
            await elapse(10)
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
            await page.wait_for_timeout(1200)
            await page.screenshot(path=OUT/'bastion-5-completo.png')
            if os.environ.get('VALORIA_TEST_REGION')=='1':
                async def visit_room(name,back):
                    if name=='mail':
                        if state['council']: await tap('Close state',False)
                    else: await council()
                    await tap('OpenSystem/'+name)
                    await until(lambda:state.get('room')==name)
                    await page.wait_for_timeout(650)
                    await page.screenshot(path=OUT/('sistema-'+name+'.png'))
                    await tap(back,False)
                    await until(lambda:not state.get('room'))
                    log.append({'room_traversed':name,'level':state['level']})
                    return True
                await travel_round_trip()
                # Visit the functional rooms while the untouched saved city
                # camera still frames the Bastion nameplate. The artistic
                # camera tour below deliberately pans away from it.
                for name,back in [('training','Return to Valoria'),('heroes','Close heroes'),
                                  ('daily','Close tributes'),('mail','Volver')]:
                    await visit_room(name,back)
                # Exercise every additional canonical room that this exact
                # saved progression exposes. These are not invented dock
                # buttons: they remain contextual Council orders, as in main.
                for name,back in [('march','Return to Valoria'),
                                  ('vigilants','Return to actual kingdom'),
                                  ('scouts','Return to actual kingdom'),
                                  ('bag','Issue186 bag close')]:
                    await visit_room(name,back)
            # The final camera tour starts in the persisted city, with the HUD
            # closed. Pinch inward in world units, then traverse via actual
            # browser touch gestures, capturing the Unity canvas throughout.
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
            graphics_errors=[e for e in errors if 'RenderPass:' in e or
                             'EndRenderPass:' in e or 'VIGILIA_PRODUCTION_CANONICAL_SESSION_NOT_READY' in e]
            assert not graphics_errors,graphics_errors
            print('PASS',browser.version,state,flush=True)
        finally:
            (OUT/'journey.json').write_text(json.dumps({'browser':browser.version,'viewport':[390,844],'clock':'UTC elapsed by test, normal game timers','log':log,'last':state,'errors':errors},indent=2))
            await page.screenshot(path=OUT/'last.png');await ctx.close();await browser.close()
asyncio.run(main())

