"""Reproduce Windows-style checkout conversion with real Git, without Unity."""
import pathlib
import subprocess
import tempfile
import unittest


class CheckoutBytesTests(unittest.TestCase):
    def test_text_attributes_need_explicit_lf_even_with_autocrlf_disabled(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = pathlib.Path(temporary)
            source = root / 'source'
            source.mkdir()

            def git(directory, *arguments):
                return subprocess.run(['git', '-C', str(directory), *arguments],
                                      check=True, capture_output=True)

            git(source, 'init')
            git(source, 'config', 'user.name', 'Checkout fixture')
            git(source, 'config', 'user.email', 'fixture@example.invalid')
            original = b'fileFormatVersion: 2\nguid: 01234567890123456789012345678901\n'
            (source / 'Asset.meta').write_bytes(original)
            (source / '.gitattributes').write_bytes(b'* text=auto\n')
            git(source, 'add', '.')
            git(source, '-c', 'commit.gpgsign=false', 'commit', '-m', 'fixture')
            for ending, exact in [('crlf', False), ('lf', True)]:
                checkout = root / ending
                git(root, 'clone', '--no-checkout', str(source), str(checkout))
                git(checkout, 'config', 'core.autocrlf', 'false')
                git(checkout, 'config', 'core.eol', ending)
                git(checkout, 'checkout', '--force', 'HEAD')
                actual = (checkout / 'Asset.meta').read_bytes()
                self.assertEqual(actual == original, exact)
                self.assertEqual(actual.replace(b'\r\n', b'\n'), original)


if __name__ == '__main__':
    unittest.main()
