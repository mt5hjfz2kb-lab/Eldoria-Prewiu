"""Offline regression tests for paired memory result integrity."""
import importlib.util,json,pathlib,tempfile,unittest
HERE=pathlib.Path(__file__).parent
spec=importlib.util.spec_from_file_location("brain_memory_ab",HERE/"brain_memory_ab.py")
brain=importlib.util.module_from_spec(spec);spec.loader.exec_module(brain)
class BrainTests(unittest.TestCase):
    def test_cases_unique(self):
        self.assertEqual(len(brain.CASES),len({c[0] for c in brain.CASES}))
        self.assertEqual(len(brain.CASES),12)
    def test_no_answers_in_memory(self):
        self.assertTrue(all("SOSTENIBLE" not in m and "CONTRADICHO" not in m for m in brain.MEMORY))
    def test_fixed_source(self):
        self.assertEqual(len(brain.EXPECTED),64)
    def test_protected_scope(self):
        self.assertTrue(any("UV" in m for m in brain.MEMORY))
        self.assertTrue(any("PASS" in m for m in brain.MEMORY))
if __name__=="__main__":unittest.main()
