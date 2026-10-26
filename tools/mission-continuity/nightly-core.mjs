// Conservative, pure eligibility gates for unattended GitHub Actions retries.
// A chat/agent's lost connection cannot be detected reliably from Actions.
export const TRANSIENT_INFRA = /(?:HTTP\s*(?:502|503|504)\b|(?:ECONNRESET|ETIMEDOUT|EAI_AGAIN|ENOTFOUND)\b|runner (?:lost communication|was removed|disconnected unexpectedly)|GitHub (?:Actions|API) (?:temporarily unavailable|service unavailable))/i;
export function localNight(now, zone = 'Europe/Madrid', start = 22, end = 8) {
  const hour = Number(new Intl.DateTimeFormat('en-GB', { timeZone: zone, hour: '2-digit', hourCycle: 'h23' }).format(new Date(now)));
  return start > end ? hour >= start || hour < end : hour >= start && hour < end;
}
// The monitor's own Actions run is not an external job competing for Windows.
export function otherActiveRuns(pages, ownRunId) {
  if (!Array.isArray(pages) || pages.length !== 5 || !ownRunId) return true;
  return pages.some(page => {
    if (!Array.isArray(page?.workflow_runs) || !Number.isInteger(page?.total_count)) return true;
    if (page.workflow_runs.length >= 100 || page.total_count > page.workflow_runs.length) return true;
    const other = page.workflow_runs.filter(run => String(run.id) !== String(ownRunId));
    return other.length > 0;
  });
}
export function checkRetry({ policy, track, owner, latest, branchSha, globalBusy, failedJobs, log, now }) {
  if (!policy.enabled || !track.retry_enabled) return 'RETRY_DISABLED';
  if (!localNight(now, policy.zone, policy.window_start_hour, policy.window_end_hour)) return 'NOT_NIGHT';
  const ownerBranch=track.owner_branch||track.branch;
  if (track.requires_owner && (!owner || owner.status !== 'active' || owner.branch !== ownerBranch)) return 'OWNER_NOT_ACTIVE';
  if (track.requires_owner && track.require_claimed_owner &&
      (!owner.owner || owner.ownership_state !== 'claimed')) return 'OWNER_NOT_CLAIMED';
  if (/^OWNER_PAUSED_/i.test(owner?.phase||'') ||
      /^PAUSED BY OWNER\\b/i.test(owner?.next||'')) return 'OWNER_PAUSED';
  if (track.requires_owner && track.runner_type === 'windows') {
    const resource = track.owner_resource || 'windows-runner-heavy';
    if (!owner.resources?.includes(resource)) return 'WINDOWS_NOT_CLAIMED';
    // The existing Unity lease is never transferred. This second branch has
    // explicit owner authorization for retries ONLY if the runner is wholly idle.
    if (resource !== 'windows-runner-heavy' && track.windows_delegation !== 'idle_only_no_lease_transfer') {
      return 'NO_IDLE_WINDOWS_DELEGATION';
    }
  }
  if (!latest || !track.workflow_names.includes(latest.name)) return 'NO_ALLOWLISTED_RUN';
  if (latest.status !== 'completed' || latest.conclusion !== 'failure') return 'NOT_FAILED';
  if (!['push', 'workflow_dispatch'].includes(latest.event)) return 'UNAUTHORIZED_EVENT';
  if ((latest.run_attempt || 1) > policy.maximum_run_attempts) return 'RETRY_LIMIT';
  if (latest.head_sha !== branchSha) return 'STALE_SOURCE';
  const age = (Date.parse(now) - Date.parse(latest.updated_at)) / 60000;
  if (!Number.isFinite(age) || age < policy.minimum_idle_minutes) return 'GRACE_PERIOD';
  if (globalBusy) return 'RUNNER_OR_REPO_BUSY';
  if (failedJobs.length !== 1 || !failedJobs[0]?.id) return 'NOT_EXACTLY_ONE_FAILED_JOB';
  if (!TRANSIENT_INFRA.test(log || '')) return 'NOT_TRANSIENT_INFRA_FAILURE';
  // Treat source/script/asset failures as definitive, regardless of log also mentioning connectivity.
  if (/(?:error CS\d{4}|Traceback \(most recent call last\)|SyntaxError:|AssertionError:|No such file or directory|ART_APPROVED|quality threshold|Missing output:|shader compilation error)/i.test(log)) return 'SOURCE_OR_ART_ERROR';
  return 'ELIGIBLE';
}
