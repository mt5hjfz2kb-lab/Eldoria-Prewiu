import bpy, math, json, hashlib, os
from pathlib import Path
from mathutils import Vector

ROOT=Path(os.environ.get("GITHUB_WORKSPACE",os.getcwd()))
SRC=ROOT/"art-source/valoria/lookdev/golden-slice-v1/primary-forms-v1/source03"
EVID=ROOT/"docs/evidence/valoria-golden-lookdev-slice-v1/primary-forms-v1/source03"
SURF=ROOT/"art-source/valoria/lookdev/golden-slice-v1/surface-v2"
SRC.mkdir(parents=True,exist_ok=True); EVID.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
ORIGIN=Vector((4.0,-22.0,7.0)); OBJS=[]

def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def mat(name,base,rough,prefix=None):
    m=bpy.data.materials.new(name); m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*base,1); bs.inputs["Roughness"].default_value=rough
    if prefix:
        ap=SURF/f"{prefix}_albedo.png"; np=SURF/f"{prefix}_normal.png"; sp=SURF/f"{prefix}_smoothness.png"
        if ap.exists():
            im=bpy.data.images.load(str(ap),check_existing=True); t=m.node_tree.nodes.new("ShaderNodeTexImage");t.image=im
            m.node_tree.links.new(t.outputs["Color"],bs.inputs["Base Color"])
        if np.exists():
            im=bpy.data.images.load(str(np),check_existing=True);im.colorspace_settings.name="Non-Color"
            t=m.node_tree.nodes.new("ShaderNodeTexImage");t.image=im;n=m.node_tree.nodes.new("ShaderNodeNormalMap");n.inputs["Strength"].default_value=.30
            m.node_tree.links.new(t.outputs["Color"],n.inputs["Color"]);m.node_tree.links.new(n.outputs["Normal"],bs.inputs["Normal"])
        if sp.exists():
            im=bpy.data.images.load(str(sp),check_existing=True);im.colorspace_settings.name="Non-Color"
            t=m.node_tree.nodes.new("ShaderNodeTexImage");t.image=im;inv=m.node_tree.nodes.new("ShaderNodeMath");inv.operation="SUBTRACT";inv.inputs[0].default_value=1
            m.node_tree.links.new(t.outputs["Color"],inv.inputs[1]);m.node_tree.links.new(inv.outputs[0],bs.inputs["Roughness"])
    return m

STONE=mat("S03 Warm Limestone",(.66,.57,.45),.72,"stone")
CUT=mat("S03 Cut Limestone",(.75,.66,.51),.66,"stone")
ROCK=mat("S03 Cliff Rock",(.39,.39,.37),.86,"rock")
EARTH=mat("S03 Plateau Earth",(.43,.35,.25),.91,"ground")
WET=mat("S03 Wet Shelf",(.27,.29,.28),.55,"shore")
LEAF=mat("S03 Canopy",(.19,.31,.18),.83,"vegetation")
BARK=mat("S03 Bark",(.18,.11,.07),.91,None)
IRON=mat("S03 Iron",(.055,.055,.05),.46,None)
BLUE=mat("S03 Heraldry",(.055,.16,.29),.63,None)
DARK=mat("S03 Portal Shadow",(.018,.022,.022),.95,None)

def uv(o,scale=3.0):
    if o.type!="MESH" or len(o.data.uv_layers): return
    u=o.data.uv_layers.new(name="MetricUV");o.data.uv_layers.active=u;u.active_render=True
    for p in o.data.polygons:
        axis=max(range(3),key=lambda k:abs(p.normal[k]))
        for li in p.loop_indices:
            v=o.data.vertices[o.data.loops[li].vertex_index].co
            u.data[li].uv=((v.y/scale,v.z/scale) if axis==0 else (v.x/scale,v.z/scale) if axis==1 else (v.x/scale,v.y/scale))
def mesh(name,verts,faces,material,smooth=False):
    me=bpy.data.meshes.new(name+"_Mesh");me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(material);uv(o)
    for p in o.data.polygons:p.use_smooth=smooth
    OBJS.append(o);return o
