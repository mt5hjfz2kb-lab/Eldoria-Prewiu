"""Use actual local Qwen vision to make an independent cliff authoring decision."""
import json,base64,io,urllib.request
from pathlib import Path
from PIL import Image
root=Path(__file__).resolve().parents[2]
out=root/"art-source/valoria/experiments/cliff-ai-authorship-ab"
def enc(p):
    with Image.open(p) as im:
        im=im.convert("RGB");im.thumbnail((640,480))
        buf=io.BytesIO();im.save(buf,format="JPEG",quality=78)
        return base64.b64encode(buf.getvalue()).decode()
ref=root/"art-source/valoria/production/first-hero-gate-v1/quality-v3/after-hero-matched.png"
assert ref.exists()
instructions=("Act as an actual 3D geological sculptor for mobile medieval city Eldoria Valoria. "
    "Image 1 is unchanged V3 quality reference. Images 2 and 3 show CURRENT cliff. "
    "Create NEW cliff INDEPENDENTLY, avoiding a floating flat slab, repeated rocks, radial symmetry and square cut sides. "
    "Direct the geometry from the beginning. Choose 12 ordered irregular perimeter control profiles "
    "clockwise around origin, each [x,y,slope,foot], around a roughly 9x8 meter footprint: "
    "x in [-5,5], y in [-4.5,4.5], slope .05-.35 and foot .02-.30. "
    "Use these individually intentional controls to create buttress-like rock masses, ledges, erosion, "
    "seam at castle footing, talus flare and layered rock face. "
    "Choose seven descending geological level heights, first 0 then to -4.8 ±0.3. "
    "Choose one actual sculpt operation from cut_strata, carve_gullies, fracture_overhang. "
    "fracture_phase numeric 0 to 6. Supply credible, specific geological reasoning, mobile legibility "
    "and describe how the scene meets the unchanged gate. No status or quality approval. JSON only.")
schema={"type":"object","properties":{
    "diagnosis":{"type":"string"},"formation":{"type":"string"},
    "artistic_decision":{"type":"string"},"integration":{"type":"string"},
    "profiles":{"type":"array","minItems":12,"maxItems":12,"items":{"type":"array","minItems":4,"maxItems":4,"items":{"type":"number"}}},
    "levels":{"type":"array","minItems":7,"maxItems":7,"items":{"type":"number"}},
    "intervention":{"type":"string","enum":["cut_strata","carve_gullies","fracture_overhang"]},
    "fracture_phase":{"type":"number","minimum":0,"maximum":6}},
    "required":["diagnosis","formation","artistic_decision","integration","profiles","levels","intervention","fracture_phase"],
    "additionalProperties":False}
payload={"model":"qwen2.5vl:3b","stream":False,"think":False,"format":schema,
    "options":{"num_predict":1800,"num_ctx":8192,"temperature":0.25},
    "messages":[{"role":"user","content":instructions,"images":[enc(ref),enc(out/"A-hero.png"),enc(out/"A-strategic.png")]}]}
attempts=[]
for attempt in range(3):
    try:
        req=urllib.request.Request("http://127.0.0.1:11434/api/chat",data=json.dumps(payload).encode(),
            headers={"Content-Type":"application/json"})
        with urllib.request.urlopen(req,timeout=550) as handle:r=json.load(handle)
        raw=r.get("message",{}).get("content","")
        item=json.JSONDecoder().raw_decode(raw[raw.find("{"):])[0]
        p=item["profiles"];lv=item["levels"]
        assert len(p)==12 and len(lv)==7
        assert all(len(v)==4 and -5.5<=v[0]<=5.5 and -5<=v[1]<=5 and .05<=v[2]<=.35 and .02<=v[3]<=.30 for v in p)
        assert abs(lv[0])<.4 and -5.1<lv[-1]<-4.4
        assert all(lv[i]>lv[i+1] for i in range(6))
        assert len(set((round(v[0],1),round(v[1],1)) for v in p))>=10
        assert all(str(item[k]).strip() for k in ("diagnosis","formation","artistic_decision","integration"))
        assert item["intervention"] in ("cut_strata","carve_gullies","fracture_overhang")
        assert 0<=item["fracture_phase"]<=6
        attempts.append({"attempt":attempt+1,"raw":raw,"valid":True})
        (out/"B-ai-art-direction.json").write_text(json.dumps({"plan":item,"traces":attempts,"source":"Qwen2.5VL local visual reasoning, no fabricated fallback"},indent=2))
        print("VALID_ART_DIRECTOR_AUTHORSHIP",item["formation"],flush=True)
        break
    except Exception as e:
        attempts.append({"attempt":attempt+1,"raw":locals().get("raw",""),"error":str(e)})
        (out/"B-ai-art-direction-traces.json").write_text(json.dumps(attempts,indent=2))
        if attempt==2:raise RuntimeError("No valid original IA cliff design after real vision retries") from e
