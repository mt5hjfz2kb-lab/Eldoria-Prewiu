"""Retrieve exact Git object bytes, no heavy checkout or external paid service."""
import hashlib,json,pathlib,re,sys,urllib.request
from urllib.parse import quote
root=pathlib.Path(sys.argv[1]);root.mkdir(parents=True,exist_ok=False)
files=json.loads(pathlib.Path(__file__).with_name("source-manifest.json").read_text())["files"]
for f in files:
    for k in ("path","destination"):
        assert all(re.fullmatch(r"[A-Za-z0-9_.-]+",p) and p not in (".","..") for p in f[k].split("/"))
    assert re.fullmatch(r"[a-f0-9]{40}",f["commit"])
    url="https://raw.githubusercontent.com/mt5hjfz2kb-lab/Eldoria-Prewiu/"+f["commit"]+"/"+quote(f["path"],safe="/")
    with urllib.request.urlopen(url,timeout=40) as r:raw=r.read(f["size"]+1)
    assert len(raw)==f["size"],f["path"]
    assert hashlib.sha1(b"blob "+str(len(raw)).encode()+b"\0"+raw).hexdigest()==f["blob_sha"],f["path"]
    p=root/f["destination"];p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(raw)
print("RELICARIO_PINNED_INPUTS",len(files))
