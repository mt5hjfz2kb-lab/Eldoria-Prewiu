"""Offline regression for the repeated out-of-source-Qwen-knots failure.

The *artist* still chooses every knot/delta. The infrastructure may only
constrain future choices using actual inspected source bounds.
"""
import importlib.util
from pathlib import Path
import unittest

SCRIPT = Path(__file__).resolve().parents[1] / "terrain_bounds_contract.py"
spec = importlib.util.spec_from_file_location("terrain_bounds_contract", SCRIPT)
contract = importlib.util.module_from_spec(spec)
spec.loader.exec_module(contract)


class SourceBoundsTests(unittest.TestCase):
    def test_exact_real_source_intersection_follows_local_geometry(self):
        limits = contract.verified_knot_ranges(
            {"local_aabb":{"min":[-120,-120,-1.25],"max":[120,120,1.25]}})
        self.assertEqual(limits[0],(-72.0,72.0))
        self.assertEqual(limits[1],(32.0,86.0))
        self.assertEqual(limits[2],(-1.24,1.24))
        self.assertTrue(contract.inside_limits([22,48,0.1],limits))
        self.assertFalse(contract.inside_limits([22,48,6],limits))
        self.assertIn("Z=-1.24..1.24",contract.limits_label(limits))

    def test_missing_aabb_is_rejected_before_expensive_qwen(self):
        for payload in ({}, {"local_aabb":{"min":[0,0],"max":[1,1]}},
                        {"local_aabb":{"min":[2,0,0],"max":[1,1,1]}}):
            with self.subTest(payload=payload):
                with self.assertRaises(ValueError):
                    contract.verified_knot_ranges(payload)

    def test_nonoverlap_does_not_guess_or_clamp_artistic_values(self):
        with self.assertRaisesRegex(ValueError,"does not overlap"):
            contract.verified_knot_ranges(
                {"local_aabb":{"min":[-20,-100,1],"max":[20,-10,2]}})

    def test_schema_has_true_coordinates_and_stays_source_bounded(self):
        limits = contract.verified_knot_ranges(
            {"local_aabb":{"min":[-60,20,-3],"max":[70,65,4]}})
        schema=contract.coordinate_schema(limits)
        self.assertEqual(schema["maxItems"],3)
        self.assertEqual(schema["prefixItems"][0]["minimum"],-59.99)
        self.assertEqual(schema["prefixItems"][1]["maximum"],64.99)
        self.assertEqual(schema["prefixItems"][2]["minimum"],-2.99)
        self.assertFalse(contract.inside_limits([70,90,-5],limits))
        self.assertFalse(contract.inside_limits([float('nan'),50,0],limits))


if __name__ == "__main__":
    unittest.main()
