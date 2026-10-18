# Blender 3.x+ readonly inspection of original approved Valoria BridgeFamilyV1 source.
# Invoked via: blender -b -t 2 --python inspect-BridgeFamilyV1.py -- ORIGINAL.blend EVIDENCE_DIR
import bpy, sys, os, json, hashlib, math
from mathutils import Vector
argv=sys.argv[sys.argv.index("--")+1:]
src,out=argv[0],argv[1]
os.makedirs(out,exist_ok=True)
expected="801fd7d393c8c151c3036e793e62ff1aad88cd22b636598194b7c47e1ba2eed4"
sha=hashlib.sha256(open(src,"rb").read()).hexdigest()
if sha!=expected: raise RuntimeError("Immutable original Blender SHA changed: "+sha)
bpy.ops.wm.open_mainfile(filepath=os.path.abspath(src))
meshes=[o for o in bpy.data.objects if o.type=="MESH"]
if not meshes: raise RuntimeError("No source mesh components")
rows=[]
allworld=[]
for obj in meshes:
    vert_world=[obj.matrix_world @ v.co for v in obj.data.vertices]
    if not vert_world: continue
    allworld+=vert_world
    low=[min(v[i] for v in vert_world) for i in range(3)]
    high=[max(v[i] for v in vert_world) for i in range(3)]
    rows.append({
      "name":obj.name,"vertices":len(obj.data.vertices),"polygons":len(obj.data.polygons),
      "triangles":sum(len(p.vertices)-2 for p in obj.data.polygons),
      "mesh_bounds_world_min":low,"mesh_bounds_world_max":high,
      "dimensions_world":[high[i]-low[i] for i in range(3)],
      "origin_world":list(obj.matrix_world.translation),
      "uv_layers":[u.name for u in obj.data.uv_layers],
      "materials":[m.name if m else None for m in obj.data.materials],
      "hidden_render":obj.hide_render
    })
world_min=[min(v[i] for v in allworld) for i in range(3)]
world_max=[max(v[i] for v in allworld) for i in range(3)]
size=[world_max[i]-world_min[i] for i in range(3)]
center=Vector([(world_min[i]+world_max[i])*.5 for i in range(3)])
result={
 "source":"art-source/valoria/production/bridge-family-v1/BridgeFamilyV1.blend",
 "source_sha256":sha,"blender_version":bpy.app.version_string,
 "model_space":"Blender XYZ (Z up); source authored for historic rising grade 3.20->7.03m",
 "source_is_original_unchanged":True,
 "mesh_count":len(rows),"total_vertices":sum(x["vertices"] for x in rows),
 "total_triangles":sum(x["triangles"] for x in rows),
 "bounds_min":world_min,"bounds_max":world_max,"bounds_size":size,
 "objects":rows,"historical_original_bridge_grade":[3.20,7.03],
 "target_isolated_bridge_surface_y":0.38,
 "decision":"Do not transplant at arbitrary scale. Inspect and construct edited-source variant if grade/length differs."
}
with open(os.path.join(out,"BridgeFamilyV1-original-geometry.json"),"w",encoding="utf-8") as f:json.dump(result,f,ensure_ascii=False,indent=2)
# Clay images are source-only visualization; not Unity screenshot nor player-camera certification.
for x in meshes:
    x.hide_render=False
world=bpy.context.scene.world
if not world:
    world=bpy.data.worlds.new("InspectionWorld");bpy.context.scene.world=world
world.color=(.19,.19,.20)
scene=bpy.context.scene
scene.render.engine='BLENDER_WORKBENCH'
scene.render.resolution_x=1280;scene.render.resolution_y=720;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.render.film_transparent=False
scene.camera=bpy.data.objects.get("SourceInspectionCamera")
if not scene.camera:
    camdat=bpy.data.cameras.new("SourceInspectionCamera")
    cam=bpy.data.objects.new("SourceInspectionCamera",camdat)
    scene.collection.objects.link(cam);scene.camera=cam
scene.camera.data.type='ORTHO'
scene.camera.data.ortho_scale=max(size)*1.65
for label, offset in [
    ("threequarter",Vector((.70,-1.20,.66))),
    ("side",Vector((1.35,-.30,.30))),
    ("top",Vector((.02,-.03,1.5)))]:
    loc=center+offset.normalized()*max(size)*2.6
    scene.camera.location=loc
    direction=center-loc
    scene.camera.rotation_euler=direction.to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=os.path.join(out,"BridgeFamilyV1-original-"+label+".png")
    bpy.ops.render.render(write_still=True)
print("VALORIA_BRIDGE_ORIGINAL_INSPECTION_PASS meshes=%d tris=%d SHA=%s bounds=%s"%(len(rows),result["total_triangles"],sha,size))
