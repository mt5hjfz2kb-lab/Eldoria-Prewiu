"""ChatGPT-authored isolated barracks study. No Qwen authorship or Unity approval claimed."""
import bpy, math, random, os, json, hashlib
from mathutils import Vector
import numpy as np
random.seed(4215)
OUT=os.path.abspath('art-studies/chatgpt-cuartel-20261015')
os.makedirs(OUT,exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)

def material(name,color,rough=.7,metal=0,texture=None):
    m=bpy.data.materials.new(name); m.diffuse_color=(*color,1); m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=(*color,1)
    p.inputs['Roughness'].default_value=rough; p.inputs['Metallic'].default_value=metal
    if texture:
        n=m.node_tree.nodes.new('ShaderNodeTexImage'); n.image=texture
        m.node_tree.links.new(n.outputs['Color'],p.inputs['Base Color'])
    return m

def image_texture(name,c,kind):
    n=512; y,x=np.mgrid[0:n,0:n]; rng=np.random.default_rng(15)
    if kind=='cloth':
        v=1+.033*np.sin(x*math.pi)+.025*np.sin(y*math.pi/1.5)+rng.normal(0,.018,(n,n))
    else:
        v=.88+.10*np.sin(x*.16+2*np.sin(y*.014))+.06*np.sin(x*.58+np.sin(y*.026))+rng.normal(0,.025,(n,n))
    px=np.ones((n,n,4),dtype=np.float32)
    for k in range(3):px[:,:,k]=np.clip(c[k]*v,0,1)
    im=bpy.data.images.new(name,n,n); im.pixels.foreach_set(px.ravel()); im.pack(); return im

blue=material('Royal blue woven canvas',(.025,.10,.29),.86,texture=image_texture('Canvas woven albedo',(.035,.14,.38),'cloth'))
blue_dark=material('Canvas shadow blue',(.024,.07,.18),.87)
gold=material('Ochre gold embroidered edging',(.70,.40,.105),.64)
wood=material('Weathered structural oak',(.20,.095,.035),.84,texture=image_texture('Oak grain albedo',(.31,.17,.075),'wood'))
endwood=material('Pale end grain',(.34,.20,.095),.9)
iron=material('Forged iron',(.065,.082,.093),.46,.65)
steel=material('Blade steel',(.34,.41,.44),.35,.75)
rope=material('Hemp ropes',(.48,.36,.19),.94)
earth=material('Trampled earth',(.21,.135,.065),1)
stone=material('Warm broken stone',(.36,.33,.26),.97)
grass=material('Dry moss green',(.16,.19,.075),1)
straw=material('Training sack and straw',(.48,.34,.16),.94)
dark=material('Interior deep shade',(.014,.022,.031),1)
asset=[]
def tag(o,mat):
    if mat:o.data.materials.append(mat)
    asset.append(o);return o
def mesh(name,verts,faces,mat,uv=None):
    d=bpy.data.meshes.new(name);d.from_pydata(verts,[],faces);d.update()
    o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o);tag(o,mat)
    if uv:
        u=d.uv_layers.new(name='UVMap')
        for p in d.polygons:
            for i in p.loop_indices:u.data[i].uv=uv[d.loops[i].vertex_index]
    return o
def cube(name,loc,scale,mat,bev=.025):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=bpy.context.object;o.name=name;o.dimensions=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);tag(o,mat)
    if bev:
        b=o.modifiers.new('Rounded worn edges','BEVEL');b.width=bev;b.segments=2
        o.modifiers.new('Weighted corner normals','WEIGHTED_NORMAL')
    return o
def beam(name,a,b,r,mat,vertices=10):
    a,b=Vector(a),Vector(b);v=b-a
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=v.length,location=(a+b)/2)
    o=bpy.context.object;o.name=name;o.rotation_euler=v.to_track_quat('Z','Y').to_euler();return tag(o,mat)
def line(name,points,r,mat):
    d=bpy.data.curves.new(name,'CURVE');d.dimensions='3D';d.resolution_u=1;d.bevel_depth=r;d.bevel_resolution=2
    s=d.splines.new('POLY');s.points.add(len(points)-1)
    for p,co in zip(s.points,points):p.co=(*co,1)
    o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o);return tag(o,mat)
def ico(name,loc,scale,mat,sub=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,radius=1,location=loc);o=bpy.context.object;o.name=name;o.scale=scale;return tag(o,mat)

