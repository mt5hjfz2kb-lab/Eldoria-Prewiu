import hashlib,json,pathlib,urllib.request,sys
from urllib.parse import quote
P=pathlib.Path
root=P(sys.argv[1]);root.mkdir(parents=True,exist_ok=False)
manifest=json.loads(P(__file__).with_name("sources.json").read_text())
for f in manifest:
 url="https://raw.githubusercontent.com/mt5hjfz2kb-lab/Eldoria-Prewiu/"+f["commit"]+"/"+quote(f["path"],safe="/")
 with urllib.request.urlopen(url,timeout=40) as r: raw=r.read(f["size"]+1)
 assert len(raw)==f["size"], f["path"]
 assert hashlib.sha1(b"blob "+str(len(raw)).encode()+b"\0"+raw).hexdigest()==f["blob_sha"],f["path"]
 p=root/f["destination"];p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(raw)
print("PINNED_PR198_INPUTS",len(manifest))
