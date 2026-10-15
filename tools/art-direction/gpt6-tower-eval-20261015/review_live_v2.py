"""Independent reviewer V2: genuinely see the edit before scoring its art.

No hashes or physical distances are invented/echoed by Qwen; these are added
by the trusted runner. The model only judges visible objects; per-component
ROIs bound to original image hashes catch demonstrably false "not visible".
"""
from __future__ import annotations
import argparse,base64,hashlib,json,pathlib,urllib.request
from PIL import Image
import numpy as np
from grounded_review import (sha,manifest,extract_json,validate_critique,
   forbidden_unobserved_terms,UnreliableCritic)
import re

def img(path,box=None):
    im=Image.open(path).convert("RGB")
    if box:im=im.crop(tuple(box))
    im.thumbnail((740,740))
    from io import BytesIO
    q=BytesIO();im.save(q,format="JPEG",quality=82)
    return base64.b64encode(q.getvalue()).decode()

def ask(model,system,user,images):
    payload={"model":model,"stream":True,"think":False,
       "options":{"temperature":0.1,"num_ctx":12288,"num_predict":1100},
       "messages":[{"role":"system","content":system},
                  {"role":"user","content":user,"images":images}]}
    req=urllib.request.Request("http://127.0.0.1:11434/api/chat",
                   data=json.dumps(payload).encode(),headers={"Content-Type":"application/json"})
    chunks=[];done=False
    with urllib.request.urlopen(req,timeout=420) as response:
        for line in response:
            if not line.strip():continue
            d=json.loads(line)
            if "error" in d:raise RuntimeError(str(d["error"]))
            chunks.append(d.get("message",{}).get("content",""))
            if d.get("done"):done=True;break
    if not done:raise RuntimeError("STREAM_TRUNCATED")
    return "".join(chunks)

def screen_grounding(regions,inventory,before,after,source_sha):
    if regions.get("before_sha256")!=sha(before) or regions.get("after_sha256")!=sha(after):
        raise UnreliableCritic("ROI_NOT_FROM_THESE_IMAGE_BYTES")
    if regions.get("source_sha256")!=source_sha:
        raise UnreliableCritic("ROI_FROZEN_SOURCE_MISMATCH")
    a=np.asarray(Image.open(after).convert("RGB"),dtype=np.int16)
    x=np.asarray(Image.open(before).convert("RGB"),dtype=np.int16)
    if a.shape!=x.shape:raise UnreliableCritic("CAMERA_SIZE_CHANGED")
    expected={p["id"] for p in inventory["new_components"]}
    declared={p["component_id"] for p in regions["regions"]}
    if expected!=declared or len(regions["regions"])!=len(expected):
        raise UnreliableCritic("ROI_INVENTORY_MISMATCH")
    report={}
    for entry in regions["regions"]:
        l,t,r,b=entry["box_px"]
        if not (0<=l<r<=a.shape[1] and 0<=t<b<=a.shape[0]):
            raise UnreliableCritic("INVALID_IMAGE_ROI")
        diff=np.abs(a[t:b,l:r]-x[t:b,l:r])
        changed=float((diff.max(axis=2)>12).mean())
        report[entry["component_id"]]={"box_px":[l,t,r,b],
            "changed_fraction":round(changed,6),
            "machine_observed_change":changed>.02,
            "note":"pixel difference within independently annotated ROI, not aesthetic quality"}
    return report

