import unittest
from competition_gate import classify_asset,native_scope
class CompetitionTests(unittest.TestCase):
    def test_identical(self):
        q=classify_asset("a"*64,"a"*64,[],[],8,9)
        self.assertFalse(q["native"]);self.assertEqual(q["decision"],"skip_native_identical")
    def test_material(self):
        self.assertFalse(classify_asset("a"*64,"b"*64,["lost silhouette"],[],8,9)["native"])
    def test_minor(self):
        q=classify_asset("a"*64,"b"*64,[],["one bright highlight"],8,8.4)
        self.assertTrue(q["native"]);self.assertFalse(q["promote"])
    def test_lower(self):
        self.assertFalse(classify_asset("a"*64,"b"*64,[],[],8,7.9)["native"])
    def test_scope(self):
        self.assertEqual(native_scope({"a":{"native":False},"b":{"native":True}})["targets"],["b"])
        self.assertFalse(native_scope({"a":{"native":False}})["native_eligible"])
    def test_unverified(self):
        with self.assertRaises(ValueError):classify_asset("abc","def",[],[],8,8.5)
if __name__=="__main__":unittest.main()
