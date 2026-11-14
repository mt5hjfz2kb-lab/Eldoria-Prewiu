import fs from "node:fs";
import assert from "node:assert/strict";
const root = "Unity/Assets/Eldoria/Branding/EonscarTrial/";
const runtime = fs.readFileSync(root + "Runtime/EonscarBrandingTrialOverlay.cs", "utf8");
const editor = fs.readFileSync(root + "Editor/EonscarBrandingTrialEditor.cs", "utf8");
const worldInput = fs.readFileSync("Unity/Assets/Eldoria/Scripts/Presentation/SlicePresenter.cs", "utf8");
const requireText = (content, value) =>
  assert(content.includes(value), "Required source contract missing: " + value);
for(const v of [
 "public static void Show()", "FindFirstObjectByType<EonscarBrandingTrialOverlay>()",
 "Resources.Load<Texture2D>(LogoResource)", "GraphicRaycaster",
 "canvas.sortingOrder = 30000", "tint.raycastTarget = true",
 "inputSink.transition = Selectable.Transition.None",
 "inputSink.navigation = new Navigation { mode = Navigation.Mode.None }",
 "button.onClick.AddListener(Dismiss)", "Object.Destroy(gameObject)",
 "ENTRAR EN VALORIA","LAS CENIZAS DE VALORIA"
]) requireText(runtime,v);
for(const forbidden of [
 "RuntimeInitializeOnLoadMethod", "PlayerPrefs", "SceneManager",
 "BuildPipeline", "PlayerSettings", "Time.timeScale",
 "DestroyAll", "Application.Quit"
]) assert(!runtime.includes(forbidden), "Disallowed in opt-in proof: "+forbidden);
requireText(worldInput,"GetComponentInParent<Selectable>()!=null");
requireText(worldInput,"IsPointerOverInteractiveUi(point)");
requireText(editor,'[MenuItem("Eldoria/EONSCAR/Mostrar prueba sobre la partida")]');
requireText(editor,"if (!EditorApplication.isPlaying)");
requireText(editor,"EonscarBrandingTrialOverlay.Show()");
const needed = [
 "Unity/Assets/Eldoria/Branding.meta",
 "Unity/Assets/Eldoria/Branding/EonscarTrial.meta",
 root+"Editor.meta",root+"Runtime.meta",
 root+"Resources.meta",root+"Resources/EonscarTrial.meta",
 root+"Runtime/EonscarBrandingTrialOverlay.cs.meta",
 root+"Editor/EonscarBrandingTrialEditor.cs.meta"
];
const guids=[];
for(const p of needed){
 const t=fs.readFileSync(p,"utf8");
 assert.match(t,/^fileFormatVersion: 2/m,"Invalid Unity meta: "+p);
 const m=t.match(/^guid: ([0-9a-f]{32})$/m);
 assert(m, "Missing GUID: "+p);
 guids.push(m[1]);
}
assert.equal(new Set(guids).size,guids.length,"Repeated Unity GUIDs");
const artPath = root + "Resources/EonscarTrial/eonscar-logo.png";
const artMeta = artPath + ".meta";
assert(fs.existsSync(artPath), "Missing exact PNG trial asset");
const png = fs.readFileSync(artPath);
assert.equal(png.subarray(0, 8).toString("hex"), "89504e470d0a1a0a", "Not a PNG file");
assert.equal(png.readUInt32BE(16), 512, "Wrong QA PNG width");
assert.equal(png.readUInt32BE(20), 384, "Wrong QA PNG height");
assert.equal(
  (await import("node:crypto")).createHash("sha256").update(png).digest("hex"),
  "c5ac3f2688196548daea6c61e7d0654f3b0cff0c613c78b44a12b4d72bbeab72",
  "Trial logo hash does not match the curated source");
assert.match(fs.readFileSync(artMeta,"utf8"), /guid: b6d1a7c910ae49efa9243570bcda5ba4/);
console.log("EONSCAR_SOURCE_SANITY_PASS: opt-in, asset PNG hash verified, input-blocking, dismissible; not a Unity build");
