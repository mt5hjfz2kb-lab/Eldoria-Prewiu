"""Do not consume native resources on identical or materially regressive candidates.
A minor defect is not an art approval, but it is not a material veto either."""
import re
def classify_asset(original_sha,candidate_sha,material_regressions,minor_issues,original_score,candidate_score):
    if not all(isinstance(s,str) and re.fullmatch("[a-f0-9]{64}",s) for s in (original_sha,candidate_sha)):
        raise ValueError("Unverified asset digest")
    if not all(isinstance(a,list) and all(isinstance(x,str) and x.strip() for x in a) for a in (material_regressions,minor_issues)):
        raise ValueError("Missing severity-typed review")
    if not all(type(v) in (float,int) and 0<=v<=10 for v in (original_score,candidate_score)):
        raise ValueError("Invalid comparative scores")
    if original_sha==candidate_sha:return {"decision":"skip_native_identical","native":False,"promote":False}
    if material_regressions:return {"decision":"retain_original_material_veto","native":False,"promote":False}
    if candidate_score<original_score:return {"decision":"retain_original_lower_score","native":False,"promote":False}
    return {"decision":"diagnostic_native_candidate","native":True,"promote":False,"minor_issues":minor_issues}
def native_scope(assets):
    if not isinstance(assets,dict) or not assets:raise ValueError("Missing verified assets")
    selected=[k for k,v in assets.items() if v["native"] is True]
    return {"native_eligible":bool(selected),"targets":selected,"promote":False}
