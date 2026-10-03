"""Valoria Visual Shell v2 — authored scan-conformed Hero-to-city transition.
Keep territorial CC0 photogrammetry for foreground/rear world scale. The hero scan is NOT rendered
as a slab: it is used as a donor for an authored lofted terrace/cliff skin via Shrinkwrap + UV transfer.
No heightfield, no voxel union, no Tripo. Poly Haven assets are CC0.
"""
import bpy, os, sys, json, math, urllib.request, urllib.parse, hashlib
from pathlib import Path
from mathutils import Vector

ASSETS=[
    {"id":"coast_line_02","name":"foreground inhabited shelf","anchor":(0.0,-3.25,-3.6),"span":54.0,"yaw":180.0,"target_tris":260000,"render":True},
    {"id":"coastal_cliff_01","name":"rear world cliff","anchor":(0.0,-5.2,10.2),"span":54.0,"yaw":180.0,"target_tris":280000,"render":True},
    {"id":"namaqualand_cliff_02","name":"hero geology donor","anchor":(0.0,-2.65,4.7),"span":28.0,"yaw":170.0,"target_tris":220000,"render":False},
]
UA={"User-Agent":"Eldoria-ShellV2/1.0 (+https://polyhaven.com)"}
ROOT=Path(os.path.abspath(os.path.join(os.path.dirname(__file__),"..","..")))
CACHE=Path(os.environ.get("ELDORIA_PH_CACHE",str(ROOT/".tmp_polyhaven")))
CACHE.mkdir(parents=True,exist_ok=True)

bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)

def unity_to_blender(p):
    x,y,z=p
    return Vector((x,-z,y))

def fetch_json(url):
    req=urllib.request.Request(url,headers=UA)
    with urllib.request.urlopen(req,timeout=120) as r:
        return json.load(r)

def download(url,path,expected_md5=None):
    path=Path(path);path.parent.mkdir(parents=True,exist_ok=True)
    if not path.exists() or path.stat().st_size==0:
        req=urllib.request.Request(url,headers=UA)
        with urllib.request.urlopen(req,timeout=240) as src, open(path,"wb") as dst:
            while True:
                chunk=src.read(1024*1024)
                if not chunk:break
                dst.write(chunk)
    if expected_md5:
        h=hashlib.md5(path.read_bytes()).hexdigest()
        if h.lower()!=expected_md5.lower():
            raise RuntimeError(f"MD5 mismatch for {path}: {h} != {expected_md5}")
    return path

def acquire_gltf(asset_id):
    files=fetch_json("https://api.polyhaven.com/files/"+asset_id)
    try:
        entry=files["gltf"]["1k"]["gltf"]
    except Exception:
        resolutions=files.get("gltf",{})
        if not resolutions:raise RuntimeError("No glTF published for "+asset_id)
        key=sorted(resolutions.keys(),key=lambda x:int(x[:-1]) if x.endswith("k") and x[:-1].isdigit() else 999)[0]
        entry=resolutions[key]["gltf"]
    dest=CACHE/asset_id
    main=download(entry["url"],dest/Path(urllib.parse.urlparse(entry["url"]).path).name,entry.get("md5"))
    for rel,inc in (entry.get("include") or {}).items():
        download(inc["url"],dest/rel,inc.get("md5"))
    if main.parent!=dest:
        target=dest/main.name
        target.write_bytes(main.read_bytes());main=target
    return main,entry

def mesh_objects(objs):
    return [o for o in objs if o.type=="MESH"]

def bounds_world(objs):
    pts=[]
    for o in mesh_objects(objs):
        pts.extend([o.matrix_world @ Vector(c) for c in o.bound_box])
    if not pts:raise RuntimeError("No mesh bounds")
    lo=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
    hi=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
    return lo,hi

