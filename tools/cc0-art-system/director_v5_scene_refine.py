#!/usr/bin/env python3
"""Eldoria Director V5 — genuine 3D presentation refinement.
Input: exactly the five author-owned V4 physical glTF assets from run 43870376065.
Output: five real glTF models, editable Blender source and comparative Blender PNGs.
This does not alter gameplay, main, scene GUIDs, Unity, iPhone or licensed vendors.
"""
import bpy, math, random, json, struct
import numpy as np
from pathlib import Path
from mathutils import Vector
from math import sin,cos,pi,radians
ROOT=Path(__file__).resolve().parents[2]
BASE=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV4"
OUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV5"
PREV=ROOT/"docs/cc0-art-system/previews/director-v5"
for p in (OUT,PREV):p.mkdir(parents=True,exist_ok=True)
MESHES=[]; FRAMES=[]; REVIEW=[]
def hexcolor(s):
    s=s.lstrip("#")
    return tuple(int(s[i:i+2],16)/255.0 for i in (0,2,4))+(1,)
def mat(name,color,rough=.88,metal=.0):
    m=bpy.data.materials.new("ELDORIA_V5_"+name)
    m.diffuse_color=hexcolor(color);m.use_nodes=True
    n=m.node_tree.nodes.get("Principled BSDF")
    n.inputs["Base Color"].default_value=m.diffuse_color
    n.inputs["Roughness"].default_value=rough
    n.inputs["Metallic"].default_value=metal
    return m
def group(name):
    c=bpy.data.collections.get(name)
    if c is None:
        c=bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(c)
    return c
def move(o,col,m):
    for c in list(o.users_collection): c.objects.unlink(o)
    group(col).objects.link(o)
    if m:o.data.materials.append(m)
    return o
def cube(name,pos,size,m,col,bevel=.0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos)
    ob=bpy.context.object;ob.name=name;ob.dimensions=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    move(ob,col,m)
    if bevel:
        mod=ob.modifiers.new("V5 hand cut edge","BEVEL")
        mod.width=bevel;mod.segments=1
    return ob
def cyl(name,pos,r,h,m,col,vertices=14,rotate=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=h,location=pos)
    ob=bpy.context.object;ob.name=name
    if rotate:ob.rotation_euler=rotate
    return move(ob,col,m)
def cone(name,pos,r,h,m,col,verts=10):
    bpy.ops.mesh.primitive_cone_add(vertices=verts,radius1=r,radius2=0.03,depth=h,location=pos)
    ob=bpy.context.object;ob.name=name
    return move(ob,col,m)
def beam(name,start,end,t,m,col):
    start,end=Vector(start),Vector(end)
    direction=end-start
    o=cube(name,(start+end)*.5,(t,t,direction.length),m,col,t*.05)
    o.rotation_euler=direction.to_track_quat("Z","Y").to_euler()
    return o
def terrain_z(x,y):
    return .12*sin(x*.28)*cos(y*.21)+.17*sin(x*.11+y*.16)-.05*max(0,1-abs(x)/3)
def remove_source(prefixes):
    n=0
    for ob in list(bpy.data.objects):
        if ob.type=="MESH" and any(ob.name.startswith(p) for p in prefixes):
            bpy.data.objects.remove(ob,do_unlink=True);n+=1
    return n
