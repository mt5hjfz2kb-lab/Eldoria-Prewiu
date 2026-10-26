import pathlib,bpy,hashlib
source=pathlib.Path(bpy.app.driver_namespace['interactive_source'])
assert hashlib.sha256(source.read_bytes()).hexdigest()=='82c7c0e6a94a27575aba4b87a1843af7e607b31e45478848a65b424578d05daf','Physical erosion requires the exact rejected normals-only candidate'
helpers=pathlib.Path(bpy.app.driver_namespace['interactive_helpers'])
script=helpers.parent/'erode_canyon_banks.py'
exec(compile(script.read_text(encoding='utf-8'),str(script),'exec'))
