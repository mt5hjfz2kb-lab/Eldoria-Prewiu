import bpy, json, hashlib, math
from pathlib import Path
import numpy as np

ROOT=Path.cwd()
SRC=ROOT/'art-source/valoria/lookdev/golden-slice-v1/surface-v2'
UNITY=ROOT/'Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures'
EVID=ROOT/'docs/evidence/valoria-golden-lookdev-slice-v1/surface-v2'
for p in (SRC,UNITY,EVID): p.mkdir(parents=True,exist_ok=True)

SEED=7105
rng=np.random.default_rng(SEED)

def sat(a): return np.clip(a,0.0,1.0)
def smooth(a,b,x):
    t=sat((x-a)/(b-a))
    return t*t*(3.0-2.0*t)

def fourier_field(u,v,count,lo,hi,seed):
    r=np.random.default_rng(seed)
    out=np.zeros_like(u,dtype=np.float64)
    norm=0.0
    for _ in range(count):
        ang=r.uniform(0,np.pi*2); freq=np.exp(r.uniform(np.log(lo),np.log(hi)))
        phase=r.uniform(0,np.pi*2); amp=1.0/freq**0.58
        out += np.sin((u*np.cos(ang)+v*np.sin(ang))*np.pi*2*freq+phase)*amp
        norm += amp
    return out/max(norm,1e-6)

def base_fields(size,seed):
    y,x=np.mgrid[0:size,0:size]
    u=(x+.5)/size;v=(y+.5)/size
    macro=fourier_field(u,v,12,.55,3.2,seed)
    mid=fourier_field(u,v,18,2.2,11.0,seed+19)
    micro=fourier_field(u,v,20,9.0,34.0,seed+47)
    return u,v,macro,mid,micro

def authored_family(kind,size):
    u,v,macro,mid,micro=base_fields(size,SEED+{'stone':0,'rock':101,'ground':211,'shore':307,'vegetation':401}[kind])

    if kind=='stone':
        rows=7.0;cols=6.0
        ry=v*rows;row=np.floor(ry).astype(int)
        uu=np.mod(u*cols+(row%2)*.5 + macro*.018,1.0);vv=np.mod(ry+mid*.018,1.0)
        edge=np.minimum.reduce([uu,1-uu,vv,1-vv])
        joint=1-smooth(.020,.072,edge)
        face=(1-(np.abs(uu-.5)*2)**3)*(1-(np.abs(vv-.5)*2)**3)
        mineral=sat(.5+.65*macro+.24*mid)
        lower=smooth(.46,.98,v)
        dirt=sat((mineral-.60)*1.45)*(.30+.70*lower)
        wear=smooth(.02,.10,edge)*(1-smooth(.10,.19,edge))
        height=.45+face*.095-joint*.155+mid*.030+micro*.010
        ao=sat(.985-joint*.37-dirt*.055)
        rough=sat(.70+joint*.17+dirt*.08-wear*.10+mid*.025)
        val=sat(.92+macro*.085+mid*.035-dirt*.105-joint*.22+wear*.045)
        base=np.array([.72,.63,.50]);albedo=sat(val[...,None]*base)

    elif kind=='rock':
        warp_u=u+macro*.075+mid*.018
        warp_v=v+macro*.035-mid*.025
        strata=.5+.5*np.sin(np.pi*2*(warp_u*2.35+warp_v*.67)+mid*1.7)
        ledge=smooth(.38,.66,strata)
        f1=np.abs(np.sin(np.pi*2*(warp_u*1.72-warp_v*.91)+mid*2.2+.7))
        f2=np.abs(np.sin(np.pi*2*(warp_u*.64+warp_v*2.61)+macro*1.8+2.0))
        fissure=1-smooth(.018,.115,np.minimum(f1,f2))
        chipped=sat(.5+.72*mid+.24*micro)
        height=.38+ledge*.205+mid*.060+micro*.018-fissure*.165
        ao=sat(.965-fissure*.46-(1-chipped)*.045)
        rough=sat(.86-ledge*.115+fissure*.075+mid*.030)
        warm=sat(.5+.5*macro)
        val=sat(.83+ledge*.135+mid*.065-fissure*.21)
        base=np.stack([.43+.035*warm,.43+.022*warm,.39+.010*warm],axis=2)
        albedo=sat(base*val[...,None])

    elif kind=='ground':
        patch=sat(.5+.78*macro+.20*mid)
        cx=.52+macro*.055
        path=np.exp(-((u-cx)/(.18+.025*sat(.5+mid)))**2)
        traffic=path*(.62+.38*sat(.5+.75*fourier_field(u,v,9,1.4,5.5,SEED+299)))
        clump=sat(.5+.64*mid+.23*micro)
        pebbles=np.zeros_like(u)
        pr=np.random.default_rng(SEED+912)
        for _ in range(34):
            px,py=pr.random(2);rad=pr.uniform(.008,.028);amp=pr.uniform(.25,.9)
            dx=np.minimum(np.abs(u-px),1-np.abs(u-px));dy=np.minimum(np.abs(v-py),1-np.abs(v-py))
            pebbles += np.exp(-(dx*dx+dy*dy)/(2*rad*rad))*amp
        pebbles=sat(pebbles)
        height=.43+patch*.075+clump*.035+pebbles*.055-traffic*.026
        ao=sat(.90+patch*.065-pebbles*.055-clump*.018)
        rough=sat(.90+(.5-patch)*.055-traffic*.035+clump*.018)
        val=sat(.78+patch*.18+clump*.045-traffic*.075+pebbles*.035)
        base=np.array([.47,.40,.27]);albedo=sat(val[...,None]*base)

    elif kind=='shore':
        contour=v+macro*.065+mid*.020
        wet=smooth(.18,.78,.5+.5*np.sin(np.pi*2*(contour*2.15+u*.19)))
        silt=sat(.5+.70*macro+.18*mid)
        ripple=.5+.5*np.sin(np.pi*2*(u*5.7-v*.72)+mid*1.1)
        height=.42+macro*.035+mid*.022+ripple*.018
        ao=sat(.77+silt*.15-mid*.025)
        rough=sat(.52-wet*.22+ripple*.025)
        val=sat(.66+silt*.16+mid*.045-wet*.055)
        base=np.array([.20,.23,.18]);albedo=sat(val[...,None]*base)

    else:
        canopy=sat(.5+.68*macro+.26*mid)
        vein=sat(.5+.60*mid+.18*micro)
        height=.46+canopy*.052+vein*.020
        ao=sat(.84+canopy*.105-mid*.025)
        rough=sat(.81+(.5-canopy)*.055)
        warm=sat(.5+.5*macro)
        val=sat(.76+canopy*.20+vein*.035)
        base=np.stack([.275+.030*warm,.420+.035*warm,.245-.005*warm],axis=2)
        albedo=sat(base*val[...,None])

    dx=(np.roll(height,-1,1)-np.roll(height,1,1))*8.5
    dy=(np.roll(height,-1,0)-np.roll(height,1,0))*8.5
    ln=np.sqrt(dx*dx+dy*dy+1)
    normal=np.stack((-dx/ln*.5+.5,-dy/ln*.5+.5,1/ln*.5+.5),axis=2)
    return albedo,normal,ao,1-rough

