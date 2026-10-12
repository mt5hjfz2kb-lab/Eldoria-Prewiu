#!/usr/bin/env python3
"""Passive, zero-cost environment readiness check for Eldoria's local 3D artist.

Does not install packages, invoke inference, modify Blender files, or write files.
Run: python tools/art-direction/verify_local_artist_environment.py
"""
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
from urllib.request import urlopen
from urllib.error import URLError

ROOT = Path(__file__).resolve().parents[2]
MODEL = os.environ.get("ELDORIA_QWEN_MODEL", "qwen3-vl:4b-instruct")
OLLAMA_URL = os.environ.get("OLLAMA_HOST", "http://127.0.0.1:11434").rstrip("/")
PROMPT = ROOT / "tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md"
GENERATOR = ROOT / "tools/art-direction/build_cliff_talus_transition_v1.py"


def probe(command, timeout=8):
    try:
        completed = subprocess.run(command, text=True, capture_output=True,
                                   timeout=timeout, check=False)
        return {"ok": completed.returncode == 0,
                "returncode": completed.returncode,
                "output": (completed.stdout or completed.stderr).strip()[:500]}
    except (OSError, subprocess.TimeoutExpired) as exc:
        return {"ok": False, "error": str(exc)}


def ollama_models():
    try:
        with urlopen(OLLAMA_URL + "/api/tags", timeout=5) as response:
            payload = json.load(response)
        names = [item.get("name", "") for item in payload.get("models", [])]
        return {"ok": True, "models": names, "desired_model_present": MODEL in names}
    except (OSError, ValueError, URLError) as exc:
        return {"ok": False, "error": str(exc)}


def main():
    result = {
        "platform": sys.platform,
        "python": sys.version.split()[0],
        "repository": str(ROOT),
        "prompt": {"present": PROMPT.is_file()},
        "generator": {"present": GENERATOR.is_file()},
        "git": probe(["git", "--version"]) if shutil.which("git") else {"ok": False},
        "blender": probe(["blender", "--version"]) if shutil.which("blender") else {"ok": False, "note": "Not on PATH; Windows installation may still exist"},
        "ollama_cli": probe(["ollama", "--version"]) if shutil.which("ollama") else {"ok": False},
        "ollama_api": ollama_models(),
        "blender_mcp": {"verified": False, "note": "MCP connection and live Blender edit need a separate explicit, non-destructive integration test"},
        "unity": {"verified": False, "note": "Unity editor, Windows runner and on-device perf not checked by passive probe"},
    }
    result["ready_for_active_trial"] = all((
        result["prompt"]["present"], result["generator"]["present"],
        result["blender"]["ok"], result["ollama_api"]["ok"],
        result["ollama_api"].get("desired_model_present", False),
    ))
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0 if result["ready_for_active_trial"] else 2


if __name__ == "__main__":
    raise SystemExit(main())
