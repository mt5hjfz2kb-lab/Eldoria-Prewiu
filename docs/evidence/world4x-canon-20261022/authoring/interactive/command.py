import pathlib,bpy
helpers=pathlib.Path(bpy.app.driver_namespace['interactive_helpers'])
preview=helpers/'artist_preview.py'
exec(compile(preview.read_text(encoding='utf-8'),str(preview),'exec'))
bpy.app.driver_namespace['interactive_snapshot']('before-macro')
script=helpers.parent/'finish_world_macro.py'
exec(compile(script.read_text(encoding='utf-8'),str(script),'exec'))
