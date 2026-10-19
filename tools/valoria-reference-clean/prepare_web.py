"""Reuse the current private Site transport for the exact native Unity output.

Recovered from the existing valoria-city-world Site source at 8500154eeb25e3bd.
The browser job tests this package; the same dist is then deployed unchanged.
"""
from pathlib import Path
import hashlib,json,shutil,sys
if len(sys.argv)!=6: raise SystemExit("Usage: prepare_web.py UNITY_OUTPUT PACKAGE REVISION SOURCE_COMMIT NATIVE_RUN")
source=Path(sys.argv[1]).resolve()
package=Path(sys.argv[2]).resolve()
revision,source_commit,native_run=sys.argv[3],sys.argv[4],int(sys.argv[5])
dist=package/"dist"
if source==dist or source in dist.parents: raise SystemExit("Package must be outside the Unity output")
dist.mkdir(parents=True,exist_ok=True)
shell=Path(__file__).parent/"web-shell"
for asset in shell.iterdir():
 if asset.is_file(): shutil.copy2(asset,dist/asset.name)
# The welcome image is an actual capture of the exact integrated scene.
review=Path("docs/evidence/valoria-reference-clean/environment-city-wide-banner-after.png")
if not review.is_file(): raise SystemExit("Native integrated city capture missing")
shutil.copy2(review,dist/"city.png")
build=source/"Build"
if not build.is_dir(): raise SystemExit("Unity Build directory missing")
def one(pattern):
 files=[p for p in build.glob(pattern) if not p.name.startswith(".")]
 if len(files)!=1: raise SystemExit(f"Expected one {pattern}: {files}")
 return files[0]
loader=one("*.loader.js")
framework=one("*.framework.js*")
data=one("*.data*")
code=one("*.wasm*")
manifest={"revision":revision,"loader":"Build/"+loader.name,"framework":"Build/"+framework.name,"data":"Build/"+data.name,"code":"Build/"+code.name,"chunks":{}}
# Replace only prior compiled assets; retain shell and browser saves.
for name in ["Build","payload","StreamingAssets","TemplateData"]:
 target=dist/name
 if target.exists():shutil.rmtree(target)
records=[]
chunk_size=20*1024*1024
for path in source.rglob("*"):
 if not path.is_file() or path.name=="index.html" or path.name.startswith("."):continue
 relative=path.relative_to(source).as_posix()
 raw=path.read_bytes();digest=hashlib.sha256(raw).hexdigest()
 target=dist/relative
 if len(raw)>chunk_size:
  parts=[];rebuilt=hashlib.sha256()
  for n,start in enumerate(range(0,len(raw),chunk_size)):
   piece=raw[start:start+chunk_size];piece_name=f"payload/{digest[:20]}/{n:03d}.bin"
   out=dist/piece_name;out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes(piece)
   parts.append(piece_name);rebuilt.update(out.read_bytes())
  assert rebuilt.hexdigest()==digest
  manifest["chunks"][relative]={"bytes":len(raw),"sha256":digest,"parts":parts}
 else:
  target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(path,target)
  assert hashlib.sha256(target.read_bytes()).hexdigest()==digest
 records.append({"file":relative,"bytes":len(raw),"sha256":digest})
hashes={item["file"]:item["sha256"][:16] for item in records}
manifest["chunks"]={name+"?v="+hashes[name]:entry for name,entry in manifest["chunks"].items()}
for key in ("loader","framework","data","code"):
 manifest[key]+="?v="+hashes[manifest[key]]
(dist/"build-manifest.json").write_text(json.dumps(manifest,indent=2)+"\n")
(package/"unity-output-verification.json").write_text(json.dumps({"source_commit":source_commit,"native_run":native_run,"files":records,"all_transport_hashes_verified":True,"physical_iphone_tested":False,"browser_runtime_qa":False},indent=2)+"\n")
assert max(p.stat().st_size for p in dist.rglob("*") if p.is_file())<=chunk_size
print(json.dumps({"files":len(records),"bytes":sum(x["bytes"] for x in records),"chunked":list(manifest["chunks"])}))
