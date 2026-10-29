#!/usr/bin/env python3
"""Read-only inventory of tracked visual assets. No Tripo, Unity or writes to sources."""
import argparse, hashlib, json, pathlib, subprocess
EXT={".glb":"model3d",".gltf":"model3d",".fbx":"model3d",".blend":"editable3d",".obj":"model3d",".png":"image2d",".jpg":"image2d",".jpeg":"image2d",".webp":"image2d",".tga":"image2d",".psd":"editable2d",".svg":"vector2d",".aseprite":"editable2d"}
def scan(root):
    paths=subprocess.check_output(["git","-C",str(root),"ls-files","-z"]).decode("utf-8","replace").split("\0")
    for rel in sorted(filter(None,paths)):
        p=pathlib.PurePosixPath(rel)
        if p.suffix.lower() not in EXT: continue
        full=root/rel
        if not full.is_file(): continue
        h=hashlib.sha256()
        with full.open("rb") as f:
            for block in iter(lambda:f.read(1048576),b""):h.update(block)
        yield {"path":rel,"kind":EXT[p.suffix.lower()],"sha256":h.hexdigest(),"bytes":full.stat().st_size,"approval":"UNREVIEWED","source":"tracked_git"}
def main():
    a=argparse.ArgumentParser()
    a.add_argument("--root",default=".")
    a.add_argument("--output",default="-")
    args=a.parse_args()
    root=pathlib.Path(args.root).resolve()
    entries=list(scan(root))
    doc={"schema_version":1,"read_only":True,"count":len(entries),"entries":entries}
    payload=json.dumps(doc,ensure_ascii=False,indent=2)+"\n"
    if args.output=="-":print(payload,end="")
    else:pathlib.Path(args.output).write_text(payload,encoding="utf-8")
if __name__=="__main__":main()
