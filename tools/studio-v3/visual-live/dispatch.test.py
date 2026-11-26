import importlib.util,json,pathlib,tempfile,unittest,zipfile,io
from unittest.mock import patch
import dispatch as d
class Tests(unittest.TestCase):
 def test_safe_archive(self):
    with tempfile.TemporaryDirectory() as tmp:
        root=pathlib.Path(tmp);f=root/"test.zip"
        with zipfile.ZipFile(f,"w") as z:z.writestr("role-inputs/test.json","{}")
        d.extract_zip(f,root/"out")
        self.assertEqual((root/"out/role-inputs/test.json").read_text(),"{}")
 def test_traversal_rejected(self):
    with tempfile.TemporaryDirectory() as tmp:
        root=pathlib.Path(tmp);f=root/"test.zip"
        with zipfile.ZipFile(f,"w") as z:z.writestr("../escape","bad")
        with self.assertRaises(ValueError):d.extract_zip(f,root/"out")
 def test_plan_bound_to_actual_record(self):
    raw,g,b=d.validate_parent()
    self.assertEqual(g["max_total_usd"],"2.00")
    self.assertEqual(b["reservations"][0]["request_sha256"],d.s.sha(raw))
 def test_reject_outside_github(self):
    with patch.dict(d.os.environ,{},clear=True):
        with self.assertRaises(ValueError):d.preflight("unused")
if __name__=="__main__":unittest.main(verbosity=2)
