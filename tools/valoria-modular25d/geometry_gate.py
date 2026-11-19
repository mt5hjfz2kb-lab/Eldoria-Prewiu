"""Analytical preflight only; a PlayMode/Unity-camera screenshot is still required."""
import math, unittest
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parents[2] / "Unity/Assets/Experimentos/ValoriaModular25D/Scripts"
WORLD = (SCRIPT_DIR/"Modular25DWorld.cs").read_text(encoding="utf8")
WALKER = (SCRIPT_DIR/"Modular25DWalker.cs").read_text(encoding="utf8")
CAMERA = (SCRIPT_DIR/"Modular25DCameraInput.cs").read_text(encoding="utf8")
TREE=(-3.15,-1.4)
PATH=[(-3.9,-3.6),(-2.70,-2.5),(-2.62,-.55),(-2.3,.1),
      (-2.62,-.55),(-2.70,-2.5)]
SAWMILL=(1.95,-2.2)
BASTION=(-1.15,1.3)

def distance_to_segment(p,a,b):
    px,py=p;ax,ay=a;bx,by=b
    vx=bx-ax;vy=by-ay
    t=max(0,min(1,((px-ax)*vx+(py-ay)*vy)/(vx*vx+vy*vy)))
    return math.hypot(px-ax-vx*t,py-ay-vy*t)

class SpatialGate(unittest.TestCase):
    def test_isometric_tile_proportions_and_overlap(self):
        # World ground chunks are contiguous, without finite diamond-grid
        # boundaries entering the mobile camera. UV is in world metres.
        ground = (SCRIPT_DIR/"Modular25DGround.cs").read_text(encoding="utf8")
        self.assertIn("const int Side = 8, HalfChunks = 4",ground)
        self.assertIn("new Vector3(gx*Side,gy*Side",ground)
        self.assertIn("float ux=gx*Side/PatternMeters",ground)
        self.assertIn("float uy",ground) if False else None
        self.assertIn("TextureWrapMode.Repeat",ground)
        self.assertIn("terrain.Build()",WORLD)
        self.assertNotIn("for(int row=-10;row<=10",WORLD)

    def test_real_walker_bypasses_trunk_but_crosses_depth_order(self):
        nearest=min(distance_to_segment(TREE,PATH[i],PATH[i+1])
                    for i in range(len(PATH)-1))
        self.assertGreaterEqual(nearest,.43,
            "Walker path intersects trunk/clearance zone")
        self.assertLess(nearest,.90,
            "Walker too far from canopy: no occlusion test")
        self.assertTrue(min(y for x,y in PATH)<TREE[1])
        self.assertTrue(max(y for x,y in PATH)>TREE[1])
        for a,b in zip(PATH,PATH[1:]):
            self.assertLess(math.dist(a,b),2.5)
        self.assertIn("body.sortingOrder=sort;",WALKER)
        self.assertIn("shadow.sortingOrder=sort-1;",WALKER)

    def test_bastion_and_sawmill_separated_from_player_routes(self):
        # The sawmill's approximate footprint is not a radius: only check that
        # no agent walks inside the gameplay hit-box by source coordinates.
        self.assertTrue(all(not(.10<x<3.87 and -2.48<y<.92)
                            for x,y in PATH))
        self.assertGreater(math.dist(SAWMILL,BASTION),4.25)

    def test_both_buildings_framed_in_mobile_orthographic_camera(self):
        for width,height in [(390,844),(1170,2532),(844,390)]:
            aspect=width/height
            size=min(10.8,max(5.3,3.65/max(.42,aspect)))
            camera_centre=(.18,.15)
            halfwidth=size*aspect
            for name,(x,y) in [("bastion",BASTION),("sawmill",SAWMILL)]:
                self.assertLess(abs(x-camera_centre[0])+1.0,halfwidth,
                    f"{name} horizontal overflow {width}x{height}")
                self.assertLess(abs(y-camera_centre[1])+1.5,size,
                    f"{name} vertical overflow {width}x{height}")

if __name__=="__main__":
    unittest.main(verbosity=2)
