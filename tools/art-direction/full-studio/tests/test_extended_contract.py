"""The extended native and node graph surface is a constrained Blender toolkit."""
import pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from contract import validate_plan,RejectedPlan
from extended_ops import SAFE_NATIVE_OPS,SAFE_SHADER_NODES,SAFE_GEOMETRY_NODES

def base(action):
 return {"schema_version":2,"asset_id":"test-node-geometry",
 "observed_defect":"The original geometry has no artist-authored native node detailing.",
 "design_intent":"Test typed native Blender editing and node data without arbitrary scripts.",
 "asset_decision":"NEW",
 "expected_visual_difference":"Build and refine real mesh geometry through native Blender edit tools.",
 "actions":[{"id":"mesh","op":"primitive","target":"Ground","reason":"Build real test geometry before editing it","params":{"shape":"cube","scale":[1,1,1]}} ,action]}

class ExtendedTests(unittest.TestCase):
 def test_native_operator_good(self):
  d=base({"id":"native","op":"native_operator","target":"Ground","reason":"Smooth real mesh with an allowed Blender operator","params":{"operator":"object.shade_smooth","kwargs":{}}})
  self.assertGreaterEqual(validate_plan(d)["geometric_actions"],2)
 def test_python_command_rejected(self):
  d=base({"id":"native","op":"native_operator","target":"Ground","reason":"Must not execute an untrusted Python command","params":{"operator":"script.python_file_run","kwargs":{"filepath":"rm -rf /"}}})
  with self.assertRaises(RejectedPlan):validate_plan(d)
 def test_shell_payload_rejected(self):
  d=base({"id":"native","op":"native_operator","target":"Ground","reason":"Disallow dangerous path or shell arguments in Blender operations","params":{"operator":"mesh.subdivide","kwargs":{"evil":"/etc/passwd"}}})
  with self.assertRaises(RejectedPlan):validate_plan(d)
 def test_material_nodes_accept(self):
  d=base({"id":"shader","op":"shader_graph","target":"Ground","reason":"Produce real editable PBR material through native nodes","params":{"nodes":[{"id":"a","type":"ShaderNodeBsdfPrincipled","inputs":{"Roughness":.7}},{"id":"b","type":"ShaderNodeOutputMaterial"}],"links":[{"from":"a","output":"BSDF","to":"b","input":"Surface"}]}})
  validate_plan(d)
 def test_geometry_nodes_accept(self):
  d=base({"id":"graph","op":"geometry_nodes","target":"Ground","reason":"Build native editable Geometry Nodes group for reuse","params":{"nodes":[{"id":"a","type":"NodeGroupInput"},{"id":"b","type":"NodeGroupOutput"}],"links":[{"from":"a","output":"Geometry","to":"b","input":"Geometry"}]}})
  validate_plan(d)
 def test_script_node_rejected(self):
  d=base({"id":"shader","op":"shader_graph","target":"Ground","reason":"Shader code execution must not be allowed by the graph","params":{"nodes":[{"id":"a","type":"ShaderNodeScript"},{"id":"b","type":"ShaderNodeOutputMaterial"}],"links":[]}})
  with self.assertRaises(RejectedPlan):validate_plan(d)
 def test_missing_graph_socket_source_rejected(self):
  d=base({"id":"shader","op":"shader_graph","target":"Ground","reason":"Never link non-existent graph nodes in a Blender material","params":{"nodes":[{"id":"b","type":"ShaderNodeOutputMaterial"}],"links":[{"from":"missing","output":"BSDF","to":"b","input":"Surface"}]}})
  with self.assertRaises(RejectedPlan):validate_plan(d)
 def test_whitelists_nonempty(self):
  self.assertGreater(len(SAFE_NATIVE_OPS),10)
  self.assertGreater(len(SAFE_SHADER_NODES),10)
  self.assertGreater(len(SAFE_GEOMETRY_NODES),10)
if __name__=="__main__":unittest.main()
