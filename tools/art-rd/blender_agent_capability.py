"""Conservative agent capability gate. No API keys, paid inference, or edits to main."""
import json, shutil, subprocess
from pathlib import Path
def command(args):
    try:
        p=subprocess.run(args,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True,timeout=45)
        return {"exit_code":p.returncode,"sample":p.stdout[:1300]}
    except Exception as ex:
        return {"error":str(ex)}
blender=shutil.which("blender")
uvx=shutil.which("uvx")
report={
    "experiment":"valoria-agent-assisted-blender",
    "mode":"unpaid independent preflight",
    "blender_available":bool(blender),
    "uvx_available":bool(uvx),
    "blender_version":command([blender,"--version"]) if blender else None,
    "mcp_cli_downloaded":Path("/tmp/mcp-cli.txt").exists(),
    "mcp_cli_log":Path("/tmp/mcp-cli.txt").read_text(errors="replace")[:1600] if Path("/tmp/mcp-cli.txt").exists() else None,
    "addon_install_log":Path("/tmp/mcp-addon.txt").read_text(errors="replace")[:1400] if Path("/tmp/mcp-addon.txt").exists() else None,
    "agent_authenticated":False,
    "agent_blender_live_connection_tested":False,
    "visual_improvement_approved":False,
    "unity_import_verified":False,
    "note":"A package installation is not a connected agent. A signed-in CLI + interactive Blender MCP session and iterative scene evaluation remain separate mandatory gates."
}
p=Path("/tmp/eldoria-agent-probe.json")
p.write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
if not blender: raise SystemExit("Blender executable missing")
