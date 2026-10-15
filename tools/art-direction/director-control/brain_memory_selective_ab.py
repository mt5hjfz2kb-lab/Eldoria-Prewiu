"""Read-only three-condition Qwen3 14B diagnosis on frozen evidence.
Submitted on the original requested path through normal tool review after the
owner explicitly requested another attempt. No Blender, downloads, subprocesses,
remote hosts, model-generated execution, protected assets or policy changes.
"""
import argparse
import hashlib
import json
import pathlib
import random
import time
import urllib.request
from brain_memory_epistemic_v2 import CASES, MEMORY, SHA
from selective_memory import recall_context

# Prospective phrasing holdout. The selector stays unchanged; gold never reaches it.
# These reuse known factual domains, so they do not establish out-of-domain transfer.
HOLDOUT = [
    ("h_source_same", "Las imágenes de las tres cámaras comparten el SHA de la misma fuente sin alteraciones.", True),
    ("h_locked", "La evidencia declara que la fuente geométrica debe permanecer inalterada.", True),
    ("h_bbox", "La caja WORLD mide aproximadamente 2,45 por 2,65 por 3,27 metros.", True),
    ("h_triangles", "La medición de la franja superior incluye 893 triángulos inclinados.", True),
    ("h_islands", "El recuento de componentes conectados distintos es 3029.", True),
    ("h_single_object", "La importación medida contiene exactamente un objeto de tipo malla.", True),
    ("h_vertical", "En estas mediciones WORLD, el sentido vertical positivo es Z.", True),
    ("h_inclines", "Hay superficies inclinadas registradas en la zona alta del modelo.", True),
    ("h_roof_count", "El total de triángulos permite asegurar que la cubierta solo tiene una vertiente.", False),
    ("h_roof_orientations", "Agrupar normales por orientación basta para certificar exactamente cuatro faldones de tejado.", False),
    ("h_contact", "Dos cajas AABB que se superponen bastan como prueba de contacto entre superficies reales.", False),
    ("h_camera_edits", "Las distintas descripciones de las vistas demuestran que sus cámaras alteraron la geometría.", False),
    ("h_uv", "En este diagnóstico está permitido reemplazar el atlas UV original.", False),
    ("h_final_material", "La iluminación neutra de una captura certifica los materiales definitivos del juego.", False),
    ("h_torches", "Ver antorchas en la referencia artística demuestra que ya existen en la malla fuente.", False),
    ("h_art", "CI verde junto con una valoración del propio director demuestra aprobación artística 8/10.", False),
]
ARMS = ("without_memory", "with_memory", "selective_memory")
SYSTEM = "Verificador epistémico de arte 3D, solo diagnóstico. JSON exclusivamente con decision PROBADO o NO_PROBADO y reason breve."
ENDING = "\nDecide PROBADO solo si se deduce inequívocamente del contexto medido; cualquier conclusión falsa o no demostrada debe ser NO_PROBADO. Devuelve JSON {decision,reason}. No hagas modificaciones."

def sha(data):
    if isinstance(data, str):
        data = data.encode("utf-8")
    return hashlib.sha256(data).hexdigest()

def context_from(g):
    if not (g.get("source_sha256") == SHA and g.get("immutable_source_verified") is True and g.get("all_cameras_same_exact_source_sha256") is True):
        raise ValueError("SOURCE_PROVENANCE_INVALID")
    return json.dumps({
        "sha256": g["source_sha256"], "immutable": g["immutable_source_verified"],
        "all_cameras_same_exact_source_sha256": g["all_cameras_same_exact_source_sha256"],
        "world_up": g["world_up"], "bounding_box": g["bounding_box"],
        "geometry": g["geometry"], "islands": g["connected_island_count"],
        "nonnegotiable_facts": g["nonnegotiable_facts"],
        "evidence_limitations": g["evidence_limitations"],
        "diagnostic_only": True, "original_unchanged": True,
    }, ensure_ascii=False, separators=(",", ":"))

def request_for(context, claim, arm):
    selected = recall_context(claim)
    lessons = MEMORY if arm == "with_memory" else [r["text"] for r in selected["selection"]] if arm == "selective_memory" else []
    prompt = "HECHOS BLENDER + CONTRATO INMUTABLE: " + context
    if lessons:
        prompt += "\nMEMORIA VERIFICADA RECUPERADA: " + json.dumps(lessons, ensure_ascii=False)
    prompt += "\nAFIRMACIÓN A EVALUAR: " + claim + ENDING
    request = {"model": "qwen3:14b", "stream": False, "think": False, "format": "json",
        "messages": [{"role": "system", "content": SYSTEM}, {"role": "user", "content": prompt}],
        "options": {"temperature": 0, "seed": 221015, "num_ctx": 4096, "num_predict": 200}}
    return request, selected if arm == "selective_memory" else None

def summary(records, cases):
    by_key = {(r["case"], r["arm"]): r for r in records}
    complete = all((key, arm) in by_key for _, key, _, _ in cases for arm in ARMS)
    totals = {arm: sum(bool(r.get("correct")) for r in records if r["arm"] == arm) for arm in ARMS}
    regressions = []
    recovered = []
    for group, key, _, _ in cases:
        base = by_key.get((key, "without_memory"), {})
        full = by_key.get((key, "with_memory"), {})
        selective = by_key.get((key, "selective_memory"), {})
        if base.get("correct") and selective and not selective.get("correct"):
            regressions.append(key)
        if full and not full.get("correct") and selective.get("correct"):
            recovered.append(key)
    errors = [r["case"] + ":" + r["arm"] for r in records if r.get("error")]
    return {"complete": complete, "counts": totals, "case_count": len(cases),
        "selective_regressions_vs_baseline": regressions,
        "selective_recoveries_vs_full": recovered, "errors": errors,
        "verdict": "INCOMPLETE" if not complete else "FAIL_ERRORS" if errors else
          "FAIL_SELECTIVE_REGRESSION" if regressions else "NO_SELECTIVE_REGRESSION_IN_THIS_SAMPLE"}

