import importlib.util
import json
from pathlib import Path
import os
import subprocess
import tempfile
import unittest

spec = importlib.util.spec_from_file_location('organization_audit', Path(__file__).with_name('audit.py'))
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class InventoryTests(unittest.TestCase):
    def test_real_git_tree_detects_missing_path_and_preserves_duplicate_paths(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            (root / 'docs').mkdir()
            (root / 'docs/documentation-catalog.json').write_text(json.dumps({'entries': []}))
            (root / 'README.md').write_text('[present](a.txt) [missing](missing.txt)\n')
            (root / 'a.txt').write_text('same bytes')
            (root / 'b.txt').write_text('same bytes')
            subprocess.run(['git', 'init', '-q', folder], check=True)
            subprocess.run(['git', '-C', folder, 'add', '.'], check=True)
            subprocess.run(['git', '-C', folder, '-c', 'user.name=Inventory fixture',
                            '-c', 'user.email=fixture@example.invalid', 'commit', '-qm', 'fixture'], check=True)
            previous = os.getcwd()
            try:
                os.chdir(folder)
                result = module.audit('HEAD')
            finally:
                os.chdir(previous)
            self.assertEqual(result['coverage']['tracked_entries'], 4)
            self.assertEqual(result['coverage']['inline_local_links_checked'], 2)
            self.assertEqual([x['resolved'] for x in result['unresolved_inline_links']], ['missing.txt'])
            self.assertEqual(result['duplicate_groups'][0]['paths'], ['a.txt', 'b.txt'])
            self.assertTrue((root / 'a.txt').exists() and (root / 'b.txt').exists())

    def test_link_titles_spaces_external_links_and_fenced_examples(self):
        text = ('[file](<folder/a b.md>) [title](a.md "A title") '
                '[remote](https://example.invalid) [anchor](#part)\n'
                '```md\n[example](does-not-exist.md)\n```\n'
                '[after](real.md#section)\n')
        self.assertEqual(list(module.local_links(text)), ['folder/a b.md', 'a.md', 'real.md'])


if __name__ == '__main__':
    unittest.main()
