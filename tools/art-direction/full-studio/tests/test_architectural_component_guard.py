"""Autonomous Qwen still authors all volume; noncreative QA rejects placemats."""
import copy,pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from architectural_component_guard import verify_volume_and_complexity,RejectedPlan
from artist_loop import schema_for_plan
def doc(op,params):
 return {"actions":[{"op":op,"target":"Eldoria_Added_GenuineTower",
                    "params":params}]}
class ComponentQuality(unittest.TestCase):
 def test_reject_actual_two_triangle_flat_platform(self):
  p=doc("mesh",{"vertices":[[0,0,0],[1,0,0],[1,1,0],[0,1,0]],"faces":[[0,1,2],[2,3,0]]})
  saved=copy.deepcopy(p)
  with self.assertRaisesRegex(RejectedPlan,"six vertices"):verify_volume_and_complexity(p)
  self.assertEqual(p,saved)
 def test_reject_coplanar_even_with_many_verts(self):
  p=doc("mesh",{"vertices":[[0,0,0],[1,0,0],[1,1,0],[0,1,0],[2,0,0],[2,1,0]],
        "faces":[[0,1,2],[0,2,3],[1,4,5],[1,5,2]]})
  with self.assertRaisesRegex(RejectedPlan,"volume"):verify_volume_and_complexity(p)
 def test_artist_designed_triangular_prism_accepted(self):
  p=doc("mesh",{"vertices":[[0,0,0],[1,0,0],[0,1,0],[0,0,1],[1,0,1],[0,1,1]],
                 "faces":[[0,1,2],[3,4,5],[0,1,4,3],[1,2,5,4],[2,0,3,5]]})
  frozen=copy.deepcopy(p);self.assertTrue(verify_volume_and_complexity(p));self.assertEqual(p,frozen)
 def test_artist_chooses_new_primitive_not_plane(self):
  with self.assertRaisesRegex(RejectedPlan,"flat PLANE"):
   verify_volume_and_complexity(doc("primitive",{"shape":"plane","scale":[.4,.4,.4]}))
  self.assertTrue(verify_volume_and_complexity(doc("primitive",{"shape":"cube","scale":[.4,.35,.7]})))
 def test_exact_assembly_schema_prevents_flat_primitives(self):
  src=[{"name":"Frozen source","faces":5000,"vertices":10000}]
  s=schema_for_plan(4,True,"full","architecture",["Frozen source"],src,True,True)
  ss=s["properties"]["actions"]["prefixItems"][0]["oneOf"]
  shape=next(x for x in ss if x["properties"]["op"]["const"]=="primitive")["properties"]["params"]["properties"]["shape"]
  mesh=next(x for x in ss if x["properties"]["op"]["const"]=="mesh")["properties"]["params"]["properties"]
  self.assertNotIn("plane",shape["enum"])
  self.assertEqual(mesh["vertices"]["minItems"],6)
  self.assertEqual(mesh["faces"]["minItems"],4)
if __name__=="__main__":unittest.main()