def cube(name,loc,scale,material,rot=0,bevel=.0):
    bpy.ops.mesh.primitive_cube_add(location=loc,scale=scale);o=bpy.context.object;o.name=name;o.rotation_euler[2]=rot;o.data.materials.append(material)
    if bevel>0:
        b=o.modifiers.new("EdgeSoftness","BEVEL");b.width=bevel;b.segments=2
    OBJS.append(o);return o
def tower(name,cx,cy,z0,sections,rot=0):
    verts=[];rings=[]
    for z,hx,hy,c in sections:
        pts=[(-hx+c,-hy),(hx-c,-hy),(hx,-hy+c),(hx,hy-c),(hx-c,hy),(-hx+c,hy),(-hx,hy-c),(-hx,-hy+c)]
        rr=[]
        for x,y in pts:
            co=Vector((x,y,0));co.rotate(__import__("mathutils").Matrix.Rotation(rot,4,"Z"))
            rr.append(len(verts));verts.append((cx+co.x,cy+co.y,z0+z))
        rings.append(rr)
    faces=[]
    for a,b in zip(rings[:-1],rings[1:]):
        for i in range(8):faces.append((a[i],a[(i+1)%8],b[(i+1)%8],b[i]))
    faces.append(tuple(reversed(rings[0])));faces.append(tuple(rings[-1]))
    return mesh(name,verts,faces,STONE,False)

def gate_arch(name,cx,cy,zs,rx,rz,depth,thick,material,segments=40):
    # arch in X/Z, depth in Y
    verts=[];faces=[];n=segments+1
    for y in (cy-depth/2,cy+depth/2):
        for outer in (0,1):
            for i in range(n):
                t=math.pi*i/segments;rxx=rx+outer*thick;rzz=rz+outer*thick
                verts.append((cx-rxx*math.cos(t),y,zs+rzz*math.sin(t)))
    for side in range(2):
        off=side*2*n
        for i in range(segments):faces.append((off+i,off+i+1,off+n+i+1,off+n+i))
    for ring in range(2):
        a=ring*n;b=2*n+ring*n
        for i in range(segments):faces.append((a+i,b+i,b+i+1,a+i+1))
    faces += [(0,n,3*n,2*n),(segments,n+segments,3*n+segments,2*n+segments)]
    return mesh(name,verts,faces,material,False)

def bridge_arch(name,cx,cy,zs,ry,rz,depth_x,thick,material,segments=40):
    # correct bridge load arch: arch spans Y/Z and has depth across X
    verts=[];faces=[];n=segments+1
    for x in (cx-depth_x/2,cx+depth_x/2):
        for outer in (0,1):
            for i in range(n):
                t=math.pi*i/segments; ryy=ry+outer*thick;rzz=rz+outer*thick
                verts.append((x,cy-ryy*math.cos(t),zs+rzz*math.sin(t)))
    for side in range(2):
        off=side*2*n
        for i in range(segments):faces.append((off+i,off+i+1,off+n+i+1,off+n+i))
    for ring in range(2):
        a=ring*n;b=2*n+ring*n
        for i in range(segments):faces.append((a+i,b+i,b+i+1,a+i+1))
    faces += [(0,n,3*n,2*n),(segments,n+segments,3*n+segments,2*n+segments)]
    return mesh(name,verts,faces,material,False)

def arch_fill(cx,cy,zs,rx,rz):
    # dark recessed portal shape behind the stone ring
    verts=[(cx,cy,zs)]
    for i in range(33):
        t=math.pi*i/32;verts.append((cx-rx*math.cos(t),cy,zs+rz*math.sin(t)))
    faces=[]
    for i in range(1,33):faces.append((0,i,i+1))
    # lower rectangular extension
    base=len(verts);verts += [(cx-rx,cy,6.85),(cx+rx,cy,6.85),(cx+rx,cy,zs),(cx-rx,cy,zs)]
    faces.append((base,base+1,base+2,base+3))
    return mesh("Gate_RecessedPortal",verts,faces,DARK,False)

