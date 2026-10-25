import pathlib,bpy
helpers=pathlib.Path(bpy.app.driver_namespace['interactive_helpers'])
preview=helpers/'artist_preview.py'
exec(compile(preview.read_text(encoding='utf-8'),str(preview),'exec'))
script=helpers.parent/'author_continuous_canyon.py'
exec(compile(script.read_text(encoding='utf-8'),str(script),'exec'))
