import bpy,json,pathlib,hashlib,numpy as np
out=pathlib.Path(bpy.app.driver_namespace['interactive_output']);old=out.parent/'executive-e5c63ba475cd-20261024'
report={};baseline=None
for file in ['00-continental-observe.blend','02-mineral-hierarchy.blend','Eldoria_Macro_Review_Candidate_NOT_APPROVED.blend']:
 with bpy.data.libraries.load(str(old/file),link=False) as (src,dst):dst.objects=['BLD_Terrain'];dst.images=['Region1 authored meadow and woodland pigment']
 terrain=dst.objects[0];pos=np.array([tuple(v.co) for v in terrain.data.vertices])
 if baseline is None:baseline=pos.copy()
 image=dst.images[0]
 report[file]={'changed_from_observed':int(np.count_nonzero(np.max(abs(pos-baseline),axis=1)>1e-5)),'max_delta_m':float(np.max(abs(pos-baseline))),'modifiers':[m.type for m in terrain.modifiers],'mode':terrain.mode,'image_filepath':image.filepath,'packed_sha256':hashlib.sha256(image.packed_file.data).hexdigest() if image.packed_file else None,'image_source':image.source,'has_shape_keys':bool(terrain.data.shape_keys)}
 bpy.data.objects.remove(terrain,do_unlink=True);bpy.data.images.remove(image)
pigment=old/'02-mineral-pigment.png'
report['saved_pigment_sha256']=hashlib.sha256(pigment.read_bytes()).hexdigest()
(out/'editable-reopen-diagnostic.json').write_text(json.dumps(report,indent=2))
print('EDITABLE_REOPEN_DIAGNOSIS',json.dumps(report))
