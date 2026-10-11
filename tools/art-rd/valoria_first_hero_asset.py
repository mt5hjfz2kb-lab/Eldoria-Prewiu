"""One reference-led Valoria gate, editable meshes and portable PBR. Blender 4.2+.
Run: blender -b -t 8 --python tools/art-rd/valoria_first_hero_asset.py
Optional ELDORIA_HERO_OUTPUT, ELDORIA_HERO_PHASE=refine, ELDORIA_BLENDER_MCP_ADDON.
"""
import bpy, math, random, os, json, hashlib, sys
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=Path(os.environ.get('ELDORIA_HERO_OUTPUT',ROOT/'art-source/valoria/production/first-hero-gate-v1'))
OUT.mkdir(parents=True,exist_ok=True)
TEX=ROOT/'art-source/valoria/production/first-hero-gate-v1/textures'
PHASE=os.environ.get('ELDORIA_HERO_PHASE','initial')
random.seed(2319)
bpy.ops.wm.read_factory_settings(use_empty=True)
asset=bpy.data.collections.new('VALORIA_HERO_GATE');bpy.context.scene.collection.children.link(asset)
def own(o):
    for c in list(o.users_collection):c.objects.unlink(o)
    asset.objects.link(o);return o
def mat(name,col,rough=.8,metal=0,texture=None):
    m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=(*col,1)
    bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(*col,1)
    bs.inputs['Roughness'].default_value=rough;bs.inputs['Metallic'].default_value=metal
    if texture:
        for channel,suffix in [('Base Color','basecolor'),('Roughness','roughness'),('Normal','normal')]:
            im=bpy.data.images.load(str(TEX/(texture+'_'+suffix+'.png')),check_existing=True)
            if suffix!='basecolor':im.colorspace_settings.name='Non-Color'
            im.pack();node=m.node_tree.nodes.new('ShaderNodeTexImage');node.image=im
            if suffix=='normal':
                nm=m.node_tree.nodes.new('ShaderNodeNormalMap');nm.inputs['Strength'].default_value=.38
                m.node_tree.links.new(node.outputs['Color'],nm.inputs['Color']);m.node_tree.links.new(nm.outputs['Normal'],bs.inputs[channel])
            elif suffix=='basecolor':
                # Bake per-material tint into image so GLB renderer preserves it.
                import numpy as np
                clone=im.copy();clone.name=name+'_albedo';px=np.empty(len(im.pixels),dtype=np.float32);im.pixels.foreach_get(px);px=px.reshape(-1,4)
                px[:,:3]*=np.array(col)/.28;clone.pixels.foreach_set(px.ravel());clone.pack();node.image=clone
                m.node_tree.links.new(node.outputs['Color'],bs.inputs[channel])
            else:m.node_tree.links.new(node.outputs['Color'],bs.inputs[channel])
    return m
stones=[mat('Valoria_Limestone_'+str(i),(.28*f,.274*f,.261*f),texture='stone') for i,f in enumerate([.88,.94,.98,1,1.03,1.07])]
trim=mat('Valoria_Carved_Limestone',(.31,.295,.272),texture='stone')
mortar=mat('Valoria_Deep_Joints',(.15,.142,.131),.95)
oak=mat('Valoria_Weathered_Oak',(.28,.28,.28),texture='oak')
iron=mat('Valoria_Forged_Iron',(.055,.063,.075),.63,.68)
gold=mat('Valoria_Tarnished_Brass',(.43,.28,.085),.48,.67)
blue=mat('Valoria_Royal_Blue_Cloth',(.018,.067,.205),.96)
char=mat('Valoria_Loophole_Interior',(.018,.019,.021),.95)
rock=mat('Valoria_Foundation_Rock',(.21,.215,.224),texture='stone')
def mesh(name,v,f,m,bevel=0):
    d=bpy.data.meshes.new(name);d.from_pydata(v,[],f);d.update();o=bpy.data.objects.new(name,d);asset.objects.link(o);d.materials.append(m)
    if bevel:
        b=o.modifiers.new('Chiselled arris','BEVEL');b.width=bevel;b.segments=2
        o.modifiers.new('Weighted face normals','WEIGHTED_NORMAL')
    return o
