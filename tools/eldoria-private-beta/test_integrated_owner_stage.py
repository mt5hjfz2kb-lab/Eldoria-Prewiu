"""Synthetic file-preservation tests; not gameplay or device certification."""
import gzip
import hashlib
import json
import shutil
import tarfile
import tempfile
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch

import stage_integrated_owner_site as stage


class IntegratedStageTests(unittest.TestCase):
    def setUp(self):
        temp = tempfile.TemporaryDirectory()
        self.addCleanup(temp.cleanup)
        self.root = Path(temp.name)
        old = self.root / 'old'
        for route, source in dict(stage.STABLE, **{'beta-native325':
                'be49d62e3daf490f41dba0bf8123a8e2eee64480'}).items():
            self.write(old / route / 'release-identity.json', {'source_sha': source})
        for route in (*stage.RETIRED, *stage.STABLE):
            for i in range(35):
                p = old / route / (str(i) + '.bin')
                p.parent.mkdir(parents=True, exist_ok=True)
                p.write_bytes((route + str(i)).encode())
        (old / '.nojekyll').write_text('')
        archive = self.root / 'original.tar'
        with tarfile.open(archive, 'w') as out:
            out.add(old, arcname='.')
        self.before = stage.tree(old)
        self.archive_sha = stage.sha(archive)
        candidate = self.root / 'candidate'
        self.raw = b'synthetic exact native bytes'
        self.data_hash = hashlib.sha256(self.raw).hexdigest()
        identity = dict(source_sha=stage.SOURCE, candidate_bundle_sha256=stage.BUNDLE,
            delivered_data_sha256=self.data_hash, native_journey_verified=True,
            native_destination_captures_verified=True, release_for_external_testers=False,
            physical_mobile_tested=False)
        self.write(candidate / 'release-identity.json', identity)
        self.write(self.root / 'browser/candidate/report.json', dict(passed=True, errors=[],
            entry={'reloaded': True}, vigiliaProductionInstallations=2, identity=identity))
        data = candidate / 'data.gz'
        data.write_bytes(gzip.compress(self.raw))
        manifest = dict(data='native-data', decoded_sizes={'native-data': len(self.raw)},
            decoded_parts={'native-data': [dict(url='data.gz', encoding='gzip',
                decoded_bytes=len(self.raw), decoded_sha256=self.data_hash)]})
        for key in ('framework', 'code'):
            p = candidate / (key + '.gz')
            p.write_bytes(gzip.compress(key.encode()))
            url = p.name + '?v=' + stage.sha(p)[:16]
            manifest[key] = url
            manifest['decoded_sizes'][url] = len(key)
        self.write(candidate / 'build-manifest.json', manifest)
        self.args = SimpleNamespace(output=self.root / 'out', candidate=candidate,
            work=self.root / 'work', browser=self.root / 'browser', pages=archive)

    def write(self, path, value):
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(json.dumps(value))

    def test_retained_routes_and_hidden_files_are_byte_exact_and_archive_untouched(self):
        with patch.object(stage, 'DATA', self.data_hash):
            stage.stage(self.args)
        after = stage.tree(self.args.output)
        for path, value in self.before.items():
            if path.split('/')[0] not in stage.RETIRED:
                self.assertEqual(after[path], value)
        self.assertEqual(stage.sha(self.args.pages), self.archive_sha)
        proof = stage.read(self.args.output / 'integrated-owner-stage-proof.json')
        self.assertTrue(proof['required_durable_archive'])
        self.assertFalse(proof['published'])
        for route in stage.RETIRED:
            self.assertEqual(list((self.args.output / route).iterdir()),
                             [self.args.output / route / 'index.html'])

    def prepare_replacement(self):
        old = self.root / 'old'
        # A real beta19 publication has archived beta10 and the clock route:
        # neither retains its old engine or release-identity.json.
        for route in stage.RETIRED:
            shutil.rmtree(old / route)
            (old / route).mkdir()
            (old / route / 'index.html').write_text('Archived diagnostic; use beta18')
        self.write(old / stage.ROUTE / 'release-identity.json', {'source_sha': 'prior-source'})
        for i in range(12):
            (old / stage.ROUTE / (str(i) + '.bin')).write_bytes(b'previous beta19')
        self.write(old / 'integrated-owner-stage-proof.json', {'source_sha': 'prior-source'})
        (old / 'unrelated-new-route').mkdir()
        (old / 'unrelated-new-route/index.html').write_text('Retain concurrent publication')
        with tarfile.open(self.args.pages, 'w') as out:
            out.add(old, arcname='.')
        self.before = stage.tree(old)
        self.archive_sha = stage.sha(self.args.pages)
        self.args.replace_source = 'prior-source'

    def test_repair_replaces_only_vigilia_and_preserves_all_other_routes(self):
        self.prepare_replacement()
        with patch.object(stage, 'DATA', self.data_hash):
            stage.stage(self.args)
        after = stage.tree(self.args.output)
        for path, value in self.before.items():
            if not path.startswith(stage.ROUTE + '/') and path != 'integrated-owner-stage-proof.json':
                self.assertEqual(after[path], value)
        self.assertEqual(stage.sha(self.args.pages), self.archive_sha)
        proof = stage.read(self.args.output / 'integrated-owner-stage-proof.json')
        self.assertEqual(proof['archived_routes'], [])
        self.assertEqual(proof['replaced_source_sha'], 'prior-source')
        self.assertEqual(proof['replaced_fingerprints'][stage.ROUTE + '/0.bin'],
                         self.before[stage.ROUTE + '/0.bin'])
        self.assertNotIn(stage.ROUTE + '/0.bin', after)

    def test_repair_refuses_a_different_public_source(self):
        self.prepare_replacement()
        self.args.replace_source = 'unexpected-source'
        with patch.object(stage, 'DATA', self.data_hash), self.assertRaisesRegex(ValueError, 'Unexpected previously'):
            stage.stage(self.args)
        self.assertEqual(stage.sha(self.args.pages), self.archive_sha)
        self.assertEqual(stage.tree(self.args.output), self.before)

    def test_corrupted_candidate_never_changes_original_or_creates_site(self):
        (self.args.candidate / 'data.gz').write_bytes(gzip.compress(b'changed'))
        with patch.object(stage, 'DATA', self.data_hash), self.assertRaises(RuntimeError):
            stage.stage(self.args)
        self.assertFalse(self.args.output.exists())
        self.assertEqual(stage.sha(self.args.pages), self.archive_sha)

    def test_browser_failure_blocks_stage(self):
        p = self.root / 'browser/candidate/report.json'
        report = stage.read(p)
        report['passed'] = False
        self.write(p, report)
        with self.assertRaisesRegex(ValueError, 'WebKit failed'):
            stage.stage(self.args)
        self.assertFalse(self.args.output.exists())

    def test_reused_part_filename_gets_a_new_http_cache_key_without_changing_bytes(self):
        candidate = self.args.candidate
        before = stage.tree(candidate)
        stage.version_decoded_fetches(candidate)
        first = stage.read(candidate / 'build-manifest.json')['decoded_parts']['native-data'][0]['url']
        for path, value in before.items():
            if path != 'build-manifest.json':
                self.assertEqual(stage.tree(candidate)[path], value)
        # Same published filename, different subsequent build: old cached HTTP
        # response must not be addressed by the new manifest.
        (candidate / 'data.gz').write_bytes(gzip.compress(b'next native build'))
        payload = (candidate / 'data.gz').read_bytes()
        stage.version_decoded_fetches(candidate)
        second = stage.read(candidate / 'build-manifest.json')['decoded_parts']['native-data'][0]['url']
        self.assertNotEqual(first, second)
        self.assertEqual(stage.safe_path(first), stage.safe_path(second))
        self.assertEqual((candidate / 'data.gz').read_bytes(), payload)


if __name__ == '__main__':
    unittest.main()
