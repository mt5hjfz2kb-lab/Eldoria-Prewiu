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

test("Region1 PR workstream observes candidate branch while retaining independent owner on main",()=>{
  const current={...mission,workstream_id:'eldoria-world-region-1-canon-20261022',
    owner_branch:'main',branch:'feature/world4x-region1-canon-20261022',
    workflow_names:['World4X Region 1 canonical candidate source and PlayMode QA'],
    require_owner:true,exact_head_only:true,report_only:true};
  const works=[{id:current.workstream_id,status:'active',branch:'main',
    owner:'codex-world4x',ownership_state:'claimed',resources:['windows-runner-heavy']}];
  const runs=[
    {id:19,name:current.workflow_names[0],head_branch:current.branch,head_sha:'old',
      status:'completed',conclusion:'failure',created_at:'2026-10-26T10:01:00Z',updated_at:'2026-10-26T10:02:00Z'},
    {id:20,name:current.workflow_names[0],head_branch:current.branch,head_sha:'candidate',
      status:'completed',conclusion:'success',created_at:'2026-10-26T10:03:00Z',updated_at:'2026-10-26T10:04:00Z'}
  ];
  const x=classifyMission({mission:current,activeWorkstreams:works,runs,
    branchSha:'candidate',now:'2026-10-26T11:00:00Z',stallMinutes:20});
  assert.equal(x.state,'WAITING_EVIDENCE');
  assert.equal(x.alert,false);
  assert.equal(x.observation_mode,'report_only_no_issues');
  assert.equal(x.latest_run.id,20);
  assert.equal(x.owner_branch,'main');
});
test("a current workstream cannot be inferred from a null owner or stolen branch",()=>{
  const current={...mission,owner_branch:'main',require_owner:true,report_only:true};
  let x=check([makeRun(1,'completed','success')],{mission:current,
    activeWorkstreams:[{id:current.workstream_id,status:'active',branch:'main',
      ownership_state:'available',owner:null}]});
  assert.equal(x.state,'OWNER_NOT_ACTIVE');
  assert.equal(x.alert,false);
  x=check([makeRun(1,'completed','success')],{mission:current,
    activeWorkstreams:[{id:current.workstream_id,status:'active',branch:'elsewhere',
      owner:'other',ownership_state:'claimed'}]});
  assert.equal(x.state,'OWNER_NOT_ACTIVE');
});
test("explicit pause overrides successful or queued CI and never generates recovery issue",()=>{
  const m={...mission,owner_branch:'main',report_only:true};
  const x=check([makeRun(4,'in_progress',null)],{mission:m,
    activeWorkstreams:[{id:mission.workstream_id,status:'active',branch:'main',
      owner:'owner',phase:'OWNER_PAUSED_WAIT_FOR_WORLD4X_PR100'}]});
  assert.equal(x.state,'OWNER_PAUSED');
  assert.equal(x.alert,false);
});
test("GitHub action_required is not mistaken for broken Unity or visual approval",()=>{
  const x=check([makeRun(8,'completed','action_required')],{mission:{...mission,report_only:true},branchSha:'deadbeef'});
  assert.equal(x.state,'GITHUB_APPROVAL_REQUIRED');
  assert.equal(x.alert,false);
  assert.match(x.next_action,/never bypass authorization/);
});
test("superseded queued run does not override a newer completed success",()=>{
  const old={...makeRun(5,'queued',null),created_at:'2026-10-14T00:20:00Z'};
  const fresh={...makeRun(6,'completed','success'),created_at:'2026-10-14T00:30:00Z'};
  const x=check([old,fresh]);
  assert.equal(x.latest_run.id,6);
  assert.equal(x.state,'WAITING_EVIDENCE');
});

test("Tripo workflow evidence from earlier main commit is historical, never a current ART PASS",()=>{
  const m={...mission,report_only:true,require_owner:true,exact_head_only:false};
  const w=[{id:mission.workstream_id,branch:mission.branch,status:'active',
    owner:'artist',ownership_state:'claimed'}];
  const r=check([makeRun(22,'completed','success')],{mission:m,activeWorkstreams:w,branchSha:'NEW_HEAD'});
  assert.equal(r.state,'STALE_SOURCE_EVIDENCE');
  assert.equal(r.alert,false);
  assert.equal(r.source_matches_branch_head,false);
  assert.match(r.next_action,/OLDER branch source/);
});
test("an older-source active art job remains observable but cannot certify current main",()=>{
  const m={...mission,report_only:true,exact_head_only:false};
  const r=check([makeRun(23,'in_progress',null)],{mission:m,branchSha:'NEW_HEAD'});
  assert.equal(r.state,'EXTERNAL_RUNNING');
  assert.equal(r.source_matches_branch_head,false);
  assert.match(r.next_action,/older source SHA/);
});
