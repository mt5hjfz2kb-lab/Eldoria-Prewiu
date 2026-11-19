"""Eldoria Bastion-I sprite bakery: Blender-authoring only, Unity runtime stays 2.5D.

All sprites are rendered with ONE orthographic 2:1 camera, ONE light rig,
ONE palette and a common world-ground origin. No reused models or external
textures. Deliberately uneven silhouettes, stonework, roof planes and carpentry
are authored as modules; individual building scenes are not backplates.
"""
import bpy, math, os, random, json, sys
from mathutils import Vector
from bpy_extras.object_utils import world_to_camera_view

RND = random.Random(250618)
OUT = os.path.abspath(sys.argv[sys.argv.index("--") + 1]) if "--" in sys.argv else "/tmp/eldoria-modular-bake"
os.makedirs(OUT, exist_ok=True)

PAL = {
    "stone": "#818B88", "stone_light": "#B5B5A5",
    "stone_dark": "#58666C", "stone_moss": "#7E8D7B",
    "mortar": "#4C5859", "roof": "#344C5E", "roof_light": "#6B8990",
    "roof_dark": "#263B4B", "timber": "#734C38", "wood_light": "#BA8156",
    "wood_dark": "#4A342E", "iron": "#293338",
    "amber": "#E6AF67", "cloth": "#58667D", "void": "#755581",
    "greens": "#486D5A", "pine": "#365A50", "pine_light": "#668875",
    "dirt": "#826F54", "skin": "#D2AE88"
}

def rgba(hexcode,alpha=1.0):
    v=hexcode.lstrip("#")
    return tuple(int(v[i:i+2],16)/255.0 for i in (0,2,4))+(alpha,)

def mat(key, variant=None, emission=0.0):
    name="Eldoria | "+key+(" | "+str(variant) if variant is not None else "")
    if name in bpy.data.materials:return bpy.data.materials[name]
    color=rgba(PAL[key])
    if variant is not None:
        k=1.0+float(variant)
        color=tuple(min(1,max(0,c*k)) for c in color[:3])+(1,)
    m=bpy.data.materials.new(name)
    m.diffuse_color=color
    m.use_nodes=True
    shader=m.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value=color
    shader.inputs["Roughness"].default_value=.86 if key not in ("iron","amber") else .48
    if emission:
        socket=shader.inputs.get("Emission Color") or shader.inputs.get("Emission")
        if socket:socket.default_value=color
        strength=shader.inputs.get("Emission Strength")
        if strength:strength.default_value=emission
    return m

def cube(name,loc,scale,material,bevel=0.04,spin=0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    obj=bpy.context.object;obj.name=name
    obj.dimensions=scale;obj.rotation_euler[2]=spin
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    obj.data.materials.append(mat(material))
    if bevel:
        mod=obj.modifiers.new("Weathered edge", "BEVEL")
        mod.width=bevel
        mod.segments=1
        obj.modifiers.new("Weighted normals","WEIGHTED_NORMAL")
    return obj

def beam(name,start,end,r,material,vertices=6):
    a,b=Vector(start),Vector(end)
    center=(a+b)/2
    direction=b-a
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=direction.length,location=center)
    ob=bpy.context.object;ob.name=name
    ob.rotation_euler=direction.to_track_quat("Z","Y").to_euler()
    ob.data.materials.append(mat(material))
    return ob

def stone(name,loc,scale,shade=None):
    if shade is None:shade=RND.choice(["stone","stone","stone_light","stone_dark","stone_moss"])
    ob=cube(name,loc,scale,shade,bevel=min(scale)*.16,
            spin=RND.uniform(-.18,.18))
    return ob

def mesh(name,vertices,faces,materials):
    data=bpy.data.meshes.new(name+" mesh")
    data.from_pydata(vertices,[],faces);data.update()
    obj=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(obj)
    for mm in materials:data.materials.append(mat(mm))
    for i,poly in enumerate(data.polygons):poly.material_index=i%len(materials)
    return obj

