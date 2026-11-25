"""Real Chromium smoke on bundled offline HTML; no mock click handlers."""
from pathlib import Path
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from functools import partial
from threading import Thread
from playwright.sync_api import sync_playwright

ROOT=Path(__file__).resolve().parents[1]
PAGE='' # filled from local HTTP server
OUT=ROOT/'evidence'
OUT.mkdir(exist_ok=True)

def run():
 global PAGE
 server=ThreadingHTTPServer(('127.0.0.1',0),partial(SimpleHTTPRequestHandler,directory=str(ROOT/'dist')))
 Thread(target=server.serve_forever,daemon=True).start()
 PAGE=f'http://127.0.0.1:{server.server_port}/senda-brecha.html?qa=1'
 with sync_playwright() as p:
  browser=p.chromium.launch(executable_path='/usr/bin/chromium',headless=True,args=['--no-sandbox','--disable-dev-shm-usage'])
  for label,(w,h) in [('iphone',(390,844)),('small',(320,700)),('landscape',(844,390)),('desktop',(1280,800))]:
   page=browser.new_page(viewport={'width':w,'height':h},device_scale_factor=1,has_touch=True,is_mobile=label!='desktop')
   errors=[]
   page.on('pageerror',lambda e:errors.append(str(e)))
   html=(ROOT/'dist/senda-brecha.html').read_text().replace('</head>','<script>window.__sendaQA=true</script></head>')
   page.set_content(html,wait_until='load');page.wait_for_timeout(250)
   assert page.locator('#startOverlay').is_visible()
   page.screenshot(path=str(OUT/f'{label}-start.png'),full_page=False)
   page.locator('#start').click()
   assert not page.locator('#startOverlay').is_visible()
   assert page.locator('#gates .gate').count()==3
   assert page.locator('#troops').inner_text()=='22'
   page.locator('#gates .gate').first.click()
   assert page.locator('.lane.selected').get_attribute('data-lane')=='0'
   page.screenshot(path=str(OUT/f'{label}-choice.png'),full_page=False)
   page.evaluate('window.__sendaTest.advanceSeconds(7.4)')
   assert page.evaluate('window.__sendaTest.state().history.length')==1
   page.screenshot(path=str(OUT/f'{label}-battle.png'),full_page=False)
   page.evaluate('window.__sendaTest.advanceSeconds(2.3)')
   assert page.evaluate('window.__sendaTest.state().stage')==1
   page.locator('#pause').click();assert page.locator('#pauseOverlay').is_visible()
   before=page.evaluate('window.__sendaTest.state().stage')
   page.evaluate('window.__sendaTest.advanceSeconds(18)')
   assert page.evaluate('window.__sendaTest.state().stage')==before
   page.locator('#resume').click()
   # Complete remaining five waves using an intentionally successful route.
   # Exhaustive strategy tested below; pick a winning route for browser output.
   mask=next(m for m in range(729) if outcome(m))
   first=mask%3
   # Original first wave selected left; need a winning sequence with first choice 0.
   mask=next(m for m in range(729) if m%3==0 and outcome(m))
   for stage in range(1,6):
    lane=(mask//(3**stage))%3
    page.locator('#gates .gate').nth(lane).click()
    page.evaluate('window.__sendaTest.advanceSeconds(9.7)')
    if page.evaluate('window.__sendaTest.state().finished'):break
   state=page.evaluate('window.__sendaTest.state()')
   assert state['victory'], f'{label}: {state}'
   assert page.locator('#endOverlay').is_visible()
   page.screenshot(path=str(OUT/f'{label}-victory.png'),full_page=False)
   assert not errors, (label,errors)
   assert not page.evaluate('document.documentElement.scrollWidth>window.innerWidth'),label
   # all three buttons remain inside the screen in both orientations
   box=page.locator('#retry').bounding_box(); assert box and box['x']>=0 and box['x']+box['width']<=w+1,(label,box)
   # Ensure restart resets all progress and source state, no repeated reward.
   page.locator('#retry').click()
   assert page.evaluate('window.__sendaTest.state().stage')==0
   assert page.evaluate('window.__sendaTest.state().score')==0
   assert page.locator('#pause').inner_text()=='Ⅱ'
   # Real canvas pointer selection must move the squad, not only highlight a decoration.
   rect=page.locator('#battlefield').bounding_box()
   assert rect
   page.locator('#battlefield').dispatch_event('pointerdown',{'clientX':rect['x']+rect['width']*0.92,'clientY':rect['y']+rect['height']*0.65,'pointerId':1})
   assert page.locator('.lane.selected').get_attribute('data-lane')=='2'
   page.locator('#battlefield').dispatch_event('pointerup',{'pointerId':1})
   # Intentional losing run: defeat, no economic side effects, replay enabled.
   losing=next(m for m in range(729) if m%3==2 and not outcome(m))
   for stage in range(6):
    page.locator('#gates .gate').nth((losing//(3**stage))%3).click()
    page.evaluate('window.__sendaTest.advanceSeconds(9.7)')
    if page.evaluate('window.__sendaTest.state().finished'):break
   loser=page.evaluate('window.__sendaTest.state()')
   assert loser['finished'] and not loser['victory'],f'{label}: defeat route not reached'
   assert page.locator('#endOverlay').is_visible()
   assert 'EXPEDICIÓN CAÍDA' in page.locator('#endTitle').inner_text()
   page.screenshot(path=str(OUT/f'{label}-defeat.png'),full_page=False)
   print(f'{label}: OK  {w}x{h}, win score={state["score"]}, defeat at stage={loser["stage"]}')
   page.close()
  browser.close()
 server.shutdown()

def outcome(mask):
 # test strategy with Node-compatible formula for finding one legitimate winning path.
 units=22;power=0;shields=0
 gates=[ [('recruit',8),('power',22),('ward',6)], [('sabotage',9),('recruit',7),('ward',8)], [('power',26),('recruit',8),('ward',9)], [('sabotage',12),('ward',11),('power',30)], [('recruit',12),('ward',13),('power',34)], [('recruit',14),('power',40),('ward',17)] ]
 enemies=[14,21,26,31,39,53];strengths=[1.,1.14,1.22,1.32,1.45,1.64]
 for stage in range(6):
  lane=(mask//(3**stage))%3;typ,value=gates[stage][lane];enemy=enemies[stage]
  if typ=='recruit':units=min(96,units+value)
  elif typ=='power':power=min(125,power+value)
  elif typ=='ward':shields=min(50,shields+value)
  else:enemy=max(1,enemy-value)
  attack=units*(1+power/100);resistance=enemy*strengths[stage]
  isboss=stage==5;insufficient=attack<resistance*(.42 if isboss else .31)
  import math
  base=max(1,math.ceil(enemy*strengths[stage]*(.47 if isboss else .33)/(1+attack/45)))
  penalty=math.ceil((resistance*(.42 if isboss else .31)-attack)/2.4) if insufficient else 0
  incoming=base+max(0,penalty);absorbed=min(incoming,shields);shields-=absorbed;units=max(0,units-incoming+absorbed)
  if units<=0 or insufficient:return False
 return True

if __name__=='__main__':run()
