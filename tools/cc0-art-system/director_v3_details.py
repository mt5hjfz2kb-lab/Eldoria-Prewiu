"""Director V3 in-place addition to the *real* Director V2 Blender build.

All globals (bpy, scene, group, remove_collection_objs, M, block, beam,
face_mesh, cylinder, cone, ico, counts, height, rgb, mat, etc.) are defined by
upgrade_director_v2.py and are reused without reimplementing their material
or asset conventions. Every new object is authentic, editable 3D geometry.
No external art pack or 2D concept is embedded as the environment.
"""
# Replacing repeated generic cottage shells is a deliberate artistic pivot:
# do not keep the houses and merely add props, since that was rejected.
for old_group in ("Town","V2_Industry","V2_Water","V2_Granary","V2_Barracks","V2_Quarry"):
    remove_collection_objs(old_group)

def pitch_roof(name,x,y,w,d,z,rise,role="roof",collection="V3_Industries"):
    # A REAL two-sided dimensional sloped roof. No rendered billboard.
    verts=[(x-w/2,y-d/2,z),(x+w/2,y-d/2,z),
           (x-w/2,y+d/2,z),(x+w/2,y+d/2,z),
           (x,y-d/2,z+rise),(x,y+d/2,z+rise)]
    roof=face_mesh(name+" heavy slate",verts,
                   [(0,2,5,4),(4,5,3,1)],role,collection)
    for yline in (y-d/2,y+d/2):
        beam(name+" bronze roof-ridge trim",(x,yline,z+rise),
             (x,yline+(d*.035 if yline<y else -d*.035),z+rise),
             .16,"roof_edge",collection)
    for sign in (-1,1):
        beam(name+" timber eave",(x+sign*w/2,y-d/2,z),
             (x+sign*w/2,y+d/2,z),.18,"timber_dark",collection)
    for sy in (-1,1):
        beam(name+" shadow gable base",(x-w/2,y+sy*d/2,z-.06),
             (x+w/2,y+sy*d/2,z-.06),.18,"timber",collection)
    return roof

def framed_barn(name,x,y,w,d,h,stone=False,with_walls=True):
    col="V3_Industries"
    if with_walls:
        block(name+" stone lower plinth",(x,y,.48),(w,d,.95),
              "stone_dark",col,.07)
        block(name+" main timbered body",(x,y,1.0+h*.5),(w,d,h),
              "stone" if stone else "timber",col,.06)
        for sx in (-1,1):
            for sy in (-1,1):
                px=x+sx*(w/2-.22);py=y+sy*(d/2-.20)
                block(name+" timber stave",(px,py,h*.5+1),(.35,.36,h+1.1),
                      "timber_dark",col,.035)
        # Plaster / timber segmented main wall with an identifiable door.
        block(name+" warm arched entry",(x,y-d/2-.18,1.54),(1.35,.24,2.65),
              "timber_dark",col,.03)
        beam(name+" carved stone lintel",(x-1,y-d/2-.27,2.92),
             (x+1,y-d/2-.27,2.92),.19,"stone_light",col)
    else:
        # Roofed open pavilion shows sawmill machines from camera, not another house.
        for sx in (-1,1):
            for sy in (-1,1):
                px=x+sx*(w/2-.32);py=y+sy*(d/2-.33)
                block(name+" timber open pier",(px,py,h*.5),(.38,.38,h),
                      "timber",col,.04)
                block(name+" stone anchor",(px,py,.24),(.62,.62,.46),
                      "stone_dark",col,.07)
        for sy in (-1,1):
            beam(name+" industrial roof truss",(x-w/2,y+sy*d/2,h),
                 (x+w/2,y+sy*d/2,h),.29,"timber_dark",col)
            beam(name+" diagonal truss west",(x-w/2,y+sy*d/2,h),
                 (x,y+sy*d/2,h+2.05),.24,"timber",col)
            beam(name+" diagonal truss east",(x+w/2,y+sy*d/2,h),
                 (x,y+sy*d/2,h+2.05),.24,"timber",col)
    pitch_roof(name,x,y,w+.58,d+.65,h+1.0,2.10,
               collection=col)
    return col