def irregular_wall(name,points,heights,base=0, materials=("stone_dark","stone","stone_light")):
    n=len(points)
    vertices=[(x,y,base) for x,y in points]+[(x,y,heights[i]) for i,(x,y) in enumerate(points)]
    faces=[tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    ob=mesh(name,vertices,faces,list(materials))
    return ob

def rubble(origin=(0,0),count=24,radius=2.1):
    for i in range(count):
        angle=RND.random()*math.tau
        rr=radius*(.56+RND.random()*.44)
        x=origin[0]+math.cos(angle)*rr
        y=origin[1]+math.sin(angle)*rr
        s=RND.uniform(.14,.38)
        stone("scattered ruined masonry", (x,y,s*.28),(s,s*.8,s*.55))

def masonry_face(name,y,x0,x1,z0,z1,seed=19):
    rng=random.Random(seed)
    # Irregular joint lengths, warm edge stones and no perfect brick grid.
    for course in range(int((z1-z0)/.32)):
        z=z0+(course+.7)*.32
        x=x0+.04+rng.uniform(0,.15)
        while x<x1-.2:
            length=rng.uniform(.33,.62)
            beam("eroded horizontal mortar", (x,y-.029,z),
                 (min(x+length,x1-.08),y-.029,z+rng.uniform(-.012,.012)),
                 .008,"mortar",5)
            if rng.random()<.70:
                beam("unmatched stone seam",(x,y-.031,z-.10),
                     (x+rng.uniform(-.045,.045),y-.031,z+.10),.006,"stone_dark",5)
            x+=length+.06

def gable_roof(name,xleft,xright,yfront,yback,eave_z,ridge_z):
    ridge_y=(yfront+yback)*.5-.08
    left=mesh(name+" north slope",[
        (xleft-.18,yfront-.22,eave_z-.02),(xright+.12,yfront-.22,eave_z+.03),
        (xright+.09,ridge_y,ridge_z),(xleft-.14,ridge_y,ridge_z-.06)],
        [(0,1,2,3)],["roof"])
    right=mesh(name+" south slope",[
        (xleft-.14,ridge_y,ridge_z-.06),(xright+.09,ridge_y,ridge_z),
        (xright+.26,yback+.17,eave_z),(xleft-.20,yback+.17,eave_z-.05)],
        [(0,1,2,3)],["roof_light"])
    # Ridge intentionally bows; tile courses overlap as individual shingles.
    for row in range(6):
        t=(row+.4)/6
        for side in range(2):
            ys = (yfront-.18)+(ridge_y-yfront+.18)*t if side==0 else ridge_y+(yback+.16-ridge_y)*t
            zz = (eave_z+.02)+(ridge_z-eave_z)*t if side==0 else ridge_z+(eave_z-ridge_z)*t
            for col in range(9):
                xx=xleft+(xright-xleft)*(col+.5)/9
                xx+=RND.uniform(-.022,.023)
                offs=RND.uniform(-.035,.03)
                beam("slate row",
                     (xx-.14,ys,zz+.032+offs),
                     (xx+.11,ys,zz+.032+offs),.018,
                     "roof_light" if (col+row)%4==0 else "roof_dark",5)
    beam("clipped ridge pole",(xleft-.17,ridge_y,ridge_z+.05),
         (xright+.10,ridge_y,ridge_z+.05),.055,"wood_dark",7)
    for xx in (xleft,xright):
        beam("rafter join",(xx,yfront-.19,eave_z-.03),
             (xx,ridge_y,ridge_z),.055,"timber",5)
        beam("rafter join",(xx,ridge_y,ridge_z),
             (xx,yback+.14,eave_z-.02),.055,"timber",5)

def stone_tower():
    # Remnant of an Imperial watchtower, not a clean cylindrical stack.
    centre=(-1.75,.80)
    n=9
    outline=[]
    for i in range(n):
        ang=math.tau*i/n
        r=.73+RND.uniform(-.12,.13)
        outline.append((centre[0]+r*math.cos(ang),centre[1]+r*math.sin(ang)))
    tops=[3.15+RND.uniform(-.18,.18) for _ in outline]
    irregular_wall("imperial round tower",outline,tops,0,("stone_dark","stone","stone_light"))
    # Missing battlement and sharp ruined crenels: deliberate asymmetry.
    for i in (0,1,2,4,6,7):
        x,y=outline[i];z=tops[i]
        stone("chipped battlement",(x,y,z+.16),(.30,.34,.32))
    for row in range(7):
        zz=.32+row*.36
        for i in range(9):
            if RND.random()<.40:continue
            angle=(i+.4*(row%2))*math.tau/9
            x=centre[0]+.72*math.cos(angle)
            y=centre[1]+.72*math.sin(angle)
            # Narrow ridged stones make masonry read at 390px, not texture spam.
            stone("tower dressed stone", (x,y,zz),(.17,.14,.09),
                "stone_light" if i%5==0 else "stone")
    cube("arrow slit",(-2.48,.54,1.68),(.022,.21,.52),"iron",.008,spin=-.38)
    cube("tower warm slit",(-2.51,.54,1.66),(.024,.06,.30),"amber",.001)

def bastion():
    """Tier-I recovery fort: low crenellated court, rear refuge and two DIFFERENT towers.
    The focal silhouette is military and damaged, never a tall generic farmhouse.
    """
    # Rough masonry boundary; gaps are real, so terrain shows between walls.
    sections=[
      ("front left",[(-2.15,-1.78),(-.61,-1.82),(-.60,-1.55),(-2.08,-1.49)],1.73),
      ("front right",[(.62,-1.82),(2.13,-1.75),(2.12,-1.49),(.60,-1.55)],1.68),
      ("west wall",[(-2.12,-1.74),(-1.87,-1.78),(-1.83,1.17),(-2.10,1.15)],1.68),
      ("north wall",[(-2.07,1.14),(1.54,1.17),(1.56,1.46),(-2.03,1.47)],1.60),
      ("broken east wall",[(1.83,-1.60),(2.15,-1.57),(2.16,1.43),(1.84,1.38)],1.38)
    ]
    for name,pts,h in sections:
        wallheights=[h+RND.uniform(-.035,.035) for _ in pts]
        if name=="broken east wall":wallheights=[1.56,1.63,.76,1.18]
        irregular_wall("bastion court "+name,pts,wallheights,0,
            ("stone_dark","stone","stone_light","stone_moss"))
        # Cap stones do not form an exact continuous rectangle.
        for j in range(8):
            u=(j+.2)/8
            x=pts[0][0]*(1-u)+pts[1][0]*u
            y=pts[0][1]*(1-u)+pts[1][1]*u
            if RND.random()>.65:continue
            stone("dressed parapet edge",(x,y,h+.05),(.18,.18,.16),
                  "stone_light" if j%4==0 else "stone")
    # Crenels along the two facing walls; irregular ruin omissions.
    for sign in (-1,1):
        for j in range(5):
            x=(.70+j*.27)*sign
            if RND.random()<.16:continue
            stone("uneven defensive battlement",(x,-1.66,1.87),(.21,.30,.31))
    # Rear refuge with sloped roof, deliberately SMALLER than the stone defences.
    refuge=[(-.94,.04),(1.09,.04),(1.18,.35),(1.13,1.35),(-1.03,1.36)]
    irregular_wall("sheltered timber hall",refuge,[2.05,2.10,2.06,2.12,2.11],0,
                   ("stone_dark","stone_moss","stone","stone_light"))
    gable_roof("refuge pitch",-1.12,1.25,-.15,1.56,2.09,2.85)
    # Asymmetric watchtower and truncated corrupted eastern turret.
    stone_tower()
    east=[(1.53,-.26),(2.07,-.49),(2.50,-.19),(2.58,.32),
          (2.20,.62),(1.63,.35)]
    irregular_wall("collapsed east watch turret",east,[2.08,1.74,1.12,1.29,1.55,1.83],0,
                  ("stone_dark","stone","stone_moss"))
    for idx in (0,2,4):
        x,y=east[idx]
        stone("fallen damaged turret coping",(x,y,1.44+idx*.08),(.30,.31,.29))
    # Distinct gatehouse: arched opening cut by intentional left/right sections.
    cube("recessed central gate",(0,-1.82,.73),(.90,.10,1.46),"iron",.12)
    for xx in (-.53,.53):
        beam("repaired gate jamb",(xx,-1.91,.16),(xx,-1.91,1.51),.115,"stone_light",6)
    for j in range(9):
        theta=math.pi*j/8
        x=.55*math.cos(theta)
        z=1.42+.35*math.sin(theta)
        stone("mismatched keystone",(x,-1.94,z),(.19,.24,.21),
              "stone_light" if j%3==0 else "stone")
    for x in (-.26,0,.26):
        beam("gate oak board",(x,-1.96,.12),(x,-1.96,1.32),.061,"timber",7)
    beam("gated iron crossbar",(-.41,-1.99,.64),(.41,-1.99,.64),.036,"iron",6)
    # Ramshackle reinforcement only on one flank, from the real earth.
    for x in (1.43,2.06):
        beam("scaffold at damaged wall",(x,-2.06,0),(x+.05,-2.06,2.28),.078,"timber",7)
    beam("unfinished repair platform",(1.38,-2.09,1.42),(2.38,-2.09,1.40),.10,"wood_light")
    beam("reused diagonal brace",(1.44,-2.10,.16),(2.07,-2.10,2.13),.061,"timber",6)
    for x in (-.77,.83):
        beam("gate torch mount",(x,-1.99,1.02),(x,-2.01,1.48),.047,"wood_dark")
        stone("torch iron basket",(x,-2.04,1.53),(.14,.13,.13),"iron")
        cube("warm torch fire",(x,-2.05,1.71),(.10,.10,.19),"amber",.04)
    # Imperial crest, weathered fabric, and a single violet fissure scar.
    beam("heraldic mast",(-.94,-1.72,1.7),(-.94,-1.72,3.37),.043,"wood_dark",7)
    mesh("torn imperial standard",[(-.94,-1.75,3.26),(-.20,-1.80,3.10),
             (-.49,-1.83,2.66),(-.71,-1.84,2.79),(-.94,-1.75,2.56)],
         [(0,1,2,3,4)],["cloth"])
    for k in range(4):
        beam("tarnished valoria sun mark",(-.84+k*.09,-1.87,2.89),
             (-.76+k*.09,-1.87,3.00),.013,"amber",5)
    # Stone debris is scattered around footprints, never on artificial platforms.
    rubble((.05,0),37,2.74)
    for k in range(4):
        x=2.24+RND.random()*.33
        z=.19+k*.34
        beam("violet glow in broken stone",(x,-.20,z),(x+.07,-.23,z+.23),
             .011,"void",5)

def sawmill(stage):
    # Shared low, asymmetric woodworks footprint; three fully distinct builds.
    for i in range(7):
        x=RND.uniform(-1.7,1.7);y=RND.uniform(-1.1,1.1)
        if stage=="site" or RND.random()<.4:
            stone("workshop low stone",(x,y,.085),(.30,.30,.14))
    # Actual cut-log stock is present in all three states.
    for row in range(3):
        for col in range(3):
            y=.67+row*.26
            z=.14+col*.20
            beam("stacked timber" ,(-1.66,y,z),( -.72,y,z),.115,"timber",9)
            for xx in (-1.66,-.72):
                cube("heartwood cut face",(xx,y,z),(.022,.17,.17),"wood_light",.02)
    if stage=="site":
        for x,y in [(-1.7,-1.0),(1.6,-.9),(1.7,1.0),(-1.6,1.12)]:
            beam("marked ground post",(x,y,0),(x,y,.45),.08,"wood_light")
        beam("survey cord",(-1.65,-1.03,.42),(1.6,-.9,.38),.012,"cloth")
        beam("survey cord",(1.6,-.9,.38),(1.7,1.0,.43),.012,"cloth")
        return
    # Stone sill only, no broad raised platform.
    for x in (-1.65,1.48):
        for y in (-.90,.90):
            stone("post footing",(x,y,.15),(.42,.37,.25))
    for x,y,height in [(-1.53,-.78,2.24),(1.43,-.8,2.18),
                       (-1.51,.89,1.70),(1.48,.93,1.78)]:
        beam("heavy sawmill post",(x,y,.12),(x+.03,y,height),.12,"timber",7)
    for y,z in ((-.83,2.17),(.93,1.66)):
        beam("roof bearer",(-1.72,y,z),(1.58,y,z),.10,"wood_light",6)
    for x in (-1.56,1.42):
        beam("diagonal carpenter brace",(x,-.86,.60),(x,-.86,2.12),.07,"timber")
        beam("diagonal carpenter brace",(x,-.87,.63),(x*.65,-.87,1.36),.055,"wood_light")
    if stage=="worksite":
        # An actual partially built roof (only three rafters), canvas and planks.
        for xx in (-1.55,.05,1.38):
            beam("open rafter",(xx,-1.0,2.24),(xx,.96,1.78),.07,"wood_light",6)
        mesh("unfixed worker tarp",[(-.84,-.90,2.38),(.62,-.9,2.33),
             (.72,.15,2.02),(-.88,.24,2.12)],[(0,1,2,3)],["cloth"])
        for i in range(5):
            y=-1.0+i*.12
            beam("planks stacked beside scaffold",(-.4,y,.14),(.85,y,.14),.045,"wood_light")
        return
    # Completed workshop has a coherent slope + staggered slate shingles.
    mesh("heavy one-way sawmill roof",[
        (-1.87,-1.07,2.42),(1.76,-1.07,2.32),
        (1.80,1.13,1.67),(-1.84,1.12,1.75)],
        [(0,1,2,3)],["roof"])
    for row in range(8):
        t=row/8
        yy=-1.04+t*2.1
        zz=2.34-t*.62
        for col in range(10):
            xx=-1.73+(col+.5)*.339
            beam("thin slate tab",(xx-.12,yy,zz+.034),
                 (xx+.14,yy,zz+.026),.016,
                 "roof_light" if (col+row)%4==0 else "roof_dark",5)
    # Saw bench and timber sled, identifiable from a mobile-sized silhouette.
    for x in (.25,1.15):
        for y in (-.55,.16):
            beam("workbench leg",(x,y,.10),(x,y,1.03),.07,"timber")
    cube("long carpenter bench",(.72,-.19,1.06),(1.40,.95,.16),"wood_light",.07)
    beam("saw arch",( .36,-.20,1.33),(1.25,-.20,1.33),.05,"iron")
    beam("saw blade",(.38,-.18,1.05),(1.19,-.18,1.05),.018,"stone_light")
    for i in range(10):
        xx=.40+i*.078
        beam("saw teeth",(xx,-.19,1.05),(xx+.018,-.19,1.00),.008,"iron",4)
    for j in range(2):
        beam("rail for timber",(-1.50,-.95,.21),(1.50,-.95,.21),.055,"wood_dark")

def pine(seed):
    """Asymmetric conifer silhouette with distributed bough clusters, not origami fans."""
    rng=random.Random(seed+721)
    h=2.60+seed*.22
    beam("ancient lichen trunk",(0,0,0),(.04,-.07,h*.90),.145,"timber",8)
    for tier in range(7):
        z=.48+tier*(h-.35)/7
        radius=(1.15-tier*.135)
        n=7 if tier<4 else 6
        for branch in range(n):
            theta=(branch/n)*math.tau+tier*.26+rng.uniform(-.20,.20)
            rr=radius*(.60+rng.random()*.24)
            x,y=math.cos(theta)*rr,math.sin(theta)*rr
            bz=z+rng.uniform(-.09,.11)
            beam("visible dark bough",(.03,-.04,bz+.07),(x,y,bz-.10),
                 .035,"wood_dark",5)
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,
                    radius=.30+rng.random()*.10,
                    location=(x,y,bz+.12))
            ob=bpy.context.object;ob.name="irregular evergreen needle cluster"
            ob.scale=(1.1,.9+rng.random()*.18,1.2+rng.random()*.32)
            ob.data.materials.append(mat("pine" if branch%4 else "pine_light"))
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=.25,
                                         location=(.04,-.03,h))
    bpy.context.object.data.materials.append(mat("pine"))

