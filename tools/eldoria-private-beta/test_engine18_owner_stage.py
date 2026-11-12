import json
import tempfile
import unittest
from pathlib import Path
from stage_engine18_owner_site import (
    NEW_ROUTE, PREVIOUS_SOURCE, PREVIOUS_BUNDLE, replace_only_prior_candidate,
    tree, version_binary_urls,
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

    def test_binary_cache_keys_change_while_exact_native_bytes_remain(self):
        manifest = dict(code='engine.unityweb', framework='framework.unityweb',
                        decoded_sizes={'engine.unityweb': 10, 'framework.unityweb': 20},
                        decoded_parts={'engine.unityweb': [dict(url='engine.unityweb')]})
        original = {name: name.encode() for name in manifest['decoded_sizes']}
        for name, data in original.items():
            (self.root / name).write_bytes(data)
        aliases = version_binary_urls(self.root, manifest)
        for alias in aliases:
            self.assertNotEqual(alias['native_path'], alias['delivery_path'])
            self.assertEqual((self.root / alias['delivery_path']).read_bytes(),
                             original[alias['native_path']])
            self.assertNotIn(alias['native_path'], manifest['decoded_sizes'])
        self.assertEqual(manifest['decoded_parts'][manifest['code']][0]['url'], manifest['code'])


if __name__ == '__main__':
    unittest.main()
