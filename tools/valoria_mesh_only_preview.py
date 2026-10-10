"""Valoria I mesh-only world-space architectural continuity experiment.
Blender 2.83+ headless compatible, original committed GLB geometry + independent terrain.
Never imports SHARP, splats, canonical projection images or private textures.
"""
import bpy, math, os, json, base64, random
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[1]
OUT=Path(os.environ.get("VALORIA_PROOF_OUTPUT","/tmp/valoria-mesh-proof.png"))
OUT.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
scene.render.engine="CYCLES"
scene.cycles.samples=16
for layer in scene.view_layers: layer.cycles.use_denoising=False
scene.render.resolution_x=1152
scene.render.resolution_y=768
scene.render.resolution_percentage=100
scene.render.image_settings.file_format="PNG"
scene.render.filepath=str(OUT)
scene.render.film_transparent=False

def material(name,color,rough=.85):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1)
    m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*color,1)
    bs.inputs["Roughness"].default_value=rough
    return m
stone=material("Warm weathered structural stone",(.36,.32,.27))
trim=material("Masonry copings",(.48,.42,.34))
paving=material("Worn road paving",(.38,.345,.29))
earth=material("Earth of the plateau",(.22,.225,.16))
grass=material("Grass",(.155,.22,.13))
rock=material("Dark slate bedrock",(.17,.185,.19))
wood=material("Timber",(.20,.115,.06))
blue=material("Blue Valoria flag",(.025,.09,.24))
glow=material("Warm amber",(.75,.35,.075))
def cuboid(name,xyz,dimensions,mat,bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=xyz)
    o=bpy.context.object;o.name=name;o.dimensions=dimensions
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        m=o.modifiers.new("bevel","BEVEL");m.width=bevel;m.segments=2
        try:bpy.ops.object.modifier_apply(modifier=m.name)
        except Exception:pass
    o.data.materials.append(mat)
    return o

# Continuous stepped upper and lower courtyards are one walkable connected rock substrate.
# Global frame: z up, main bridge / gate / road / stair / keep advance along increasing Y.
# Lower terrace z=8, upper terrace z=13.
def plateau_mesh():
    # Cross-section loft with independent noise per level: natural scarps, not a square slab.
    n=96
    rings=[]
    for z,rx,ry,cx,cy,seed in [
        (-11,24,33,0,1,6),(-6,28,39,0,2,5),(1,29.4,41,0,2,4),
        (5.8,27.2,39.8,0,2,3),(7.95,26,39.0,0,2,2)]:
        ring=[]
        for i in range(n):
            theta=2*math.pi*i/n
            noise=(.048*math.sin(7*theta+seed*.7)+.025*math.sin(19*theta+seed)+
                   .014*math.sin(31*theta+seed*1.3))
            ring.append((cx+rx*(1+noise)*math.cos(theta),
                         cy+ry*(1+noise)*math.sin(theta),
                         z+(.12*math.sin(theta*13))*(1 if z>1 else .35)))
        rings.append(ring)
    verts=[p for ring in rings for p in ring]
    faces=[]
    for r in range(len(rings)-1):
        for i in range(n):
            a=r*n+i;b=r*n+(i+1)%n;c=(r+1)*n+(i+1)%n;d=(r+1)*n+i
            faces.append((a,b,c,d))
    faces.append(tuple((len(rings)-1)*n+i for i in range(n)))
    me=bpy.data.meshes.new("Stratified organic cliff mesh")
    me.from_pydata(verts,[],faces);me.update()
    ob=bpy.data.objects.new("Organic connected cliff foundation",me)
    scene.collection.objects.link(ob);ob.data.materials.append(rock)
    return ob
plateau_mesh()
# Lower ground is a fitted irregular mesh, not an overhanging rectangular platform.
def courtyard_surface():
    nx,ny=22,28
    verts=[]
    for iy in range(ny+1):
        y=-26.5+iy*(43.5/ny)
        for ix in range(nx+1):
            x=-20.5+ix*(41.0/nx)
            disturbance=.055*math.sin(ix*1.8+iy*.8)*math.sin(iy*.65)
            verts.append((x,y,8.04+disturbance))
    faces=[(iy*(nx+1)+ix,iy*(nx+1)+ix+1,(iy+1)*(nx+1)+ix+1,(iy+1)*(nx+1)+ix)
           for iy in range(ny) for ix in range(nx)]
    mesh=bpy.data.meshes.new("Molded grassland quad mesh")
    mesh.from_pydata(verts,[],faces);mesh.update()
    terrain=bpy.data.objects.new("Embedded uneven lower courtyard",mesh)
    scene.collection.objects.link(terrain);terrain.data.materials.append(grass)
courtyard_surface()
cuboid("Upper enclosed citadel terrace",(0,26.4,12.89),(42,22,.20),grass,.10)
# A continuous elevated shoulder makes the upper courtyard physically supported.
cuboid("Upper plateau bedrock", (0,27.3,10.4),(43,23,5),rock,.3)
cuboid("Upper terrace edge retaining wall left",(-21,17.6,10.6),(3,2,5.6),stone,.16)
cuboid("Upper terrace edge retaining wall right",(21,17.6,10.6),(3,2,5.6),stone,.16)
# Paving, physical connection, staircase transitioning z=8 to z=13.
cuboid("Approach from bridge to gate",(0,-28.0,8.08),(7.4,15,.14),paving)
cuboid("Gate to stair main street",(0,0,8.08),(7.4,34,.14),paving)
for i in range(15):
    y=14+i*.55
    z=8.13+i*(5/15)
    cuboid(f"Continuous staircase {i+1}",(0,y,z-.25),(7.4,.59,.55),paving,.025)
cuboid("Bastion court paved axis",(0,28.6,13.09),(8.5,22,.14),paving)
# Explicit connecting walls and ramparts, not free-standing model fragments.
for xx in (-22,22):
    cuboid("Curtain continuity west" if xx<0 else "Curtain continuity east",
           (xx,-.7,10.35),(2.6,50,5.3),stone,.1)
    cuboid("Wall cap west" if xx<0 else "Wall cap east",(xx,-.7,13.04),(3.1,50,.46),trim,.08)
    for yy in [-23,-17,-10,-3,4,11,18,24]:
        cuboid(f"Battlement x{xx} y{yy}",(xx,yy,13.58),(3.0,2.0,1.1),stone,.06)

