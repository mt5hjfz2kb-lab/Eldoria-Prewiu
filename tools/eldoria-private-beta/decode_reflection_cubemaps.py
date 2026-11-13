"""Controlled compatibility candidate: decode five DXT5 reflection cubes only.
No resizing, mip generation, material edits, shader edits or gameplay changes.
Every authored face/mip is decoded separately; all other objects must be byte-identical.
"""
import gc, hashlib, json, sys
from pathlib import Path
import UnityPy
from UnityPy.export.Texture2DConverter import parse_image_data

def sha(b): return hashlib.sha256(b).hexdigest()
def key(o): return o.assets_file.name+':'+str(o.path_id)
def run(source,destination):
    src=Path(source);out=Path(destination);out.mkdir(parents=True,exist_ok=True)
    expected='2c502a1d766aca83c2377347de290467d16d75efd8c6f5edad41766f4d3ce73a'
    assert sha(src.read_bytes())==expected, 'Requires exact mobile5 parent'
    env=UnityPy.load(str(src));original={key(o):sha(o.get_raw_data()) for o in env.objects}
    changed={};metadata={};rows=[];decoded_checks={}
    allowed={'m_TextureFormat','m_CompleteImageSize','image data'}
    for o in env.objects:
        if o.type.name!='Cubemap': continue
        t=o.read()
        assert t.m_Name in {'ReflectionProbe-'+str(i) for i in range(5)}
        assert t.m_Width==128 and t.m_Height==128 and t.m_ImageCount==6 and t.m_MipCount==8 and t.m_TextureFormat==12
        assert not t.m_StreamData.size
        raw=t.get_image_data();parts=[];offset=0;checks=[]
        for face in range(6):
            for mip in range(8):
                w=max(1,128>>mip);size=((w+3)//4)**2*16
                block=raw[offset:offset+size];assert len(block)==size;offset+=size
                image=parse_image_data(block,w,w,12,o.version,o.platform,flip=False).convert('RGBA')
                pixels=image.tobytes()
                # Verify the destination decoder sees the same exact pixel ordering.
                decoded=parse_image_data(pixels,w,w,4,o.version,o.platform,flip=False).convert('RGBA')
                assert decoded.tobytes()==pixels
                parts.append(pixels);checks.append({'face':face,'mip':mip,'bytes':len(pixels),'sha256':sha(pixels)})
        assert offset==len(raw)==t.m_CompleteImageSize*6
        before=o.read_typetree();metadata[key(o)]={k:v for k,v in before.items() if k not in allowed}
        t.image_data=b''.join(parts);t.m_TextureFormat=4;t.m_CompleteImageSize=len(t.image_data)//6;t.save()
        changed[key(o)]=sha(o.data);decoded_checks[key(o)]=checks
        rows.append({'object':key(o),'name':t.m_Name,'old_bytes':len(raw),'new_bytes':len(t.image_data),'faces':6,'mips':8,'pixel_checks':checks})
    assert len(changed)==5
    payload=env.file.save(packer='lz4');(out/'data.unity3d').write_bytes(payload)
    candidate_sha=sha(payload);del env,payload;gc.collect()
    check=UnityPy.load(str(out/'data.unity3d'));found={key(o):sha(o.get_raw_data()) for o in check.objects}
    assert found.keys()==original.keys()
    for k,v in found.items():assert v==changed.get(k,original[k]),('unexpected mutation',k)
    for o in check.objects:
        if key(o) not in changed:continue
        tree=o.read_typetree()
        for k,v in metadata[key(o)].items():assert tree[k]==v,('metadata changed',key(o),k)
        t=o.read();data=t.get_image_data();offset=0
        assert t.m_TextureFormat==4 and len(data)==t.m_CompleteImageSize*6
        for m in decoded_checks[key(o)]:
            b=data[offset:offset+m['bytes']];offset+=m['bytes'];assert sha(b)==m['sha256']
        assert offset==len(data)
    proof={'source_sha256':expected,'candidate_sha256':candidate_sha,'all_objects_verified':len(original),'changed_cubemaps':5,'decoded_faces_and_mips':240,'added_serialized_bytes':sum(r['new_bytes']-r['old_bytes'] for r in rows),'method':'Decode existing DXT5 faces/mips to RGBA32; no resizing or recompression','runtime_verified':False,'physical_iphone_verified':False}
    (out/'integrity.json').write_text(json.dumps(proof,indent=2));(out/'cubemap-report.json').write_text(json.dumps(rows,indent=2));print(json.dumps(proof),flush=True)
if __name__=='__main__':run(*sys.argv[1:])
