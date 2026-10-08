#!/usr/bin/env node
// Eldoria coordinator v1 — read-only, fail-closed triage. Never dispatches agents, QA or releases.
const fs = require('node:fs');
const path = require('node:path');

// Optional independent local pilot evidence gate; default M07 behavior unchanged.
if (process.argv[2] === '--verify-local-pilot') {
  const dir = process.argv[3];
  const run = String(process.argv[4] || '');
  const sha = String(process.argv[5] || '');
  const load = (name) => { try { return JSON.parse(fs.readFileSync(path.join(dir,name),'utf8')); } catch { return null; } };
  const proof=load('proof.json'), cleanup=load('final-check.json'), worker=load('pilot-report.json');
  const checks=[];
  const check=(key,condition)=>checks.push({key,passed:Boolean(condition)});
  check('RUN', /^\d+$/.test(run) && String(worker?.run_id)===run);
  check('SHA', /^[a-f0-9]{40}$/.test(sha) && worker?.source_sha===sha);
  check('MISSION', worker?.order_id==='eldoria-local-agent-isolated-pilot-v1');
  check('MODEL', proof?.model==='qwen2.5-coder:3b' && worker?.model===proof?.model);
  check('MODEL_DIGEST', /^[a-f0-9]{64}$/.test(String(proof?.digest||'')) && worker?.model_digest===proof?.digest);
  check('GENERATED_SOURCE_SHA', /^[a-f0-9]{64}$/.test(String(proof?.source_sha256||'')) && worker?.generated_source_sha256===proof?.source_sha256);
  check('NINE_TESTS', proof?.status==='PASS' && proof?.test_cases_passed===9 && worker?.independent_test_cases_passed===9);
  check('ZERO_APIS', proof?.external_api_calls===0 && worker?.external_api_calls===0);
  check('NO_ARBITRARY_CODE', proof?.arbitrary_generated_code_executed===false && worker?.arbitrary_generated_code_executed===false);
  check('CLEANUP', cleanup?.status==='PASS' && worker?.cleanup==='PASS');
  check('HANDOFF', worker?.coordinator==='eldoria-coordinator-v1' && worker?.verdict==='LOCAL_WORKER_PASS_AWAITING_INDEPENDENT_COORDINATOR_REVIEW');
  const verdict=checks.every(x=>x.passed)?'LOCAL_PILOT_EVIDENCE_VERIFIED':'BLOCKED_LOCAL_PILOT_EVIDENCE';
  const report={schema_version:1,workstream:'eldoria-local-agent-isolated-pilot-v1',run_id:run,source_sha:sha,verdict,checks,notes:['Read-only independent coordinator artifact review; no generated code executed.','Local-only / zero API assertions are based on worker proof, not a forensic network audit.']};
  fs.writeFileSync(path.join(dir,'coordinator-verification.json'),JSON.stringify(report,null,2)+'\n');
  console.log(JSON.stringify(report));
  process.exit(verdict==='LOCAL_PILOT_EVIDENCE_VERIFIED'?0:1);
}
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
