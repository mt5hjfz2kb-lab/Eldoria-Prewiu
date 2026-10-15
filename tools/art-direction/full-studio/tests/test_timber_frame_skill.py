import copy,pathlib,sys,unittest
ROOT=pathlib.Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'full-studio'));sys.path.insert(0,str(ROOT/'gpt6-tower-eval-20261015'))
from timber_frame_skill import compile_joinery,validate
from joinery_contract import validate as joinery_validate
from contract import RejectedPlan

class AnchoredFrameTests(unittest.TestCase):
    def plan(self):
        return {'frame_skill_version':1,'art_approved':False,'asset_id':'test',
          'source_object':'MeasuredTower','anchor_face':1,'design_intent':'Test an artist-owned architectural composition with measured anchors.',
          'width_m':1.5,'height_m':1.95,'bottom_z_m':0.,'post_offsets_m':[-.65,.65],
          'post_width_m':.14,'depth_m':.16,'lintel_height_m':.14,'surface_offset_m':.01,
          'bevel_m':.008,'wood_color_rgba':[.3,.19,.095,1]}
    def test_mechanical_fit_preserves_composition(self):
        s=self.plan();original=copy.deepcopy(s)
        p=compile_joinery(s,[-.1093,-.8519,1.0277],[0,-1,0])
        self.assertEqual(joinery_validate(p)['joint_count'],2)
        self.assertEqual(s,original)
        self.assertAlmostEqual(p['members'][1]['center_m'][0],-.7593)
        self.assertAlmostEqual(p['members'][1]['center_m'][2]-p['members'][1]['size_m'][2]/2,0)
    def test_different_artist_post_count_and_width(self):
        s=self.plan();s['width_m']=2.;s['post_offsets_m']=[-.8,0,.8]
        p=compile_joinery(s,[.5,-.9,1.],[0,-1,0])
        self.assertEqual(joinery_validate(p)['member_count'],4)
    def test_bad_composition_is_rejected_not_relocated(self):
        s=self.plan();s['post_offsets_m']=[-.9,.9]
        with self.assertRaisesRegex(RejectedPlan,'OUTSIDE_ARTIST'):validate(s)
    def test_roof_face_cannot_silently_become_wall(self):
        with self.assertRaisesRegex(RejectedPlan,'VERTICAL_WALL'):compile_joinery(self.plan(),[0,0,2],[0,0,1])
if __name__=='__main__':unittest.main()
