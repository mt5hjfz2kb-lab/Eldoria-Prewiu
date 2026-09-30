import fs from 'node:fs';
import path from 'node:path';

const dir = '.github/workflows';
const files = fs.readdirSync(dir).filter(name => /\.ya?ml$/i.test(name)).sort();
const failures = [];
let legacy = 0;
let automatic = 0;

function triggerBlock(text) {
  const lines = text.split(/\r?\n/);
  const start = lines.findIndex(line => /^on:\s*$/.test(line));
  if (start < 0) return '';
  const out = [];
  for (let i = start + 1; i < lines.length; i++) {
    const line = lines[i];
    if (/^[A-Za-z0-9_-]+:\s*/.test(line)) break;
    out.push(line);
  }
  return out.join('\n');
}

for (const file of files) {
  const full = path.join(dir, file);
  const text = fs.readFileSync(full, 'utf8');
  const firstName = (text.match(/^name:\s*(.+)$/m) || [])[1] || '';
  const trigger = triggerBlock(text);
  const isLegacy = /\[LEGACY\]/i.test(firstName);
  const hasAutomatic = /^\s{2}(push|pull_request|schedule|workflow_run):/m.test(trigger);
  if (isLegacy) {
    legacy++;
    if (hasAutomatic) failures.push(`${file}: [LEGACY] workflows must be workflow_dispatch-only.`);
  }
  if (hasAutomatic) automatic++;
  if (/actions\/runs\/\d+\/cancel/.test(text)) {
    failures.push(`${file}: hard-coded workflow-run cancellation IDs are forbidden.`);
  }
  const heavyWindows = /runs-on:\s*\[self-hosted,\s*windows,\s*unity-6000-3-23f1\]/i.test(text);
  const selfPathSingle = `- '.github/workflows/${file}'`;
  const selfPathDouble = `- ".github/workflows/${file}"`;
  const selfPathBare = `- .github/workflows/${file}`;
  const workflowSelfTrigger = trigger.includes(selfPathSingle) || trigger.includes(selfPathDouble) || trigger.includes(selfPathBare);
  if (heavyWindows && workflowSelfTrigger) {
    failures.push(`${file}: heavy Windows workflows must not auto-trigger from edits to their own workflow file.`);
  }
}

console.log(`Workflow governance: ${files.length} workflows, ${automatic} automatic, ${legacy} legacy/manual.`);
if (failures.length) {
  for (const failure of failures) console.error('FAIL:', failure);
  process.exit(1);
}
console.log('Workflow governance PASS.');
