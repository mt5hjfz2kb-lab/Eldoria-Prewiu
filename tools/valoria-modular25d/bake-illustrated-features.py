"""Recover geographic features for original art. Export coordinates and class masks only."""
import io,json,struct,sys
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw
from scipy import ndimage as nd
root=Path(__file__).resolve().parents[2]/'Unity/Assets/Eldoria/World4XCanon/Resources/World4X'
out=root/'Illustrated'
blob=Path(sys.argv[1] if len(sys.argv)>1 else root/'Blender/RegionOneBlenderWorld.glb').read_bytes()
length=struct.unpack_from('<I',blob,12)[0];g=json.loads(blob[20:20+length]);binary=memoryview(blob)[28+length:]
nodes={n['name']:n for n in g['nodes']}
def acc(idx):
    a=g['accessors'][idx];v=g['bufferViews'][a['bufferView']]
    dt=np.dtype({5126:'<f4',5125:'<u4',5123:'<u2',5121:'u1'}[a['componentType']])
    k={'VEC3':3,'VEC2':2,'SCALAR':1}[a['type']]
    return np.ndarray((a['count'],k),dtype=dt,buffer=binary,offset=v.get('byteOffset',0)+a.get('byteOffset',0),strides=(v.get('byteStride',k*dt.itemsize),dt.itemsize))
def water(extent,size):
    xmin,zmin,xmax,zmax=extent;w,h=size
    im=Image.new('L',size);draw=ImageDraw.Draw(im)
    for name,node in nodes.items():
        if not (name=='Pilot_River' or name.startswith(('BLD_AuthoredTributary_','BLD_EasternSea','BLD_RiverExtension_'))):continue
        assert 'translation' not in node and 'rotation' not in node
        for primitive in g['meshes'][node['mesh']]['primitives']:
            v=acc(primitive['attributes']['POSITION'])
            xy=np.column_stack(((v[:,0]-xmin)*w/(xmax-xmin),(zmax+v[:,2])*h/(zmax-zmin)))
            for tri in acc(primitive['indices']).reshape(-1,3):draw.polygon([tuple(p) for p in xy[tri]],fill=255)
    return np.array(im)
def road_class(primitive,X,Z):
    pos=acc(primitive['attributes']['POSITION']);uv=acc(primitive['attributes']['TEXCOORD_0'])
    Xform=np.stack([pos[:,0],-pos[:,2],np.ones(len(pos))],1)
    co=np.linalg.lstsq(Xform,uv,rcond=None)[0]
    m=g['materials'][primitive['material']]['pbrMetallicRoughness']['baseColorTexture']['index']
    source=g['images'][g['textures'][m]['source']]
    view=g['bufferViews'][source['bufferView']];off=view.get('byteOffset',0)
    tex=np.asarray(Image.open(io.BytesIO(binary[off:off+view['byteLength']])).convert('RGB')).astype('f')
    classifier=np.clip((tex[:,:,0]/(tex[:,:,1]+1)-1.11)/.12,0,1)
    u=co[0,0]*X+co[1,0]*Z+co[2,0];v=co[0,1]*X+co[1,1]*Z+co[2,1]
    result=nd.map_coordinates(classifier,[v*(tex.shape[0]-1),u*(tex.shape[1]-1)],order=1,mode='nearest')
    return result*((X>=pos[:,0].min())&(X<=pos[:,0].max())&(Z>=-pos[:,2].max())&(Z<=-pos[:,2].min()))
extent=(-200,-148,216,364);size=(832,1024)
Z,X=np.mgrid[0:size[1],0:size[0]];X=extent[0]+(X+.5)*.5;Z=extent[3]-(Z+.5)*.5
wet=water(extent,size)/255.
road=np.zeros(wet.shape)
for p in g['meshes'][nodes['BLD_Terrain']['mesh']]['primitives'][1:4]:
    road=np.maximum(road,road_class(p,X,Z))
road=nd.maximum_filter(road,size=3)
bank=np.exp(-nd.distance_transform_edt(wet==0)/2.8)*(1-wet)
signed=(nd.distance_transform_edt(wet>0)-nd.distance_transform_edt(wet==0))*.5
shore=np.clip(.5+nd.gaussian_filter(signed,.95)/12,0,1)
Image.fromarray(np.dstack((np.uint8(np.stack([wet,road,bank],axis=2).clip(0,1)*255),np.uint8(shore*255+.5)))).save(out/'NearFeatures.png')
# Collision ford gaps stay traversable, while the illustrated river now passes beneath them.
p=out/'SurfaceWeights.png';geo=np.array(Image.open(p).convert('RGBA'));geo[:,:,0]=np.maximum(geo[:,:,0],water((-910,-1800,910,1800),(geo.shape[1],geo.shape[0])));Image.fromarray(geo).save(p)
trees=[]
for name,n in sorted(nodes.items()):
    if not name.startswith(('Pilot_Pine','BLD_ForestMass_')):continue
    x,y,z=n['translation'];scale=n.get('scale',[1,1,1]);trees.append((x,-z,max(scale[0],scale[2])))
with (out/'ForestPositions.bytes').open('wb') as f:
    f.write(struct.pack('<I',len(trees)))
    for tree in trees:f.write(struct.pack('<3f',*tree))

