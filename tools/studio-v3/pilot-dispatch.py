"""One-shot Senda V3 pilot gate and persistent transport.

Only an immutable, individually reserved request in an authorized branch push
can invoke the existing V2 API client. Fixture transport uses no network or key.
No model-provided code or command is executed by this process.
"""
import base64
import datetime as dt
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
from urllib.parse import urlencode

P=Path(__file__).resolve().parent
S=importlib.util.spec_from_file_location("live_guard",P/"live-role-guard.py")
guard=importlib.util.module_from_spec(S);S.loader.exec_module(guard)
a=guard.adapter
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"
WORKSTREAM="eldoria-studio-v3-autonomous-execution-20261126"
OWNER="chatgpt-studio-v3-live-pilot-20261126"
SOURCE="e1d3b9892e1a75a9e7bfbafd0db225de7aacba74"
V2="1657b2caa6b1a955319538ce3a8b2906398599b3"
PLAN="tools/studio-v3/senda-pilot-plan.json"
BUDGET="tools/studio-v3/senda-pilot-budget.json"
REQUEST="tools/studio-v3/live-requests/senda-paired-20261126.json"
REQUEST_MESSAGE="Authorize one isolated V3 Senda paired pilot within USD 1.05"
DAY=lambda:dt.datetime.now(dt.timezone.utc).date()

def need(ok,msg):
    if not ok:raise ValueError(msg)
def run(*cmd,limit=4000000):
    p=subprocess.run(cmd,check=True,stdout=subprocess.PIPE,stderr=subprocess.PIPE,timeout=40)
    need(len(p.stdout)<=limit,"Trusted command output too large")
    return p.stdout
def git(*args):return run("git",*args).decode().strip()
def gh(path):return json.loads(run("gh","api","--method","GET",path))
def hash_bytes(b):return hashlib.sha256(b).hexdigest()
def plan_proof():
    line=run("node","tools/studio-v3/senda-pilot.mjs","--plan").decode().strip()
    need(line.startswith("STUDIO_V3_PLAN "),"Trusted plan output invalid")
    return json.loads(line.split("STUDIO_V3_PLAN ",1)[1])
def make_fixture_budget(plan):
    rows=[]
    for arm in plan["arms"]:
        g=arm["grant"]
        rows.append({"id":g["id"],"run_id":g["id"],"reserve_usd":g["cap_usd"],"status":"authorized_once","request_sha256":a.digest(g)})
    return {"v3_isolated":True,"cycle":{"starts":"2026-11-26","ends_exclusive":"2026-12-01","pilot_cap_usd":"1.05","automatic_reset":False},"reservations":rows}
def validate_budget(plan,budget):
    expected=make_fixture_budget(plan)
    need(budget.get("v3_isolated") is True and budget.get("cycle")==expected["cycle"] and budget.get("reservations")==expected["reservations"],"Reservation differs from exact two grants")
    need(sum(guard.D(x["reserve_usd"]) for x in budget["reservations"])==guard.D("1.05"),"Total reservation mismatch")
    guard.budget_guard.validate(budget,DAY())
def dispatch_for(grant):
    return {"branch":guard.BRANCH,"run_attempt":1,"grant_sha256":a.digest(grant),"verified_unique_dispatch":True,"owner_authorized":True}
