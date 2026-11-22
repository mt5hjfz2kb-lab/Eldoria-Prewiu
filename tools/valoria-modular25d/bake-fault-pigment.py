"""Warp the original fault painting onto the certified Region 1 fault arms."""
import json
from pathlib import Path
import numpy as np
from PIL import Image
from scipy.ndimage import map_coordinates

root = Path(__file__).resolve().parents[2] / "Unity/Assets/Eldoria/World4XCanon/Resources/World4X"
art = root / "Illustrated"
faults = json.loads((root / "Blender/production-region.json").read_text())["faults"]
texture = np.asarray(Image.open(art / "Fault.png").convert("RGBA"), dtype=float) / 255
# x[-416,-96], z[0,288]; these boundaries correspond to RegionPaint.shader.
density = 4
atlas = np.zeros((288*density, 320*density, 4), dtype=float)

def raster_triangle(vertices, texcoords, layer):
    a,b,c = vertices
    lo = np.maximum(0, np.floor(vertices.min(axis=0)).astype(int))
    hi = np.minimum([layer.shape[1],layer.shape[0]], np.ceil(vertices.max(axis=0)).astype(int)+1)
    if np.any(hi<=lo): return
    y,x = np.mgrid[lo[1]:hi[1],lo[0]:hi[0]]
    x,y = x+.5-a[0], y+.5-a[1]
    d,e = b-a,c-a
    det = d[0]*e[1]-d[1]*e[0]
    if abs(det)<.001: return
    u,v = (x*e[1]-y*e[0])/det, (d[0]*y-d[1]*x)/det
    inside = (u>=-1e-6)&(v>=-1e-6)&(u+v<=1.000001)
    uv = texcoords[0]+u[...,None]*(texcoords[1]-texcoords[0])+v[...,None]*(texcoords[2]-texcoords[0])
    coords = [(1-uv[...,1])*(texture.shape[0]-1),uv[...,0]*(texture.shape[1]-1)]
    tile = layer[lo[1]:hi[1],lo[0]:hi[0]]
    for channel in range(4):
        samples = map_coordinates(texture[...,channel], coords, order=1, mode="nearest")
        tile[...,channel][inside] = samples[inside]

arms=[]
for seg in faults:
    start,end=(seg["ax"],seg["az"]),(seg["bx"],seg["bz"])
    if not arms or arms[-1][-1]!=start: arms.append([start])
    arms[-1].append(end)
# Draw tributaries first, then the continuous dominant lip through each junction.
for arm_id in reversed(range(len(arms))):
    points=np.array(arms[arm_id],dtype=float)
    steps=np.diff(points,axis=0)
    lengths=np.linalg.norm(steps,axis=1)
    tangents=steps/lengths[:,None]
    joins=np.vstack((tangents[0],tangents[:-1]+tangents[1:],tangents[-1]))
    joins/=np.linalg.norm(joins,axis=1)[:,None]
    normals=np.column_stack((-joins[:,1],joins[:,0]))
    width=faults[0]["radius"]*(2.4 if arm_id==0 else 1.35)
    strip=np.stack((points-normals*width/2,points+normals*width/2),axis=1)
    strip[...,0]=(strip[...,0]+416)*density
    strip[...,1]=(288-strip[...,1])*density
    along=np.r_[0,np.cumsum(lengths)]/lengths.sum()
    layer=np.zeros_like(atlas)
    for i in range(len(steps)):
        quad=np.array([strip[i,0],strip[i,1],strip[i+1,1],strip[i+1,0]])
        uv=np.array([[along[i],0],[along[i],1],[along[i+1],1],[along[i+1],0]])
        for ids in ([0,1,2],[0,2,3]): raster_triangle(quad[ids],uv[ids],layer)
    alpha=layer[...,3:4]
    atlas[...,:3]=layer[...,:3]*alpha+atlas[...,:3]*(1-alpha)
    atlas[...,3:4]=alpha+atlas[...,3:4]*(1-alpha)
atlas[...,:3]/=np.maximum(atlas[...,3:4],1e-9)
Image.fromarray(np.uint8(np.clip(atlas,0,1)*255+.5)).quantize(256,method=Image.Quantize.FASTOCTREE).save(art/"FaultPigment.png",optimize=True)