def inject_trusted_facts(response,manifest,hashes):
    """Do not train the model to fake SHA hashes or ground-truth proximity."""
    if not isinstance(response,dict):raise UnreliableCritic("NOT_OBJECT")
    data=dict(response)
    if "image_sha256" in data and data["image_sha256"]!=hashes:
        # Not a criterion of perception; it was not asked to recite hash bytes.
        data.pop("image_sha256")
    data["image_sha256"]=dict(hashes)
    facts={p["id"]:p for p in manifest["new_components"]}
    if not isinstance(data.get("observations"),list):
        raise UnreliableCritic("MISSING_OBSERVATIONS")
    observations=[]
    for o in data["observations"]:
        if not isinstance(o,dict) or o.get("component_id") not in facts:
            raise UnreliableCritic("HALLUCINATED_OBJECT_REFERENCE")
        copied=dict(o);actual=("NEAR_SOURCE" if facts[o["component_id"]]["surface_near_original"] else "NOT_NEAR_SOURCE")
        if "measured_contact" in copied and copied["measured_contact"]!=actual:
            raise UnreliableCritic("MODEL_INVENTED_FALSE_PHYSICAL_MEASUREMENT")
        copied["measured_contact"]=actual
        observations.append(copied)
    data["observations"]=observations
    return data

def phase1_check(model_response,inventory,roi):
    ids={p["id"] for p in inventory["new_components"]}
    observations=model_response.get("observations") if isinstance(model_response,dict) else None
    errors=[]
    if not isinstance(observations,list) or len(observations)!=len(ids):
        return {"status":"IDENTIFICATION_UNRELIABLE","errors":["COUNT_MISMATCH"]}
    returned=[o.get("component_id") for o in observations if isinstance(o,dict)]
    if len(returned)!=len(ids) or set(returned)!=ids or len(set(returned))!=len(ids):
        errors.append("MODEL_REFERENCED_NONEXISTENT_OR_MISSING_MESH")
    for o in observations:
        if not isinstance(o,dict) or o.get("component_id") not in roi:continue
        ident=o["component_id"]
        if o.get("visibility") not in ("visible","not_visible","uncertain"):
            errors.append("INVALID_VISIBILITY")
        if o.get("visual_joinery") not in ("convincing","poor","uncertain","not_applicable"):
            errors.append("INVALID_JOINERY")
        if not isinstance(o.get("visual_reason"),str) or len(o["visual_reason"])<8:
            errors.append("UNEXPLAINED_VISUAL_STATEMENT")
        if roi[ident]["machine_observed_change"] and o.get("visibility")=="not_visible":
            errors.append("OBJECTIVE_VISIBILITY_CONTRADICTION:"+ident)
    utterance=json.dumps(model_response,ensure_ascii=False).lower()
    for pattern in forbidden_unobserved_terms(inventory):
        if re.search(pattern,utterance):errors.append("INVENTED_COMPONENT:"+pattern)
    return {"status":"IDENTIFICATION_UNRELIABLE" if errors else "IDENTIFICATION_GROUNDED",
            "errors":sorted(set(errors)),"all_present":len(returned)==len(ids) and set(returned)==ids}

