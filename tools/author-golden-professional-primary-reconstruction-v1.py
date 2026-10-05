import bpy, math, json, hashlib, os
from pathlib import Path
from mathutils import Vector

ROOT=Path(os.environ.get("GITHUB_WORKSPACE",os.getcwd()))
OUT=ROOT/"art-source/valoria/lookdev/golden-slice-v1/professional-primary-reconstruction-v1"
EVID=ROOT/"docs/evidence/valoria-golden-lookdev-slice-v1/professional-primary-reconstruction-v1"
SURF=ROOT/"art-source/valoria/lookdev/golden-slice-v1/surface-v2"
OUT.mkdir(parents=True,exist_ok=True); EVID.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)

ORIGIN=Vector((4.0,-22.0,7.0))
OBJS=[]

def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()

def material(name,base,rough,prefix=None,metallic=0.0):
    m=bpy.data.materials.new(name); m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*base,1)
    bs.inputs["Roughness"].default_value=rough
    bs.inputs["Metallic"].default_value=metallic
    if prefix:
        ap=SURF/f"{prefix}_albedo.png"; np=SURF/f"{prefix}_normal.png"; sp=SURF/f"{prefix}_smoothness.png"
        if ap.exists():
            im=bpy.data.images.load(str(ap),check_existing=True)
            t=m.node_tree.nodes.new("ShaderNodeTexImage"); t.image=im
            m.node_tree.links.new(t.outputs["Color"],bs.inputs["Base Color"])
        if np.exists():
            im=bpy.data.images.load(str(np),check_existing=True); im.colorspace_settings.name="Non-Color"
            t=m.node_tree.nodes.new("ShaderNodeTexImage"); t.image=im
            n=m.node_tree.nodes.new("ShaderNodeNormalMap"); n.inputs["Strength"].default_value=.36
            m.node_tree.links.new(t.outputs["Color"],n.inputs["Color"]); m.node_tree.links.new(n.outputs["Normal"],bs.inputs["Normal"])
        if sp.exists():
            im=bpy.data.images.load(str(sp),check_existing=True); im.colorspace_settings.name="Non-Color"
            t=m.node_tree.nodes.new("ShaderNodeTexImage"); t.image=im
            inv=m.node_tree.nodes.new("ShaderNodeMath"); inv.operation="SUBTRACT"; inv.inputs[0].default_value=1
            m.node_tree.links.new(t.outputs["Color"],inv.inputs[1]); m.node_tree.links.new(inv.outputs[0],bs.inputs["Roughness"])
    return m

STONE=material("PRV1 Warm Fortification Stone",(.63,.54,.42),.72,"stone")
CUT=material("PRV1 Cut Stone",(.74,.64,.49),.64,"stone")
ROCK=material("PRV1 Geological Rock",(.35,.35,.33),.88,"rock")
GROUND=material("PRV1 Ground",(.42,.34,.23),.91,"ground")
WET=material("PRV1 Wet Shelf",(.24,.27,.26),.52,"shore")
LEAF=material("PRV1 Canopy",(.16,.27,.15),.84,"vegetation")
BARK=material("PRV1 Bark",(.17,.10,.06),.92,None)
WATER=material("PRV1 Water",(.045,.13,.16),.22,None)
DARK=material("PRV1 Portal Shadow",(.012,.016,.017),.96,None)

def metric_uv(o,scale=3.2):
    if o.type!="MESH" or len(o.data.uv_layers): return
    u=o.data.uv_layers.new(name="MetricUV"); o.data.uv_layers.active=u; u.active_render=True
    for p in o.data.polygons:
        axis=max(range(3),key=lambda k:abs(p.normal[k]))
        for li in p.loop_indices:
            v=o.data.vertices[o.data.loops[li].vertex_index].co
            if axis==0: uv=(v.y/scale,v.z/scale)
            elif axis==1: uv=(v.x/scale,v.z/scale)
            else: uv=(v.x/scale,v.y/scale)
            u.data[li].uv=uv

