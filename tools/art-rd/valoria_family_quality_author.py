"""V3-minimum family workshop, driven by genuine external artist plans.
Reuse established pinned BlenderMCP transport and immutable V3 artwork.
Architecture is bonded, not warped; geology is constructed as fault planes.
"""
import bpy, bmesh, hashlib, json, math, os, random, sys
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree
ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/'art-source/valoria/production/asset-family-v2'
OUT=BASE/'quality-v3';OUT.mkdir(parents=True,exist_ok=True)
GATE=ROOT/'art-source/valoria/production/first-hero-gate-v1/quality-v3/ValoriaHeroGate.blend'
GATE_HASH=hashlib.sha256(GATE.read_bytes()).hexdigest()
DESIGN_DOCUMENT=json.loads((BASE/'qwen-family-quality-directions.json').read_text())
IS_TEST=bool(DESIGN_DOCUMENT.get('TEST_FIXTURE_NON_AUTHORITATIVE'))
assert not IS_TEST or os.environ.get('ELDORIA_FAMILY_ALLOW_TEST_FIXTURE')=='1','Engineering fixture cannot become artistic production'
PLANS=DESIGN_DOCUMENT['plans']
assert len(PLANS)==7
helper=ROOT/'tools/art-rd/valoria_family_v2_1_mcp_author.py'
namespace={'__file__':str(helper)}
exec(compile(helper.read_text().split('for asset,plan in PLANS.items():')[0],str(helper),'exec'),namespace)
EXECUTE=namespace['execute']
RNG=random.Random(331011)
STONE=[];MATS={};LOG=[];CURRENT=None

def reset():
 bpy.ops.wm.read_factory_settings(use_empty=True)
 with bpy.data.libraries.load(str(GATE),link=False) as (src,dst):dst.materials=list(src.materials)
 global STONE,MATS
 STONE=[bpy.data.materials['Valoria_Limestone_'+str(i)] for i in range(6)]
 MATS={k:bpy.data.materials[n] for k,n in {'trim':'Valoria_Carved_Limestone','mortar':'Valoria_Deep_Joints','iron':'Valoria_Forged_Iron','wood':'Valoria_Weathered_Oak','blue':'Valoria_Royal_Blue_Cloth','gold':'Valoria_Tarnished_Brass'}.items()}
 for key in ('rock','soil'):
  mat=bpy.data.materials.new('Valoria family '+key+' portable PBR');mat.use_nodes=True
  nodes=mat.node_tree.nodes;links=mat.node_tree.links;bs=nodes.get('Principled BSDF')
  for suffix,socket in [('basecolor','Base Color'),('roughness','Roughness'),('normal','Normal')]:
   im=bpy.data.images.load(str(BASE/'quality-textures'/(key+'_'+suffix+'.png')),check_existing=True);im.pack()
   if suffix!='basecolor':im.colorspace_settings.name='Non-Color'
   node=nodes.new('ShaderNodeTexImage');node.image=im
   if suffix=='normal':
    normal=nodes.new('ShaderNodeNormalMap');normal.inputs['Strength'].default_value=.55
    links.new(node.outputs['Color'],normal.inputs['Color']);links.new(normal.outputs['Normal'],bs.inputs['Normal'])
   else:links.new(node.outputs['Color'],bs.inputs[socket])
  MATS[key]=mat

def uv_project(ob,scale=.3):
 mesh=ob.data;uv=mesh.uv_layers.get('WorldScaleUV') or mesh.uv_layers.new(name='WorldScaleUV')
 mesh.update()
 for p in mesh.polygons:
  n=p.normal
  for li in p.loop_indices:
   v=mesh.vertices[mesh.loops[li].vertex_index].co
   if abs(n.z)>=max(abs(n.x),abs(n.y)):a,b=v.x,v.y
   elif abs(n.y)>=abs(n.x):a,b=v.x,v.z
   else:a,b=v.y,v.z
   uv.data[li].uv=(a*scale,b*scale)

