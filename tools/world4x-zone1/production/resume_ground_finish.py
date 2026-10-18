"""Resume the real saved baseline after rejected plans; no repeated baseline render.
Uses the installed text director for original typed values, not a copied historical plan.
Reference interpretation by ChatGPT and prior visual-model output are recorded separately.
"""
import argparse, hashlib, json, math, pathlib, shutil, sys, time
import bpy
from mathutils import Vector
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent))
import continue_ground_finish as workshop

BASE=workshop.ROOT/'docs/world4x-zone1/evidence/ground-finish-39126030933'
BASE_HASH='7e77e1f4985fa0ccdbc5d8d29578f8675c7c9a8ed2bbbebf8285fb7a7f93226b'
IMAGE_HASHES={'global':'9f204820c0ecb3e31ac9fd859dd446c320f2c1f9ec4762858d37fe7fe0985593','regional':'9150671b323938104aee407331a6e84f269743ff3514db23799b89b5b00216ef','local':'c134987db307c1799a4de86bfa386a433b85acf7ac171045fb03477fe2cf1654','mobile':'9c5aadad3b80a4515382aaa0684addfa5f3b457dc199f55ed9b9b96eeac0bbce'}

def make_plan(out,context):
    names=[m['name'] for m in workshop.request('/api/tags')['models']]
    if 'qwen3:14b' not in names:raise RuntimeError('Installed text director qwen3:14b absent; no download')
    # Only relevant observations, excluding old sculpt coordinates that the first model copied.
    observations={
      'reviewer':'ChatGPT direct inspection of original PNG bytes on 2026-10-18',
      'reference_global':'Open olive/golden prairie; central purple corrupted earth, southern coast, scattered mountain and forest groups; one coordinate space.',
      'reference_regional':'Broad clear areas for cities; mountains grounded into meadow, gold soil paths, readable nodes. No field of random grass noise.',
      'reference_local':'Warm stone city on clear ground, ochre branching tracks, sparse rock exposures, clustered conifers, gold crops. This pass is ground-only.',
      'actual_baseline':'A uniform grainy olive lawn with faint relief and a pale Y-like route. Relief exists in geometry (height range -5.91 to8.15m), but its distinction is weak. Preserve the city pad and all earlier mesh edits.',
      'failed_strategy':'Three previous 8B responses copied old tiny sculpt choices and returned invalid units/ranges. No edit was executed. Do not repeat old numerical plans. Original raw responses remain in ground-finish-39126030933.',
      'new_strategy':'Short source-specific context and installed 14B director. Original asymmetric broad landforms plus restrained surface response. No historic parameter examples, no automatic parameter rewriting.'}
    workshop.write(out/'independent-observation-before.json',observations)
    brief='''You are the terrain artist for an existing Eldoria 4X sector, not a consultant. Author the actual edit recipe. This TEXT request contains independently inspected image descriptions, not direct image input. Return JSON only. Add 2-3 broad ASYMMETRIC hills/basins to the existing mesh, enough to read at strategic camera distance. Prefer one clearly legible landform with height magnitude above 5m plus lower secondary form; leave open level space. Each ellipse uses WORLD X/Y as horizontal, Z vertical, METRES. The 240m patch is a local proof sector, NOT the entire world; no coast or central Breach added in this pass. Protected city pad and border are automatically excluded, not relocated. Existing routes remain. Pick your original positions/proportions, no code. The existing executor directly applies your exact values.
REQUIRED palette keys: meadow, meadow_light, dry, soil, rock, path. Each is [R,G,B] sRGB INTEGERS 0..255, NOT linear colours. Meadow and meadow_light EACH: R80..160 G90..170 B25..100; G>=R; G>B+30. Each corresponding meadow_light channel may differ from meadow by AT MOST35. Restrained olive-gold, not saturated green. Dry: R150..220 G130..200 B65..140. Warm soil and light ochre path; warm grey rock, avoid very dark paths. Broad low-contrast surface transitions, not camouflage.
Each landform EXACT keys x,y,rx,ry,height,angle. x/y -92..92; rx12..48; ry12..58; height -3.5..8.5; angle degrees -180..180. At least one |height|>=3. Other fields: macro_scale24..60 METRES, dry_amount0.12..0.30 FRACTION, micro_strength0.025..0.09 FRACTION, slope_rock_start11..22 DEGREES, slope_rock_end25..40 DEGREES. Slope values like0.2 or0.8 are WRONG units. Explain visible diagnosis and intended composition in two short fields diagnosis/design_intent. No ART PASS. Check ALL limits before returning.'''
    content=brief+'\nACTUAL SOURCE '+json.dumps(context)+'\nVISUAL OBSERVATIONS '+json.dumps(observations)
    errors=[]
    for attempt in range(1,4):
        payload={'model':'qwen3:14b','stream':False,'think':False,'format':workshop.schema(),'messages':[{'role':'system','content':'You are the Eldoria source-editing terrain director. Choose original spatial/material values, comply with the exact schema and stated units. No markdown or code.'},{'role':'user','content':content+('\nCorrect this exact rejection with a NEW full valid original plan: '+errors[-1] if errors else '')}],'options':{'temperature':.3,'num_ctx':8192,'num_predict':2300},'keep_alive':'2m'}
        workshop.write(out/f'director-request-{attempt}.json',payload)
        start=time.time();raw=workshop.request('/api/chat',payload,timeout=330);workshop.write(out/f'qwen-director-raw-{attempt}.json',raw)
        try:
            plan=json.loads(raw['message']['content']);workshop.validate(plan)
            workshop.write(out/'artist-plan.json',plan)
            workshop.write(out/'genuine-author.json',{'model':'qwen3:14b','new_inference':True,'image_input':False,'reference_observations_input':True,'original_values_rewritten':False,'seconds':time.time()-start,'plan_sha256':workshop.digest(out/'artist-plan.json'),'previous_rejected_run':39126030933,'art_approved':False})
            return plan
        except (ValueError,KeyError,TypeError) as e:
            errors.append(str(e)+'; actual proposed values: '+raw['message']['content'])
            workshop.write(out/f'plan-rejection-{attempt}.json',{'error':str(e),'edited':False})
    raise RuntimeError('Focused director plans rejected; no source modification: '+errors[-1])

