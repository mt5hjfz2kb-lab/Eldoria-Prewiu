import bpy, addon_utils, inspect, json
addon_utils.modules_refresh()
addon_utils.enable("blender_mcp",default_set=False,persistent=False)
import blender_mcp
cl=blender_mcp.BlenderMCPServer
print("ELDORIA_SERVER_METHODS",json.dumps([x for x in dir(cl) if not x.startswith("_")]))
for n in ["start","stop","run","execute_command","_handle_client","_process_commands","handle_client"]:
 method=getattr(cl,n,None)
 if method:
  try: print("ELDORIA_METHOD",n,str(inspect.signature(method)),inspect.getsource(method)[:2500])
  except Exception as exc:print("ELDORIA_INSPECT_ERROR",n,str(exc))
print("ELDORIA_CLASS_SOURCE",inspect.getsource(cl)[:10000])
print("ELDORIA_INTROSPECTION_PASS")
