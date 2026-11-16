#!/usr/bin/env python3
"""True temporary Blender import of publisher .gltf geometry, visual A/B only.
Flattened PNG + audit metadata are the ONLY deliverables from this workflow.
No raw external meshes, modified meshes, source BLEND or original textures enter
the public repository or public artifacts. This is NOT the Unity sandbox.
"""
import bpy, json, math, os, hashlib
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
TMP=Path(os.environ.get("ELDORIA_VENDOR_REVIEW", str(ROOT/"_vendor_review_temp")))
OUT=ROOT/"_vendor_render_output"
OUT.mkdir(parents=True,exist_ok=True)
targets=[
 ("Barracks_SecondAge_Level3.gltf","CUARTEL · II",( -16.0,  8.7,0),8.2),
 ("TownCenter_SecondAge_Level3.gltf","CENTRO · II",(0.0, 8.7,0),8.2),
 ("Storage_FirstAge_Level1.gltf","GRANERO · I",(16.0, 8.7,0),8.2),
 ("Mine.gltf","CANTERA · I",(-16.0,-8.7,0),8.2),
 ("Resource_PineTree_Group.gltf","BOSQUE",(0.0,-8.7,0),8.2),
 ("WallTowers_Door_FirstAge.gltf","PUERTA · I",(16.0,-8.7,0),8.2)
]
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene
scene.render.engine="CYCLES"
scene.cycles.samples=20
scene.cycles.use_denoising=False
for l in scene.view_layers: l.cycles.use_denoising=False
scene.render.threads_mode="FIXED";scene.render.threads=4
scene.render.resolution_x=1660;scene.render.resolution_y=960
scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.view_settings.view_transform="Filmic"
scene.view_settings.look="Medium High Contrast"
def mat(name,c,rough=.8,metal=0):
    m=bpy.data.materials.new("AUDIT_"+name)
    m.diffuse_color=(*c,1)
    m.use_nodes=True
    p=m.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value=(*c,1)
    p.inputs["Roughness"].default_value=rough
    p.inputs["Metallic"].default_value=metal
    return m
materials={
 "stone":mat("stone",(.46,.49,.50)),
 "stone_light":mat("limestone",(.67,.67,.60)),
 "stone_dark":mat("slate",(.29,.34,.39)),
 "wood":mat("oak",(.40,.31,.25)),
 "moss":mat("moss",(.32,.42,.33)),
 "gold":mat("bronze",(.69,.50,.30),.52,.35),
 "soil":mat("soil",(.26,.32,.30)),
 "platform":mat("tile",(.22,.26,.29))
}
def block(name,pos,scale,material):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos)
    o=bpy.context.object
    o.name=name;o.dimensions=scale
    o.data.materials.append(material)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return o
def bounds(objects):
    xyz=[]
    for o in objects:
        if o.type=='MESH':
            for v in o.bound_box:xyz.append(o.matrix_world@Vector(v))
    if not xyz:raise RuntimeError("imported model did not produce mesh objects")
    return Vector([min(v[i] for v in xyz) for i in range(3)]),Vector([max(v[i] for v in xyz) for i in range(3)])
scene.world.use_nodes=True
scene.world.node_tree.nodes["Background"].inputs["Color"].default_value=(.18,.22,.29,1)
scene.world.node_tree.nodes["Background"].inputs["Strength"].default_value=.72
block("shared landscape slab",(0,0,-.85),(51.0,33.3,1.4),materials["soil"])
loaded=[]
for fname,label,pos,maxwidth in targets:
    imported_before={o.as_pointer() for o in bpy.data.objects}
    bpy.ops.import_scene.gltf(filepath=str(TMP/fname),import_pack_images=True)
    imported=[o for o in bpy.data.objects if o.as_pointer() not in imported_before]
    meshes=[o for o in imported if o.type=='MESH']
    mn,mx=bounds(meshes)
    lengths=mx-mn
    max_h=max(lengths.x,lengths.y,lengths.z)
    if max_h<.0001:raise RuntimeError("empty model "+fname)
    factor=min(maxwidth/max(lengths.x,lengths.y),6.3/max(lengths.z,.001))
    root=bpy.data.objects.new("TEMP Quaternius published mesh "+fname,None)
    scene.collection.objects.link(root)
    parents=[o for o in imported if o.parent is None]
    for o in parents:
        mw=o.matrix_world.copy()
        o.parent=root
        o.matrix_world=mw
    center=(mn+mx)/2
    root.scale=(factor,)*3
    root.location=(pos[0]-center.x*factor,pos[1]-center.y*factor,.06-mn.z*factor)
    block("grounded pedestal "+fname,(pos[0],pos[1],-.03),(13.3,12.7,.17),materials["platform"])
    bpy.ops.object.text_add(location=(pos[0]-4.9,pos[1]-4.8,.32))
    labelobj=bpy.context.object
    labelobj.name="caption "+label
    labelobj.data.body=label
    labelobj.data.size=.79
    labelobj.data.extrude=.004
    labelobj.data.materials.append(materials["gold"])
    loaded.append({"asset":fname,"blender_meshes":len(meshes),
                   "pre_scale_bbox_xyz":[round(v,5) for v in lengths],
                   "visual_scale":round(factor,5)})
bpy.ops.object.light_add(type="SUN",location=(0,0,20))
sun=bpy.context.object
sun.rotation_euler=(math.radians(69), math.radians(5), math.radians(-38))
sun.data.energy=2.45;sun.data.color=(1.0,.75,.55);sun.data.angle=math.radians(3.0)
bpy.ops.object.camera_add(location=(33,-42,36))
cam=bpy.context.object
cam.rotation_euler=(Vector((0,0,1.7))-cam.location).to_track_quat('-Z','Y').to_euler()
cam.data.type="ORTHO";cam.data.ortho_scale=55
scene.camera=cam
scene.render.filepath=str(OUT/"Quaternius_Source_2022_ActualModels.png")
bpy.ops.render.render(write_still=True)
p=OUT/"Quaternius_Source_2022_ActualModels.png"
assert p.stat().st_size>100000,p.stat().st_size
# A preview of imported surfaces only. This is purposely NOT claimed as in-game art.
report=json.loads((TMP/"quaternius-source-review.json").read_text())
report["render"]="Quaternius_Source_2022_ActualModels.png"
report["blender_native_import_pass"]=True
report["unity_native_import_pass"]=False
report["art_approved"]=False
report["license_certified"]=False
report["models"]=loaded
(OUT/"Quaternius_ActualModels_Audit.json").write_text(json.dumps(report,indent=2)+"\n")
print("QUATERNIUS_REAL_BLENDER_MODELS_PASS",len(loaded),p.stat().st_size)
