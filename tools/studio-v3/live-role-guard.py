"""Bounded V3 role-call accounting library. No live CLI or secret-bearing workflow.

A trusted dispatch gate must authenticate owner authorization, verify a unique
immutable request event and active lease before constructing a Session. This
library additionally binds the isolated grant, enforces slot reservations and
persists usage before releasing a typed packet. It does NOT certify GitHub event
authenticity or an API-side billing cap. No automatic retry or credit transfer.
"""
import datetime as dt
import hashlib
import importlib.util
import json
import pathlib
import re
from decimal import Decimal

P = pathlib.Path(__file__).resolve().parent
S = importlib.util.spec_from_file_location("v3_role_adapter", P/"v2-role-adapter.py")
adapter = importlib.util.module_from_spec(S)
S.loader.exec_module(adapter)
import budget_guard
import economics

BRANCH = "refs/heads/pilot/eldoria-studio-v3-autonomous-execution-20261126"
RESERVE = {"producer": Decimal("0.20"), "auditor": Decimal("0.15")}
D = Decimal
SMALL_PILOT_RESERVE = {"producer": D("0.15"), "auditor": D("0.10")}

def checked_reserves(reserves, g):
    if reserves is None:
        return RESERVE.copy()
    require(reserves == SMALL_PILOT_RESERVE and g["id"] in ("v3-senda-a","v3-senda-b") and g["max_attempts"] <= 2 and economics.money(g["cap_usd"]) <= D("0.525"), "Unapproved smaller reservation profile")
    return reserves.copy()

def require(ok,msg):
    if not ok:
        raise ValueError(msg)

def sha(raw):
    return hashlib.sha256(raw).hexdigest()