# ---------------------------------------------------------------------------
# LOWER GATE source03: real void-first gatehouse, articulated towers, no box curtain.
# ---------------------------------------------------------------------------
gy=-25.35
tower("GateS03_WestTower",-0.15,gy,6.62,[(0,3.35,2.30,.52),(.55,3.05,2.08,.44),(1.15,2.78,1.90,.38),(6.15,2.65,1.80,.36),(6.65,2.92,1.98,.42),(7.45,3.10,2.08,.46),(8.05,2.82,1.90,.40)],math.radians(-1.5))
tower("GateS03_EastTower",9.30,gy+.14,6.58,[(0,3.50,2.34,.54),(.62,3.14,2.10,.45),(1.22,2.82,1.92,.38),(5.95,2.70,1.82,.36),(6.48,3.00,2.02,.44),(7.30,3.18,2.12,.47),(7.92,2.88,1.94,.41)],math.radians(1.2))

# Deep actual gateway; front ring + second inner ring + shadow void make access depth read.
gate_arch("GateS03_OuterArch",4.57,gy-1.18,9.15,2.85,3.42,.58,.52,CUT,42)
gate_arch("GateS03_InnerArch",4.57,gy+.55,9.15,2.55,3.12,.38,.34,STONE,38)
arch_fill(4.57,gy+1.00,9.15,2.44,3.02)

# Stone above arch only, leaving the void dominant.
cube("GateS03_ArchCrownBeam",(4.57,gy-.04,13.17),(2.86,1.26,.64),STONE,0,.08)
for x in [2.10,3.70,5.32,6.92]:
    cube("GateS03_CentralMerlon",(x,gy-.04,14.08),(.47,1.28,.55),CUT,0,.06)

# Tower architectural hierarchy: projecting string courses, buttresses, sparse corbels and broad merlons.
for side,(cx,cy,sgn) in enumerate([(-.15,gy,-1),(9.30,gy+.14,1)]):
    cube(f"GateS03_TowerBandA_{side}",(cx,cy,8.12),(2.95,2.05,.16),CUT,0,.04)
    cube(f"GateS03_TowerBandB_{side}",(cx,cy,12.68),(2.86,1.94,.18),CUT,0,.04)
    # front outside buttress gives vertical load language
    bx=cx+sgn*2.40
    tower(f"GateS03_Buttress_{side}",bx,cy-1.58,6.55,[(0,.72,.58,.12),(.52,.62,.52,.10),(4.70,.40,.42,.08),(5.35,.28,.34,.06)],0)
    for k in range(4):
        x=cx-1.65+k*1.10
        cube(f"GateS03_Corbel_{side}_{k}",(x,cy-2.00,13.30),(.34,.28,.28),CUT,0,.05)
    # broad crown crenels, no tiny repeated teeth
    for k,(dx,dy) in enumerate([(-1.65,-1.55),(0,-1.70),(1.65,-1.55),(-1.65,1.50),(1.65,1.50)]):
        cube(f"GateS03_Merlon_{side}_{k}",(cx+dx,cy+dy,14.72),(.48,.40,.60),CUT,0,.07)

# Stepped wings embed into plateau, creating descending silhouette.
for side in (-1,1):
    if side<0:
        segs=[(-4.6,-25.05,9.15,1.55),(-7.1,-24.70,8.55,1.25),(-9.15,-24.25,7.95,1.05)]
    else:
        segs=[(13.7,-24.88,9.05,1.55),(16.0,-24.55,8.45,1.25),(18.0,-24.15,7.85,1.05)]
    prev=None
    for i,(x,y,z,hx) in enumerate(segs):
        cube(f"GateS03_Wing_{side}_{i}",(x,y,z),(hx,.88,1.18),STONE,math.radians(side*2),.05)

# Portcullis and two broad banners.
for i in range(6):
    cube(f"GateS03_GridV_{i}",(2.45+i*.84,gy+.84,9.1),(.045,.05,2.05),IRON)
for x,z,h,w in [(-.35,11.10,2.45,.60),(9.55,10.95,2.15,.56)]:
    verts=[(x-w,gy-2.13,z+h/2),(x+w,gy-2.13,z+h/2),(x+w*.70,gy-2.13,z-h/2),(x,gy-2.13,z-h/2-.40),(x-w*.70,gy-2.13,z-h/2)]
    mesh("GateS03_Banner",verts,[(0,1,2,3,4)],BLUE)

