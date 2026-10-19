"""Real Blender source for ONE coherent medieval rebuilding street-block.
Existing CC0 stone and historical sawmill oak are reused; original GLBs untouched.
This is an art candidate only: Unity player-camera A/B will decide acceptance.
"""
import bpy, json, math, os, hashlib, random
from pathlib import Path
from mathutils import Vector
root=Path.cwd()
ev=root/"docs/evidence/valoria-reference-clean"
src=root/"Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources"
texroot=src/"MaterialInputs/sandstone_blocks_05"
inputs=[texroot/"sandstone_blocks_05_diff_1k.png",texroot/"sandstone_blocks_05_nor_gl_1k.png",
        texroot/"license-manifest.json",src/"SawmillRefinedOak.png"]
for file in inputs:
    assert file.is_file(), "Retained art missing: "+str(file)
assert "CC0" in inputs[2].read_text().upper()
sha=lambda file: hashlib.sha256(file.read_bytes()).hexdigest()
source_hashes={str(f.relative_to(root)):sha(f) for f in inputs}
bpy.ops.wm.read_factory_settings(use_empty=True)
def mat(name,rgb,texture=None,normal=None):
    m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=(*rgb,1)
    p=m.node_tree.nodes.get("Principled BSDF");p.inputs["Base Color"].default_value=(*rgb,1)
    p.inputs["Roughness"].default_value=.87
    if texture:
        t=m.node_tree.nodes.new("ShaderNodeTexImage")
        t.image=bpy.data.images.load(str(texture),check_existing=True)
        m.node_tree.links.new(t.outputs["Color"],p.inputs["Base Color"])
    if normal:
        t=m.node_tree.nodes.new("ShaderNodeTexImage");t.image=bpy.data.images.load(str(normal),check_existing=True)
        t.image.colorspace_settings.name="Non-Color"
        n=m.node_tree.nodes.new("ShaderNodeNormalMap");n.inputs["Strength"].default_value=.45
        m.node_tree.links.new(t.outputs["Color"],n.inputs["Color"])
        m.node_tree.links.new(n.outputs["Normal"],p.inputs["Normal"])
    return m
