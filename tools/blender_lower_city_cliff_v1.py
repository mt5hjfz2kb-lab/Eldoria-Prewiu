import argparse, math, os, sys, json
import bpy
from mathutils import Vector

def parse_args():
    argv=sys.argv
    argv=argv[argv.index("--")+1:] if "--" in argv else []
    p=argparse.ArgumentParser()
    p.add_argument("--output",required=True)
    p.add_argument("--report",required=True)
    return p.parse_args(argv)

def noise(x,z):
    return (
        math.sin(x*1.37+z*.73)*.11 +
        math.sin(x*2.91-z*1.17)*.055 +
        math.sin(x*.61+z*2.43)*.07 +
        math.sin((x+z)*4.15)*.025
    )

a=parse_args()
bpy.ops.wm.read_factory_settings(use_empty=True)

nx,nz=55,37
z0,z1=-3.2,3.3
verts=[]
uvs=[]
faces=[]

# Irregular trapezoid: narrower camera/front edge, broad rear that buries into hero island.
def half_width(t):
    # v3: narrow camera-facing toe, widening only as the cliff enters the fortress.
    return 2.35 + 2.35*t + .16*math.sin(t*math.pi*2.0)

def center_y(t):
    return -0.10 + 2.00*t + .18*math.sin(t*math.pi)

# Top surface.
for iz in range(nz):
    t=iz/(nz-1)
    y2=z0+(z1-z0)*t
    hw=half_width(t)
    for ix in range(nx):
        u=ix/(nx-1)
        x=-hw+2*hw*u
        edge=abs(u-.5)*2.0
        crown=(1.0-edge*edge)*.22
        z=center_y(t)+crown+noise(x,y2)*(0.48+0.78*(1-edge))
        # carve a subtle central approach valley, not a flat road.
        valley=math.exp(-((x/1.25)**2))*0.13
        z-=valley
        verts.append((x,y2,z))
        uvs.append((x*.095,y2*.095))

for iz in range(nz-1):
    for ix in range(nx-1):
        a0=iz*nx+ix
        b0=a0+1
        c0=a0+nx
        d0=c0+1
        faces.append((a0,c0,d0))
        faces.append((a0,d0,b0))

# Side/front cliff skirt.
top_count=len(verts)
edge_indices=[]
# left
for iz in range(nz): edge_indices.append(iz*nx)
# rear
for ix in range(1,nx): edge_indices.append((nz-1)*nx+ix)
# right descending
for iz in range(nz-2,-1,-1): edge_indices.append(iz*nx+(nx-1))
# front reversed
for ix in range(nx-2,0,-1): edge_indices.append(ix)

ring_top=edge_indices
ring_mid=[]
ring_bottom=[]
for idx in ring_top:
    x,y,z=verts[idx]
    # v2: two fractured wall bands. This creates readable ledges and avoids a single
    # smooth skirt when the mesh is lit from the official strategic camera.
    t=(y-z0)/(z1-z0)
    drop=.66+.20*t + .10*math.sin(x*1.9+y*.8)
    lateral=.10*math.sin(x*2.7-y*1.3)+.05*math.sin(y*3.1)
    ring_mid.append(len(verts))
    verts.append((x*1.018+lateral,y,z-drop*.46 + .05*math.sin(x*3.2)))
    uvs.append((x*.095,y*.095))
    ring_bottom.append(len(verts))
    verts.append((x*1.045-lateral*.35,y,z-drop))
    uvs.append((x*.095,y*.095))

m=len(ring_top)
for i in range(m):
    j=(i+1)%m
    ti=ring_top[i]; tj=ring_top[j]
    mi=ring_mid[i]; mj=ring_mid[j]
    bi=ring_bottom[i]; bj=ring_bottom[j]
    faces.append((ti,mi,mj))
    faces.append((ti,mj,tj))
    faces.append((mi,bi,bj))
    faces.append((mi,bj,mj))

mesh=bpy.data.meshes.new("Eldoria Unified Lower City Cliff v1")
mesh.from_pydata(verts,[],faces)
mesh.update(calc_edges=True)
obj=bpy.data.objects.new("UnifiedLowerCityCliff",mesh)
bpy.context.collection.objects.link(obj)

# UVs.
uv_layer=mesh.uv_layers.new(name="UVMap")
for poly in mesh.polygons:
    for li in poly.loop_indices:
        vi=mesh.loops[li].vertex_index
        uv_layer.data[li].uv=uvs[vi]

# Weighted smoothing via bevel + triangulation, preserving macro silhouette.
for p in mesh.polygons: p.use_smooth=True
bev=obj.modifiers.new("Cliff edge softening","BEVEL")
bev.width=.035
bev.segments=2
bev.limit_method='ANGLE'
bev.angle_limit=math.radians(38)
bpy.context.view_layer.objects.active=obj
obj.select_set(True)
try: bpy.ops.object.modifier_apply(modifier=bev.name)
except Exception: pass

tri=obj.modifiers.new("Triangulate","TRIANGULATE")
bpy.ops.object.modifier_apply(modifier=tri.name)

os.makedirs(os.path.dirname(a.output),exist_ok=True)
bpy.ops.export_scene.gltf(filepath=a.output,export_format="GLB",use_selection=False,export_apply=True,export_yup=True)

mesh.calc_loop_triangles()
report={
    "vertices":len(mesh.vertices),
    "triangles":len(mesh.loop_triangles),
    "bounds_min":[min(v.co[i] for v in mesh.vertices) for i in range(3)],
    "bounds_max":[max(v.co[i] for v in mesh.vertices) for i in range(3)],
    "output":a.output,
    "bytes":os.path.getsize(a.output),
    "tripo_credits":0,
    "method":"Blender procedural organic cliff mesh v3; narrow buried toe + widening fortress wedge + fractured wall bands; hero-island rock material assigned at Unity integration"
}
with open(a.report,"w",encoding="utf-8") as f: json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
