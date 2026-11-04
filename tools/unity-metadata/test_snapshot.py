import hashlib
import tempfile
import unittest
from pathlib import Path
from snapshot import git_blob, project_roots, snapshot


class SnapshotTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        self.project = self.root / 'checkout/Unity'
        (self.project / 'ProjectSettings').mkdir(parents=True)
        (self.project / 'ProjectSettings/ProjectVersion.txt').write_text('6000.3.23f1')
        self.asset = self.project / 'Assets/Hero.cs'
        self.asset.parent.mkdir()
        self.asset.write_bytes(b'class Hero {}\n')
        self.meta = Path(str(self.asset) + '.meta')
        self.meta.write_bytes(b'fileFormatVersion: 2\nguid: abcdef0123456789abcdef0123456789\n')
        self.request = {'source_sha': 'test', 'targets': [
            {'path': 'Unity/Assets/Hero.cs', 'blob_sha': git_blob(self.asset)}]}

    def tearDown(self):
        self.temp.cleanup()

    def test_git_blob_matches_git_object_format(self):
        data = self.asset.read_bytes()
        expected = hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()
        self.assertEqual(git_blob(self.asset), expected)

    def test_copies_exact_metadata_without_touching_project(self):
        original = self.meta.read_bytes(), self.asset.read_bytes(), self.meta.stat().st_mtime_ns
        report = snapshot(self.request, [self.root], self.root / 'result')
        self.assertEqual(len(report['candidates']), 1)
        copied = self.root / 'result' / report['candidates'][0]['snapshot_path']
        self.assertEqual(copied.read_bytes(), original[0])
        self.assertEqual(original, (self.meta.read_bytes(), self.asset.read_bytes(), self.meta.stat().st_mtime_ns))

    def test_different_asset_is_not_recovered(self):
        self.asset.write_text('different revision')
        report = snapshot(self.request, [self.root], self.root / 'result')
        self.assertFalse(report['candidates'])
        self.assertEqual(report['observations'][0]['status'], 'ASSET_DIFFERS')

    def test_missing_metadata_is_not_generated(self):
        self.meta.unlink()
        report = snapshot(self.request, [self.root], self.root / 'result')
        self.assertFalse(report['candidates'])
        self.assertFalse(self.meta.exists())

    def test_search_does_not_enter_unity_cache(self):
        p = self.root / 'Library/other'
        (p / 'Assets').mkdir(parents=True)
        (p / 'ProjectSettings').mkdir()
        (p / 'ProjectSettings/ProjectVersion.txt').write_text('test')
        self.assertEqual(project_roots([self.root])[0], [self.project])

    def test_refuses_reuse_of_snapshot_destination(self):
        out = self.root / 'result'
        out.mkdir()
        with self.assertRaises(FileExistsError):
            snapshot(self.request, [self.root], out)

    def test_refuses_path_escape(self):
        self.request['targets'][0]['path'] = 'Unity/Assets/../../outside'
        with self.assertRaises(ValueError):
            snapshot(self.request, [self.root], self.root / 'result')

    def test_hashing_budget_is_enforced(self):
        report = snapshot(self.request, [self.root], self.root / 'result', byte_budget=0)
        self.assertFalse(report['candidates'])
        self.assertEqual(report['observations'][0]['status'], 'SKIPPED')


if __name__ == '__main__':
    unittest.main()