def preflight():
    need(os.environ.get("GITHUB_REF")==guard.BRANCH and os.environ.get("GITHUB_RUN_ATTEMPT")=="1" and os.environ.get("GITHUB_EVENT_NAME")=="push","Not authorized push first attempt")
    event=json.loads(Path(os.environ["GITHUB_EVENT_PATH"]).read_text())
    head=os.environ["GITHUB_SHA"];before=event.get("before")
    need(event.get("after")==head and event.get("ref")==guard.BRANCH and event.get("forced") is False and event.get("deleted") is False,"Unexpected event")
    need(git("rev-parse","HEAD")==head and git("rev-parse","HEAD^")==before,"Not exact direct child")
    need(git("log","-1","--format=%s")==REQUEST_MESSAGE,"Wrong authorization commit message")
    need(git("diff","--name-status",before,head)=="A\t"+REQUEST,"Only immutable request addition permitted")
    request=a.unique_json(Path(REQUEST).read_text())
    need(a.exact(request,["schema","id","plan_sha256","budget_sha256","owner_authorized"]) and request["schema"]==1 and request["id"]=="senda-paired-20261126" and request["owner_authorized"] is True,"Invalid immutable request")
    plan_raw=run("git","show",before+":"+PLAN,limit=32000);budget_raw=run("git","show",before+":"+BUDGET,limit=16000)
    need(hash_bytes(plan_raw)==request["plan_sha256"] and hash_bytes(budget_raw)==request["budget_sha256"],"Unreserved plan or budget bytes")
    plan=a.unique_json(plan_raw.decode());budget=a.unique_json(budget_raw.decode())
    proof=plan_proof()
    need(plan==proof["plan"] and proof["baseline"]["invalid"]["failed"],"Plan differs from exact pinned source / no baseline defect")
    validate_budget(plan,budget)
    prepared=budget.get("prepared_ci")
    need(a.exact(prepared,["sha","run_id"]) and type(prepared["run_id"]) is int,"Missing offline proof")
    # The approved preparation is allowed to differ from the reservation parent
    # only by addition of these two frozen JSON files; no executable changes.
    need(set(git("diff","--name-status",prepared["sha"],before).splitlines())=={"A\t"+PLAN,"A\t"+BUDGET},"Prepared code changed after offline proof")
    need(git("diff","--name-only",V2,"HEAD","--","tools/openai-development-studio")=="","Pinned V2 client changed")
    offline=gh("repos/"+REPO+"/actions/runs/"+str(prepared["run_id"]))
    need(offline["head_sha"]==prepared["sha"] and offline["head_branch"]==guard.BRANCH.removeprefix("refs/heads/") and offline["name"]=="Studio V3 offline execution proof" and offline["status"]=="completed" and offline["conclusion"]=="success","Offline proof not certified")
    branch=gh("repos/"+REPO+"/git/ref/heads/"+guard.BRANCH.removeprefix("refs/heads/"))
    need(branch["object"]["sha"]==head,"Branch changed; opportunity revoked")
    history=gh("repos/"+REPO+"/commits?"+urlencode({"sha":before,"path":REQUEST,"per_page":1}))
    need(isinstance(history,list) and not history,"Request ID historically used")
    current=gh("repos/"+REPO+"/actions/runs/"+os.environ["GITHUB_RUN_ID"])
    need(current["head_sha"]==head and current["event"]=="push" and current["run_attempt"]==1 and current["name"]=="Studio V3 Senda paired live pilot","Unexpected workflow run")
    meta=gh("repos/"+REPO+"/contents/pipeline/active-workstreams.json?ref=main")
    blob=gh("repos/"+REPO+"/git/blobs/"+meta["sha"])
    registry=json.loads(base64.b64decode(blob["content"]))
    active=[x for x in registry["active"] if x["id"]==WORKSTREAM]
    need(len(active)==1,"Missing unique V3 claim")
    w=active[0]
    need(w["owner"]==OWNER and w["ownership_state"]=="claimed" and w["ownership_revision"]==3 and w["branch"]==guard.BRANCH.removeprefix("refs/heads/") and w["pilot"]["source_sha"]==SOURCE and w["pilot"]["max_total_reservation_usd"]=="1.05","Canonical V3 ownership or authorization changed")
    # Each arm is separately checked by the existing budget library.
    for arm in plan["arms"]:
        g=arm["grant"];guard.authorize(a.canonical(g).encode(),budget,dispatch_for(g),DAY(),guard.SMALL_PILOT_RESERVE)
    return plan,budget,{"head":head,"before":before,"prepared_sha":prepared["sha"],"prepared_ci_run":prepared["run_id"],"registry_blob":meta["sha"],"run_id":int(os.environ["GITHUB_RUN_ID"]),"plan_sha256":request["plan_sha256"],"budget_sha256":request["budget_sha256"],"owner":OWNER}
