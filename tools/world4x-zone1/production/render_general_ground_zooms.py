"""Render matched REAL Blender A/B at three strategic scales; no Unity claim."""
import argparse
import hashlib
import json
import pathlib
import sys
import bpy

p=argparse.ArgumentParser()
p.add_argument("--before", type=pathlib.Path, required=True)
p.add_argument("--after", type=pathlib.Path, required=True)
p.add_argument("--out", type=pathlib.Path, required=True)
a=p.parse_args(sys.argv[sys.argv.index("--")+1:])
a.out.mkdir(parents=True,exist_ok=False)
bpy.ops.wm.open_mainfile(filepath=str(a.after.resolve()))
scene=bpy.context.scene
camera=scene.camera
assert camera and camera.data.type=="ORTHO", "Use actual artist's captured strategic camera"
edited=[o for o in bpy.data.objects if o.type=="MESH" and not o.name.startswith("_EldoriaReview")]
assert len(edited)==1,"Edited terrain must be one continuous source mesh"
edited=edited[0]
bpy.ops.import_scene.gltf(filepath=str(a.before.resolve()))
baseline=[o for o in bpy.data.objects if o.type=="MESH" and o!=edited and not o.name.startswith("_EldoriaReview")]
assert len(baseline)==1,"Baseline source must be one continuous source mesh"
baseline=baseline[0]
assert len(edited.data.vertices)==len(baseline.data.vertices),"Topology gate required before zoom review"
scene.render.engine="CYCLES"
scene.cycles.samples=12
scene.cycles.use_denoising=False
scene.render.resolution_percentage=100
scene.render.film_transparent=False
camera.data.type="ORTHO"
camera.data.sensor_fit="VERTICAL"
views=[("global",260,1000,800),("regional",140,1000,800),("local",70,1000,800),("mobile",140,390,844)]
records=[]
for name,scale,width,height in views:
 camera.data.ortho_scale=scale
 scene.render.resolution_x=width
 scene.render.resolution_y=height
 bpy.context.view_layer.update()
 matrix=[[float(v) for v in row] for row in camera.matrix_world]
 for label,visible in [("before",baseline),("after",edited)]:
  baseline.hide_render=(visible!=baseline)
  edited.hide_render=(visible!=edited)
  path=a.out/(label+"-"+name+"-blender.png")
  scene.render.filepath=str(path.resolve())
  bpy.ops.render.render(write_still=True)
  records.append({"view":name,"version":label,"source_sha256":hashlib.sha256(
    (a.before if label=="before" else a.after).read_bytes()).hexdigest(),
    "image_sha256":hashlib.sha256(path.read_bytes()).hexdigest(),
    "width":width,"height":height,"ortho_vertical_scale":scale,
    "camera_matrix":matrix,"filename":path.name})
for name,_,_,_ in views:
 pair=[x for x in records if x["view"]==name]
 assert len(pair)==2 and pair[0]["camera_matrix"]==pair[1]["camera_matrix"]
report={"kind":"MATCHED_REAL_BLENDER_ZOOM_REVIEW_NOT_UNITY",
 "same_ground_and_camera":True,"art_approved":False,"unity_tested":False,
 "physical_mobile_tested":False,"records":records}
(a.out/"zoom-review.json").write_text(json.dumps(report,indent=2)+"\n")
print("ZONEONE_TRUE_3ZOOM_CAPTURE",len(records),flush=True)
