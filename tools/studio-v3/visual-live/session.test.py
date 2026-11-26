import datetime as dt,importlib.util,json,pathlib,tempfile,unittest
from unittest.mock import patch
P=pathlib.Path(__file__).resolve().parent
S=importlib.util.spec_from_file_location("session",P/"session.py")
s=importlib.util.module_from_spec(S);S.loader.exec_module(s)
DAY=dt.date(2026,11,26)
# Tiny PNG-header fixtures exercise the pinned V2 payload contract only.
# Actual decoded browser frames are validated in the visual CI separately.
PNG=b"\x89PNG\r\n\x1a\nfixture"
class Tests(unittest.TestCase):
    def setUp(self):
        self.tmp=tempfile.TemporaryDirectory();self.root=pathlib.Path(self.tmp.name)
        self.images=[self.root/"a.png",self.root/"b.png"];self.refs=[self.root/"a.md",self.root/"b.md"]
        for p in self.images:p.write_bytes(PNG)
        for p in self.refs:p.write_text("reference")
        self.e={"primary_sha256":s.sha(PNG),"secondary_sha256":s.sha(PNG),"reference_text_sha256":s.sha(b"referencereference")}
        self.g={"schema":1,"id":"v3-senda-visual-paired-20261126","source_sha":s.SOURCE,"reference_sha":s.REFERENCE,"target":s.TARGET,"production_evidence":self.e,"calibration_run":123,"max_total_usd":"2.00","arm_max_usd":"0.80","judge_max_usd":"0.40","expires_on":"2026-12-01","owner_authorized":True}
        self.raw=s.canonical(self.g).encode()
        self.b={"v3_visual_isolated":True,"cycle":{"starts":"2026-11-26","ends_exclusive":"2026-12-01","pilot_cap_usd":"2.00","automatic_reset":False},"reservations":[{"id":self.g["id"],"run_id":self.g["id"],"reserve_usd":"2.00","status":"authorized_once","request_sha256":s.sha(self.raw)}]}
        self.d={"branch":s.BRANCH,"run_attempt":1,"grant_sha256":s.sha(self.raw),"verified_unique_dispatch":True,"owner_authorized":True,"verified_calibration_run":123}
        self.mock=patch.object(s.studio,"request_api",side_effect=AssertionError("No real network in tests"));self.net=self.mock.start()
    def tearDown(self):self.net.assert_not_called();self.mock.stop();self.tmp.cleanup()
    def session(self):return s.Session(self.raw,self.b,self.d,DAY,self.root/"ledger")
    def req(self,role="single",attempt=1):
        r={"schema":1,"role":role,"attempt":attempt,"target":s.TARGET,"source_sha":s.SOURCE,"reference_sha":s.REFERENCE,"objective":"Fixture only","evidence":self.e,"contract":{},"prior":{}}
        r["request_digest"]=s.digest(r);return r
    def response(self,r,p,large=False):
        return {"id":"fixture","status":"completed","model":p["model"],"service_tier":"standard","usage":{"input_tokens":1000000 if large else 100,"output_tokens":50},"output":[{"type":"message","content":[{"type":"output_text","text":s.canonical({"request_digest":r["request_digest"],"payload":{"fixture":True}})}]}]}
    def test_equal_ceiling(self):
        self.assertEqual(sum(x[2] for x in s.SLOTS.values()),s.D("2.00"))
        for arm,cap in s.CAPS.items():self.assertEqual(sum(x[2] for x in s.SLOTS.values() if x[3]==arm),cap)
    def test_no_fresh_approval_denied(self):
        self.d["owner_authorized"]=False
        with self.assertRaises(ValueError):self.session()
    def test_closed_or_other_budget_denied(self):
        self.b["reservations"][0]["status"]="closed_reserved"
        with self.assertRaises(ValueError):self.session()
    def test_pre_reservation_and_duplicate(self):
        session=self.session();r=self.req()
        def network(p,k):
            l=json.loads((self.root/"ledger/ledger.json").read_text());self.assertEqual(l["entries"][0]["state"],"reserved_before_network")
            self.assertEqual(l["entries"][0]["reserved_usd"],"0.40")
            return self.response(r,p)
        session.call("single-1",r,self.images,self.refs,"fixture",network)
        with self.assertRaises(ValueError):session.call("single-1",r,self.images,self.refs,"fixture",network)
    def test_wrong_evidence_and_dependency(self):
        session=self.session();r=self.req("illustrator")
        with self.assertRaises(ValueError):session.call("illustrator-1",r,self.images,self.refs,"fixture",lambda *_:self.fail("Network"))
        self.images[0].write_bytes(PNG+b"altered")
        with self.assertRaises(ValueError):session.call("single-1",self.req(),self.images,self.refs,"fixture",lambda *_:self.fail("Network"))
    def test_unknown_cost_halts_all(self):
        session=self.session()
        def bad(*_):raise TimeoutError()
        with self.assertRaises(TimeoutError):session.call("single-1",self.req(),self.images,self.refs,"fixture",bad)
        self.assertTrue(session.ledger["halted"]);self.assertIsNone(session.ledger["entries"][0]["estimated_usd"])
        with self.assertRaises(ValueError):session.call("director-1",self.req("director"),self.images,self.refs,"fixture",bad)
    def test_judge_only_actual_verified_external_render(self):
        session=self.session()
        with self.assertRaises(ValueError):session.register_judge_evidence(self.e,{"mode":"hand-authored-calibration"})
        with self.assertRaises(ValueError):session.call("judge-1",self.req("judge"),self.images,self.refs,"fixture",lambda *_:self.fail("Network"))
if __name__=="__main__":unittest.main(verbosity=2)
