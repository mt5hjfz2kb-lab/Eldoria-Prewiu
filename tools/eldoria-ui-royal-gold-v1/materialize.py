"""Serialize editable uGUI objects with stable GUIDs. Authored source, NOT native Unity certification."""
from pathlib import Path
import hashlib,json,re,os,shutil
ROOT=Path(__file__).resolve().parents[2]
UI=ROOT/'Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV1'
OUT=ROOT/'docs/ui-royal-gold-v1'
UI.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True)
def guid(path):return hashlib.md5(('eldoria-royal-gold-v1|'+str(path)).encode()).hexdigest()
def file_ref(i=0):return {'fileID':i}
def vec(x=0,y=0):return {'x':x,'y':y}
def col(r=1,g=1,b=1,a=1):return dict(r=r,g=g,b=b,a=a)
def scalar(v):return json.dumps(v,ensure_ascii=False,separators=(',',':'))
def meta(p,folder=False):
 rel=p.relative_to(UI.parent).as_posix();m=Path(str(p)+'.meta')
 if m.exists():
  found=re.search(r'^guid: ([0-9a-f]{32})',m.read_text(),re.M)
  if found:return found.group(1)
 g=guid(rel);body='fileFormatVersion: 2\nguid: '+g+'\n'
 if folder:body+='folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n'
 elif p.suffix=='.cs':body+='MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n'
 elif p.suffix=='.asmdef':body+='AssemblyDefinitionImporter:\n  externalObjects: {}\n'
 elif p.suffix=='.prefab':body+='PrefabImporter:\n  externalObjects: {}\n'
 else:body+='DefaultImporter:\n  externalObjects: {}\n'
 m.write_text(body,encoding='utf8');return g
classes=['UITheme','RoyalFrameGraphic','RoyalIconGraphic','RoyalSpriteGraphic','RoyalSolidGraphic','RoyalText','RoyalButton','ResourceBar','RealmHeader','MissionPanel','BottomNavigation','PrimaryActionButton','BuildingDetailsPanel','BuildingLabel','NotificationIndicator','ModalWindow','RoyalSafeArea','RoyalInputGate','RoyalHUD','RoyalScaler','RoyalRaycaster','RoyalHudLayout']
scripts={}
for n in classes:
 p=UI/'Components'/f'{n}.cs';p.parent.mkdir(parents=True,exist_ok=True);scripts[n]=meta(p)
p=UI/'Preview/RoyalPreviewDriver.cs';p.parent.mkdir(parents=True,exist_ok=True);scripts['RoyalPreviewDriver']=meta(p)
objects=[];serial=1000
class Node:
 def __init__(self,name,parent=None,anchor=(0,1),pos=(0,0),size=(100,100),pivot=(0,1),stretch=False,active=True):
  global serial
  self.name=name;self.parent=parent;self.children=[];self.components=[];self.active=active;self.anchor=anchor;self.pos=pos;self.size=size;self.pivot=pivot;self.stretch=stretch
  serial+=1;self.go=serial;serial+=1;self.rt=serial
  if parent:parent.children.append(self)
  objects.append(self)
 def native(self,kind,props):
  global serial
  serial+=1;self.components.append((kind,serial,None,props));return file_ref(serial)
 def mono(self,name,**props):
  global serial
  serial+=1;self.components.append((114,serial,name,props));return file_ref(serial)
 def graphic(self,name='RoyalFrameGraphic',ray=False,**props):
  if not any(c[0]==222 for c in self.components):self.native(222,dict(m_CullTransparentMesh=1))
  defaults=dict(m_Material=file_ref(),m_Color=col(),m_RaycastTarget=int(ray),m_RaycastPadding=dict(x=0,y=0,z=0,w=0),m_Maskable=1,m_OnCullStateChanged={'m_PersistentCalls':{'m_Calls':[]}});defaults.update(props)
  return self.mono(name,**defaults)
 def button(self,target=None):
  if target is None:target=self.graphic('RoyalSolidGraphic',ray=True,m_Color=col(0,0,0,0))
  return self.mono('RoyalButton',m_Navigation={'m_Mode':3,'m_WrapAround':0,'m_SelectOnUp':file_ref(),'m_SelectOnDown':file_ref(),'m_SelectOnLeft':file_ref(),'m_SelectOnRight':file_ref()},m_Transition=1,m_Colors={'m_NormalColor':col(),'m_HighlightedColor':col(1,.95,.81),'m_PressedColor':col(.75,.75,.8),'m_SelectedColor':col(1,.95,.81),'m_DisabledColor':col(.6,.6,.6),'m_ColorMultiplier':1,'m_FadeDuration':.1},m_Interactable=1,m_TargetGraphic=target,m_OnClick={'m_PersistentCalls':{'m_Calls':[]}})
