"""Valoria V19: shore-cliff source from measured live-terrain axes + already
owned Holotna rock geometry and licensed Poly Haven rock_3 PBR. Blender Z-up.
SOURCE CANDIDATE, never ART_APPROVED without actual Unity camera comparison.
"""
import bpy,math,json,hashlib,os,pathlib
from mathutils import Vector,Matrix
R=pathlib.Path.cwd()
SRC=R/"Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources"
E=R/"docs/evidence/valoria-reference-clean"
T=SRC/"MaterialInputs/rock_3"
W=R/"Unity/Assets/Eldoria/ArtTests/World4XZone1/Models/ZoneOneSectorCandidate.glb"
inputs=[W,T/"rock_3_diff_1k.png",T/"rock_3_nor_gl_1k.png",T/"rock_3_rough_1k.png",T/"license-manifest.json"]
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
assert all(p.is_file() for p in inputs) and "CC0" in inputs[4].read_text().upper()
originals={str(p.relative_to(R)):digest(p) for p in inputs}
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(W))
root=bpy.data.objects.get("Existing Holotna rock.001")
assert root,"Exact source-world rock is absent"
parts=[o for o in [root,*root.children_recursive] if o.type=="MESH" and len(o.data.vertices)>20]
assert parts
donor=parts[0].data.copy()
donor.transform(parts[0].matrix_world)
pts=[v.co for v in donor.vertices]
low=Vector([min(p[i] for p in pts) for i in range(3)])
high=Vector([max(p[i] for p in pts) for i in range(3)])
extent=max(high[i]-low[i] for i in range(3))
assert .2<extent<10000
donor.transform(Matrix.Scale(1/extent,4)@Matrix.Translation(-Vector(((high.x+low.x)/2,(high.y+low.y)/2,low.z))))
for o in list(bpy.data.objects):bpy.data.objects.remove(o,do_unlink=True)
def material():
    m=bpy.data.materials.new("Eldoria river escarpment / Poly Haven CC0 rock_3");m.use_nodes=True
    nodes=m.node_tree.nodes;links=m.node_tree.links;p=nodes.get("Principled BSDF")
    diffuse=nodes.new("ShaderNodeTexImage");diffuse.image=bpy.data.images.load(str(inputs[1]))
    color=nodes.new("ShaderNodeMixRGB");color.blend_type="MULTIPLY";color.inputs[0].default_value=1
    color.inputs[2].default_value=(.76,.74,.70,1)
    links.new(diffuse.outputs["Color"],color.inputs[1]);links.new(color.outputs[0],p.inputs["Base Color"])
    n=nodes.new("ShaderNodeTexImage");n.image=bpy.data.images.load(str(inputs[2]))
    n.image.colorspace_settings.name="Non-Color"
    normal=nodes.new("ShaderNodeNormalMap");normal.inputs["Strength"].default_value=.52
    links.new(n.outputs["Color"],normal.inputs["Color"]);links.new(normal.outputs["Normal"],p.inputs["Normal"])
    rough=nodes.new("ShaderNodeTexImage");rough.image=bpy.data.images.load(str(inputs[3]))
    rough.image.colorspace_settings.name="Non-Color";links.new(rough.outputs["Color"],p.inputs["Roughness"])
    return m
rockmat=material()
donor.materials.clear();donor.materials.append(rockmat)
layer=donor.uv_layers.get("UVMap") or donor.uv_layers.new(name="UVMap")
for face in donor.polygons:
 for li in face.loop_indices:
    p=donor.vertices[donor.loops[li].vertex_index].co
    layer.data[li].uv=(p.x*.85+p.z*.26,p.y*.85+p.z*.18)
def river(x):
  return -23+1.5*math.sin(x*.065)+.85*math.sin(x*.19+.4)*max(0,min(1,(abs(x)-6)/11))
def sm(x):
  v=max(0,min(1,x));return v*v*(3-2*v)
