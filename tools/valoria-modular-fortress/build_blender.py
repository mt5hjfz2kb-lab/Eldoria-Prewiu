# Eldoria reference-first Blender-native module study (isolated, zero credits)
# Blender 4.3-5.2, native meshes + procedural nodes, no add-ons/purchases.
# This generator is independent of the conversation's already-exported GLB pack.
import bpy, math, random, json, os, sys
from pathlib import Path
from mathutils import Vector
R=random.Random(4415)
root=Path(os.environ.get("RUNNER_TEMP",os.getcwd()))/"valoria_modular_blender"
root.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
colors={"stone":(.53,.48,.42,1),"light":(.67,.59,.48,1),"dark":(.31,.29,.30,1),"rock":(.28,.29,.36,1),
"grass":(.36,.40,.17,1),"dirt":(.38,.28,.19,1),"wood":(.25,.13,.065,1),"iron":(.08,.09,.11,1),
"gold":(.85,.51,.12,1),"blue":(.022,.11,.45,1),"pine":(.045,.16,.095,1),"leaf":(.17,.30,.095,1),
"water":(.026,.19,.31,1),"fire":(1,.23,.018,1)}
mats={}
for name,c in colors.items():
    m=bpy.data.materials.new("ELDORIA_"+name);m.diffuse_color=c;m.use_nodes=True
    n=m.node_tree.nodes;links=m.node_tree.links
    bs=next(x for x in n if x.type=="BSDF_PRINCIPLED")
    bs.inputs["Base Color"].default_value=c
    bs.inputs["Roughness"].default_value=.29 if name=="water" else (.43 if name=="iron" else .86)
    bs.inputs["Metallic"].default_value=.8 if name in ("gold","iron") else 0
    if name not in ("fire","water"):
        tex=n.new("ShaderNodeTexNoise");tex.inputs["Scale"].default_value=24 if name in ("stone","light","dark","rock") else 15
        tex.inputs["Detail"].default_value=4
        bump=n.new("ShaderNodeBump");bump.inputs["Strength"].default_value=.16 if name in ("stone","light","dark","rock") else .08
        bump.inputs["Distance"].default_value=.035 if name in ("stone","rock") else .014
        links.new(tex.outputs["Fac"],bump.inputs["Height"]);links.new(bump.outputs["Normal"],bs.inputs["Normal"])
    if name=="fire":
        bs.inputs["Emission Color"].default_value=c
        bs.inputs["Emission Strength"].default_value=7
    mats[name]=m
groups={};active=None
def new(name):
    global active
    active=name;groups[name]=[]
def keep(o,material=None):
    if material:o.data.materials.append(mats[material])
    groups[active].append(o);return o
def cube(p,dim,mat="stone",bev=.028):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=active+"__"+mat;o.dimensions=dim
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bev:
        mod=o.modifiers.new("Stone edge light catch","BEVEL");mod.width=min(bev,min(dim)*.13);mod.segments=2
        mod.limit_method="ANGLE"
    return keep(o,mat)
def cylinder(p,r,h,mat="wood",verts=8):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=h,location=p)
    return keep(bpy.context.object,mat)
def cone(p,r,h,mat="pine",verts=12):
    bpy.ops.mesh.primitive_cone_add(vertices=verts,radius1=r,radius2=0,depth=h,location=(p[0],p[1],p[2]+h/2))
    return keep(bpy.context.object,mat)
def beam(a,b,r=.08,mat="wood"):
    av=Vector(a);bv=Vector(b);mid=(av+bv)/2
    bpy.ops.mesh.primitive_cylinder_add(vertices=8,radius=r,depth=(bv-av).length,location=mid)
    o=bpy.context.object;o.rotation_euler=(bv-av).to_track_quat('Z','Y').to_euler()
    return keep(o,mat)
def brick(x,y,z,w=.83,d=.60,h=.43,i=0):
    o=cube((x,y,z+h/2),(w,d,h),["stone","light","stone","dark"][i%4],.036)
    o.rotation_euler[2]=R.uniform(-.014,.014);return o
