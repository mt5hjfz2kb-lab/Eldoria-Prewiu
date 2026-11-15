import copy,unittest
from check_lease import validate,ID,OWNER,BRANCH
class LeaseTests(unittest.TestCase):
 def setUp(self):
  self.head='a'*40
  self.reg={'active':[{'id':ID,'owner':OWNER,'branch':BRANCH,'ownership_state':'claimed','status':'active','resources':['windows-runner-heavy'],'native_source_sha':self.head,'ownership_revision':2}]}
 def test_exact_sole_holder(self):self.assertEqual(validate(self.reg,self.head,self.head),2)
 def test_busy_foreign_owner(self):
  self.reg['active'][0]['owner']='other-chat'
  with self.assertRaises(RuntimeError):validate(self.reg,self.head,self.head)
 def test_foreign_task_same_owner(self):
  self.reg['active'][0]['id']='other-experiment'
  with self.assertRaises(RuntimeError):validate(self.reg,self.head,self.head)
 def test_two_holders(self):
  self.reg['active'].append(copy.deepcopy(self.reg['active'][0]))
  with self.assertRaises(RuntimeError):validate(self.reg,self.head,self.head)
 def test_no_holder(self):
  self.reg['active'][0]['resources']=[]
  with self.assertRaises(RuntimeError):validate(self.reg,self.head,self.head)
 def test_moved_branch(self):
  with self.assertRaises(RuntimeError):validate(self.reg,self.head,'b'*40)
 def test_wrong_pinned_source(self):
  self.reg['active'][0]['native_source_sha']='b'*40
  with self.assertRaises(RuntimeError):validate(self.reg,self.head,self.head)
 def test_inactive_claim(self):
  self.reg['active'][0]['status']='complete'
  with self.assertRaises(RuntimeError):validate(self.reg,self.head,self.head)
if __name__=='__main__':unittest.main()
