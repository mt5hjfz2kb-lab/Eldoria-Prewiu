import pathlib,bpy,math
from mathutils import Vector,Matrix
saved={k:bpy.app.driver_namespace[k] for k in ['interactive_source','interactive_output','interactive_helpers']}
out=pathlib.Path(saved['interactive_output'])
import hashlib
assert hashlib.sha256((out/'Eldoria_Region1_Production.blend').read_bytes()).hexdigest()=='76312a3482f914cd7fe5bd83c97633a3145f4ff42f9ddce296ba48d12a0f1278'
bpy.ops.wm.open_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'))
bpy.app.driver_namespace.update(saved)
helpers=pathlib.Path(saved['interactive_helpers'])
preview=helpers/'artist_preview.py'
exec(compile(preview.read_text(encoding='utf-8'),str(preview),'exec'))
script=helpers.parent/'sculpt_living_geography.py'
code=script.read_text(encoding='utf-8').split("bpy.app.driver_namespace['artist_keep_interior_lights']=True",1)[1]
bpy.app.driver_namespace['artist_keep_interior_lights']=True
exec(compile(code,str(script)+'-review-only','exec'))
