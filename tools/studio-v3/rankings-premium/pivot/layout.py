"""Agent-authored declarative templates and CSS; canonical data + trusted behavior.
No model scripts, execution or external URLs. New illustrated sprites are separate.
"""
import json,re,html,pathlib,shutil,sys,hashlib
from html.parser import HTMLParser
from PIL import Image
P=pathlib.Path;sys.path.insert(0,str(P(__file__).resolve().parent.parent))
from render import capture,SECTIONS,need,exact
PAGES=SECTIONS+["DiscoveryDetail"]
ATLAS=[
 ["power","kingdom","army","vigilants","hunters","explorers","profile","trophy","unknown"],
 ["ashes","fissure","frontier","title","banner","bastion","guard","hunt","scout"],
 ["founder","fountain","hunting-monument","fissure-memorial","ash-memorial","prestige","chronicle","seal","gem"]
]
class SafeTemplate(HTMLParser):
    def __init__(self):super().__init__(convert_charrefs=False);self.stack=[]
    def handle_starttag(self,t,a):
        need(t in ("div","section","article","span","aside","figure","header"),"Forbidden tag")
        need(len(a)<=1 and all(k=="class" and re.fullmatch(r"x-[a-z0-9-]+(?: x-[a-z0-9-]+)*",v or "") for k,v in a),"Only x- classes")
        self.stack.append(t)
    def handle_endtag(self,t):
        need(self.stack and self.stack.pop()==t,"Mismatched template tag")
    def handle_data(self,t):need(not t.strip(),"No hardcoded text in templates")
    def handle_comment(self,t):raise ValueError("No comments")
    def handle_entityref(self,t):raise ValueError("No entities")
    def handle_charref(self,t):raise ValueError("No entities")
def validate_template(t,keys):
    need(type(t) is str and len(t)<=2000,"Template size")
    found=re.findall(r"\{\{([a-z]+)\}\}",t)
    need(sorted(found)==sorted(keys),"Missing or repeated tokens")
    stripped=re.sub(r"\{\{[a-z]+\}\}","",t)
    need("{" not in stripped and "}" not in stripped and not re.search(r"[<>]",re.sub(r"</?[a-z]+(?: class=\"[a-z0-9 -]+\")?>","",stripped)),"Unsafe template")
    p=SafeTemplate();p.feed(stripped);p.close();need(not p.stack,"Unclosed template")
    return t
def validate_pages(p):
    need(exact(p,PAGES),"Expected seven existing views")
    for k,x in p.items():
        need(exact(x,["outer","lead","row","art"]) and x["art"] in ("hall","relic"),"Page fields")
        validate_template(x["outer"],["heading","art","lead","rows"]+(["back"] if k=="DiscoveryDetail" else []))
        need(x["outer"].index("{{lead}}")<x["outer"].index("{{rows}}"),"Keep data order")
        for a in ("lead","row"):validate_template(x[a],["icon","title","detail","state"])
    return p
def validate_css(c):
    need(type(c) is str and 500<len(c)<=30000,"CSS length")
    need(not re.search(r"url\s*\(|@import|@font-face|expression\s*\(|javascript:|<|\\\\|/\*|@keyframes",c,re.I),"Unsafe CSS content")
    need(all(m.group(1).strip() in ('""',"''","none","normal") for m in re.finditer(r"(?:^|[;{])\s*content\s*:\s*([^;}]+)",c,re.I)),"No synthetic UI labels")
    return c
def crop_sheets(art,out):
    for n,keys in enumerate(ATLAS):
        with Image.open(P(art)/("atlas-"+str(n+1)+".png")) as im:
            im=im.convert("RGBA");w,h=im.size
            need(w>=768 and h>=768,"Small atlas")
            for i,k in enumerate(keys):
                x,y=i%3,i//3;cell=im.crop((round(x*w/3),round(y*h/3),round((x+1)*w/3),round((y+1)*h/3)))
                cell.save(P(out)/(k+".png"))