def villager(frame):
    step=[-.15,.15][frame%2]
    for sx in (-1,1):
        x=sx*.15
        beam("leather boot",(x,step*sx*.4,.04),(x,step*sx*.4,.32),.115,"wood_dark",7)
        beam("wool sleeve",(sx*.28,0,1.13),(sx*.37,step*sx*.3,.67),.105,"cloth",7)
    ob=cube("working apron",(0,0,.87),(.52,.38,.82),"cloth",.15)
    stone("beige face",(0,-.03,1.49),(.34,.31,.37),"skin")
    cube("rough worker cap",(0,-.02,1.68),(.41,.35,.13),"roof",.06)
    cube("work belt",(0,-.13,.72),(.53,.18,.10),"wood_dark",.02)
    cube("leather satchel",(.36,.02,.87),(.18,.30,.27),"timber",.03)

def bush(variant):
    rng=random.Random(variant+42)
    for i in range(9):
        theta=i*math.tau/9
        rr=.28+.10*rng.random()
        x,y=rr*math.cos(theta),rr*math.sin(theta)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=.27+rng.random()*.10,location=(x,y,.23+rng.random()*.11))
        ob=bpy.context.object;ob.name="low bramble cluster"
        ob.data.materials.append(mat("greens" if i%3 else "pine_light"))

