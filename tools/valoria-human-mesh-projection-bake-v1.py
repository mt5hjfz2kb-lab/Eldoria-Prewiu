import bpy, os, math, json, zipfile, urllib.request
from mathutils import Vector
from bpy_extras.object_utils import world_to_camera_view
from PIL import Image
import numpy as np

OUT="/tmp/valoria-human-projection"
SRC=os.path.join(OUT,"source"); os.makedirs(SRC,exist_ok=True); os.makedirs(OUT,exist_ok=True)
ROOT="docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1"

def dl(url,path):
    req=urllib.request.Request(url,headers={"User-Agent":"EldoriaResearch/1.0"})
    with urllib.request.urlopen(req,timeout=180) as r, open(path,"wb") as f:
        while True:
            b=r.read(1024*1024)
            if not b: break
            f.write(b)

zip_path=os.path.join(SRC,"gatehouse.zip")
dl("https://opengameart.org/sites/default/files/76122_GateHouse2Upload_blend.zip",zip_path)
with zipfile.ZipFile(zip_path) as z: z.extractall(SRC)
blend=next((os.path.join(dp,f) for dp,_,fs in os.walk(SRC) for f in fs if f.lower().endswith(".blend")),None)
if not blend: raise RuntimeError("No gatehouse blend")
bpy.ops.wm.open_mainfile(filepath=blend)
meshes=[o for o in bpy.context.scene.objects if o.type=="MESH"]
if not meshes: raise RuntimeError("No human-authored meshes")

# Canonical target, tight semantic crop.
back=Image.open(os.path.join(ROOT,"gate-back.png")).convert("RGBA")
front=Image.open(os.path.join(ROOT,"gate-front.png")).convert("RGBA")
target=Image.alpha_composite(back,front)
bb=target.getbbox()
if not bb: raise RuntimeError("empty target")
target=target.crop(bb)
target_aspect=target.width/target.height
ta=np.asarray(target).astype(np.float32); mask=ta[...,3]>24; rgb=ta[...,:3]
valid=rgb[mask]; median=np.median(valid,axis=0) if len(valid) else np.array([95,88,78],dtype=np.float32)
a=ta[...,3:4]/255.0; filled=rgb*a+median*(1-a)
target_rgb=Image.fromarray(np.clip(filled,0,255).astype(np.uint8),"RGB")
target_path=os.path.join(OUT,"target-gate-tight.png"); target_rgb.save(target_path)

def bounds(objs):
    pts=[o.matrix_world@Vector(c) for o in objs for c in o.bound_box]
    mn=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
    mx=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
    return mn,mx,(mn+mx)*.5,mx-mn
mn,mx,ctr,dims=bounds(meshes)

# Conform only macro screen-space aspect; preserve authored depth/topology.
desired_width=dims.z*target_aspect
sx=desired_width/max(dims.x,1e-6)
for o in meshes:
    # scale around common center in X
    o.location.x=ctr.x+(o.location.x-ctr.x)*sx
    o.scale.x*=sx
bpy.context.view_layer.update()
mn,mx,ctr,dims=bounds(meshes)
span=max(dims.x,dims.z)

# Clean old lights/cameras only.
for o in list(bpy.context.scene.objects):
    if o.type in ("CAMERA","LIGHT"): bpy.data.objects.remove(o,do_unlink=True)

# Projector.
scene=bpy.context.scene
bpy.ops.object.camera_add(location=(ctr.x,ctr.y-span*2.2,ctr.z))
proj=bpy.context.object; proj.name="CanonicalProjector"; proj.data.type="ORTHO"; proj.data.ortho_scale=max(dims.z,dims.x/target_aspect)*1.02
proj.rotation_euler=(ctr-proj.location).to_track_quat("-Z","Y").to_euler(); scene.camera=proj; bpy.context.view_layer.update()
cam_dir=(ctr-proj.location).normalized()

# Projection material.
img=bpy.data.images.load(target_path,check_existing=True)
frontmat=bpy.data.materials.new("CanonicalProjectedFacade"); frontmat.use_nodes=True
n=frontmat.node_tree.nodes; l=frontmat.node_tree.links
for x in list(n): n.remove(x)
out=n.new("ShaderNodeOutputMaterial"); bs=n.new("ShaderNodeBsdfPrincipled"); tex=n.new("ShaderNodeTexImage"); tex.image=img; tex.extension="EXTEND"
uv=n.new("ShaderNodeUVMap"); uv.uv_map="CanonicalProjectionUV"; l.new(uv.outputs["UV"],tex.inputs["Vector"]); l.new(tex.outputs["Color"],bs.inputs["Base Color"]); bs.inputs["Roughness"].default_value=.72
bw=n.new("ShaderNodeRGBToBW"); bump=n.new("ShaderNodeBump"); bump.inputs["Strength"].default_value=.14; bump.inputs["Distance"].default_value=.04
l.new(tex.outputs["Color"],bw.inputs["Color"]); l.new(bw.outputs["Val"],bump.inputs["Height"]); l.new(bump.outputs["Normal"],bs.inputs["Normal"]); l.new(bs.outputs["BSDF"],out.inputs["Surface"])

