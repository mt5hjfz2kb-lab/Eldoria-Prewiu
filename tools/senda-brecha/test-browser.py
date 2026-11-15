#!/usr/bin/env python3
"""Exact offline HTML browser QA with screenshots, no server, no user account."""
from playwright.sync_api import sync_playwright
from pathlib import Path
import json

html=Path(__file__).with_name('senda-brecha-standalone.html')
if not html.exists():
    raise FileNotFoundError('Run node build-standalone.js first')
qa=Path(__file__).with_name('qa')
qa.mkdir(exist_ok=True)
results=[]
with sync_playwright() as p:
    browser=p.chromium.launch(executable_path='/usr/bin/chromium',headless=True,args=['--no-sandbox'])
    for width,height in [(390,844),(844,390),(430,932)]:
        page=browser.new_page(viewport={'width':width,'height':height},device_scale_factor=1,is_mobile=(width<500),has_touch=(width<500))
        errors=[]
        page.on('pageerror',lambda e: errors.append(str(e)))
        page.set_content(html.read_text(encoding='utf8'), wait_until='load')
        page.screenshot(path=str(qa/f'intro-{width}x{height}.png'))
        assert page.locator('#start').is_visible()
        page.locator('#start').click()
        assert page.evaluate('window.__sendaTest.state().phase')=='choice'
        page.screenshot(path=str(qa/f'choice-{width}x{height}.png'))
        page.locator('#right').click()
        # Touch is also wired to the actual canvas, not just the buttons.
        if width < 500:
            page.locator('#arena').tap(position={'x':int(width*.25),'y':int(page.locator('#arena').bounding_box()['height']*.55)})
            assert page.evaluate('window.__sendaTest.state().lane')==0
            page.locator('#arena').tap(position={'x':int(width*.75),'y':int(page.locator('#arena').bounding_box()['height']*.55)})
        assert page.locator('#gate-right').get_attribute('class').find('selected') != -1
        assert page.evaluate('window.__sendaTest.state().lane')==1
        page.evaluate('window.__sendaTest.step(7)')
        assert page.evaluate('window.__sendaTest.state().phase')=='combat'
        assert page.evaluate('window.__sendaTest.state().soldiers')>=20
        page.screenshot(path=str(qa/f'combat-{width}x{height}.png'))
        # Deterministic winning path 110 with stage 1 right, 2 right, 3 left.
        page.evaluate('''() => {
          for(let i=0;i<6000;i++) {
             let s=window.__sendaTest.state();
             if(s.phase==='choice')window.__sendaTest.select(s.stage===3?0:1);
             if(s.phase==='victory'||s.phase==='defeat') break;
             window.__sendaTest.step(.05);
          }
        }''')
        outcome=page.evaluate('window.__sendaTest.state()')
        assert outcome['phase']=='victory',outcome
        assert page.locator('#result').is_visible()
        page.screenshot(path=str(qa/f'victory-{width}x{height}.png'))
        page.locator('#again').click()
        assert page.evaluate('window.__sendaTest.state().phase')=='choice'
        assert page.locator('#result').is_hidden()
        assert page.locator('#chapter').inner_text()=='1 / III'
        # Whole UI stays contained inside viewport except allowed arena clip.
        bounds=page.evaluate('''() => ({root:document.querySelector('.app').getBoundingClientRect().toJSON(), right:document.querySelector('#right').getBoundingClientRect().toJSON(), left:document.querySelector('#left').getBoundingClientRect().toJSON(), canvas:document.querySelector('#arena').getBoundingClientRect().toJSON(), vw:innerWidth,vh:innerHeight,scrollH:document.body.scrollHeight})''')
        assert bounds['root']['height']<=height+.5,bounds
        for k in ('left','right'):
          b=bounds[k]; assert b['x']>=0 and b['right']<=width+.5 and b['bottom']<=height+.5,bounds
        assert bounds['canvas']['height']>=200,bounds
        assert not errors,errors
        results.append({'viewport':f'{width}x{height}','phase':'victory','elapsed_s':round(outcome['time'],1),'survivors':outcome['soldiers'],'browser_errors':errors,'canvas_h':round(bounds['canvas']['height'])})
        # Losing path and replay from a clean reset, proving both outcomes.
        page.evaluate('window.__sendaTest.reset()')
        page.evaluate('''() => {
          for(let i=0;i<6000;i++) {
            const s=window.__sendaTest.state();
            if(s.phase==='choice')window.__sendaTest.select(s.stage===1?1:0);
            if(s.phase==='victory'||s.phase==='defeat') break;
            window.__sendaTest.step(.05);
          }
        }''')
        assert page.evaluate('window.__sendaTest.state().phase')=='defeat'
        assert page.locator('#reward').is_hidden()
        page.screenshot(path=str(qa/f'defeat-{width}x{height}.png'))
        results[-1]['defeat_path_verified']=True
        page.close()
    browser.close()
(qa/'report.json').write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps(results,ensure_ascii=False,indent=2))
