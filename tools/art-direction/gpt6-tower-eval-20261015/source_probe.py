"""Read-only source probe. The frozen tower GLB is NEVER changed."""
import base64,hashlib,json,pathlib,sys
import bpy
from mathutils import Vector
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]/"full-studio"))
from blender_toolbox import render_setup, camera_override, ensure_review_lighting, ensure_review_stage

argv=sys.argv[sys.argv.index("--")+1:]
src=pathlib.Path(argv[0]);out=pathlib.Path(argv[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869","FROZEN_SHA_MISMATCH"
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(src.resolve()))
items=[];all_positions=[]
for o in bpy.data.objects:
 if o.type!="MESH":continue
 coords=[o.matrix_world @ v.co for v in o.data.vertices]
 all_positions.extend(coords)
 bounds=[[min(float(v[i]) for v in coords),max(float(v[i]) for v in coords)] for i in range(3)]
 zs=sorted(float(v.z) for v in coords)
 bands=[]
 for k in range(7):
  lo=zs[min(len(zs)-1,k*len(zs)//7)]
  hi=zs[min(len(zs)-1,(k+1)*len(zs)//7-1)]
  subset=[v for v in coords if lo<=v.z<=hi]
  bands.append({"height_min_m":round(lo,3),"height_max_m":round(hi,3),"x_range":[round(min(v.x for v in subset),3),round(max(v.x for v in subset),3)],"y_range":[round(min(v.y for v in subset),3),round(max(v.y for v in subset),3)]})
 items.append({"name":o.name,"vertices":len(o.data.vertices),"faces":len(o.data.polygons),"triangles":sum(max(0,len(p.vertices)-2) for p in o.data.polygons),"world_xyz_ranges_m":[[round(x,4) for x in q] for q in bounds],"height_bands":bands,"materials":[m.name if m else None for m in o.data.materials]})
proof={"source_sha256":hashlib.sha256(src.read_bytes()).hexdigest(),"meshes":items,"render_camera":"same-as-fixed-review","scope":"READ_ONLY"}
(out/"source-inspection.json").write_text(json.dumps(proof,indent=2)+"\n",encoding="utf-8")
print("ELDORIA_SOURCE_INSPECTION="+json.dumps(proof,ensure_ascii=True))
render_setup({"samples":8,"width":640,"height":480})
ensure_review_lighting();ensure_review_stage()
views={"hero":{"location":[6,-9,6],"target":[0,0,0.5],"ortho_scale":6},
"reverse":{"location":[-7,8,6],"target":[0,0,0.5],"ortho_scale":6},
"top":{"location":[0,0,12],"target":[0,0,0],"ortho_scale":5.5}}
for name,spec in views.items():
 camera_override(spec)
 scene=bpy.context.scene
 scene.render.image_settings.file_format="JPEG"
 scene.render.image_settings.quality=43
 scene.render.filepath=str((out/("original-"+name+".jpg")).resolve())
 bpy.ops.render.render(write_still=True)
 b64=base64.b64encode((out/("original-"+name+".jpg")).read_bytes()).decode("ascii")
 print("ELDORIA_ACTUAL_TOWER_FRAME_"+name.upper()+"="+b64)
print("ELDORIA_SOURCE_PROBE_SUCCESS")