def compose(data,source,art,pages,css,out):
    validate_pages(pages);validate_css(css);out=P(out);out.mkdir(parents=True,exist_ok=False);assets=out/"assets";shutil.copytree(P(source)/"assets",assets)
    crop_sheets(art,assets)
    for k in ["hall","relic"]:
        with Image.open(P(art)/(k+".png")) as im:im.convert("RGB").save(assets/(k+".jpg"),quality=94)
    js=r"""
    const data=DATA,pages=PAGES;let profile=data.profiles.at(-1),section="Overview",detail=null;
    const esc=s=>String(s).replaceAll("&","&amp;").replaceAll("<","&lt;").replaceAll(">","&gt;").replaceAll('"',"&quot;");
    const replace=(template,values)=>template.replace(/\{\{([a-z]+)\}\}/g,(_,k)=>values[k]??"");
    const img=(id,cls="icon")=>'<img class="'+cls+'" src="assets/'+id+'.png" alt="">';
    const icons={Overview:"profile",Rankings:"power",Trophies:"trophy",Discoveries:"chronicle",Heraldry:"banner",Monuments:"founder"};
    const states={Earned:"Conseguido",Locked:"Bloqueado",Unavailable:"No disponible",Available:"Disponible"};
    const rowIcons={"name":"profile","power":"power","kingdom":"kingdom","army":"army","prestige":"prestige","title":"title","next-title":"title","trophies":"trophy","discoveries":"explorers","offline":"seal","ranking:TotalPower":"power","ranking:Kingdom":"kingdom","ranking:Army":"army","ranking:Vigilants":"vigilants","ranking:Hunters":"hunters","ranking:Explorers":"explorers","week:discoveries":"explorers","week:gathered":"kingdom","ashes":"ashes","fissure":"fissure","frontier":"frontier","monument-founder":"founder","monument-guard":"fountain","monument-hunt":"hunting-monument","monument-fissure":"fissure-memorial","monument-ash":"ash-memorial"};
    function iconFor(r){
      if(r.title==="Distinción por descubrir"||r.title==="Hazaña por descubrir"||r.title==="Hallazgo desconocido")return "unknown";
      if(rowIcons[r.key])return rowIcons[r.key];
      for(const [a,b] of [["bastion-","bastion"],["vigilant-","guard"],["hunt-","hunt"],["scout-","scout"],["discovery-","explorers"],["title:","title"],["banner-","banner"]])if(r.key.startsWith(a))return b;
      return section==="Discoveries"?"chronicle":icons[section];
    }
    function render(){
      const s=profile.sections.find(x=>x.id===section)||profile.sections[0];section=s.id;
      const nav=document.querySelector("#nav");nav.replaceChildren();
      for(const x of profile.sections){const b=document.createElement("button");b.dataset.section=x.id;b.setAttribute("aria-current",String(section===x.id));b.innerHTML=img(icons[x.id],"nav-icon")+'<span>'+esc(x.label)+'</span>';b.onclick=()=>{section=x.id;detail=null;render()};nav.append(b)}
      const id=detail!==null?"DiscoveryDetail":section,p=pages[id],rows=detail!==null?profile.details[detail]:s.rows;
      function row(r,i){
        const canOpen=section==="Discoveries"&&detail===null&&Object.hasOwn(profile.details,r.key);
        const tag=canOpen?"button":"article";
        const t=replace(i===0?p.lead:p.row,{icon:img(iconFor(r)),title:'<span class="title">'+esc(r.title)+'</span>',detail:'<span class="detail">'+esc(r.detail)+'</span>',state:states[r.state]?'<span class="state">'+states[r.state]+'</span>':'<span class="state empty"></span>'});
        return '<'+tag+' class="row '+(i===0?'lead ':'')+(canOpen?'action':'')+'" data-key="'+esc(r.key)+'" data-state="'+esc(r.state)+'">'+t+'</'+tag+'>'
      }
      const main=document.querySelector("main");
      main.innerHTML='<div class="page page-'+id+'">'+replace(p.outer,{heading:'<div class="section-heading"><span class="chapter">Valoria · Bastión '+profile.bastion+'</span><h1>'+esc(s.label)+'</h1></div>',art:'<div class="section-art"><img class="artwork" src="assets/'+p.art+'.jpg" alt=""></div>',lead:rows.length?row(rows[0],0):"",rows:rows.slice(1).map((r,i)=>row(r,i+1)).join(""),back:'<button class="back">Volver a hallazgos</button>'})+'</div>';
      if(detail!==null)main.querySelector(".back").onclick=()=>{detail=null;render()};
      for(const b of main.querySelectorAll(".row.action"))b.onclick=()=>{detail=b.dataset.key;render()};main.scrollTop=0;
    }
    document.querySelector("#level").onchange=e=>{profile=data.profiles.find(x=>x.bastion===Number(e.target.value));section="Overview";detail=null;render()};window.qa={get:()=>({profile,section,detail}),render};render();
    """.replace("DATA",json.dumps(data,ensure_ascii=False).replace("<","\\u003c"),1).replace("PAGES",json.dumps(pages,ensure_ascii=False).replace("<","\\u003c"),1)
    safety="*{box-sizing:border-box}html,body{margin:0;height:100%;overflow:hidden}#app{height:100%;display:flex;flex-direction:column;overflow:hidden}header,#nav,footer{flex:none}main{flex:1;min-height:0;overflow:auto}img{max-width:100%;object-fit:contain}button{font:inherit;cursor:pointer}header>*{min-width:0}#nav button{min-width:0;overflow-wrap:anywhere}.row{min-width:0;max-width:100%}.title,.detail{overflow-wrap:anywhere}"
    page='<!doctype html><html lang="es"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Eldoria · Legado</title><style>'+safety+css+'</style><body><div id="app"><header class="global-header"><img class="brand" src="assets/identity_crest.png" alt=""><strong>ELDORIA</strong><span id="badge">PRUEBA QA</span><select id="level" aria-label="Bastión de prueba">'+''.join(f'<option value="{n}" {"selected" if n==5 else ""}>Bastión {n}</option>' for n in range(1,6))+'</select></header><nav id="nav"></nav><main></main><footer>Prototipo visual · datos de prueba del dominio PR #198</footer></div><script>'+js+'</script></body></html>'
    (out/"index.html").write_text(page,encoding="utf-8");(out/"theme.json").write_text(json.dumps({"pages":pages,"css":css},ensure_ascii=False,indent=2))
    rows=capture(out)
    # Additional visible-data check, including scrolled rows. Never claim native.
    from playwright.sync_api import sync_playwright
    with sync_playwright() as p:
        b=p.chromium.launch(headless=True,args=["--no-sandbox"])
        page=b.new_page(viewport={"width":320,"height":720})
        page.route("**/*",lambda route:route.continue_() if route.request.url.startswith("file:") else route.abort())
        page.goto((out/"index.html").resolve().as_uri())
        for k in SECTIONS:
            page.locator('[data-section="'+k+'"]').click()
            need(page.locator(".title").count()>0,"Missing text")
            need(page.locator(".title").count()==page.locator(".detail").count(),"Missing detail")
            for el in page.locator(".title,.detail").all():
                need(el.is_visible(),"Hidden real game data")
                need(el.evaluate("(e)=>parseFloat(getComputedStyle(e).fontSize)>=11"),"Text under minimum")
        b.close()
    return rows
