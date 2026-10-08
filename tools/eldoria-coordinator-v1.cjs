#!/usr/bin/env node
// Eldoria coordinator v1 — read-only, fail-closed triage. Never dispatches agents, QA or releases.
const fs = require('node:fs');
const path = require('node:path');
const registry = JSON.parse(fs.readFileSync('pipeline/active-workstreams.json', 'utf8'));
const m07id = 'm07-r1-sawmill-construction-visual-correction';
const active = registry.active || [];
const history = registry.history || [];
const m07 = active.find(x => x.id === m07id) || history.find(x => x.id === m07id);
const checks = [];
const requireGate = (key, ok, reason) => checks.push({key, passed: Boolean(ok), reason});
requireGate('M07_RECORD', !!m07, 'Canonical workstream record must exist');
requireGate('M07_CLOSED', !!m07 && m07.status === 'completed' && !active.some(x => x.id === m07id), 'M07 must be completed and absent from active registry');
requireGate('RUNNER_RELEASED', !!m07 && m07.runner_released === true, 'Shared resources must be released');
// Evidence must be explicitly certified, not inferred from green CI, code or filenames.
const result = m07 && m07.result || {};
requireGate('POST_PATCH_UNITY_CAPTURE', result.post_patch_capture_reviewed === true && !!result.post_patch_capture_artifact_id && !!result.post_patch_capture_source_sha, 'Reviewed Unity captures linked to exact correction SHA required');
requireGate('INDEPENDENT_VISUAL_PASS', result.independent_visual_pass === true && !!result.visual_review_evidence, 'Independent visual review required; technical green is insufficient');
requireGate('WEBGL_QA_RELEVANT_PASS', result.published_webgl_verified === true && !!result.published_probe_run_id && !!result.published_source_sha, 'Exact published build and QA-relevant checks required');
const verdict = checks.every(c => c.passed) ? 'READY_FOR_QA_HANDOFF_REVIEW' : 'BLOCKED_NO_HANDOFF';
const report = {schema_version: 1,generated_at: new Date().toISOString(),workstream: m07id,verdict,checks,notes:['Read-only assessment. READY is not QA execution or QA approval.','A separate authorized owner and executor are required to start QA.','No chat, Work task, PR, workflow or paid resource is triggered by this script.']};
const output = JSON.stringify(report,null,2)+'\n';
fs.mkdirSync('pipeline/reports',{recursive:true});
fs.writeFileSync(path.join('pipeline/reports','coordinator-v1-latest.json'),output);
console.log(output);