def text(parent,name,value,x,y,w,h,size=24,align=0,bold=0,color=None,anchor=(0,1),pivot=(0,1)):
 n=Node(name,parent,anchor,(x,y),(w,h),pivot);f=dict(m_Font=file_ref(),m_FontSize=size,m_FontStyle=bold,m_BestFit=0,m_MinSize=20,m_MaxSize=size,m_Alignment=align,m_AlignByGeometry=0,m_RichText=0,m_HorizontalOverflow=0,m_VerticalOverflow=0,m_LineSpacing=1)
 return n.graphic('RoyalText',m_Color=color or col(.95,.90,.8),m_FontData=f,m_Text=value)
def icon(parent,name,kind,x,y,size,anchor=(0,1),pivot=(0,1)):
 n=Node(name,parent,anchor,(x,y),(size,size),pivot);n.graphic('RoyalIconGraphic',Icon=kind,m_Color=col(.84,.71,.46));return n
def button(parent,name,label,x,y,w=140,h=72,anchor=(0,1),pivot=(0,1),kind=None):
 n=Node(name,parent,anchor,(x,y),(w,h),pivot);bg=n.graphic(ray=True,Variant=1,Round=0);b=n.button(bg)
 if kind is None:lab=text(n,'Label',label,10,-8,w-20,h-16,24,4,1)
 else:icon(n,'Symbol',kind,(w-52)/2,-(h-52)/2,52);lab=None
 return n,b,lab
root=Node('RoyalGoldHUD',anchor=(0,0),size=(0,0),pivot=(.5,.5),stretch=True)
root.native(223,dict(serializedVersion=3,m_Enabled=1,m_RenderMode=0,m_Camera=file_ref(),m_PlaneDistance=100,m_PixelPerfect=0,m_ReceivesEvents=1,m_OverrideSorting=0,m_OverridePixelPerfect=0,m_SortingLayerID=0,m_SortingOrder=40,m_TargetDisplay=0,m_AdditionalShaderChannelsFlag=0))
root.native(225,dict(m_Enabled=1,m_Alpha=1,m_Interactable=1,m_BlocksRaycasts=1,m_IgnoreParentGroups=0))
root.mono('RoyalScaler');root.mono('RoyalRaycaster',m_IgnoreReversedGraphics=1,m_BlockingObjects=0,m_BlockingMask={'serializedVersion':2,'m_Bits':0})
safe=Node('SafeArea',root,anchor=(0,0),size=(0,0),pivot=(.5,.5),stretch=True);safe.mono('RoyalSafeArea')
realm=Node('RealmHeader',safe,pos=(26,-22),size=(330,104));realm.graphic(ray=True)
crest=icon(realm,'Valoria crest',0,10,-8,84);icon(crest,'Bastion emblem',9,17,-11,50).components[-1][3]['m_Color']=col(.065,.1,.17)
rname=text(realm,'Realm','Valoria',102,-18,212,34,30,0,1);rpow=text(realm,'Power','—',102,-57,212,28,24)
info=realm.button();realmview=realm.mono('RealmHeader',Realm=rname,Power=rpow,Info=info)
res=Node('ResourceBar',safe,anchor=(.5,1),pos=(0,-22),size=(570,90),pivot=(.5,1));res.graphic(ray=True)
amounts=[]
for i,name in enumerate(['Madera','Piedra','Comida']):
 x=16+i*184;icon(res,name+' icon',i+1,x,-16,52);text(res,name+' name',name,x+58,-12,105,26,20)
 amounts.append(text(res,name+' amount','—',x+58,-40,110,35,26,0,1))
