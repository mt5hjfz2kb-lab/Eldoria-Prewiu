#!/usr/bin/env python3
"""Build a small separate iPhone A/B on GitHub Pages without replacing beta10.

Only the newly compiled candidate .data chunks are replaced by tiny, exact
VCDIFF deltas over the *immutable* beta10 chunks already on the same site.
Every reconstructed byte is SHA-verified before staging. No save files or
localStorage are touched, and this stage never authorizes external testers.
"""
import argparse
import gzip
import hashlib
import json
import shutil
import subprocess
import tarfile
import tempfile
from pathlib import Path,PurePosixPath
from stage_isolated_iphone_preview import (
    STABLE_ROUTE,CANDIDATE_ROUTE,STABLE_REVISION,STABLE_SOURCE,STABLE_BUNDLE,
    check_binary_site,fingerprint_tree,files
)

NEW_REV="native325-mobile12-minimal-world-clock-qa"
NEW_SOURCE="004d3ef9c26e9070762e9e38ce84d69d525efd4f"
LIMIT=650_000_000

def need(condition,message):
    if not condition:raise RuntimeError(message)
def safe(value):
    p=PurePosixPath(value.split("?",1)[0])
    need(bool(p.parts) and not p.is_absolute() and ".." not in p.parts
         and "\\" not in value and ":" not in value,"Unsafe part URL: "+str(value))
    return p.as_posix()
def digest(data):return hashlib.sha256(data).hexdigest()
def decoded(binary,item):
    data=gzip.decompress(binary)
    need(len(data)==item["decoded_bytes"] and digest(data)==item["decoded_sha256"],
         "A streamed original or candidate chunk is corrupted")
    return data
def restore_tar(tarpath,output):
    need(not output.exists(),"Never overwrite existing old Pages directory")
    with tarfile.open(tarpath,"r:*") as archive:
        for m in archive:
            p=PurePosixPath(m.name)
            need(not p.is_absolute() and ".." not in p.parts and "\\" not in m.name,
                 "Unsafe archived path")
            if m.isdir():continue
            need(m.isfile(),"Non-regular old site member")
            dest=output.joinpath(*p.parts)
            dest.parent.mkdir(parents=True,exist_ok=True)
            with archive.extractfile(m) as src,dest.open("wb") as dst:
                shutil.copyfileobj(src,dst,1024*1024)
    need((output/STABLE_ROUTE/"release-identity.json").is_file(),
         "Archived Pages is not the stable game")
def exact_parts(root,man):
    data=man["data"]
    parts=man.get("decoded_parts",{}).get(data)
    need(isinstance(parts,list) and len(parts)==31 and
         all(p.get("encoding")=="gzip" and p.get("decoded_sha256") and
             p.get("decoded_bytes",0)>0 for p in parts),
         "Exact 31-part WebGL data manifest required")
    for p in parts: need((root/safe(p["url"])).is_file(),
                         "Source chunk missing: "+p["url"])
    return parts