# ---------------------------------------------------------------------------
# BRIDGE source03: correct longitudinal arch/load path + rising deck.
# ---------------------------------------------------------------------------
ys=[-34.2,-33.0,-31.7,-30.3,-28.9,-27.6,-26.65]
verts=[];faces=[]
for j,y in enumerate(ys):
    t=j/(len(ys)-1);z=3.15+3.68*t+.15*math.sin(math.pi*t);half=3.55-.08*math.sin(math.pi*t)
    verts += [(4.55-half,y,z),(4.55+half,y,z)]
for j in range(len(ys)-1):
    a=j*2;b=a+2;faces.append((a,a+1,b+1,b))
mesh("BridgeS03_DeckTop",verts,faces,STONE)

# Thick side edge/spandrels following rise; side silhouette is coherent.
for side in (-1,1):
    x=4.55+side*3.55
    vv=[]
    for j,y in enumerate(ys):
        t=j/(len(ys)-1);zt=3.15+3.68*t+.15*math.sin(math.pi*t)
        vv += [(x,y,zt-.65),(x,y,zt+.10)]
    ff=[]
    for j in range(len(ys)-1):
        a=j*2;b=a+2;ff.append((a,b,b+1,a+1))
    mesh(f"BridgeS03_Spandrel_{side}",vv,ff,STONE)
    # parapet follows grade
    for j in range(len(ys)-1):
        y=(ys[j]+ys[j+1])/2;t=(j+.5)/(len(ys)-1);z=3.15+3.68*t+.15*math.sin(math.pi*t)
        cube(f"BridgeS03_Parapet_{side}_{j}",(x,y,z+.48),(.20,(ys[j]-ys[j+1])*.48,.43),CUT,0,.05)

# One structural barrel arch spanning along bridge direction, visible on both sides.
bridge_arch("BridgeS03_LoadArch",4.55,-30.55,1.45,2.38,2.42,7.55,.46,CUT,42)
# front and rear buried receivers, modest not giant blocks
for y,z,h in [(-34.0,2.55,1.70),(-26.85,6.10,1.35)]:
    for x in (1.18,7.92):
        tower("BridgeS03_Receiver",x,y,z-h,[(0,.72,.92,.14),(h*.55,.61,.82,.12),(h,.50,.70,.10)],0)

# ---------------------------------------------------------------------------
# TERRAIN source03: plateau + real cliff face + wet shelf. No sigmoid sheet.
# ---------------------------------------------------------------------------
def terrain_side(name,x0,x1,side):
    xs=np=[x0+(x1-x0)*i/7 for i in range(8)]
    # irregular terrace front line, same macro zone; plateau behind it is nearly horizontal.
    front=[-28.10 + .28*math.sin(i*.88 + (0 if side<0 else .7)) for i in range(8)]
    # top surface, four Y rows, sparse large quads
    yr=[-18.4,-21.9,-25.0,-27.95]
    verts=[]
    for j,y in enumerate(yr):
        for i,x in enumerate(xs):
            yy=y if j<3 else front[i]
            z=6.95 + .10*math.sin(i*.73+j*.57) + .06*math.sin(x*.21)
            verts.append((x,yy,z))
    faces=[]
    cols=8
    for j in range(3):
        for i in range(7):
            a=j*cols+i;b=a+1;c=a+cols;d=c+1
            faces += [(a,c,d),(a,d,b)] if (i+j)%2==0 else [(a,c,b),(b,c,d)]
    top=mesh(name+"_Plateau",verts,faces,EARTH,False)
    # cliff cross-section rows: top -> ledge -> fractured mid -> toe
    bands=[(0.0,6.92),(-.38,5.72),(-.86,4.25),(-1.28,2.70),(-1.72,1.35)]
    cv=[];rows=[]
    for bi,(dy,zbase) in enumerate(bands):
        row=[]
        for i,x in enumerate(xs):
            y=front[i]+dy + .18*math.sin(i*1.23+bi*.91+side)
            z=zbase + .25*math.sin(i*.67+bi*.51+side*.6)
            # large strata offsets in x, not noise
            xx=x + (.20*math.sin(i*.93+bi*.42) if 0<i<7 else 0)
            row.append(len(cv));cv.append((xx,y,z))
        rows.append(row)
    cf=[]
    for bi in range(len(rows)-1):
        for i in range(7):
            a=rows[bi][i];b=rows[bi][i+1];c=rows[bi+1][i];d=rows[bi+1][i+1]
            cf += [(a,c,d),(a,d,b)] if (i+bi)%2==0 else [(a,c,b),(b,c,d)]
    cliff=mesh(name+"_Cliff",cv,cf,ROCK,False)
    # shore shelf from toe to water edge, low and irregular but connected to cliff.
    sv=[];sf=[]
    for row_i,(ddy,z) in enumerate([(0,1.30),(-1.45,.62),(-3.55,.20)]):
        for i,x in enumerate(xs):
            sv.append((x,front[i]-1.72+ddy+.15*math.sin(i*.7+row_i),z+.08*math.sin(i*.8+row_i)))
    for r in range(2):
        for i in range(7):
            a=r*8+i;b=a+1;c=a+8;d=c+1
            sf += [(a,c,d),(a,d,b)]
    sh=mesh(name+"_WetShelf",sv,sf,WET,False)
    return top,cliff,sh