def make_mesh(name,verts,faces,mat,smooth=False,bevel=0.0):
    me=bpy.data.meshes.new(name+"_Mesh"); me.from_pydata(verts,[],faces); me.update()
    o=bpy.data.objects.new(name,me); bpy.context.collection.objects.link(o); o.data.materials.append(mat); metric_uv(o)
    for p in o.data.polygons: p.use_smooth=smooth
    if bevel>0:
        b=o.modifiers.new("Primary bevel","BEVEL"); b.width=bevel; b.segments=3; b.limit_method="ANGLE"
    OBJS.append(o); return o

def loft(name,rings,mat,cap=True,bevel=.0):
    verts=[]; idx=[]
    for ring in rings:
        rr=[]
        for p in ring: rr.append(len(verts)); verts.append(p)
        idx.append(rr)
    n=len(idx[0]); faces=[]
    for a,b in zip(idx[:-1],idx[1:]):
        for i in range(n): faces.append((a[i],a[(i+1)%n],b[(i+1)%n],b[i]))
    if cap:
        faces.append(tuple(reversed(idx[0]))); faces.append(tuple(idx[-1]))
    return make_mesh(name,verts,faces,mat,False,bevel)

def oct_ring(cx,cy,z,hx,hy,cut=.35,rot=0.0,front_bias=0.0):
    pts=[(-hx+cut,-hy),(hx-cut,-hy),(hx,-hy+cut),(hx,hy-cut),(hx-cut,hy),(-hx+cut,hy),(-hx,hy-cut),(-hx,-hy+cut)]
    out=[]
    c,s=math.cos(rot),math.sin(rot)
    for x,y in pts:
        yy=y + (front_bias if y<0 else 0)
        out.append((cx+x*c-y*s,cy+x*s+yy*c,z))
    return out

def gate_tower(name,cx,cy,side):
    # One continuous tapered bastion body; no additive tower-helper stack.
    profiles=[
      (6.45,3.45,2.44,.54,0.00),
      (7.25,3.24,2.30,.50,-.04),
      (9.65,2.92,2.12,.45,-.10),
      (12.45,2.68,1.96,.42,-.16),
      (13.20,2.92,2.08,.45,-.19),
      (14.25,3.12,2.16,.48,-.23),
      (15.05,2.86,2.00,.44,-.26),
    ]
    rings=[oct_ring(cx,cy,z,hx,hy,cut,math.radians(side*1.2),fb) for z,hx,hy,cut,fb in profiles]
    return loft(name,rings,STONE,True,.09)

def arch_profile(cx,zspring,rx,rz,segments=24):
    pts=[]
    # left jamb -> arch -> right jamb, clockwise viewed from front
    pts.append((cx-rx,6.55))
    pts.append((cx-rx,zspring))
    for i in range(segments+1):
        t=math.pi-i*math.pi/segments
        pts.append((cx+rx*math.cos(t),zspring+rz*math.sin(t)))
    pts.append((cx+rx,6.55))
    return pts

def portal_shell():
    # Thick custom arch shell from outer/inner profiles with deep reveal.
    cx=4.57; yo=-27.02; yi=-23.92
    outer=arch_profile(cx,9.0,3.18,3.82,28)
    inner=arch_profile(cx,9.0,2.50,3.10,28)
    verts=[]; faces=[]; O=[]; I=[]
    for y,prof,arr in [(yo,outer,O),(yi,outer,O)]:
        pass
    # four loops: outer front/back, inner front/back
    loops=[]
    for y,prof in [(yo,outer),(yi,outer),(yo-.02,inner),(yi-.02,inner)]:
        row=[]
        for x,z in prof: row.append(len(verts)); verts.append((x,y,z))
        loops.append(row)
    n=len(outer)
    # outer depth and inner reveal depth
    for i in range(n-1):
        faces.append((loops[0][i],loops[0][i+1],loops[1][i+1],loops[1][i]))
        faces.append((loops[2][i],loops[3][i],loops[3][i+1],loops[2][i+1]))
        faces.append((loops[0][i],loops[2][i],loops[2][i+1],loops[0][i+1]))
        faces.append((loops[1][i],loops[1][i+1],loops[3][i+1],loops[3][i]))
    make_mesh("GatePRV1_DeepPortalShell",verts,faces,CUT,False,.07)
    # recessed void plane
    iv=arch_profile(cx,9.0,2.36,2.98,28)
    vv=[(x,yi+.08,z) for x,z in iv]
    ff=[tuple(range(len(vv)))]
    make_mesh("GatePRV1_PortalVoid",vv,ff,DARK,False,0)