def wall(length=4,height=3,broken=False,crenel=False):
    rows=int(height/.46); n=7
    for j in range(rows):
        for i in range(n):
            if broken and i>4 and j>3:continue
            brick(-length/2+(i+.5)*length/n+(j%2)*.08,0,j*.46,length/n-.022,.71,.43,i+j)
    if crenel:
        for i in range(0,7,2):brick(-length/2+(i+.5)*length/7,0,height,.54,.77,.68,i)
def arch_segment(r,a,b,d,mat):
    v=[]
    for y in (-d/2,d/2):
        for rad,ang in ((r,a),(r+.42,a),(r+.42,b),(r,b)):
            v.append((rad*math.cos(ang),y,rad*math.sin(ang)+r+.42))
    f=[(0,1,2),(0,2,3),(4,6,5),(4,7,6)]
    for j in range(4):
        k=(j+1)%4;f.extend(((j,k,k+4),(j,k+4,j+4)))
    mesh=bpy.data.meshes.new("radial masonry");mesh.from_pydata(v,[],f);mesh.update()
    ob=bpy.data.objects.new(active+"__voussoir",mesh);bpy.context.collection.objects.link(ob);keep(ob,mat)
def arch(r=1.5):
    for i in range(13):
        a=math.pi*i/13+.003;b=math.pi*(i+1)/13-.003
        arch_segment(r,a,b,1.04,["stone","light","stone"][i%3])
    for x in (-r-.25,r+.25):
        for j in range(int((r+.45)/.43)):brick(x,0,j*.44,.65,1.06,.42,j)
def leaf_cluster(scale):
    for j in range(9):
        t=j*2.4;rr=.32+R.random()*.24
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=scale*.34,location=(math.cos(t)*rr,math.sin(t)*rr,scale*.36+R.random()*.2))
        keep(bpy.context.object,"leaf")
