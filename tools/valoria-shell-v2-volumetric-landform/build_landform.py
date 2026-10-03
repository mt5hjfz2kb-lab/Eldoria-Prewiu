"""Valoria volumetric contour landform v2.
Closed irregular polar landform with a continuous top surface and soft city elevation bands.
No open terrain sheet, no global terrace rings, no voxel union, no photogrammetry plates.
"""
import bpy, bmesh, json, math, os, sys
from mathutils import Vector

OUT=sys.argv[sys.argv.index("--")+1] if "--" in sys.argv else "ValoriaVolumetricLandform.glb"
REPORT=sys.argv[sys.argv.index("--")+2] if "--" in sys.argv and len(sys.argv)>sys.argv.index("--")+2 else OUT+".json"

def ub(v):
    x,y,z=v
    return (x,-z,y)

def smooth(a,b,x):
    if b==a:return 0.0
    t=max(0.0,min(1.0,(x-a)/(b-a)))
    return t*t*(3.0-2.0*t)

def city_height(x,z):
    # Broad inhabited bands match the existing lower / middle / upper district elevations
    # without creating full-width horizontal ledges around the landform.
    y=0.03
    y+=1.04*smooth(-1.75,-0.05,z)
    y+=0.82*smooth(1.90,3.18,z)
    y+=0.70*smooth(4.45,5.90,z)
    # Tiny geological modulation disappears on the civic plateaus but breaks sterile gradients.
    plateau_guard=max(
        1.0-smooth(-2.15,-1.65,z)*smooth(-.15,.25,z),
        1.0-smooth(1.55,1.95,z)*smooth(3.18,3.55,z),
        1.0-smooth(4.10,4.45,z)*smooth(5.90,6.25,z)
    )
    noise=(.038*math.sin(x*.73+z*.31)+.024*math.sin(x*1.61-z*.47))*plateau_guard
    crown=.10*(1.0-smooth(4.5,8.5,abs(x)))*smooth(3.3,5.4,z)
    return y+noise+crown

N=84
RINGS=14
RX=10.65
RZ=8.10
CZ=.45
BOTTOM=-2.05
verts=[]
rings=[]

# Center top vertex.
center_idx=0
verts.append(ub((0.0,city_height(0.0,CZ),CZ)))

for r in range(1,RINGS+1):
    f=r/float(RINGS)
    row=[]
    for i in range(N):
        a=2.0*math.pi*i/N
        warp=1.0+(0.018+0.050*f)*math.sin(3*a+.21)+(0.010+0.027*f)*math.sin(7*a-.54)+.012*f*math.cos(11*a+.37)
        x=f*RX*warp*math.cos(a)+.11*f*math.sin(5*a)
        z=CZ+f*RZ*(1.0+.034*f*math.sin(4*a-.16))*math.sin(a)+.10*f*math.cos(6*a+.4)
        h=city_height(x,z)
        # Only the outermost 12% rolls down into the cliff edge; occupied city stays on the continuous top.
        edge=smooth(.88,1.0,f)
        y=(1.0-edge)*h+edge*(-1.42+.08*math.sin(5*a+.2))
        row.append(len(verts));verts.append(ub((x,y,z)))
    rings.append(row)

faces=[]
# Center fan.
first=rings[0]
for i in range(N):
    j=(i+1)%N
    faces.append((center_idx,first[i],first[j]))

# Continuous top quads.
for r in range(len(rings)-1):
    arow,brow=rings[r],rings[r+1]
    for i in range(N):
        j=(i+1)%N
        faces.append((arow[i],brow[i],brow[j],arow[j]))

# Closed irregular cliff skirt.
outer=rings[-1]
bottom=[]
for i in range(N):
    p=Vector(verts[outer[i]])
    idx=len(verts)
    # Blender coordinates: z is Unity y. Preserve x/y footprint; drop only elevation.
    verts.append((p.x,p.y,BOTTOM))
    bottom.append(idx)
for i in range(N):
    j=(i+1)%N
    faces.append((outer[i],bottom[i],bottom[j],outer[j]))

bottom_center=len(verts)
verts.append(ub((0.0,BOTTOM,CZ)))
for i in range(N):
    j=(i+1)%N
    faces.append((bottom_center,bottom[j],bottom[i]))

mesh=bpy.data.meshes.new("ValoriaVolumetricLandformMesh")
mesh.from_pydata(verts,[],faces);mesh.update()
obj=bpy.data.objects.new("ValoriaVolumetricLandform",mesh)
bpy.context.scene.collection.objects.link(obj)

rock=bpy.data.materials.new("LandformSurface")
rock.diffuse_color=(.35,.32,.27,1)
obj.data.materials.append(rock)
for p in obj.data.polygons:p.material_index=0

# Closed mesh => reliable outward normals and deterministic manifold check.
bm=bmesh.new();bm.from_mesh(mesh)
bmesh.ops.recalc_face_normals(bm,faces=bm.faces)
nonmanifold=sum(1 for e in bm.edges if len(e.link_faces)!=2)
bm.to_mesh(mesh);bm.free();mesh.update()
if nonmanifold!=0:
    raise RuntimeError("Volumetric landform must be closed; nonmanifold edges="+str(nonmanifold))

# Minimal edge treatment only; preserve the continuous terrain silhouette.
bpy.context.view_layer.objects.active=obj
obj.select_set(True)
bev=obj.modifiers.new("Cliff rim soften","BEVEL")
bev.width=.045;bev.segments=2;bev.limit_method='ANGLE'
bpy.ops.object.modifier_apply(modifier=bev.name)

os.makedirs(os.path.dirname(OUT),exist_ok=True)
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
bpy.ops.export_scene.gltf(filepath=OUT,export_format='GLB',use_selection=True,export_apply=True,export_yup=True)

obj.data.calc_loop_triangles()
pts=[obj.matrix_world@Vector(c) for c in obj.bound_box]
lo=[min(p[k] for p in pts) for k in range(3)]
hi=[max(p[k] for p in pts) for k in range(3)]
report={
 "method":"closed irregular polar landform with continuous top and soft city elevation bands",
 "open_sheet":False,
 "closed_volume":True,
 "global_terrace_rings":False,
 "nonmanifold_edges":nonmanifold,
 "voxel_union":False,
 "photogrammetry_placement":False,
 "polar_rings":RINGS,
 "ring_vertices":N,
 "triangles":len(obj.data.loop_triangles),
 "bounds_blender":{"min":[round(x,3) for x in lo],"max":[round(x,3) for x in hi]},
 "output_bytes":os.path.getsize(OUT),
 "tripo_credits":0
}
with open(REPORT,"w",encoding="utf-8") as f:json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
