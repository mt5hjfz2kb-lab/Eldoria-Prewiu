import json
import hashlib
import tempfile
import unittest
from pathlib import Path
from stage_engine18_owner_site import (
    NEW_ROUTE, PREVIOUS_SOURCE, PREVIOUS_BUNDLE, replace_only_prior_candidate,
    tree, verify_binary_urls,
)


class OwnerReplacementContract(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self.tmp.cleanup)
        self.root = Path(self.tmp.name)
        for i in range(144):
            p = self.root / 'stable' / str(i)
            p.parent.mkdir(exist_ok=True)
            p.write_bytes(('immutable-' + str(i)).encode())
        (self.root / '.nojekyll').write_bytes(b'')
        self.candidate = self.root / NEW_ROUTE
        self.candidate.mkdir()
        self.identity = dict(source_sha=PREVIOUS_SOURCE,
                             candidate_bundle_sha256=PREVIOUS_BUNDLE,
                             release_for_external_testers=False,
                             owner_delivery=dict(format='ELDORIA_COPY_ADD_V1'))
        for i in range(32):
            (self.candidate / str(i)).write_bytes(b'previous candidate')
        self.save_identity()

    def save_identity(self):
        (self.candidate / 'release-identity.json').write_text(json.dumps(self.identity))

    def test_replaces_only_pinned_candidate_preserving_hidden_and_all_stable_bytes(self):
        before = tree(self.root)
        protected, replaced = replace_only_prior_candidate(self.root)
        self.assertEqual(tree(self.root), protected)
        self.assertEqual({**protected, **replaced}, before)
        self.assertIn('.nojekyll', protected)
        self.assertFalse(self.candidate.exists())

    def test_refuses_changed_live_candidate_without_removing_any_file(self):
        self.identity['source_sha'] = 'unknown'
        self.save_identity()
        before = tree(self.root)
        with self.assertRaisesRegex(RuntimeError, 'unrecognized'):
            replace_only_prior_candidate(self.root)
        self.assertEqual(tree(self.root), before)

    def test_refuses_external_release_identity_without_removing_any_file(self):
        self.identity['release_for_external_testers'] = True
        self.save_identity()
        before = tree(self.root)
        with self.assertRaisesRegex(RuntimeError, 'Owner-only'):
            replace_only_prior_candidate(self.root)
        self.assertEqual(tree(self.root), before)

    def test_native_versioned_urls_and_full_size_keys_are_preserved(self):
        manifest = dict(decoded_sizes={},decoded_parts={})
        for key,name in [('code','engine.unityweb'),('framework','framework.unityweb')]:
            data=name.encode()
            (self.root/name).write_bytes(data)
            url=name+'?v='+hashlib.sha256(data).hexdigest()[:16]
            manifest[key]=url
            manifest['decoded_sizes'][url]=len(data)
        before=json.loads(json.dumps(manifest))
        records=verify_binary_urls(self.root,manifest)
        self.assertEqual(manifest,before)
        for record in records:
            self.assertEqual(record['delivery_url'],manifest[record['role']])
            self.assertEqual(record['native_path'],record['delivery_path'])
            self.assertTrue((self.root/record['native_path']).is_file())

    def test_rejects_stale_cache_version_instead_of_rewriting_native_manifest(self):
        (self.root/'engine.unityweb').write_bytes(b'new native bytes')
        manifest=dict(code='engine.unityweb?v=0000000000000000',decoded_sizes={})
        with self.assertRaisesRegex(RuntimeError,'cache version'):
            verify_binary_urls(self.root,manifest)
        self.assertEqual((self.root/'engine.unityweb').read_bytes(),b'new native bytes')


if __name__ == '__main__':
    unittest.main()
