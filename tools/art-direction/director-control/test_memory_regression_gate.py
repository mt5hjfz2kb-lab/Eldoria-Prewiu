"""Executable regression checks against paired-memory safety gate."""
import json,pathlib,subprocess,sys,tempfile,unittest
SCRIPT=pathlib.Path(__file__).with_name("memory_regression_gate.py")
def run(pairs,extra=None):
    d={"pairs":pairs,"case_count":len(pairs),"baseline_correct":sum(bool(p["baseline_correct"]) for p in pairs),"memory_correct":sum(bool(p["memory_correct"]) for p in pairs),"gain_count":sum(not p["baseline_correct"] and p["memory_correct"] for p in pairs),"regression_count":sum(p["baseline_correct"] and not p["memory_correct"] for p in pairs),"art_approved":False,"any_blender_edits":False}
    d.update(extra or {})
    with tempfile.TemporaryDirectory() as t:
        root=pathlib.Path(t);(root/"input.json").write_text(json.dumps(d),encoding="utf-8")
        result=subprocess.run([sys.executable,str(SCRIPT),"--input",str(root/"input.json"),"--out",str(root/"out.json")],capture_output=True,text=True)
        output=json.loads((root/"out.json").read_text()) if (root/"out.json").exists() else {}
        return result.returncode,output
class MemoryRegressionGateTests(unittest.TestCase):
    def test_improvement_without_regression(self):
        code,out=run([{"id":"a","baseline_correct":False,"memory_correct":True}])
        self.assertEqual(code,0);self.assertEqual(out["status"],"IMPROVED_FIXED_SAMPLE")
    def test_regression_fails(self):
        code,out=run([{"id":"a","baseline_correct":True,"memory_correct":False}])
        self.assertEqual(code,2);self.assertEqual(out["status"],"REGRESSION")
    def test_no_gain_is_not_improvement(self):
        code,out=run([{"id":"a","baseline_correct":True,"memory_correct":True}])
        self.assertEqual(code,0);self.assertEqual(out["status"],"NO_GAIN")
    def test_inconsistent_counts_rejected(self):
        code,_=run([{"id":"a","baseline_correct":False,"memory_correct":True}],{"gain_count":0})
        self.assertNotEqual(code,0)
    def test_duplicate_cases_rejected(self):
        code,_=run([{"id":"a","baseline_correct":True,"memory_correct":True},{"id":"a","baseline_correct":False,"memory_correct":True}])
        self.assertNotEqual(code,0)
    def test_art_approval_rejected(self):
        code,_=run([{"id":"a","baseline_correct":True,"memory_correct":True}],{"art_approved":True})
        self.assertNotEqual(code,0)
    def test_blender_edit_rejected(self):
        code,_=run([{"id":"a","baseline_correct":True,"memory_correct":True}],{"any_blender_edits":True})
        self.assertNotEqual(code,0)
if __name__=="__main__":unittest.main()
