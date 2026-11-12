#!/usr/bin/env python3
"""Only explicit WebGL opt-in may replace the legacy Canvas before release.

PASS is source protection, not a WebGL build or iPhone runtime test.
"""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
RUNTIME = ROOT / "Unity/Assets/Eldoria/BreachWatch/Runtime"
bootstrap = (RUNTIME / "BreachWatchBetaBootstrap.cs").read_text()
hud = (RUNTIME / "BreachWatchHud.cs").read_text()


def need(ok, explanation):
    if not ok:
        raise AssertionError(explanation)


def main():
    need((RUNTIME / "BreachWatchBetaBootstrap.cs.meta").is_file(),
         "New bootstrap must have a stable Unity GUID")
    need("#if UNITY_WEBGL && !UNITY_EDITOR" in bootstrap,
         "Beta bootstrap must not silently activate in Unity Editor or APK")
    need('const string Flag = "eldoria_vigilia";' in bootstrap,
         "Explicit preview URL flag is required")
    need('key==Flag && val=="1"' in bootstrap,
         "Only the exact URL query opt-in may activate new UI")
    need('if(!IsOptedIn(UnityEngine.Application.absoluteURL))' in bootstrap,
         "Normal web gameplay must retain its previous UI")
    need("host && host.Ready && host.Hud" in bootstrap and
         "BreachWatchHud.Install(host)" in bootstrap,
         "Only a real fully initialized canonical game session may be presented")
    need("VIGILIA_OPT_IN_ROLLED_BACK_TO_CANONICAL_HUD" in bootstrap and
         "Destroy(partial.gameObject)" in bootstrap,
         "Failed beta opt-in must restore the old Canvas")
    need("if(legacyCanvas)legacyCanvas.enabled=originalCanvas;" in hud and
         "if(legacyRaycaster)legacyRaycaster.enabled=originalRaycaster;" in hud,
         "OnDestroy must undo temporary native UI takeover")
    print("VIGILIA_WEBGL_PREVIEW_EXPLICIT_OPT_IN_SOURCE_PASS")
    print("NOT A WEBGL BUILD OR IOS RUNTIME CERTIFICATION")


if __name__ == "__main__":
    main()
