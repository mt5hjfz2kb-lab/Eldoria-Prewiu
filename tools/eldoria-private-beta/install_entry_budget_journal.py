"""Attach the local journal to the exact new native candidate before browser QA."""
import hashlib,json,re,sys
from pathlib import Path

SOURCE='ca5e1c3d52a58d8c6d9c9a9114d2d35445a18029'
PLAY='c2ce74d1da4dc8eb2f3d88887fefeb0695d06090155ddf2b71c7c0fbeba89a98'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def install(root):
    root=Path(root)
    identity=json.loads((root/'release-identity.json').read_text())
    assert identity['source_sha']==SOURCE and str(identity['native_run'])=='43707493997'
    assert identity['owner_only'] and not identity['release_for_external_testers']
    before={str(p.relative_to(root)):sha(p) for p in root.rglob('*') if p.is_file()}
    play=root/'play.js';assert sha(play)==PLAY,'Unexpected candidate shell'
    source=play.read_text();marker='\n"use strict";\n// Fetch already exposes decoded gzip/Brotli bytes.'
    assert source.count(marker)==1 and source.count('function mount() {')==1
    extension=Path(__file__).with_name('world-phase-diagnostic.js').read_text()
    extension=extension.replace("ELDORIA_P0_LAZY_WORLD_REQUESTED: 'world-requested',", "ELDORIA_P0_LAZY_WORLD_REQUESTED: 'world-requested',\n    ELDORIA_P0_ENTRY_RENDER_BUDGET_REQUESTED: 'render-budget-requested',\n    ELDORIA_P0_ENTRY_RENDER_BUDGET_PREPARED: 'render-budget-ready',\n    ELDORIA_P0_ENTRY_RENDER_BUDGET_RESTORED: 'render-budget-restored',")
    extension=extension.replace('Diagnóstico · 21d4','Diagnóstico · 21e1')
    source=source.replace('function mount() {','function mount() {\n    return; // local world journal owns display',1)
    source=source.replace(marker,'\n'+extension+marker,1)
    play.write_text(source)
    name='play-'+sha(play)[:16]+'.js';(root/name).write_bytes(play.read_bytes())
    index=root/'index.html';html=index.read_text()
    old='play-'+PLAY[:16]+'.js';assert html.count(old)==1
    index.write_text(html.replace(old,name))
    changed={'play.js','index.html',name}
    for p,h in before.items():
        if p not in changed:assert sha(root/p)==h,p
    assert {str(p.relative_to(root)) for p in root.rglob('*') if p.is_file()}==set(before)|{name}
    print(json.dumps(dict(source=SOURCE,journal='21e1',native_bytes_unchanged=True,play_sha256=sha(play))))
if __name__=='__main__':install(sys.argv[1])
