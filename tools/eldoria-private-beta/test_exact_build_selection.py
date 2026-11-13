import json
import tempfile
import unittest
from pathlib import Path
from prepare_lazy_owner_site import exact_build_dir, WEB_STATUS

class ExactBuildSelectionTests(unittest.TestCase):
    def setUp(self):
        self.tmp=tempfile.TemporaryDirectory();self.addCleanup(self.tmp.cleanup)
        self.root=Path(self.tmp.name);self.sha='a'*40
        self.selected=self.root/'pr179-lazy-full-webgl-43211338159-1'
        self.make(self.selected)
        self.web=dict(source_sha=self.sha,status=WEB_STATUS,bytes=8,
            files=['Builds/ValoriaWebPlaytest/index.html','Builds/ValoriaWebPlaytest/Build/engine.wasm'])
    def make(self,p):
        (p/'Build').mkdir(parents=True)
        (p/'index.html').write_bytes(b'page')
        (p/'Build/engine.wasm').write_bytes(b'wasm')
    def select(self):
        return exact_build_dir(self.root,self.web,43211338159,self.sha,
            proof_path=self.root/'selection.json')
    def test_exact_recorded_build_and_hashes_preserve_sibling(self):
        stale=self.root/'pr179-lazy-full-webgl-40000000000-1';self.make(stale)
        self.assertEqual(self.select(),self.selected)
        proof=json.loads((self.root/'selection.json').read_text())
        self.assertEqual(proof['ignored_siblings'],[stale.name])
        self.assertEqual(len(proof['files']),2)
        self.assertTrue(all(len(x['sha256'])==64 for x in proof['files']))
        self.assertEqual((stale/'Build/engine.wasm').read_bytes(),b'wasm')
    def test_reject_wrong_run_even_if_only_build(self):
        self.selected.rename(self.root/'pr179-lazy-full-webgl-1-1')
        with self.assertRaisesRegex(ValueError,'exact native'):self.select()
    def test_reject_extra_file(self):
        (self.selected/'unexpected').write_text('stale')
        with self.assertRaisesRegex(ValueError,'membership'):self.select()
    def test_reject_wrong_total_bytes(self):
        self.web['bytes']=9
        with self.assertRaisesRegex(ValueError,'byte count'):self.select()
    def test_reject_symlink(self):
        file=self.selected/'Build/engine.wasm';file.unlink()
        target=self.root/'outside';target.write_bytes(b'wasm');file.symlink_to(target)
        with self.assertRaisesRegex(ValueError,'Symlink'):self.select()
    def test_corrected_flat_upload(self):
        self.selected.rename(self.root/'container')
        actual=self.root/'container'
        self.assertEqual(exact_build_dir(actual,self.web,43211338159,self.sha),actual)

if __name__=='__main__':unittest.main()
