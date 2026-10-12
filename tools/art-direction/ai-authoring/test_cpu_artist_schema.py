#!/usr/bin/env python3
"""Safety contracts for Qwen structured artist output; zero inference/cloud credits."""
import unittest
from author_brief import cpu_artist_schema

class ArtistSchemaTests(unittest.TestCase):
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
