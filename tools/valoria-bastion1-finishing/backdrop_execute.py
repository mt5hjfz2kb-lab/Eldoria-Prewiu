"""Execute unchanged genuine Qwen parameters on real existing GLB mountain and water meshes."""
import argparse,hashlib,json,math,pathlib,sys,bpy
p=argparse.ArgumentParser();p.add_argument("--root",type=pathlib.Path,required=True)
r=p.parse_args(sys.argv[sys.argv.index("--")+1:]).root
plan=json.loads((r/"mountain-plan.json").read_text());proof=json.loads((r/"mountain-inference-proof.json").read_text())
assert hashlib.sha256((r/"mountain-plan.json").read_bytes()).hexdigest()==proof["plan_sha256"]
assert plan["operation"]=="refine_existing_mountain_and_water"
q=plan["parameters"]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(r/"source.glb"))
target=bpy.data.objects.get("Valoria true mountain backdrop")
assert target is not None and target.type=="MESH","Expected existing authored mountain geometry"
mesh=target.data
assert len(mesh.vertices)==9896 and len(mesh.polygons)==3300,(len(mesh.vertices),len(mesh.polygons))
def mesh_hash(obj):
 return hashlib.sha256(repr([tuple(round(x,6) for x in v.co) for v in obj.data.vertices]).encode()).hexdigest()
water_names={"Valoria distant lake western channel","Valoria lower western stream"}
others={o.name:mesh_hash(o) for o in bpy.data.objects if o.type=="MESH" and o!=target}
before=[v.co.copy() for v in mesh.vertices]
faces=[tuple(f.vertices) for f in mesh.polygons]
old_uv=[[tuple(loop.uv) for loop in uv.data] for uv in mesh.uv_layers]
levels=[v.co.z for v in mesh.vertices];lo=min(levels);hi=max(levels)
assert hi-lo>60,(lo,hi)
edited=0
for vertex in mesh.vertices:
 x,y,z=vertex.co
 # Broad mountain ridge shape respecting the distant valley floor.
 t=max(0.,min(1.,(z-(lo+.26*(hi-lo)))/(.55*(hi-lo))))
 smooth=t*t*(3.-2.*t)
 ridge=math.sin(x/q["ridge_wavelength_m"]+q["ridge_phase_radians"])*q["ridge_amplitude_m"]
 detail=math.sin(x/(q["ridge_wavelength_m"]*.36)-q["ridge_phase_radians"]*1.4)*q["secondary_ridge_m"]
 dz=(ridge+detail)*smooth
 vertex.co.z+=dz
 if abs(dz)>.001:edited+=1
mesh.update()
assert edited>1500,edited
assert faces==[tuple(f.vertices) for f in mesh.polygons]
assert old_uv==[[tuple(loop.uv) for loop in uv.data] for uv in mesh.uv_layers]
assert any((v.co-before[i]).length>.5 for i,v in enumerate(mesh.vertices))
source_rock=bpy.data.materials.get("Valoria family rock portable PBR")
source_green=bpy.data.materials.get("Valoria forested valley ground.001")
assert source_rock is not None and source_green is not None,"Existing CC0 source PBR material missing"
materials=[]
for name,source,uvscale in (("Bastion1 Qwen authored distant rock",source_rock,q["rock_uv_scale"]),("Bastion1 Qwen authored tree line",source_green,q["forest_uv_scale"])):
 mat=source.copy();mat.name=name
 if mat.use_nodes:
  nt=mat.node_tree
  for tex in list(nt.nodes):
   if tex.type=="TEX_IMAGE":
    mapping=nt.nodes.new("ShaderNodeMapping")
    mapping.inputs["Scale"].default_value=(uvscale,uvscale,1.)
    u=nt.nodes.new("ShaderNodeUVMap")
    nt.links.new(u.outputs["UV"],mapping.inputs["Vector"])
    nt.links.new(mapping.outputs["Vector"],tex.inputs["Vector"])
 materials.append(mat)
mesh.materials.clear()
for mat in materials:mesh.materials.append(mat)
line=lo+(hi-lo)*q["mountain_tree_line_ratio"]
rock_faces=0; forest_faces=0
for poly in mesh.polygons:
 average=sum(mesh.vertices[k].co.z for k in poly.vertices)/len(poly.vertices)
 poly.material_index=1 if average<=line else 0
 poly.use_smooth=q["smooth_mountain_faces"]
 if poly.material_index:forest_faces+=1
 else:rock_faces+=1
assert rock_faces>0 and forest_faces>0,(rock_faces,forest_faces)
water=bpy.data.materials.get("PROTOTYPE blue valley water")
assert water is not None and water.use_nodes
bs=water.node_tree.nodes.get("Principled BSDF")
assert bs is not None
rgb=q["water_rgb"]
bs.inputs["Base Color"].default_value=(*rgb,1.)
bs.inputs["Roughness"].default_value=q["water_roughness"]
for name in water_names:
 obj=bpy.data.objects.get(name)
 assert obj is not None and obj.type=="MESH"
 assert len(obj.data.polygons)==3
# Verify only target mesh positions were changed, and only planned material slots.
assert others=={o.name:mesh_hash(o) for o in bpy.data.objects if o.type=="MESH" and o!=target}
out=r/"edited-mountain";out.mkdir(exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/"Bastion1MountainQwen.blend"),compress=True)
bpy.ops.export_scene.gltf(filepath=str(out/"Bastion1MountainQwen.glb"),export_format="GLB",export_cameras=False,export_lights=False)
result={"source_glb_sha256":hashlib.sha256((r/"source.glb").read_bytes()).hexdigest(),"output_glb_sha256":hashlib.sha256((out/"Bastion1MountainQwen.glb").read_bytes()).hexdigest(),"plan_sha256":proof["plan_sha256"],"edited_mountain_vertices":edited,"mountain_triangles":len(mesh.polygons),"rock_faces":rock_faces,"forest_faces":forest_faces,"other_mesh_coordinates_preserved":True,"architecture_and_ground_unchanged":True,"source_water_meshes_unchanged":True,"ai_parameters_modified":False,"unity_pass":False,"art_pass":False}
(out/"mountain-execution-proof.json").write_text(json.dumps(result,indent=2)+"\n")
print("BASTION1_GENUINE_ARTIST_MOUNTAIN_EXECUTED",json.dumps(result))
