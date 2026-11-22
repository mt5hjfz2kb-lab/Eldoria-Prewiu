"""Pack the authored lower mip chain of legacy regional model textures for WebGL."""
import gzip
import hashlib
import json
import re
import struct
import sys
import tempfile
from pathlib import Path
import UnityPy

LEGACY = re.compile(r"(?:authored-texture|texture|tex)-\d+")
SCENE = "sharedassets1.assets"
SIG = b"UnityWebData1.0\0"


def read_entries(stream):
    assert stream.read(len(SIG)) == SIG
    end, = struct.unpack('<I',stream.read(4))
    items = []
    while stream.tell() < end:
        start, size, length = struct.unpack('<III',stream.read(12))
        items.append((stream.read(length),start,size))
    return items


def pack_scene(file, log):
    game = UnityPy.load(str(file))
    bundle = next(iter(game.files.values()))
    region = bundle.files[SCENE]
    before = {}
    for key, asset in list(bundle.files.items()):
        if key == SCENE:
            continue
        reader = asset.reader
        reader.flags = asset.flags
        before[key] = hashlib.sha256(reader.bytes).hexdigest()
        bundle.files[key] = reader
    trimmed = {}
    for obj in region.objects.values():
        if obj.type.name != 'Texture2D':
            continue
        texture = obj.read()
        if not LEGACY.fullmatch(texture.m_Name) or max(texture.m_Width,texture.m_Height)<=256:
            continue
        channels = {3:3,4:4,5:4}.get(texture.m_TextureFormat)
        assert channels and not texture.m_StreamData.size and texture.m_MipCount>1
        width,height = texture.m_Width,texture.m_Height
        offset = 0
        count = texture.m_MipCount
        while max(width,height)>256:
            offset += width*height*channels
            width,height=max(1,width//2),max(1,height//2)
            count-=1
        remaining = texture.image_data[offset:]
        expected = sum(max(1,width>>i)*max(1,height>>i)*channels for i in range(count))
        assert remaining and len(remaining)==expected
        texture.m_Width,texture.m_Height,texture.m_MipCount=width,height,count
        texture.m_CompleteImageSize = len(remaining)
        texture.image_data = remaining
        texture.save()
        trimmed[str(obj.path_id)] = {"name":texture.m_Name,"bytes_removed":offset,
            "mip_sha256":hashlib.sha256(remaining).hexdigest()}
    assert len(trimmed)>=6
    # Compare every untouched serialized object, including routes, meshes and logic.
    other_objects={str(o.path_id):hashlib.sha256(o.get_raw_data()).hexdigest()
                   for o in region.objects.values() if str(o.path_id) not in trimmed}
    log.write_text(json.dumps({"files":before,"objects":other_objects,"textures":trimmed},indent=2))
    file.write_bytes(bundle.save(packer="original"))


def verify(file,log):
    expected=json.loads(log.read_text())
    bundle=next(iter(UnityPy.load(str(file)).files.values()))
    for name,digest in expected["files"].items():
        assert hashlib.sha256(bundle.files[name].reader.bytes).hexdigest()==digest,name
    objects=bundle.files[SCENE].objects
    assert len(objects)==len(expected["textures"])+len(expected["objects"])
    for idx,record in expected["textures"].items():
        tex=objects[int(idx)].read()
        assert max(tex.m_Width,tex.m_Height)<=256 and tex.m_Name==record["name"]
        assert hashlib.sha256(tex.image_data).hexdigest()==record["mip_sha256"]
    for idx,digest in expected["objects"].items():
        assert hashlib.sha256(objects[int(idx)].get_raw_data()).hexdigest()==digest,idx


def main():
    path=Path(sys.argv[1])
    with tempfile.TemporaryDirectory() as tmp:
        folder=Path(tmp)
        bundle=folder/'data.unity3d'
        proof=folder/'proof.json'
        with gzip.open(path,'rb') as f:
            entries=read_entries(f)
            remaining={}
            for name,start,length in entries:
                assert f.tell()==start
                data=f.read(length)
                assert len(data)==length
                if name==b'data.unity3d':
                    bundle.write_bytes(data)
                else:
                    remaining[name]=data
                del data
        pack_scene(bundle,proof)
        verify(bundle,proof)
        stats=json.loads(proof.read_text())['textures']
        print('Retained authored mips for',len(stats),'embedded textures; bytes removed:',sum(v['bytes_removed'] for v in stats.values()),flush=True)
        end=len(SIG)+4+sum(12+len(name) for name,_,_ in entries)
        temp=path.with_suffix('.mobile-tmp')
        with gzip.open(temp,'wb',compresslevel=3) as out:
            out.write(SIG+struct.pack('<I',end))
            pos=end
            for name,_,_ in entries:
                n=bundle.stat().st_size if name==b'data.unity3d' else len(remaining[name])
                out.write(struct.pack('<III',pos,n,len(name))+name)
                pos+=n
            for name,_,_ in entries:
                if name==b'data.unity3d':
                    import shutil
                    with bundle.open('rb') as src:shutil.copyfileobj(src,out)
                else:
                    out.write(remaining[name])
        temp.replace(path)


if __name__=="__main__":
    main()
