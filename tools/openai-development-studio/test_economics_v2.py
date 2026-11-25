import csv,json,tempfile,unittest
from pathlib import Path
from decimal import Decimal as D
from economics import estimate
from intervention_report import collect,summarize
from reconcile import reconcile
from routing import plan,lessons
class Tests(unittest.TestCase):
    def test_cache(self):
        u={"input_tokens":3981,"output_tokens":565,"input_tokens_details":{"cached_tokens":0,"cache_write_tokens":3978,"cache_write_tokens_details":{"ttl_30m_tokens":3978,"ttl_12h_tokens":0}}}
        self.assertEqual(estimate("gpt-6.1-terra",u),D(".0078005"))
    def test_long_cache(self):
        self.assertEqual(estimate("gpt-6.2-sol",{"input_tokens":100,"output_tokens":10,"input_tokens_details":{"cached_tokens":20,"cache_write_tokens":50,"cache_write_tokens_details":{"ttl_30m_tokens":25,"ttl_12h_tokens":25}}}),D(".0003495"))
    def test_image(self):
        self.assertEqual(estimate("gpt-image-2.6-flare",{"input_tokens":534,"input_tokens_details":{"text_tokens":534,"image_tokens":0},"output_tokens":1449}),D(".04614"))
    def test_unknown(self):
        for u in ({"input_tokens":1,"output_tokens":1,"input_tokens_details":{"cached_tokens":2}},
                  {"input_tokens":300000,"output_tokens":1},{"input_tokens":1,"output_tokens":-1}):
            with self.assertRaises(ValueError):estimate("gpt-6.2-sol",u)
        with self.assertRaises(ValueError):estimate("gpt-6.2-sol",{"input_tokens":1,"output_tokens":1},"fast")
    def test_routes(self):
        self.assertEqual(plan("review","economy")[-1]["model"],"gpt-6-luna")
        self.assertEqual(plan("review","standard")[-1]["model"],"gpt-6.2-sol")
        self.assertEqual(plan("review","deep")[0]["reasoning"],"medium")
        with self.assertRaises(ValueError):plan("art_candidate","economy")
    def test_memory(self):
        self.assertIn("L004",lessons("MILL_YARD","review"))
        self.assertNotIn("L004",lessons("HEROES","review"))
    def record(self,root):
        (root/"ledger.json").write_text(json.dumps({"reservations":[{"role":"director","reserve_usd":".15","state":"completed","model":"gpt-6.1-terra","usage":{"input_tokens":100,"output_tokens":20}},{"role":"auditor","reserve_usd":".15","state":"reserved","model":"gpt-6.1-terra"}]}))
        return collect(root,123,"test-request","2026-11-25T10:00:00Z","2026-11-25T10:01:00Z","pending","failure")
    def test_partial(self):
        with tempfile.TemporaryDirectory() as d:
            r=self.record(Path(d));self.assertEqual(r["estimated_known_usd"],"0.0002")
            self.assertEqual(r["calls_with_unknown_cost"],1)
            self.assertFalse(r["estimate_complete"]);self.assertIsNone(r["billed_usd"])
            self.assertIsNone(summarize([r])["cost_per_accepted_usd"])
            with self.assertRaises(ValueError):summarize([r,r])
    def test_csv(self):
        with tempfile.TemporaryDirectory() as d:
            root=Path(d);r=self.record(root);r["estimate_complete"]=True;r["calls_with_unknown_cost"]=0
            p=root/"costs.csv";p.write_text("day,project,cost,currency\n2026-11-25,eldoria,0.0002,USD\n2026-11-25,other,4,USD\n")
            a=reconcile([r],p,"eldoria","day","project","cost","currency","2026-11-25","2026-11-26","2026-11-26")
            self.assertEqual(a["days"][0]["status"],"matched")
            self.assertFalse(a["individual_billed_costs_available"])
            b=reconcile([r],p,"eldoria","day","project","cost","currency","2026-11-25","2026-11-26","2026-11-25")
            self.assertEqual(b["days"][0]["status"],"provisional")
            p.write_text("day,project,cost,currency\n2026-11-25,eldoria,0.0002,EUR\n")
            with self.assertRaises(ValueError):reconcile([r],p,"eldoria","day","project","cost","currency","2026-11-25","2026-11-26","2026-11-26")
if __name__=="__main__":unittest.main(verbosity=2)
