"""Original editable crown authoring, deterministic hosted calibration.

Only a bounded declarative design file is consumed. No network, model Python,
third-party assets, game files or paid tooling. Use actual mesh/curve geometry,
layered materials and a cinematic camera. Not a live independent agent result.
"""
import bpy,math,json,sys,pathlib,hashlib
from mathutils import Vector
from math import sin,cos,pi
P=pathlib.Path
args=sys.argv[sys.argv.index("--")+1:]
plan=json.loads(P(args[0]).read_text());out=P(args[1]);out.mkdir(parents=True,exist_ok=False)
D=plan["design"]
assert plan["card_id"]=="corona-valoria" and plan["art_approved"] is False
def bounds(x,lo,hi):
    assert type(x) in (int,float) and math.isfinite(x) and lo<=x<=hi
    return float(x)
R=bounds(D["radius"],1.35,1.8);ey=bounds(D["ellipticity"],.7,1);H=bounds(D["band_height"],.42,.68);T=bounds(D["thickness"],.08,.18)
peaks=[bounds(x,.3,1.2) for x in D["point_heights"]];assert len(peaks)==8
def rgb(k):
    c=D[k];assert len(c)==3
    return tuple(bounds(x,0,1) for x in c)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
def mat(name,base,metal=0,rough=.5,noise=0,bump=0):
    m=bpy.data.materials.new(name);m.diffuse_color=(*base,1);m.use_nodes=True
    nodes=m.node_tree.nodes;links=m.node_tree.links;bs=nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*base,1);bs.inputs["Metallic"].default_value=metal;bs.inputs["Roughness"].default_value=rough
    if noise:
        tex=nodes.new("ShaderNodeTexNoise");tex.inputs["Scale"].default_value=37;tex.inputs["Detail"].default_value=4
        ramp=nodes.new("ShaderNodeValToRGB")
        ramp.color_ramp.elements[0].position=.18;ramp.color_ramp.elements[0].color=(*(max(0,v*(1-noise)) for v in base),1)
        ramp.color_ramp.elements[1].position=.83;ramp.color_ramp.elements[1].color=(*(min(1,v*(1+noise)) for v in base),1)
        links.new(tex.outputs["Fac"],ramp.inputs[0]);links.new(ramp.outputs["Color"],bs.inputs["Base Color"])
        b=nodes.new("ShaderNodeBump");b.inputs["Strength"].default_value=.22;b.inputs["Distance"].default_value=bump;links.new(tex.outputs["Fac"],b.inputs["Height"]);links.new(b.outputs["Normal"],bs.inputs["Normal"])
    return m
gold=mat("Valoria • chased antique gold",rgb("gold_rgb"),.87,.29,.30,.018)
rim=mat("Polished hammered gold edges",(.82,.49,.15),.92,.23,.10,.007)
darkgold=mat("Oxidised recesses",(.21,.087,.022),.72,.45,.4,.018)
enamel=mat("Midnight amethyst enamel",rgb("enamel_rgb"),.28,.3,.16,.008)
purple=mat("Faceted royal amethyst",rgb("gem_rgb"),.15,.16)
amber=mat("Faceted ember amber",rgb("amber_rgb"),.14,.18)
for m in [purple,amber]:
    bs=m.node_tree.nodes.get("Principled BSDF")
    for k in ["Coat Weight","Clearcoat"]:
        if k in bs.inputs:bs.inputs[k].default_value=.45
    for k in ["Emission Color","Emission"]:
        if k in bs.inputs:bs.inputs[k].default_value=(*m.diffuse_color[:3],1)
    if "Emission Strength" in bs.inputs:bs.inputs["Emission Strength"].default_value=.07