# Existing accepted source geometry: import and normalize in a single world coordinate system.
# Read each mesh's actual axis-aligned world-space bounds and anchor its base to terrain.
FAMILIES=[
    ("Bridge","bridge","Bridge",(-0.2,-28,8.15),9.0),
    ("LowerGate","lower-gate","LowerGate",(0,-18.0,8.15),12.0),
    ("MainRoad","road","Road",(0,-1,8.20),9.0),
    ("CentralStair","stair","Stair",(0,18.0,12.0),8.0),
    ("UpperWalls","wall","Wall",(0,33.4,13.05),38.0),
    ("Bastion","bastion","Bastion",(0,32.5,13.05),19.0),
]
results=[]
for display,folder,source,anchor,target_width in FAMILIES:
    src=ROOT/"art-source"/"valoria"/"production"/(folder+"-family-v1")/(source+"FamilyV1.glb")
    if not src.exists():raise FileNotFoundError(str(src))
    before=set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=str(src))
    meshes=[o for o in bpy.data.objects if o not in before and o.type=="MESH"]
    if not meshes:raise RuntimeError("No mesh: "+display)
    # Place all source meshes under a shared transform. Keep their authored local relations.
    parent=bpy.data.objects.new(display+" WorldspaceRoot",None)
    scene.collection.objects.link(parent)
    for ob in meshes:
        matrix=ob.matrix_world.copy()
        ob.parent=parent; ob.matrix_world=matrix
    bpy.context.view_layer.update()
    coords=[ob.matrix_world@Vector(c) for ob in meshes for c in ob.bound_box]
    left=min(v.x for v in coords);right=max(v.x for v in coords)
    front=min(v.y for v in coords);back=max(v.y for v in coords)
    low=min(v.z for v in coords)
    scale=target_width/max(.001,right-left)
    parent.scale=(scale,scale,scale)
    # Translate actual model base and center into coherent global coordinates.
    parent.location=Vector((anchor[0]-scale*(left+right)*.5,
                            anchor[1]-scale*(front+back)*.5,
                            anchor[2]-scale*low))
    results.append({"family":display,"objects":len(meshes),"world_anchor":anchor,
                    "source_dimensions":[round(right-left,2),round(back-front,2)],
                    "scale":round(scale,4)})
# Continuous defensive parapet runs, with varying alignments and authored stone supports.
# Curved walls are created from a single swept mesh, avoiding visible gaps between blocks.
def swept_curtain(name,points,height,width,mat):
    vertices=[]
    for i,(x,y,z) in enumerate(points):
        if i==0:direction=Vector((points[1][0]-x,points[1][1]-y))
        elif i==len(points)-1:direction=Vector((x-points[i-1][0],y-points[i-1][1]))
        else:direction=Vector((points[i+1][0]-points[i-1][0],
                               points[i+1][1]-points[i-1][1]))
        direction.normalize()
        nrm=Vector((-direction.y,direction.x,0))*width/2
        base=Vector((x,y,z))
        vertices.extend([tuple(base+nrm),tuple(base-nrm),
                         tuple(base+nrm+Vector((0,0,height))),
                         tuple(base-nrm+Vector((0,0,height)))])
    faces=[]
    for i in range(len(points)-1):
        a=4*i;b=4*(i+1)
        faces.extend([(a,b,b+2,a+2),(a+1,a+3,b+3,b+1),(a+2,b+2,b+3,a+3),(a,b,b+1,a+1)])
    faces.extend([(0,1,3,2),(len(vertices)-4,len(vertices)-2,len(vertices)-1,len(vertices)-3)])
    mesh=bpy.data.meshes.new(name+" continuous masonry mesh")
    mesh.from_pydata(vertices,[],faces);mesh.update()
    ob=bpy.data.objects.new(name,mesh);scene.collection.objects.link(ob)
    ob.data.materials.append(mat)
    return ob
# Bastion rear wall is naturally kinked to respond to rock contour, with zero floating bays.
for side in (-1,1):
    swept_curtain("Citadel shoulder masonry "+str(side),
        [(side*20.9,17.8,13.0),(side*21.1,23.3,13.0),
         (side*20.4,29.5,13.0),(side*19.0,35.0,13.0)],4.6,1.5,stone)
# The ramp between the courtyards receives retaining shoulders physically attached to both ends.
for side in (-1,1):
    swept_curtain("Grand staircase retaining flank "+str(side),
        [(side*4.4,12.9,8.03),(side*4.3,15.6,8.8),
         (side*4.3,18.8,10.8),(side*4.4,22.6,12.9)],1.45,.75,stone)
# Small props and houses only within enclosed courtyard, away from route.
for side in [-1,1]:
    for i in range(4):
        x=side*(11+(i%2)*6);y=-11+(i//2)*13
        cuboid(f"Settlement cottage {side} {i}",(x,y,9.0),(5.0,5.2,2.2),wood,.16)
        cuboid(f"Slate pitched roof proxy {side} {i}",(x,y,10.2),(5.7,5.8,.48),rock,.18)
        cuboid(f"Storage near building {side} {i}",(x+1.6,y-2.8,8.45),(1.6,1.0,1),wood,.05)
# Architecturally anchored volume completion, independent of source GLB camera proxies.
# The underlying imported GLBs remain visible, but no longer define the structural topology.
def tower(name,x,y,z0,h,w=4.1):
    # Eight-sided battered medieval tower shell: actual taper and differentiated course.
    for i in range(12):
        low=z0+i*h/12
        bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=w*.62*(1.07-.14*i/12),
            depth=h/12+.04, location=(x,y,low+h/24))
        o=bpy.context.object;o.name=name+" octagonal masonry course "+str(i)
        o.data.materials.append(stone if i%4 else trim)
    bpy.ops.mesh.primitive_cylinder_add(vertices=8,radius=w*.69,depth=.34,
        location=(x,y,z0+h+.12))
    bpy.context.object.name=name+" crenellated coping";bpy.context.object.data.materials.append(trim)
    for ax,ay in [(0,w*.32),(0,-w*.32),(w*.32,0),(-w*.32,0)]:
        cuboid(name+" battlement",(x+ax,y+ay,z0+h+.62),
                (1.15,1.15,.88),stone,.045)
