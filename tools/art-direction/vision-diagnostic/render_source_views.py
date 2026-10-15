"""Read-only original-Valoria three-view vision benchmark. Never saves or exports a 3D asset."""
import argparse, hashlib, json, math, pathlib, sys
import bpy
from mathutils import Vector

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
parser=argparse.ArgumentParser()
parser.add_argument("--source", required=True)
parser.add_argument("--out", required=True)
args=parser.parse_args(argv)
source=pathlib.Path(args.source).resolve()
out=pathlib.Path(args.out).resolve()
out.mkdir(parents=True,exist_ok=True)
original_sha=hashlib.sha256(source.read_bytes()).hexdigest()
assert source.suffix.lower()==".glb" and source.read_bytes()[:4]==b"glTF"
# Frozen Valoria_Tower_CohV1.glb as independently documented in review 38694405904.
EXPECTED="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
if original_sha != EXPECTED: raise RuntimeError("SOURCE_SHA_MISMATCH: do not silently switch tower sources")
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(source))
meshes=[obj for obj in bpy.context.scene.objects if obj.type=="MESH" and len(obj.data.vertices)>0]
if not meshes: raise RuntimeError("ORIGINAL_HAS_NO_MESHES")
points=[obj.matrix_world @ Vector(corner) for obj in meshes for corner in obj.bound_box]
lo=[min(p[i] for p in points) for i in range(3)]
hi=[max(p[i] for p in points) for i in range(3)]
span=[hi[i]-lo[i] for i in range(3)]
center=Vector([(lo[i]+hi[i])/2 for i in range(3)])
# Non-original stage objects are preview-only, never exported or saved.
bpy.ops.mesh.primitive_plane_add(size=2, location=(center.x,center.y,lo[2]-0.055))
floor=bpy.context.object; floor.name="_EldoriaDiagnosticGround"
floor.scale=(max(span)*3,max(span)*3,1)
mat=bpy.data.materials.new("_EldoriaDiagnosticGround")
mat.diffuse_color=(0.32,0.33,0.35,1); floor.data.materials.append(mat)
world=bpy.data.worlds.new("DiagnosticNeutralWorld")
world.use_nodes=True;world.node_tree.nodes.get("Background").inputs["Color"].default_value=(0.45,0.48,0.53,1)
world.node_tree.nodes.get("Background").inputs["Strength"].default_value=0.65
scene=bpy.context.scene;scene.world=world
engine_ok=False
for engine in ("BLENDER_EEVEE_NEXT","BLENDER_EEVEE"):
    try: scene.render.engine=engine;engine_ok=True;break
    except TypeError: pass
if not engine_ok: raise RuntimeError("Cannot set EEVEE: no approved rendering fallback")
scene.render.resolution_x=768;scene.render.resolution_y=576;scene.render.resolution_percentage=100
scene.render.image_settings.file_format="PNG"
scene.render.film_transparent=False
scene.camera=None
for delta,power,size in [((1,-2,2.9),950,4),((-2,1,2.0),550,3)]:
    lightdata=bpy.data.lights.new("diagnostic_area","AREA")
    lightdata.energy=power;lightdata.shape="DISK";lightdata.size=size
    light=bpy.data.objects.new("diagnostic_area",lightdata);scene.collection.objects.link(light)
    light.location=center+Vector(delta)*max(span); direction=center-light.location
    light.rotation_euler=direction.to_track_quat("-Z","Y").to_euler()
camera_data=bpy.data.cameras.new("diagnostic_camera")
camera=bpy.data.objects.new("diagnostic_camera",camera_data);scene.collection.objects.link(camera)
scene.camera=camera;camera_data.type="ORTHO";camera_data.ortho_scale=max(span)*1.92
views={
 "front_three_quarter":(1.30,-1.60,0.75),
 "reverse_three_quarter":(-1.50,1.15,0.90),
 "elevated_roof":(1.05,0.80,2.00)
}
frames=[]
for name,vector in views.items():
    camera.location=center+Vector(vector)*max(span)*2.3
    camera.rotation_euler=(center-camera.location).to_track_quat("-Z","Y").to_euler()
    file=out/(name+".png")
    scene.render.filepath=str(file)
    bpy.ops.render.render(write_still=True)
    raw=file.read_bytes()
    if not raw.startswith(b"\x89PNG\r\n\x1a\n"): raise RuntimeError("RENDER_IS_NOT_REAL_PNG")
    frames.append({"view":name,"png":file.name,"sha256":hashlib.sha256(raw).hexdigest(),
                   "camera_world_location":[round(float(v),5) for v in camera.location],
                   "camera_rotation":[round(float(v),5) for v in camera.rotation_euler]})
if hashlib.sha256(source.read_bytes()).hexdigest()!=original_sha:
    raise RuntimeError("ORIGINAL_SOURCE_MUTATED")
report={"schema_version":1,"assessment":"READ_ONLY_OBSERVATION_NOT_ART_APPROVAL",
        "asset":"original Valoria_Tower_CohV1.glb","source_sha256":original_sha,
        "original_unchanged":True,"meshes":len(meshes),"world_xyz_z_up":True,
        "world_bounds_min":lo,"world_bounds_max":hi,"frames":frames}
(out/"vision-observation.json").write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8")
print("ELDORIA_READ_ONLY_REAL_MULTIVIEW_RENDER_PASS",original_sha, len(frames))
