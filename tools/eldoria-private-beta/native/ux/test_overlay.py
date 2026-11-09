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
 def test_scene_anchored_construction_clock_is_exact_and_no_fixed_dock(self):
  root=Path(__file__).parent
  manifest=json.loads((root/'manifest.json').read_text(encoding='utf-8'))
  entry=next(x for x in manifest['files'] if x['overlay']=='RoyalGoldV3ContextHud.cs.txt')
  candidate=(root/entry['overlay']).read_bytes()
  self.assertEqual(hashlib.sha256(candidate).hexdigest(),entry['candidate_sha256'])
  text=candidate.decode('utf-8')
  self.assertIn('private bool TryPlaceConstructionTimer(',text)
  self.assertIn('FindAnchor(id == "upgrade-barracks" ? "barracks" : id)',text)
  self.assertIn('CanProject(target, camera, safe, true, out projectedRoof)',text)
  self.assertIn('timer.Root.anchoredPosition = center;',text)
  self.assertIn('BUILDING_NOT_VISIBLE_OR_NO_ROOM',text)
  # There may still be an operation dock for scouting/travel, never for buildings.
  self.assertIn('if (!IsConstructionTimer(candidate)',text)
  render=text.split('private void RenderTimer(',1)[1]
  self.assertIn('if (construction)',render)
  self.assertIn('TryPlaceConstructionTimer(id, camera',render)
  probe=(root/'PretestUXProbe.cs').read_text(encoding='utf-8')
  self.assertIn('construction timer moves on-screen with the camera pan',probe)
  self.assertIn('timer hides when its building leaves the camera',probe)
  self.assertNotIn('timer stays at fixed HUD position:',probe)
if __name__=='__main__':unittest.main()
