"""Export sprites for future Unity import: no modification to a Unity project."""
from pathlib import Path
from PIL import Image
p=Path(__file__).resolve().parent.parent
out=p/'unity_export'
out.mkdir(exist_ok=True)
for rarity in ['common','rare','epic','legendary']:
 s=p/'assets'/f'frame_{rarity}.webp'
 Image.open(s).convert('RGBA').save(out/f'frame_{rarity}.png',compress_level=3)
print('Unity sprite candidates exported:',out)
