"""Isolated native325 texture candidate. Never overwrites source. Not mobile certification."""
import UnityPy,astc_encoder as a,numpy as np,json,hashlib,re,time,sys,gc
from pathlib import Path
from UnityPy.export import Texture2DConverter as tc

def key(o):return o.assets_file.name+':'+str(o.path_id)
def digest(b):return hashlib.sha256(b).hexdigest()
def run(source,destination):
 out=Path(destination);out.mkdir(parents=True,exist_ok=True)
 assert digest(Path(source).read_bytes())=='299102dbc1835004a6d58a29ae33397f4836949c1eee8f7d321db8f05ea337e3'
 env=UnityPy.load(source);original={key(o):digest(o.get_raw_data()) for o in env.objects};deny=set();materials=set();normals=set()
 for o in env.objects:
  if o.type.name=='Material':
   for prop,tex in o.read().m_SavedProperties.m_TexEnvs:
    if tex.m_Texture.m_PathID:
     obj=tex.m_Texture.read().object_reader
     if obj.type.name!='Texture2D':continue
     k=key(obj);materials.add(k)
     if re.search('normal|bump',prop,re.I):normals.add(k)
     elif re.search('mask|height|depth|lookup|biome',prop,re.I):deny.add(k)
  if o.type.name=='Sprite':
   t=o.read()
   for ref in [t.m_RD.texture,t.m_RD.alphaTexture]:
    if ref.m_PathID:deny.add(key(ref.read().object_reader))
 contexts={cs:a.ASTCContext(a.ASTCConfig(profile,4,4,1,quality=98,flags=a.ASTCConfigFlags.USE_DECODE_UNORM8),threads=4) for cs,profile in [(0,a.ASTCProfile.LDR),(1,a.ASTCProfile.LDR_SRGB)]}
 sw=a.ASTCSwizzle.from_str('RGBA');changed={};metadata={};reports=[];savings=0
 for o in sorted(env.objects,key=lambda o:o.byte_size,reverse=True):
  if o.type.name!='Texture2D' or o.byte_size<1000000 or key(o) in deny or key(o) not in materials:continue
  t=o.read()
  if t.m_TextureFormat not in (3,4,5) or t.m_ColorSpace not in (0,1) or t.m_ImageCount!=1 or t.m_IsReadable:continue
  if re.search('engendro-reference|mask|height|depth|splat|lookup|noise|ramp|flow',t.m_Name,re.I):continue
  if t.m_StreamData and t.m_StreamData.size:continue
  ctx=contexts[t.m_ColorSpace]
  raw=t.get_image_data();w,h=t.m_Width,t.m_Height;pos=0;chunks=[];metrics=[];good=True
  for mip in range(t.m_MipCount or 1):
   n=w*h*(3 if t.m_TextureFormat==3 else 4);b=raw[pos:pos+n];pos+=n
   img=tc.parse_image_data(b,w,h,t.m_TextureFormat,o.version,o.platform,t.m_PlatformBlob,False).convert('RGBA')
   comp=ctx.compress(a.ASTCImage(a.ASTCType.U8,w,h,1,img.tobytes()),sw)
   decoded=tc.parse_image_data(comp,w,h,48,o.version,o.platform,None,False).convert('RGBA')
   p=np.asarray(img).astype('float32');q=np.asarray(decoded).astype('float32');d=np.abs(p-q);mse=float(np.mean(d*d));psnr=99 if mse==0 else float(10*np.log10(255**2/mse))
   amean=float(d[:,:,3].mean());metrics.append({'mip':mip,'psnr':psnr,'alpha_mae':amean})
   if psnr<(42 if mip<3 else 34) or amean>0.25:good=False
   if key(o) in normals:
    v=p[:,:,:3]/127.5-1;z=q[:,:,:3]/127.5-1
    v/=np.maximum(np.linalg.norm(v,axis=2,keepdims=True),1e-8);z/=np.maximum(np.linalg.norm(z,axis=2,keepdims=True),1e-8)
    angles=np.degrees(np.arccos(np.clip(np.sum(v*z,axis=2),-1,1)))
    p95=float(np.percentile(angles,95));p99=float(np.percentile(angles,99))
    metrics[-1].update(normal_degrees_p95=p95,normal_degrees_p99=p99)
    if p95>(1 if mip<3 else 2) or p99>(2 if mip<3 else 3):good=False
   chunks.append(comp);w=max(1,w//2);h=max(1,h//2)
  assert pos==len(raw),(t.m_Name,pos,len(raw))
  record={'id':key(o),'name':t.m_Name,'before':len(raw),'after':sum(map(len,chunks)),'accepted':good,'metrics':metrics}
  reports.append(record)
  if good:
   # Preserve every non-payload field, including dimensions, mip count, color space and sampler.
   before=o.read_typetree()
   metadata[key(o)]={k:v for k,v in before.items() if k not in ('image data','m_TextureFormat','m_CompleteImageSize')}
   t.image_data=b''.join(chunks);t.m_TextureFormat=48;t.m_CompleteImageSize=len(t.image_data);t.save()
   assert o.data is not None
   changed[key(o)]=digest(o.data);savings+=len(raw)-len(t.image_data)
  print(json.dumps({k:record[k] for k in ['id','name','before','after','accepted']}),flush=True)
 (out/'texture-report.json').write_text(json.dumps({'changes':reports,'serialized_saved_bytes':savings,'changed_count':len(changed)},indent=2))
 assert changed,'no passing textures'
 payload=env.file.save(packer='lz4');(out/'data.unity3d').write_bytes(payload);del env;gc.collect()
 check=UnityPy.load(str(out/'data.unity3d'));found={key(o):digest(o.get_raw_data()) for o in check.objects}
 assert found.keys()==original.keys(),'object graph membership changed'
 for k,v in found.items():assert v==changed.get(k,original[k]),('unexpected object mutation',k)
 for o in check.objects:
  if key(o) in changed:
   tree=o.read_typetree();assert tree['m_TextureFormat']==48
   for k,v in metadata[key(o)].items():assert tree[k]==v,('texture metadata changed',key(o),k)
 proof={'source_sha256':digest(Path(source).read_bytes()),'candidate_sha256':digest(payload),'changed_textures':len(changed),'all_objects_verified':len(original),'saved_serialized_bytes':savings,'candidate_bundle_bytes':len(payload),'runtime_verified':False,'physical_iphone_verified':False}
 (out/'integrity.json').write_text(json.dumps(proof,indent=2));print('INTEGRITY_PASS',json.dumps(proof),flush=True)
if __name__=='__main__':run(sys.argv[1],sys.argv[2])
