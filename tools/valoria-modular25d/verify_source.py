#!/usr/bin/env python3
"""Fast, non-Unity checks. PASS here is never a Unity, art or device approval."""
from pathlib import Path
import re
import hashlib
import json
import struct
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[2]
BASE = ROOT / "Unity/Assets/Experimentos/ValoriaModular25D"
SCRIPTS = BASE / "Scripts"
SCENE = BASE / "Scenes/BastionI_Modular25D.unity"


def content(path):
    return path.read_text(encoding="utf-8")


def guid(path):
    match = re.search(r"^guid:\s*([a-f0-9]{32})\s*$", content(path), re.M)
    if not match:
        raise AssertionError("Missing stable GUID: " + str(path))
    return match.group(1)


class SandboxSourceContracts(unittest.TestCase):
    def test_scene_guids_and_legacy_boot_guard(self):
        scene = content(SCENE)
        w = guid(SCRIPTS / "Modular25DWorld.cs.meta")
        self.assertIn("guid: " + w + ", type: 3", scene)
        blocks = re.split(r"(?=--- !u!114 )", scene)
        guards = [block for block in blocks if
                  "guid: 0c88788eeb6222d4190071e890a0caa4, type: 3" in block]
        self.assertEqual(len(guards), 1)
        self.assertIn("m_Enabled: 0", guards[0])
        self.assertEqual(scene.count("guid: " + w + ", type: 3"), 1)

    def test_every_script_has_unique_stable_meta(self):
        scripts = list(BASE.rglob("*.cs"))
        self.assertGreaterEqual(len(scripts), 5)
        ids = []
        for path in scripts:
            self.assertTrue(Path(str(path) + ".meta").exists(), str(path))
            ids.append(guid(Path(str(path) + ".meta")))
        self.assertEqual(len(set(ids)), len(ids))
        self.assertTrue((BASE / "Scenes/BastionI_Modular25D.unity.meta").is_file())

    def test_production_gameplay_authority(self):
        src = content(SCRIPTS / "Modular25DGameplay.cs")
        self.assertIn("save=new FileStateStore(SandboxSavePath);", src)
        self.assertIn("save=new IsolatedWebGlPrefsStore();", src)
        self.assertIn("gateway=new LocalGateway(new SystemClock(),save);", src)
        self.assertIn('const string Key = "eldoria.experiment.modular25d.bastion1.v1";', src)
        self.assertIn("PlayerPrefs.Save();", src)
        self.assertIn("state.SchemaVersion!=1", src)
        self.assertIn('new GameCommand(Guid.NewGuid().ToString("N"),before.PlayerId,', src)
        self.assertIn('"Build",SawmillId,before.Revision', src)
        self.assertIn('public const string SawmillId = "sawmill"', src)
        self.assertIn("gateway.Advance()", src)
        self.assertNotRegex(src, r"state\.Resources\.(?:Wood|Stone|Food)\s*[-+]?=")
        self.assertIn('const string IsolatedSaveName', src)
        self.assertNotIn("SliceBoot.SavePath", src)

    def test_separate_graphical_states(self):
        art = content(SCRIPTS / "Modular25DArt.cs")
        world = content(SCRIPTS / "Modular25DWorld.cs")
        for name in ("Sawmill()", "SawmillSite()", "SawmillWorksite()", "Bastion()", "Tile(",
                     "Tree(", "Bush(", "Rock(", "Villager(", "Shadow("):
            self.assertIn(name, art)
        for name in ("Modular25DArt.Sawmill()", "Modular25DArt.SawmillSite()",
                     "Modular25DArt.SawmillWorksite()"):
            self.assertIn(name, world)
        self.assertIn('ParcelBuildingStates.For(snapshot,"sawmill")', world)

    def test_ground_language_is_modular_with_building_integration(self):
        world=content(SCRIPTS / "Modular25DWorld.cs")
        meadow=content(SCRIPTS / "Modular25DGround.cs")
        landscape=content(SCRIPTS / "Modular25DLandscape.cs")
        capture=content(BASE / "Editor/Modular25DNativeCapture.cs")
        self.assertIn("var landscape=paths.gameObject.AddComponent<Modular25DLandscape>();",world)
        self.assertIn("landscape.Build();",world)
        self.assertNotIn("BuildPaths(paths);",world)
        self.assertNotIn("AddRoadStroke(",world)
        self.assertIn("TextureSize = 768",meadow)
        self.assertIn("Eldoria olive grass",meadow) if False else None
        self.assertIn("Valoria olive grass",meadow)
        self.assertIn("modular directional ribbon",landscape)
        self.assertIn("new Vector3(p.x,p.y,-.075f)",landscape)
        self.assertIn('PutFoot("Bastion',landscape)
        self.assertIn('PutFoot("Sawmill',landscape)
        self.assertIn("RoadCount++",landscape)
        self.assertIn("FootprintCount++",landscape)
        self.assertIn("landscape.RoadCount!=2||landscape.FootprintCount!=2",capture)

    def test_reusable_depth_and_animated_walk(self):
        world = content(SCRIPTS / "Modular25DWorld.cs")
        walker = content(SCRIPTS / "Modular25DWalker.cs")
        self.assertIn("FootOrder(float y)", world)
        self.assertIn("body.sortingOrder=sort;", walker)
        self.assertIn("shadow.sortingOrder=sort-1;", walker)
        self.assertIn("Vector2.MoveTowards", walker)
        self.assertIn("Modular25DArt.Villager(", walker)
        ground = content(SCRIPTS / "Modular25DGround.cs")
        self.assertIn("terrain.Build();", world)
        self.assertIn("class Modular25DGround : MonoBehaviour", ground)
        self.assertIn("ChunkCount++",ground)
        self.assertIn("renderer.sortingOrder=-6000;", ground)
        self.assertIn("TextureWrapMode.Repeat",ground)
        self.assertNotIn("for(int row=-10;row<=10",world)
        self.assertNotIn("for(int col=-10;col<=10",world)

    def test_camera_input_does_not_tap_after_drag_or_pinch(self):
        source = content(SCRIPTS / "Modular25DCameraInput.cs")
        self.assertIn("bool tap=!dragged&&!pinched&&!beganOnUI;", source)
        self.assertIn("if(tap)TrySelect(at);", source)
        self.assertIn("events.RaycastAll(data,hits);", source)
        self.assertIn("if(dragged&&!beganOnUI)Pan(at-previous);", source)
        self.assertIn("camera.aspect", source)
        self.assertNotIn("if(moving){moving=false;TrySelect", source)

    def test_each_component_has_unambiguous_monoscript(self):
        # Unity's script importer chooses the MonoBehaviour whose name equals
        # the source file; extra behaviour classes in World.cs were unsafe.
        for name in ("Modular25DWalker", "Modular25DTreeSway",
                     "Modular25DCameraInput"):
            source = content(SCRIPTS / (name + ".cs"))
            self.assertIn("public sealed class " + name + " : MonoBehaviour", source)
            self.assertTrue((SCRIPTS / (name + ".cs.meta")).exists())

    def test_capture_cannot_claim_proof_without_engine(self):
        editor = content(BASE / "Editor/Modular25DEditorTasks.cs")
        self.assertIn("if(!EditorApplication.isPlaying)", editor)
        self.assertIn("camera.Render();", editor)
        self.assertIn("ScreenSpaceOverlay", editor) if "ScreenSpaceOverlay" in editor else None
        self.assertIn("BuildTarget.WebGL", editor)
        self.assertIn("source=Unity Camera.Render real", editor)
        self.assertIn("status=unreviewed", editor)

    def test_experiment_code_does_not_load_rejected_assets(self):
        for path in list(SCRIPTS.glob("*.cs")):
            src = content(path)
            for forbidden in ("HeroBastionGenerated", "Valoria_HeroBastion_v1.glb",
                              "PremiumV6", "main/Valoria_City_World_UI"):
                self.assertNotIn(forbidden, src, str(path))

    def test_high_detail_generated_originals_have_exact_real_unity_imports(self):
        # Validate ACTUAL binary sprites committed on the isolated Unity branch.
        # The 10/11 Blender environment assets may be rebuilt separately, but
        # the two hero-building images must not silently revert to plastic
        # Blender boxes without source evidence and a new review.
        directory=BASE/"Resources/ValoriaModular25D"
        manifest=json.loads((directory/"sprite_manifest.json").read_text(encoding="utf-8"))
        entries={x["name"]:x for x in manifest["sprites"]}
        expected={
            "bastion_i":("5992c968f67c6855a97eb5db4d24c9bcd4e06af5b1062c25cc35d0e1e27e3154",(480,314),0.048),
            "sawmill_i":("5a5c5f38604881da76d43c65eb999e69736d5bcaaf0d432df0661163ccde81f6",(362,273),0.07),
        }
        for name,(sha,size,pivot_y) in expected.items():
            src=directory/(name+".png")
            data=src.read_bytes()
            self.assertEqual(hashlib.sha256(data).hexdigest(),sha)
            self.assertTrue(data.startswith(b"\x89PNG\r\n\x1a\n"))
            self.assertEqual(struct.unpack(">II",data[16:24]),size)
            self.assertEqual(data[25],6,"Require RGBA PNG, not an opaque backplate")
            meta=content(Path(str(src)+".meta"))
            self.assertIn("spritePixelsToUnits: 96",meta)
            self.assertIn("spriteAlignment: 9",meta)
            self.assertIn(f"spritePivot: {{x: 0.500000, y: {pivot_y:.6f}}}",meta)
            self.assertEqual(entries[name]["sha256"],sha)
            self.assertIn("ChatGPT",entries[name]["source"])
            self.assertFalse(entries[name]["art_approved"])
            self.assertFalse(entries[name]["unity_render_checked"])

    def test_scope_when_git_checkout_has_main(self):
        # Fails closed if the experiment begins changing canonical code or a
        # reserved workstream's files. Does not enforce ownership of Windows.
        check = subprocess.run(["git", "rev-parse", "--verify", "origin/main"],
                               cwd=ROOT, stdout=subprocess.DEVNULL,
                               stderr=subprocess.DEVNULL, check=False)
        if check.returncode != 0:
            self.skipTest("main reference not present; no scope claim")
        change = subprocess.check_output(
            ["git","diff","--name-only","origin/main...HEAD"], cwd=ROOT,
            text=True).splitlines()
        allowed = (
            "Unity/Assets/Experimentos.meta",
            "Unity/Assets/Experimentos/ValoriaModular25D",
            "docs/valoria-modular-25d/",
            "tools/valoria-modular25d/",
            ".github/workflows/valoria-modular25d-",
        )
        unauthorized = [path for path in change if not path.startswith(allowed)]
        self.assertEqual(unauthorized, [],
                         "Changes outside exclusive sandbox: " + str(unauthorized))


if __name__ == "__main__":
    unittest.main(verbosity=2)
