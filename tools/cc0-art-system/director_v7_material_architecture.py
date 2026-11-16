#!/usr/bin/env python3
"""Director V7 / ELDORIA: physical authored corrections to V6, not AI images.

V6 real GLBs from Actions 43878136136 are the ONLY input. For all 5 levels:
- remove two false modular pyramid annex roofs; replace with connected stone
  wings and pitched slate roof geometry based on proper gable surfaces;
- bake color correction into the ORIGINAL embedded 3D textures (V6 shader-only
  tint did not touch them), preserve GLB textures/vertex painted terrain;
- replace visibly mint green triangular trees by dark, asymmetrical branching
  conifers, trim pale rock and soften industrial roof contrasts;
- rebuild the keep's frontage and distinctive production attachments;
- preserve exact spatial anchors, render the same camera I/V + phone portrait.
Original source geometry only. No Unity or physical-phone pass claimed.
"""
import bpy,math,json,struct,random,hashlib,io
import numpy as np
from PIL import Image as PILImage
from pathlib import Path
from math import sin,cos,pi,radians
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[2]
SRC=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV6"
OUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV7"
PRE=ROOT/"docs/cc0-art-system/previews/director-v7"
for p in (OUT,PRE):p.mkdir(parents=True,exist_ok=True)
FRAMES=[];LEVELS=[];METRICS=[]
def rgb(h):
    return tuple(int(h[i:i+2],16)/255 for i in (1,3,5))
def material(name,h,rough=.88,metal=0,emit=0):
    x=bpy.data.materials.new("ELDORIA_V7_"+name)
    x.use_nodes=True
    col=rgb(h);x.diffuse_color=(*col,1)
    bs=x.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*col,1)
    bs.inputs["Roughness"].default_value=rough
    bs.inputs["Metallic"].default_value=metal
    if emit:
        for s in ("Emission Color","Emission"):
            if s in bs.inputs:bs.inputs[s].default_value=(*col,1);break
        if "Emission Strength" in bs.inputs:bs.inputs["Emission Strength"].default_value=emit
    return x
def palette():
    return {k:material(k,*v) for k,v in {
        "stone":("#7D807A",.89),"limestone":("#9E9E8E",.86),
        "shadow":("#46525B",.95),"slate":("#2A3947",.82),
        "bronze":("#9A7348",.62,.33),"iron":("#262D31",.59,.36),
        "oak":("#554333",.88),"bark":("#362F2A",.96),
        "leaf1":("#152E27",.98),"leaf2":("#234236",.98),
        "leaf3":("#355443",.98),"moss":("#3A5140",.99),
        "rock":("#48534F",.94),"earth":("#574E3C",.96),
        "banner":("#503A58",.91),"amber":("#E5A35E",.41,0,1.3),
        "grain":("#A18A56",.97),"grass":("#465E45",.99),
        "purple":("#735B80",.96)}.items()}
def group(name):
    c=bpy.data.collections.get(name)
    if not c:
        c=bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(c)
    return c
def as_mesh(name,verts,faces,mat,col):
    m=bpy.data.meshes.new(name+"_geo");m.from_pydata(verts,[],faces);m.update()
    m.materials.append(mat)
    o=bpy.data.objects.new(name,m)
    group(col).objects.link(o)
    return o
def box(name,p,d,mat,col,b=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p)
    o=bpy.context.object;o.name=name;o.dimensions=d
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    for c in list(o.users_collection):c.objects.unlink(o)
    group(col).objects.link(o)
    o.data.materials.append(mat)
    if b:
        m=o.modifiers.new("v7 hand-cut edge","BEVEL")
        m.width=b;m.segments=1
    return o
def cyl(name,p,r,h,mat,col,steps=12,rot=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=steps,radius=r,depth=h,location=p)
    o=bpy.context.object;o.name=name
    if rot:o.rotation_euler=rot
    for c in list(o.users_collection):c.objects.unlink(o)
    group(col).objects.link(o);o.data.materials.append(mat)
    return o
