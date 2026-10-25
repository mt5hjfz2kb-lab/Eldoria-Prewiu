"""Tests for read-only asset discovery and reuse search, without Unity/Blender."""
import importlib.util
import tempfile
import unittest
from pathlib import Path

HERE = Path(__file__).parent

def load(name, filename):
    spec = importlib.util.spec_from_file_location(name, HERE / filename)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module

class VaultTests(unittest.TestCase):
    def test_known_paths_include_all_tripo_variants(self):
        scanner = load("scan_candidates", "scan-candidates.py")
        example = {"records":[{"path":"main.glb","editable":"source.blend",
                   "sources":{"raw":{"path":"original.glb"}},
                   "integration":{"near":{"path":"near.glb"}}}]}
        self.assertTrue({"main.glb","source.blend","original.glb","near.glb"}.issubset(
            scanner.known_paths(example)))

    def test_rejected_never_auto_promoted(self):
        search = load("find_reuse", "find-reuse.py")
        self.assertLess(search.RANK["REJECT_CURRENT_VISUAL"],0)
        self.assertLess(search.RANK["REJECT_FOR_CURRENT_CANON"],0)

    def test_aliases_support_owner_terms(self):
        search = load("find_reuse", "find-reuse.py")
        self.assertIn("quarry", search.ALIASES["cantera"])
        self.assertIn("boar", search.ALIASES["jabalí"])
        self.assertIn("spawnling", search.ALIASES["engendro"])

    def test_tracked_tree_includes_sparse_unchecked_paths(self):
        scanner = load("scan_candidates", "scan-candidates.py")
        original = scanner.git
        try:
            scanner.git = lambda *args: (
                b"100644 blob abcdef 120\tAssets/Rock.glb\0"
                b"100644 blob 112233 240\tAssets/Source.blend\0"
                b"100644 blob 111111 11\tAssets/ignore.png\0"
            )
            self.assertEqual(scanner.tracked(),
                             ["Assets/Rock.glb", "Assets/Source.blend"])
        finally:
            scanner.git = original

if __name__=="__main__":
    unittest.main()