materials=[
 mat("CC0 Valoria limestone",(.55,.52,.45),inputs[0],inputs[1]),
 mat("Retained Sawmill oak",(.28,.18,.12),inputs[3]),
 mat("Retained charcoal slate",(.155,.16,.17)),
 mat("Worn limestone chipped trim",(.43,.40,.34)),
 mat("Oxidised connection iron",(.10,.105,.11))
]
class Mesh:
    def __init__(self):self.verts=[];self.faces=[];self.slots=[];self.uv=[]
    def face(self,verts,slot):
        a=len(self.verts);self.verts.extend(verts)
        self.faces.append(tuple(range(a,a+len(verts))));self.slots.append(slot)
        p=[Vector(v) for v in verts]
        tangent=(p[1]-p[0]).normalized()
        normal=(p[1]-p[0]).cross(p[2]-p[0]).normalized()
        across=normal.cross(tangent).normalized()
        self.uv.append([((v-p[0]).dot(tangent)*.33,(v-p[0]).dot(across)*.33) for v in p])
    def box(self,x0,x1,y0,y1,z0,z1,slot):
        p=[(x0,y0,z0),(x1,y0,z0),(x1,y1,z0),(x0,y1,z0),
           (x0,y0,z1),(x1,y0,z1),(x1,y1,z1),(x0,y1,z1)]
        for order in [(3,2,1,0),(4,5,6,7),(0,1,5,4),(3,7,6,2),(0,4,7,3),(1,2,6,5)]:
            self.face([p[i] for i in order],slot)
    def beam(self,p0,p1,width,slot):
        a=Vector(p0);b=Vector(p1);axis=(b-a).normalized()
        up=Vector((0,0,1))
        if abs(axis.dot(up))>.91:up=Vector((0,1,0))
        tangent=axis.cross(up).normalized();bitan=axis.cross(tangent).normalized()
        rings=[]
        for p in [a,b]:
            rings.append([tuple(p+width*(math.cos(j*math.tau/8)*tangent+
                                  math.sin(j*math.tau/8)*bitan)) for j in range(8)])
        self.face(list(reversed(rings[0])),slot);self.face(rings[1],slot)
        for j in range(8):
            k=(j+1)%8
            self.face([rings[0][j],rings[0][k],rings[1][k],rings[1][j]],slot)
    def ribbon(self,points,z0,z1,width,slot):
        halves=[]
        for j,p in enumerate(points):
            before=Vector(points[max(0,j-1)])
            after=Vector(points[min(len(points)-1,j+1)])
            d=(after-before).normalized();perp=Vector((-d.y,d.x))
            q=Vector(p);halves.append((q+perp*width*.5,q-perp*width*.5))
        for (a,b),(c,d) in zip(halves[:-1],halves[1:]):
            def P(v,z):return(v.x,v.y,z)
            self.face([P(a,z0),P(c,z0),P(c,z1),P(a,z1)],slot)
            self.face([P(b,z1),P(d,z1),P(d,z0),P(b,z0)],slot)
            self.face([P(a,z1),P(c,z1),P(d,z1),P(b,z1)],slot)
        for a,b in [halves[0],halves[-1]]:
            self.face([(a.x,a.y,z0),(b.x,b.y,z0),(b.x,b.y,z1),(a.x,a.y,z1)],slot)
    def gothic(self,x):
        for y0,y1 in [(-3.25,-2.65),(2.65,3.25)]:
            self.box(x-.47,x+.47,y0,y1,-.40,3.06,0)
        for sign in [-1,1]:
            for j in range(12):
                def point(k,outer):
                    t=k/12*math.pi/2
                    y=sign*(2.98*math.cos(t))
                    z=3.02+2.13*math.sin(t)+outer*.51
                    return(y,z)
                y0,z0=point(j,False);y1,z1=point(j+1,False)
                Y0,Z0=point(j,True);Y1,Z1=point(j+1,True)
                self.face([(x-.5,y0,z0),(x+.5,y0,z0),(x+.5,y1,z1),(x-.5,y1,z1)],0)
                self.face([(x-.5,Y0,Z0),(x-.5,Y1,Z1),(x+.5,Y1,Z1),(x+.5,Y0,Z0)],0)
                self.face([(x+.5,y0,z0),(x+.5,Y0,Z0),(x+.5,Y1,Z1),(x+.5,y1,z1)],0)
        self.beam((x,-3.35,4.04),(x,3.35,4.04),.11,1)
    def output(self):
        mesh=bpy.data.meshes.new("Precinct continuous authored source")
        mesh.from_pydata(self.verts,[],self.faces);mesh.update()
        obj=bpy.data.objects.new("Eldoria / single connected reconstructed street block v16",mesh)
        bpy.context.collection.objects.link(obj)
        for material in materials:mesh.materials.append(material)
        uv=mesh.uv_layers.new(name="Stone timber UV")
        for face,index,coords in zip(mesh.polygons,self.slots,self.uv):
            face.material_index=index
            for j,loop in enumerate(face.loop_indices):uv.data[loop].uv=coords[j]
        return obj
g=Mesh()
# Continuous battered retaining walls and a single inhabited frontage: two
# sides of one lane, not isolated houses or identical prefab clusters.
g.ribbon([(-11.6,-3.3),(-8,-3.65),(-4,-3.25),(0,-3.15),
          (4,-3.45),(8,-3.65),(11.6,-3.25)],-.44,.52,.94,0)
g.ribbon([(-11.6,3.3),(-8.5,3.70),(-5,3.5),(-1,3.05),
          (4.0,3.15),(8,3.58),(11.6,3.45)],-.44,.52,.89,0)