def make(n):
    new(n)
    if n.startswith("stone_ashlar"):brick(0,0,0,i=int(n[-2:]))
    elif n=="stone_half_block":brick(0,0,0,.43,.58,.43)
    elif n=="stone_slab_cap":brick(0,0,0,1,.70,.25)
    elif n=="stone_corner_quoin":brick(0,0,0,.90,.90,.48)
    elif n=="mortar_joint_recess":cube((0,0,.2),(1.8,.05,.38),"dark",0)
    elif n=="wall_course_4m":
        for i in range(5):brick(-1.6+i*.8,0,0,.77,.75,.44,i)
    elif n.startswith("wall_panel"):wall(4,3,"ruined" in n,"crenellated" in n)
    elif n=="wall_corner_90":
        wall()
        before=groups[n].copy()
        for ob in before:
            ob.location.x+=1.55
            dup=ob.copy();dup.data=ob.data.copy();bpy.context.collection.objects.link(dup)
            p=Vector(ob.location)-Vector((1.55,0,0))
            dup.location=( -p.y, p.x+1.55,p.z)
            dup.rotation_euler.z+=math.pi/2;groups[n].append(dup)
    elif n=="wall_buttress":
        for j in range(7):brick(0,-.08*j,j*.42,1.2-j*.035,1.5-j*.08,.43,j)
    elif n.startswith("merlon"):
        for j in range(3 if "tall" in n else 1):brick(0,0,j*.4,.61,.73,.40,j)
    elif n=="arrow_slit_frame":
        cube((0,0,.66),(.85,.21,1.32),"light")
        cube((0,-.13,.65),(.11,.10,.90),"iron",.009)
    elif n=="parapet_walk_4m":
        wall(4,3,False,True);cube((0,0,.07),(4.2,1.2,.16),"stone")
    elif n.startswith("tower_square"):
        h=8.3 if "tall" in n else 5.7
        for j in range(int(h/.48)):
            for side in range(4):
                angle=side*math.pi/2
                for i in range(5):
                    px=-1.8+i*.9;py=-1.95
                    xx=px*math.cos(angle)-py*math.sin(angle)
                    yy=px*math.sin(angle)+py*math.cos(angle)
                    ob=brick(xx,yy,j*.48,.89,.60,.46,i+j);ob.rotation_euler.z+=angle
        cube((0,0,h),(4.30,4.30,.22),"light")
        for side in range(4):
            for i in range(0,5,2):
                angle=side*math.pi/2;px=-1.8+i*.9;py=-1.95
                xx=px*math.cos(angle)-py*math.sin(angle);yy=px*math.sin(angle)+py*math.cos(angle)
                ob=brick(xx,yy,h,.61,.81,.73,i);ob.rotation_euler.z+=angle
    elif n.startswith("arch_"):
        r=2.0 if "major" in n else 1.35
        if n.startswith("arch_complete"):arch(r)
        else:arch_segment(r,math.pi/2-.15,math.pi/2+.15,1.0,"light")
    elif n.startswith("portcullis"):
        w=3.4 if "large" in n else 2.4;h=3.4 if "large" in n else 2.6
        for i in range(int(w/.32)+1):cube((-w/2+i*.32,0,h/2),(.085,.14,h),"iron",.01)
        for j in range(5):cube((0,0,.38+j*.60),(w,.18,.10),"iron",.01)
    elif n=="reinforced_oak_gate":
        for i in range(8):cube((-.87+i*.25,0,1.42),(.24,.22,2.85),"wood",.028)
        for z in (.50,2.2):cube((0,-.17,z),(3.1,.10,.10),"iron",.013)
    elif n.startswith("paver_"):cube((0,0,.08),(1.25 if "flagstone" in n else (.52 if "short" in n else .90),.68,.15),"stone",.038)
    elif n=="stair_step":cube((0,0,.12),(4,.65,.24),"light",.034)
    elif n=="staircase_flight_12":
        for j in range(12):cube((0,-3.4+j*.61,.12+j*.24),(4,.64,.24),"light",.024)
        for x in (-2.2,2.2):
            for j in range(8):brick(x,-3.4+j*.86,.22+j*.34,.55,.80,.44,j)
    elif n=="bridge_span_4m":
        cube((0,0,.13),(5,4,.27),"stone")
        for x in (-2.4,2.4):
            for j in range(7):brick(x,-1.8+j*.6,.50,.55,.59,.50,j)
    elif n=="arch_bridge_support":arch(1.4)
    elif n.startswith("timber_beam"):
        cube((0,0,1.5 if n.endswith("1") else 2.2),(.22,.22,3 if n.endswith("1") else 4.4),"wood",.008)
    elif n=="timber_brace_X":
        beam((-1.3,0,0),(1.3,0,3));beam((1.3,0,0),(-1.3,0,3))
    elif n.startswith("scaffold"):
        h=6.5 if "watch" in n else 3.6
        for x in (-1.4,1.4):
            for y in (-1.1,1.1):cube((x,y,h/2),(.21,.22,h),"wood",.015)
        for z in (1.8,h-1.1):
            for y in (-1.1,1.1):beam((-1.4,y,z),(1.4,y,z))
            for x in (-1.4,1.4):beam((x,-1.1,z),(x,1.1,z))
        cube((0,0,h-1.1),(3.5,2.8,.20),"wood")
    elif n.startswith("banner"):
        w=1.30 if "royal" in n else .82;h=2.5 if "royal" in n else 1.6
        cube((0,0,h/2),(w,.06,h),"blue",.018)
        cube((0,-.052,h*.60),(w*.11,.055,h*.38),"gold",.008)
        for sign in (-1,1):cone((sign*w*.21,-.06,h*.51),w*.10,h*.22,"gold",6)
        cube((0,0,h+.1),(w+.3,.16,.15),"wood",.010)
    elif n=="brazier_pedestal":
        for j in range(3):brick(0,0,j*.36,.76,.76,.34,j)
        cylinder((0,0,1.15),.37,.20,"iron")
        for j in range(4):
            a=j*math.pi/2;cone((math.cos(a)*.14,math.sin(a)*.14,1.23),.16,.47,"fire",8)
    elif n=="torch_wall":
        cube((0,0,.5),(.16,.16,.95),"wood")
        cylinder((0,0,1.05),.19,.13,"iron")
        cone((0,0,1.08),.18,.55,"fire")
    elif n.startswith("pine"):
        h={"pine_small":5,"pine_medium":7.4,"pine_tall":10.5}[n]
        cylinder((0,0,h*.36),.15,h*.73,"wood")
        for j in range(5):cone((0,0,h*(.13+j*.14)),(h*.23)*(1-j*.16),h*.38,"pine",11)
    elif n.startswith("boulder") or n.startswith("cliff"):
        r=2.6 if n=="cliff_rock_column" else (.8 if "chip" in n else 1.1)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=r,location=(0,0,r*.49))
        ob=keep(bpy.context.object,"rock");ob.scale=(.9,.69,1.25 if n=="cliff_rock_column" else .63)
    elif n.startswith("shrub"):leaf_cluster(1.0)
    elif n.startswith("grass_tuft"):
        for j in range(11):
            angle=R.random()*math.tau;x=R.uniform(-.25,.25);y=R.uniform(-.25,.25)
            cone((x,y,0),.035,.18+R.random()*.3,"grass",3)
    elif n=="cobble_pebbles":
        for j in range(25):
            ang=j*2.4;rad=R.random()*.7
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=.075+R.random()*.06,location=(rad*math.cos(ang),rad*math.sin(ang),.07))
            keep(bpy.context.object,"rock")
    elif n=="dirt_ground_patch":cube((0,0,.018),(3.4,3.4,.035),"dirt",0)
    elif n=="grass_ground_patch":cube((0,0,.018),(4,4,.035),"grass",0)
    elif n=="river_water_tile":cube((0,0,.022),(4,4,.04),"water",0)
    elif n=="river_foam_line":cube((0,0,.03),(2.1,.085,.02),"light",0)
    else:cube((0,0,.20),(.7,.7,.4),"stone")
