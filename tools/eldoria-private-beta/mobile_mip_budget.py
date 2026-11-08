"""Mobile GPU material budget using existing authored mips; no resampling or gameplay edits."""
import UnityPy,json,hashlib,gc,sys
from pathlib import Path

def sha(b):return hashlib.sha256(b).hexdigest()
def key(o):return o.assets_file.name+':'+str(o.path_id)
def mip_bytes(w,h,fmt):
 if fmt in (3,4,5):return w*h*(3 if fmt==3 else 4)
 if fmt==48:return ((w+3)//4)*((h+3)//4)*16
 raise ValueError('unsupported format')
def run(source,destination,expected):
 src=Path(source);assert sha(src.read_bytes())==expected
 out=Path(destination);out.mkdir(parents=True,exist_ok=True)
 env=UnityPy.load(str(src));original={key(o):sha(o.get_raw_data()) for o in env.objects}
 material=set();sprites=set()
 for o in env.objects:
  if o.type.name=='Material':
   for _,entry in o.read().m_SavedProperties.m_TexEnvs:
    if entry.m_Texture.m_PathID:
     ref=entry.m_Texture.read().object_reader
     if ref.type.name=='Texture2D':material.add(key(ref))
  elif o.type.name=='Sprite':
   t=o.read()
   for ref in (t.m_RD.texture,t.m_RD.alphaTexture):
    if ref.m_PathID:sprites.add(key(ref.read().object_reader))
 changed={};metadata={};report=[]
 allowed={'m_Width','m_Height','m_MipCount','m_CompleteImageSize','image data'}
 for o in env.objects:
  if o.type.name!='Texture2D' or key(o) not in material or key(o) in sprites:continue
  t=o.read()
  if t.m_IsReadable or t.m_ImageCount!=1 or t.m_MipCount<2 or t.m_TextureFormat not in (3,4,5,48):continue
  if t.m_StreamData and t.m_StreamData.size:continue
  # Never touch CPU/query textures, sprite atlases, UI textures or single-mip images.
  if max(t.m_Width,t.m_Height)<=1024:continue
  raw=t.get_image_data();w,h=t.m_Width,t.m_Height;total=0
  for _ in range(t.m_MipCount):total+=mip_bytes(w,h,t.m_TextureFormat);w=max(1,w//2);h=max(1,h//2)
  assert total==len(raw),(key(o),total,len(raw))
  w,h=t.m_Width,t.m_Height;skip=0;levels=0
  while max(w,h)>1024 and levels<t.m_MipCount-1:
   skip+=mip_bytes(w,h,t.m_TextureFormat);w=max(1,w//2);h=max(1,h//2);levels+=1
  before=o.read_typetree();metadata[key(o)]={k:v for k,v in before.items() if k not in allowed}
  item=dict(object=key(o),name=t.m_Name,old_width=t.m_Width,old_height=t.m_Height,new_width=w,new_height=h,skipped_mips=levels,removed_bytes=skip,retained_payload_sha256=sha(raw[skip:]))
  t.m_Width=w;t.m_Height=h;t.m_MipCount-=levels;t.image_data=raw[skip:];t.m_CompleteImageSize=len(t.image_data);t.save();changed[key(o)]=sha(o.data);report.append(item)
 assert changed
 (out/'mip-report.json').write_text(json.dumps(report,indent=2))
 payload=env.file.save(packer='lz4');(out/'data.unity3d').write_bytes(payload);del env;gc.collect()
 check=UnityPy.load(str(out/'data.unity3d'));found={key(o):sha(o.get_raw_data()) for o in check.objects}
 assert found.keys()==original.keys()
 for k,v in found.items():assert v==changed.get(k,original[k]),('unexpected mutation',k)
 bykey={r['object']:r for r in report}
 for o in check.objects:
  if key(o) not in changed:continue
  tree=o.read_typetree()
  for k,v in metadata[key(o)].items():assert tree[k]==v,('metadata changed',key(o),k)
  assert sha(o.read().get_image_data())==bykey[key(o)]['retained_payload_sha256']
 proof=dict(source_sha256=expected,candidate_sha256=sha(payload),all_objects_verified=len(original),changed_textures=len(changed),removed_texture_bytes=sum(r['removed_bytes'] for r in report),candidate_bundle_bytes=len(payload),max_material_texture_dimension=1024,method='exact retained authored mip suffix; no new lossy encoding',runtime_verified=False,physical_iphone_verified=False)
 (out/'integrity.json').write_text(json.dumps(proof,indent=2));print(json.dumps(proof),flush=True)
if __name__=='__main__':run(*sys.argv[1:])
