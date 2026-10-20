import { validChannel } from './check-channel.mjs';

export const MIN_QUIET_MINUTES = 45;
export const MAX_QUIET_MINUTES = 60 * 24 * 90;
export const CANONICAL_OWNER = 'valoria-master-plan-layered-construction-20261016';

// Pure release decision. GitHub API lookup and Windows Unity run live in Actions,
// never here. An inconclusive gate fails closed and preserves the installed APK.
export function decideNightly({
  now, latestUnityCommitAt, headSha, releasedUnityTree, currentUnityTree,
  channel, manualBuildEnabled = false, manualReleaseEnabled = false,
  canonicalOwner = false, conflictingLease = false, windowsBusy = false,
  gameQaVerified = false, enabled = true, dryRun = false
}) {
  const skip = (reason) => ({ eligible: false, shouldBuild: false, reason });
  if (!enabled) return skip('nightly_disabled');
  if (!channel || channel.enabled !== true || !validChannel(channel, 0))
    return skip('published_channel_not_verified');
  if (manualBuildEnabled || manualReleaseEnabled) return skip('manual_android_operation_active');
  if (!canonicalOwner || conflictingLease) return skip('workstream_owner_or_runner_conflict');
  if (!/^[0-9a-f]{40}$/.test(headSha || '')) return skip('invalid_main_source_sha');
  if (!/^[0-9a-f]{40}$/.test(channel.sourceSha || '')) return skip('invalid_channel_source_sha');
  if (!/^[0-9a-f]{40}$/.test(currentUnityTree || '') ||
      !/^[0-9a-f]{40}$/.test(releasedUnityTree || ''))
    return skip('unity_tree_not_available');
  if (headSha === channel.sourceSha || currentUnityTree === releasedUnityTree)
    return skip('no_new_unity_source');
  const nowMs = new Date(now).getTime();
  const lastMs = new Date(latestUnityCommitAt).getTime();
  if (!Number.isFinite(nowMs) || !Number.isFinite(lastMs))
    return skip('unity_change_time_unavailable');
  const quietMinutes = (nowMs - lastMs) / 60000;
  if (quietMinutes < MIN_QUIET_MINUTES) return skip('source_still_changing');
  if (quietMinutes > MAX_QUIET_MINUTES) return skip('stale_unity_source_time');
  if (windowsBusy) return skip('native_windows_work_in_progress');
  if (!gameQaVerified) return skip('no_matching_successful_gameplay_qa');
  if (!Number.isInteger(channel.versionCode) || channel.versionCode >= 999999)
    return skip('android_version_limit');
  const code = channel.versionCode + 1;
  const plan = { eligible: true, shouldBuild: !dryRun, reason: dryRun ? 'dry_run_eligible' : 'eligible',
    code, version: '1.' + code, sourceSha: headSha, unityTree: currentUnityTree,
    priorVersion: channel.versionCode, quietMinutes: Math.floor(quietMinutes) };
  return plan;
}
