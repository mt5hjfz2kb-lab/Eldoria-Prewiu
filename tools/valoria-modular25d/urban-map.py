"""Draw the native Unity survey with north toward the top of the plan."""
import json
import sys
import xml.etree.ElementTree as ET
from pathlib import Path
survey=json.loads(Path(sys.argv[1]).read_text(encoding="utf-8-sig"))
svg=ET.Element("svg",xmlns="http://www.w3.org/2000/svg",viewBox="0 0 930 632")
def node(tag,**attrs):
    return ET.SubElement(svg,tag,{k.replace("_","-"):str(v) for k,v in attrs.items()})
def text(x,y,words,size=15,color="#e9dcc0"):
    el=node("text",x=x,y=y,fill=color,font_family="system-ui, sans-serif",font_size=size)
    el.text=words
def point(p):
    return 463+27*p["x"],290-27*p["y"]
def rect(lower,size,fill,stroke):
    x,y=point(lower)
    return node("rect",x=x,y=y-27*size["y"],width=27*size["x"],height=27*size["y"],rx=4,fill=fill,stroke=stroke)
def road(points,color,width,dashed=False):
    coords=" ".join("%.2f,%.2f"%point(p) for p in points)
    node("polyline",points=coords,fill="none",stroke=color,stroke_width=width,stroke_linejoin="round",stroke_linecap="round",stroke_dasharray="5 6" if dashed else "none")
node("rect",x=0,y=0,width=930,height=632,fill="#202322")
text(27,42,"Valoria · capital en formación",29)
text(28,66,"Parcelas y vías medidas en la escena de Unity",15)
for i,reserve in enumerate(survey["reserves"]):
    rect(reserve["lower"],reserve["size"],"#263b33","#7b9b7c")
    x,y=point(reserve["lower"]);text(x+8,y-48,"Reserva "+("oeste" if i==0 else "nordeste"),14)
    text(x+8,y-27,"5 × 3",13);road(reserve["access"],"#91b195",5,True)
for lot in survey["lots"]:
    road(lot["road"],"#bca375" if lot["id"] in {"bastion","barracks"} else "#987351",11)
road(survey["freightRoad"],"#bc9569",10)
names={"bastion":"Bastión","sawmill":"Aserradero","barracks":"Cuartel","granary":"Granero","stoneworks":"Cantera"}
for lot in survey["lots"]:
    foot,size=lot["foot"],lot["size"]
    lower={k:foot[k]-.5*size[k] for k in ["x","y"]}
    rect(lower,size,"#544a3b","#d1ad71")
    x,y=point(foot);text(x-12*size["x"]+5,y-3,names[lot["id"]],14)
    x,y=point(lot["door"]);node("circle",cx=x,cy=y,r=4,fill="#ffe1a3")
for k,label in [("junction","Pozo"),("freight","Cruce de acarreo")]:
    x,y=point(survey[k]);node("circle",cx=x,cy=y,r=4,fill="#eacb8b");text(x+8,y+18,label,13)
text(28,575,"Punto dorado · umbral o cruce      Trazo continuo · vía actual      Trazo discontinuo · acceso reservado",15)
text(28,605,"Revisión "+survey["source"][:12],12,"#c0b9a6")
ET.ElementTree(svg).write(sys.argv[2],encoding="utf-8",xml_declaration=True)
