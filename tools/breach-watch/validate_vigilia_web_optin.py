#!/usr/bin/env python3
"""Exclusive approved Vigilia must replace retired UI in every player build.

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
    need("#if !UNITY_EDITOR" in bootstrap,
         "Production installs in every player while native evidence installs explicitly")
    need('if(!IsOptedIn(UnityEngine.Application.absoluteURL))' not in bootstrap,
         "Approved exclusive presentation must not depend on a preview flag")
    need("host && host.Ready && host.Hud" in bootstrap and
         "BreachWatchHud.Install(host)" in bootstrap,
         "Only the initialized canonical game session may be presented")
    need("Canvas.willRenderCanvases+=SuppressRetiredDuringStartup" in bootstrap and
         "if(retired)retired.enabled=false;" in bootstrap,
         "The retired Canvas must stay hidden before initial production rendering")
    need("VIGILIA_PRODUCTION_PRESENTATION_FAILED_NO_LEGACY_FALLBACK" in bootstrap and
         "if(oldCanvas)oldCanvas.enabled=false;" in bootstrap and
         "VIGILIA_OPT_IN_ROLLED_BACK_TO_CANONICAL_HUD" not in bootstrap,
         "Installation failure must remain observable without restoring old UI")
    need("if(legacyCanvas)legacyCanvas.enabled=!productionPresentation && originalCanvas;" in hud and
         "if(legacyRaycaster)legacyRaycaster.enabled=!productionPresentation && originalRaycaster;" in hud,
         "Production destruction must never re-enable old rendering or input")
    print("VIGILIA_EXCLUSIVE_PRODUCTION_BOOTSTRAP_SOURCE_PASS")
    print("NOT A WEBGL BUILD OR IOS RUNTIME CERTIFICATION")


if __name__ == "__main__":
    main()
