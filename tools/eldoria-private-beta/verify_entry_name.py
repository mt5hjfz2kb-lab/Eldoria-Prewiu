"""Verify the nickname actually rendered after a real browser reload."""
import sys,json,hashlib,subprocess,re
from pathlib import Path
from PIL import Image
p=Path(sys.argv[1]);image=Image.open(p);scale=image.width/390
crop=image.crop(tuple(round(v*scale) for v in (38,1,142,22))).resize((624,126),Image.Resampling.BICUBIC)
target=p.with_name('reloaded-name-crop.png');crop.save(target)
text=subprocess.check_output(['tesseract',str(target),'stdout','--psm','7'],text=True).strip()
matched=re.sub(r'\s+','',text).casefold()=='betaqa'
result={'method':'OCR of visible HUD after actual page reload','text':text,'matched':matched,'screenshot_sha256':hashlib.sha256(p.read_bytes()).hexdigest()}
p.with_name('name-result.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))
assert matched,'Reloaded game did not visibly restore expected nickname'
