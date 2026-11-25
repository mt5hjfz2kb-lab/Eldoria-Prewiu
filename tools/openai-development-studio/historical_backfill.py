"""Recover the original immutable pilot receipts using read-only GitHub token."""
import json,subprocess,tempfile
from pathlib import Path
from intervention_report import collect,write,markdown,summarize
from decimal import Decimal as D
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"
RUNS=[
 (45670034719,"connectivity-45670034719","eldoria-openai-smoke-45670034719","15bad5acaa909a38c59e351ca70e0a17caa5fd99",1,"not_applicable",None),
 (45670811229,"valoria-five-roles-45670811229","eldoria-studio-mission-45670811229","383b1574d6c4e7359a995c1d77a05b83fa8687e1",5,"rejected",45638738009),
 (45671428903,"valoria-mill-yard-v2","eldoria-studio-mission-45671428903","e23001a10b01e6cf765041f74818ebc541daeab4",5,"rejected",45638738009),
 (45676636310,"historic-b1-camera-review-20261125","studio-request-result-45676636310","e4418f79f32f5e67a876c9895a492f7db3c798ef",4,"technical_only",44713811420)]
def run(*a):return subprocess.check_output(a,timeout=120)
def main():
    out=Path("economics-backfill");out.mkdir(exist_ok=False)
    reports=[]
    for id,request,artifact,sha,n,decision,source in RUNS:
        data=json.loads(run("gh","api",f"repos/{REPO}/actions/runs/{id}"))
        if data["head_sha"]!=sha or data["status"]!="completed":raise ValueError("Historical identity mismatch")
        with tempfile.TemporaryDirectory() as d:
            run("gh","run","download",str(id),"--repo",REPO,"--name",artifact,"--dir",d)
            r=collect(d,id,request,data["run_started_at"],data["updated_at"],decision,data["conclusion"],source)
            if r["calls_recorded"]!=n or not r["estimate_complete"]:raise ValueError("Incomplete historical receipt")
            if id==45671428903:r["quality_evidence_run"]=45679533675
            elif decision in ("rejected","technical_only"):r["quality_evidence_run"]=id
            write(out/f"run-{id}.json",r);reports.append(r)
    s=summarize(reports)
    assert s["calls"]==15 and D(s["estimated_known_usd"])==D("0.18984755"),s
    write(out/"summary.json",s)
    print("STUDIO_HISTORICAL_COST_BACKFILL_PASS",json.dumps(s))
if __name__=="__main__":main()
