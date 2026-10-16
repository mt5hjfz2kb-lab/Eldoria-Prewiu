"""Optional existing local vision peer review. No downloads, authoring or paid API."""
import pathlib,os,json,base64,urllib.request,time,subprocess,hashlib
root=pathlib.Path.cwd();out=root/"docs/evidence/valoria-scene-breakthrough-20261016";process=None
def request(path,data=None,timeout=20):
 req=urllib.request.Request("http://127.0.0.1:11434"+path,data=json.dumps(data).encode() if data is not None else None,headers={"Content-Type":"application/json"})
 with urllib.request.urlopen(req,timeout=timeout) as r:return json.load(r)
try:
 try:request("/api/version",timeout=3)
 except Exception:
  candidates=[pathlib.Path(os.environ.get("LOCALAPPDATA",""))/"Programs/Ollama/ollama.exe",pathlib.Path(os.environ.get("LOCALAPPDATA",""))/"Ollama/ollama.exe",pathlib.Path(os.environ.get("ProgramFiles","C:/Program Files"))/"Ollama/ollama.exe"]
  exe=next((p for p in candidates if p.exists()),None)
  if exe is None:raise RuntimeError("Existing Ollama executable not found at registered conventional locations")
  process=subprocess.Popen([str(exe),"serve"],stdout=(out/"local-review-server.log").open("w"),stderr=subprocess.STDOUT)
  ready=False
  for i in range(30):
   try:request("/api/version",timeout=2);ready=True;break
   except Exception:time.sleep(1)
  if not ready:raise RuntimeError("Own ephemeral server did not become ready")
 models=request("/api/tags").get("models",[]);model=next((m["name"] for m in models if "qwen" in m["name"].lower() and "vl" in m["name"].lower()),None)
 if model is None:raise RuntimeError("No already installed Qwen vision model; downloading forbidden")
 memory_file=out/("review-memory-"+os.environ.get("GITHUB_RUN_ID","local")+".txt")
 subprocess.run(["node","tools/art-direction/learning-evidence/cycle-hooks.mjs","prepare","--target","bastion1","--out",str(memory_file),"--max-chars","1200"],check=True)
 history=memory_file.read_text()
 prompt="""You are a skeptical visual reviewer, not the author. Evaluate the ACTUAL attached Unity comparison. Columns left-to-right: approved artistic reference, frozen old scene after explicitly documented frame normalization, experimental 3D, experimental 3D with native distant landscape bake. Rows are main, landscape, portrait, pan, zoom. Reference is the same static illustration repeated for context, not fabricated camera views. Score whole-frame composition/proportion, architecture, geology/landscape, material coherence, atmosphere/depth, mobile read, each 0-10. Do not score navigation/performance from pictures. Technical SUCCESS, changed pixels, actual mesh edits and previous claims never establish art quality. Do not invent hidden detail or assert weights learned. Experimental acceptance needs about 6/10 and clear ~1 point improvement over baseline. A result under that is NO VISUAL WIN. Identify at most three visible root failures and any real strengths; explicitly state confidence/limits. Return concise JSON with independent column scores, verdict and visible reasons. Do not copy any claimed scores from prior history; none are provided."""
 memory={"verified_lessons":["CI success and geometry edits are not art acceptance","whole scene matters more than microasset changes","never infer GPU/device performance from a screenshot","judge only current native captures against the exact reference"]}
 payload={"model":model,"stream":False,"think":False,"messages":[{"role":"system","content":"Independent screenshot critique only. No tool calls or model authoring."},{"role":"user","content":prompt+"\nVerified evaluation constraints: "+json.dumps(memory)+"\nExisting verified memory (advice, never a score): "+history,"images":[base64.b64encode((out/"comparison-four-columns.jpg").read_bytes()).decode()]}],"options":{"temperature":0.1,"num_ctx":8192,"num_predict":1400,"seed":72},"keep_alive":0}
 response=request("/api/chat",payload,timeout=480)
 (out/"independent-local-vision-review.json").write_text(json.dumps({"model":model,"scope":"advisory peer critique, not professional calibration or formal approval","prompt_sha256":hashlib.sha256(payload["messages"][1]["content"].encode()).hexdigest(),"memory_brief_sha256":hashlib.sha256(history.encode()).hexdigest(),"comparison_sha256":hashlib.sha256((out/"comparison-four-columns.jpg").read_bytes()).hexdigest(),"response":response},indent=2))
 print("LOCAL_INDEPENDENT_CRITIQUE_RECORDED",model)
except Exception as ex:
 (out/"independent-local-vision-review.json").write_text(json.dumps({"status":"UNAVAILABLE","reason":str(ex),"no_models_downloaded":True,"no_paid_api":True},indent=2))
 print("OPTIONAL_LOCAL_CRITIQUE_UNAVAILABLE",str(ex))
finally:
 if process is not None:
  process.terminate()
  try:process.wait(timeout=10)
  except subprocess.TimeoutExpired:process.kill()