def import_and_place(spec):
    gltf,entry=acquire_gltf(spec["id"])
    before=set(bpy.context.scene.objects)
    bpy.ops.import_scene.gltf(filepath=str(gltf))
    imported=[o for o in bpy.context.scene.objects if o not in before]
    meshes=mesh_objects(imported)
    if not meshes:raise RuntimeError("No meshes imported: "+spec["id"])
    root=bpy.data.objects.new("PH · "+spec["name"],None)
    bpy.context.scene.collection.objects.link(root)
    tops=[o for o in imported if o.parent is None]
    for o in tops:o.parent=root
    root.rotation_euler[2]=math.radians(-spec["yaw"])
    bpy.context.view_layer.update()
    lo,hi=bounds_world(imported)
    size=hi-lo
    scale=spec["span"]/max(size.x,size.y,.001)
    root.scale=(scale,scale,scale)
    bpy.context.view_layer.update()

    raw_tris=sum(sum(max(0,len(p.vertices)-2) for p in o.data.polygons) for o in meshes)
    target=spec.get("target_tris",raw_tris)
    if raw_tris>target:
        ratio=max(.08,min(1.0,target/float(raw_tris)))
        for o in meshes:
            if o.data and len(o.data.polygons)>100:
                bpy.context.view_layer.objects.active=o
                o.select_set(True)
                mod=o.modifiers.new("fixed-camera scan decimate","DECIMATE")
                mod.decimate_type="COLLAPSE";mod.ratio=ratio
                bpy.ops.object.modifier_apply(modifier=mod.name)
                o.select_set(False)

    bpy.context.view_layer.update()
    lo,hi=bounds_world(imported)
    center=(lo+hi)*.5
    target_pos=unity_to_blender(spec["anchor"])
    root.location += Vector((target_pos.x-center.x,target_pos.y-center.y,target_pos.z-lo.z))
    bpy.context.view_layer.update()
    lo,hi=bounds_world(imported)
    tris=sum(sum(max(0,len(p.vertices)-2) for p in o.data.polygons) for o in meshes)
    return {
        "id":spec["id"],"name":spec["name"],"source_tris":raw_tris,"tris":tris,
        "bounds":{"min":[round(v,3) for v in lo],"max":[round(v,3) for v in hi]},
        "download_url":entry["url"],"render":spec["render"]
    },root,meshes

def material_from(meshes):
    for o in sorted(meshes,key=lambda x:len(x.data.polygons),reverse=True):
        if len(o.data.materials)>0 and o.data.materials[0] is not None:
            return o.data.materials[0]
    raise RuntimeError("Hero donor has no usable material")

def largest_mesh(meshes):
    return max(meshes,key=lambda o:len(o.data.polygons))

def authored_transition(donor_meshes):
    donor=largest_mesh(donor_meshes)
    donor_mat=material_from(donor_meshes)

    # Camera-authored cross sections: lower district -> mid terrace -> Bastion rock skirt.
    # This is a lofted environment skin, not a generated terrain/heightfield.
    key_profiles=[
        (-1.55,0.48,7.80),
        (-0.35,0.68,7.55),
        ( 0.85,1.02,7.10),
        ( 2.05,1.36,6.55),
        ( 3.20,1.73,6.05),
        ( 4.30,2.05,5.55),
        ( 5.35,2.33,5.05),
        ( 6.30,2.52,4.65),
    ]
    cols=13
    profiles=[]
    # Interpolate each authored band to give Shrinkwrap enough vertices without creating a generic world grid.
    for k in range(len(key_profiles)-1):
        a=key_profiles[k];b=key_profiles[k+1]
        steps=3 if k<5 else 2
        for s in range(steps):
            t=s/float(steps)
            z=a[0]+(b[0]-a[0])*t
            y=a[1]+(b[1]-a[1])*t
            w=a[2]+(b[2]-a[2])*t
            profiles.append((z,y,w))
    profiles.append(key_profiles[-1])

    verts=[]
    for z,y,w in profiles:
        row=[]
        for i in range(cols):
            nx=-1.0+2.0*i/(cols-1)
            # Slight centre crown and irregular shoulder rhythm; fixed, deterministic and camera-authored.
            yy=y + .07*(1.0-abs(nx)) + .035*math.sin((z+2.1)*1.7+nx*2.3)
            xx=nx*w
            row.append(len(verts))
            verts.append(tuple(unity_to_blender((xx,yy,z))))
        # row indices are implicit by insertion order

    faces=[]
    rows=len(profiles)
    for r in range(rows-1):
        for c in range(cols-1):
            a=r*cols+c;b=a+1;d=(r+1)*cols+c;e=d+1
            faces.append((a,d,e,b))

    # Side skirts make the authored piece read as a cut mountainside instead of a paper sheet.
    skirt_y=-2.75
    for side in (0,cols-1):
        for r in range(rows-1):
            a=r*cols+side;b=(r+1)*cols+side
            va=verts[a];vb=verts[b]
            ba=len(verts);verts.append((va[0],va[1],skirt_y))
            bb=len(verts);verts.append((vb[0],vb[1],skirt_y))
            faces.append((a,b,bb,ba))

    mesh=bpy.data.meshes.new("ValoriaAuthoredScanConformedTransitionMesh")
    mesh.from_pydata(verts,[],faces);mesh.update()
    skin=bpy.data.objects.new("ValoriaAuthoredScanConformedTransition",mesh)
    bpy.context.scene.collection.objects.link(skin)
    skin.data.materials.append(donor_mat)

    # Project the authored silhouette vertically onto the photogrammetry donor.
    bpy.context.view_layer.objects.active=skin;skin.select_set(True)
    sw=skin.modifiers.new("ConformToHeroScan","SHRINKWRAP")
    sw.target=donor
    sw.wrap_method='PROJECT'
    sw.wrap_mode='ON_SURFACE'
    sw.use_project_z=True
    sw.use_positive_direction=True
    sw.use_negative_direction=True
    sw.project_limit=3.2
    sw.offset=.035
    bpy.ops.object.modifier_apply(modifier=sw.name)

    # Transfer the scan UVs so the custom silhouette inherits real scanned texture detail.
    if donor.data.uv_layers:
        # Data Transfer does not create the destination UV layer for us.
        # Seed one explicitly so the scan texture survives export on the authored skin.
        uv_name=donor.data.uv_layers.active.name if donor.data.uv_layers.active else "UVMap"
        skin.data.uv_layers.new(name=uv_name)
        skin.data.uv_layers.active=skin.data.uv_layers[-1]
        dt=skin.modifiers.new("TransferScanUV","DATA_TRANSFER")
        dt.object=donor
        dt.use_loop_data=True
        dt.data_types_loops={'UV'}
        dt.loop_mapping='POLYINTERP_NEAREST'
        bpy.ops.object.modifier_apply(modifier=dt.name)

    for p in skin.data.polygons:p.use_smooth=True

    # Structural terrace lips + civic ascent: buried into the same geological section, never one long ribbon.
    pieces=[]
    def box(name,c,dims,bevel=.06):
        bpy.ops.mesh.primitive_cube_add(size=1,location=unity_to_blender(c))
        o=bpy.context.object;o.name=name
        o.dimensions=(dims[0],dims[2],dims[1])
        bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
        if bevel:
            m=o.modifiers.new("buried edge","BEVEL");m.width=bevel;m.segments=2
            bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=m.name)
        o.data.materials.append(donor_mat)
        # Structural pieces use deterministic cube UVs; they are small/buried and must not
        # fall back to a flat base-colour material in Unity.
        bpy.context.view_layer.objects.active=o
        o.select_set(True)
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.uv.cube_project(cube_size=1.25)
        bpy.ops.object.mode_set(mode='OBJECT')
        o.select_set(False)
        pieces.append(o)
        return o

    for level,(z,y,w) in enumerate([(-.55,.74,6.7),(2.55,1.55,5.6),(4.75,2.18,4.7)]):
        # Split wall leaves the central civic route open and reads as retaining structure.
        seg=w*.68
        box(f"Retaining_{level}_W",(-w*.53,y,z),(seg,.78,.34),.08)
        box(f"Retaining_{level}_E",( w*.53,y,z),(seg,.78,.34),.08)

    n=13
    for i in range(n):
        t=i/(n-1)
        z=-1.30+7.05*t
        # authored ascent follows the terrace stack rather than a flat ribbon
        y=.56 + 1.90*(t*t*(3-2*t))
        x=.18*math.sin(t*math.pi*2.0)
        box(f"CivicStep_{i:02d}",(x,y,z),(1.62,.12,.63),.035)

    # Keep only the custom transition + structural pieces selected for this local group.
    return skin,pieces

