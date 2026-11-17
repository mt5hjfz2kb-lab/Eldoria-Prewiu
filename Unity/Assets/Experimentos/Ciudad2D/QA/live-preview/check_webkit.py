#!/usr/bin/env python3
"""Safari/WebKit engine compatibility proof; NOT a physical iPhone test or Unity PlayMode."""
import json
import os
from pathlib import Path
from playwright.sync_api import sync_playwright

URL = os.environ.get("CITY2D_TEST_URL", "http://127.0.0.1:8765/QA/live-preview/index.html")
OUT = Path(__file__).resolve().parents[1] / "evidence-webkit"
OUT.mkdir(parents=True, exist_ok=True)
report = []

with sync_playwright() as p:
    browser = p.webkit.launch(headless=True)
    for name, width, height in [("iphone_portrait", 390, 844), ("iphone_landscape", 844, 390)]:
        context = browser.new_context(
            viewport={"width": width, "height": height},
            device_scale_factor=1,
            is_mobile=True,
            has_touch=True,
        )
        page = context.new_page()
        errors = []
        page.on("pageerror", lambda err: errors.append(str(err)))
        page.goto(URL, wait_until="load", timeout=30000)
        page.wait_for_function(
            "window.city2dDebug && window.city2dDebug.state.ready && window.city2dDebug.state.frames>3",
            timeout=45000
        )
        state = page.evaluate("window.city2dDebug.state")
        assert state["imageCount"] == 24, state
        assert state["terrainName"] == "Terreno_Organico.png", state
        assert state["buildingCount"] == 6 and state["decorCount"] == 32, state
        assert state["landscape"] == (width > height), state
        page.screenshot(path=str(OUT / f"{name}-bastion-I.png"))

        page.locator("#terrainCompare").tap()
        assert page.evaluate("window.city2dDebug.state.terrainName") == "Terreno.png"
        page.locator("#terrainCompare").tap()
        assert page.evaluate("window.city2dDebug.state.terrainName") == "Terreno_Organico.png"

        page.locator("#tier5").tap()
        assert page.evaluate("window.city2dDebug.state.stage") == 5
        state5 = page.evaluate("window.city2dDebug.state")
        assert state5["visibility"]["cantera"]["phase"] == "available"
        assert state5["visibility"]["salon"]["phase"] == "locked"
        page.screenshot(path=str(OUT / f"{name}-bastion-V.png"))

        selected = False
        for wx, wy in [(460, 732), (469, 765), (465, 795), (495, 770)]:
            sx, sy = page.evaluate(
                "([x,y])=>window.city2dDebug.worldToScreen(x,y)", [wx,wy]
            )
            if not (5 < sx < width-5 and 62 < sy < height-70):
                continue
            page.touchscreen.tap(sx, sy)
            if page.evaluate("window.city2dDebug.state.selected") == "bastion":
                selected = True
                break
        assert selected, f"Real touch did not select Bastion in {name}"
        assert not errors, (name, errors)
        report.append({
            "name": name, "engine": "Playwright WebKit (Safari-like, NOT physical iPhone)",
            "viewport": [width, height], "loaded_sprites": state["imageCount"],
            "terrain_ab_pass": True, "bastion_level_switch_pass": True,
            "real_touch_bastion_pass": selected,
            "locked_hero_hall_absent": True, "errors": errors
        })
        context.close()
    browser.close()

(OUT / "webkit-proof.json").write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8")
print(json.dumps(report, indent=2, ensure_ascii=False))
