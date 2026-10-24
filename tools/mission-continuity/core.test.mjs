import test from "node:test";
import assert from "node:assert/strict";
import { classifyMission, issueBody, issueMarker } from "./core.mjs";
const mission = {
  workstream_id: "valoria-bastion1-player-scene-integration-20261012",
  branch: "integration/valoria-bastion1-player-scene-20261012",
  workflow_names: ["Qwen art proposal", "Native Unity review"],
  required_evidence: ["Real Unity same-camera A/B", "Independent art review"],
  resume_hint: "Continue on original branch."
};
const activeWorkstreams = [{ id: mission.workstream_id, branch: mission.branch, status: "active" }];
const now = "2026-10-14T02:00:00Z";
const makeRun = (id, status, conclusion, updatedAt = "2026-10-14T00:40:00Z",
  name = "Qwen art proposal", branch = mission.branch) => ({
    id, status, conclusion, name, head_branch: branch,
    created_at: "2026-10-14T00:20:00Z", updated_at: updatedAt,
    html_url: "https://github.com/example/repo/actions/runs/" + id, head_sha: "deadbeef"
  });
function check(runs, extra = {}) {
  return classifyMission({
    mission, activeWorkstreams, runs, branchSha: "12345",
    now, stallMinutes: 20, ...extra
  });
}
test("owner must still be active; a completed CI run cannot create a mission", () => {
  const x = check([makeRun(1, "completed", "success")], { activeWorkstreams: [] });
  assert.equal(x.state, "OWNER_NOT_ACTIVE");
  assert.equal(x.alert, false);
});
test("ongoing external run does not open a stalled issue", () => {
  const x = check([makeRun(5, "in_progress", null)]);
  assert.equal(x.state, "EXTERNAL_RUNNING");
  assert.equal(x.alert, false);
});
test("successful technical run is not classified as artistic completion", () => {
  const x = check([makeRun(5, "completed", "success")]);
  assert.equal(x.state, "WAITING_EVIDENCE");
  assert.equal(x.alert, true);
  assert.match(x.next_action, /not artistic approval/);
  assert.deepEqual(x.required_evidence, mission.required_evidence);
});
test("failed run points to recovery without automatic expensive retries", () => {
  const x = check([makeRun(6, "completed", "failure")]);
  assert.equal(x.state, "NEEDS_RECOVERY");
  assert.equal(x.alert, true);
  assert.match(x.next_action, /Inspect failed/);
});
test("cancelled run is not treated as success", () => {
  assert.equal(check([makeRun(6, "completed", "cancelled")]).state, "NEEDS_RECOVERY");
});
test("recent completion stays non-alerting during short grace window", () => {
  const x = check([makeRun(6, "completed", "success", "2026-10-14T01:55:00Z")]);
  assert.equal(x.state, "WAITING_EVIDENCE");
  assert.equal(x.alert, false);
});
test("running run supersedes an older failing run", () => {
  const x = check([makeRun(6, "completed", "failure"),
    makeRun(7, "queued", null, "2026-10-14T01:50:00Z")]);
  assert.equal(x.state, "EXTERNAL_RUNNING");
  assert.equal(x.latest_run.id, 7);
});
test("ignore unrelated branches and names, preserve missing-run recovery", () => {
  const x = check([makeRun(8, "completed", "success", now, "Other flow"),
    makeRun(9, "completed", "success", now, "Qwen art proposal", "main")]);
  assert.equal(x.state, "IDLE_UNFINISHED");
  assert.equal(x.alert, true);
});
test("issue handoff contains stable marker, run and explicit evidence", () => {
  const x = check([makeRun(12, "completed", "success")]);
  const body = issueBody(x);
  assert.ok(body.includes(issueMarker(mission.workstream_id)));
  assert.match(body, /12/);
  assert.match(body, /Real Unity same-camera A\/B/);
  assert.match(body, /NOT a second workstream/);
});
test("reject unsafe mission identifiers", () => {
  assert.throws(() => issueMarker("invalid -->"), /Invalid mission/);
});

test("explicit owner pause never creates an idle-unfinished alert or asks for execution",()=>{
 const paused={...activeWorkstreams[0],owner:null,phase:'OWNER_PAUSED_WAIT_FOR_WORLD4X_PR100',
   next:'PAUSED BY OWNER'};
 const x=check([makeRun(15,'completed','failure')],{activeWorkstreams:[paused]});
 assert.equal(x.state,'OWNER_PAUSED');
 assert.equal(x.alert,false);
 assert.equal(x.latest_run,null);
 assert.match(x.next_action,/do not retry/i);
});
test("unassigned canonical slot does not masquerade as an active executor",()=>{
 const vacant={...activeWorkstreams[0],owner:null,phase:'READY_FOR_CLAIM'};
 const x=check([],{activeWorkstreams:[vacant]});
 assert.equal(x.state,'OWNER_UNASSIGNED');
 assert.equal(x.alert,false);
});
test("only exact full commit SHA can invalidate a previous successful run",()=>{
 const old='a'.repeat(40),current='b'.repeat(40);
 const r={...makeRun(42,'completed','success'),head_sha:old};
 const x=check([r],{branchSha:current});
 assert.equal(x.state,'SOURCE_DRIFT_REVALIDATION');
 assert.equal(x.alert,true);
 assert.match(x.next_action,/previous SHA passed/i);
 const same=check([r],{branchSha:old});
 assert.equal(same.state,'WAITING_EVIDENCE');
 assert.equal(same.alert,true);
});
