import pathlib,bpy
helpers=pathlib.Path(bpy.app.driver_namespace['interactive_helpers'])
script=helpers.parent/'compact_continuous_source.py'
exec(compile(script.read_text(encoding='utf-8'),str(script),'exec'))
