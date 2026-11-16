#!/usr/bin/env python3
"""Prueba real de Chromium para el comparador HTML. NO es PlayMode de Unity."""
from pathlib import Path
import base64
import json
import os
import re
import shutil
from PIL import Image, ImageChops
from playwright.sync_api import sync_playwright

root = Path(__file__).resolve().parent
asset_dirs = [root/'../../Sprites', root/'../../Unity/Assets/Experimentos/Ciudad2D/Sprites']
sprites = next((d.resolve() for d in asset_dirs if (d/'Terreno.png').exists()), None)
assert sprites is not None, 'Sprite source not found'
output = root/'evidence-web'
output.mkdir(parents=True,exist_ok=True)
html = (root/'index.html').read_text(encoding='utf8')
assets = {p.name:'data:image/png;base64,'+base64.b64encode(p.read_bytes()).decode('ascii') for p in sprites.glob('*.png')}
assert len(assets) == 8, 'Expected 8 artwork files'
html = re.sub(r"const base='[^']+';", "const base='';const imageData="+json.dumps(assets,separators=(',',':'))+';', html, count=1)
html = html.replace('i.src=base+name','i.src=imageData[name]')
browser_path = os.environ.get('CITY2D_BROWSER') or shutil.which('chromium') or shutil.which('google-chrome')

with sync_playwright() as pw:
    browser = pw.chromium.launch(headless=True, executable_path=browser_path, args=['--no-sandbox']) if browser_path else pw.chromium.launch(headless=True, args=['--no-sandbox'])
    for width,height in [(390,844),(844,390),(1170,2532)]:
        page=browser.new_page(viewport={'width':width,'height':height},device_scale_factor=1)
        errors=[]
        page.on('pageerror',lambda e: errors.append(str(e)))
        page.set_content(html,wait_until='domcontentloaded',timeout=45000)
        page.wait_for_function('window.__city2dQA?.state.loaded === true',timeout=25000)
        before = page.evaluate('window.__city2dQA.state')
        assert before['level']==1 and before['loaded']
        first=output/f'viewport-{width}x{height}-I.png'
        fifth=output/f'viewport-{width}x{height}-V.png'
        page.screenshot(path=str(first))
        h5=page.get_by_role('button',name='Mostrar Bastión nivel 5')
        assert h5.bounding_box()['height']>=44, 'Bastion controls below touch target'
        h5.click()
        assert page.evaluate('window.__city2dQA.state.level')==5
        page.screenshot(path=str(fifth))
        assert ImageChops.difference(Image.open(first).convert('RGB'),Image.open(fifth).convert('RGB')).getbbox(), 'No visual tier change'
        # Probar el collider web aproximado del Bastion en tres pantallas.
        coord=page.evaluate('window.__city2dQA.project(470,694)')
        page.locator('#map').click(position=coord, force=True)
        assert page.evaluate('window.__city2dQA.state.selected')=='bastion', 'Wrong building selection'
        assert page.locator('#detail').inner_text().find('hito V')>=0
        z=page.evaluate('window.__city2dQA.state.camera.z')
        page.get_by_role('button',name='Acercar').click()
        assert page.evaluate('window.__city2dQA.state.camera.z')>z
        page.get_by_role('button',name='Centrar').click()
        assert abs(page.evaluate('window.__city2dQA.state.camera.z')-1)<1e-7
        assert not errors,errors
        print(f'BROWSER_PASS {width}x{height} click/hito/zoom/home/touch-targets')
        page.close()
    browser.close()
print('INTERACTIVE_BROWSER_SMOKE_PASS: 3 viewport sizes, no Unity claim')