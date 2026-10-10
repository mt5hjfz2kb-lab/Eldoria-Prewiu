#!/usr/bin/env python3
"""Zero-cost regression tests for Eldoria's GLB structural gate."""
import importlib.util
import json
import pathlib
import struct
import tempfile
import unittest

HERE = pathlib.Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location("audit_glb", HERE / "audit_glb.py")
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


def pack_glb(payload, binary=b"\x00" * 36):
    document = json.dumps(payload, separators=(",", ":")).encode("utf-8")
    document += b" " * (-len(document) % 4)
    binary += b"\x00" * (-len(binary) % 4)
    chunks = struct.pack("<I4s", len(document), b"JSON") + document
    chunks += struct.pack("<I4s", len(binary), b"BIN\x00") + binary
    return struct.pack("<4sII", b"glTF", 2, 12 + len(chunks)) + chunks


class TestAuditGlb(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.path = pathlib.Path(self.temp.name) / "asset.glb"
        self.payload = {
            "asset": {"version": "2.0"},
            "buffers": [{"byteLength": 36}],
            "accessors": [{"componentType": 5126, "type": "VEC3", "count": 3}],
            "meshes": [{"primitives": [{"attributes": {"POSITION": 0}}]}],
        }

    def write(self, payload=None, binary=None):
        self.path.write_bytes(pack_glb(payload or self.payload, b"\x00" * 36 if binary is None else binary))
        return module.audit(self.path)

    def test_valid_triangle_model(self):
        report = self.write()
        self.assertEqual(report["errors"], [])
        self.assertEqual(report["triangles_estimated"], 1)
        self.assertEqual(report["meshes"], 1)

    def test_truncated_glb_fails(self):
        self.path.write_bytes(b"glTF")
        self.assertIn("Truncated GLB header", module.audit(self.path)["errors"])

    def test_declared_length_mismatch_fails(self):
        self.path.write_bytes(pack_glb(self.payload)[:-1])
        self.assertTrue(module.audit(self.path)["errors"])

    def test_absent_accessor_fails(self):
        self.payload["meshes"][0]["primitives"][0]["attributes"]["POSITION"] = 99
        self.assertTrue(self.write()["errors"])

    def test_buffer_overflow_fails(self):
        self.payload["buffers"][0]["byteLength"] = 1024
        self.assertTrue(self.write()["errors"])

    def test_missing_uv_is_warning_not_failure(self):
        report = self.write()
        self.assertEqual(report["errors"], [])
        self.assertTrue(any("UV0" in warning for warning in report["warnings"]))


if __name__ == "__main__":
    unittest.main()
