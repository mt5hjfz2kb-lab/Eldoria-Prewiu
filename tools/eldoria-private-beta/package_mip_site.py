import sys,json,gzip,struct,hashlib,shutil
from pathlib import Path

def sha(b):return hashlib.sha256(b).hexdigest()
def extract(site,out):
 site=Path(site);out=Path(out);out.mkdir(parents=True,exist_ok=True)
 m=json.loads((site/'build-manifest.json').read_text());raw=bytearray()
 for part in m['decoded_parts'][m['data']]:
  b=gzip.decompress((site/part['url']).read_bytes());assert len(b)==part['decoded_bytes'] and sha(b)==part['decoded_sha256'];raw.extend(b)
 assert raw[:16]==b'UnityWebData1.0\x00';end=struct.unpack_from('<I',raw,16)[0];pos=20;entries=[]
 while pos<end:
  off,size,n=struct.unpack_from('<III',raw,pos);pos+=12;name=bytes(raw[pos:pos+n]).decode();pos+=n;entries.append((name,off,size))
 assert pos==end
 for name,off,size in entries:
  if name=='data.unity3d':(out/'data.unity3d').write_bytes(raw[off:off+size])
 (out/'data.bin').write_bytes(raw);(out/'entries.json').write_text(json.dumps(entries));assert sha((out/'data.unity3d').read_bytes())=='e5299e7293f4e2ed5a5bed30c9c87d3141a7594401d3cdb965761cba9a834faa'
def package(site,extracted,candidate,out):
 site=Path(site);extracted=Path(extracted);candidate=Path(candidate);out=Path(out)
 shutil.copytree(site,out,dirs_exist_ok=True);shutil.rmtree(out/'payload-decoded');(out/'payload-decoded').mkdir()
 raw=(extracted/'data.bin').read_bytes();entries=json.loads((extracted/'entries.json').read_text());bundle=(candidate/'data.unity3d').read_bytes();proof=json.loads((candidate/'integrity.json').read_text());assert sha(bundle)==proof['candidate_sha256']
 files=[(name,bundle if name=='data.unity3d' else raw[off:off+size]) for name,off,size in entries]
 header=20+sum(12+len(n.encode()) for n,_ in files);buf=bytearray(b'UnityWebData1.0\x00'+struct.pack('<I',header));offset=header
 for name,data in files:
  nb=name.encode();buf.extend(struct.pack('<III',offset,len(data),len(nb))+nb);offset+=len(data)
 for name,data in files:buf.extend(data)
 revision='native325-mobile5';parts=[]
 for i,start in enumerate(range(0,len(buf),16*1024*1024)):
  data=bytes(buf[start:start+16*1024*1024]);name=f'payload-decoded/{revision}-{i}.gz';(out/name).write_bytes(gzip.compress(data,compresslevel=6,mtime=0));parts.append(dict(url=name,encoding='gzip',decoded_bytes=len(data),decoded_sha256=sha(data)))
 m=json.loads((out/'build-manifest.json').read_text());old=m['data'];new='Build/'+revision+'.data';m['data']=new;m['revision']=revision;m['decoded_sizes'].pop(old,None);m['decoded_sizes'][new]=len(buf);m['decoded_parts'].pop(old,None);m['decoded_parts'][new]=parts;(out/'build-manifest.json').write_text(json.dumps(m,indent=2))
 i=json.loads((out/'release-identity.json').read_text());i['decoded_files']=[x for x in i['decoded_files'] if not x.get('url','').startswith('payload-decoded/')]+parts;i['packaged_bytes']=sum(p.stat().st_size for p in out.rglob('*') if p.is_file());i.update(revision=revision,candidate_bundle_sha256=proof['candidate_sha256'],runtime_verified=False,physical_mobile_tested=False,release_for_external_testers=False,mobile_mip_budget=proof,decoded_parts=parts);(out/'release-identity.json').write_text(json.dumps(i,indent=2))
 print(json.dumps(dict(revision=revision,data_bytes=len(buf),site_bytes=sum(p.stat().st_size for p in out.rglob('*') if p.is_file()),bundle_sha256=proof['candidate_sha256'])))
if __name__=='__main__':
 if sys.argv[1]=='extract':extract(*sys.argv[2:])
 else:package(*sys.argv[2:])