def crown_profile(name,cx,cy,side):
    # Crown is one authored silhouette strip with unequal shoulders.
    y0=cy-2.10; y1=cy+1.95
    xs=[cx-2.68,cx-2.18,cx-1.82,cx-1.22,cx-.72,cx-.18,cx+.42,cx+1.02,cx+1.66,cx+2.20,cx+2.64]
    ztops=[14.55,15.28,15.02,15.63,15.00,15.46,14.96,15.55,14.94,15.34,14.42]
    verts=[]; front=[]; back=[]
    for x,z in zip(xs,ztops): front.append(len(verts)); verts.append((x,y0,z))
    for x,z in zip(xs,ztops): back.append(len(verts)); verts.append((x,y1,z-.12))
    # lower line
    fl=[]; bl=[]
    for x in xs: fl.append(len(verts)); verts.append((x,y0,13.75))
    for x in xs: bl.append(len(verts)); verts.append((x,y1,13.75))
    faces=[]
    for i in range(len(xs)-1):
        faces += [(front[i],fl[i],fl[i+1],front[i+1]),(back[i+1],bl[i+1],bl[i],back[i]),
                  (front[i],back[i],bl[i],fl[i]),(front[i+1],fl[i+1],bl[i+1],back[i+1])]
    faces += [tuple(front+list(reversed(back))),tuple(reversed(fl+list(reversed(bl))))]
    make_mesh(name,verts,faces,CUT,False,.05)

def gate_wing(name,side):
    # Integrated descending stone-to-rock wing, one strong wedge not repeated blocks.
    if side<0:
        outer=[(-2.6,-25.4,12.9),(-5.4,-25.0,11.7),(-8.4,-24.5,9.8),(-11.0,-24.0,8.2)]
        inner=[(-2.7,-22.9,12.55),(-5.6,-22.6,11.25),(-8.6,-22.2,9.35),(-11.2,-21.9,7.85)]
    else:
        outer=[(11.8,-25.2,12.7),(14.5,-24.8,11.4),(17.3,-24.4,9.55),(19.8,-23.9,8.1)]
        inner=[(11.7,-22.8,12.35),(14.4,-22.4,11.05),(17.2,-22.0,9.15),(20.0,-21.7,7.75)]
    verts=[]; top=[]; base=[]
    for p in outer: top.append(len(verts)); verts.append(p)
    for p in inner: top.append(len(verts)); verts.append(p)
    for x,y,z in outer: base.append(len(verts)); verts.append((x,y,6.65))
    for x,y,z in inner: base.append(len(verts)); verts.append((x,y,6.65))
    # custom hull strips across 4 stations
    faces=[]
    for i in range(3):
        faces += [(top[i],top[i+1],top[4+i+1],top[4+i]),
                  (base[i+1],base[i],base[4+i],base[4+i+1]),
                  (top[i],base[i],base[i+1],top[i+1]),
                  (top[4+i+1],base[4+i+1],base[4+i],top[4+i])]
    faces += [(top[0],top[4],base[4],base[0]),(top[3],base[3],base[7],top[7])]
    make_mesh(name,verts,faces,STONE,False,.08)

