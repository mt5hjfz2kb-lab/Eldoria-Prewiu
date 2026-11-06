#!/usr/bin/env python3
"""Safely add a private SuperLocalMemory MCP stdio server to Codex user config.

This script only modifies the user's Codex config TOML, never the game's
repository, and backs up the exact previous bytes.  It does not read or
modify Codex OAuth credentials or run the Codex agent.
"""
from __future__ import annotations

import argparse
import json
import os
from pathlib import Path
import shutil
import sys
import tempfile
import tomllib
from datetime import datetime, timezone


BLOCK_NAME = "[mcp_servers.superlocalmemory]"
MCP_ENV = {
    "SLM_SKIP_LAYA": "1",
    "SLM_MCP_PROFILE": "core",
    "SLM_AGENT_ID": "codex",
}


def _quoted(value: str) -> str:
    return json.dumps(str(value), ensure_ascii=True)


def _slm_block(executable: Path, store: Path) -> str:
    env = {**MCP_ENV, "SLM_HOME": str(store)}
    env_spec = ", ".join(
        f"{key} = {_quoted(value)}" for key, value in sorted(env.items())
    )
    return (
        f"\n{BLOCK_NAME}\n"
        f"command = {_quoted(str(executable))}\n"
        'args = ["mcp"]\n'
        f"env = {{ {env_spec} }}\n"
    )


def _parse(content: str) -> dict:
    return tomllib.loads(content) if content.strip() else {}


def configure(config: Path, executable: Path, store: Path, backup_dir: Path) -> dict:
    if not executable.is_file():
        raise ValueError("isolated slm.exe does not exist")
    if not store.is_dir():
        raise ValueError("SLM_HOME store is absent")
    if not str(config).casefold().endswith("/.codex/config.toml") and not (
        os.name == "nt" and str(config).replace("\\", "/").casefold().endswith("/.codex/config.toml")
    ):
        raise ValueError("config must be a user Codex config.toml")
    if config.is_symlink() or (config.exists() and not config.is_file()):
        raise ValueError("refusing non-regular/symlink Codex config")
    raw = config.read_bytes() if config.exists() else b""
    try:
        content = raw.decode("utf-8-sig")
    except UnicodeDecodeError as exc:
        raise ValueError("Codex config is not valid UTF-8") from exc
    before = _parse(content)
    existing = before.get("mcp_servers", {}).get("superlocalmemory")
    expected = {"command": str(executable), "args": ["mcp"],
                "env": {**MCP_ENV, "SLM_HOME": str(store)}}
    if existing is not None:
        if existing == expected:
            return {"result": "unchanged", "other_entries_preserved": True}
        raise ValueError("different superlocalmemory entry exists: manual reconciliation required")
    suffix = _slm_block(executable, store)
    after = _parse(content + suffix)
    if after.get("mcp_servers", {}).get("superlocalmemory") != expected:
        raise AssertionError("TOML connection verification failed before writing")
    other_before = dict(before.get("mcp_servers", {}))
    other_after = dict(after.get("mcp_servers", {}))
    other_after.pop("superlocalmemory", None)
    if other_after != other_before:
        raise AssertionError("other MCP servers changed")
    config.parent.mkdir(parents=True, exist_ok=True)
    backup_dir.mkdir(parents=True, exist_ok=True)
    if raw:
        suffix_name = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%S%fZ")
        backup = backup_dir / f"codex-config-before-slm-{suffix_name}.toml"
        backup.write_bytes(raw)
    else:
        backup = None
    fd, temp = tempfile.mkstemp(prefix=".config-slm-", suffix=".toml", dir=config.parent)
    try:
        with os.fdopen(fd, "wb") as stream:
            new_bytes = (content.rstrip("\r\n") + "\n" + suffix.lstrip("\n")).encode("utf-8")
            stream.write(new_bytes)
            stream.flush()
            os.fsync(stream.fileno())
        os.replace(temp, config)
    finally:
        if os.path.exists(temp):
            os.unlink(temp)
    verified = _parse(config.read_text(encoding="utf-8-sig"))
    if verified.get("mcp_servers", {}).get("superlocalmemory") != expected:
        raise AssertionError("persisted TOML config verification failed")
    return {
        "result": "configured",
        "backup_created": bool(backup),
        "other_entries_preserved": True,
        "mcp_transport": "stdio",
        "profile": "core",
    }


def main(argv: list[str]) -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--config", required=True, type=Path)
    ap.add_argument("--slm-executable", required=True, type=Path)
    ap.add_argument("--slm-home", required=True, type=Path)
    ap.add_argument("--backup-dir", required=True, type=Path)
    args = ap.parse_args(argv)
    result = configure(args.config, args.slm_executable, args.slm_home, args.backup_dir)
    print("SLM_CODEX_CONFIG_PASS " + json.dumps(result, sort_keys=True))
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main(sys.argv[1:]))
    except Exception as e:
        print("SLM_CODEX_CONFIG_FAIL: " + type(e).__name__ + ": " + str(e)[:230], file=sys.stderr)
        sys.exit(1)
