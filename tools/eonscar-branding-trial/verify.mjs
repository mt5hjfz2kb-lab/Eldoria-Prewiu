import fs from "node:fs";
import assert from "node:assert/strict";
const root = "Unity/Assets/Eldoria/Branding/EonscarTrial/";
const runtime = fs.readFileSync(root + "Runtime/EonscarBrandingTrialOverlay.cs", "utf8");
const editor = fs.readFileSync(root + "Editor/EonscarBrandingTrialEditor.cs", "utf8");
const requireText = (content, value) =>
  assert(content.includes(value), "Required source contract missing: " + value);
for(const v of [
 "public static void Show()", "FindFirstObjectByType<EonscarBrandingTrialOverlay>()",
 "Resources.Load<Texture2D>(LogoResource)", "GraphicRaycaster",
 "canvas.sortingOrder = 30000", "tint.raycastTarget = true",
 "button.onClick.AddListener(Dismiss)", "Object.Destroy(gameObject)",
 "ENTRAR EN VALORIA","LAS CENIZAS DE VALORIA"
]) requireText(runtime,v);
for(const forbidden of [
 "RuntimeInitializeOnLoadMethod", "PlayerPrefs", "SceneManager",
 "BuildPipeline", "PlayerSettings", "Time.timeScale",
 "DestroyAll", "Application.Quit"
]) assert(!runtime.includes(forbidden), "Disallowed in opt-in proof: "+forbidden);
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
if(!fs.existsSync(root+"Resources/EonscarTrial/eonscar-logo.png"))
 console.warn("EONSCAR_VISUAL_ASSET_PENDING: original PNG not yet committed; native art QA prohibited");
console.log("EONSCAR_SOURCE_SANITY_PASS: opt-in, input-blocking, dismissible, no save/scene code; not a Unity build");
