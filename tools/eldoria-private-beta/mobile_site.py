"""Create an isolated mobile candidate from original native325 plus audited bundle."""
import sys,json,struct,gzip,hashlib,shutil,importlib.util
from pathlib import Path

def pack_data(original,bundle,output):
 with open(original,'rb') as src:
  assert src.read(16)==b'UnityWebData1.0\0'
  end=struct.unpack('<I',src.read(4))[0];header=src.read(end-20);pos=0;entries=[]
  while pos<len(header):
   offset,size,n=struct.unpack_from('<III',header,pos);pos+=12;name=header[pos:pos+n];pos+=n
   entries.append((offset,size,name))
  assert sum(name==b'data.unity3d' for _,_,name in entries)==1
  newhead=b'UnityWebData1.0\0'+struct.pack('<I',end);newpos=end
  for offset,size,name in entries:
   n=Path(bundle).stat().st_size if name==b'data.unity3d' else size
   newhead+=struct.pack('<III',newpos,n,len(name))+name;newpos+=n
  assert len(newhead)==end
  with open(output,'wb') as out:
   out.write(newhead)
   for offset,size,name in entries:
    if name==b'data.unity3d':
     with open(bundle,'rb') as b:shutil.copyfileobj(b,out)
    else:
     src.seek(offset);data=src.read(size);assert len(data)==size;out.write(data)

def candidate(original_site,original_data,bundle,destination,revision='native325-mobile-candidate1'):
 assert revision.replace('-','').isalnum()
 original_site=Path(original_site);out=Path(destination);out.mkdir(parents=True,exist_ok=True)
 # Shared engine/wrapper is copied byte-for-byte. Only the manifest and data differ.
 for p in original_site.rglob('*'):
  if not p.is_file() or 'payload-decoded' in p.parts:continue
  dest=out/p.relative_to(original_site);dest.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(p,dest)
 raw=out/'candidate-data.bin';pack_data(original_data,bundle,raw)
 m=json.loads((out/'build-manifest.json').read_text());old=m['data'];m['data']='Build/ValoriaWebPlaytest.data.unityweb?candidate='+revision;m['decoded_sizes'].pop(old);m['decoded_parts'].pop(old)
 m['decoded_sizes'][m['data']]=raw.stat().st_size;parts=[]
 with raw.open('rb') as f:
  while block:=f.read(16*1024*1024):
   rel=f'payload-decoded/{revision}-{len(parts)}.gz';p=out/rel;p.parent.mkdir(exist_ok=True);p.write_bytes(gzip.compress(block,compresslevel=6,mtime=0))
   parts.append({'url':rel,'encoding':'gzip','decoded_bytes':len(block),'decoded_sha256':hashlib.sha256(block).hexdigest()})
 m['decoded_parts'][m['data']]=parts;m['delivery_adapter']='mobile-astc-candidate-v1';m['revision']=revision
 raw.unlink();(out/'build-manifest.json').write_text(json.dumps(m,indent=2))
 identity=json.loads((out/'release-identity.json').read_text());identity.update(revision=m['revision'],game_bytes_unchanged=False,gameplay_code_unchanged=True,texture_payload_modified=True,physical_mobile_tested=False,runtime_verified=False,candidate=True)
 identity['decoded_files']=[{'path':p['url'],'decoded_sha256':p['decoded_sha256'],'decoded_bytes':p['decoded_bytes']} for parts in m['decoded_parts'].values() for p in parts]
 identity['candidate_bundle_sha256']=hashlib.sha256(Path(bundle).read_bytes()).hexdigest()
 (out/'release-identity.json').write_text(json.dumps(identity,indent=2))
 print('MOBILE_SITE_READY',m['decoded_sizes'][m['data']],sum(p.stat().st_size for p in out.rglob('*') if p.is_file()),flush=True)
if __name__=='__main__':candidate(*sys.argv[1:])
