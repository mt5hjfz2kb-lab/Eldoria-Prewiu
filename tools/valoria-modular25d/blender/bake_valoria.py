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
    """Unified handcrafted-material palette. Procedural microtexture, no external PBR set.

    Macro-silhouette is geometry. Stone/roof/wood surface variation is rendered
    into sprites from a single shared camera and light rig. Never random per run.
    """
    name="Eldoria | "+key+(" | "+str(variant) if variant is not None else "")
    if name in bpy.data.materials:
        return bpy.data.materials[name]
    color=rgba(PAL[key])
    if variant is not None:
        k=1.0+float(variant)
        color=tuple(min(1,max(0,c*k)) for c in color[:3])+(1,)
    m=bpy.data.materials.new(name)
    m.diffuse_color=color
    m.use_nodes=True
    nodes=m.node_tree.nodes
    links=m.node_tree.links
    shader=nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value=color
    shader.inputs["Roughness"].default_value=.91 if key not in ("iron","amber") else .55
    roughness=shader.inputs.get("Metallic")
    if roughness and key=="iron":
        roughness.default_value=.62

    # Stone must read as ancient worn blocks, not an untextured grey box.
    # Shared noise parameters preserve material identity across all assets.
    stone_keys={"stone","stone_light","stone_dark","stone_moss","mortar"}
    timber_keys={"timber","wood_light","wood_dark"}
    roof_keys={"roof","roof_light","roof_dark"}
    foliage_keys={"pine","pine_light","greens"}
    if key in stone_keys|timber_keys|roof_keys|foliage_keys|{"dirt","cloth","skin"}:
        tex=nodes.new("ShaderNodeTexNoise")
        tex.label="Eroded material grain"
        tex.inputs["Scale"].default_value=(
            23 if key in stone_keys else
            42 if key in timber_keys else
            35 if key in roof_keys else
            16 if key in foliage_keys else 28)
        tex.inputs["Detail"].default_value=3.5
        tex.inputs["Roughness"].default_value=.72
        ramp=nodes.new("ShaderNodeValToRGB")
        ramp.label="Restricted material ink"
        ramp.color_ramp.elements.remove(ramp.color_ramp.elements[1])
        def shade(factor):
            return tuple(max(0,min(1,c*factor)) for c in color[:3])+(1,)
        def el(pos,val):
            e=ramp.color_ramp.elements.new(pos)
            e.color=shade(val)
        ramp.color_ramp.elements[0].position=.18
        ramp.color_ramp.elements[0].color=shade(.69 if key in stone_keys else .72)
        el(.44,.91)
        el(.63,1.06)
        el(.82,1.19 if key not in foliage_keys else 1.12)
        links.new(tex.outputs["Fac"],ramp.inputs["Fac"])
        links.new(ramp.outputs["Color"],shader.inputs["Base Color"])
        bump=nodes.new("ShaderNodeBump")
        bump.inputs["Strength"].default_value=(
            .27 if key in stone_keys else .18 if key in timber_keys else .11)
        bump.inputs["Distance"].default_value=.055 if key in stone_keys else .020
        links.new(tex.outputs["Fac"],bump.inputs["Height"])
        links.new(bump.outputs["Normal"],shader.inputs["Normal"])
        if key in timber_keys:
            wave=nodes.new("ShaderNodeTexWave")
            wave.wave_type="BANDS"
            wave.bands_direction="Z"
            wave.inputs["Scale"].default_value=7.5
            wave.inputs["Distortion"].default_value=3.4
            mix=nodes.new("ShaderNodeMixRGB")
            mix.blend_type="MULTIPLY"
            mix.inputs[0].default_value=.26
            links.new(ramp.outputs["Color"],mix.inputs[1])
            links.new(wave.outputs["Color"],mix.inputs[2])
            links.new(mix.outputs["Color"],shader.inputs["Base Color"])

    if key=="amber":
        socket=shader.inputs.get("Emission Color") or shader.inputs.get("Emission")
        if socket:
            socket.default_value=color
        strength=shader.inputs.get("Emission Strength")
        if strength:
            strength.default_value=2.2
    if key=="void":
        socket=shader.inputs.get("Emission Color") or shader.inputs.get("Emission")
        if socket:
            socket.default_value=color
        strength=shader.inputs.get("Emission Strength")
        if strength:
            strength.default_value=.68
    if emission:
        socket=shader.inputs.get("Emission Color") or shader.inputs.get("Emission")
        if socket:
            socket.default_value=color
        strength=shader.inputs.get("Emission Strength")
        if strength:
            strength.default_value=emission
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
    """Open imperial sentry turret with ruined crenels and carved stonework."""
    centre=(-1.68,.87)
    n=10
    outline=[]
    for i in range(n):
        ang=math.tau*i/n
        r=.69+RND.uniform(-.055,.075)
        outline.append((centre[0]+r*math.cos(ang),centre[1]+r*math.sin(ang)))
    tops=[4.18+RND.uniform(-.08,.10) for _ in outline]
    irregular_wall("ancient imperial watchtower",outline,tops,0,
                   ("stone_dark","stone","stone_light","stone_moss"))
    # Upper open sentry ring: shorter gap-tooth rhythm, weathered missing merlon.
    for i in range(n):
        if i in (2,7):
            continue
        x,y=outline[i]
        stone("crenellated tower rim",(x,y,tops[i]+.22),
              (.28,.29,.47),"stone_light" if i%3==0 else "stone")
    # Hand-set bands of dressed masonry break the monolithic surface.
    for row in range(10):
        zz=.33+row*.36
        for i in range(12):
            if RND.random()<.33:
                continue
            ang=(i+(row%2)*.42)*math.tau/12
            x=centre[0]+.68*math.cos(ang)
            y=centre[1]+.68*math.sin(ang)
            stone("tower chipped bond",(x,y,zz),(.14,.12,.084),
                 "stone_light" if (row+i)%7==0 else "stone")
    # Deep narrow windows with tiny amber slit, readable on mobile.
    for z in (1.18,2.47,3.46):
        cube("tower black arrow slit",(-2.37,.62,z),(.033,.25,.45),"iron",.011,spin=-.4)
        cube("tower window ember",(-2.42,.59,z),(.034,.07,.20),"amber",.007)
    # Very compact dark blue cap, no roof competing with the Bastion gate.
    for j in range(n):
        a=math.tau*j/n
        x=centre[0]+.56*math.cos(a)
        y=centre[1]+.56*math.sin(a)
        beam("open watch parapet ring",(x,y,4.18),
             (centre[0]+.56*math.cos(a+math.tau/n),
              centre[1]+.56*math.sin(a+math.tau/n),4.18),.063,"stone_light",6)