stone=mat("Ancient black slate",(.046,.052,.067),.05,.72,.35,.045)
velvet=mat("Royal violet velvet",(.032,.012,.055),0,.85,.25,.016)
def mesh(name,v,f,m,bevel=0,smooth=False):
    data=bpy.data.meshes.new(name);data.from_pydata(v,[],f);data.update()
    obj=bpy.data.objects.new(name,data);scene.collection.objects.link(obj);obj.data.materials.append(m)
    if smooth:
        for p in data.polygons:p.use_smooth=True
    if bevel:
        mod=obj.modifiers.new("Hand-polished edges","BEVEL");mod.width=bevel;mod.segments=3
        mod=obj.modifiers.new("Weighted material normals","WEIGHTED_NORMAL")
    return obj
def polar(theta,r,z):
    return (r*cos(theta),ey*r*sin(theta),z)
def closed_profile(name,profile,material,n=192):
    v=[polar(2*pi*i/n,r,z) for r,z in profile for i in range(n)]
    f=[(a*n+i,a*n+(i+1)%n,((a+1)%len(profile))*n+(i+1)%n,((a+1)%len(profile))*n+i) for a in range(len(profile)) for i in range(n)]
    return mesh(name,v,f,material,0,True)
z0=.68
closed_profile("Crown band • multi-level forged profile",[(R-T,z0),(R+.01,z0),(R+.035,z0+.045),(R+.02,z0+.12),(R-.035,z0+H-.12),(R+.015,z0+H-.05),(R-.01,z0+H),(R-T,z0+H)],gold)
for z,rad in [(z0+.052,R+.04),(z0+.12,R+.025),(z0+H-.064,R+.018)]:
    closed_profile("Engraved rim • gold",[(rad-.015,z-.02),(rad+.013,z-.02),(rad+.026,z),(rad+.013,z+.02),(rad-.015,z+.02)],rim)
