"""Resolve a chosen real source face, then reuse the verified timber workshop."""
import argparse,json,pathlib,sys,math
import bpy
from mathutils import Matrix,Vector
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
    zs=[float((o.matrix_world@v.co).z) for v in o.data.vertices]
    band=(point[2]-min(zs))/max(.001,max(zs)-min(zs))
    horizontal=math.hypot(normal[0],normal[1])
    print('ACTUAL_FRAME_ANCHOR='+json.dumps({'point':point,'normal':normal,'height_fraction':band}))
    if not .15<band<.6 or horizontal<.1:
        raise ValueError('FRAME_ANCHOR_NOT_IN_MEASURED_WALL_HEIGHT_BAND')
    # A faceted log surface is not an ideal vertical plane. Build the joint
    # system in its own horizontal wall frame, then place the whole assembly
    # by the TRUE source point and projected horizontal surface normal.
    # No artist widths, heights, offsets, or post spacings are changed.
    angle=math.atan2(normal[1],normal[0])+math.pi/2
    placement=Matrix.Translation(Vector((point[0],point[1],0))) @ Matrix.Rotation(angle,4,'Z')
    compiled=compile_joinery(s,[0,0,point[2]],[0,-1,0])
    build(compiled,a.source,json.loads(a.review.read_text(encoding="utf8")),a.out,digest(a.plan),placement=placement)
    (a.out/'authored-frame-plan.json').write_bytes(a.plan.read_bytes())
    (a.out/'joinery-plan.json').write_text(json.dumps(compiled,indent=2)+'\n')
    (a.out/'frame-compilation-proof.json').write_text(json.dumps({
      'source_sha256':digest(a.source),'authored_plan_sha256':digest(a.plan),
      'source_face':s['anchor_face'],'world_point_m':point,'world_normal':normal,
      'frame_rotation_z_rad':angle,'anchor_height_fraction':band,
      'placement_matrix':[[float(v) for v in row] for row in placement],
      'creative_parameters_rewritten':False,'fit_clearance_m':.006,
      'skill':'real face anchored frame with artist-authored post count and spacing',
      'art_approved':False},indent=2)+'\n')
if __name__=='__main__':main()