def loose_stone():
    stone("isolated broken stone",(0,0,.21),(.78,.49,.42))

def reset_geometry():
    for o in list(bpy.data.objects):
        if o.type!="CAMERA" and o.type!="LIGHT":bpy.data.objects.remove(o,do_unlink=True)
    RND.seed(250618)

def setup():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    scene=bpy.context.scene
    scene.render.engine='CYCLES'
    scene.cycles.samples=12
    scene.render.resolution_x=704
    scene.render.resolution_y=704
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG'
    scene.render.image_settings.color_mode='RGBA'
    scene.render.film_transparent=True
    scene.render.image_settings.color_depth='8'
    scene.world.use_nodes=True
    bg=scene.world.node_tree.nodes.get("Background")
    bg.inputs["Color"].default_value=(.30,.37,.45,1)
    bg.inputs["Strength"].default_value=.60
    scene.view_settings.view_transform='Standard'
    try:scene.view_settings.look='Medium High Contrast'
    except Exception:pass
    scene.view_settings.exposure=0
    scene.view_settings.gamma=1
    bpy.ops.object.camera_add(location=(9.5,-9.5,10.7))
    camera=bpy.context.object
    target=Vector((0,0,1.2))
    camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
    camera.data.type='ORTHO'
    camera.data.ortho_scale=9.0
    scene.camera=camera
    # Warm west key, cool opposite fill. Shared for EVERY asset.
    for location,color,power,energy in [
      ((-5,-7,12),(.99,.79,.59),800,6),
      ((6,5,7),(.51,.68,.95),300,7)
    ]:
        bpy.ops.object.light_add(type='AREA',location=location)
        ob=bpy.context.object
        ob.data.energy=power
        ob.data.color=color
        ob.data.shape='DISK'
        ob.data.size=energy
        ob.rotation_euler=(Vector((0,0,0))-ob.location).to_track_quat('-Z','Y').to_euler()
    return scene,camera

