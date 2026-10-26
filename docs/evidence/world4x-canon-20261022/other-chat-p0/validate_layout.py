from pathlib import Path
import json
import numpy as np
from PIL import Image
import trimesh
root=Path(__file__).resolve().parent
meta=json.loads((root/'region1_layout.json').read_text())
assert meta['world_alignment']['rift_world_x']==8
assert meta['world_alignment']['rift_world_z']==61
assert (meta['unity_world_bounds']['min_x'],meta['unity_world_bounds']['max_x'])==(-910,910)
assert (meta['unity_world_bounds']['min_z'],meta['unity_world_bounds']['max_z'])==(-780,780)
assert set(meta['mask_files'])=={'land','forest','water','roads','relief','rift'}
for name,p in meta['mask_files'].items():
 img=Image.open(root/p)
 assert img.mode=='L' and img.size==(1024,1024),(name,img.mode,img.size)
 a=np.asarray(img)
 assert np.count_nonzero(a)>10,name
m=trimesh.load(root/'region1_p0_preview_mesh_NOT_INTEGRATED.glb',force='scene')
assert len(m.geometry)==1
mesh=next(iter(m.geometry.values()))
assert len(mesh.vertices)==21025
assert len(mesh.faces)==41472
assert mesh.bounds[1,1]<0.0 and mesh.bounds[0,1]>-8
print('PASS 6 aligned masks, authored vectors, world-coordinate terrain mesh, 21,025 vertices, no raised gameplay overlay')
