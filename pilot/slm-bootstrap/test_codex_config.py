import importlib.util
from pathlib import Path
import tempfile
import tomllib
import unittest

SCRIPT = Path(__file__).parent / "Connect-SlmCodex.py"
spec = importlib.util.spec_from_file_location("slm_codex_safe", SCRIPT)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class CodexConfigTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.config = self.root / ".codex" / "config.toml"
        self.exe = self.root / "memory" / "Scripts" / "slm.exe"
        self.exe.parent.mkdir(parents=True)
        self.exe.write_text("stub", encoding="utf-8")
        self.store = self.root / "store"
        self.store.mkdir()
        self.backup = self.root / "backups"

    def connect(self):
        return module.configure(self.config, self.exe, self.store, self.backup)

    def parse(self):
        return tomllib.loads(self.config.read_text(encoding="utf-8-sig"))

    def test_new_config(self):
        r = self.connect()
        self.assertEqual(r["result"], "configured")
        a = self.parse()["mcp_servers"]["superlocalmemory"]
        self.assertEqual(a["command"], str(self.exe))
        self.assertEqual(a["args"], ["mcp"])
        self.assertEqual(a["env"]["SLM_HOME"], str(self.store))
        self.assertEqual(a["env"]["SLM_MCP_PROFILE"], "core")

    def test_idempotent(self):
        self.connect()
        b = self.config.read_bytes()
        r = self.connect()
        self.assertEqual(r["result"], "unchanged")
        self.assertEqual(self.config.read_bytes(), b)

    def test_preserves_other_servers(self):
        self.config.parent.mkdir(parents=True, exist_ok=True)
        old = b'[mcp_servers.other]\ncommand = "other-client"\nargs=["--flag"]\n'
        self.config.write_bytes(old)
        r = self.connect()
        self.assertTrue(r["backup_created"])
        self.assertEqual(self.parse()["mcp_servers"]["other"]["command"], "other-client")
        self.assertEqual(len(list(self.backup.glob("*.toml"))), 1)
        self.assertEqual(next(self.backup.glob("*.toml")).read_bytes(), old)

    def test_refuses_conflicting_memory(self):
        self.config.parent.mkdir(parents=True, exist_ok=True)
        old = b'[mcp_servers.superlocalmemory]\ncommand="unknown"\nargs=["mcp"]\n'
        self.config.write_bytes(old)
        with self.assertRaises(ValueError):
            self.connect()
        self.assertEqual(self.config.read_bytes(), old)

    def test_refuses_malformed_config(self):
        self.config.parent.mkdir(parents=True, exist_ok=True)
        old = b'[broken\n'
        self.config.write_bytes(old)
        with self.assertRaises(tomllib.TOMLDecodeError):
            self.connect()
        self.assertEqual(self.config.read_bytes(), old)

    def test_refuses_other_path(self):
        with self.assertRaises(ValueError):
            module.configure(self.root / "something.toml", self.exe, self.store, self.backup)

    def test_refuses_missing_binary(self):
        with self.assertRaises(ValueError):
            module.configure(self.config, self.root / "missing.exe", self.store, self.backup)


if __name__ == "__main__":
    unittest.main()
