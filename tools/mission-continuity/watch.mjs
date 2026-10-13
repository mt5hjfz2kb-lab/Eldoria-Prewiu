import fs from "node:fs";
import path from "node:path";
import { classifyMission, issueBody, issueMarker } from "./core.mjs";

const root = process.env.GITHUB_WORKSPACE || process.cwd();
const config = JSON.parse(fs.readFileSync(path.join(root, "pipeline/mission-continuity-policy-v1.json"), "utf8"));
const registry = JSON.parse(fs.readFileSync(path.join(root, "pipeline/active-workstreams.json"), "utf8"));
const repo = process.env.GITHUB_REPOSITORY;
const token = process.env.GITHUB_TOKEN;
const allowIssueWrites = process.env.MISSION_GUARD_WRITE_ISSUES === "true";
const now = new Date().toISOString();
if (!repo || !/^[a-zA-Z0-9_.-]+\/[a-zA-Z0-9_.-]+$/.test(repo)) throw Error("Valid GITHUB_REPOSITORY required");
if (!token) throw Error("GITHUB_TOKEN required for live evidence");
if (config.schema_version !== 1 || !Array.isArray(config.missions) || !Array.isArray(registry.active)) {
  throw Error("Invalid continuity policy or canonical registry");
}
if (config.allow_workflow_dispatch !== false) throw Error("Production dispatch must remain disabled");

async function api(route, opts = {}) {
  const url = "https://api.github.com/repos/" + repo + route;
  const res = await fetch(url, {
    ...opts,
    headers: { Accept: "application/vnd.github+json", Authorization: "Bearer " + token,
      "X-GitHub-Api-Version": "2022-11-28", ...(opts.headers || {}) },
    signal: AbortSignal.timeout(15000)
  });
  const txt = await res.text();
  if (!res.ok) throw Error(opts.method || "GET" + " " + route + " returned " + res.status + ": " + txt.slice(0, 500));
  return txt ? JSON.parse(txt) : {};
}
async function existingIssue(marker) {
  // Bound the scan and fail closed rather than creating duplicate issues if there are too many.
  for (let page = 1; page <= 10; page++) {
    const issues = await api("/issues?state=all&per_page=100&page=" + page);
    const match = issues.find((x) => !x.pull_request && typeof x.body === "string" && x.body.includes(marker));
    if (match) return match;
    if (issues.length < 100) return null;
  }
  throw Error("Issue scan exceeds 1000; refusing possible duplicate issue");
}
async function reconcileIssue(report) {
  const marker = issueMarker(report.id);
  const found = await existingIssue(marker);
  if (found && found.state === "closed") return "owner_closed_recovery_issue";
  if (!found && !report.alert) return "not_stalled";
  const body = issueBody(report);
  if (!found) {
    const created = await api("/issues", { method: "POST",
      body: JSON.stringify({ title: "[Mission continuity] " + report.id, body }) });
    return "created_issue_" + created.number;
  }
  if (found.body === body) return "unchanged_issue_" + found.number;
  await api("/issues/" + found.number, { method: "PATCH", body: JSON.stringify({ body }) });
  return "updated_issue_" + found.number;
}
const reports = [];
const failures = [];
for (const mission of config.missions) {
  try {
    if (!mission.workstream_id || !mission.branch || !Array.isArray(mission.workflow_names)) {
      throw Error("Policy mission requires id, branch and workflow names");
    }
    const branch = await api("/branches/" + mission.branch.split("/").map(encodeURIComponent).join("/"));
    const runPage = await api("/actions/runs?branch=" + encodeURIComponent(mission.branch) + "&per_page=100");
    const report = classifyMission({
      mission, activeWorkstreams: registry.active, runs: runPage.workflow_runs,
      branchSha: branch.commit.sha, now, stallMinutes: config.stall_minutes
    });
    report.issue_sync = allowIssueWrites ? await reconcileIssue(report) : "dry_run";
    reports.push(report);
  } catch (error) {
    const message = mission.workstream_id + ": " + error.message;
    failures.push(message);
    reports.push({ id: mission.workstream_id, state: "MONITOR_ERROR", error: message, alert: false });
  }
}
const output = { schema_version: 1, inspected_at: now, branch: process.env.GITHUB_REF_NAME || "unknown",
  message: "External jobs and chats are distinct. This tool never certifies artistic success or dispatches production.",
  reports, errors: failures };
const outputPath = process.env.MISSION_GUARD_OUTPUT || path.join(root, "mission-continuity-report.json");
fs.writeFileSync(outputPath, JSON.stringify(output, null, 2) + "\n");
const markdown = ["## Eldoria mission continuity",
  "This is a recovery monitor, not a build, artistic approval, or autonomous chat.",
  "",
  "| Workstream | State | Last run | Age (min) | Issue sync |",
  "| --- | --- | --- | --- | --- |"];
for (const item of reports) markdown.push("| " + item.id + " | " + item.state + " | " +
  (item.latest_run ? String(item.latest_run.id) : "—") + " | " +
  (item.age_minutes ?? "—") + " | " + (item.issue_sync || "—") + " |");
if (failures.length) markdown.push("", "### Monitoring failures", ...failures.map((x) => "- " + x));
if (process.env.GITHUB_STEP_SUMMARY) fs.appendFileSync(process.env.GITHUB_STEP_SUMMARY, markdown.join("\n") + "\n");
console.log(JSON.stringify(output, null, 2));
if (failures.length) process.exitCode = 1;
