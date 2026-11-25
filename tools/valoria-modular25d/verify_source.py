#!/usr/bin/env python3
"""Fast, non-Unity checks. PASS here is never a Unity, art or device approval."""
from pathlib import Path
import re
import base64
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

    def test_premium_ui_assets_keep_vigilia_original_bytes_and_licenses(self):
        # The isolated static job sparse-checks out the experiment, NOT the
        # canonical game art. Verify byte-exact copies by original Git blob
        # hashes, without including or changing the other workstream's files.
        owned=BASE/"Resources/ValoriaModular25D/UI"
        canonical_blob_sha1={
            "slate_resource.png":"eb55936d2e3089aca10d4535bcab2d837fd6472e",
            "brass_portrait.png":"d519dfee8c4c6719feae47423e6023b5f5783e35",
            "profile_plate.png":"51dd2c0399d42e85e3a783b1470ab20b66f030a8",
            "burgundy_action.png":"6b6928a2c93053c16b94fbc6b22b03aa3ddce3c9",
            "Fonts/CormorantGaramond-SemiBold.ttf":"c30cb04d87c0abdb7bb86db19272d6790ed46758",
            "Fonts/SourceSans3-Semibold.ttf":"4aff9a3aea9b5f1e746f03ed40e1b293740dc8a2",
            "Fonts/OFL.txt":"5b8e7415d1dfb3e3167a837c523a90f089903202",
            "Fonts/OFL_SOURCE_SANS_3.txt":"22c601b82f29fc6bb801445098c9f23ffdca4f94",
        }
        for rel,sha in canonical_blob_sha1.items():
            file=owned/rel
            data=file.read_bytes()
            git_blob=(f"blob {len(data)}\0").encode("utf-8")+data
            self.assertEqual(hashlib.sha1(git_blob).hexdigest(),sha,
                             "Licensed original artwork/font has been altered: "+rel)
            self.assertTrue(Path(str(file)+".meta").is_file())
            self.assertRegex(guid(Path(str(file)+".meta")),r"^[0-9a-f]{32}$")
        for name in ("slate_resource.png","brass_portrait.png","profile_plate.png","burgundy_action.png"):
            self.assertIn("spriteBorder:",content(owned/(name+".meta")))
        self.assertNotEqual(guid(owned/"slate_resource.png.meta"),
                            "90ec0b5c9a2b463194f198a623e5e36c")
        self.assertNotEqual(guid(owned/"Fonts/CormorantGaramond-SemiBold.ttf.meta"),
                            "f80a1bd0b2b74402a57ff14cfe73040a")
        self.assertNotEqual(guid(owned/"Fonts/SourceSans3-Semibold.ttf.meta"),
                            "e34ced249ba448679a6bfe93ba5dc4f4")
        skin=content(SCRIPTS/"Modular25DInterface.cs")
        self.assertIn("image.type=Image.Type.Sliced;",skin)
        self.assertIn("graphic.type=Image.Type.Sliced;",skin)
        self.assertIn("Resources.Load<Font>",skin)

    def test_original_sawyer_binary_textasset_aliases(self):
        directory=BASE/"Resources/ValoriaModular25D"
        proof_path=ROOT/"docs/valoria-modular-25d/WORKER_SAW_BINARY_TEXTASSET_PROOF.json"
        proof=json.loads(proof_path.read_text(encoding="utf8"))
        self.assertTrue(proof["source_assets_unchanged"])
        self.assertFalse(proof["engine_verified"],"Do not invent Unity verification")
        self.assertEqual(len(proof["frames"]),2)
        cs=content(SCRIPTS/"Modular25DArt.cs")
        self.assertIn('Resources.Load<TextAsset>("ValoriaModular25D/"+key+"_png")',cs)
        self.assertIn("ImageConversion.LoadImage(tex,packed.bytes,false)",cs)
        names=set()
        for entry in proof["frames"]:
            name=entry["name"]
            names.add(name)
            original=(directory/(name+".png")).read_bytes()
            packed=(directory/(name+"_png.bytes")).read_bytes()
            self.assertEqual(packed,original,"Alias differs from original artist PNG: "+name)
            self.assertEqual(hashlib.sha256(packed).hexdigest(),
                             entry["original_and_textasset_sha256"])
            self.assertEqual((entry["geometry"]["width"],entry["geometry"]["height"]),(96,96))
            self.assertEqual(entry["geometry"]["color_type"],6)
            self.assertTrue(entry["source_pixels_unchanged"])
            self.assertEqual(guid(directory/(name+"_png.bytes.meta")),
                             entry["textasset_guid"])
            self.assertIn("TextScriptImporter:",content(directory/(name+"_png.bytes.meta")))
            self.assertNotEqual(guid(directory/(name+".png.meta")),
                                guid(directory/(name+"_png.bytes.meta")))
        self.assertEqual(names,{"worker_saw_0","worker_saw_1"})

    def test_owned_aldric_avatar_is_source_verified(self):
        original=ROOT/"docs/valoria-modular-25d/UMBRAL_II_ALDRIC_PORTRAIT_SOURCE.json"
        d=json.loads(original.read_text(encoding="utf8"))
        self.assertEqual(d["identity"],"Sir Aldric")
        self.assertEqual(d["source_sha256"],
                         "156791ca04f17c45fdc85662a134352902f76229337d02e6210f6dea10c1c685")
        self.assertEqual(d["crop_box_in_512"],[100,130,410,440])
        self.assertTrue(d["not_gameplay"])
        avatar=ROOT/d["output_path"]
        self.assertEqual(hashlib.sha256(avatar.read_bytes()).hexdigest(),d["output_sha256"])
        self.assertEqual(guid(Path(str(avatar)+".meta")),d["unity_guid"])
        ui=content(SCRIPTS/"Modular25DInterface.cs")
        self.assertIn('"ValoriaModular25D/UI/UmbralII/aldric_avatar"',ui)
        self.assertIn('"Authentic Sir Aldric character portrait"',ui)

    def test_original_six_gameplay_icon_png_binary_source(self):
        manifest=ROOT/"docs/valoria-modular-25d/UMBRAL_II_GAMEPLAY_ICONS_SOURCE.json"
        d=json.loads(manifest.read_text(encoding="utf8"))
        self.assertEqual(d["source_palette_sha256"],
                         "0c656ddb672784f6bea83252cf778e83f5985e93fb02e4cc42bc1b3913838695")
        raw=base64.b64decode((ROOT/d["source_file"]).read_text(encoding="ascii"))
        self.assertEqual(hashlib.sha256(raw).hexdigest(),d["source_palette_sha256"])
        packed=ROOT/d["packed_file"]
        data=packed.read_bytes()
        self.assertEqual(hashlib.sha256(data).hexdigest(),d["packed_rgba_sha256"])
        self.assertEqual(len(data),d["packed_bytes"])
        self.assertEqual(struct.unpack(">II",data[16:24]),(144,96))
        self.assertEqual(data[25],6,"Unity runtime must decode standard truecolour RGBA source")
        self.assertEqual(d["cells_row_major_top_down"],
                         ["wood","stone","food","hammer","city","task"])
        self.assertEqual(guid(Path(str(packed)+".meta")),d["textasset_guid"])
        src=content(SCRIPTS/"Modular25DInterface.cs")
        self.assertIn('Resources.Load<TextAsset>(path)',src)
        self.assertIn('originalIconAtlas.width!=144',src)
        self.assertIn('woodIcon=AtlasIcon(0,"wood")',src)
        self.assertIn('taskIcon=AtlasIcon(5,"task")',src)
        self.assertIn('mailboxDock = DockButton("OpenSystem/mail",rootTransform,"CORREO"',src)
        self.assertNotIn('DockButton("Abrir "',src)
        self.assertNotIn('woodIcon=ResourceIcon(',src)

    def test_original_two_frame_sawyer_art_manifest_and_import(self):
        directory=BASE/"Resources/ValoriaModular25D"
        entries={p["name"]:p for p in
                 json.loads((directory/"sprite_manifest.json").read_text(encoding="utf8"))["sprites"]}
        expected={
            "worker_saw_0":("31efdfda261100db7e599ffff6be13912d3144fd2bb271d726e7daaa4736dcdf",8910),
            "worker_saw_1":("00dc4ae6e2eb81563e4fad085322f0721f1275dedc207ce9b74d13b7ef8a6553",8529),
        }
        for name,(sha,bytecount) in expected.items():
            path=directory/(name+".png")
            content_bytes=path.read_bytes()
            self.assertEqual(len(content_bytes),bytecount)
            self.assertEqual(hashlib.sha256(content_bytes).hexdigest(),sha)
            self.assertEqual(struct.unpack(">II",content_bytes[16:24]),(96,96))
            self.assertEqual(content_bytes[25],6,"Losslessly decoded RGBA PNG with alpha, not a backplate")
            meta=(directory/(name+".png.meta")).read_text(encoding="utf8")
            self.assertIn("spritePixelsToUnits: 100",meta)
            self.assertIn("spritePivot: {x: 0.500000, y: 0.060000}",meta)
            self.assertEqual(entries[name]["sha256"],sha)
            self.assertFalse(entries[name]["art_approved"])
            self.assertFalse(entries[name]["unity_render_checked"])
        world=content(SCRIPTS/"Modular25DWorld.cs")
        act=content(SCRIPTS/"Modular25DWorkshopAction.cs")
        self.assertIn("Modular25DArt.Sawyer(0)",world)
        self.assertIn("keyframes[stroke>0f?0:1]",act)
        self.assertNotIn("body.sprite=Modular25DArt.Villager",act)

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
        self.assertIn("paths.gameObject.AddComponent<Modular25DLandscape>();",world)
        self.assertIn("landscape.Build();",world)
        self.assertNotIn("BuildPaths(paths);",world)
        self.assertNotIn("AddRoadStroke(",world)
        self.assertIn("TextureSize = 512",meadow)
        self.assertIn("Valoria olive grass",meadow)
        self.assertIn("modular directional ribbon",landscape)
        self.assertIn("new Vector3(p.x,p.y,-.075f)",landscape)
        self.assertNotIn("PutFoot(",landscape)
        self.assertIn("RoadCount++",landscape)
        terrain=content(BASE / "Resources/ValoriaModular25D/DistrictGround.shader")
        self.assertIn("timberLine=distanceToLane(millGround",terrain)
        self.assertIn("ground.HasMillYard",capture)
        self.assertIn("GetComponentsInChildren<SpriteRenderer>(true).Length!=0",capture)

    def test_original_owned_painterly_grass_is_byte_exact_and_tiled(self):
        # Only grass pixels were sampled from the original Eldoria *concept*
        # image; NEVER a city panorama or any competitor's copyright art.
        root=BASE/"Resources/ValoriaModular25D"
        source=(root/"grass_reference_b64.txt").read_text().strip()
        data=base64.b64decode(source,validate=True)
        self.assertEqual(len(data),826)
        self.assertEqual(hashlib.sha256(data).hexdigest(),
            "7a27f0f1f8b057b20820d8942cbddb44bca973dcf3ccef7c8103d8b4dc708559")
        self.assertTrue(data.startswith(b"\xff\xd8"))
        self.assertTrue(data.endswith(b"\xff\xd9"))
        ground=content(SCRIPTS/"Modular25DGround.cs")
        self.assertIn("MakePaintedSource(source.text)",ground)
        self.assertIn("binary.Length!=826",ground)
        self.assertIn("src.width!=128 || src.height!=128",ground)
        self.assertIn("TextureWrapMode.Repeat",ground)
        self.assertIn("pixels[a]=pixels[b]",ground)

    def test_world_declarations_not_swallowed_by_escaped_comment_newlines(self):
        # Native Unity 44648516764 failed CS0103: prop/mover were swallowed
        # by '//' after literal backslash-n comment injection. Roslyn
        # syntax-only did not catch missing variable declarations.
        src=content(SCRIPTS / "Modular25DWorld.cs")
        malformed=[n for n,line in enumerate(src.splitlines(),1)
                   if line.lstrip().startswith("//") and "\\n" in line]
        self.assertEqual(malformed,[],f"Literal escaped newlines in comments: {malformed}")
        # Controlled original-art ecotone clusters replaced uniform random scatter.
        self.assertIn("var art=Modular25DArt.Tuft((zone+placed)%2);",src)
        self.assertIn("SettlementZonesPlaced=4;",src)
        self.assertIn("CreateWorkshopWorker(people,new Vector2(1.45f,-4.04f));",src)
        self.assertIn("var mover=actor.AddComponent<Modular25DWalker>();",src)
        self.assertIn("var mover=actor.AddComponent<Modular25DWalker>();",src)

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
