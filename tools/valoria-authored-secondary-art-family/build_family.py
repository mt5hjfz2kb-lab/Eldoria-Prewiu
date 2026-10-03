import bpy, math, os, json
from mathutils import Vector

OUT=os.environ.get("ELDORIA_AUTHORED_FAMILY_DIR","pipeline/candidates/valoria-authored-secondary-art-family-v1")
REPORT=os.environ.get("ELDORIA_AUTHORED_FAMILY_REPORT","pipeline/evidence/valoria-authored-secondary-art-family-v1.json")
SAWMILL=os.environ.get("ELDORIA_CANONICAL_SAWMILL")
if not SAWMILL or not os.path.isfile(SAWMILL):
    raise RuntimeError("Canonical sawmill GLB missing: "+str(SAWMILL))
os.makedirs(OUT,exist_ok=True); os.makedirs(os.path.dirname(REPORT),exist_ok=True)

def clear():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for coll in (bpy.data.meshes,bpy.data.materials,bpy.data.images):
        pass

def mat(name,color,rough=.72,metal=0.0):
    m=bpy.data.materials.get(name)
    if m:return m
    m=bpy.data.materials.new(name);m.use_nodes=True
    bsdf=m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value=(*color,1)
    bsdf.inputs["Roughness"].default_value=rough
    bsdf.inputs["Metallic"].default_value=metal
    return m

STONE=mat("Eldoria_Stone",(0.56,0.53,0.46),.82)
STONE_DARK=mat("Eldoria_Stone_Dark",(0.36,0.35,0.32),.88)
PLASTER=mat("Eldoria_Warm_Plaster",(0.71,0.65,0.54),.86)
TIMBER=mat("Eldoria_Timber",(0.25,0.16,0.09),.78)
TIMBER_LITE=mat("Eldoria_Timber_Light",(0.39,0.25,0.13),.76)
ROOF=mat("Eldoria_Blue_Slate",(0.12,0.22,0.30),.74)
ROOF_EDGE=mat("Eldoria_Blue_Slate_Light",(0.20,0.31,0.39),.72)
METAL=mat("Eldoria_Iron",(0.12,0.13,0.13),.48,.15)

def apply_mods(obj):
    bpy.context.view_layer.objects.active=obj;obj.select_set(True)
    for mod in list(obj.modifiers):
        try:bpy.ops.object.modifier_apply(modifier=mod.name)
        except:pass
    obj.select_set(False)

def box(name,loc,scale,material,bevel=.04,rot=(0,0,0)):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc,rotation=rot)
    o=bpy.context.object;o.name=name;o.dimensions=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if material:o.data.materials.append(material)
    if bevel>0:
        b=o.modifiers.new("Bevel","BEVEL");b.width=bevel;b.segments=2
        n=o.modifiers.new("WeightedNormal","WEIGHTED_NORMAL");n.keep_sharp=True;n.weight=50
        apply_mods(o)
    return o

def beam(name,a,b,width,depth,material):
    mid=(Vector(a)+Vector(b))/2;vec=Vector(b)-Vector(a)
    o=box(name,mid,(width,depth,vec.length),material,bevel=min(width,depth)*.16)
    o.rotation_mode='QUATERNION';o.rotation_quaternion=vec.to_track_quat('Z','Y')
    return o

def duplicate_semantic_material(source,name,factor,roughness):
    m=source.copy();m.name=name
    if m.use_nodes:
        bsdf=m.node_tree.nodes.get("Principled BSDF")
        if bsdf:
            try:bsdf.inputs["Base Color"].default_value=(*factor,1)
            except:pass
            try:bsdf.inputs["Roughness"].default_value=roughness
            except:pass
    return m

def basecolor_image(material):
    if not material or not material.use_nodes:return None
    candidates=[]
    for n in material.node_tree.nodes:
        if n.type=='TEX_IMAGE' and n.image:
            score=0
            nm=(n.image.name+" "+n.name).lower()
            if "base" in nm or "diff" in nm or "albedo" in nm:score+=10
            candidates.append((score,n.image))
    return sorted(candidates,key=lambda x:x[0],reverse=True)[0][1] if candidates else None

def sample_face_rgb(obj,poly,image):
    me=obj.data
    uv=me.uv_layers.active
    if not image or not uv or not image.has_data:return (0.5,0.5,0.5)
    u=v=0.0;n=0
    for li in poly.loop_indices:
        co=uv.data[li].uv;u+=co.x;v+=co.y;n+=1
    if n==0:return (0.5,0.5,0.5)
    u=(u/n)%1.0;v=(v/n)%1.0
    x=max(0,min(image.size[0]-1,int(u*(image.size[0]-1))))
    y=max(0,min(image.size[1]-1,int(v*(image.size[1]-1))))
    idx=(y*image.size[0]+x)*4
    px=image.pixels
    return (float(px[idx]),float(px[idx+1]),float(px[idx+2]))

