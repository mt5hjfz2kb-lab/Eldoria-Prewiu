import argparse, math, os, json, sys
import bpy

def args():
    argv=sys.argv
    argv=argv[argv.index("--")+1:] if "--" in argv else []
    p=argparse.ArgumentParser()
    p.add_argument("--output-dir",required=True)
    p.add_argument("--report",required=True)
    return p.parse_args(argv)

A=args()
bpy.ops.wm.read_factory_settings(use_empty=True)

def mat(name,color,rough=.78,metal=0.0):
    m=bpy.data.materials.new(name)
    m.diffuse_color=(*color,1)
    m.use_nodes=True
    bsdf=m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value=(*color,1)
    bsdf.inputs["Roughness"].default_value=rough
    bsdf.inputs["Metallic"].default_value=metal
    return m

STONE=mat("Eldoria Warm Structural Stone",(0.50,0.45,0.37))
STONE_DARK=mat("Eldoria Structural Shadow Stone",(0.31,0.30,0.27))
STONE_LIGHT=mat("Eldoria Structural Cornice Stone",(0.62,0.57,0.48))
SLATE=mat("Eldoria Slate Cap",(0.16,0.20,0.23))
BLUE=mat("Eldoria Civic Blue",(0.08,0.22,0.42))

def box(name,loc,scale,material=STONE,bevel=.06):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=bpy.context.object; o.name=name; o.dimensions=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel>0:
        b=o.modifiers.new("authored edge","BEVEL"); b.width=bevel; b.segments=2
        b.limit_method='ANGLE'
        bpy.context.view_layer.objects.active=o
        bpy.ops.object.modifier_apply(modifier=b.name)
    o.data.materials.append(material)
    return o

def wedge(name,center,r0,r1,a0,a1,depth,material=STONE_LIGHT):
    # vertical XY ring segment, depth on Y in Unity after glTF Y-up conversion
    verts=[]; faces=[]
    for y in (-depth/2,depth/2):
        for r,a in ((r0,a0),(r1,a0),(r1,a1),(r0,a1)):
            x=center[0]+r*math.cos(a); z=center[2]+r*math.sin(a)
            verts.append((x,center[1]+y,z))
    faces=[(0,1,2,3),(4,7,6,5),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)]
    me=bpy.data.meshes.new(name+"Mesh"); me.from_pydata(verts,[],faces); me.update()
    o=bpy.data.objects.new(name,me); bpy.context.collection.objects.link(o); o.data.materials.append(material)
    return o

def arch_ring(prefix,cx,zbase,width,height,depth):
    radius=width*.50
    center_z=zbase+height-radius
    r0=radius*.78; r1=radius
    for i in range(11):
        a0=math.radians(18+i*(144/11)); a1=math.radians(18+(i+1)*(144/11))
        wedge(prefix+f" voussoir {i:02d}",(cx,0,center_z),r0,r1,a0,a1,depth,STONE_LIGHT)

def facade(name,width,height,depth,bays,parapet=True,banner=False):
    # Foundation / silhouette frame
    box(name+" foundation",(0,0,0.22),(width,depth,0.44),STONE_DARK,.04)
    box(name+" upper spandrel",(0,0,height-.72),(width,depth*.92,1.44),STONE,.05)
    bayw=width/(bays+0.72)
    opening=bayw*.60
    pierw=(width-opening*bays)/(bays+1)
    x=-width/2+pierw/2
    for i in range(bays+1):
        box(name+f" buttress {i}",(x,0,height*.42),(pierw*.78,depth*1.10,height*.77),STONE,.055)
        box(name+f" buttress cap {i}",(x,-.03,height*.80),(pierw*1.18,depth*1.18,.22),STONE_LIGHT,.04)
        x += pierw/2 + (opening if i<bays else 0) + pierw/2
    start=-width/2+pierw+opening/2
    for i in range(bays):
        cx=start+i*(opening+pierw)
        arch_ring(name+f" arch {i}",cx,.42,opening,height*.57,depth*1.07)
        box(name+f" recessed shadow {i}",(cx,depth*.43,height*.35),(opening*.78,.08,height*.46),STONE_DARK,.02)
    box(name+" cornice",(0,-.02,height-.13),(width*1.04,depth*1.16,.24),STONE_LIGHT,.045)
    if parapet:
        box(name+" parapet",(0,0,height+.12),(width*.99,depth*.86,.34),STONE,.04)
        count=max(5,int(width/1.2))
        for i in range(count):
            x=-width*.45+i*(width*.90/max(1,count-1))
            box(name+f" parapet merlon {i}",(x,0,height+.42),(width/(count*2.7),depth*.91,.36),STONE_LIGHT,.035)
    if banner:
        box(name+" civic banner",(0,-depth*.57,height*.61),(width*.16,.05,height*.46),BLUE,.01)

def export_group(filename,builder):
    # delete previous authored objects but retain shared materials
    for o in list(bpy.data.objects): bpy.data.objects.remove(o,do_unlink=True)
    builder()
    for o in bpy.context.scene.objects:o.select_set(True)
    bpy.context.view_layer.objects.active=next(iter(bpy.context.scene.objects),None)
    path=os.path.join(A.output_dir,filename)
    os.makedirs(os.path.dirname(path),exist_ok=True)
    bpy.ops.export_scene.gltf(filepath=path,export_format="GLB",use_selection=True,export_apply=True,export_yup=True)
    return {"file":filename,"bytes":os.path.getsize(path),"objects":len(bpy.context.scene.objects)}

reports=[]
reports.append(export_group("CivicRetainingBay.glb",lambda: facade("Civic Retaining Bay",6.4,4.8,1.25,2,True,True)))
reports.append(export_group("LowerArcadedFront.glb",lambda: facade("Lower Arcaded Front",9.2,3.4,1.35,3,True,False)))

def cuartel():
    facade("Cuartel Terrace Support",5.5,3.6,1.55,2,True,True)
    box("Cuartel side return",(2.50,1.00,1.35),(1.15,2.4,2.55),STONE,.06)
    box("Cuartel side cornice",(2.50,1.00,2.66),(1.34,2.55,.22),STONE_LIGHT,.04)
reports.append(export_group("CuartelTerraceSupport.glb",cuartel))

with open(A.report,"w",encoding="utf-8") as f:
    json.dump({
      "method":"Blender authored structural architecture; explicit arcades, buttresses, cornices, parapets and civic banner relief; no Tripo",
      "sources_reused":["Eldoria stone/slate/civic-blue material grammar"],
      "outputs":reports,"tripo_credits":0,"paid_credits":0
    },f,indent=2)
print(json.dumps(reports,indent=2))