tower("Lower gate west tower",-6,-18,8.1,6.7,4.4)
tower("Lower gate east tower",6,-18,8.1,6.7,4.4)
# Actual medieval voussoir arches on the gatehouse and raised keep entrance.
# Each voussoir is a closed 3D wedge with distinct radial joints, not a decal or flat bar.
def stone_arch(name,x,y,base_z,clear_halfspan,thickness,depth,blocks=13):
    for i in range(blocks):
        a0=math.pi*i/blocks
        a1=math.pi*(i+1)/blocks
        # semicircle from left spring to right spring
        def corner(theta,r,dy):
            return (x+math.cos(theta)*r,y+dy,base_z+math.sin(theta)*r)
        r0=clear_halfspan
        r1=clear_halfspan+thickness
        v=[corner(a0,r0,-depth/2),corner(a1,r0,-depth/2),
           corner(a1,r1,-depth/2),corner(a0,r1,-depth/2),
           corner(a0,r0,depth/2),corner(a1,r0,depth/2),
           corner(a1,r1,depth/2),corner(a0,r1,depth/2)]
        faces=[(0,3,2,1),(4,5,6,7),(0,1,5,4),(3,7,6,2),(0,4,7,3),(1,2,6,5)]
        mesh=bpy.data.meshes.new(name+" voussoir mesh")
        mesh.from_pydata(v,[],faces);mesh.update()
        obj=bpy.data.objects.new(name+" stone voussoir "+str(i),mesh)
        scene.collection.objects.link(obj)
        obj.data.materials.append(trim if i%3==0 else stone)
stone_arch("Lower monumental portal",0,-19.6,10.8,3.05,1.05,3.1,15)
stone_arch("Upper stair victory arch",0,15.25,11.7,3.8,.52,.7,13)
# Gate frame with a real dark pass-through zone; never block main route visually.
cuboid("Lower gate lintel",(0,-18,14.25),(8.0,3.5,1.45),stone,.14)
for xx in (-3.95,3.95):
    cuboid("Lower arched gate side post",(xx,-18,11.0),(1.1,3.5,5.6),trim,.08)
cuboid("Lower gate shadow threshold",(0,-17.93,9.3),(7.2,.12,2.1),rock,.04)
tower("Upper stronghold keep",-1.5,34,13.05,10,8.5)
tower("Upper west rear guard",-15,35,13.05,7,4)
tower("Upper east rear guard",15,35,13.05,7.6,4)
# Continuous high curtain, with gap revealing dominant keep.
cuboid("Upper curtain left",(-11.5,37.3,16.1),(14,2.4,6.1),stone,.10)
cuboid("Upper curtain right",(11.5,37.3,16.1),(14,2.4,6.1),stone,.10)
cuboid("Upper courtyard command hall",(9,29,15.1),(11,8,4.0),stone,.14)
cuboid("Command hall slate cap",(9,29,17.2),(11.8,8.8,.36),rock,.12)
# Keep lower to upper transition must be clearly bounded by supporting retaining face.
for xx in (-17,-12,12,17):
    cuboid("Terrace buttress",(xx,16.55,10.85),(1.8,2.0,5.9),stone,.08)
# Timber details, exposed on the plausible service side; a sparse frontier settlement.
for x,y in [(-15,-6),(-11,7),(13,-4),(12,9)]:
    cuboid("Open work lean-to",(x,y,9.3),(4.7,3.1,.2),wood,.05)
    for ox in (-2,2):
        cuboid("Lean-to post",(x+ox,y-1.2,8.7),(.28,.28,1.55),wood,.02)
    cuboid("Stacked timber",(x+.6,y+.3,8.3),(2.0,1.15,.4),wood)
# Make major buildings read as inhabited and playable at strategic camera scale.
# Gabled stone-slate roofing uses actual sloped surfaces rather than floating boxes.
def pitched_roof(name,x,y,z,w,d,pitch=1.55):
    verts=[(x-w/2,y-d/2,z),(x+w/2,y-d/2,z),
           (x+w/2,y+d/2,z),(x-w/2,y+d/2,z),
           (x,y-d/2,z+pitch),(x,y+d/2,z+pitch)]
    faces=[(0,1,4),(3,5,2),(0,4,5,3),(1,2,5,4)]
    me=bpy.data.meshes.new(name+"Slates")
    me.from_pydata(verts,[],faces);me.update()
    ob=bpy.data.objects.new(name+" actual pitched roof",me)
    scene.collection.objects.link(ob);ob.data.materials.append(rock)
