"""Global Blender coordinate facts never equal raw imported GLB local coordinate assumptions."""
import pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from world_space_evidence import summarize_world_points
class WorldCoordinates(unittest.TestCase):
    def test_real_world_up_z_and_bounds(self):
        world=[(-1,-1,0),(1,-1,0),(1,1,0),(-1,1,0),
               (-1,-1,2.8),(1,-1,2.8),(1,1,2.8),(-1,1,2.8)]
        g=summarize_world_points(world)
        self.assertEqual(g["world_up_axis"],"+Z")
        self.assertEqual(g["world_bounds_min_m"],[-1,-1,0])
        self.assertEqual(g["world_bounds_max_m"],[1,1,2.8])
        self.assertEqual(g["world_bounds_center_m"],[0,0,1.4])
        self.assertEqual(g["world_bounds_dimensions_m"],[2,2,2.8])
    def test_reject_empty_and_nonfinite(self):
        with self.assertRaises(ValueError):summarize_world_points([])
        with self.assertRaises(ValueError):summarize_world_points([(1,float("nan"),2)])
if __name__=="__main__":unittest.main()