def main():
    p=argparse.ArgumentParser()
    for name in ("plan","catalog","contact","reference","before","after","roi","out"):p.add_argument("--"+name,required=True)
    p.add_argument("--model",default="qwen3-vl:8b-instruct-q4_K_M")
    z=p.parse_args()
    out=pathlib.Path(z.out);out.mkdir(parents=True,exist_ok=True)
    artplan=json.loads(pathlib.Path(z.plan).read_text())
    catalog=json.loads(pathlib.Path(z.catalog).read_text())
    contacts=json.loads(pathlib.Path(z.contact).read_text())
    if catalog["artist_plan_sha256"]!=sha(z.plan):
        raise UnreliableCritic("ORIGINAL_RUN_PLAN_PROVENANCE_MISMATCH")
    inv=manifest(artplan,catalog,contacts)
    if catalog["source_sha256"]!=json.loads(pathlib.Path(z.roi).read_text())["source_sha256"]:
        raise UnreliableCritic("SOURCE_SHA_MISMATCH")
    rois=json.loads(pathlib.Path(z.roi).read_text())
    ground=screen_grounding(rois,inv,z.before,z.after,catalog["source_sha256"])
    hashes={"reference":sha(z.reference),"before":sha(z.before),"after":sha(z.after)}
    result={"status":"REVIEW_HOLD","art_approved":False,"calibration_validated":False,
            "source_sha256":catalog["source_sha256"],
            "images":hashes,"objective_roi_change":ground,
            "phase1":"NOT_RUN","phase2":"NOT_RUN"}
    raw1=raw2=""
    names=[{"component_id":k["id"],"mesh_shape":k["shape"],
            "dimensions_m":k["dimensions_m"]} for k in inv["new_components"]]
    focus=[min(p["box_px"][0] for p in ground.values()),min(p["box_px"][1] for p in ground.values()),
           max(p["box_px"][2] for p in ground.values()),max(p["box_px"][3] for p in ground.values())]
    focus=[max(0,focus[0]-20),max(0,focus[1]-20),focus[2]+20,focus[3]+20]
    try:
        raw1=ask(z.model,
           "PHASE 1: object recognition ONLY. You will see BEFORE photo, AFTER photo, and a ZOOM of AFTER front facade, all from the SAME Blender camera. "
           "Identify new parts from actual exported mesh names. Compare AFTER to BEFORE, not to unrelated architecture. "
           "A visible timber frame is not absent because it was not in BEFORE. Never mention previous experiments. "
           "Return JSON only: {observations:[{component_id,visibility:visible|not_visible|uncertain,visual_joinery:convincing|poor|uncertain|not_applicable,visual_reason}]}. "
           "Exactly one record per identifier. No hash copying, no physical distance claims, no quality score.",
           "Check the NEW parts exactly: "+json.dumps(names,ensure_ascii=False)+"; image1=BEFORE, image2=AFTER, image3=zoomed AFTER.",
           [img(z.before),img(z.after),img(z.after,focus)])
        parsed1=extract_json(raw1)
        check=phase1_check(parsed1,inv,ground)
        result["phase1"]=check
        if check["status"]=="IDENTIFICATION_GROUNDED":
            raw2=ask(z.model,
              "PHASE 2: independently judge aesthetic change from reference, BEFORE, AFTER, and zoomed AFTER. "
              "Use the inventory; never invent new structural components. Physical contact is measured by Blender "
              "and visual joinery can still look poor. Return strict JSON: "
              "{observations:[{component_id,visibility,visual_joinery,visual_reason}],"
              "decision:IMPROVED|REJECTED|INCONCLUSIVE,"
              "scores:{silhouette,architectural_contact,materials,style,game_camera},"
              "reasons:[text],uncertainties:[text]}. Values for scores are 0-10. "
              "Do NOT echo SHA/image provenance or physical distances; runner attaches verified data. "
              "Provisional visual critique only, NEVER ART_APPROVED.",
              "Actual new components: "+json.dumps(names,ensure_ascii=False)+
              ". Phase1 verified identifications: "+json.dumps(parsed1["observations"],ensure_ascii=False),
              [img(z.reference),img(z.before),img(z.after),img(z.after,focus)])
            data=inject_trusted_facts(extract_json(raw2),inv,hashes)
            assessment=validate_critique(data,inv,hashes)
            result["phase2"]=assessment
            if assessment["status"]=="PROVISIONAL_REVIEW_GROUNDED":
                result["status"]="PROVISIONAL_REVIEW_GROUNDED"
        else:
            result["phase2"]="BLOCKED_BY_VISIBILITY_GATE"
    except Exception as e:
        result["error"]=type(e).__name__+": "+str(e)[:500]
    finally:
        (out/"critic-v2-phase1.txt").write_text(raw1,encoding="utf8")
        (out/"critic-v2-phase2.txt").write_text(raw2,encoding="utf8")
        (out/"critic-v2-evaluation.json").write_text(json.dumps(result,indent=2)+"\n")
        print("ELDORIA_REVIEW_V2="+json.dumps(result))
if __name__=="__main__":main()
