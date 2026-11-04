#!/usr/bin/env python3
"""SLM Mode-A disposable benchmark. Public Eldoria facts only; no main/Unity/Windows access."""
import json
import os
import pathlib
import subprocess
import sys
import time
from datetime import datetime, timezone

outdir = pathlib.Path(os.environ["RUNNER_TEMP"]) / "eldoria-slm-result"
outdir.mkdir(parents=True, exist_ok=True)
calls = []
def run(*args, timeout=80):
    t = time.monotonic()
    try:
        x = subprocess.run(["slm", *args], capture_output=True, text=True,
                           errors="replace", timeout=timeout, check=False)
        o = {"command": "slm " + args[0], "rc": x.returncode,
             "stdout": x.stdout[-5000:], "stderr": x.stderr[-2000:],
             "seconds": round(time.monotonic()-t, 3)}
    except subprocess.TimeoutExpired as e:
        o = {"command": "slm " + args[0], "rc": 124,
             "stdout": "TIMEOUT", "stderr": str(e)[-500:],
             "seconds": round(time.monotonic()-t, 3)}
    calls.append(o)
    return o

# Public facts taken from README, manifest, QA documents and design decisions.
# This is a technical memory test, not a claim that the current memory data has been imported.
facts = [
  ("En Eldoria la fuente jugable principal está en Unity/; v0220 es el prototipo web histórico.",
   [("¿Dónde está el juego principal de Eldoria?", "Unity/"), ("¿Cuál es el directorio del prototipo web histórico?", "v0220")]),
  ("El proyecto Eldoria usa Unity Editor 6000.3.23f1 y el pipeline de render URP 17.3.0.",
   [("¿Cuál es la versión actual del editor Unity de Eldoria?", "6000.3.23f1"), ("¿Qué versión de URP usa Eldoria?", "17.3.0")]),
  ("La coordinación canónica de trabajos concurrentes de Eldoria se encuentra en pipeline/active-workstreams.json.",
   [("¿Dónde se registra quién tiene un trabajo?", "active-workstreams.json"), ("¿En qué carpeta está el registro de trabajos?", "pipeline/")]),
  ("La PR de interfaz canónica Eldoria es #136 y su rama es fix/player-feedback-context-ui-20261030.",
   [("¿Cuál es la PR canónica de interfaz?", "136"), ("¿Cómo se llama la rama de interfaz?", "fix/player-feedback-context-ui-20261030")]),
  ("La referencia maestra artística del Salón de Héroes está en docs/heroes-ui/REFERENCIA_MAESTRA_APROBADA.png.",
   [("¿Cuál es el archivo de la referencia maestra de héroes?", "REFERENCIA_MAESTRA_APROBADA.png"), ("¿En qué carpeta están las referencias de héroes?", "heroes-ui")]),
  ("El combate de Eldoria se resuelve automáticamente y las decisiones tácticas se toman antes de enviar la marcha.",
   [("¿El combate de Eldoria exige pulsar habilidades durante la lucha?", "automáticamente"), ("¿Cuándo se toman las decisiones tácticas de combate?", "antes")]),
  ("El mundo 4X de Eldoria tiene tres niveles de zoom: cercano, regional y panorámico.",
   [("¿Cómo se llama el zoom intermedio del mundo 4X?", "regional"), ("¿Cómo se llama el zoom más alejado del mundo 4X?", "panorámico")]),
  ("En Eldoria el PvE evita muertos; en PvP hay heridos graves y muertes cuando el hospital está lleno.",
   [("¿En qué modo de combate no se producen muertos?", "PvE"), ("¿Qué ocurre si se llena el hospital en PvP?", "muertes")]),
  ("Eldoria entrega pruebas WebGL en itch.io y la configuración de la actualización familiar Android está en updates/android/family.json.",
   [("¿Dónde se publica WebGL para las pruebas?", "itch.io"), ("¿Qué archivo controla el canal Android familiar?", "family.json")]),
  ("Eldoria verifica las pantallas en cuatro resoluciones: 1600x900, 900x1600, 390x844 y 844x390.",
   [("¿Qué resolución horizontal grande se utiliza en QA?", "1600x900"), ("¿Qué resolución horizontal móvil se utiliza en QA?", "844x390")]),
]
negative_queries = [
 "¿Cuál es la contraseña del servidor secreto de Eldoria?",
 "¿Qué número de tarjeta bancaria está asociado a Eldoria?",
 "¿Quién es el ganador del torneo Eldoria que tendrá lugar en 2035?",
]
summary = {"timestamp": datetime.now(timezone.utc).isoformat(),
           "package_pin": "superlocalmemory==4.2.12", "environment": "GitHub-hosted Ubuntu only",
           "no_windows": True, "no_unity": True, "no_secrets": True,
           "no_main_modification": True, "facts_count": len(facts),
           "queries_count": sum(len(qs) for _,qs in facts),
           "outcomes": {}, "recall_checks": [], "unknown_checks": []}
