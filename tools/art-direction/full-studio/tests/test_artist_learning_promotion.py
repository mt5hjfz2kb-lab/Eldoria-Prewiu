"""Fail-closed curation tests; synthetic bytes are fixtures, not quality proof."""
import hashlib
import json
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from learning_context import stage_pending_art_evidence
from curate_learning import curate_case

def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()

class VerifiedCurationTests(unittest.TestCase):
    def make_cycle(self, root):
        root=Path(root);out=root/"cycle-1";stage=out/"output"
        stage.mkdir(parents=True)
        files={
            "plan_file":out/"artist-plan.json",
            "raw_response_file":out/"qwen-raw.json",
            "before_image":stage/"before-blender.png",
            "after_image":stage/"after-blender.png",
            "critique_file":out/"qwen-critique.json",
            "comparison_file":out/"camera-screen-impact.json",
            "output_blend":stage/"artist-asset.blend",
            "output_glb":stage/"artist-asset.glb"}
        for key,p in files.items():
            data={
                "before_image":b"\x89PNG\r\n\x1a\nbefore",
                "after_image":b"\x89PNG\r\n\x1a\nafter",
                "output_blend":b"BLENDERfixture",
                "output_glb":b"glTFfixture"
            }.get(key,key.encode())
            p.write_bytes(data)
        pending=out/"pending-learning-case.json"
        stage_pending_art_evidence(pending,model="eldoria-artist:latest",
            source_hash="f"*64,**files)
        casebook=root/"casebook.json"
        casebook.write_text(json.dumps({"schema_version":1,"cases":[]}))
        review=root/"review.json"
        review.write_text(json.dumps({
            "reviewer_role":"INDEPENDENT_ART_REVIEW",
            "result":"VISUAL_REJECTED",
            "pending_sha256":sha(pending),
            "before_sha256":sha(files["before_image"]),
            "after_sha256":sha(files["after_image"]),
            "run_id":12123,"artifact_id":88888,
            "diagnosis":"The roof is disconnected from the tower and the image does not establish plausible contact.",
            "safe_lesson":"Check whether each roof connects physically to its tower and whether its silhouette improves the actual camera framing."
        }))
        return pending,review,casebook,files
    def test_reviews_need_exact_a_b_and_evidence_and_never_train_weights(self):
        with tempfile.TemporaryDirectory() as temp:
            pending,review,casebook,files=self.make_cycle(temp)
            new=curate_case(pending,review,casebook)
            self.assertEqual(len(new["cases"]),1)
            self.assertEqual(new["cases"][0]["result"],"VISUAL_REJECTED")
            self.assertEqual(new["cases"][0]["raw_response_sha256"],sha(files["raw_response_file"]))
            self.assertFalse(new["cases"][0]["review_provenance"]["unity_verified"])
            self.assertTrue(new["cases"][0]["review_provenance"]["no_model_weight_training"])
            with self.assertRaises(ValueError):curate_case(pending,review,Path(temp)/"missing.json")
            files["after_image"].write_bytes(b"tampered")
            with self.assertRaises(ValueError):curate_case(pending,review,casebook)
    def test_qwen_self_review_cannot_be_promoted(self):
        with tempfile.TemporaryDirectory() as temp:
            pending,review,casebook,_=self.make_cycle(temp)
            doc=json.loads(review.read_text());doc["reviewer_role"]="QWEN_SELF_CRITIQUE"
            review.write_text(json.dumps(doc))
            with self.assertRaises(ValueError):curate_case(pending,review,casebook)

if __name__=="__main__":unittest.main()