def cone(name,p,r,h,mat,col,segments=10,top=.01):
    bpy.ops.mesh.primitive_cone_add(vertices=segments,radius1=r,radius2=top,depth=h,location=p)
    o=bpy.context.object;o.name=name
    for c in list(o.users_collection):c.objects.unlink(o)
    group(col).objects.link(o);o.data.materials.append(mat)
    return o
def beam(name,a,b,t,mat,col):
    a=Vector(a);b=Vector(b);d=b-a
    if d.length<.01:return None
    o=box(name,(a+b)*.5,(t,t,d.length),mat,col,.035)
    o.rotation_euler=d.to_track_quat("Z","Y").to_euler()
    return o
def ridged_slate(name,x,y,w,d,eave,rise,mt,col):
    """Real double-pitched roof + gable end triangles and masonry eave treatment.

    No visible pyramid "module" pitched four ways. Ridge aligns with hall length,
    and slate sections join the masonry beneath.
    """
    v=[(x-w/2,y-d/2,eave),(x+w/2,y-d/2,eave),
       (x-w/2,y+d/2,eave),(x+w/2,y+d/2,eave),
       (x,y-d/2,eave+rise),(x,y+d/2,eave+rise)]
    roof=as_mesh(name+" / joined double pitched slate",v,
       [(0,4,5,2),(4,1,3,5)],mt["slate"],col)
    as_mesh(name+" / filled limestone gables",v,
            [(0,1,4),(2,5,3)],mt["limestone"],col)
    beam(name+" / aged bronze ridge",(x,y-d/2-.10,eave+rise),
         (x,y+d/2+.10,eave+rise),.13,mt["bronze"],col)
    for edge in (-1,1):
        beam(name+" / oak eaves",(x+edge*w/2,y-d/2,eave-.03),
             (x+edge*w/2,y+d/2,eave-.03),.17,mt["oak"],col)
        # Thin slate coursing adds material richness without repeating boxes as separate roofs.
        for j in range(1,6):
            fx=j/6
            ex=x+edge*w*.5*(1-fx)
            ez=eave+rise*fx
            beam(name+" / roof slate course", (ex,y-d/2+.09,ez+.025),
                 (ex,y+d/2-.09,ez+.025),.035,mt["shadow"],col)
    return roof
def remove_exact():
    prefixes=("fortress linked lateral chapel","keep slate vaulted pyramidal roof",
              "buttress integrated to main keep","V6 irregular 3D fir",
              "organic crooked conifer trunk")
    deleted={}
    for o in list(bpy.data.objects):
        if o.type!="MESH":continue
        for p in prefixes:
            if o.name.startswith(p):
                bpy.data.objects.remove(o,do_unlink=True)
                deleted[p]=deleted.get(p,0)+1
                break
    return deleted
