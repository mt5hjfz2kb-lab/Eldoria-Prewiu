from PIL import Image, ImageDraw, ImageFont
import cv2,numpy as np, json, pathlib, math, zipfile, hashlib
ROOT=pathlib.Path('/mnt/data/eldoria_p0_art_directed')
(ROOT/'masks').mkdir(parents=True,exist_ok=True)
REF=Image.open('/mnt/data/eldoria_world_canon/03_vista_panoramica.png').convert('RGB')
W,H=REF.size
# Map the image's central geological rift into the actual Unity position.
# Image x=255,y=421 becomes relative world X=8,Z=61.
CX,CY=255.,421.
SX,SZ=425./220.,690./293.
WX0,WX1=-910.,910.
WZ0,WZ1=-780.,780.
RES=1024
X=np.linspace(WX0,WX1,RES,dtype=np.float32)
Z=np.linspace(WZ1,WZ0,RES,dtype=np.float32)  # top row is world north
xx,zz=np.meshgrid(X,Z)
px=((xx-8)/SX+CX).astype(np.float32)
py=((61-zz)/SZ+CY).astype(np.float32)
a=np.asarray(REF)
# Pure masks, NO scenic reference pixels are used as runtime art.
remap=lambda m: cv2.remap(m.astype(np.uint8),px,py,cv2.INTER_LINEAR,borderMode=cv2.BORDER_CONSTANT,borderValue=0)
# Region silhouette traces the concept; outer north/west are intentionally fog, not detached gameplay islands.
outline=[(185,137),(220,124),(253,132),(290,139),(326,152),(355,170),(387,187),(415,213),(449,237),(468,266),(469,292),(453,312),(472,335),(473,364),(483,398),(472,422),(488,456),(491,494),(483,526),(469,555),(471,590),(462,620),(450,647),(431,671),(400,684),(367,693),(344,713),(309,729),(275,744),(243,738),(207,731),(175,743),(148,739),(115,730),(86,709),(63,687),(45,660),(37,630),(28,593),(29,560),(20,527),(29,484),(24,456),(20,415),(25,380),(25,337),(34,308),(28,280),(39,250),(44,219),(61,193),(95,168),(122,150),(156,142)]
land=Image.new('L',(W,H),0);ld=ImageDraw.Draw(land);ld.polygon(outline,fill=255)
land0=np.array(land)
landworld=remap(land0)
# Forests from semantic material colors (dark evergreen blue-green), not copied painted pixels.
hsv=cv2.cvtColor(a,cv2.COLOR_RGB2HSV)
r,g,b=[a[:,:,i].astype(np.int16) for i in range(3)]
rawforest=((g>r*0.96) & (g>b*.78)& (r<153) & (g<160)& (hsv[:,:,1]>49)& (hsv[:,:,0]>=27)& (hsv[:,:,0]<=102))
# Restrict to authored landscape and avoid dark decorations in title/legend.
rawforest=(rawforest & (land0>0)).astype(np.uint8)*255
forest_base=cv2.morphologyEx(rawforest,cv2.MORPH_OPEN,np.ones((2,2),np.uint8))
forestworld=remap(forest_base)
forestworld=cv2.GaussianBlur(forestworld,(0,0),2.2)
forestworld=(forestworld*.9).astype(np.uint8)
# Add explicitly artist-defined woodland chains at fixed world coordinates;
# these can be moved/edited independently from the source image.
forest_belts=[
 {'name':'bosque_noroeste','width_world':14,'points':[[-389,240],[-329,225],[-252,252],[-190,284]]},
 {'name':'bosque_este_superior','width_world':15,'points':[[225,410],[285,375],[335,296],[390,271]]},
 {'name':'bosque_oeste_mediano','width_world':18,'points':[[-402,60],[-364,3],[-343,-69],[-284,-114]]},
 {'name':'bosque_sur_oeste','width_world':16,'points':[[-375,-355],[-309,-385],[-251,-410],[-186,-438]]},
 {'name':'bosque_sur_este','width_world':13,'points':[[120,-355],[173,-392],[242,-424],[305,-453]]},
]
# Image-inspired river courses in exact editable geographic units (not copied graphics)
rivers=[
 {'name':'río_principal','width_world':5.0,'points':[[-45,651],[-19,585],[14,508],[-28,445],[-59,360],[-98,277],[-122,204],[-184,149],[-198,68],[-224,-15],[-199,-105],[-163,-193],[-132,-269],[-96,-353],[-49,-429],[22,-515],[105,-582],[230,-641],[360,-692]]},
 {'name':'afluente_norte','width_world':2.8,'points':[[247,606],[181,529],[116,481],[49,462],[14,508]]},
 {'name':'afluente_central','width_world':3,'points':[[-360,90],[-302,98],[-256,132],[-198,68]]},
 {'name':'afluente_sureste','width_world':3.3,'points':[[350,-160],[280,-198],[205,-238],[151,-293],[-96,-353]]},
 {'name':'afluente_suroeste','width_world':3.3,'points':[[-388,-314],[-313,-350],[-241,-369],[-163,-193]]}
]
# Road routes deliberately AVOID a four-spoke cross at the Breach. Curved, linked corridors.
roads=[
 {'name':'camino_occidental','width_world':3.4,'points':[[-337,527],[-272,451],[-236,361],[-279,246],[-327,160],[-354,34],[-342,-138],[-273,-260],[-220,-381],[-134,-475]]},
 {'name':'camino_oriental','width_world':3.1,'points':[[336,547],[295,457],[255,366],[291,267],[354,178],[355,67],[326,-56],[285,-188],[300,-308],[367,-400]]},
 {'name':'camino_central_norte','width_world':3.2,'points':[[-276,430],[-198,393],[-103,338],[-26,299],[69,269],[158,325],[255,366]]},
 {'name':'camino_central_sur','width_world':3.0,'points':[[-342,-138],[-275,-172],[-177,-206],[-101,-254],[0,-304],[122,-314],[198,-359],[300,-308]]},
 {'name':'camino_sur','width_world':2.7,'points':[[-220,-381],[-130,-413],[-41,-445],[52,-487],[147,-513],[244,-535],[350,-600]]},
]
rift=[
 {'name':'grieta_principal','width_world':3.1,'points':[[8,61],[0,84],[-6,109],[8,136],[15,176],[11,211]]},
 {'name':'grieta_noroeste','width_world':2.4,'points':[[3,89],[-41,108],[-87,147],[-121,167],[-158,194]]},
 {'name':'grieta_noreste','width_world':2.2,'points':[[10,117],[49,126],[85,164],[123,157],[159,201]]},
 {'name':'grieta_suroeste','width_world':2.5,'points':[[8,61],[-31,47],[-64,14],[-118,-19],[-160,-52]]},
 {'name':'grieta_sur','width_world':3.4,'points':[[8,61],[22,21],[0,-30],[16,-71],[38,-113],[30,-158]]},
 {'name':'grieta_este','width_world':2.1,'points':[[22,21],[75,17],[133,-3],[170,-40],[202,-89]]},
]
# Construct geographic semantic layers at 1024^2 without using the source scene as a billboard.
def point(p):
 return (int(round((p[0]-WX0)/(WX1-WX0)*(RES-1))),int(round((WZ1-p[1])/(WZ1-WZ0)*(RES-1))))