pieces=[]
bands=[4.65,5.35,6.5,7.65,8.5,9.6,11.1,12.4]
ht=[-3.25,-3.0,-2.58,-1.57,-.64,.38,.18,-.40]
for west,(start,end) in enumerate([(-79.,-23.),(23.,79.)]):
 for side in [-1,1]:
    verts=[];faces=[];ns=len(bands);sections=48
    for j in range(sections):
      x=start+(end-start)*j/(sections-1)
      taper=sm((x-start)/5)*sm((end-x)/5)
      for k,off in enumerate(bands):
        layer_break=.32*math.sin(x*.47+k*1.11)+.19*math.sin(x*1.03-k*.32)
        y=river(x)+side*(off+(.33*math.sin(x*.57+k*.7) if k not in (0,7) else 0))
        z=ht[k]+(layer_break*(.78 if k in (2,3,4,5) else .15))*taper
        if k in (2,3,4,5):z-=.40*(1-taper)
        verts.append((x,y,z))
    for j in range(sections-1):
      for k in range(ns-1):
        a=j*ns+k;b=(j+1)*ns+k
        faces.extend([(a,b,a+1),(b,b+1,a+1)] if side==1 else [(a,a+1,b),(b,a+1,b+1)])
    mesh=bpy.data.meshes.new("Rock/terrain contact measured on river")
    mesh.from_pydata(verts,[],faces);mesh.update()
    mesh.materials.append(rockmat)
    layer=mesh.uv_layers.new(name="UVMap")
    for face in mesh.polygons:
      for li in face.loop_indices:
       p=mesh.vertices[mesh.loops[li].vertex_index].co
       layer.data[li].uv=(p.x*.13+p.y*.03,p.z*.23+p.y*.12)
    o=bpy.data.objects.new("Eldoria authored riverside geology / section %d side %d"%(west,side),mesh)
    bpy.context.collection.objects.link(o);pieces.append(o)
    # Original rock vertices are reused for genuinely irregular large outcrops;
    # they interpenetrate the continuous sculpted bank, never stand as pebbles.
    for j in range(5):
      x=start+6+(end-start-12)*j/4
      o=bpy.data.objects.new("Source Holotna rock embedded / %d %d %d"%(west,side,j),donor.copy())
      bpy.context.collection.objects.link(o)
      o.location=(x,river(x)+side*(7.8+.33*math.sin(j*2)), -1.36)
      o.scale=(3.0+.7*math.sin(j*2),2.3+.3*math.cos(j),2.9+.4*math.sin(j))
      o.rotation_euler=(.07*math.sin(j),.17*math.cos(j),j*1.77)
      pieces.append(o)
assert len(pieces)==24
bpy.ops.object.select_all(action="DESELECT")
for o in pieces:o.select_set(True)
bpy.context.view_layer.objects.active=pieces[0]
output=SRC/"OwnerCityCliffsV19.glb"
bpy.ops.export_scene.gltf(filepath=str(output),export_format="GLB",use_selection=True,
  export_apply=True,export_yup=True,export_normals=True,export_texcoords=True,export_materials="EXPORT")
blend=E/"OwnerCityCliffsV19.blend"
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(blend))
assert all(digest(R/k)==v for k,v in originals.items())
report={"stage":"BLENDER_SOURCE_ONLY","source_sha":os.environ.get("GITHUB_SHA"),
 "input_hashes":originals,"output_path":str(output.relative_to(R)),"output_sha256":digest(output),
 "blend_path":str(blend.relative_to(R)),"blend_sha256":digest(blend),
 "mesh_count":len(pieces),"triangles":sum(sum(len(p.vertices)-2 for p in ob.data.polygons) for ob in pieces),
 "geometry":"continuous four coastal formations over authentic river curve, donor rock massing",
 "protected":"all Bastion, bridge, lower-gate and plot region x[-20,20]; no collision/new gameplay",
 "art_approved":False,"unity_AB_pending":True,"cost_eur":0}
(E/"owner-city-cliffs-v19-source.json").write_text(json.dumps(report,indent=2),encoding="utf-8")
print("ELDORIA_OWNER_CITY_CLIFFS_V19_SOURCE_PASS",report["triangles"],report["output_sha256"])
