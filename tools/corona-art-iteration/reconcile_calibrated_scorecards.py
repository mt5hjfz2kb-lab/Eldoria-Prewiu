"""Read-only verification and reconciliation of real weighted art review artifacts.
Does not change the candidate, invoke the API, or promote art.
"""
import argparse, json, math, statistics, zipfile
from pathlib import Path
from decimal import Decimal as D

BASE="5ef2aee67cf8fde57ba2fc4b3d929b517ee67944"
PRIOR_UI="3486754d5f4c28db2ae4ffa2a7def3b0f000fb3c"
PRIOR_WORLD="ff38763376006ede276c875e498a8557d07509b3"
FINAL="41c124f4b9ef10cd9b4a9d87562a7d2727b730b6"
WEIGHTS={"resources":20,"integration":20,"language":15,"composition":15,"light_material":10,"mobile":10,"finish":10}
GROUPS={
    "hud":["hud-b1","hud-b5","tutorial","construction"],
    "management":["march","bag","mail","heroes","training"],
    "narrative":["name","daily","scouts","vigilants"],
    "auxiliary":["council-b1","council-b2","sawmill-built","bastion-upgrade","barracks-plot"],
    "city":["bastion-1","bastion-2","bastion-3","bastion-4","bastion-5","bastion-5-panorama"],
    "world-close":["home","wood","stone","woodland","river"],
    "world-strategic":["region","breach","panorama","breach-panorama"]}
TARGETS={"hud-b1":8.5,"hud-b5":8.5,"tutorial":8,"construction":8,"name":8.3,"march":8.3,"bag":8.5,"mail":8.3,"daily":8.5,"heroes":8.5,"training":8.5,"scouts":8.8,"vigilants":8.7}
ROLES={k for ks in GROUPS.values() for k in ks}
WORLD=set(GROUPS["world-close"]+GROUPS["world-strategic"])
def require(ok,msg):
    if not ok:raise ValueError(msg)
def weighted(v):
    if v is None:return None
    require(isinstance(v,list) and len(v)==7 and all(type(x) in (float,int) and math.isfinite(x) and 0<=x<=10 for x in v),"Invalid seven-dimensional scores")
    return round(float(sum(D(str(x))*w for x,w in zip(v,WEIGHTS.values()))/D(100)),2)
def read(z,name):return json.loads(z.read(name))
def load(z,final):
    r=read(z,"calibrated-scorecard.json")
    require(r["complete"] and r["weights"]==WEIGHTS and set(r["screens"])==ROLES,"Incomplete or different rubric")
    require(r["rubric_id"]=="eldoria-art-rubric-20261124-v1","Wrong rubric")
    require(r.get("visual_approval") is False and r.get("motion_evaluated") is False,"Unexpected automatic artistic approval")
    if final:require(r["candidate_sha"]==FINAL and r["baseline_sha"]==BASE,"Wrong final source")
    p=read(z,"provenance.json")
    require(set(p)==ROLES,"Incomplete provenance")
    total_cost=D(0)
    for group,keys in GROUPS.items():
        g=read(z,group+"/scores.json")
        receipt=read(z,group+"/auditor.json")
        ledger=read(z,group+"/ledger.json")
        require(set(g["screens"])==set(keys),"Missing group "+group)
        require(len(ledger["reservations"])==1 and ledger["reservations"][0]["state"]=="completed","Incomplete agent "+group)
        cost=D(str(receipt["cost_estimate_usd"]))
        require(cost==D(ledger["reservations"][0]["estimated_usd"]) and D(0)<=cost<=D(".06"),"Unexpected charge")
        total_cost+=cost
        for k in keys:
            row=r["screens"][k]
            prov=p[k]
            expected=(FINAL if final else PRIOR_WORLD) if k in WORLD else (FINAL if final else PRIOR_UI)
            require(prov["after_sha"]==expected and prov["before_sha"]==(expected if k in WORLD else BASE),"Wrong screenshot revision "+k)
            require(all(isinstance(prov[x],str) and len(prov[x])==64 and all(c in "0123456789abcdef" for c in prov[x]) for x in ("before_png","after_png")),"Missing screenshot hash "+k)
            require(row["provenance"]==prov,"Mismatched provenance "+k)
            for dim in ("before","after","reference"):
                require(row[dim]==g["screens"][k][dim],"Group response differs "+k)
                require(weighted(row[dim])==row[dim+"_total"],"Incorrect weighted total "+k)
            require(row["reference_kind"] in ("direct","analog","none") and ((row["reference_kind"]=="none")==(row["reference"] is None)),"Invalid reference "+k)
            delta=None if row["before_total"] is None or row["after_total"] is None else round(row["after_total"]-row["before_total"],2)
            require(row["delta"]==delta,"Incorrect paired delta "+k)
    require(total_cost==D(r["estimated_usd"]),"Invalid aggregate cost")
    return r