def box(name,p,s,m,bevel=.025,wear=False):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=own(bpy.context.object);o.name=name;o.dimensions=s
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(m)
    if wear:
        for v in o.data.vertices:
            v.co.x+=random.uniform(-.018,.018);v.co.z+=random.uniform(-.013,.013);v.co.y+=random.uniform(-.013,.013)
    if bevel:
        b=o.modifiers.new('Worn stone edges','BEVEL');b.width=bevel;b.segments=2;o.modifiers.new('Weighted face normals','WEIGHTED_NORMAL')
    return o
def courses(name,x0,x1,y,z0,z1,depth=.18):
    z=z0;row=0
    while z<z1-.01:
        h=min(.37+random.uniform(-.028,.028),z1-z);x=x0-(.39 if row%2 else 0)
        while x<x1:
            w=random.uniform(.60,.92);a=max(x,x0);b=min(x+w,x1)
            if b-a>.065:box(name,((a+b)/2,y+random.uniform(-.008,.008),z+h/2),(b-a-.013,depth,h-.017),random.choice(stones),.014,True)
            x+=w
        z+=h;row+=1
def tower(cx):
    # Continuous structural volume with shallow masonry skins: no disconnected blocks.
    box('Tower structural core',(cx,.25,3.25),(2.77,3.01,6.5),mortar,.035)
    courses('Tower front ashlar',cx-1.46,cx+1.46,-1.30,0,6.47)
    courses('Tower rear ashlar',cx-1.46,cx+1.46,1.80,0,6.47)
    # Side courses authored in XY then rotated; consistent world-sized stone units.
    before=set(asset.objects)
    courses('Tower return ashlar',-1.3,1.80,0,0,6.47)
    for o in set(asset.objects)-before:
        old=o.location.x;o.location.x=cx+1.46;o.location.y=old;o.rotation_euler.z=math.pi/2
    before=set(asset.objects);courses('Tower inner return ashlar',-1.3,1.80,0,0,6.47)
    for o in set(asset.objects)-before:
        old=o.location.x;o.location.x=cx-1.46;o.location.y=old;o.rotation_euler.z=math.pi/2
    box('Battered tower foot',(cx,.25,.30),(3.15,3.46,.60),trim,.045)
    box('Tower waist stringcourse',(cx,.25,4.53),(3.05,3.29,.16),trim,.025)
    box('Tower corbelled crown',(cx,.25,6.47),(3.16,3.49,.25),trim,.036)
    box('Walkable tower terrace',(cx,.25,6.57),(2.98,3.25,.12),stones[2],.025)
    # Defensive parapet is a continuous chest-height wall with narrow crenel tops.
    for y in (-1.37,1.87):box('Tower parapet base',(cx,y,6.87),(3.13,.33,.61),stones[3],.025)
    for x in (cx-1.40,cx+1.40):box('Tower parapet return',(x,.25,6.87),(.33,3.12,.61),stones[3],.025)
    for i in range(4):
        xx=cx-1.18+i*.79
        for yy in (-1.37,1.87):
            box('Tower crenel lower course',(xx,yy,7.30),(.52,.42,.25),random.choice(stones),.023,True)
            box('Tower crenel cap',(xx,yy,7.55),(.50,.41,.26),trim,.025,True)
    for xx in (cx-1.40,cx+1.40):
        for yy in (-.55,.24,1.03):
            box('Tower return crenel',(xx,yy,7.43),(.41,.52,.51),stones[3],.027,True)
    # Front arrow loops use small dark cavities and angled stone surrounds.
    for zz in (2.42,5.38):
        box('Arrow loop shadow',(cx,-1.416,zz),(.12,.012,.58),char,.005)
        box('Arrow loop sill',(cx,-1.44,zz-.32),(.28,.16,.10),trim,.01)
    # Quoin continuity, larger alternating corners with restrained relief.
    for zz in [i*.74+.36 for i in range(9)]:
        for side in (-1,1):
            box('Corner quoin',(cx+side*1.22,-1.385,zz),(.44,.22,.35),trim,.024,True)
    for i in range(5):
        box('Machicolation support',(cx-1.12+i*.56,-1.41,6.25),(.19,.38,.24),trim,.019)
    # Hanging cloth is an actual waved, tapered mesh, blue/gold per approved reference.
    v=[];nx=10;nz=18
    for j in range(nz+1):
        t=j/nz;z=6.55-2.25*t
        for i in range(nx+1):
            u=i/nx;edge=(.87 if t<.78 else .87*(1-.25*(t-.78)/.22))
            xx=cx+(u-.5)*edge;yy=-1.56-.035*math.sin(u*math.tau*1.6+t*2)-.05*t*t
            v.append((xx,yy,z+.10*math.sin(u*math.pi)*(t**8)))
    faces=[]
    for j in range(nz):
        for i in range(nx):a=j*(nx+1)+i;faces.append((a,a+1,a+nx+2,a+nx+1))
    cloth=mesh('Royal blue Valoria banner',v,faces,blue);solid=cloth.modifiers.new('Cloth thickness','SOLIDIFY');solid.thickness=.014
    # Stylized heraldic lily, silhouette made of curves rather than a square logo.
    def leaf(dx,dz,angle=0):
        shape=[(0,.18),(.07,.08),(.055,-.09),(0,-.19),(-.055,-.09),(-.07,.08)]
        vv=[(cx+dx+x*math.cos(angle)-z*math.sin(angle),-1.685,5.47+dz+x*math.sin(angle)+z*math.cos(angle)) for x,z in shape]
        mesh('Valoria lily embroidery',vv,[tuple(range(6))],gold)
    leaf(0,.12);leaf(-.14,.01,-.7);leaf(.14,.01,.7)
    box('Heraldic lily band',(cx,-1.695,5.31),(.31,.013,.065),gold,.012)
    box('Banner mounting bar',(cx,-1.53,6.60),(1.04,.07,.07),iron,.015)
