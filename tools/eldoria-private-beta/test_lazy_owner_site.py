import json
import tempfile
import unittest
from pathlib import Path
from prepare_lazy_owner_site import (
    FROZEN_SOURCE, BETA14_SOURCE, BETA14_REVISION,
    LAZY_HOST_MODE, verify_source_proofs, verify_beta14_shell,
)

class LazyOwnerSiteEvidenceTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        root=Path(self.temp.name)
        self.proof=root/"proof"
        self.playmode=root/"play"
        self.proof.mkdir()
        self.playmode.mkdir()
        (self.proof/"native-journey").mkdir()
        self.sha="a"*40
        self.put(self.playmode/"lazy-world-native-result.json", {
            "source_head":self.sha,"unity_tests_passed":True,
            "restored":True,"frozen_ux_files":19,"hot_source_base":FROZEN_SOURCE})
        self.put(self.proof/"warm-cache-restoration.json", {
            "restored":True,"conflicts":[],"status_changes":[],"existing_input_changes":[]})
        self.put(self.proof/"source-overlay.json",{
            "overlay_commit":self.sha,"files":[{"mode":LAZY_HOST_MODE}]})
        files=[{"path":"Unity/Assets/Eldoria/UI/A"+str(i)+".cs"} for i in range(17)]
        files+= [
            {"path":"Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV1/Core/RoyalContracts.cs"},
            {"path":"Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs"}
        ]
        self.put(self.proof/"ux-source-overlay.json",{"files":files})
        self.put(self.proof/"comparison.json",{
            "status":"NATIVE_PARITY_PASS_NOT_MOBILE_CERTIFICATION",
            "passes":[{"parity":True},{"parity":True}]})
        self.put(self.proof/"native-journey"/"integration-result.json",{
            "status":"INTEGRATION_PASS_ART_REVIEW_REQUIRED"})
        self.put(self.proof/"web-build.json",{
            "status":"WEB_COMPILED_CURRENT_INTEGRATED_SCENE_NOT_PHYSICAL_IPHONE_TESTED","source_sha":self.sha})
    def put(self,path,obj):
        path.write_text(json.dumps(obj))
    def mutate(self,path,patch):
        data=json.loads(path.read_text())
        data.update(patch)
        self.put(path,data)
    def test_all_real_certificates_required_for_owner_package(self):
        result=verify_source_proofs(self.proof,self.playmode,self.sha)
        self.assertEqual(result["native"]["status"],"INTEGRATION_PASS_ART_REVIEW_REQUIRED")
    def test_reject_skipped_unrun_playmode_or_wrong_head(self):
        path=self.playmode/"lazy-world-native-result.json"
        self.mutate(path,{"unity_tests_passed":False})
        with self.assertRaisesRegex(ValueError,"PlayMode"):verify_source_proofs(self.proof,self.playmode,self.sha)
        self.mutate(path,{"unity_tests_passed":True,"source_head":"b"*40})
        with self.assertRaisesRegex(ValueError,"PlayMode"):verify_source_proofs(self.proof,self.playmode,self.sha)
    def test_reject_unrestored_project_and_missing_original_ux(self):
        path=self.proof/"warm-cache-restoration.json"
        self.mutate(path,{"conflicts":["WorldRoot.cs"]})
        with self.assertRaisesRegex(ValueError,"restored"):verify_source_proofs(self.proof,self.playmode,self.sha)
        self.mutate(path,{"conflicts":[]})
        ux=self.proof/"ux-source-overlay.json"
        self.mutate(ux,{"files":[{"path":"fake"}]})
        with self.assertRaisesRegex(ValueError,"19"):verify_source_proofs(self.proof,self.playmode,self.sha)
    def test_reject_mismatched_lazy_source_and_webgl(self):
        path=self.proof/"source-overlay.json"
        self.mutate(path,{"files":[{"mode":"CITY_ONLY"}]})
        with self.assertRaisesRegex(ValueError,"lazy host"):verify_source_proofs(self.proof,self.playmode,self.sha)
        self.mutate(path,{"files":[{"mode":LAZY_HOST_MODE}]})
        path=self.proof/"web-build.json"
        self.mutate(path,{"source_sha":"b"*40})
        with self.assertRaisesRegex(ValueError,"WebGL"):verify_source_proofs(self.proof,self.playmode,self.sha)
    def test_reject_non_owner_confirmed_beta14_shell(self):
        shell=Path(self.temp.name)/"shell"
        shell.mkdir()
        for name in ("index.html","style.css","city.png","play.js"):
            (shell/name).write_bytes(b"frozen example shell")
        self.put(shell/"release-identity.json",{
            "revision":BETA14_REVISION,"source_sha":BETA14_SOURCE,
            "release_for_external_testers":False})
        self.assertEqual(len(verify_beta14_shell(shell)),4)
        self.mutate(shell/"release-identity.json",{"source_sha":"0"*40})
        with self.assertRaisesRegex(ValueError,"beta14"):verify_beta14_shell(shell)

if __name__=="__main__":
    unittest.main()
