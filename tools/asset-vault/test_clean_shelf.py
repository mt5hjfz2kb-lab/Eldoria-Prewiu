"""Guarantee the production-facing asset library does not expose archive/vendor clutter."""
import importlib.util,json,unittest
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
TOOL=ROOT/"tools/asset-vault/find-reuse.py"
spec=importlib.util.spec_from_file_location("vault_find",TOOL)
mod=importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)

class ProductionShelfTests(unittest.TestCase):
    def test_shelf_is_bounded_and_classified(self):
        shelf=json.loads((ROOT/"pipeline/asset-vault/clean-production-shelf-v1.json").read_text())
        self.assertEqual([len(g["items"]) for g in shelf["groups"]],[5,20,1])
        self.assertEqual(shelf["counts"]["files_in_active_shelf"],25)

    def test_default_returns_only_clean_shelf(self):
        result=mod.search("muralla piedra",limit=200)
        self.assertEqual(result["mode"],"CLEAN_SHELF_ONLY")
        self.assertFalse(any(x["source"]=="CURRENT_TREE" for x in result["top"]))
        self.assertFalse(any(x["source"]=="HISTORICAL_TREE" for x in result["top"]))
        self.assertTrue(all(x["verdict"] not in
                        ("REJECT_CURRENT_VISUAL","REJECT_FOR_CURRENT_CANON","THIRD_PARTY_LICENSE_UNVERIFIED")
                            for x in result["top"]))

    def test_building_production_asset_found(self):
        found=mod.search("aserradero",limit=100)
        self.assertTrue(any(x["name"]=="Valoria_Aserradero_AP2_v1" for x in found["top"]))

    def test_research_may_look_up_archived_sources(self):
        full=mod.search("cantera",limit=5,research=True)
        self.assertEqual(full["mode"],"FULL_RESEARCH_UNREVIEWED")
        self.assertTrue(full["historical_loaded"])

    def test_restricted_landmark_not_present_in_default(self):
        found=mod.search("GateStreetRiseRock",limit=100)
        self.assertFalse(any(x["name"]=="GateStreetRiseRock_MV1" for x in found["top"]))

if __name__=="__main__":unittest.main()
