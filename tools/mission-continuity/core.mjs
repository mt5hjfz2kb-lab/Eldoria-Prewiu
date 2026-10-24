// Pure continuity decisions. No API calls, runner dispatches or file mutations.
export function classifyMission({ mission, activeWorkstreams, runs, branchSha, now, stallMinutes }) {
  const owner = (activeWorkstreams || []).find((w) => w.id === mission.workstream_id && w.status === "active");
  if (!owner || owner.branch !== mission.branch) {
    return { id: mission.workstream_id, state: "OWNER_NOT_ACTIVE", alert: false,
      latest_run: null, branch_sha: branchSha || null,
      next_action: "Reconcile the canonical active-workstreams registry; do not claim or dispatch another owner's work.",
      required_evidence: mission.required_evidence || [] };
  }
  // Registry entries explicitly paused by their owner may still be in "active".
  // Never create a recovery issue, dispatch recommendation or misleading alert
  // while the owner has deliberately parked UI/Valoria for another workstream.
  const paused = /^OWNER_PAUSED_/i.test(owner.phase || '') ||
    /^PAUSED BY OWNER\b/i.test(owner.next || '');
  if (paused) return {id:mission.workstream_id,state:'OWNER_PAUSED',alert:false,
    branch:mission.branch,branch_sha:branchSha || null,latest_run:null,
    next_action:'Owner pause in canonical registry. Preserve evidence and ownership; do not retry, dispatch or treat this as idle failure.',
    required_evidence:mission.required_evidence || []};
  // An unassigned "active" entry is not proof of a live executor.
  if (Object.hasOwn(owner,'owner') && !owner.owner) return {
    id:mission.workstream_id,state:'OWNER_UNASSIGNED',alert:false,
    branch:mission.branch,branch_sha:branchSha || null,latest_run:null,
    next_action:'Registry has no assigned owner. Verify scope and authorized handoff before any further production step.',
    required_evidence:mission.required_evidence || []};
  const allowed = new Set(mission.workflow_names || []);
  const relevant = (runs || []).filter((r) =>
    r.head_branch === mission.branch && allowed.has(r.name));
  relevant.sort((a, b) => Date.parse(b.created_at || 0) - Date.parse(a.created_at || 0));
  const running = relevant.find((r) => ["in_progress", "queued", "waiting", "requested", "pending"].includes(r.status));
  const latest = running || relevant[0] || null;
  let state;
  let nextAction;
  if (!latest) {
    state = "IDLE_UNFINISHED";
    nextAction = "No matching run found. The owning chat must inspect the branch and execute the next permitted production gate.";
  } else if (running) {
    state = "EXTERNAL_RUNNING";
    nextAction = "Watch the actual GitHub job. On completion consume its artifacts and execute the next gate; do not call the mission finished.";
  } else if (latest.conclusion === "success") {
    // Only compare full commit hashes. Older/noncanonical test fixtures cannot
    // accidentally trigger branch drift, and old green runs never certify HEAD.
    const validSha = /^[a-f0-9]{40}$/i;
    const drift = validSha.test(branchSha || '') && validSha.test(latest.head_sha || '') &&
      branchSha.toLowerCase() !== latest.head_sha.toLowerCase();
    state = drift ? "SOURCE_DRIFT_REVALIDATION" : "WAITING_EVIDENCE";
    nextAction = drift ?
      "A previous SHA passed, not the current branch HEAD. Inspect exact diff and run only the affected authorized verification gates; do not reuse old success as acceptance." :
      "Inspect the successful run's genuine artifacts and visual/functional gates, then perform the next unfinished phase. CI success is not artistic approval.";
  } else {
    state = "NEEDS_RECOVERY";
    nextAction = "Inspect failed/cancelled run logs; correct or choose an authorized bounded alternative, then rerun only the affected gate.";
  }
  const observedTime = latest ? Date.parse(latest.updated_at || latest.created_at) : NaN;
  const ageMinutes = Number.isFinite(observedTime) ? Math.max(0, (Date.parse(now) - observedTime) / 60000) : null;
  const stale = ageMinutes === null ? true : ageMinutes >= stallMinutes;
  return {
    id: mission.workstream_id, state, alert: state !== "EXTERNAL_RUNNING" && stale,
    branch: mission.branch, branch_sha: branchSha || null,
    latest_run: latest ? { id: latest.id, status: latest.status, conclusion: latest.conclusion || null,
      url: latest.html_url, updated_at: latest.updated_at, head_sha: latest.head_sha } : null,
    age_minutes: ageMinutes === null ? null : Math.floor(ageMinutes),
    next_action: nextAction + " " + (mission.resume_hint || ""),
    required_evidence: mission.required_evidence || []
  };
}
export function issueMarker(id) {
  if (!/^[a-z0-9_-]+$/i.test(id)) throw Error("Invalid mission id");
  return "<!-- eldoria-mission-continuity:" + id + " -->";
}
export function issueBody(report) {
  const run = report.latest_run;
  const lines = [
    issueMarker(report.id), "# Eldoria mission continuity — not a closure or release gate", "",
    "**Registry mission:** " + report.id,
    "**State:** " + report.state,
    "**Branch:** " + (report.branch || "none"),
    "**Observed HEAD:** " + (report.branch_sha || "unknown"),
    "**Last matching run:** " + (run ? ("[" + run.id + "](" + run.url + ") — " +
      run.status + " / " + (run.conclusion || "pending") + " at " + run.updated_at) : "none"),
    "", "## Next permitted action", report.next_action, "",
    "## Evidence required before the owner may claim completion"
  ];
  for (const evidence of report.required_evidence) lines.push("- " + evidence);
  lines.push("", "This issue is an **automatic recovery pointer**, NOT a second workstream, a decision-maker, or a completion certificate.",
    "Do not close the project mission based on this issue, a green workflow, pixel-change percentage, or a screenshot alone.");
  return lines.join("\n") + "\n";
}
