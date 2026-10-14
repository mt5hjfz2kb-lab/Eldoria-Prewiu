"""Evidence-backed cross-cycle lessons without copying irrelevant failed artist actions.

The Qwen 2B regression from run 38558877838 was caused by pasting raw cliff
journal text into the *new medieval watchtower* request. It emitted obsolete
cliff coordinates/IDs repeatedly and exhausted the response budget.

This extractor does not invent corrections and never uses old plans as new
plans. It retains original journal hash for audit, selects only broadly
transferable visual lessons and strips IDs/paths/coordinates from *context*.
"""
from __future__ import annotations
import hashlib,re

TOPICS={
    "silhouette":5,"composition":4,"material":3,"visual":3,"camera":3,
    "reference":3,"coherent":3,"modular":3,"asset":2,"geometry":2,
    "ground":2,"lighting":2,"source":2,"quality":2,"texture":3,
    "proportion":3,"terrain":1,"mobile":1,
}
# Historic cliff/tool coordinates were poisoning unrelated asset briefs.
BLOCK=re.compile(
    r"(?:https?://|sha256|artifact|run #|runner|qwen[23]?|"
    r"[-+]?\b\d{3,}\b|[-+]?\[\s*-?\d+\.?\d*,|"
    r"\bcliff\b|\bsculpt_region\b|\bV[34568]\b)",
    re.I,
)
REDACTIONS=[
 (re.compile(r"\`[^\`]{1,250}\`"),""),
 (re.compile(r"https?://\S+"),""),
 (re.compile(r"(?<!\w)[a-f0-9]{32,64}(?!\w)",re.I),""),
 (re.compile(r"\s+")," "),
]

def transferable_lessons(journal: str, brief: str="", limit: int=900):
    """Return only literal, origin-grounded journal fragments, with SHA."""
    if not isinstance(journal,str) or not 240<=limit<=3000:
        raise ValueError("Full-studio journal context is bounded")
    raw_hash=hashlib.sha256(journal.encode("utf-8")).hexdigest()
    scored=[]
    for line_index,line in enumerate(journal.splitlines()):
        raw=line.strip().lstrip("-* ")
        if not 30<len(raw)<950:continue
        # Split recorded observations, not a whole historical failed plan.
        segments=re.split(r"(?<=[.!?])\s+(?=[A-Z])|;\s+",raw)
        for chunk in segments:
            text=chunk.strip()
            for pattern,replacement in REDACTIONS:
                text=pattern.sub(replacement,text)
            text=text.strip(" \t-*:,.")
            if not 38<=len(text)<=210 or BLOCK.search(text):continue
            low=text.lower()
            matches=sum(weight for word,weight in TOPICS.items() if word in low)
            if matches<5:continue
            # Relevant object-specific words can prioritize, but do not
            # manufacture new examples not present in the checked-in journal.
            brief_words={w for w in re.findall(r"[a-zA-Z]{5,}",brief.lower())
                         if w not in ("create","medieval","artist","family")}
            matches+=sum(2 for w in brief_words if w in low)
            scored.append((matches,line_index,text))
    ranked=sorted(scored,key=lambda item:(-item[0],-item[1]))
    chosen=[];seen=set();length=0
    for score,line,text in ranked:
        key=text.lower()[:70]
        if key in seen:continue
        if length+len(text)+3>limit:continue
        seen.add(key);chosen.append({"journal_line":line+1,"lesson":text,"relevance":score})
        length+=len(text)+3
        if len(chosen)>=6:break
    return {"journal_sha256":raw_hash,"source":"ARTIST_LEARNING_JOURNAL.md",
            "verbatim_grounded":True,"lessons":chosen,"prompt_text":" | ".join(x["lesson"] for x in chosen)}


