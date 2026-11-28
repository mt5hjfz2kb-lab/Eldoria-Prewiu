import unittest,copy
from guard import memory,context,role_brief,plan_gate,outcome_gate
class LearningTests(unittest.TestCase):
 def setUp(self):
    self.p=dict(scope="Valoria/market",source_sha="a"*40,baseline_frame_hashes=["b"*64],strengths=["roof silhouette","working entrance","stone timber contrast"],defects=["silo overlap"],alternatives=["preserve and reframe","rebuild footprint"],adjacency=["well and granary"],native_checks=["matched mobile A/B"],hypothesis="A lower side awning reveals the entrance without hiding the roof.",mobile_preview=True,reversible=True,same_failed_mechanism=False,owner_budget_verified=True)
    self.o=dict(native=True,blind=True,matched=True,same_source=True,actor_controlled=True,evidence_run=1,artifact=1,before=8.0,after=8.7,material_regressions=[])
 def test_memory_grounded(self):self.assertEqual(len(memory()["cases"]),5)
 def test_no_holdout_leak(self):
    for c in memory()["cases"]:
      x=context(c["id"]);self.assertNotIn(c["id"],[v["id"] for v in x["cases"]]);self.assertFalse(any(c["id"] in r["from"] for r in x["rules"]))
 def test_roles(self):
    for r in ("director","artist","integrator","auditor"):self.assertIn("Role contract",role_brief(r))
 def test_valid_plan(self):self.assertTrue(plan_gate(self.p)["production_eligible"])
 def test_no_repeated_mechanism(self):
    self.p["same_failed_mechanism"]=True
    with self.assertRaises(ValueError):plan_gate(self.p)
 def test_requires_preservation(self):
    self.p["strengths"]=[]
    with self.assertRaises(ValueError):plan_gate(self.p)
 def test_requires_mobile(self):
    self.p["mobile_preview"]=False
    with self.assertRaises(ValueError):plan_gate(self.p)
 def test_requires_budget(self):
    self.p["owner_budget_verified"]=False
    with self.assertRaises(ValueError):plan_gate(self.p)
 def test_replay_rejects_actual_failures(self):
    for c in memory()["cases"]:
      if c["kind"]=="blind_native":
       o={**self.o,"before":c["before"],"after":c["after"],"evidence_run":c["run"],"artifact":c["artifact"]}
       self.assertEqual(outcome_gate(o)["decision"],"reject",c["id"])
 def test_reject_regression_even_score(self):
    self.o["material_regressions"]=["well occluded"]
    self.assertEqual(outcome_gate(self.o)["decision"],"reject")
 def test_reject_small_delta(self):
    self.o["after"]=8.15
    self.assertEqual(outcome_gate(self.o)["decision"],"reject")
 def test_reject_uncontrolled_actors(self):
    self.o["actor_controlled"]=False
    with self.assertRaises(ValueError):outcome_gate(self.o)
 def test_reject_no_native(self):
    self.o["native"]=False
    with self.assertRaises(ValueError):outcome_gate(self.o)
 def test_review_never_promotes(self):
    x=outcome_gate(self.o);self.assertEqual(x["decision"],"owner_review");self.assertFalse(x["art_approved"]);self.assertFalse(x["promote"])
if __name__=="__main__":unittest.main(verbosity=2)
