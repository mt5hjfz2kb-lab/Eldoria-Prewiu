"""Phase 2: deterministic modular meshes. Blender 4.0+; ALL buildings are kit instances in Unity.
Only one existing 256x256 approved Hero Atlas; no new art textures. Local Z is vertical
inside Blender and becomes Unity Y. Every exported object pivot is (0,0,0) at its base.
"""
import bpy, bmesh, math, random, json, csv, hashlib, faulthandler
faulthandler.enable()
print('KIT_GENERATOR_ENTRY', flush=True)
from pathlib import Path
from mathutils import Vector
K = Path(__file__).resolve().parents[1]
PHASE1 = K.parent / "Phase1"
MODELS = K / "Models"; MODELS.mkdir(parents=True, exist_ok=True)
SOURCE = K / "Source~"; SOURCE.mkdir(parents=True, exist_ok=True)
ATLAS_PATH = PHASE1 / "Textures" / "Hero_Atlas_256.png"
assert ATLAS_PATH.exists(), "Approved 256x256 atlas absent"
atlas_bytes = ATLAS_PATH.read_bytes()
atlas_sha = hashlib.sha256(atlas_bytes).hexdigest()
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
atlas = bpy.data.images.load(str(ATLAS_PATH), check_existing=True)
assert tuple(atlas.size[:]) == (256,256), f"Expected 256x256, got {atlas.size[:]}"
bpy.context.scene.unit_settings.system="METRIC"
bpy.context.scene.unit_settings.scale_length=1
random.seed(2161126)
materials = []
for name, power in (("Kit_Surface",0),("Kit_WarmLight",1.55),("Kit_Corruption",2.15)):
    mat=bpy.data.materials.new(name)
    mat.use_nodes=True
    nodes=mat.node_tree.nodes; nodes.clear(); links=mat.node_tree.links
    output=nodes.new("ShaderNodeOutputMaterial")
    bsdf=nodes.new("ShaderNodeBsdfPrincipled")
    bsdf.inputs["Roughness"].default_value=.86
    tex=nodes.new("ShaderNodeTexImage");tex.image=atlas;tex.interpolation="Linear"
    color=nodes.new("ShaderNodeVertexColor");color.layer_name="Color"
    mult=nodes.new("ShaderNodeMixRGB");mult.blend_type="MULTIPLY"
    mult.inputs[0].default_value=1
    links.new(tex.outputs["Color"],mult.inputs[1])
    links.new(color.outputs["Color"],mult.inputs[2])
    links.new(mult.outputs[0],bsdf.inputs["Base Color"])
    if power:
        links.new(mult.outputs[0],bsdf.inputs["Emission Color"])
        bsdf.inputs["Emission Strength"].default_value=power
    links.new(bsdf.outputs["BSDF"],output.inputs["Surface"])
    materials.append(mat)