def main():
    p = argparse.ArgumentParser()
    p.add_argument("--grounding", required=True)
    p.add_argument("--out", required=True)
    p.add_argument("--dry-run", action="store_true")
    a = p.parse_args()
    raw = pathlib.Path(a.grounding).read_bytes()
    context = context_from(json.loads(raw.decode("utf-8-sig")))
    groups = (("original", CASES), ("phrasing_holdout", HOLDOUT))
    cases = [(group, key, claim, gold) for group, items in groups for key, claim, gold in items]
    for group, items in groups:
        if len(items) != 16 or sum(gold for _, _, gold in items) != 8 or len({key for key, _, _ in items}) != 16:
            raise ValueError("CASE_INTEGRITY_FAILURE:" + group)
    trials = [(group, key, claim, gold, arm) for group, key, claim, gold in cases for arm in ARMS]
    random.Random(221015).shuffle(trials)
    # Predeclared manifest and exact request payloads retained BEFORE inference.
    manifest = [{"group": group, "case": key, "claim": claim,
                 "gold": "PROBADO" if gold else "NO_PROBADO"} for group, key, claim, gold in cases]
    records = []
    result = {"method": "three-arm randomized paired read-only diagnostic, original 16 plus prospective phrasing holdout 16",
        "source_sha256": SHA, "grounding_sha256": sha(raw), "context_sha256": sha(context),
        "model": "qwen3:14b", "case_manifest": manifest,
        "case_manifest_sha256": sha(json.dumps(manifest, ensure_ascii=False, separators=(",", ":"))),
        "model_options": {"temperature": 0, "seed": 221015, "num_ctx": 4096, "num_predict": 200},
        "source_files_sha256": {name: sha(pathlib.Path(__file__).with_name(name).read_bytes())
            for name in ("brain_memory_epistemic_v2.py", "selective_memory.py", "brain_memory_selective_ab.py")},
        "responses": records, "summary": summary(records, cases),
        "selective_memory_production_enabled": False, "art_approved": False,
        "calibration_validated": False, "any_blender_edits": False, "model_weights_trained": False,
        "limitations": ["Single fixed-seed run per case/arm.", "Holdout is new phrasing in known domains, not out-of-domain generalisation.",
            "Original cases influenced selector development.", "No visual or artistic validation."]}
    out = pathlib.Path(a.out)
    out.parent.mkdir(parents=True, exist_ok=True)
    def save():
        result["summary"] = summary(records, cases)
        result["by_group"] = {group: summary([r for r in records if r["group"] == group],
                    [c for c in cases if c[0] == group]) for group, _ in groups}
        out.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    save()
    if a.dry_run:
        for _, _, claim, _, arm in trials:
            req, _ = request_for(context, claim, arm)
            if req["model"] != "qwen3:14b":
                raise ValueError("MODEL_MISMATCH")
        print("ELDORIA_THREE_ARM_DRY_RUN_ONLY", len(trials), flush=True)
        return
    for group, key, claim, gold, arm in trials:
        req, selected = request_for(context, claim, arm)
        body = json.dumps(req, ensure_ascii=False).encode("utf-8")
        record = {"group": group, "case": key, "arm": arm,
            "gold": "PROBADO" if gold else "NO_PROBADO",
            "request": req, "request_sha256": sha(body),
            "prompt_sha256": sha(req["messages"][1]["content"]), "recall": selected}
        started = time.monotonic()
        try:
            # Fixed localhost endpoint only; never downloads or executes model output.
            request = urllib.request.Request("http://127.0.0.1:11434/api/chat", data=body,
                headers={"Content-Type": "application/json"})
            with urllib.request.urlopen(request, timeout=650) as response:
                response_bytes = response.read()
            response = json.loads(response_bytes)
            response_text = response.get("message", {}).get("content", "")
            record.update({"raw_api_response": response_bytes.decode("utf-8"),
                "raw_api_response_sha256": sha(response_bytes), "raw_model_response": response_text,
                "raw_model_response_sha256": sha(response_text),
                "prompt_eval_count": response.get("prompt_eval_count"), "eval_count": response.get("eval_count"),
                "total_duration_ns": response.get("total_duration")})
            decision = json.loads(response_text).get("decision")
            if not response.get("done") or decision not in ("PROBADO", "NO_PROBADO"):
                raise ValueError("MALFORMED_OR_INCOMPLETE")
            record["decision"] = decision
        except Exception as error:
            record["error"] = str(error)[:1000]
        record["seconds"] = round(time.monotonic() - started, 3)
        record["correct"] = record.get("decision") == record["gold"]
        records.append(record)
        save()
        print("ELDORIA_THREE_ARM", group, key, arm, record["correct"], flush=True)
    print("ELDORIA_THREE_ARM_COMPLETE", json.dumps(result["summary"]), flush=True)
    if result["summary"]["errors"] or result["summary"]["selective_regressions_vs_baseline"]:
        raise SystemExit(2)

if __name__ == "__main__":
    main()
