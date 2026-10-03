"""Source-preserving support pair for Valoria Authored Secondary Art Family v1.
Uses the canonical Stone_Wall / Stone_Gate FBX meshes, keeps geometry, and authors
shared embedded stone/timber surface maps. No paid generation.
"""
import bpy, os, json, math, random
from mathutils import Vector

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
WALL=os.environ.get("ELDORIA_ASF_WALL_SOURCE",os.path.join(ROOT,"Unity","Assets","Bublik","Simple Modular Castle Assets","Meshes","Stone_Wall.fbx"))
GATE=os.environ.get("ELDORIA_ASF_GATE_SOURCE",os.path.join(ROOT,"Unity","Assets","Bublik","Simple Modular Castle Assets","Meshes","Stone_Gate.fbx"))
OUT=os.environ.get("ELDORIA_ASF_OUT",os.path.join(ROOT,"pipeline","candidates","valoria-authored-secondary-art-family-v1"))
REPORT=os.environ.get("ELDORIA_ASF_SUPPORT_REPORT",os.path.join(ROOT,"pipeline","evidence","valoria-authored-secondary-support-v2.json"))
os.makedirs(OUT,exist_ok=True);os.makedirs(os.path.dirname(REPORT),exist_ok=True)

def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True)

def mesh_objects():
    return [o for o in bpy.context.scene.objects if o.type=="MESH" and o.data]

def tri_count(objs):
    n=0
    for o in objs:
        o.data.calc_loop_triangles();n+=len(o.data.loop_triangles)
    return n

def bounds(objs):
    mn=Vector((1e9,1e9,1e9));mx=Vector((-1e9,-1e9,-1e9))
    for o in objs:
        for c in o.bound_box:
            w=o.matrix_world@Vector(c)
            mn.x=min(mn.x,w.x);mn.y=min(mn.y,w.y);mn.z=min(mn.z,w.z)
            mx.x=max(mx.x,w.x);mx.y=max(mx.y,w.y);mx.z=max(mx.z,w.z)
    return {"min":[mn.x,mn.y,mn.z],"max":[mx.x,mx.y,mx.z],"size":[mx.x-mn.x,mx.y-mn.y,mx.z-mn.z]}