closed_profile("Recessed amethyst band",[(R-.025,z0+.16),(R+.003,z0+.16),(R-.027,z0+H-.14),(R-.06,z0+H-.14)],enamel)
# Eight individually contoured, solid raised crown leaves with profile thickness.
# Angular shoulders and rounded cusp design rather than stacked primitive cones.
def plaque(theta,width,base,height,outline,material,thick,offset=0,name="Forged leaf"):
    v=[]
    for layer in (0,1):
        for u,w in outline:
            t=theta+u*width
            radius=R+offset+(0.075+0.05*w)*w-layer*thick
            v.append(polar(t,radius,base+height*w))
    n=len(outline);f=[tuple(range(n)),tuple(range(2*n-1,n-1,-1))]
    f +=[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    return mesh(name,v,f,material,.014)
# Real central front point at -Y, intentionally tallest.
# Geometry drawn with layered arch, split angular crown leaves, inset enamel.
front=-pi/2
order=[2,3,4,5,6,7,0,1]
outer=[(-1,0),(-.92,.20),(-.68,.32),(-.70,.46),(-.45,.58),(-.52,.71),(-.23,.81),(0,1),(.23,.81),(.52,.71),(.45,.58),(.70,.46),(.68,.32),(.92,.20),(1,0)]
inner=[(-.58,.12),(-.54,.28),(-.31,.45),(-.28,.63),(0,.85),(.28,.63),(.31,.45),(.54,.28),(.58,.12)]
def tube(name,pts,material,r=.015):
    curve=bpy.data.curves.new(name,"CURVE");curve.dimensions="3D";curve.resolution_u=12;curve.bevel_depth=r;curve.bevel_resolution=3
    spline=curve.splines.new("POLY");spline.points.add(len(pts)-1)
    for p,v in zip(spline.points,pts):p.co=(*v,1)
    o=bpy.data.objects.new(name,curve);scene.collection.objects.link(o);curve.materials.append(material);return o
def facet(name,center,rx,rz,depth,theta,material,n=10):
    # Multi-faceted elliptical cut gemstone with table, crown and pavilion.
    normal=Vector((cos(theta),sin(theta),0));tangent=Vector((-sin(theta),cos(theta),0));up=Vector((0,0,1));c=Vector(center)
    vv=[]
    for scale,d in [(0.72,-depth*.36),(1,0),(.57,depth*.40)]:
        for i in range(n):
            q=2*pi*i/n;vv.append(tuple(c+tangent*(rx*scale*cos(q))+up*(rz*scale*sin(q))+normal*d))
    ff=[tuple(range(n-1,-1,-1)),tuple(range(2*n,3*n))]
    for ring in range(2):
        for i in range(n):
            j=(i+1)%n;a=ring*n+i;b=ring*n+j;c1=(ring+1)*n+j;d=(ring+1)*n+i
            ff.extend([(a,b,c1),(a,c1,d)])
    return mesh(name,vv,ff,material)
for idx,k in enumerate(order):
    t=front+idx*pi/4;h=peaks[k]
    plaque(t,.36,z0+H-.02,h,outer,gold,.11,.003,"Authored pointed crown leaf "+str(idx+1))
    plaque(t,.36,z0+H-.02,h,inner,enamel,.025,.097,"Recessed enamel shield "+str(idx+1))
    pts=[polar(t+u*.36,R+.102+.052*w,z0+H-.02+h*w) for u,w in inner+[inner[0]]]
    tube("Chased inlaid leaf edge "+str(idx+1),pts,rim,.016)
    # Central amethyst and smaller amber/amethyst accents.
    isfront=idx==0
    z=z0+H+h*(.46 if isfront else .42);rx=.205 if isfront else .115;rz=.28 if isfront else .16
    center=polar(t,R+.205,z)
    facet("Gold bezel "+str(idx+1),polar(t,R+.15,z),rx*1.22,rz*1.18,.13,t,rim,12)
    facet("Royal stone "+str(idx+1),center,rx,rz,.20 if isfront else .12,t,purple if isfront or idx%2==0 else amber,12)
    # Small embossed tendrils trace local crown contour.
    for side in (-1,1):
        curve=[]
        for n in range(28):
            v=n/27
            theta=t+side*(.055+.16*v+.035*sin(v*2*pi))
            zz=z0+.19+(H-.34)*v+.02*sin(v*4*pi)
            curve.append(polar(theta,R+.034,zz))
        tube("Curved chased gold relief",curve,rim,.012)
    facet("Band rivet",polar(t+pi/8,R+.065,z0+.26),.056,.056,.06,t+pi/8,amber,8)
# Sculptural Valoria castle heraldry in relief on crown front band, separate authored mesh.
outline=[(-.18,-.11),(-.18,.10),(-.13,.10),(-.13,.17),(-.08,.17),(-.08,.10),(-.027,.10),(-.027,.20),(.027,.20),(.027,.10),(.08,.10),(.08,.17),(.13,.17),(.13,.10),(.18,.10),(.18,-.11)]
cv=[]
for depth in (0,.04):
    for x,z in outline:cv.append((x,-ey*(R+.045)-depth,z0+.27+z*.60))
n=len(outline);cf=[tuple(range(n)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
mesh("Relief • Valoria bastion heraldry",cv,cf,rim,.008)
# Deep inset velvet lining, crown preserved as an actual hollow object.
closed_profile("Velvet inner lining",[(R-T-.06,z0+.08),(R-T-.018,z0+.08),(R-T-.04,z0+H-.09),(R-T-.09,z0+H-.09)],velvet)
def cylinder(name,radius,depth,z,material,vertices=96):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=(0,0,z))
    o=bpy.context.object;o.name=name;o.data.materials.append(material)
    mod=o.modifiers.new("Stone chamfer","BEVEL");mod.width=.065;mod.segments=3;o.modifiers.new("Normals","WEIGHTED_NORMAL")
    return o
cylinder("Octagonal relic plinth",1.93,.30,.48,stone,12)
cylinder("Lower step",2.06,.18,.24,stone,12)
cylinder("Velvet resting pad",1.70,.065,.655,velvet,96)
# Fabric behind the crown: broad, deliberately quiet billows, subdued background.
nx,ny=100,45;vv=[];ff=[]
for iy in range(ny+1):
    y=-2.6+iy/ny*7.0
    for ix in range(nx+1):
        x=-7+ix/nx*14
        zz=-.06+.035*sin(x*5+y*1.8)+.045*sin(x*2.1+y*2.8)
        vv.append((x,y,zz))
for iy in range(ny):
    for ix in range(nx):
        a=iy*(nx+1)+ix;ff.append((a,a+1,a+nx+2,a+nx+1))
mesh("Low contrast velvet ground",vv,ff,velvet,0,True)
# Large bokeh architectural background, no other game object or copied image.
bpy.ops.mesh.primitive_plane_add(size=30,location=(0,5,-.12))
back=bpy.context.object;back.name="Dark slate studio ground";back.data.materials.append(stone)
world=bpy.data.worlds.new("Quiet twilight");scene.world=world;world.use_nodes=True
world.node_tree.nodes["Background"].inputs["Color"].default_value=(.105,.095,.17,1);world.node_tree.nodes["Background"].inputs["Strength"].default_value=.42
def light(name,pos,power,col,size,target=(0,0,1.35)):
    data=bpy.data.lights.new(name,"AREA");data.energy=power;data.color=col;data.shape="DISK";data.size=size
    obj=bpy.data.objects.new(name,data);scene.collection.objects.link(obj);obj.location=pos;obj.rotation_euler=(Vector(target)-obj.location).to_track_quat("-Z","Y").to_euler()
light("Large warm key",(-3.2,-4.4,7),1050,(1,.78,.50),4.3)
light("Cool royal fill",(5,-2,3.6),560,(.62,.72,1),3.5)
light("Amethyst back rim",(1.0,4.2,5.2),1050,(.68,.38,1),2.8)
light("Small warm metal edge",(-4.4,1.5,3.1),620,(1,.51,.25),1.4)
camd=bpy.data.cameras.new("Card illustration orthographic camera");cam=bpy.data.objects.new("Card illustration camera",camd);scene.collection.objects.link(cam)
cam.location=tuple(bounds(x,-15,15) for x in D["camera"]["position"]);target=Vector(tuple(bounds(x,-4,4) for x in D["camera"]["target"]))
cam.rotation_euler=(target-cam.location).to_track_quat("-Z","Y").to_euler();camd.type="ORTHO";camd.ortho_scale=bounds(D["camera"]["scale"],4.3,5.6);scene.camera=cam
scene.render.engine="CYCLES";scene.cycles.samples=40;scene.cycles.use_denoising=True;scene.cycles.max_bounces=6
scene.render.resolution_x=768;scene.render.resolution_y=864;scene.render.resolution_percentage=100
scene.render.image_settings.file_format="PNG";scene.render.image_settings.color_mode="RGB";scene.render.film_transparent=False
try:scene.view_settings.view_transform="AgX"
except:scene.view_settings.view_transform="Filmic"
scene.view_settings.exposure=.35
scene.render.filepath=str(out/"illustration.png")
bpy.ops.wm.save_as_mainfile(filepath=str(out/"crown.blend"))
bpy.ops.render.render(write_still=True)
assert (out/"illustration.png").stat().st_size>10000
objects=[{"name":o.name,"vertices":len(o.data.vertices),"faces":len(o.data.polygons)} for o in scene.objects if o.type=="MESH"]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
proof={"schema":1,"source":"actual direct bpy on isolated hosted runner","authorship":"current assistant calibration, NOT paid independent roles","blender_version":bpy.app.version_string,"blend_sha256":sha(out/"crown.blend"),"illustration_sha256":sha(out/"illustration.png"),"plan_sha256":sha(P(args[0])),"editable_meshes":len(objects),"geometry":objects,"materials":sorted(x.name for x in bpy.data.materials),"render_px":[768,864],"blender_mcp_executed":False,"unity_mcp_executed":False,"new_api_calls":0,"art_approved":False}
(out/"blender-proof.json").write_text(json.dumps(proof,indent=2))
print("RELICARIO_REAL_BLENDER_PROOF "+json.dumps({k:v for k,v in proof.items() if k!="geometry"}))