def build_sawmill_from_canonical():
    clear()
    bpy.ops.import_scene.gltf(filepath=SAWMILL,merge_vertices=False)
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    if not meshes:raise RuntimeError("Canonical sawmill imported without mesh")
    # Join only if importer created multiple mesh objects; preserve all original vertices/faces.
    if len(meshes)>1:
        bpy.ops.object.select_all(action='DESELECT')
        for o in meshes:o.select_set(True)
        bpy.context.view_layer.objects.active=meshes[0]
        bpy.ops.object.join();meshes=[bpy.context.object]
    o=meshes[0];o.name="Valoria_Authored_Aserradero_v1"
    me=o.data;me.update(calc_edges=True,calc_edges_loose=True)
    if len(me.materials)<1:raise RuntimeError("Canonical sawmill has no material")
    source=me.materials[0]
    image=basecolor_image(source)
    # Semantic slots preserve the source texture graph; factors/roughness create shared family response.
    semantic=[
        duplicate_semantic_material(source,"Eldoria_Sawmill_Foundation",(0.76,0.72,0.64),.86),
        duplicate_semantic_material(source,"Eldoria_Sawmill_Timber",(0.70,0.57,0.42),.78),
        duplicate_semantic_material(source,"Eldoria_Sawmill_Roof",(0.55,0.68,0.79),.74),
        duplicate_semantic_material(source,"Eldoria_Sawmill_Infill",(0.96,0.91,0.81),.84),
    ]
    while len(me.materials):me.materials.pop(index=0)
    for m in semantic:me.materials.append(m)

    zs=[(o.matrix_world@v.co).z for v in me.vertices]
    zmin,zmax=min(zs),max(zs);h=max(1e-6,zmax-zmin)
    samples=[]
    for p in me.polygons:
        centre=o.matrix_world@p.center
        zn=(centre.z-zmin)/h
        normal=(o.matrix_world.to_3x3()@p.normal).normalized()
        rgb=sample_face_rgb(o,p,image)
        lum=.2126*rgb[0]+.7152*rgb[1]+.0722*rgb[2]
        warm=rgb[0]-(rgb[2]*.92)
        samples.append((p,zn,normal,rgb,lum,warm))

    # Derive timber from the asset's own albedo distribution instead of absolute RGB thresholds.
    # This is robust to Blender's color-space conversion and keeps the classification source-led.
    vertical_mid=[x for x in samples if .14<=x[1]<=.78 and abs(x[2].z)<.48]
    vertical_lums=sorted(x[4] for x in vertical_mid)
    if not vertical_lums: raise RuntimeError("No mid-height vertical faces available for semantic split")
    q_index=max(0,min(len(vertical_lums)-1,int(len(vertical_lums)*.32)))
    timber_lum_threshold=vertical_lums[q_index]
    warm_values=sorted(x[5] for x in vertical_mid)
    warm_median=warm_values[len(warm_values)//2]

    counts=[0,0,0,0]
    lum_stats={"min":min(x[4] for x in samples),"max":max(x[4] for x in samples),
               "timber_threshold":timber_lum_threshold,"warm_median":warm_median}
    for p,zn,normal,rgb,lum,warm in samples:
        # Foundation: conservative lower band only. The canonical mesh geometry is unchanged.
        if zn < .135:
            idx=0
        # Roof: upper surfaces with a clear upward-facing component.
        elif zn > .50 and normal.z > .20:
            idx=2
        # Timber: darkest third of plausible structural vertical faces, with a small warm-color assist.
        elif .14<=zn<=.78 and abs(normal.z)<.48 and (lum<=timber_lum_threshold or (lum<=timber_lum_threshold*1.18 and warm>=warm_median)):
            idx=1
        else:
            idx=3
        p.material_index=idx;counts[idx]+=1

    total_faces=max(1,sum(counts))
    if counts[1] < total_faces*.04: raise RuntimeError("Semantic timber split too small: %s"%counts)
    if counts[2] < total_faces*.02: raise RuntimeError("Semantic roof split too small: %s"%counts)
    if counts[3] < total_faces*.10: raise RuntimeError("Semantic infill split too small: %s"%counts)

    me.calc_loop_triangles()
    return "Valoria_Authored_Aserradero_v1.glb",{
        "source":"Unity/Assets/Eldoria/Resources/Valoria/Valoria_Aserradero_AP2_v1.glb",
        "triangles":len(me.loop_triangles),
        "vertices":len(me.vertices),
        "semantic_face_counts":{"foundation":counts[0],"timber":counts[1],"roof":counts[2],"infill":counts[3]},
        "semantic_thresholds":lum_stats,
        "source_image":image.name if image else None
    }

def build_wall():
    clear()
    # Reusable 4.8m support module: large stone masses first, secondary timber/blue identity second.
    box("foundation",(0,0,.18),(4.95,1.05,.36),STONE_DARK,bevel=.055)
    box("wall_core",(0,0,.92),(4.80,.88,1.52),STONE,bevel=.065)
    for row in range(4):
        z=.42+row*.34;offset=.38 if row%2 else 0;x=-2.35-offset;idx=0
        while x<2.34:
            w=.72+(idx%3)*.08
            box(f"ashlar_{row}_{idx}",(x+w/2,-.48,z),(w-.035,.13,.27),STONE_DARK if (idx+row)%7==0 else STONE,bevel=.022)
            x+=w;idx+=1
    box("wall_cap",(0,0,1.72),(4.98,1.02,.20),STONE_DARK,bevel=.035)
    # Lower, irregular crenellation cadence avoids VQB's old machine-stamped rhythm.
    for i,(x,w,h) in enumerate([(-2.05,.58,.50),(-.78,.52,.42),(.55,.62,.54),(1.90,.56,.46)]):
        box(f"merlon_{i}",(x,0,1.98+h*.5),(w,.90,h),STONE,bevel=.04)
    # Shared timber brace and restrained blue marker tie the support vocabulary to the building family.
    box("timber_binding",(0,-.57,1.25),(3.65,.12,.13),TIMBER,bevel=.018)
    box("blue_ward_marker",(0,-.65,1.48),(.24,.05,.64),ROOF,bevel=.014)
    return "Valoria_Authored_Wall_v1.glb",{}

def build_gate():
    clear()
    # Gate support pair, not a monumental replacement gatehouse.
    for x in (-1.72,1.72):
        box(f"foot_{x}",(x,0,.20),(1.42,1.55,.40),STONE_DARK,bevel=.06)
        box(f"pier_{x}",(x,0,1.58),(1.16,1.30,2.92),STONE,bevel=.075)
        for z in (.55,1.18,1.82,2.46):
            box(f"pier_course_{x}_{z}",(x,-.69,z),(1.23,.12,.11),STONE_DARK,bevel=.018)
        box(f"pier_cap_{x}",(x,0,3.08),(1.42,1.52,.22),STONE_DARK,bevel=.04)
        # modest roof hood echoes the Aserradero roof instead of another tower.
        box(f"hood_{x}",(x,0,3.42),(1.58,1.72,.18),ROOF,bevel=.035,rot=(0,math.radians(3 if x<0 else -3),0))
        box(f"marker_{x}",(x,-.79,2.15),(.24,.05,.72),ROOF_EDGE,bevel=.014)
    box("lintel",(0,0,2.70),(2.62,1.04,.58),STONE,bevel=.07)
    beam("timber_crossbrace",(-1.15,-.59,2.40),(1.15,-.59,2.40),.18,.20,TIMBER)
    # Open aperture by design; canonical gameplay gate/route remains authoritative.
    return "Valoria_Authored_Gate_v1.glb",{}

def normalize_and_export(filename,meta):
    objs=[o for o in bpy.context.scene.objects if o.type=='MESH']
    if not objs:raise RuntimeError("No mesh objects for "+filename)
    mins=[1e9]*3;maxs=[-1e9]*3
    for o in objs:
        for c in o.bound_box:
            w=o.matrix_world@Vector(c)
            for i in range(3):mins[i]=min(mins[i],w[i]);maxs[i]=max(maxs[i],w[i])
    cx=(mins[0]+maxs[0])/2;cy=(mins[1]+maxs[1])/2;z0=mins[2]
    for o in objs:o.location-=Vector((cx,cy,z0))
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0]
    path=os.path.abspath(os.path.join(OUT,filename))
    bpy.ops.export_scene.gltf(filepath=path,export_format='GLB',use_selection=True,export_apply=True,export_yup=True)
    tris=0
    for o in objs:o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles)
    meta.update({"file":filename,"path":path,"objects":len(objs),"triangles":tris,
      "materials":sorted({m.name for o in objs for m in o.data.materials if m}),"bytes":os.path.getsize(path)})
    if meta["bytes"]<5000:raise RuntimeError("Export too small: "+filename)
    return meta

results=[]
name,meta=build_sawmill_from_canonical();results.append(normalize_and_export(name,meta))
name,meta=build_wall();results.append(normalize_and_export(name,meta))
name,meta=build_gate();results.append(normalize_and_export(name,meta))

with open(REPORT,"w",encoding="utf-8") as f:
    json.dump({"schema_version":2,"family":"Valoria Authored Secondary Art Family v1",
      "principles":["canonical Aserradero geometry preserved","source-level semantic material slots",
        "shared stone/timber/infill/blue-slate vocabulary","metric modular wall/gate support","beveled support geometry","zero paid generation"],
      "assets":results},f,indent=2)
print("VALORIA_AUTHORED_SECONDARY_FAMILY=PASS")
print(json.dumps(results,indent=2))