def main():
    p=argparse.ArgumentParser();p.add_argument('--out',required=True,type=pathlib.Path);args=p.parse_args(sys.argv[sys.argv.index('--')+1:]);out=args.out.resolve();out.mkdir(parents=True,exist_ok=False)
    assert workshop.digest(workshop.SOURCE)==workshop.EXPECTED
    assert workshop.digest(BASE/'before.blend')==BASE_HASH
    bpy.ops.wm.open_mainfile(filepath=str(BASE/'before.blend'))
    terrain=bpy.data.objects.get(workshop.TARGET)
    if terrain is None:raise RuntimeError('Saved source target absent')
    context=json.loads((BASE/'source-context.json').read_text(encoding='utf-8'))
    workshop.write(out/'source-context.json',context)
    before=[]
    for label,focus,scale,w,h in workshop.VIEWS:
        source=BASE/('before-'+label+'-blender.png');assert workshop.digest(source)==IMAGE_HASHES[label]
        dest=out/source.name;shutil.copyfile(source,dest)
        scene=bpy.context.scene;cam=scene.camera;target=Vector(focus)
        cam.location=target+Vector((0,-210,300));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=scale
        bpy.context.view_layer.update()
        before.append({'view':label,'width':w,'height':h,'ortho_scale':scale,'matrix':[list(r) for r in cam.matrix_world],'sha256':workshop.digest(dest),'filename':dest.name,'reused_exact_verified_png':True})
    workshop.write(out/'baseline-provenance.json',{'blend_sha256':BASE_HASH,'source_run':39126030933,'same_source_camera_light':True,'captures':before})
    plan=make_plan(out,context)
    proof=workshop.apply_terrain(terrain,plan);workshop.write(out/'geometry-proof.json',proof)
    # Save the genuine sculpt immediately, even if the later material stage fails.
    bpy.ops.wm.save_as_mainfile(filepath=str(out/'sculpted.blend'))
    material=workshop.paint(terrain,plan,out);workshop.write(out/'material-proof.json',material)
    bpy.ops.wm.save_as_mainfile(filepath=str(out/'after.blend'))
    bpy.ops.object.select_all(action='DESELECT');terrain.select_set(True);bpy.context.view_layer.objects.active=terrain
    bpy.ops.export_scene.gltf(filepath=str(out/'ZoneOneGroundFinish.glb'),export_format='GLB',use_selection=True,export_apply=True)
    after=workshop.render(out,'after')
    # Blender's transform matrix is updated by the actual render; compare within float tolerance.
    delta=max(abs(a-b) for ba,aa in zip(before,after) for br,ar in zip(ba['matrix'],aa['matrix']) for a,b in zip(br,ar))
    if delta>1e-5:raise RuntimeError('Paired camera changed: '+str(delta))
    workshop.write(out/'result.json',{'state':'SOURCE_CANDIDATE_REQUIRES_INDEPENDENT_VISUAL_REVIEW','source_sha256':workshop.EXPECTED,'candidate_sha256':workshop.digest(out/'ZoneOneGroundFinish.glb'),'plan_sha256':workshop.digest(out/'artist-plan.json'),'geometry':proof,'material':material,'before':before,'after':after,'camera_max_delta':delta,'blender_version':bpy.app.version_string,'unity_executed':False,'art_approved':False,'paid_credits':0})
    assert workshop.digest(workshop.SOURCE)==workshop.EXPECTED
if __name__=='__main__':main()
