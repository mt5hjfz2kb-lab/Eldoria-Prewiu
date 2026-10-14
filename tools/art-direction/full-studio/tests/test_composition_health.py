"""QA for wrong architectural silhouette rejected in actual Qwen authoring."""
import pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from composition_health import check_new_silhouette,CompositionRejected

def candidate(action_pairs):
 return {"asset_decision":"NEW","actions":[
  {"op":"primitive","target":name,"params":{
     "shape":shape,"location":location,"scale":scale,"rotation":[0,0,0]}}
  for name,shape,location,scale in action_pairs
 ]}

GOOD=[
 ("Stone shaft","cylinder",[0,0,2],[1.2,1.2,2]),
 ("Medieval roof","cone",[0,0,4.9],[1.7,1.7,1.1]),
 ("Oak door","cube",[0,-1.2,.95],[.45,.18,.9]),
]
REAL_FAILED=[
 ("tower-silhouette","cylinder",[-1.5,.3,.5],[2,.5,2]),
 ("tower-roof","cone",[.5,.3,.5],[2,.5,2]),
 ("tower-base","cube",[-1.5,.3,.5],[2,.5,2]),
 ("battlements","cube",[-1.5,.3,.5],[2,.5,2]),
]
class CompositionHealthTests(unittest.TestCase):
 def test_real_qwen_run_38568103614_rejected(self):
  with self.assertRaises(CompositionRejected) as error:
   check_new_silhouette(candidate(REAL_FAILED),"architecture")
  msg=str(error.exception)
  self.assertIn("buried",msg)
  self.assertIn("same height",msg)
  self.assertIn("roof",msg)
 def test_realistic_artist_chosen_modular_architecture_accepted(self):
  result=check_new_silhouette(candidate(GOOD),"architecture")
  self.assertEqual(result["new_object_count"],3)
  self.assertFalse(result["art_approved"])
  self.assertTrue(result["no_template_geometry_inserted"])
 def test_bad_roof_offset_rejected(self):
  wrong=[GOOD[0],("Medieval roof","cone",[8,8,4.9],[1.7,1.7,1.1])]
  with self.assertRaisesRegex(CompositionRejected,"spatially detached"):
   check_new_silhouette(candidate(wrong),"architecture")
 def test_tiny_rock_details_do_not_count_as_art_quality(self):
  p=candidate([("pebble","cube",[0,0,.1],[.12,.12,.1]),
               ("small stone","cylinder",[.01,0,.1],[.12,.12,.1])])
  with self.assertRaises(CompositionRejected):
   check_new_silhouette(p,"architecture")
 def test_terrain_can_include_below_grade_ground(self):
  p=candidate([("ground","cube",[0,0,-1],[4,4,1]),
               ("cliff","cube",[3,0,.4],[1,1,1])])
  self.assertEqual(check_new_silhouette(p,"terrain")["new_object_count"],2)
if __name__=="__main__":unittest.main()
