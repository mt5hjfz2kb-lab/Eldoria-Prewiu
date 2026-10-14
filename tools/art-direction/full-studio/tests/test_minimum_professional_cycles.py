"""Professional artists never self-stop before configured real finishing cycles."""
import unittest,pathlib,sys
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from artist_loop import should_continue_after_critique as decide
class CycleTests(unittest.TestCase):
 def test_two_real_successes_required_despite_self_praise(self):
  self.assertTrue(decide(1,3,False,{"clearly_failed":False},2,1))
  self.assertFalse(decide(2,3,False,{"clearly_failed":False},2,2))
 def test_rejection_or_no_success_continues(self):
  self.assertTrue(decide(2,3,False,{"clearly_failed":True},2,1))
 def test_maximum_never_exceeded(self):
  self.assertFalse(decide(3,3,False,{"clearly_failed":True},2,1))
 def test_backwards_compatible_default(self):
  self.assertFalse(decide(1,3,False,{"clearly_failed":False}))
  self.assertTrue(decide(1,3,True,{"clearly_failed":False}))
if __name__=="__main__":unittest.main()
