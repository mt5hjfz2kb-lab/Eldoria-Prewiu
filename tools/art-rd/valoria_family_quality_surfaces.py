"""Original portable geology/earth PBR workshop maps, no shader-only lookdev.
Authored asset maps are shared across the entire family and packed into GLB.
"""
from pathlib import Path
import numpy as np
from PIL import Image,ImageFilter
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art-source/valoria/production/asset-family-v2/quality-textures';OUT.mkdir(parents=True,exist_ok=True)
N=1024;rng=np.random.default_rng(331011)
def noise(radius):
 a=rng.normal(size=(N,N))
 fy=np.fft.fftfreq(N)[:,None];fx=np.fft.fftfreq(N)[None,:]
 filt=np.exp(-2*np.pi*np.pi*radius*radius*(fx*fx+fy*fy))
 b=np.fft.ifft2(np.fft.fft2(a)*filt).real.astype(np.float32)
 return np.clip((b-b.mean())/(b.std()+1e-5),-2.4,2.4)
def save(name,a):Image.fromarray(np.uint8(np.clip(a,0,255))).save(OUT/name)
fine,grain,large,patch=noise(.7),noise(3),noise(21),noise(64)
y,x=np.mgrid[:N,:N];ridge=np.sin((x+.13*y)*.045+large*.5)
mineral=4*fine+5*grain+7*large+6*patch+3*ridge
save('rock_basecolor.png',np.stack((124+mineral,119+mineral,113+mineral),axis=-1))
save('rock_roughness.png',212+5*grain+6*large)
dy,dx=np.gradient(.5*fine+grain+2*large+1.5*ridge)
save('rock_normal.png',np.stack((128-8*dx,128-8*dy,np.full((N,N),252)),axis=-1))
earth=np.stack((113+8*large+7*grain,92+6*large+5*grain,61+4*large+4*grain),axis=-1)
grass=np.stack((85+6*large+5*grain,97+7*large+5*grain,52+3*large+3*grain),axis=-1)
mix=np.clip(.56+patch*.29,0,1)[...,None]
save('soil_basecolor.png',earth*(1-mix)+grass*mix+fine[...,None]*2)
save('soil_roughness.png',225+4*grain)
dy,dx=np.gradient(fine*.35+grain*.6+large*.8)
save('soil_normal.png',np.stack((128-5*dx,128-5*dy,np.full((N,N),253)),axis=-1))
