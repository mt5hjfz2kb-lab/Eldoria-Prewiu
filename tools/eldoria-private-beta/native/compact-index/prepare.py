"""Extract the frozen terrain query data and the exact production compact-index class."""
import gzip,hashlib,json,struct,sys
from pathlib import Path
import UnityPy
source=Path(sys.argv[1]);project=Path('tools/eldoria-private-beta/native/compact-index')
m=json.loads((source/'build-manifest.json').read_text())
assert m['revision']=='native325-mobile6-cubemap'
raw=bytearray()
for p in m['decoded_parts'][m['data']]:
 b=gzip.decompress((source/p['url']).read_bytes())
 assert len(b)==p['decoded_bytes'] and hashlib.sha256(b).hexdigest()==p['decoded_sha256']
 raw.extend(b)
assert raw[:16]==b'UnityWebData1.0\0'
end=struct.unpack_from('<I',raw,16)[0];pos=20;bundle=None
while pos<end:
 off,size,n=struct.unpack_from('<III',raw,pos);pos+=12
 name=bytes(raw[pos:pos+n]).decode();pos+=n
 if name=='data.unity3d':bundle=bytes(raw[off:off+size])
assert hashlib.sha256(bundle).hexdigest()=='e54d547487e0d915f5a8c3fbbd0354a79672041dd201b3e63672de5e740ea6c1'
del raw
env=UnityPy.load(bundle);found=0
for o in env.objects:
 if o.type.name=='TextAsset':
  t=o.read()
  if t.m_Name=='RegionOneBlenderQueries':
   b=t.m_Script.encode('utf-8','surrogateescape') if isinstance(t.m_Script,str) else t.m_Script
   (project/'queries.gz').write_bytes(b);found+=1
assert found==1
text=Path('Unity/Assets/Eldoria/World4XCanon/RegionOneBlenderLandscape.cs').read_text()
start=text.index('// COMPACT_TERRAIN_VERIFICATION_BEGIN');end=text.index('// COMPACT_TERRAIN_VERIFICATION_END')+len('// COMPACT_TERRAIN_VERIFICATION_END')
helper=text[start:end];(project/'CompactTerrainVerification.cs').write_text(helper)
(project/'provenance.json').write_text(json.dumps({'helper_sha256':hashlib.sha256(helper.encode()).hexdigest(),'query_gzip_sha256':hashlib.sha256((project/'queries.gz').read_bytes()).hexdigest(),'candidate':'mobile6'},indent=2))
print('Exact candidate query data and exact production helper extracted')
