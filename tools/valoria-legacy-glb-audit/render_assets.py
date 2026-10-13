#!/usr/bin/env python3
"""True imported GLB PBR and clay captures in Blender, NOT Unity approval."""
import sys,os,json,math,traceback,hashlib,datetime
from pathlib import Path
import bpy
from mathutils import Vector

argv=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
out=Path(argv[0] if argv else "audit-output")
manifest=json.loads((out/"audit-inputs.json").read_text())
screens=out/"renders"
screens.mkdir(parents=True,exist_ok=True)
result=[]

def setup_scene():
    scene=bpy.context.scene
    scene.render.engine="CYCLES"
    scene.cycles.device="CPU"
    scene.cycles.samples=5
    for layer in scene.view_layers:
        layer.cycles.use_denoising=False
    scene.render.resolution_x=576
    scene.render.resolution_y=480
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    scene.render.film_transparent=False
    scene.view_settings.view_transform="Standard"
    scene.view_settings.look="Medium High Contrast"
    scene.view_settings.exposure=0
    scene.view_settings.gamma=1
    world=bpy.data.worlds.new("Fixed studio world")
    scene.world=world
    world.use_nodes=True
    world.node_tree.nodes["Background"].inputs["Color"].default_value=(0.16,0.18,0.22,1)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value=.65
    sun=bpy.data.lights.new("Benchmark sunlight","SUN")
    sun.energy=2
    sun.angle=math.radians(7)
    s=bpy.data.objects.new("Benchmark sunlight",sun)
    scene.collection.objects.link(s)
    s.rotation_euler=(math.radians(35),math.radians(-18),math.radians(-30))
    return scene

def run(e):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene=setup_scene()
    bpy.ops.import_scene.gltf(filepath=str(Path(e["path"]).absolute()))
    objects=[o for o in scene.objects if o.type=="MESH"]
    if not objects: raise RuntimeError("No mesh after GLB import")
    corners=[o.matrix_world@Vector(v) for o in objects for v in o.bound_box]
    bmin=Vector([min(v[j] for v in corners) for j in range(3)])
    bmax=Vector([max(v[j] for v in corners) for j in range(3)])
    center=(bmin+bmax)*.5
    dimensions=bmax-bmin
    extent=max(dimensions.length,.3)
    camdat=bpy.data.cameras.new("Fixed angle comparison")
    cam=bpy.data.objects.new("Fixed angle comparison",camdat)
    scene.collection.objects.link(cam)
    direction=Vector((1.25,-1.6,1.15)).normalized()
    cam.location=center+direction*extent*2.6
    look=center-cam.location
    cam.rotation_euler=look.to_track_quat('-Z','Y').to_euler()
    camdat.type="ORTHO"
    camdat.ortho_scale=extent*1.50
    camdat.lens=50
    scene.camera=cam

    tris=sum(len(p.vertices)-2 for o in objects for p in o.data.polygons)
    mesh_count=len(objects)
    mats={m.name for o in objects for m in o.data.materials if m}
    images={nd.image.name for m in bpy.data.materials if m and m.use_nodes for nd in m.node_tree.nodes if nd.type=="TEX_IMAGE" and nd.image}
    total_verts=sum(len(o.data.vertices) for o in objects)
    missing_uv=sum(1 for o in objects if len(o.data.uv_layers)==0)
    nan_verts=sum(1 for o in objects for v in o.data.vertices if not all(math.isfinite(float(x)) for x in v.co))
    base=screens/e["id"]
    scene.render.filepath=str(base)+"-pbr.png"
    bpy.ops.render.render(write_still=True)

    clay=bpy.data.materials.new("Independent neutral-clay review")
    clay.diffuse_color=(.51,.49,.46,1)
    clay.use_nodes=True
    bs=clay.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(.51,.49,.46,1)
    bs.inputs["Roughness"].default_value=.75
    for o in objects:
        for slot in o.material_slots:
            slot.material=clay
        if not o.material_slots:
            o.data.materials.append(clay)
    scene.render.filepath=str(base)+"-clay.png"
    bpy.ops.render.render(write_still=True)

    return dict(id=e["id"],origin=e["origin"],sha256=e["sha256"],
      mesh_count=mesh_count,triangles=tris,vertices=total_verts,materials=len(mats),
      texture_images=len(images),mesh_without_uv=missing_uv,nonfinite_vertices=nan_verts,
      bounds_m=[round(float(v),3) for v in dimensions],camera_direction=[1.25,-1.6,1.15],
      framing="per-asset normalized extent, fixed direction, exposure, sun, resolution",
      pbr=str(base.name)+"-pbr.png",clay=str(base.name)+"-clay.png",status="RENDERED_UNITY_NOT_TESTED")

for i,e in enumerate(manifest["entries"],start=1):
    try:
        outcome=run(e)
        result.append(outcome)
        print("PASS",i,len(manifest["entries"]),e["id"],outcome["triangles"],flush=True)
    except Exception as exc:
        result.append(dict(id=e["id"],origin=e["origin"],sha256=e["sha256"],status="ERROR",error=str(exc),traceback=traceback.format_exc()[-1500:]))
        print("ERROR",e["id"],str(exc),flush=True)
    (out/"audit-result.json").write_text(json.dumps(dict(schema_version=1,engine="Blender Cycles CPU",unity_certified=False,images_reviewed=False,assets=result),indent=2))
bpy.ops.wm.read_factory_settings(use_empty=True)
print("FINISHED",sum(a["status"].startswith("RENDERED") for a in result),"of",len(result),flush=True)
