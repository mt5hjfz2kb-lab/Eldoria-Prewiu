import bpy, os, math, json
from mathutils import Vector
from bpy_extras.object_utils import world_to_camera_view
from PIL import Image

ROOT=os.getcwd()
OUT="/tmp/valoria-geometry-projection-bake"
os.makedirs(OUT,exist_ok=True)

# Canonical gate appearance from semantic extraction.
ev="docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1"
back=Image.open(os.path.join(ev,"gate-back.png")).convert("RGBA")
front=Image.open(os.path.join(ev,"gate-front.png")).convert("RGBA")
target=Image.alpha_composite(back,front)
# Replace transparent regions with a stone tone sampled from the Gate itself so
# geometry that extends beyond the semantic cutout never turns black.
import numpy as np
ta=np.asarray(target).astype(np.float32)
mask=ta[...,3]>24
rgb=ta[...,:3]
valid=rgb[mask]
median=np.median(valid,axis=0) if len(valid) else np.array([95,88,78],dtype=np.float32)
filled=np.zeros_like(rgb); filled[:]=median
a=(ta[...,3:4]/255.0)
filled=rgb*a+filled*(1.0-a)
target_rgb=Image.fromarray(np.clip(filled,0,255).astype(np.uint8),"RGB")
target_path=os.path.join(OUT,"target-gate.png"); target_rgb.save(target_path)

bpy.ops.wm.read_factory_settings(use_empty=True)

def import_fbx(path,prefix):
    before=set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=path)
    objs=[o for o in bpy.data.objects if o not in before and o.type=="MESH"]
    for i,o in enumerate(objs): o.name=f"{prefix}_{i:02d}"
    if not objs: raise RuntimeError("No mesh from "+path)
    return objs

def bounds(objs):
    pts=[o.matrix_world@Vector(c) for o in objs for c in o.bound_box]
    mn=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
    mx=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
    return mn,mx,(mn+mx)*.5,mx-mn

def normalize_group(objs,desired_height,center):
    mn,mx,ctr,dims=bounds(objs)
    scale=desired_height/max(dims.z,1e-6)
    for o in objs:
        o.scale*=scale
    bpy.context.view_layer.update()
    mn,mx,ctr,dims=bounds(objs)
    delta=Vector(center)-ctr
    for o in objs: o.location+=delta
    bpy.context.view_layer.update()
    return dims*scale

gate_path=os.path.join(ROOT,"Unity/Assets/Bublik/Simple Modular Castle Assets/Meshes/Stone_Gate.fbx")
tower_path=os.path.join(ROOT,"Unity/Assets/Bublik/Simple Modular Castle Assets/Meshes/Stone_Tower.fbx")
gate=import_fbx(gate_path,"Gate")
normalize_group(gate,4.8,(0,0,2.4))
towerL=import_fbx(tower_path,"TowerL"); normalize_group(towerL,6.8,(-4.1,0.4,3.4))
towerR=import_fbx(tower_path,"TowerR"); normalize_group(towerR,6.8,(4.1,0.4,3.4))
all_meshes=gate+towerL+towerR

# Slight depth staggering for silhouette and real parallax.
for o in gate: o.location.y=-0.15
for o in towerL+towerR: o.location.y=0.35
bpy.context.view_layer.update()

mn,mx,ctr,dims=bounds(all_meshes)
span=max(dims.x,dims.z)

# Projector camera matched to assembled hero gate.
bpy.ops.object.camera_add(location=(ctr.x,ctr.y-span*2.4,ctr.z))
proj=bpy.context.object; proj.name="CanonicalProjector"; proj.data.type="ORTHO"; proj.data.ortho_scale=max(dims.z,dims.x*target.height/target.width)*1.05
proj.rotation_euler=(ctr-proj.location).to_track_quat("-Z","Y").to_euler()
scene=bpy.context.scene
scene.camera=proj
bpy.context.view_layer.update()

# Materials.
img=bpy.data.images.load(target_path,check_existing=True)
mat_front=bpy.data.materials.new("CanonicalProjection"); mat_front.use_nodes=True
n=mat_front.node_tree.nodes; l=mat_front.node_tree.links
for x in list(n): n.remove(x)
out=n.new("ShaderNodeOutputMaterial"); bs=n.new("ShaderNodeBsdfPrincipled"); tex=n.new("ShaderNodeTexImage"); tex.image=img; tex.extension="EXTEND"
uvn=n.new("ShaderNodeUVMap"); uvn.uv_map="CanonicalProjectionUV"; l.new(uvn.outputs["UV"],tex.inputs["Vector"])
l.new(tex.outputs["Color"],bs.inputs["Base Color"]); bs.inputs["Roughness"].default_value=.72
bump=n.new("ShaderNodeBump"); bump.inputs["Strength"].default_value=.18; bump.inputs["Distance"].default_value=.055
bw=n.new("ShaderNodeRGBToBW"); l.new(tex.outputs["Color"],bw.inputs["Color"]); l.new(bw.outputs["Val"],bump.inputs["Height"]); l.new(bump.outputs["Normal"],bs.inputs["Normal"])
l.new(bs.outputs["BSDF"],out.inputs["Surface"])