try:
    for key, args, limit in [
        ("version", ["--version"], 30),
        ("init", ["init", "--auto"], 95),
        ("mode", ["mode", "a"], 40),
        ("serve", ["serve", "start"], 90),
    ]:
        res = run(*args, timeout=limit)
        summary["outcomes"][key] = {"rc": res["rc"], "stdout": res["stdout"][-350:],
                                    "stderr": res["stderr"][-350:], "seconds": res["seconds"]}
        if key == "init" and res["rc"] != 0:
            raise RuntimeError("SLM initialization failed; benchmark not meaningful")
    warmup = run("warmup", timeout=210)
    summary["outcomes"]["warmup"] = {"rc": warmup["rc"],
          "stdout": warmup["stdout"][-350:], "stderr": warmup["stderr"][-350:],
          "seconds": warmup["seconds"]}
    remembered = 0
    for idx,(content,qs) in enumerate(facts, 1):
        r = run("remember", content, "--kind", "fact", "--tags", "eldoria,isolated-pilot", timeout=65)
        if r["rc"] == 0:
            remembered += 1
        else:
            summary.setdefault("remember_failures", []).append({"index":idx,"error":r["stderr"][-500:]})
    summary["remembered_count"] = remembered
    if remembered < 1:
        raise RuntimeError("No fact could be remembered")
    time.sleep(6)
    hit = 0
    for idx,(content,qs) in enumerate(facts, 1):
        for question,expected in qs:
            r = run("recall", question, timeout=60)
            correct = r["rc"] == 0 and expected.casefold() in r["stdout"].casefold()
            hit += int(correct)
            summary["recall_checks"].append({
               "fact_index": idx, "question":question, "expected_substring":expected,
               "match":correct, "returncode":r["rc"],
               "reply":r["stdout"][-800:], "error":r["stderr"][-180:],
               "seconds":r["seconds"]})
    summary["simple_substring_hits"] = hit
    for q in negative_queries:
        r = run("recall", q, timeout=60)
        text = (r["stdout"] + "\n" + r["stderr"]).casefold()
        abstained = any(x in text for x in ["isn't in superlocalmemory",
              "no confident", "no matching", "no memories", "not found",
              "no relevant", "no results", "no answer", "no tengo", "no se encuentra"])
        summary["unknown_checks"].append({"question":q, "abstention_indicator":abstained,
                                            "rc":r["rc"],"reply":r["stdout"][-700:]})
    summary["result"] = ("PASS_BASIC_MEMORY" if remembered == len(facts) and hit>=1
                         else "INCOMPLETE_OR_FAILED")
except Exception as err:
    summary["result"] = "FAILED"
    summary["error"] = str(err)
finally:
    try:
        z=run("serve", "stop", timeout=20)
        summary["outcomes"]["stop"] = {"rc":z["rc"]}
    except Exception:
        pass
    (outdir/"benchmark.json").write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding="utf-8")
    print(json.dumps({k:summary.get(k) for k in ("result","package_pin","facts_count","remembered_count","queries_count",
        "simple_substring_hits","unknown_checks","outcomes","error") if k in summary},ensure_ascii=False,indent=2))
sys.exit(0 if summary["result"]=="PASS_BASIC_MEMORY" else 2)
