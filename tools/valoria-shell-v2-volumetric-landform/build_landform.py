"""Valoria volumetric landform v3 — camera-envelope carrier.
A single closed territorial mass extends far beyond every authorized camera, so no
landform perimeter or pedestal can enter frame. The city elevation is a local feature
inside that mass, not the mass itself.
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

def local_city_height(x,z):
    # Broad architectural elevation bands; no full-width terrace rings.
    y=.02
    y+=.98*smooth(-1.90,-.10,z)
    y+=.78*smooth(1.80,3.15,z)
    y+=.62*smooth(4.35,5.72,z)
    # Keep the central Bastion approach gently crowned.
    y+=.16*(1.0-smooth(3.8,7.8,abs(x)))*smooth(3.7,5.4,z)
    return y

def territorial_height(x,z):
    # The urban landform is only a local rise inside a much larger camera-envelope mass.
    # Outside the city ellipse the carrier falls into a low natural valley and keeps
    # extending beyond the frustum, so the viewer can never see an island perimeter.
    q=math.sqrt((x/12.8)**2+((z-1.1)/10.0)**2)
    city_w=1.0-smooth(.70,1.12,q)
    outer=-1.18 + .055*math.sin(x*.19) + .050*math.sin(z*.23) + .025*math.sin((x+z)*.41)
    city=local_city_height(x,z)
    y=outer*(1.0-city_w)+city*city_w
    # Small macro breakup only outside occupied platforms.
    breakup=(.055*math.sin(x*.63+z*.22)+.035*math.sin(x*.31-z*.57))
    y+=breakup*(1.0-smooth(.30,.78,city_w))
    return y

N=96
# Non-uniform radial samples put density where the city changes elevation and very
# little geometry in the off-camera territorial carrier.
RADIAL=[0.00,.06,.12,.18,.25,.33,.42,.52,.63,.74,.84,.92,1.00]
RX=34.0
RZ=25.5
CZ=.70
BOTTOM=-4.60

verts=[]
rings=[]

# Center point.
verts.append(ub((0.0,territorial_height(0.0,CZ),CZ)))
center_idx=0

for ri,f in enumerate(RADIAL[1:],start=1):
    row=[]
    for i in range(N):
        a=2.0*math.pi*i/N
        # Irregularity grows outward; inner city stays compositionally controlled.
        warp=1.0+(0.010+0.035*f)*math.sin(3*a+.18)+(0.006+0.020*f)*math.sin(7*a-.48)+.008*f*math.cos(11*a)
        x=f*RX*warp*math.cos(a)+(.08+.18*f)*math.sin(5*a)
        z=CZ+f*RZ*(1.0+.026*f*math.sin(4*a-.22))*math.sin(a)+.10*f*math.cos(6*a+.35)
        row.append(len(verts))
        verts.append(ub((x,territorial_height(x,z),z)))
    rings.append(row)

faces=[]
# Center fan.
for i in range(N):
    j=(i+1)%N
    faces.append((center_idx,rings[0][i],rings[0][j]))

# Continuous territorial top.
for r in range(len(rings)-1):
    arow,brow=rings[r],rings[r+1]
    for i in range(N):
        j=(i+1)%N
        faces.append((arow[i],brow[i],brow[j],arow[j]))

# Close the volume far outside the authorized camera envelope.
outer=rings[-1]
bottom=[]
for i in range(N):
    p=Vector(verts[outer[i]])
    idx=len(verts);verts.append((p.x,p.y,BOTTOM));bottom.append(idx)
for i in range(N):
    j=(i+1)%N
    faces.append((outer[i],bottom[i],bottom[j],outer[j]))
bottom_center=len(verts)
verts.append(ub((0.0,BOTTOM,CZ)))
for i in range(N):
    j=(i+1)%N
    faces.append((bottom_center,bottom[j],bottom[i]))

mesh=bpy.data.meshes.new("ValoriaCameraEnvelopeLandformMesh")
mesh.from_pydata(verts,[],faces);mesh.update()
obj=bpy.data.objects.new("ValoriaCameraEnvelopeLandform",mesh)
bpy.context.scene.collection.objects.link(obj)

mat=bpy.data.materials.new("LandformSurface")
mat.diffuse_color=(.35,.32,.27,1)
obj.data.materials.append(mat)

bm=bmesh.new();bm.from_mesh(mesh)
bmesh.ops.recalc_face_normals(bm,faces=bm.faces)
nonmanifold=sum(1 for e in bm.edges if len(e.link_faces)!=2)
bm.to_mesh(mesh);bm.free();mesh.update()
if nonmanifold!=0:
    raise RuntimeError("Camera-envelope landform must be closed; nonmanifold edges="+str(nonmanifold))

# Only soften the remote closure rim. No bevel broad enough to flatten the authored slope.
bpy.context.view_layer.objects.active=obj;obj.select_set(True)
bev=obj.modifiers.new("Remote rim soften","BEVEL")
bev.width=.08;bev.segments=2;bev.limit_method='ANGLE'
bpy.ops.object.modifier_apply(modifier=bev.name)

os.makedirs(os.path.dirname(OUT),exist_ok=True)
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
bpy.ops.export_scene.gltf(filepath=OUT,export_format='GLB',use_selection=True,export_apply=True,export_yup=True)

obj.data.calc_loop_triangles()
pts=[obj.matrix_world@Vector(c) for c in obj.bound_box]
lo=[min(p[k] for p in pts) for k in range(3)]
hi=[max(p[k] for p in pts) for k in range(3)]
report={
 "method":"closed camera-envelope territorial carrier with local city elevation",
 "open_sheet":False,
 "closed_volume":True,
 "visible_perimeter_intended":False,
 "carrier_radius_x":RX,
 "carrier_radius_z":RZ,
 "nonmanifold_edges":nonmanifold,
 "voxel_union":False,
 "photogrammetry_placement":False,
 "global_terrace_rings":False,
 "radial_samples":len(RADIAL),
 "ring_vertices":N,
 "triangles":len(obj.data.loop_triangles),
 "bounds_blender":{"min":[round(x,3) for x in lo],"max":[round(x,3) for x in hi]},
 "output_bytes":os.path.getsize(OUT),
 "tripo_credits":0
}
with open(REPORT,"w",encoding="utf-8") as h:json.dump(report,h,indent=2)
print(json.dumps(report,indent=2))
