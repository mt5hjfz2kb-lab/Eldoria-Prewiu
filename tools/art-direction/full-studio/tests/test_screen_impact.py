"""Exact image comparison is technical presence evidence, NOT art approval."""
import sys,tempfile,unittest
from pathlib import Path
from PIL import Image
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from screen_impact import screen_impact

class ScreenImpactTests(unittest.TestCase):
 def test_same_camera_visible_geometry(self):
  with tempfile.TemporaryDirectory() as d:
   a=Path(d)/"before.png";b=Path(d)/"after.png"
   im=Image.new("RGB",(100,100),(200,200,200));im.save(a)
   for x in range(20,80):
    for y in range(20,80):im.putpixel((x,y),(60,30,10))
   im.save(b)
   result=screen_impact(a,b)
   self.assertTrue(result["visually_present"])
   self.assertEqual(result["changed_pixels"],3600)
   self.assertEqual(result["bounding_rectangle"],[20,20,80,80])
   self.assertFalse(result["art_approved"])
 def test_invisible_small_edit_rejected(self):
  with tempfile.TemporaryDirectory() as d:
   a=Path(d)/"before.png";b=Path(d)/"after.png"
   im=Image.new("RGB",(100,100),(0,0,0));im.save(a)
   for x in range(2):
    for y in range(2):im.putpixel((x,y),(255,255,255))
   im.save(b)
   self.assertFalse(screen_impact(a,b)["visually_present"])
 def test_different_camera_canvas_rejected(self):
  with tempfile.TemporaryDirectory() as d:
   a=Path(d)/"before.png";b=Path(d)/"after.png"
   Image.new("RGB",(100,100)).save(a)
   Image.new("RGB",(120,100)).save(b)
   with self.assertRaises(ValueError):screen_impact(a,b)
if __name__=="__main__":unittest.main()
