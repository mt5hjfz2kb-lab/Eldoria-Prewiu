import importlib.util,json,tempfile,unittest
from pathlib import Path
spec=importlib.util.spec_from_file_location("iterate",Path(__file__).with_name("iterate.py"))
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
class Tests(unittest.TestCase):
    def fixture(self):
        return {"screens":{x[0]:{"score":None,"reference_score":None,"evidence":"unverified","reference":"none","high_impact":"none"} for x in module.CFG["screens"]},"choices":{}}
    def test_invalid_option(self):
        p=self.fixture();p["choices"]={"hud_caption":"arbitrary code"}
        with self.assertRaises(ValueError):module.validate_plan(p)
    def test_extra_file_rejected(self):
        p=self.fixture();p["choices"]={"main":"write"}
        with self.assertRaises(ValueError):module.validate_plan(p)
    def test_missing_screen_rejected(self):
        p=self.fixture();p["screens"].pop("name")
        with self.assertRaises(ValueError):module.validate_plan(p)
    def test_actual_literal_replacement(self):
        p=self.fixture();p["choices"]={"hud_caption":"larger"}
        k=module.CFG["knobs"]["hud_caption"]
        with tempfile.TemporaryDirectory() as d:
            root=Path(d);f=root/k["file"];f.parent.mkdir(parents=True)
            f.write_text("\n".join(x[0] for x in k["variants"]["larger"]))
            module.apply(p,root,root/"proof.json")
            for a,b in k["variants"]["larger"]:self.assertIn(b,f.read_text())
    def test_stale_source_rejected(self):
        p=self.fixture();p["choices"]={"hud_caption":"larger"}
        k=module.CFG["knobs"]["hud_caption"]
        with tempfile.TemporaryDirectory() as d:
            root=Path(d);f=root/k["file"];f.parent.mkdir(parents=True);f.write_text("stale")
            with self.assertRaises(ValueError):module.apply(p,root,root/"proof.json")
            self.assertEqual(f.read_text(),"stale")
if __name__=="__main__":unittest.main()
