#!/usr/bin/env python3
"""Independent typed spatial preflight for actual Qwen-designed Blender actions.

Do not silently fix failed model geometry: return precise errors to Qwen for
a second inference or reject. The executor and frozen source remain sovereign.
"""
import math

STRUCTURAL={'sculpt_region','sculpt_path','reprofile_crest','bridge_boundary_loops','construct_mesh'}
ALLOWED=STRUCTURAL|{'assign_material','place_existing_asset'}
def vec(v,n=3):
 return isinstance(v,(list,tuple)) and len(v)==n and all(
  isinstance(x,(int,float)) and not isinstance(x,bool) and math.isfinite(x) for x in v)
def length(v):return math.sqrt(sum(x*x for x in v))
def inspect_artist_plan(design,target,mode,max_actions=2,topology=None,require_geometry=True,require_camera_effect=False):
 errors=[]
 if not isinstance(design,dict):return ['Artist response must be a JSON object']
 if len(str(design.get('observed_defect') or ''))<35:errors.append('observed_defect must diagnose REAL visual defect in at least 35 chars')
 if len(str(design.get('design_intent') or ''))<35:errors.append('design_intent must describe camera-visible artistic transformation in at least 35 chars')
 decision=design.get('asset_decision')
 if decision not in ('REUSE','MODIFY','NEW'):errors.append('asset_decision must be REUSE, MODIFY or NEW')
 if mode=='existing' and decision=='NEW':errors.append('Existing target is FROZEN: choose MODIFY or REUSE; NEW/construct_mesh is not permitted')
 if mode=='new' and decision!='NEW':errors.append('A genuinely new target must have asset_decision NEW')
 actions=design.get('planned_actions')
 if not isinstance(actions,list) or not 1<=len(actions)<=max_actions:
  return errors+[f'planned_actions must be a JSON ARRAY with 1..{max_actions} actions, never a numbered object']
 aabb=(topology or {}).get('local_aabb') if isinstance(topology,dict) else None
 def inside(v):
  if not isinstance(aabb,dict) or not vec(v):return True
  lo,hi=aabb.get('min'),aabb.get('max')
  return not(vec(lo) and vec(hi)) or all(lo[i]-.5<=v[i]<=hi[i]+.5 for i in range(3))
 approved_loops=[x.get('indices') for x in (topology or {}).get('boundary_loops',[]) if isinstance(x,dict)]
 ids=set()
 for i,op in enumerate(actions):
  if not isinstance(op,dict):errors.append(f'Action {i}: not a JSON object');continue
  typ,pr=op.get('type'),op.get('parameters')
  if not isinstance(pr,dict):errors.append(f'Action {i}: parameters MUST be a JSON object');continue
  if typ not in ALLOWED:errors.append(f'Action {i}: unsupported Blender action {typ}')
  if mode=='existing' and typ=='construct_mesh':errors.append('Cannot construct NEW mesh inside existing source')
  if mode=='new' and i==0 and typ!='construct_mesh':errors.append('NEW mode must begin with construct_mesh')
  if op.get('target')!=target:errors.append(f'Action {i}: target MUST be exactly {target!r}')
  if not isinstance(op.get('id'),str) or not op['id'].strip() or op['id'] in ids:errors.append(f'Action {i}: blank/duplicate id')
  else:ids.add(op['id'])
  if len(str(op.get('rationale') or ''))<15:errors.append(f'Action {i}: artistic rationale too short')
  if typ=='sculpt_region':
   if not vec(pr.get('center')) or not inside(pr['center']):errors.append('sculpt_region center MUST be within real LOCAL mesh AABB')
   if not vec(pr.get('delta')) or not .0051<=length(pr['delta'])<=20:errors.append('sculpt_region delta MUST BE NONZERO, executable 3D displacement (>0.005, <=20)')
   if not isinstance(pr.get('radius'),(int,float)) or not .05<=pr['radius']<=60:errors.append('sculpt_region radius must be 0.05..60')
   if require_camera_effect:
    # Non-approval screen-space preflight: 5mm movements on a 12m cliff are
    # quantization noise; Blender A/B still independently measures visibility.
    if not vec(pr.get('delta')) or length(pr['delta'])<.2:
     errors.append('SCULPT_CAMERA_IMPACT_FAIL: Qwen must design >=0.2m nontrivial displacement for this wide strategy-camera rock face, not millimeter jitter')
    if not isinstance(pr.get('radius'),(int,float)) or pr['radius']<1.0:
     errors.append('SCULPT_CAMERA_IMPACT_FAIL: Qwen must affect >=1.0m radius of this rock face; tiny invisible patches cannot qualify as artistic improvement')
  if typ=='sculpt_path':
   knots=pr.get('knots')
   if not isinstance(knots,list) or not 2<=len(knots)<=24:errors.append('sculpt_path must contain TWO OR MORE authored 3D knots')
   else:
    seen=[]
    if require_camera_effect and (not any(vec(k.get('delta')) and length(k['delta'])>=.2 for k in knots if isinstance(k,dict)) or
                                  not any(isinstance(k,dict) and isinstance(k.get('radius'),(int,float)) and k['radius']>=1.0 for k in knots)):
     errors.append('SCULPT_CAMERA_IMPACT_FAIL: 3D path must include substantial nontrivial displacement and spatial radius')
    for j,k in enumerate(knots):
     if not isinstance(k,dict):errors.append(f'Knot {j}: invalid object');continue
     if not vec(k.get('center')) or not inside(k['center']):errors.append(f'Knot {j}: center outside source local mesh bounds')
     else:seen.append(tuple(k['center']))
     if not vec(k.get('delta')) or not .005<=length(k['delta'])<=20:errors.append(f'Knot {j}: delta MUST BE NONZERO')
     if not isinstance(k.get('radius'),(int,float)) or not .05<=k['radius']<=60:errors.append(f'Knot {j}: radius must be 0.05..60')
    if len(set(seen))<2:errors.append('sculpt_path requires at least TWO DISTINCT 3D knot positions')
  if typ=='bridge_boundary_loops':
   x,y=pr.get('loop_a'),pr.get('loop_b')
   if not isinstance(x,list) or not isinstance(y,list) or x not in approved_loops or y not in approved_loops or x==y:
    errors.append('bridge_boundary_loops must select TWO DISTINCT certified original Blender loops; never invent vertex indices')
  if typ=='reprofile_crest':
   axis,height=pr.get('axis'),pr.get('height_axis')
   cp=pr.get('control_points')
   if axis not in ('x','y') or height not in ('y','z') or axis==height:errors.append('reprofile_crest axis and height_axis must be distinct valid axes')
   if not isinstance(cp,list) or not 3<=len(cp)<=24 or not all(vec(v,2) for v in cp):
    errors.append('reprofile_crest needs THREE OR MORE valid [position,height] control points')
   elif len({tuple(v) for v in cp})<3:errors.append('reprofile_crest needs three DISTINCT control points')
  if typ=='construct_mesh' and mode=='new':
   verts,faces=pr.get('vertices'),pr.get('faces')
   if not isinstance(verts,list) or not 4<=len(verts)<=256 or not all(vec(v) for v in verts):errors.append('construct_mesh must author 4..256 real vertices')
   if not isinstance(faces,list) or not 3<=len(faces)<=512 or not all(isinstance(face,list) and 3<=len(face)<=12 and all(isinstance(v,int) and v>=0 and v<len(verts or []) for v in face) for face in faces):
    errors.append('construct_mesh must author valid polygon faces from those vertices')
 if require_geometry and not any(isinstance(op,dict) and op.get('type') in STRUCTURAL for op in actions):errors.append('At least ONE Qwen-designed STRUCTURAL geometry operation required for this cliff')
 return errors