def repaint_real_pngs(source_glb):
    """Extract and color-grade actual PNGs from the physical GLB BIN buffer.

    Blender 4 imported several packed pictures with no resolvable filepath or
    pixel buffer, so image.reload and image.pixels cannot alter their bytes.
    Operate on the exact physically embedded PNGs instead, save new own source
    textures, then reconnect image texture nodes to those real local files.
    Verify sha256(before)!=sha256(after) for every re-baked image.
    """
    ratios={
      "DirectorV4_stone_Diffuse":(.77,.75,.72),
      "DirectorV4_dark_Diffuse":(.84,.85,.86),
      "DirectorV4_edge_Diffuse":(.83,.82,.77),
      "DirectorV4_roof_Diffuse":(.78,.83,.89),
      "DirectorV4_rock_Diffuse":(.65,.72,.71),
      "DirectorV4_wood_Diffuse":(.81,.77,.72),
      "DirectorV4_wooddark_Diffuse":(.83,.79,.74),
      "ED_CC0_ForgedIron":(.84,.86,.88),
      "ED_CC0_AgedBronze":(.87,.82,.74)}
    raw=source_glb.read_bytes()
    json_length=struct.unpack_from("<I",raw,12)[0]
    meta=json.loads(raw[20:20+json_length])
    at=20+json_length
    bin_length,bin_type=struct.unpack_from("<I4s",raw,at)
    if bin_type!=b"BIN\x00":raise RuntimeError("Source glTF has no BIN chunk")
    base_offset=at+8
    staged={}
    files=[]
    for image in meta.get("images",[]):
        name=image.get("name","")
        factor=ratios.get(name)
        if factor is None or "bufferView" not in image:continue
        view=meta["bufferViews"][image["bufferView"]]
        start=base_offset+view.get("byteOffset",0)
        end=start+view["byteLength"]
        original=raw[start:end]
        if not original.startswith(b"\x89PNG\r\n\x1a\n"):
            raise RuntimeError("Physical source image is not PNG: "+name)
        picture=PILImage.open(io.BytesIO(original)).convert("RGBA")
        arr=np.asarray(picture).astype(np.float32).copy()
        for chan in range(3):
            arr[:,:,chan]*=factor[chan]
        arranged=PILImage.fromarray(np.clip(arr,0,255).astype(np.uint8),"RGBA")
        outpath=OUT/("DirectorV7_REBAKED_"+name+".png")
        arranged.save(outpath,format="PNG",optimize=True)
        changed=outpath.read_bytes()
        before=hashlib.sha256(original).hexdigest()
        after=hashlib.sha256(changed).hexdigest()
        if before==after:raise RuntimeError("Physical image stayed identical: "+name)
        loaded=bpy.data.images.load(str(outpath),check_existing=False)
        loaded.name="DirectorV7_REBAKED_"+name
        staged[name]=loaded
        files.append({"name":name,"old_sha256":before,"new_sha256":after,
                      "bytes":len(changed)})
    replaced=0
    for material in bpy.data.materials:
        if not material.use_nodes:continue
        for node in material.node_tree.nodes:
            if node.type!="TEX_IMAGE" or not node.image:continue
            oldname=node.image.name.split(".")[0]
            found=staged.get(oldname)
            if found:
                node.image=found
                replaced+=1
    if replaced<6 or len(files)<6:
        raise RuntimeError("Cannot physically reconnect rebaked GLB materials: "+
                           repr((len(files),replaced,
                                 [x.name for x in bpy.data.images][:35])))
    # Normalize untextured stone and rocks as an additional safety net.
    for m in bpy.data.materials:
        if not m.use_nodes:continue
        if m.name.startswith("ELDORIA_V6_rocklight"):
            bs=m.node_tree.nodes.get("Principled BSDF")
            if bs:bs.inputs["Base Color"].default_value=(.35,.38,.35,1)
    print("ELDORIA_V7_ACTUAL_PBR_IMAGE_BYTES_CHANGED",len(files),replaced)
    return len(files),files

