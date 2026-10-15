"""Resolve a chosen real source face, then reuse the verified timber workshop."""
import argparse,json,pathlib,sys
import bpy
HERE=pathlib.Path(__file__).resolve().parent
sys.path.insert(0,str(HERE));sys.path.insert(0,str(HERE.parent/'gpt6-tower-eval-20261015'))
from timber_frame_skill import compile_joinery
from joinery_blender import build,digest

def main():
    p=argparse.ArgumentParser()
    for n in ('plan','source','review','out'):p.add_argument('--'+n,type=pathlib.Path,required=True)
    a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
    s=json.loads(a.plan.read_text(encoding="utf8"))
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(a.source.resolve()))
    bpy.context.view_layer.update()
    o=bpy.data.objects.get(s['source_object'])
    if o is None or o.type!='MESH' or not 0<=s['anchor_face']<len(o.data.polygons):
        raise ValueError('EXACT_SOURCE_FACE_MISSING')
    face=o.data.polygons[s['anchor_face']]
    point=list(o.matrix_world@face.center)
    normal=list((o.matrix_world.to_3x3().inverted().transposed()@face.normal).normalized())
    compiled=compile_joinery(s,point,normal)
    build(compiled,a.source,json.loads(a.review.read_text(encoding="utf8")),a.out,digest(a.plan))
    (a.out/'authored-frame-plan.json').write_bytes(a.plan.read_bytes())
    (a.out/'joinery-plan.json').write_text(json.dumps(compiled,indent=2)+'\n')
    (a.out/'frame-compilation-proof.json').write_text(json.dumps({
      'source_sha256':digest(a.source),'authored_plan_sha256':digest(a.plan),
      'source_face':s['anchor_face'],'world_point_m':point,'world_normal':normal,
      'creative_parameters_rewritten':False,'fit_clearance_m':.006,
      'skill':'real face anchored frame with artist-authored post count and spacing',
      'art_approved':False},indent=2)+'\n')
if __name__=='__main__':main()