def authorize(raw_grant, isolated_budget, dispatch, day, reserves=None):
    require(type(raw_grant) is bytes and 0 < len(raw_grant) <= 16000,"Invalid grant bytes")
    g = adapter.unique_json(raw_grant.decode("utf-8"))
    keys = ["schema","id","mission_id","source_sha","target","objective","producer","auditor","required_checks","files","max_attempts","cap_usd","routing_profile","approved_public_game_evidence","expires_on"]
    require(adapter.exact(g,keys) and g["schema"]==1,"Invalid grant schema")
    require(g["approved_public_game_evidence"] is True,"Public evidence approval required")
    require(type(g["id"]) is str and re.fullmatch(r"v3-[a-z0-9-]{3,70}",g["id"]),"Invalid grant ID")
    require(type(g["source_sha"]) is str and re.fullmatch(r"[0-9a-f]{40}",g["source_sha"]),"Invalid pinned source")
    require(type(g["max_attempts"]) is int and 1<=g["max_attempts"]<=3,"Pilot allows 1–3 attempts")
    require(g["producer"]!=g["auditor"] and all(type(g[k]) is str and 0<len(g[k])<=2000 for k in ("mission_id","target","objective","producer","auditor")),"Invalid mission/role binding")
    require(type(g["required_checks"]) is list and 1<=len(g["required_checks"])<=20 and all(type(c) is str and re.fullmatch(r"[a-z0-9-]{1,80}",c) for c in g["required_checks"]) and len(set(g["required_checks"]))==len(g["required_checks"]),"Invalid checks")
    require(type(g["files"]) is dict and 1<=len(g["files"])<=8,"Invalid source scope")
    for path,h in g["files"].items():
        require(type(path) is str and len(path)<=200 and all(re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9_.-]*",x) and x not in (".","..") for x in path.split("/")),"Unsafe source path")
        require(not any(x.lower().startswith(("budget","secret","credential","agents.md")) for x in path.split("/")),"Protected path")
        require(type(h) is str and re.fullmatch(r"[0-9a-f]{64}",h),"Missing source digest")
    require(g["routing_profile"] in ("standard","economy","deep"),"Unknown routing")
    require(day < dt.date.fromisoformat(g["expires_on"]),"Grant expired")
    # The caller is the trust boundary. These fields are verified by a future
    # deployment-specific gate, not inferred from a model response.
    require(adapter.exact(dispatch,["branch","run_attempt","grant_sha256","verified_unique_dispatch","owner_authorized"]),"Unverified dispatch")
    require(dispatch["branch"]==BRANCH and dispatch["run_attempt"]==1 and dispatch["grant_sha256"]==sha(raw_grant) and dispatch["verified_unique_dispatch"] is True and dispatch["owner_authorized"] is True,"Dispatch not authorized")
    cap=economics.money(g["cap_usd"])
    required=g["max_attempts"]*sum(checked_reserves(reserves,g).values(),D(0))
    require(required<=cap<=D("5.00"),"Insufficient isolated reservation or excessive pilot cap")
    # Isolate this cycle from the active V2/Corona allocation.
    require(isolated_budget.get("v3_isolated") is True,"V3 isolated budget required")
    budget_guard.authorize(isolated_budget,day,g["id"],g["id"],dispatch["run_attempt"],cap)
    rows=[r for r in isolated_budget["reservations"] if r["id"]==g["id"]]
    require(len(rows)==1 and rows[0].get("request_sha256")==sha(raw_grant),"Grant bytes not reserved")
    return g

def verify_role_request(req,g):
    adapter.validate_request(req)
    require(all(req[k]==g[k] for k in ("mission_id","target","source_sha")),"Role request outside granted mission")
    role=req["role"];attempt=req["attempt"];inp=req["input"]
    require(req["identity"]==g[role] and 1<=attempt<=g["max_attempts"],"Role or attempt outside grant")
    require(inp.get("objective")==g["objective"] and inp.get("required_checks")==g["required_checks"],"Objective or checks changed")
    baseline=inp.get("baseline")
    require(type(baseline) is dict and set(baseline)==set(g["files"]),"Baseline scope changed")
    require(all(type(v) is str and sha(v.encode("utf-8"))==g["files"][p] for p,v in baseline.items()),"Baseline bytes changed")
    if role=="auditor":
        files=inp.get("files")
        require(type(files) is dict and set(files)==set(baseline) and all(type(v) is str for v in files.values()),"Candidate scope changed")
        require(adapter.digest(files)==inp.get("candidate_digest"),"Candidate digest mismatch")
        checks=inp.get("checks")
        require(type(checks) is list and len(checks)==len(g["required_checks"]) and {c.get("id") for c in checks}==set(g["required_checks"]) and all(c.get("pass") is True and c.get("candidate_digest")==inp["candidate_digest"] and type(c.get("evidence")) is str and c["evidence"] for c in checks),"Technical evidence missing, failed or unbound")
    return role,attempt

class Session:
    def __init__(self,raw_grant,isolated_budget,dispatch,day,root,reserves=None):
        self.grant=authorize(raw_grant,isolated_budget,dispatch,day,reserves)
        self.reserves=checked_reserves(reserves,self.grant)
        self.cap=economics.money(self.grant["cap_usd"])
        self.root=pathlib.Path(root)
        self.root.mkdir(parents=False,exist_ok=False)
        self.ledger={"schema":1,"grant_id":self.grant["id"],"grant_sha256":sha(raw_grant),"mission_id":self.grant["mission_id"],"source_sha":self.grant["source_sha"],"cap_usd":str(self.cap),"reserved_usd":"0","entries":[],"halted":False,"billing_verified":False}
        self._persist()
    def _persist(self):
        # A new session directory is required; a crash cannot reopen or retry it.
        p=self.root/"ledger.json";tmp=self.root/"ledger.tmp"
        with tmp.open("x",encoding="utf-8") as f:
            json.dump(self.ledger,f,ensure_ascii=False,indent=2)
            f.flush()
            import os
            os.fsync(f.fileno())
        tmp.replace(p)
    def snapshot(self):
        return json.loads(json.dumps(self.ledger))
    def call(self,req,key,network=None):
        # No CLI calls this method. Only a future trusted, unique dispatch host.
        require(self.ledger["halted"] is False,"Prior uncertainty/failure: no further calls")
        role,attempt=verify_role_request(req,self.grant)
        slot=role+"-"+str(attempt)
        entries=self.ledger["entries"]
        require(not any(e["slot"]==slot or e["request_digest"]==req["request_digest"] for e in entries),"Slot or request already reserved")
        if role=="auditor":
            require(any(e["slot"]=="producer-"+str(attempt) and e["state"]=="completed" for e in entries),"Auditor requires completed producer")
        v2role,payload,manifest=adapter.prepare_payload(req,self.grant["routing_profile"])
        if self.reserves == SMALL_PILOT_RESERVE:
            require(len(adapter.canonical(req).encode("utf-8")) <= 16000,"Small pilot request exceeds 16KB")
        require(type(key) is str and bool(key.strip()),"Trusted host must supply key")
        reserve=self.reserves[role]
        new_total=D(self.ledger["reserved_usd"])+reserve
        require(new_total<=self.cap,"Cumulative reservation exhausted")
        entry={"slot":slot,"request_digest":req["request_digest"],"reserved_usd":str(reserve),"state":"reserved_before_network","estimated_usd":None}
        entries.append(entry);self.ledger["reserved_usd"]=str(new_total)
        self._persist()
        import time
        started=time.monotonic()
        try:
            send=network if network is not None else adapter.studio.request_api
            raw=send(payload,key)
            entry.update(response_id=raw.get("id"),api_status=raw.get("status"),usage=raw.get("usage"),duration_ms=round((time.monotonic()-started)*1000))
            try:
                entry["estimated_usd"]=str(economics.estimate(raw.get("model") or payload["model"],raw.get("usage"),raw.get("service_tier") or "standard"))
            except (KeyError,ValueError,TypeError):
                entry["estimated_usd"]=None
            self._persist()
            report=adapter.studio.report(v2role,payload,manifest,raw)
            cost=economics.money(report["cost_estimate_usd"])
            entry.update(estimated_usd=str(cost),usage=report["usage"],model=report["model_returned"],tariff=report["tariff"],billing_verified=False)
            if cost>reserve:
                entry["state"]="over_reservation";self.ledger["halted"]=True
                self._persist()
                raise RuntimeError("Role estimate exceeded reservation; stop")
            packet=adapter.package_report(req,report,self.grant["id"]+":"+slot,profile=self.grant["routing_profile"])
            entry.update(state="completed",packet_digest=packet["payload_digest"],receipt_ref=packet["receipt_ref"])
            self._persist()
            return packet
        except Exception:
            if entry["state"]!="over_reservation":
                entry["duration_ms"]=round((time.monotonic()-started)*1000)
                entry["state"]="failed_reserved";self.ledger["halted"]=True
                self._persist()
            raise
