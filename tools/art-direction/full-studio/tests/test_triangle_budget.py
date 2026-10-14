import unittest,pathlib,sys
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from triangle_budget import check_triangle_growth
class TriangleGate(unittest.TestCase):
 def test_exact_previous_false_rejection(self):
  r=check_triangle_growth(33522,33522)
  self.assertTrue(r["allowed"]);self.assertEqual(r["triangle_delta"],0)
 def test_growth_rejected(self):
  self.assertFalse(check_triangle_growth(33522,44000)["allowed"])
 def test_small_delta_within_budget(self):
  self.assertTrue(check_triangle_growth(1000,2900)["allowed"])
  self.assertFalse(check_triangle_growth(1000,3100)["allowed"])
 def test_missing_proof_fails_closed(self):
  for b,a in ((None,33522),(0,33522),(5344.0,33522),(33522,None)):
   with self.assertRaises(ValueError):check_triangle_growth(b,a)
if __name__=="__main__":unittest.main()