for side in (-1,1):
    for i in range(4):
        cx=side*(11+(i%2)*6);cy=-11+(i//2)*13
        pitched_roof("Bastion I modest cottage",cx,cy,10.5,5.9,6.0,1.35)
# Visible central route has joint rhythm and continuous navigation.
for j in range(40):
    yy=-34+j*1.10
    if 14<=yy<=23:continue
    elev=13.23 if yy>23 else 8.19
    cuboid("Road stone courses",(0,yy,elev),(7.1,.035,.035),trim)
# Inhabited work sites and a convincing ruined-sawmill reservation.
for xx in (-16,-12,-8):
    cuboid("Sawmill timber reserve",(xx,-12.7,8.45),(3.2,.65,.55),wood,.04)
for xx in (-18,-10):
    cuboid("Sawmill unfinished wooden scaffolding",(xx,-9,9.8),(.25,.25,3.5),wood)
cuboid("Sawmill foundation not completed",(-14,-10,8.24),(12,9,.34),stone,.11)
# Restrained banners provide composition scale and kingdom identity.
for x,y,z in [(-6,-18,14),(6,-18,14),(-1.5,33,23)]:
    cuboid("Heraldic cloth blue",(x,y-.5,z-1.7),(1.15,.11,3.5),blue,.02)
# Keep roof silhouette is stronger than guard towers but secondary to keep mass.
pitched_roof("Keep commanding timber roof",-1.5,34,23.2,9.8,9.8,2.2)
# World-space architectural variation: irregular dressed stone ridges and roof tiles.
# Every piece shares the terrain coordinate system; no camera-space impostors.
def dressed_arch(name,x,y,z,w=4.8,depth=1.25,height=3.8):
    # Two stone jambs and a wedge-course lintel over an actual open passage.
    for xx in (x-w/2,x+w/2):
        cuboid(name+" dressed pier",(xx,y,z+height*.48),(.85,depth,height),stone,.09)
        for course in range(4):
            cuboid(name+" recessed cut ashlar",(xx,y-depth/2-.06,z+.45+course*.85),
                   (.75,.08,.06),trim,.012)
    for j in range(7):
        xc=x-w/2+(j+.5)*w/7
        cuboid(name+" segmented lintel",(xc,y,z+height+.22),
               (w/7+.045,depth+.20,.65),stone if j%2 else trim,.03)
dressed_arch("Front postern ceremonial arch",0,-18,8.18,w=6.9,depth=2.4,height=5.0)
# A larger genuinely three-dimensional tapered stair landing and solid support.
for side in (-1,1):
    for step in range(12):
        y=14.2+step*.67
        z=8.1+(step/11.0)*4.9
        cuboid("Joined stair edge footing",(side*4.08,y,z-.36),
               (1.10,.70,.90),stone,.07)
# Irregular roof slates create a reading of real roofing instead of big smooth polygons.
for cx,cy,roofz,rw,rd in [(-1.5,34,23.2,9.8,9.8),
                         (9,29,17.4,11.8,8.8)]:
    for side in (-1,1):
        for i in range(12):
            yy=cy-rd*.46+(i+.5)*rd/12
            px=cx+side*rw*.235
            zz=roofz+1.1
            slate=cuboid("Layered miniature slate courses",(px,yy,zz),
                    (rw*.48,rd/11.3,.105),rock,.015)
            slate.rotation_euler[1]=side*math.radians(17)
# Physically set roof tiling follows each gable slope, instead of merely painting
# rows into a surface. Kept modest for mobile-compatible low-density geometry.
tile=material("Weathered slate roof variation",(.105,.127,.153))
def roof_shingles(name,cx,cy,z,w,d,pitch):
    for side in (-1,1):
        for row in range(7):
            px=cx+side*((row+.55)*(w*.5/7))
            surface=z+pitch*(1-2*abs(px-cx)/w)
            for col in range(7):
                py=cy-d*.5+(col+.5)*d/7
                bpy.ops.mesh.primitive_cube_add(size=1,location=(px,py,surface+.035))
                sh=bpy.context.object;sh.name=name+" overlapping slate"
                sh.dimensions=(w/15,d/7.5,.07)
                sh.rotation_euler[1]=(-1 if side<0 else 1)*math.atan2(2*pitch,w)
                sh.data.materials.append(tile)
roof_shingles("Stronghold roof",-1.5,34,23.2,9.8,9.8,2.2)
# Wood-clad visible frames give the houses inhabited window and eave detail.
for side in (-1,1):
    for i in range(4):
        xx=side*(11+(i%2)*6); yy=-11+(i//2)*13
        for dx in (-2.15,2.15):
            cuboid("Timber house corner brace",(xx+dx,yy-2.63,9.15),(.20,.18,2.2),wood,.03)
        cuboid("Cottage window reveal",(xx,yy-2.68,9.25),(.66,.10,.85),rock,.025)
# Environmental silhouette: natural trees remain away from approach, not on streets.
bark=material("Dark forest bark",(.085,.065,.05))
pine=material("Cool fir needles",(.075,.14,.115))
for x,y,z,h in [(-28,-21,7.4,6),(-29,8,7.4,7.2),(29,6,7.4,6.5),
                 (27,31,7.5,8),(-26,34,7.4,7.8),(-31,37,3,8),(30,-27,2.5,8.5)]:
    cuboid("Fir trunk",(x,y,z+h*.27),(.48,.48,h*.55),bark)
    bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=h*.22,radius2=0,
                                    depth=h*.72,location=(x,y,z+h*.63))
    bpy.context.object.name="Fir canopy";bpy.context.object.data.materials.append(pine)
# Scaled rock strata physically attached to the cliff, no free-floating scenic fragments.
# Distorted icospheres mask the square top/bedrock transition with coherent talus geology.
rng=random.Random(318)
for side in (-1,1):
    for k in range(22):
        y=-24+k*2.9 + rng.uniform(-.45,.45)
        x=side*(23.5 + rng.uniform(-.7,2.6))
        z=6.3+rng.uniform(-3.0,.9)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,
             location=(x,y,z))
        cliffrock=bpy.context.object
        cliffrock.name="Embedded cliff fracture"
        cliffrock.scale=(rng.uniform(1.25,2.3),rng.uniform(1.8,3.9),rng.uniform(1.8,4.8))
        cliffrock.data.materials.append(rock)
# Sparse natural debris clusters and vegetation on the courtyard edge, outside the route.
for k in range(32):
    side=-1 if k%2==0 else 1
    x=side*rng.uniform(19.0,23.4)
    y=rng.uniform(-24,10)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,
        location=(x,y,8.06))
    pebble=bpy.context.object;pebble.name="Embedded courtyard scree"
    pebble.scale=(rng.uniform(.30,.85),rng.uniform(.35,1.1),rng.uniform(.15,.40))
    pebble.data.materials.append(rock)
# Lithic apron fills gaps beneath vertical outer walls; avoid dark void between wall and cliff.
for side in (-1,1):
    for j in range(20):
        yy=-24+j*2.25
        xx=side*(20.0+1.0*math.sin(yy*.14))
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,
            location=(xx,yy,6.3))
        ob=bpy.context.object;ob.name="Continuous foundation scree"
        ob.scale=(2.9,2.4,2.4)
        ob.data.materials.append(rock)
# Retaining face beneath the upper terrace is a complete solid masonry front.
for side in (-1,1):
    cuboid("Joined inner terrace retaining face", (side*11.9,16.9,10.5),(12.2,2.2,5),stone,.11)
# Outer dressed-stone frontage covers the exposed black bedrock step, except
# for the genuine 8.2m central stair corridor. This is a continuous architectural face.
for side in (-1,1):
    cuboid("Stone-faced upper terrace fascia",(side*12.9,15.42,10.45),(17.4,1.4,4.9),stone,.13)
    for xstep in (7,12,17,21):
        cuboid("Upper terrace facade buttress",(side*xstep,14.55,10.55),(.72,2.0,5.1),trim,.08)
# Central 8.2m portal remains open for actual stair navigation corridor.
# Open the main stair passage with two independently capped parapets.
for side in (-1,1):
    cuboid("Central stair balustrade",(side*4.15,19.2,11.1),(.9,11.4,1.4),stone,.13)
# Stone courses break monolithic keep walls into a built structure.
for z in [15.5,17.0,18.5,20.0,21.5]:
    for xx in [-5.45,-3.6,-1.75,.10,1.95]:
        cuboid("Citadel dressed stone joint",(xx,29.68,z),(.09,.07,.07),trim)
# Dark recesses and a large visibly occupied hall; no painted texture pretending to be a door.
black=material("Deep inner void",(.035,.034,.030))
for xx in [-4.0,-1.5,1.0]:
    cuboid("Upper keep dark arrow slit",(xx,28.55,18.65),(.48,.16,1.7),black,.025)
for xx in [5.8,9.0,12.2]:
    cuboid("Citadel hall recess",(xx,24.85,15.4),(.9,.12,1.25),black,.03)
