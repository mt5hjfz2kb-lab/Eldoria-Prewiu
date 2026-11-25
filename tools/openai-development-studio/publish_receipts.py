"""Publish only sanitized JSON receipts, never prompts, keys, images or CSV billing data."""
import argparse,base64,json,re,subprocess,time
from pathlib import Path
from intervention_report import summarize,read
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"
BRANCH="pilot/eldoria-openai-development-studio-20261125"
BASE="tools/openai-development-studio/receipts"
def publish(path):
    r=read(path)
    rid=r["run_id"]
    if type(rid) is not int or rid<=0:raise ValueError("Receipt identity")
    if path.name==f"infra-run-{rid}.json":
        from productivity import resources
        if set(r)!={"run_id","jobs"}:raise ValueError("Invalid resource schema")
        for job in r["jobs"]:
            if set(job)!={"job_id","name","labels","started_at","completed_at","conclusion"}:raise ValueError("Invalid resource job")
        resources([r])
    else:
        if path.name!=f"run-{rid}.json" or r["schema"]!=2:raise ValueError("Receipt identity")
        if r["billing_status"]!="unreconciled" or r["billed_usd"] is not None:raise ValueError("Unexpected billing information")
        if any("response" in c or "prompt" in c for c in r["calls"]):raise ValueError("Unexpected private content")
    raw=path.read_bytes()
    if len(raw)>100000:raise ValueError("Receipt too large")
    endpoint=f"repos/{REPO}/contents/{BASE}/{path.name}"
    payload=json.dumps({"message":f"Archive Studio intervention receipt {rid}","content":base64.b64encode(raw).decode(),"branch":BRANCH})
    for attempt in range(3):
        result=subprocess.run(["gh","api","--method","PUT",endpoint,"--input","-"],input=payload.encode(),stdout=subprocess.PIPE,stderr=subprocess.PIPE,timeout=30)
        if result.returncode==0:
            print("STUDIO_DURABLE_RECEIPT",rid);return
        existing=subprocess.run(["gh","api",endpoint+"?ref="+BRANCH],stdout=subprocess.PIPE,stderr=subprocess.PIPE,timeout=30)
        if existing.returncode==0:
            if base64.b64decode(json.loads(existing.stdout)["content"])==raw:
                print("STUDIO_DURABLE_RECEIPT_ALREADY_PRESENT",rid);return
            raise RuntimeError("Existing receipt differs; never overwrite")
        if b"409" not in result.stderr and b"422" not in result.stderr:raise RuntimeError("Receipt publication refused: "+result.stderr.decode()[:300])
        time.sleep(2+attempt)
    raise RuntimeError("Receipt publication failed; artifact retained")
if __name__=="__main__":
    p=argparse.ArgumentParser();p.add_argument("directory");a=p.parse_args()
    paths=sorted(Path(a.directory).glob("run-*.json"))+sorted(Path(a.directory).glob("infra-run-*.json"))
    if not paths:raise ValueError("No receipts")
    for f in paths:publish(f)
