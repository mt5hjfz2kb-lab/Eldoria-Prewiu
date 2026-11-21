"""Bake original regional pigments using only canonical heights, semantic masks and faults."""
from pathlib import Path
import gzip, json, struct
import numpy as np
from scipy import ndimage as nd
from PIL import Image
w=Path(__file__).resolve().parents[2]/"Unity/Assets/Eldoria/World4XCanon/Resources/World4X"
out=w/"Illustrated"
with gzip.open(w/'Blender/RegionOneBlenderQueries.bytes') as f:
 _,nx,nz,dx,dz=struct.unpack('<IIIff',f.read(20));h=np.frombuffer(f.read(nx*nz*4),'<f4').reshape(nz,nx);mask=np.frombuffer(f.read(nx*nz*4),'u1').reshape(nz,nx,4)/255.
zz,xx=np.mgrid[0:nz,0:nx];xx=(xx/(nx-1)-.5)*dx;zz=(zz/(nz-1)-.5)*dz
rng=np.random.default_rng(81451)
def noise(step):
 small=rng.normal(size=(nz//step+2,nx//step+2));noise=nd.map_coordinates(small,[np.arange(nz)[:,None]/step+np.zeros((nz,nx)),np.arange(nx)[None,:]/step+np.zeros((nz,nx))],order=3);return noise
macro=noise(34)*.6+noise(11)*.27+noise(4)*.13
def mix(a,c,t):return a+(np.array(c)-a)*np.clip(t,0,1)[:,:,None]
land=np.zeros((nz,nx,3))+[137,143,89];land=mix(land,[98,123,78],.5+.52*macro);land=mix(land,[69,97,69],mask[:,:,3]*.71)
land=mix(land,[170,166,123],np.clip((h-24)/80,0,1)*.84)
gz,gx=np.gradient(nd.gaussian_filter(h,1.05),4);relief=np.clip(1+.40*gz-.45*gx,.71,1.21)
land*=relief[:,:,None];land=mix(land,[169,143,103],mask[:,:,2]*.94)
wet=np.maximum(1-np.clip((mask[:,:,0]-.48)*13,0,1),mask[:,:,1])
land=mix(land,[157,151,108],nd.gaussian_filter(wet,1.3)*.65)
land=mix(land,[58,106,108],wet);land=mix(land,[42,86,98],(mask[:,:,0]<.16)*.75)
fault=np.zeros((nz,nx));cfg=json.loads((w/'Blender/production-region.json').read_text(encoding='utf-8-sig'))
for seg in cfg['faults']:
 ax,az,bx,bz,r=[seg[k] for k in ('ax','az','bx','bz','radius')];vx,vz=bx-ax,bz-az;t=np.clip(((xx-ax)*vx+(zz-az)*vz)/(vx*vx+vz*vz),0,1);d=np.hypot(xx-ax-vx*t,zz-az-vz*t)
 fault=np.maximum(fault,np.exp(-(d/(r*.85+3))**1.55))
land=mix(land,[64,62,69],fault*.94);land=mix(land,[77,53,92],np.power(fault,4)*.85)
land*=np.clip(1+.035*macro,.92,1.08)[:,:,None]
p=out/'RegionColour.jpg';Image.fromarray(np.uint8(land.clip(0,255))[::-1]).save(p,quality=94)
p=out/'SurfaceWeights.png';Image.fromarray(np.uint8(np.stack([wet,mask[:,:,2],fault,mask[:,:,3]],axis=2).clip(0,1)*255)[::-1]).save(p)