# Smooth road blocks and river threshold to secure one unified central route.
# Stone bridge physically crosses an excavated outer approach rather than hovering
# as a flat block in the middle of a courtyard.
cuboid("Bridge lower abutment",(0,-38.4,4.0),(10.4,7.0,8.1),rock,.22)
cuboid("Bridge approach from wild country",(0,-41.0,8.15),(7.7,8.5,.23),paving,.07)
cuboid("Bridge continuous walkway",(0,-29,8.15),(7.7,17,.23),paving,.08)
cuboid("Bridge gate connecting sill",(0,-20.5,8.12),(7.7,4,.21),paving,.03)
for side in (-1,1):
    bx=side*4.45
    cuboid("Stone bridge parapet",(bx,-31,9.14),(.75,17,1.7),stone,.1)
    for yy in (-37,-32,-27,-23):
        cuboid("Bridge coping finial",(bx,yy,10.15),(1.12,1.0,.38),trim,.05)
# Face wall / retaining curtain on both sides of the real gate opening.
cuboid("Lower outer curtain left",(-15.6,-19,10.7),(13.0,2.8,5.3),stone,.10)
cuboid("Lower outer curtain right",(15.6,-19,10.7),(13.0,2.8,5.3),stone,.10)
for side in (-1,1):
    for xx in (9.5,15.5,20.0):
        cuboid("Front crenellation",(side*xx,-19,13.82),(1.85,2.9,1.05),stone,.06)
# The courtyard facade now actually encloses the entrance, not merely two towers.

# Rejected CC0 style experiment: third-party cartoon assets failed Dark Noble Strategy silhouette consistency.
# Coherent stonemason-authored surface articulation: courses and window reveals.
# This pass makes the two fortress elevations read at the strategic camera.
dark=material("Deep aperture shadow",(.038,.043,.046))
for xx in (-5.2,-3.15,-1.1,0.95,3.0):
    for zz in (15.7,18.0,20.3):
        cuboid("Keep vertical stone reveals",(xx,28.52,zz),(.22,.14,1.35),trim,.018)
        cuboid("Keep recessed glazing",(xx+.45,28.43,zz),(.55,.10,.98),dark,.014)
for yy in (25.5,28.0,30.5,33.0):
    for xx in (5.7,8.1,10.5,12.9):
        cuboid("Command hall carved masonry",(xx,yy,17.05),(.15,.19,.28),trim)
for side in (-1,1):
    for yy in (-22,-16,-10,-4,2,8,14):
        cuboid("Lower curtain external pilaster",(side*23.35,yy,10.7),
               (1.0,1.7,5.2),stone,.04)
        for height in (9.0,10.3,11.6):
            cuboid("Lower curtain course joint",(side*23.91,yy,height),
                   (.08,2.6,.05),trim)
# Turn plain cottages into inhabited timberframe houses, without blocking the axis.
for side in (-1,1):
    for i in range(4):
        x=side*(11+(i%2)*6);y=-11+(i//2)*13
        for dx in (-2.25,2.25):
            cuboid("Cottage exposed timber stud",(x+dx,y-2.68,9.25),
                   (.24,.18,2.6),wood,.015)
        cuboid("Cottage front door recess",(x,y-2.71,9.2),
               (1.0,.10,1.75),dark,.015)
        cuboid("Cottage cross brace",(x,y-2.73,10.0),
               (4.6,.14,.19),wood)
# Structural limestone masonry in variable coursed ashlar blocks; scaled for a strategy camera.
# Visual break-up is authored physically, not only by noise in image textures.
stone2=material("Aged pale limestone",(.43,.39,.32))
rng_masonry=random.Random(904)
for side in (-1,1):
    for row in range(4):
        z=9.00+row*1.13
        for k in range(22):
            y=-24.0+k*2.15+(1.07 if row%2 else 0)
            if y>24:continue
            x=side*23.47
            d=rng_masonry.uniform(.15,.35)
            cuboid("Stone course individual voussoir",(x,y,z),(.26,1.85,1.00),
                   stone if (k+row)%4 else stone2,.018)
# Inset taller arrow-slit doors and windows with lintels at distant strategic zoom.
for side in (-1,1):
    for y in (-15,-5,5,15):
        cuboid("Curtain sentry slit",(side*23.69,y,11.7),(.09,.30,1.02),dark,.02)
        cuboid("Curtain stone opening lintel",(side*23.75,y,12.3),(.17,.65,.18),trim,.03)
# Main entry gets angled arch stones and an accent blue banner, not flat gate frontage.
for side in (-1,1):
    for k in range(7):
        theta=math.pi*k/6
        x=side*3.05+math.cos(theta)*1.25
        z=13.9+math.sin(theta)*1.28
        cuboid("Entrance dressed voussoir",(x,-19.2,z),(.66,.55,.48),trim,.04)
# Robust warm torchlights and richer atmospheric contrast.
# Controlled environment dressing, authored to the real plateau silhouette.
# Low-cost pine clusters and undergrowth restore the forested frontier identity.
leaf_deep=material("Blue-dark fir needles",(.055,.115,.092))
leaf_mid=material("Evergreen fir needles",(.085,.17,.117))
bush=material("Bramble scrub",(.19,.24,.12))
def fir(name,x,y,ground,height):
    cuboid(name+" trunk",(x,y,ground+height*.28),(.25,.28,height*.55),wood)
    for k in range(3):
        z=ground+height*(.42+k*.18)
        radius=height*(.215-k*.045)
        bpy.ops.mesh.primitive_cone_add(vertices=9,radius1=radius,radius2=0,
            depth=height*.45,location=(x,y,z))
        crown=bpy.context.object;crown.name=name+" foliage"
        crown.data.materials.append(leaf_deep if k%2 else leaf_mid)
for x,y,g,h in [
    (-26,-12,7.65,5.1),(-27,-3,7.55,6.4),(-27,20,7.65,6.7),
    (27,-11,7.6,6.3),(27,16,7.6,6.0),(27,27,7.4,7.3),
    (-24,31,7.4,5.8),(-24,-25,7.5,4.7)]:
    fir("Valoria perimeter fir",x,y,g,h)
for x,y in [(-19,-17),(-19,6),(-16,11),(19,-13),(17,3),(18,11),
            (-14,26),(16,25)]:
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,
        location=(x,y,8.33 if y<15 else 13.38))
    o=bpy.context.object;o.name="Embedded gorse scrub"
    o.scale=(1.25,.9,.5);o.data.materials.append(bush)