def evidence_case_lessons(casebook: dict, brief: str = "", limit: int = 1400, max_cases: int = 5):
    """Select relevant *recorded* lessons; never replay old actions or promote a verdict.

    A case with provenance metadata is not proof that an independent reviewer
    approved it. Every selected observation keeps the original evidence IDs.
    """
    if not isinstance(casebook, dict) or casebook.get("schema_version") != 1:
        raise ValueError("Versioned artist casebook required")
    if not 240 <= limit <= 4000 or not 1 <= max_cases <= 8:
        raise ValueError("Artist case memory budget invalid")
    words = {w for w in re.findall(r"[a-zA-Z]{4,}", brief.lower())
             if w not in {"make", "with", "from", "artist", "real", "medieval"}}
    scored = []
    for idx, case in enumerate(casebook.get("cases", [])):
        if not isinstance(case, dict):
            continue
        lesson = case.get("safe_lesson")
        raw_sha = case.get("raw_response_sha256")
        if not (isinstance(lesson, str) and 35 <= len(lesson) <= 450 and
                isinstance(raw_sha, str) and re.fullmatch(r"[a-fA-F0-9]{64}", raw_sha) and
                type(case.get("run_id")) is int and case["run_id"] > 0 and
                type(case.get("evidence_artifact_id")) is int and case["evidence_artifact_id"] > 0):
            continue
        if BLOCK.search(lesson):
            continue
        clean = re.sub(r"\s+", " ", lesson).strip()
        score = sum(3 for word in words if word in clean.lower())
        score += sum(weight for topic, weight in TOPICS.items() if topic in clean.lower())
        scored.append((score, idx, clean, case))
    selected = []
    used = set()
    budget = 0
    for score, idx, clean, case in sorted(scored, key=lambda v: (-v[0], -v[1])):
        if clean.lower() in used:
            continue
        outcome = str(case.get("result", "UNVERIFIED"))[:70]
        # Explicitly identify this as history, never as an accepted current plan.
        prompt = "HISTORICAL OBSERVATION [" + outcome + "]: " + clean
        if budget + len(prompt) + 1 > limit:
            continue
        selected.append({"case_id":case.get("id"), "run_id":case["run_id"],
                         "artifact_id":case["evidence_artifact_id"],
                         "raw_model_sha256":case["raw_response_sha256"],
                         "historical_result":outcome, "lesson":clean,
                         "relevance":score})
        used.add(clean.lower())
        budget += len(prompt) + 1
        if len(selected) >= max_cases:
            break
    return {"schema_version":1, "kind":"recorded_lessons_not_model_training",
            "source":"learning_cases.json", "selected":selected,
            "evidence_fields_checked":True,
            "independent_art_approval_inferred":False,
            "prompt_text":"\n".join("HISTORICAL OBSERVATION [" +
                       x["historical_result"] + "]: " + x["lesson"] for x in selected)}


def stage_pending_art_evidence(out_file, *, model: str, source_hash,
                               plan_file, before_image, after_image,
                               critique_file, comparison_file, output_blend, output_glb,
                               raw_response_file=None, screen_gate="REVIEW_REQUIRED",
                               protected_regions_file=None):
    """Emit provenance for future review. Never auto-label it learned/approved."""
    import json
    from pathlib import Path
    if screen_gate not in ("REVIEW_REQUIRED", "SCREENSPACE_REJECTED", "PROTECTED_ACCESS_OCCLUSION_REJECTED"):
        raise ValueError("Unknown bounded screen impact state")
    entries = {}
    for label, file in (("plan", plan_file), ("raw_model_response", raw_response_file),
                        ("before_image", before_image), ("after_image", after_image),
                        ("model_self_critique", critique_file),
                        ("screen_comparison", comparison_file),
                        ("blend", output_blend), ("glb", output_glb)):
        if file is None and label in ("raw_model_response", "model_self_critique"):
            continue
        p = Path(file)
        if not p.is_file() or not p.stat().st_size:
            raise ValueError("Missing genuine artist-cycle input: " + label)
        entries[label] = {"filename":p.name,
                          "sha256":hashlib.sha256(p.read_bytes()).hexdigest(),
                          "size_bytes":p.stat().st_size}
    if protected_regions_file is not None:
        p = Path(protected_regions_file)
        if not p.is_file() or not p.stat().st_size:
            raise ValueError("Protected camera region evidence missing")
        entries["protected_camera_regions"]={"filename":p.name,
            "sha256":hashlib.sha256(p.read_bytes()).hexdigest(),
            "size_bytes":p.stat().st_size}
    if source_hash is not None and not re.fullmatch(r"[a-f0-9]{64}", source_hash):
        raise ValueError("Frozen source identity must be a SHA-256")
    record = {"schema_version":1, "status":"PENDING_INDEPENDENT_VERIFICATION",
              "art_approved":False,"independent_visual_review":False,
              "weight_training_performed":False, "model":model,
              "technical_screen_gate":screen_gate,
              "original_source_sha256":source_hash, "evidence":entries,
              "next_gate":"Verify A/B camera, review model quality independently, then Unity/mobile before promotion"}
    Path(out_file).write_text(json.dumps(record,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    return record