for xa,xb,h in [(-10.8,-7.2,3.4),(-7.05,-4.75,2.8),(-2.3,.85,3.1),(3.7,5.55,2.3)]:
    g.box(xa,xb,3.20,3.95,-.40,h,0)
    g.box(xa-.2,xb+.15,3.03,4.13,h-.08,h+.20,3)
for x,y,z in [(-10.2,3.5,3.20),(-4.85,3.48,2.8),(5.2,3.5,2.65),(9.0,-3.25,2.60)]:
    g.box(x-.47,x+.47,y-.63,y+.63,-.43,z,0)
    g.box(x-.63,x+.63,y-.83,y+.83,z-.12,z+.22,3)
g.gothic(1.60)
# The same linked workfront carries staggered scaffolding, arch shoring,
# two incomplete roofing spans and structural braces visibly rooted in stone.
for j,(x,y) in enumerate([(-9,-3.3),(-4,-3.25),(2,-3.43),(7,-3.63),(10,-3.3)]):
    height=[4.24,3.68,4.36,3.74,3.35][j]
    for shift in [0,1.07]:
        g.beam((x,y+shift,-.43),(x,y+shift,height),.14,1)
        g.beam((x-.19,y+shift,-.36),(x+1.12,y+shift,height*.77),.09,1)
    g.beam((x,y-.12,height),(x,y+1.35,height),.15,1)
for xa,xb,z in [(-9,-4,4.05),(-4,2,3.4),(2,7,4.0),(7,10,3.05)]:
    for y in [-3.42,-2.30]:g.beam((xa,y,z),(xb,y,z),.085,1)
for xa,xb,z in [(-9,-4.05,4.11),(2.1,6.85,4.14)]:
    g.face([(xa,-3.86,z),(xb,-3.86,z-.06),(xb,-1.34,z-1.02),(xa,-1.34,z-.96)],2)
    for x in [xa,xb]:g.beam((x,-3.86,z),(x,-1.34,z-.96),.08,1)
rng=random.Random(16019)
for j in range(6):
    x=-10.4+j*4.05;side=1 if j%2 else -1
    y=3.42 if side>0 else -3.47
    g.beam((x,y+side*.58,-.35),(x+1.17,y,2.0+rng.random()*.68),.11,1)
for x in [-9.2,-4.3,1.6,7.1]:
    g.beam((x,-3.1,2.82),(x+.1,-3.1,3.04),.06,4)
obj=g.output()
triangles=sum(len(poly.vertices)-2 for poly in obj.data.polygons)
assert 500<triangles<55000
bpy.ops.object.select_all(action="DESELECT")
obj.select_set(True);bpy.context.view_layer.objects.active=obj
path=src/"ConnectedPrecinctV16.glb"
bpy.ops.export_scene.gltf(filepath=str(path),export_format="GLB",use_selection=True,
                          export_apply=True,export_yup=True,export_texcoords=True,export_normals=True,
                          export_materials="EXPORT")
bpy.context.preferences.filepaths.save_version=0
blend=ev/"ConnectedPrecinctV16.blend"
bpy.ops.wm.save_as_mainfile(filepath=str(blend))
assert all(sha(root/key)==value for key,value in source_hashes.items())
report={"source_sha":os.environ.get("GITHUB_SHA"),"input_sha256":source_hashes,
"output_sha256":sha(path),"blender_sha256":sha(blend),"source_glb":str(path.relative_to(root)),
"blender_source":str(blend.relative_to(root)),"triangles":triangles,"vertices":len(obj.data.vertices),
"design":"one connected roofless stone and timber reconstruction frontage with Gothic arch",
"reused":"retained CC0 sandstone normal and diffuse; retained Eldoria Sawmill oak",
"art_approved":False,"unity_native_player_capture_pending":True,"budget_eur":0}
(ev/"connected-precinct-v16-author.json").write_text(json.dumps(report,indent=2),encoding="utf8")
print("VALORIA_CONNECTED_PRECINCT_V16_AUTHOR_DONE",triangles)