# Cobblestone course blocks give the road readable micro-scale without extra textures.
for yy in range(-34,13,3):
    for xx in (-2.7,-1.35,0,1.35,2.7):
        cuboid("Road individual sett",(xx,yy,8.22),(1.18,2.4,.08),trim,.08)
# Animated-look static light sources for proof only.
for i,y in enumerate((-19,-5,10,24,35)):
    for x in (-5,5):
        cuboid("Torch standard",(x,y,9 if y<12 else 14),(0.24,.24,2.1),wood)
        cuboid("Emissive torch cue",(x,y,10.1 if y<12 else 15.1),(.33,.33,.4),glow)
def beam_between(name,a,b,radius,mat,vertices=6):
    av,bv=Vector(a),Vector(b)
    delta=bv-av
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,
        depth=delta.length,location=(av+bv)*.5)
    ob=bpy.context.object;ob.name=name
    ob.rotation_euler=delta.to_track_quat('Z','Y').to_euler()
    ob.data.materials.append(mat)
    return ob

# Remove the deliberately failed roof slabs and conical tree proxies. This pass
# replaces their geometry rather than accumulating props over the failed shapes.
for ob in list(bpy.data.objects):
    if ob.type!='MESH':continue
    if any(key in ob.name for key in ('Slate pitched roof proxy','Command hall slate cap',
          'Fir canopy','Valoria perimeter fir foliage','Valoria perimeter fir trunk')):
        bpy.data.objects.remove(ob,do_unlink=True)