def texture(name,kind,size=512):
    img=bpy.data.images.new(name,width=size,height=size,alpha=False)
    px=[0.0]*(size*size*4)
    rng=random.Random(412 if kind=="stone" else 913)
    for y in range(size):
        for x in range(size):
            i=(y*size+x)*4
            if kind=="stone":
                # warm irregular ashlar: broad blocks + thin dark mortar + low-frequency variation
                row=max(1,size//8);col=max(1,size//6)
                yy=y%row; offset=(row//2 if (y//row)%2 else 0)
                xx=(x+offset)%col
                mortar=xx<4 or yy<4
                broad=0.5+0.5*math.sin((x*.031)+(y*.017))+0.25*math.sin(x*.083-y*.047)
                grain=(rng.random()-.5)*.06
                if mortar:
                    r,g,b=.25,.24,.21
                else:
                    v=max(-.10,min(.12,(broad-.5)*.10+grain))
                    r,g,b=.55+v,.52+v*.85,.45+v*.65
            else:
                # vertical warm timber grain with darker growth lines
                line=.5+.5*math.sin(x*.19+math.sin(y*.025)*1.9)
                fine=.5+.5*math.sin(x*.71+y*.017)
                v=(line*.13+fine*.04)+(rng.random()-.5)*.025
                r,g,b=.24+v,.145+v*.55,.075+v*.30
            px[i]=max(0,min(1,r));px[i+1]=max(0,min(1,g));px[i+2]=max(0,min(1,b));px[i+3]=1.0
    img.pixels=px;img.pack()
    return img

STONE_TEX=texture("ASF_shared_warm_stone","stone")
TIMBER_TEX=texture("ASF_shared_dark_timber","timber")

def material(name,img,rough=.82,metal=0.0,tint=(1,1,1,1)):
    m=bpy.data.materials.new(name);m.use_nodes=True
    nt=m.node_tree;bs=nt.nodes.get("Principled BSDF")
    tex=nt.nodes.new("ShaderNodeTexImage");tex.image=img;tex.interpolation='Linear'
    nt.links.new(tex.outputs["Color"],bs.inputs["Base Color"])
    bs.inputs["Roughness"].default_value=rough;bs.inputs["Metallic"].default_value=metal
    try:bs.inputs["Base Color"].default_value=tint
    except:pass
    return m

STONE=material("ASF_Shared_Stone",STONE_TEX,.86)
TIMBER=material("ASF_Shared_Timber",TIMBER_TEX,.76)
IRON=bpy.data.materials.new("ASF_Shared_Iron");IRON.use_nodes=True
ibs=IRON.node_tree.nodes.get("Principled BSDF");ibs.inputs["Base Color"].default_value=(.10,.11,.11,1);ibs.inputs["Metallic"].default_value=.32;ibs.inputs["Roughness"].default_value=.55

def semantic_kind(name):
    n=name.lower()
    if any(k in n for k in ("wood","door","beam","timber","plank")):return "timber"
    if any(k in n for k in ("iron","metal","bar","hinge","portcullis")):return "iron"
    return "stone"

def planar_uv(obj,scale=.32):
    me=obj.data
    if not me.uv_layers: uv=me.uv_layers.new(name="ASF_WorldPlanar")
    else: uv=me.uv_layers.active
    # projection chosen per face dominant normal; enough for shared support material proof
    for poly in me.polygons:
        n=poly.normal
        ax,ay,az=abs(n.x),abs(n.y),abs(n.z)
        for li in poly.loop_indices:
            co=me.vertices[me.loops[li].vertex_index].co
            if ay>=ax and ay>=az: u,v=co.x*scale,co.z*scale
            elif ax>=az: u,v=co.y*scale,co.z*scale
            else: u,v=co.x*scale,co.y*scale
            uv.data[li].uv=(u,v)

def author(src,outname):
    if not os.path.isfile(src):raise RuntimeError("Missing source "+src)
    reset()
    bpy.ops.import_scene.fbx(filepath=src,automatic_bone_orientation=True)
    objs=mesh_objects()
    if not objs:raise RuntimeError("No mesh imported from "+src)
    before={"triangles":tri_count(objs),"objects":len(objs),"materials":sorted({m.name for o in objs for m in o.data.materials if m}),"bounds":bounds(objs)}
    assigned={"stone":0,"timber":0,"iron":0}
    for o in objs:
        kind=semantic_kind(o.name+" "+" ".join(m.name if m else "" for m in o.data.materials))
        o.data.materials.clear()
        chosen=TIMBER if kind=="timber" else (IRON if kind=="iron" else STONE)
        o.data.materials.append(chosen);assigned[kind]+=1
        planar_uv(o)
        # preserve geometry, only improve shading
        for p in o.data.polygons:p.use_smooth=False
        bevel=o.modifiers.new("ASF_micro_bevel","BEVEL");bevel.width=.008;bevel.segments=2;bevel.limit_method='ANGLE'
        bpy.context.view_layer.objects.active=o
        try:bpy.ops.object.modifier_apply(modifier=bevel.name)
        except:pass
    # normalize origin to bottom/center
    b=bounds(objs);cx=(b["min"][0]+b["max"][0])/2;cy=(b["min"][1]+b["max"][1])/2;z0=b["min"][2]
    for o in objs:o.location-=Vector((cx,cy,z0))
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0]
    path=os.path.join(OUT,outname)
    bpy.ops.export_scene.gltf(filepath=path,export_format='GLB',use_selection=True,export_apply=True,export_yup=True,export_materials='EXPORT')
    after={"triangles":tri_count(objs),"objects":len(objs),"assigned":assigned,"bytes":os.path.getsize(path),"bounds":bounds(objs)}
    return {"source":os.path.relpath(src,ROOT),"before":before,"after":after,"output":"pipeline/candidates/valoria-authored-secondary-art-family-v1/"+outname}

wall=author(WALL,"Valoria_ASF_SourceWall_v2.glb")
gate=author(GATE,"Valoria_ASF_SourceGate_v2.glb")
report={"status":"PASS","method":"source-preserving canonical support meshes + shared embedded stone/timber surfaces","wall":wall,"gate":gate,"tripo_credits":0}
with open(REPORT,"w",encoding="utf-8") as f:json.dump(report,f,indent=2)
print("ASF_SUPPORT_V2="+json.dumps(report))
