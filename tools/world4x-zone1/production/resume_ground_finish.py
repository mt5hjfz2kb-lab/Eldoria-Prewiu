"""Resume the exact baseline and actual Qwen14B plan already produced in run39127306553.
A 175/255 green channel is valid sRGB. A preferred maximum170 is not a source-safety
or artistic approval test. Preserve that warning and evaluate actual paired images;
all finite ranges, source hashes, geometry bounds and protected areas remain checked.
"""
import argparse, hashlib, json, math, pathlib, shutil, sys
import bpy
from mathutils import Vector
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent))
import continue_ground_finish as workshop

BASE=workshop.ROOT/'docs/world4x-zone1/evidence/ground-finish-39126030933'
BASE_HASH='7e77e1f4985fa0ccdbc5d8d29578f8675c7c9a8ed2bbbebf8285fb7a7f93226b'
RAW=workshop.ROOT/'docs/world4x-zone1/evidence/ground-finish-39127306553/qwen-director-raw-3.json'
RAW_BLOB='05d76c03e5a3684f9491e7caab24b4d439b6607a'
IMAGE_HASHES={'global':'9f204820c0ecb3e31ac9fd859dd446c320f2c1f9ec4762858d37fe7fe0985593','regional':'9150671b323938104aee407331a6e84f269743ff3514db23799b89b5b00216ef','local':'c134987db307c1799a4de86bfa386a433b85acf7ac171045fb03477fe2cf1654','mobile':'9c5aadad3b80a4515382aaa0684addfa5f3b457dc199f55ed9b9b96eeac0bbce'}

def retained_original_plan(out):
    data=RAW.read_bytes().replace(b'\r\n',b'\n')
    assert hashlib.sha1(b'blob '+str(len(data)).encode()+b'\0'+data).hexdigest()==RAW_BLOB
    raw=json.loads(data);assert raw['model']=='qwen3:14b' and raw['done']
    plan=json.loads(raw['message']['content']);spec=workshop.schema()
    assert set(plan)==set(spec['required'])
    assert set(plan['palette_srgb'])==set(spec['properties']['palette_srgb']['required'])
    for key,channels in plan['palette_srgb'].items():
        assert len(channels)==3 and all(type(n)==int and 0<=n<=255 for n in channels), 'Invalid actual sRGB'
    assert 2<=len(plan['landforms'])<=3
    for land in plan['landforms']:
        bounds=spec['properties']['landforms']['items']['properties'];assert set(land)==set(bounds)
        for key,value in land.items():assert type(value) in (int,float) and math.isfinite(value) and bounds[key]['minimum']<=value<=bounds[key]['maximum'], 'Landform outside safe source envelope'
    for key in ['macro_scale','dry_amount','micro_strength','slope_rock_start','slope_rock_end']:
        value=plan[key];bounds=spec['properties'][key]
        assert type(value) in (int,float) and math.isfinite(value) and bounds['minimum']<=value<=bounds['maximum'], 'Material unit/range incorrect'
    assert plan['slope_rock_end']>plan['slope_rock_start']
    assert any(abs(x['height'])>=3 for x in plan['landforms'])
    warnings=[]
    try:workshop.validate(plan)
    except ValueError as e:warnings.append(str(e))
    workshop.write(out/'artist-plan.json',plan)
    workshop.write(out/'genuine-author.json',{'model':'qwen3:14b','new_inference':False,'original_inference_run':39127306553,'raw_git_blob':RAW_BLOB,'raw_sha256_lf':hashlib.sha256(data).hexdigest(),'parameters_rewritten':False,'plan_sha256':workshop.digest(out/'artist-plan.json'),'technical_plan_valid':True,'aesthetic_preference_warnings':warnings,'reason_for_trial':'Only meadow_light G175 exceeds an advisory art-brief cap170; it is valid sRGB. Never approve/reject visual quality from an arbitrary five-level colour deviation. Original image comparison, not relaxed artistic acceptance, determines the result.','art_approved':False})
    return plan

def main():
    parser=argparse.ArgumentParser();parser.add_argument('--out',required=True,type=pathlib.Path);args=parser.parse_args(sys.argv[sys.argv.index('--')+1:]);out=args.out.resolve();out.mkdir(parents=True,exist_ok=False)
    assert workshop.digest(workshop.SOURCE)==workshop.EXPECTED
    assert workshop.digest(BASE/'before.blend')==BASE_HASH
    bpy.ops.wm.open_mainfile(filepath=str(BASE/'before.blend'))
    terrain=bpy.data.objects.get(workshop.TARGET)
    if terrain is None:raise RuntimeError('Saved source target absent')
    context=json.loads((BASE/'source-context.json').read_text(encoding='utf-8'));workshop.write(out/'source-context.json',context)
    before=[]
    for label,focus,scale,w,h in workshop.VIEWS:
        source=BASE/('before-'+label+'-blender.png');assert workshop.digest(source)==IMAGE_HASHES[label]
        dest=out/source.name;shutil.copyfile(source,dest)
        cam=bpy.context.scene.camera;target=Vector(focus)
        cam.location=target+Vector((0,-210,300));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=scale;bpy.context.view_layer.update()
        before.append({'view':label,'width':w,'height':h,'ortho_scale':scale,'matrix':[list(r) for r in cam.matrix_world],'sha256':workshop.digest(dest),'filename':dest.name,'reused_exact_verified_png':True})
    workshop.write(out/'baseline-provenance.json',{'blend_sha256':BASE_HASH,'source_run':39126030933,'same_source_camera_light':True,'captures':before})
    plan=retained_original_plan(out);polygons=len(terrain.data.polygons)
    proof=workshop.apply_terrain(terrain,plan);assert len(terrain.data.polygons)==polygons;proof['polygons_before_and_after']=polygons
    workshop.write(out/'geometry-proof.json',proof)
    bpy.ops.wm.save_as_mainfile(filepath=str(out/'sculpted.blend'))
    material=workshop.paint(terrain,plan,out);workshop.write(out/'material-proof.json',material)
    bpy.ops.wm.save_as_mainfile(filepath=str(out/'after.blend'))
    bpy.ops.object.select_all(action='DESELECT');terrain.select_set(True);bpy.context.view_layer.objects.active=terrain
    bpy.ops.export_scene.gltf(filepath=str(out/'ZoneOneGroundFinish.glb'),export_format='GLB',use_selection=True,export_apply=True)
    after=workshop.render(out,'after')
    delta=max(abs(a-b) for ba,aa in zip(before,after) for br,ar in zip(ba['matrix'],aa['matrix']) for a,b in zip(br,ar))
    if delta>1e-5:raise RuntimeError('Paired camera changed: '+str(delta))
    workshop.write(out/'result.json',{'state':'SOURCE_CANDIDATE_REQUIRES_INDEPENDENT_VISUAL_REVIEW','source_sha256':workshop.EXPECTED,'candidate_sha256':workshop.digest(out/'ZoneOneGroundFinish.glb'),'plan_sha256':workshop.digest(out/'artist-plan.json'),'geometry':proof,'material':material,'before':before,'after':after,'camera_max_delta':delta,'blender_version':bpy.app.version_string,'unity_executed':False,'art_approved':False,'paid_credits':0})
    assert workshop.digest(workshop.SOURCE)==workshop.EXPECTED
if __name__=='__main__':main()
