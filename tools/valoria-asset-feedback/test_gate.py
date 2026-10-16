import json
import tempfile
import unittest
from pathlib import Path
from unittest import mock
import gate

class FeedbackContractTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.ids, cls.assets, cls.issues=gate.registry()
    def record(self):
        selected=[{"id":i,"sha256":"a"*64,"usage":"candidate_in_scene"} for i in ["tower_square_short", "wall_panel_regular_4m", "merlon_tall"]]
        acknowledgements=[]
        for a in selected:
            entries=[]
            for issue_id in self.assets[a["id"]]["correction_ids"]:
                c=self.issues[issue_id]
                disp=("pending_review" if c["state"] in ("TRIAL_PENDING","OBSERVED_NEEDS_REVIEW") else
                      "preserved_assembly" if c["scope"]=="ASSEMBLY_CONTRACT" else
                      "scene_only" if c["scope"]=="SCENE_LOCAL" else "pending_review")
                entries.append({"id":issue_id,"disposition":disp,"evidence_url":c["proof"]})
            acknowledgements.append({"id":a["id"],"sha256":a["sha256"],"art_approved":False,"decisions":entries})
        return {"selected":selected,"constraints":{"art_approved":False}},{"selected_feedback":acknowledgements}
    def run_files(self, selection, ack):
        with tempfile.TemporaryDirectory() as directory:
            p=Path(directory)
            (p/"selected.json").write_text(json.dumps(selection))
            (p/"ack.json").write_text(json.dumps(ack))
            return gate.verify_selection(p/"selected.json",p/"ack.json")
    def test_catalogue_63(self):
        self.assertEqual(len(self.ids), 63)
    def test_six_real_corrections(self):
        self.assertEqual(len(self.issues), 6)
    def test_selected_ack_pass(self):
        s,a=self.record()
        self.assertEqual(self.run_files(s,a)[0],3)
    def test_prior_correction_not_ignored(self):
        s,a=self.record()
        a["selected_feedback"][0]["decisions"].pop()
        with self.assertRaisesRegex(ValueError,"unacknowledged"):
            self.run_files(s,a)
    def test_version_change_invalidates_old_ack(self):
        s,a=self.record()
        s["selected"][0]["sha256"]="b"*64
        with self.assertRaisesRegex(ValueError,"version changed"):
            self.run_files(s,a)
    def test_scene_specific_adjustment_not_global(self):
        s,a=self.record()
        a["selected_feedback"][0]["decisions"][0]["disposition"]="applied"
        a["selected_feedback"][0]["decisions"][0]["validation_ab_url"]="https://example.com/"
        with self.assertRaisesRegex(ValueError,"disposition"):
            self.run_files(s,a)
    def test_unresolved_not_art_approved(self):
        s,a=self.record()
        a["selected_feedback"][0]["art_approved"]=True
        with self.assertRaisesRegex(ValueError,"art approval"):
            self.run_files(s,a)
    def test_new_feedback_forces_reack(self):
        s,a=self.record()
        orig=gate.registry
        def new_registry():
            ids,assets,issues=orig()
            assets["tower_square_short"]["correction_ids"].append("NEW-DEFECT")
            issues["NEW-DEFECT"]={"id":"NEW-DEFECT","asset_id":"tower_square_short","scope":"ASSET_GLOBAL"}
            return ids,assets,issues
        with mock.patch.object(gate,"registry",new_registry):
            with self.assertRaisesRegex(ValueError,"unacknowledged"):
                self.run_files(s,a)

if __name__=="__main__":
    unittest.main()
