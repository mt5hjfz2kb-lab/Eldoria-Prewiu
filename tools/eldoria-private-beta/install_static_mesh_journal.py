"""Attach bounded memory journal to the exact certified native CPU-retirement build."""
import hashlib,json,sys
from pathlib import Path
SOURCE='05e1846f0213c6320850505bc054110c53934f55'
PLAY='c2ce74d1da4dc8eb2f3d88887fefeb0695d06090155ddf2b71c7c0fbeba89a98'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def install(root,native_run):
 root=Path(root);assert str(native_run).isdigit()
 identity=json.loads((root/'release-identity.json').read_text())
 assert identity['source_sha']==SOURCE and str(identity['native_run'])==str(native_run)
 assert identity['native_journey_verified'] and identity['native_destination_captures_verified']
 assert identity['owner_only'] and not identity['release_for_external_testers']
 before={str(p.relative_to(root)):sha(p) for p in root.rglob('*') if p.is_file()}
 play=root/'play.js';assert sha(play)==PLAY,'Unexpected exact native wrapper'
 source=play.read_text();marker='\n"use strict";\n// Fetch already exposes decoded gzip/Brotli bytes.'
 assert source.count(marker)==1 and source.count('function mount() {')==1
 extension=Path(__file__).with_name('world-phase-memory-diagnostic.js').read_text()
 tag="diagnostic:'beta21-world-v5',persistent:persistent,"
 assert extension.count(tag)==1
 extension=extension.replace(tag,tag+'nativeSource:'+json.dumps(SOURCE)+',nativeRun:'+json.dumps(str(native_run))+',')
 source=source.replace('function mount() {','function mount() {\n    return; // bounded world journal owns display',1)
 source=source.replace(marker,'\n'+extension+marker,1);play.write_text(source)
 name='play-'+sha(play)[:16]+'.js';(root/name).write_bytes(play.read_bytes())
 index=root/'index.html';html=index.read_text();old='play-'+PLAY[:16]+'.js';assert html.count(old)==1
 index.write_text(html.replace(old,name))
 changed={'play.js','index.html',name}
 assert all(sha(root/p)==h for p,h in before.items() if p not in changed)
 assert {str(p.relative_to(root)) for p in root.rglob('*') if p.is_file()}==set(before)|{name}
 print(json.dumps(dict(source=SOURCE,native_run=str(native_run),journal='21e2',native_bytes_unchanged=True,play_sha256=sha(play))))
if __name__=='__main__':install(*sys.argv[1:])
