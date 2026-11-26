import datetime as dt
import hashlib
import importlib.util
import json
import pathlib
import tempfile
import unittest
from unittest.mock import patch

P=pathlib.Path(__file__).resolve().parent
S=importlib.util.spec_from_file_location("guard",P/"live-role-guard.py")
guard=importlib.util.module_from_spec(S);S.loader.exec_module(guard)
a=guard.adapter
DAY=dt.date(2026,11,26)
SOURCE="1657b2caa6b1a955319538ce3a8b2906398599b3"
BASELINE={"candidate.json":'{"value":1}'}

def setup():
    grant={"schema":1,"id":"v3-test-only","mission_id":"test-mission","source_sha":SOURCE,"target":"T","objective":"Set value to 3","producer":"programmer","auditor":"independent","required_checks":["value"],"files":{"candidate.json":guard.sha(BASELINE["candidate.json"].encode())},"max_attempts":2,"cap_usd":"0.70","routing_profile":"standard","approved_public_game_evidence":True,"expires_on":"2026-12-01"}
    raw=a.canonical(grant).encode()
    budget={"v3_isolated":True,"cycle":{"starts":"2026-11-26","ends_exclusive":"2026-12-02","pilot_cap_usd":"0.70","automatic_reset":False},"reservations":[{"id":grant["id"],"run_id":grant["id"],"status":"authorized_once","reserve_usd":"0.70","request_sha256":guard.sha(raw)}]}
    dispatch={"branch":guard.BRANCH,"run_attempt":1,"grant_sha256":guard.sha(raw),"verified_unique_dispatch":True,"owner_authorized":True}
    return grant,raw,budget,dispatch
def request(grant,role="producer",attempt=1):
    inp={"objective":grant["objective"],"required_checks":grant["required_checks"],"baseline":BASELINE}
    if role=="auditor":
        files={"candidate.json":'{"value":3}'}
        inp.update(files=files,candidate_digest=a.digest(files),checks=[{"id":"value","pass":True,"candidate_digest":a.digest(files),"evidence":"Parsed actual candidate"}])
    req={"schema":1,"mode":"offline","mission_id":grant["mission_id"],"source_sha":grant["source_sha"],"target":grant["target"],"role":role,"identity":grant[role],"attempt":attempt,"input":inp}
    req["request_digest"]=a.digest(req)
    return req

def fake_result(req,model,large=False):
    if req["role"]=="producer":
        body={"target":req["target"],"producer":req["identity"],"strategy":"fix-value","operations":[{"path":"candidate.json","before":'"value":1',"after":'"value":3'}]}
    else:
        body={"reviewer":req["identity"],"candidate_digest":req["input"]["candidate_digest"],"decision":"accept","reason":"Fixture result","quality":"not_assessed","evidence_refs":["value"]}
    return {"status":"completed","model":model,"service_tier":"standard","usage":{"input_tokens":100000 if large else 100,"output_tokens":50},"output":[{"type":"message","content":[{"type":"output_text","text":a.canonical({"request_digest":req["request_digest"],"payload":body})}]}]}

class GuardTests(unittest.TestCase):
    def setUp(self):
        self.g,self.raw,self.b,self.d=setup()
        self.tmp=tempfile.TemporaryDirectory()
        self.root=pathlib.Path(self.tmp.name)/"ledger"
        self.network=patch.object(a.studio,"request_api",side_effect=AssertionError("Real API forbidden in tests"))
        self.mock=self.network.start()
    def tearDown(self):
        self.mock.assert_not_called();self.network.stop();self.tmp.cleanup()
    def session(self):
        return guard.Session(self.raw,self.b,self.d,DAY,self.root)
    def test_reject_unapproved_dispatch(self):
        self.d["owner_authorized"]=False
        with self.assertRaises(ValueError):self.session()
        self.assertFalse(self.root.exists())
    def test_reject_retry(self):
        self.d["run_attempt"]=2
        with self.assertRaises(ValueError):self.session()
    def test_reject_wrong_budget(self):
        self.b["v3_isolated"]=False
        with self.assertRaises(ValueError):self.session()
    def test_reject_changed_grant_bytes(self):
        self.raw+=b" "
        with self.assertRaises(ValueError):self.session()
    def test_reject_baseline_change(self):
        s=self.session();r=request(self.g);r["input"]["baseline"]={"candidate.json":"wrong"};r["request_digest"]=a.digest({k:v for k,v in r.items() if k!="request_digest"})
        with self.assertRaises(ValueError):s.call(r,"fixture",lambda *_:self.fail("Must not call network"))
        self.assertEqual(s.snapshot()["entries"],[])
    def test_reservation_written_before_call_and_duplicate_rejected(self):
        s=self.session();r=request(self.g)
        def fake(payload,key):
            state=json.loads((self.root/"ledger.json").read_text())
            self.assertEqual(state["entries"][0]["state"],"reserved_before_network")
            self.assertEqual(state["reserved_usd"],"0.20")
            return fake_result(r,payload["model"])
        packet=s.call(r,"fixture",fake)
        self.assertEqual(packet["origin"],"archived_role_output")
        self.assertEqual(s.snapshot()["entries"][0]["state"],"completed")
        with self.assertRaises(ValueError):s.call(r,"fixture",fake)
        with self.assertRaises(FileExistsError):self.session()
    def test_unknown_cost_stops_all_later_calls(self):
        s=self.session();r=request(self.g)
        def bad(*_):raise TimeoutError("Fixture transport timeout")
        with self.assertRaises(TimeoutError):s.call(r,"fixture",bad)
        self.assertTrue(s.snapshot()["halted"])
        self.assertIsNone(s.snapshot()["entries"][0]["estimated_usd"])
        with self.assertRaises(ValueError):s.call(request(self.g,attempt=2),"fixture",lambda *_:self.fail("Must not retry"))
    def test_over_reservation_halts(self):
        s=self.session();r=request(self.g)
        with self.assertRaises(RuntimeError):s.call(r,"fixture",lambda p,k:fake_result(r,p["model"],large=True))
        self.assertTrue(s.snapshot()["halted"])
        self.assertEqual(s.snapshot()["entries"][0]["state"],"over_reservation")
    def test_auditor_requires_producer_and_actual_checks(self):
        s=self.session();r=request(self.g,"auditor")
        with self.assertRaises(ValueError):s.call(r,"fixture",lambda *_:self.fail("Must not call"))
        prod=request(self.g)
        s.call(prod,"fixture",lambda p,k:fake_result(prod,p["model"]))
        r["input"]["checks"][0]["pass"]=False;r["request_digest"]=a.digest({k:v for k,v in r.items() if k!="request_digest"})
        with self.assertRaises(ValueError):s.call(r,"fixture",lambda *_:self.fail("Failed check"))
        r=request(self.g,"auditor")
        s.call(r,"fixture",lambda p,k:fake_result(r,p["model"]))
        self.assertEqual(s.snapshot()["reserved_usd"],"0.35")
        self.assertEqual(len(s.snapshot()["entries"]),2)

if __name__=="__main__":
    unittest.main(verbosity=2)