def fixture_result(req,model):
    # Deliberately exercise correction in B. These are NOT measured agent results.
    if req["role"]=="producer":
        all_clauses=["!Number.isSafeInteger(source.units)","source.units > 96","!Number.isSafeInteger(source.attackBonus)","source.attackBonus < 0","source.attackBonus > 125","!Number.isSafeInteger(source.shields)","source.shields < 0","source.shields > 50","!Number.isSafeInteger(source.totalLost)","source.totalLost < 0","!Number.isSafeInteger(source.totalRecruited)","source.totalRecruited < 0","!Number.isSafeInteger(source.score)","source.score < 0","!Array.isArray(source.history)","source.history.length !== source.stage","source.finished !== false","source.victory !== false"]
        clauses=["source.shields < 0"] if req["mission_id"]=="senda-pilot-b" and req["attempt"]==1 else all_clauses
        before="  }\n  function playWave(state, lane) {"
        after="    if ("+" || ".join(clauses)+") throw new Error('Estado de la expedición inválido');\n"+before
        body={"target":req["target"],"producer":req["identity"],"strategy":"state-guard","operations":[{"path":"tools/senda-brecha/src/rules.js","before":before,"after":after}]}
    else:
        body={"reviewer":req["identity"],"candidate_digest":req["input"]["candidate_digest"],"decision":"accept","reason":"Fixture only: independent checker consumed exact candidate checks","quality":"not_assessed","evidence_refs":["safe-edit","malformed-states","route-parity"]}
    return {"status":"completed","model":model,"service_tier":"standard","usage":{"input_tokens":100,"output_tokens":50},"output":[{"type":"message","content":[{"type":"output_text","text":a.canonical({"request_digest":req["request_digest"],"payload":body})}]}]}
def transport(root,live):
    if live:
        plan,budget,proof=preflight()
        key=os.environ.get("OPENAI_API_KEY")
        need(type(key) is str and bool(key.strip()),"Missing authorized API secret")
    else:
        plan=plan_proof()["plan"];budget=make_fixture_budget(plan);proof={"fixture":True}
        key="fixture-not-a-secret"
        # Hard fail if fixture ever tries the real API.
        guard.adapter.studio.request_api=lambda *_:(_ for _ in ()).throw(AssertionError("Fixture API forbidden"))
    root=Path(root)
    with (root/"dispatch-proof.json").open("x") as f:json.dump(proof,f,indent=2)
    sessions={}
    for arm in plan["arms"]:
        g=arm["grant"];sessions[g["id"]]=guard.Session(a.canonical(g).encode(),budget,dispatch_for(g),DAY(),root/g["id"],guard.SMALL_PILOT_RESERVE)
    def save():
        dest=root/"ledger-summary.json";tmp=root/"ledger-summary.tmp"
        with tmp.open("w") as f:json.dump([s.snapshot() for s in sessions.values()],f,indent=2)
        tmp.replace(dest)
    save()
    try:
        for line in sys.stdin:
            try:
                need(len(line)<=50000,"Transport message oversized")
                msg=a.unique_json(line)
                need(a.exact(msg,["id","req"]) and msg["id"] in sessions,"Unknown arm")
                # Uncertainty or an overrun in either arm freezes ALL new spend.
                need(not any(e["estimated_usd"] is None or e["state"]=="over_reservation" for s in sessions.values() for e in s.snapshot()["entries"]),"Prior cost unknown or above reserve")
                session=sessions[msg["id"]];req=msg["req"]
                network=None if live else lambda payload,key:fixture_result(req,payload["model"])
                packet=session.call(req,key,network=network)
                if not live:packet["origin"]="offline_fixture"
                response={"ok":True,"packet":packet}
            except Exception as exc:
                # Never print external responses, secrets, or arbitrary model text.
                response={"ok":False,"code":type(exc).__name__}
            finally:save()
            print(json.dumps(response,ensure_ascii=False,separators=(",",":")),flush=True)
    finally:save()
if __name__=="__main__":
    mode=sys.argv[1] if len(sys.argv)>1 else ""
    if mode=="--preflight":
        _,_,proof=preflight();print("STUDIO_V3_LIVE_PREFLIGHT_PASS "+json.dumps(proof))
    elif mode in ("--transport","--fixture-transport") and len(sys.argv)==3:
        transport(sys.argv[2],mode=="--transport")
    else:raise SystemExit("Explicit authorized pilot mode required")
