"""Remove old illustrations from the build-copy GLBs that now supply only geometry."""
import hashlib
import json
from pathlib import Path
import struct
import sys
import zlib

def chunk(tag, data):
    return struct.pack('>I',len(data))+tag+data+struct.pack('>I',zlib.crc32(tag+data)&0xffffffff)

neutral=(b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>2I5B',1,1,8,6,0,0,0))+
         chunk(b'IDAT',zlib.compress(b'\x00\x80\x80\x80\xff'))+chunk(b'IEND',b''))

def write_glb(path):
    data=path.read_bytes()
    magic,version,size=struct.unpack_from('<4sII',data)
    assert magic==b'glTF' and version==2 and size==len(data)
    json_size,tag=struct.unpack_from('<I4s',data,12)
    assert tag==b'JSON'
    model=json.loads(data[20:20+json_size])
    bin_size,tag=struct.unpack_from('<I4s',data,20+json_size)
    assert tag==b'BIN\x00' and len(model['buffers'])==1
    binary=memoryview(data)[28+json_size:28+json_size+bin_size]
    views=model.get('bufferViews',[])
    images={image['bufferView'] for image in model.get('images',[]) if 'bufferView' in image}
    if not images:return
    assert all(a.get('bufferView') not in images for a in model.get('accessors',[]))
    encoded=bytearray()
    for i,view in enumerate(views):
        assert view['buffer']==0 and 'EXT_meshopt_compression' not in view.get('extensions',{})
        off=view.get('byteOffset',0);old=binary[off:off+view['byteLength']]
        assert len(old)==view['byteLength']
        new=neutral if i in images else old
        view['byteOffset']=len(encoded);view['byteLength']=len(new)
        encoded.extend(new)
        if i not in images:
            assert hashlib.sha256(old).digest()==hashlib.sha256(memoryview(encoded)[view['byteOffset']:]).digest()
        encoded.extend(b'\x00'*(-len(encoded)%4))
    for image in model.get('images',[]):
        if 'bufferView' in image:image['mimeType']='image/png'
    model['buffers'][0]['byteLength']=len(encoded)
    metadata=json.dumps(model,separators=(',',':'),ensure_ascii=False).encode()
    metadata+=b' '*(-len(metadata)%4)
    output=path.with_suffix('.packing')
    with output.open('wb') as f:
        f.write(struct.pack('<4sIIII',b'glTF',2,28+len(metadata)+len(encoded),len(metadata),0x4e4f534a))
        f.write(metadata);f.write(struct.pack('<I4s',len(encoded),b'BIN\0'));f.write(encoded)
    output.replace(path)
    print(path.name,len(images),'archived image slots;',len(views)-len(images),'identical geometry/animation buffer views')
root=Path(sys.argv[1])/'Assets/Eldoria/World4XCanon/Resources/World4X'
for stem in ['Blender/RegionOneBlenderWorld','Blender/RegionOneResources','Creatures/RockBoarNear','Creatures/RockBoarFar','Creatures/SpawnlingNear','Creatures/SpawnlingFar']:
    write_glb(root/(stem+'.glb'))
