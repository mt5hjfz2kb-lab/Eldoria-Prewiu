"""True loopback TCP -> BlenderMCP queue -> bpy main-thread protocol proof.
Blender background harness uses the addon's actual _server_loop and queue drain;
it is NOT a persistent GUI agent service.
"""
import bpy, addon_utils, json, socket, threading, time
addon_utils.modules_refresh()
addon_utils.enable("blender_mcp",default_set=False,persistent=False)
import blender_mcp
srv=blender_mcp.BlenderMCPServer(host="127.0.0.1",port=0)
srv.socket=socket.socket(socket.AF_INET,socket.SOCK_STREAM)
srv.socket.bind(("127.0.0.1",0))
port=srv.socket.getsockname()[1]
srv.socket.listen(1)
srv.running=True
srv.server_thread=threading.Thread(target=srv._server_loop,daemon=True)
srv.server_thread.start()
reply=[]
error=[]
def client():
 try:
  sock=socket.create_connection(("127.0.0.1",port),timeout=5)
  sock.settimeout(10)
  command={"type":"execute_code","params":{"code":"bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2); bpy.context.object.name='Eldoria TCP scene edit'"}}
  sock.sendall(json.dumps(command).encode("utf-8"))
  buf=b""
  while True:
   chunk=sock.recv(4096)
   if not chunk:break
   buf+=chunk
   try:
    reply.append(json.loads(buf.decode("utf-8")))
    break
   except json.JSONDecodeError:
    pass
  sock.close()
 except Exception as exc:error.append(str(exc))
th=threading.Thread(target=client,daemon=True);th.start()
deadline=time.monotonic()+13
while th.is_alive() and time.monotonic()<deadline:
 srv._drain_command_queue()
 time.sleep(.05)
th.join(timeout=.5)
assert not error,error
assert reply, "TCP client did not receive response"
assert reply[0].get("status")=="success",reply
assert bpy.data.objects.get("Eldoria TCP scene edit") is not None
print("ELDORIA_TCP_SCENE_EDIT_PASS",json.dumps({"port_ephemeral":True,"tcp_response":reply[0].get("status"),"edit_verified":True}))
srv.stop()