def road_mesh(name,points,M,width=2.8,steps=42):
    """Earth surface follows terrain; no floating pads, repeated squares or road ladder."""
    curves=[Vector(p) for p in points]
    vertices=[];faces=[];indices=[]
    for i in range(steps):
        t=i/(steps-1)
        pos=((1-t)**3*curves[0]+3*(1-t)**2*t*curves[1]+
             3*(1-t)*t*t*curves[2]+t**3*curves[3])
        tangent=(3*(1-t)**2*(curves[1]-curves[0])+
                 6*(1-t)*t*(curves[2]-curves[1])+3*t*t*(curves[3]-curves[2]))
        tangent.normalize()
        outward=Vector((-tangent.y,tangent.x,0))
        # four strips: green bank - darker wheel rut - worn centre - bank
        for k,offset in enumerate((-1.11,-.48,.48,1.11)):
            q=pos+outward*offset*width*.5*(1+.08*sin(i*.7))
            vertices.append((q.x,q.y,terrain_z(q.x,q.y)+(.018 if k in (0,3) else .029)))
        if i:
            a=4*i
            for lane in range(3):
                faces.append((a-4+lane,a+lane,a+lane+1,a-3+lane))
                indices.append(1 if lane==1 else 0)
    mesh=bpy.data.meshes.new(name+"_native_road")
    mesh.from_pydata(vertices,[],faces)
    mesh.materials.append(M["edge"])
    mesh.materials.append(M["road"])
    mesh.update()
    for f,j in zip(mesh.polygons,indices):f.material_index=j
    ob=bpy.data.objects.new("Eldoria V5 | "+name,mesh)
    group("V5_TerrainRoads").objects.link(ob)
    return ob
def improve_terrain(M):
    roads=[
      ("castle gate",[(-.2,-37,0),(1,-26,0),(-2,-15,0),(0,-6,0)],3.5),
      ("mill track",[(-3,-18,0),(-9,-18,0),(-17,-16,0),(-22,-12,0)],1.6),
      ("granary supply",[(3,-18,0),(9,-20,0),(16,-16,0),(22,-12,0)],1.6),
      ("quarry supply",[(-18,-13,0),(-27,-9,0),(-28,3,0),(-23,9,0)],1.6),
      ("barracks supply",[(16,-12,0),(28,-5,0),(29,3,0),(24,7,0)],1.7)]
    for name,points,width in roads:road_mesh(name,points,M,width)
    # Environment clumps grouped per patch; less random white scatter.
    for p in range(37):
        xx=random.uniform(-49,49);yy=random.uniform(-46,46)
        if abs(xx)<15 and -25<yy<25:continue
        verts=[];faces=[]
        for k in range(12):
            x=xx+random.uniform(-1.8,1.8);y=yy+random.uniform(-1.8,1.8)
            z=terrain_z(x,y);a=len(verts)
            verts += [(x-.08,y,z+.02),(x+.08,y,z+.02),(x,y,z+random.uniform(.27,.70))]
            faces.append((a,a+1,a+2))
        mesh=bpy.data.meshes.new("native grass tufts")
        mesh.from_pydata(verts,[],faces);mesh.materials.append(M["grass"]);mesh.update()
        ob=bpy.data.objects.new("mixed meadow",mesh)
        group("V5_Grass").objects.link(ob)
    for p in range(45):
        x=random.uniform(-48,48);y=random.uniform(-40,40)
        if abs(x)<10 and -12<y<13:continue
        cube("earth embedded rock",(x,y,terrain_z(x,y)+.17),
             (random.uniform(.27,.70),random.uniform(.28,.63),random.uniform(.24,.38)),
             M["rock"],"V5_Scatter",.05)
def irregular_fir(x,y,h,M):
    """One canopy mesh, instead of three uniform pale cones."""
    verts=[];faces=[];mi=[]
    basez=terrain_z(x,y)
    angle=random.random()*2*pi
    for layer in range(5):
        t=layer/5
        level=basez+h*(.34+.125*layer)
        radius=h*(.17-.025*layer)
        for lobe in range(5):
            a=angle+2*pi*lobe/5+random.uniform(-.18,.18)
            dx=cos(a)*radius*.65;dy=sin(a)*radius*.65
            r=radius*random.uniform(.36,.53)
            idx=len(verts)
            verts.extend([(x+dx-r,y+dy,level-.15),
                          (x+dx+r,y+dy,level-.15),
                          (x+dx,y+dy-r,level-.15),
                          (x+dx,y+dy+r,level-.15),
                          (x+dx+random.uniform(-.11,.11),y+dy,level+r*1.1)])
            for ar in ((0,2,4),(2,1,4),(1,3,4),(3,0,4),(0,3,2),(1,2,3)):
                faces.append(tuple(idx+q for q in ar))
                mi.append((layer+lobe)%3)
    mesh=bpy.data.meshes.new("natural hanging spruce branches")
    mesh.from_pydata(verts,[],faces)
    for k in ("foliage1","foliage2","foliage3"):mesh.materials.append(M[k])
    mesh.update()
    for face,i in zip(mesh.polygons,mi):face.material_index=i
    ob=bpy.data.objects.new("spruce clusters | single low draw mesh",mesh)
    group("V5_Trees").objects.link(ob)
    cyl("dark tree trunk",(x,y,basez+h*.23),h*.04,h*.46,M["bark"],"V5_Trees",7)
