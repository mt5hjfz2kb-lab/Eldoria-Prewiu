"""Connect grounded 14B director into the independent evidence gate in diagnostic mode.
Only NO_EDIT is currently authorized; this deliberately cannot produce Blender actions.
"""
import argparse,hashlib,json,pathlib
p=argparse.ArgumentParser()
p.add_argument("--grounding",required=True)
p.add_argument("--observation",required=True)
p.add_argument("--director",required=True)
p.add_argument("--out",required=True)
a=p.parse_args()
files={key:pathlib.Path(getattr(a,key)) for key in ("grounding","observation","director")}
docs={}
for name,path in files.items():
    docs[name]=json.loads(path.read_text(encoding="utf-8-sig"))
EXPECTED="e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"
for key in ("grounding","observation","director"):
    if docs[key].get("source_sha256",docs[key].get("original_sha256"))!=EXPECTED:
        raise RuntimeError("SOURCE_MISMATCH_"+key)
director=docs["director"]
if director.get("model")!="qwen3:14b" or not director.get("done"):
    raise RuntimeError("INCOMPLETE_14B_DIRECTOR")
if director.get("art_approved") or director.get("any_geometry_edited"):
    raise RuntimeError("UNAUTHORIZED_EDIT_OR_APPROVAL_IN_DIAGNOSIS")
text=director.get("text","")
if "NO_EDIT" not in text.upper() or len(text)<350:
    raise RuntimeError("DIRECTOR_MUST_EXPLICITLY_ABSTAIN_UNTIL_VERIFIED")
proposal={
    "schema_version":1,"original_sha256":EXPECTED,
    "mode":"DIAGNOSTIC_ONLY","art_approved":False,"editor_actions":[],
    "proposals":[],
    "model":"qwen3:14b",
    "director_answer_sha256":hashlib.sha256(text.encode()).hexdigest(),
    "data_sha256":{key:hashlib.sha256(path.read_bytes()).hexdigest()
                   for key,path in files.items()},
    "decision":"NO_EDIT_INSUFFICIENT_EVIDENCE",
    "reason":"World-space directions do not certify exact roof structure or contacts; no asset modification is authorized.",
    "independent_visual_review":"STILL_REQUIRED_FOR_ANY_FUTURE_ART_ACTION"
}
out=pathlib.Path(a.out);out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(proposal,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("ELDORIA_STRONG_DIRECTOR_SAFE_HANDOFF_NO_EDIT",proposal["director_answer_sha256"])
