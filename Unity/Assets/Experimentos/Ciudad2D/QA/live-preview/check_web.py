#!/usr/bin/env python3
"""Prueba de render/interacción del previsualizador web; nunca certifica Unity."""
import json
import os
from pathlib import Path
from playwright.sync_api import sync_playwright

URL=os.environ.get('CITY2D_TEST_URL','http://127.0.0.1:8765/QA/live-preview/index.html')
OUT=Path(__file__).resolve().parents[1]/'evidence'
OUT.mkdir(parents=True, exist_ok=True)
report=[]
with sync_playwright() as p:
    browser=p.chromium.launch(headless=True, executable_path=os.environ.get('CITY2D_CHROMIUM_PATH') or None, args=['--no-sandbox'])
    for name,w,h in [('iphone_portrait',390,844),('iphone_landscape',844,390),('iphone_resolution',1170,2532),('desktop',1600,900)]:
        page=browser.new_page(viewport={'width':w,'height':h},device_scale_factor=1)
        errors=[];page.on('pageerror',lambda e: errors.append(str(e)))
        page.goto(URL,wait_until='load',timeout=20000)
        page.wait_for_function('window.city2dDebug.state.ready && window.city2dDebug.state.frames>3',timeout=30000)
        state=page.evaluate('window.city2dDebug.state')
        assert state['imageCount']==23 and state['buildingCount']==6 and state['decorCount']==16 and state['glowCount']==7 and state['layoutVersion']==3, state
        assert state['landscape']==(w>h),state
        page.screenshot(path=str(OUT/f'{name}-i.png'))
        page.locator('#tier5').click()
        assert page.evaluate('window.city2dDebug.state.artLevel')==5
        page.screenshot(path=str(OUT/f'{name}-v.png'))
        # The exact world point is mapped using camera state; click hit is measured by actual alpha.
        hit=None
        for wx,wy in [(460,732),(469,765),(465,795),(495,770),(490,702),(473,813)]:
            sx,sy=page.evaluate('([x,y])=>window.city2dDebug.worldToScreen(x,y)',[wx,wy])
            if 5<sx<w-5 and 52<sy<h-64:
                page.mouse.click(sx,sy)
                value=page.evaluate('window.city2dDebug.state.selected')
                if value=='bastion':hit=(wx,wy);break
        if not hit and name in ('iphone_portrait','iphone_resolution'):
            raise AssertionError('Bastión no seleccionable en retrato')
        if hit:
            assert page.locator('#name').inner_text()=='Bastión'
            page.locator('#detail').click()
            assert page.locator('#sheet.open').count()==1
            page.locator('#close').click()
        if w/h > 1.2:
            old=page.evaluate('window.city2dDebug.state.centerY')
            page.mouse.move(w*.47,h*.65)
            page.mouse.down()
            page.mouse.move(w*.48,h*.3,steps=9)
            page.mouse.up()
            newer=page.evaluate('window.city2dDebug.state.centerY')
            assert abs(newer-old)>30, (old,newer)
        oldzoom=page.evaluate('window.city2dDebug.state.zoom')
        page.mouse.move(w/2,h/2)
        page.mouse.wheel(0,-240)
        page.wait_for_timeout(120)
        z=page.evaluate('window.city2dDebug.state.zoom')
        assert z>oldzoom,(z,oldzoom)
        assert not errors,errors
        report.append({'name':name,'viewport':[w,h],'loaded':state['imageCount'],'tier_change':True,'bastion_click_hit':hit,'zoom_after_wheel':z,'errors':errors})
        page.close()
    browser.close()
(OUT/'browser-proof.json').write_text(json.dumps(report,indent=2,ensure_ascii=False),encoding='utf-8')
print(json.dumps(report,indent=2,ensure_ascii=False))