class Piece:
    def __init__(self, name, family, grid=(1,1)):
        self.name=name;self.family=family;self.grid=grid
        self.vertices=[];self.faces=[];self.colors=[];self.swatches=[];self.slots=[]
        self.rng=random.Random(2161126 + sum((i+11)*ord(c) for i,c in enumerate(name)))
    def face(self, points, k=0, slot=0, shade=1.0, outward=None):
        p=[Vector(q) for q in points]
        if len(p)<3:return
        if outward is not None and (p[1]-p[0]).cross(p[2]-p[0]).dot(Vector(outward))<0:p.reverse()
        start=len(self.vertices)
        self.vertices.extend(tuple(q) for q in p)
        self.faces.append(tuple(range(start,start+len(p))))
        self.swatches.append(int(k));self.slots.append(int(slot))
        self.colors.append(max(.56,min(1.18,shade*self.rng.uniform(.94,1.06))))
    def box(self, center, size, k=0, slot=0, bevel=.055, rot=0.0, shade=1.0):
        """Actual 26-face clipped/biselled asymmetric stone/timber cuboid, not a sharp cube."""
        c=Vector(center); dims=[max(.025,float(x)) for x in size]
        h=[x*.5 for x in dims];b=max(.007,min(bevel,min(h)*.48))
        angle=float(rot); co=math.cos(angle);si=math.sin(angle)
        def trans(p):
            return (c.x+co*p[0]-si*p[1],c.y+si*p[0]+co*p[1],c.z+p[2])
        def put(points):
            points=[Vector(p) for p in points]
            center_local=sum(points,Vector())/len(points)
            self.face([trans(p) for p in points],k,slot,shade,
                      (co*center_local.x-si*center_local.y,
                       si*center_local.x+co*center_local.y, center_local.z))
        for a in range(3):
            other=[z for z in range(3) if z!=a]
            for sign in [-1,1]:
                ps=[]
                for u,v in [(-1,-1),(1,-1),(1,1),(-1,1)]:
                    t=[0.,0.,0.];t[a]=sign*h[a]
                    t[other[0]]=u*(h[other[0]]-b)
                    t[other[1]]=v*(h[other[1]]-b);ps.append(t)
                put(ps)
        for a,baxis in ((0,1),(0,2),(1,2)):
            other=3-a-baxis
            for sa in [-1,1]:
                for sb in [-1,1]:
                    pts=[]
                    for sc in [-1,1]:
                        v=[0.,0.,0.];v[a]=sa*h[a];v[baxis]=sb*(h[baxis]-b);v[other]=sc*(h[other]-b);pts.append(v)
                        v2=[0.,0.,0.];v2[a]=sa*(h[a]-b);v2[baxis]=sb*h[baxis];v2[other]=sc*(h[other]-b);pts.append(v2)
                    put((pts[0],pts[1],pts[3],pts[2]))
        for sx in [-1,1]:
            for sy in [-1,1]:
                for sz in [-1,1]:
                    pts=[(sx*h[0],sy*(h[1]-b),sz*(h[2]-b)),
                         (sx*(h[0]-b),sy*h[1],sz*(h[2]-b)),
                         (sx*(h[0]-b),sy*(h[1]-b),sz*h[2])]
                    put(pts)
    def rod(self,a,b,thick,k=4,slot=0,shade=1):
        start=Vector(a);end=Vector(b);direction=end-start
        if direction.length<.015:return
        rotation=direction.to_track_quat('Z','Y')
        # Build an oriented square beam, center at mid-point, with bevel edges.
        before=len(self.vertices)
        self.box((0,0,0),(thick,thick,direction.length),k,slot,max(.012,thick*.14),shade=shade)
        middle=(start+end)*.5
        for i in range(before,len(self.vertices)):
            self.vertices[i]=tuple(rotation @ Vector(self.vertices[i])+middle)
    def slate(self, quad, index=3,thick=.10,shade=1):
        # Slightly nonparallel heavy overlapping slate with actual thickness.
        zdrop=Vector((0,0,-thick))
        top=[Vector(x) for x in quad]
        down=[x+zdrop for x in top]
        centroid=sum(top+down,Vector())/8
        for poly in [top,down[::-1],*[ [top[j],top[(j+1)%4],down[(j+1)%4],down[j]] for j in range(4) ]]:
            normal=sum([p for p in poly],Vector())/len(poly)-centroid
            self.face(poly,index,0,shade,normal)
    def mesh(self):
        # Bring the lowest edge, including slate thickness and slanted rafters,
        # to Z=0: exact reusable bottom pivot, no module ever sinks below the grid.
        minz=min(v[2] for v in self.vertices)
        if abs(minz)>.001:
            self.vertices=[(x,y,z-minz) for (x,y,z) in self.vertices]
        me=bpy.data.meshes.new(self.name)
        me.from_pydata(self.vertices,[],self.faces)
        me.update()
        obj=bpy.data.objects.new(self.name,me)
        bpy.context.collection.objects.link(obj)
        for m in materials:me.materials.append(m)
        uv=me.uv_layers.new(name="UVMap")
        col=me.vertex_colors.new(name="Color")
        for n,p in enumerate(me.polygons):
            p.material_index=self.slots[n]
            k=self.swatches[n]
            shade=self.colors[n]
            for j,li in enumerate(p.loop_indices):
                u=(j%4 in (1,2));v=(j%4 in (2,3))
                uv.data[li].uv=((k%4+(7+(45 if u else 0))/64)/4,
                                (k//4+(7+(45 if v else 0))/64)/4)
                # Mix custom stone tonality into vertex colors, never a separate texture.
                col.data[li].color=(shade,shade,shade,1)
        assert all(v.co.z>=-.015 for v in me.vertices), "Below-base geometry: "+self.name
        return obj

entries=[]
def save(piece):
    assert piece.faces, piece.name
    print('KIT_GENERATING',piece.name,'raw_faces',len(piece.faces),flush=True)
    obj=piece.mesh()
    bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
    path=MODELS/(piece.name+".fbx")
    bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,
        object_types={'MESH'},axis_forward='-Z',axis_up='Y',
        apply_unit_scale=True,global_scale=1.0,mesh_smooth_type='FACE',
        use_mesh_modifiers=True,add_leaf_bones=False,bake_anim=False,
        path_mode='AUTO')
    tri=sum(len(f)-2 for f in piece.faces)
    xs=[v[0] for v in piece.vertices];ys=[v[1] for v in piece.vertices];zs=[v[2] for v in piece.vertices]
    record={"id":piece.name,"family":piece.family,"fbx":"Models/"+piece.name+".fbx",
      "triangles":tri,"faces":len(piece.faces),"grid_m":4,
      "footprint_cells":list(piece.grid),
      "local_min_m":[round(min(xs),3),round(min(ys),3),round(min(zs),3)],
      "local_max_m":[round(max(xs),3),round(max(ys),3),round(max(zs),3)],
      "pivot_m":[0,0,0],"materials":["Kit_Surface","Kit_WarmLight","Kit_Corruption"],
      "uv_layer":"UVMap","vertex_color_layer":"Color"}
    entries.append(record)
    # Keep the .blend useful to artists, but FBX pivot stays at zero.
    idx=len(entries)-1;obj.location=(idx%8*6.8,idx//8*7.1,0)
    return record

def construct(name,family,func,grid=(1,1)):
    p=Piece(name,family,grid)
    func(p)
    save(p)

def masonry(p,damaged=False,corner=False,door=False,window=False):
    w=4.0;h=3.7
    # Mortar is recessed; individual irregular stone blocks form staggered courses.
    if door or window:
        p.box((-1.52,0,1.8),(.95,.40,3.6),2,bevel=.09)
        p.box((1.52,0,1.8),(.95,.40,3.6),0,bevel=.08)
        if window:
            p.box((0,0,.67),(2.18,.38,1.25),1,bevel=.08)
            p.box((0,0,3.13),(2.26,.43,1.13),0,bevel=.08)
            p.box((0,-.255,1.64),(1.60,.075,1.24),12,slot=1,bevel=.02)
            p.box((0,-.32,1.65),(.10,.12,1.48),4,bevel=.017)
            p.box((0,-.33,1.61),(1.78,.11,.10),4,bevel=.014)
            for xx in (-.97,.97):p.rod((xx,-.24,1.0),(xx,-.24,2.4),.15,4)
        else:
            p.box((0,0,3.15),(2.2,.44,1.1),0,bevel=.08)
            for x in [-.85,-.55,-.27,0,.27,.55,.85]:
                p.box((x,-.27,1.04),(.25,.10,2.07),4,bevel=.018,shade=.96)
            for z in (.44,1.53):
                p.box((0,-.345,z),(1.77,.09,.095),10,bevel=.012)
            p.box((0,-.58,.09),(2.25,1.08,.18),1,bevel=.07)
        p.box((0,0,.12),(4,.52,.22),2,bevel=.055)
    else:
        p.box((0,.07,1.84),(3.91,.24,3.65),2,bevel=.12)
        for row in range(7):
            z=.20+row*.48
            off=.29 if row%2 else .03
            x=-1.95+off
            while x<1.88:
                width=min(p.rng.uniform(.55,1.06),1.98-x)
                if damaged and row>=4 and p.rng.random()<.32:
                    x+=width;continue
                shade=.92 if (row+int(x*10))%3 else 1.03
                p.box((x+width*.5,-.19,z+.24),(max(.16,width-.025),.18,.45),
                      p.rng.choice((0,0,0,1,2)),bevel=.04,shade=shade)
                x+=width
        for z in (.34,3.36):
            p.rod((-1.94,-.27,z),(1.94,-.27,z),.14,4,shade=.90)
    for x in [-1.83,1.83]:
        p.rod((x,-.35,0),(x+.03,-.34,3.67),.15,4)
    if corner:
        p.box((-1.83,1.78,1.84),(.41,3.72,3.56),0,bevel=.09)
        p.rod((-1.88,3.6,.15),(-1.88,3.6,3.7),.18,4)
    if damaged:
        # Broken plaster edges, a braced collapse and earth-contact rubble.
        for x in (-1.35,-.62,.70,1.6):
            z=p.rng.uniform(.19,.34)
            p.box((x,-.53,z),(.55,.60,.22),p.rng.choice((0,1,2)),bevel=.065,rot=p.rng.uniform(-.25,.25))
        p.rod((-1.88,-.43,.37),(-.61,-.55,2.63),.22,4)
        p.rod((1.80,-.40,.55),(.90,-.45,3.02),.17,4)
        for i in range(3):
            p.box((p.rng.uniform(-1.6,1.6),-.42,p.rng.uniform(2.1,3.55)),
                  (.37,.15,.21),6,bevel=.04,shade=.77)

for kind in ("Straight","Corner","Door","Window"):
    for status in ("Intact","Damaged"):
        construct("Wall_"+kind+"_"+status,"masonry",lambda p,k=kind,s=status:
          masonry(p,damaged=s=="Damaged",corner=k=="Corner",door=k=="Door",window=k=="Window"))

def roof_gable(p,w=8.40,d=8.48,height=2.63):
    for side in (-1,1):
        for r in range(8):
            u0=r/8;u1=min(1.04,(r+1)/8+.022)
            for i in range(10):
                v0=i/10;v1=min(1,(i+1)/10+.015)
                def point(u,v):
                    return (side*w*.5*u+.06*math.sin(v*8),
                            -d*.5+d*v,
                            height*max(0,1-u)**1.40 + .06*math.sin(v*6)*u + .02)
                p.slate((point(u0,v0),point(u1,v0),point(u1,v1),point(u0,v1)),
                         3,thick=.12,shade=p.rng.choice((.89,.94,.99,1.05)))
        p.rod((side*w*.50,-d*.51,.04),(side*w*.50,d*.51,.04),.25,4)
        for i in range(5):
            yy=-d*.47+i*d/4
            p.rod((0,yy,height-.08),(side*w*.5,yy,.04),.14,4)
    for yy in (-d*.52,d*.52):
        p.rod((0,yy,height),(0,yy,.08),.19,4)
        p.rod((-w*.48,yy,.10),(0,yy,height),.20,4)
        p.rod((w*.48,yy,.10),(0,yy,height),.20,4)
    p.rod((0,-d*.53,height),(0,d*.53,height),.28,4)

def roof_tower(p):
    w=4.50;h=2.25
    for side in range(4):
        a=side*math.pi/2
        for row in range(8):
            t0=row/8;t1=(row+1)/8
            for j in range(6):
                v0=j/6;v1=(j+1)/6
                def point(t,v):
                    # Four swept radial pitches, slightly curved rise.
                    x=-w*.5+(w*v)
                    y=w*.5*(1-t)
                    return (x*math.cos(a)-y*math.sin(a),
                            x*math.sin(a)+y*math.cos(a),
                            h*t**.79 + .02*math.sin(j*3.1))
                p.slate((point(t0,v0),point(t0,v1),point(t1,v1),point(t1,v0)),
                         3,thick=.085,shade=p.rng.choice((.88,.98,1.04)))
    p.box((0,0,h+.10),(.35,.35,.45),11,bevel=.07)
    p.rod((0,0,h+.38),(0,0,h+1.05),.16,10)

def roof_shed(p):
    w=4.6;d=5.10
    for i in range(10):
        x0=-w*.5+i*w/10;x1=x0+w/10+.018
        for j in range(6):
            y0=-d*.5+j*d/6;y1=y0+d/6+.022
            def point(x,y):
                return (x,y,.14+1.56*((y+d*.5)/d)**.92+.05*math.sin(x*1.8))
            p.slate([point(x0,y0),point(x1,y0),point(x1,y1),point(x0,y1)],3,.10,shade=p.rng.choice((.90,.96,1.04)))
    for x in (-w*.48,w*.48):
        p.rod((x,-d*.52,.12),(x,d*.51,1.74),.18,4)
    for y in (-d*.48,d*.48):
        p.rod((-w*.52,y,.12+1.56*(y+d*.5)/d),(w*.52,y,.12+1.56*(y+d*.5)/d),.18,4)

construct("Roof_Gable","roofs",roof_gable,(2,2))
construct("Roof_Tower","roofs",roof_tower,(1,1))
construct("Roof_Shed","roofs",roof_shed,(1,1))

def tower(p,ruin=False):
    width=3.92
    for row in range(12):
        z=row*.45
        for side in range(4):
            for col in range(5):
                if ruin and row>6 and (col+side*2+row*3)%5<2:continue
                axis=side*math.pi/2
                x=-1.80+col*.90
                y=-1.88
                X=x*math.cos(axis)-y*math.sin(axis);Y=x*math.sin(axis)+y*math.cos(axis)
                p.box((X,Y,z+.245),(.87,.29,.44),p.rng.choice((0,0,1,2)),bevel=.045,rot=axis,shade=p.rng.uniform(.86,1.06))
    if ruin:
        for i in range(14):
            a=i*2.399;rad=p.rng.uniform(.45,2.35)
            p.box((rad*math.cos(a),rad*math.sin(a),.12),(.42,.52,.25),p.rng.choice((0,1,2)),bevel=.065,rot=a)
        for i in range(4):
            a=i*math.pi/2
            p.rod((1.65*math.cos(a),1.65*math.sin(a),4.2),
                  (.8*math.cos(a)+p.rng.uniform(-.8,.8),.8*math.sin(a),5.9),.18,4)
        return
    for col in (-1,1):
        for row in (-1,1):
            x=col*1.45;y=row*1.50
            p.rod((x,y,5.45),(x*.95,y*.95,7.58),.25,4)
    for side in (-1,1):
        for y in (-1.58,1.58):
            p.rod((side*1.55,y,7.5),(-side*1.55,y,7.5),.20,4)
    p.rod((-1.1,-.6,6.7),(1.1,-.6,6.7),.23,4)
    # Suspended bronze bell, convex lathe silhouette, visible bronze rim and dark mouth.
    for row in range(5):
        z=5.95+row*.17;rad=[.62,.53,.44,.36,.25][row]
        n=10
        for j in range(n):
            a=j*math.tau/n;bb=(j+1)*math.tau/n
            r2=[.62,.53,.44,.36,.25][min(4,row+1)]
            p.face([(rad*math.cos(a),-.60+rad*math.sin(a),z),
                    (rad*math.cos(bb),-.60+rad*math.sin(bb),z),
                    (r2*math.cos(bb),-.60+r2*math.sin(bb),z+.17),
                    (r2*math.cos(a),-.60+r2*math.sin(a),z+.17)],11)
    p.rod((0,-.60,6.25),(0,-.60,5.72),.13,10)
construct("Tower_Bell","towers",lambda p:tower(p,False),(1,1))
construct("Tower_Broken","towers",lambda p:tower(p,True),(1,1))

def chimney(p):
    for i in range(10):
        zz=i*.42
        for x in (-.48,.48):
            for y in (-.48,.48):
                p.box((x,y,zz+.20),(.90,.90,.39),p.rng.choice((0,1,2)),bevel=.07,shade=p.rng.uniform(.86,1.01))
    p.box((0,0,4.20),(2.14,2.14,.37),1,bevel=.12)
    p.box((0,0,4.44),(1.15,1.15,.06),10,bevel=.015)
def balcony(p):
    for j in range(5):
        x=-1.85+j*.90
        p.box((x,0,.25),(.85,2.17,.20),4,bevel=.055)
    for x in (-1.83,0,1.83):
        p.rod((x,-.88,.3),(x,-.88,1.45),.17,4)
    p.rod((-1.95,-.88,1.45),(1.95,-.88,1.45),.17,4)
    for x in (-1.65,1.65):
        p.rod((x,.8,.14),(x,-.9,1.1),.18,4)
def shield(p):
    p.box((0,.05,.6),(.90,.16,1.2),4,bevel=.12)
    for i in range(16):
        a=i*math.tau/16;b=(i+1)*math.tau/16
        p.face([(0,-.11,1.65),(.64*math.cos(a),-.13,.99+.64*math.sin(a)),
                (.64*math.cos(b),-.13,.99+.64*math.sin(b))],11,shade=.95)
    p.rod((-.27,-.24,.55),(.27,-.24,1.35),.085,10)
    p.rod((.27,-.24,.55),(-.27,-.24,1.35),.085,10)
def beam_piece(p):
    p.rod((0,0,0),(.15,.08,4.20),.30,4)
    p.box((0,0,.13),(.48,.48,.27),5,bevel=.06)
    p.box((.15,.08,4.16),(.48,.48,.27),5,bevel=.06)
for name,func in (("Chimney",chimney),("Balcony",balcony),("Shield",shield),("Beam",beam_piece)):
    construct(name,"architectural_props",func)

def cone(p,center,radius,zlow,zhigh,k=6,sl=0,segments=9):
    cx,cy=center
    for i in range(segments):
        a=i*math.tau/segments;b=(i+1)*math.tau/segments
        shape=.89+.14*math.sin(i*2.37+radius)
        p.face([(cx+radius*shape*math.cos(a),cy+radius*shape*math.sin(a),zlow),
                (cx+radius*shape*math.cos(b),cy+radius*shape*math.sin(b),zlow),
                (cx+.06*math.sin(zlow),cy+.04*math.cos(zhigh),zhigh)],
               k if i%3 else 7,slot=sl,shade=.82+.05*(i%5))

def pine(p,height):
    p.rod((0,0,0),(.035,0,height*.95),height*.105,4)
    base=height*.26
    for j in range(5):
        low=base+j*height*.115; high=low+height*.38
        cone(p,(.01*math.sin(j),0),height*(.25-j*.034),low,min(height*.99,high))
    for i in range(8):
        a=i*math.tau/8
        z=base+(i%3)*height*.10
        p.rod((0,0,z),(height*.20*math.cos(a),height*.20*math.sin(a),z-height*.025),height*.018,4)
for suffix,h in (("Small",3.8),("Medium",6.2),("Tall",9.45)):
    construct("Pine_"+suffix,"vegetation",lambda p,hh=h:pine(p,hh))

def fallen(p):
    p.rod((-2.1,-.18,.32),(2.15,.35,.51),.62,4)
    for j in range(6):
        x=-1.78+j*.69
        p.rod((x,.13,.48),(x+.45,p.rng.choice((-.9,.82)),p.rng.uniform(.12,.8)),.12,4)
        p.box((x,-.08,.12),(.38,.52,.16),7,bevel=.06)
def shrubs(p):
    for i in range(8):
        a=i*2.399;rr=.24+(i%4)*.19
        x=rr*math.cos(a);y=rr*math.sin(a)
        for j in range(5):
            ang=j*math.tau/5
            p.face([(x+.21*math.cos(ang),y+.21*math.sin(ang),.15),
                    (x+.22*math.cos(ang+.95),y+.22*math.sin(ang+.95),.18),
                    (x,y,.42+.21*(i%3))],6 if i%3 else 7)
def tall_grass(p):
    for i in range(16):
        a=i*2.399;rr=.1+.8*(i%5)/5
        x=rr*math.cos(a);y=rr*math.sin(a);h=p.rng.uniform(.6,1.6)
        w=p.rng.uniform(.07,.14)
        for side in (0,math.pi/2):
            d=(math.cos(a+side),math.sin(a+side))
            p.face([(x-w*d[1],y+w*d[0],0),
                    (x+w*d[1],y-w*d[0],0),
                    (x+.14*math.cos(a),y+.14*math.sin(a),h)],
                   7 if i%4==0 else 6,shade=.78+.17*(i%3)/3)
for n,f in (("Fallen_Log",fallen),("Shrubs",shrubs),("Tall_Grass",tall_grass)):
    construct(n,"vegetation",f)

def irregular_rock(p,size):
    sx,sy,sz=size
    points=[]
    for i in range(9):
        a=i*math.tau/9
        rr=p.rng.uniform(.79,1.09)
        points.append((sx*rr*math.cos(a),sy*rr*math.sin(a),.04))
    top=[]
    for i in range(9):
        a=i*math.tau/9;rr=p.rng.uniform(.54,.78)
        top.append((sx*rr*math.cos(a)+.07,sy*rr*math.sin(a),sz*p.rng.uniform(.72,1.05)))
    crown=(.03,-.05,sz*1.07)
    for i in range(9):
        j=(i+1)%9
        p.face((points[i],points[j],top[j],top[i]),0 if i%3 else 2)
        p.face((top[i],top[j],crown),1 if i%3 else 0)
def path_slab(p):
    for off in ((0,0),(.43,.29)):
        x,y=off
        p.box((x,y,.115),(1.30,.94,.22),1 if x else 0,bevel=.07,rot=.12 if x else -.07)
for n,size in (("Rock_Small",(.37,.32,.27)),("Rock_Medium",(.82,.70,.59)),("Rock_Large",(1.72,1.22,1.18))):
    construct(n,"rocks",lambda p,s=size:irregular_rock(p,s))
construct("Path_Slab","paths",path_slab)

def corruption(p,height):
    p.box((0,0,.12),(max(.45,height*.55),max(.48,height*.47),.24),14,bevel=.08,shade=.80)
    crystals=[(-.32,-.08,.9,.38),(0,.16,1,.50),(.24,-.13,.62,.27),(-.10,.35,.50,.27)]
    for dx,dy,hscale,rscale in crystals:
        h=height*hscale
        rad=max(.10,rscale*height*.28)
        twist=p.rng.uniform(-.28,.28)
        for i in range(6):
            a=i*math.tau/6;bb=(i+1)*math.tau/6
            low=(dx+rad*math.cos(a),dy+rad*math.sin(a),.14)
            lowb=(dx+rad*math.cos(bb),dy+rad*math.sin(bb),.14)
            mid=(dx+rad*.80*math.cos(a+twist),dy+rad*.80*math.sin(a+twist),h*.42)
            midb=(dx+rad*.80*math.cos(bb+twist),dy+rad*.80*math.sin(bb+twist),h*.42)
            tip=(dx+twist*h*.18,dy+twist*h*.12,h)
            p.face((low,lowb,midb,mid),13,slot=2,shade=p.rng.uniform(.95,1.13))
            p.face((mid,midb,tip),13 if i%2 else 14,slot=2,shade=p.rng.uniform(.94,1.08))
def roots(p):
    # Tapered roots and purple inlaid seams directly follow a wall/terrain diagonal.
    paths=[
      [(0,0,0),(.20,0,.45),(.48,0,1.18),(.16,0,2.21),(.70,0,3.35)],
      [(0,0,.18),(-.28,0,.82),(-.81,0,1.45),(-.74,0,2.12)],
      [(.43,0,1.30),(1.12,0,1.62),(1.40,0,2.44)]
    ]
    for index,points in enumerate(paths):
        for a,b in zip(points,points[1:]):
            p.rod(a,b,.24-index*.04,14,shade=.70)
            va=Vector(a);vb=Vector(b)
            va.y-=.138;vb.y-=.138
            p.rod(va,vb,.065,13,slot=2,shade=1.06)
    for i in range(4):
        p.box((.18*i-.27,-.12,.06),(.30,.28,.11),14,bevel=.025)
for n,h in (("Crystal_Small",1.0),("Crystal_Medium",2.15),("Crystal_Large",4.35)):
    construct(n,"corruption",lambda p,hh=h:corruption(p,hh))
construct("Corrupt_Roots","corruption",roots)

assert len(entries)==31, f"Expected 31 reusable kit pieces, got {len(entries)}"
assert len(set(x["id"] for x in entries))==31
assert all(e["local_min_m"][2]>=-.015 for e in entries)
manifest={"version":2,"blender":bpy.app.version_string,"source":"Blender native scripted deterministic Kit generator",
 "owner_approval":"Phase1 approved, Phase2 pending","atlas":"Assets/Art_Reboot/Phase1/Textures/Hero_Atlas_256.png",
 "atlas_size":[256,256],"atlas_sha256":atlas_sha,"grid_m":4,
 "materials":[x.name for x in materials],"material_count":len(materials),
 "pieces":entries,"total_triangles":sum(x["triangles"] for x in entries)}
(SOURCE/"kit_manifest.json").write_text(json.dumps(manifest,indent=2,ensure_ascii=False)+"\n")
with (SOURCE/"kit_polygons.csv").open("w",newline="",encoding="utf-8") as fp:
    w=csv.writer(fp);w.writerow(("piece","family","triangles","grid_cells_x","grid_cells_z"))
    for e in entries:w.writerow((e["id"],e["family"],e["triangles"],*e["footprint_cells"]))
# Inspector scene in the BLEND itself. PNG output is only accepted from Unity.
bpy.ops.object.select_all(action="SELECT")
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/"modular_kit.blend"))
print("BLENDER_KIT_PASS",len(entries),"pieces",manifest["total_triangles"],"triangles","atlas",atlas_sha)