mat_side=bpy.data.materials.new("ValoriaSideStone"); mat_side.use_nodes=True
ns=mat_side.node_tree.nodes; ls=mat_side.node_tree.links
for x in list(ns): ns.remove(x)
outs=ns.new("ShaderNodeOutputMaterial"); bss=ns.new("ShaderNodeBsdfPrincipled")
bss.inputs["Base Color"].default_value=(.19,.17,.145,1); bss.inputs["Roughness"].default_value=.82
noise=ns.new("ShaderNodeTexNoise"); noise.inputs["Scale"].default_value=6; noise.inputs["Detail"].default_value=5; noise.inputs["Roughness"].default_value=.7
coord=ns.new("ShaderNodeTexCoord"); bump2=ns.new("ShaderNodeBump"); bump2.inputs["Strength"].default_value=.28; bump2.inputs["Distance"].default_value=.08
ls.new(coord.outputs["Generated"],noise.inputs["Vector"]); ls.new(noise.outputs["Fac"],bump2.inputs["Height"]); ls.new(bump2.outputs["Normal"],bss.inputs["Normal"]); ls.new(bss.outputs["BSDF"],outs.inputs["Surface"])

# Camera-project UVs and assign projection only to surfaces facing projector.
for o in all_meshes:
    me=o.data
    me.materials.clear(); me.materials.append(mat_front); me.materials.append(mat_side)
    uv=me.uv_layers.get("CanonicalProjectionUV") or me.uv_layers.new(name="CanonicalProjectionUV")
    # polygons facing projector: transform normal to world and test against camera direction.
    cam_dir=(ctr-proj.location).normalized()
    for poly in me.polygons:
        wn=(o.matrix_world.to_3x3()@poly.normal).normalized()
        # visible/front if normal points substantially toward projector.
        # Projection is appropriate on facade/back planes regardless of imported
        # winding direction. Tangential side faces keep true fallback stone.
        facing=abs(wn.dot(cam_dir))
        poly.material_index=0 if facing>0.34 else 1
        for li in poly.loop_indices:
            vi=me.loops[li].vertex_index
            co=o.matrix_world@me.vertices[vi].co
            ndc=world_to_camera_view(scene,proj,co)
            uv.data[li].uv=(ndc.x,ndc.y)
    bev=o.modifiers.new("EdgeFinish","BEVEL"); bev.width=max(.025,span*.003); bev.segments=2; bev.limit_method="ANGLE"; bev.angle_limit=math.radians(28)

# Lighting/world.
if scene.world is None: scene.world=bpy.data.worlds.new("World")
scene.world.use_nodes=True; bg=scene.world.node_tree.nodes.get("Background"); bg.inputs["Color"].default_value=(.022,.03,.045,1); bg.inputs["Strength"].default_value=.35
for loc,en,size,col in [
    (ctr+Vector((-span*.7,-span*.9,span*1.1)),1500,span,(1.0,.72,.48)),
    (ctr+Vector((span*.9,span*.35,span*.6)),650,span,(.42,.58,1.0)),
    (ctr+Vector((0,span*.9,span*1.0)),800,span*.8,(.55,.68,1.0))]:
    bpy.ops.object.light_add(type="AREA",location=loc); L=bpy.context.object; L.data.energy=en; L.data.size=size; L.data.color=col; L.rotation_euler=(ctr-L.location).to_track_quat("-Z","Y").to_euler()
# Ground.
bpy.ops.mesh.primitive_plane_add(size=span*4,location=(ctr.x,ctr.y+1,mn.z-.05))
gm=bpy.data.materials.new("Ground"); gm.diffuse_color=(.065,.07,.06,1); bpy.context.object.data.materials.append(gm)

# Render cameras.
bpy.ops.object.camera_add(); cam=bpy.context.object; scene.camera=cam; cam.data.lens=60
scene.render.engine="BLENDER_EEVEE"; scene.render.resolution_x=1280; scene.render.resolution_y=900; scene.render.resolution_percentage=100; scene.render.image_settings.file_format="PNG"
views={"front":(0,-1.7,.25),"three-quarter":(1.05,-1.45,.55),"side":(1.7,-.05,.35),"close":(.45,-1.2,.32)}
for name,v in views.items():
    cam.location=ctr+Vector(v)*span; cam.rotation_euler=(ctr-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.render.filepath=os.path.join(OUT,name+".png"); bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,"geometry-projection-bake.blend"))
report={"method":"canonical image projected into UVs of real human-authored modular 3D; projection only on projector-facing faces; non-facing faces remain real fallback stone","mesh_count":len(all_meshes),"bounds":list(dims),"paid_credits":0}
open(os.path.join(OUT,"report.json"),"w").write(json.dumps(report,indent=2))
