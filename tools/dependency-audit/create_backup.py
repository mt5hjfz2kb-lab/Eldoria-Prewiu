#!/usr/bin/env python3
"""Read-only snapshot ZIP exporter for 354 previously audited Eldoria assets."""
import argparse, hashlib, json, os, pathlib, shutil, subprocess, sys, time, zipfile
SHA="d9d4e1f9ffb9f7ab29e1c963a80eb08321c35bb4"
KEEP_REF="archive/eldoria-assets-before-cleanup-20261026"
def git(repo,*args):
    return subprocess.run(["git","-C",str(repo),*args],check=True,stdout=subprocess.PIPE,stderr=subprocess.PIPE).stdout
def main():
    a=argparse.ArgumentParser()
    a.add_argument("--repo",required=True);a.add_argument("--audit-jsonl",required=True);a.add_argument("--out",required=True)
    o=a.parse_args();root=pathlib.Path(o.repo).resolve();out=pathlib.Path(o.out).resolve()
    if not (root/".git").exists():raise RuntimeError("Missing Git working tree")
    git(root,"cat-file","-e",SHA+"^{commit}")
    candidates=[json.loads(s) for s in pathlib.Path(o.audit_jsonl).read_text(encoding="utf-8").splitlines() if s.strip()]
    candidates=[c for c in candidates if c["category"]=="REVIEW_NO_UNITY_REFERENCE"]
    if len(candidates)!=354 or sum(c["bytes"] for c in candidates)!=1909173156:
        raise RuntimeError("Audit file mismatch; no archive created")
    tree={}
    for entry in git(root,"ls-tree","-r","-z","-l",SHA).split(b"\0"):
        if not entry:continue
        header,name=entry.split(b"\t",1); mode,typ,sha,size=header.decode().split(" ")
        if typ=="blob":tree[name.decode()]=(sha,int(size))
    wanted=set()
    for c in candidates:
        p="Unity/"+c["path"]
        if p not in tree:raise RuntimeError("Pinned Git asset missing: "+p)
        if p+".meta" not in tree:raise RuntimeError("Missing Unity GUID .meta: "+p)
        meta_data=git(root,"cat-file","blob",SHA+":"+p+".meta")
        if ("guid: "+c["guid"]).encode() not in meta_data:raise RuntimeError("Unity GUID .meta mismatch: "+p)
        wanted.add(p);wanted.add(p+".meta")
        ancestor=pathlib.PurePosixPath(p).parent
        while str(ancestor) not in ("Unity/Assets","Unity","."):
            if str(ancestor)+".meta" in tree:wanted.add(str(ancestor)+".meta")
            ancestor=ancestor.parent
    paths=sorted(wanted)
    out.parent.mkdir(parents=True,exist_ok=True)
    if out.exists():raise RuntimeError("Refusing to overwrite existing archive: "+str(out))
    free=shutil.disk_usage(out.parent).free
    if free < 4*1024**3:raise RuntimeError("At least 4 GiB of free disk space required; no output created")
    tmp=out.with_suffix(out.suffix+".partial")
    manifest={"schema":1,"source_sha":SHA,"restore_branch":KEEP_REF,
              "asset_count":354,"asset_bytes":1909173156,"all_source_files":len(paths),
              "scope":"Original Unity assets, sibling .meta files, folder .meta files; no Git modifications",
              "created_utc":time.strftime("%Y-%m-%dT%H:%M:%SZ",time.gmtime()),"files":[]}
    try:
        with zipfile.ZipFile(tmp,"w",compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as z:
            for i,p in enumerate(paths,1):
                blob,size=tree[p];h=hashlib.sha1();h.update(("blob "+str(size)+"\0").encode());sha256=hashlib.sha256()
                sub=subprocess.Popen(["git","-C",str(root),"cat-file","blob",SHA+":"+p],stdout=subprocess.PIPE,stderr=subprocess.PIPE)
                n=0
                try:
                    with z.open(p,"w") as target:
                        while True:
                            block=sub.stdout.read(1024*1024)
                            if not block:break
                            target.write(block);h.update(block);sha256.update(block);n+=len(block)
                    err=sub.stderr.read()
                    if sub.wait()!=0:raise RuntimeError("Cannot read "+p+": "+err.decode(errors="replace")[-300:])
                finally:
                    if sub.poll() is None:sub.kill()
                    sub.stdout.close();sub.stderr.close()
                if n!=size or h.hexdigest()!=blob:raise RuntimeError("Git object integrity mismatch: "+p)
                manifest["files"].append({"path":p,"bytes":n,"git_blob_sha":blob,"sha256":sha256.hexdigest()})
                if i%50==0:print("ELDORIA_ARCHIVE_VERIFIED_FILES",i,"/",len(paths),flush=True)
            z.writestr("ELDORIA_ARCHIVE_MANIFEST.json",json.dumps(manifest,ensure_ascii=False,indent=2))
        with zipfile.ZipFile(tmp,"r") as z:
            bad=z.testzip()
            if bad:raise RuntimeError("ZIP internal CRC failed on "+bad)
        os.replace(tmp,out)
        archive_hash=hashlib.sha256()
        with out.open("rb") as fp:
            for chunk in iter(lambda:fp.read(1024*1024),b""):archive_hash.update(chunk)
        out.with_suffix(out.suffix+".sha256").write_text(archive_hash.hexdigest()+"  "+out.name+"\n",encoding="utf-8")
        print("ELDORIA_ARCHIVE_BACKUP_VERIFIED",json.dumps({"file":str(out),"bytes":out.stat().st_size,"sha256":archive_hash.hexdigest(),"unity_candidates":len(candidates),"included_files":len(paths)}),flush=True)
    finally:
        if tmp.exists():tmp.unlink()
if __name__=="__main__":
    main()
