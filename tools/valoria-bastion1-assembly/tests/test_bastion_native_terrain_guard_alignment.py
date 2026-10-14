"""Regression: Bastion A19 actual authoring limits and Unity gate must agree.

No artistic approval here. Road/future-plots safety is more important than
a technically green job; verify its original hard limit is preserved.
"""
import pathlib
import re
import unittest

ROOT=pathlib.Path(__file__).resolve().parents[3]
WORKFLOW=(ROOT/'.github/workflows/valoria-bastion1-player-scene-integration.yml')
SCULPT=(ROOT/'tools/valoria-bastion1-assembly/sculpt_bastion_native_terrain.py')


def guard_value(text,field):
    patterns={
      "max":r'\$land\.max_terrain_delta_m\s+-gt\s+([\d.]+)',
      "protected":r'\$land\.protected_road_and_future_parcels_max_delta_m\s+-gt\s+([\d.]+)',
      "min":r'\$land\.max_terrain_delta_m\s+-lt\s+([\d.]+)',
    }
    values=re.findall(patterns[field],text)
    if len(values)!=1:
        raise AssertionError(f"Expected one bounded {field} native terrain guard, got {values}")
    return float(values[0])


class BastionNativeTerrainGuardTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.workflow=WORKFLOW.read_text(encoding="utf-8")
        cls.sculpt=SCULPT.read_text(encoding="utf-8")

    def test_acceptance_limit_covers_source_producer_not_legacy_half_meter(self):
        result=re.search(r'delta\s*=\s*np\.clip\s*\(\s*delta\s*,\s*([-.\d]+)\s*,\s*([-.\d]+)\s*\)',self.sculpt)
        self.assertIsNotNone(result,"Real native terrain clip must be documented in source")
        minimum,maximum=(float(v) for v in result.groups())
        self.assertLess(minimum,0)
        self.assertEqual(maximum,1.28)
        self.assertGreaterEqual(guard_value(self.workflow,"max"),maximum)
        self.assertLessEqual(guard_value(self.workflow,"max"),maximum+.025)
        self.assertGreater(guard_value(self.workflow,"min"),0)

    def test_road_and_future_building_lots_remain_exactly_protected(self):
        self.assertLessEqual(guard_value(self.workflow,"protected"),0.0005)
        self.assertIn("$land.other_original_geometry_modified -ne $false",self.workflow)
        self.assertIn("$land.new_colliders -ne 0",self.workflow)
        self.assertIn("$land.new_resource_nodes -ne 0",self.workflow)
        self.assertIn("$land.original_source_sha256 -ne $preSha",self.workflow)

    def test_native_witness_stays_art_review_not_auto_promotion(self):
        self.assertIn("no_unapproved_promotion=true",self.workflow)
        self.assertIn("artistic",self.sculpt.lower())
        self.assertIn("artistically_approved=False",self.sculpt)
        self.assertIn("mobile_hardware_certified=False",self.sculpt)


if __name__=="__main__":
    unittest.main()
