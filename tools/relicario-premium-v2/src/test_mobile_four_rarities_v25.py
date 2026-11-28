"""Chromium evidence for four Eldoria card rarities at realistic mobile size.
Run: python src/test_mobile_four_rarities_v25.py
Outputs: eight mobile PNG screenshots, contact PNG, JSON report. No Unity/iPhone claim.
"""
from pathlib import Path
from playwright.sync_api import sync_playwright
from PIL import Image, ImageDraw
import json, base64

root=Path(__file__).resolve().parents[1]
out=root/'qa'
out.mkdir(exist_ok=True)
css=(root/'styles.css').read_text(encoding='utf-8')+'\n'+(root/'polish.css').read_text(encoding='utf-8')
app=(root/'app.js').read_text(encoding='utf-8')
app=app.replace("const view=new URLSearchParams(location.search).get('view')||'gallery';","const view='mobile';")
js=(root/'card-data.js').read_text(encoding='utf-8')+'\n'+(root/'stat-plaques.js').read_text(encoding='utf-8')+'\n'+app
mime={'.webp':'image/webp','.jpg':'image/jpeg','.svg':'image/svg+xml'}
assets={f'assets/{p.name}':f'data:{mime[p.suffix]};base64,'+base64.b64encode(p.read_bytes()).decode() for p in (root/'assets').iterdir() if p.suffix in mime}
rarities=['common','rare','epic','legendary']
records=[];crops=[]
with sync_playwright() as pw:
 browser=pw.chromium.launch(headless=True,executable_path='/usr/bin/chromium',args=['--no-sandbox','--disable-gpu'])
 for width,height in ((390,844),(320,720),(430,932)):
  page=browser.new_page(viewport={'width':width,'height':height},is_mobile=True,has_touch=True)
  errors=[]
  page.on('pageerror',lambda e:errors.append(str(e)))
  page.set_content('<!doctype html><html lang=es><head><meta name=viewport content="width=device-width,initial-scale=1"><style>'+css+'</style></head><body><div id=app></div></body></html>')
  page.evaluate('(a)=>window.__ELDORIA_ASSETS__=a',assets)
  page.add_script_tag(content=js)
  page.locator('.relic-card').first.wait_for()
  assert not errors,errors
  for indestructible in (False,True):
   for rarity in rarities:
    page.evaluate("""([rarity,ind])=>{
      const card=window.ELDORIA_RELICARIO_V2;
      document.querySelector('.mobile-stage').innerHTML='<div class="card-frame">'+card.cardHTML(card.CARD,rarity,ind)+'</div>';
    }""",[rarity,indestructible])
    page.wait_for_function('() => [...document.querySelectorAll("img")].every(x=>x.complete&&x.naturalWidth>0)')
    result=page.locator('.relic-card').evaluate("""card=>{
      const cr=card.getBoundingClientRect(),fields={};
      for(const selector of ['.relic-rarity','.relic-title','.relic-effect strong','.relic-effect span']){
       const el=card.querySelector(selector),rect=el.getBoundingClientRect();
       const range=document.createRange();range.selectNodeContents(el);
       const tr=range.getBoundingClientRect();
       fields[selector]={font:parseFloat(getComputedStyle(el).fontSize),left:tr.left-rect.left,right:rect.right-tr.right,overflow:el.scrollHeight>el.clientHeight+1||el.scrollWidth>el.clientWidth+1};
      }
      const stats=[...card.querySelectorAll('.relic-stats-svg .relic-stat')].map(g=>{
       const r=g.querySelector('.plaque-inner').getBBox(),v=g.querySelector('.value').getBBox();
       return {delta:Math.abs(r.x+r.width/2-(v.x+v.width/2)),margin:Math.min(v.x-r.x,r.x+r.width-(v.x+v.width))};
      });
      return {card:{x:cr.x,y:cr.y,width:cr.width,height:cr.height},fields,stats,ind:!!card.querySelector('.relic-indestructible')};
    }""")
    assert abs(result['card']['height']/result['card']['width']-1.6)<.005
    assert len(result['stats'])==4 and all(v['delta']<2 and v['margin']>1 for v in result['stats'])
    assert result['ind']==indestructible
    assert not any(v['overflow'] for v in result['fields'].values()),(width,rarity,indestructible)
    assert result['fields']['.relic-rarity']['left']>=3 and result['fields']['.relic-rarity']['right']>=3
    assert page.evaluate('document.documentElement.scrollWidth<=innerWidth+1')
    if width==390:
     assert result['fields']['.relic-effect span']['font']>=12.0
     filename=f'mobile_v25_{rarity}_{"ind" if indestructible else "normal"}_390.png'
     page.screenshot(path=str(out/filename))
     im=Image.open(out/filename).convert('RGB')
     x,y,w,h=[int(round(result['card'][key])) for key in ('x','y','width','height')]
     crops.append(im.crop((x-3,y-3,x+w+3,y+h+3)))
    records.append({'viewport':[width,height],'rarity':rarity,'indestructible':indestructible,'fontPx':round(result['fields']['.relic-effect span']['font'],2),'rarityMarginPx':round(result['fields']['.relic-rarity']['left'],2)})
  page.close()
 browser.close()
assert len(records)==24
sheet=Image.new('RGB',(1170,940),(14,15,21));d=ImageDraw.Draw(sheet)
for i,im in enumerate(crops):
 row,col=divmod(i,4);x,y=16+col*288,30+row*458
 sheet.paste(im.resize((265,425),Image.Resampling.LANCZOS),(x,y))
 d.text((x+4,y-18),('INDESTRUCTIBLE ' if row else '')+rarities[col].upper(),fill=(235,208,170))
sheet.save(out/'four_rarities_mobile_v25.png',optimize=True)
report={'status':'PASS','test':'Real Chromium mobile renders using the versioned PR assets','cases':len(records),'viewports':['390x844','320x720','430x932'],'rarities':rarities,'normalAndIndestructible':True,'ratio':'5:8','clippedText':0,'numberMisalignment':0,'minEffectFontPx390':min(x['fontPx'] for x in records if x['viewport'][0]==390),'pending':['Unity PlayMode','physical iPhone Safari','final owner art approval']}
(out/'four_rarities_mobile_v25.json').write_text(json.dumps(report,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
print(json.dumps(report,ensure_ascii=False))