def grow_fir(x,y,z,h,mt):
    """Branch-like, uneven, downward fir silhouettes, not stacked ice-cream cones.

    Each 3D tree is a mesh of individual radial bough fans, sloping from a
    central irregular spine. The silhouette has outward growing lower branches,
    so its recognisable mature-fir shape is natural at an isometric camera.
    """
    col="V7_FIR_FOREST"
    cyl("dark living fir trunk",(x,y,z+h*.29),h*.035,h*.59,mt["bark"],col,7)
    verts=[];faces=[];ids=[]
    for level in range(6):
        t=level/5
        centerz=z+h*(.38+.55*t)
        spread=h*(.215-.169*t)
        cx=x+sin(t*4.3)*h*.028
        cy=y+cos(t*5.1)*h*.021
        strands=random.randint(9,13)
        for k in range(strands):
            ang=2*pi*(k/strands)+random.uniform(-.24,.24)+level*.13
            radial=spread*random.uniform(.7,1.30)
            inner=radial*.32
            tipx=cx+radial*cos(ang);tipy=cy+radial*sin(ang)
            endz=centerz-radial*.19+random.uniform(-.09,.04)*h/6
            ax=cx+inner*cos(ang-.28);ay=cy+inner*sin(ang-.28)
            bx=cx+inner*cos(ang+.28);by=cy+inner*sin(ang+.28)
            topz=centerz+h*.10
            off=len(verts)
            verts.extend([
                (ax,ay,topz),(bx,by,topz),
                (tipx,tipy,endz),
                (tipx+radial*.08*cos(ang+pi/2),
                 tipy+radial*.08*sin(ang+pi/2),endz+h*.028)])
            faces.extend([(off,off+1,off+2),(off+1,off+3,off+2)])
            ids.extend([random.randrange(3),random.randrange(3)])
    mesh=bpy.data.meshes.new("evergreen spread bough mesh")
    mesh.from_pydata(verts,[],faces);mesh.update()
    for k in ("leaf1","leaf2","leaf3"):mesh.materials.append(mt[k])
    for face,mi in zip(mesh.polygons,ids):face.material_index=mi
    obj=bpy.data.objects.new("Eldoria V7 / asymmetrical branched spruce",mesh)
    group(col).objects.link(obj)
def rewrite_forest(mt):
    random.seed(161126)
    count=0
    for k in range(160):
        x=random.uniform(-56,56);y=random.uniform(-51,51)
        if abs(x)<19 and -29<y<27:continue
        if abs(x+23)<11 and -17<y<21:continue
        if abs(x-23)<11 and -18<y<21:continue
        h=random.uniform(5.0,11.0)
        z=.12*sin(x*.28)*cos(y*.21)+.17*sin(x*.11+y*.16)
        grow_fir(x,y,z,h,mt);count+=1
    # Low broad bushes contrast with tall conifers, an ecological break from
    # the earlier mono-species cone forest.
    for i in range(95):
        x=random.uniform(-51,51);y=random.uniform(-45,45)
        if abs(x)<11 and -21<y<20:continue
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=random.uniform(.4,.94),
                                            location=(x,y,.41))
        ob=bpy.context.object;ob.name="Eldoria V7 / bramble and sage bush"
        ob.scale=(1.4,1.1,.63)
        for c in list(ob.users_collection):c.objects.unlink(ob)
        group("V7_GROUND_SHRUBS").objects.link(ob)
        ob.data.materials.append(mt["leaf2" if i%3 else "leaf3"])
    return count