def bounded_repair_feedback(errors,max_items=8):
 return ('Your previous artist plan is REJECTED; no geometry was executed. '
         'Correct the following errors through your OWN artistic 3D decision, not scripted defaults: '
         +' | '.join(errors[:max_items])+
         '. Keep the original defect/destination, choose real local-space cliff crest/slope anchors '
         'and output one safe, NONZERO structural Blender action with exact coordinates. '
         'Return COMPLETE JSON (including planned_actions ARRAY), never new face lists.')


def inspect_projected_environment_scale(design,minimum_radius,minimum_displacement):
 """Scene-specific LOWER-BOUND preflight, never a substitute for matched-camera A/B.

 The caller must derive these limits from its own target scale/camera framing;
 this function only REJECTS insufficient Qwen-authored spatial proposals.
 It does not enlarge, move, fabricate or rewrite artist geometry.
 """
 if not 0 <= minimum_radius <= 50 or not 0 <= minimum_displacement <= 10:
  raise ValueError('Unreasonable per-scene artistic screen-scale limits')
 if not minimum_radius and not minimum_displacement:return []
 actions=design.get('planned_actions',[]) if isinstance(design,dict) else []
 if not isinstance(actions,list):return []
 errors=[]
 for i,op in enumerate(actions):
  if not isinstance(op,dict):continue
  kind,pr=op.get('type'),op.get('parameters')
  if not isinstance(pr,dict):continue
  if kind=='sculpt_region':
   radius=pr.get('radius')
   delta=pr.get('delta')
   if minimum_radius and (not isinstance(radius,(float,int)) or radius<minimum_radius):
    errors.append(f'CAMERA_SCALE_REJECT action {i}: sculpt radius must be at least {minimum_radius:g}m for THIS distant camera (model must author revised radius)')
   if minimum_displacement and (not vec(delta) or length(delta)<minimum_displacement):
    errors.append(f'CAMERA_SCALE_REJECT action {i}: model-authored displacement must be at least {minimum_displacement:g}m for THIS camera')
  elif kind=='sculpt_path':
   knots=pr.get('knots')
   if not isinstance(knots,list) or len(knots)<2:continue
   if minimum_radius and any(not isinstance(k,dict) or not isinstance(k.get('radius'),(int,float)) or k['radius']<minimum_radius for k in knots):
    errors.append(f'CAMERA_SCALE_REJECT action {i}: EVERY broad scarp knot must span >= {minimum_radius:g}m radius')
   if minimum_displacement and not any(isinstance(k,dict) and vec(k.get('delta')) and length(k['delta'])>=minimum_displacement for k in knots):
    errors.append(f'CAMERA_SCALE_REJECT action {i}: at least one genuine knot displacement must be >= {minimum_displacement:g}m')
  elif kind=='reprofile_crest':
   points=pr.get('control_points')
   if not isinstance(points,list) or len(points)<3 or not all(vec(q,2) for q in points):continue
   if minimum_radius and max(q[0] for q in points)-min(q[0] for q in points)<minimum_radius*2:
    errors.append(f'CAMERA_SCALE_REJECT action {i}: crest span needs >= {2*minimum_radius:g}m across player-visible scarp')
   if minimum_displacement and max(q[1] for q in points)-min(q[1] for q in points)<minimum_displacement:
    errors.append(f'CAMERA_SCALE_REJECT action {i}: crest needs >= {minimum_displacement:g}m height variation')
 return errors
