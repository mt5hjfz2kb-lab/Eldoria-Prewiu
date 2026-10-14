"""No-creative-rewrite regressions for Qwen terrain and real source Z lookup."""
import importlib.util
import math
from pathlib import Path
import unittest

SCRIPT=Path(__file__).resolve().parents[1]/"terrain_bounds_contract.py"
spec=importlib.util.spec_from_file_location("terrain_bounds_contract",SCRIPT)
contract=importlib.util.module_from_spec(spec)
spec.loader.exec_module(contract)


def limits():
    return contract.verified_knot_ranges(
        {"local_aabb":{"min":[-120,-120,-5.91],"max":[120,120,8.15]}})


def original_plan():
    return {
        "schema_version":1,
        "observed_defect":"Source terrain has a visually flat, synthetic grass carpet.",
        "design_intent":"Create distinct asymmetrical chasm and fractured shoulder from inherited source.",
        "asset_decision":"MODIFY",
        "planned_actions":[
            {"id":"chasm","type":"sculpt_path","target":"ZoneOne continuous prairie source",
             "rationale":"Use an actual long curved depression to shape the Brecha.",
             "parameters":{"knots":[
                 {"center_xy":{"x":-72,"y":32},"vertical_displacement_m":-9,"radius_m_choice":"18"},
                 {"center_xy":{"x":-40,"y":86},"vertical_displacement_m":-14,"radius_m_choice":"24"},
                 {"center_xy":{"x":24,"y":45},"vertical_displacement_m":-10,"radius_m_choice":"20"}]}},
            {"id":"shoulder","type":"sculpt_path","target":"ZoneOne continuous prairie source",
             "rationale":"Establish a non-parallel fractured geological counter-ridge.",
             "parameters":{"knots":[
                 {"center_xy":{"x":72,"y":32},"vertical_displacement_m":7,"radius_m_choice":"18"},
                 {"center_xy":{"x":40,"y":74},"vertical_displacement_m":12,"radius_m_choice":"24"},
                 {"center_xy":{"x":-12,"y":44},"vertical_displacement_m":9,"radius_m_choice":"20"}]}}
        ]}


class SourceContractTests(unittest.TestCase):
    def test_real_source_bounds_preserved(self):
        actual=limits()
        self.assertEqual(actual[0],(-72,72))
        self.assertEqual(actual[1],(32,86))
        self.assertEqual(actual[2],(-5,7))
        self.assertTrue(contract.inside_limits([25,60,1],actual))
        self.assertFalse(contract.inside_limits([25,60,12],actual))
        self.assertIn("Z=-5..7",contract.limits_label(actual))

    def test_invalid_source_inventory_fails_closed(self):
        with self.assertRaises(ValueError):contract.verified_knot_ranges({})
        with self.assertRaises(ValueError):contract.verified_knot_ranges(
            {"local_aabb":{"min":[2,0,0],"max":[1,1,1]}})
        with self.assertRaisesRegex(ValueError,"does not overlap"):
            contract.verified_knot_ranges(
                {"local_aabb":{"min":[-20,-100,1],"max":[20,-10,2]}})

    def test_model_schema_has_no_invented_center_height(self):
        props=contract.model_knot_schema(limits())["properties"]
        self.assertIn("center_xy",props)
        self.assertNotIn("center_xyz",props)
        self.assertEqual(set(props["center_xy"]["properties"]),{"x","y"})
        self.assertEqual(props["vertical_displacement_m"]["minimum"],-14)
        self.assertEqual(props["radius_m_choice"]["enum"],list(contract.RADIUS_CHOICES))

    def test_xy_are_genuinely_authored_and_source_bounded(self):
        self.assertEqual(contract.artist_xy_positions(original_plan(),limits()),
                         [[-72,32],[-40,86],[24,45],[72,32],[40,74],[-12,44]])
        wrong=original_plan()
        wrong["planned_actions"][0]["parameters"]["knots"][0]["center_xy"]["x"]=999
        with self.assertRaisesRegex(ValueError,"exceeds"):
            contract.artist_xy_positions(wrong,limits())

    def test_exact_artist_decisions_are_never_rewritten(self):
        original=original_plan()
        source_z=[.4,2.3,-.5,1.1,3.2,0.9]
        normalized=contract.normalize_artist_authored_knots(original,source_z)
        steps=[k for a in normalized["planned_actions"] for k in a["parameters"]["knots"]]
        self.assertEqual(steps[0],{"center":[-72,32,.4],"delta":[0,0,-9],"radius":18})
        self.assertEqual(steps[1],{"center":[-40,86,2.3],"delta":[0,0,-14],"radius":24})
        self.assertEqual(steps[4],{"center":[40,74,3.2],"delta":[0,0,12],"radius":24})
        self.assertIn("center_xy",original["planned_actions"][0]["parameters"]["knots"][0])
        self.assertEqual(original["planned_actions"][0]["parameters"]["knots"][1]["vertical_displacement_m"],-14)

    def test_invalid_radii_source_z_and_delta_are_rejected(self):
        bad=original_plan()
        bad["planned_actions"][0]["parameters"]["knots"][2]["radius_m_choice"]="15"
        with self.assertRaisesRegex(ValueError,"radius"):
            contract.artist_xy_positions(bad,limits())
        with self.assertRaises(ValueError):
            contract.normalize_artist_authored_knots(original_plan(),[0])
        with self.assertRaisesRegex(ValueError,"source Z"):
            contract.normalize_artist_authored_knots(original_plan(),[0,1,2,3,4,float("nan")])
        bad=original_plan()
        bad["planned_actions"][0]["parameters"]["knots"][0]["vertical_displacement_m"]=-30
        with self.assertRaisesRegex(ValueError,"vertical"):
            contract.artist_xy_positions(bad,limits())

    def test_duplicate_ai_knots_still_fail_independent_spatial_gate(self):
        import sys
        root=Path(__file__).resolve().parents[4]
        sys.path.insert(0,str(root/"tools/art-direction/ai-authoring"))
        from artist_plan_validation import inspect_artist_plan
        original=original_plan()
        knots=original["planned_actions"][0]["parameters"]["knots"]
        knots[1]["center_xy"]=dict(knots[0]["center_xy"])
        knots[2]["center_xy"]=dict(knots[0]["center_xy"])
        heights=[.4,.4,.4,1.1,3.2,.9]
        normalized=contract.normalize_artist_authored_knots(original,heights)
        errors=inspect_artist_plan(normalized,"ZoneOne continuous prairie source",
                                   "existing",2,topology={"local_aabb":{
                                       "min":[-120,-120,-6],"max":[120,120,9]}})
        self.assertTrue(any("DISTINCT" in err for err in errors),errors)


if __name__=="__main__":
    unittest.main()
