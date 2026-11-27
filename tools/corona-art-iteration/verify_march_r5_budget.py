"""Verify completed additional-budget receipts, never infer invoice settlement.
Keep the full original $5 held. Reconcile only the owner-authorized additional
$2 after all 11 calls completed, retaining $1 conservative exposure.
No historical ledger is altered. New R5 review reserve $0.24.
"""
from pathlib import Path
from decimal import Decimal as D
import json
EXPECTED=[
("artist",45866050061,15140472209,"6d58a4064ca8cb56b76d3588bf9d6a645ff6f116",D(".87"),["ledger.json"]),
("direction",45882958150,15148881104,"296ea6a5730e083f3322dcd39a59560c1576697f",D(".20"),["ledger.json"]),
("r3",45900917700,15156298587,"0aa89f7dda367ba3afc3a666a8c0574198ad9c8f",D(".30"),["review-1/ledger.json","review-2/ledger.json"]),
("r4-production",45907960440,15159060997,"5217126f5952820162d40d316b2068bd5ae2ba96",D(".39"),["ledger.json"]),
("r4-review",45956670942,15179553857,"512db0837453d5a50e1f5d7db6e98f87fb0d36e1",D(".24"),["review-1/ledger.json","review-2/ledger.json"])]
SKIPPED=[(45883366271,"independent-r3-audit"),(45892797979,"independent-r3-audit"),(45908381212,"independent-r4-audit"),(45908730757,"independent-r4-audit")]
def need(ok,msg):
    if not ok:raise ValueError(msg)
def read(p):return json.loads(p.read_text(encoding="utf-8"))
def verify_budget(repo,gh):
    total=uncached_max=held=D(0);count=0;rows=[]
    for folder,run,artifact,sha,expected,ledgers in EXPECTED:
        r=gh(f"repos/{repo}/actions/runs/{run}")
        need(r["head_sha"]==sha and r["conclusion"]=="success" and r["run_attempt"]==1,"Unknown paid run "+folder)
        need(any(x["id"]==artifact and not x["expired"] for x in gh(f"repos/{repo}/actions/runs/{run}/artifacts")["artifacts"]),"Unknown paid artifact "+folder)
        path=Path("receipts")/folder;sub=D(0)
        need({str(p.relative_to(path)).replace("\\","/") for p in path.rglob("ledger.json")}==set(ledgers),"Unexpected/missing ledger "+folder)
        for n in ledgers:
            p=path/n;l=read(p);reserved=D(0)
            for x in l["reservations"]:
                a=read(p.parent/(x["role"]+".json"))
                v=D(x["estimated_usd"]);res=D(x["reserve_usd"]);u=D(str(a.get("estimated_usd_standard_uncached",v)))
                actual=D(str(a.get("cost_estimate_usd",a.get("estimated_usd"))))
                need(x["state"]=="completed" and a["usage"] is not None and actual==v and v.is_finite() and u.is_finite() and D(0)<=v<=res and u>=0,"Incomplete paid receipt")
                reserved+=res;total+=v;uncached_max+=max(v,u);count+=1
                rows.append({"run":run,"artifact":artifact,"role":x["role"],"estimated_usd":str(v),"conservative_standard_estimate":str(max(v,u)),"historical_reserve":str(res)})
            need(reserved==D(l["cap_usd"])==D(l["reserved_usd"]),"Ledger reserve mismatch")
            sub+=reserved
        need(sub==expected,"Historical reservation mismatch")
        held+=sub
    for run,name in SKIPPED:
        r=gh(f"repos/{repo}/actions/runs/{run}")
        need(r["run_attempt"]==1 and r["status"]=="completed","Unexpected prior retry")
        need(next(x for x in gh(f"repos/{repo}/actions/runs/{run}/jobs")["jobs"] if x["name"]==name)["conclusion"]=="skipped","Prior paid job not skipped")
    need(count==11 and held==D("2") and total==D("0.120232125") and uncached_max==D("0.128002175"),"Prior receipt totals changed")
    need(uncached_max*4<D("1"),"Insufficient conservative buffer")
    need(D("5")+D("1")+D(".24")+D(".76")==D("7"),"Owner cap exceeded")
    return {"owner_cap_usd":"7.00","original_held_usd":"5.00","historical_additional_reserved_usd":"2.00","completed_additional_calls":count,"completed_additional_estimate_usd":str(total),"max_each_standard_or_actual_estimate_usd":str(uncached_max),"conservative_exposure_held_usd":"1.00","new_review_reserve_usd":".24","unallocated_usd":".76","invoice_verified":False,"historical_ledgers_unchanged":True,"no_automatic_retry":True,"receipts":rows}
