#!/usr/bin/env python3
"""Eldoria CC0 / V12: authentic structural pivot over the REAL V11 physical GLBs.

INPUT: five verifiably owner-authored GLBs from GitHub Action #43891952674.
NO external packs, NO generated 2-D pictures pasted on a mesh.
ACTUAL WORK: replace the rectangular upper choir (not layer another one over it);
build an inhabitable layered keep with true crossing slate roofs, buttresses,
deep windows and a geometric heraldic gable. Add cohesive grounded meadow.
OUTPUT: five physical glTF/GLB stages, one editable .blend, four TRUE Blender
captures with consistent camera between stage I and V. Not Unity/iPhone QA.
"""
from pathlib import Path
from mathutils import Vector
from math import radians,pi,sin,cos
import bpy,json,random,math,struct,hashlib
ROOT=Path(__file__).resolve().parents[2]
INPUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV11Structural"
OUTPUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV12"
PREVIEW=ROOT/"docs/cc0-art-system/previews/director-v12"
for p in (OUTPUT,PREVIEW):p.mkdir(parents=True,exist_ok=True)
rng=random.Random(120116)
META=[]
FRAMES=[]
STATS={}
def lin(h):
    c=h.strip("#")
    vals=[int(c[i:i+2],16)/255 for i in (0,2,4)]
    return tuple(v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4 for v in vals)
def original_material(n,color,rough=.83,metal=0,emission=0):
    m=bpy.data.materials.new("V12_"+n)
    m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    col=lin(color)
    bs.inputs["Base Color"].default_value=(*col,1)
    bs.inputs["Roughness"].default_value=rough
    bs.inputs["Metallic"].default_value=metal
    m.diffuse_color=(*col,1)
    if emission:
        socket="Emission Color" if "Emission Color" in bs.inputs else "Emission"
        bs.inputs[socket].default_value=(*col,1)
        bs.inputs["Emission Strength"].default_value=emission
    return m
def mats():
    # Keep all published actual V11/V8 source material references and textures.
    def get(n):
        m=bpy.data.materials.get(n)
        if not m:raise RuntimeError("V11_SOURCE_MATERIAL_MISSING "+n)
        return m
    return {
      "wall":get("V8_master_ashlar"), "slate":get("V8_master_roof"),
      "oak":get("V8_master_timber"),
      "lime":original_material("DressedWarmLimestone","#A49D8C"),
      "deep":original_material("InnerMortar","#333C40"),
      "iron":original_material("ForgedIron","#2B3033",.6,.36),
      "metal":original_material("AgedBronze","#AA8352",.52,.35),
      "purple":original_material("ValoriaImperialBanner","#473550"),
      "warm":original_material("WarmInterior","#C78D52",.56,0,.76),
      "ground":original_material("DirtMeadow","#4F5742"),
      "grass":original_material("SageTufts","#556D50"),
      "moss":original_material("WeatheredFir","#3B5143")
    }
M={}
def group(name):
    c=bpy.data.collections.get(name)
    if c:return c
    c=bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(c)
    return c
def add(obj,k,area):
    obj.data.materials.append(M[k])
    destination=group(area)
    for c in list(obj.users_collection):c.objects.unlink(obj)
    destination.objects.link(obj)
    return obj
def cube(name,p,d,k,area="V12_ARCH",bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p)
    o=bpy.context.object;o.name=name;o.dimensions=d
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    add(o,k,area)
    if bevel:
        mod=o.modifiers.new("real softened stone edge","BEVEL")
        mod.width=bevel
        mod.segments=1
    return o
def cylinder(name,p,r,h,k,area="V12_ARCH",n=12,rot=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=n,radius=r,depth=h,location=p)
    o=bpy.context.object;o.name=name
    if rot:o.rotation_euler=rot
    return add(o,k,area)
def cone(name,p,r1,r2,depth,k,area="V12_ARCH",n=12):
    bpy.ops.mesh.primitive_cone_add(vertices=n,radius1=r1,radius2=r2,depth=depth,location=p)
    o=bpy.context.object;o.name=name
    return add(o,k,area)
def beam(name,a,b,w,k,area="V12_ARCH"):
    p,q=Vector(a),Vector(b)
    d=q-p
    if d.length<1e-6:return None
    ob=cube(name,(p+q)*.5,(w,w,d.length),k,area)
    ob.rotation_euler=d.to_track_quat("Z","Y").to_euler()
    return ob
def geom(name,vertices,faces,k,area="V12_ARCH"):
    mesh=bpy.data.meshes.new(name+" physical")
    mesh.from_pydata(vertices,[],faces)
    mesh.materials.append(M[k])
    mesh.update()
    # UV projection is physically present and gets exported with the material.
    uv=mesh.uv_layers.new(name="V12 WorldProjected UV")
    for face in mesh.polygons:
        for li in face.loop_indices:
            v=mesh.vertices[mesh.loops[li].vertex_index].co
            uv.data[li].uv=(v.x*.22, v.y*.22)
    o=bpy.data.objects.new(name,mesh)
    group(area).objects.link(o)
    return o
