"""Trusted renderer: no model-generated executable code, only typed art/layout data."""
import json, pathlib, html, re, shutil, hashlib
from PIL import Image, ImageDraw
from playwright.sync_api import sync_playwright
P=pathlib.Path
SECTIONS=["Overview","Rankings","Trophies","Discoveries","Heraldry","Monuments"]
COLORS=["ink","surface","trim","text","muted","accent","action"]
ICONS=dict(zip(SECTIONS,["identity_crest.png","world_rune.png","objective_rune.png","icon_scroll.png","nav_medallion.png","icon_city.png"]))
def need(x,msg):
    if not x:raise ValueError(msg)
def exact(o,keys):return type(o) is dict and set(o)==set(keys)
def validate_director(d):
    need(exact(d,["concept","palette","hall_prompt","relic_prompt"]),"Director shape")
    need(type(d["concept"]) is str and 0<len(d["concept"])<=900,"Concept")
    need(exact(d["palette"],COLORS) and all(type(v) is str and re.fullmatch("#[0-9a-fA-F]{6}",v) for v in d["palette"].values()),"Palette")
    for k in ("hall_prompt","relic_prompt"):need(type(d[k]) is str and 50<=len(d[k])<=1800,"Artist brief")
    return d
def validate_ux(d):
    need(exact(d,SECTIONS),"Missing section design")
    for k,v in d.items():
        need(exact(v,["layout","art","hero_height","density"]) and v["layout"] in ("list","tiles","featured") and v["art"] in ("hall","relic") and type(v["hero_height"]) is int and 100<=v["hero_height"]<=172 and v["density"] in ("compact","normal"),"Invalid section layout "+k)
        if k=="Rankings":need(v["layout"]!="tiles","Ranking status must remain full-width and clear")
    return d
def validate_integration(d):
    need(exact(d,["gutter","gap","radius","title_px","body_px","detail_px","art_strength","ornament","frame"]),"Integration shape")
    for k,a,b in [("gutter",12,20),("gap",6,12),("radius",4,12),("title_px",17,22),("body_px",14,17),("detail_px",12,14),("art_strength",25,70)]:
        need(type(d[k]) is int and a<=d[k]<=b,k)
    need(d["ornament"] in ("crest","medallion","minimal") and d["frame"] in ("gold","subtle"),"Frame")
    return d
