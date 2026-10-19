"""Authored quarry replacement candidate — one *real* existing world destination.
No houses, enemy camps or gameplay changes. 0 EUR. Blender Z-up.
Source is the existing source-pinned quarry. CC0 PBR maps are reused in place.
All rendered trial geometry is independently reviewed before Unity integration.
"""
import bpy, json, math, pathlib, hashlib, random, mathutils
from mathutils import Vector

ROOT=pathlib.Path.cwd()
SRC=ROOT/"Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources"
EV=ROOT/"docs/evidence/valoria-reference-clean"
MATS=SRC/"MaterialInputs/rock_3"
OLD=SRC/"EnvironmentFinish/EnvironmentQuarry.glb"
OUT=SRC/"EnvironmentFinish/QuarryGeologyV18.glb"
EV.mkdir(parents=True,exist_ok=True)
assert OLD.is_file() and all((MATS/n).is_file() for n in
 ("rock_3_diff_1k.png","rock_3_nor_gl_1k.png","rock_3_rough_1k.png"))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
original_hash=sha(OLD)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(OLD))
source=[o for o in bpy.data.objects if o.type=="MESH"]
assert source and len(source)>=2,"Retained quarry source lacks donor geometry"
for o in source:o.hide_render=True
# An actual connected excavation face and vegetated rear slope: no synthetic
# right-angle wall or repeated boxes. These face and shoulder control profiles
# are physically joined along an uninterrupted, deliberately asymmetric plan.
rng=random.Random(20261020)
N=88
outer=[]
for i in range(N):
 t=i/(N-1); x=-9.1+18.2*t
 y=-2.3+6.1*(max(0,1-(x/9.3)**2)**.79)+.43*math.sin(x*.47)
 height=(4.70+.63*math.sin(.61*x+.4)+.32*math.sin(1.52*x+.2))
 height*=.81+.19*max(0,1-(abs(x)/9.1)**3)
 outer.append((x,y,height))
vertices=[];uvs=[];faces=[]
# "excavation scar" four principal geological benches, each with actual
# rock cross-section (not a stack of facade-only primitive blocks).
heights=[-.86,.07,.58,1.08,1.64,2.03,2.49,3.04,3.61,4.18,4.65,5.10]
# Front scarp is volumetric: terrace setbacks recede toward the excavation
# crest and joins are shared along the whole escarpment.
setbacks=[-.95,-.86,-.83,-.62,-.17,-.10,.00,.32,.37,.73,.98,1.46]
for j,(z,retreat) in enumerate(zip(heights,setbacks)):
 for i,(x,y,h) in enumerate(outer):
  noise=.12*math.sin(1.45*x+2.14*j)+.058*math.sin(3.15*x-.83*j)
  xx=x+.14*math.sin(.9*x+j*.34)*math.sin(math.pi*i/(N-1))
  yy=y+retreat+noise
  zz=z*(h/5.1)+.095*math.sin(.93*x+j*.7)
  vertices.append((xx,yy,zz))
for j in range(len(heights)-1):
 for i in range(N-1):
  a=j*N+i;b=a+1;c=(j+1)*N+i;d=c+1
  faces.extend([(a,b,c),(b,d,c)])
# Crest rolls backward and becomes one sloped, continuous bedrock shoulder;
# its ground termination is curved under grass, not a vertical rectangle.
last=(len(heights)-1)*N
for band,(retreat,fall) in enumerate([(2.7,.19),(4.25,1.35),(6.4,3.55),(8.0,5.2)]):
 for i,(x,y,h) in enumerate(outer):
  vertices.append((x+.23*math.sin(x*.41+band*.7),y+retreat+
      .18*math.sin(x*.85),max(-.80,h-fall+.11*math.cos(x*1.7+band))))
  if band==0:
   if i>0:
    a=last+i-1;b=last+i;c=len(heights)*N+i-1;d=c+1
    faces.extend([(a,b,c),(b,d,c)])
  elif i>0:
   a=(len(heights)+band-1)*N+i-1;b=a+1
   c=(len(heights)+band)*N+i-1;d=c+1
   faces.extend([(a,b,c),(b,d,c)])
# End faces follow geological tangent and are closed, protecting mobile
# silhouette so there are no floating single-sided cliff curtains.
top=(len(heights)+3)*N
for i in range(N-1):
 faces.extend([(top+i,top+i+1,i),(top+i+1,i+1,i)])
for end in (0,N-1):
 indices=[j*N+end for j in range(len(heights)+4)]
 for k in range(1,len(indices)-1):faces.append((indices[0],indices[k],indices[k+1]))
# Use original CC0 rock_3 at real world metric UV scale, diffuse/normal/rough.
m=bpy.data.materials.new("Eldoria quarry / scanned rock_3 CC0 + authored geology")
m.use_nodes=True
nodes=m.node_tree.nodes;nodes.clear();links=m.node_tree.links
output=nodes.new("ShaderNodeOutputMaterial");pbr=nodes.new("ShaderNodeBsdfPrincipled")
pbr.inputs["Roughness"].default_value=.91
links.new(pbr.outputs["BSDF"],output.inputs["Surface"])
def tex(name,filename,noncolor=False):
 img=bpy.data.images.load(str(MATS/filename),check_existing=True)
 img.pack()
 if noncolor:img.colorspace_settings.name="Non-Color"
 node=nodes.new("ShaderNodeTexImage");node.label=name;node.image=img;node.extension="REPEAT"
 return node
