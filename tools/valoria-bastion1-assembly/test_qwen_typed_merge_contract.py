#!/usr/bin/env python3
"""Pure-stdlib contract tests for the EXACT Qwen plan-validation block in Blender merge."""
import json
import math
import pathlib
import tempfile
import types
import unittest

ROOT=pathlib.Path(__file__).resolve().parent
SOURCE=(ROOT/'apply_qwen_artist_mesh.py').read_text('utf8')
BEGIN='plan=json.loads(a.plan.read_text())'
END="if a.out.resolve() in (a.source.parent.resolve(),a.artist.parent.resolve()):"
assert SOURCE.count(BEGIN)==1 and SOURCE.count(END)==1
SCHEMA_CODE=SOURCE[SOURCE.index(BEGIN):SOURCE.index(END)]
compile(SCHEMA_CODE,str(ROOT/'apply_qwen_artist_mesh.py'),'exec')
TARGET='Valoria natural playable hilltop and rock scarps'

def run_plan(operations):
 with tempfile.TemporaryDirectory() as tmp:
  path=pathlib.Path(tmp)/'plan.json'
  path.write_text(json.dumps(operations))
  scope={'a':types.SimpleNamespace(plan=path,target=TARGET),'json':json,'math':math}
  exec(SCHEMA_CODE,scope,scope)
  return scope['plan']

def op(kind,p):
 return {'id':'genuine-artist-change','type':kind,'target':TARGET,'parameters':p}

def region():
 return op('sculpt_region',{'center':[-19,-28,-15],'radius':19,'delta':[0,-2.2,.5]})

def path():
 return op('sculpt_path',{'knots':[
  {'center':[-22,-29,-16],'radius':17,'delta':[-1.0,-2.0,.1]},
  {'center':[20,-29,-15],'radius':19,'delta':[1.2,-1.6,.8]}]})

def crest():
 return op('reprofile_crest',{'axis':'x','height_axis':'z',
  'control_points':[[-25,-13],[0,-12],[25,-16]],'band_min':-50,'band_max':-5})

class TypedMergeContractTests(unittest.TestCase):
 def test_all_three_original_qwen_structural_methods(self):
  for plan in ([region()],[path()],[crest()],[path(),crest()]):
   self.assertEqual(run_plan(plan),plan)
 def test_no_unrelated_model_actions_or_forged_target(self):
  for plan in ([op('construct_mesh',{'vertices':[]})],
      [dict(path(),target='other-terrain')],[path(),path(),path()],[]):
   with self.assertRaises(ValueError):run_plan(plan)
 def test_reject_invalid_path_geometry(self):
  for p in ({'knots':[path()['parameters']['knots'][0]]},
            {'knots':[path()['parameters']['knots'][0]]*2},
            {'knots':[dict(path()['parameters']['knots'][0],radius=0),
                       path()['parameters']['knots'][1]]}):
   with self.assertRaises(ValueError):run_plan([op('sculpt_path',p)])
 def test_reject_invalid_crest_control_profile(self):
  p=dict(crest()['parameters'])
  p['control_points']=[[-25,-13],[-25,-12],[25,-16]]
  with self.assertRaises(ValueError):run_plan([op('reprofile_crest',p)])
 def test_cannot_forge_nil_or_nonfinite_geometry(self):
  p=dict(region()['parameters']);p['delta']=[float('nan'),1,0]
  with self.assertRaises(ValueError):run_plan([op('sculpt_region',p)])
 def test_preserve_original_integration_safety_gates(self):
  for invariant in (
   'protected_cell_changes', 'Artist edit too small or too strong',
   'Sculpt unexpectedly changed mesh faces','Terrain geometry cardinality unexpectedly changed',
   'saved_material_indices', 'poly.material_index=slot','Source SHA mismatch'):
   self.assertIn(invariant,SOURCE,invariant)

if __name__=='__main__':
 unittest.main()