def reconcile(prior,final):
    results={}
    for group,keys in GROUPS.items():
        for k in keys:
            a,b=prior["screens"][k],final["screens"][k]
            same_baseline=a["provenance"]["before_png"]==b["provenance"]["before_png"]
            row={"group":group,"before":b["before_total"],"after":b["after_total"],"delta":b["delta"],"reference":b["reference_total"],"reference_kind":b["reference_kind"],
                 "prior_delta":a["delta"],"same_baseline_pixels":same_baseline,"minimum":8.5 if k.startswith("hud") else 8,"target":TARGETS.get(k,8),
                 "baseline_drift":None,"delta_of_deltas":None,"remaining":b["remaining"]}
            if same_baseline and a["before_total"] is not None and b["before_total"] is not None:
                row["baseline_drift"]=round(b["before_total"]-a["before_total"],2)
                if a["delta"] is not None and b["delta"] is not None:
                    row["delta_of_deltas"]=round(b["delta"]-a["delta"],2)
            row["minimum_met"]=row["after"] is not None and row["after"]>=row["minimum"]
            row["target_met"]=row["after"] is not None and row["after"]>=row["target"]
            row["one_point_gain"]=row["delta"] is not None and row["delta"]>=1
            results[k]=row
    stats={}
    for g,keys in list(GROUPS.items())+[("all",list(ROLES))]:
        rows=[results[k] for k in keys]; scored=[r for r in rows if r["delta"] is not None]
        stats[g]={"roles":len(rows),"scored":len(scored),"minimum_met":sum(r["minimum_met"] for r in rows),"target_met":sum(r["target_met"] for r in rows),"one_point_gain":sum(r["one_point_gain"] for r in rows),
                  "mean_before":None if not scored else round(statistics.mean(r["before"] for r in scored),2),
                  "mean_after":None if not scored else round(statistics.mean(r["after"] for r in scored),2),
                  "mean_paired_delta":None if not scored else round(statistics.mean(r["delta"] for r in scored),2)}
    return {"candidate_sha":FINAL,"rubric":WEIGHTS,"scores":results,"summary":stats,"artistic_gate_met":all(x["minimum_met"] for x in results.values()),"all_specific_targets_met":all(x["target_met"] for x in results.values()),"visual_approval":False,"motion_evaluated":False,"physical_iphone":"NOT_TESTED","notes":"World uses same-camera palette toggles, not a complete revision comparison. Compare paired deltas; delta-of-deltas only when baseline pixels identical. Baseline drift is reviewer variability, not a game change. No automatic publication."}
def main():
    p=argparse.ArgumentParser();p.add_argument("--prior",type=Path,required=True);p.add_argument("--final",type=Path);p.add_argument("--out",type=Path)
    a=p.parse_args()
    with zipfile.ZipFile(a.prior) as z:prior=load(z,False)
    if not a.final:
        print("VERIFIED PRIOR",len(prior["screens"]),prior["estimated_usd"])
        return
    require(a.out is not None,"Specify output")
    with zipfile.ZipFile(a.final) as z:final=load(z,True)
    report=reconcile(prior,final);a.out.write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding="utf-8")
    print("VERIFIED FINAL",json.dumps(report["summary"],ensure_ascii=False))
if __name__=="__main__":main()