def bridge():
    # Variable-width, cambered deck with continuous structural belly and embedded end sections.
    ys=[-35.4,-34.0,-32.5,-31.0,-29.4,-28.0,-26.6,-25.5]
    centers=[]; widths=[]; tops=[]; bottoms=[]
    for i,y in enumerate(ys):
        t=i/(len(ys)-1)
        cx=4.48 + .10*math.sin(t*math.pi)
        w=3.78 - .38*math.sin(t*math.pi)
        z=2.25 + 4.55*t + .28*math.sin(t*math.pi)
        belly=z-(1.05+.38*math.sin(t*math.pi))
        centers.append(cx); widths.append(w); tops.append(z); bottoms.append(belly)
    verts=[]; rings=[]
    for cx,w,y,zt,zb in zip(centers,widths,ys,tops,bottoms):
        # trapezoid section: underside narrower and deliberately non-orthogonal
        rings.append([len(verts)+i for i in range(4)])
        verts += [(cx-w,y,zt),(cx+w,y,zt),(cx+w*.86,y,zb),(cx-w*.86,y,zb)]
    faces=[]
    for a,b in zip(rings[:-1],rings[1:]):
        for i in range(4): faces.append((a[i],a[(i+1)%4],b[(i+1)%4],b[i]))
    faces += [tuple(reversed(rings[0])),tuple(rings[-1])]
    make_mesh("BridgePRV1_StructuralBody",verts,faces,STONE,False,.10)
    # True longitudinal arch void cut/read is represented as deep dark intrados volume nested in body.
    vv=[]; ff=[]; seg=30
    for x in (1.35,7.62):
        row=[]
        for i in range(seg+1):
            t=math.pi*i/seg
            y=-30.55-2.60*math.cos(t); z=1.15+2.55*math.sin(t)
            row.append(len(vv)); vv.append((x,y,z))
        row += []
    # intrados side bands, two sides, thick enough to read as structure
    make_mesh("BridgePRV1_IntradosWest",[(1.18,-33.2,1.12),(1.18,-30.55,3.70),(1.18,-27.9,1.12),(1.52,-27.9,1.12),(1.52,-30.55,3.28),(1.52,-33.2,1.12)],[(0,1,2,3,4,5)],DARK,False,0)
    make_mesh("BridgePRV1_IntradosEast",[(7.78,-33.2,1.12),(7.78,-30.55,3.70),(7.78,-27.9,1.12),(7.44,-27.9,1.12),(7.44,-30.55,3.28),(7.44,-33.2,1.12)],[(0,1,2,3,4,5)],DARK,False,0)
    # low parapets as continuous custom profiles
    for side in (-1,1):
        pv=[]; pf=[]
        for i,(cx,w,y,z) in enumerate(zip(centers,widths,ys,tops)):
            x=cx+side*(w-.12)
            pv += [(x,y,z+.08),(x,y,z+.78)]
        for i in range(len(ys)-1):
            a=i*2; b=a+2; pf.append((a,b,b+1,a+1))
        make_mesh(f"BridgePRV1_Parapet_{side}",pv,pf,CUT,False,.05)

