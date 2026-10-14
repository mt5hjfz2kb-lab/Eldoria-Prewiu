"""Verified, topic-neutral artistic lesson retrieval; no old Qwen plan replay."""
import hashlib,sys,unittest
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from learning_context import transferable_lessons

class MemoryTests(unittest.TestCase):
 def test_old_failed_cliff_plan_not_used_as_new_tower_instruction(self):
  raw=("Historical artist rejected old action run #38323534537 "
       "sculpt_region center [-40,57,-21.6243] and delta [.5,.5,.5].\n"
       "- Improve the overall medieval silhouette and coherently integrated materials rather than adding invisible decorative noise.\n"
       "- Compare with the actual player camera and ensure the architecture remains visually readable at small strategy zoom.\n"
       "- Reuse actual modular meshes and make the foundation join the surrounding terrain convincingly.\n")
  out=transferable_lessons(raw,"medieval stone tower",limit=600)
  self.assertTrue(out["verbatim_grounded"])
  self.assertEqual(out["journal_sha256"],hashlib.sha256(raw.encode()).hexdigest())
  self.assertIn("silhouette",out["prompt_text"].lower())
  for invalid in ("38323534537","[-40","sculpt_region","21.6243"):
   self.assertNotIn(invalid,out["prompt_text"])
 def test_nothing_is_invented_from_empty_journal(self):
  self.assertEqual(transferable_lessons("","new tower")["lessons"],[])
 def test_bounded_and_stable(self):
  raw=("- Improve overall medieval silhouette with independent UV layouts and coherent PBR material surfaces.\n")*100
  result=transferable_lessons(raw,"tower",limit=300)
  self.assertLessEqual(len(result["prompt_text"]),300)
  self.assertLessEqual(len(result["lessons"]),6)
if __name__=="__main__":unittest.main()
