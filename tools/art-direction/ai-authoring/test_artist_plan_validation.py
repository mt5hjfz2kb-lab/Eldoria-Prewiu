#!/usr/bin/env python3
"""No Qwen inference: regression against genuine failed V5 cliff plans."""
import unittest
from artist_plan_validation import inspect_artist_plan,bounded_repair_feedback
REAL_TARGET='Continuous eroded cliff rock face'
REAL_TOPOLOGY={'local_aabb':{'min':[-10,-9,-5],'max':[10,9,5]},'boundary_loops':[]}
def draft(action):
 return {'observed_defect':'The existing blocky cliff has a visible straight seam that breaks the terrain silhouette.',
         'design_intent':'Sculpt an irregular but connected cliff transition, retaining the original terrain and playable area.',
         'asset_decision':'REUSE','reason_for_new_geometry':'Existing source supports topology-preserving deformation.',
         'planned_actions':[action]}
def region(delta,center=[1.,2.,0.]):
 return {'id':'rock-correction','type':'sculpt_region','target':REAL_TARGET,
   'rationale':'Sculpt natural stone silhouette near upper crest',
   'parameters':{'center':center,'radius':2.0,'delta':delta}}
class PlanContract(unittest.TestCase):
 def check(self,d,mode='existing'):
  return inspect_artist_plan(d,REAL_TARGET,mode,1,REAL_TOPOLOGY)
 def test_real_96_second_qwen_zero_displacement_rejected(self):
  err=self.check(draft(region([0,0,0])))
  self.assertIn('delta MUST BE NONZERO',' | '.join(err))
  self.assertIn('NONZERO',bounded_repair_feedback(err))
 def test_genuine_spatial_edit_not_preselected(self):
  for delta in ([0,.85,.3],[.4,0,0],[0,0,-1.2]):
   self.assertEqual(self.check(draft(region(delta))),[])
 def test_reject_new_on_frozen_source(self):
  d=draft(region([0,.5,0]));d['asset_decision']='NEW'
  self.assertIn('MODIFY or REUSE',' | '.join(self.check(d)))
 def test_reject_nonconvergent_single_knot_path(self):
  d=draft({'id':'rock-correction','type':'sculpt_path','target':REAL_TARGET,
           'rationale':'Shape descending rocky natural silhouette',
           'parameters':{'knots':[{'center':[0,0,0],'radius':2,'delta':[0,0,0]}]}})
  self.assertIn('TWO OR MORE',' | '.join(self.check(d)))
 def test_reject_copied_same_knot_and_zero_delta(self):
  d=draft({'id':'rock-correction','type':'sculpt_path','target':REAL_TARGET,
          'rationale':'Shape descending rocky natural silhouette',
          'parameters':{'knots':[{'center':[0,0,0],'radius':2,'delta':[0,0,0]},
                                 {'center':[0,0,0],'radius':2,'delta':[0,.2,0]}]}})
  self.assertIn('DISTINCT',' | '.join(self.check(d)))
 def test_reject_far_corner_and_fabricated_loops(self):
  self.assertIn('mesh AABB',' | '.join(self.check(draft(region([0,1,0],[200,200,-2])))))
  d=draft({'id':'rock-correction','type':'bridge_boundary_loops','target':REAL_TARGET,
     'rationale':'Bridge gap naturally using original mesh loops',
     'parameters':{'loop_a':[1,2,3],'loop_b':[4,5,6]}})
  self.assertIn('certified',' | '.join(self.check(d)))
 def test_reject_no_structural_change(self):
  d=draft({'id':'mat','type':'assign_material','target':REAL_TARGET,
           'rationale':'Apply realistic rough stone PBR tint',
           'parameters':{'rgb':[.5,.5,.5],'roughness':.9}})
  self.assertIn('STRUCTURAL',' | '.join(self.check(d)))
 def test_no_auto_repair(self):
  error=self.check(draft(region([0,0,0])))
  feedback=bounded_repair_feedback(error)
  self.assertNotIn('delta:[0,1,0]',feedback)
  self.assertNotIn('radius:1',feedback)
if __name__=='__main__':unittest.main()
