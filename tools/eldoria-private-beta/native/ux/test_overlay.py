import hashlib,json,tempfile,unittest
from pathlib import Path
from apply_overlay import apply
class TransactionTest(unittest.TestCase):
 def test_later_mismatch_does_not_partially_write(self):
  with tempfile.TemporaryDirectory() as tmp:
   root=Path(tmp);source=root/'source';overlay=root/'overlay';source.mkdir();overlay.mkdir();entries=[]
   for name in ['first.cs','second.cs']:
    (source/name).write_bytes(b'old');(overlay/(name+'.txt')).write_bytes(b'new')
    entries.append(dict(path=name,overlay=name+'.txt',baseline_sha256=hashlib.sha256(b'old').hexdigest(),candidate_sha256=hashlib.sha256(b'new').hexdigest()))
   manifest=overlay/'manifest.json';manifest.write_text(json.dumps(dict(files=entries)))
   (source/'second.cs').write_bytes(b'foreign edit')
   with self.assertRaises(ValueError):apply(source,manifest)
   self.assertEqual((source/'first.cs').read_bytes(),b'old')
   self.assertEqual((source/'second.cs').read_bytes(),b'foreign edit')
   (source/'second.cs').write_bytes(b'old');self.assertEqual(apply(source,manifest),2)
   self.assertEqual((source/'first.cs').read_bytes(),b'new')
if __name__=='__main__':unittest.main()
