"""Pack textures into existing editable Blender sources without rerendering.
blender -b --python tools/ashen-citadel/pack_sources.py -- artifacts/ashen-blender
"""
import bpy,sys
from pathlib import Path
root=Path(sys.argv[sys.argv.index('--')+1]) if '--' in sys.argv else Path('artifacts/ashen-blender')
for p in sorted(root.glob('*.blend')):
 bpy.ops.wm.open_mainfile(filepath=str(p.resolve()))
 bpy.ops.file.pack_all()
 assert all(im.packed_file for im in bpy.data.images if im.source=='FILE' and im.filepath)
 bpy.context.preferences.filepaths.save_version=0
 bpy.ops.wm.save_as_mainfile(filepath=str(p.resolve()))
 print('ASHEN_SELF_CONTAINED_BLEND',p.name,flush=True)
