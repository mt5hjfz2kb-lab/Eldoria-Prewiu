"""Resource-only handoff for the owner's Issue200 native proof.
Never changes game sources or claims another workstream's write scope.
CAS update to the existing registry; fails on live conflict.
"""
import os,sys,json,subprocess,base64,copy
from datetime import datetime,timezone
repo=os.environ["REPO"]; run=int(os.environ["GITHUB_RUN_ID"])
path="pipeline/active-workstreams.json"
STUDIO="eldoria-studio-v3-autonomous-execution-20261126"
B1="eldoria-valoria-modular25d-bastion1-20261119"
OWNER="chatgpt-studio-v3-rankings-premium-20261127"
def gh(p):
    return json.loads(subprocess.check_output(["gh","api",f"repos/{repo}/{p}"],text=True))
def load():
    m=gh(f"contents/{path}?ref=main")
    b=gh("git/blobs/"+m["sha"])
    assert b["sha"]==m["sha"] and b["encoding"]=="base64"
    return m["sha"],json.loads(base64.b64decode(b["content"]))
def active(d,id):return next(w for w in d["active"] if w["id"]==id)
def require_no_studio_native():
    # Only this mission's own workflow is allowed to be active; stale historic queue rows
    # without actual jobs do not permit assuming a PC is idle. Native process gate follows.
    for status in ("in_progress","queued","waiting","pending","requested"):
        runs=gh(f"actions/runs?status={status}&per_page=100")["workflow_runs"]
        for w in runs:
            if w["id"]==run:continue
            if w["head_branch"] in ("pilot/eldoria-studio-v3-autonomous-execution-20261126","experiment/valoria-ciudad-region-unificada-20261121"):
                raise RuntimeError("Other live studio/Corona run: "+str(w["id"]))
            jobs=gh(f"actions/runs/{w['id']}/jobs?per_page=100")["jobs"]
            if any(j.get("status")!="completed" and ("self-hosted" in j.get("labels",[]) or "unity-6000-3-23f1" in j.get("labels",[])) for j in jobs):
                raise RuntimeError("Another native job is live: "+str(w["id"]))
def write(sha,d,msg):
    payload={"message":msg,"branch":"main","sha":sha,"content":base64.b64encode((json.dumps(d,ensure_ascii=False,indent=2)+"\n").encode()).decode()}
    result=subprocess.run(["gh","api","--method","PUT",f"repos/{repo}/contents/{path}","--input","-"],input=json.dumps(payload),text=True,capture_output=True)
    if result.returncode:raise RuntimeError("Registry CAS failed; no retry or override")
    print("REGISTRY_COMMIT "+json.loads(result.stdout)["commit"]["sha"],flush=True)
mode=sys.argv[1]
sha,d=load();s=active(d,STUDIO);b=active(d,B1);r=d["resource_rotation"]
now=datetime.now(timezone.utc).isoformat()
holders=[w["id"] for w in d["active"] if "windows-runner-heavy" in w.get("resources",[])]
assert s["owner"]==OWNER and s["ownership_state"]=="claimed"
if mode=="reserve":
    assert s["ownership_revision"]==14 and b["ownership_revision"]==19
    assert holders==[STUDIO] and r["current_workstream"]==STUDIO and r["status"]=="pr198_atelier_native" and r["return_to"]==B1
    assert gh("actions/runs/45971684843")["status"]=="completed"
    require_no_studio_native()
    assert gh("actions/runs/46032398265")["conclusion"]=="success"
    # Same studio owns the temporary resource; explicit owner-directed isolated task.
    # Preserve all PR198 delivery evidence and the Corona write claim.
    s["ownership_revision"]=15
    s["issue200_native_proof"]={"issue":200,"run_id":run,"source_sha":"91b8de9874aed87219d733efa6e8e0aa56207f1e","scope":"scratch only B castle and C sawmill; no writes to game/PR198/experimental","owner_authorization":"27 Nov: owner selected B+C and explicitly instructed execution using available connections; same Studio holder, no cancellation","asset_run":46032398265,"state":"native_reserved","reserved_at":now,"prior_rotation":copy.deepcopy(r)}
    d["resource_rotation"]={"resource":"windows-runner-heavy","mode":"exclusive_one_workstream_at_a_time","current_workstream":STUDIO,"current_run_id":run,"return_to":B1,"next_workstream":B1,"status":"issue200_bastion_i_native_proof","authorized_by":"Owner-directed Issue200 real Unity proof using same studio temporary PC reservation; no scope transfer","previous_rotation":copy.deepcopy(r),"rules":["Check actual PC processes before Unity; scratch only","Return resource to canonical Bastion/Corona after terminal native job, no other write scope changes"]}
    d["updated_at"]=now
    write(sha,d,"Coordinate same Studio PC reservation for owner-directed issue200 native proof only")
elif mode=="return":
    assert s["ownership_revision"]==15 and b["ownership_revision"]==19
    assert holders==[STUDIO] and r["current_workstream"]==STUDIO and r["current_run_id"]==run and r["status"]=="issue200_bastion_i_native_proof"
    jobs=gh(f"actions/runs/{run}/jobs?per_page=100")["jobs"]
    native=[j for j in jobs if j["name"]=="native-proof"]
    assert len(native)==1 and native[0]["status"]=="completed"
    require_no_studio_native()
    s["resources"]=[x for x in s["resources"] if x!="windows-runner-heavy"]
    s["ownership_revision"]=16
    s["issue200_native_proof"].update(state="resource_returned",terminal_native_job=native[0]["id"],native_conclusion=native[0]["conclusion"],returned_at=now)
    b["resources"]=list(b["resources"])+["windows-runner-heavy"]
    b["ownership_revision"]=20
    b["resource_handoff_note"]="Issue200 isolated Studio native proof terminal; resource returned only; no write ownership change."
    d["resource_rotation"]={"resource":"windows-runner-heavy","mode":"exclusive_one_workstream_at_a_time","current_workstream":B1,"current_run_id":None,"next_workstream":None,"return_to":None,"status":"corona_native_reserved","authorized_by":"Existing designated return after Issue200 studio native proof terminal","previous_rotation":copy.deepcopy(r),"rules":["No game changes or art approval from resource return","Recheck actual owner and native processes"]}
    d["updated_at"]=now
    write(sha,d,"Return Studio temporary PC reservation to canonical Bastion/Corona after issue200")
else:raise ValueError("Explicit reserve or return required")
