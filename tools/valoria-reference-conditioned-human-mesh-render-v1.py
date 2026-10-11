import bpy, os, math, json
from mathutils import Vector
ROOT="/tmp/valoria-hybrid-proof"; EV=os.path.join(ROOT,"evidence")
blend=open(os.path.join(ROOT,"blend-path.txt")).read().strip()
bpy.ops.wm.open_mainfile(filepath=blend)
meshes=[o for o in bpy.context.scene.objects if o.type=="MESH"]
if not meshes: raise RuntimeError("No meshes")

stone=bpy.data.images.load(os.path.join(ROOT,"source","valoria-stone.jpg"),check_existing=True)
def make_mat(name,dark=False,wood=False):
    m=bpy.data.materials.new(name); m.use_nodes=True
    n=m.node_tree.nodes; l=m.node_tree.links
    for x in list(n): n.remove(x)
    out=n.new("ShaderNodeOutputMaterial"); bs=n.new("ShaderNodeBsdfPrincipled")
    tex=n.new("ShaderNodeTexImage"); tex.image=stone; tex.projection="BOX"; tex.projection_blend=.28
    coord=n.new("ShaderNodeTexCoord"); mapping=n.new("ShaderNodeMapping")
    l.new(coord.outputs["Generated"],mapping.inputs["Vector"]); l.new(mapping.outputs["Vector"],tex.inputs["Vector"])
    mapping.inputs["Scale"].default_value=(4.5,4.5,4.5)
    if dark:
        ramp=n.new("ShaderNodeValToRGB")
        ramp.color_ramp.elements[0].color=(.025,.035,.055,1); ramp.color_ramp.elements[1].color=(.16,.14,.13,1)
        l.new(tex.outputs["Color"],ramp.inputs["Fac"]); l.new(ramp.outputs["Color"],bs.inputs["Base Color"])
    elif wood:
        ramp=n.new("ShaderNodeValToRGB")
        ramp.color_ramp.elements[0].color=(.06,.025,.012,1); ramp.color_ramp.elements[1].color=(.32,.15,.055,1)
        l.new(tex.outputs["Color"],ramp.inputs["Fac"]); l.new(ramp.outputs["Color"],bs.inputs["Base Color"])
    else:
        l.new(tex.outputs["Color"],bs.inputs["Base Color"])
    bump=n.new("ShaderNodeBump"); bump.inputs["Strength"].default_value=.22; bump.inputs["Distance"].default_value=.08
    rgb=n.new("ShaderNodeRGBToBW"); l.new(tex.outputs["Color"],rgb.inputs["Color"]); l.new(rgb.outputs["Val"],bump.inputs["Height"]); l.new(bump.outputs["Normal"],bs.inputs["Normal"])
    bs.inputs["Roughness"].default_value=.76 if not dark else .58
    bs.inputs["Metallic"].default_value=0
    l.new(bs.outputs["BSDF"],out.inputs["Surface"])
    return m
stone_mat=make_mat("Valoria_Stone"); dark_mat=make_mat("Valoria_Dark",dark=True); wood_mat=make_mat("Valoria_Wood",wood=True)

for o in meshes:
    # choose by original material/object semantic names where possible
    sem=(" ".join([o.name]+[m.name for m in o.data.materials if m])).lower()
    mat=dark_mat if any(k in sem for k in ("roof","slate","black","metal")) else wood_mat if any(k in sem for k in ("wood","door","beam")) else stone_mat
    o.data.materials.clear(); o.data.materials.append(mat)
    # small real geometric edge treatment; no silhouette redesign
    dims=o.dimensions
    if max(dims)>0.25:
        bev=o.modifiers.new("PremiumEdge","BEVEL"); bev.width=min(.055,max(dims)*.008); bev.segments=3
        bev.limit_method="ANGLE"; bev.angle_limit=math.radians(28)

pts=[o.matrix_world@Vector(c) for o in meshes for c in o.bound_box]
mn=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
mx=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
ctr=(mn+mx)*.5; dims=mx-mn; span=max(dims)

sc=bpy.context.scene
for o in list(sc.objects):
    if o.type in ("CAMERA","LIGHT"): bpy.data.objects.remove(o,do_unlink=True)
if sc.world is None: sc.world=bpy.data.worlds.new("ValoriaWorld")
sc.world.use_nodes=True
bg=sc.world.node_tree.nodes.get("Background"); bg.inputs["Color"].default_value=(.055,.07,.095,1); bg.inputs["Strength"].default_value=.85
sc.render.engine="BLENDER_EEVEE"; sc.render.resolution_x=1280; sc.render.resolution_y=900; sc.render.resolution_percentage=100
sc.view_settings.look="Medium High Contrast"
sc.view_settings.exposure=1.7
sc.render.image_settings.file_format="PNG"
# ground
bpy.ops.mesh.primitive_plane_add(size=span*4,location=(ctr.x,ctr.y,mn.z-.06))
gm=bpy.data.materials.new("Ground"); gm.diffuse_color=(.075,.07,.065,1); bpy.context.object.data.materials.append(gm)
# warm key / cool fill / rim
for loc,energy,size,color in [
 (ctr+Vector((-span*.8,-span*.9,span*1.3)),4200,span*1.2,(1.0,.70,.45)),
 (ctr+Vector((span*.9,span*.35,span*.65)),1900,span,(.40,.58,1.0)),
 (ctr+Vector((0,span*.8,span*1.15)),2300,span*.8,(.55,.68,1.0))]:
    bpy.ops.object.light_add(type="AREA",location=loc); L=bpy.context.object; L.data.energy=energy; L.data.size=size; L.data.color=color; L.rotation_euler=(0,0,0)
    L.rotation_euler=(ctr-L.location).to_track_quat("-Z","Y").to_euler()
bpy.ops.object.camera_add(); cam=bpy.context.object; sc.camera=cam; cam.data.lens=58
views={"front":(0,-1.55,.32),"three-quarter":(1.05,-1.35,.62),"close":(.55,-1.05,.35),"side":(1.55,0,.32)}
for name,v in views.items():
    cam.location=ctr+Vector(v)*span; cam.rotation_euler=(ctr-cam.location).to_track_quat("-Z","Y").to_euler()
    sc.render.filepath=os.path.join(EV,name+".png"); bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(EV,"hybrid-lookdev.blend"))
tris=sum(sum(max(0,len(p.vertices)-2) for p in o.data.polygons) for o in meshes)
open(os.path.join(EV,"report.json"),"w").write(json.dumps({"meshes":len(meshes),"triangles_source":tris,"bounds":list(dims),"method":"human CC0 macro mesh + canonical-reference color transfer + box-mapped stone + micro-bump + bevel + authored lighting"},indent=2))
