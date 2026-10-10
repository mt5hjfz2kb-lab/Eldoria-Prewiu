import bpy, addon_utils, json
addon_utils.modules_refresh()
addon_utils.enable("blender_mcp",default_set=False,persistent=False)
import blender_mcp
server=blender_mcp.BlenderMCPServer(host="127.0.0.1",port=9876)
report=server.execute_command({"type":"get_scene_info","params":{}})
print("SCENE_INFO",json.dumps(report,default=str)[:3000])
if report.get("status") != "success":raise RuntimeError("MCP scene info command failed")
print("ELDORIA_SOCKET_REQUEST_PASS internal protocol handler verified; TCP session NOT tested")
