"""Source/model checks for the beta10-only clock hotfix.

These tests are deliberately NOT Unity tests or iPhone certificates.
They catch detached HUD clocks before the expensive real-device A/B.
"""
import math
import unittest
from pathlib import Path

CLOCK = Path(__file__).resolve().parents[1] / "native/iphone-hotfix/RoyalGoldV3ContextHud.beta10-minimal.cs.txt"


def model_position(roof, foot, width=220, height=46, rect=(-200,-420,200,420),
                   roof_visible=True, foot_visible=True):
    """Model the source's bounded anchor placement, not actual Unity geometry."""
    xmin,ymin,xmax,ymax=rect
    city_x = roof[0] if roof_visible else (foot[0] if foot_visible else None)
    if city_x is None:
        return None
    minx=xmin+width*.5+8
    maxx=xmax-width*.5-8
    if minx>maxx:
        return None
    x=max(minx,min(maxx,city_x))
    if abs(x-city_x)>28:
        return None
    low=ymin+90+height*.5
    high=ymax-94-height*.5
    y=(roof[1]+height*.5+10) if roof_visible else (foot[1]-height*.5-10)
    if y<low or y>high:
        y=foot[1]-height*.5-10 if foot_visible else math.nan
    if math.isnan(y) or y<low or y>high:
        return None
    return x,y


class MinimalClockSourceTest(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.text = CLOCK.read_text(encoding="utf-8")

    def test_only_one_existing_hud_and_no_new_gameplay_state(self):
        t=self.text
        self.assertEqual(t.count("class RoyalGoldV3ContextHud"),1)
        self.assertEqual(t.count("private void RenderTimer("),1)
        self.assertEqual(t.count("private static bool CanProject("),1)
        self.assertIn("private static bool IsCityConstruction(string id)",t)
        self.assertNotIn("private bool TryPlaceConstructionTimer(",t)
        self.assertNotIn("new Vector3[4]",t)
        self.assertNotIn("SaveGame(",t)
        self.assertNotIn("StartCoroutine(",t)
        self.assertIn('Frame("V3 / real-time operation / " + id, safe, false)',t)

    def test_real_building_projection_and_bounded_detachment(self):
        t=self.text.split("private void RenderTimer(",1)[1]
        self.assertIn('FindAnchor(id == "upgrade-barracks" ? "barracks" : id)',t)
        self.assertIn('CanProject(target, camera, safe, true, out roof)',t)
        self.assertIn('if (!roofVisible)',t)
        self.assertIn('if (Mathf.Abs(x - xAnchor) > 28f)',t)
        self.assertIn("float upper = bounds.yMax - 94f - height * .5f",t)
        self.assertIn("if (float.IsNaN(y) || y < lower || y > upper)",t)
        self.assertIn("timer.Root.anchoredPosition = center;",t)

    def test_clock_is_not_a_dock_and_world_jobs_keep_old_dock(self):
        t=self.text.split("private void RenderTimer(",1)[1]
        city=t.split("if (city)",1)[1].split("else\n            {",1)[0]
        non_city=t.split("else\n            {",1)[1]
        self.assertIn("center = new Vector2(x, y);",city)
        self.assertNotIn("safe.rect.xMax - width * .5f - 12f",city)
        self.assertIn("safe.rect.xMax - width * .5f - 12f",non_city)
        self.assertIn("!IsCityConstruction(candidate)",non_city)

    def test_visible_above_roof_or_below_real_foot(self):
        self.assertEqual(model_position((0,0),(0,-50)),(0,33))
        self.assertEqual(model_position((0,300),(0,80)),(0,47))
        self.assertEqual(model_position((0,500),(0,150),roof_visible=False), (0,117))

    def test_hide_when_object_offscreen_or_no_safe_room(self):
        self.assertIsNone(model_position((0,0),(0,0),roof_visible=False,foot_visible=False))
        self.assertIsNone(model_position((180,0),(180,-50)))
        self.assertIsNone(model_position((0,340),(0,-400)))
        self.assertIsNone(model_position((0,0),(0,-50),rect=(-100,-150,100,150)))
        self.assertIsNone(model_position((0,0),(0,-50),width=390,rect=(-200,-420,200,420)))

    def test_smooth_camera_pan_not_stuck_at_screen_edge(self):
        before=model_position((3,15),(3,-70))
        after=model_position((43,15),(43,-70))
        self.assertIsNotNone(before)
        self.assertIsNotNone(after)
        self.assertAlmostEqual(after[0]-before[0],40)
        self.assertAlmostEqual(after[1]-before[1],0)


if __name__=="__main__":
    unittest.main()
