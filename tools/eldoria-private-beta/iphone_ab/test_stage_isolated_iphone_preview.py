import gzip,hashlib,json,tempfile,unittest
from pathlib import Path
from stage_isolated_iphone_preview import stage,STABLE_BUNDLE,STABLE_REVISION,STABLE_SOURCE,CANDIDATE_ROUTE

NEW_SOURCE='687111873d7a2274c62f1c88d392d99594f17afd'
NEW_REV='native325-mobile12-minimal-world-clock-qa'
NEW_BUNDLE='abcd'*16

def create(root,revision,source,bundle,native):
    root.mkdir(parents=True,exist_ok=True)
    (root/'index.html').write_text('<html><head></head><body>Unity genuine entry</body></html>')
    (root/'play.js').write_text('// self-hosted game boot, no test shortcut')
    id={'revision':revision,'source_sha':source,'candidate_bundle_sha256':bundle,
        'release_for_external_testers':False,'native_journey_verified':native,
        'physical_mobile_tested':False}
    (root/'release-identity.json').write_text(json.dumps(id))
    (root/'Build').mkdir()
    for a in ('loader','framework','code','data'):
        (root/'Build'/(a+'.bin')).write_bytes((a*12).encode())
    (root/'payload-decoded').mkdir()
    compressed=gzip.compress(b'original Unity assets\x00\x01')
    (root/'payload-decoded/chunk0.gz').write_bytes(compressed)
    man={**{a:'Build/'+a+'.bin' for a in ('loader','framework','code','data')},
         'decoded_parts':{'Build/data.bin':[{'url':'payload-decoded/chunk0.gz'}]}}
    (root/'build-manifest.json').write_text(json.dumps(man))
    return root

class Tests(unittest.TestCase):
 def setUp(self):
    self.tmp=tempfile.TemporaryDirectory();self.addCleanup(self.tmp.cleanup)
    self.root=Path(self.tmp.name)
    self.stable=self.root/'base';create(self.stable/'beta-native325',STABLE_REVISION,STABLE_SOURCE,STABLE_BUNDLE,True)
    (self.stable/'unity-owner').mkdir();(self.stable/'unity-owner/index.html').write_text('legacy owner link')
    self.new=create(self.root/'new',NEW_REV,NEW_SOURCE,NEW_BUNDLE,True)
    self.out=self.root/'output'
    self.args=(self.stable,self.new,self.out,NEW_REV,NEW_SOURCE,NEW_BUNDLE)
 def test_isolated_candidate_preserves_all_stable_bytes(self):
    old=(self.stable/'beta-native325/index.html').read_bytes()
    proof=stage(*self.args)
    self.assertEqual(proof['role'],'OWNER_AB_IOS_QA_ONLY')
    self.assertFalse(proof['external_testers_enabled'])
    self.assertFalse(proof['physical_iphone_stability_verified'])
    self.assertEqual((self.out/'beta-native325/index.html').read_bytes(),old)
    self.assertEqual((self.out/'unity-owner/index.html').read_text(),'legacy owner link')
    self.assertEqual(json.loads((self.out/CANDIDATE_ROUTE/'release-identity.json').read_text())['source_sha'],NEW_SOURCE)
    with self.assertRaisesRegex(ValueError,'Never overwrite'):stage(*self.args)
 def test_wrong_candidate_or_unsound_gates_are_rejected(self):
    meta=self.new/'release-identity.json';j=json.loads(meta.read_text());j['release_for_external_testers']=True;meta.write_text(json.dumps(j))
    with self.assertRaisesRegex(ValueError,'External tester'):stage(*self.args)
    self.assertFalse(self.out.exists())
 def test_remote_paths_and_missing_data_fail_before_copy(self):
    m=self.new/'build-manifest.json';obj=json.loads(m.read_text());obj['data']='https://other.example.com/data.bin';m.write_text(json.dumps(obj))
    with self.assertRaisesRegex(ValueError,'Unsafe or remote'):stage(*self.args)
    self.assertFalse(self.out.exists())
 def test_size_cap_fails_closed_without_writing_public_site(self):
    with self.assertRaisesRegex(ValueError,'Too large'):stage(*self.args,byte_limit=128)
    self.assertFalse(self.out.exists())
 def test_saved_game_never_used_or_changed(self):
    (self.stable/'beta-native325/player-data.txt').write_text('DO NOT TOUCH GAME SAVE')
    stage(*self.args)
    self.assertEqual((self.out/'beta-native325/player-data.txt').read_text(),'DO NOT TOUCH GAME SAVE')
    self.assertEqual((self.stable/'beta-native325/player-data.txt').read_text(),'DO NOT TOUCH GAME SAVE')

if __name__=='__main__':unittest.main()
