import fs from 'node:fs';

const policyPath = process.argv[2] || 'pipeline/visual-fallback-policy.json';
const failureClass = process.argv[3] || 'unknown';
const out = process.argv[4] || '';
const policy = JSON.parse(fs.readFileSync(policyPath, 'utf8'));

const selected = policy.failure_classes[failureClass] || policy.failure_classes.unknown;
const result = {
  schema_version: 1,
  router: policy.policy_id,
  failure_class: policy.failure_classes[failureClass] ? failureClass : 'unknown',
  known_evidence_only: policy.default_mode === 'known_evidence_only',
  automatic_route: Boolean(selected.auto),
  open_new_research: false,
  owner_approval_required: false,
  route: selected.route || [],
  tools: selected.tools || [],
  conditional_tools: selected.conditional_tools || [],
  exclusions: selected.exclusions || [],
  decision: selected.auto ? 'ROUTE_KNOWN_FALLBACK' : 'STOP_AND_CLASSIFY'
};

if (result.open_new_research) throw new Error('Policy violation: fallback router cannot open new R&D.');
const json = JSON.stringify(result, null, 2);
console.log('VISUAL_FALLBACK_ROUTE=PASS');
console.log(json);
if (out) fs.writeFileSync(out, json + '\n');
