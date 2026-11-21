"""Read the released player through its canvas, using visible UI and normal inputs."""
import asyncio, csv, io, json, os, re, shutil, subprocess, sys, time, unicodedata
from pathlib import Path
from playwright.async_api import async_playwright

OUT=Path('world-browser-evidence')
OUT.mkdir(exist_ok=True)
URL=sys.argv[1]
def normalize(value):
    return ''.join(c for c in unicodedata.normalize('NFD',value.lower()) if unicodedata.category(c)!='Mn')

async def main():
    async with async_playwright() as pw:
        browser=await pw.chromium.launch(executable_path=shutil.which('google-chrome') or shutil.which('chromium'),args=['--no-sandbox','--disable-dev-shm-usage','--use-angle=swiftshader','--enable-unsafe-swiftshader'])
        ctx=await browser.new_context(viewport={'width':390,'height':844},has_touch=True,is_mobile=True,device_scale_factor=2,user_agent='Mozilla/5.0 (Linux; Android 14) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Mobile Safari/537.36')
        page=await ctx.new_page()
        logs=[]
        page.on('console',lambda msg:logs.append(msg.text[:600]))
        page.on('pageerror',lambda e:logs.append('ERROR '+str(e)))
        page.on('dialog',lambda dialog:asyncio.create_task(dialog.dismiss()))
        async def labels(path):
            await page.screenshot(path=str(path))
            tsv=subprocess.check_output(['tesseract',str(path),'stdout','-l','eng','--psm','11','tsv'],stderr=subprocess.DEVNULL).decode()
            lines={}
            for r in csv.DictReader(io.StringIO(tsv),delimiter='\t'):
                if not r['text'].strip():continue
                k=(r['block_num'],r['par_num'],r['line_num'])
                lines.setdefault(k,[]).append(r)
            result=[]
            for words in lines.values():
                text=' '.join(r['text'] for r in words)
                x1=int(words[0]['left']);x2=int(words[-1]['left'])+int(words[-1]['width'])
                y=int(words[0]['top'])+int(words[0]['height'])/2
                result.append((normalize(text),(x1+x2)/4,y/2))
            return result
        try:
            await page.goto(URL,wait_until='domcontentloaded',timeout=200000)
            await page.wait_for_selector('canvas',state='visible',timeout=200000)
            for _ in range(120):
                await page.wait_for_timeout(3000)
                startup=await labels(OUT/'startup.png')
                if any(re.search(r'nombre|valoria|aserradero|name|reino|jugar|bastion|entrar|continue',line) for line,_,_ in startup):
                    break
            else:
                raise RuntimeError("The startup canvas did not present readable game UI.")
            history=[]; named=False;edited=False
            for i in range(24):
                visible=await labels(OUT/f'{i:02}.png')
                text=' | '.join(w[0] for w in visible)
                command=None
                def find(pattern):return next((w for w in reversed(visible) if re.search(pattern,w[0])),None)
                if not named and ('nombre' in text or 'name' in text or 'nick' in text):
                    if not edited:
                        await page.touchscreen.tap(195,489)
                        await page.keyboard.type('ValoriaQA',delay=120)
                        await page.keyboard.press('Enter')
                        edited=True
                        await page.wait_for_timeout(900)
                        continue
                    command=find(r'confirm|acept|entrar|continu|begin')
                    if command:named=True
                if not command:command=find(r'^(continuar|siguiente|entendido|aceptar|explorar|continue|next|omitir|skip|comenzar)\b')
                if not command:command=find(r'^(construir|build)\b')
                if not command:command=find(r'^(mundo|world|salir al mundo)\b')
                if not command:command=find(r'^aserradero\b')
                if not command:break
                key=(command[0],round(command[1]),round(command[2]))
                if len(history)>2 and history[-2:]==[key,key]:break
                history.append(key);await page.touchscreen.tap(command[1],command[2])
                await page.wait_for_timeout(3200)
            (OUT/'inputs.json').write_text(json.dumps(history,ensure_ascii=False))
            (OUT/'browser.txt').write_text(browser.version)
        finally:
            (OUT/'console.txt').write_text('\n'.join(logs))
            await browser.close()
asyncio.run(main())
