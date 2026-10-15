"""Selective recall of *existing* verified Eldoria lessons, diagnosis only.

Do not invent facts, create new weights or infer approval from recall.
The canonical seven lesson texts stay in brain_memory_epistemic_v2.MEMORY:
this module adds ONLY topic indexing and a provenance record.
"""
import hashlib
import json
import re
import unicodedata
from brain_memory_epistemic_v2 import MEMORY

MAX_RECALLED = 2
CATALOG_SOURCE = "docs/evidence/ELDORIA_ART_BRAIN_V1_VALIDATION_20261015.md"
ORIGINAL_SHA256 = "e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869"

# Index refers to the canonical MEMORY entry. Every topic needs ALL required cue groups.
# These describe potential error classes, NOT answer labels or particular test case IDs.
TOPICS = (
    (0, (r"render|captura|observac|descripcion|imagen", r"interpret|equival|demuestr|prueb|certific|cambi|diferente")),
    (1, (r"camara|perspectiva|vistas?", r"archivo|glb|sha|vertice|geometr|mutac|modific")),
    (2, (r"tejado|cubierta|faldon|vertiente|aguas", r"numero|cuant|exact|unic|dos|cuatro|certific|prueb|demuestr")),
    (3, (r"aabb|caja", r"contact|solap|superpos|intersect|union")),
    (4, (r"referencia|antorcha|iluminacion|luz neutra|material|textura", r"presencia|final|certific|prueb|demuestr|equival|existe")),
    (5, (r"uv|escalera|barandilla|entrada", r"edit|modific|sustitu|cambi|reemplaz|mov|permite|autoriza|protegid")),
    (6, (r"pass|gate|autocritica|calidad|aprobacion|8/10|ci verde", r"art|calidad|certific|prueb|demuestr|valid|aprobad")),
)

def _plain(value):
    text = unicodedata.normalize("NFKD", str(value or "").casefold())
    return "".join(char for char in text if not unicodedata.combining(char))

def recall(claim, *, max_lessons=MAX_RECALLED):
    """Return source-grounded topical lessons; abstain for claims without error-risk cues.

    Selection is deterministic and can never consult expected labels, model responses,
    geometry measurements or evaluation outcomes. No external network or embeddings.
    """
    if not isinstance(claim, str):
        raise TypeError("claim must be text")
    if not isinstance(max_lessons, int) or isinstance(max_lessons, bool) or not 0 <= max_lessons <= 2:
        raise ValueError("max_lessons must be 0, 1 or 2")
    if len(MEMORY) != 7 or len(TOPICS) != len(MEMORY):
        raise ValueError("canonical memory catalog changed; re-review topical index")
    norm = _plain(claim)
    matches = []
    for index, cues in TOPICS:
        if all(re.search(cue, norm, flags=re.IGNORECASE) for cue in cues):
            matches.append({"index": index, "text": MEMORY[index], "topic_evidence": list(cues),
                            "lesson_sha256": hashlib.sha256(MEMORY[index].encode()).hexdigest(),
                            "source": CATALOG_SOURCE})
    # Higher specificity wins, tied matches retain canonical index order.
    matches.sort(key=lambda m: (-sum(len(x) for x in m["topic_evidence"]), m["index"]))
    return matches[:max_lessons]

def recall_context(claim, *, max_lessons=MAX_RECALLED):
    """Exact user-prompt memory payload + auditable record with no answer leakage."""
    selected = recall(claim, max_lessons=max_lessons)
    payload = [r["text"] for r in selected]
    encoded = json.dumps(payload, ensure_ascii=False, separators=(",", ":"))
    return {"prompt_fragment": "LECCIONES VERIFICADAS RELEVANTES: " + encoded if payload else "",
            "selection": selected,
            "selection_count": len(selected),
            "selection_payload_sha256": hashlib.sha256(encoded.encode()).hexdigest(),
            "original_source_sha256": ORIGINAL_SHA256,
            "source_document": CATALOG_SOURCE}