report={"method":"territorial_photogrammetry_plus_authored_shrinkwrap_transition",
        "powered_by":"Poly Haven","license":"CC0","assets":[]}
records=[]
for spec in ASSETS:
    rec,root,meshes=import_and_place(spec)
    report["assets"].append(rec)
    records.append((spec,root,meshes))

hero_spec,hero_root,hero_meshes=records[2]
skin,pieces=authored_transition(hero_meshes)

# Donor remains in Blender only; it is not exported. Foreground/rear scans remain visible.
for o in hero_meshes:
    o.hide_render=True;o.hide_viewport=True

# Export only intended world pieces. This prevents raw hero scan slab boundaries from reaching Unity.
bpy.ops.object.select_all(action="DESELECT")
exported=[]
for spec,root,meshes in records[:2]:
    for o in meshes:
        o.select_set(True);exported.append(o)
skin.select_set(True);exported.append(skin)
for o in pieces:o.select_set(True);exported.append(o)

idx=sys.argv.index("--") if "--" in sys.argv else -1
out=Path(sys.argv[idx+1] if idx>=0 else "ValoriaPhotogrammetryMidground.glb").resolve()
report_path=Path(sys.argv[idx+2] if idx>=0 and len(sys.argv)>idx+2 else str(out.with_suffix(".json"))).resolve()
out.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.gltf(filepath=str(out),export_format="GLB",use_selection=True,export_apply=True,export_materials="EXPORT",export_yup=True)

skin.data.calc_loop_triangles()
report["authored_transition"]={
    "vertices":len(skin.data.vertices),
    "triangles":len(skin.data.loop_triangles),
    "structural_pieces":len(pieces),
    "donor":"namaqualand_cliff_02",
    "donor_visible":False,
    "technique":["authored cross-section loft","Shrinkwrap Project","Data Transfer UV"]
}
report["output_bytes"]=out.stat().st_size
report_path.write_text(json.dumps(report,indent=2),encoding="utf-8")
print("VALORIA_AUTHORED_SCAN_CONFORMED_MIDGROUND_BUILT",json.dumps(report))
