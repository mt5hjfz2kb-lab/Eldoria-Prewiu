"""Preregistered balanced A/B benchmark: static integrity checks."""
import importlib.util,pathlib,unittest
DIR=pathlib.Path(__file__).parent
spec=importlib.util.spec_from_file_location("brain_memory_epistemic_v2",DIR/"brain_memory_epistemic_v2.py")
m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
class BalancedMemoryTests(unittest.TestCase):
    def test_binary_balance(self):
        self.assertEqual(len(m.CASES),16)
        self.assertEqual(sum(g for _,_,g in m.CASES),8)
    def test_no_duplicates(self):
        self.assertEqual(len(set(x[0] for x in m.CASES)),16)
    def test_source_sha(self):
        self.assertEqual(m.SHA,"e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869")
    def test_memory_has_no_decision_labels(self):
        self.assertTrue(m.MEMORY)
        self.assertFalse(any("PROBADO" in x for x in m.MEMORY))
    def test_guard_on_documented_mistakes(self):
        self.assertTrue(any("AABB" in x for x in m.MEMORY))
        self.assertTrue(any("UV" in x for x in m.MEMORY))
    def test_all_cases_require_exact_boolean_gold(self):
        self.assertTrue(all(type(g) is bool for _,_,g in m.CASES))
if __name__=="__main__":unittest.main()
