"""Human-authored locked-camera access masks reject Qwen overbuilds, never art authoring."""
import json,pathlib,sys,tempfile,unittest
from PIL import Image,ImageDraw
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from protected_camera_regions import inspect_protected_regions
class AccessQA(unittest.TestCase):
 def test_reject_obscured_entry_and_preserve_safe_changes(self):
  with tempfile.TemporaryDirectory() as directory:
   d=pathlib.Path(directory)
   review=d/"review.json";review.write_text(json.dumps({
      "protected_screen_regions":[{"name":"entry","box_px":[20,20,60,60],
                                    "max_changed_fraction":0.20}]}))
   original=Image.new("RGB",(100,100),"#aaaaaa");original.save(d/"before.png")
   after=original.copy();ImageDraw.Draw(after).rectangle((20,20,59,59),fill="#101010")
   after.save(d/"blocker.png")
   verdict=inspect_protected_regions(d/"before.png",d/"blocker.png",review)
   self.assertFalse(verdict["allowed"]);self.assertGreater(verdict["zones"][0]["changed_fraction"],0.99)
   self.assertFalse(verdict["art_approved"]);self.assertTrue(verdict["no_artist_parameters_modified"])
   safe=original.copy();ImageDraw.Draw(safe).rectangle((65,65,95,95),fill="#101010")
   safe.save(d/"safe.png")
   accepted=inspect_protected_regions(d/"before.png",d/"safe.png",review)
   self.assertTrue(accepted["allowed"]);self.assertLess(accepted["zones"][0]["changed_fraction"],.001)
 def test_missing_regions_never_suggests_quality_approval(self):
  with tempfile.TemporaryDirectory() as directory:
   d=pathlib.Path(directory)
   (d/"review.json").write_text("{}")
   img=Image.new("RGB",(10,10),"white");img.save(d/"image.png")
   proof=inspect_protected_regions(d/"image.png",d/"image.png",d/"review.json")
   self.assertTrue(proof["allowed"]);self.assertFalse(proof["art_approved"])
if __name__=="__main__":unittest.main()