def stage(stable_archive,candidate_path,output,decoder_bundle,decoder_license,
          transport_source,max_bytes=LIMIT):
    candidate=Path(candidate_path).resolve()
    need(candidate.is_dir() and not output.exists(),
         "Candidate missing or refusing to replace existing build")
    before_meta=json.loads((candidate/"build-manifest.json").read_text(encoding="utf-8-sig"))
    before_identity=json.loads((candidate/"release-identity.json").read_text(encoding="utf-8-sig"))
    need(before_identity["revision"]==NEW_REV and before_identity["source_sha"]==NEW_SOURCE,
         "Not the exact Unity-certified isolated clock candidate")
    need(before_identity["physical_mobile_tested"] is False and
         before_identity["release_for_external_testers"] is False,
         "Do not promote a phone-unverified candidate as a release")
    need(check_binary_site(candidate,NEW_REV,NEW_SOURCE,
                           before_identity["candidate_bundle_sha256"],True)==before_identity,
         "Full candidate integrity not certified")
    new_parts=exact_parts(candidate,before_meta)

    with tempfile.TemporaryDirectory(prefix="eldoria-compact-delta-") as tmp:
        tmp=Path(tmp)
        old_root=tmp/"old"
        restore_tar(stable_archive,old_root)
        stable=old_root/STABLE_ROUTE
        old_identity=check_binary_site(stable,STABLE_REVISION,STABLE_SOURCE,STABLE_BUNDLE,False)
        old_parts=exact_parts(stable,json.loads((stable/"build-manifest.json").read_text(encoding="utf-8-sig")))
        original_tree=fingerprint_tree(old_root)
        staged=tmp/"staged"
        shutil.copytree(old_root,staged)
        dest=staged/CANDIDATE_ROUTE
        exclude={safe(p["url"]) for p in new_parts}
        dest.mkdir()
        for item in files(candidate):
            relative=item.relative_to(candidate).as_posix()
            if relative in exclude:continue
            where=dest/relative
            where.parent.mkdir(parents=True,exist_ok=True)
            shutil.copyfile(item,where)
        patch_dir=dest/"delta-patches"
        patch_dir.mkdir()
        raw_old=tmp/"stable.part"
        raw_new=tmp/"candidate.part"
        patch=tmp/"candidate.vcdiff"
        recovered=tmp/"reconstructed.part"
        patched_parts=[]
        total_patch_size=0
        for index,(a,b) in enumerate(zip(old_parts,new_parts)):
            first=decoded((stable/safe(a["url"])).read_bytes(),a)
            second=decoded((candidate/safe(b["url"])).read_bytes(),b)
            raw_old.write_bytes(first)
            raw_new.write_bytes(second)
            subprocess.run(["xdelta3","-f","-e","-S","none","-A=",
                            "-s",str(raw_old),str(raw_new),str(patch)],
                           check=True,stdout=subprocess.PIPE,stderr=subprocess.PIPE)
            subprocess.run(["xdelta3","-f","-d","-s",str(raw_old),str(patch),str(recovered)],
                           check=True,stdout=subprocess.PIPE,stderr=subprocess.PIPE)
            need(digest(recovered.read_bytes())==b["decoded_sha256"],
                 "VCDIFF reconstructed bytes differ from compiled Unity "+str(index))
            payload=patch.read_bytes()
            need(len(payload)>0 and len(payload)<3_000_000,
                 "Patch unexpectedly large, refuse mobile preview "+str(index))
            patch_rel="delta-patches/part-%02d.vcdiff"%index
            (dest/patch_rel).write_bytes(payload)
            total_patch_size+=len(payload)
            patched_parts.append({
                "index":index,
                "base_url":"../"+STABLE_ROUTE+"/"+safe(a["url"]),
                "patch_url":patch_rel,
                "patch_sha256":digest(payload),
                "decoded_sha256":b["decoded_sha256"],
                "decoded_bytes":b["decoded_bytes"]
            })
        need(total_patch_size<20_000_000,"Delta exceeds safe small-mobile limit")
        man=dict(before_meta)
        man["clock_delta_parts"]=patched_parts
        man["clock_delta_source_sha"]=STABLE_SOURCE
        man["clock_delta_candidate_sha"]=NEW_SOURCE
        (dest/"build-manifest.json").write_text(json.dumps(man,indent=2)+"\n")
        # The existing Unity bootstrap allocates the final .data buffer once.
        # Override only that fetch with a 31-part verified delta stream.
        boot=dest/"play.js"
        js=boot.read_text(encoding="utf-8")
        needle="installExactDecodedTransport(build.decoded_sizes,build.decoded_parts);"
        need(js.count(needle)==1 and "installEldoriaClockDeltaTransport" not in js,
             "Unexpected Unity bootstrap: refuse heuristic modifications")
        js=js.replace(needle,needle+"\n"+
              "installEldoriaClockDeltaTransport(build.data,build.clock_delta_parts,build.decoded_sizes[build.data]);")
        boot.write_text(js,encoding="utf-8")
        html=dest/"index.html"
        markup=html.read_text(encoding="utf-8")
        need(markup.count("<head>")==1 and "clock_delta_transport.js" not in markup,
             "Unexpected candidate HTML: fail closed")
        markup=markup.replace("<head>","<head>\n"+
            '<script src="./vcdiff-browser.bundle.js"></script>\n'+
            '<script src="./clock_delta_transport.js"></script>\n',1)
        html.write_text(markup,encoding="utf-8")
        require=[(decoder_bundle,"vcdiff-browser.bundle.js"),
                 (decoder_license,"vcdiff-decoder.APACHE-2.0.txt"),
                 (transport_source,"clock_delta_transport.js")]
        for source,target in require:
            src=Path(source)
            need(src.is_file() and 100<src.stat().st_size<200_000,
                 "Untrusted or missing local VCDIFF source "+str(src))
            shutil.copyfile(src,dest/target)

        # Preserve EVERY previously published beta10 file, legacy redirect and
        # root byte-for-byte. Do not touch browser storage.
        need(fingerprint_tree(staged)=={
            **original_tree,
            **{k:v for k,v in fingerprint_tree(staged).items() if k.startswith(CANDIDATE_ROUTE+"/")}
        },"A prior stable route changed unexpectedly")
        for name,(size,expected_sha) in original_tree.items():
            original=staged/name
            need(original.stat().st_size==size and digest(original.read_bytes())==expected_sha,
                 "Original owner playable beta10 changed: "+name)
        need(json.loads((staged/STABLE_ROUTE/"release-identity.json").read_text())==old_identity,
             "Identity of previously playable beta10 changed")
        size=sum(p.stat().st_size for p in files(staged))
        need(0<size<max_bytes,"Combined delta site exceeds safe Pages budget")
        proof={"schema":1,"role":"OWNER_CLOCK_AB_IOS_QA_ONLY",
            "external_testers_enabled":False,"physical_iphone_stability_verified":False,
            "stable_route":STABLE_ROUTE+"/","stable_source_sha":STABLE_SOURCE,
            "candidate_route":CANDIDATE_ROUTE+"/","candidate_source_sha":NEW_SOURCE,
            "candidate_revision":NEW_REV,"candidate_bundle_sha256":before_identity["candidate_bundle_sha256"],
            "reconstruction":"VCDIFF verified 31-chunk, SHA256 each segment",
            "delta_patch_bytes":total_patch_size,"combined_bytes":size,
            "old_files_sha256_unchanged":len(original_tree),
            "client_runtime":"@ably/vcdiff-decoder@1.0.6 Apache-2.0 bundled locally",
            "browser_save_mutation":False,"qa_status":"not physically tested on iPhone"}
        (staged/"beta-iphone-clock-ab-proof.json").write_text(json.dumps(proof,indent=2)+"\n")
        staged.rename(output)
    print("ELDORIA_P0_EXACT_COMPACT_AB_STAGE_PASS",json.dumps(proof,sort_keys=True),flush=True)
    return proof

if __name__=="__main__":
    parser=argparse.ArgumentParser()
    parser.add_argument("--stable-pages-artifact",required=True,type=Path)
    parser.add_argument("--candidate-site",required=True,type=Path)
    parser.add_argument("--output-site",required=True,type=Path)
    parser.add_argument("--decoder-bundle",required=True,type=Path)
    parser.add_argument("--decoder-license",required=True,type=Path)
    parser.add_argument("--transport-source",required=True,type=Path)
    parser.add_argument("--max-bytes",type=int,default=LIMIT)
    args=parser.parse_args()
    stage(args.stable_pages_artifact,args.candidate_site,args.output_site,
          args.decoder_bundle,args.decoder_license,args.transport_source,args.max_bytes)
