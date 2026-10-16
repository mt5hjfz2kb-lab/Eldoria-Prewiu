"""Read-only recovery of frozen native comparison assets into the study."""
import os,pathlib,shutil,json
root=pathlib.Path.cwd();out=root/"Unity/Assets/Eldoria/ArtTests/SceneBreakthrough/FrozenBaseline"
out.mkdir(parents=True,exist_ok=True);temp=pathlib.Path(os.environ["RUNNER_TEMP"])
base=temp/"breakthrough-original";candidate=temp/"breakthrough-baseline"
generated=list(base.rglob("Generated"))
found=[p for p in generated if (p.parent/"Bastion1ReferenceAssembly.unity").exists()]
if len(found)!=1:
 found=[p for p in generated if "Bastion1Assembly" in p.as_posix()]
if len(found)!=1:raise RuntimeError("Frozen original Generated directory ambiguous "+str(found))
original=found[0].parent
for name in ["Generated","GeneratedNormals"]:
 src=original/name
 if src.exists():shutil.copytree(src,out/"Original"/name,dirs_exist_ok=True)
candidates=list(candidate.rglob("Bastion1Modular63Candidate.unity"))
if len(candidates)!=1:raise RuntimeError("Exact frozen candidate scene absent")
shutil.copy2(candidates[0],out/"FrozenCandidate.unity")
for item in candidate.rglob("GeneratedModular63"):
 if item.is_dir():shutil.copytree(item,out/"GeneratedModular63",dirs_exist_ok=True)
(root/"docs/evidence/valoria-scene-breakthrough-20261016/frozen-baseline-lineage.json").write_text(json.dumps({"original_run":38419031491,"candidate_run":38940426182,"candidate_artifact":12062799612,"protected_branch_modified":False,"normalization":"reversible uniform source-frame normalization in a NEW review scene, original PNG and source retained"},indent=2))