for cx in (-3.42,3.42):tower(cx)
# Gateway: actual opening through a vaulted wall and deep radial stone arch.
radius=1.72;spring=2.68;top=5.78
for side in (-1,1):
    x0,x1=(-2.00,-radius) if side<0 else (radius,2.00)
    box('Portal continuous jamb core',((x0+x1)/2,.25,spring/2),(x1-x0,2.2,spring),mortar,.02)
    courses('Portal jamb dressed ashlar',x0,x1,-.98,0,spring,.23)
segments=24
for i in range(segments):
    a0=math.pi*i/segments;a1=math.pi*(i+1)/segments
    x0=radius*math.cos(a0);x1=radius*math.cos(a1);z0=spring+radius*math.sin(a0);z1=spring+radius*math.sin(a1)
    vv=[(x0,-.85,z0),(x1,-.85,z1),(x1,-.85,top),(x0,-.85,top),(x0,1.35,z0),(x1,1.35,z1),(x1,1.35,top),(x0,1.35,top)]
    mesh('Continuous vaulted portal',vv,[(0,1,2,3),(7,6,5,4),(0,4,5,1),(3,2,6,7),(0,3,7,4),(1,5,6,2)],stones[3])
def arch_layer(name,r,width,y,depth):
    for i in range(17):
        a0=math.pi*i/17+.002;a1=math.pi*(i+1)/17-.002;v=[]
        for yy in (y-depth/2,y+depth/2):
            for rr,a in [(r,a0),(r+width,a0),(r+width,a1),(r,a1)]:v.append((rr*math.cos(a),yy,spring+rr*math.sin(a)))
        mesh(name,v,[(0,1,2,3),(7,6,5,4),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)],trim,.018)
arch_layer('Load-bearing carved voussoir',radius,.36,-1.12,.35)
arch_layer('Recessed vault stone',radius-.04,.15,-.60,.50)
# Dressed spandrel courses follow the arch, instead of leaving a broad flat slab.
for row in range(5):
    za=4.0+row*.355;zb=min(za+.335,top)
    if zb<=za:continue
    x=-1.91-(.32 if row%2 else 0)
    while x<1.91:
        a=max(-1.91,x);b=min(1.91,x+.63)
        def edge(xx):return max(za,spring+math.sqrt(max(0,(radius+.37)**2-xx*xx)))
        ba=edge(a);bb=edge(b)
        if min(ba,bb)<zb-.045:
            ba=min(ba,zb-.025);bb=min(bb,zb-.025)
            vv=[(a+.007,-1.01,ba),(b-.007,-1.01,bb),(b-.007,-1.01,zb),(a+.007,-1.01,zb),(a+.007,-.87,ba),(b-.007,-.87,bb),(b-.007,-.87,zb),(a+.007,-.87,zb)]
            mesh('Arch spandrel fitted stone',vv,[(0,1,2,3),(7,6,5,4),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)],random.choice(stones),.012)
        x+=.63
