import bpy, math, json, hashlib, os
from pathlib import Path
from mathutils import Vector

ROOT=Path(os.environ.get("GITHUB_WORKSPACE",os.getcwd()))
SRC=ROOT/"art-source/valoria/lookdev/golden-slice-v1/primary-forms-v1"
EVID=ROOT/"docs/evidence/valoria-golden-lookdev-slice-v1/primary-forms-v1"
SURF=ROOT/"art-source/valoria/lookdev/golden-slice-v1/surface-v2"
SRC.mkdir(parents=True,exist_ok=True);EVID.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
ORIGIN=Vector((4.0,-22.0,7.0)); OBJS=[]

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
            tx=m.node_tree.nodes.new("ShaderNodeTexImage");tx.image=im;inv=m.node_tree.nodes.new("ShaderNodeMath");inv.operation="SUBTRACT";inv.inputs[0].default_value=1
            m.node_tree.links.new(tx.outputs["Color"],inv.inputs[1]);m.node_tree.links.new(inv.outputs[0],bs.inputs["Roughness"])
    return m

STONE=mat_pbr("Primary Warm Stone",(.63,.55,.45),.72,"stone")
STONE2=mat_pbr("Primary Cut Stone",(.73,.64,.50),.66,"stone")
ROCK=mat_pbr("Primary Rock",(.43,.42,.39),.84,"rock")
GROUND=mat_pbr("Primary Earth",(.45,.37,.27),.90,"ground")
SHORE=mat_pbr("Primary Wet Shelf",(.29,.31,.29),.52,"shore")
BARK=mat_pbr("Primary Bark",(.20,.12,.07),.90,None)
LEAF=mat_pbr("Primary Canopy",(.22,.34,.20),.80,"vegetation")
BANNER=mat_pbr("Primary Banner Blue",(.07,.19,.32),.62,None)
IRON=mat_pbr("Primary Iron",(.08,.08,.07),.42,None)

def add_uv(o,scale=3.0):
    if o.type!="MESH" or len(o.data.uv_layers): return
    uv=o.data.uv_layers.new(name="MetricUV");o.data.uv_layers.active=uv;uv.active_render=True
    for p in o.data.polygons:
        axis=max(range(3),key=lambda k:abs(p.normal[k]))
        for li in p.loop_indices:
            v=o.data.vertices[o.data.loops[li].vertex_index].co
            uv.data[li].uv=((v.y/scale,v.z/scale) if axis==0 else (v.x/scale,v.z/scale) if axis==1 else (v.x/scale,v.y/scale))

def mesh_obj(name,verts,faces,mat,smooth=False):
    me=bpy.data.meshes.new(name+"_Mesh");me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(mat);add_uv(o)
    for p in o.data.polygons:p.use_smooth=smooth
    OBJS.append(o);return o

def section_tower(name,cx,cy,z0,sections,rot=0.0):
    # sections: (z, halfx, halfy, corner_cut)
    verts=[]; rings=[]
    for z,hx,hy,cut in sections:
        pts=[(-hx+cut,-hy), (hx-cut,-hy),(hx,-hy+cut),(hx,hy-cut),(hx-cut,hy),(-hx+cut,hy),(-hx,hy-cut),(-hx,-hy+cut)]
        ring=[]
        for x,y in pts:
            c=math.cos(rot);s=math.sin(rot);ring.append(len(verts));verts.append((cx+x*c-y*s,cy+x*s+y*c,z0+z))
        rings.append(ring)
    faces=[]
    for a,b in zip(rings[:-1],rings[1:]):
        for i in range(8):faces.append((a[i],a[(i+1)%8],b[(i+1)%8],b[i]))
    faces.append(tuple(reversed(rings[0])));faces.append(tuple(rings[-1]))
    return mesh_obj(name,verts,faces,STONE,False)

def crenel_crown(name,cx,cy,z,rx,ry,rot=0.0,phase=0):
    # sparse broad merlons around crown, not repeated tiny blocks
    for i in range(8):
        if (i+phase)%3==1: continue
        a=2*math.pi*i/8+rot
        x=cx+math.cos(a)*rx*.78;y=cy+math.sin(a)*ry*.78
        bpy.ops.mesh.primitive_cube_add(location=(x,y,z+.43),scale=(.42,.34,.43))
        o=bpy.context.object;o.name=f"{name}_Merlon_{i}"
        o.rotation_euler[2]=a;o.data.materials.append(STONE2);be=o.modifiers.new("SoftEdge","BEVEL");be.width=.08;be.segments=2
        OBJS.append(o)

