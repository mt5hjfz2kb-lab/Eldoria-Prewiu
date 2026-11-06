#!/usr/bin/env python3
"""MCP stdio smoke test in the Windows owner's isolated SLM installation.
Does not contact public endpoints, mutate Eldoria or expose memory/credentials.
Uses the tested official Python MCP client rather than hand-built HTTP sessions.
"""
import asyncio
import os
import pathlib
import sys

from mcp import Client, StdioServerParameters


async def verify():
    base = pathlib.Path(os.environ.get("LOCALAPPDATA", ""))
    slm = base / "EldoriaSLM" / "venv-4.2.12" / "Scripts" / "slm.exe"
    if not slm.is_file():
        raise RuntimeError("isolated slm executable not found")
    required_home = base / "EldoriaSLM" / "data"
    if os.environ.get("SLM_HOME", "").casefold() != str(required_home).casefold():
        raise RuntimeError("unexpected memory home; refusing other profiles")
    params = StdioServerParameters(command=str(slm), args=["mcp"], env=dict(os.environ))
    async with Client(params) as client:
        tools = await client.list_tools()
        names = {t.name for t in tools.tools}
        if "recall" not in names:
            raise RuntimeError("MCP core profile has no recall tool")
        for q, expected in [
            ("Unity main runtime v0220 prototype", "Unity/"),
            ("Qwen Ollama art discontinued October 2026", "Qwen"),
        ]:
            response = await client.call_tool("recall", {"query": q, "limit": 8})
            if bool(response.is_error):
                raise RuntimeError("recall tool returned error")
            text = "\n".join(getattr(item, "text", "") for item in response.content)
            if expected.casefold() not in text.casefold():
                raise RuntimeError("persisted source-reviewed memory not found via MCP")
    print("SLM_MCP_STDIO_PASS: isolated independent client and two persisted recalls")


if __name__ == "__main__":
    try:
        asyncio.run(asyncio.wait_for(verify(), timeout=90))
    except Exception as exc:
        print("SLM_MCP_STDIO_FAIL: " + type(exc).__name__ + ": " + str(exc)[:350], file=sys.stderr)
        sys.exit(1)
