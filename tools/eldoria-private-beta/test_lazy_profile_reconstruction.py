import sys,tempfile,unittest
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from stage_lazy_profile_owner_site import derive
class ForwardOnlyReconstruction(unittest.TestCase):
 def verify(self,old,new):
  ops=derive(old,new);out=bytearray();cursor=0
  for kind,at,n in ops:
   self.assertGreater(n,0)
   if kind=='copy':
    self.assertGreaterEqual(at,cursor);cursor=at+n;out.extend(old[at:at+n])
   else:out.extend(new[at:at+n])
  self.assertEqual(bytes(out),new);return ops
 def test_identical(self):
  b=bytes(range(256))*2048;ops=self.verify(b,b);self.assertEqual(ops,[('copy',0,len(b))])
 def test_shifted_header_and_short_tail(self):
  b=bytes(range(251))*5000;self.verify(b,b'new native header'+b+b'end')
 def test_deleted_middle_does_not_read_backwards(self):
  a=b'A'*131072;b=b'B'*131072;c=b'C'*131072
  self.verify(a+b+c,a+c)
 def test_changed_chunk_rejoins_source(self):
  b=bytes(range(251))*1500;c=bytes(range(249))*1500
  self.verify(b+c,b+b'changed'*20000+c)
 def test_no_anchor_and_zero_length(self):
  self.verify(b'a'*70000,b'Q'*150001);self.verify(b'abc',b'');self.verify(b'',b'abc')
if __name__=='__main__':unittest.main(verbosity=2)
