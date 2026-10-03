"""Valoria volumetric contour-loft landform v1.
One closed irregular landform volume with embedded terrace shelves.
No open terrain sheet, no voxel union, no photogrammetry plates, no gameplay geometry.
"""
import bpy, bmesh, json, math, os, sys
from mathutils import Vector

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
OUT=sys.argv[sys.argv.index("--")+1] if "--" in sys.argv else "ValoriaVolumetricLandform.glb"
REPORT=sys.argv[sys.argv.index("--")+2] if "--" in sys.argv and len(sys.argv)>sys.argv.index("--")+2 else OUT+".json"

def ub(v):
    x,y,z=v
    return (x,-z,y)

def contour(spec,n=72):
    y,cx,cz,rx,rz,phase=spec
    pts=[]
    for i in range(n):
        a=2.0*math.pi*i/n
        radial=1.0+0.060*math.sin(3*a+phase)+0.028*math.sin(7*a-phase*.7)+0.016*math.cos(11*a+phase*.3)
        x=cx+rx*radial*math.cos(a)+0.13*math.sin(5*a+phase)
        z=cz+rz*(1.0+0.040*math.sin(4*a-phase))*math.sin(a)+0.11*math.cos(6*a+phase)
        pts.append(ub((x,y,z)))
    return pts

# Unity-space authored contour sequence. Adjacent equal-height rings create real shelves;
# the rings between shelves create closed cliff slopes. Centers migrate rearward toward Bastion.
specs=[
 (-1.65, 0.00,-0.35,10.75,7.65,0.10),  # buried skirt/base
 ( 0.02, 0.00,-0.45,10.05,7.05,0.22),  # lower terrace outer edge
 ( 0.02, 0.00, 1.80, 7.85,4.10,0.37),  # lower terrace inner edge
 ( 1.08, 0.00, 2.18, 7.35,3.78,0.51),  # mid slope crest / shelf outer
 ( 1.08, 0.00, 2.72, 5.92,2.92,0.66),  # mid shelf inner
 ( 1.92, 0.00, 3.55, 5.62,2.62,0.82),  # upper slope crest / shelf outer
 ( 1.92, 0.00, 4.08, 4.72,2.06,0.98),  # upper shelf inner
 ( 2.58, 0.00, 4.72, 5.18,2.34,1.13),  # Bastion shoulder outer
 ( 2.58, 0.00, 5.22, 4.42,1.86,1.29),  # Bastion shoulder inner/top cap
]
N=72
rings=[contour(s,N) for s in specs]
verts=[v for ring in rings for v in ring]
faces=[]
material_ids=[]

# Connect contour rings. Closed volume; no open sheet perimeter.
for r in range(len(rings)-1):
    a0=r*N;b0=(r+1)*N
    shelf=abs(specs[r][0]-specs[r+1][0])<0.001
    for i in range(N):
        j=(i+1)%N
        faces.append((a0+i,a0+j,b0+j,b0+i))
        material_ids.append(1 if shelf else 0)

# Bottom and top caps.
bottom_center=len(verts)
verts.append(tuple(Vector(rings[0][0])*0 + Vector(ub((0,-2.05,-0.35)))))
for i in range(N):
    j=(i+1)%N
    faces.append((bottom_center,j,i));material_ids.append(0)

top_center=len(verts)
last=specs[-1]
verts.append(ub((last[1],last[0],last[2])))
top0=(len(rings)-1)*N
for i in range(N):
    j=(i+1)%N
    faces.append((top_center,top0+i,top0+j));material_ids.append(1)

mesh=bpy.data.meshes.new("ValoriaVolumetricLandformMesh")
mesh.from_pydata(verts,[],faces);mesh.update()
obj=bpy.data.objects.new("ValoriaVolumetricLandform",mesh)
bpy.context.scene.collection.objects.link(obj)

rock=bpy.data.materials.new("RockSlope")
rock.diffuse_color=(.33,.31,.28,1)
ground=bpy.data.materials.new("TerraceGround")
ground.diffuse_color=(.38,.32,.24,1)
obj.data.materials.append(rock);obj.data.materials.append(ground)
for p,mi in zip(obj.data.polygons,material_ids):p.material_index=mi

# Closed geometry lets Blender robustly recalculate outward normals.
bm=bmesh.new();bm.from_mesh(mesh)
bmesh.ops.recalc_face_normals(bm,faces=bm.faces)
nonmanifold=sum(1 for e in bm.edges if len(e.link_faces)!=2)
bm.to_mesh(mesh);bm.free();mesh.update()
if nonmanifold!=0:
    raise RuntimeError("Volumetric landform must be closed; nonmanifold edges="+str(nonmanifold))

# Very small bevel removes razor-cut shelf edges without turning the landform into a blob.
bpy.context.view_layer.objects.active=obj
obj.select_set(True)
bev=obj.modifiers.new("Geologic edge soften","BEVEL")
bev.width=.055;bev.segments=2;bev.limit_method='ANGLE'
bpy.ops.object.modifier_apply(modifier=bev.name)

# Export only the closed landform.
os.makedirs(os.path.dirname(OUT),exist_ok=True)
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
bpy.ops.export_scene.gltf(filepath=OUT,export_format='GLB',use_selection=True,export_apply=True,export_yup=True)

obj.data.calc_loop_triangles()
# Blender-space bounds for deterministic diagnostics.
pts=[obj.matrix_world@Vector(c) for c in obj.bound_box]
lo=[min(p[k] for p in pts) for k in range(3)]
hi=[max(p[k] for p in pts) for k in range(3)]
report={
 "method":"closed irregular contour-loft landform with embedded terrace shelves",
 "open_sheet":False,
 "closed_volume":True,
 "nonmanifold_edges":nonmanifold,
 "voxel_union":False,
 "photogrammetry_placement":False,
 "heightfield":False,
 "contour_rings":len(specs),
 "ring_vertices":N,
 "triangles":len(obj.data.loop_triangles),
 "bounds_blender":{"min":[round(x,3) for x in lo],"max":[round(x,3) for x in hi]},
 "output_bytes":os.path.getsize(OUT),
 "tripo_credits":0
}
with open(REPORT,"w",encoding="utf-8") as f:json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
