// Permanent, tool-independent boundary: WIP art proofs MUST NEVER confer an
// approved production state, even if their structure validates successfully.
export const ART_APPROVAL_STATES = Object.freeze([
  'ART_APPROVED', 'MOBILE_VALIDATED', 'GAMEPLAY_VALIDATED', 'RELEASED'
]);
export function requireMatchingApprovedCycle(cycle, request, workstreamId) {
  const state=String(request?.visual_gate_state||'').toUpperCase();
  if (!ART_APPROVAL_STATES.includes(state))
    throw new Error('Production state is not an approved artist state: '+state);
  if (!cycle || !ART_APPROVAL_STATES.includes(cycle.state) || cycle.state!==state)
    throw new Error('Approved request '+state+' requires matching approved artist cycle, got '+String(cycle?.state));
  if (cycle.workstream_id!==workstreamId || request.workstream_id!==workstreamId)
    throw new Error('Artist cycle belongs to a different workstream');
  if (cycle.asset_id===undefined || typeof cycle.asset_id!=='string' || !cycle.asset_id.trim())
    throw new Error('Artist cycle missing asset identity');
  return true;
}
