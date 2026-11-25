"""Project productivity: independent quality decisions and measured runner occupancy."""
import argparse,datetime as dt,json,re,subprocess
from pathlib import Path
from intervention_report import summarize,read,write
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"
def seconds(start,end):
    if not start or not end:return None
    a,b=[dt.datetime.fromisoformat(x.replace("Z","+00:00")) for x in (start,end)]
    if a.tzinfo is None or b.tzinfo is None or b<a:raise ValueError("Bad runner timestamps")
    return (b-a).total_seconds()
def resources(runs):
    seen=set();total={"github_hosted_seconds":0,"self_hosted_windows_seconds":0,"other_runner_seconds":0,"unknown_duration_jobs":0}
    for r in runs:
        for j in r["jobs"]:
            key=(r["run_id"],j["job_id"])
            if key in seen:raise ValueError("Duplicate runner job")
            seen.add(key)
            if j.get("conclusion")=="skipped":continue
            t=seconds(j.get("started_at"),j.get("completed_at"))
            if t is None:total["unknown_duration_jobs"]+=1;continue
            labels=j.get("labels",[])
            field="self_hosted_windows_seconds" if "self-hosted" in labels and "windows" in labels else "github_hosted_seconds" if any(x.startswith(("ubuntu-","windows-","macos-")) for x in labels) and "self-hosted" not in labels else "other_runner_seconds"
            total[field]+=t
    return {**total,"github_billed_usd":None,"pc_electricity_usd":None,"scope":"Selected jobs only; occupancy, not billed duration; no electricity or financial extrapolation."}
def apply_decisions(records, decisions):
    by={r["run_id"]:dict(r) for r in records}
    if len(by)!=len(records):raise ValueError("Duplicate run")
    seen=set()
    for d in decisions:
        rid=d["run_id"]
        if rid not in by or rid in seen:raise ValueError("Unknown or duplicate decision")
        if d["decision"] not in ("accepted","rejected","technical_only"):raise ValueError("Invalid decision")
        if type(d.get("evidence_run")) is not int or d["evidence_run"]<=0 or not isinstance(d.get("criterion"),str) or not d["criterion"].strip():raise ValueError("Evidence and criterion required")
        for k in ("quality_score_before","quality_score_after"):
            v=d.get(k)
            if v is not None and (type(v) not in (int,float) or not 0<=v<=10):raise ValueError("Invalid quality score")
        seen.add(rid)
        by[rid].update(quality_decision=d["decision"],quality_evidence_run=d["evidence_run"],quality_score_before=d.get("quality_score_before"),quality_score_after=d.get("quality_score_after"))
    return list(by.values())
def snapshot(run_id):
    data=json.loads(subprocess.check_output(["gh","api",f"repos/{REPO}/actions/runs/{run_id}/jobs?per_page=100"],timeout=30))
    if data["total_count"]>100:raise ValueError("Incomplete pagination")
    jobs=[{"job_id":x["id"],"name":x["name"],"labels":x["labels"],"started_at":x["started_at"],"completed_at":x["completed_at"],"conclusion":x["conclusion"]} for x in data["jobs"] if x["name"]!="archive"]
    return {"run_id":run_id,"jobs":jobs}
def dashboard(root,decisions=None):
    root=Path(root);receipts=[read(p) for p in sorted((root/"receipts").glob("run-*.json"))]
    ds=read(Path(decisions))["decisions"] if decisions else []
    result=summarize(apply_decisions(receipts,ds))
    runs=read(root/"infra-baseline.json")["runs"]
    snapshots=[read(p) for p in sorted((root/"receipts").glob("infra-run-*.json"))]
    ids={r["run_id"] for r in runs}
    result["runner_usage"]=resources(runs+[r for r in snapshots if r["run_id"] not in ids])
    result["quality_method"]="Only evidence-backed review decisions; no fabricated numeric score or hours saved."
    return result
if __name__=="__main__":
    p=argparse.ArgumentParser();sp=p.add_subparsers(dest="cmd",required=True)
    a=sp.add_parser("snapshot");a.add_argument("--run",type=int,required=True);a.add_argument("--out",required=True)
    b=sp.add_parser("dashboard");b.add_argument("--root",required=True);b.add_argument("--decisions");b.add_argument("--out",required=True)
    x=p.parse_args()
    r=snapshot(x.run) if x.cmd=="snapshot" else dashboard(x.root,x.decisions)
    write(x.out,r);print(json.dumps(r,indent=2))