names="""arch_bridge_support arch_complete_major arch_complete_minor arch_keystone_major arch_keystone_minor arch_voussoir_major arch_voussoir_minor arrow_slit_frame banner_royal_blue banner_small boulder_1 boulder_2 boulder_3 brazier_pedestal bridge_span_4m cliff_rock_column cliff_stone_chip cobble_pebbles dirt_ground_patch grass_ground_patch grass_tuft_1 grass_tuft_2 merlon_low merlon_tall mortar_joint_recess parapet_walk_4m paver_flagstone paver_rect_long paver_rect_short pine_medium pine_small pine_tall portcullis_large portcullis_small reinforced_oak_gate river_foam_line river_water_tile scaffold_small scaffold_watch_platform shrub_1 shrub_2 shrub_3 stair_step staircase_flight_12 stone_ashlar_01 stone_ashlar_02 stone_ashlar_03 stone_ashlar_04 stone_corner_quoin stone_half_block stone_slab_cap timber_beam_1 timber_beam_2 timber_brace_X torch_wall tower_square_short tower_square_tall wall_buttress wall_corner_90 wall_course_4m wall_panel_crenellated_4m wall_panel_regular_4m wall_panel_ruined_4m""".split()
assert len(names)==63
for index,n in enumerate(names):
    make(n)
    bpy.ops.object.select_all(action="DESELECT")
    for o in groups[n]:o.select_set(True)
    if groups[n]:bpy.context.view_layer.objects.active=groups[n][0]
    bpy.ops.export_scene.gltf(filepath=str(root/(n+".glb")),export_format='GLB',use_selection=True,export_apply=True)
    print("ASSET",index+1,"/63",n,len(groups[n]),flush=True)
