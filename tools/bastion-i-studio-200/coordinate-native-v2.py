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
sha,d=load();b=active(d,B1);r=d["resource_rotation"]
now=datetime.now(timezone.utc).isoformat()
holders=[w["id"] for w in d["active"] if "windows-runner-heavy" in w.get("resources",[])]
assert b["owner"]=="chatgpt-corona-brecha-commercial-20261126" and b["ownership_state"]=="claimed"
assert holders==[B1] and r["current_workstream"]==B1
if mode=="reserve":
    assert b["ownership_revision"]==20 and r["status"]=="corona_native_reserved" and r.get("current_run_id") is None
    assert gh("actions/runs/46032921829")["status"]=="completed"
    assert gh("actions/runs/46050064308")["conclusion"]=="success"
    require_no_studio_native()
    b["ownership_revision"]=21
    b["issue200_native_v2"]={"issue":200,"run_id":run,"source_sha":"91b8de9874aed87219d733efa6e8e0aa56207f1e","scope":"scratch-only localized ground and independent sprite integration; no game source changes","state":"native_reserved","reserved_at":now,"prior_rotation":copy.deepcopy(r)}
    d["resource_rotation"]={"resource":"windows-runner-heavy","mode":"exclusive_one_workstream_at_a_time","current_workstream":B1,"current_run_id":run,"return_to":B1,"next_workstream":None,"status":"issue200_bastion_i_native_v2","authorized_by":"Owner explicitly continues Issue200 B+C proof; existing Bastion resource holder, no write scope transfer","previous_rotation":copy.deepcopy(r)}
    d["updated_at"]=now
    write(sha,d,"Reserve canonical Bastion PC for isolated Issue200 integration correction")
elif mode=="return":
    assert b["ownership_revision"]==21 and r["current_run_id"]==run and r["status"]=="issue200_bastion_i_native_v2"
    jobs=gh(f"actions/runs/{run}/jobs?per_page=100")["jobs"]
    native=[j for j in jobs if j["name"]=="native-proof"]
    assert len(native)==1 and native[0]["status"]=="completed"
    require_no_studio_native()
    b["ownership_revision"]=22
    b["issue200_native_v2"].update(state="resource_returned",terminal_native_job=native[0]["id"],native_conclusion=native[0]["conclusion"],returned_at=now)
    d["resource_rotation"]={"resource":"windows-runner-heavy","mode":"exclusive_one_workstream_at_a_time","current_workstream":B1,"current_run_id":None,"next_workstream":None,"return_to":None,"status":"corona_native_reserved","authorized_by":"Restore prior holder after terminal isolated Issue200 proof","previous_rotation":copy.deepcopy(r),"rules":["No game changes or artistic approval from resource return","Recheck live owner and actual native processes"]}
    d["updated_at"]=now
    write(sha,d,"Restore Bastion resource state after isolated Issue200 integration test")
else:raise ValueError("Explicit reserve or return required")
