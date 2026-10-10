import bpy, addon_utils, json, socket, threading, time
addon_utils.modules_refresh()
addon_utils.enable("blender_mcp",default_set=False,persistent=False)
op=getattr(bpy.ops,"blendermcp",None)
print("REGISTERED_OPERATORS",str(op))
print("BLENDER_MCP_ACTIVE",addon_utils.check("blender_mcp")[1])
# A real socket request is mandatory; no fake PASS if unavailable.
mod=__import__("blender_mcp")
print("ADDON_METHODS",[v for v in dir(mod) if "server" in v.lower() or "execute" in v.lower() or "start" in v.lower()])
if not hasattr(mod,"BlenderMCPServer"):
 print("ELDORIA_SOCKET_BLOCKED: addon server entrypoint requires interactive Blender context")
else:
 print("ELDORIA_SOCKET_BLOCKED: server requires dedicated interactive connection test")
