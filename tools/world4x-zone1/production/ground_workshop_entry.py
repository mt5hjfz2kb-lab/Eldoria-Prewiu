"""Terrain presentation/normal adaptation of a frozen, read-only general workshop."""
import hashlib, pathlib, sys
import bpy
flag=sys.argv.index('--ground-studio-original');original=pathlib.Path(sys.argv[flag+1]).resolve()
del sys.argv[flag:flag+2]
known={'blender_toolbox.py':'ee11b29732e5fbc592eebc8904b9ae2e95cd0bcc',
       'inspect_source.py':'f8b88d9275503bd21d258b70b603eaa1bff88cf4'}
raw=original.read_bytes();blob=hashlib.sha1(b'blob '+str(len(raw)).encode()+b'\0'+raw).hexdigest()
if original.parent.name!='full-studio' or known.get(original.name)!=blob:
 raise ValueError('Only exact pinned, committed general workshop entry is permitted')
sys.path.insert(0,str(original.parent))
import blender_toolbox as workshop
# A z=-0.1 product-display plane intersects terrain valleys below zero. Ground
# needs no display stand; this only changes presentation, never source geometry.
workshop.ensure_review_stage=lambda:None
camera_original=workshop.camera_override
def camera_override(spec):
 camera_original(spec)
 if bpy.context.scene.camera:bpy.context.scene.camera.data.sensor_fit='VERTICAL'
workshop.camera_override=camera_override
sculpt_original=workshop.sculpt
def sculpt(obj,params):
 count=sculpt_original(obj,params)
 coordinates=[list(v.co) for v in obj.data.vertices]
 if obj.data.has_custom_normals:
  obj.data.normals_split_custom_set([(0.,0.,0.)]*len(obj.data.loops));obj.data.update()
 assert coordinates==[list(v.co) for v in obj.data.vertices]
 return count
workshop.sculpt=sculpt
print('ZONEONE_SCOPED_WORKSHOP_ADAPTER: no display floor, vertical strategy framing, automatic normals after genuine sculpt; pinned core unchanged',flush=True)
if original.name=='inspect_source.py':
 import inspect_source
 inspect_source.main()
else:workshop.main()
