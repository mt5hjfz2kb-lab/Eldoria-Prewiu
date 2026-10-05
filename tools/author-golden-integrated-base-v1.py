import bpy, math, json, hashlib, os
from pathlib import Path
from mathutils import Vector, Matrix

ROOT=Path(os.environ.get("GITHUB_WORKSPACE",os.getcwd()))
SRC=ROOT/"art-source/valoria/lookdev/golden-slice-v1/integrated-base-v1"
EVID=ROOT/"docs/evidence/valoria-golden-lookdev-slice-v1/integrated-base-v1"
SURF=ROOT/"art-source/valoria/lookdev/golden-slice-v1/surface-v2"
SRC.mkdir(parents=True,exist_ok=True);EVID.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
ORIGIN=Vector((4.0,-22.0,7.0))
OBJS=[]

def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()

def mat_pbr(name,base,rough,prefix=None):
    m=bpy.data.materials.new(name);m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*base,1);bs.inputs["Roughness"].default_value=rough
    if prefix:
        ap=SURF/f"{prefix}_albedo.png";np=SURF/f"{prefix}_normal.png";sp=SURF/f"{prefix}_smoothness.png"
        if ap.exists():
            im=bpy.data.images.load(str(ap),check_existing=True);tx=m.node_tree.nodes.new("ShaderNodeTexImage");tx.image=im
            m.node_tree.links.new(tx.outputs["Color"],bs.inputs["Base Color"])
        if np.exists():
            im=bpy.data.images.load(str(np),check_existing=True);im.colorspace_settings.name="Non-Color"
            tx=m.node_tree.nodes.new("ShaderNodeTexImage");tx.image=im;nm=m.node_tree.nodes.new("ShaderNodeNormalMap");nm.inputs["Strength"].default_value=.34
            m.node_tree.links.new(tx.outputs["Color"],nm.inputs["Color"]);m.node_tree.links.new(nm.outputs["Normal"],bs.inputs["Normal"])
        if sp.exists():
            im=bpy.data.images.load(str(sp),check_existing=True);im.colorspace_settings.name="Non-Color"
            tx=m.node_tree.nodes.new("ShaderNodeTexImage");tx.image=im;inv=m.node_tree.nodes.new("ShaderNodeMath");inv.operation="SUBTRACT";inv.inputs[0].default_value=1.0
            m.node_tree.links.new(tx.outputs["Color"],inv.inputs[1]);m.node_tree.links.new(inv.outputs[0],bs.inputs["Roughness"])
    return m

GROUND=mat_pbr("Golden Ground V2 Integrated",(.48,.41,.30),.87,"ground")
ROCK=mat_pbr("Golden Rock V2 Integrated",(.49,.47,.42),.81,"rock")
SHORE=mat_pbr("Golden Shore V2 Integrated",(.38,.38,.34),.49,"shore")
STONE=mat_pbr("Golden Stone V2 Integrated",(.67,.59,.49),.68,"stone")

def add_uv(o,scale=3.0):
    if o.type!="MESH" or len(o.data.uv_layers): return
    uv=o.data.uv_layers.new(name="MetricUV");o.data.uv_layers.active=uv;uv.active_render=True
    for p in o.data.polygons:
        axis=max(range(3),key=lambda k:abs(p.normal[k]))
        for li in p.loop_indices:
            v=o.data.vertices[o.data.loops[li].vertex_index].co
            uv.data[li].uv=((v.y/scale,v.z/scale) if axis==0 else (v.x/scale,v.z/scale) if axis==1 else (v.x/scale,v.y/scale))

def mesh_obj(name,verts,faces,mat):
    me=bpy.data.meshes.new(name+"_Mesh");me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(mat);add_uv(o)
    for p in o.data.polygons:p.use_smooth=True
    OBJS.append(o);return o

def import_glb(path,prefix,world_pos=None,target_size=None,yaw=0.0):
    before=set(bpy.context.scene.objects)
    bpy.ops.import_scene.gltf(filepath=str(path))
    new=[o for o in bpy.context.scene.objects if o not in before]
    roots=[o for o in new if o.parent is None]
    # Consolidated transform through parent empty so internal authored hierarchy/materials survive.
    bpy.ops.object.empty_add(type="PLAIN_AXES",location=(0,0,0));root=bpy.context.object;root.name=prefix+"_ROOT"
    for o in roots:o.parent=root
    if target_size:
        # Compute imported bounds in local world before transform.
        pts=[]
        for o in new:
            if o.type=="MESH": pts += [o.matrix_world@Vector(c) for c in o.bound_box]
        if pts:
            lo=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
            hi=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
            sz=hi-lo
            scale=min(target_size[0]/max(sz.x,.001),target_size[1]/max(sz.y,.001),target_size[2]/max(sz.z,.001))
            root.scale=(scale,scale,scale)
            center=(lo+hi)*.5
            root.location-=center*scale
    root.rotation_euler[2]=yaw
    if world_pos: root.location+=Vector(world_pos)
    for o in new:
        if o.type=="MESH":
            for p in o.data.polygons:p.use_smooth=True
            OBJS.append(o)
    return root,new

