import pathlib,bpy
saved={k:bpy.app.driver_namespace[k] for k in ['interactive_source','interactive_output','interactive_helpers']}
bpy.ops.wm.open_mainfile(filepath=saved['interactive_source'])
bpy.app.driver_namespace.update(saved)
helpers=pathlib.Path(saved['interactive_helpers'])
preview=helpers/'artist_preview.py'
exec(compile(preview.read_text(encoding='utf-8'),str(preview),'exec'))
script=helpers.parent/'author_continuous_canyon.py'
exec(compile(script.read_text(encoding='utf-8'),str(script),'exec'))
