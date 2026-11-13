from playwright.sync_api import sync_playwright
from pathlib import Path
import json,base64
r=Path(__file__).resolve().parent.parent
with sync_playwright() as p:
 browser=p.chromium.launch(headless=True,executable_path='/usr/bin/chromium',args=['--no-sandbox'])
 errs=[]
 def setup(view,wh):
  print('setup',view,wh,flush=True);page=browser.new_page(viewport={'width':wh[0],'height':wh[1]},is_mobile=wh[0]==390,has_touch=wh[0]==390)
  page.on('pageerror',lambda x:errs.append(str(x)))
  print('set content',flush=True);page.set_content('<!DOCTYPE html><html lang=es><head><meta name=viewport content="width=device-width,initial-scale=1"><style>'+(r/'styles.css').read_text()+(r/'polish.css').read_text()+'</style></head><body><div id=app></div></body></html>')
  assets={}
  for fn,mime in [('frame_common.webp','image/webp'),('frame_rare.webp','image/webp'),('frame_epic.webp','image/webp'),('frame_legendary.webp','image/webp'),('bosque_valoria_art.jpg','image/jpeg'),('icon_wood.svg','image/svg+xml'),('overlay_indestructible.svg','image/svg+xml')]:
   assets['assets/'+fn]='data:'+mime+';base64,'+base64.b64encode((r/'assets'/fn).read_bytes()).decode()
  print('assets loaded',sum(len(x) for x in assets.values()),flush=True);page.evaluate('(assets)=>{window.__ELDORIA_ASSETS__=assets}',assets);print('injected',flush=True)
  js=(r/'card-data.js').read_text()+'\n'+(r/'stat-plaques.js').read_text()+'\n'+(r/'app.js').read_text().replace("const view=new URLSearchParams(location.search).get('view')||'gallery';",f"const view='{view}';")
  print('add js',flush=True);page.add_script_tag(content=js);print('added',flush=True)
  print('cards',page.locator('.relic-card').count(),'errs',errs,'html',page.locator('#app').inner_html()[:320],flush=True)
  page.locator('.relic-card').first.wait_for(timeout=2500)
  page.wait_for_timeout(120)
  broken=page.locator('img').evaluate_all('(xs)=>xs.filter(x=>!x.complete||!x.naturalWidth).map(x=>x.src)')
  assert not broken,broken
  return page
 print('START',flush=True);board=setup('gallery',(1400,1240))
 cards=board.locator('.relic-card')
 assert cards.count()==8,cards.count()
 dims=cards.evaluate_all('(xs)=>xs.map(x=>{let r=x.getBoundingClientRect();return [r.width,r.height,r.height/r.width,x.dataset.rarity,x.dataset.indestructible,x.getAttribute("aria-label")]})')
 assert all(abs(a[2]-1.6)<.005 for a in dims),dims
 assert len(set((round(a[0],1),round(a[1],1))for a in dims))==1,dims
 assert all(('N 6, E 4, S 3, O 2' in a[5] and '20 minutos' in a[5]) for a in dims), 'canonical stats/effect varied'
 print('gallery screenshot',flush=True);board.screenshot(path=str(r/'qa'/'premium_gallery_8.png'),full_page=True);print('gallery done',flush=True)
 phone=setup('mobile',(390,844))
 phone.screenshot(path=str(r/'qa'/'premium_390_844_common.png'))
 overflow=phone.evaluate('document.documentElement.scrollWidth>innerWidth+1')
 assert not overflow, 'mobile overflow'
 base=phone.locator('.relic-card').bounding_box();phone.select_option('#rarity','legendary');phone.locator('#indestructible').check()
 phone.screenshot(path=str(r/'qa'/'premium_390_844_indestructible.png'))
 post=phone.locator('.relic-card').bounding_box()
 assert abs(base['width']-post['width'])<0.5 and abs(base['height']-post['height'])<0.5,'size changed on overlay'
 phone.select_option('#rarity','common');phone.locator('#indestructible').uncheck()
 boardmobile=setup('board',(390,844));boardmobile.screenshot(path=str(r/'qa'/'premium_390_844_board.png'))
 assert boardmobile.locator('.relic-card').count()==7
 assert boardmobile.locator('.board-cell').count()==9
 assert not errs, errs
 report={'status':'PASS','browser':'Chromium','gallery':{'variants':8,'allDimensionsEqual':True,'aspect':5/8,'exactCardMetadata':True},'mobile390x844':{'horizontalOverflow':False,'sameSizeWhenIndestructible':True},'board':{'cells':9,'cards':7},'limits':['No Unity PlayMode','No physical iPhone','No aesthetic approval']}
 (r/'qa'/'result.json').write_text(json.dumps(report,indent=2,ensure_ascii=False)+'\n')
 browser.close()
 print(json.dumps(report,ensure_ascii=False))