resourceview=res.mono('ResourceBar',Wood=amounts[0],Stone=amounts[1],Food=amounts[2])
settings,settingsbutton,_=button(safe,'Interface settings','',-24,-22,96,96,anchor=(1,1),pivot=(1,1),kind=10)
mission=Node('MissionPanel',safe,pos=(26,-158),size=(316,224));mission.graphic(ray=True)
chapter=text(mission,'Chapter','CAPÍTULO',20,-18,214,28,22,0,1,color=col(.85,.72,.48))
_,toggle,_=button(mission,'Collapse','',-8,-5,66,64,anchor=(1,1),pivot=(1,1),kind=12)
detail=Node('Details',mission,pos=(0,-67),size=(316,144))
objective=text(detail,'Objective','',20,-2,268,62,26)
progress=text(detail,'Progress','',20,-66,180,28,22)
track=Node('Track',detail,pos=(20,-108),size=(276,8));track.graphic('RoyalSolidGraphic',m_Color=col(.16,.19,.23))
fill=Node('Fill',track,anchor=(0,0),pos=(0,0),size=(0,0),pivot=(0,0),stretch=True);fill.graphic('RoyalSolidGraphic',m_Color=col(.77,.62,.36))
_,openmission,_=button(detail,'Open objective','',-10,-48,68,64,anchor=(1,1),pivot=(1,1),kind=12)
missionview=mission.mono('MissionPanel',Chapter=chapter,Objective=objective,ProgressText=progress,Fill=file_ref(fill.rt),Details=file_ref(detail.rt),Toggle=toggle,Open=openmission)
nav=Node('BottomNavigation',safe,anchor=(.5,0),pos=(0,16),size=(920,128),pivot=(.5,0))
items=[]
for i,(label,section,k) in enumerate([('Héroes',1,4),('Ejército',2,5),('Misiones',3,6),('Inventario',4,7),('Mundo',5,8),('Bastión',6,9)]):
 n=Node(label,nav,anchor=(.5,0),pos=(-380+i*152,0),size=(140,128),pivot=(.5,0));circle=Node('Medallion',n,pos=(29,-2),size=(82,82));art=circle.graphic(Variant=1,Round=1);icon(circle,'Emblem',k,13,-13,56)
 lab=text(n,'Label',label,0,-92,140,32,23,4,1);n.graphic('RoyalSolidGraphic',ray=True,m_Color=col(0,0,0,0));b=n.button(art);items.append(dict(Section=section,Button=b,Label=lab))
