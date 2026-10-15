#!/usr/bin/env node
/**
 * Read-only capability discovery on whichever machine executes this command.
 * Never mistakes a workflow declaration for a live Windows connection.
 * No installations, process starts, runner leases, writes, or paid API calls.
 */
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { execFileSync, spawnSync } from 'node:child_process';
import http from 'node:http';
import { createHash } from 'node:crypto';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const defaultRoot = path.resolve(__dirname, '..');
const args = process.argv.slice(2);
const valueAfter = (flag) => {
  const i = args.indexOf(flag);
  return i >= 0 ? args[i + 1] : undefined;
};
const root = path.resolve(valueAfter('--root') || defaultRoot);
const probe = !args.includes('--no-probe');

function readFile(relative) {
  try { return fs.readFileSync(path.join(root, relative), 'utf8'); }
  catch { return null; }
}
function localCommand(command, argv = []) {
  try { return execFileSync(command, argv, {cwd: root, encoding: 'utf8', timeout: 1500, stdio: ['ignore', 'pipe', 'ignore']}).trim(); }
  catch { return null; }
}
function detectedBinary(envNames, commands) {
  if (!probe) return {status: 'not_tested', path: null};
  for (const name of envNames) {
    const candidate = process.env[name];
    if (candidate && fs.existsSync(candidate) && fs.statSync(candidate).isFile()) {
      return {status: 'present_on_this_host', path: candidate, source: name};
    }
  }
  const finder = process.platform === 'win32' ? 'where.exe' : 'which';
  for (const command of commands) {
    const found = spawnSync(finder, [command], {encoding: 'utf8', timeout: 1500, windowsHide: true});
    if (found.status === 0 && found.stdout?.trim()) {
      return {status: 'present_on_this_host', path: found.stdout.trim().split(/\r?\n/)[0], source: 'PATH'};
    }
  }
  return {status: 'not_found_on_this_host', path: null};
}
function ollamaLocalVersion() {
  return new Promise(resolve => {
    if (!probe) return resolve({status: 'not_tested'});
    const req = http.get({hostname: '127.0.0.1', port: 11434, path: '/api/version', timeout: 1000}, res => {
      let output = '';
      res.on('data', part => { if (output.length < 1024) output += part.toString(); });
      res.on('end', () => {
        try {
          const data = JSON.parse(output);
          resolve(res.statusCode === 200 && typeof data.version === 'string'
            ? {status: 'listening_on_this_host', version: data.version}
            : {status: 'not_listening_on_this_host'});
        } catch { resolve({status: 'not_listening_on_this_host'}); }
      });
    });
    req.on('timeout', () => req.destroy());
    req.on('error', () => resolve({status: 'not_listening_on_this_host'}));
  });
}
function declaredRoutes() {
  const dir = path.join(root, '.github', 'workflows');
  let workflows = [];
  try { workflows = fs.readdirSync(dir).filter(name => /\.ya?ml$/i.test(name)); } catch {}
  const specs = workflows.map(name => ({name, content: readFile('.github/workflows/' + name) || ''}));
  const windowsUnity = specs.filter(x => /self-hosted/i.test(x.content) && /windows/i.test(x.content) && /unity/i.test(x.content));
  const artist = specs.filter(x => /qwen|ollama|blender|artist/i.test(x.name) || (/ollama/i.test(x.content) && /blender/i.test(x.content)));
  return {
    windows_unity: {status: windowsUnity.length ? 'declared_not_verified_live' : 'not_found', workflows: windowsUnity.map(x => x.name).slice(0, 12)},
    ai_artist_blender: {status: artist.length ? 'declared_not_verified_live' : 'not_found', workflows: artist.map(x => x.name).slice(0, 20)}
  };
}
function productionPause() {
  try {
    const state = JSON.parse(readFile('pipeline/active-workstreams.json'));
    return state.owner_production_pause?.active === true ? 'active' :
      state.owner_production_pause?.active === false ? 'inactive' : 'unknown';
  } catch { return 'unknown'; }
}
function recoveryCheckpoint() {
  // Existing bootstrap is the sole checkpoint; no service, second registry or execution bus.
  const source = readFile('tools/ELDORIA_AGENT_SESSION_BOOTSTRAP.md') || '';
  const section = source.split('<!-- ELDORIA_RECOVERY_CHECKPOINT_START -->')[1]?.split('<!-- ELDORIA_RECOVERY_CHECKPOINT_END -->')[0];
  const encoded = section?.match(/```json\s*([\s\S]*?)```/)?.[1];
  if (!encoded) return {status: 'checkpoint_missing', next_action: 'Read live main bootstrap through the connected GitHub tool.'};
  let checkpoint;
  try {
    checkpoint = JSON.parse(encoded);
    if (checkpoint.schema_version !== 1 || !Array.isArray(checkpoint.inputs) ||
        !Array.isArray(checkpoint.capabilities) || !checkpoint.next_action?.id) throw new Error();
  } catch {
    return {status: 'checkpoint_invalid', next_action: 'Recover valid bootstrap from live main; do not execute from malformed state.'};
  }
  let inventory = null;
  let inventoryStatus = 'not_supplied';
  const inventoryPath = valueAfter('--tool-inventory');
  if (inventoryPath) {
    try {
      const parsed = JSON.parse(fs.readFileSync(path.resolve(inventoryPath), 'utf8'));
      const names = Array.isArray(parsed) ? parsed : parsed.tools;
      if (!Array.isArray(names) || !names.every(n => typeof n === 'string')) throw new Error();
      inventory = new Set(names);
      inventoryStatus = 'supplied_exposure_only';
    } catch { inventoryStatus = 'invalid'; }
  }
  const inputs = checkpoint.inputs.map(input => {
    if (typeof input.path !== 'string' || path.isAbsolute(input.path) ||
        input.path.split(/[\\/]/).includes('..') || !/^[a-f0-9]{40}$/.test(input.git_blob_sha || '')) {
      return {path: input.path, status: 'invalid_checkpoint_input', repeat_gate: 'review_required'};
    }
    let raw;
    try { raw = fs.readFileSync(path.join(root, input.path)); }
    catch { return {...input, status: 'not_present_on_this_checkout', repeat_gate: 'retrieve_exact_blob_before_deciding'}; }
    const actual = createHash('sha1').update(`blob ${raw.length}\0`).update(raw).digest('hex');
    return {...input, actual_git_blob_sha: actual,
      status: actual === input.git_blob_sha ? 'unchanged' : 'changed',
      repeat_gate: actual === input.git_blob_sha ? 'reuse_only_previously_certified_scope' : 'review_affected_gate'};
  });
  const interfaces = checkpoint.capabilities.map(capability => {
    const names = capability.inventory_tools || [];
    return {key: capability.key,
      exposure_now: !names.length ? 'no_callable_interface_recorded' :
        !inventory ? 'not_tested' : names.every(n => inventory.has(n)) ? 'available_in_inventory_only' : 'unavailable_in_this_session',
      missing_tools: inventory ? names.filter(n => !inventory.has(n)) : [],
      verified_now: false,
      last_recorded_observation: capability.last_observation};
  });
  return {status: 'checkpoint_recovered', checkpoint_is_historical: true,
    repository: checkpoint.repository, branch: checkpoint.branch,
    last_verified_source_head: checkpoint.last_verified_source_head,
    recorded_at: checkpoint.recorded_at, mission_status: checkpoint.mission_status,
    next_action: checkpoint.next_action, last_working_execution: checkpoint.last_working_execution,
    selective_memory_qwen_validated: checkpoint.selective_memory_qwen_validated === true,
    inputs, completed_gates: checkpoint.completed_gates,
    blocked_operations: checkpoint.blocked_operations, tool_inventory_status: inventoryStatus,
    interfaces, windows_live_verified_by_this_process: false,
    evidence: checkpoint.evidence,
    required_live_reads: ['main/AGENTS.md', 'assigned branch HEAD', 'active workstreams', 'relevant Actions jobs/artifacts'],
    conclusion: 'Recovered pending work and historical evidence only. Tool exposure is not authentication, execution approval or remote health.'};
}
const report = {
  schema_version: 2,
  purpose: 'Eldoria agent start: capability discovery without execution',
  inspected_at: new Date().toISOString(),
  repository: 'mt5hjfz2kb-lab/Eldoria-Prewiu',
  checkout: {path: root, agents_md_present: Boolean(readFile('AGENTS.md')),
    head: probe ? localCommand('git', ['rev-parse', 'HEAD']) : null},
  machine: {platform: process.platform,
    github_actions: process.env.GITHUB_ACTIONS === 'true',
    runner_os: process.env.RUNNER_OS || null,
    runner_environment: process.env.RUNNER_ENVIRONMENT || null,
    windows_self_hosted_context_reported: process.env.GITHUB_ACTIONS === 'true' &&
      /^windows$/i.test(process.env.RUNNER_OS || '') &&
      process.env.RUNNER_ENVIRONMENT === 'self-hosted'},
  routes: declaredRoutes(),
  local_tools: {
    unity: detectedBinary(['UNITY_EDITOR_PATH'], ['Unity.exe', 'Unity']),
    blender: detectedBinary(['BLENDER_PATH', 'BLENDER_BIN'], ['blender.exe', 'blender']),
    ollama: detectedBinary(['OLLAMA_PATH'], ['ollama.exe', 'ollama']),
    git: detectedBinary([], ['git.exe', 'git']),
    ollama_http: await ollamaLocalVersion()
  },
  owner_production_pause: productionPause(),
  recovery: recoveryCheckpoint(),
  required_follow_up: [
    'Read live main/AGENTS.md, HEAD, and active workstreams before doing anything else.',
    'In a ChatGPT session: separately verify its connected GitHub tools; this process cannot see ChatGPT connectors.',
    'Check Actions runs/jobs and exact workflow/ref/runner rather than assuming a declared Windows route is online.',
    'Ollama not listening on this machine does not mean it is absent from the Windows PC. Inspect actual Windows runner evidence before concluding.',
    'Respect the current production pause and resource owners; this discovery never authorizes execution.'
  ],
  conclusion: 'Machine-local facts and repository route declarations only; NOT proof of remote Windows availability.'
};
process.stdout.write(JSON.stringify(report, null, 2) + '\n');

