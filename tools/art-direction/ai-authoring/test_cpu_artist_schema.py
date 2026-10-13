#!/usr/bin/env python3
"""Safety contracts for Qwen structured artist output; zero inference/cloud credits."""
import unittest
from author_brief import cpu_artist_schema, prioritize_foreground_anchors

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

if __name__=='__main__': unittest.main()
