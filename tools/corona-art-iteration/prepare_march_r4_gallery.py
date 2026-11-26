import hashlib,json,sys
from pathlib import Path
from PIL import Image,ImageFilter
import numpy as np
src,dest=map(Path,sys.argv[1:3]);dest.mkdir(parents=True,exist_ok=True)
raw=src.read_bytes(); assert hashlib.sha256(raw).hexdigest()=='f9a5ea1ea6489baffca413dc03db9dc6ceeca5a937f8c08ed3387be2d610a5e8'
im=Image.open(src).convert('RGB'); assert im.size==(1536,1024)
# Preserve the table surface, suppress the competing central flag/right fire,
# and soften distant detail. Original bytes remain in the studio artifact.
im=im.crop((96,0,1440,860));w,h=im.size
x,y=np.meshgrid(np.linspace(0,1,w),np.linspace(0,1,h))
arr=np.asarray(im,dtype=np.float32)/255
soft=np.asarray(im.filter(ImageFilter.GaussianBlur(6.0)),dtype=np.float32)/255
shelf=1/(1+np.exp(-(y-.855)*65))
blur=(.74*(1-shelf)+.12*shelf)[...,None]
a=arr*(1-blur)+soft*blur
luma=np.sum(a*np.array([.2126,.7152,.0722],dtype=np.float32),axis=-1,keepdims=True)
a=luma+(a-luma)*(.77+.18*shelf[...,None])
center=np.exp(-((x-.50)/.31)**2-((y-.37)/.43)**2)
fire=np.exp(-((x-.84)/.16)**2-((y-.42)/.22)**2)
light=(.87-.23*center-.15*fire)*(1-shelf)+.94*shelf
a=a*light[...,None];a=(a+.015)*(1-.08*(1-shelf[...,None])*a)
Image.fromarray(np.uint8(np.clip(a,0,1)*255+.5),'RGB').save(dest/'march_command_gallery.png',optimize=True)
x,y=np.meshgrid(np.linspace(-1,1,160),np.linspace(-1,1,64))
alpha=np.exp(-4.8*x*x-6.8*y*y)*np.clip((1-x*x)*(1-y*y),0,1)*.72
shadow=np.zeros((64,160,4),dtype=np.uint8);shadow[:,:,3]=np.uint8(alpha*255+.5)
Image.fromarray(shadow,'RGBA').save(dest/'march_gallery_contact.png',optimize=True)
manifest={'source_sha256':hashlib.sha256(raw).hexdigest(),'source_review':'reject: excessive ornament, right light; transform to quieter asset candidate','transformation':'crop [96,0,1440,860]; spatial soft-focus 6px; desaturate and suppress banner and right light; preserve ledge; separate elliptical alpha contact shadow','not_art_approved':True,'assets':{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in dest.glob('*.png')}}
(dest/'manifest.json').write_text(json.dumps(manifest,indent=2))
print(json.dumps(manifest))
