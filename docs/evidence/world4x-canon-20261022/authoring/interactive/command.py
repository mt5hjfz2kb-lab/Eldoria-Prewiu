import pathlib,bpy
helpers=pathlib.Path(bpy.app.driver_namespace['interactive_helpers'])
script=helpers.parent/'finish_surface_orientation.py'
exec(compile(script.read_text(encoding='utf-8'),str(script),'exec'))
