"""Non-destructive camera-scale mountain silhouette pass on existing verified GLB.

Changes only the existing distant mountain mesh; does not rebuild Bastion I.
Use in Blender 4.x: blender -b --python this_file.py -- input.glb output.glb
"""
import bpy, sys, math, json, hashlib, pathlib
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:]
if len(args)!=2:raise SystemExit('Expected input.glb output.glb')
src,dst=map(pathlib.Path,args)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(src))
meshes=[o for o in bpy.data.objects if o.type=='MESH']
candidates=[o for o in meshes if 'mountain' in o.name.lower() and len(o.data.vertices)>500]
if len(candidates)!=1:raise RuntimeError('Ambiguous original mountain meshes: '+str([(o.name,len(o.data.vertices)) for o in candidates]))
o=candidates[0]
verts=o.data.vertices
before=[tuple(v.co) for v in verts]
xs=[v[0] for v in before];ys=[v[1] for v in before]
xmin,xmax=min(xs),max(xs);ymin,ymax=min(ys),max(ys)
if xmax-xmin<20 or ymax-ymin<10:raise RuntimeError('Mountain source AABB too small')
# Three overlapping asymmetrical distant crests. Smooth, bounded uplift and
# one broad notch replace the low-impact local cliff sculpt strategy.
# Original material, UVs, mesh connectivity, architecture and gameplay untouched.
changed=0;max_delta=0.0
for v,old in zip(verts,before):
 x=(old[0]-xmin)/(xmax-xmin)
 y=(old[1]-ymin)/(ymax-ymin)
 back=max(0.0,min(1.0,(y-.10)/.70))
 ridge=(13.0*math.exp(-((x-.19)/.13)**2)
       +21.0*math.exp(-((x-.54)/.17)**2)
       +16.0*math.exp(-((x-.84)/.11)**2)
       -7.0*math.exp(-((x-.36)/.085)**2))
 dz=ridge*back*back*(3-2*back)
 if abs(dz)>.001:
  v.co.z=old[2]+dz;changed+=1;max_delta=max(max_delta,abs(dz))
if changed<300 or max_delta<8:raise RuntimeError('No environment-scale intervention')
o.data.update()
dst.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',export_apply=False)
proof={'source_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'output_sha256':hashlib.sha256(dst.read_bytes()).hexdigest(),
 'target':o.name,'changed_vertices':changed,'total_target_vertices':len(verts),'max_delta_m':round(max_delta,4),
 'other_mesh_geometry_modified':False,'intervention':'distant-mountain-composition','art_approved':False}
dst.with_suffix('.proof.json').write_text(json.dumps(proof,indent=2))
print('BASTION1_DISTANCE_SILHOUETTE_CANDIDATE',json.dumps(proof))
