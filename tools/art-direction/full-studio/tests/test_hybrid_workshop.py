import copy,json,pathlib,sys,unittest
ROOT=pathlib.Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'full-studio'));sys.path.insert(0,str(ROOT/'gpt6-tower-eval-20261015'))
from hybrid_trial import common_schema
from joinery_contract import validate,JoineryRejected
GOOD=json.loads((ROOT/'gpt6-tower-eval-20261015/gpt6-joinery-plan-r3.json').read_text())

class WorkshopTests(unittest.TestCase):
    def test_same_workshop_accepts_existing_real_plan(self):
        import jsonschema
        jsonschema.validate(GOOD,common_schema())
        self.assertEqual(validate(GOOD)['joint_count'],2)
    def test_hidden_disconnected_joint_rejected_in_xy(self):
        p=copy.deepcopy(GOOD);p['joints'][0]['center_m'][0]+=1
        with self.assertRaisesRegex(JoineryRejected,'OUTSIDE_POST_XY'):validate(p)
    def test_peg_cannot_become_freestanding(self):
        p=copy.deepcopy(GOOD);p['joints'][0]['peg']['center_m'][1]+=2
        with self.assertRaisesRegex(JoineryRejected,'DOES_NOT_INTERSECT'):validate(p)

if __name__=='__main__':unittest.main()