# Geometry authored in native Blender. Compose a bounded showcase, reusing shared module designs.
def instance(name,loc,ang=0,scale=1):
    for source in groups[name]:
        ob=source.copy();ob.data=source.data;ob.hide_render=False;ob.rotation_euler.z+=ang
        ob.location=Vector(loc)+source.location*scale
        ob.scale=source.scale*scale
        bpy.context.collection.objects.link(ob)
        return_objects.append(ob)
return_objects=[]
# Keep donor source hidden from the visual scene only.
for group in groups.values():
    for o in group:o.hide_render=True
instance('grass_ground_patch',(0,0,0),0,18)
for i in range(12):
    x=-24+i*4.25;instance('wall_panel_crenellated_4m',(x,13,0))
for name,x,y in [('tower_square_tall',0,15),('tower_square_short',-13,15),('tower_square_short',13,15),
                 ('tower_square_short',-7,-14),('tower_square_short',7,-14)]:
    instance(name,(x,y,0))
for y in (-13,-9,-5,0,5,10):
    for x in (-2,0,2):instance('paver_flagstone',(x,y,0))
for x in (-4.5,4.5):
    instance('banner_royal_blue',(x,-14,5))
    instance('brazier_pedestal',(x,-12,0))
for i in range(10):
    x=-24+(i*7)%49;y=16+R.uniform(-2,8)
    instance('pine_'+['small','medium','tall'][i%3],(x,y,0),R.random()*6.28)
for i in range(32):
    x=R.uniform(-25,25);y=R.uniform(-18,16)
    if abs(x)<3:continue
    instance('shrub_'+str(i%3+1),(x,y,0),R.random()*6.28,R.uniform(.6,1.2))
# lights and real Blender Eevee render
world=bpy.data.worlds.new('Eldoria blue dusk');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.28,.42,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.75
scene=bpy.context.scene;scene.world=world
def light(kind,position,power,rgb,size):
    ld=bpy.data.lights.new("Eldoria "+kind,kind);ld.energy=power;ld.color=rgb
    if kind=='AREA':ld.shape='DISK';ld.size=size
    lo=bpy.data.objects.new(ld.name,ld);bpy.context.collection.objects.link(lo)
    lo.location=position
    if kind=='AREA':lo.rotation_euler=(Vector((0,0,0))-lo.location).to_track_quat('-Z','Y').to_euler()
light('AREA',(-25,-36,52),4400,(1,.78,.58),32)
light('AREA',(28,13,39),3000,(.56,.75,1),39)
for x in (-4.5,4.5):light('POINT',(x,-12,2),420,(1,.39,.09),0)
camd=bpy.data.cameras.new("Valoria reference pose");cam=bpy.data.objects.new('Valoria reference pose',camd)
bpy.context.collection.objects.link(cam);cam.location=(48,-66,47);cam.rotation_euler=(Vector((0,0,2))-cam.location).to_track_quat('-Z','Y').to_euler()
camd.type='ORTHO';camd.ortho_scale=75;scene.camera=cam
try:scene.render.engine='BLENDER_EEVEE_NEXT'
except Exception:scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=1536;scene.render.resolution_y=1024;scene.render.resolution_percentage=100
scene.render.filepath=str(root/'blender_scene.png');scene.render.image_settings.file_format='PNG'
bpy.ops.wm.save_as_mainfile(filepath=str(root/'valoria_modular_library.blend'))
bpy.ops.render.render(write_still=True)
manifest={'asset_count':len(names),'names':names,'blender_version':bpy.app.version_string,'render':str(root/'blender_scene.png'),
  'status':'TECHNICAL_ASSET_AUTHORING_ONLY_NOT_ART_APPROVED','cost_eur':0}
(root/'manifest.json').write_text(json.dumps(manifest,indent=2))
print('ELDORIA_63_BLENDER_ASSETS_CREATED',len(names),flush=True)
