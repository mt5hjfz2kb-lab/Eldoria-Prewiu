"""Reuse only byte-identical, previously verified native325 texture transformations.
No new compression decisions. Reject unexpected metadata/reference changes.
This verifies serialized integrity only; it does not certify rendering or iPhone stability.
"""
import argparse, gc, hashlib, json
from pathlib import Path
import UnityPy

ORIGINAL = "6efe4b1f1eeedd33727ec8f24d4d25075b516478d3bb80ea61a6bdbb12528ca0"
VERIFIED = "60b43335bb263568dd366faca06ab0d648506dac31024342b1d4edefd8a7c315"
ALLOWED = {"m_IsReadable", "m_TextureFormat", "m_CompleteImageSize", "image data"}

def digest(data):
    return hashlib.sha256(data).hexdigest()

def file_hash(path):
    with open(path, "rb") as f:
        return hashlib.file_digest(f, "sha256").hexdigest()

def key(obj):
    return obj.assets_file.name + ":" + str(obj.path_id)

def semantic_key(tree):
    # Names identify separate imported copies; readability is handled separately.
    # Every pixel, mip and all remaining sampler/color/shape metadata must match.
    metadata = {k:v for k,v in tree.items() if k not in {"m_Name", "m_IsReadable", "image data"}}
    encoded = json.dumps(metadata,sort_keys=True,separators=(",",":"),default=lambda v:bytes(v).hex()).encode()
    return digest(encoded+b"\0"+bytes(tree["image data"]))

def catalog(original, verified):
    assert file_hash(original) == ORIGINAL, "Wrong original frozen bundle"
    assert file_hash(verified) == VERIFIED, "Wrong previously verified texture bundle"
    source = UnityPy.load(str(original))
    candidates = UnityPy.load(str(verified))
    old = {key(o): o for o in source.objects if o.type.name == "Texture2D"}
    transforms = {}
    semantic = {}
    for obj in candidates.objects:
        if obj.type.name != "Texture2D":
            continue
        before = old[key(obj)]
        a, b = before.get_raw_data(), obj.get_raw_data()
        if a == b:
            continue
        ta, tb = before.read_typetree(), obj.read_typetree()
        assert ta.keys() == tb.keys()
        differences = {k for k in ta if ta[k] != tb[k]}
        assert differences <= ALLOWED, (key(obj), differences)
        assert ta.get("m_StreamData", {}).get("size", 0) == 0
        assert tb.get("m_StreamData", {}).get("size", 0) == 0
        if "m_IsReadable" in differences:
            assert ta["m_IsReadable"] and not tb["m_IsReadable"]
        if "m_TextureFormat" in differences:
            assert ta["m_TextureFormat"] in (3, 4, 5) and tb["m_TextureFormat"] == 48
        assert before.version == obj.version
        record = {"raw": b, "hash": digest(b), "name": ta["m_Name"],
                  "version": list(obj.version), "fields": sorted(differences),
                  "before_bytes": len(a), "after_bytes": len(b)}
        previous = transforms.get(digest(a))
        assert previous is None or previous["hash"] == record["hash"], "Ambiguous transform"
        transforms[digest(a)] = record
        sk = semantic_key(ta)
        patch = {k:tb[k] for k in ("m_TextureFormat","m_CompleteImageSize","image data")}
        if sk in semantic:
            assert semantic[sk]["patch"] == patch, "Ambiguous semantic texture transform"
        semantic[sk] = {"patch":patch,"reference":digest(a),"record":record}
    assert transforms
    del source, candidates, old
    gc.collect()
    return transforms, semantic

