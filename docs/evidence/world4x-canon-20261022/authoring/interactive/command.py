import pathlib,bpy
source=pathlib.Path(bpy.app.driver_namespace['interactive_helpers']).parent/'approved_breach_ground.py'
exec(compile(source.read_text(encoding='utf-8'),str(source),'exec'))