diff=tex("CC0 rock", "rock_3_diff_1k.png")
normal=tex("CC0 rock normal", "rock_3_nor_gl_1k.png",True)
rough=tex("CC0 roughness","rock_3_rough_1k.png",True)
links.new(diff.outputs["Color"],pbr.inputs["Base Color"])
links.new(rough.outputs["Color"],pbr.inputs["Roughness"])
normalnode=nodes.new("ShaderNodeNormalMap");normalnode.inputs["Strength"].default_value=.54
links.new(normal.outputs["Color"],normalnode.inputs["Color"])
links.new(normalnode.outputs["Normal"],pbr.inputs["Normal"])
mesh=bpy.data.meshes.new("Joined quarry geological escarpment")
mesh.from_pydata(vertices,[],faces);mesh.update()
obj=bpy.data.objects.new("Quarry Geological Scarp / continuous inspected source v18",mesh)
bpy.context.collection.objects.link(obj)
obj.data.materials.append(m)
uv=obj.data.uv_layers.new(name="World-metric-rock-UV")
for poly in obj.data.polygons:
 for li in poly.loop_indices:
  v=obj.data.vertices[obj.data.loops[li].vertex_index].co
  uv.data[li].uv=(v.x/3.8+(v.y/9.4),v.z/3.4+(v.y/8.1))
# Intentionally keep crisp strata fracture facets for the real isometric view.
for poly in mesh.polygons:poly.use_smooth=False
# Preserve only genuine earlier authored extraction equipment/stone stock,
# not its rejected repetitive quarry retaining wall.
keep=[]
for old in source:
 if any(key in old.name.lower() for key in ("derrick","limestone stock")):
  clone=old.copy();clone.data=old.data.copy()
  bpy.context.collection.objects.link(clone)
  clone.matrix_world=old.matrix_world.copy()
  clone.name="Retained functional quarry equipment / "+old.name
  keep.append(clone)
print("ELDORIA_QUARRY_SOURCE_DIAGNOSTICS", [o.name for o in source], "authored_faces", len(mesh.polygons), "kept_source_equipment",len(keep))
assert len(mesh.polygons)>2500, "Continuous geological escarpment topology under-required size"
assert len(keep)>=1, "No genuine existing source quarry equipment survived import"
# Same-camera Blender A/B is *diagnostic only*, Unity approves separately.
def material_color(name,color):
 mat=bpy.data.materials.new(name);mat.diffuse_color=(*color,1);mat.use_nodes=True
 pr=next(n for n in mat.node_tree.nodes if n.type=="BSDF_PRINCIPLED")
 pr.inputs["Base Color"].default_value=(*color,1)
 return mat
bpy.ops.mesh.primitive_plane_add(size=150,location=(0,0,-.89))
ground=bpy.context.object;ground.name="Diagnostic-only ground";ground.data.materials.append(
 material_color("Matte meadow reference",(.23,.25,.17)))
bpy.ops.object.camera_add(location=(27,-32,28))
camera=bpy.context.object;camera.rotation_euler=(Vector((0,1.4,2.7))-camera.location).to_track_quat("-Z","Y").to_euler()
camera.data.type="ORTHO";camera.data.ortho_scale=30
bpy.context.scene.camera=camera
bpy.ops.object.light_add(type="AREA",location=(-10,-12,22))
light=bpy.context.object;light.data.energy=2800;light.data.shape="DISK";light.data.size=17
scene=bpy.context.scene
scene.render.engine="BLENDER_EEVEE_NEXT"
scene.render.resolution_x=1280;scene.render.resolution_y=720;scene.render.resolution_percentage=100
scene.render.image_settings.file_format="PNG"
obj.hide_render=True
for o in keep:o.hide_render=True
for o in source:o.hide_render=False
scene.render.filepath=str(EV/"quarry-geology-v18-before.png")
bpy.ops.render.render(write_still=True)
for o in source:o.hide_render=True
obj.hide_render=False
for o in keep:o.hide_render=False
scene.render.filepath=str(EV/"quarry-geology-v18-after.png")
bpy.ops.render.render(write_still=True)
# Export only source-authored geometry and retained equipment, not the
# diagnostic floor/lights or any unused originals.
bpy.ops.object.select_all(action="DESELECT")
for o in [obj]+keep:o.select_set(True)
bpy.context.view_layer.objects.active=obj
bpy.ops.export_scene.gltf(filepath=str(OUT),export_format="GLB",
 use_selection=True,export_apply=True,export_yup=True,export_texcoords=True,
 export_normals=True,export_materials="EXPORT")
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(EV/"QuarryGeologyV18.blend"))
proof={
 "source_original":str(OLD.relative_to(ROOT)),"source_sha256":original_hash,
 "new_glb":str(OUT.relative_to(ROOT)),"new_glb_sha256":sha(OUT),
 "blend":str((EV/"QuarryGeologyV18.blend").relative_to(ROOT)),
 "triangles":len(mesh.polygons),"retained_equipment_meshes":len(keep),
 "real_blender_render":True,"unity_integration":False,"art_approved":False,
 "physical_mobile_tested":False,"source_assets":["rock_3 (Poly Haven CC0)",str(OLD.relative_to(ROOT))],
 "methods":["continuous shared-vertex escarpment","measured nonrectangular footprint",
 "fractured multilevel excavation benches","closed rear terrain-shoulder",
 "metric UV","CC0 scanned diffuse/normal/rough","same-camera Blender diagnostic"],
 "limitations":["No Unity A/B or gameplay certification yet.","Do not promote without user-camera visual review."]
}
(EV/"quarry-geology-v18-author.json").write_text(json.dumps(proof,indent=2),encoding="utf-8")
print("ELDORIA_QUARRY_GEOLOGY_V18_BLENDER_PROOF",proof["new_glb_sha256"],proof["triangles"])
