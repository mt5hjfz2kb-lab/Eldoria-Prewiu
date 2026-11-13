#!/usr/bin/env python3
"""Read-only, SHA-verified potential chunk-sharing analysis for two real Unity WebGL releases.

This script never writes either build or modifies game state. It compares decoded,
verified Unity asset chunks and reports whether a low-memory XOR delta transport
could host both exact builds on one GitHub Pages deployment under the 10-minute
backend deployment ceiling. Results are NOT physical iPhone verification.
"""
import argparse
import gzip
import hashlib
import io
import json
import tarfile
import zlib
from pathlib import Path, PurePosixPath

STABLE_REV="native325-mobile10-pretest-ux"
STABLE_SHA="be49d62e3daf490f41dba0bf8123a8e2eee64480"
CANDIDATE_REV="native325-mobile12-minimal-world-clock-qa"
CANDIDATE_SHA="004d3ef9c26e9070762e9e38ce84d69d525efd4f"

def read_json(data):
    return json.loads(data.decode("utf-8-sig"))

def safe_path(value):
    p=PurePosixPath(value.split("?",1)[0])
    if p.is_absolute() or ".." in p.parts or not p.parts or "\\" in value:
        raise RuntimeError("Unsafe asset path: "+value)
    return p.as_posix()

def get_data_parts(obj):
    name=obj["data"]
    parts=obj.get("decoded_parts",{}).get(name)
    if not isinstance(parts,list) or len(parts)!=31:
        raise RuntimeError("Not 31 verified streamed data chunks")
    if any(x.get("encoding")!="gzip" or not isinstance(x.get("decoded_bytes"),int)
           or not x.get("decoded_sha256") for x in parts):
        raise RuntimeError("Missing integrity metadata")
    return parts

def decode(data,info,label):
    out=gzip.decompress(data)
    if len(out)!=info["decoded_bytes"] or hashlib.sha256(out).hexdigest()!=info["decoded_sha256"]:
        raise RuntimeError("SHA256 or size mismatch on "+label)
    return out

def xor(a,b):
    # bytes -> bigint is a C-level operation; no O(500 MiB) Python zip loop.
    n=max(len(a),len(b))
    return (int.from_bytes(a,"little") ^ int.from_bytes(b,"little")).to_bytes(n,"little")

def analyze(stable_archive,candidate_root):
    cand=Path(candidate_root)
    b=read_json((cand/"release-identity.json").read_bytes())
    if b.get("revision")!=CANDIDATE_REV or b.get("source_sha")!=CANDIDATE_SHA:
        raise RuntimeError("Wrong, unapproved candidate identity")
    bparts=get_data_parts(read_json((cand/"build-manifest.json").read_bytes()))
    items=[]
    with tarfile.open(stable_archive,"r:*") as oldtar:
        members={}
        for t in oldtar:
            key=PurePosixPath(t.name).as_posix().removeprefix("beta-native325/")
            if key.startswith("beta-native325/"):
                key=key.removeprefix("beta-native325/")
            if t.isfile():members[key]=t
        def tarbytes(rel):
            name=safe_path(rel)
            if name not in members:raise RuntimeError("Missing original exact site file: "+name)
            f=oldtar.extractfile(members[name])
            if f is None:raise RuntimeError("Archive file unreadable: "+name)
            return f.read()
        oldid=read_json(tarbytes("release-identity.json"))
        if oldid.get("revision")!=STABLE_REV or oldid.get("source_sha")!=STABLE_SHA:
            raise RuntimeError("Wrong previously owner-playable beta10 source")
        aparts=get_data_parts(read_json(tarbytes("build-manifest.json")))
        for i,(a,bpart) in enumerate(zip(aparts,bparts)):
            nameA=safe_path(a["url"]); nameB=safe_path(bpart["url"])
            oldCompressed=tarbytes(nameA)
            newCompressed=(cand/nameB).read_bytes()
            before=decode(oldCompressed,a,f"stable data part {i}")
            after=decode(newCompressed,bpart,f"candidate data part {i}")
            n=max(len(before),len(after))
            aligned_equal=sum(before[k]==after[k] for k in range(min(len(before),len(after))))
            delta=xor(before,after)
            compressed=zlib.compress(delta,9)
            # reconstruct exact candidate to rule out broken patches
            reconstructed=xor(before,delta)[:len(after)]
            if reconstructed!=after or hashlib.sha256(reconstructed).hexdigest()!=bpart["decoded_sha256"]:
                raise RuntimeError("XOR reconstruction mismatch at "+str(i))
            entry={"index":i,"stable_encoded_bytes":len(oldCompressed),
                   "candidate_encoded_bytes":len(newCompressed),
                   "stable_decoded_bytes":len(before),"candidate_decoded_bytes":len(after),
                   "aligned_equal_percent":round(100*aligned_equal/n,3),
                   "compressed_xor_bytes":len(compressed),
                   "delta_to_full_percent":round(100*len(compressed)/max(1,len(newCompressed)),3)}
            items.append(entry)
            print("PART",json.dumps(entry,sort_keys=True),flush=True)
    totals={k:sum(x[k] for x in items) for k in
            ["stable_encoded_bytes","candidate_encoded_bytes","stable_decoded_bytes","candidate_decoded_bytes","compressed_xor_bytes"]}
    totals["candidate_delta_ratio"]=round(totals["compressed_xor_bytes"]/totals["candidate_encoded_bytes"],5)
    totals["potential_combined_web_bytes"]=totals["stable_encoded_bytes"]+totals["compressed_xor_bytes"]
    totals["candidate_requires_untrusted_new_JS"]=True
    totals["candidate_not_live"]=True
    totals["iphone_stability_tested"]=False
    print("VERIFIED_CHUNK_XOR_REUSE_TOTALS",json.dumps(totals,sort_keys=True),flush=True)
    return {"sourceStable":STABLE_SHA,"sourceCandidate":CANDIDATE_SHA,"parts":items,"totals":totals}

if __name__=="__main__":
    p=argparse.ArgumentParser()
    p.add_argument("--stable-pages-artifact-tar",type=Path,required=True)
    p.add_argument("--candidate-root",type=Path,required=True)
    p.add_argument("--output",type=Path,required=True)
    a=p.parse_args()
    result=analyze(a.stable_pages_artifact_tar,a.candidate_root)
    a.output.write_text(json.dumps(result,indent=2)+"\n")