def make_preview(data,source,art,theme,out):
    out=P(out);out.mkdir(parents=True,exist_ok=False)
    d=validate_director(theme["director"]);ux=validate_ux(theme["ux"]);t=validate_integration(theme["integration"]);c=d["palette"]
    shutil.copytree(P(source)/"assets",out/"assets")
    for n in ("hall","relic"):
        p=P(art)/(n+".png")
        with Image.open(p) as im:
            im.verify()
        with Image.open(p) as im:
            im.convert("RGB").save(out/"assets"/(n+".jpg"),quality=94)
    payload=json.dumps(data,ensure_ascii=False,separators=(",",":")).replace("<","\\u003c")
    design=json.dumps(ux,separators=(",",":"))
    icons=json.dumps(ICONS)
    fade=t["art_strength"]/100
    style=f"""
    *{{box-sizing:border-box}} html,body{{margin:0;height:100%;background:#0e0b15;color:{c['text']};font-family:system-ui,sans-serif}} body{{overflow:hidden}}
    #app{{position:relative;isolation:isolate;max-width:660px;margin:auto;height:100%;display:flex;flex-direction:column;background:radial-gradient(ellipse at top,{c['surface']}, {c['ink']} 63%);overflow:hidden}}
    header{{height:52px;flex:none;padding:0 {t['gutter']}px;display:flex;align-items:center;gap:8px;border-bottom:1px solid {c['trim']}70;background:linear-gradient(90deg,#0e0b15,{c['surface']});z-index:3}}
    header img{{height:40px;width:40px;object-fit:contain}} header strong{{font-family:Georgia,serif;font-weight:normal;letter-spacing:1.6px;font-size:18px;flex:1}} header select{{border:1px solid {c['trim']}70;background:{c['ink']};color:{c['text']};font-size:12px;padding:7px;border-radius:5px;max-width:102px}} #badge{{font-size:9px;color:#ede4cd;background:#43334c;border-radius:3px;padding:3px 5px;white-space:nowrap}}
    #nav{{flex:none;display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:4px;padding:6px {t['gutter']}px 7px;z-index:3;background:linear-gradient({c['ink']},{c['surface']});border-bottom:1px solid {c['trim']}60}}
    #nav button{{min-height:44px;display:flex;align-items:center;justify-content:center;gap:5px;padding:4px;border:1px solid {c['trim']}50;border-radius:5px;background:{c['ink']};color:{c['muted']};font-size:12px;line-height:1.15;cursor:pointer}}
    #nav button img{{width:23px;height:23px;object-fit:contain}} #nav button[aria-current=true]{{border-color:{c['action']};color:{c['text']};background:linear-gradient(125deg,#164148,{c['surface']});box-shadow:inset 0 0 0 1px {c['action']}40}}
    main{{flex:1;overflow:auto;overscroll-behavior:contain;scrollbar-width:thin;scrollbar-color:{c['trim']} {c['ink']};min-height:0}}
    .hero{{position:relative;display:flex;flex-direction:column;justify-content:flex-end;min-height:100px;padding:14px {t['gutter']}px 16px;background-size:cover;background-position:center 44%;border-bottom:1px solid {c['trim']}95}}
    .hero:after{{content:"";pointer-events:none;position:absolute;inset:0;background:linear-gradient(0deg,{c['ink']} 0%,{c['ink']}b0 34%,transparent 78%);z-index:0}} .hero>*{{z-index:1;position:relative}}
    .hero h1{{font-family:Georgia,serif;font-size:30px;line-height:1.1;font-weight:normal;margin:4px 0;color:{c['text']};text-shadow:0 2px 5px black;letter-spacing:.2px}} .hero .kicker{{font-size:11px;text-transform:uppercase;letter-spacing:1.6px;color:{c['trim']};font-weight:650}} .hero .emblem{{position:absolute;right:15px;bottom:15px;width:64px;height:64px;object-fit:contain}}
    .content{{display:grid;gap:{t['gap']}px;padding:12px {t['gutter']}px 24px;grid-template-columns:minmax(0,1fr)}} .content.tiles{{grid-template-columns:repeat(2,minmax(0,1fr))}}
    .row{{min-width:0;position:relative;display:flex;gap:10px;align-items:center;padding:12px 11px;border:1px solid {c['trim']}65;border-radius:{t['radius']}px;background:linear-gradient(137deg,{c['surface']}f5,{c['ink']}fa);box-shadow:inset 0 1px 0 #ffffff0c,0 3px 9px #0004;min-height:60px}}
    .row.compact{{padding:9px;min-height:54px}} .row .rowicon{{flex:none;width:38px;height:38px;object-fit:contain;filter:drop-shadow(0 2px 2px #0008)}} .row .copy{{min-width:0;flex:1}} .row .title{{font-size:{t['body_px']}px;font-weight:650;color:{c['text']};overflow-wrap:anywhere;line-height:1.25}} .row .detail{{font-size:{t['detail_px']}px;color:{c['muted']};margin-top:4px;line-height:1.35;overflow-wrap:anywhere}}
    .row[data-state=Earned]{{border-color:{c['trim']}b0;background:linear-gradient(125deg,{c['surface']}, {c['ink']});}} .row[data-state=Locked]{{border-color:#82748560;}} .row[data-state=Locked] .rowicon{{filter:grayscale(1) brightness(.65)}} .row[data-state=Unavailable]{{border-color:#93829c70}} .row[data-state=Unavailable] .rowicon{{opacity:.55}}
    .state{{font-size:9px;letter-spacing:.5px;color:{c['trim']};text-transform:uppercase;margin-top:6px;line-height:1.2}} [data-state=Locked] .state,[data-state=Unavailable] .state{{color:{c['muted']}} .row.action{{cursor:pointer;text-align:left;width:100%;font-family:inherit;color:inherit}}
    .tiles .row{{display:flex;flex-direction:column;align-items:flex-start;min-height:115px;padding:10px}} .tiles .row .rowicon{{width:44px;height:44px}} .tiles .copy{{width:100%}}
    .content.featured .row:first-child,.row.notice{{grid-column:1/-1;min-height:84px;background:linear-gradient(130deg,{c['surface']}, {c['ink']}ee),url('assets/panel_obsidian.png') center/cover;border-color:{c['trim']};}}
    .row.notice .detail{{line-height:1.4}} .row.notice .title{{font-size:{t['title_px']}px;font-family:Georgia,serif;font-weight:normal}} .content.featured .row:first-child .title{{font-size:{t['title_px']}px;font-family:Georgia,serif}}
    .row[data-key="prestige"] .detail,.row[data-key="power"] .detail{{font-size:20px;color:{c['trim']};font-weight:750}}
    .back{{min-height:44px;border:1px solid {c['action']}80;border-radius:5px;background:#163940;color:{c['text']};font-size:14px;padding:10px;grid-column:1/-1;cursor:pointer}}
    footer{{flex:none;font-size:10px;letter-spacing:.25px;text-align:center;background:{c['ink']};color:{c['muted']};border-top:1px solid {c['trim']}55;padding:7px 10px}}
    @media(max-width:360px){{#nav button{{font-size:11px;gap:3px}}#nav button img{{width:20px;height:20px}} .row .rowicon{{width:32px;height:32px}} .hero h1{{font-size:27px}} header strong{{font-size:15px}}}}
    @media(orientation:landscape){{#app{{max-width:none}} header{{height:45px}} #nav{{grid-template-columns:repeat(6,minmax(0,1fr));padding:4px 12px}} #nav button{{min-height:40px}} .hero{{min-height:88px!important;height:88px!important;padding:8px 20px;background-position:center 42%}} .hero h1{{font-size:25px}} .hero .emblem{{width:48px;height:48px}} .content{{grid-template-columns:repeat(2,minmax(0,1fr))}} .content.tiles{{grid-template-columns:repeat(3,minmax(0,1fr))}} }}
    """
    # Extra material choices from technical artist are consumed in actual render.
    if t["frame"]=="gold":style+=".row{border-top-color:"+c["trim"]+"c0;border-left-color:"+c["trim"]+"90}"
    if t["ornament"]=="minimal":style+=".hero .emblem{display:none}"
    if t["ornament"]=="medallion":style+=".hero .emblem{filter:drop-shadow(0 3px 6px #000);width:72px;height:72px}"
    script="""
    const data=DATA,design=DESIGN,icons=ICONS;let profile=data.profiles.at(-1),section="Overview",detail=null;
    const el=(tag,cls,txt)=>{const n=document.createElement(tag);if(cls)n.className=cls;if(txt!==undefined)n.textContent=txt;return n};
    const img=(src,cls)=>{const n=el("img",cls);n.src="assets/"+src;n.alt="";return n};
    const states={Earned:"Conseguido",Locked:"Bloqueado",Unavailable:"No disponible",Available:"Disponible"};
    function render(){
      const s=profile.sections.find(x=>x.id===section)||profile.sections[0];section=s.id;
      const nav=document.querySelector("#nav");nav.replaceChildren();
      for(const x of profile.sections){const b=el("button");b.dataset.section=x.id;b.setAttribute("aria-current",String(section===x.id));b.append(img(icons[x.id]),el("span",null,x.label));b.onclick=()=>{section=x.id;detail=null;render()};nav.append(b)}
      const v=design[section],hero=document.querySelector(".hero");hero.style.height=v.hero_height+"px";hero.style.backgroundImage="linear-gradient(90deg,rgba(15,11,23,FADE),rgba(15,11,23,.15)),url('assets/"+v.art+".jpg')";hero.replaceChildren(el("div","kicker","Valoria · Bastión "+profile.bastion),el("h1",null,s.label),img(icons[section],"emblem"));
      const body=document.querySelector(".content");body.className="content "+v.layout;body.replaceChildren();
      let rows=s.rows;if(detail!==null){rows=profile.details[detail];const b=el("button","back","Volver a hallazgos");b.onclick=()=>{detail=null;render()};body.append(b)}
      for(const r of rows){const canOpen=section==="Discoveries"&&detail===null&&Object.hasOwn(profile.details,r.key);
        const n=el(canOpen?"button":"article","row "+v.density+(r.key==="offline"?" notice":"")+(canOpen?" action":""));n.dataset.key=r.key;n.dataset.state=r.state;
        if(canOpen)n.onclick=()=>{detail=r.key;render()};
        const ico=section==="Rankings"&&r.key==="offline"?"icon_scroll.png":icons[section];n.append(img(ico,"rowicon"));
        const copy=el("div","copy");copy.append(el("div","title",r.title),el("div","detail",r.detail));if(states[r.state])copy.append(el("div","state",states[r.state]));n.append(copy);body.append(n)
      }
      document.querySelector("main").scrollTop=0;
    }
    document.querySelector("#level").onchange=e=>{profile=data.profiles.find(x=>x.bastion===Number(e.target.value));section="Overview";detail=null;render()};
    window.qa={get:()=>({profile,section,detail}),render};render();
    """.replace("DATA",payload,1).replace("DESIGN",design,1).replace("ICONS",icons,1).replace("FADE",str(fade),1)
    page='<!doctype html><html lang="es"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Eldoria · Perfil</title><style>'+style+'</style><body><div id="app"><header><img src="assets/identity_crest.png" alt=""><strong>ELDORIA</strong><span id="badge">PRUEBA QA</span><select id="level" aria-label="Bastión de prueba">'+''.join(f'<option value="{n}" {"selected" if n==5 else ""}>Bastión {n}</option>' for n in range(1,6))+'</select></header><nav id="nav"></nav><main><div class="hero"></div><div class="content"></div></main><footer>Prototipo visual · datos de prueba del dominio PR #198</footer></div><script>'+script+'</script></body></html>'
    (out/"index.html").write_text(page,encoding="utf-8")
    (out/"theme.json").write_text(json.dumps(theme,ensure_ascii=False,indent=2))
