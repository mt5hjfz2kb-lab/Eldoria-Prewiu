#!/usr/bin/env python3
"""Safety contracts for Qwen structured artist output; zero inference/cloud credits."""
import unittest
from author_brief import cpu_artist_schema, prioritize_foreground_anchors
from artist_plan_validation import inspect_projected_environment_scale

class ArtistSchemaTests(unittest.TestCase):
 def test_real_foreground_scarp_anchors_are_not_hidden_by_aabb_corners(self):
  anchors=[{'role':'deep_slope','index':1,'position':[-50,57,-21.6]},
           {'role':'highest_crest','index':2,'position':[0,0,3.4]},
           {'role':'negative_y_cliff_left','index':3,'position':[-36,-32,-7.0]},
           {'role':'negative_y_cliff_right','index':4,'position':[35,-30,-8.0]},
           {'role':'southern_join','index':5,'position':[0,-45,-10]}]
  result=prioritize_foreground_anchors(anchors,3)
  self.assertEqual([r['index'] for r in result],[3,4,5])
  self.assertNotIn('delta',str(result))
 def test_existing_mesh_can_design_deformations_but_cannot_fabricate_new_mesh(self):
  s=cpu_artist_schema('Continuous eroded cliff rock face','existing',2)
  p=s['properties']
  self.assertEqual(p['asset_decision']['enum'],['REUSE','MODIFY'])
  self.assertEqual(p['planned_actions']['maxItems'],2)
  self.assertEqual(p['planned_actions']['type'],'array')
  a=p['planned_actions']['items']['properties']
  self.assertEqual(a['target']['const'],'Continuous eroded cliff rock face')
  self.assertEqual(set(a['type']['enum']),{'sculpt_region','sculpt_path','reprofile_crest'})
  self.assertNotIn('construct_mesh',a['type']['enum'])
  self.assertIn('parameters',p['planned_actions']['items']['required'])
 def test_measured_repeated_method_is_removed_from_future_schema_not_all_art(self):
  s=cpu_artist_schema('authored existing cliff','existing',2,{'sculpt_region'})
  allowed=s['properties']['planned_actions']['items']['properties']['type']['enum']
  self.assertEqual(set(allowed),{'sculpt_path','reprofile_crest'})
  with self.assertRaises(ValueError):
   cpu_artist_schema('authored existing cliff','existing',2,{'sculpt_region','sculpt_path','reprofile_crest'})
 def test_new_asset_can_be_designed_from_scratch(self):
  s=cpu_artist_schema('new authored tower','new',2)
  self.assertIn('construct_mesh',s['properties']['planned_actions']['items']['properties']['type']['enum'])
  self.assertEqual(s['properties']['asset_decision']['enum'],['NEW'])
 def test_full_existing_artist_retains_modular_authoring(self):
  s=cpu_artist_schema('real source','existing',8)
  a=s['properties']['planned_actions']['items']['properties']['type']['enum']
  self.assertIn('bridge_boundary_loops',a)
  self.assertIn('place_existing_asset',a)
  self.assertNotIn('construct_mesh',a)
 def test_explicit_identity_and_rationale_required(self):
  s=cpu_artist_schema('existing object','existing',2)
  self.assertFalse(s['additionalProperties'])
  actions=s['properties']['planned_actions']['items']
  self.assertEqual(set(actions['required']),{'id','type','target','rationale','parameters'})
  self.assertGreaterEqual(actions['properties']['rationale']['minLength'],15)

 def test_distant_camera_structural_minimum_schema_preserves_artist_coordinates(self):
  s=cpu_artist_schema('Valoria original terrain','existing',2,{'sculpt_region'},large_cliff_radius=12)
  actions=s['properties']['planned_actions']['items']['properties']
  self.assertNotIn('sculpt_region',actions['type']['enum'])
  self.assertEqual(actions['parameters']['properties']['knots']['items']['properties']['radius']['minimum'],12)
  self.assertEqual(actions['parameters']['properties']['knots']['minItems'],2)
  self.assertEqual(actions['parameters']['minProperties'],1)
  self.assertEqual(cpu_artist_schema('Valoria original terrain','existing',2)['properties']['planned_actions']['items']['properties']['parameters']['minProperties'],2)
 def test_reject_micro_path_without_rewriting_art(self):
  tiny={'planned_actions':[{'type':'sculpt_path','parameters':{'knots':[
    {'center':[-11,-25,-13],'radius':2.5,'delta':[0,0.5,0]},
    {'center':[11,-25,-13],'radius':2.5,'delta':[0,0.5,0]}]}}]}
  original=str(tiny)
  errs=inspect_projected_environment_scale(tiny,12,1)
  self.assertEqual(len(errs),2)
  self.assertEqual(str(tiny),original)
  good={'planned_actions':[{'type':'sculpt_path','parameters':{'knots':[
    {'center':[-18,-27,-11],'radius':15,'delta':[0,2,0]},
    {'center':[18,-27,-11],'radius':16,'delta':[0,2.5,0]}]}}]}
  self.assertEqual(inspect_projected_environment_scale(good,12,1),[])
  self.assertEqual(inspect_projected_environment_scale(tiny,0,0),[])
 def test_reject_crest_too_flat_or_too_narrow_for_current_camera(self):
  narrow={'planned_actions':[{'type':'reprofile_crest','parameters':{
   'control_points':[[0,-13],[3,-12.8],[6,-13.1]]}}]}
  self.assertEqual(len(inspect_projected_environment_scale(narrow,12,1)),2)
  wide={'planned_actions':[{'type':'reprofile_crest','parameters':{
   'control_points':[[-20,-13],[0,-9],[20,-14]]}}]}
  self.assertEqual(inspect_projected_environment_scale(wide,12,1),[])

if __name__=='__main__': unittest.main()
