#!/usr/bin/env bash
set -euo pipefail
# Ephemeral, no-paid-services Ollama + small Qwen vision startup for Linux CI.
# Optional vision adviser; never automatically approves art.
export OLLAMA_HOST=127.0.0.1:11434
export OLLAMA_MODELS="${RUNNER_TEMP:-/tmp}/eldoria-ollama-models"
export OLLAMA_NUM_PARALLEL=1
export OLLAMA_MAX_LOADED_MODELS=1
export OLLAMA_KEEP_ALIVE=0
MODEL="${ELDORIA_VISION_MODEL:-qwen3-vl:2b}"
mkdir -p "$OLLAMA_MODELS"
if ! command -v ollama >/dev/null 2>&1; then
  curl -fsSL https://ollama.com/install.sh -o "${RUNNER_TEMP:-/tmp}/ollama-install.sh"
  bash "${RUNNER_TEMP:-/tmp}/ollama-install.sh"
fi
ollama serve > "${RUNNER_TEMP:-/tmp}/ollama-server.log" 2>&1 &
pid=$!
trap 'kill "$pid" 2>/dev/null || true' EXIT
ready=0
for i in $(seq 1 40); do
  if curl -fsS --max-time 2 http://127.0.0.1:11434/api/tags > /dev/null; then ready=1; break; fi
  sleep 2
done
if [ "$ready" -ne 1 ]; then echo "Ollama startup failed"; tail -80 "${RUNNER_TEMP:-/tmp}/ollama-server.log"; exit 1; fi
ollama pull "$MODEL"
python3 - <<'PY'
import base64,json,urllib.request
# A local 1x1 white PNG: probe API/model capacity, not model quality.
png=base64.b64decode("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/iFEAAAAASUVORK5CYII=")
p={'model':'qwen3-vl:2b','stream':False,'messages':[{'role':'user','content':'Describe the image in three words.', 'images':[base64.b64encode(png).decode()]}], 'options':{'num_predict':35,'num_ctx':1024}}
req=urllib.request.Request("http://127.0.0.1:11434/api/chat",data=json.dumps(p).encode(),headers={'Content-Type':'application/json'})
with urllib.request.urlopen(req,timeout=360) as response: d=json.load(response)
assert d.get('message',{}).get('content'),d
print(json.dumps({'status':'QWEN_VISION_INFERENCE_VERIFIED','model':p['model'],'answer':d['message']['content'][:250]},indent=2))
PY
