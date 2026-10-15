"""Execute image-grounded TWO-PHASE independent Ollama vision critique.

Phase 1: actual part identification. Phase 2: aesthetics only if Phase 1
passed stringent semantic/hallucination checks. Failure remains REVIEW_HOLD.
"""
import argparse,base64,hashlib,json,pathlib,urllib.request,urllib.error
from PIL import Image
from io import BytesIO
from grounded_review import (sha,manifest,identification_messages,
    validate_identification,critic_messages,validate_critique,extract_json,
    UnreliableCritic)

def image_b64(p):
    with Image.open(p) as im:
        rgb=im.convert("RGB")
        rgb.thumbnail((700,700))
        output=BytesIO();rgb.save(output,format="JPEG",quality=80)
    return base64.b64encode(output.getvalue()).decode()

def ask(model, messages,encoded,timeout):
    messages=[dict(x) for x in messages]
    messages[-1]["images"]=encoded
    packet={"model":model,"stream":True,"think":False,
       "options":{"temperature":0.1,"num_ctx":8192,"num_predict":1250},
       "messages":messages}
    req=urllib.request.Request("http://127.0.0.1:11434/api/chat",
             data=json.dumps(packet).encode(),headers={"Content-Type":"application/json"})
    text=[];done=False
    with urllib.request.urlopen(req,timeout=timeout) as f:
        for line in f:
            if not line.strip():continue
            data=json.loads(line)
            if data.get("error"):raise RuntimeError(data["error"])
            text.append(data.get("message",{}).get("content",""))
            if data.get("done"):
                done=True;break
    if not done:raise RuntimeError("TRUNCATED_MODEL_RESPONSE")
    return "".join(text)

def main():
    p=argparse.ArgumentParser()
    for x in ("plan","catalog","contact","reference","before","after","out"):
        p.add_argument("--"+x,required=True)
    p.add_argument("--model",default="qwen3-vl:8b-instruct-q4_K_M")
    a=p.parse_args()
    out=pathlib.Path(a.out);out.mkdir(parents=True,exist_ok=True)
    plan=json.loads(pathlib.Path(a.plan).read_text())
    cat=json.loads(pathlib.Path(a.catalog).read_text())
    contacts=json.loads(pathlib.Path(a.contact).read_text())
    if cat["artist_plan_sha256"]!=sha(a.plan):
        raise UnreliableCritic("PLAN_SHA_DOES_NOT_MATCH_ACTUAL_ARTIFACT")
    inventory=manifest(plan,cat,contacts)
    (out/"grounded-inventory.json").write_text(json.dumps(inventory,indent=2)+"\n")
    hashes={"reference":sha(a.reference),"before":sha(a.before),"after":sha(a.after)}
    encoded=[image_b64(x) for x in (a.reference,a.before,a.after)]
    raw1="";raw2=""
    result={"status":"REVIEW_HOLD","phase_1":"NOT_RUN","phase_2":"NOT_RUN",
            "art_approved":False,"calibration_validated":False,"images":hashes}
    try:
        raw1=ask(a.model,identification_messages(inventory,hashes),encoded,320)
        identification=validate_identification(extract_json(raw1),inventory,hashes)
        result["phase_1"]=identification
        if identification["status"]=="IDENTIFICATION_TRUSTED":
            raw2=ask(a.model,critic_messages(inventory,hashes),encoded,320)
            result["phase_2"]=validate_critique(extract_json(raw2),inventory,hashes)
            if result["phase_2"]["status"]=="PROVISIONAL_REVIEW_GROUNDED":
                result["status"]="PROVISIONAL_REVIEW_GROUNDED"
        else:
            result["phase_2"]="BLOCKED_BY_IDENTIFICATION"
    except (OSError,ValueError,RuntimeError) as exc:
        result["error"]=type(exc).__name__+": "+str(exc)[:300]
    finally:
        (out/"critic-phase1-raw.txt").write_text(raw1,encoding="utf8")
        (out/"critic-phase2-raw.txt").write_text(raw2,encoding="utf8")
        (out/"grounded-critic-verdict.json").write_text(json.dumps(result,indent=2)+"\n")
        print("ELDORIA_GROUNDED_CRITIC="+json.dumps(result,ensure_ascii=False))
    return result

if __name__=="__main__":main()
