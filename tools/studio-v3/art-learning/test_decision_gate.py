import unittest
from decision_gate import arbitrate,PROTECTED
class GateTests(unittest.TestCase):
 def setUp(self):
    self.p={"preferred":"Y","protected_features":{k:{"X":"good","Y":"good","evidence":"same visible element"} for k in PROTECTED}}
 def test_owner_selection_is_not_reopened(self):self.assertEqual(arbitrate({}, "Y","X","X")["selected"],"X")
 def test_landmark_veto(self):
    self.p["protected_features"]["landmarks"]["Y"]="material_defect"
    self.assertEqual(arbitrate(self.p,"X","Y")["decision"],"reject_regression")
 def test_preserve_function(self):
    self.p["protected_features"]["function"]["Y"]="material_defect"
    self.assertEqual(arbitrate(self.p,"X","Y")["selected"],"X")
 def test_uncertainty(self):
    self.p["protected_features"]["access"]["Y"]="uncertain"
    self.assertEqual(arbitrate(self.p,"X","Y")["decision"],"insufficient_evidence")
 def test_no_missing_features(self):
    del self.p["protected_features"]["access"]
    with self.assertRaises(ValueError):arbitrate(self.p,"X","Y")
 def test_eligible_never_promotes(self):
    r=arbitrate(self.p,"X","Y")
    self.assertEqual(r["decision"],"eligible_for_native_score_gate");self.assertFalse(r["promote"])
 def test_blind_mapping(self):
    self.p["preferred"]="X"
    self.p["protected_features"]["identity"]["X"]="material_defect"
    self.assertEqual(arbitrate(self.p,"Y","X")["selected"],"Y")
if __name__=="__main__":unittest.main(verbosity=2)