def raster_routes(items,blur=0.6):
 canvas=Image.new('L',(RES,RES),0);d=ImageDraw.Draw(canvas)
 for item in items:
  loc=[point(p) for p in item['points']]
  width=max(1,int(round(item['width_world']/(WX1-WX0)*RES*2)))
  d.line(loc,fill=255,width=width,joint='curve')
  for p in loc:
   rad=width//2;d.ellipse((p[0]-rad,p[1]-rad,p[0]+rad,p[1]+rad),fill=255)
 out=np.array(canvas)
 if blur:out=cv2.GaussianBlur(out,(0,0),blur)
 return out
river=raster_routes(rivers,1.1)
path=raster_routes(roads,0.8)
fault=raster_routes(rift,1.6)
# Reference semantic water source (blue vs green) merges into guided river/lakes.
water_ref=((b>r*1.09)&(b>g*.91)&(hsv[:,:,0]>=85)&(hsv[:,:,0]<=133)&(hsv[:,:,1]>52)).astype(np.uint8)*255
water_ref=cv2.morphologyEx(water_ref,cv2.MORPH_OPEN,np.ones((2,2),np.uint8))
water_ref=remap(water_ref)
# Keep open sea only outside main land. Inside retains lakes, but river linear network is vector-controlled.
water=np.maximum(water_ref,river)
# semantic Breach is central geographical ground basin, never a vertical portal
ridge=Image.new('L',(RES,RES),0);rd=ImageDraw.Draw(ridge)
ridges=[(-335,560,75,40),(-333,-472,77,51),(265,475,57,45),(306,-480,69,35)]
for cx,cz,rx,rz in ridges:
 pc=point([cx,cz]);rpx=round(rx/(WX1-WX0)*RES);rpz=round(rz/(WZ1-WZ0)*RES)
 rd.ellipse([pc[0]-rpx,pc[1]-rpz,pc[0]+rpx,pc[1]+rpz],fill=190)