def integrated_architecture(stage,mt):
    # Original V6 annex roof form was a pyramid. Entire annex components were
    # removed; new wings share stone language with the same central keep.
    if stage>=4:
        for side in (-1,1):
            x=side*6.15; y=5.7
            box("stone masonry / attached nave wing",(x,y,4.45),
                (4.3,7.5,6.4),mt["stone"],"V7_ARCHITECTURE",.13)
            ridged_slate("joined lateral slate nave",x,y,4.68,7.8,7.85,2.75,
                         mt,"V7_ARCHITECTURE")
            for yy in (3.3,6.1,8.8):
                box("tall inset nave lancet",(x+side*2.19,yy,5.25),
                    (.12,.68,1.80),mt["amber"],"V7_ARCHITECTURE",.03)
            # Three buttresses actually connect to the outer wall, not hovering.
            for yy in (2.8,6,9.0):
                beam("flying chapel buttress",(x+side*2.18,yy,6.2),
                     (x+side*3.37,yy,1.2),.41,mt["limestone"],"V7_ARCHITECTURE")
            box("village face bronze fascia",(x,y-3.80,7.92),
                (3.35,.22,.23),mt["bronze"],"V7_ARCHITECTURE",.03)
    # Industry silhouettes must belong to different crafts rather than four houses.
    # Industrial sawmill is visibly open, hence only external construction remains.
    sx,sy=-22,-7.5
    for yy in (-12.5,-11.6,-10.7):
        beam("sawmill exposed timber stack",(-24.7,yy,.65),
             (-21.8,yy,.65),.26,mt["oak"],"V7_INDUSTRIES")
    # Quarry cut rock bench: 3 tiers are physically linked to excavation slope.
    for j in range(3):
        x=-29.5+j*.66;y=13.4+j*1.28
        box("quarry stepped stone face",(x,y,1.1+j*.43),
            (4.15-j*.4,1.5,.9+j*.23),mt["rock"],"V7_INDUSTRIES",.05)
    # Granary ventilation, real storage design.
    for x in (25.9,28.8):
        for z in (2.6,3.8,5.0):
            box("granary vent in silo wall",(x,-9.6,z),(.58,.13,.24),
                mt["shadow"],"V7_INDUSTRIES",.025)
    # Barracks stout military face and shield rack.
    for i in range(4):
        x=20+i*1.05
        cyl("barracks shield with iron boss",(x,6.7,1.55),.35,.12,
            mt["bronze"],"V7_INDUSTRIES",12,rot=(pi/2,0,0))
    if stage==5:
        # An unmistakable rebuilt imperial identity, not another generic tower.
        box("monumental kept gallery portal",(0,2.67,11.9),
            (4.25,.62,2.18),mt["stone"],"V7_ARCHITECTURE",.08)
        cyl("imperial metal heraldic medallion",(0,2.30,12.25),
            .65,.15,mt["bronze"],"V7_ARCHITECTURE",18,rot=(pi/2,0,0))
        # Tall central loggia and finials above the previous wall line.
        for s in (-1,1):
            box("rebuilt central buttress quoins",(s*2.45,2.43,12.1),
                (.49,.72,4.5),mt["limestone"],"V7_ARCHITECTURE",.04)
            cone("restored roof edge finial",(s*2.4,2.5,15.0),
                 .36,1.25,mt["bronze"],"V7_ARCHITECTURE",8)
