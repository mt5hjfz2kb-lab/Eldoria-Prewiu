import hashlib
import json
import tarfile
import tempfile
import unittest
from pathlib import Path
from types import SimpleNamespace
from urllib.parse import urlsplit
import restore_published_owner_snapshot as target


class PublicRecoveryTests(unittest.TestCase):
    def make(self, root):
        content = {'protected/save-route.txt': b'preserve owner route',
                   'beta-integrated-vigilia/index.html': b'current player'}
        row = lambda b: {'bytes': len(b), 'sha256': hashlib.sha256(b).hexdigest()}
        proof = {'source_sha': 'source', 'candidate_route': 'beta-integrated-vigilia',
                 'preserved_fingerprints': {'protected/save-route.txt': row(content['protected/save-route.txt'])},
                 'candidate_fingerprints': {'index.html': row(content['beta-integrated-vigilia/index.html'])},
                 'combined_bytes': 0}
        for _ in range(8):
            blob = (json.dumps(proof, indent=2) + '\n').encode()
            proof['combined_bytes'] = len(blob) + sum(map(len, content.values()))
        blob = (json.dumps(proof, indent=2) + '\n').encode()
        content['integrated-owner-stage-proof.json'] = blob
        path = root / 'proof.json'; path.write_bytes(blob)
        args = SimpleNamespace(proof=path, proof_sha256=hashlib.sha256(blob).hexdigest(),
              source='source', origin='https://example.test', output=root/'snapshot', archive=root/'snapshot.tar')
        def fetch(url, dest):
            dest.write_bytes(content[urlsplit(url).path.lstrip('/')])
        return args, content, fetch

    def test_complete_archive_preserves_every_certified_public_byte(self):
        with tempfile.TemporaryDirectory() as folder:
            args, content, fetch = self.make(Path(folder))
            result = target.restore(args, fetch)
            self.assertEqual(result['files'], 3)
            with tarfile.open(args.archive) as tar:
                self.assertEqual({n: tar.extractfile(n).read() for n in tar.getnames()}, content)

    def test_changed_live_file_blocks_creation_of_replacement_archive(self):
        with tempfile.TemporaryDirectory() as folder:
            args, content, fetch = self.make(Path(folder))
            content['protected/save-route.txt'] = b'changed route'
            with self.assertRaisesRegex(ValueError, 'Public file has changed'):
                target.restore(args, fetch)
            self.assertFalse(args.archive.exists())

    def test_traversal_cannot_escape_recovery_directory(self):
        for path in ('../outside', '/absolute', 'C:/outside', 'a\\outside'):
            with self.assertRaises(ValueError): target.safe_path(path)


if __name__ == '__main__':
    unittest.main()