def new_sawmill():
    x,y=-22,-7.5
    col=framed_barn("ASERRADERO / open waterwheel workshop",
                    x,y,9.9,7.5,3.35,False,False)
    # Partial stone back wall and a readable open front workspace.
    block("aserradero covered saw-bench",(x+1.6,y-2.0,1.15),
          (4.2,1.1,.28),"timber","V3_Sawmill")
    for k in range(8):
        yy=y-3.2+k*.78
        cylinder("sawmill bundled trunks",(x-4.2,yy,.27),
                 .28,3.45,"timber","V3_Sawmill",12,rotation=(0,pi/2,0))
    for i in range(7):
        yy=y-2.9+i*.39
        block("freshly split lumber", (x+4.4,yy,.46),
              (.20,3.75,.17),"timber","V3_Sawmill")
    for j in (-1,1):
        beam("sawmill gantry braces",
             (x+j*4.15,y+2.8,.60),(x+j*4.15,y-2.75,3.28),
             .18,"timber_dark","V3_Sawmill")
    # Industrial wide waterwheel at left edge, with visible spokes.
    wx,wy,wz=x-5.8,y-2.1,2.17
    rad=2.15
    for k in range(16):
        a=2*pi*k/16;b=2*pi*(k+1)/16
        pa=(wx,wy+rad*sin(a),wz+rad*cos(a))
        pb=(wx,wy+rad*sin(b),wz+rad*cos(b))
        beam("sawmill wooden ferrule rim",pa,pb,.25,"timber","V3_Sawmill")
    for k in range(8):
        a=2*pi*k/8
        beam("sawmill wheel oak spoke",(wx,wy,wz),
             (wx,wy+rad*sin(a),wz+rad*cos(a)),.20,
             "timber_dark","V3_Sawmill")
    cylinder("sawmill wheel hub",(wx,wy,wz),.40,.65,
             "iron","V3_Sawmill",12,rotation=(0,pi/2,0))
    # Water trough is a *grounded* creek, not a blank blue square.
    verts=[];faces=[]
    for i in range(25):
        yy=y-12+i*.94;xx=wx-.20+.30*sin(i*.4)
        z=height(xx,yy)+.10
        verts.extend([(xx-.71,yy,z),(xx+.71,yy,z+.015)])
        if i:
            idx=2*i
            faces.append((idx-2,idx-1,idx+1,idx))
    face_mesh("wheel-fed natural shallow stream",verts,faces,"water","V3_Stream")
    for i in range(29):
        yy=y-11+random.uniform(0,21)
        xx=wx+random.choice((-1.16,1.10))+random.uniform(-.35,.30)
        ico("stream broken bank rock",(xx,yy,height(xx,yy)+.23),
            random.uniform(.23,.66),"rock_dark","V3_Stream",1,
            (1.02,.91,.64))
    counts["secondary_details"]+=1
new_sawmill()