def arch_ring(name,cx,cy,zspring,rx,rz,depth,thick,mat,segments=32):
    verts=[];faces=[]
    for y in (cy-depth/2,cy+depth/2):
        for radd in (0,thick):
            for i in range(segments+1):
                t=math.pi*i/segments
                verts.append((cx-rx*math.cos(t)*(1+radd/rx),y,zspring+rz*math.sin(t)+radd*math.sin(t)))
    n=segments+1
    # front/back ring faces
    for side in range(2):
        off=side*2*n
        for i in range(segments):faces.append((off+i,off+i+1,off+n+i+1,off+n+i))
    # inner/outer depth
    for ring in range(2):
        a=ring*n;b=2*n+ring*n
        for i in range(segments):faces.append((a+i,b+i,b+i+1,a+i+1))
    faces += [(0,n,3*n,2*n),(segments,n+segments,3*n+segments,2*n+segments)]
    return mesh_obj(name,verts,faces,mat,False)

def wall_poly(name,outline,z0,z1,depth,mat):
    # vertical wall following 2D x/z outline, centered y with real depth
    verts=[];faces=[];cy=-25.55
    for y in (cy-depth/2,cy+depth/2):
        for x,z in outline:verts.append((x,y,z))
    n=len(outline)
    faces.append(tuple(range(n)));faces.append(tuple(range(n,2*n)))
    for i in range(n):j=(i+1)%n;faces.append((i,j,n+j,n+i))
    return mesh_obj(name,verts,faces,mat,False)

# LOWER GATE — rebuild complete visible primary forms around locked macro center.
gate_y=-25.55
west=section_tower("Gate_Primary_West",-0.1,gate_y,6.70,[(0,3.25,2.05,.45),(.65,2.95,1.86,.38),(5.65,2.62,1.72,.34),(7.55,2.78,1.82,.40),(8.20,2.98,1.94,.42)],math.radians(-1.8))
east=section_tower("Gate_Primary_East",9.30,gate_y+.10,6.62,[(0,3.45,2.12,.50),(.72,3.08,1.90,.40),(5.45,2.72,1.76,.35),(7.30,2.90,1.88,.42),(8.12,3.13,2.00,.46)],math.radians(1.4))
crenel_crown("GateWestCrown",-0.1,gate_y,14.95,2.75,1.80,math.radians(-1.8),0)
crenel_crown("GateEastCrown",9.30,gate_y+.1,14.75,2.92,1.86,math.radians(1.4),1)
arch_ring("Gate_Primary_DeepArch",4.60,gate_y-.22,9.45,3.05,3.58,2.55,.62,STONE2,34)
# thick upper curtain, deliberately stepped/asymmetric
outline=[(1.55,10.0),(1.55,14.10),(3.10,14.10),(3.10,13.45),(6.35,13.45),(6.35,14.22),(7.70,14.22),(7.70,10.0)]
wall_poly("Gate_Primary_UpperCurtain",outline,0,0,2.25,STONE)
# wing masses descend into terrain instead of ending as flat boxes
for name,pts in [
 ("Gate_WestWing",[(-3.25,7.15),(-3.25,11.5),(-7.3,11.15),(-9.2,9.8),(-9.2,7.0)]),
 ("Gate_EastWing",[(12.7,7.1),(12.7,11.45),(16.2,11.0),(18.0,9.55),(18.0,6.9)])
]:
    wall_poly(name,pts,0,0,1.65,STONE)
# portcullis depth cue in opening
for ix in range(6):
    x=2.25+ix*.94
    bpy.ops.mesh.primitive_cube_add(location=(x,gate_y+.78,10.65),scale=(.055,.055,2.0));o=bpy.context.object;o.name=f"Gate_Portcullis_{ix}";o.data.materials.append(IRON);OBJS.append(o)
# banners as broad asymmetric identity
for x,z,h,w in [(-.35,11.55,2.2,.55),(9.55,11.35,1.9,.52)]:
    verts=[(x-w,gate_y-2.08,z+h/2),(x+w,gate_y-2.08,z+h/2),(x+w*.68,gate_y-2.08,z-h/2),(x,gate_y-2.08,z-h/2-.35),(x-w*.72,gate_y-2.08,z-h/2)]
    mesh_obj("Gate_Banner",verts,[(0,1,2,3,4)],BANNER,False)