# 1) Real closed-family geometry is the architectural substrate, restored at its exact export origins.
gate_path=ROOT/"art-source/valoria/production/lower-gate-family-v1/LowerGateFamilyV1.glb"
bridge_path=ROOT/"art-source/valoria/production/bridge-family-v1/BridgeFamilyV1.glb"
gate_root,gate_objs=import_glb(gate_path,"GateCore",(5.566,-25.6,7.1))
bridge_root,bridge_objs=import_glb(bridge_path,"BridgeCore",(2.0,-30.0,3.2))

# Material relationship: keep authored family hierarchy but move main masonry toward Golden Surface V2 response.
for o in gate_objs+bridge_objs:
    if o.type!="MESH":continue
    n=o.name.lower()
    if any(k in n for k in ["body","spandrel","pier","wall","structural","deck"]):
        if len(o.data.materials)==0:o.data.materials.append(STONE)
        else:o.data.materials[0]=STONE

# 2) Continuous sculpt-like substrate: broad organic cross-sections, no local apron perimeters.
def bank(name,side):
    ys=[-35.2,-34.2,-33.2,-32.0,-30.8,-29.5,-28.2,-26.8,-25.2,-23.4,-21.0,-18.6]
    cols=15
    outer=-21.5 if side<0 else 30.0
    verts=[]
    for j,y in enumerate(ys):
        # central water/route-side inner edge
        inner=(.9+.18*math.sin((y+31)*.7)) if side<0 else (8.15+.20*math.sin((y+30)*.64))
        for i in range(cols):
            u=i/(cols-1)
            # ease distribution toward inner route, preserving broad natural beds
            ue=u*u*(3-2*u)
            x=outer*(1-ue)+inner*ue
            if y<-33.3: base=.25+(y+35.2)*.14
            else:
                base=.42+6.58/(1+math.exp(-(y+29.0)*1.02))
            # authored stratification: few broad beds, not per-face noise
            bed=.22*math.sin((y+34.0)*1.22 + i*.17)+.10*math.sin((x*(.22 if side<0 else .19))+j*.41)
            erosion=.07*math.sin(i*.77+j*.53)
            z=base+bed*(.35+.65*(1-u))+erosion
            if y>-26.5 and u>.72:z=6.95+.07*math.sin(y*.62+x*.28)
            verts.append((x,y,z))
    faces=[]
    for j in range(len(ys)-1):
        for i in range(cols-1):
            a=j*cols+i;b=a+1;c=a+cols;d=c+1
            faces += [(a,c,d),(a,d,b)] if (i+j)%2==0 else [(a,c,b),(b,c,d)]
    o=mesh_obj(name,verts,faces,GROUND);o.data.materials.append(ROCK);o.data.materials.append(SHORE)
    for p in o.data.polygons:
        cy=sum(o.data.vertices[v].co.y for v in p.vertices)/len(p.vertices)
        cz=sum(o.data.vertices[v].co.z for v in p.vertices)/len(p.vertices)
        p.material_index=2 if cy<-33.0 else (1 if cz<5.55 else 0)
    return o

bank("IntegratedSubstrate_West",-1);bank("IntegratedSubstrate_East",1)

# 3) High-information existing geometry is reauthored as buried geological/architectural transition,
#    not placed as freestanding props.
rockwall=ROOT/"Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/RockToWallTransition.glb"
terrace=ROOT/"Unity/Assets/Eldoria/Resources/Valoria/TerrainTerraceKit_v1/SteppedRockTerrace.glb"
stone_candidates=[
 ROOT/"Unity/Assets/Eldoria/Resources/Valoria/StoneKit/piece_02_12602tris.glb",
 ROOT/"Unity/Assets/Eldoria/Resources/Valoria/StoneKit/piece_04_7824tris.glb",
 ROOT/"Unity/Assets/Eldoria/Resources/Valoria/StoneKit/piece_06_7602tris.glb",
]
# Gate foot transitions
for idx,(pos,sz,yaw) in enumerate([
    ((-4.8,-26.3,6.9),(5.8,3.8,3.2),math.radians(8)),
    ((15.5,-25.8,6.8),(6.0,3.6,3.1),math.radians(-10)),
]):
    import_glb(rockwall,f"GateBurial_{idx}",pos,sz,yaw)