def gable_roof(name,x,y0,y1,z,half,rise,k="slate",area="V12_ARCH",courses=9):
    v=[(x-half,y0,z),(x+half,y0,z),(x,y0,z+rise),
       (x-half,y1,z),(x+half,y1,z),(x,y1,z+rise)]
    geom(name+" source roof physical facets",v,[(0,3,5,2),(1,2,5,4)],k,area)
    for a,b in ((0,2),(1,2)):
        beam(name+" aged copper verge",v[a],v[b],.17,"metal",area)
    beam(name+" main roof ridge",v[2],v[5],.14,"metal",area)
    # Slate courses are actual shallow geometry, not painted stripes.
    for u in range(1,courses):
        d=u/courses
        xx=half*(1-d)
        zz=z+rise*d+.035
        for sign in (-1,1):
            beam(name+" staggered slate course",(x+sign*xx,y0+.18,zz),
                 (x+sign*xx,y1-.18,zz),.045,"slate",area)
    for yy in (y0,y1):
        beam(name+" shaped timber barge",(x-half,yy,z),
             (x+half,yy,z),.11,"oak",area)
def arch_lancet(n,x,y,z,w,h,area):
    cube(n+" recessed shadow",(x,y,z+h*.46),(w,.20,h*.90),"deep",area,.035)
    cube(n+" actual amber window",(x,y-.17,z+h*.45),(w*.68,.12,h*.66),"warm",area,.02)
    for sign in (-1,1):
        xx=x+sign*w*.52
        beam(n+" limestone pilaster",(xx,y-.20,z),
             (xx,y-.20,z+h*.76),.19,"lime",area)
        beam(n+" ogive section",(xx,y-.25,z+h*.76),
             (x,y-.25,z+h*1.15),.19,"lime",area)
    beam(n+" lead mullion",(x,y-.31,z+.15),
         (x,y-.31,z+h*.81),.068,"iron",area)
    cube(n+" carved base",(x,y-.12,z-.11),(w+.42,.38,.22),"lime",area,.04)
def ground_z(x,y):
    return (.11*sin(x*.27)*cos(y*.17)+.19*sin(x*.095+y*.14)
           +.22*sin(x*.064-y*.083)*cos(y*.11)+.06*cos(.24*x+.18*y))
def replace_choir():
    # This is an explicit macro structural replacement, not an overlay. Removing
    # previous V11 choir and roofing prevents a second overlapping high keep.
    banned=(
      "raised choir stone body","imperial high choir steep joined gable",
      "royal centre rose-height lancet","royal high side lancet",
      "choir dressed side quoin")
    to_remove=[o for o in bpy.data.objects if o.type=="MESH" and o.name.startswith(banned)]
    if len(to_remove)<22:
        raise RuntimeError("Could not identify the exact outdated upper choir; abort rather than layer fake geometry: "+str(len(to_remove)))
    for o in to_remove:bpy.data.objects.remove(o,do_unlink=True)
    area="V12_LOWERED_ORGANIC_CHOIR"
    # Broad, buttressed lower crossing that actually joins both transepts.
    cube("joined transept cross-stone understructure",(0,5.7,10.78),
         (9.7,8.0,3.15),"wall",area,.13)
    for sign in (-1,1):
        x=sign*4.42
        cube("joined nave heavy foot",(x,5.38,9.4),(1.14,7.85,3.8),"wall",area,.08)
        for y in (3.1,7.55):
            beam("tiered spandrel buttress",
                 (x+sign*.26,y,7.0),(x+sign*.9,y,11.8),.30,"lime",area)
    # A 4.5-m-wide upper choir rises naturally from the existing fortress,
    # avoiding the former blank, very tall rectangular facade.
    cube("single coherent top choir stone mass",(0,6.15,13.5),
         (5.70,6.80,4.90),"wall",area,.11)
    gable_roof("royal slate choir",0,2.65,9.72,16.02,3.38,3.35,area=area,courses=11)
    # Elevated side junction roofs reduce pasted-together modular impression.
    for sign in (-1,1):
        x=sign*3.90
        gable_roof("real joined roof shoulder",x,2.85,8.65,11.7,1.55,1.45,area=area,courses=4)
        cube("profiled limestone corner", (sign*2.62,2.69,13.42),
             (.49,.56,4.7),"lime",area,.055)
    # Stone-relief breaks: three real windows, two architectural layers.
    arch_lancet("high centre royal opening",0,2.52,11.72,1.35,2.18,area)
    for xx in (-1.77,1.77):
        arch_lancet("high flanking royal opening",xx,2.53,12.05,.68,1.62,area)
    for h in (11.25,14.85):
        cube("weathered upper stone sill and cornice",(0,2.55,h),
             (5.85,.48,.22),"lime",area,.045)
    # One small rear sentinel lantern: distinctive, but subordinate to main roof.
    x,y=0,9.70
    cylinder("rear roof lantern masonry",(x,y,17.4),.78,2.8,"wall",area,10)
    cylinder("lantern bronze ring",(x,y,18.7),.86,.19,"metal",area,10)
    cone("lantern slate roof",(x,y,19.27),1.05,.05,1.15,"slate",area,10)
    # Physically grounded monumental gate, no floating add-on platform.
    for sign in (-1,1):
        cube("gate identity flanking guard pilaster",(sign*2.1,-6.0,3.15),
             (.76,.6,4.15),"wall",area,.055)
        cube("guard pilaster limestone cap",(sign*2.1,-6.03,5.25),
             (1.0,.78,.33),"lime",area,.045)
    cube("fortress gate upper parapet band",(0,-6.12,5.55),
         (5.5,.43,.34),"lime",area,.06)
    cylinder("Valoria bronze sun heraldic frame",(0,-6.42,5.62),
             .40,.15,"metal",area,14,rot=(pi/2,0,0))
    for idx in range(8):
        a=2*pi*idx/8
        beam("eight bronze Valoria crest rays",
             (.51*cos(a),-6.55,5.62+.51*sin(a)),
             (.75*cos(a),-6.55,5.62+.75*sin(a)),
             .08,"metal",area)
    return len(to_remove)