for s in (-1,1):
    for i in range(7):box('Arch pilaster course',(s*(radius+.17),-1.13,.20+i*.39),(.34,.37,.37),trim,.017,True)
box('Portal crown coping',(0,.25,5.85),(4.0,2.44,.22),trim,.025)
box('Portal defensive parapet',(0,-.93,6.11),(3.84,.30,.37),stones[3],.02)
for i in range(5):box('Portal modest merlon',(-1.56+i*.78,-.94,6.50),(.49,.39,.45),trim,.021,True)
# Rastrillo: readable negative spaces; lifted middle region allows view through vault.
for i in range(13):
    x=-1.56+i*.26;ceiling=spring+math.sqrt(max(0,radius*radius-x*x))
    bottom=2.13+.12*abs(x)
    box('Iron portcullis vertical',(x,-.44,(ceiling+bottom)/2),(.075,.10,ceiling-bottom),iron,.012)
    mesh('Forged pointed portcullis tooth',[(x-.037,-.50,bottom),(x+.037,-.50,bottom),(x,-.50,bottom-.16),(x-.037,-.40,bottom),(x+.037,-.40,bottom),(x,-.40,bottom-.16)],[(0,2,1),(3,4,5),(0,1,4,3),(1,2,5,4),(2,0,3,5)],iron)
for zz in (2.52,3.10,3.65):
    width=2*math.sqrt(max(0,radius**2-max(0,zz-spring)**2))
    box('Portcullis crossbar',(0,-.42,zz),(width,.12,.075),iron,.014)
# Open weathered oak doors are tucked inside the actual passage.
for side in (-1,1):
    for i in range(7):
        o=box('Open oak gate leaf',(side*1.59,.10+i*.21,1.3),(.12,.195,2.60),oak,.017)
    for z in (.47,1.77):box('Forged oak hinge strap',(side*1.52,.73,z),(.045,1.49,.12),iron,.012)
# Small flanking connection stubs keep library boundaries explicit.
for side in (-1,1):
    cx=side*5.25;box('Wall socket continuous core',(cx,.30,1.92),(1.0,1.36,3.84),mortar,.03)
    courses('Wall socket stone',cx-.5,cx+.5,-.41,0,3.84,.19)
    courses('Wall socket back stone',cx-.5,cx+.5,1.02,0,3.84,.19)
    box('Wall socket coping',(cx,.30,3.91),(1.08,1.65,.21),trim,.024)
    for dx in (-.31,.31):box('Wall socket crenel',(cx+dx,.30,4.25),(.30,1.54,.49),stones[3],.025,True)
# Plinth, entry paving and a few integral foundation fragments; no whole-world stage.
for side in (-1,1):
    box('Torch pedestal',(side*2.24,-2.04,.40),(.65,.66,.80),stones[2],.03)
    box('Torch stone cap',(side*2.24,-2.04,.85),(.77,.78,.17),trim,.03)
    bpy.ops.mesh.primitive_cone_add(vertices=12,radius1=.18,radius2=.31,depth=.21,location=(side*2.24,-2.04,1.02));o=own(bpy.context.object);o.name='Iron brazier bowl';o.data.materials.append(iron)
