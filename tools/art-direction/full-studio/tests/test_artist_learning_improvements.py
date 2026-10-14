"""Offline regressions: evidence-based memory, richer vision, and no fake training."""
import base64
import hashlib
import json
import sys
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from learning_context import evidence_case_lessons, stage_pending_art_evidence
import artist_loop


class ArtistLearningImprovementTests(unittest.TestCase):
    def test_casebook_has_relevant_traceable_memory_not_old_action_payload(self):
        casebook = {
            "schema_version":1,
            "cases":[
              {"id":"old", "run_id":12, "evidence_artifact_id":400,
               "raw_response_sha256":"a"*64,
               "result":"TECHNICALLY_EXECUTED_ART_VISUALLY_REJECTED",
               "safe_lesson":"The medieval tower roof must align with supporting masonry and remain readable from the player camera."},
              {"id":"invalid", "safe_lesson":"Invent and replay coordinates from an old job without evidence."}
            ]
        }
        result=evidence_case_lessons(casebook,"tower roof composition",limit=800)
        self.assertEqual(len(result["selected"]),1)
        self.assertEqual(result["selected"][0]["run_id"],12)
        self.assertIn("HISTORICAL OBSERVATION",result["prompt_text"])
        self.assertIn("VISUALLY_REJECTED",result["prompt_text"])
        self.assertNotIn("Invent and replay",result["prompt_text"])
        self.assertFalse(result["independent_art_approval_inferred"])

    def test_casebook_empty_or_corrupted_fails_closed(self):
        with self.assertRaises(ValueError):
            evidence_case_lessons({"schema_version":2},"tower")
        self.assertEqual(evidence_case_lessons({"schema_version":1,"cases":[]},"tower")["selected"],[])

    def test_case_memory_bounded_and_deterministic(self):
        cases=[{"id":str(i),"run_id":i+1,"evidence_artifact_id":100+i,
                "raw_response_sha256":"e"*64,"result":"REJECTED",
                "safe_lesson":"Improve visible tower silhouette and camera placement without copying obsolete construction geometry."}
               for i in range(30)]
        casebook={"schema_version":1,"cases":cases}
        first=evidence_case_lessons(casebook,"tower silhouette",limit=300)
        second=evidence_case_lessons(casebook,"tower silhouette",limit=300)
        self.assertEqual(first,second)
        self.assertLessEqual(len(first["prompt_text"]),300)
        self.assertEqual(len(first["selected"]),1)

    def test_pending_learning_requires_real_files_and_never_approves_itself(self):
        with tempfile.TemporaryDirectory() as t:
            root=Path(t)
            files={}
            for name in ("plan","before","after","critique","comparison","blend","glb"):
                p=root/(name+".dat")
                p.write_bytes((name+"-genuine-fixture").encode())
                files[name]=p
            result=stage_pending_art_evidence(
                root/"pending.json",model="local-qwen",
                source_hash="a"*64,plan_file=files["plan"],
                before_image=files["before"],after_image=files["after"],
                critique_file=files["critique"],comparison_file=files["comparison"],
                output_blend=files["blend"],output_glb=files["glb"])
            self.assertEqual(result["status"],"PENDING_INDEPENDENT_VERIFICATION")
            self.assertFalse(result["art_approved"])
            self.assertFalse(result["weight_training_performed"])
            self.assertEqual(result["evidence"]["plan"]["sha256"],hashlib.sha256(files["plan"].read_bytes()).hexdigest())
            self.assertEqual(json.loads((root/"pending.json").read_text())["status"],result["status"])
            files["glb"].unlink()
            with self.assertRaises(ValueError):
                stage_pending_art_evidence(
                    root/"other.json",model="local-qwen",source_hash="a"*64,
                    plan_file=files["plan"],before_image=files["before"],
                    after_image=files["after"],critique_file=files["critique"],
                    comparison_file=files["comparison"],output_blend=files["blend"],
                    output_glb=files["glb"])

    def test_images_keep_aspect_and_offer_1024px_for_model(self):
        try:
            from PIL import Image
        except ImportError:
            self.skipTest("Pillow optional in bare Python environment")
        with tempfile.TemporaryDirectory() as t:
            image=Path(t)/"source.png"
            Image.new("RGB",(1800,900)).save(image)
            encoded=artist_loop.image_data(image,1024)
            from io import BytesIO
            with Image.open(BytesIO(base64.b64decode(encoded))) as decoded:
                self.assertEqual(decoded.size,(1024,512))
            with self.assertRaises(ValueError):
                artist_loop.image_data(image,4096)

    def test_current_artist_role_does_not_truncate_artistic_method(self):
        source=(Path(__file__).resolve().parents[1]/"artist_loop.py").read_text()
        self.assertNotIn("canonical_role[:530]",source)
        self.assertIn('role=(canonical_role+',source)
        self.assertIn('case_context=evidence_case_lessons',source)
        self.assertIn('stage_pending_art_evidence(',source)


if __name__=="__main__":
    unittest.main()
