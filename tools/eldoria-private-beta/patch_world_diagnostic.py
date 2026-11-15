"""Patch ONLY the certified beta21 shell; preserve every Unity and protected-route byte."""
import hashlib,json,re,sys
from pathlib import Path

PROOF_SHA='f4bc2c0ca3768f62992a66a7ef3fbe299ee2b561354c1b899847432fa62895fa'
PLAY_SHA='c2ce74d1da4dc8eb2f3d88887fefeb0695d06090155ddf2b71c7c0fbeba89a98'

def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def patch(root):
 root=Path(root);proof=root/'integrated-owner-stage-proof.json'
 assert sha(proof)==PROOF_SHA,'Unexpected publication'
 d=json.loads(proof.read_text()); route=root/d['candidate_route']
 assert d['source_sha']=='d0b21fd5708c5070d1bd62dc6c6a98399c549051'
 rows=dict(d['preserved_fingerprints']);rows.update({d['candidate_route']+'/'+k:v for k,v in d['candidate_fingerprints'].items()})
 assert {str(p.relative_to(root)) for p in root.rglob('*') if p.is_file()}==set(rows)|{'integrated-owner-stage-proof.json'}
 for path,row in rows.items():
  f=root/path;assert f.stat().st_size==row['bytes'] and sha(f)==row['sha256'],path
 before={str(p.relative_to(root)):sha(p) for p in root.rglob('*') if p.is_file()}
 play=route/'play.js';assert sha(play)==PLAY_SHA
 source=play.read_text(); marker='\n"use strict";\n// Fetch already exposes decoded gzip/Brotli bytes.'
 assert source.count(marker)==1 and source.count('function mount() {')==1
 # Retain the existing bounded startup/lifecycle API; replace its old display only.
 source=source.replace('function mount() {','function mount() {\n    return; // v4 world panel owns the diagnostic display',1)
 extension=(Path(__file__).parent/'world-phase-diagnostic.js').read_text()
 source=source.replace(marker,'\n'+extension+marker,1)
 play.write_text(source); name='play-'+sha(play)[:16]+'.js';(route/name).write_bytes(play.read_bytes())
 index=route/'index.html';html=index.read_text();old='play-c2ce74d1da4dc8eb.js';assert html.count(old)==1
 index.write_text(html.replace(old,name))
 changed={'beta-integrated-vigilia/'+x for x in ['play.js',name,'index.html']}
 for p,h in before.items():
  if p not in changed: assert sha(root/p)==h,p
 for p in changed:
  f=root/p;d['candidate_fingerprints'][str(f.relative_to(route))]={'bytes':f.stat().st_size,'sha256':sha(f)}
 d['diagnostic_shell']={'revision':'21d4','previous_publication_sha256':'f4bc2c0ca3768f62992a66a7ef3fbe299ee2b561354c1b899847432fa62895fa','native_bytes_unchanged':True,'physical_iphone_fixed':False,'local_only':True}
 d['status']='DIAGNOSTIC_SHELL_STAGED_NOT_PHYSICAL_IPHONE';d['physical_iphone_verified']=False;d['external_testers_enabled']=False;d['published']=False
 other=sum(p.stat().st_size for p in root.rglob('*') if p.is_file() and p!=proof)
 for _ in range(10):
  proof.write_text(json.dumps(d,indent=2)+'\n');n=other+proof.stat().st_size
  if d['combined_bytes']==n:break
  d['combined_bytes']=n
 else:raise AssertionError('Inventory size did not converge')
 assert d['combined_bytes']==sum(p.stat().st_size for p in root.rglob('*') if p.is_file())
 print('ONLY_DIAGNOSTIC_SHELL_CHANGED',sorted(changed),sha(proof))
if __name__=='__main__':patch(sys.argv[1])
