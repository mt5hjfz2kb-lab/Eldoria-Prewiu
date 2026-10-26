import pathlib,bpy
helpers=pathlib.Path(bpy.app.driver_namespace['interactive_helpers'])
script=helpers.parent/'finish_canyon_shading.py'
exec(compile(script.read_text(encoding='utf-8'),str(script),'exec'))
