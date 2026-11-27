"""Windows compatible actual Unity MCP stdio handshake; no arbitrary model command."""
import json,os,queue,subprocess,sys,threading,time,pathlib,shutil
P=pathlib.Path;out=P(sys.argv[1]);project=str(P(sys.argv[2]).resolve());exe=shutil.which("unity") or shutil.which("unity.exe")
if not exe:raise RuntimeError("Unity CLI missing")
# Discover installed CLI syntax; this Windows CLI rejects --cli-tools.
help_result=subprocess.run([exe,"--no-banner","mcp","--help"],capture_output=True,text=True,encoding="utf-8",errors="replace",timeout=20,cwd=project)
print("INSTALLED_MCP_HELP",help_result.stdout[:6000],help_result.stderr[:1000],flush=True)
cmd=[exe,"--no-banner","mcp"]
if "--cli-tools" in help_result.stdout:cmd.append("--cli-tools")
p=subprocess.Popen(cmd,stdin=subprocess.PIPE,stdout=subprocess.PIPE,stderr=subprocess.PIPE,text=True,encoding="utf-8",errors="replace",bufsize=1,cwd=project)
q=queue.Queue()
def reader():
 for line in p.stdout:
  try:q.put(json.loads(line))
  except Exception:pass
threading.Thread(target=reader,daemon=True).start()
stderr_lines=[]
def err_reader():
 for line in p.stderr:
  if len(stderr_lines)<40: stderr_lines.append(line.strip()[:500])
threading.Thread(target=err_reader,daemon=True).start()
def send(o):p.stdin.write(json.dumps(o,separators=(",",":"))+"\n");p.stdin.flush()
def call(i,method,params):
 send({"jsonrpc":"2.0","id":i,"method":method,"params":params});end=time.monotonic()+30
 while time.monotonic()<end:
  try:r=q.get(timeout=1)
  except queue.Empty:
   if p.poll() is not None:raise RuntimeError("MCP exited")
   continue
  if r.get("id")==i:
   if "result" not in r:raise RuntimeError("MCP protocol call failed")
   return r["result"]
 raise TimeoutError("MCP response timeout")
try:
 init=call(1,"initialize",{"protocolVersion":"2025-03-26","capabilities":{},"clientInfo":{"name":"eldoria-pr198-native","version":"1.0"}})
 send({"jsonrpc":"2.0","method":"notifications/initialized"});tools=call(2,"tools/list",{})["tools"];names=[t["name"] for t in tools]
 out.write_text(json.dumps({"protocol":init.get("protocolVersion"),"tool_names":names,"status_invoked":False,"editor_status_success":False},indent=2),encoding="utf-8")
 print("MCP_TOOLS_DISCOVERED",json.dumps(tools)[:18000],flush=True)
 status_tool=next((name for name in ("unity-cli-status","unity-status","unity_status","status","get_editor_state","get-editor-state") if name in names),None)
 if not status_tool:raise RuntimeError("MCP connected; known read-only editor status tool not listed")
 status=call(3,"tools/call",{"name":status_tool,"arguments":{}})
 excerpts=[str(x.get("text",""))[:1200] for x in status.get("content",[])]
 connected=not status.get("isError",False)
 proof={"protocol":init.get("protocolVersion"),"tool_names":names,"status_invoked":True,"status_tool":status_tool,"editor_status_success":connected,"status_excerpt":excerpts,"native_action_transport":"separate trusted Unity CLI script eval","project_isolated":True}
 out.write_text(json.dumps(proof,indent=2),encoding="utf-8");print("PR198_UNITY_MCP_PROOF",json.dumps(proof),flush=True)
 if not connected:raise RuntimeError("Unity MCP did not confirm an editor")
except Exception as exc:
 print("UNITY_MCP_PROBE_FAILURE",type(exc).__name__,str(exc),flush=True)
 print("MCP_STDERR",json.dumps(stderr_lines),flush=True)
 raise
finally:
 try:p.stdin.close()
 except Exception:pass
 if p.poll() is None:
  p.terminate()
  try:p.wait(timeout=5)
  except subprocess.TimeoutExpired:p.kill()