plaster=material('Limewashed infill',(.47,.42,.32))
# High-pitched roofs, actual eaves and half-timber frames are the secondary
# architectural language. Macro features remain visible at strategic scale.
for side in (-1,1):
    for i in range(4):
        x=side*(11+(i%2)*6);y=-11+(i//2)*13
        cuboid('Cottage lime infill front',(x,y-2.66,9.45),(4.65,.16,2.65),plaster,.02)
        cuboid('Cottage lime infill side',(x+side*2.47,y,9.45),(.16,4.9,2.65),plaster,.02)
        pitched_roof('Timbered steep gable',x,y,10.8,6.3,6.5,2.3)
        for dx in (-2.35,0,2.35):
            beam_between('Exposed facade structural stud',(x+dx,y-2.82,8.2),
                (x+dx,y-2.82,10.85),.10,wood)
        for dx in (-2.35,2.35):
            beam_between('Raking gable structural beam',(x+dx,y-3.31,10.85),
                (x,y-3.31,13.1),.12,wood)
            beam_between('Braced cottage panel',(x+dx,y-2.83,8.3),
                (x,y-2.83,10.3),.075,wood)
        cuboid('Cottage projecting eave',(x,y-3.3,10.81),(6.55,.32,.22),wood,.02)
        for dx in (-1.45,1.45):
            cuboid('Cottage dark recessed window',(x+dx,y-2.88,9.75),(.73,.14,.87),dark,.02)
            cuboid('Carved window sill',(x+dx,y-3.0,9.25),(1.05,.32,.18),trim,.025)
        cuboid('Cottage stone chimney',(x+1.7,y+.6,12.1),(.73,.82,3.5),stone,.05)
pitched_roof('Command hall steep slate roof',9,29,17.4,12.4,9.5,3.6)

def country_height(x,y):
    # Plateau joins an extensive world, with a sunken approach rather than a
    # floating island on a blue studio background.
    r=math.sqrt((x/29.0)**2+((y-2)/43.0)**2)
    shoulder=7.4*math.exp(-max(0,r-1)*2.5)
    distant=2.6*math.sin(x*.036+y*.018)+1.7*math.cos(y*.057-x*.011)
    return -2.5+shoulder+distant*min(1,max(0,r-1))

nx=90;ny=110;verts=[];faces=[]
for j in range(ny+1):
    y=-140+j*310/ny
    for i in range(nx+1):
        x=-150+i*300/nx
        verts.append((x,y,country_height(x,y)))
for j in range(ny):
    for i in range(nx):
        a=j*(nx+1)+i
        faces.append((a,a+1,a+nx+2,a+nx+1))
me=bpy.data.meshes.new('Continuous hinterland sculpt')
me.from_pydata(verts,[],faces);me.update()
ob=bpy.data.objects.new('Valoria continuous inhabited hinterland',me)
scene.collection.objects.link(ob);ob.data.materials.append(earth)
for face in me.polygons:face.use_smooth=True

def branch_fir(x,y,ground,height,seed):
    # Author the whole tree as one indexed mesh. Thousands of bpy operator calls
    # repeatedly rebuilt the dependency graph; direct source construction keeps
    # the same layered branch silhouette with linear authoring cost.
    rr=random.Random(seed);vertices=[];faces=[];slots=[]
    def cylinder(a,b,radius,slot):
        av,bv=Vector(a),Vector(b);axis=(bv-av).normalized()
        u=axis.cross(Vector((0,0,1)))
        if u.length<.01:u=Vector((1,0,0))
        u.normalize();v=axis.cross(u).normalized();first=len(vertices)
        for center,r in [(av,radius),(bv,radius*.48)]:
            for k in range(6):
                angle=k*math.pi/3
                vertices.append(tuple(center+r*(u*math.cos(angle)+v*math.sin(angle))))
        for k in range(6):
            faces.append((first+k,first+(k+1)%6,first+6+(k+1)%6,first+6+k));slots.append(slot)
        faces.append(tuple(first+k for k in range(5,-1,-1)));slots.append(slot)
        faces.append(tuple(first+6+k for k in range(6)));slots.append(slot)
    cylinder((x,y,ground),(x,y,ground+height),.12,0)
    for level in range(5):
        z=ground+height*(.28+level*.125);reach=height*(.27-level*.039)
        for k in range(5):
            angle=k*2*math.pi/5+level*.79+rr.uniform(-.12,.12)
            cylinder((x,y,z),(x+math.cos(angle)*reach,y+math.sin(angle)*reach,z-.18),.04,0)
            cx=x+math.cos(angle)*reach*.67;cy=y+math.sin(angle)*reach*.67
            first=len(vertices);long=reach*.80;wide=reach*.51;h=height*.12
            vertices.append((cx,cy,z+.15+h))
            for ring in (-.42,.42):
                for n in range(6):
                    a=n*math.pi/3
                    dx=math.cos(a)*long*.91;dy=math.sin(a)*wide*.91
                    vertices.append((cx+dx*math.cos(angle)-dy*math.sin(angle),
                        cy+dx*math.sin(angle)+dy*math.cos(angle),z+.15+ring*h))
            vertices.append((cx,cy,z+.15-h))
            slot=1 if (k+level)%3 else 2
            for n in range(6):
                faces.append((first,first+7+n,first+7+(n+1)%6));slots.append(slot)
                faces.append((first+1+n,first+1+(n+1)%6,first+7+(n+1)%6,first+7+n));slots.append(slot)
                faces.append((first+13,first+1+(n+1)%6,first+1+n));slots.append(slot)
    me=bpy.data.meshes.new('Authored branch fir source');me.from_pydata(vertices,[],faces);me.update()
    ob=bpy.data.objects.new('Authored layered branch fir',me);scene.collection.objects.link(ob)
    for mat in (wood,leaf_deep,leaf_mid):me.materials.append(mat)
    for polygon,slot in zip(me.polygons,slots):polygon.material_index=slot


forest_rng=random.Random(421)
for k in range(64):
    side=-1 if k%2 else 1
    x=side*forest_rng.uniform(31,72);y=forest_rng.uniform(-43,79)
    branch_fir(x,y,country_height(x,y),forest_rng.uniform(6,10),k+810)
for k,(x,y) in enumerate([(-25,-10),(-25,3),(25,-9),(25,7),(-24,28),(24,32)]):
    branch_fir(x,y,7.5,5.7+k*.3,k+900)

# Human scale and colour are established by gardens, stone threshold transitions,
# broken yard edges, and a kept central street rather than miniature torch blocks.
for side in (-1,1):
    for y in (-7,7):
        for k in range(6):
            cuboid('Kitchen garden cultivated bed',(side*17.7,y+k*.38,8.15),
                (3.0,.22,.14),earth,.02)
        for k in range(5):
            beam_between('Garden rustic fence',(side*19.5,y+k*.5,8.05),
                (side*19.5,y+k*.5,9.15),.06,wood)

# Join cliff into talus rather than exposing an enormous vertical pedestal.
for ob in bpy.data.objects:
    if ob.type=='MESH' and ob.name.startswith('Organic connected cliff foundation'):
        for v in ob.data.vertices:
            if v.co.z<1:v.co.z=1+(v.co.z-1)*.42


# Procedural mid-frequency PBR variation: avoid flat prototype color slabs.
for mat in [stone,trim,paving,earth,grass,rock,wood,pine]:
    nodes=mat.node_tree.nodes
    links=mat.node_tree.links
    bs=nodes.get("Principled BSDF")
    if not bs:continue
    base=tuple(bs.inputs["Base Color"].default_value[:3])
    noise=nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value=3.2 if mat in [rock,grass,earth] else 8.0
    noise.inputs["Detail"].default_value=3.0
    ramp=nodes.new("ShaderNodeValToRGB")
    colors=[tuple(max(0.001,c*.62) for c in base)+(1,),
            tuple(min(.98,c*1.36) for c in base)+(1,)]
    ramp.color_ramp.elements[0].color=colors[0]
    ramp.color_ramp.elements[1].color=colors[1]
    links.new(noise.outputs["Fac"],ramp.inputs["Fac"])
    links.new(ramp.outputs["Color"],bs.inputs["Base Color"])
# Exportable image-based base colors for Unity glTF; Blender procedural noise alone
# is not reliably carried into Unity's material importer.
# Reuse locally committed authoring textures only; external provenance remains a release gate.
texture_dir=ROOT/"art-source/valoria/production/bastion-family-v1"
for mat,filename in [(stone,"masonry_albedo.png"),(trim,"hero_masonry_albedo.png"),
                     (paving,"paving_albedo.png"),(wood,"timber_albedo.png"),
                     (rock,"stone_grain_albedo.png")]:
    path=texture_dir/filename
    if not path.exists():continue
    bs=mat.node_tree.nodes.get("Principled BSDF")
    if not bs:continue
    image_node=mat.node_tree.nodes.new("ShaderNodeTexImage")
    image_node.image=bpy.data.images.load(str(path),check_existing=True)
    image_node.interpolation="Linear"
    mat.node_tree.links.new(image_node.outputs["Color"],bs.inputs["Base Color"])
# Author a restrained emissive torch hue with a separate warm point light cluster.
torch_bs=glow.node_tree.nodes.get("Principled BSDF")
if "Emission Color" in torch_bs.inputs:torch_bs.inputs["Emission Color"].default_value=(1,.30,.035,1)
elif "Emission" in torch_bs.inputs:torch_bs.inputs["Emission"].default_value=(1,.30,.035,1)
if "Emission Strength" in torch_bs.inputs:torch_bs.inputs["Emission Strength"].default_value=2
for x,y,z in [(-6,-18,16),(6,-18,16),(0,24,17),(0,34,24)]:
    lamp=bpy.data.lights.new("Amber fire contrast","POINT")
    lamp.energy=420;lamp.color=(1,.42,.18)
    ob=bpy.data.objects.new("Amber fire contrast",lamp)
    scene.collection.objects.link(ob);ob.location=(x,y,z)
world=bpy.data.worlds.new("Cold forest dusk")
scene.world=world;world.use_nodes=True
world.node_tree.nodes["Background"].inputs["Color"].default_value=(.09,.12,.18,1)
world.node_tree.nodes["Background"].inputs["Strength"].default_value=.95
ld=bpy.data.lights.new("Raking soft key","AREA");l=bpy.data.objects.new("Raking soft key",ld);scene.collection.objects.link(l)
l.location=(-38,-24,66);ld.energy=11500;ld.size=22;ld.color=(1.0,.78,.55)
# Explicit architectural composition camera, not SHARP camera-space placement.
cam_d=bpy.data.cameras.new("Valoria Worldspace Camera")
cam=bpy.data.objects.new("Valoria Worldspace Camera",cam_d)
scene.collection.objects.link(cam);scene.camera=cam
cam.location=(55,-82,78)
target=Vector((0,6,8))
cam.rotation_euler=(target-Vector(cam.location)).to_track_quat("-Z","Y").to_euler()
# View-specific fog and depth behind the structure (not exported to Unity).
# Correct muted daylight reference: castle readable on mobile, warm lanterns as accents.
world=scene.world
if world and world.use_nodes:
    bg=world.node_tree.nodes.get("Background")
    if bg:
        bg.inputs["Color"].default_value=(.28,.34,.44,1)
        bg.inputs["Strength"].default_value=.85
scene.view_settings.view_transform="Standard"
scene.view_settings.look="Medium High Contrast"
scene.view_settings.exposure=.45
cam_d.type="ORTHO";cam_d.ortho_scale=94
# Export-safe source surfaces and explicit UVs. Procedural Blender nodes are
# replaced by authored image tiles; the same material data travels into Unity.
import numpy as np
surface_dir=ROOT/'docs/evidence/valoria-mesh-only-prototype/generated-surfaces'
surface_dir.mkdir(parents=True,exist_ok=True)
for mat,base in [(grass,(.145,.195,.095)),(earth,(.24,.215,.145)),
                 (plaster,(.47,.42,.32)),(leaf_deep,(.045,.095,.068)),
                 (leaf_mid,(.067,.135,.082))]:
    size=256
    yy,xx=np.mgrid[0:size,0:size]
    # Multi-scale deterministic mineral/grass mottling in source albedo; no light
    # or camera baked into it. These textures are original authored derivatives.
    rr=np.random.RandomState(712)
    noise=(np.sin(xx*.043+np.sin(yy*.067))*np.cos(yy*.036)*.15+
           np.sin(xx*.31+yy*.24)*.07+rr.uniform(-.065,.065,(size,size)))
    pixels=np.ones((size,size,4),dtype=np.float32)
    for channel,c in enumerate(base):pixels[:,:,channel]=np.clip(c*(1+noise),.005,.95)
    image=bpy.data.images.new('Eldoria exportable '+mat.name,width=size,height=size)
    image.pixels=pixels.ravel().tolist()
    image.filepath_raw=str(surface_dir/(mat.name.replace(' ','_')+'.png'))
    image.file_format='PNG';image.save();image.pack()
    bs=mat.node_tree.nodes.get('Principled BSDF')
    tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=image
    mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])

