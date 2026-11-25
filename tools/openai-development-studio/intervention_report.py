"""Sanitized, immutable economic and productivity report. No network or API key."""
import argparse,datetime as dt,json,re,sys
from pathlib import Path
from decimal import Decimal
from economics import D,RATES,estimate,receipt,money

ROLES=("smoke","director","artist","programmer","auditor","coordinator")
DECISIONS=("technical_only","rejected","pending","accepted","not_applicable")
def read(p):return json.loads(p.read_text(encoding="utf-8-sig"))
def utcday(s):
    if not isinstance(s,str): raise ValueError("UTC timestamp required")
    d=dt.datetime.fromisoformat(s.replace("Z","+00:00"))
    if d.tzinfo is None: raise ValueError("Timezone required")
    return d.astimezone(dt.timezone.utc).date().isoformat()
def collect(root,run_id,request_id,started,finished,decision="pending",status="unknown",source_run=None):
    if type(run_id) is not int or run_id<=0 or not re.fullmatch(r"[a-z0-9-]{3,100}",request_id):
        raise ValueError("Invalid report identity")
    if decision not in DECISIONS or status not in ("success","failure","cancelled","unknown"):
        raise ValueError("Invalid decision/status")
    root=Path(root)
    start=dt.datetime.fromisoformat(started.replace("Z","+00:00"))
    finish=dt.datetime.fromisoformat(finished.replace("Z","+00:00"))
    if start.tzinfo is None or finish.tzinfo is None or finish<start: raise ValueError("Bad elapsed time")
    led=read(root/"ledger.json") if (root/"ledger.json").is_file() else None
    reservations=led["reservations"] if led else []
    seen=set(); rows=[]
    for row in reservations:
        slot=row["role"]
        if slot not in ROLES or slot in seen:raise ValueError("Duplicate or invalid role")
        seen.add(slot)
        p=root/(slot+".json")
        v=read(p) if p.is_file() else {}
        model=v.get("model_returned") or v.get("model") or row.get("model")
        usage=v.get("usage") or row.get("usage")
        ts=v.get("utc") or row.get("utc") or None
        elapsed=v.get("duration_seconds",row.get("duration_seconds"))
        try:
            if row["state"] not in ("completed","completed_over_reservation") or not model or not usage:raise ValueError("Missing completed receipt")
            if row.get("usage") and v.get("usage") and row["usage"]!=v["usage"]:raise ValueError("Conflicting receipt usage")
            r=receipt(slot,model,usage,v.get("service_tier",row.get("service_tier","standard")),elapsed,ts)
        except (ValueError,KeyError,TypeError):
            r={"role":slot,"model":model,"usage":None,"estimated_usd":None,"billed_usd":None,
               "billing_status":"unreconciled","utc":ts,"duration_seconds":elapsed,"tariff":RATES["version"]}
        r.update(state=row["state"],reservation_usd=str(money(row["reserve_usd"])))
        rows.append(r)
    if not led:
        p=root/"eldoria-api-proof.json"
        if p.is_file():
            v=read(p);r=receipt("smoke",v["model_returned"],v["usage"],utc=v["utc"])
            r.update(state="completed",reservation_usd="0.01");rows.append(r)
        # A missing ledger for a live run cannot be interpreted as zero spend.
    known=sum((money(r["estimated_usd"]) for r in rows if r["estimated_usd"] is not None),D(0))
    unknown=sum(r["estimated_usd"] is None for r in rows)
    if not rows:unknown=1
    if status!="success" and not rows:unknown=1
    return {"schema":2,"run_id":run_id,"request_id":request_id,"source_run":source_run,
       "started_utc":started,"finished_utc":finished,"utc_day":utcday(started),
       "wall_seconds":round((finish-start).total_seconds(),3),"workflow_status":status,
       "quality_decision":decision,"quality_score_before":None,"quality_score_after":None,
       "quality_evidence_run":None,"calls_recorded":len(rows),"calls_with_unknown_cost":unknown,
       "estimated_known_usd":str(known),"estimate_complete":unknown==0,
       "billed_usd":None,"billing_status":"unreconciled","github_billed_usd":None,"pc_electricity_usd":None,
       "windows_seconds":None,"manual_review_seconds":None,"tariff":RATES["version"],"calls":rows}