terrain_side("TerrainS03_West",-20.5,.72,-1)
terrain_side("TerrainS03_East",8.40,29.0,1)

# A connected small central landing receives bridge at foreground; not an island.
lv=[(-1.0,-35.4,.18),(10.0,-35.4,.18),(9.0,-33.1,1.05),(.0,-33.1,1.05)]
mesh("TerrainS03_ForegroundLanding",lv,[(0,1,2,3)],WET)

# ---------------------------------------------------------------------------
# VEGETATION source03: richer authored canopy hierarchy and denser frame read.
# ---------------------------------------------------------------------------
def conifer(name,x,y,z,h,r,phase):
    cube(name+"_Trunk",(x,y,z+h*.31),(r*.10,r*.09,h*.31),BARK,phase*.04,.03)
    seg=10
    rings=[(.34,1.00),(.46,.92),(.59,.77),(.71,.61),(.82,.44),(.91,.27),(.98,.07)]
    vv=[];rr=[]
    for ri,(zf,rf) in enumerate(rings):
        row=[]
        for i in range(seg):
            a=2*math.pi*i/seg+phase+ri*.09
            irregular=1+.12*math.sin(i*2.17+ri*.83+phase)+.06*math.sin(i*3.1-ri*.47)
            rad=r*rf*irregular
            ox=.13*r*math.sin(ri*.95+phase);oy=.09*r*math.cos(ri*.77+phase)
            row.append(len(vv));vv.append((x+ox+math.cos(a)*rad,y+oy+math.sin(a)*rad,z+h*zf))
        rr.append(row)
    ff=[]
    for a,b in zip(rr[:-1],rr[1:]):
        for i in range(seg):ff.append((a[i],a[(i+1)%seg],b[(i+1)%seg],b[i]))
    ff.append(tuple(reversed(rr[0])));ff.append(tuple(rr[-1]))
    mesh(name+"_Canopy",vv,ff,LEAF,False)

conifer("VegS03_WestUpper",-7.5,-21.0,7.0,7.8,1.85,.4)
conifer("VegS03_WestFore",-12.0,-30.4,1.25,8.6,2.05,1.45)
conifer("VegS03_EastUpper",20.8,-24.0,6.95,8.3,1.92,2.5)


# ---------------------------------------------------------------------------
# SOURCE03 MATERIAL CHANGE: richer architectural articulation + embedded contacts.
# ---------------------------------------------------------------------------

# Gate arch voussoir rhythm: large authored wedge-like blocks around the primary void.
for side in (-1, 1):
    for i in range(7):
        a = math.radians(22 + i*21)
        x = 4.57 + math.cos(a) * 3.08 * side
        z = 9.15 + math.sin(a) * 3.62
        cube(f"GateS03_Voussoir_{side}_{i}", (x, gy-1.52, z), (.42,.38,.32), CUT, math.radians(side*(8+i*2)), .06)