# BRIDGE — purpose-built arch/load path, same macro landing.
def bridge_arch_surface(name,cx,cy,width,span,zspring,rz,depth,thickness):
    # visible masonry arch + deep barrel
    return arch_ring(name,cx,cy,zspring,span/2,rz,depth,thickness,STONE2,40)
bridge_arch_surface("Bridge_Primary_Arch",4.50,-30.50,10.0,10.0,1.10,3.25,3.0,.55)
# tapered abutments with genuine buried feet
section_tower("Bridge_WestAbutment",-1.0,-30.45,.15,[(0,2.7,2.25,.42),(.7,2.35,2.0,.34),(4.5,1.8,1.75,.28)],math.radians(-2.5))
section_tower("Bridge_EastAbutment",10.0,-30.35,.12,[(0,2.8,2.3,.44),(.75,2.38,2.04,.35),(4.45,1.85,1.76,.28)],math.radians(2))
# cambered deck as lofted strips, not a flat slab
ys=[-33.25,-32.2,-31.1,-30.0,-28.9,-27.8]
verts=[];faces=[]
for j,y in enumerate(ys):
    t=j/(len(ys)-1);z=4.55+.48*math.sin(math.pi*t)
    half=4.75-.12*math.cos(math.pi*t)
    for x in (4.5-half,4.5+half):verts.append((x,y,z))
for j in range(len(ys)-1):
    a=2*j;b=a+2;faces.append((a,a+1,b+1,b))
deck=mesh_obj("Bridge_Primary_DeckTop",verts,faces,STONE,False)
# edge/parapet masses follow deck camber with terminal thickening
for side in (-1,1):
    pts=[]
    for j,y in enumerate(ys):
        t=j/(len(ys)-1);z=4.55+.48*math.sin(math.pi*t);half=4.75-.12*math.cos(math.pi*t)
        pts.append((4.5+side*(half-.18),y,z+.55))
    # swept square rail
    cu=bpy.data.curves.new(f"BridgeParapet{side}","CURVE");cu.dimensions="3D";cu.resolution_u=4
    sp=cu.splines.new("BEZIER");sp.bezier_points.add(len(pts)-1)
    for p,co in zip(sp.bezier_points,pts):p.co=co;p.handle_left_type="AUTO";p.handle_right_type="AUTO"
    cu.bevel_depth=.22;cu.bevel_resolution=1
    o=bpy.data.objects.new(f"Bridge_Primary_Parapet_{side}",cu);bpy.context.collection.objects.link(o);o.data.materials.append(STONE2);OBJS.append(o)

# CONTINUOUS CLIFF/GROUND/SHORE — two banks with authored geological shelves.
def bank(name,side):
    ys=[-35.6,-34.6,-33.5,-32.4,-31.2,-30.0,-28.7,-27.2,-25.6,-23.8,-21.5,-18.5]
    cols=18; outer=-22.5 if side<0 else 31.0
    verts=[]
    for j,y in enumerate(ys):
        inner=(.25+.28*math.sin((y+31)*.42)) if side<0 else (8.75+.30*math.sin((y+30)*.38))
        for i in range(cols):
            u=i/(cols-1);ue=u*u*(3-2*u);x=outer*(1-ue)+inner*ue
            # large stepped geological progression with erosion, never a sheet plane
            rise=6.55/(1+math.exp(-(y+29.0)*1.04))
            bed=.30*math.sin((y+34.1)*.85+i*.11)+.15*math.sin(x*.22+j*.33)
            shelf=.22*math.sin((y+31.0)*1.55)*(1-u)
            z=.30+rise+bed*(.25+.75*(1-u))+shelf
            if y>-26.2 and u>.72:z=6.92+.09*math.sin(y*.55+x*.20)
            verts.append((x,y,z))
    faces=[]
    for j in range(len(ys)-1):
        for i in range(cols-1):
            a=j*cols+i;b=a+1;c=a+cols;d=c+1
            faces += [(a,c,d),(a,d,b)] if (i+j)%2==0 else [(a,c,b),(b,c,d)]
    o=mesh_obj(name,verts,faces,GROUND,False);o.data.materials.append(ROCK);o.data.materials.append(SHORE)
    for p in o.data.polygons:
        cy=sum(o.data.vertices[v].co.y for v in p.vertices)/len(p.vertices);cz=sum(o.data.vertices[v].co.z for v in p.vertices)/len(p.vertices)
        p.material_index=2 if cy<-33.15 else (1 if cz<5.65 else 0)
    return o