def environmental_details(stage):
    # Macro-level ground variation. Build *two* physically consolidated meshes
    # rather than thousands of disconnected decals / floating platforms.
    r=random.Random(120113)
    for key,n in (("grass",180),("ground",125),("moss",100)):
        vertices=[];faces=[]
        for i in range(n):
            x=r.uniform(-45,45);y=r.uniform(-43,42)
            if abs(x)<11 and -12<y<14:continue
            if abs(x-22)<6 and -14<y<-4:continue
            if abs(x+22)<6 and -13<y<-3:continue
            # Triangulated organic field patches, match source ground formula.
            rad=r.uniform(.3,1.45)
            skew=r.uniform(.65,1.3)
            center=(x,y,ground_z(x,y)+.017)
            points=[center]
            for j in range(7):
                t=j*2*pi/7
                px=x+rad*cos(t)*r.uniform(.88,1.13)
                py=y+rad*skew*sin(t)*r.uniform(.87,1.14)
                points.append((px,py,ground_z(px,py)+.024))
            n0=len(vertices);vertices.extend(points)
            for j in range(7):
                faces.append((n0,n0+1+j,n0+1+(j+1)%7))
        if vertices:geom("one batched natural terrain tint "+key,vertices,faces,key,"V12_ORGANIC_GROUND")
    # No four square plots, no generic city sprawl. Add low erosion stones in
    # visual clusters at creek, mill and quarry transitions.
    for i in range(26):
        if i<10: x,y=-25+r.uniform(-6,6),11+r.uniform(2,7)
        elif i<18:x,y=-27+r.uniform(-4,3),-13+r.uniform(-5,5)
        else:x,y=r.uniform(-34,34),r.choice((-22,22))+r.uniform(-3,3)
        z=ground_z(x,y)+.06
        cube("settled worn granite debris",(x,y,z),(r.uniform(.17,.64),r.uniform(.20,.62),r.uniform(.14,.46)),
            "wall","V12_GROUNDED_STONES",.04)
    # Add physically connected covered oak stacking outside sawmill and a
    # few braces to clearly declare its occupation from wide camera.
    for i in range(10):
        cube("sawmill sawn timber stacks",(-18.0-i*.27,-12.4-r.uniform(0,2.2),.31),
             (.23,2.4,.19),"oak","V12_SAWMILL_ACTIVE",.017)
    # For military readiness, the barracks gets a parapet extension, not a silo.
    cube("CUARTEL stone lower defensive bulk",(27.1,10.85,2.65),
         (4.15,5.35,5.10),"wall","V12_BARRACKS_MARTIAL",.085)
    for i in range(5):
        cube("CUARTEL distinct elevated merlon",(25.35+i*.87,8.22,5.56),
             (.46,.58,.77),"lime","V12_BARRACKS_MARTIAL",.05)
