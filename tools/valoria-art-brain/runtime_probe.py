#!/usr/bin/env python3
"""Read-only Eldoria art-brain runtime audit. Never installs models or modifies scenes."""
import json,os,pathlib,shutil,subprocess,urllib.request,urllib.error
BASE=pathlib.Path(__file__).resolve().parents[2]
def exe(cmd):
    p=shutil.which(cmd)
    return {"available":bool(p),"path":p}
def ollama():
    endpoint=os.getenv("ELDORIA_OLLAMA_URL","http://127.0.0.1:11434")
    if not (endpoint.startswith("http://127.0.0.1:") or endpoint.startswith("http://localhost:")):
        return {"status":"not_probed_nonlocal_endpoint","reason":"Remote services require explicit access"}
    try:
        with urllib.request.urlopen(endpoint+"/api/tags",timeout=4) as response:
            data=json.load(response)
        names=[x.get("name","") for x in data.get("models",[])]
        return {"status":"available","models":names,"vision_model_present":any("qwen" in n.lower() and ("vl" in n.lower() or "vision" in n.lower()) for n in names)}
    except (OSError, ValueError) as e:
        return {"status":"unavailable","detail":str(e)}
def main():
    bridge=BASE/"tools/valoria-art-brain/bridge.py"
    result={"blender":exe("blender"),"ollama_cli":exe("ollama"),"ollama_local":ollama(),
            "unity_editor":exe("Unity") if os.name!="nt" else exe("Unity.exe"),
            "bridge_source_exists":bridge.exists(),
            "hero_model_exists":(BASE/"art-source/valoria/production/first-hero-gate-v1/ValoriaHeroGate.blend").exists(),
            "mcp_report_exists":(BASE/"art-source/valoria/production/first-hero-gate-v1/mcp-report.json").exists()}
    if result["mcp_report_exists"]:
        report=json.loads((BASE/"art-source/valoria/production/first-hero-gate-v1/mcp-report.json").read_text())
        result["historical_mcp_report"]=report
    result["conclusion"]="Capabilities checked, not automatically started. Existing BlenderMCP proof is historical, Qwen requires local running service; Unity via owned Windows runner, not local Linux."
    print(json.dumps(result,indent=2))
if __name__=="__main__":main()
