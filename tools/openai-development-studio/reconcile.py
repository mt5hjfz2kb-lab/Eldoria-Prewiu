"""Private CSV reconciliation: exact project/day only, never allocate billed costs to individual requests."""
import argparse,csv,datetime as dt,hashlib,json
from pathlib import Path
from economics import D,money
from intervention_report import summarize,read,write
def reconcile(records,csv_path,project,day_col,project_col,amount_col,currency_col,start,end,complete_until):
    start,end,cutoff=map(dt.date.fromisoformat,(start,end,complete_until))
    if not start<end or cutoff<start:raise ValueError("Invalid UTC interval")
    # Only compare complete past days; current day is provisional even if CSV contains it.
    sums={};rows=0
    with open(csv_path,encoding="utf-8-sig",newline="") as f:
        reader=csv.DictReader(f)
        cols={day_col,project_col,amount_col,currency_col}
        if len(cols)!=4 or not cols.issubset(reader.fieldnames or []):raise ValueError("CSV column mapping invalid")
        for row in reader:
            if row[project_col]!=project:continue
            if row[currency_col].strip().upper()!="USD":raise ValueError("Non-USD row")
            day=dt.date.fromisoformat(row[day_col].strip())
            if start<=day<end:
                sums[day]=sums.get(day,D(0))+money(row[amount_col]);rows+=1
    s=summarize(records)
    estimated={dt.date.fromisoformat(k):money(v) for k,v in s["estimated_by_utc_day"].items()}
    # UTC crossing/missing per-call dates makes day-level attribution unsafe.
    for r in records:
        if not r["estimate_complete"]:raise ValueError("Unknown-cost intervention; reconcile only after recovering receipt")
        if r["started_utc"][:10]!=r["finished_utc"][:10]:raise ValueError("Cross-midnight intervention requires per-call allocation")
    days=[]
    for i in range((end-start).days):
        d=start+dt.timedelta(days=i);e=estimated.get(d,D(0));actual=sums.get(d)
        delta=actual-e if actual is not None else None
        state="provisional" if d>=cutoff else "missing_export_row" if actual is None else "matched" if abs(delta)<=D(".005") else "difference"
        days.append({"utc_day":d.isoformat(),"estimated_studio_usd":str(e),"export_project_usd":str(actual) if actual is not None else None,
                     "difference_usd":str(delta) if delta is not None else None,"status":state})
    return {"schema":1,"scope":"project_daily_only","currency":"USD","project":project,
       "start_utc":start.isoformat(),"end_exclusive_utc":end.isoformat(),"complete_until_exclusive":cutoff.isoformat(),
       "csv_sha256":hashlib.sha256(Path(csv_path).read_bytes()).hexdigest(),"matching_rows":rows,"days":days,
       "individual_billed_costs_available":False,"note":"Differences may include other API usage, delayed export, pricing or rounding; no invented attribution."}
if __name__=="__main__":
    p=argparse.ArgumentParser()
    for n in ("records","csv","project","day-col","project-col","amount-col","currency-col","start","end","complete-until","out"):
        p.add_argument("--"+n,required=True)
    a=p.parse_args();r=reconcile([read(f) for f in Path(a.records).glob("run-*.json")],a.csv,a.project,a.day_col,a.project_col,a.amount_col,a.currency_col,a.start,a.end,a.complete_until)
    write(a.out,r);print(json.dumps(r,indent=2))
