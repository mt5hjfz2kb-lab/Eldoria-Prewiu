import numpy as np,cv2
from PIL import Image
from pathlib import Path
import trimesh
p=Path('/mnt/data/eldoria_p0_art_directed')
res=145
x=np.linspace(-910,910,res,dtype=np.float32)
z=np.linspace(-780,780,res,dtype=np.float32)
xx,zz=np.meshgrid(x,z)
# All heights remain under the actual zero-height resources/POIs.
relief=np.array(Image.open(p/'masks/relief.png').convert('L'))/255.
river=np.array(Image.open(p/'masks/water.png').convert('L'))/255.
relief=cv2.resize(relief.astype('float32'),(res,res),interpolation=cv2.INTER_LINEAR)
water=cv2.resize(river.astype('float32'),(res,res),interpolation=cv2.INTER_LINEAR)
relief=np.flipud(relief);water=np.flipud(water)
center=np.sqrt(xx*xx+zz*zz)
protected=np.clip((center-95)/130,0,1)
# The mesh is a safe non-gameplay PRESENTATION candidate. No colliders.
height=-5.6+relief*6.4*protected -water*1.8*protected
vertices=np.stack([xx.flatten(),height.flatten(),zz.flatten()],axis=-1).astype('float32')
tris=[]
for j in range(res-1):
 for i in range(res-1):
  a=j*res+i
  tris.extend([(a,a+res,a+1),(a+1,a+res,a+res+1)])
uv=np.stack([(xx.flatten()+910)/1820,(zz.flatten()+780)/1560],axis=-1)
mat=trimesh.visual.material.SimpleMaterial(image=Image.open(p/'geographic_layout_preview_NOT_UNITY.png').convert('RGBA'))
mesh=trimesh.Trimesh(vertices=vertices,faces=np.array(tris,dtype=np.int32),process=False,visual=trimesh.visual.TextureVisuals(uv=uv,material=mat))
mesh.metadata={'purpose':'P0 art-direction editor candidate (no gameplay colliders)','unity_coordinates':'X,Y,Z','not_actual_unity_render':True}
path=p/'region1_p0_preview_mesh_NOT_INTEGRATED.glb'
mesh.export(path)
print(path,path.stat().st_size,len(vertices),len(tris),mesh.bounds.tolist())
