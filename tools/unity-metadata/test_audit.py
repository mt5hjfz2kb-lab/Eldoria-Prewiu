import unittest
import tempfile
import subprocess
from pathlib import Path
from audit import audit, ignored_metadata, verify_archives, REJECTED_RESOURCE


class AuditTests(unittest.TestCase):
    def test_rejected_resource_cannot_return_even_with_valid_metadata(self):
        entries = {REJECTED_RESOURCE: 'model', REJECTED_RESOURCE + '.meta': 'meta'}
        result = audit(entries, lambda p: b'guid: 0123456789abcdef0123456789abcdef\n', {})
        self.assertTrue(any('Owner-rejected legacy model' in f for f in result['failures']))

    def test_archive_preserves_both_original_blobs_outside_unity(self):
        destination = 'ArtSource/RejectedLegacyBastion/Hero.glb'
        manifest = {'archived_rejected_assets': [{'source': REJECTED_RESOURCE,
            'destination': destination, 'asset_blob_sha': 'original-model',
            'metadata_blob_sha': 'original-meta'}]}
        entries = {destination: 'original-model', destination + '.meta': 'original-meta'}
        self.assertFalse(verify_archives(entries, manifest))
        for changed in [{destination: 'changed'}, {}, {REJECTED_RESOURCE: 'original-model'}]:
            self.assertTrue(verify_archives(changed, manifest))

    def test_missing_file_requires_exact_baselined_asset(self):
        p = 'Unity/Assets/Hero.cs'
        self.assertTrue(audit({p: 'new'}, lambda p: b'', {})['failures'])
        baseline = {'allowed_missing': {p: 'old'}}
        self.assertTrue(audit({p: 'new'}, lambda p: b'', baseline)['failures'])
        result = audit({p: 'old'}, lambda p: b'', baseline)
        self.assertFalse(result['failures'])
        self.assertEqual(len(result['known_missing']), 1)

    def test_duplicate_guid_is_rejected(self):
        entries = {p: p for p in ['Unity/Assets/A.cs', 'Unity/Assets/B.cs',
                                  'Unity/Assets/A.cs.meta', 'Unity/Assets/B.cs.meta']}
        result = audit(entries, lambda p: b'guid: 0123456789abcdef0123456789abcdef\n', {})
        self.assertEqual(len(result['duplicate_guids']), 1)
        self.assertTrue(result['failures'])

    def test_empty_folder_meta_is_preserved(self):
        entries = {'Unity/Assets/Empty.meta': 'blob'}
        result = audit(entries, lambda p: b'guid: 0123456789abcdef0123456789abcdef\nfolderAsset: yes\n', {})
        self.assertFalse(result['failures'])
        self.assertEqual(result['preserved_empty_folder_metadata'], list(entries))

    def test_orphan_asset_meta_is_not_an_empty_folder(self):
        p = 'Unity/Assets/Lost.png.meta'
        result = audit({p: 'blob'}, lambda p: b'guid: 0123456789abcdef0123456789abcdef\n', {})
        self.assertTrue(result['failures'])
        self.assertEqual(result['orphan_files'][0]['path'], p)

    def test_bundled_plugin_internals_do_not_require_individual_meta(self):
        p = 'Unity/Assets/Plugins/Example.androidlib/build.gradle'
        result = audit({p: 'blob'}, lambda p: b'', {'allowed_missing_folders': [
            'Unity/Assets/Plugins', 'Unity/Assets/Plugins/Example.androidlib']})
        self.assertFalse(result['missing'])
        self.assertFalse(result['failures'])

    def test_bundle_root_itself_requires_identity(self):
        result = audit({'Unity/Assets/Example.androidlib/build.gradle': 'blob'}, lambda p: b'', {})
        self.assertIn('Unity/Assets/Example.androidlib', result['missing_folder_metadata'])

    def test_new_folder_without_identity_fails(self):
        result = audit({'Unity/Assets/New/File.cs': 'a', 'Unity/Assets/New/File.cs.meta': 'b'},
                       lambda p: b'guid: 0123456789abcdef0123456789abcdef\n', {})
        self.assertIn('Unity/Assets/New', result['missing_folder_metadata'])
        self.assertTrue(result['failures'])

    def test_invalid_guid_is_rejected(self):
        entries = {'Unity/Assets/Hero.cs': 'a', 'Unity/Assets/Hero.cs.meta': 'b'}
        self.assertTrue(audit(entries, lambda p: b'guid: invented\n', {})['failures'])

    def test_ignore_regression_is_detected_even_for_versioned_metadata(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            subprocess.run(['git', 'init', '-q', tmp], check=True)
            p = root / 'Unity/Assets/ArtTests/Hero.cs.meta'
            p.parent.mkdir(parents=True)
            p.write_text('guid: 0123456789abcdef0123456789abcdef\n')
            subprocess.run(['git', 'add', p.relative_to(root).as_posix()], cwd=root, check=True)
            ignore = root / 'Unity/.gitignore'
            ignore.write_text('Assets/ArtTests/**/*.meta\n')
            relative = p.relative_to(root).as_posix()
            self.assertEqual(ignored_metadata(root, [relative]), [relative])
            ignore.write_text('Library/\nTemp/\n')
            self.assertEqual(ignored_metadata(root, [relative]), [])


if __name__ == '__main__':
    unittest.main()
