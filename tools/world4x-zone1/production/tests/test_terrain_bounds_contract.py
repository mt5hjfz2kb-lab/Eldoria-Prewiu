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


    def test_named_scalar_model_schema_has_axis_specific_ranges(self):
        limits=contract.verified_knot_ranges(
            {"local_aabb":{"min":[-100,0,-1],"max":[100,90,2]}})
        schema=contract.model_knot_schema(limits)
        prop=schema["properties"]["center_xyz"]["properties"]
        self.assertEqual(prop["x"]["minimum"],-72.0)
        self.assertEqual(prop["y"]["maximum"],86.0)
        self.assertEqual(prop["z"]["maximum"],1.99)
        self.assertEqual(schema["properties"]["vertical_displacement_m"]["minimum"],-14)
        self.assertEqual(schema["required"],["center_xyz","vertical_displacement_m","radius"])

    def test_serialize_only_artist_picked_numbers_no_creative_rewrite(self):
        raw={"planned_actions":[{
          "id":"path1","type":"sculpt_path","parameters":{
            "knots":[{"center_xyz":{"x":-35.5,"y":68.25,"z":0.75},
                      "vertical_displacement_m":-10.25,"radius":23.0}]}}]}
        normalized=contract.normalize_artist_authored_knots(raw)
        knot=normalized["planned_actions"][0]["parameters"]["knots"][0]
        self.assertEqual(knot,{"center":[-35.5,68.25,0.75],
                                "delta":[0,0,-10.25],"radius":23.0})
        self.assertIn("center_xyz",raw["planned_actions"][0]["parameters"]["knots"][0])
        self.assertEqual(raw["planned_actions"][0]["parameters"]["knots"][0]["vertical_displacement_m"],-10.25)

    def test_invalid_coordinate_does_not_get_clamped_or_invented(self):
        doc={"planned_actions":[{"parameters":{"knots":[
          {"center_xyz":{"x":"oops","y":66,"z":0},"vertical_displacement_m":-10,"radius":24}
        ]}}]}
        with self.assertRaisesRegex(ValueError,"invalid numeric"):
            contract.normalize_artist_authored_knots(doc)


    def test_real_failure_pattern_repeated_knot_centers_remains_rejected(self):
        import sys
        root=Path(__file__).resolve().parents[4]
        sys.path.insert(0,str(root/"tools/art-direction/ai-authoring"))
        from artist_plan_validation import inspect_artist_plan
        typed={"planned_actions":[{
          "id":"chasm","type":"sculpt_path","target":"ZoneOne continuous prairie source",
          "rationale":"Sculpt a genuinely original non-linear chasm path",
          "parameters":{"knots":[
            {"center_xyz":{"x":-72,"y":32,"z":-5},"vertical_displacement_m":-9,"radius":18},
            {"center_xyz":{"x":-72,"y":32,"z":-5},"vertical_displacement_m":-14,"radius":25},
            {"center_xyz":{"x":-72,"y":32,"z":-5},"vertical_displacement_m":-14,"radius":20}]}}]}
        typed.update({
            "observed_defect":"Terrain reads as uniform flat grass with repeated fake beige stripes.",
            "design_intent":"Sculpt a visibly winding chasm on genuinely inherited source terrain.",
            "asset_decision":"MODIFY"})
        normalized=contract.normalize_artist_authored_knots(typed)
        errors=inspect_artist_plan(normalized,"ZoneOne continuous prairie source",
                                   "existing",2,topology={"local_aabb":{
                                     "min":[-120,-120,-6],"max":[120,120,9]}})
        self.assertTrue(any("DISTINCT" in x for x in errors),errors)



if __name__ == "__main__":
    unittest.main()
