#!/usr/bin/env python3
"""Scene-specific camera-footprint contract: reject measured invisible tiny 3D edits."""
import unittest
from artist_plan_validation import camera_footprint_errors

TARGET='Valoria natural playable hilltop and rock scarps'
def design(kind,pars):
 return {'planned_actions':[{'id':'qwen','type':kind,'target':TARGET,'parameters':pars}]}

class CameraFootprintContractTests(unittest.TestCase):
 def test_original_real_qwen_path_rejected_before_blender(self):
  original=design('sculpt_path',{'knots':[
    {'center':[-10.6827,-25.3393,-13.6378],'radius':2.5,'delta':[0.,.5,0.]},
    {'center':[10.6827,-25.3393,-13.0879],'radius':2.5,'delta':[0.,.5,0.]}]})
  self.assertIn('CAMERA_FOOTPRINT_TOO_SMALL',str(camera_footprint_errors(original,10,1)))
  self.assertEqual(camera_footprint_errors(original),[])
 def test_meaningful_model_authored_path_allowed(self):
  model=design('sculpt_path',{'knots':[
   {'center':[-20,-28,-15],'radius':14,'delta':[0,-1.7,.8]},
   {'center':[18,-28,-16],'radius':18,'delta':[0,-1.2,-.5]}]})
  self.assertEqual(camera_footprint_errors(model,10,1),[])
 def test_minima_must_be_on_same_knot(self):
  plan=design('sculpt_path',{'knots':[
   {'radius':18,'delta':[0,.2,0]},
   {'radius':2,'delta':[0,2,0]}]})
  self.assertTrue(camera_footprint_errors(plan,10,1))
 def test_crest_span_and_height_must_both_be_significant(self):
  narrow=design('reprofile_crest',{'control_points':[[-2,-12],[0,-10],[3,-11]]})
  broad=design('reprofile_crest',{'control_points':[[-24,-12],[0,-10],[24,-13]]})
  self.assertTrue(camera_footprint_errors(narrow,10,1))
  self.assertEqual(camera_footprint_errors(broad,10,1),[])
 def test_region_tiny_rejected(self):
  x=design('sculpt_region',{'center':[0,-30,-10],'radius':2.5,'delta':[0,.5,0]})
  self.assertTrue(camera_footprint_errors(x,10,1))
  x['planned_actions'][0]['parameters']['radius']=14
  x['planned_actions'][0]['parameters']['delta']=[0,-2,0]
  self.assertEqual(camera_footprint_errors(x,10,1),[])
 def test_irrelevant_decisions_deferred_to_global_validator(self):
  self.assertEqual(camera_footprint_errors({},10,1),[])
  self.assertEqual(camera_footprint_errors(design('assign_material',{}),10,1),[])

if __name__=='__main__':
 unittest.main()