def terrain_bank(name,xs,side):
    # One bank mesh from plateau through cliff strata into wet shelf, with irregular plan and no planar cut face.
    rows=[
      ("back",-19.0,7.05),
      ("shoulder",-24.0,6.95),
      ("lip",-27.25,6.65),
      ("ledge1",-27.85,5.50),
      ("ledge2",-28.65,3.95),
      ("toe",-29.55,1.45),
      ("wet",-31.15,.52),
      ("wateredge",-33.05,.16),
    ]
    verts=[]; ridx=[]
    for r,(label,ybase,zbase) in enumerate(rows):
        row=[]
        for i,x in enumerate(xs):
            edge=(i/(len(xs)-1))
            # authored large-scale geological waves; each band offset differently.
            y=ybase + .30*math.sin(i*.83+r*.61+side*.55) + .12*math.sin(i*1.71-r*.37)
            z=zbase + .22*math.sin(i*.72+r*.47+side) + (0.10 if r<3 else 0)
            xx=x + .24*math.sin(i*.91+r*.33)
            row.append(len(verts)); verts.append((xx,y,z))
        ridx.append(row)
    faces=[]
    for r in range(len(ridx)-1):
        for i in range(len(xs)-1):
            a=ridx[r][i]; b=ridx[r][i+1]; c=ridx[r+1][i]; d=ridx[r+1][i+1]
            if (i+r)%2: faces += [(a,c,b),(b,c,d)]
            else: faces += [(a,c,d),(a,d,b)]
    o=make_mesh(name,verts,faces,ROCK,False,.03)
    # assign material by geological zone using polygon centers after mesh creation
    o.data.materials.append(GROUND); o.data.materials.append(WET)
    # first two row strips = ground, last two = wet, middle = rock
    strips=len(xs)-1
    for p in o.data.polygons:
        z=p.center.z if hasattr(p,"center") else 3.0
        if z>6.4: p.material_index=1
        elif z<1.0: p.material_index=2
        else: p.material_index=0
    return o

def bridge_receiver(name,side):
    # Sculptural abutment surface fusing bridge flank into cliff lip.
    x0=.55 if side<0 else 8.42
    x1=1.52 if side<0 else 7.44
    pts=[
      (x0,-28.55,5.95),(x1,-28.10,6.55),(x1,-26.15,6.75),(x0,-25.65,6.62),
      (x0,-30.35,2.20),(x1,-30.10,2.55),(x1,-28.10,5.85),(x0,-28.55,5.30)
    ]
    faces=[(0,1,2,3),(4,5,6,7),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)]
    make_mesh(name,pts,faces,ROCK,False,.12)

def canopy_cluster(name,centers):
    # One compound canopy mesh per composition mass; irregular overlapping lobes merged in one source object.
    verts=[]; faces=[]
    seg=12
    for ci,(cx,cy,cz,rx,ry,rz,phase) in enumerate(centers):
        rings=[]
        for j,phi in enumerate([-1.10,-.55,0,.55,1.10]):
            row=[]
            rr=math.cos(phi)
            for i in range(seg):
                a=2*math.pi*i/seg+phase+j*.09
                irr=1+.10*math.sin(i*2.13+ci*.71+j)
                x=cx+math.cos(a)*rx*rr*irr
                y=cy+math.sin(a)*ry*rr*(1+.06*math.cos(i*1.7))
                z=cz+math.sin(phi)*rz
                row.append(len(verts)); verts.append((x,y,z))
            rings.append(row)
        for a,b in zip(rings[:-1],rings[1:]):
            for i in range(seg): faces.append((a[i],a[(i+1)%seg],b[(i+1)%seg],b[i]))
        faces.append(tuple(reversed(rings[0]))); faces.append(tuple(rings[-1]))
    make_mesh(name,verts,faces,LEAF,True,.02)

def trunk(name,x,y,z,h,r,leanx=.0,leany=.0):
    rings=[]
    verts=[]
    for j,t in enumerate([0,.45,1.0]):
        cx=x+leanx*t; cy=y+leany*t; zz=z+h*t; rr=r*(1-.45*t)
        row=[]
        for i in range(8):
            a=2*math.pi*i/8; row.append(len(verts)); verts.append((cx+rr*math.cos(a),cy+rr*math.sin(a),zz))
        rings.append(row)
    faces=[]
    for a,b in zip(rings[:-1],rings[1:]):
        for i in range(8): faces.append((a[i],a[(i+1)%8],b[(i+1)%8],b[i]))
    faces += [tuple(reversed(rings[0])),tuple(rings[-1])]
    make_mesh(name,verts,faces,BARK,False,.015)