# Bridge spring/bank transitions
for idx,(pos,sz,yaw) in enumerate([
    ((-3.0,-31.2,2.0),(5.2,4.0,3.2),math.radians(18)),
    ((10.5,-31.0,2.0),(5.5,4.2,3.4),math.radians(-14)),
]):
    import_glb(terrace,f"BridgeBurial_{idx}",pos,sz,yaw)
# Larger geological breaks, intentionally sparse and partly buried.
for idx,(path,pos,sz,yaw) in enumerate([
    (stone_candidates[0],(-10.8,-29.3,3.5),(6.2,5.0,4.2),math.radians(20)),
    (stone_candidates[1],(18.0,-29.0,3.6),(6.5,5.1,4.3),math.radians(-22)),
    (stone_candidates[2],(-7.2,-33.5,.8),(4.5,3.4,2.2),math.radians(8)),
    (stone_candidates[0],(13.8,-33.3,.8),(4.6,3.5,2.3),math.radians(-12)),
]):
    import_glb(path,f"GeoBreak_{idx}",pos,sz,yaw)

# 4) Deepen the most visible architectural profiles with swept stone caps tied to real architecture.
def profile(name,pts,depth=.18):
    cu=bpy.data.curves.new(name+"_Curve","CURVE");cu.dimensions="3D";cu.resolution_u=10
    sp=cu.splines.new("BEZIER");sp.bezier_points.add(len(pts)-1)
    for p,co in zip(sp.bezier_points,pts):p.co=co;p.handle_left_type="AUTO";p.handle_right_type="AUTO"
    cu.bevel_depth=depth;cu.bevel_resolution=3
    o=bpy.data.objects.new(name,cu);bpy.context.collection.objects.link(o);o.data.materials.append(STONE)
    bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.convert(target="MESH");o.select_set(False);add_uv(o,2.0)
    OBJS.append(o)

profile("GateProfile_WestBurial",[(-7.2,-26.1,7.15),(-5.4,-25.8,7.55),(-2.5,-25.55,7.30)],.22)
profile("GateProfile_EastBurial",[(12.1,-25.45,7.26),(15.0,-25.55,7.62),(17.8,-25.3,7.18)],.22)
profile("BridgeProfile_WestSpring",[(-4.5,-31.3,2.1),(-2.0,-30.6,2.75),(.3,-30.0,3.25)],.19)
profile("BridgeProfile_EastSpring",[(7.5,-30.0,3.25),(9.7,-30.5,2.72),(12.1,-31.2,2.0)],.19)

# 5) Use the already-authored vegetation family, not skeletal procedural trees.
veg_path=ROOT/"art-source/valoria/production/vegetation-family-v1/representative/VegetationRepresentativeFamilyV1.glb"
for idx,(pos,sz,yaw) in enumerate([
    ((-6.0,-19.7,7.0),(4.7,4.7,8.4),math.radians(12)),
    ((-10.0,-32.1,2.0),(5.2,5.2,9.1),math.radians(-18)),
    ((22.4,-26.7,4.6),(4.9,4.9,8.6),math.radians(26)),
]):
    import_glb(veg_path,f"Vegetation_{idx}",pos,sz,yaw)

# Save world-space source.
blend=SRC/"GoldenIntegratedBaseV1.blend"
bpy.ops.wm.save_as_mainfile(filepath=str(blend))

# Export in established Golden local origin convention.
all_mesh=[o for o in bpy.context.scene.objects if o.type=="MESH"]
for o in bpy.context.scene.objects:
    if o.parent is None and o.type!="LIGHT" and o.type!="CAMERA": o.location-=ORIGIN