navview=nav.mono('BottomNavigation',Items=items)
panel=Node('BuildingDetailsPanel',safe,anchor=(1,.5),pos=(-24,-2),size=(414,542),pivot=(1,.5),active=False);panel.graphic(ray=True)
title=text(panel,'Title','',24,-28,278,40,30,0,1);level=text(panel,'Level','',24,-77,356,28,22,0,1,color=col(.85,.72,.48))
_,close,_=button(panel,'Close','',-6,-6,82,82,anchor=(1,1),pivot=(1,1),kind=11)
function=text(panel,'Function','',24,-126,364,72,24);status=text(panel,'Status','',24,-208,360,30,24,0,1)
costs=text(panel,'Costs','',24,-251,364,68,23);requirements=text(panel,'Requirements','',24,-324,364,82,23,color=col(.96,.75,.50));time=text(panel,'Time','',24,-417,360,27,23)
primary,primarybutton,primarylabel=button(panel,'PrimaryAction','',22,-466,370,64)
primaryview=primary.mono('PrimaryActionButton',Button=primarybutton,Label=primarylabel)
panelview=panel.mono('BuildingDetailsPanel',Title=title,Level=level,Function=function,Status=status,Costs=costs,Requirements=requirements,Time=time,Primary=primaryview,Close=close)
feedback=text(safe,'Feedback','',0,-122,650,36,22,4,anchor=(.5,1),pivot=(.5,1))
mark=text(safe,'TestMark','',26,-130,340,22,14,0,color=col(.9,.75,.45))
modal=Node('ModalWindow',root,anchor=(0,0),size=(0,0),pivot=(.5,.5),stretch=True,active=False);modal.graphic('RoyalSolidGraphic',ray=True,m_Color=col(.005,.009,.017,.85))
msafe=Node('SafeArea',modal,anchor=(0,0),size=(0,0),pivot=(.5,.5),stretch=True);msafe.mono('RoyalSafeArea')
window=Node('Window',msafe,anchor=(.5,.5),pos=(0,0),size=(590,580),pivot=(.5,.5));window.graphic(ray=True)
mtitle=text(window,'Title','',30,-36,440,48,32,0,1);mbody=text(window,'Body','',32,-119,516,302,26)
_,mclose,_=button(window,'Close','',-8,-8,88,88,anchor=(1,1),pivot=(1,1),kind=11)
_,mextra,extralabel=button(window,'Primary','',30,-436,530,66)
_,msecond,secondlabel=button(window,'Secondary','',30,-511,530,64)
modalview=modal.mono('ModalWindow',Title=mtitle,Body=mbody,Close=mclose,Extra=mextra,ExtraLabel=extralabel,Secondary=msecond,SecondaryLabel=secondlabel)
hud=root.mono('RoyalHUD',Realm=realmview,Resources=resourceview,Mission=missionview,Navigation=navview,Building=panelview,Modal=modalview,Feedback=feedback,TestMark=mark,Settings=settingsbutton)
safe.mono('RoyalHudLayout',Resources=file_ref(res.rt),Mission=file_ref(mission.rt),Feedback=file_ref(next(n.rt for n in objects if n.name=='Feedback')))
# Serialize independent reusable components as full graphs, never an empty loader object.
labelroot=Node('BuildingLabel',size=(248,70));ln,lb,_=button(labelroot,'Label target','',0,0,248,70);lt=text(ln,'Name','Bastión',8,-9,232,50,24,4,1);labelroot.mono('BuildingLabel',Label=lt,Select=lb,Target=file_ref(),PlayerCamera=file_ref(),Layer=file_ref(),WorldOffset={'x':0,'y':2,'z':0})
notice=Node('NotificationIndicator',size=(40,40));notice.graphic(Variant=1,Round=1);nt=text(notice,'Count','1',0,0,40,40,20,4,1);notice.mono('NotificationIndicator',Count=nt)
def descendants(n):return [n]+[q for c in n.children for q in descendants(c)]
def emit(n,standalone=False):
 lines=['%YAML 1.1','%TAG !u! tag:unity3d.com,2011:'];nodes=descendants(n)
 def block(kind,id,name,props):
  lines.extend([f'--- !u!{kind} &{id}',name+':'])
  for k,v in props.items():lines.append('  '+k+': '+scalar(v))
 def common():return dict(m_ObjectHideFlags=0,m_CorrespondingSourceObject=file_ref(),m_PrefabInstance=file_ref(),m_PrefabAsset=file_ref())
 for obj in nodes:
  p=common();p.update(serializedVersion=6,m_Component=[{'component':file_ref(i)} for i in [obj.rt]+[c[1] for c in obj.components]],m_Layer=5,m_Name=obj.name,m_TagString='Untagged',m_Icon=file_ref(),m_NavMeshLayer=0,m_StaticEditorFlags=0,m_IsActive=1 if standalone and obj==n else int(obj.active));block(1,obj.go,'GameObject',p)
  p=common();p.update(m_GameObject=file_ref(obj.go),m_LocalRotation=dict(x=0,y=0,z=0,w=1),m_LocalPosition=dict(x=0,y=0,z=0),m_LocalScale=dict(x=1,y=1,z=1),m_ConstrainProportionsScale=0,m_Children=[file_ref(c.rt) for c in obj.children],m_Father=file_ref(0 if obj==n else obj.parent.rt),m_RootOrder=0,m_LocalEulerAnglesHint=dict(x=0,y=0,z=0),m_AnchorMin=vec(*obj.anchor),m_AnchorMax=vec(1,1) if obj.stretch else vec(*obj.anchor),m_AnchoredPosition=vec(*obj.pos),m_SizeDelta=vec(*obj.size),m_Pivot=vec(*obj.pivot));block(224,obj.rt,'RectTransform',p)
  for kind,id,script,props in obj.components:
   p=common();p['m_GameObject']=file_ref(obj.go)
   if kind==114:p.update(m_Enabled=1,m_EditorHideFlags=0,m_Script={'fileID':11500000,'guid':scripts[script],'type':3},m_Name='',m_EditorClassIdentifier='')
   p.update(props);block(kind,id,script and 'MonoBehaviour' or {222:'CanvasRenderer',223:'Canvas',225:'CanvasGroup'}[kind],p)
 return '\n'.join(lines)+'\n'
def save(rel,data):
 p=UI/rel;p.parent.mkdir(parents=True,exist_ok=True);p.write_text(data,encoding='utf8');meta(p);return p
