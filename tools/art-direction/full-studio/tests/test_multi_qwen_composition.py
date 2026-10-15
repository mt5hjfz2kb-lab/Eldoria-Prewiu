"""Original Qwen can author a 2-3 beam assembly; verifier never builds it."""
import copy,pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from assembly_composition_gate import verify_qwen_composition
from artist_loop import schema_for_plan
from contract import RejectedPlan

SOURCE=[{"name":"Original Valoria timber tower","faces":5338,"edges":12000,
    "world_bounds_min_m":[-1.22387,-1.325,0],
    "world_bounds_max_m":[1.22387,1.325,3.27077]}]
def new(name,point,size):
    return {"id":"act-"+name[-7:],"op":"primitive","target":name,
            "reason":"Qwen authored unique physical timber architecture",
            "params":{"shape":"cube","location":point,"rotation":[0,0,0],"scale":size}}
A=new("Eldoria_Added_C01_TimberWallBrace",[1.10,.90,2.50],[.20,.15,.40])
B=new("Eldoria_Added_C01_RoofCrossBeam",[.90,-.80,2.35],[.40,.13,.10])

class MultiQwenArchitecture(unittest.TestCase):
    def test_original_separate_components_can_make_valid_attached_beams(self):
        source=copy.deepcopy([A,B])
        proof=verify_qwen_composition({"actions":source},SOURCE,1)
        self.assertEqual(proof["original_component_count"],2)
        self.assertFalse(proof["art_approved"])
        self.assertFalse(proof["actual_surface_contact_verified"])
        self.assertEqual(source,[A,B],"Gate must not author or alter the artist's values")
    def test_reject_real_float_as_one_of_three_components(self):
        floating=new("Eldoria_Added_C01_BadFlyingWood",[.5,2.5,1.2],[.3,.3,.3])
        with self.assertRaisesRegex(RejectedPlan,"NO_SOURCE_CONTACT_AABB"):
            verify_qwen_composition({"actions":[A,B,floating]},SOURCE,1)
    def test_not_just_one_primitive_and_material(self):
        finish={"op":"material","target":A["target"],
                "params":{"base_color":[.4,.2,.1,1],"roughness":.8,"metallic":0}}
        with self.assertRaisesRegex(RejectedPlan,"at least two"):
            verify_qwen_composition({"actions":[A,finish]},SOURCE,1)
    def test_new_component_names_cannot_collide_or_escape_current_cycle(self):
        with self.assertRaisesRegex(RejectedPlan,"unique"):
            verify_qwen_composition({"actions":[A,A]},SOURCE,1)
        with self.assertRaisesRegex(RejectedPlan,"unique"):
            verify_qwen_composition({"actions":[A,{**B,"target":"Eldoria_Added_C02_CrossBeam"}]},SOURCE,1)
    def test_finish_acts_on_current_created_part_not_frozen_tower(self):
        invalid={"op":"material","target":SOURCE[0]["name"],"params":{}}
        with self.assertRaisesRegex(RejectedPlan,"not an actual earlier component"):
            verify_qwen_composition({"actions":[A,B,invalid]},SOURCE,1)
    def test_genuinely_multi_qwen_typed_schema_preserves_freedom(self):
        sch=schema_for_plan(4,True,"full","architecture",
                            [SOURCE[0]["name"]],SOURCE,True,True,1,False,
                            set(),True)
        arr=sch["properties"]["actions"]
        self.assertEqual((arr["minItems"],arr["maxItems"]),(2,4))
        for i in (0,1):
            choices={v["properties"]["op"]["const"] for v in arr["prefixItems"][i]["oneOf"]}
            self.assertEqual(choices,{"primitive","mesh"})
        self.assertIn("material",{v["properties"]["op"]["const"]
                        for v in arr["prefixItems"][2]["oneOf"]})
        self.assertEqual({v["properties"]["op"]["const"]
                        for v in arr["prefixItems"][3]["oneOf"]},{"material","uv"})
if __name__=="__main__":unittest.main()
