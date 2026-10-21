#!/usr/bin/env python3
"""Zero-credit non-image regression of exact SOURCE-BOUNDED Qwen creature authoring.
No Ollama, Blender, Tripo account, Unity or model generation.
"""
import pathlib
import sys
import unittest

ROOT = pathlib.Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools/art-direction/ai-authoring"))
from author_brief import creature_spatial_scale_errors  # noqa: E402

SOURCE = {"schema_version": 1, "target": "Spawnling_Near_Authored",
          "local_aabb": {"min": [-.484305, -.377675, -.392411],
                         "max": [.500755, .378969, .391876]}}


def plan(center, radius, delta, rationale="Improve the facial head silhouette"):
    return {"design_intent": "Improve the head and face of the existing Spawnling for gameplay camera",
            "planned_actions": [{
                "id": "source-Qwen", "type": "sculpt_region",
                "target": "Spawnling_Near_Authored", "rationale": rationale,
                "parameters": {"center": center, "radius": radius, "delta": delta}
            }]}


class CreatureGates(unittest.TestCase):
    def test_reject_historical_micro_center_edit_that_was_invisible(self):
        errors = creature_spatial_scale_errors(plan([0, 0, 0], .088, [0, 0, .015]), SOURCE, "creature")
        self.assertTrue(any("INVISIBLE_MICROSCULPT" in e for e in errors), errors)
        self.assertTrue(any("HEAD_MISLOCALIZED" in e for e in errors), errors)

    def test_reject_global_source_translation(self):
        errors = creature_spatial_scale_errors(plan([0, 0, 0], 10.24, [.5, .5, .5]), SOURCE, "creature")
        self.assertTrue(any("GLOBAL_SCULPT" in e for e in errors), errors)
        self.assertTrue(any("GLOBAL_SHIFT" in e for e in errors), errors)

    def test_allow_model_authored_forward_region_as_candidate_only(self):
        self.assertEqual(
            creature_spatial_scale_errors(plan([.30, 0, .14], .18, [0, .03, .045]), SOURCE, "creature"), [])
        # Passing the gate is not ART_APPROVED: a genuine Unity/art review is still required.

    def test_no_overfit_to_terrain_or_other_kinds(self):
        self.assertEqual(
            creature_spatial_scale_errors(plan([0, 0, 0], .088, [0, 0, .015]), SOURCE, "terrain"), [])
        self.assertEqual(
            creature_spatial_scale_errors(plan([0, 0, 0], .088, [0, 0, .015]), SOURCE, "mineral_prop"), [])

    def test_detect_missing_real_mesh_aabb(self):
        errors = creature_spatial_scale_errors(plan([.30, 0, .14], .18, [0, .03, .045]), {}, "creature")
        self.assertTrue(any("SOURCE_BOUNDS_MISSING" in e for e in errors))

    def test_versioned_creature_role_exists_without_modifying_canonical_environment_role(self):
        assert (ROOT / "tools/tripo-art-brain/CREATURE_ARTIST_SYSTEM_ADDENDUM_V1.md").is_file()
        text=(ROOT / "tools/tripo-art-brain/CREATURE_ARTIST_SYSTEM_ADDENDUM_V1.md").read_text()
        self.assertIn("rostro", text.lower())
        self.assertIn("Blender", text)
        self.assertIn("2/307200", text)
        self.assertIn("0.088", text)


if __name__ == "__main__":
    unittest.main()
