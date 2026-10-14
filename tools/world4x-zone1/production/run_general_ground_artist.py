"""Scoped API documentation/schema adapter; pinned general workshop remains read-only."""
import copy, pathlib, sys
studio=pathlib.Path(sys.argv[1]);sys.argv.pop(1);sys.path.insert(0,str(studio))
import artist_loop
original_brief=artist_loop.contract_brief
original_schema=artist_loop.schema_for_plan
original_ask=artist_loop.ask_plan
MANUAL='''
IMPORTANT CURRENT GEOGRAPHY TOOL SIGNATURES (strict, not Blender UI labels):
sculpt params={center:[x,y,z],delta:[dx,dy,dz],radius:r}; the brush edits existing vertices with smooth radial falloff. This is appropriate for continuous large prairie hills, not creating disconnected objects. No face selection is required. Terrain is 240 METRES wide, current z range -4.42..6.31m. Tiny offsets 0.1/0.2m cannot solve the visible landform failure. You choose meaningful broad original hills and saddles at real world scale.
TOPOLOGY LOCK FOR CONTINUOUS GROUND: the independent gate identifies the original 14,641 terrain vertices. Only sculpt operations are allowed this cycle: no subdivide, bevel, extrude, inset, remesh or topology-changing modifiers. Do not move any source vertex in XY or change protected city pad / border heights. Preserve original UVs and materials. Avoid an isolated circular pimple: arrange multiple original complementary elevations and saddles using brushes at staggered positions. Broad overlapping forms matter from the strategic camera, not only a maximum Z number.\nPreserve the existing texture and UVs; no material operation in this geography-first cycle. Sculpt brush centers must stay in northern working region x=-70..70,y=35..75,z=-4..7; radii 25..40m remain within ground and outside protected city. delta x=y=0 keeps map continuity. You choose all centers, positive hill or negative valley heights, radii and their asymmetric composition, not a preset hill template. No coincident identical brushes, repeating row, or tiny isolated lump. Use several complementary large landform edits visible from the real source camera. Ground remains one mesh and open meadow dominates.
Prior general-workshop run 38571917109 had three genuinely non-executable proposals and executed no geometry. This attempt must return your own corrected valid operations. Do not copy those rejected pseudo-parameters. Geometry before texturing; no self-approval.
'''
def brief(max_actions,source_exists=False):return original_brief(max_actions,source_exists)+MANUAL
def schema(max_actions=6,source_exists=False):
 result=original_schema(max_actions,source_exists)
 if source_exists:
  # The existing mesh/borders are already triangulated and indexed; topology
  # changing tool operations break the independent identity-based safety gate.
  # Constrain the model's tools, not its creative placement/shape decisions.
  action=copy.deepcopy(result['properties']['actions']['items'])
  vec={'type':'array','minItems':3,'maxItems':3,'items':{'type':'number'}}
  action['properties']['op']={'type':'string','const':'sculpt'}
  action['properties']['params']={'type':'object',
    'properties':{'center':vec,'delta':vec,'radius':{'type':'number','minimum':25,'maximum':40}},
    'required':['center','delta','radius'],'additionalProperties':False}
  result['properties']['actions']['items']=action
 return result
def ask_plan(*args,**kwargs):
 doc=original_ask(*args,**kwargs)
 sculpts=[action for action in doc['actions'] if action['op']=='sculpt']
 if len(sculpts)<2 or len(sculpts)!=len(doc['actions']):
  raise artist_loop.RejectedPlan('Topology-stable terrain cycle requires at least two genuine sculpt landform actions, no subdivide/bevel/remesh or invented face indices.')
 if not any(a['params']['delta'][2]>=6 for a in sculpts) or not any(a['params']['delta'][2]<=-2.5 for a in sculpts):
  raise artist_loop.RejectedPlan('Visual relief needs an authored broad hill >= +6m and a complementary valley <= -2.5m; keep original coordinates and choose original compositions.')
 if sculpts and not any(abs(action['params']['delta'][2])>=6 for action in sculpts):
  raise artist_loop.RejectedPlan('Actual source review rejected previous +2.5m/-1.2m edits as insufficient strategic relief. Choose YOUR OWN broad landforms with at least one 6..12m vertical brush; do not repeat a tiny valid-render correction. Do not alter existing rejected proposal parameters externally.')
 for action in sculpts:
  c=action['params']['center'];d=action['params']['delta']
  if not (-70<=c[0]<=70 and 35<=c[1]<=75 and -4<=c[2]<=7 and d[0]==0 and d[1]==0 and abs(d[2])<=12):
   raise artist_loop.RejectedPlan('Choose original vertical sculpt brushes within stated safe northern region and <=12m displacement; preserve XY, city and borders.')
 return doc
artist_loop.contract_brief=brief;artist_loop.schema_for_plan=schema;artist_loop.ask_plan=ask_plan
artist_loop.main()
