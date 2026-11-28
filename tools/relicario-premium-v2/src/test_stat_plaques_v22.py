"""V2.2: geometric assertions for four dynamic numbers inside four vector metallic sockets."""
from pathlib import Path
from playwright.sync_api import sync_playwright
import base64, json
root=Path(__file__).resolve().parent.parent
css=(root/'styles.css').read_text()+'\n'+(root/'polish.css').read_text()
js=(root/'card-data.js').read_text()+'\n'+(root/'stat-plaques.js').read_text()+'\n'+(root/'app.js').read_text()
kinds={'webp':'image/webp','jpg':'image/jpeg','svg':'image/svg+xml'}
assets={}
for file in (root/'assets').iterdir():
    if file.suffix.lstrip('.') in kinds:
        assets['assets/'+file.name]='data:'+kinds[file.suffix.lstrip('.') ]+';base64,'+base64.b64encode(file.read_bytes()).decode()
scenarios=[('gallery',1440,1120),('gallery',768,1024),('gallery',390,844),('mobile',390,844),('mobile',320,720),('mobile',430,932),('board',390,844)]
results=[]
with sync_playwright() as p:
 browser=p.chromium.launch(headless=True,executable_path='/usr/bin/chromium',args=['--no-sandbox'])
 for view,w,h in scenarios:
  page=browser.new_page(viewport={'width':w,'height':h},is_mobile=w<=430,has_touch=w<=430)
  page.set_content('<html><head><meta name=viewport content="width=device-width,initial-scale=1"><style>'+css+'</style></head><body><div id=app></div></body></html>')
  page.evaluate('(a)=>window.__ELDORIA_ASSETS__=a',assets)
  page.add_script_tag(content=js.replace("const view=new URLSearchParams(location.search).get('view')||'gallery';",f"const view='{view}';"))
  cards=page.locator('.relic-card')
  assert cards.count()==(7 if view=='board' else 8 if view=='gallery' else 1)
  found=cards.evaluate_all("""cards=>cards.map(c=>{
    let s=c.getBoundingClientRect();
    let plates=[...c.querySelectorAll('.relic-stats-svg .relic-stat')].map(g=>{
      let box=g.querySelector('.plaque-inner').getBBox();
      let label=g.querySelector('.compass').getBBox();
      let text=g.querySelector('.value').getBBox();
      let cx=box.x+box.width/2,center=text.x+text.width/2;
      return {dir:g.dataset.dir,value:g.querySelector('.value').textContent,
        xMargin:Math.min(label.x-box.x,text.x-box.x,box.x+box.width-(label.x+label.width),box.x+box.width-(text.x+text.width)),
        yMargin:Math.min(label.y-box.y,text.y-box.y,box.y+box.height-(label.y+label.height),box.y+box.height-(text.y+text.height)),
        centerDelta:Math.abs(cx-center)};
    });
    return {ratio:s.height/s.width,plates};
  })""")
  for card in found:
   assert abs(card['ratio']-1.6)<.01
   assert len(card['plates'])==4
   for v in card['plates']:
    assert v['xMargin']>=1.5 and v['yMargin']>=1.5 and v['centerDelta']<2,(view,w,v)
  assert page.evaluate('document.documentElement.scrollWidth<=innerWidth+1')
  if view=='mobile':
   for rarity in ('common','rare','epic','legendary'):
    page.select_option('#rarity',rarity)
    for value in (False,True):
     page.locator('#indestructible').set_checked(value)
     assert page.locator('.relic-stats-svg .relic-stat').count()==4
  if view=='gallery' and w==1440:page.screenshot(path=str(root/'qa/premium_gallery_v22.png'),full_page=True)
  if view=='mobile' and w==390:page.screenshot(path=str(root/'qa/premium_mobile_v22.png'))
  results.append({'view':view,'size':[w,h],'socketsChecked':len(found)*4,'aligned':True})
  page.close()
 # Test the longest supported value in every badge.
 page=browser.new_page(viewport={'width':390,'height':844})
 page.set_content('<html><head><meta name=viewport content="width=device-width,initial-scale=1"><style>'+css+'</style></head><body><div id=app></div></body></html>')
 page.evaluate('(a)=>window.__ELDORIA_ASSETS__=a',assets)
 page.add_script_tag(content=js.replace("const view=new URLSearchParams(location.search).get('view')||'gallery';","const view='mobile';"))
 assert page.evaluate("""()=>{
  let c=structuredClone(window.ELDORIA_RELICARIO_V2.CARD);
  c.values={N:10,E:10,S:10,O:10};
  document.querySelector('#preview').innerHTML='<div class="card-frame">'+window.ELDORIA_RELICARIO_V2.cardHTML(c,'epic',true)+'</div>';
  return [...document.querySelectorAll('.relic-stat')].every(x=>x.classList.contains('two-digit'))}""")
 for locator in page.locator('.relic-stat').all():
  result=locator.evaluate("""g=>{
    let p=g.querySelector('.plaque-inner').getBBox(),t=g.querySelector('.value').getBBox();
    return {inside:t.x>p.x+1&&t.x+t.width<p.x+p.width-1,delta:Math.abs(t.x+t.width/2-(p.x+p.width/2))}}""")
  assert result['inside'] and result['delta']<2,result
 page.close();browser.close()
report={'status':'PASS','scenarioCount':len(scenarios),'results':results,'stress':'N/E/S/O=10 centered','limits':['No Unity PlayMode','No iPhone physical','Art approval pending']}
(root/'qa/stat_plaques_v22.json').write_text(json.dumps(report,indent=2,ensure_ascii=False)+'\n')
print('PASS — vector sockets share coordinates with all 4 numbers, 7 sizes and 4x10')