bpy.ops.object.select_all(action="DESELECT")
for o in all_mesh:o.select_set(True)
glb=SRC/"GoldenIntegratedBaseV1.glb"
bpy.ops.export_scene.gltf(filepath=str(glb),export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT",export_normals=True,export_tangents=True)
for o in bpy.context.scene.objects:
    if o.parent is None and o.type!="LIGHT" and o.type!="CAMERA": o.location+=ORIGIN
bpy.context.view_layer.update()

# Isolated art-review lighting. Neutral enough to judge geometry/material hierarchy.
scene=bpy.context.scene;scene.render.engine="BLENDER_EEVEE";scene.render.resolution_x=1400;scene.render.resolution_y=900;scene.render.resolution_percentage=100
world=bpy.data.worlds.new("GoldenIntegratedWorld");world.use_nodes=True
world.node_tree.nodes["Background"].inputs[0].default_value=(.28,.31,.35,1);world.node_tree.nodes["Background"].inputs[1].default_value=.68;scene.world=world
center=Vector((4.0,-27.3,6.0))
for loc,energy,size,color in [
    ((-16,-43,31),2250,11,(1.0,.84,.66)),
    ((26,-15,21),1050,10,(.65,.78,1.0)),
    ((4,-20,35),650,8,(1.0,.94,.82)),
]:
    bpy.ops.object.light_add(type="AREA",location=loc);l=bpy.context.object;l.data.energy=energy;l.data.size=size;l.data.color=color;l.rotation_euler=(center-l.location).to_track_quat("-Z","Y").to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type="ORTHO";scene.camera=cam
clay=bpy.data.materials.new("GoldenIntegratedClay");clay.diffuse_color=(.57,.55,.51,1);clay.roughness=.88
views=[
 ("source-lit-three-quarter",(30,-54,30),43,False),
 ("source-official-proxy",(24,-56,30),39,False),
 ("source-clay",(30,-54,30),43,True),
]
previews=[]
for name,loc,span,isclay in views:
    cam.location=loc;cam.data.ortho_scale=span;cam.rotation_euler=(center-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.view_layers[0].material_override=clay if isclay else None
    out=EVID/(name+".png");scene.render.filepath=str(out);bpy.ops.render.render(write_still=True);previews.append(str(out.relative_to(ROOT)).replace("\\","/"))
scene.view_layers[0].material_override=None

tris=verts=0;bounds=[];mats=set()
for o in all_mesh:
    o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles);verts+=len(o.data.vertices)
    bounds += [o.matrix_world@Vector(c) for c in o.bound_box]
    for m in o.data.materials:
        if m:mats.add(m.name)
lo=[min(p[i] for p in bounds) for i in range(3)];hi=[max(p[i] for p in bounds) for i in range(3)]
report={
 "authoring_standard":"BLENDER_PROFESSIONAL_V1",
 "family":"GoldenIntegratedBaseV1",
 "source_iteration":"integrated-base-source02",
 "method":"Real Lower Gate + Bridge production geometry reauthored as one micro-environment with continuous substrate and buried certified rich stone/terrain source geometry; no local apron shell.",
 "authoring_method":"Import exact closed-family Gate/Bridge source, restore canonical world origins, reauthor screen-visible structural contacts with buried/deformed high-information StoneArchitecture/TerrainTerrace/StoneKit geometry and continuous authored banks; preserve macro positions.",
 "tool_families":["mesh_edit","curves_profiles","source_geometry_reauthoring","controlled_deformation_by_fit","uv_unwrap_texel_density","material_relationship_authoring","continuous_terrain_mesh"],
 "primitive_role":"No primitive stack used. Existing certified meshes and authored continuous surfaces are the source basis.",
 "source":str(blend.relative_to(ROOT)).replace("\\","/"),"export":str(glb.relative_to(ROOT)).replace("\\","/"),
 "source_sha":sha(blend),"export_sha":sha(glb),
 "geometry_metrics":{"triangles":tris,"vertices":verts,"objects":len(all_mesh),"materials":sorted(mats),"bounds_world_blender":[lo,hi],"uv":True,"normals":True,"tangents_exported":True},
 "material_families":["Golden Surface V2 stone","Golden Surface V2 rock","Golden Surface V2 ground","Golden Surface V2 shore","retained embedded source PBR"],
 "preview_evidence":previews,
 "unity_placement":{"position":[ORIGIN.x,ORIGIN.z,ORIGIN.y],"rotation_y":180,"scale":1},
 "scope_guards":{"canonical_camera_changed":False,"macro_gate_position_changed":False,"macro_bridge_position_changed":False,"gameplay_changed":False,"colliders":0,"tripo_credits":0},
 "isolated_art_review":"PENDING HUMAN/VISUAL REVIEW — never infer from TECH PASS"
}
(EVID/"source-report.json").write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8")
print(json.dumps(report,indent=2))