scene,camera=setup()
spec=[("bastion_i",bastion),
      ("sawmill_i",lambda:sawmill("built")),
      ("sawmill_available",lambda:sawmill("site")),
      ("sawmill_under_construction",lambda:sawmill("worksite")),
      ("pine_0",lambda:pine(0)),("pine_1",lambda:pine(1)),
      ("pine_2",lambda:pine(2)),
      ("villager_walk_0",lambda:villager(0)),("villager_walk_1",lambda:villager(1)),
      ("bush_0",lambda:bush(0)),("bush_1",lambda:bush(1)),
      ("bush_2",lambda:bush(2)),("stone",loose_stone)]
if os.environ.get("ELDORIA_BAKE_ONLY"):
    requested=set(os.environ["ELDORIA_BAKE_ONLY"].split(","))
    spec=[item for item in spec if item[0] in requested]
for key,build in spec:
    reset_geometry()
    build()
    scene.render.filepath=os.path.join(OUT,key+"_uncropped.png")
    # Save screen pixel location of original (0,0,0) foot point.
    ndc=world_to_camera_view(scene,camera,Vector((0,0,0)))
    record={"asset":key,"source_size":[scene.render.resolution_x,scene.render.resolution_y],
        "foot_xy_pixel":[ndc.x*704,(1.0-ndc.y)*704],
        "ortho_scale":9.0,"camera":[9.5,-9.5,10.7],
        "target":[0,0,1.2],"samples":12,"palette":PAL}
    with open(os.path.join(OUT,key+".json"),"w") as file:json.dump(record,file,indent=2)
    bpy.ops.render.render(write_still=True)
    print("ELDORIA_BAKED",key,scene.render.filepath,flush=True)