# Thin irregular terrain patch: open footprint, no oversized stone pedestal.
n=52;rs=[1+random.uniform(-.035,.035) for i in range(n)]
v=[(0,0,.015)]+[(5.65*math.cos(i*2*math.pi/n)*rs[i],4.25*math.sin(i*2*math.pi/n)*rs[i],-.005) for i in range(n)]
base=mesh('Removable dirt ground patch',v,[(0,i+1,(i+1)%n+1) for i in range(n)],earth)
so=base.modifiers.new('Ground thickness','SOLIDIFY');so.thickness=.07

def tent(cx,cy,w,L,H,label):
    z0=.06; eave=H*.39; apex=H
    # Thick timber ridge poles actually support the textile.
    for yy in [-L*.47,L*.47]:
        beam(label+' ridge support',(cx,cy+yy,z0),(cx,cy+yy,H+.1),.055,wood)
    beam(label+' ridge beam',(cx,cy-L*.51,H),(cx,cy+L*.51,H),.06,wood)
    def surf(side,u,t):
        x=cx+side*w*u
        y=cy+L*(t-.5)
        z=apex*(1-u)+eave*u-.13*math.sin(math.pi*u)+.024*math.sin(t*math.pi*16)*u
        return (x,y,z)
    for side in [-1,1]:
        for panel in range(8):
            verts=[];uv=[]; nu=12;nv=6
            for i in range(nu+1):
                for j in range(nv+1):
                    u=i/nu;t=(panel+j/nv)/8;verts.append(surf(side,u,t));uv.append((u,t))
            faces=[]
            for i in range(nu):
                for j in range(nv):
                    a=i*(nv+1)+j;faces.append((a,a+nv+1,a+nv+2,a+1))
            o=mesh(label+' tailored roof panel',verts,faces,blue,uv)
            sol=o.modifiers.new('Canvas thickness','SOLIDIFY');sol.thickness=.012
            for p in o.data.polygons:p.use_smooth=True
        for t in [0,.25,.5,.75,1]:
            line(label+' raised stitched roof seam',[surf(side,i/24,t) for i in range(25)],.009,gold if t in [0,1] else blue_dark)
        line(label+' gold eave braid',[surf(side,1,i/80) for i in range(81)],.025,gold)
        # Pleated hanging walls beneath the roof, with scalloped gold hem.
        verts=[];uv=[]
        for j in range(65):
            t=j/64;y=cy+L*(t-.5);xx=cx+side*(w-.04+.025*math.sin(t*32*math.pi))
            for k in [0,1]:verts.append((xx,y,.09 if k==0 else eave));uv.append((t,k))
        mesh(label+' pleated side canvas',verts,[(2*j,2*j+1,2*j+3,2*j+2) for j in range(64)],blue,uv)
        for yy in [-L*.48,0,L*.48]:
            start=(cx+side*w,cy+yy,eave+.01);end=(cx+side*(w+.62),cy+yy+(.20 if yy>0 else -.20),.045)
            mid=tuple((start[k]+end[k])/2 for k in range(3));mid=(mid[0],mid[1],mid[2]-.07)
            line(label+' tensioned guy rope',[start,mid,end],.014,rope)
            beam(label+' driven stake',(end[0],end[1],-.02),(end[0]-.05*side,end[1],.24),.025,wood)
        # Front fabric tied back to expose a real opening and dark interior.
        yf=cy-L*.505
        points=[(cx,yf,H),(cx+side*w,yf,eave),(cx+side*(w-.08),yf,.10),(cx+side*.55,yf+.18,.10),(cx+side*.62,yf+.10,H*.64)]
        o=mesh(label+' gathered front flap',points,[(0,1,4),(1,2,3,4)],blue,[(.5,1),(1,.4),(1,0),(.6,0),(.6,.65)])
        line(label+' flap gold edge',[points[0],points[4],points[3]],.022,gold)
        line(label+' flap tie',[(cx+side*.60,yf,.65),(cx+side*(w-.08),yf,.78)],.02,rope)
    # Back triangular textile closure and floor; open front remains geometrically open.
    yb=cy+L*.498
    mesh(label+' back canvas',[(cx-w,yb,.05),(cx+w,yb,.05),(cx+w,yb,eave),(cx,yb,H),(cx-w,yb,eave)],[(0,1,2,3,4)],blue,[(0,0),(1,0),(1,.4),(.5,1),(0,.4)])
    cube(label+' floor mat',(cx,cy,.032),(w*1.9,L*.98,.035),dark,0)
    # Entrance decking makes access and ground contact readable.
    for i in range(7):cube(label+' entrance oak plank',(cx+(i-3)*.19,cy-L/2-.33,.055),(.18,.69,.065),wood,.012)
    # Front and back ridge finials.
    for yy in [-L*.51,L*.51]:
        beam(label+' finial',(cx,cy+yy,H-.02),(cx,cy+yy,H+.26),.027,gold)
        ico(label+' finial knob',(cx,cy+yy,H+.27),(.065,.065,.09),gold,2)