def replace_forest(M):
    deleted=remove_source(("forest spruce irregular tier","tree / trunk",
      "spruce layered asymmetric silhouette","spruce weighted trunk",
      "Eldoria V4 | low natural underbrush",
      "natural lichen outcrop","perennial meadow flowers"))
    for k in range(103):
        x=random.uniform(-50,50);y=random.uniform(-45,45)
        if abs(x)<18 and -28<y<23:continue
        if abs(x-23)<11 and -17<y<20:continue
        if abs(x+23)<11 and -17<y<20:continue
        irregular_fir(x,y,random.uniform(5,10),M)
    return deleted
def enhance_bastion(M,stage):
    if stage<4:return
    col="V5_CitadelGrowth"
    if stage==5:
        # A true imperial second-stage chapel keep, conspicuous rising above I.
        for x in (-2.85,2.85):
            cube("imperial high keep corner pier",(x,3.1,12),
                 (.65,.8,4.7),M["ashlar"],col,.08)
            cone("imperial high keep corner pinnacle",(x,3.13,15.3),
                 .38,2.0,M["roof"],col,10)
        for x in (-1.3,0,1.3):
            cube("imperial lancet deep glass",(x,2.46,14.85),
                 (.72,.17,2.2),M["amber"],col,.04)
            for side in (-1,1):
                beam("lancet ogive voussoir",
                     (x+side*.52,2.25,15.7),(x,2.25,16.57),
                     .18,M["ashlar"],col)
        # Rear octagonal watch turret gives a new and unmistakable silhouette.
        cx,cy=0,10.0
        cyl("V5 rebuilt octagonal lookout",(cx,cy,13.75),
            1.65,9.0,M["stone"],col,12)
        cyl("V5 imperial lookout coping",(cx,cy,18.42),
            1.88,.39,M["ashlar"],col,12)
        cone("V5 roof lantern",(cx,cy,20.0),2.10,3.0,M["roof"],col,12)
        for i in range(12):
            a=2*pi*i/12
            cube("lookout stone merlon",
                 (cx+1.7*cos(a),cy+1.7*sin(a),19.00),
                 (.51,.52,.73),M["stone"],col,.05)
        beam("imperial crest staff",(cx,cy,20.8),(cx,cy,24.0),.13,M["gold"],col)
        cube("imperial plum flag",
             (cx+.64,cy,23.20),(1.16,.12,.87),M["purple"],col,.03)
        # Landmark front entry strongly differentiated from early unfinished gate.
        for s in (-1,1):
            beam("imperial buttress flares",(s*7.20,-5.0,5.25),
                 (s*8.75,-6.30,.30),.65,M["stone"],col)
        for s in (-1,1):
            cube("imperial gate bronze banner",(s*4.65,-5.32,5.5),
                 (.85,.12,1.95),M["purple"],col,.045)
    else:
        # Bastion IV is substantial, but without the final summit tower.
        for s in (-1,1):
            beam("Bastion IV reinforced pier",
                 (s*6.65,-4.7,5.7),(s*7.9,-5.8,.20),.43,M["stone"],col)