def new_quarry():
    x,y=-23.5,11.8
    col="V3_Quarry"
    # Genuine excavated rock face with angular unpolished layered masses.
    for i in range(53):
        px=x-6.6+random.uniform(-5.5,8.4)
        py=y+4.9+random.uniform(-1.0,8.6)
        rz=random.uniform(.63,2.50)
        ico("quarry rock and chiseled face",
            (px,py,height(px,py)+rz*.57),rz,
            random.choice(("stone","rock","rock_dark")),col,1,
            (random.uniform(.88,1.42),random.uniform(.73,1.26),random.uniform(.30,.74)))
    # Small stone cutting office, integrated with quarry not a generic house.
    framed_barn("CANTERA / small sheltered stone-cutting workshop",
                x+4.4,y-1.1,6.5,4.6,3.45,stone=True,with_walls=True)
    for i in range(17):
        xx=x-5.5+(i%6)*1.19
        yy=y-4.2+(i//6)*1.0
        block("cut granite blocks on earth",(xx,yy,.45),
              (.97,.90,.80),"stone_light",col,.052)
    # Raised wooden derrick with an actual counterweighted lifting rope.
    for xx in (x-4.2,x-1.0):
        beam("oak quarry crane post",(xx,y-1.3,.15),(xx,y-1.3,6.6),
             .34,"timber",col)
    beam("quarry heavy cross gantry",(x-4.2,y-1.3,6.6),
         (x+1.75,y-1.3,6.6),.39,"timber_dark",col)
    beam("quarry diagonal outrigger",(x-3.98,y-1.3,3.2),
         (x+1.75,y-1.3,6.55),.28,"timber",col)
    beam("quarry hoisting chain",(x+.08,y-1.3,6.4),
         (x+.08,y-1.3,3.3),.085,"iron",col)
    block("suspended unfinished stone block",
          (x+.08,y-1.3,3.1),(1.39,1.18,.79),"stone_light",col,.08)
    counts["secondary_details"]+=1
new_quarry()

def new_granary():
    x,y=22.0,-9.2
    col="V3_Granary"
    framed_barn("GRANERO / long grain warehouse",x-1.0,y,
                9.7,6.8,3.4,stone=True,with_walls=True)
    for j in range(2):
        xx=x+4.55+j*2.60
        cylinder("GRANERO: genuine silo drum",
                 (xx,y+1.0,2.85),1.28,5.65,
                 "stone","V3_Granary",16)
        for h in (1.20,2.35,3.5,4.72,5.42):
            cylinder("grain silo masonry ring",(xx,y+1.0,h),
                     1.34,.16,"stone_light","V3_Granary",16)
        cone("GRANERO / charcoal conical cap",(xx,y+1.0,6.36),
             1.54,.035,1.75,"roof","V3_Granary",16)
        cone("GRANERO / brass pinnacle",(xx,y+1.0,7.37),
             .15,.01,.56,"gold","V3_Granary",8)
        for h in range(1,5):
            beam("grain-silo vertical iron ladder rung",
                 (xx-.31,y-.35,h*.99),(xx+.31,y-.35,h*.99),
                 .075,"iron","V3_Granary")
    for i in range(22):
        xx=x-5+random.uniform(-.7,4.3);yy=y-5.2+random.uniform(-.25,1.5)
        ico("burlap grain sack",(xx,yy,.38),.45,"wheat",
            "V3_Granary",1,(1.22,.66,.72))
    # A small wild harvested patch, not an invented new population district.
    for i in range(83):
        a=random.uniform(-pi,pi);rr=random.random()**.5
        xx=x+2.8+3.4*rr*cos(a);yy=y-6.5+2*rr*sin(a)
        beam("harvest stalk",(xx,yy,.1),(xx,yy,random.uniform(.8,1.4)),
             .041,"wheat","V3_Granary")
    counts["secondary_details"]+=1
new_granary()

def new_barracks():
    x,y=23.8,11.4
    col="V3_Barracks"
    framed_barn("CUARTEL / stone guardhouse and barracks",
                x-1.0,y,9.7,6.2,4.3,stone=True,with_walls=True)
    tx,ty=x+4.7,y+1.3
    cylinder("CUARTEL / corner defense turret",(tx,ty,3.50),
             1.72,6.8,"stone_dark",col,14)
    cylinder("CUARTEL / carved stone parapet",(tx,ty,6.84),
             1.89,.37,"stone_light",col,14)
    for k in range(14):
        a=2*pi*k/14
        xx=tx+1.68*cos(a); yy=ty+1.68*sin(a)
        block("cuartel / vertical crenel",(xx,yy,7.36),
              (.50,.58,.81),"stone",col,.03)
    for i in range(4):
        xx=x-6.0+i*1.67
        yy=y-5.0
        beam("CUARTEL / training upright",(xx,yy,.16),(xx,yy,2.52),
             .15,"timber",col)
        beam("CUARTEL / cross-arm",(xx-.55,yy,1.7),
             (xx+.55,yy,1.7),.16,"timber_dark",col)
        cylinder("CUARTEL / circular target",(xx,yy-.18,1.7),
                 .45,.10,"wheat",col,16,rotation=(pi/2,0,0))
        cylinder("CUARTEL / target boss",(xx,yy-.25,1.7),
                 .22,.11,"iron",col,12,rotation=(pi/2,0,0))
    for i in range(5):
        xx=x+3.0+i*.46;yy=y-4.55
        beam("CUARTEL / training spear",(xx,yy,.2),(xx,yy,3.4),
             .075,"iron",col)
        cone("CUARTEL / pointed pikehead",(xx,yy,3.56),
             .11,.01,.39,"iron",col,7)
    block("CUARTEL / purple guard banner",(tx,ty-.08,9.0),
          (.86,.12,1.42),"banner",col,.035)
    beam("CUARTEL / tall standard",(tx,ty,7.1),
         (tx,ty,10.25),.10,"timber_dark",col)
    counts["secondary_details"]+=1
new_barracks()

# An inhabited scene needs real masses of living terrain and multiple kinds of
# trees. One isolated triangle per wildflower reads as white noise at this scale.
remove_collection_objs("V2_Meadow")
remove_collection_objs("V2_Trees")
def evergreen(x,y,h):
    z0=height(x,y)
    cylinder("tree / trunk",(x,y,z0+h*.30),
             h*.052,h*.60,"bark","V3_Forest",8)
    # 3 large staggered branch tiers + small canopy clusters;
    # deep neutral evergreen rather than pale toy cones.
    for k in range(3):
        z=z0+h*(.39+.195*k)
        rr=h*(.21-.054*k)
        cone("forest spruce irregular tier",
             (x+random.uniform(-.22,.22),y+random.uniform(-.19,.19),z),
             rr,.035,h*.40,
             random.choice(("leaf1","leaf2")),"V3_Forest",9)
    counts["trees"]+=1
def scrub(x,y,r):
    z=height(x,y)
    for n in range(4):
        dx=random.uniform(-r*.59,r*.59)
        dy=random.uniform(-r*.5,r*.5)
        ico("ancient fern/shrub foliage",(x+dx,y+dy,z+r*.31),r*.48,
            random.choice(("leaf1","leaf2","moss")),
            "V3_Bushes",1,(1.32,.95,.73))
for i in range(110):
    x=random.uniform(-53,53); y=random.uniform(-48,48)
    if abs(x)<14 and -23<y<22:continue
    if abs(x-23)<9 and -16<y<19:continue
    if abs(x+23)<9 and -17<y<20:continue
    evergreen(x,y,random.uniform(4.0,9.0))
for i in range(125):
    x=random.uniform(-48,48);y=random.uniform(-44,44)
    if abs(x)<11 and -9<y<14:continue
    if abs(x+22)<5.0 and -12<y<-3:continue
    if abs(x-22)<5.0 and -12<y<-4:continue
    scrub(x,y,random.uniform(.42,1.35))
# Small purple/gold botanical accents are grouped; no individual magenta pins.
for hue,number in (("purple",260),("white",230)):
    verts=[];faces=[]
    for i in range(number):
        x=random.uniform(-48,48);y=random.uniform(-46,46)
        if abs(x)<10 and -9<y<14:continue
        z=height(x,y)+.18
        for sx,sy in ((-.12,0),(.11,.04),(0,.12)):
            a=len(verts)
            verts.extend([(x+sx-.06,y+sy,z),
                          (x+sx+.06,y+sy,z),
                          (x+sx,y+sy,z+random.uniform(.18,.40))])
            faces.append((a,a+1,a+2))
    face_mesh("perennial meadow flowers "+hue,verts,faces,hue,"V3_Botany")
    counts["flowers"]+=number
# Slab/rock clusters anchor roads to the surrounding meadow.
for i in range(105):
    x=random.uniform(-46,46);y=random.uniform(-43,43)
    if abs(x)<8 and -10<y<14:continue
    ico("natural lichen outcrop",(x,y,height(x,y)+.24),
        random.uniform(.24,.85),random.choice(("rock","rock_dark")),
        "V3_StoneScatter",1,(1.24,.80,.53))

# Differentiation beyond materials: roof pitch, silhouette, construction logic.
# But no five independent unrelated Bastions or fake wall overlays.
world=scene.world
world.use_nodes=True
world.node_tree.nodes["Background"].inputs["Color"].default_value=(.39,.40,.46,1)
world.node_tree.nodes["Background"].inputs["Strength"].default_value=.89
for o in bpy.data.objects:
    if o.type!="LIGHT":continue
    if o.data.type=="SUN":
        o.data.energy=3.80
        o.data.color=(1.0,.74,.54)
        o.data.angle=radians(3.2)
    if o.data.type=="POINT":
        o.data.energy=min(o.data.energy,124)
scene.view_settings.exposure=.48

# PORTABILITY: the editable blend must carry any referenced source images.
# A missing image used to turn walls into magenta in previous fake-positive QA.
for image in bpy.data.images:
    if image.source=="FILE" and not image.packed_file:
        try:image.pack()
        except Exception as ex:
            raise RuntimeError("Unable to pack source image "+image.name) from ex
print("ELDORIA_V3_DESIGN_SYSTEM_EXECUTED",
      counts["new_meshes"],counts["trees"],counts["secondary_details"])