def save_png(name,arr,path):
    h,w=arr.shape[:2]
    if arr.ndim==2:
        rgb=np.repeat(arr[...,None],3,axis=2)
    else: rgb=arr
    rgba=np.concatenate([sat(rgb),np.ones((h,w,1))],axis=2).astype(np.float32)
    if name.endswith('_smoothness'):
        rgba[...,3]=sat(arr if arr.ndim==2 else arr[...,0])
        rgba[...,:3]=0.0
    img=bpy.data.images.new(name,width=w,height=h,alpha=True,float_buffer=False)
    img.pixels.foreach_set(rgba.reshape(-1))
    img.file_format='PNG';img.filepath_raw=str(path);img.save()
    bpy.data.images.remove(img)

sizes={'stone':512,'rock':512,'ground':512,'shore':256,'vegetation':256}
records=[]
for kind,size in sizes.items():
    albedo,normal,ao,smoothness=authored_family(kind,size)
    channels={'albedo':albedo,'normal':normal,'ao':ao,'smoothness':smoothness}
    for channel,arr in channels.items():
        src=SRC/f'{kind}_{channel}.png'
        save_png(f'Valoria_Golden_{kind}_{channel}',arr,src)
        unity=UNITY/src.name;unity.write_bytes(src.read_bytes())
        records.append({
            'family':kind,'channel':channel,'width':size,'height':size,
            'source':str(src.relative_to(ROOT)).replace('\\','/'),
            'unity':str(unity.relative_to(ROOT)).replace('\\','/'),
            'bytes':src.stat().st_size,
            'sha256':hashlib.sha256(src.read_bytes()).hexdigest()
        })

pixels=sum(r['width']*r['height'] for r in records)
# Expected mobile GPU budget if albedo BC1, normal BC5, AO/smoothness BC4; +33% mip chain.
gpu_est=0
for r in records:
    bpp=1.0 if r['channel']=='normal' else .5
    gpu_est += r['width']*r['height']*bpp
gpu_est=int(gpu_est*4/3)
manifest={
    'method':'Blender headless deterministic authored masks + multiscale secondary breakup; persistent PNG; no geometry authoring',
    'blender_version':bpy.app.version_string,
    'seed':SEED,
    'families':sizes,
    'textures':records,
    'texture_count':len(records),
    'source_png_bytes':sum(r['bytes'] for r in records),
    'estimated_mobile_gpu_bytes_bc_with_mips':gpu_est,
    'geometry_changed':False,
    'tripo_credits':0,
    'channels':['albedo','tangent-space normal','AO','metallic0+smoothness alpha'],
    'authoring_notes':{
        'stone':'staggered limestone courses + irregular mortar + mineral/dirt/wear masks',
        'rock':'warped directional strata + two fissure families + ridge roughness hierarchy',
        'ground':'macro earth patches + compacted route mask + clumps + deterministic pebble field',
        'shore':'wet contour/silt mask + ripple relief + lower roughness wet response',
        'vegetation':'restrained canopy value/hue breakup; no density increase'
    },
    'next':'Import in Unity Golden Slice only and judge official close/16:9/mobile views.'
}
(EVID/'texture-manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
(EVID/'README.md').write_text(
    '# Golden Surface v2 authored maps\n\n'
    'Generated headlessly in Blender from deterministic authored material masks. '
    'Multiscale procedural breakup is secondary to explicit masonry/strata/path/wetness masks. '
    'No geometry is created or modified. See texture-manifest.json.\n',
    encoding='utf-8'
)
print('GOLDEN_SURFACE_V2_MAPS_PASS',json.dumps({'textures':len(records),'png_bytes':manifest['source_png_bytes'],'gpu_est_with_mips':gpu_est,'blender':bpy.app.version_string}))