def summarize(reports):
    seen=set();role={};models={};daily={};total=D(0);unknown=0;decisions={x:0 for x in DECISIONS};seconds=0
    for r in reports:
        if r["run_id"] in seen:raise ValueError("Duplicate run")
        seen.add(r["run_id"]);n=money(r["estimated_known_usd"]);total+=n;unknown+=r["calls_with_unknown_cost"]
        seconds+=r["wall_seconds"];decisions[r["quality_decision"]]+=1
        day=r["utc_day"];daily[day]=daily.get(day,D(0))+n
        for c in r["calls"]:
            if c["estimated_usd"] is None:continue
            value=money(c["estimated_usd"])
            for dst,key in ((role,c["role"]),(models,c["model"])):
                v=dst.setdefault(key,{"calls":0,"cost":D(0),"min":None,"max":None})
                v["calls"]+=1;v["cost"]+=value
                v["min"]=value if v["min"] is None else min(value,v["min"])
                v["max"]=value if v["max"] is None else max(value,v["max"])
    def pack(d):return {k:{**v,"cost":str(v["cost"]),"min":str(v["min"]),"max":str(v["max"])} for k,v in d.items()}
    accepted=decisions["accepted"];evaluated=accepted+decisions["rejected"]
    return {"schema":2,"interventions":len(reports),"calls":sum(r["calls_recorded"] for r in reports),
       "unknown_cost_calls":unknown,"estimated_known_usd":str(total),"estimate_complete":unknown==0,
       "billed_usd":None,"billing_status":"unreconciled","by_role":pack(role),"by_model":pack(models),
       "estimated_by_utc_day":{k:str(v) for k,v in daily.items()},"quality_decisions":decisions,
       "acceptance_rate":accepted/evaluated if evaluated else None,
       "cost_per_accepted_usd":str(total/accepted) if accepted and not unknown else None,
       "summed_workflow_wall_seconds":seconds,"elapsed_caveat":"Workflow times can overlap; not human hours saved.",
       "time_savings_measured":False,"github_billed_usd":None,"pc_electricity_usd":None}
def write(path,data):
    p=Path(path);p.parent.mkdir(parents=True,exist_ok=True)
    with p.open("x",encoding="utf-8") as f:json.dump(data,f,ensure_ascii=False,indent=2);f.write("\n")
def markdown(r):
    lines=["# Studio — intervención "+str(r["run_id"]),"",
           "Coste API estimado conocido: **"+r["estimated_known_usd"]+" USD**. Facturación: pendiente.",
           "Solicitudes registradas: "+str(r["calls_recorded"])+"; costes desconocidos: "+str(r["calls_with_unknown_cost"])+".",
           "Duración de la intervención medida: "+str(r["wall_seconds"])+" s.",
           "Estado técnico: "+r["workflow_status"]+"; decisión de calidad: "+r["quality_decision"]+".","",
           "| Rol | Modelo | Entrada | Salida | USD estimados |","|---|---|---:|---:|---:|"]
    for c in r["calls"]:
        u=c.get("usage") or {}
        lines.append("| "+str(c["role"])+" | "+str(c["model"])+" | "+str(u.get("input_tokens","?"))+" | "+str(u.get("output_tokens","?"))+" | "+str(c["estimated_usd"])+" |")
    lines+=["","No incluye costes de GitHub, electricidad ni tiempo humano. Una propuesta no es una mejora aceptada."]
    return "\n".join(lines)+"\n"
if __name__=="__main__":
    p=argparse.ArgumentParser();sp=p.add_subparsers(dest="cmd",required=True)
    a=sp.add_parser("collect");a.add_argument("--root",required=True);a.add_argument("--run",type=int,required=True)
    a.add_argument("--request",required=True);a.add_argument("--started",required=True);a.add_argument("--finished",required=True)
    a.add_argument("--decision",default="pending");a.add_argument("--status",default="unknown");a.add_argument("--source-run",type=int);a.add_argument("--out",required=True)
    b=sp.add_parser("summary");b.add_argument("--records",required=True);b.add_argument("--out",required=True)
    x=p.parse_args()
    if x.cmd=="collect":
        r=collect(x.root,x.run,x.request,x.started,x.finished,x.decision,x.status,x.source_run);write(x.out,r)
        Path(x.out).with_suffix(".md").write_text(markdown(r),encoding="utf-8");print(markdown(r))
    else:
        r=summarize([read(f) for f in sorted(Path(x.records).glob("run-*.json"))]);write(x.out,r);print(json.dumps(r))