relief=cv2.GaussianBlur(np.asarray(ridge),(0,0),13)
# Irregular attenuation at coast or covered sea; must never alter existing hotspots.
river=((river.astype(np.float32)/255.)*(landworld>0)).astype(np.float32)
water=np.maximum(water, (river*255).astype(np.uint8))
# Avoid a false geological stain over Valoria's protected gameplay approach.
rr=np.sqrt(xx*xx+zz*zz)
fault=np.where(rr<90,0,fault).astype(np.uint8)
# Transportable mask PNG names & bounds. Each mask is a SEMANTIC LAYER, never a full reference image.
masks={'land':landworld,'forest':forestworld,'water':water,'roads':path,'relief':relief,'rift':fault}
for name,data in masks.items():Image.fromarray(data,mode='L').save(ROOT/'masks'/f'{name}.png')
# Preview in local world geometry (renders labels, not the approved concept as a texture).
canvas=np.zeros((RES,RES,3),np.uint8);canvas[:]=[43,75,93]
val=np.clip((landworld.astype(float)/255)[:,:,None],0,1)
canvas=(canvas*(1-val)+np.array([177,173,106])[None,None,:]*val).astype(np.uint8)
# dry/wet meadow variation from editable colour masks only
foreststrength=(forestworld.astype(float)/255*.95)[:,:,None]
canvas=(canvas*(1-foreststrength)+np.array([55,104,59])[None,None,:]*foreststrength).astype(np.uint8)
shade=(relief.astype(float)/255*.6)[:,:,None]
canvas=(canvas*(1-shade)+np.array([136,117,90])[None,None,:]*shade).astype(np.uint8)
# coast is water, not overlayed reference art
wm=(water.astype(float)/255)[:,:,None];canvas=(canvas*(1-wm)+np.array([29,115,160])[None,None,:]*wm).astype(np.uint8)
rm=(path.astype(float)/255)[:,:,None];canvas=(canvas*(1-rm)+np.array([219,191,130])[None,None,:]*rm).astype(np.uint8)
fm=(fault.astype(float)/255)[:,:,None];canvas=(canvas*(1-fm)+np.array([96,32,112])[None,None,:]*fm).astype(np.uint8)
preview=Image.fromarray(canvas)
draw=ImageDraw.Draw(preview)
# Renderer orientation is same Unity XYZ as masks. Mark gameplay origin and actual Breach.
ox,oz=point([0,0]);bx,bz=point([8,61]);
draw.ellipse((ox-7,oz-7,ox+7,oz+7),outline=(245,245,245),width=2)
draw.ellipse((bx-6,bz-6,bx+6,bz+6),outline=(238,147,244),width=2)
preview.save(ROOT/'geographic_layout_preview_NOT_UNITY.png')
info={'schema':'eldoria.world4x.authored-layout-p0.v1','status':'ART_EXPERIMENT_ONLY_NOT_UNITY_RENDER','unity_world_bounds':{'min_x':WX0,'max_x':WX1,'min_z':WZ0,'max_z':WZ1},'world_alignment':{'rift_world_x':8,'rift_world_z':61,'source_rift_pixel_x':CX,'source_rift_pixel_y':CY,'world_units_per_source_x_pixel':SX,'world_units_per_source_y_pixel':SZ},'mask_resolution':RES,'mask_files':{k:f'masks/{k}.png' for k in masks},'source_reference':'docs/visual-reference/world-map/03_vista_panoramica.png','usage':'Semantic placement only. Never render concept source PNG as ground. Blend layer data into existing Unity hybrid ground, forest LOD, water, roads, relief and breach; protect actual WorldHotspot locations.', 'editable_polylines':{'rivers':rivers,'roads':roads,'rift':rift,'forest_belts':forest_belts},'explicit_relief_knots':[{'x':x,'z':z,'radius_x':rx,'radius_z':rz} for x,z,rx,rz in ridges],'protection':{'no_fake_hotspots':True,'no_save_changes':True,'no_ui_changes':True,'preserve_valoria_approach_radius_world':90}}
(ROOT/'region1_layout.json').write_text(json.dumps(info,indent=2,ensure_ascii=False),encoding='utf8')
(ROOT/'README.md').write_text('''# ELDORIA — P0 region-geography authoring experiment

**This is an independent editable art-direction deliverable, NOT an integrated Unity screenshot or approved game build.** The active PR #100 remains owned by a different chat (ownership revision 11); this pack does not overwrite it, change gameplay, consume runner or purchase credits.

- `region1_layout.json`: editable roads, waterways, rift splines, forest belts and relief anchors. Coordinates are actual World4X local X/Z with the existing Breach at (8,61).
- `masks/*.png`: six **semantic masks** at 1024×1024 (land, forest, water, roads, relief, rift), aligned to `[-910,910] x [-780,780]` world units. All are derived/curated from existing owner reference only for placement; **not** the concept PNG pasted as a ground texture.
- `geographic_layout_preview_NOT_UNITY.png`: structural diagnostic drawing; **not** a game render and must never be shown as such.
- `build_eldoria_p0.py`: reproducible generating script expecting owner reference at `/mnt/data/eldoria_world_canon/03_vista_panoramica.png`. Requires Pillow, cv2, numpy.

To integrate: sample the independent masks at shared world coordinates into the existing RegionOneHybridTerrainArt, RegionalForestImpostors, `SampleHeight`, and sea/road/breach masks as a reversible EXPERIMENT variant. Use the same XYZ and existing hotspots; compare against a genuine unity real-world-camera baseline. Do NOT merge/release without authentic PlayMode captures and owner approval.
''',encoding='utf8')
(ROOT/'build_eldoria_p0.py').write_text(pathlib.Path(__file__).read_text(),encoding='utf8')
archive='/mnt/data/eldoria_p0_art_directed.zip'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
 for p in ROOT.rglob('*'):
  if p.is_file():z.write(p,p.relative_to(ROOT))
print('Created',archive,round(pathlib.Path(archive).stat().st_size/1024),'KB')
for x in ROOT.rglob('*'):
 if x.is_file():print(str(x.relative_to(ROOT)),x.stat().st_size)
