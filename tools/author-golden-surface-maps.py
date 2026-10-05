import bpy, math, json, hashlib
from pathlib import Path

ROOT=Path.cwd()
SRC=ROOT/'art-source/valoria/lookdev/golden-slice-v1/surface-v2'
UNITY=ROOT/'Unity/Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures'
EVID=ROOT/'docs/evidence/valoria-golden-lookdev-slice-v1/surface-v2'
for p in (SRC,UNITY,EVID): p.mkdir(parents=True,exist_ok=True)

def clamp(v,a=0.0,b=1.0): return max(a,min(b,v))
def fract(v): return v-math.floor(v)
def smoothstep(a,b,x):
    if a==b:return 0.0
    t=clamp((x-a)/(b-a));return t*t*(3-2*t)

def fields(kind,u,v):
    m=(math.sin(2*math.pi*(u*2.0+v*.31))+math.cos(2*math.pi*(v*1.7-u*.18))+math.sin(2*math.pi*(u+v)*.73+.8))/3
    m2=math.sin(2*math.pi*(u*5.1-v*3.8)+1.2)*math.cos(2*math.pi*(v*4.2+u*.7)-.5)
    if kind=='stone':
        rows=7.0; cols=6.0; ry=v*rows; row=int(math.floor(ry))
        uu=fract(u*cols+(row%2)*.5); vv=fract(ry)
        edge=min(uu,1-uu,vv,1-vv)
        joint=1-smoothstep(.018,.065,edge)
        face=(1-(abs(uu-.5)*2)**3)*(1-(abs(vv-.5)*2)**3)
        stain=smoothstep(.56,.95,clamp(.5+.5*(m+.35*m2)))
        h=.46+face*.085-joint*.14+m2*.025
        ao=1-joint*.34-stain*.07
        rough=.68+joint*.17+stain*.06-math.pow(max(face,0),2)*.045
        base=(.71,.62,.49); val=.92+m*.055-stain*.085-joint*.20
    elif kind=='rock':
        strata=.5+.5*math.sin(2*math.pi*(u*2.6+v*.74)+math.sin(v*10)*.35)
        fracture=min(abs(math.sin(2*math.pi*(u*2.15-v*.82)+.5)),abs(math.sin(2*math.pi*(u*.73+v*3.4)+1.7)))
        crack=1-smoothstep(.0,.11,fracture)
        ridge=smoothstep(.40,.63,strata)
        h=.40+ridge*.19+m2*.055-crack*.16
        ao=.96-crack*.43
        rough=.84-ridge*.12+crack*.07
        base=(.47,.45,.40); val=.86+ridge*.13+m*.05-crack*.20
    elif kind=='ground':
        patch=.5+.5*m
        path=math.exp(-((u-.52)/.22)**2)*(.68+.32*math.sin(v*math.pi*10))
        grit=.5+.5*math.sin(2*math.pi*(u*11.0+v*7.0)+math.sin(u*31)*.45)
        h=.46+patch*.07+grit*.035-path*.025
        ao=.88+patch*.08-grit*.025
        rough=.88+(.5-patch)*.05+path*.02
        base=(.47,.40,.27); val=.80+patch*.16-path*.08+grit*.035
    elif kind=='shore':
        band=.5+.5*math.sin(2*math.pi*(v*4.7+u*.35))
        ripple=.5+.5*math.sin(2*math.pi*(u*7.3-v*.9)+.6)
        silt=smoothstep(.35,.72,band)
        h=.45+band*.065+ripple*.025
        ao=.74+silt*.18
        rough=.46-silt*.14+ripple*.03
        base=(.18,.22,.17); val=.72+silt*.18+ripple*.05
    else: # vegetation
        vein=1-smoothstep(.0,.10,abs(fract(u*2)-.5))
        canopy=.5+.5*math.sin(2*math.pi*(v*3.7+u*.55)+.2)
        h=.47+vein*.07+canopy*.035
        ao=.83+canopy*.12
        rough=.79+(.5-canopy)*.05
        base=(.28,.43,.25); val=.78+canopy*.18+vein*.04
    c=tuple(clamp(ch*val) for ch in base)
    return c,clamp(h),clamp(ao),clamp(rough)

def write_map(kind,channel,size):
    pixels=[0.0]*(size*size*4)
    heights=[0.0]*(size*size)
    cached=[]
    for y in range(size):
        v=(y+.5)/size
        for x in range(size):
            u=(x+.5)/size
            c,h,ao,rough=fields(kind,u,v);idx=y*size+x
            heights[idx]=h;cached.append((c,ao,rough))
    for y in range(size):
        for x in range(size):
            idx=y*size+x
            c,ao,rough=cached[idx]
            if channel=='albedo':
                rgba=(*c,1.0)
            elif channel=='ao':
                rgba=(ao,ao,ao,1.0)
            elif channel=='smoothness':
                s=1.0-rough;rgba=(s,s,s,s)
            else:
                xl=(x-1)%size;xr=(x+1)%size;yd=(y-1)%size;yu=(y+1)%size
                dx=(heights[y*size+xr]-heights[y*size+xl])*9.0
                dy=(heights[yu*size+x]-heights[yd*size+x])*9.0
                nz=1.0;ln=math.sqrt(dx*dx+dy*dy+nz*nz)
                rgba=(-dx/ln*.5+.5,-dy/ln*.5+.5,nz/ln*.5+.5,1.0)
            o=idx*4;pixels[o:o+4]=rgba
    img=bpy.data.images.new(f'Valoria_Golden_{kind}_{channel}',width=size,height=size,alpha=True,float_buffer=False)
    img.pixels.foreach_set(pixels)
    img.file_format='PNG'
    out_src=SRC/f'{kind}_{channel}.png'
    img.filepath_raw=str(out_src);img.save()
    out_unity=UNITY/out_src.name
    out_unity.write_bytes(out_src.read_bytes())
    bpy.data.images.remove(img)
    return out_src,out_unity

sizes={'stone':512,'rock':512,'ground':512,'shore':256,'vegetation':256}
records=[]
for kind,size in sizes.items():
    for channel in ('albedo','normal','ao','smoothness'):
        s,u=write_map(kind,channel,size)
        records.append({
          'family':kind,'channel':channel,'width':size,'height':size,
          'source':str(s.relative_to(ROOT)).replace('\\','/'),
          'unity':str(u.relative_to(ROOT)).replace('\\','/'),
          'bytes':s.stat().st_size,
          'sha256':hashlib.sha256(s.read_bytes()).hexdigest()
        })
manifest={
 'method':'Blender 4.x headless procedural authored masks; persistent PNG; no geometry authoring',
 'blender_version':bpy.app.version_string,
 'families':sizes,
 'textures':records,
 'texture_count':len(records),
 'source_png_bytes':sum(r['bytes'] for r in records),
 'tripo_credits':0,
 'geometry_changed':False,
 'channels':['albedo','tangent-space normal','AO','smoothness'],
 'next':'Import as Golden Surface v2 in Unity and evaluate official close/16:9/mobile captures.'
}
(EVID/'texture-manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
(EVID/'README.md').write_text('# Golden Surface v2 authored maps\n\nGenerated headlessly in Blender from deterministic authored material masks. No geometry is created or modified. See texture-manifest.json.\n',encoding='utf-8')
print('GOLDEN_SURFACE_V2_MAPS_PASS',json.dumps({'textures':len(records),'png_bytes':manifest['source_png_bytes'],'blender':bpy.app.version_string}))
