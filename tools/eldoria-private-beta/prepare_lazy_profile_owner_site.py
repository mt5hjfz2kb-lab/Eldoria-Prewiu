#!/usr/bin/env python3
"""Candidate-only reuse of the exact existing mobile6 texture profile.
Never rebuilds Unity, publishes, changes gameplay objects or grants testers access.
"""
import argparse,gc,gzip,hashlib,json,struct
from pathlib import Path
from types import SimpleNamespace
import prepare_structural_site as structural
import prepare_lazy_owner_site as lazy
import deduplicate_exact_textures as dedup
import reuse_mobile6_profile as profile

def digest(path):
    with Path(path).open("rb") as f:return hashlib.file_digest(f,"sha256").hexdigest()
def extract_reference(root,output):
    root=Path(root);manifest=json.loads((root/"build-manifest.json").read_text())
    assembled=Path(output).with_suffix(".data.bin")
    with assembled.open("xb") as dst:
        for part in manifest["decoded_parts"][manifest["data"]]:
            block=gzip.decompress((root/part["url"]).read_bytes())
            lazy.require(len(block)==part["decoded_bytes"] and
                hashlib.sha256(block).hexdigest()==part["decoded_sha256"],
                "Reference mobile6 chunk changed")
            dst.write(block)
    with assembled.open("rb") as src:
        lazy.require(src.read(16)==b"UnityWebData1.0\0","Invalid reference web data")
        end=struct.unpack("<I",src.read(4))[0];header=src.read(end-20)
        pos=0;found=[]
        while pos<len(header):
            offset,size,n=struct.unpack_from("<III",header,pos);pos+=12
            name=header[pos:pos+n];pos+=n
            if name==b"data.unity3d":found.append((offset,size))
        lazy.require(pos==len(header) and len(found)==1,"Reference bundle membership mismatch")
        offset,size=found[0];src.seek(offset)
        with Path(output).open("xb") as dst:
            remaining=size
            while remaining:
                block=src.read(min(16*1024*1024,remaining))
                lazy.require(bool(block),"Reference bundle truncated")
                dst.write(block);remaining-=len(block)
    lazy.require(digest(output)==profile.REFERENCE,"Wrong exact mobile6 texture profile")
    assembled.unlink()

def run(a):
    lazy.require(a.revision=="native325-mobile16-full-lazy-profile-owner-qa","Unexpected candidate label")
    work=Path(a.work);work.mkdir(parents=True,exist_ok=False)
    native=lazy.sole_dir(a.native_artifact,"comparison.json",(
        "warm-cache-restoration.json","source-overlay.json","ux-source-overlay.json",
        "web-build.json","native-journey/integration-result.json"))
    play=lazy.sole_dir(a.playmode_artifact,"lazy-world-native-result.json")
    build=lazy.sole_dir(a.webgl_artifact,"index.html",("Build",))
    shell=lazy.sole_dir(a.beta14_shell_artifact,"release-identity.json",
        ("index.html","style.css","city.png","play.js"))
    lazy.verify_source_proofs(native,play,a.source_head)
    shell_hashes=lazy.verify_beta14_shell(shell)
    parent=lazy.sole_dir(a.reference_parent,"data.unity3d")/"data.unity3d"
    lazy.require(digest(parent)==profile.SOURCE,"Wrong reference parent")
    reference=work/"mobile6.unity3d"
    extract_reference(a.reference_site,reference)
    original=work/"original";structural.extract(build,original)
    original_sha=digest(original/"data.unity3d")
    exact=work/"deduplicated"
    dedup.apply(original/"data.unity3d",exact,original_sha)
    exact_proof=json.loads((exact/"integrity.json").read_text())
    transformed=work/"profile"
    profile.run(SimpleNamespace(parent=str(parent),reference=str(reference),
        fresh=str(exact/"data.unity3d"),
        expected_fresh_sha256=exact_proof["candidate_sha256"],output=str(transformed)))
    p=json.loads((transformed/"integrity.json").read_text())
    lazy.require(p["source_sha256"]==exact_proof["candidate_sha256"] and
        p["all_objects_verified"]==exact_proof["all_objects_verified"] and
        p["exact_serialized_matches_only"] is True,"Broken lossless/proven-profile verification chain")
    p.update(source_sha256=original_sha,
        verification_chain=[original_sha,exact_proof["candidate_sha256"],p["candidate_sha256"]],
        removed_exact_texture_objects=exact_proof["removed_exact_texture_objects"],
        no_new_compression_decisions=True,reference_profile_preexisting=True)
    composed=transformed/"composed-integrity.json"
    composed.write_text(json.dumps(p,indent=2))
    gc.collect()
    structural.package(SimpleNamespace(build=str(build),wrapper=str(shell),
        output=a.output,data=str(original/"data.bin"),bundle=str(transformed/"data.unity3d"),
        texture_proof=str(composed),native_proof=str(native/"comparison.json"),
        web_proof=str(native/"web-build.json"),overlay_commit=a.source_head,
        revision=a.revision,native_run=a.native_run))
    out=Path(a.output);identity_path=out/"release-identity.json"
    identity=json.loads(identity_path.read_text())
    lazy.require(identity["source_sha"]==a.source_head and
        identity["candidate_bundle_sha256"]==p["candidate_sha256"],"Packaged identity mismatch")
    identity.update(source_composition="Actual native lazy Region1 plus frozen UX19; exact imported-texture deduplication and byte-identical pre-existing mobile6 profile transforms only; fresh WASM/framework unchanged",
        original_full_world_bundle_sha256=original_sha,full_region1_compiled=True,
        real_lazy_unity_playmode_passed=True,real_native_i_v_journey_passed=True,
        preexisting_mobile6_profile_reused=True,no_new_texture_compression_decisions=True,
        mac_same_binary_ab_verified=False,physical_iphone_30min_verified=False,
        physical_android_30min_verified=False,world_entry_physical_verified=False,
        runtime_verified=False,release_for_external_testers=False,
        owner_only=True,published=False,recommended_owner_opt_in="eldoria_p0_lazy_world=1")
    identity_path.write_text(json.dumps(identity,indent=2,ensure_ascii=False))
    summary=dict(source_head=a.source_head,native_run=a.native_run,revision=a.revision,
        original_bundle_sha256=original_sha,candidate_bundle_sha256=p["candidate_sha256"],
        exact_duplicate_textures_removed=exact_proof["removed_exact_texture_objects"],
        existing_profile_transforms=p["changed_textures"],
        original_decoded_bytes=json.loads((original/"extraction.json").read_text())["bytes"],
        candidate_decoded_bytes=sum(x.get("decoded_bytes",0) for x in identity["decoded_files"]),
        same_fresh_wasm_and_framework=True,no_new_compression_decisions=True,
        all_serialized_objects_verified=p["all_objects_verified"],
        frozen_beta14_shell_sha256=shell_hashes,art_approved=False,
        physical_iphone_verified=False,published=False)
    (out/"owner-only-profile-proof.json").write_text(json.dumps(summary,indent=2))
    print("FULL_LAZY_EXISTING_PROFILE_SERIALIZED_INTEGRITY_PASS_NOT_RUNTIME",json.dumps(summary))
if __name__=="__main__":
    parser=argparse.ArgumentParser()
    for key in ("native-artifact","playmode-artifact","webgl-artifact","beta14-shell-artifact",
        "reference-parent","reference-site","work","output","source-head","native-run","revision"):
        parser.add_argument("--"+key,required=True)
    run(parser.parse_args())
