"""Immutable cost calculation from usage receipts. Never asserts an invoice amount."""
from decimal import Decimal, InvalidOperation
import json
from pathlib import Path
D=Decimal
RATES=json.loads(Path(__file__).with_name("rates-20261125.json").read_text())
def count(v):
    if type(v) is not int or v<0: raise ValueError("Missing or invalid token count")
    return v
def estimate(model, usage, tier="standard"):
    if tier not in ("standard","default"): raise ValueError("Unpriced service tier")
    if not isinstance(usage,dict): raise ValueError("Missing usage")
    i,o=count(usage["input_tokens"]),count(usage["output_tokens"])
    if i>272000: raise ValueError("Long context not priced by this tariff")
    d=usage.get("input_tokens_details") or {}
    if model in RATES["text"]:
        a,c,w30,w12,b=map(D,RATES["text"][model])
        # Never silently assume zero if a returned breakdown is malformed.
        cached=count(d.get("cached_tokens",0))
        writes=count(d.get("cache_write_tokens",0))
        wd=d.get("cache_write_tokens_details") or {}
        long=count(wd.get("ttl_12h_tokens",0))
        short=count(wd.get("ttl_30m_tokens",writes-long))
        if cached+writes>i or long+short!=writes: raise ValueError("Invalid cache breakdown")
        result=(D(i-cached-writes)*a+D(cached)*c+D(short)*w30+D(long)*w12+D(o)*b)/D(1000000)
    elif model in RATES["images"]:
        a,c,b,e,f=map(D,RATES["images"][model])
        t,v=count(d["text_tokens"]),count(d["image_tokens"])
        if t+v!=i: raise ValueError("Invalid image input breakdown")
        ct=count(d.get("cached_text_tokens",0));cv=count(d.get("cached_image_tokens",0))
        if ct>t or cv>v or count(d.get("cached_tokens",ct+cv))!=ct+cv: raise ValueError("Unallocated image cache")
        od=usage.get("output_tokens_details") or {}
        if count(od.get("text_tokens",0))!=0 or count(od.get("image_tokens",o))!=o: raise ValueError("Unsupported image output")
        result=(D(t-ct)*a+D(ct)*c+D(v-cv)*b+D(cv)*e+D(o)*f)/D(1000000)
    else: raise ValueError("Unknown model")
    return result
def receipt(role, model, usage, service_tier="standard", elapsed_seconds=None, utc=None):
    cost=estimate(model,usage,service_tier)
    return {"role":role,"model":model,"usage":usage,"utc":utc,"duration_seconds":elapsed_seconds,
            "estimated_usd":str(cost),"tariff":RATES["version"],"service_tier":service_tier,
            "billed_usd":None,"billing_status":"unreconciled"}
def money(value):
    try: v=D(str(value))
    except (InvalidOperation,ValueError): raise ValueError("Invalid money")
    if not v.is_finite() or v<0: raise ValueError("Invalid money")
    return v
