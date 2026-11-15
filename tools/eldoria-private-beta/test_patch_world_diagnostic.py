import unittest,tempfile,json
from pathlib import Path
import patch_world_diagnostic as m
class PatchTests(unittest.TestCase):
 def fixture(self,root):
  route=root/'beta-integrated-vigilia';route.mkdir()
  (route/'play.js').write_text('function mount() {}\n"use strict";\n// Fetch already exposes decoded gzip/Brotli bytes.\nUNCHANGED_TRANSPORT')
  (route/'index.html').write_text('<script src="play-c2ce74d1da4dc8eb.js"></script>')
  (route/'native.wasm').write_bytes(b'EXACT NATIVE PLAYER')
  (root/'stable.html').write_text('PRESERVE STABLE BETA')
  def row(p):return {'bytes':p.stat().st_size,'sha256':m.sha(p)}
  data={'candidate_route':route.name,'source_sha':'d0b21fd5708c5070d1bd62dc6c6a98399c549051','candidate_fingerprints':{p.name:row(p) for p in route.iterdir()},'preserved_fingerprints':{'stable.html':row(root/'stable.html')},'combined_bytes':0}
  proof=root/'integrated-owner-stage-proof.json';proof.write_text(json.dumps(data));m.PROOF_SHA=m.sha(proof);m.PLAY_SHA=m.sha(route/'play.js')
 def test_preserves_every_native_and_protected_byte(self):
  with tempfile.TemporaryDirectory() as t:
   root=Path(t);self.fixture(root);m.patch(root)
   self.assertEqual((root/'beta-integrated-vigilia/native.wasm').read_bytes(),b'EXACT NATIVE PLAYER')
   self.assertEqual((root/'stable.html').read_text(),'PRESERVE STABLE BETA')
   self.assertTrue((root/'beta-integrated-vigilia/play.js').read_text().endswith('UNCHANGED_TRANSPORT'))
   proof=json.loads((root/'integrated-owner-stage-proof.json').read_text())
   self.assertEqual(proof['combined_bytes'],sum(p.stat().st_size for p in root.rglob('*') if p.is_file()))
   self.assertFalse(proof['diagnostic_shell']['physical_iphone_fixed'])
 def test_refuses_changed_native_bytes(self):
  with tempfile.TemporaryDirectory() as t:
   root=Path(t);self.fixture(root);(root/'beta-integrated-vigilia/native.wasm').write_bytes(b'WRONG')
   with self.assertRaises(AssertionError):m.patch(root)
if __name__=='__main__':unittest.main()
