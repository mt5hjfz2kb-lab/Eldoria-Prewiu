"""Real Blender Cycles renders for academy exercises, fixed meshes/camera per pair.
No project assets, no Unity, no arbitrary model code."""
import bpy,math,json,sys
from mathutils import Vector
from pathlib import Path
args=sys.argv[sys.argv.index("--")+1:];root=Path(args[0])
def reset():
    bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
    sc=bpy.context.scene;sc.render.engine="CYCLES";sc.cycles.samples=64
    sc.cycles.use_denoising=False;sc.cycles.seed=37;sc.cycles.device="CPU"
    sc.render.resolution_x=660;sc.render.resolution_y=660;sc.render.resolution_percentage=100
    sc.render.image_settings.file_format="PNG";sc.render.film_transparent=False
    sc.view_settings.view_transform="AgX" if "AgX" in [x.identifier for x in sc.view_settings.bl_rna.properties["view_transform"].enum_items] else "Standard"
    sc.world=bpy.data.worlds.new("Academy world");sc.world.use_nodes=True
    return sc
def material(name,col,rough,metal=0,noise=0,grain=False):
    m=bpy.data.materials.new(name);m.use_nodes=True
    n=m.node_tree.nodes;links=m.node_tree.links;p=n.get("Principled BSDF")
    p.inputs["Base Color"].default_value=(*col,1);p.inputs["Roughness"].default_value=rough;p.inputs["Metallic"].default_value=metal
    if noise:
        tc=n.new("ShaderNodeTexCoord");mapping=n.new("ShaderNodeVectorMath");mapping.operation="MULTIPLY"
        mapping.inputs[1].default_value=(4,4,.5) if grain else (1,1,1)
        links.new(tc.outputs["Generated"],mapping.inputs[0])
        tex=n.new("ShaderNodeTexNoise");tex.inputs["Scale"].default_value=9 if grain else 18;tex.inputs["Detail"].default_value=3
        links.new(mapping.outputs[0],tex.inputs["Vector"])
        ramp=n.new("ShaderNodeValToRGB");ramp.color_ramp.elements[0].position=.18;ramp.color_ramp.elements[1].position=.82
        ramp.color_ramp.elements[0].color=(*(x*.74 for x in col),1);ramp.color_ramp.elements[1].color=(*(min(.95,x*1.17) for x in col),1)
        links.new(tex.outputs["Fac"],ramp.inputs[0]);links.new(ramp.outputs[0],p.inputs["Base Color"])
        bump=n.new("ShaderNodeBump");bump.inputs["Strength"].default_value=noise;bump.inputs["Distance"].default_value=.035
        links.new(tex.outputs["Fac"],bump.inputs["Height"]);links.new(bump.outputs["Normal"],p.inputs["Normal"])
    return m
def apply(o,m):
    o.data.materials.append(m);return o
def cube(name,pos,dim,m,bev=.035):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.name=name;o.dimensions=dim
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);apply(o,m)
    if bev:
        b=o.modifiers.new("beveled edges","BEVEL");b.width=bev;b.segments=3;o.modifiers.new("weighted normals","WEIGHTED_NORMAL") if hasattr(o.data,"use_auto_smooth") and o.data.use_auto_smooth else None
    return o