bank("PrimaryTerrain_West",-1);bank("PrimaryTerrain_East",1)

# Sparse embedded ledges generated from same geology, materially part of the cliff.
for idx,(cx,cy,cz,sx,sy,sz,phase) in enumerate([
 (-7.5,-30.0,3.6,3.5,1.7,1.3,.2),(15.8,-29.4,3.8,3.8,1.8,1.4,1.0),
 (-11.0,-26.2,5.6,3.2,1.4,1.0,2.0),(20.0,-25.8,5.7,3.4,1.5,1.1,2.7),
 (-5.8,-33.4,.65,2.3,1.15,.65,3.4),(13.0,-33.2,.72,2.5,1.2,.7,4.1)
]):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,location=(cx,cy,cz))
    o=bpy.context.object;o.name=f"PrimaryGeoLedge_{idx}";o.scale=(sx,sy,sz);o.rotation_euler=(.05*math.sin(phase),.1*math.cos(phase),phase*.18);o.data.materials.append(SHORE if cy<-32.8 else ROCK)
    # deterministic low-amplitude deformation to avoid sphere read
    for v in o.data.vertices:
        n=v.co.normalized();amp=1+.12*math.sin(n.x*5+phase)+.08*math.sin(n.z*7-phase)
        v.co*=amp
    OBJS.append(o)

# VEGETATION — 3 strong canopy masses, no cones/branch skeleton.
def tree(name,x,y,z,h,r,phase):
    bpy.ops.mesh.primitive_cylinder_add(vertices=10,radius=r*.13,depth=h*.72,location=(x,y,z+h*.34))
    tr=bpy.context.object;tr.name=name+"_Trunk";tr.rotation_euler=(.02*math.sin(phase),.03*math.cos(phase),0);tr.data.materials.append(BARK);OBJS.append(tr)
    for k,(zf,rs) in enumerate([(0.42,1.0),(0.58,.86),(0.72,.67),(0.84,.46),(0.93,.27)]):
        # offset lobes produce asymmetrical canopy silhouette
        for l in range(2 if k<3 else 1):
            a=phase+k*.83+l*2.4;off=r*(.18 if l else .08)
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,location=(x+math.cos(a)*off,y+math.sin(a)*off,z+h*zf))
            o=bpy.context.object;o.name=f"{name}_Canopy_{k}_{l}";o.scale=(r*rs*(1+.08*math.sin(a)),r*rs*.72,h*.105*(1+.10*math.cos(a)));o.rotation_euler[2]=a*.35;o.data.materials.append(LEAF)
            for v in o.data.vertices:
                n=v.co.normalized();v.co*=1+.10*math.sin(n.x*7+n.z*4+phase+k)
            OBJS.append(o)
tree("PrimaryTree_WestGate",-7.0,-20.2,7.0,8.5,2.3,.3)
tree("PrimaryTree_ForegroundWest",-10.4,-32.0,1.65,9.1,2.65,1.4)
tree("PrimaryTree_EastBank",22.4,-27.0,4.5,8.7,2.45,2.6)

# Convert curve parapets and apply modest bevels where useful.
for o in list(OBJS):
    if o.type=="CURVE":
        bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.convert(target="MESH");o.select_set(False);add_uv(o,2.5)

# Save/editable source.
blend=SRC/"GoldenPrimaryFormsV1.blend";bpy.ops.wm.save_as_mainfile(filepath=str(blend))

# Export from established Golden local origin.
for o in bpy.context.scene.objects:
    if o.parent is None and o.type not in {"LIGHT","CAMERA"}:o.location-=ORIGIN
