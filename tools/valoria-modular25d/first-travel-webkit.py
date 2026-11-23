"""Fresh touch journey on macOS WebKit using Unity's read-only localhost probes."""
import asyncio
import json
import time
import os
import gzip
from urllib.parse import urlparse
from pathlib import Path
from playwright.async_api import async_playwright

OUT = Path("evidence-webkit")
OUT.mkdir(exist_ok=True)


async def main():
    async with async_playwright() as pw:
        browser = await pw.webkit.launch()
        context = await browser.new_context(**pw.devices["iPhone 13"])
        page = await context.new_page()
        # WebKit's local-network privacy prompt is outside Playwright's page.
        # Serve the release bytes through the test context itself.
        async def local_file(route):
            path = Path("mobile") / urlparse(route.request.url).path.lstrip("/")
            if not path.is_file():
                await route.fulfill(status=404,body="missing")
                return
            print("GET",path,flush=True)
            body = path.read_bytes()
            if path.name.endswith((".gz",".unityweb")):
                body = gzip.decompress(body)
            mime = ("text/html" if path.suffix==".html" else "application/wasm" if ".wasm." in path.name
                    else "application/javascript" if ".js." in path.name or path.suffix==".js"
                    else "application/octet-stream")
            await route.fulfill(body=body,content_type=mime)
        await page.route("http://localhost:8824/**",local_file)
        city, region, errors = {}, {}, []
        samples = 0

        def observe(msg):
            nonlocal samples
            for tag, dest in [("VALORIA_JOURNEY ",city), ("VALORIA_REGION ",region)]:
                if tag + "{" in msg.text:
                    dest.update(json.JSONDecoder().raw_decode(msg.text.split(tag,1)[1])[0])
                    if dest is city:
                        samples += 1
                    return
            if 'context lost' in msg.text.lower() or 'GPU_LOST' in msg.text:
                errors.append(msg.text)
            if msg.type in ('error','warning') or 'GPU_' in msg.text or 'REGION' in msg.text or 'ELDORIA' in msg.text:
                print(msg.text[:8000], flush=True)
        page.on("console", observe)
        page.on("pageerror", lambda exc: errors.append(str(exc)))
        page.on("crash", lambda _: errors.append("WebKit page crashed"))

        async def until(predicate, seconds=100):
            deadline = time.monotonic() + seconds
            while time.monotonic() < deadline:
                if errors:
                    raise RuntimeError(errors)
                if predicate():
                    return
                await page.wait_for_timeout(120)
            raise TimeoutError((city, region, await page.title(), (await page.locator("body").inner_text())[:300]))

        def target(name):
            return next((t for t in city.get("taps",[]) if t["name"]==name), None)

        async def touch(item):
            box = await page.locator("canvas").bounding_box()
            await page.touchscreen.tap(box["x"]+item["x"]*box["width"], box["y"]+item["y"]*box["height"])

        try:
            await page.goto("http://localhost:8824/index.html?journey-qa",wait_until="commit",timeout=120000)
            await until(lambda:target("Confirm your name"),240)
            await page.keyboard.type("ValoriaQA", delay=100)
            await page.keyboard.press("Enter")
            await touch(target("Confirm your name"))
            await until(lambda:not target("Confirm your name") and target("Toggle left activities"))
            if not target("Sawmill world nameplate"):
                await touch(target("Toggle left activities"))
                await until(lambda:target("Current real construction objective"))
                await touch(target("Current real construction objective"))
                await until(lambda:target("Close sawmill"))
                await touch(target("Close sawmill"))
                await until(lambda:target("Sawmill world nameplate"))
            await touch(target("Sawmill world nameplate"))
            await until(lambda:target("Execute canonical Build command") and city.get("handVisible"))
            await page.screenshot(path=OUT/"01-mano.png")
            await touch(target("Execute canonical Build command"))
            await until(lambda:city.get("sawmillClockVisible"),10)
            await page.screenshot(path=OUT/"02-obra.png")
            await until(lambda:city.get("sawmill")==1 and target("Visitar Región 1"))
            assert city["level"]==1
            await touch(target("Visitar Región 1"))
            await until(lambda:region.get("sample",0)>0,240)
            await page.wait_for_timeout(1700)
            for _ in range(32):
                if not region.get("blocker") and region.get("canTravel"):
                    break
                sample = region["sample"]
                if region.get("blocker"):
                    await touch({"x":region["blockerX"],"y":region["blockerY"]})
                await until(lambda:region.get("sample",0)>sample)
                await page.wait_for_timeout(300)
            assert region["level"]==1 and region["canTravel"] and not region.get("blocker"), region
            await page.screenshot(path=OUT/"03-region.png")
            before = samples
            def city_arrived():
                return samples>before and city.get("activeScene")=="BastionI_Modular25D" and not city.get("travelCurtainVisible",True)
            for _ in range(24):
                if city_arrived(): break
                sample=region.get("sample",0)
                if region.get("blocker"):
                    await touch({"x":region["blockerX"],"y":region["blockerY"]})
                    await until(lambda:city_arrived() or region.get("sample",0)>sample,20)
                    await page.wait_for_timeout(320)
                elif region.get("canTravel"):
                    await touch(region)
                    await until(lambda:city_arrived() or region.get("blocker"),150)
                else:
                    await until(lambda:city_arrived() or region.get("sample",0)>sample,20)
            await until(city_arrived)
            assert city["level"]==1 and city["sawmill"]==1
            await page.screenshot(path=OUT/"04-regreso.png")
            print("WEBKIT_B1_ROUNDTRIP_AND_CONSTRUCTION_PASS",flush=True)
        finally:
            if not page.is_closed():
                await page.screenshot(path=OUT/"ultimo-estado.png", timeout=8000)
            (OUT/"diagnostico.json").write_text(json.dumps({"city":city,"world":region,"errors":errors}, indent=2))
            await browser.close()

asyncio.run(main())
