"""Synthetic fail-closed wrapper tests; actual beta13 artifact is checked in CI."""
import json
import tempfile
import unittest
from pathlib import Path
from stage_postready_profile import stage


class P0StageTest(unittest.TestCase):
    def prepare(self, root):
        site = root / "site"
        site.mkdir()
        (site / "Build").mkdir()
        for p in ("code.wasm.unityweb", "framework.js.unityweb", "loader.js"):
            (site / "Build" / p).write_bytes(b"ORIGINAL_ENGINE_" + p.encode())
        (site / "play.js").write_text(
            'window.__eldoriaDiagnosticRevision="beta13";\n'
            'const instance=await createUnityInstance(canvas,{'
        )
        (site / "index.html").write_text(
            '<script src="play-0123456789abcdef.js"></script>'
        )
        (site / "release-identity.json").write_text(json.dumps({
            "source_sha": "7ae5416613480ba526a839b429459d53629041e5",
            "revision": "native325-mobile13-prebaked-collision-qa",
            "physical_mobile_tested": False,
            "release_for_external_testers": False
        }))
        module = root / "postready_profile.js"
        module.write_text(
            "/* eldoria_p0 */ function installEldoriaPostreadyProfile(){}"
        )
        return site, module

    def test_only_shell_changes(self):
        with tempfile.TemporaryDirectory() as d:
            site, module = self.prepare(Path(d))
            before = {p.name: p.read_bytes() for p in (site / "Build").iterdir()}
            new = stage(site, module)
            self.assertIn(new, (site / "index.html").read_text())
            self.assertIn("installEldoriaPostreadyProfile(window,document)",
                          (site / "play.js").read_text())
            self.assertEqual(before, {p.name: p.read_bytes()
                              for p in (site / "Build").iterdir()})
            self.assertFalse(json.loads((site / "release-identity.json").read_text())
                             ["release_for_external_testers"])

    def test_wrong_source_rejected(self):
        with tempfile.TemporaryDirectory() as d:
            site, module = self.prepare(Path(d))
            identity = site / "release-identity.json"
            obj = json.loads(identity.read_text())
            obj["source_sha"] = "untrusted"
            identity.write_text(json.dumps(obj))
            with self.assertRaises(AssertionError):
                stage(site, module)

    def test_missing_script_anchor_rejected(self):
        with tempfile.TemporaryDirectory() as d:
            site, module = self.prepare(Path(d))
            (site / "index.html").write_text("<title>no script</title>")
            with self.assertRaises(AssertionError):
                stage(site, module)


if __name__ == "__main__":
    unittest.main()
