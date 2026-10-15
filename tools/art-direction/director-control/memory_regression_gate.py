"""Evaluate paired director results; never authorize artistic edits."""
import argparse,json,pathlib,sys
p=argparse.ArgumentParser()
p.add_argument("--input",required=True)
p.add_argument("--out",required=True)
a=p.parse_args()
d=json.loads(pathlib.Path(a.input).read_text(encoding="utf-8"))
required={"baseline_correct","memory_correct","gain_count","regression_count","case_count","pairs"}
if not required.issubset(d):raise SystemExit("MISSING_AB_EVIDENCE")
pairs=d["pairs"]
if len(pairs)!=d["case_count"] or len({x["id"] for x in pairs})!=len(pairs):raise SystemExit("INVALID_PAIRS")
baseline=sum(bool(x["baseline_correct"]) for x in pairs)
memory=sum(bool(x["memory_correct"]) for x in pairs)
gains=sum(not x["baseline_correct"] and x["memory_correct"] for x in pairs)
regressions=sum(x["baseline_correct"] and not x["memory_correct"] for x in pairs)
if (baseline,memory,gains,regressions)!=(d["baseline_correct"],d["memory_correct"],d["gain_count"],d["regression_count"]):raise SystemExit("INCONSISTENT_COUNTS")
if d.get("art_approved") is not False or d.get("any_blender_edits") is not False:raise SystemExit("UNAUTHORIZED_EDIT_OR_APPROVAL")
verdict="REGRESSION" if regressions else ("IMPROVED_FIXED_SAMPLE" if gains else "NO_GAIN")
result={"status":verdict,"cases":len(pairs),"baseline":baseline,"memory":memory,"gains":gains,"regressions":regressions,"art_approved":False,"can_edit_blender":False}
pathlib.Path(a.out).parent.mkdir(parents=True,exist_ok=True)
pathlib.Path(a.out).write_text(json.dumps(result,indent=2)+"\n",encoding="utf-8")
print("ELDORIA_MEMORY_GATE",verdict)
sys.exit(2 if regressions else 0)
