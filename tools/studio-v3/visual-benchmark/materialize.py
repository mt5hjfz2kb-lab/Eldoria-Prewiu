"""Fetch only exact public Git blobs needed by the visual benchmark.
Verify Git SHA-1 object identity and byte length before writing.
No checkout of large Unity trees, no secrets, no spending.
"""
import hashlib,json,pathlib,re,sys,urllib.request
from urllib.parse import quote
HERE=pathlib.Path(__file__).resolve().parent
root=pathlib.Path(sys.argv[1]);root.mkdir(parents=True,exist_ok=False)
rows=json.loads((HERE/"source-manifest.json").read_text())
assert len(rows["files"])==15
proof=[]
for f in rows["files"]:
    for k in ("path","destination"):
        assert not f[k].startswith("/") and all(re.fullmatch(r"[A-Za-z0-9_.-]+",p) and p not in (".","..") for p in f[k].split("/"))
    assert re.fullmatch(r"[0-9a-f]{40}",f["commit"]) and re.fullmatch(r"[0-9a-f]{40}",f["blob_sha"])
    url="https://raw.githubusercontent.com/mt5hjfz2kb-lab/Eldoria-Prewiu/"+f["commit"]+"/"+quote(f["path"],safe="/")
    with urllib.request.urlopen(url,timeout=40) as res:
        raw=res.read(f["size"]+1)
    assert len(raw)==f["size"],"Size mismatch "+f["path"]
    blob=hashlib.sha1(b"blob "+str(len(raw)).encode()+b"\0"+raw).hexdigest()
    assert blob==f["blob_sha"],"Git blob mismatch "+f["path"]
    dst=root/f["destination"];dst.parent.mkdir(parents=True,exist_ok=True)
    with dst.open("xb") as stream:stream.write(raw)
    proof.append({"path":f["path"],"commit":f["commit"],"blob_sha":blob,"sha256":hashlib.sha256(raw).hexdigest(),"bytes":len(raw)})
print("STUDIO_V3_VISUAL_PINNED_SOURCE "+json.dumps(proof))
