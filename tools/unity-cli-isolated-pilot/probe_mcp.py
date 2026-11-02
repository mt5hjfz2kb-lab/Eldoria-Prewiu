#!/usr/bin/env python3
"""Real MCP stdio handshake with Unity CLI in an isolated environment, no Unity Editor."""
import json, os, select, subprocess, sys, time
from pathlib import Path

def run(path):
    p=subprocess.Popen(
        ["unity","--no-banner","mcp","--cli-tools"],
        stdin=subprocess.PIPE,stdout=subprocess.PIPE,stderr=subprocess.PIPE,
        env=os.environ.copy(),bufsize=0)
    buf=b""; events=[]
    def send(message):
        p.stdin.write((json.dumps(message,separators=(',',':'))+"\n").encode())
        p.stdin.flush()
    def receive(expected, deadline):
        nonlocal buf
        while time.monotonic()<deadline:
            if p.poll() is not None: raise RuntimeError("MCP process exited "+str(p.returncode))
            wait=max(.05,min(2,deadline-time.monotonic()))
            ready,_,_=select.select([p.stdout],[],[],wait)
            if not ready:continue
            data=os.read(p.stdout.fileno(),65536)
            if not data:raise RuntimeError("Unity CLI MCP stdout closed")
            buf+=data
            while b"\n" in buf:
                line,buf=buf.split(b"\n",1)
                try:message=json.loads(line)
                except Exception:
                    events.append({"ignored_non_json_prefix":line.decode(errors='replace')[:80]})
                    continue
                if message.get("id")==expected:return message
        raise TimeoutError("Unity CLI MCP did not respond to JSON-RPC id "+str(expected))
    try:
        send({"jsonrpc":"2.0","id":1,"method":"initialize",
              "params":{"protocolVersion":"2025-03-26","capabilities":{},
                        "clientInfo":{"name":"eldoria-issue147-safe-probe","version":"1.0"}}})
        first=receive(1,time.monotonic()+20)
        if "result" not in first:raise RuntimeError("MCP initialize error: "+str(first)[:250])
        send({"jsonrpc":"2.0","method":"notifications/initialized"})
        send({"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}})
        second=receive(2,time.monotonic()+20)
        if "result" not in second:raise RuntimeError("MCP tools/list error: "+str(second)[:250])
        tools=second["result"].get("tools",[])
        names=[x.get("name","") for x in tools]
        if not names:raise RuntimeError("No MCP CLI read-only tools advertised")
        if "unity-cli-status" not in names:
            raise RuntimeError("Expected read-only CLI status tool not provided")
        send({"jsonrpc":"2.0","id":3,"method":"tools/call",
              "params":{"name":"unity-cli-status","arguments":{}}})
        third=receive(3,time.monotonic()+20)
        if "result" not in third:
            raise RuntimeError("MCP tools/call failed protocol: "+str(third)[:300])
        short=[str(item.get("text",""))[:600] for item in third["result"].get("content",[])]
        tool_is_error=bool(third["result"].get("isError"))
        expected_no_editor=tool_is_error and any(
            "No Unity Editor instances found with reachable Pipeline servers" in msg for msg in short)
        if tool_is_error and not expected_no_editor:
            raise RuntimeError("Unexpected MCP tool failure: "+str(third)[:300])
        result={"mcp_stdio_handshake_success":True,
                "protocol_reported":first["result"].get("protocolVersion"),
                "mcp_tool_count":len(names),"mcp_tool_names":names,
                "editor_connected":False,"tools_invoked":True,
                "read_only_tool_invoked":"unity-cli-status",
                "read_only_tool_success":not tool_is_error,
                "expected_no_editor_failure":expected_no_editor,
                "read_only_tool_result_excerpt":short,
                "non_json_lines":events}
        Path(path).write_text(json.dumps(result,indent=2))
        print(json.dumps({"protocol_success":True,"tool_count":len(names),
                          "read_only_tool_success":not tool_is_error,"no_editor":expected_no_editor}))
    finally:
        try:p.stdin.close()
        except Exception:pass
        if p.poll() is None:
            p.terminate()
            try:p.wait(timeout=3)
            except subprocess.TimeoutExpired:
                p.kill();p.wait(timeout=2)

if __name__=="__main__":
    run(sys.argv[1])
