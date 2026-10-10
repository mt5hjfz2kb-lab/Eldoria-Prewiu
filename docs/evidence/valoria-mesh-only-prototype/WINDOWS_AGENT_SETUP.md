# Eldoria Windows BlenderMCP controlled setup
Isolated experimental environment. No changes to Unity, main, renderer or public build.

- Prerequisites: Blender desktop installed and `uvx` available (uv Python tooling).
- Dry-run first: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/art-rd/windows-blender-mcp-setup.ps1`
- On the **owner PC, in its own Windows session**, after reading the commands: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/art-rd/windows-blender-mcp-setup.ps1 -Install -StartBlender`
- Then manually enable the Blender addon, click *Start MCP Server* and attach a compatible AI client. The GitHub runner agent is not itself an authenticated interactive MCP client.
- Local-only socket. Never open remote inbound ports or put credentials into source control.
- Installation is reversible: disable/remove Blender add-on. Does not alter Eldoria source files.
- A successful installation is **not** a live-agent connection or visual quality gate.
- The self-hosted Windows Unity runner is a separate shared resource reserved by ongoing work; do not remotely dispatch this installer or occupy that runner without coordinating its release.
- Required next proof: client-to-Blender request/response + scene modification + screenshot + iterative visual comparison + verified export to Unity.
