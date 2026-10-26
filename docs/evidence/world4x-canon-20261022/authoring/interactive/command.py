import pathlib,bpy,hashlib
source=pathlib.Path(bpy.app.driver_namespace['interactive_source'])
assert hashlib.sha256(source.read_bytes()).hexdigest()=='fac19536d7915601b330eaa21ab373bcf465edc8f2c8cd45ae1e2057fad7cd5a','Canyon shading requires the final reviewed orientation source'
helpers=pathlib.Path(bpy.app.driver_namespace['interactive_helpers'])
script=helpers.parent/'finish_canyon_shading.py'
exec(compile(script.read_text(encoding='utf-8'),str(script),'exec'))
