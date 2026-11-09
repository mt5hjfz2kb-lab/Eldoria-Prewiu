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
 def test_candidate_shell_must_not_redirect_to_stable_game(self):
    html=self.new/'index.html'
    html.write_text('<html><meta http-equiv="refresh" content="0;url=../beta-native325/"></html>')
    with self.assertRaisesRegex(ValueError,'redirects'):stage(*self.args)
    self.assertFalse(self.out.exists())
 def test_candidate_loader_cannot_fetch_stable_old_binary(self):
    script=self.new/'play.js'
    script.write_text("fetch('https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/beta-native325/Build/old.data')")
    with self.assertRaisesRegex(ValueError,'stable beta10'):stage(*self.args)
    self.assertFalse(self.out.exists())
 def test_size_cap_fails_closed_without_writing_public_site(self):
    with self.assertRaisesRegex(ValueError,'Too large'):stage(*self.args,byte_limit=128)
    self.assertFalse(self.out.exists())
 def test_saved_game_never_used_or_changed(self):
    (self.stable/'beta-native325/player-data.txt').write_text('DO NOT TOUCH GAME SAVE')
    stage(*self.args)
    self.assertEqual((self.out/'beta-native325/player-data.txt').read_text(),'DO NOT TOUCH GAME SAVE')
    self.assertEqual((self.stable/'beta-native325/player-data.txt').read_text(),'DO NOT TOUCH GAME SAVE')

 def test_genuine_chunked_webgl_data_is_valid_without_monolith(self):
    # Real frozen beta10 and candidate loaders request .data.unityweb but
    # both receive their data via decoded_parts (31 gzip chunks).
    for base in (self.stable/'beta-native325',self.new):
        path=base/'Build/data.bin'
        path.unlink()
        meta=base/'build-manifest.json';m=json.loads(meta.read_text())
        source=b'original Unity assets\x00\x01'
        for part in m['decoded_parts'][m['data']]:
            part.update(encoding='gzip',decoded_bytes=len(source),
                        decoded_sha256=hashlib.sha256(source).hexdigest())
        # Two compressed chunks and an exact decoded_sizes value are required
        # when the monolithic WebGL data file intentionally does not exist.
        old=m['decoded_parts'][m['data']][0]
        (base/'payload-decoded/chunk1.gz').write_bytes(gzip.compress(source))
        m['decoded_parts'][m['data']]=[old,{**old,'url':'payload-decoded/chunk1.gz'}]
        m['decoded_sizes']={m['data']:len(source)*2}
        meta.write_text(json.dumps(m))
    proof=stage(*self.args)
    self.assertEqual(proof['role'],'OWNER_AB_IOS_QA_ONLY')
    self.assertEqual(json.loads((self.out/CANDIDATE_ROUTE/'build-manifest.json').read_text())['decoded_sizes']['Build/data.bin'],len(source)*2)
    self.assertFalse((self.out/'beta-native325/Build/data.bin').exists())
    self.assertFalse((self.out/CANDIDATE_ROUTE/'Build/data.bin').exists())
 def test_streaming_chunk_cannot_be_missing_or_corrupt(self):
    for base in (self.stable/'beta-native325',self.new):
        (base/'Build/data.bin').unlink()
        meta=base/'build-manifest.json';m=json.loads(meta.read_text())
        source=b'original Unity assets\x00\x01'
        old=m['decoded_parts'][m['data']][0]
        old.update(encoding='gzip',decoded_bytes=len(source),
                   decoded_sha256=hashlib.sha256(source).hexdigest())
        (base/'payload-decoded/chunk1.gz').write_bytes(gzip.compress(source))
        m['decoded_parts'][m['data']]=[old,{**old,'url':'payload-decoded/chunk1.gz'}]
        m['decoded_sizes']={m['data']:len(source)*2}
        meta.write_text(json.dumps(m))
    victim=self.new/'payload-decoded/chunk1.gz'
    victim.unlink()
    with self.assertRaisesRegex(ValueError,'Missing binary part'):stage(*self.args)
    self.assertFalse(self.out.exists())
    victim.write_bytes(gzip.compress(b'corrupt data'))
    with self.assertRaisesRegex(ValueError,'Corrupt decoded WebGL binary'):stage(*self.args)
    self.assertFalse(self.out.exists())
 def test_split_data_fails_closed_on_wrong_total(self):
    for base in (self.stable/'beta-native325',self.new):
        (base/'Build/data.bin').unlink()
        meta=base/'build-manifest.json';m=json.loads(meta.read_text())
        source=b'original Unity assets\x00\x01'
        old=m['decoded_parts'][m['data']][0]
        old.update(encoding='gzip',decoded_bytes=len(source),
                   decoded_sha256=hashlib.sha256(source).hexdigest())
        (base/'payload-decoded/chunk1.gz').write_bytes(gzip.compress(source))
        m['decoded_parts'][m['data']]=[old,{**old,'url':'payload-decoded/chunk1.gz'}]
        m['decoded_sizes']={m['data']:1}
        meta.write_text(json.dumps(m))
    with self.assertRaisesRegex(ValueError,'Streamed WebGL byte-count mismatch'):stage(*self.args)
    self.assertFalse(self.out.exists())

if __name__=='__main__':unittest.main()