# Stronger tower crown support and vertical hierarchy.
for side,(cx,cy) in enumerate([(-.15,gy),(9.30,gy+.14)]):
    for k,dx in enumerate([-1.65,-.55,.55,1.65]):
        cube(f"GateS03_Machicolation_{side}_{k}", (cx+dx,cy-2.02,13.62), (.32,.44,.26), CUT, 0, .05)
    for k,dx in enumerate([-1.95,1.95]):
        cube(f"GateS03_FrontPier_{side}_{k}", (cx+dx,cy-1.76,9.75), (.38,.34,2.55), STONE, 0, .05)

# Close the visual gap between bridge and gate with a real receiving apron.
apron_verts=[
    (1.10,-27.55,6.65),(8.05,-27.55,6.65),
    (8.28,-25.95,6.88),(0.88,-25.95,6.88),
    (1.52,-29.25,5.68),(7.64,-29.25,5.68)
]
apron_faces=[(0,1,2,3),(4,5,1,0),(4,0,3),(5,2,1),(4,3,2,5)]
mesh("GateS03_ReceivingApron",apron_verts,apron_faces,STONE,False)

# Irregular cliff shoulder rocks break the long planar top edge and embed walls.
for side,xs in [(-1,[-18.0,-14.7,-10.9,-7.6]),(1,[12.4,16.1,20.3,24.6])]:
    for i,x in enumerate(xs):
        y=-27.7 + .35*math.sin(i*1.7+side)
        z=6.65 + .25*math.sin(i*.9)
        tower(f"TerrainS03_ShoulderRock_{side}_{i}",x,y,z-.55,
              [(0,.78,.62,.16),(.55,.68,.55,.14),(1.25,.48,.42,.12),(1.65,.26,.30,.08)],
              math.radians((i-1.5)*8))

# Shore/water read: a bounded water plane beneath the bridge and wet shelf.
WATER=mat("S03 Water",(.055,.16,.18),.24,None)
water_verts=[(-21.5,-39.0,.08),(30.0,-39.0,.08),(29.0,-29.2,.08),(-20.5,-29.2,.08)]
mesh("TerrainS03_Water",water_verts,[(0,1,2,3)],WATER,False)

# More vegetation with varied height/radius and asymmetrical spacing.
conifer("VegS03_WestMid",-16.0,-25.8,3.15,6.2,1.45,3.2)
conifer("VegS03_WestGate",-4.9,-20.4,7.0,5.9,1.35,4.0)
conifer("VegS03_EastGate",14.9,-21.3,7.0,6.5,1.50,5.1)
conifer("VegS03_EastFore",25.3,-30.1,1.10,7.3,1.72,5.8)

# Low shrub masses at wall/cliff contacts to soften hard seams without hiding forms.
def shrub(name,x,y,z,r,h,phase):
    seg=9; verts=[]; rings=[]
    for ri,(zf,rf) in enumerate([(0,.72),(.45,1.0),(.82,.66),(1.0,.18)]):
        row=[]
        for i in range(seg):
            a=2*math.pi*i/seg + phase + ri*.13
            rr=r*rf*(1+.15*math.sin(i*1.9+phase+ri))
            row.append(len(verts)); verts.append((x+math.cos(a)*rr,y+math.sin(a)*rr,z+h*zf))
        rings.append(row)
    faces=[]
    for a,b in zip(rings[:-1],rings[1:]):
        for i in range(seg): faces.append((a[i],a[(i+1)%seg],b[(i+1)%seg],b[i]))
    faces.append(tuple(reversed(rings[0]))); faces.append(tuple(rings[-1]))
    mesh(name,verts,faces,LEAF,False)

for i,(x,y,z,r,h) in enumerate([
    (-8.8,-26.9,6.6,1.05,1.45),(-5.7,-26.7,6.65,.88,1.25),
    (13.9,-26.6,6.65,.95,1.35),(17.2,-26.8,6.55,1.10,1.55),
    (-12.7,-31.0,1.25,.92,1.28),(21.7,-30.4,1.15,1.0,1.4)
]):
    shrub(f"VegS03_Shrub_{i}",x,y,z,r,h,i*.57)


