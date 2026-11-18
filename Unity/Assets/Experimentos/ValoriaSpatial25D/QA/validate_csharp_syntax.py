#!/usr/bin/env python3
"""Tree-sitter syntax check: NOT Unity compilation and NOT shader compilation."""
from pathlib import Path
from tree_sitter import Language, Parser
import tree_sitter_c_sharp

ROOT = Path(__file__).resolve().parents[1]
SOURCES = [
    ROOT / "SpatialPilotBootstrap.cs",
    ROOT / "SpatialPilotWorker.cs",
    ROOT / "SpatialPilotActorArt.cs",
    ROOT / "SpatialTerrainForeground.cs",
    ROOT / "Editor/SpatialPilotNativeEvidence.cs",
]
parser = Parser(Language(tree_sitter_c_sharp.language()))
for src in SOURCES:
    assert src.is_file(), f"Missing C# source: {src}"
    root = parser.parse(src.read_bytes()).root_node
    assert not root.has_error, f"C# syntax invalid: {src}"
print(f"CSHARP_SYNTAX_PASS={len(SOURCES)}; UNITY_COMPILATION=NOT_EXECUTED")