fire=mat('Brazier amber flame',(.95,.19,.016),.65)
bs=fire.node_tree.nodes.get('Principled BSDF');bs.inputs['Emission Color'].default_value=(1,.19,.01,1);bs.inputs['Emission Strength'].default_value=4
for side in (-1,1):
    # Tapered tongues with a bent tip, not pink glowing spheres.
    for i in range(3):
        cx=side*2.24+(i-1)*.10;cy=-2.04;bottom=1.12;h=.38 if i==1 else .27
        vv=[(cx-.085,cy-.05,bottom),(cx+.085,cy-.05,bottom),(cx+.06,cy+.05,bottom),(cx-.06,cy+.05,bottom),(cx+.055,cy-.015,bottom+h)]
        mesh('Carved ember flame',vv,[(0,1,2,3),(0,4,1),(1,4,2),(2,4,3),(3,4,0)],fire,.018)
    d=bpy.data.lights.new('Warm entry pool','POINT');d.energy=150;d.color=(1,.35,.075);d.shadow_soft_size=.50
    o=bpy.data.objects.new('Warm entry pool',d);bpy.context.collection.objects.link(o);o.location=(side*2.24,-2.04,1.44)
# UV mapping in metres: equivalent surface density across stone sizes and returns.
for o in asset.objects:
    if o.type!='MESH':continue
    uv=o.data.uv_layers.new(name='WorldScaleUV')
    for p in o.data.polygons:
        axis=max(range(3),key=lambda i:abs(p.normal[i]))
        pair=[i for i in range(3) if i!=axis]
        for li in p.loop_indices:
            vv=o.matrix_world@o.data.vertices[o.data.loops[li].vertex_index].co
            uv.data[li].uv=(vv[pair[0]]/.85,vv[pair[1]]/.85)
# Scene: restrained warm key/cool fill, reference view, actual geometry shadows.
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True
scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG'
scene.world=bpy.data.worlds.new('Cool dusk studio');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.17,.22,.31,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.45
def light(name,p,col,power,size):
    d=bpy.data.lights.new(name,'AREA');d.energy=power;d.color=col;d.shape='DISK';d.size=size
    o=bpy.data.objects.new(name,d);scene.collection.objects.link(o);o.location=p;o.rotation_euler=(Vector((0,0,3))-o.location).to_track_quat('-Z','Y').to_euler()