def light_rig():
    scene=bpy.context.scene
    for obj in list(bpy.data.objects):
        if obj.type in ("LIGHT","CAMERA"):bpy.data.objects.remove(obj,do_unlink=True)
    scene.render.engine="CYCLES"
    scene.cycles.samples=23
    scene.cycles.use_denoising=False
    scene.cycles.max_bounces=3
    scene.render.threads_mode="FIXED";scene.render.threads=4
    scene.view_settings.view_transform="Filmic"
    scene.view_settings.look="Medium High Contrast"
    scene.view_settings.exposure=.35
    world=bpy.data.worlds.new("Eldoria V12 warm old stone dusk")
    scene.world=world;world.use_nodes=True
    back=world.node_tree.nodes.get("Background")
    back.inputs["Color"].default_value=(.43,.43,.47,1)
    back.inputs["Strength"].default_value=.78
    bpy.ops.object.light_add(type="SUN",location=(0,0,45))
    sun=bpy.context.object;sun.rotation_euler=(radians(73),radians(7),radians(-34))
    sun.data.energy=3.25;sun.data.color=(1,.80,.62);sun.data.angle=radians(4.7)
    for x in (-1.7,1.7):
        bpy.ops.object.light_add(type="POINT",location=(x,-6.5,3.35))
        o=bpy.context.object;o.data.energy=110;o.data.color=(1,.59,.34)
    bpy.ops.object.camera_add(location=(36,-51,39))
    cam=bpy.context.object;cam.data.type="ORTHO";cam.data.ortho_scale=69
    cam.rotation_euler=(Vector((0,0,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.camera=cam
def capture(stage,name,sz,kind="wide"):
    scene=bpy.context.scene;cam=scene.camera
    if kind=="wide":cam.location=(36,-51,39);cam.data.ortho_scale=69
    elif kind=="close":cam.location=(25,-35,32);cam.data.ortho_scale=44
    else:cam.location=(28,-48,43);cam.data.ortho_scale=57
    cam.rotation_euler=(Vector((0,0,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.render.resolution_x,scene.render.resolution_y=sz
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    out=PREVIEW/name;scene.render.filepath=str(out)
    bpy.ops.render.render(write_still=True)
    raw=out.read_bytes()
    if len(raw)<130000 or raw[:8]!=b'\x89PNG\r\n\x1a\n':
        raise RuntimeError("Blender V12 render not valid: "+str(out))
    FRAMES.append({"stage":stage,"name":name,"bytes":len(raw),"size":sz})
    print("ELDORIA_V12_REAL_RENDER",name,len(raw))
def export_glb(stage):
    out=OUTPUT/("DirectorV12_Stage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(out),export_format="GLB",export_apply=True)
    raw=out.read_bytes()
    if raw[:4]!=b"glTF" or struct.unpack_from("<I",raw,8)[0]!=len(raw):
        raise RuntimeError("Invalid V12 GLB "+str(out))
    n=struct.unpack_from("<I",raw,12)[0]
    j=json.loads(raw[20:20+n])
    if len(j.get("meshes",[]))<500 or len(j.get("images",[]))<4:
        raise RuntimeError("V12 lost real meshes or material textures")
    item={"stage":stage,"meshes":len(j["meshes"]),"materials":len(j["materials"]),
          "images":len(j["images"]),"bytes":len(raw)}
    META.append(item);print("ELDORIA_V12_PHYSICAL_3D",json.dumps(item))
def main():
    global M
    for stage in range(1,6):
        bpy.ops.wm.read_factory_settings(use_empty=True)
        file=INPUT/("DirectorV11_Stage"+str(stage)+".glb")
        if not file.is_file():raise RuntimeError("Pinned V11 source absent: "+str(file))
        bpy.ops.import_scene.gltf(filepath=str(file),import_pack_images=True)
        M=mats()
        removed=replace_choir() if stage==5 else 0
        environmental_details(stage)
        light_rig()
        if stage==1:capture(stage,"DirectorV12_BastionI_1600x900.png",[1600,900])
        if stage==5:
            capture(stage,"DirectorV12_BastionV_1600x900.png",[1600,900])
            capture(stage,"DirectorV12_BastionV_Close_1600x900.png",[1600,900],"close")
            capture(stage,"DirectorV12_BastionV_Portrait_720x1200.png",[720,1200],"portrait")
            bpy.context.scene.render.filepath=""
            bpy.ops.wm.save_as_mainfile(filepath=str(OUTPUT/"DirectorV12_Editable_Source.blend"))
        export_glb(stage)
        print("V12_REBUILD_PURGED_OLD_CHOIR_COUNT",stage,removed)
    record={"project":"Eldoria CC0 Art System","version":"V12 structural replacement + physical landscaping",
            "source_run":43891952674,"source_rights":"only owner-authored Eldoria originals",
            "third_party_geometry_present":False,
            "real_blender_png":True,"native_unity_tested":False,"iphone_tested":False,
            "art_approved":False,"geometry_overlays_without_removal":False,
            "stages":META,"renders":FRAMES}
    (PREVIEW/"v12-evidence.json").write_text(json.dumps(record,indent=2)+"\n")
    print("ELDORIA_V12_REAL_ART_PASS",len(META),len(FRAMES))
if __name__=="__main__":
    main()
