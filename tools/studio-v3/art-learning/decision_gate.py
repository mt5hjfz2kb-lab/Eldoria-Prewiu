"""Decision arbitration separates a model's recommendation from production eligibility.
No model or score can waive the owner's approved resource or a protected feature regression."""
from guard import need
PROTECTED=("identity","function","landmarks","access","integration","mobile_readability")
def arbitrate(p,baseline,candidate,owner_approved=None):
    need({baseline,candidate}=={"X","Y"},"Invalid blind mapping")
    if owner_approved is not None:
        need(owner_approved in ("X","Y"),"Invalid owner selection")
        return {"decision":"preserve_owner_selection","selected":owner_approved,"promote":False,"owner_lock":True}
    need(p.get("preferred") in ("X","Y","tie"),"Missing judge choice")
    features=p.get("protected_features")
    need(isinstance(features,dict) and set(features)==set(PROTECTED),"Incomplete protected feature review")
    blockers=[];uncertain=[]
    for feature,x in features.items():
        need(isinstance(x,dict) and set(x)=={"X","Y","evidence"} and isinstance(x["evidence"],str) and x["evidence"].strip(),"Unbound feature comparison")
        for label in ("X","Y"):need(x[label] in ("good","minor_defect","material_defect","uncertain"),"Invalid severity")
        scale={"good":0,"minor_defect":1,"material_defect":2}
        if "uncertain" in (x[baseline],x[candidate]):uncertain.append(feature)
        elif x[candidate]=="material_defect" and scale[x[candidate]]>scale[x[baseline]]:blockers.append(feature)
    if blockers:return {"decision":"reject_regression","selected":baseline,"blockers":blockers,"promote":False}
    if uncertain:return {"decision":"insufficient_evidence","selected":baseline,"uncertain":uncertain,"promote":False}
    if p["preferred"]!=candidate:return {"decision":"retain_original","selected":baseline,"promote":False}
    return {"decision":"eligible_for_native_score_gate","selected":candidate,"promote":False}