light('Sunset key',(-7,-10,14),(1,.77,.53),1800,7)
light('Sky fill',(6,-1,11),(.49,.64,1),1150,8)
light('Warm rim',(-4,7,10),(1,.71,.48),1700,6)
bpy.ops.object.camera_add(location=(13,-19,13));camera=bpy.context.object;camera.name='Reference strategic camera';scene.camera=camera;camera.data.type='ORTHO'
scene.view_settings.view_transform='AgX'
# Neutral ground is render context and deliberately excluded from asset export.
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.025));floor=bpy.context.object;floor.name='RENDER_CONTEXT_ONLY';floor.data.materials.append(mat('Studio ground',(.105,.123,.15),.92))
def render(name,pos=(13,-19,13),scale=15.5,size=(1280,960),target=(0,0,3.5)):
    camera.location=pos;camera.rotation_euler=(Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.ortho_scale=scale
    scene.render.resolution_x=size[0];scene.render.resolution_y=size[1];scene.render.filepath=str(OUT/(name+'.png'));bpy.ops.render.render(write_still=True)
if PHASE=='refine':
    # Main-thread BlenderMCP transport, adapted from studio's pinned rock-contact recipe.
    # Scoped correction chosen only after the initial actual renders were inspected.
    import importlib.util,socket,threading,time
    addon=Path(os.environ['ELDORIA_BLENDER_MCP_ADDON'])
    assert hashlib.sha256(addon.read_bytes()).hexdigest()=='eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5'
    spec=importlib.util.spec_from_file_location('blender_mcp',addon);module=importlib.util.module_from_spec(spec);sys.modules['blender_mcp']=module;spec.loader.exec_module(module)
    for feature in ('polyhaven','hyper3d','sketchfab','polypizza','hunyuan3d','tripo'):setattr(bpy.types.Scene,'blendermcp_use_'+feature,bpy.props.BoolProperty(default=False))
    changed={}
    def correct():
        # Seat and soften the foundation mass, deepen void readability, weather crown.
        for o in asset.objects:
            if o.name.startswith('Tower crenel') or o.name.startswith('Tower return crenel'):
                o.location.z-=.07;changed['crenels_seated']=changed.get('crenels_seated',0)+1
            if o.name.startswith('Corner quoin'):
                o.scale.y*=.83;o.location.y+=.045;changed['quoin_relief_reduced']=changed.get('quoin_relief_reduced',0)+1
        bpy.context.view_layer.update()
    bpy.app.driver_namespace['eldoria_hero_directed_refinement']=correct
    server=module.BlenderMCPServer(host='127.0.0.1',port=0);server.socket=socket.socket(socket.AF_INET,socket.SOCK_STREAM);server.socket.bind(('127.0.0.1',0));port=server.socket.getsockname()[1];server.socket.listen(1);server.running=True
    server.server_thread=threading.Thread(target=server._server_loop,daemon=True);server.server_thread.start();response=[];errors=[]
    def client():
        try:
            conn=socket.create_connection(('127.0.0.1',port),timeout=5);conn.settimeout(40);conn.sendall(json.dumps({'type':'execute_code','params':{'code':"import bpy\nbpy.app.driver_namespace['eldoria_hero_directed_refinement']()"}}).encode());data=b''
            while True:
                data+=conn.recv(65536)
                try:response.append(json.loads(data));break
                except json.JSONDecodeError:pass
            conn.close()
        except Exception as e:errors.append(str(e))
    thread=threading.Thread(target=client,daemon=True);thread.start();deadline=time.monotonic()+45
    while thread.is_alive() and time.monotonic()<deadline:server._drain_command_queue();time.sleep(.05)
    thread.join(timeout=1);assert not errors and response and response[0].get('status')=='success' and changed,(errors,response)
    server.stop();(OUT/'mcp-report.json').write_text(json.dumps({'transport':'Pinned BlenderMCP TCP main-thread queue','addon_sha256':hashlib.sha256(addon.read_bytes()).hexdigest(),'changes':changed,'response':response,'automatic_art_acceptance':False},indent=2))
render('hero-'+PHASE)
render('strategic-'+PHASE,(16,-23,18),24,(1280,720),(0,0,4.0))
render('mobile-'+PHASE,(16,-23,18),24,(640,360),(0,0,4.0))
render('front-'+PHASE,(0,-24,7),14,(1280,960),(0,0,3.5))
render('rear-'+PHASE,(-13,19,13),15.5)
render('detail-'+PHASE,(5,-12,7),7.1,(1000,1000),(1.8,-.6,3.7))
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'ValoriaHeroGate.blend'))
# Export joined by material for practical draw calls; editable source remains unjoined.
objects=[o for o in asset.objects if o.type=='MESH']
depsgraph=bpy.context.evaluated_depsgraph_get()
for o in objects:
    # Evaluate modifiers without thousands of global scene-selection updates.
    if o.modifiers:
        evaluated=o.evaluated_get(depsgraph)
        d=bpy.data.meshes.new_from_object(evaluated,preserve_all_data_layers=True,depsgraph=depsgraph)
        o.modifiers.clear();o.data=d
groups={}
for o in objects:groups.setdefault(o.data.materials[0].name,[]).append(o)
for group in groups.values():
    if len(group)<2:continue
    bpy.ops.object.select_all(action='DESELECT')
    for o in group:o.select_set(True)
    bpy.context.view_layer.objects.active=group[0];bpy.ops.object.join()
objects=[o for o in asset.objects if o.type=='MESH'];bpy.ops.object.select_all(action='DESELECT')
for o in objects:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(OUT/'ValoriaHeroGate.glb'),export_format='GLB',use_selection=True,export_apply=True,export_texcoords=True,export_normals=True,export_materials='EXPORT')
report={'phase':PHASE,'reference_sha256':hashlib.sha256((ROOT/'references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg').read_bytes()).hexdigest(),'triangles':sum(len(p.vertices)-2 for o in objects for p in o.data.polygons),'mesh_count':len(objects),'materials':len(groups),'blender_version':bpy.app.version_string,'glb_bytes':(OUT/'ValoriaHeroGate.glb').stat().st_size,'paid_credits':0,'unity_verified':False,'art_acceptance':'requires actual director review','mcp_executed':PHASE=='refine'}
(OUT/'production-report.json').write_text(json.dumps(report,indent=2));print('ELDORIA_HERO_ASSET_EXPORTED',json.dumps(report))