def setup_light_camera():
    scene=bpy.context.scene
    scene.render.engine="CYCLES"
    scene.cycles.samples=24
    scene.cycles.use_denoising=False
    for l in scene.view_layers:l.cycles.use_denoising=False
    scene.render.threads_mode="FIXED";scene.render.threads=4
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    scene.view_settings.view_transform="Filmic"
    scene.view_settings.look="Medium High Contrast"
    scene.view_settings.exposure=.15
    w=bpy.data.worlds.new("Eldoria V7 / dusk ambient")
    scene.world=w;w.use_nodes=True
    bg=w.node_tree.nodes.get("Background")
    bg.inputs["Color"].default_value=(.28,.31,.41,1)
    bg.inputs["Strength"].default_value=.60
    for o in list(bpy.data.objects):
        if o.type=="LIGHT" or o.type=="CAMERA":
            bpy.data.objects.remove(o,do_unlink=True)
    bpy.ops.object.light_add(type="SUN",location=(0,0,25))
    sun=bpy.context.object
    sun.rotation_euler=(radians(71),radians(10),radians(-41))
    sun.data.energy=2.65;sun.data.color=(1,.77,.58)
    sun.data.angle=radians(3.8)
    for x in (-1.8,1.8):
        bpy.ops.object.light_add(type="POINT",location=(x,-5.8,3.9))
        l=bpy.context.object
        l.data.energy=90;l.data.color=(1,.53,.29)
        l.data.shadow_soft_size=.9
    bpy.ops.object.camera_add(location=(39,-54,46))
    cam=bpy.context.object;cam.name="V7 actual isometric comparison"
    cam.data.type="ORTHO";cam.data.ortho_scale=68
    cam.rotation_euler=(Vector((0,0,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.camera=cam
def snapshot(stage,name,w,h,portrait=False):
    s=bpy.context.scene;cam=s.camera
    cam.location=(29,-46,43) if portrait else (39,-54,46)
    cam.rotation_euler=(Vector((0,0,5))-cam.location).to_track_quat("-Z","Y").to_euler()
    cam.data.ortho_scale=60 if portrait else 68
    s.render.resolution_x=w;s.render.resolution_y=h
    path=PRE/name;s.render.filepath=str(path)
    bpy.ops.render.render(write_still=True)
    data=path.read_bytes()
    if len(data)<85000 or data[:8]!=b'\x89PNG\r\n\x1a\n':
        raise RuntimeError("Missing actual Blender V7 image "+str(path))
    FRAMES.append({"stage":stage,"file":name,"bytes":len(data),"size":[w,h]})
    print("ELDORIA_V7_REAL_BLENDER_FRAME",stage,name,len(data))
def export(stage):
    file=OUT/("DirectorV7_Stage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(file),export_format="GLB",export_apply=True)
    dat=file.read_bytes()
    if dat[:4]!=b'glTF' or struct.unpack_from('<I',dat,8)[0]!=len(dat):
        raise RuntimeError("V7 GLB invalid")
    size=struct.unpack_from('<I',dat,12)[0]
    j=json.loads(dat[20:20+size])
    info={"stage":stage,"size_bytes":len(dat),
          "mesh_count":len(j.get("meshes",[])),
          "embedded_images":len(j.get("images",[])),
          "terrain_vertex_color":any(
              any("COLOR_0" in q.get("attributes",{}) for q in m.get("primitives",[]))
              for m in j.get("meshes",[]))}
    if not info["terrain_vertex_color"] or info["embedded_images"]<8:
        raise RuntimeError("V7 lost physical ground or embedded material textures "+str(info))
    LEVELS.append(info)
    print("ELDORIA_V7_ACTUAL_GLTF",json.dumps(info))
def build(stage):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    source=SRC/("DirectorV6_Stage"+str(stage)+".glb")
    if not source.is_file():raise RuntimeError("Exact V6 source missing "+str(source))
    bpy.ops.import_scene.gltf(filepath=str(source),import_pack_images=True)
    random.seed(161126)
    mt=palette()
    discarded=remove_exact()
    n,files=repaint_real_pngs(source)
    if n<5:raise RuntimeError("Source textures were not actually re-baked into V7: "+str(files))
    trees=rewrite_forest(mt)
    integrated_architecture(stage,mt)
    setup_light_camera()
    if stage==1:snapshot(stage,"DirectorV7_BastionI_1600x900.png",1600,900)
    if stage==5:
        snapshot(stage,"DirectorV7_BastionV_1600x900.png",1600,900)
        snapshot(stage,"DirectorV7_BastionV_Portrait_720x1200.png",720,1200,True)
        bpy.context.scene.render.filepath=""
        for i in bpy.data.images:
            if i.source=="FILE" and not i.packed_file:
                try:i.pack()
                except Exception:pass
        bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"DirectorV7_Editable_Source.blend"))
    export(stage)
    METRICS.append({"stage":stage,"removed_previous_modular_parts":discarded,
                    "baked_texture_images":n,"baked_texture_changes":files,
                    "real_fir_trees":trees})
for stage in range(1,6):build(stage)
evidence={"experiment":"Eldoria CC0 Art System",
          "version":"Director V7 — masonry, integrated slate gables and irregular dark forest",
          "source_run":43878136136,"source_art_is_original":True,
          "vendor_assets_included":False,"rendered_in_blender":True,
          "native_unity_tested":False,"iphone_tested":False,"art_approved":False,
          "levels":LEVELS,"frames":FRAMES,"metrics":METRICS}
(PRE/"director-v7-evidence.json").write_text(json.dumps(evidence,indent=2,ensure_ascii=False)+"\n")
print("ELDORIA_V7_PHYSICAL_SOURCE_AND_RENDER_PASS",len(LEVELS),len(FRAMES))