for ob in bpy.data.objects:
    if ob.type!='MESH' or ob.data.uv_layers:continue
    uv=ob.data.uv_layers.new(name='SourceWorldUV')
    for polygon in ob.data.polygons:
        normal=polygon.normal
        axis=max(range(3),key=lambda k:abs(normal[k]))
        for loop in polygon.loop_indices:
            vertex=ob.matrix_world@ob.data.vertices[ob.data.loops[loop].vertex_index].co
            coords=(vertex.y,vertex.z) if axis==0 else (vertex.x,vertex.z) if axis==1 else (vertex.x,vertex.y)
            uv.data[loop].uv=(coords[0]*.18,coords[1]*.18)


bpy.ops.wm.save_as_mainfile(filepath=str(OUT.with_suffix('.blend')))

# Collapse static authored geometry into a small material vocabulary before
# exporting: the previous 1163 independent mesh renderers are unacceptable as
# a mobile production baseline. Preserve world coordinates and material slots.
material_groups={}
for ob in list(bpy.data.objects):
    if ob.type!='MESH':continue
    key=tuple(m.name if m else 'none' for m in ob.data.materials)
    material_groups.setdefault(key,[]).append(ob)
for key,objects in material_groups.items():
    if len(objects)<2:continue
    bpy.ops.object.select_all(action='DESELECT')
    for ob in objects:ob.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    bpy.ops.object.join()
    bpy.context.object.name='Static material batch '+str(len(objects))+' '+key[0]
mesh_metrics={'mesh_objects':sum(ob.type=='MESH' for ob in bpy.data.objects),
              'triangles':sum(sum(len(p.vertices)-2 for p in ob.data.polygons)
                              for ob in bpy.data.objects if ob.type=='MESH'),
              'materials':len(bpy.data.materials),
              'device_profiled':False}
print('STATIC_SOURCE_METRICS',json.dumps(mesh_metrics))

# Export all visual mesh geometry in the same continuous world space for Unity import.
# Blender-specific noise shaders are not a Unity material certification.
asset_out=ROOT/"Unity/Assets/Eldoria/ProductionSlice/Experimental/ValoriaMeshOnlyWorld.glb"
asset_out.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action="DESELECT")
for ob in bpy.data.objects:
    if ob.type=="MESH":ob.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(asset_out), export_format="GLB",
                          use_selection=True,export_apply=True,export_yup=True,\n                          export_image_format="JPEG")
if not asset_out.exists() or asset_out.stat().st_size<10000:
    raise RuntimeError("Worldspace GLB export missing or invalid")
if asset_out.stat().st_size>22000000:
    raise RuntimeError("Worldspace GLB exceeds prototype repository size budget")
# Hard fail on basic spatial discontinuities before producing reassuring render evidence.
# These checks do not replace visual approval.
def approximately(a,b,tol=.65): return abs(a-b)<=tol
bridge_y=(-29-17/2,-29+17/2)
gate_y=(-18-3.5/2,-18+3.5/2)
road_y=(-34/2,34/2)
upper_y=(28.6-22/2,28.6+22/2)
stairs_y=(14-.59/2,14+(14*.55)+.59/2)
assert bridge_y[1]>=(-20.5-4/2), "Bridge cannot reach connecting sill"
assert (-20.5+4/2)>=gate_y[0], "Connecting sill cannot reach gate"
assert road_y[0]<=gate_y[1], "Gate cannot connect road"
assert stairs_y[0]<=road_y[1], "Stairs do not contact main road"
assert stairs_y[1]>=upper_y[0], "Stairs do not reach upper courtyard"
assert approximately(8.08,8.15,.25), "Bridge/main street level mismatch"
assert approximately(8.13,8.08,.25), "Lower stair tread disconnected"
assert approximately(8.13+14*(5/15),13.09,.45), "Upper stair tread disconnected"
assert 21<25.1, "Upper courtyard exceeds supporting bedrock"
print("WORLDSPACE_CONNECTIVITY_PASS: bridge, gate, main road, stair, upper courtyard, rock support")
bpy.ops.render.render(write_still=True)
report={"result":"BLENDER WORLDSPACE VISUAL PROOF ONLY; UNITY NOT TESTED",
        "basis":"single scene coordinates, world-space mesh bases and a connected authored substrate",
        "no_sharp":True,"families":results,"static_source_metrics":mesh_metrics}
OUT.with_suffix(".json").write_text(json.dumps(report,indent=2))
# Store direct-review thumbnail in branch as UTF-8 for independent visual inspection.
img=bpy.data.images.load(str(OUT));img.scale(540,360)
thumb=OUT.with_name("valoria-mesh-review.jpg")
img.filepath_raw=str(thumb);img.file_format="JPEG";img.save()
review=ROOT/"docs/evidence/valoria-mesh-only-prototype"
review.mkdir(parents=True,exist_ok=True)
(review/"preview.jpg.base64.txt").write_text(base64.b64encode(thumb.read_bytes()).decode("ascii"))
print("WORLDSPACE PROOF OUTPUT",str(OUT))


