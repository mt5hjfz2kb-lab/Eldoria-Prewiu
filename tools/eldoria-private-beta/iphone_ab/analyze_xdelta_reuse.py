#!/usr/bin/env python3
"""SHA-checked read-only xdelta3 feasibility for real Bastion I–V Unity chunks.

Uses exact prior beta10 Pages archive and a new certified clock candidate.
Does not publish or alter either build. Requires xdelta3 command installed.
"""
import argparse
import gzip
import hashlib
import json
import subprocess
import tarfile
import tempfile
from pathlib import Path,PurePosixPath

SOURCE_STABLE="be49d62e3daf490f41dba0bf8123a8e2eee64480"
SOURCE_NEW="004d3ef9c26e9070762e9e38ce84d69d525efd4f"

def safe_rel(s):
    p=PurePosixPath(s.split("?",1)[0])
    if p.is_absolute() or ".." in p.parts or "\\" in s:raise ValueError("unsafe path")
    return p.as_posix()
def chunks(man):
    parts=man["decoded_parts"][man["data"]]
    if len(parts)!=31 or any(x.get("encoding")!="gzip" or not x.get("decoded_sha256") for x in parts):
        raise ValueError("manifest is not a verified 31-chunk stream")
    return parts
def decode(b,item):
    v=gzip.decompress(b)
    if len(v)!=item["decoded_bytes"] or hashlib.sha256(v).hexdigest()!=item["decoded_sha256"]:
        raise RuntimeError("SHA mismatch")
    return v

def run(stable_tar, new_root, out_file, emit_dir=None):
    new=Path(new_root)
    meta_new=json.loads((new/"release-identity.json").read_text())
    if meta_new["source_sha"]!=SOURCE_NEW:raise RuntimeError("wrong new source")
    bparts=chunks(json.loads((new/"build-manifest.json").read_text()))
    rows=[]
    if emit_dir is not None:
        if emit_dir.exists():raise RuntimeError('Never replace an existing output patch directory')
        emit_dir.mkdir(parents=True,exist_ok=False)
    with tarfile.open(stable_tar,"r:*") as tar:
        members={}
        for m in tar:
            if not m.isfile():continue
            p=PurePosixPath(m.name).as_posix().removeprefix("beta-native325/")
            if p.startswith("beta-native325/"):p=p.removeprefix("beta-native325/")
            members[p]=m
        def oldbytes(name):
            m=members.get(safe_rel(name))
            if not m:raise RuntimeError("no old file "+name)
            with tar.extractfile(m) as f:return f.read()
        meta_old=json.loads(oldbytes("release-identity.json"))
        if meta_old["source_sha"]!=SOURCE_STABLE:raise RuntimeError("wrong old source")
        aparts=chunks(json.loads(oldbytes("build-manifest.json")))
        with tempfile.TemporaryDirectory() as td:
            temp=Path(td)
            before=temp/"before.bin";after=temp/"after.bin"
            delta=temp/"patch.vcdiff";restored=temp/"restored.bin"
            for i,(a,b) in enumerate(zip(aparts,bparts)):
                left=decode(oldbytes(a["url"]),a)
                right=decode((new/safe_rel(b["url"])).read_bytes(),b)
                before.write_bytes(left);after.write_bytes(right)
                subprocess.run(["xdelta3","-f","-e","-S","none","-A=","-s",str(before),str(after),str(delta)],check=True,
                               stdout=subprocess.PIPE,stderr=subprocess.PIPE)
                subprocess.run(["xdelta3","-f","-d","-s",str(before),str(delta),str(restored)],
                               check=True,stdout=subprocess.PIPE,stderr=subprocess.PIPE)
                if hashlib.sha256(restored.read_bytes()).hexdigest()!=b["decoded_sha256"]:
                    raise RuntimeError("xdelta verification failed: "+str(i))
                if emit_dir is not None:
                    from shutil import copyfile
                    copyfile(delta,emit_dir/('part-%02d.vcdiff'%i))
                row={"part":i,"old_compressed":len(gzip.compress(left,1)),
                     "new_compressed_original":len((new/safe_rel(b["url"])).read_bytes()),
                     "new_decoded":len(right),"delta_bytes":delta.stat().st_size,
                     "compression_pct":round(delta.stat().st_size/max(1,len(right))*100,3)}
                rows.append(row)
                print("VERIFIED_VCDIFF_PART",json.dumps(row),flush=True)
    totals={"vcdiff_bytes":sum(x["delta_bytes"] for x in rows),
        "new_original_gzip_bytes":sum(x["new_compressed_original"] for x in rows)}
    totals["relative_to_original_candidate"]=round(totals["vcdiff_bytes"]/totals["new_original_gzip_bytes"],5)
    totals["decoded_sha_verified"]=True
    totals["physical_iphone_tested"]=False
    totals["reconstruction_runs_without_external_network"]=True
    print("VERIFIED_VCDIFF_TOTALS",json.dumps(totals),flush=True)
    out_file.write_text(json.dumps({"stable_sha":SOURCE_STABLE,"candidate_sha":SOURCE_NEW,
                      "rows":rows,"totals":totals},indent=2)+"\n")
if __name__=="__main__":
    p=argparse.ArgumentParser()
    p.add_argument("--stable-pages-artifact-tar",required=True,type=Path)
    p.add_argument("--candidate-root",required=True,type=Path)
    p.add_argument("--output",required=True,type=Path)
    p.add_argument("--emit-dir",type=Path,default=None)
    a=p.parse_args()
    run(a.stable_pages_artifact_tar,a.candidate_root,a.output,a.emit_dir)
