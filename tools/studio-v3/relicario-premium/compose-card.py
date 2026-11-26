"""Use exact existing V2.5 artwork frame, dynamic plaques and cardHTML on isolated card data.
This is an actual browser composition, not a generated screenshot of a card.
"""
import base64,json,sys,pathlib,hashlib,math
from playwright.sync_api import sync_playwright
from PIL import Image
P=pathlib.Path
src,out=P(sys.argv[1]),P(sys.argv[2])
template=src/"template"
card=json.loads((out/"card-data.json").read_text())
assert card["id"]=="corona-valoria" and card["rarity"]=="legendary"
art=Image.open(out/"illustration.png");art.verify()
assets={}
def data(path,mime):return "data:"+mime+";base64,"+base64.b64encode(path.read_bytes()).decode()
for name,mime in [("frame_legendary.webp","image/webp"),("frame_common.webp","image/webp"),("overlay_indestructible.svg","image/svg+xml"),("icon_wood.svg","image/svg+xml"),("bosque_valoria_art.jpg","image/jpeg")]:
    assets["assets/"+name]=data(template/"assets"/name,mime)
assets["assets/illustration.png"]=data(out/"illustration.png","image/png")
# Original construction symbol as a separate editable SVG (not game stats).
icon='<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64"><defs><linearGradient id="g" x2="1" y2="1"><stop stop-color="#f6dfa7"/><stop offset="1" stop-color="#99672f"/></linearGradient></defs><path fill="#231b29" stroke="#c99c54" stroke-width="2" d="M32 3 58 16 58 48 32 61 6 48 6 16Z"/><path fill="url(#g)" d="M13 30 13 19 20 19 20 25 26 25 26 19 33 19 33 25 40 25 40 19 47 19 47 30 43 34 43 47 34 47 34 39 28 39 28 47 17 47 17 34Z"/><path fill="none" stroke="#d5b777" stroke-width="2" d="M43 11 53 11 53 21 M53 11 41 23"/></svg>'
(out/"icon_construction.svg").write_text(icon)
assets["assets/icon_construction.svg"]=data(out/"icon_construction.svg","image/svg+xml")
css=(template/"styles.css").read_text()+"\n"+(template/"polish.css").read_text()
app=(template/"app.js").read_text()
assert app.count("CARD.id!=='bosque-valoria'")==1
# Reuse cardHTML unchanged. Only the historical hard-coded example guard is
# specialized in a scratch copy; gallery text is not part of the card.
app=app.split("function gridCard",1)[0].replace("CARD.id!=='bosque-valoria'","CARD.id!=='corona-valoria'")
js=(template/"stat-plaques.js").read_text()+"\n"+app+"\nwindow.renderActualCard=(c,i,compact=false)=>cardHTML(c,c.rarity,i,compact);"
qa=[];errors=[]
with sync_playwright() as pw:
    browser=pw.chromium.launch(headless=True,args=["--no-sandbox"])
    for width,height,cw,board in [(840,1200,640,False),(390,844,265,False),(320,720,265,False),(430,932,265,False),(390,844,105,True)]:
        page=browser.new_page(viewport={"width":width,"height":height},device_scale_factor=1)
        page.on("pageerror",lambda e:errors.append(str(e)))
        page.set_content('<!doctype html><html lang=es><head><meta name=viewport content="width=device-width, initial-scale=1"><style>'+css+'\nbody{display:grid;place-items:center;min-height:100vh;background:radial-gradient(ellipse at 50% 35%,#30223f,#0a0c10 68%)}#host{width:'+str(cw)+'px;height:'+str(round(cw*1.6))+'px}#host .relic-card{height:100%;width:100%}'+('</style></head><body><div id="host" class="board-cell"></div></body></html>' if board else '</style></head><body><div id="host"></div></body></html>'))
        page.evaluate("(a)=>window.__ELDORIA_ASSETS__=a",assets)
        page.evaluate("(c)=>window.ELDORIA_RELICARIO_DATA=c",card)
        page.add_script_tag(content=js)
        for ind in (False,True):
            page.evaluate("([c,i,b])=>document.querySelector('#host').innerHTML=window.renderActualCard(c,i,b)",[card,ind,board])
            page.wait_for_function("()=>[...document.images].every(i=>i.complete&&i.naturalWidth>0)")
            data1=page.locator(".relic-card").evaluate("""(el)=>{
 const r=el.getBoundingClientRect(),stats=Object.fromEntries([...el.querySelectorAll('.relic-stat')].map(e=>[e.dataset.dir,Number(e.querySelector('.value').textContent)]));
 const clip=[];for(const sel of ['.relic-rarity','.relic-title','.relic-effect strong','.relic-effect span']){const n=el.querySelector(sel);if(getComputedStyle(n).display==='none'||n.getBoundingClientRect().width===0)continue;if(n.scrollWidth>n.clientWidth+1||n.scrollHeight>n.clientHeight+1)clip.push(sel)}
 const v=[...el.querySelectorAll('.relic-stat')].map(e=>{const r=e.querySelector('.plaque-inner').getBBox(),v=e.querySelector('.value').getBBox();return v.x>=r.x-1&&v.x+v.width<=r.x+r.width+1});
 return {w:r.width,h:r.height,stats,clip,statsInside:v.every(Boolean),rare:el.dataset.rarity,ind:el.dataset.indestructible==='true',title:el.querySelector('.relic-title').textContent,effect:el.querySelector('.relic-effect span').textContent,aria:el.getAttribute('aria-label')};
}""")
            assert abs(data1["h"]/data1["w"]-1.6)<.01,data1
            assert data1["stats"]==card["values"] and data1["statsInside"] and data1["rare"]=="legendary" and data1["ind"]==ind,data1
            assert not data1["clip"],data1
            assert data1["effect"]==card["effectShort"] and card["effect"] in data1["aria"],data1
            label=("board" if board else "master" if cw==640 else str(width))+"-"+("indestructible" if ind else "normal")
            page.locator(".relic-card").screenshot(path=str(out/(label+".png")),animations="disabled")
            qa.append({"label":label,"viewport":[width,height],"card_px":[data1["w"],data1["h"]],"values":data1["stats"],"clipping":data1["clip"]})
        page.close()
    browser.close()
assert not errors,errors
proof={"schema":1,"actual_chromium":True,"source_template":"6c2357c943c247510eea18d84c2b8908ef7f8456","source_card":card,"cases":qa,"new_api_calls":0,"art_approved":False,"unity_native":False,"physical_device":False}
(out/"card-proof.json").write_text(json.dumps(proof,indent=2,ensure_ascii=False))
print("RELICARIO_CARD_PROOF "+json.dumps({"cases":len(qa),"clipped":sum(len(x["clipping"]) for x in qa),"same_values":True,"rare":"legendary","master_px":[640,1024],"art_approved":False}))
