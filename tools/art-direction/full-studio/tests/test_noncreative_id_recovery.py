"""Failure regression: local Qwen tracking IDs must not abort real art authoring.

ID repair may not invent artistic decisions, change operations, object names,
numbers, materials, or bypass the Blender schema/contract.
"""
import copy
import hashlib
import json
import sys
import tempfile
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import artist_loop
from contract import RejectedPlan, validate_plan


def source_plan():
    return {
        "schema_version": 2, "asset_id": "valoria-tower-recovery",
        "observed_defect": "The inherited tower silhouette needs stronger articulated upper edges.",
        "design_intent": "Improve actual inherited stone geometry instead of replacing the original tower.",
        "asset_decision": "MODIFY",
        "expected_visual_difference": "Inherited upper stone silhouette becomes visibly less monotonous from player camera.",
        "mobile_risks": "Preserve the original mobile triangle budget.",
        "actions": [
            {"id": "dup", "op": "sculpt_anchor", "target": "Valoria tower source",
             "reason": "Art-direct the real top parapet outline in the existing mesh.",
             "params": {"anchor": "z_max", "direction": "+Z", "distance": 0.8, "radius": 2.5}},
            {"id": "dup", "op": "sculpt_anchor", "target": "Valoria tower source",
             "reason": "Visibly adjust a second existing tower wall feature.",
             "params": {"anchor": "x_max", "direction": "-X", "distance": 0.95, "radius": 3.0}},
        ]
    }


class RecoveryTests(unittest.TestCase):
    def test_repairs_duplicate_tracking_id_only(self):
        original = source_plan()
        doc = copy.deepcopy(original)
        updated, repairs = artist_loop.repair_noncreative_action_ids(doc)
        self.assertEqual(updated["actions"][0]["id"], "dup")
        self.assertEqual(updated["actions"][1]["id"], "step_002")
        self.assertEqual(len(repairs), 1)
        self.assertEqual(repairs[0]["scope"], "NON_ARTISTIC_LOG_IDENTIFIER_ONLY")
        for field in ("op", "target", "reason", "params"):
            self.assertEqual(updated["actions"][0][field], original["actions"][0][field])
            self.assertEqual(updated["actions"][1][field], original["actions"][1][field])
        validate_plan(updated)

    def test_invalid_tracking_id_not_a_silent_blender_command(self):
        doc = source_plan()
        doc["actions"][0]["id"] = "bad id; rm -rf /"
        doc["actions"][1]["op"] = "execute_python"
        corrected, fixes = artist_loop.repair_noncreative_action_ids(doc)
        self.assertEqual(len(fixes), 1)
        self.assertNotIn(";", corrected["actions"][0]["id"])
        with self.assertRaises(RejectedPlan):
            validate_plan(corrected)

    def test_recovery_never_overwrites_a_future_valid_id(self):
        doc = source_plan()
        doc["actions"][0]["id"] = ""
        doc["actions"][1]["id"] = "step_001"
        corrected, fixes = artist_loop.repair_noncreative_action_ids(doc)
        self.assertEqual(corrected["actions"][0]["id"], "step_001_")
        self.assertEqual(corrected["actions"][1]["id"], "step_001")
        self.assertEqual(len(fixes), 1)
        validate_plan(corrected)

    def test_raw_model_response_retained_before_noncreative_repair(self):
        doc = source_plan()
        raw = json.dumps(doc)
        args = SimpleNamespace(
            max_actions=2, max_tokens=900,
            endpoint="http://127.0.0.1:11434/api/chat",
            model="qwen3-vl:2b-instruct",
            brief="Improve inherited Valoria tower",
            asset_category="architecture", phase="mesh_edit")
        with tempfile.TemporaryDirectory() as directory:
            trial = Path(directory)
            with patch.object(artist_loop, "image_data", return_value="image"), patch.object(
                    artist_loop, "request_ollama", return_value=raw):
                result = artist_loop.ask_plan(args, Path("reference"), None,
                                               "", "artist", "", trial, 1,
                                               True, ["Valoria tower source"])
            self.assertEqual(result["actions"][1]["id"], "step_002")
            self.assertEqual((trial / "qwen-raw-attempt-1.json").read_text(), raw)
            audit = json.loads((trial / "metadata-repairs-attempt-1.json").read_text())
            self.assertEqual(audit["raw_response_sha256"],
                             hashlib.sha256(raw.encode()).hexdigest())
            self.assertEqual(audit["scope"], "NON_ARTISTIC_LOG_IDENTIFIERS_ONLY")


if __name__ == "__main__":
    unittest.main()