def apply(original, verified, fresh, expected_hash, destination):
    assert file_hash(fresh) == expected_hash, "Unexpected fresh source"
    destination = Path(destination)
    destination.mkdir(parents=True, exist_ok=False)
    transforms, semantic = catalog(original, verified)
    print('Verified texture catalog',len(transforms),'exact',len(semantic),'semantic',flush=True)
    env = UnityPy.load(str(fresh))
    before, changed, reports, used = {}, {}, [], set()
    semantic_expected = {}
    for obj in env.objects:
        data = obj.get_raw_data()
        h = digest(data)
        before[key(obj)] = h
        if obj.type.name != "Texture2D":
            continue
        match = "exact"
        tree = None
        if h not in transforms:
            tree = obj.read_typetree()
            # Do not alter new CPU-readable usages through semantic matching.
            candidate = semantic.get(semantic_key(tree)) if not tree.get("m_IsReadable",True) else None
            if candidate is None:
                continue
            transform = candidate["record"]
            reference = candidate["reference"]
            match = "identical pixels and all metadata except name/readability"
        else:
            transform = transforms[h]
            reference = h
        assert list(obj.version) == transform["version"], "Serialization version changed"
        if tree is None:
            obj.set_raw_data(transform["raw"])
        else:
            updated = dict(tree)
            updated.update(candidate["patch"])
            assert {k for k in tree if tree[k] != updated[k]} <= {"m_TextureFormat","m_CompleteImageSize","image data"}
            obj.save_typetree(updated)
            # UnityPy 1.25.4 read_typetree/get_raw_data still read the original
            # backing stream until the bundle is saved and reopened.
            semantic_expected[key(obj)] = ({k:v for k,v in updated.items() if k != "image data"},digest(bytes(updated["image data"])))
        changed[key(obj)] = digest(obj.data)
        used.add(reference)
        reports.append({k: v for k, v in transform.items() if k != "raw"} | {"object": key(obj), "source_hash": h, "matching":match, "reference_source_hash":reference})
    assert changed, "No byte-identical verified texture transformations found"
    print("TRANSFORMS_STAGED",len(changed),"semantic",len(semantic_expected),
          "reference_payload_savings",sum(r["before_bytes"]-r["after_bytes"] for r in reports),flush=True)
    output = destination / "data.unity3d"
    output.write_bytes(env.file.save(packer="lz4"))
    del env
    gc.collect()
    check = UnityPy.load(str(output))
    after = {key(o): digest(o.get_raw_data()) for o in check.objects}
    assert before.keys() == after.keys(), "Object graph membership changed"
    for k, h in after.items():
        assert h == changed.get(k, before[k]), ("Unexpected object mutation", k)
    for obj in check.objects:
        if key(obj) in semantic_expected:
            expected_meta, expected_pixels = semantic_expected[key(obj)]
            tree = obj.read_typetree()
            assert {k:v for k,v in tree.items() if k != "image data"} == expected_meta, ("Saved texture metadata mismatch",key(obj))
            assert digest(bytes(tree["image data"])) == expected_pixels, ("Saved texture payload mismatch",key(obj))
    proof = {
        "source_sha256": expected_hash, "candidate_sha256": file_hash(output),
        "reference_source_sha256": ORIGINAL, "reference_candidate_sha256": VERIFIED,
        "all_objects_verified": len(before), "changed_textures": len(changed),
        "verified_transforms_available": len(transforms),
        "verified_transforms_reused": len(used),
        "unmatched_transforms": sorted(set(transforms) - used),
        "texture_changes": reports, "runtime_verified": False,
        "physical_iphone_verified": False, "release_for_external_testers": False
    }
    (destination / "integrity.json").write_text(json.dumps(proof, indent=2))
    print(json.dumps({k:v for k,v in proof.items() if k != "texture_changes"}), flush=True)

if __name__ == "__main__":
    p = argparse.ArgumentParser()
    p.add_argument("--original", required=True)
    p.add_argument("--verified", required=True)
    p.add_argument("--fresh", required=True)
    p.add_argument("--expected-fresh-sha256", required=True)
    p.add_argument("--destination", required=True)
    a = p.parse_args()
    apply(a.original, a.verified, a.fresh, a.expected_fresh_sha256, a.destination)