def detailing(M):
    col="V5_Industries"
    # Sawmill: timber hoist and lanterns, functional structure not a generic cottage.
    sx,sy=-22,-7.5
    for i in range(8):
        yy=sy-3.2+i*.74
        cyl("sawmill stock log",(sx+3,yy,.38),.22,2.6,M["wood"],col,10,
            rotate=(pi/2,0,0))
    beam("sawmill timber gantry",(sx-5,sy-4,3.5),
         (sx+4.8,sy-4,3.5),.28,M["wood"],col)
    # Quarry: exposed rock face and a weight hanging from the lifting frame.
    qx,qy=-23.5,11.8
    for i in range(14):
        xx=qx-8+(i%5)*1.4; yy=qy+5+(i//5)*1.1
        cube("quarry worked stone profile",(xx,yy,terrain_z(xx,yy)+.5),
             (1.2,1.1,.8),M["ashlar"],col,.05)
    beam("quarry heavy hoist rope",(qx,qy,6.9),(qx,qy,3.55),.09,M["metal"],col)
    cube("quarry counterweighted block",(qx,qy,3.3),(1.4,1.28,.86),
         M["stone"],col,.08)
    # Granary: truly differentiated twin silos and grain sacks.
    gx,gy=22,-9.2
    for j in range(2):
        xx=gx+4.55+j*2.6
        for z in (1.55,2.5,3.7,4.9):
            cyl("granary bronze silo binding",
                (xx,gy+1,z),1.35,.11,M["gold"],col,20)
    for j in range(15):
        xx=gx-2.7+random.uniform(-2.3,2.3)
        yy=gy-5.5+random.uniform(-.5,1.2)
        cube("grain sack store",(xx,yy,.33),(.55,.9,.52),
             M["wheat"],col,.17)
    # Barracks reads military from its stone face and training parapet.
    bx,by=22.8,11.4
    for i in range(6):
        xx=bx-3.8+i*1.5
        cube("barracks machicolated parapet",(xx,by-3.4,5.98),
             (.88,.57,.84),M["stone"],col,.08)
    for j in range(4):
        x=bx+5.0;y=by-2.4+j*.85
        cyl("barracks training shield",(x,y,1.7),.42,.13,
            M["wood"],col,12,rotate=(pi/2,0,0))
        cyl("barracks shield iron boss",(x,y-.1,1.7),.15,.14,
            M["metal"],col,12,rotate=(pi/2,0,0))
def lighting():
    scene=bpy.context.scene
    scene.render.engine="CYCLES";scene.cycles.samples=18
    scene.cycles.use_denoising=False
    for v in scene.view_layers:v.cycles.use_denoising=False
    scene.render.threads_mode="FIXED";scene.render.threads=4
    scene.render.resolution_percentage=100;scene.render.image_settings.file_format="PNG"
    scene.view_settings.view_transform="Filmic"
    scene.view_settings.look="Medium High Contrast"
    scene.view_settings.exposure=.9
    sky=bpy.data.worlds.new("Valoria | outdoor warm blue atmosphere")
    scene.world=sky;sky.use_nodes=True
    bg=sky.node_tree.nodes.get("Background")
    bg.inputs["Color"].default_value=(.47,.49,.53,1)
    bg.inputs["Strength"].default_value=.83
    bpy.ops.object.light_add(type="SUN",location=(0,0,30))
    s=bpy.context.object;s.name="Sun | western low amber"
    s.rotation_euler=(radians(60),radians(8),radians(-32))
    s.data.energy=3.25;s.data.color=(1.0,.81,.64);s.data.angle=radians(4.1)
    for x in (-1.85,1.85):
        bpy.ops.object.light_add(type="POINT",location=(x,-5.6,3.8))
        l=bpy.context.object
        l.data.energy=120;l.data.color=(1,.58,.31)
        l.data.shadow_soft_size=1.2
    bpy.ops.object.camera_add(location=(35,-48,42))
    cam=bpy.context.object;cam.name="TRUE V5 sandbox camera"
    cam.data.type="ORTHO"
    cam.data.ortho_scale=60
    cam.rotation_euler=(Vector((0,-1,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.camera=cam
def screenshot(stage,width,height,name,portrait=False):
    scene=bpy.context.scene
    cam=scene.camera
    cam.location=(31,-44,41) if portrait else (35,-48,42)
    cam.rotation_euler=(Vector((0,-1,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    cam.data.ortho_scale=49 if portrait else 60
    scene.render.resolution_x=width;scene.render.resolution_y=height
    dest=PREV/name;scene.render.filepath=str(dest)
    bpy.ops.render.render(write_still=True)
    data=dest.read_bytes()
    if len(data)<100000 or data[:8]!=b"\x89PNG\r\n\x1a\n":raise RuntimeError("Render invalid "+str(dest))
    FRAMES.append({"stage":stage,"filename":name,"bytes":len(data),
                   "width":width,"height":height,"render_type":"genuine Blender output (not Unity)"})
    print("ELDORIA_DIRECTOR_V5_AUTHENTIC_FRAME",str(dest),len(data))
def model(stage):
    out=OUT/("DirectorV5_Stage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(out),export_format="GLB",export_apply=True)
    dat=out.read_bytes()
    if dat[:4]!=b"glTF" or len(dat)<500000:raise RuntimeError("Missing real GLB "+str(out))
    if struct.unpack("<I",dat[8:12])[0]!=len(dat):raise RuntimeError("GLB length corrupt")
    n=struct.unpack("<I",dat[12:16])[0];gltf=json.loads(dat[20:20+n])
    entry={"stage":stage,"file":out.name,"bytes":len(dat),
           "meshes":len(gltf.get("meshes",[])),"materials":len(gltf.get("materials",[])),
           "images":len(gltf.get("images",[]))}
    if entry["images"]<8:raise RuntimeError("Lost embedded textures "+str(entry))
    MESHES.append(entry)
    print("ELDORIA_DIRECTOR_V5_PHYSICAL_MODEL",json.dumps(entry))
def run(stage):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    source=BASE/("DirectorV4_ValoriaStage"+str(stage)+".glb")
    if not source.is_file():raise RuntimeError("Exact original V4 source missing "+str(source))
    bpy.ops.import_scene.gltf(filepath=str(source),import_pack_images=True)
    random.seed(201126)
    M={k:mat(k,c) for k,c in {
       "road":"#62543F","edge":"#3E5141","grass":"#4F6C4A",
       "foliage1":"#244438","foliage2":"#355846","foliage3":"#4E7059",
       "bark":"#42382D","stone":"#77807A","ashlar":"#A4A396",
       "roof":"#334450","gold":"#AC8551","purple":"#533851",
       "amber":"#C28F5E","metal":"#272D33",
       "wood":"#664A31","rock":"#59645D","wheat":"#AA925C"}.items()}
    removed=replace_forest(M)
    improve_terrain(M)
    detailing(M)
    enhance_bastion(M,stage)
    lighting()
    if stage==1:screenshot(stage,1600,900,"DirectorV5_BastionI_1600x900.png")
    if stage==5:
        screenshot(stage,1600,900,"DirectorV5_BastionV_1600x900.png")
        screenshot(stage,720,1200,"DirectorV5_BastionV_Portrait_720x1200.png",True)
        bpy.context.scene.render.filepath=""
        bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"DirectorV5_Source.blend"))
    model(stage)
    REVIEW.append({"stage":stage,"replaced_generic_tree_objects":removed})
for level in range(1,6):run(level)
report={"workstream":"eldoria-cc0-art-system-20261116",
        "name":"Director V5 — real scene, not AI lookdev",
        "upstream_v4_run_id":43870376065,
        "owned_original_source":True,"third_party_source_imported":False,
        "real_blender_render":True,"native_unity_verified":False,
        "iphone_verified":False,"art_approved":False,
        "evidence":{"levels":MESHES,"frames":FRAMES,"per_stage":REVIEW}}
(PREV/"director-v5-evidence.json").write_text(json.dumps(report,indent=2)+"\n")
print("ELDORIA_DIRECTOR_V5_REAL_ART_PASS",len(MESHES),len(FRAMES))
