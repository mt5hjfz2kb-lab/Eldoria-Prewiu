"""Verify the Blender MCP addon loads inside the actual Blender runtime.
This is not a connected agent, socket test, or artistic approval.
"""
import bpy, addon_utils, json
name="blender_mcp"
addon_utils.modules_refresh()
try:
    addon_utils.enable(name, default_set=False, persistent=False)
except Exception as exc:
    print("ELDORIA_MCP_ADDON_LOAD_FAIL", type(exc).__name__, str(exc))
    raise
enabled=addon_utils.check(name)[1]
if not enabled:
    raise RuntimeError("Blender MCP did not enable")
print("ELDORIA_MCP_ADDON_LOAD_PASS", json.dumps({"blender":bpy.app.version_string,"module":name,"enabled":enabled}))
