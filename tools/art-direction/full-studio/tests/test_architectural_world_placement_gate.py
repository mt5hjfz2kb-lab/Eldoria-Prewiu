"""Physical QA rejects door-blocking and sideways chimney without moving AI geometry."""
import copy,pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from architectural_world_placement_gate import reject_obviously_bad_component_placement
from contract import RejectedPlan
BASE=[{"name":"Actual imported GLB source","evaluated_triangles":33546,
       "world_bounds_min_m":[-1.22387,-1.325,0.0],
       "world_bounds_max_m":[1.22387,1.325,3.27077]}]
def primitive(name,shape,location,scale,rotation=(0,0,0)):
 return {"actions":[{"op":"primitive","target":name,
          "params":{"shape":shape,"location":list(location),"scale":list(scale),"rotation":list(rotation)}}]}
class Coherence(unittest.TestCase):
 def test_real_ai_door_blocker_rejected(self):
  p=primitive("Eldoria_Added_C01_Tower_Entrance_Extension","cube",[0,0,0],[.8,1.2,.8])
  original=copy.deepcopy(p)
  with self.assertRaisesRegex(RejectedPlan,"BLOCKS"):reject_obviously_bad_component_placement(p,BASE)
  self.assertEqual(p,original)
 def test_real_ai_horizontal_chimney_rejected(self):
  p=primitive("Eldoria_Added_C02_Chimney","cylinder",[0,3,0],[.5,2,.5])
  with self.assertRaisesRegex(RejectedPlan,"WORLD"):reject_obviously_bad_component_placement(p,BASE)
 def test_tower_roof_chimney_vertical_allowed_but_not_approved(self):
  p=primitive("Eldoria_Added_C04_Chimney","cylinder",[.7,.7,3.2],[.25,.25,.6])
  original=copy.deepcopy(p);result=reject_obviously_bad_component_placement(p,BASE)
  self.assertFalse(result["art_approved"]);self.assertEqual(p,original)
 def test_slim_support_attachment_is_allowed(self):
  p=primitive("Eldoria_Added_C04_StoneSupport","cube",[1.2,0,1.4],[.28,.28,.5])
  self.assertEqual(reject_obviously_bad_component_placement(p,BASE)["changes_to_artist_values"],0)
 def test_real_qwen_coat_of_arms_invisible_internal_cube_rejected(self):
  p=primitive("Eldoria_Added_C01_Tower_CoatOfArms","cube",
              [0.0,0.0,1.63539],[0.2,0.3,0.15])
  original=copy.deepcopy(p)
  with self.assertRaisesRegex(RejectedPlan,"COMPLETELY BURIED"):
   reject_obviously_bad_component_placement(p,BASE)
  self.assertEqual(p,original)
 def test_real_qwen_small_belfry_inside_tower_rejected(self):
  p=primitive("Eldoria_Added_C03_Tower_Belfry_01","cylinder",
              [0.0,0.0,2.0],[0.5,0.5,0.5])
  with self.assertRaisesRegex(RejectedPlan,"COMPLETELY BURIED"):
   reject_obviously_bad_component_placement(p,BASE)
 def test_artist_exterior_door_trim_and_roof_detail_not_forbidden(self):
  door=primitive("Eldoria_Added_C02_External_DoorTrim","cube",
                 [0,-1.15,1.0],[.25,.15,.3])
  crown=primitive("Eldoria_Added_C03_Stone_Crown","cone",
                  [0,0,3.18],[.3,.3,.2])
  for p in (door,crown):
   original=copy.deepcopy(p)
   result=reject_obviously_bad_component_placement(p,BASE)
   self.assertEqual(result["changes_to_artist_values"],0)
   self.assertEqual(p,original)
 def test_actual_pc_qwen_cycle1_pedestal_overlaps_front_low_center(self):
  p=primitive("Eldoria_Added_C01_Pedestal","cube",
              [0.0,-0.04135,0.15164],[0.8,0.3,0.8])
  copy_before=copy.deepcopy(p)
  with self.assertRaisesRegex(RejectedPlan,"BLOCKS"):
   reject_obviously_bad_component_placement(p,BASE)
  self.assertEqual(p,copy_before)
 def test_actual_pc_qwen_cycle2_block_protrudes_past_roof_gable(self):
  p=primitive("Eldoria_Added_C02_StoneWatchtower","cube",
              [0.0,3.0,2.0],[1.0,2.0,1.0])
  copy_before=copy.deepcopy(p)
  with self.assertRaisesRegex(RejectedPlan,"OUTSIZES"):
   reject_obviously_bad_component_placement(p,BASE)
  self.assertEqual(p,copy_before)
 def test_sensible_roof_attachment_and_slim_support_still_allowed(self):
  roof=primitive("Eldoria_Added_C03_RoofFinial","cone",
                 [0,0,3.2],[.15,.15,.4])
  side=primitive("Eldoria_Added_C04_SideBracket","cube",
                 [1.2,-0.2,2.5],[.18,.25,.2])
  for p in (roof,side):
   v=reject_obviously_bad_component_placement(p,BASE)
   self.assertFalse(v["art_approved"])
   self.assertEqual(v["changes_to_artist_values"],0)
 def test_disconnected_object_is_rejected(self):
  p=primitive("Eldoria_Added_C04_Support","cube",[10,0,1],[.25,.25,.5])
  with self.assertRaisesRegex(RejectedPlan,"disconnected"):reject_obviously_bad_component_placement(p,BASE)
if __name__=="__main__":unittest.main()
