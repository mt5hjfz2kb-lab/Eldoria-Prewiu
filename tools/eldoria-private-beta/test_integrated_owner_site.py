import gzip
import hashlib
import json
import struct
import tempfile
import unittest
from pathlib import Path
from types import SimpleNamespace

from prepare_integrated_owner_site import package


class IntegratedDeliveryTests(unittest.TestCase):
    """Synthetic transport tests, not genuine Unity or mobile certification."""
    def setUp(self):
        temp = tempfile.TemporaryDirectory()
        self.addCleanup(temp.cleanup)
        self.root = Path(temp.name)
        self.sha = "a" * 40
        self.proof, self.ui, self.shell, self.build = [self.root / x for x in ("proof", "ui", "shell", "build")]
        for folder in (self.proof, self.ui, self.shell, self.build / "Build"):
            folder.mkdir(parents=True)
        self.write(self.proof / "integration-result.json", dict(source=self.sha,
            status="INTEGRATION_PASS_ART_REVIEW_REQUIRED", checks=[
                "continuous native I-V authored objectives complete", "continuous I-V final saved state exact"]))
        (self.ui / "frame.png").write_bytes(b"synthetic screenshot placeholder")
        shot = dict(file="frame.png", sha256=hashlib.sha256((self.ui / "frame.png").read_bytes()).hexdigest())
        for name, count, status in (("BreachWatchDestinationEvidence.json", 16, "VIGILIA_DESTINATIONS_CAPTURED_ART_REVIEW_PENDING"),
                ("BreachWatchIntegratedTreasureEvidence.json", 4, "COMBINED_TREASURE_ENTRY_CALENDAR_TWO_TOUCHES_PASS")):
            screens = []
            for i in range(count):
                file = name + str(i) + ".png"
                (self.ui / file).write_bytes((self.ui / "frame.png").read_bytes())
                screens.append(dict(shot, file=file))
            self.write(self.ui / name, dict(source=self.sha, status=status, saveUnchanged=True,
                                           screenshotCount=count, screens=screens))
        (self.shell / "release-identity.json").write_text('{"synthetic":true}')
        (self.shell / "index.html").write_text('<script src="play.js"></script>')
        (self.shell / "play.js").write_text('window.__eldoriaDiagnosticRevision="old";button.textContent="Diagnóstico · beta 18";')
        (self.shell / "style.css").write_text("body{}")
        (self.shell / "city.png").write_bytes(b"placeholder")
        (self.build / "index.html").write_text("synthetic native index")
        name, bundle = b"data.unity3d", b"unchanged synthetic bundle"
        header = 20 + 12 + len(name)
        self.data = b"UnityWebData1.0\0" + struct.pack("<I", header) + struct.pack("<III", header, len(bundle), len(name)) + name + bundle
        for name, raw in (("engine.data.unityweb", self.data), ("engine.wasm.unityweb", b"synthetic wasm"),
                          ("engine.framework.js.unityweb", b"synthetic framework")):
            (self.build / "Build" / name).write_bytes(gzip.compress(raw, mtime=0))
        (self.build / "Build/engine.loader.js").write_text("synthetic loader")
        files = [p for p in self.build.rglob("*") if p.is_file()]
        self.write(self.proof / "web-build.json", dict(source_sha=self.sha,
            status="WEB_COMPILED_CURRENT_INTEGRATED_SCENE_NOT_PHYSICAL_IPHONE_TESTED",
            bytes=sum(p.stat().st_size for p in files),
            files=["Builds/ValoriaWebPlaytest/" + p.relative_to(self.build).as_posix() for p in files]))
        self.args = SimpleNamespace(source_head=self.sha, revision="integrated-mobile19-owner-qa",
            build_proof=self.proof, ui_proof=self.ui, stable_shell=self.shell,
            stable_identity_sha256=hashlib.sha256((self.shell / "release-identity.json").read_bytes()).hexdigest(),
            webgl_artifact=self.build, native_run="123", output=self.root / "out", work=self.root / "work")

    def write(self, path, data):
        path.write_text(json.dumps(data))

    def test_exact_data_engine_and_unpublished_identity(self):
        package(self.args)
        out = self.args.output
        manifest = json.loads((out / "build-manifest.json").read_text())
        parts = manifest["decoded_parts"][manifest["data"]]
        self.assertEqual(b"".join(gzip.decompress((out / p["url"]).read_bytes()) for p in parts), self.data)
        self.assertEqual((out / "Build/engine.wasm.unityweb").read_bytes(),
                         (self.build / "Build/engine.wasm.unityweb").read_bytes())
        identity = json.loads((out / "release-identity.json").read_text())
        for flag in ("published", "runtime_verified", "physical_mobile_tested", "artwork_approved", "release_for_external_testers"):
            self.assertIs(identity[flag], False)

    def test_rejects_changed_screenshot(self):
        (self.ui / "BreachWatchDestinationEvidence.json0.png").write_bytes(b"changed")
        with self.assertRaisesRegex(ValueError, "Screenshot bytes"):
            package(self.args)
        self.assertFalse(self.args.output.exists())

    def test_rejects_other_source_and_wrapper(self):
        self.args.source_head = "b" * 40
        with self.assertRaisesRegex(ValueError, "Exact native journey"):
            package(self.args)
        self.args.source_head = self.sha
        self.args.stable_identity_sha256 = "0" * 64
        with self.assertRaisesRegex(ValueError, "wrapper identity"):
            package(self.args)


if __name__ == "__main__":
    unittest.main()