tent(1.15,1.15,1.62,3.65,3.24,'Command pavilion')
tent(-2.55,1.85,.95,2.25,1.98,'Recruit shelter')

def fence_segment(a,b):
    for t in [0,.5,1]:
        p=Vector(a).lerp(Vector(b),t);cube('Oak fence post',(p.x,p.y,.50),(.12,.12,1.0),wood,.017)
    for z in [.32,.72]:beam('Split oak fence rail',(a[0],a[1],z),(b[0],b[1],z),.044,wood,6)
for a,b in [((-4,3.4),(-1,3.4)),((-1,3.4),(2,3.4)),((2,3.4),(4,3.4)),((-4,3.4),(-4,.2)),((4,3.4),(4,.3)),((-4,-2.4),(-2.4,-2.4)),((2.5,-2.4),(4,-2.4))]:fence_segment(a,b)

def shield(x,y,z,scale=1):
    # Curved heater shield, gold border and central cross; faces bow forward.
    outline=[(-.27, .37),(.27,.37),(.27,.03),(.17,-.23),(0,-.40),(-.17,-.23),(-.27,.03)]
    verts=[(x,y-.07,z)]+[(x+u*scale,y,z+v*scale) for u,v in outline]
    o=mesh('Bowed blue heater shield',verts,[(0,i+1,(i+1)%7+1) for i in range(7)],blue)
    sol=o.modifiers.new('Shield thickness','SOLIDIFY');sol.thickness=.035
    line('Shield golden rim',[verts[i+1] for i in range(7)]+[verts[1]],.018*scale,gold)
    cube('Heraldic upright',(x,y-.081,z+.03*scale),(.052*scale,.022,.43*scale),gold,.008)
    cube('Heraldic crossbar',(x,y-.084,z+.10*scale),(.30*scale,.022,.052*scale),gold,.008)

def spear(x,y,h):
    beam('Ash spear shaft',(x,y,.08),(x,y,h),.025,wood)
    mesh('Forged spear point',[(x,y,h+.31),(x-.065,y,h+.11),(x,y-.035,h+.08),(x+.065,y,h+.11),(x,y+.035,h+.08)],[(0,1,2),(0,2,3),(0,3,4),(0,4,1),(1,4,3,2)],steel)
    beam('Spear socket',(x,y,h-.03),(x,y,h+.13),.036,iron)

# Weapons rack facing the courtyard, physically connected feet/rails.
for x in [-3.45,-1.8]:
    cube('Rack upright',(x,.05,.68),(.105,.105,1.36),wood)
    cube('Rack stable foot',(x,.05,.07),(.16,.66,.12),wood)
beam('Rack upper crossbar',(-3.45,.05,1.18),(-1.8,.05,1.18),.06,wood)
for i in range(5):spear(-3.30+i*.27,.04,1.45+(.09 if i%2 else 0))
shield(-3.0,-.13,.68,.8);shield(-2.15,-.13,.67,.8)

# Training dummy and straw base on the left; clear central circulation.
dx,dy=-2.8,-1.45
beam('Training post',(dx,dy,0),(dx,dy,1.65),.07,wood)
beam('Dummy shoulder spar',(dx-.46,dy,1.12),(dx+.46,dy,1.12),.055,wood)
ico('Stuffed training torso',(dx,dy,1.02),(.24,.17,.37),straw,2)
ico('Stuffed training head',(dx,dy,1.53),(.14,.13,.16),straw,2)
for rr in [.07,.15]:
    line('Training target ring',[(dx+rr*math.cos(t*2*math.pi/40),dy-.18,1.05+rr*math.sin(t*2*math.pi/40)) for t in range(41)],.012,rope)
