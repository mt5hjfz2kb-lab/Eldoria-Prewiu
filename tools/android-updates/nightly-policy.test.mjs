import test from 'node:test';
import assert from 'node:assert/strict';
import { decideNightly, MIN_QUIET_MINUTES } from './nightly-policy.mjs';
import { propose } from './check-channel.mjs';

const earlier = 'a'.repeat(40), head = 'b'.repeat(40), previousTree = 'c'.repeat(40),
  currentTree = 'd'.repeat(40);
const channel = propose({ versionCode: 10, versionName: '1.10', sourceSha: earlier,
  sha256: 'e'.repeat(64), bytes: 247491508 });
function payload(change = {}) {
  return { now: '2026-10-21T02:17:00.000Z',
    latestUnityCommitAt: '2026-10-20T23:00:00.000Z',
    headSha: head, releasedUnityTree: previousTree, currentUnityTree: currentTree,
    channel, canonicalOwner: true, gameQaVerified: true, ...change };
}
test('One approved nightly release has monotonic Android code 11 from channel10', () => {
  const d=decideNightly(payload());
  assert.equal(d.eligible, true);
  assert.equal(d.shouldBuild, true);
  assert.equal(d.code, 11);
  assert.equal(d.version, '1.11');
  assert.equal(d.sourceSha, head);
  assert.ok(d.quietMinutes > MIN_QUIET_MINUTES);
});
test('No changes in integrated Unity tree means no nightly build despite docs or automation commits', () => {
  assert.equal(decideNightly(payload({currentUnityTree:previousTree})).reason,'no_new_unity_source');
  assert.equal(decideNightly(payload({headSha:earlier})).reason,'no_new_unity_source');
});
test('Block unverified or disabled channels; never create an APK from a bogus signature', () => {
  assert.equal(decideNightly(payload({channel:{...channel,enabled:false}})).reason,'published_channel_not_verified');
  assert.equal(decideNightly(payload({channel:{...channel,certificateSha256:'f'.repeat(64)}})).reason,'published_channel_not_verified');
});
test('Fail closed on unfinished/manual release, foreign runner leases and busy Windows native work', () => {
  for(const [key,value,why] of [
    ['manualBuildEnabled',true,'manual_android_operation_active'],
    ['manualReleaseEnabled',true,'manual_android_operation_active'],
    ['canonicalOwner',false,'workstream_owner_or_runner_conflict'],
    ['conflictingLease',true,'workstream_owner_or_runner_conflict'],
    ['windowsBusy',true,'native_windows_work_in_progress'] ])
    assert.equal(decideNightly(payload({[key]:value})).reason,why,key);
});
test('No green QA matching the exact Unity tree blocks publication',()=>{
  assert.equal(decideNightly(payload({gameQaVerified:false})).reason,'no_matching_successful_gameplay_qa');
  assert.equal(decideNightly(payload({currentUnityTree:''})).reason,'unity_tree_not_available');
});
test('Source must be quiet for 45 minutes before producing a single nightly APK',()=>{
  assert.equal(decideNightly(payload({latestUnityCommitAt:'2026-10-21T02:00:00.000Z'})).reason,'source_still_changing');
  assert.equal(decideNightly(payload({latestUnityCommitAt:'2026-10-21T02:17:01.000Z'})).reason,'source_still_changing');
});
test('Dry-run reports eligibility but never consumes Windows or publishes',()=>{
  const d=decideNightly(payload({dryRun:true}));
  assert.equal(d.eligible,true);
  assert.equal(d.shouldBuild,false);
  assert.equal(d.reason,'dry_run_eligible');
});
test('Version exhaustion and malformed SHA fail closed',()=>{
  assert.equal(decideNightly(payload({headSha:'xyz'})).reason,'invalid_main_source_sha');
  const max=propose({versionCode:999999,versionName:'1.999999',sourceSha:earlier,
    sha256:'e'.repeat(64),bytes:200000000});
  assert.equal(decideNightly(payload({channel:max})).reason,'android_version_limit');
});

test('Explicit nightly kill switch is nonpublishing even when content is green',()=>{
  const d=decideNightly(payload({enabled:false}));
  assert.equal(d.shouldBuild,false);
  assert.equal(d.reason,'nightly_disabled');
});
test('An already verified revision is never repackaged because automation metadata changed',()=>{
  const sameHead=decideNightly(payload({headSha:channel.sourceSha}));
  assert.equal(sameHead.shouldBuild,false);
  assert.equal(sameHead.reason,'no_new_unity_source');
});