bpy.ops.object.select_all(action="DESELECT")
all_mesh=[o for o in bpy.context.scene.objects if o.type=="MESH"]
for o in all_mesh:o.select_set(True)
glb=SRC/"GoldenPrimaryFormsV1.glb"
bpy.ops.export_scene.gltf(filepath=str(glb),export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT",export_normals=True,export_tangents=True)
for o in bpy.context.scene.objects:
    if o.parent is None and o.type not in {"LIGHT","CAMERA"}:o.location+=ORIGIN
bpy.context.view_layer.update()

# Review setup: geometry-first neutral lighting.
scene=bpy.context.scene;scene.render.engine="BLENDER_EEVEE";scene.render.resolution_x=1536;scene.render.resolution_y=1024;scene.render.resolution_percentage=100
world=bpy.data.worlds.new("PrimaryWorld");world.use_nodes=True;world.node_tree.nodes["Background"].inputs[0].default_value=(.32,.34,.36,1);world.node_tree.nodes["Background"].inputs[1].default_value=.60;scene.world=world
center=Vector((4.5,-27.0,7.0))
for loc,energy,size,color in [((-18,-46,34),2100,12,(1,.84,.67)),((28,-17,23),900,11,(.68,.79,1)),((4,-24,38),450,9,(1,.94,.84))]:
    bpy.ops.object.light_add(type="AREA",location=loc);l=bpy.context.object;l.data.energy=energy;l.data.size=size;l.data.color=color;l.rotation_euler=(center-l.location).to_track_quat("-Z","Y").to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type="ORTHO";scene.camera=cam
clay=bpy.data.materials.new("PrimaryClay");clay.diffuse_color=(.58,.56,.52,1);clay.roughness=.92
views=[
 ("primary-clay-silhouette",(31,-55,31),43,True),
 ("primary-lit-three-quarter",(31,-55,31),43,False),
 ("primary-official-proxy",(24,-56,30),39,False)
]
previews=[]
for name,loc,span,isclay in views:
    cam.location=loc;cam.data.ortho_scale=span;cam.rotation_euler=(center-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.view_layers[0].material_override=clay if isclay else None
    out=EVID/(name+".png");scene.render.filepath=str(out);bpy.ops.render.render(write_still=True);previews.append(str(out.relative_to(ROOT)).replace("\\","/"))
scene.view_layers[0].material_override=None

tris=verts=0;bounds=[];mats=set()
for o in all_mesh:
    o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles);verts+=len(o.data.vertices);bounds += [o.matrix_world@Vector(c) for c in o.bound_box]
    for m in o.data.materials:
        if m:mats.add(m.name)
lo=[min(p[i] for p in bounds) for i in range(3)];hi=[max(p[i] for p in bounds) for i in range(3)]
report={
 "authoring_standard":"BLENDER_PROFESSIONAL_V1",
 "family":"GoldenPrimaryFormsV1",
 "method":"Purpose-built full screen-visible primary forms after Integrated Method B exhaustion. No inherited Gate/Bridge primary meshes, no donor geometry, no overlay/apron method.",
 "primary_form_design":{
   "lower_gate":"battered asymmetric tower masses, deep structural arch, stepped upper curtain, terrain-descending wings, broad crown hierarchy",
   "bridge":"deep arch barrel, tapered abutments, cambered deck, continuous structural parapet lines",
   "terrain":"continuous graded banks with large geological beds and topographic wet shelf",
   "vegetation":"three sparse asymmetrical opaque canopy-mass trees with visible trunks"
 },
 "source":str(blend.relative_to(ROOT)).replace("\\","/"),"export":str(glb.relative_to(ROOT)).replace("\\","/"),
 "source_sha":sha(blend),"export_sha":sha(glb),
 "geometry_metrics":{"triangles":tris,"vertices":verts,"objects":len(all_mesh),"materials":sorted(mats),"bounds_world_blender":[lo,hi],"uv":True,"normals":True,"tangents_exported":True},
 "preview_evidence":previews,
 "canonical_reference":"references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg",
 "scope_guards":{"canonical_camera_changed":False,"macro_gate_position_changed":False,"macro_bridge_position_changed":False,"gameplay_changed":False,"road_stair_bastion_changed":False,"colliders":0,"tripo_credits":0},
 "visual_gate":{"required":4.0,"geometry_silhouette":"PENDING VISUAL REVIEW","material_response":"PENDING VISUAL REVIEW","contact_integration":"PENDING VISUAL REVIEW","environment_coherence":"PENDING VISUAL REVIEW","premium_perception":"PENDING VISUAL REVIEW"},
 "unity_integration":"PROHIBITED UNTIL EXPLICIT SOURCE VISUAL PASS"
}
(EVID/"source-report.json").write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8")
print(json.dumps(report,indent=2))
