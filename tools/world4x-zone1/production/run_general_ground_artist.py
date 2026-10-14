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
extrude params={faces:[verified_face_indices],offset:[dx,dy,dz]}; NOT direction, amount or use_smooth_shade. inset params={faces:[verified_face_indices],thickness:t}; NOT distance. Faces were NOT selected or supplied in this brief; do not invent them or extrude the whole terrain.
subdivide params={faces:[verified_face_indices],cuts:n}; bevel params={edges:[verified_edge_indices],width:w,segments:n}; topology changes require independent continuity verification. modifier params={kind:BEVEL|SUBSURF|SOLIDIFY|DECIMATE|REMESH|MIRROR|ARRAY|WEIGHTED_NORMAL|TRIANGULATE|SIMPLE_DEFORM|DISPLACE,options:{supported_values}}; an unmasked displacement of the whole plane would destroy protected city/borders and be rejected.
Preserve the existing texture and UVs; no material operation in this geography-first cycle. Sculpt brush centers must stay in northern working region x=-70..70,y=35..75,z=-4..7; radii 25..40m remain within ground and outside protected city. delta x=y=0 keeps map continuity. You choose all centers, positive hill or negative valley heights, radii and their asymmetric composition, not a preset hill template. No coincident identical brushes, repeating row, or tiny isolated lump. Use several complementary large landform edits visible from the real source camera. Ground remains one mesh and open meadow dominates.
Prior general-workshop run 38571917109 had three genuinely non-executable proposals and executed no geometry. This attempt must return your own corrected valid operations. Do not copy those rejected pseudo-parameters. Geometry before texturing; no self-approval.
'''
def brief(max_actions,source_exists=False):return original_brief(max_actions,source_exists)+MANUAL
def schema(max_actions=6,source_exists=False):
 result=original_schema(max_actions,source_exists)
 if source_exists:
  action=result['properties']['actions']['items']
  vec={'type':'array','minItems':3,'maxItems':3,'items':{'type':'number'}}
  variants=[]
  for op,fields in [('sculpt',{'center':vec,'delta':vec,'radius':{'type':'number','minimum':25,'maximum':40}}),
                    ('extrude',{'faces':{'type':'array','items':{'type':'integer'}},'offset':vec}),
                    ('inset',{'faces':{'type':'array','items':{'type':'integer'}},'thickness':{'type':'number'}})]:
   variant=copy.deepcopy(action);variant['properties']['op']={'type':'string','const':op}
   variant['properties']['params']={'type':'object','properties':fields,'required':list(fields),'additionalProperties':False}
   variants.append(variant)
  other=copy.deepcopy(action)
  other['properties']['op']['enum']=[op for op in other['properties']['op']['enum'] if op not in ('sculpt','extrude','inset')]
  variants.append(other)
  result['properties']['actions']['items']={'oneOf':variants}
 return result
def ask_plan(*args,**kwargs):
 doc=original_ask(*args,**kwargs)
 sculpts=[action for action in doc['actions'] if action['op']=='sculpt']
 if sculpts and not any(abs(action['params']['delta'][2])>=6 for action in sculpts):
  raise artist_loop.RejectedPlan('Actual source review rejected previous +2.5m/-1.2m edits as insufficient strategic relief. Choose YOUR OWN broad landforms with at least one 6..12m vertical brush; do not repeat a tiny valid-render correction. Do not alter existing rejected proposal parameters externally.')
 for action in sculpts:
  c=action['params']['center'];d=action['params']['delta']
  if not (-70<=c[0]<=70 and 35<=c[1]<=75 and -4<=c[2]<=7 and d[0]==0 and d[1]==0 and abs(d[2])<=12):
   raise artist_loop.RejectedPlan('Choose original vertical sculpt brushes within stated safe northern region and <=12m displacement; preserve XY, city and borders.')
 return doc
artist_loop.contract_brief=brief;artist_loop.schema_for_plan=schema;artist_loop.ask_plan=ask_plan
artist_loop.main()