def cyl(name,pos,r,depth,m,verts=32,rot=(0,0,0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=depth,location=pos,rotation=rot)
    o=bpy.context.object;o.name=name;apply(o,m)
    b=o.modifiers.new("edge bevel","BEVEL");b.width=.018;b.segments=2;o.modifiers.new("weighted normals","WEIGHTED_NORMAL");return o
def torus(name,pos,r,minor,m,rot=(0,0,0)):
    bpy.ops.mesh.primitive_torus_add(major_segments=36,minor_segments=10,location=pos,rotation=rot,major_radius=r,minor_radius=minor)
    o=bpy.context.object;o.name=name;apply(o,m)
    for p in o.data.polygons:p.use_smooth=True
    return o
def camera(sc,pos,target,scale):
    bpy.ops.object.camera_add(location=pos);o=bpy.context.object;o.rotation_euler=(Vector(target)-o.location).to_track_quat("-Z","Y").to_euler()
    o.data.type="ORTHO";o.data.ortho_scale=scale;sc.camera=o
def light(name,pos,power,size,target):
    bpy.ops.object.light_add(type="AREA",location=pos);o=bpy.context.object;o.name=name;o.data.energy=power;o.data.shape="DISK";o.data.size=size
    o.rotation_euler=(Vector(target)-o.location).to_track_quat("-Z","Y").to_euler();return o
def materials(v):
    sv=v["stone_value"];wv=v["wood_value"];iv=v["iron_value"]
    return (material("Stone",(sv,sv*.94,sv*.83),v["stone_rough"],noise=.30),
      material("Timber",(wv,wv*.54,wv*.27),v["wood_rough"],noise=.22,grain=True),
      material("Iron",(iv*.86,iv*.93,iv),v["iron_rough"],v["iron_metal"],noise=.08))
def lighting(sc,v,target,scale=1):
    sc.world.node_tree.nodes["Background"].inputs["Color"].default_value=(.64,.72,.83,1)
    sc.world.node_tree.nodes["Background"].inputs["Strength"].default_value=v["world_strength"]
    key=light("Key upper-left",(-3.8,-4.8,6.6),v["key_power"]*scale,v["key_size"],target);key.data.color=(1,.88,.72)
    fill=light("Fill",(4,1.5,4),v["fill_power"]*scale,3.2,target);fill.data.color=(.77,.88,1)
def still(v):
    sc=reset();stone,wood,iron=materials(v)
    floor=material("Fixed neutral floor",(.19,.20,.18),.8)
    cube("floor",(0,0,-.12),(12,12,.24),floor)
    for x,m,name in [(-1.65,stone,"stone"),(0,wood,"wood"),(1.65,iron,"iron")]:
        bpy.ops.mesh.primitive_uv_sphere_add(segments=48,ring_count=24,radius=.68,location=(x,0,.68));o=bpy.context.object;o.name=name;apply(o,m)
        for p in o.data.polygons:p.use_smooth=True
    camera(sc,(5,-10,7),(0,0,.55),6.1);lighting(sc,v,(0,0,.5));return sc
def roofmesh(m):
    vertices=[(-1.62,-.68,2.1),(1.62,-.68,2.1),(1.62,2.75,2.1),(-1.62,2.75,2.1),(0,-.68,3.1),(0,2.75,3.1)]
    faces=[(0,1,4),(3,5,2),(0,4,5,3),(1,2,5,4),(0,3,2,1)]
    me=bpy.data.meshes.new("Gabled roof");me.from_pydata(vertices,[],faces);me.update()
    o=bpy.data.objects.new("Main roof",me);bpy.context.collection.objects.link(o);apply(o,m)
def courtyard(v):
    sc=reset();stone,wood,iron=materials(v)
    wall=v["wall_value"];roof=v["roof_value"];hero=v["hero_value"]
    plaster=material("Plaster",(wall,wall*.86,wall*.68),.84,noise=.12)
    tiles=material("Slate",(roof*.70,roof*.84,roof),.74,noise=.14)
    trim=material("Hero lintel",(hero,hero*.56,hero*.27),.7,noise=.12)
    grass=material("Ground",(.16,.21,.12),.94,noise=.16)
    path=material("Fixed road",(.33,.30,.23),.91,noise=.25)
    cube("ground",(0,0,-.1),(14,14,.2),grass)
    cube("road",(0,-1.8,.012),(.92,2.85,.025),path,.02)
    cube("hall floor",(0,1.04,.075),(2.8,3.1,.15),stone)
    cube("hall back",(0,2.55,1.12),(2.8,.20,2.24),plaster)
    for x in (-1.3,1.3):
        cube("hall side",(x,1.05,1.12),(.20,2.9,2.24),plaster)
    for x in (-.95,.95):
        cube("front pillar",(x,-.46,1.13),(.75,.25,2.26),plaster)
        cube("timber jamb",(x*.60,-.61,.92),(.13,.14,1.84),trim)
    cube("front lintel",(0,-.5,1.94),(2.8,.25,.46),trim)
    roofmesh(tiles)
    cube("forge chimney",(.98,1.7,2.94),(.46,.45,1.26),stone)
    # Working threshold, anvil and tools, not merely decorative rectangles.
    cube("anvil pedestal",(0,.52,.37),(.49,.48,.59),wood)
    cube("anvil",(0,.52,.77),(.82,.34,.22),iron)
    cube("anvil top",(0,.52,.92),(1.0,.39,.09),iron)
    wx,wy=v["well_x"],v["well_y"];cx,cy=v["cart_x"],v["cart_y"];sx,sy=v["stall_x"],v["stall_y"]
    for row in range(2):
        for i in range(12):
            t=2*math.pi*(i+row*.5)/12
            o=cube("well masonry",(wx+.39*math.cos(t),wy+.39*math.sin(t),.13+.22*row),(.24,.25,.22),stone,.018);o.rotation_euler.z=t
    cyl("well dark interior",(wx,wy,.10),.32,.04,material("Water",(.035,.075,.08),.3))
    torus("well rim",(wx,wy,.49),.39,.055,stone)
    cube("cart tray",(cx,cy,.39),(.78,1.1,.14),wood)
    for xx in (-.43,.43):
        torus("cart wheel",(cx+xx,cy,.26),.25,.038,iron,(0,math.pi/2,0))
        for z in range(4):
            o=cube("wheel spoke",(cx+xx,cy,.26),(.04,.47,.035),wood,.008);o.rotation_euler.x=z*math.pi/4
    for x in (-.36,.36):
        for y in (-.33,.33):cube("stall post",(sx+x,sy+y,.6),(.10,.10,1.2),wood)
    cube("stall counter",(sx,sy,.63),(.90,.76,.12),wood)
    cube("stall canopy",(sx,sy,1.28),(1.04,.85,.08),trim)
    for x in (-.22,.12):cyl("stall vessel",(sx+x,sy,.82),.12,.26,iron,verts=16)
    camera(sc,(8,-11,10),(0,.1,.75),8.0);lighting(sc,v,(0,.2,.6),1.6);return sc
if len(args)>1 and args[1]=="clearance":
    sys.path.insert(0,str(Path(__file__).parent))
    from placement_solver import solve
    v=json.loads((root/"control.json").read_text())["settings"]
    sc=courtyard(v)
    solve(sc,v,root/"clearance-metrics.json")
    sc.render.filepath=str(root/"corrected-courtyard.png")
    bpy.ops.render.render(write_still=True)
    assert Path(sc.render.filepath).is_file()
    print("ACADEMY_CLEARANCE_RENDER_COMPLETE",bpy.app.version_string,flush=True)
else:
    for arm in ("control","learned"):
        data=json.loads((root/(arm+".json")).read_text());v=data["settings"]
        for name,fn in (("materials",still),("courtyard",courtyard)):
            sc=fn(v);sc.render.filepath=str(root/(arm+"-"+name+".png"))
            bpy.ops.render.render(write_still=True)
            assert Path(sc.render.filepath).is_file()
            print("ACADEMY_BLENDER_RENDER",arm,name,flush=True)
    print("ACADEMY_BLENDER_COMPLETE",bpy.app.version_string,flush=True)
