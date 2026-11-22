"""Fresh touch journey on macOS WebKit using Unity's read-only localhost probes."""
import asyncio
import json
import time
from pathlib import Path
from playwright.async_api import async_playwright

OUT = Path("evidence-webkit")
OUT.mkdir(exist_ok=True)


async def main():
    async with async_playwright() as pw:
        browser = await pw.webkit.launch()
        context = await browser.new_context(**pw.devices["iPhone 13"], viewport={"width":390, "height":844})
        page = await context.new_page()
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
            if msg.type == 'error' or 'ELDORIA_MEMORY_PHASE' in msg.text:
                print(msg.text[:600], flush=True)
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
            raise TimeoutError((city, region))

        def target(name):
            return next((t for t in city.get("taps",[]) if t["name"]==name), None)

        async def touch(item):
            box = await page.locator("canvas").bounding_box()
            await page.touchscreen.tap(box["x"]+item["x"]*box["width"], box["y"]+item["y"]*box["height"])

        try:
            await page.goto("http://127.0.0.1:8824/index.html?journey-qa",wait_until="domcontentloaded")
            await until(lambda:target("Confirm your name"),240)
            await page.keyboard.type("ValoriaQA", delay=100)
            await page.keyboard.press("Enter")
            await touch(target("Confirm your name"))
            await until(lambda:not target("Confirm your name") and target("Sawmill world nameplate"))
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
            await touch(region)
            await until(lambda:samples>before and city.get("activeScene")=="BastionI_Modular25D"
                        and not city.get("travelCurtainVisible",True),200)
            assert city["level"]==1 and city["sawmill"]==1
            await page.screenshot(path=OUT/"04-regreso.png")
            print("WEBKIT_B1_ROUNDTRIP_AND_CONSTRUCTION_PASS",flush=True)
        finally:
            (OUT/"diagnostico.json").write_text(json.dumps({"city":city,"world":region,"errors":errors}, indent=2))
            await browser.close()

asyncio.run(main())
