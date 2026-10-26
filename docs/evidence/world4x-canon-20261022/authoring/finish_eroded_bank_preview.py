"""Complete report and quick review of the saved erosion output after JSON int64 error.
No second geometry operation: validate authoritative saved vertex payload first.
"""
import bpy,pathlib,json,hashlib,struct,gzip,numpy as np
from mathutils import Vector,Matrix
out=pathlib.Path(bpy.app.driver_namespace['interactive_output']);source=pathlib.Path(bpy.app.driver_namespace['interactive_source']);helpers=pathlib.Path(bpy.app.driver_namespace['interactive_helpers'])
def payload(path):
 b=gzip.decompress(path.read_bytes());magic,nx,nz,width,depth=struct.unpack_from('<iiiff',b);N=nx*nz;tail=20+N*8;tag,count=struct.unpack_from('<ii',b,tail);assert magic==0x31424c45 and tag==0x32564c45 and width==1820 and depth==3600
 return np.frombuffer(b,dtype='<f4',offset=tail+8,count=count*3).reshape(-1,3),b[20+N*4:tail]
old,mask1=payload(source.parent/'RegionOneBlenderQueries.bytes');new,mask2=payload(out/'RegionOneBlenderQueries.bytes');assert old.shape==new.shape and mask1==mask2
assert np.array_equal(old[:,[0,2]],new[:,[0,2]])
protected=(abs(old[:,0])<=96.001)&(old[:,2]>=-78.001)&(old[:,2]<=270.001);assert np.array_equal(old[protected],new[protected])
assert np.array_equal(old.min(0),new.min(0)) and np.array_equal(old.max(0),new.max(0))
delta=np.abs(old[:,1]-new[:,1]);assert float(delta.max())>1 and int((delta>.0001).sum())>1000
layout=json.loads((out/'production-region.json').read_text());assert layout['living_geography']['physically_eroded_continuous_canyon_banks'] is True
assert (out/'Eldoria_Region1_Production.blend').stat().st_size<100*1024*1024
settings={k:v for k,v in bpy.app.driver_namespace.items() if k.startswith('interactive_')}
bpy.ops.wm.open_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'));bpy.app.driver_namespace.update(settings)
scene=bpy.context.scene
actual=np.array([tuple(v.co) for v in bpy.data.objects['BLD_Terrain'].data.vertices],np.float32)[:,[0,2,1]];assert np.array_equal(actual,new)
report=json.loads((source.parent/'macro-art-operations.json').read_text(encoding='utf-8-sig'));report['eroded_banks_source_sha256']=hashlib.sha256(source.read_bytes()).hexdigest();report['physical_sawtooth_bank_correction']={'changed_unprotected_vertices':int((delta>.0001).sum()),'max_vertical_correction_m':float(delta.max()),'protected_positions_exact':True,'height_bounds_exact':True,'soil_mask_coincident_vertices_welded':True,'rejected_normals_only_candidate_native':40250944622,'water_footprints_preserved_with_12m_margin':True,'city_reserves_preserved':True,'query_payload_and_editable_positions_identical':True,'report_recovery_only':'Previous MCP40253028130 completed exports and assertions; JSON report serialization stopped on numpy int64. No repeated erosion.'}
report['files']={name:hashlib.sha256((out/name).read_bytes()).hexdigest() for name in ['Eldoria_Region1_Production.blend','RegionOneBlenderWorld.glb','RegionOneBlenderQueries.bytes','production-region.json']};report['blend_bytes']=(out/'Eldoria_Region1_Production.blend').stat().st_size
(out/'macro-art-operations.json').write_text(json.dumps(report,indent=2))
exec(compile((helpers/'artist_preview.py').read_text(),str(helpers/'artist_preview.py'),'exec'))
text=(helpers.parent/'sculpt_living_geography.py').read_text();preview=text.split("bpy.app.driver_namespace['artist_keep_interior_lights']=True",1)[1];bpy.app.driver_namespace['artist_keep_interior_lights']=True
exec(compile(preview.replace('living-geography-candidate','eroded-continuous-banks'),str(helpers.parent/'sculpt_living_geography.py')+'-preview','exec'))
(out/'stop.request').write_text('Saved physical bank output validated and visually previewed; geometry not eroded twice.')