def capture(out):
    out=P(out);shots=out/"captures";shots.mkdir()
    rows=[]
    with sync_playwright() as p:
        browser=p.chromium.launch(headless=True,args=["--no-sandbox"])
        for name,w,h in [("phone",390,844),("compact",320,720),("landscape",844,390)]:
            page=browser.new_page(viewport={"width":w,"height":h},device_scale_factor=1)
            errors=[];page.on("pageerror",lambda e:errors.append(str(e)))
            page.goto((out/"index.html").resolve().as_uri())
            page.wait_for_function("window.qa")
            for k in SECTIONS:
                page.locator('[data-section="'+k+'"]').click()
                page.wait_for_timeout(70)
                q=page.evaluate("""() => {const s=qa.get(), rows=s.profile.sections.find(x=>x.id===s.section).rows, dom=Array.from(document.querySelectorAll('.row'));return {exact:rows.length===dom.length&&rows.every((x,i)=>dom[i].dataset.key===x.key&&dom[i].querySelector('.title').textContent===x.title&&dom[i].querySelector('.detail').textContent===x.detail), horizontal:document.querySelector('#app').scrollWidth<=innerWidth+1,clipped:dom.filter(e=>e.scrollWidth>e.clientWidth+1).length,buttons:Array.from(document.querySelectorAll('#nav button')).every(e=>e.getBoundingClientRect().width>=44),publicRankings:s.profile.publicRankings}}""")
                need(q["exact"] and q["horizontal"] and q["clipped"]==0 and q["buttons"] and not q["publicRankings"],"Actual projection/layout failed "+name+"/"+k)
                dest=shots/(name+"-"+k+".png");page.screenshot(path=str(dest))
                rows.append({"view":name,"section":k,"sha256":hashlib.sha256(dest.read_bytes()).hexdigest(),**q})
            page.locator('[data-section="Discoveries"]').click()
            page.locator('.row.action').first.click()
            q=page.evaluate("""() => {let s=qa.get(),rows=s.profile.details[s.detail],dom=Array.from(document.querySelectorAll('.row'));return rows.length===dom.length&&rows.every((x,i)=>dom[i].dataset.key===x.key&&dom[i].querySelector('.title').textContent===x.title&&dom[i].querySelector('.detail').textContent===x.detail)}""")
            need(q,"Discovery detail mismatch")
            dest=shots/(name+"-DiscoveryDetail.png");page.screenshot(path=str(dest))
            rows.append({"view":name,"section":"DiscoveryDetail","sha256":hashlib.sha256(dest.read_bytes()).hexdigest(),"exact":True})
            page.locator(".back").click()
            page.select_option("#level","1");need(page.locator("#nav button").count()==1,"Bastion I unlock regression")
            page.select_option("#level","2");need(page.locator("#nav button").count()==3,"Bastion II unlock regression")
            page.select_option("#level","3");need(page.locator("#nav button").count()==4,"Bastion III unlock regression")
            page.select_option("#level","4");need(page.locator("#nav button").count()==5,"Bastion IV unlock regression")
            need(not errors,"Browser error")
            page.close()
        browser.close()
    (out/"capture-proof.json").write_text(json.dumps(rows,indent=2))
    # Two actual screenshot sheets for independent visual review; same original resolution.
    for file,keys in [("audit-1.jpg",SECTIONS[:3]),("audit-2.jpg",SECTIONS[3:]+["DiscoveryDetail"])]:
        sheet=Image.new("RGB",(390*len(keys),844+24),"#100d19");draw=ImageDraw.Draw(sheet)
        for i,k in enumerate(keys):
            im=Image.open(shots/("phone-"+k+".png")).convert("RGB");sheet.paste(im,(390*i,24));draw.text((390*i+8,5),k,fill="white")
        sheet.save(out/file,quality=87)
    print("PR198_RENDER_PASS",len(rows),flush=True)
    return rows
