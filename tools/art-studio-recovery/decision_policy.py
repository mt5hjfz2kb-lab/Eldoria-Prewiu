"""Fail closed on contradictory art decisions; model output never grants ART PASS."""
def semantic_conflicts(decision):
    conflicts=[]
    verdict=decision.get('decision');action=decision.get('next_action')
    if verdict not in ('retain_candidate','reject_candidate'):
        conflicts.append('missing_or_invalid_verdict')
    if action not in ('keep_for_unity_test','bury_seams','change_donor','retain_baseline'):
        conflicts.append('missing_or_invalid_action')
    if verdict=='retain_candidate' and action in ('change_donor','retain_baseline'):
        conflicts.append('retain_candidate_conflicts_with_replacing_it')
    if verdict=='reject_candidate' and action=='keep_for_unity_test':
        conflicts.append('reject_candidate_conflicts_with_retaining_it')
    if decision.get('reference_quality_met') is True and action in ('change_donor','retain_baseline'):
        conflicts.append('reference_quality_claim_conflicts_with_replacement')
    return conflicts
