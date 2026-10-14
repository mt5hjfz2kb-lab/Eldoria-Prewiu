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
