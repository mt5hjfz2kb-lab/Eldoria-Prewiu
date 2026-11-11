import json
import gzip
import struct
from types import SimpleNamespace
from prepare_lazy_owner_site import package_owner
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
    def test_synthetic_exact_webgl_bundle_packaging(self):
        # Small fake UnityWebData container; this verifies ALL packaging I/O,
        # manifests, source hashes, frozen shell and no texture substitutions.
        # It is deliberately NOT a Unity build or iPhone gameplay test.
        root=Path(self.temp.name)
        orig=root/"source-webgl"
        (orig/"Build").mkdir(parents=True)
        bundle=b"synthetic-original-data.unity3d\x00" * 25
        filename=b"data.unity3d"
        end=20+12+len(filename)
        raw=(b"UnityWebData1.0\x00"+struct.pack("<I",end)
             +struct.pack("<III",end,len(bundle),len(filename))
             +filename+bundle)
        (orig/"Build/demo.data.unityweb").write_bytes(gzip.compress(raw))
        (orig/"Build/demo.framework.js.unityweb").write_bytes(gzip.compress(b"synthetic framework"))
        (orig/"Build/demo.wasm.unityweb").write_bytes(gzip.compress(b"\x00asm\x01\x00\x00\x00"))
        (orig/"Build/demo.loader.js").write_text("synthetic loader")
        (orig/"index.html").write_text("native synthetic index")
        shell=root/"frozen-beta14-shell"
        shell.mkdir()
        (shell/"index.html").write_text('<html><script src="play.js"></script></html>')
        (shell/"style.css").write_text("body{}")
        (shell/"city.png").write_bytes(b"fake-png-not-rendered")
        (shell/"play.js").write_text('window.__eldoriaDiagnosticRevision="frozen";\nbutton.textContent = "Diagnóstico · beta 14";')
        self.put(shell/"release-identity.json",{
            "revision":BETA14_REVISION,"source_sha":BETA14_SOURCE,
            "release_for_external_testers":False})
        compared=self.proof/"comparison.json"
        self.mutate(compared,{"passes":[{
            "baseline":{"inputAudit":"b"*64},"staged":{"inputAudit":"b"*64}},
            {"baseline":{"inputAudit":"b"*64},"staged":{"inputAudit":"b"*64}}]})
        output=root/"packaged-owner"
        package_owner(SimpleNamespace(
            native_artifact=str(self.proof),playmode_artifact=str(self.playmode),
            webgl_artifact=str(orig),beta14_shell_artifact=str(shell),
            output=str(output),work=str(root/"rebuild"),
            source_head=self.sha,native_run="43000000000",
            revision="native325-mobile15-full-lazy-owner-qa"))
        identity=json.loads((output/"release-identity.json").read_text())
        manifest=json.loads((output/"build-manifest.json").read_text())
        self.assertFalse(identity["texture_payload_modified"])
        self.assertFalse(identity["release_for_external_testers"])
        self.assertFalse(identity["physical_iphone_30min_verified"])
        self.assertFalse(identity["mac_same_binary_ab_verified"])
        self.assertTrue(identity["full_region1_compiled"])
        self.assertEqual(identity["source_sha"],self.sha)
        self.assertTrue(manifest["decoded_parts"][manifest["data"]])
        self.assertEqual(identity["candidate_bundle_sha256"],
                         __import__("hashlib").sha256(bundle).hexdigest())
        self.assertTrue((output/"owner-only-lazy-proof.json").exists())

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