for i in range(13):
    a=random.random()*6.28;r=random.random()*.4
    beam('Fallen straw',(dx+r*math.cos(a),dy+r*math.sin(a),.025),(dx+r*math.cos(a)+.14,dy+r*math.sin(a)+.06,.025),.006,straw,4)

# Banner with actual ripples; restrained readable gold cross, no invented lettering.
bx,by=-.65,.55
beam('Standard pole',(bx,by,0),(bx,by,3.30),.038,wood)
beam('Standard crossbar',(bx-.04,by,3.04),(bx+.72,by,3.04),.025,gold)
ico('Standard finial',(bx,by,3.34),(.07,.07,.12),gold,2)
verts=[];uv=[]
for j in range(19):
    for i in range(13):
        u=i/12;v=j/18
        verts.append((bx+.07+u*.59,by-.035+.065*math.sin(u*8+v*2)*v,3.03-v*1.15+.045*math.sin(u*7)*v));uv.append((u,v))
faces=[]
for j in range(18):
    for i in range(12):a=j*13+i;faces.append((a,a+1,a+14,a+13))
mesh('Wind-rippled royal standard',verts,faces,blue,uv)
cube('Standard gold vertical',(bx+.36,by-.078,2.48),(.055,.022,.47),gold,.009)
cube('Standard gold crossbar',(bx+.36,by-.080,2.55),(.31,.022,.055),gold,.009)
line('Banner gold bottom braid',[verts[-13+i] for i in range(13)],.016,gold)

# Supply chest: plank lid, iron straps and lock.
cx,cy=3.05,1.25
cube('Supply chest body',(cx,cy,.29),(.85,.55,.52),wood,.04)
for i in range(5):cube('Chest lid board',(cx+(i-2)*.17,cy,.58),(.162,.59,.075),wood,.014)
for xx in [cx-.27,cx+.27]:cube('Iron chest band',(xx,cy,.627),(.055,.60,.025),iron,.007)
cube('Chest lock',(cx,cy-.30,.43),(.12,.034,.14),iron,.014)
shield(3.28,.03,.59,.95)

# Brazier in foreground right, low warm source, flame is preview only.
fx,fy=1.55,-1.35
for a in [0,2.094,4.189]:beam('Brazier splayed leg',(fx+.22*math.cos(a),fy+.22*math.sin(a),.03),(fx+.13*math.cos(a),fy+.13*math.sin(a),.45),.025,iron)
bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=8,radius=1,location=(fx,fy,.49));o=bpy.context.object;o.name='Iron fire bowl';o.scale=(.29,.29,.12);tag(o,iron)
coal=material('Warm embers',(.40,.055,.006),.9)
p=coal.node_tree.nodes.get('Principled BSDF'); inp=p.inputs.get('Emission') or p.inputs.get('Emission Color');inp.default_value=(1,.10,.004,1)
if p.inputs.get('Emission Strength'):p.inputs['Emission Strength'].default_value=2
for i in range(8):ico('Glowing coal',(fx+random.uniform(-.15,.15),fy+random.uniform(-.15,.15),.60),(.06,.05,.035),coal,1)
flame=material('Preview flame',(.95,.27,.015),.7);p=flame.node_tree.nodes.get('Principled BSDF');inp=p.inputs.get('Emission') or p.inputs.get('Emission Color');inp.default_value=(1,.23,.005,1)
if p.inputs.get('Emission Strength'):p.inputs['Emission Strength'].default_value=3
for i in range(3):ico('Preview stylized flame',(fx+random.uniform(-.09,.09),fy+random.uniform(-.09,.09),.72+i*.06),(.065-i*.012,.065-i*.012,.18-i*.035),flame,2)

# Sparse set dressing at the perimeter, rather than noisy vegetation everywhere.
for i in range(30):
    a=random.uniform(0,6.28);x=5.1*math.cos(a);y=3.75*math.sin(a)
    if i%3==0:ico('Perimeter stone',(x,y,.07),(random.uniform(.08,.18),random.uniform(.07,.14),random.uniform(.05,.10)),stone,1)
    else:
        for j in range(3):
            xx=x+random.uniform(-.12,.12);yy=y+random.uniform(-.12,.12)
            mesh('Dry grass blade',[(xx-.015,yy,.01),(xx+.015,yy,.01),(xx+.05,yy+.025,random.uniform(.10,.19))],[(0,1,2)],grass)

