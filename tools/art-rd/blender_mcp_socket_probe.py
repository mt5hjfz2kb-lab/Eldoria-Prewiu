import bpy, addon_utils, json
addon_utils.modules_refresh()
addon_utils.enable("blender_mcp",default_set=False,persistent=False)
import blender_mcp
server=blender_mcp.BlenderMCPServer(host="127.0.0.1",port=9876)
before=server.execute_command({"type":"get_scene_info","params":{}})
assert before["status"] == "success"
assert not any(o["name"]=="Eldoria MCP verified sculpture" for o in before["result"]["objects"])
response=server.execute_command({"type":"execute_code","params":{"code":"bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=2.0); bpy.context.object.name='Eldoria MCP verified sculpture'"}})
assert response["status"]=="success",response
after=server.execute_command({"type":"get_scene_info","params":{}})
assert after["status"]=="success" and after["result"]["object_count"]==before["result"]["object_count"]+1
assert any(o["name"]=="Eldoria MCP verified sculpture" for o in after["result"]["objects"])
print("ELDORIA_SOCKET_REQUEST_PASS real addon protocol inspection + safe scene modification verified; TCP network transport NOT tested")
