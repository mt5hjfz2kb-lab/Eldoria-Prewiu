#!/usr/bin/env python3
"""Optimise wholly NEW Valoria source GLB for mobile.
Preserves every triangle, vertex, glTF material, and UV; source .blend remains
editable and unaltered. Batches same-material nearby meshes by site.
Requires trimesh and numpy. Verified on Blender V6 GLB (2,129 -> 144 meshes).
This does NOT establish physical iPhone or Unity performance approval.
"""
from collections import defaultdict
from pathlib import Path
import json
import struct
import sys
import time
import numpy as np
import trimesh

def zone_of(x,z):
    # glTF: Blender y becomes -glTF z; x stays x.
    places=[('BastionV',-2,-8,25),('Aserradero',-30,10,17),
            ('Cuartel',28,-10,17),('Granero',-30,-19,17),
            ('Cantera',28,15,17)]
    near=[((x-cx)**2+(z-cz)**2,n) for n,cx,cz,r in places
          if (x-cx)**2+(z-cz)**2<r*r]
    return min(near)[1] if near else 'Paisaje'

def execute(source,dest):
    started=time.monotonic()
    scene=trimesh.load(str(source),force='scene')
    if not len(scene.geometry)>500:
        raise RuntimeError('Unexpected source scene, refusing batch')
    vertices=sum(len(g.vertices) for g in scene.geometry.values())
    faces=sum(len(g.faces) for g in scene.geometry.values())
    buckets=defaultdict(list)
    for node in scene.graph.nodes_geometry:
        transform,name=scene.graph.get(node)
        geo=scene.geometry[name]
        if not len(geo.faces): continue
        material=getattr(geo.visual,'material',None)
        material_name=getattr(material,'name',None) or 'UNMATERIALIZED'
        # Mesh-authored geometry (terrain, enclosed roofs, stone arches) often
        # has IDENTITY node transform and world-space vertices. Transform-only
        # placement would incorrectly group those roof faces with Bastion V,
        # breaking per-building visibility/selection. Use true WORLD centroid.
        part=geo.copy()
        part.apply_transform(transform)
        world_center=(part.bounds[0]+part.bounds[1])*.5
        x,z=float(world_center[0]),float(world_center[2])
        if len(part.faces)>10000 or 'ONE unified sculpted terrain' in str(node):
            zone='TerrenoUnificado'
        elif 'river' in str(node).lower() or 'bridge' in str(node).lower():
            zone='Infraestructura'
        else:zone=zone_of(x,z)
        buckets[(zone,material_name)].append(part)
    out=trimesh.Scene()
    for (zone,material),meshes in sorted(buckets.items()):
        combined=trimesh.util.concatenate(meshes) if len(meshes)>1 else meshes[0]
        out.add_geometry(combined,geom_name=zone+'__'+material,
                         node_name=zone+'__'+material,transform=np.eye(4))
    dest.parent.mkdir(parents=True,exist_ok=True)
    # trimesh defaults to omitting glTF NORMAL attributes. On original roof
    # gables and bevelled masonry that silently destroys authored shading.
    # Force emission of the original transformed, concatenated normal streams.
    dest.write_bytes(out.export(file_type='glb',include_normals=True))
    result=trimesh.load(str(dest),force='scene')
    vertices_after=sum(len(g.vertices) for g in result.geometry.values())
    faces_after=sum(len(g.faces) for g in result.geometry.values())
    assert vertices_after==vertices
    raw=dest.read_bytes()
    json_len=struct.unpack('<I',raw[12:16])[0]
    tree=json.loads(raw[20:20+json_len])
    primitives=[p for m in tree.get('meshes',[]) for p in m.get('primitives',[])]
    assert primitives and all('NORMAL' in p.get('attributes',{}) for p in primitives), 'GLB missing authored normals'
    assert faces_after==faces
    assert len(result.geometry)<len(scene.geometry)*.20
    old_materials={getattr(g.visual.material,'name','') for g in scene.geometry.values()}
    new_materials={getattr(g.visual.material,'name','') for g in result.geometry.values()}
    assert old_materials==new_materials
    assert np.allclose(scene.bounds,result.bounds,atol=.0005,rtol=0), 'world geometry drift'
    region_names={key[0] for key in buckets}
    assert all(x in region_names for x in ('BastionV','Aserradero','Cuartel','Granero','Cantera'))
    assert all(g.visual.kind=='texture' and len(g.visual.uv)==len(g.vertices)
               for g in result.geometry.values())
    manifest={'source_meshes':len(scene.geometry),'derived_meshes':len(result.geometry),
              'source_vertices':vertices,'derived_vertices':vertices_after,
              'source_faces':faces,'derived_faces':faces_after,
              'materials':len(new_materials),
              'spatial_region_detection':'evaluated_world_bounds_not_node_origin',
              'isolated_regions':sorted(region_names),
              'world_bounds_preserved':True,
              'gltf_normal_attributes_preserved':True,
              'source_bytes':source.stat().st_size,
              'derived_bytes':dest.stat().st_size,
              'seconds':round(time.monotonic()-started,2),
              'original_blend_retained':True,
              'unity_render':False,'iphone_physical_tested':False}
    print('GLB_MOBILE_STATIC_BATCH_VALID',json.dumps(manifest))
    dest.with_suffix('.json').write_text(json.dumps(manifest,indent=2))
    return manifest

if __name__=='__main__':
    execute(Path(sys.argv[1]),Path(sys.argv[2]))