def mesh(name,vertices,faces,material,bevel=0):
 data=bpy.data.meshes.new(name);data.from_pydata(vertices,[],faces);data.update()
 ob=bpy.data.objects.new(name,data);bpy.context.scene.collection.objects.link(ob)
 data.materials.append(material);uv_project(ob)
 bm=bmesh.new();bm.from_mesh(data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(data);bm.free()
 if bevel:
  mod=ob.modifiers.new('Quarried arris, preserved structural contacts','BEVEL');mod.width=bevel;mod.segments=2
  mod=bpy.context.object
  ob.modifiers.new('Planar weighted stone normals','WEIGHTED_NORMAL')
 return ob

def box(name,center,size,material=None,bevel=.023,wear=.009):
 x,y,z=center;dx,dy,dz=(d/2 for d in size)
 verts=[(x+a*dx,y+b*dy,z+c*dz) for c in (-1,1) for b in (-1,1) for a in (-1,1)]
 if wear:
  verts=[(a+RNG.uniform(-wear,wear),b+RNG.uniform(-wear,wear),c+RNG.uniform(-wear,wear)) for a,b,c in verts]
 return mesh(name,verts,[(0,2,3,1),(4,5,7,6),(0,1,5,4),(2,6,7,3),(0,4,6,2),(1,3,7,5)],material or RNG.choice(STONE),bevel)

def complete_v3(predicate,offset=(0,0,0),prefix='V3 preserved '):
 with bpy.data.libraries.load(str(GATE),link=False) as (src,dst):dst.objects=list(src.objects)
 # Imported datablocks have no evaluated world matrices/bounds until linked.
 # Evaluate complete source first, then select and bake, then remove context.
 source=[ob for ob in dst.objects if ob is not None]
 for ob in source:bpy.context.scene.collection.objects.link(ob)
 bpy.context.view_layer.update()
 chosen=[ob for ob in source if ob.type=='MESH' and predicate(ob)]
 matrices={ob:ob.matrix_world.copy() for ob in chosen}
 for ob in source:
  if ob in matrices:
   matrix=matrices[ob];ob.parent=None;ob.data=ob.data.copy()
   for v in ob.data.vertices:v.co=matrix@v.co+Vector(offset)
   ob.matrix_world.identity();ob.data.update();ob.name=prefix+ob.name
  else:bpy.data.objects.remove(ob,do_unlink=True)
 assert chosen
 bpy.context.view_layer.update()
 return chosen

def cx(ob):
 points=[ob.matrix_world@Vector(p) for p in ob.bound_box];return (min(p.x for p in points)+max(p.x for p in points))/2

def tower(plan):
 prefixes=('Tower ','Battered tower','Machicolation support','Arrow loop','Royal blue Valoria banner','Valoria lily embroidery','Banner mounting bar','Heraldic lily band')
 chosen=complete_v3(lambda ob:ob.name.startswith(prefixes) and cx(ob)>1.5,(-3.42,-.25,0),'V3 complete watchtower / ')
 assert len(chosen)>300,('incomplete V3 tower',len(chosen))
 LOG.append({'operation':'v3_complete_tower','parts':len(chosen),'source_sha256':GATE_HASH,'whole_component_affine_translation_only':True})
 # The tower keeps all existing relief and crown; artist secondary detail is
 # a masonry threshold and rear ladder, never a deforming taper of global vertices.
 box('Tower rear oak access lintel',(0,1.78,1.75),(1.0,.13,.17),MATS['wood'],.018,0)
 for side in (-1,1):box('Rear ladder rail',(side*.38,1.75,3.5),(.10,.12,4.9),MATS['wood'],.013,0)
 for i in range(13):box('Rear ladder rung',(0,1.72,1.24+i*.36),(.84,.14,.085),MATS['wood'],.012,0)
 LOG.append({'operation':'oak_iron_bracing','ladder_rungs':13})

def wall_run(length,plan,x=0,y=0,angle=0,end_faces=True,loops=True,buttresses=True):
 before=set(bpy.context.scene.objects)
 h=4.48;t=1.25;rowh=plan['stone_height'];nrows=math.ceil(h/rowh);rowh=h/nrows
 loops_x=[-length/2+length*(i+1)/(plan['loop_count']+1) for i in range(plan['loop_count'])] if loops else []
 # Backing follows the same embrasure holes as face stones: true voids.
 spans=[];cursor=-length/2+.085
 for q in loops_x:spans.append((cursor,q-.095));cursor=q+.095
 spans.append((cursor,length/2-.085))
 for a,b in spans:box('Masonry recessed structural backing',((a+b)/2,0,h/2),(b-a,1.08,h),MATS['mortar'],0,0)
 for q in loops_x:
  for lo,hi in ((0,2.48),(3.45,h)):box('Loop backing above or below opening',(q,0,(lo+hi)/2),(.19,1.08,hi-lo),MATS['mortar'],0,0)
 for j in range(nrows):
  z0=j*rowh;z1=(j+1)*rowh
  exclusions=[(q-.095,q+.095) for q in loops_x] if z0<3.45 and z1>2.48 else []
  x0=-length/2-(plan['stone_length']*.47 if j%2 else 0)
  while x0<length/2:
   width=plan['stone_length']*RNG.uniform(.78,1.26);a=max(-length/2,x0);b=min(length/2,x0+width)
   segments=[(a,b)]
   for lo,hi in exclusions:
    new=[]
    for sa,sb in segments:
     if sa<lo:new.append((sa,min(sb,lo)))
     if sb>hi:new.append((max(sa,hi),sb))
    segments=new
   for sa,sb in segments:
    if sb-sa<.06:continue
    for side in (-1,1):box('Bonded ashlar course',((sa+sb)/2,side*.595,(z0+z1)/2),(sb-sa-.018,.15,rowh-.018),bevel=.021,wear=.007)
   x0+=width
 if end_faces:
  for side in (-1,1):
   for j in range(nrows):
    for k in range(2):box('Dressed modular end face',(side*(length/2-.05),(k-.5)*.62,(j+.5)*rowh),(.10,.60,rowh-.018),bevel=.015,wear=.002)
 # Bonded foot rather than monolithic block strip.
 for j in range(math.ceil(length/.9)):
  w=length/math.ceil(length/.9)
  box('Dressed wall foot',(-length/2+(j+.5)*w,0,.17),(w-.013,1.52,.34),bevel=.032)
 # Separate walkway, solid rear parapet and segmented corbelled front crown.
 n=math.ceil(length/.74);w=length/n
 for j in range(n):
  q=-length/2+(j+.5)*w
  box('Walkway dressed coping',(q,0,4.52),(w-.012,1.52,.22),MATS['trim'],.026)
  box('Parapet breastwork course',(q,-.54,4.83),(w-.012,.43,.42),bevel=.02)
  if j%2==0:
   box('Crown corbel',(q,-.76,4.31),(.25,.35,.29),MATS['trim'],.025)
   box('Bonded crenellation',(q,-.54,5.21),(.60,.47,.44),bevel=.036,wear=.015)
 if buttresses:
  count=max(1,round(length/3));positions=[-length/2+length*(i+.5)/count for i in range(count)]
  for q in positions:
   if any(abs(q-k)<.4 for k in loops_x):q-=.5
   projection=plan['buttress_projection']
   for j in range(10):
    z=.175+j*.35;pr=projection*(1-.60*z/3.6)
    box('Bonded tapered buttress',(q,-.68-pr*.5,z),(.57,pr+.12,.337),bevel=.025)
   box('Buttress sloping shoulder',(q,-.75,3.58),(.63,.36,.20),MATS['trim'],.025)
 for q in loops_x:
  for side in (-1,1):box('Embrasure dressed jamb',(q+side*.165,-.68,2.97),(.17,.20,1.14),MATS['trim'],.017)
  box('Arrow slit drip lintel',(q,-.72,3.55),(.55,.28,.13),MATS['trim'],.022)
  box('Arrow slit sill',(q,-.73,2.40),(.52,.29,.14),MATS['trim'],.022)
 created=[o for o in bpy.context.scene.objects if o not in before and o.type=='MESH']
 # Apply one coherent rigid transform, never deform the architectural parts.
 ca,sa=math.cos(angle),math.sin(angle)
 for ob in created:
  for v in ob.data.vertices:
   vx,vy=v.co.x,v.co.y;v.co.x=x+ca*vx-sa*vy;v.co.y=y+sa*vx+ca*vy
  ob.data.update();uv_project(ob)
 LOG.append({'operation':'bonded_masonry','parts':len(created),'length_m':length,'stone_course_height_m':rowh,'true_arrow_loops':len(loops_x),'contact_planes':[-length/2,length/2]})

def wall(plan):wall_run(8,plan)
def corner(plan):
 wall_run(3.32,plan,x=-2.34,loops=False,buttresses=False,end_faces=False)
 wall_run(3.32,plan,y=2.34,angle=math.pi/2,loops=False,buttresses=False,end_faces=False)
 # Join dressed corner quoin pier overlaps structural core, not facades.
 nrows=math.ceil(4.48/plan['stone_height']);rowh=4.48/nrows
 for j in range(nrows):box('Alternating bonded corner quoin',(0,0,(j+.5)*rowh),(1.36,1.36,rowh-.016),bevel=.023)
 box('Corner walkway dressed coping',(0,0,4.52),(1.54,1.54,.22),MATS['trim'],.028)
 for q in [(-.52,-.52),(.52,-.52)]:
  box('Corner bonded parapet',(q[0],q[1],4.83),(.58,.47,.42),bevel=.023)
  box('Corner bonded crenel',(q[0],q[1],5.21),(.58,.47,.44),bevel=.033)
 LOG.append({'operation':'dressed_corner_bond','turn_degrees':90,'wall_walkway_height':4.52})
def connections(plan):
 wall_run(2,plan,loops=False,buttresses=False)
 nrows=math.ceil(4.48/plan['stone_height']);rowh=4.48/nrows
 for j in range(nrows):box('Socket bonded retaining pier',(0,-.78,(j+.5)*rowh),(.67,.63,rowh-.016),bevel=.023)
 LOG.append({'operation':'modular_socket','contact_width_m':2,'end_plane_tolerance_m':.012})

def road(plan):
 slab=plan['road_slab_length'];rows=math.ceil(8/slab);h=8/rows
 # No disk foundation: a shallow connected, flush soil ribbon, closed sides.
 box('Buried earthen roadbed',(0,0,-.13),(4.24,8,.26),MATS['soil'],.025,0)
 for j in range(rows):
  yy=-4+(j+.5)*h;start=-2;col=0
  lengths=[.48,.73,.59,.81,.65,.74]
  while start<2-.05:
   w=min(lengths[(col+2*j)%len(lengths)]*RNG.uniform(.91,1.06),2-start)
   if w<.06:break
   xx=start+w/2;z=.016+RNG.uniform(-.006,.006)
   box('Closely fitted hand dressed flagstone',(xx,yy,z),(w-.021,h-.021,.065),bevel=.018,wear=.009)
   start+=w;col+=1
 # Irregular earthen verge staying below road plane, no raised curb.
 for side in (-1,1):
  verts=[]
  for j in range(25):
   yy=-4+8*j/24;outer=2.22+.09*math.sin(j*1.7)+.07*math.sin(j*.8)
   verts.extend([(side*1.94,yy,-.009),(side*outer,yy,-.07)])
  faces=[(2*j,2*j+1,2*j+3,2*j+2) for j in range(24)]
  mesh('Exposed continuous soil verge',verts,faces,MATS['soil'])
 LOG.append({'operation':'fitted_flagstone_road','rows':rows,'supported':True,'module_m':[4,8],'maximum_relief_m':.025})

def rock_mass(name,center,size,seed,material=None):
 r=random.Random(seed);n=7;verts=[]
 for z,rad in [(-.5,.78),(-.06,1),(.40,.68)]:
  for i in range(n):
   a=math.tau*i/n;rr=rad*r.uniform(.88,1.1)
   verts.append((center[0]+math.cos(a)*size[0]*rr,center[1]+math.sin(a)*size[1]*rr,center[2]+(z+r.uniform(-.055,.055))*size[2]))
 faces=[tuple(range(n-1,-1,-1)),tuple(range(2*n,3*n))]
 for k in range(2):
  for i in range(n):faces.append((k*n+i,k*n+(i+1)%n,(k+1)*n+(i+1)%n,(k+1)*n+i))
 return mesh(name,verts,faces,material or MATS['rock'],.018)

def cliff(plan):
 lean=plan['fault_lean'];depth=plan['fracture_depth'];N=96;levels=28
 verts=[]
 # Angular primary mass, longitudinal faults. Continuous topology without
 # concentric rings or sinusoidal cake shelves. Shared top and base edges.
 anchors=[0,.31,-.27,.12,-.16,0]
 def front(x,u):
  v=min(5,max(0,(x+6+.58*(1-u))/12*5));i=min(4,int(v));f=v-i
  shoulder=anchors[i]*(1-f)+anchors[i+1]*f
  groove=-depth*max(0,1-abs((v+.17)%1-.5)*7)
  # Slope continuous rock backward near crown; fractures change over depth.
  return -1.8-lean*(1-u)+shoulder+groove*(.65+.35*u)
 for k in range(levels+1):
  u=k/levels
  for i in range(N+1):
   x=-6+12*i/N;z=-3.5+3.5*u
   z+=.035*math.sin(x*1.3)*(1-u)
   verts.append((x,front(x,u),z))
 faces=[]
 for k in range(levels):
  for i in range(N):
   a=k*(N+1)+i;faces.extend([(a,a+1,a+N+2),(a,a+N+2,a+N+1)])
 # Crown rear support is genuinely connected to cliff face, no floating slab.
 rear0=len(verts);verts.extend([(-6,2,0),(6,2,0),(-6,2,-3.5),(6,2,-3.5)])
 top=levels*(N+1)
 faces.append(tuple([top+i for i in range(N+1)]+[rear0+1,rear0]))
 faces.append((0,top,rear0,rear0+2));faces.append((N,rear0+3,rear0+1,top+N))
 faces.append(tuple([i for i in range(N,-1,-1)]+[rear0+2,rear0+3]))
 faces.append((rear0,rear0+1,rear0+3,rear0+2))
 bed=mesh('Continuous angular escarpment bedrock',verts,faces,MATS['rock'],.018)
 # Reuse the proven CC0 scan as a sculptural donor, not a pasted texture.
 # Its 1017-vertex worn shape breaks the low-detail prism/organ-pipe ceiling.
 donor_source=BASE/'cliff_rocks/cliff_rocks-v2-1.blend'
 with bpy.data.libraries.load(str(donor_source),link=False) as (src,dst):
  donor_names=[n for n in src.objects if n.startswith('CC0 horizontal integrated bedrock strata')]
  assert donor_names,'Verified CC0 photogrammetric geology source missing'
  dst.objects=[donor_names[0]]
 donor=dst.objects[0];donor_mesh=donor.data.copy();bpy.data.objects.remove(donor,do_unlink=True)
 low=Vector([min(v.co[k] for v in donor_mesh.vertices) for k in range(3)])
 high=Vector([max(v.co[k] for v in donor_mesh.vertices) for k in range(3)]);extent=high-low
 for vertex in donor_mesh.vertices:vertex.co=Vector([(vertex.co[k]-low[k])/extent[k] for k in range(3)])
 # Unequal angular fault masses, each with three geological planes and clipped
 # side facets. Major visible shapes replace the uniform extruded facade.
 widths=[2.75,2.05,3.4,3.8];left=-6
 for i,width in enumerate(widths):
  center=left+width/2;left+=width;v=[];leanx=[.60,-.42,.74,-.55][i]
  if True:
   data=donor_mesh.copy();data.materials.clear();data.materials.append(MATS['rock'])
   rock=bpy.data.objects.new('AI-sculpted CC0 fractured bedrock mass '+str(i),data);bpy.context.scene.collection.objects.link(rock)
   height=3.50+RNG.uniform(-.04,.02);facey=-2.25+RNG.uniform(-.08,.08)
   for vertex in data.vertices:
    px,py,pz=vertex.co
    if i%2:px=1-px
    vertex.co=(max(-6,min(6,center+(px-.5)*width*1.37+leanx*(pz-.35))),
               facey+(py-.5)*2.00+lean*pz,
               -3.52+pz*height)
   for face in data.polygons:face.use_smooth=False
   data.update();uv_project(rock,.3)
   continue
 # Transfer the actual scan surface into ONE connected sculpted cliff face.
 # Orthographic sampling removes the pasted-boulder-over-flat-wall silhouette:
 # every fault mass becomes real relief in the escarpment topology.
 bpy.context.view_layer.update()
 masses=[o for o in bpy.context.scene.objects if o.name.startswith('AI-sculpted CC0 fractured bedrock mass')]
 bvhs=[BVHTree.FromObject(o,bpy.context.evaluated_depsgraph_get()) for o in masses]
 for vertex in list(bed.data.vertices)[:(N+1)*(levels+1)]:
  x,y,z=vertex.co
  hits=[b.ray_cast(Vector((x,-12,z)),Vector((0,1,0)),16)[0] for b in bvhs]
  vertex.co.y=min([y]+[hit.y for hit in hits if hit is not None])
 for ob in masses:bpy.data.objects.remove(ob,do_unlink=True)
 bed.data.update();uv_project(bed,.3)
 soil=plan['crest_soil_depth'];v=[]
 for i in range(N+1):
  x=-6+12*i/N;f=front(x,1)-.012
  # Positive crown offset avoids coplanar soil/bedrock surfaces after export.
  v.extend([(x,f,.065),(x,2,.065),(x,f-.03,-soil),(x,2,-soil)])
 f=[]
 for i in range(N):
  a=4*i;f.extend([(a,a+4,a+5,a+1),(a,a+2,a+6,a+4),(a+1,a+5,a+7,a+3),(a+2,a+3,a+7,a+6)])
 f.extend([(0,1,3,2),(4*N,4*N+2,4*N+3,4*N+1)])
 mesh('Continuous soil and grass cliff crest',v,f,MATS['soil'],.009)
 clusters=plan['talus_clusters']
 for j in range(clusters):
  x=-5.3+j*10.6/(clusters-1)+RNG.uniform(-.15,.15)
  for k in range(6):
   s=1.0 if k==0 else RNG.uniform(.25,.60)
   rock_mass('Embedded angular talus cluster',(x+RNG.uniform(-.7,.7),-2.65+RNG.uniform(-.25,.35),-3.34+s*.15),(s*.86,s*.72,s*.86),3310+j*31+k)
 LOG.append({'operation':'fault_plane_escarpment','continuous_vertices':len(verts),'primary_fault_slabs':len(widths),'scan_to_connected_surface_transfer':True,'real_cc0_scanned_masses':len(widths),'cc0_source':'https://opengameart.org/content/free-photogrammetric-rock','donor_blend_sha256':hashlib.sha256(donor_source.read_bytes()).hexdigest(),'fault_lean_m':lean,'fracture_depth_m':depth,'embedded_talus_clusters':clusters,'soil_depth_m':soil,'footing_plane_z':.065})

def props(plan):
 labels=('Royal blue Valoria banner','Valoria lily embroidery','Heraldic lily band','Banner mounting bar','Torch pedestal','Torch stone cap','Carved ember flame','Iron brazier bowl')
 chosen=complete_v3(lambda o:o.name.startswith(labels),prefix='V3 complete props / ')
 # Move the banner AND attached embroidery as one design group.
 for ob in chosen:
  isbanner=any(k in ob.name for k in ('banner','embroidery','lily band','mounting bar'))
  sign=1 if sum(v.co.x for v in ob.data.vertices)/len(ob.data.vertices)>0 else -1
  for v in ob.data.vertices:
   if isbanner:v.co.x-=sign*1.35;v.co.z-=2.1
   else:v.co.x-=sign*.90;v.co.y+=1.55
 for side in (-1,1):
  bars=[o for o in chosen if 'Banner mounting bar' in o.name and (sum(v.co.x for v in o.data.vertices)>0)==(side>0)]
  cloth=[o for o in chosen if 'Royal blue Valoria banner' in o.name and (sum(v.co.x for v in o.data.vertices)>0)==(side>0)]
  top=max(v.co.z for o in bars for v in o.data.vertices)+.12
  behind=max(v.co.y for o in cloth for v in o.data.vertices)+.13
  box('Banner freestanding oak mount',(side*2.07,behind,top/2),(.15,.17,top),MATS['wood'],.014,0)
  box('Forged mount foot plate',(side*2.07,behind,.08),(.56,.54,.16),MATS['iron'],.02,0)
  box('Forged banner mounting collar',(side*2.07,behind,top-.19),(.20,.23,.13),MATS['iron'],.012,0)
 for i in range(5):
  box('Oak defensive stake',((i-2)*.54,1.65,.86),(.16,.20,1.72),MATS['wood'],.016,0)
 for z in (.58,1.23):box('Oak defensive crossbar',(0,1.57,z),(2.5,.14,.15),MATS['wood'],.018,0)
 LOG.append({'operation':'v3_complete_heraldry_and_braziers','original_meshes':len(chosen),'gold_embroideries_preserved':True,'brazier_bowls_preserved':True})

BUILD={'wall_straight':wall,'watchtower':tower,'wall_corner':corner,'wall_connections':connections,'stone_path_platform':road,'cliff_rocks':cliff,'defensive_props':props}

def setup():
 sc=bpy.context.scene;sc.render.engine='CYCLES';sc.cycles.samples=int(os.environ.get('ELDORIA_FAMILY_SAMPLES','40'));sc.cycles.use_denoising=True
 sc.render.resolution_x=1100;sc.render.resolution_y=850;sc.render.resolution_percentage=100;sc.render.image_settings.file_format='PNG'
 sc.view_settings.view_transform='AgX';sc.view_settings.look='None';sc.render.film_transparent=False
 world=bpy.data.worlds.new('V3 matched studio environment');world.use_nodes=True
 world.node_tree.nodes['Background'].inputs['Color'].default_value=(.17,.22,.31,1);world.node_tree.nodes['Background'].inputs['Strength'].default_value=.5;sc.world=world
 for name,pos,energy,color,size in [('Sunset key',(-7,-10,14),1800,(1,.77,.53),7),('Sky fill',(6,-1,11),1150,(.49,.64,1),6),('Warm rim',(-4,7,10),1700,(1,.71,.48),6)]:
  light=bpy.data.lights.new(name,'AREA');ob=bpy.data.objects.new(name,light);sc.collection.objects.link(ob);ob.location=pos
  ob.rotation_euler=(Vector((0,0,2))-ob.location).to_track_quat('-Z','Y').to_euler();light.energy=energy;light.color=color;light.shape='DISK';light.size=size
 cam=bpy.data.cameras.new('Family review camera');ob=bpy.data.objects.new('Family review camera',cam);sc.collection.objects.link(ob);cam.type='ORTHO';sc.camera=ob

def bounds():
 points=[o.matrix_world@Vector(p) for o in bpy.context.scene.objects if o.type=='MESH' for p in o.bound_box]
 return [(min(v[i] for v in points),max(v[i] for v in points)) for i in range(3)]

def render(folder,label):
 sc=bpy.context.scene
 scale={'watchtower':13.3,'wall_straight':11.7,'wall_corner':11.7,'wall_connections':8,'stone_path_platform':12.4,'cliff_rocks':18,'defensive_props':10.4}[CURRENT]
 target=Vector((0,0,-1.4) if CURRENT=='cliff_rocks' else (0,0,.3) if CURRENT=='stone_path_platform' else (0,0,3.5) if CURRENT=='watchtower' else (0,0,2.5))
 for view,pos,scale in [('hero',(13,-19,13),scale),('strategic',(16,-23,18),scale*1.35)]:
  cam=sc.camera;cam.location=target+Vector(pos);cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=scale
  sc.render.filepath=str(folder/(label+'-'+view+'.png'));bpy.ops.render.render(write_still=True)

def export(asset,folder):
 sc=bpy.context.scene;meshes=[o for o in sc.objects if o.type=='MESH']
 # Source stays individually editable. Render-only environment is not exported.
 # Do not ship unused reference textures/materials in every editable source.
 used={m for ob in meshes for m in ob.data.materials if m is not None}
 for mat in list(bpy.data.materials):
  if mat not in used:bpy.data.materials.remove(mat)
 for im in list(bpy.data.images):
  if im.users==0:bpy.data.images.remove(im)
 bpy.ops.wm.save_as_mainfile(filepath=str(folder/(asset+'.blend')))
 deps=bpy.context.evaluated_depsgraph_get()
 for o in meshes:
  mod=o.modifiers.new('Portable explicit triangulation for tangent normals','TRIANGULATE')
  if hasattr(mod,'keep_custom_normals'):mod.keep_custom_normals=True
  bpy.context.view_layer.update()
  o.data=bpy.data.meshes.new_from_object(o.evaluated_get(deps),preserve_all_data_layers=True,depsgraph=deps);o.modifiers.clear()
  bm=bmesh.new();bm.from_mesh(o.data)
  bmesh.ops.triangulate(bm,faces=[f for f in bm.faces if len(f.verts)>4])
  bm.to_mesh(o.data);bm.free();o.data.update()
 # V3 patina copies often use exactly the same linked shader. Merge only
 # identical effective PBR values/images, not visually different limestone.
 def signature(mat):
  if not mat.use_nodes:return ('flat',tuple(mat.diffuse_color))
  values=[]
  for node in mat.node_tree.nodes:
   if node.type=='OUTPUT_MATERIAL':continue
   entry=[node.type]
   if node.type=='TEX_IMAGE' and node.image:
    im=node.image;entry.append(hashlib.sha256(im.packed_file.data).hexdigest() if im.packed_file else im.filepath)
   for inp in node.inputs:
    if inp.is_linked or not hasattr(inp,'default_value'):continue
    value=inp.default_value
    try:value=list(value)
    except TypeError:pass
    if isinstance(value,(str,int,float,bool,list)):entry.append([inp.name,value])
   values.append(entry)
  return json.dumps(sorted(values,key=str),sort_keys=True)
 canonical={}
 for ob in meshes:
  for i,mat in enumerate(ob.data.materials):
   key=signature(mat);canonical.setdefault(key,mat);ob.data.materials[i]=canonical[key]
 # Merge only identical-material geometry for mobile draw calls, preserving source.
 groups={}
 for ob in meshes:
  key=tuple(m.name for m in ob.data.materials);groups.setdefault(key,[]).append(ob)
 for group in groups.values():
  if len(group)<2:continue
  bpy.ops.object.select_all(action='DESELECT')
  for o in group:o.select_set(True)
  bpy.context.view_layer.objects.active=group[0];bpy.ops.object.join()
 meshes=[o for o in sc.objects if o.type=='MESH'];bpy.ops.object.select_all(action='DESELECT')
 for ob in meshes:ob.select_set(True)
 bpy.context.view_layer.objects.active=meshes[0]
 bpy.ops.export_scene.gltf(filepath=str(folder/(asset+'.glb')),export_format='GLB',use_selection=True,export_materials='EXPORT',export_texcoords=True,export_normals=True,export_tangents=True)
 triangles=sum(len(p.vertices)-2 for o in meshes for p in o.data.polygons)
 lods=[]
 for level,ratio in [(1,.60),(2,.36)]:
  backups={o:o.data for o in meshes}
  for ob in meshes:
   ob.data=ob.data.copy();bpy.context.view_layer.objects.active=ob
   # Preserve thin heraldry, mounting metal and emissive flame silhouettes.
   if any(any(token in mat.name.lower() for token in ('brass','blue_cloth','ember','flame','forged_iron')) for mat in ob.data.materials):continue
   mod=ob.modifiers.new('Distance LOD '+str(level),'DECIMATE');mod.ratio=ratio;mod.use_collapse_triangulate=True
   bpy.ops.object.modifier_apply(modifier=mod.name)
  count=sum(len(p.vertices)-2 for o in meshes for p in o.data.polygons)
  bpy.ops.export_scene.gltf(filepath=str(folder/(asset+'-LOD'+str(level)+'.glb')),export_format='GLB',use_selection=True,export_materials='EXPORT',export_texcoords=True,export_normals=True,export_tangents=True)
  lods.append({'level':level,'triangles':count,'relative_to_lod0':round(count/triangles,3),'visual_distance_use':'requires engine-screen-size selection; not physical-device certified'})
  for ob,data in backups.items():ob.data=data
 result={'asset':asset,'triangles':triangles,'export_meshes':len(meshes),'effective_materials':len(canonical),'lods':lods,'bounds_m':bounds(),'glb_bytes':(folder/(asset+'.glb')).stat().st_size,'source_gate_sha256':GATE_HASH,'authoring_evidence':'TEST_FIXTURE_NON_AUTHORITATIVE' if IS_TEST else 'GENUINE_EXTERNAL_QWEN_PLAN_BEFORE_BLENDER_MCP','ai_plan':PLANS[asset],'executed_operations':LOG,'art_pass':False,'unity_pass':False}
 (folder/'evidence.json').write_text(json.dumps(result,indent=2))
 return result

manifest={}
selected=os.environ.get('ELDORIA_FAMILY_ASSETS','').split(',') if os.environ.get('ELDORIA_FAMILY_ASSETS') else list(BUILD)
for asset in selected:
 CURRENT=asset
 plan=PLANS[asset];folder=OUT/asset;folder.mkdir(exist_ok=True)
 # Matched old-source capture with same studio illumination and framing.
 old=BASE/asset/(asset+'-v2-1.blend')
 if old.exists():
  bpy.ops.wm.open_mainfile(filepath=str(old))
  for ob in list(bpy.context.scene.objects):
   if ob.type!='MESH':bpy.data.objects.remove(ob,do_unlink=True)
  setup();render(folder,'before')
 reset();LOG=[];CURRENT=asset
 EXECUTE(lambda:BUILD[asset](plan))
 setup();render(folder,'after');manifest[asset]=export(asset,folder)
 print('WORKSHOP_TEST_ONLY' if IS_TEST else 'AI_DIRECTED_FAMILY_QUALITY_PRODUCED',asset,manifest[asset]['triangles'],flush=True)
assert hashlib.sha256(GATE.read_bytes()).hexdigest()==GATE_HASH,'Immutable gate modified'
(OUT/'manifest.json').write_text(json.dumps(manifest,indent=2))
