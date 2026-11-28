import unittest
from academy.context import load,brief,ROLES
from guard import role_brief
class AcademyTests(unittest.TestCase):
 def test_eight_grounded_cards(self):
    _,t,known=load()
    self.assertEqual(len(t["cards"]),8)
    self.assertEqual(len(known),10)
    self.assertFalse(t["provenance"]["course_completion"])
 def test_roles(self):
    for r in ROLES:
        x=brief(r)
        self.assertEqual({v["competency"] for v in x["cards"]},set(ROLES[r]))
        self.assertTrue(all(s["url"].startswith("https://") for s in x["sources"]))
 def test_scope(self):
    self.assertEqual({c["competency"] for c in brief("artist",["material"])["cards"]},{"material"})
    with self.assertRaises(ValueError):brief("integrator",["material"])
 def test_provenance(self):
    x=brief("auditor")
    self.assertEqual(next(s for s in x["sources"] if s["id"]=="B-MAT")["read_status"],"curated_only")
 def test_brief_opt_in(self):
    self.assertNotIn("Academy technique context",role_brief("artist"))
    self.assertIn("Academy technique context",role_brief("artist",academy=True,competencies=["material"]))
 def test_holdout_not_contaminated(self):
    with self.assertRaises(ValueError):role_brief("auditor",holdout="market-pilot",academy=True)
if __name__=="__main__":unittest.main()
