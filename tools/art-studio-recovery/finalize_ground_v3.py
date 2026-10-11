"""Review matched Blender render against exported GLB and prior scene."""
import json
from pathlib import Path
from PIL import Image, ImageDraw
import numpy as np

root = Path(__file__).resolve().parents[2]
out = root / 'art-source/valoria/production/studio-rocks-v3-ground'
before = Image.open(out / 'before-overview.png').convert('RGB')
after = Image.open(out / 'after-overview.png').convert('RGB')
roundtrip = Image.open(out / 'roundtrip-overview.png').convert('RGB')
board = Image.new('RGB', (1280 * 3, 842), (31, 36, 44))
d = ImageDraw.Draw(board)
for index, (label, source) in enumerate((('V2: roca y suelo anterior', before), ('V3: suelo CC0 en Blender', after), ('V3: GLB reimportado en Blender', roundtrip))):
    board.paste(source, (1280 * index, 42))
    d.text((1280 * index + 14, 13), label, fill='white')
board.save(out / 'Blender-V2-V3-GLB-Comparacion.png')
a = np.asarray(after).astype(np.float32)
b = np.asarray(roundtrip).astype(np.float32)
mae = float(np.mean(np.abs(a - b)))
(out / 'roundtrip-report.json').write_text(json.dumps({
    'GLB_reimported': True, 'grass_materials': 4,
    'same_camera_render_mean_absolute_RGB_error_0_to_255': round(mae, 3),
    'visual_pass': False, 'unity_tested': False,
    'director_observation': 'Less flat grass and real scanned rock; repeated boulders and visible cliff seam remain. Retain study, not final art acceptance.'
}, indent=2))
print('GLB round-trip RGB MAE:', mae)