# ---------------------------- build primary forms ----------------------------
gy=-25.15
gate_tower("GatePRV1_WestTower",-0.18,gy,-1)
gate_tower("GatePRV1_EastTower",9.28,gy+.10,1)
portal_shell()
crown_profile("GatePRV1_WestCrown",-0.18,gy,-1)
crown_profile("GatePRV1_EastCrown",9.28,gy+.10,1)
gate_wing("GatePRV1_WestWing",-1); gate_wing("GatePRV1_EastWing",1)

# central arch crown/keystone mass as custom wedge tied to towers, not a floating beam
make_mesh("GatePRV1_CentralCrown",
 [(1.55,-27.0,12.55),(7.58,-27.0,12.55),(7.15,-27.0,14.10),(5.25,-27.0,14.55),(3.95,-27.0,14.72),(2.02,-27.0,14.08),
  (1.75,-23.9,12.55),(7.38,-23.9,12.55),(6.98,-23.9,13.82),(5.18,-23.9,14.18),(4.00,-23.9,14.34),(2.18,-23.9,13.80)],
 [(0,1,2,3,4,5),(11,10,9,8,7,6),(0,6,7,1),(1,7,8,2),(2,8,9,3),(3,9,10,4),(4,10,11,5),(5,11,6,0)],STONE,False,.08)

bridge()
terrain_bank("TerrainPRV1_WestBank",[-21,-18,-15,-12,-9,-6,-3,-.2],-1)
terrain_bank("TerrainPRV1_EastBank",[8.3,11.2,14.2,17.5,20.7,24.0,27.0,30.0],1)
bridge_receiver("TerrainPRV1_WestReceiver",-1); bridge_receiver("TerrainPRV1_EastReceiver",1)

# water beneath bridge and between authored wet shelves
make_mesh("TerrainPRV1_Water",[(-22,-40,.06),(31,-40,.06),(30,-31.9,.06),(-21,-31.9,.06)],[(0,1,2,3)],WATER,False,0)

# Vegetation as 4 screen-space masses rather than scattered isolated trees.
trunk("VegPRV1_WestUpperTrunk",-8.2,-22.0,6.9,5.7,.30,.25,-.15)
canopy_cluster("VegPRV1_WestUpperMass",[
 (-8.1,-22.0,11.3,1.9,1.55,2.55,.2),(-9.3,-22.4,10.3,1.55,1.35,2.0,1.1),(-6.9,-21.8,10.1,1.40,1.20,1.8,2.2)])
trunk("VegPRV1_WestForeTrunk",-14.6,-29.4,1.2,6.0,.34,-.18,.20)
canopy_cluster("VegPRV1_WestForeMass",[
 (-14.7,-29.3,6.0,2.25,1.75,2.95,.8),(-16.1,-29.0,5.3,1.75,1.45,2.3,1.7),(-13.1,-29.8,5.2,1.65,1.35,2.1,2.6)])
trunk("VegPRV1_EastUpperTrunk",18.7,-22.5,6.9,6.2,.34,.12,-.12)
canopy_cluster("VegPRV1_EastUpperMass",[
 (18.8,-22.5,11.8,2.15,1.65,2.8,1.4),(20.2,-22.9,10.8,1.65,1.35,2.2,2.3),(17.3,-22.2,10.5,1.55,1.3,2.0,.3)])
trunk("VegPRV1_EastForeTrunk",25.0,-30.0,1.1,5.6,.32,-.20,.10)
canopy_cluster("VegPRV1_EastForeMass",[
 (25.0,-30.0,5.6,2.15,1.75,2.65,2.0),(23.6,-29.6,4.9,1.55,1.35,2.0,.4),(26.5,-30.3,4.8,1.60,1.30,1.95,1.2)])

# Save source before export-origin transform.
blend=OUT/"GoldenPrimaryProfessionalReconstructionV1.blend"
bpy.ops.wm.save_as_mainfile(filepath=str(blend))

