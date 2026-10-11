"""Original Eldoria tiling micro-surfaces, portable PBR bitmaps (no shader-only detail)."""
from pathlib import Path
import numpy as np
from PIL import Image, ImageFilter
out=Path(__file__).resolve().parents[3]/'art-source/valoria/production/first-hero-gate-v1/textures'
out.mkdir(parents=True,exist_ok=True)
rng=np.random.default_rng(2319); n=1024
def noise(radius):
    a=Image.fromarray(rng.integers(0,256,(n,n),dtype=np.uint8)).filter(ImageFilter.GaussianBlur(radius))
    a=np.asarray(a,dtype=float);return (a-a.mean())/(a.std()+1e-5)
def save(name,a):Image.fromarray(np.uint8(np.clip(a,0,255))).save(out/name)
grain=noise(.6); mineral=noise(9); patches=noise(38)
stone=np.stack([133+7*grain+6*mineral+4*patches,130+6*grain+5*mineral+4*patches,122+6*grain+6*mineral+4*patches],axis=-1)
save('stone_basecolor.png',stone)
save('stone_roughness.png',205+12*grain+8*mineral)
dy,dx=np.gradient(grain*.5+mineral*.9)
save('stone_normal.png',np.stack([128-9*dx,128-9*dy,np.full((n,n),254)],axis=-1))
y,x=np.mgrid[0:n,0:n]; veins=np.sin(x*.26+np.sin(y*.002)*1.7+noise(12)*.25)+np.sin(x*.69+np.sin(y*.001)*2)*.3
wood=np.stack([69+14*veins+3*grain,43+9*veins+2*grain,25+5*veins+grain],axis=-1)
save('oak_basecolor.png',wood)
save('oak_roughness.png',193+12*veins)
dy,dx=np.gradient(veins)
save('oak_normal.png',np.stack([128-24*dx,128-10*dy,np.full((n,n),253)],axis=-1))
print('Original portable stone/oak PBR maps:',out)