def bastion():
    """Eldoria Bastion I: a damaged early-military gatehouse, NOT a townhouse.

    Macro pivot after rejecting both roof-dominated candidates: gatehouse and
    strong irregular curtain walls must define the silhouette. A tiny artisan
    shelter is subordinate. All modules touch real terrain and have no plinth.
    """
    # Military footprint, asymmetrical from the FIRST level.
    walls=[
      ("front west",[(-2.12,-1.72),(-.62,-1.77),(-.60,-1.49),(-2.08,-1.43)],2.05),
      ("front east",[(.62,-1.76),(2.10,-1.68),(2.10,-1.42),(.60,-1.49)],2.05),
      ("west", [(-2.13,-1.72),(-1.81,-1.73),(-1.78,1.22),(-2.09,1.22)],1.90),
      ("northern", [(-2.08,1.22),(1.48,1.20),(1.51,1.49),(-2.08,1.50)],1.84),
      ("eastern broken",[(1.80,-1.42),(2.11,-1.45),(2.15,1.42),(1.87,1.39)],1.52)
    ]
    for name,pts,h in walls:
        elevations=[h+RND.uniform(-.06,.06) for _ in pts]
        if name=="eastern broken":elevations=[1.94,1.42,.82,1.26]
        irregular_wall(name+" curtain wall",pts,elevations,0,
                       ("stone_dark","stone","stone_light","stone_moss"))
        if name!="eastern broken":
            for j in range(9):
                t=(j+.5)/9
                x=pts[0][0]+(pts[1][0]-pts[0][0])*t
                y=pts[0][1]+(pts[1][1]-pts[0][1])*t
                if j%3!=1:
                    stone("chipped wall merlon",(x,y,h+.15),(.19,.24,.31),
                          "stone_light" if j%6==0 else "stone")
    # Tall imperial watch post to the north-west, visibly higher than all
    # buildings. Its front is partially collapsed, not newly pristine.
    stone_tower()
    # Surviving eastern gate-tower is shorter and partially dismantled.
    east=[(1.44,-.38),(2.08,-.62),(2.53,-.28),(2.55,.35),
          (2.15,.67),(1.53,.38)]
    irregular_wall("wounded eastern gate turret",east,
                   [2.66,2.56,1.47,1.31,1.96,2.48],0,
                   ("stone_dark","stone","stone_moss","stone_light"))
    for i,(x,y) in enumerate(east):
        if i%3==2:continue
        stone("partial eastern turret crown",(x,y,2.46 if i<2 else 1.75),
              (.22,.20,.30),"stone_light" if i%2 else "stone")
    # Main gate arch is the HERO, with hewn voussoirs and independent door.
    for x in (-.61,.61):
        stone("gatehouse buttress",(x,-1.87,1.06),(.42,.49,2.14))
        beam("old timber reinforcing gate jamb",(x,-2.12,.12),
             (x,-2.13,1.83),.071,"timber",7)
    cube("recessed double oak portal",(0,-1.96,.81),(1.07,.16,1.55),"wood_dark",.045)
    for j in range(11):
        th=math.pi*j/10
        x=.58*math.cos(th)
        z=1.72+.45*math.sin(th)
        stone("gate rebuilt irregular voussoir",(x,-2.05,z),(.19,.22,.23),
              "stone_light" if j%3==0 else "stone")
    beam("gate arch keystone copper line",(-.15,-2.04,2.09),(.15,-2.04,2.09),
         .035,"wood_light")
    for x in (-.34,-.10,.12,.35):
        beam("oak plank split",(x,-2.052,.12),(x,-2.052,1.54),.035,"timber",7)
    for z in (.49,1.12):
        beam("crosswise iron brace",(-.42,-2.08,z),(.42,-2.08,z),.041,"iron",6)
    for x in (-.32,.30):
        stone("iron gate boss",(x,-2.10,.80),(.08,.035,.10),"amber")
    # The gatehouse cap is STONE and militarily recognisable, no giant roof.
    cube("weathered gate platform",(0,-1.74,2.30),(1.62,.74,.20),"stone",.06)
    for xx in (-.76,-.26,.22,.72):
        stone("gate parapet battlement",(xx,-1.95,2.54),(.24,.28,.38))
    # Small occupied rear timber shelter occupies less than 20% of silhouette,
    # evidence of reconstruction rather than a generic farmhouse.
    shelter=[(-.72,.46),(.67,.48),(.70,.69),(.60,1.30),(-.77,1.29)]
    irregular_wall("small rebuilt hall",shelter,
                   [1.47,1.47,1.56,1.55,1.51],0,
                   ("stone_dark","stone_moss","stone_light","stone"))
    gable_roof("half-restored timber roof",-.80,.75,.40,1.39,1.49,2.06)
    for x in (-.65,.58):
        beam("survivor wall timber joist",(x,.40,.10),(x,.40,1.55),.085,"timber")
    # Scaffolding on the breached eastern wall reads as construction.
    for x in (1.45,2.16):
        beam("unfinished repair mast",(x,-1.87,.03),(x,-1.87,2.60),.085,"timber",7)
    for z in (.9,1.65):
        beam("scaffold access deck",(1.28,-1.97,z),(2.37,-1.97,z),.096,"wood_light",7)
    beam("cross-brace 1",(1.43,-2.04,.18),(2.20,-2.04,2.44),.061,"timber",5)
    beam("cross-brace 2",(2.18,-2.06,.15),(1.44,-2.06,1.67),.053,"timber",5)
    # Actual handlaid banners and iron-basket torches, not giant decor covers.
    for x in (-.88,.90):
        beam("warm iron torch arm",(x,-1.95,1.20),(x,-2.04,1.65),.052,"wood_dark")
        stone("raised torch basket",(x,-2.08,1.63),(.15,.16,.13),"iron")
        cube("warm tiny ember",(x,-2.11,1.78),(.10,.10,.19),"amber",.02)
    beam("bent royal banner pole",(-1.05,-1.68,1.75),(-1.05,-1.68,3.46),.047,"wood_dark")
    mesh("battle-worn banner",[(-1.04,-1.76,3.41),(-.36,-1.85,3.29),
       (-.59,-1.87,2.75),(-.78,-1.84,2.95),(-1.04,-1.76,2.63)],
       [(0,1,2,3,4)],["cloth"])
    for i in range(4):
        beam("Valoria gilded insignia",(-.99+i*.11,-1.88,3.12),
             (-.95+i*.11,-1.88,2.92),.011,"amber",4)
    # Collapsed imperial stone scattered in world space, not a base island.
    rubble((.0,.0),34,2.67)
    for j in range(5):
        x=2.21+RND.random()*.32
        z=.15+j*.31
        beam("faint violet fissure etched in old tower",(x,-.28,z),
             (x+.05,-.30,z+.24),.012,"void",5)


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
    """Natural 2.5D evergreen, not the rejected stack of faceted blocks."""
    rng=random.Random(seed+721)
    h=2.85+seed*.28
    lean=rng.uniform(-.12,.12)
    beam("bent ancient tree trunk",(0,0,0),(lean,-.04,h*.93),.13,"timber",10)
    # Dark bough silhouette with uneven clusters, smooth volume at this scale.
    for tier in range(8):
        z=.43+tier*(h-.52)/8
        r=1.24*(1-tier/9.0)**1.12
        n=8 if tier<4 else 6
        for j in range(n):
            angle=j*math.tau/n+tier*.32+rng.uniform(-.21,.21)
            distance=r*(.62+rng.random()*.24)
            x=math.cos(angle)*distance+lean*.25
            y=math.sin(angle)*distance
            dz=rng.uniform(-.13,.12)
            beam("thin pine branch",(.02,-.02,z+.11),
                 (x,y,z-.16+dz),.025,"wood_dark",6)
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,
                radius=.24+rng.random()*.09,location=(x,y,z+.09+dz))
            branch=bpy.context.object
            branch.name="Organic conifer needles"
            branch.scale=(1.22,.86,1.63)
            for poly in branch.data.polygons:
                poly.use_smooth=True
            branch.data.materials.append(mat("pine_light" if j%5==0 else "pine"))
            # Fine downward spruce fronds add spiky breakup to the silhouette.
            for t in range(3):
                theta=angle+.2*(t-1)
                rr=distance+r*(.10+.15*t)
                nx=math.cos(theta)*rr+lean*.25
                ny=math.sin(theta)*rr
                mesh("spruce needle sprays",[
                    (x,y,z+.40+dz),(nx-.10,ny,z-.28+dz),
                    (nx,ny,z-.39+dz),(nx+.10,ny,z-.28+dz)
                ],[(0,1,2,3)],["pine_light" if t==1 and j%4==0 else "pine"])
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=.22,
                                         location=(lean,-.02,h))
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
    scene.cycles.samples=24
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
    bg.inputs["Strength"].default_value=.46
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
      ((6,5,7),(.51,.68,.95),165,7)
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
