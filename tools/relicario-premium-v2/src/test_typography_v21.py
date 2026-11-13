"""Chromium responsive text collision regression for Relicario V2.1; Unity not tested."""
from pathlib import Path
from playwright.sync_api import sync_playwright
import base64, json

root=Path(__file__).resolve().parent.parent
css=(root/'styles.css').read_text()+'\n'+(root/'polish.css').read_text()
scripts=(root/'card-data.js').read_text()+'\n'+(root/'stat-plaques.js').read_text()+'\n'+(root/'app.js').read_text()
sources=[('frame_common.webp','image/webp'),('frame_rare.webp','image/webp'),('frame_epic.webp','image/webp'),('frame_legendary.webp','image/webp'),('bosque_valoria_art.jpg','image/jpeg'),('icon_wood.svg','image/svg+xml'),('overlay_indestructible.svg','image/svg+xml')]
assets={'assets/'+n:'data:'+mime+';base64,'+base64.b64encode((root/'assets'/n).read_bytes()).decode() for n,mime in sources}
cases=[('gallery',1440,1120),('gallery',768,1024),('gallery',390,844),('mobile',390,844),('mobile',320,720),('mobile',430,932),('board',390,844)]
results=[]

with sync_playwright() as pw:
 browser=pw.chromium.launch(headless=True,executable_path='/usr/bin/chromium',args=['--disable-gpu','--no-sandbox'])
 for view,w,h in cases:
  page=browser.new_page(viewport={'width':w,'height':h},is_mobile=w<=430,has_touch=w<=430)
  page.set_content('<html lang=es><head><meta name=viewport content="width=device-width,initial-scale=1"><style>'+css+'</style></head><body><div id=app></div></body></html>')
  page.evaluate('(a)=>window.__ELDORIA_ASSETS__=a',assets)
  page.add_script_tag(content=scripts.replace("const view=new URLSearchParams(location.search).get('view')||'gallery';",f"const view='{view}';"))
  cards=page.locator('.relic-card')
  assert cards.count() in (1,7,8)
  boxes=cards.evaluate_all("""cards=>cards.map(card=>{
    const keys=['.relic-rarity','.stat-n','.stat-s','.relic-title','.relic-effect','.relic-effect strong','.relic-effect span'];
    const r={};for(const key of keys){const e=card.querySelector(key);if(!e){r[key]=null;continue}
    let b=e.getBoundingClientRect();r[key]={left:b.left,top:b.top,right:b.right,bottom:b.bottom,scrollHeight:e.scrollHeight,clientHeight:e.clientHeight,scrollWidth:e.scrollWidth,clientWidth:e.clientWidth};}
    const b=card.getBoundingClientRect();return {size:[b.width,b.height],r};
  })""")
  assert len(set((round(x['size'][0],1),round(x['size'][1],1)) for x in boxes))==1
  assert all(abs(x['size'][1]/x['size'][0]-1.6)<.01 for x in boxes)
  for box in boxes:
   rr=box['r']
   for lhs,rhs in [('.relic-rarity','.stat-n'),('.stat-s','.relic-title'),('.relic-title','.relic-effect'),('.relic-effect strong','.relic-effect span')]:
    a,b=rr[lhs],rr[rhs]
    if a and b:
     dx=max(0,min(a['right'],b['right'])-max(a['left'],b['left']))
     dy=max(0,min(a['bottom'],b['bottom'])-max(a['top'],b['top']))
     assert dx*dy<=1,(view,w,lhs,rhs)
   for key in ('.relic-title','.relic-effect strong','.relic-effect span'):
    r=rr[key]
    if r: assert r['scrollHeight']<=r['clientHeight']+1 and r['scrollWidth']<=r['clientWidth']+1,(view,w,key)
  assert page.evaluate('document.documentElement.scrollWidth<=innerWidth+1')
  if view=='mobile':
   before=cards.first.bounding_box()
   for rarity in ('common','rare','epic','legendary'):
    page.select_option('#rarity',rarity)
    for special in (False,True):
     page.locator('#indestructible').set_checked(special)
     after=page.locator('.relic-card').bounding_box()
     assert abs(after['width']-before['width'])<.15 and abs(after['height']-before['height'])<.15
     assert page.locator('.relic-effect strong').inner_text()=='AL USARLA, RECIBES'
  if view=='gallery' and w==1440:page.screenshot(path=str(root/'qa/premium_gallery_typography_v21.png'),full_page=True)
  if view=='mobile' and w==390:page.screenshot(path=str(root/'qa/premium_mobile_typography_v21.png'))
  results.append({'view':view,'viewport':[w,h],'cards':len(boxes),'overlaps':0,'clipping':0})
  page.close()
 browser.close()
(root/'qa/typography_v21.json').write_text(json.dumps({'status':'PASS','checks':results,'not_tested':['Unity','physical iPhone']},indent=2,ensure_ascii=False)+'\n')
print('PASS V2.1: 7 sizes, no DOM text overlap or clipped labels')