# Convert bevels, curves, normals to actual exportable geometry. UV every mesh.
for o in list(asset):
    bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
    if o.type=='CURVE':bpy.ops.object.convert(target='MESH')
    if o.type=='MESH':
        for mod in list(o.modifiers):
            try:bpy.ops.object.modifier_apply(modifier=mod.name)
            except RuntimeError:pass
        if not o.data.uv_layers:
            bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(island_margin=.03);bpy.ops.object.mode_set(mode='OBJECT')

# Export asset without presentation lighting/background. Meter units, ground pivot.
bpy.ops.object.select_all(action='DESELECT')
for o in asset:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=OUT+'/Cuartel_ChatGPT_v1.glb',export_format='GLB',use_selection=True,export_yup=True)

# Presentation scene: genuine Blender Cycles renders.
stage=material('Presentation backdrop',(.105,.13,.14),.95)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.095));bpy.context.object.name='PREVIEW background only';bpy.context.object.data.materials.append(stage)
world=bpy.context.scene.world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.36,.46,.62,1);world.node_tree.nodes['Background'].inputs[1].default_value=.32
def area(name,loc,power,color,size):
    d=bpy.data.lights.new(name,'AREA');d.energy=power;d.color=color;d.size=size;o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o);o.location=loc;o.rotation_euler=(Vector((0,0,0))-o.location).to_track_quat('-Z','Y').to_euler()
area('Warm late afternoon key',(-5,-5,9),1800,(1,.77,.51),5)
area('Cool sky fill',(3,5,7),1400,(.47,.66,1),6)
ld=bpy.data.lights.new('Brazier warm light','POINT');ld.energy=35;ld.color=(1,.25,.04);lo=bpy.data.objects.new('Brazier warm light',ld);bpy.context.collection.objects.link(lo);lo.location=(fx,fy,.85)
bpy.ops.object.camera_add();cam=bpy.context.object;cam.name='Review camera';cam.data.type='ORTHO';bpy.context.scene.camera=cam
sc=bpy.context.scene;sc.render.engine='CYCLES';sc.cycles.samples=32;sc.cycles.use_denoising=True;sc.render.image_settings.file_format='PNG';sc.render.film_transparent=False
sc.view_settings.view_transform='Standard';sc.view_settings.look='Medium High Contrast';sc.view_settings.exposure=0;sc.view_settings.gamma=1
def render(name,pos,scale,w,h):
    cam.location=pos;cam.rotation_euler=(Vector((0,.25,1.05))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=scale
    sc.render.resolution_x=w;sc.render.resolution_y=h;sc.render.resolution_percentage=100;sc.render.filepath=OUT+'/'+name+'.png';bpy.ops.render.render(write_still=True)
render('cuartel-hero',(10,-15,11),12.6,1400,1000)
render('cuartel-strategic',(10,-16,18),13.2,1200,900)
render('cuartel-rear',(-11,12,10),12.7,1100,850)
cam.location=(10,-15,11);cam.rotation_euler=(Vector((0,.25,1.05))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=12.6
bpy.ops.wm.save_as_mainfile(filepath=OUT+'/Cuartel_ChatGPT_v1.blend')
stats={'author':'ChatGPT, this conversation; explicit owner request 15 Oct 2026','execution':bpy.app.version_string,'qwen_used':False,'credits_spent':0,'unity_verified':False,'art_approved':False,'calibration_validated':False,'status':'EXPERIMENTAL_ASSET_REQUIRES_INDEPENDENT_REVIEW','objects':len(asset),'vertices':sum(len(o.data.vertices) for o in asset if o.type=='MESH'),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in asset if o.type=='MESH'),'reference_sha256':hashlib.sha256(open('references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg','rb').read()).hexdigest(),'design':'Compact early Bastion military camp: blue canvas ridge pavilion, smaller recruit shelter, oak fencing, open training yard, weapons and royal standard. Frozen existing assets unchanged.'}
stats['files']={f:hashlib.sha256(open(OUT+'/'+f,'rb').read()).hexdigest() for f in os.listdir(OUT) if f.endswith(('.glb','.blend','.png'))}
open(OUT+'/evidence.json','w').write(json.dumps(stats,indent=2))
print(json.dumps(stats,indent=2))