# Apply curve-free source; save/export.
blend=SRC/"GoldenPrimaryFormsV1_source03.blend";bpy.ops.wm.save_as_mainfile(filepath=str(blend))
for o in bpy.context.scene.objects:
    if o.parent is None and o.type not in {"LIGHT","CAMERA"}:o.location-=ORIGIN
bpy.ops.object.select_all(action="DESELECT")
all_mesh=[o for o in bpy.context.scene.objects if o.type=="MESH"]
for o in all_mesh:o.select_set(True)
glb=SRC/"GoldenPrimaryFormsV1_source03.glb"
bpy.ops.export_scene.gltf(filepath=str(glb),export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT",export_normals=True,export_tangents=True)
for o in bpy.context.scene.objects:
    if o.parent is None and o.type not in {"LIGHT","CAMERA"}:o.location+=ORIGIN
bpy.context.view_layer.update()

# Geometry-first review lighting.
scene=bpy.context.scene;scene.render.engine="BLENDER_EEVEE";scene.render.resolution_x=1536;scene.render.resolution_y=1024;scene.render.resolution_percentage=100
world=bpy.data.worlds.new("S03 World");world.use_nodes=True;world.node_tree.nodes["Background"].inputs[0].default_value=(.34,.37,.40,1);world.node_tree.nodes["Background"].inputs[1].default_value=.78;scene.world=world
center=Vector((4.5,-27.0,7.0))
for loc,energy,size,color in [((-18,-46,34),2800,13,(1,.86,.70)),((28,-17,23),1250,12,(.70,.82,1)),((4,-24,38),760,10,(1,.96,.88))]:
    bpy.ops.object.light_add(type="AREA",location=loc);l=bpy.context.object;l.data.energy=energy;l.data.size=size;l.data.color=color;l.rotation_euler=(center-l.location).to_track_quat("-Z","Y").to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type="ORTHO";scene.camera=cam
clay=bpy.data.materials.new("S03 Clay");clay.diffuse_color=(.59,.57,.53,1);clay.roughness=.93
views=[
 ("source03-clay-silhouette",(31,-55,31),41,True),
 ("source03-lit-three-quarter",(31,-55,31),41,False),
 ("source03-official-proxy",(24,-56,30),39,False)
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
 "source_iteration":"source03",
 "method_change_from_source02":[
   "Gate rebuilt around a visible deep void; rectangular upper curtain removed; tower load hierarchy, buttresses, corbels, stepped wings added.",
   "Bridge load arch rotated into correct longitudinal Y/Z span; rising deck and continuous side spandrels replace block-like support read.",
   "Terrain method replaced: flat plateau + explicit multi-band cliff + connected wet shelf replaces broad sigmoid sheet.",
   "Vegetation method replaced: one continuous asymmetrical tapered canopy mesh per tree replaces stacked icosphere masses."
 ],
 "source":str(blend.relative_to(ROOT)).replace("\\","/"),"export":str(glb.relative_to(ROOT)).replace("\\","/"),
 "source_sha":sha(blend),"export_sha":sha(glb),
 "geometry_metrics":{"triangles":tris,"vertices":verts,"objects":len(all_mesh),"materials":sorted(mats),"bounds_world_blender":[lo,hi],"uv":True,"normals":True},
 "preview_evidence":previews,
 "canonical_reference":"references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg",
 "scope_guards":{"canonical_camera_changed":False,"macro_gate_position_changed":False,"macro_bridge_position_changed":False,"gameplay_changed":False,"road_stair_bastion_changed":False,"colliders":0,"tripo_credits":0},
 "visual_gate":{"required":4.0,"geometry_silhouette":"PENDING VISUAL REVIEW","material_response":"PENDING VISUAL REVIEW","contact_integration":"PENDING VISUAL REVIEW","environment_coherence":"PENDING VISUAL REVIEW","premium_perception":"PENDING VISUAL REVIEW"},
 "unity_integration":"PROHIBITED UNTIL EXPLICIT SOURCE VISUAL PASS"
}
(EVID/"source-report.json").write_text(json.dumps(report,indent=2)+"\n",encoding="utf-8")
print(json.dumps(report,indent=2))