side=bpy.data.materials.new("ValoriaFallbackStone"); side.use_nodes=True
ns=side.node_tree.nodes; ls=side.node_tree.links
for x in list(ns): ns.remove(x)
outs=ns.new("ShaderNodeOutputMaterial"); bss=ns.new("ShaderNodeBsdfPrincipled"); bss.inputs["Base Color"].default_value=(.18,.16,.135,1); bss.inputs["Roughness"].default_value=.82
noise=ns.new("ShaderNodeTexNoise"); noise.inputs["Scale"].default_value=7; noise.inputs["Detail"].default_value=5
coord=ns.new("ShaderNodeTexCoord"); bp=ns.new("ShaderNodeBump"); bp.inputs["Strength"].default_value=.22; bp.inputs["Distance"].default_value=.055
ls.new(coord.outputs["Generated"],noise.inputs["Vector"]); ls.new(noise.outputs["Fac"],bp.inputs["Height"]); ls.new(bp.outputs["Normal"],bss.inputs["Normal"]); ls.new(bss.outputs["BSDF"],outs.inputs["Surface"])

projected=0; fallback=0
for o in meshes:
    me=o.data; me.materials.clear(); me.materials.append(frontmat); me.materials.append(side)
    layer=me.uv_layers.get("CanonicalProjectionUV") or me.uv_layers.new(name="CanonicalProjectionUV")
    for poly in me.polygons:
        wn=(o.matrix_world.to_3x3()@poly.normal).normalized()
        facing=abs(wn.dot(cam_dir))
        use=facing>0.38
        poly.material_index=0 if use else 1
        projected+=1 if use else 0; fallback+=0 if use else 1
        for li in poly.loop_indices:
            vi=me.loops[li].vertex_index; co=o.matrix_world@me.vertices[vi].co
            ndc=world_to_camera_view(scene,proj,co); layer.data[li].uv=(ndc.x,ndc.y)
    bev=o.modifiers.new("PremiumEdge","BEVEL"); bev.width=min(.05,span*.006); bev.segments=2; bev.limit_method="ANGLE"; bev.angle_limit=math.radians(28)

# World, floor, lights.
if scene.world is None: scene.world=bpy.data.worlds.new("World")
scene.world.use_nodes=True; bg=scene.world.node_tree.nodes.get("Background"); bg.inputs["Color"].default_value=(.02,.027,.04,1); bg.inputs["Strength"].default_value=.38
bpy.ops.mesh.primitive_plane_add(size=span*4,location=(ctr.x,ctr.y,mn.z-.04)); gm=bpy.data.materials.new("Ground"); gm.diffuse_color=(.07,.065,.055,1); bpy.context.object.data.materials.append(gm)
for loc,en,size,col in [
 (ctr+Vector((-span*.75,-span*.9,span*1.1)),1700,span*1.1,(1.0,.70,.45)),
 (ctr+Vector((span*.9,span*.35,span*.65)),650,span,(.42,.58,1.0)),
 (ctr+Vector((0,span*.85,span)),800,span*.8,(.55,.68,1.0))]:
    bpy.ops.object.light_add(type="AREA",location=loc); L=bpy.context.object; L.data.energy=en; L.data.size=size; L.data.color=col; L.rotation_euler=(ctr-L.location).to_track_quat("-Z","Y").to_euler()

bpy.ops.object.camera_add(); cam=bpy.context.object; scene.camera=cam; cam.data.lens=58
scene.render.engine="BLENDER_EEVEE"; scene.render.resolution_x=1280; scene.render.resolution_y=900; scene.render.resolution_percentage=100; scene.render.image_settings.file_format="PNG"
views={"front":(0,-1.62,.28),"three-quarter":(.95,-1.35,.55),"side":(1.6,0,.35),"close":(.42,-1.08,.32)}
for name,v in views.items():
    cam.location=ctr+Vector(v)*span; cam.rotation_euler=(ctr-cam.location).to_track_quat("-Z","Y").to_euler(); scene.render.filepath=os.path.join(OUT,name+".png"); bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,"human-projection.blend"))
open(os.path.join(OUT,"report.json"),"w").write(json.dumps({"method":"tight canonical Gate projected into UVs of human-authored CC0 gatehouse after X/Z silhouette aspect conformance","mesh_count":len(meshes),"projected_polygons":projected,"fallback_polygons":fallback,"target_aspect":target_aspect,"bounds":list(dims),"paid_credits":0},indent=2))