prefabs=[root,realm,res,mission,nav,panel,primary,labelroot,notice,modal]
for n in prefabs:save('Prefabs/'+n.name+'.prefab',emit(n,True))
# Preview-only controls are added AFTER the production graph is saved.
test=Node('TEST controls',safe,pos=(26,-424),size=(300,210));test.graphic(ray=True)
text(test,'Test help','PRUEBA · DATOS AISLADOS',12,-12,282,28,18,0,1)
refs={}
for i,(name,field) in enumerate([('Normal','Normal'),('Bloqueado','Locked'),('En curso','Busy'),('Error','Error')]):
 _,b,_=button(test,name,name,12+(i%2)*140,-50-(i//2)*68,134,60);refs[field]=b
picker=Node('TEST selection',safe,anchor=(.5,.5),pos=(0,-110),size=(360,190),pivot=(.5,.5))
text(picker,'Test title','SELECCIÓN DE PRUEBA',0,0,360,30,22,4,1)
for i,(name,field) in enumerate([('Aserradero','Sawmill'),('Cuartel','Barracks'),('Bastión','Bastion'),('Marcha','March')]):
 _,b,_=button(picker,name,name,(i%2)*180,-42-(i//2)*76,170,68);refs[field]=b
root.mono('RoyalPreviewDriver',Hud=hud,**refs)
# The preview camera and one InputSystem EventSystem are created only by the explicit TEST driver.
scene=save('Preview/RoyalGold_UI_TEST.unity',emit(root))
# Original PNGs get real reusable sprite importers; no numbers/text are baked by this serializer.
recovered=ROOT/'tools/eldoria-ui-royal-gold-v1/recovered'
for original in sorted(recovered.glob('*.png')):
 dest=UI/'Sprites/Recovered'/original.name;dest.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(original,dest)
 g=guid(dest.relative_to(UI.parent).as_posix())
 Path(str(dest)+'.meta').write_text(f'''fileFormatVersion: 2\nguid: {g}\nTextureImporter:\n  serializedVersion: 12\n  externalObjects: {{}}\n  mipmaps:\n    enableMipMap: 0\n    sRGBTexture: 1\n  isReadable: 0\n  textureFormat: 1\n  maxTextureSize: 2048\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 1\n    aniso: 1\n    mipBias: 0\n    wrapU: 1\n    wrapV: 1\n    wrapW: 1\n  nPOTScale: 0\n  alphaIsTransparency: 1\n  alphaSource: 1\n  textureType: 8\n  spriteMode: 1\n  spriteMeshType: 0\n  spritePixelsToUnits: 100\n  spriteAlignment: 0\n  spritePivot: {{x: 0.5, y: 0.5}}\n  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}\n''')
for p in list(UI.rglob('*')):
 if p.is_file() and p.suffix!='.meta':meta(p)
for p in sorted([UI]+[p for p in UI.rglob('*') if p.is_dir()]):meta(p,True)
# UI's parent meta is separately owned by this same workstream.
parent_meta=Path(str(UI.parent)+'.meta')
if not parent_meta.exists():parent_meta.write_text('fileFormatVersion: 2\nguid: '+guid('UI')+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n')
report={'source_sha':os.environ.get('GITHUB_SHA'),'run_id':os.environ.get('GITHUB_RUN_ID'),'native_unity':False,'serialized_prefabs':len(prefabs),'test_scene_objects':len(descendants(root)),'graphs':[]}
for p in sorted(UI.rglob('*.prefab'))+[scene]:
 data=p.read_text();ids=re.findall(r'^--- !u!\d+ &(\d+)',data,re.M);assert len(ids)==len(set(ids));known=set(ids)|{'0'}
 for line in data.splitlines():
  if '"guid"' in line:continue
  for id in re.findall(r'"fileID":(\d+)',line):assert id in known,(p,id)
 report['graphs'].append({'path':p.relative_to(ROOT).as_posix(),'objects':len(ids),'sha256':hashlib.sha256(data.encode()).hexdigest()})
report['object_reference_graph']='PASS';(OUT/'serialization-check.json').write_text(json.dumps(report,indent=2))
print('SERIALIZED_UI_GRAPH_PASS',len(prefabs),'prefabs;',len(descendants(root)),'test objects; native Unity NOT claimed')