for o in bpy.context.scene.objects:
    if o.parent is None and o.type not in {"LIGHT","CAMERA"}: o.location-=ORIGIN
bpy.ops.object.select_all(action="DESELECT")
all_mesh=[o for o in bpy.context.scene.objects if o.type=="MESH"]
for o in all_mesh: o.select_set(True)
glb=OUT/"GoldenPrimaryProfessionalReconstructionV1.glb"
bpy.ops.export_scene.gltf(filepath=str(glb),export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT",export_normals=True,export_tangents=True)
for o in bpy.context.scene.objects:
    if o.parent is None and o.type not in {"LIGHT","CAMERA"}: o.location+=ORIGIN
bpy.context.view_layer.update()

# ---------------------------- evidence rendering ----------------------------
scene=bpy.context.scene
scene.render.engine="BLENDER_EEVEE"
scene.render.resolution_x=1536; scene.render.resolution_y=864; scene.render.resolution_percentage=100
scene.render.image_settings.file_format="PNG"
world=bpy.data.worlds.new("PRV1 World"); world.use_nodes=True
world.node_tree.nodes["Background"].inputs[0].default_value=(.36,.39,.41,1)
world.node_tree.nodes["Background"].inputs[1].default_value=.74
scene.world=world
center=Vector((4.45,-27.0,7.3))
for loc,energy,size,color in [
 ((-18,-46,34),2500,14,(1.0,.86,.69)),
 ((29,-18,23),1050,12,(.69,.80,1.0)),
 ((4,-24,38),680,11,(1.0,.95,.86))
]:
    bpy.ops.object.light_add(type="AREA",location=loc)
    l=bpy.context.object; l.data.energy=energy; l.data.size=size; l.data.color=color
    l.rotation_euler=(center-l.location).to_track_quat("-Z","Y").to_euler()

bpy.ops.object.camera_add(); cam=bpy.context.object; cam.data.type="ORTHO"; scene.camera=cam
clay=bpy.data.materials.new("PRV1 Clay"); clay.diffuse_color=(.60,.58,.54,1); clay.roughness=.94

views=[
 ("professional-clay-gate",(24,-56,30),39.0,True),
 ("professional-lit-review",(24,-56,30),39.0,False),
 ("professional-official-proxy",(24,-56,30),39.0,False),
 ("professional-contact-closeup",(13,-43,17),22.0,False)
]
previews=[]
for name,loc,span,isclay in views:
    cam.location=loc; cam.data.ortho_scale=span
    cam.rotation_euler=(center-cam.location).to_track_quat("-Z","Y").to_euler()
    scene.view_layers[0].material_override=clay if isclay else None
    p=EVID/(name+".png"); scene.render.filepath=str(p); bpy.ops.render.render(write_still=True)
    previews.append(str(p.relative_to(ROOT)).replace("\\","/"))
scene.view_layers[0].material_override=None

tris=verts=0; bounds=[]; mats=set()
for o in all_mesh:
    o.data.calc_loop_triangles(); tris+=len(o.data.loop_triangles); verts+=len(o.data.vertices)
    bounds += [o.matrix_world@Vector(c) for c in o.bound_box]
    for m in o.data.materials:
        if m: mats.add(m.name)
lo=[min(p[i] for p in bounds) for i in range(3)]
hi=[max(p[i] for p in bounds) for i in range(3)]

report={
 "authoring_standard":"BLENDER_PROFESSIONAL_V1",
 "workstream":"GOLDEN PRIMARY FORMS — PROFESSIONAL MESH RECONSTRUCTION v1",
 "method":"silhouette-first custom profile loops + direct vertex/edge authoring + tapered lofts + continuous geological surfaces + compound vegetation massing",
 "supersedes_method":"source02/source03 repeated primitive/additive primary-form method",
 "source":str(blend.relative_to(ROOT)).replace("\\","/"),
 "export":str(glb.relative_to(ROOT)).replace("\\","/"),
 "source_sha":sha(blend),"export_sha":sha(glb),
 "geometry_metrics":{"triangles":tris,"vertices":verts,"objects":len(all_mesh),"materials":sorted(mats),"bounds_world_blender":[lo,hi],"uv":True,"normals":True},
 "screen_space_changes":[
   "Lower Gate reduced to a few continuous tapered authored masses; tower bodies and crowns are custom profiles rather than stacked blocks.",
   "Portal is a deep continuous shell with explicit reveal/void hierarchy and non-rectangular crown silhouette.",
   "Bridge is a continuous variable-section structural body with camber, taper, integrated parapet profiles and buried geological receivers.",
   "Terrain banks run continuously from plateau through cliff strata to wet shelf with irregular cross-band offsets instead of flat platform + vertical face.",
   "Vegetation is composed as four asymmetrical canopy masses made from overlapping authored lobes rather than isolated decorative trees."
 ],
 "preview_evidence":previews,
 "canonical_reference":"references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg",
 "scope_guards":{"canonical_camera_changed":False,"macro_gate_position_changed":False,"macro_bridge_position_changed":False,"gameplay_changed":False,"road_stair_bastion_changed":False,"families_outside_golden_crop_changed":False,"colliders":0,"tripo_credits":0},
 "clay_gate":{"required_geometry_silhouette":4.0,"status":"PENDING HUMAN/AGENT VISUAL REVIEW"},
 "visual_gate":{"required":4.0,"geometry_silhouette":"PENDING","material_response":"PENDING","contact_integration":"PENDING","environment_coherence":"PENDING","premium_perception":"PENDING"},
 "unity_integration":"PROHIBITED UNTIL ALL VISUAL METRICS >=4.0"
}
(EVID/"source-report.json").write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8")

analysis="""# Golden Primary Forms — Professional Reconstruction v1 — Reference Analysis

Canonical target: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`.

Primary gap drivers identified before authoring:
1. Source03 silhouette is assembled from many small rectilinear components; target reads in fewer, stronger, sculptural masses.
2. Gate towers lack meaningful taper and crown hierarchy at gameplay scale.
3. Gate reads as an object placed on terrain, not a fortification grown into the rock/access system.
4. Bridge reads as a discrete placed asset; target requires bridge, apron, abutment and cliff to read as one load path.
5. Terrain has long planar shelves and abrupt cut faces instead of hierarchical geological planes/ledges.
6. Shore transition is too diagrammatic; target requires cliff-to-toe-to-wet-shelf continuity.
7. Vegetation is object-count driven and sparse; target uses grouped canopy masses to control screen-space density.
8. Contact transitions are secondary patches in source03; target makes them part of the primary geometry.
9. Depth is too dependent on lighting; target obtains depth first from overlapping primary forms.
10. Premium perception is limited by procedural repetition before material quality is considered.

Color/grading is intentionally excluded from this diagnosis because primary form remains the dominant failure class.
"""
(EVID/"REFERENCE_ANALYSIS.md").write_text(analysis,encoding="utf-8")

diagnosis="""# Source03 Screen-Space Diagnosis

Dominant masses in the official proxy:
- Lower Gate towers + portal crown: primary focal mass.
- Bridge deck/body: foreground directional mass.
- West/east cliff lips: largest continuous horizontal masses.
- Four vegetation zones: edge framing and depth separators.

Geometry removed/reconstructed:
- stacked tower-helper sections as the visible language,
- repeated cube merlons/corbels/front piers as silhouette drivers,
- strip-like bridge spandrels/parapet segments,
- planar plateau + discrete vertical cliff-face logic,
- scattered single-tree/shrub filler.

Professional reconstruction rule:
each dominant screen-space mass must be understandable in clay before materials are allowed to carry the read.
"""
(EVID/"SCREEN_SPACE_DIAGNOSIS.md").write_text(diagnosis,encoding="utf-8")
print(json.dumps(report,indent=2))
