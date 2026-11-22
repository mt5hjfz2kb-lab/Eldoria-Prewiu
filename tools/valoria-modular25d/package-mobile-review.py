"""Generate versioned mobile delivery from an already built Unity WebGL player."""
import gzip
import json
import shutil
import sys
from pathlib import Path

root, out, template = map(Path, sys.argv[1:4])
revision = sys.argv[4]
prefix = f'/r/{revision[:7]}/'
assets_dir = out / prefix.strip('/')
assets_dir.mkdir(parents=True,exist_ok=True)
for path in root.rglob('*'):
    if path.is_file() and path.name!='index.html':
        dest = assets_dir / path.relative_to(root)
        dest.parent.mkdir(parents=True,exist_ok=True)
        shutil.copyfile(path,dest)
loader = assets_dir/'Build/RegionModular.loader.js'
code = loader.read_text()
marker = 'e=A("dataUrl");'
assert code.count(marker)==1
loader.write_text(code.replace(marker,'e=b.valoriaLoadData?b.valoriaLoadData():A("dataUrl");'))
mobile=assets_dir/'mobile'
mobile.mkdir(exist_ok=True)
size=16*1024*1024
total=0
with gzip.open(assets_dir/'Build/RegionModular.data.unityweb','rb') as stream:
    while block:=stream.read(size):
        n = total//size
        (mobile/f'val-data-{n:03}.gz').write_bytes(gzip.compress(block,compresslevel=3,mtime=0))
        total+=len(block)
html=template.read_text().replace('506818849',str(total)).replace("'/mobile/",f"'{prefix}mobile/").replace("'/Build/",f"'{prefix}Build/").replace("'Build/",f"'{prefix}Build/").replace("'StreamingAssets'",f"'{prefix}StreamingAssets'")
(out/'index.html').write_text(html)
manifest={str(f.relative_to(assets_dir)):f.stat().st_size for f in assets_dir.rglob('*') if f.is_file()}
(out/'assets.json').write_text(json.dumps(manifest))
